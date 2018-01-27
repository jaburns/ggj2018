using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Ground))]
public class GroundEditor : Editor
{
    enum EditMode {
        MoveSingle,
        SelectMultiple,
        MoveMultiple,
        Insert,
        Delete
    }

    const float GRID_SIZE = 0.5f;
    const float BUTTON_SIZE = 0.125f;
    const float SNAP_THRESHOLD = 0.3f;

    Ground targ { get { return target as Ground; } }

    EditMode _editMode = EditMode.MoveSingle;
    bool _editNodes = true;
    List<int> _selectedNodeIndices = new List<int>();

    bool _snapToNeighbor = true;
    bool _snapToGrid;

    int _testMax;

    static bool DrawButton(Vector3 pos)
    {
        var handleSize = HandleUtility.GetHandleSize(pos) * BUTTON_SIZE;
        return Handles.Button(pos, Quaternion.identity, handleSize, handleSize, Handles.SphereCap);
    }

    void OnEnable()
    {
        Undo.undoRedoPerformed += OnUndoRedo;
    }

    void OnDisable()
    {
        Undo.undoRedoPerformed -= OnUndoRedo;
    }

    void generate()
    {
        var nodesArray = targ.Nodes.ToArray();
        var poly = targ.gameObject.EnsureComponent<PolygonCollider2D>();
        poly.points = nodesArray;
    }

    void generateCool()
    {
        var g = new CoolGenerator();
        g.Configure(targ.JagFlipNormals);
        g.Generate(targ.gameObject, targ.Nodes.ToArray());
    }

    void OnSceneGUI()
    {
        ensureTargetInit();

        if (_snapToGrid) {
            drawGrid();
        }

        // Draw outline of the geometry.
        Handles.color = Color.white;
        Handles.DrawLine(targ.transform.position + targ.Nodes[targ.Nodes.Count-1].AsVector3(), targ.transform.position + targ.Nodes[0].AsVector3());
        for (int i = 1; i < targ.Nodes.Count; ++i) {
            Handles.DrawLine(targ.transform.position + targ.Nodes[i-1].AsVector3(), targ.transform.position + targ.Nodes[i].AsVector3());
        }

        if (!_editNodes) return;

        if (_editMode == EditMode.MoveMultiple) {
            var averagePos = Vector2.zero;
            for (int i = 0; i < targ.Nodes.Count; ++i) {
                if (_selectedNodeIndices.Contains(i)) {
                    averagePos += targ.transform.position.AsVector2() + targ.Nodes[i];
                    Handles.color = Color.green;
                } else {
                    Handles.color = Color.red;
                }
                Handles.SphereCap(0, targ.transform.position + targ.Nodes[i].AsVector3(),
                    Quaternion.identity,
                    HandleUtility.GetHandleSize(targ.transform.position.AsVector2()) * BUTTON_SIZE);
            }
            averagePos /= _selectedNodeIndices.Count;

            EditorGUI.BeginChangeCheck();
            var delta = Handles.PositionHandle(averagePos, Quaternion.identity).AsVector2() - averagePos;
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(target, "Moved Multiple Ground Nodes");
                for (int i = 0; i < _selectedNodeIndices.Count; ++i) {
                    targ.Nodes[_selectedNodeIndices[i]] += delta;
                }
                generate();
            }
        }
        else for (int i = 0; i < targ.Nodes.Count; ++i) {
            switch (_editMode) {
                case EditMode.MoveSingle:
                    EditorGUI.BeginChangeCheck();
                    var newPos = Handles.PositionHandle(targ.transform.position + targ.Nodes[i].AsVector3(), Quaternion.identity);
                    if (EditorGUI.EndChangeCheck()) {
                        Undo.RecordObject(target, "Moved Single Ground Node");
                        targ.Nodes[i] = applySnapping(i, newPos - targ.transform.position);
                        generate();
                    }
                    break;
                case EditMode.SelectMultiple:
                    Handles.color = Color.red;
                    if (_selectedNodeIndices.Contains(i)) {
                        Handles.color = Color.green;
                        if (DrawButton(targ.transform.position + targ.Nodes[i].AsVector3())) {
                            _selectedNodeIndices.Remove(i);
                        }
                    } else if (DrawButton(targ.transform.position + targ.Nodes[i].AsVector3())) {
                        _selectedNodeIndices.Add(i);
                    }
                    break;
                case EditMode.Insert:
                    Handles.color = Color.green;
                    var half = (targ.Nodes[i] + targ.Nodes[(i+1)%targ.Nodes.Count]) / 2;
                    if (DrawButton(targ.transform.position + half.AsVector3())) {
                        _selectedNodeIndices.Clear();
                        Undo.RecordObject(target, "Insert Ground Node");
                        targ.Nodes.Insert(i+1, half);
                        generate();
                        return;
                    }
                    break;
                case EditMode.Delete:
                    Handles.color = Color.green;
                    if (DrawButton(targ.transform.position + targ.Nodes[i].AsVector3())) {
                        _selectedNodeIndices.Clear();
                        Undo.RecordObject(target, "Delete Ground Node");
                        targ.Nodes.RemoveAt(i);
                        generate();
                        return;
                    }
                    break;
            }
        }
    }

    void OnUndoRedo()
    {
        generate();
    }

    Vector2 applySnapping(int nodeIndex, Vector2 newNodePos)
    {
        if (_snapToGrid) {
            newNodePos += targ.transform.position.AsVector2();
            newNodePos.x = Mathf.Round(newNodePos.x / GRID_SIZE) * GRID_SIZE;
            newNodePos.y = Mathf.Round(newNodePos.y / GRID_SIZE) * GRID_SIZE;
            newNodePos -= targ.transform.position.AsVector2();
            return newNodePos;
        }

        if (_snapToNeighbor) {
            var prev = targ.Nodes[nodeIndex-1 >= 0 ? nodeIndex-1 : targ.Nodes.Count-1];
            var next = targ.Nodes[(nodeIndex+1) % targ.Nodes.Count];

            if (Mathf.Abs(newNodePos.x - prev.x) < SNAP_THRESHOLD) {
                newNodePos.x = prev.x;
            }
            else if (Mathf.Abs(newNodePos.x - next.x) < SNAP_THRESHOLD) {
                newNodePos.x = next.x;
            }

            if (Mathf.Abs(newNodePos.y - prev.y) < SNAP_THRESHOLD) {
                newNodePos.y = prev.y;
            }
            else if (Mathf.Abs(newNodePos.y - next.y) < SNAP_THRESHOLD) {
                newNodePos.y = next.y;
            }
        }

        return newNodePos;
    }

    void drawGrid ()
    {
        Handles.color = Color.grey;
        for (float ix = -1000; ix < 1000; ix += GRID_SIZE) {
            Handles.DrawLine (
                new Vector3 (ix, -1000, 0),
                new Vector3 (ix, 1000, 0)
            );
        }
        for (float iy = -1000; iy < 1000; iy += GRID_SIZE) {
            Handles.DrawLine (
                new Vector3 (-1000, iy, 0),
                new Vector3 (1000, iy, 0)
            );
        }
    }

    void ensureTargetInit()
    {
        if (targ.Nodes == null || targ.Nodes.Count < 3) {
            targ.Nodes = new List<Vector2> {
                new Vector2(-1, -1),
                new Vector2(0,  2),
                new Vector2(1, -1)
            };
        }
    }

    override public void OnInspectorGUI()
    {
        ensureTargetInit();

        if (ChangedToggle("Editable", ref _editNodes)) {
            SceneView.RepaintAll();
        }

        if (!_editNodes) return;

        targ.KeepDebugMesh = EditorGUILayout.Toggle("Keep Mesh", targ.KeepDebugMesh);
        targ.UseCoolGenerator = true;

        _testMax = EditorGUILayout.IntField("_testMax", _testMax);

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Edit Mode", EditorStyles.boldLabel);

            if (EditorTool.EnumToggleList<EditMode>(ref _editMode)) {
                if (_editMode == EditMode.Delete && targ.Nodes.Count < 4) {
                    _editMode = EditMode.MoveSingle;
                }
                SceneView.RepaintAll();
            }

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Snapping Options", EditorStyles.boldLabel);

            if (ChangedToggle("Snap to Grid", ref _snapToGrid)) {
                SceneView.RepaintAll();
            }
            _snapToNeighbor = EditorGUILayout.Toggle("Snap to Neighbor", _snapToNeighbor);

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Jaggy Options", EditorStyles.boldLabel);

            if (ChangedToggle("Use Jags", ref targ.UseJags)) {
                generate();
            }
            if (targ.UseJags) {
                if (ChangedFloat("Step Size", ref targ.JagStepSize)) generate();
                if (ChangedFloat("Jaggy Size", ref targ.JaggySize)) generate();
                if (ChangedToggle("Flip Normals", ref targ.JagFlipNormals)) generate();
            }

        EditorGUILayout.Separator();

        if (GUILayout.Button("Flip Horizontal")) flipHorizontal();
        if (GUILayout.Button("Centralize Pivot")) centralizePivot();

        if (targ.UseCoolGenerator) {
            EditorGUILayout.Separator();
            if (GUILayout.Button("Generate")) generateCool();
        }
    }

    static bool ChangedToggle(string caption, ref bool val)
    {
        var oldVal = val;
        val = EditorGUILayout.Toggle(caption, val);
        return oldVal != val;
    }

    static bool ChangedFloat(string caption, ref float val)
    {
        var oldVal = val;
        val = EditorGUILayout.FloatField(caption, val);
        return Math.Abs(oldVal - val) > float.Epsilon;
    }

    void flipHorizontal()
    {
        for (int i = 0; i < targ.Nodes.Count; ++i) {
            var node = targ.Nodes[i];
            var flippedX = -(targ.transform.localPosition.x + targ.Nodes[i].x) - targ.transform.localPosition.x;
            targ.Nodes[i] = new Vector2 {
                x = flippedX,
                y = node.y
            };
        }
        generate();
    }

    void centralizePivot()
    {
        var offset = Vector2.zero;
        for (int i = 0; i < targ.Nodes.Count; ++i) {
            offset += targ.Nodes[i];
        }
        offset /= targ.Nodes.Count;
        for (int i = 0; i < targ.Nodes.Count; ++i) {
            targ.Nodes[i] -= offset;
        }
        targ.transform.position += offset.AsVector3();

        generate();
    }
}
