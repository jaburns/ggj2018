using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using TriangleNet.Geometry;
using CoolGroundGenerator;

public class CoolGenerator : IGroundGenerator
{
    const bool SHARP_EDGE = false;
    const float _groundPull = 1.2f;

    bool _windBackwards;

    public void Configure(bool windBackwards)
    {
        _windBackwards = windBackwards;
    }

    public void Generate(GameObject targetObject, Vector2[] nodes)
    {
        UnityEngine.Random.seed = 32767;

        if (_windBackwards) {
            Array.Reverse(nodes);
        }

        var shape = FacePoints.getPointsForShape(nodes);
        var faceMesh = generateFaceMesh(shape);
        var extrudeMeshes = getExtrusions(shape.edge);

        if (SHARP_EDGE) {
            var wholeOuter = combineVertices(combineMeshes(extrudeMeshes.ToArray()));

            var meshes = new List<Mesh>();
            meshes.Add(faceMesh);
            meshes.Add(wholeOuter);

            useMeshWithObject(targetObject,
                combineMeshes(
                    meshes.ToArray()));
        } else {
            var meshes = new List<Mesh>();
            meshes.Add(faceMesh);
            meshes.AddRange(extrudeMeshes);

            useMeshWithObject(targetObject,
                combineVertices(
                    combineMeshes(
                        meshes.ToArray())));
        }
    }

    void useMeshWithObject(GameObject targetObject, Mesh mesh)
    {
        var filter = targetObject.EnsureComponent<MeshFilter>();
        filter.sharedMesh = mesh;

        var renderer = targetObject.EnsureComponent<MeshRenderer>();
        if (renderer.sharedMaterial == null) {
            renderer.sharedMaterial = EditorGUIUtility.Load("_DEBUG_Ground.mat") as Material;
        }
        renderer.sortingOrder = 1000;

        SceneView.RepaintAll();
    }

    Mesh combineMeshes(Mesh[] meshes)
    {
        var combine = new CombineInstance[meshes.Length];
        for (int i = 0; i < meshes.Length; ++i) {
            combine[i] = new CombineInstance {
                mesh = meshes[i],
                transform = Matrix4x4.identity
            };
        }
        var ret = new Mesh();
        ret.CombineMeshes(combine);
        return ret;
    }

    List<Mesh> getExtrusions(FacePoint[] pts)
    {
        var ret = new List<Mesh>();

        for (int i = 0; i < pts.Length; ++i) {
            var curr = pts[i];
            var next = pts[(i + 1) % pts.Length];

            var vertices = new Vector3[] {
                new Vector3(curr.x, curr.y, -curr.groundness*_groundPull),
                new Vector3(next.x, next.y, -next.groundness*_groundPull),
                new Vector3(next.x, next.y, 2),
                new Vector3(curr.x, curr.y, 2)
            };

            var dnext = next.p - curr.p;
            var verticalSeg = Math.Abs(dnext.y) > Math.Abs(dnext.x);

            var uvs = new Vector2[] {
                vertices[0].AsVector2(),
                vertices[1].AsVector2(),
                vertices[2].AsVector2() + 2 * (verticalSeg ? Vector2.right : Vector2. up),
                vertices[3].AsVector2() + 2 * (verticalSeg ? Vector2.right : Vector2. up),
            };

            var uv2 = new Vector2[] {
                new Vector2(curr.groundness*curr.groundness, 0),
                new Vector2(next.groundness*next.groundness, 0),
                new Vector2(next.groundness*next.groundness, 0),
                new Vector2(curr.groundness*curr.groundness, 0),
            };

            var triangles = new int[] { 0, 1, 2, 0, 2, 3 };

            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            mesh.uv2 = uv2;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            ret.Add(mesh);
        }

        return ret;
    }

    Mesh getMeshFromOutline(Vector2[] vertices2D)
    {
        var tr = new Triangulator(vertices2D);
        int[] indices = tr.Triangulate();

        var vertices = new Vector3[vertices2D.Length];
        var uvs = new Vector2[vertices2D.Length];
        for (int i=0; i<vertices.Length; i++) {
            vertices[i] = new Vector3(vertices2D[i].x, vertices2D[i].y, 0);
            uvs[i] = vertices[i].AsVector2();
        }

        var mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = indices;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    Mesh combineVertices(Mesh mesh)
    {
        var verts = mesh.vertices;
        var tris = mesh.triangles;

        var indexMap = new int[verts.Length];
        for (int i = 0; i < verts.Length; ++i) {
            indexMap[i] = i;
        }

        for (int i = 0; i < verts.Length - 1; ++i) {
            for (int j = i+1; j < verts.Length; ++j) {
                if ((verts[i] - verts[j]).sqrMagnitude < 1e-9f) {
                    indexMap[i] = j;
                }
            }
        }

        var newTriangles = new int[tris.Length];
        for (int i = 0; i < tris.Length; ++i) {
            newTriangles[i] = indexMap[mesh.triangles[i]];
        }

        mesh.triangles = newTriangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    Mesh generateFaceMesh(ShapePoints shape)
    {
        var mesh = new Mesh();
        var verts = new List<Vector3>();
        var tris = new List<int>();
        var uvs = new List<Vector2>();
        var uv2 = new List<Vector2>();

        var geometry = new InputGeometry();
        for (int i = 0; i < shape.edge.Length; ++i) {
            var pt = shape.edge[i];
            geometry.AddPoint(pt.x, pt.y);
            verts.Add(pt.p.AsVector3(-pt.groundness*1.2f));
            uvs.Add(pt.p);
            uv2.Add(new Vector2(pt.groundness*pt.groundness, 0));
            geometry.AddSegment(i, (i+1)%shape.edge.Length);
        }

        for (int i = 0; i < shape.interior.Length; ++i) {
            var pt = shape.interior[i];
            geometry.AddPoint(pt.x, pt.y);
            verts.Add(pt.p.AsVector3(-pt.groundness*1.2f + UnityEngine.Random.value*0.4f));
            uvs.Add(pt.p);
            uv2.Add(new Vector2(pt.groundness*pt.groundness, 0));
        }

        var behave = new TriangleNet.Behavior();
        behave.Algorithm = TriangleNet.TriangulationAlgorithm.Incremental;

        var meshRepresentation = new TriangleNet.Mesh(behave);
        meshRepresentation.Triangulate(geometry);

        foreach (var tri in meshRepresentation.Triangles) {
            tris.Add(tri.GetVertex(2).ID);
            tris.Add(tri.GetVertex(1).ID);
            tris.Add(tri.GetVertex(0).ID);
        }

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.uv2 = uv2.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    Mesh isolateTriangles(Mesh input)
    {
        var mesh = new Mesh();
        var verts = new List<Vector3>();
        var tris = new List<int>();

        var inputVerts = input.vertices;
        var inputTris = input.triangles;

        for (int i = 0; i < inputTris.Length; i++) {
            verts.Add(inputVerts[inputTris[i]]);
            tris.Add(i);
        }

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    Mesh justPoints(Vector2[] pts)
    {
        const float D = 0.2f;

        var mesh = new Mesh();
        var verts = new List<Vector3>();
        var tris = new List<int>();

        for (int i = 0; i < pts.Length; ++i) {
            verts.Add(new Vector3(pts[i].x - D, pts[i].y - D, 0));
            verts.Add(new Vector3(pts[i].x - D, pts[i].y + D, 0));
            verts.Add(new Vector3(pts[i].x + D, pts[i].y + D, 0));
            verts.Add(new Vector3(pts[i].x + D, pts[i].y - D, 0));
            tris.Add(4*i + 0);
            tris.Add(4*i + 1);
            tris.Add(4*i + 2);
            tris.Add(4*i + 0);
            tris.Add(4*i + 2);
            tris.Add(4*i + 3);
        }

        mesh.vertices = verts.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}
