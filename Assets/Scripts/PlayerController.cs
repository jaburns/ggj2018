using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerController : MonoBehaviour 
{
    static public List<CellController> AllCells { get; private set; }
    static public List<EnemyController> EnemyCells { get; private set; }

    [SerializeField] GameObject cellPrefab;
    [SerializeField] GameObject selectoidPrefab;
    [SerializeField] float cameraMoveSpeed;
    [SerializeField] int maxCellAttractPairs;

    static public GameObject CellPrefab { get; private set; } 

    SelectoidController selectoid;
    Vector2 selectionPtA, selectionPtB;

    Vector2 cameraTarget;

    List<CellController> cachedSelection = new List<CellController>();
    List<CellController> selectedCells = new List<CellController>();
    List<CellController> unselectedCells = new List<CellController>();

    CameraBounds bounds;

    void Awake()
    {
        AllCells = generateCells();
        cameraTarget = transform.position;
        CellPrefab = cellPrefab;

        EnemyCells = GameObject.FindObjectsOfType<EnemyController>().ToList();

        bounds = FindObjectOfType<CameraBounds>();
    }

    List<CellController> generateCells()
    {
        int count = 100;
        var basePos = transform.position.WithZ(0);
        var radius = 5f;
        var spawner = FindObjectOfType<CellSpawner>();
        if (spawner != null) {
            count = spawner.CellCount;
            radius = spawner.Radius;
            basePos = spawner.transform.position.WithZ(0);
        }

        var cellList = new List<CellController>();
        for (int i = 0; i < count; ++i) {
            var cellObj = Instantiate(cellPrefab, basePos + Vector3.right * Random.Range(-radius, radius) + Vector3.up * Random.Range(-radius, radius), Quaternion.identity) as GameObject;
            cellList.Add(cellObj.GetComponent<CellController>());
        }
        return cellList;
    }

    static Vector2 getMouseWorldPos()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var t = -ray.origin.z / ray.direction.normalized.z;
        return ray.origin + ray.direction * t;
    }

    void Update()
    {
        while (AllCells.Remove(null)) { }

        if (AllCells.Count < 1) {
            FadeCameraController.Lose();
            Destroy(this);
            return;
        }
        
        var mousePos = getMouseWorldPos();

        if (Input.GetMouseButtonDown(0)) {
            cachedSelection.Clear();
            if (Input.GetKey(KeyCode.LeftShift)) {
                cachedSelection.AddRange(selectedCells);
            }

            selectoid = (Instantiate(selectoidPrefab, null) as GameObject).GetComponent<SelectoidController>();
            selectionPtA = mousePos;
        }

        if (Input.GetMouseButton(0)) {
            selectionPtB = mousePos;
            selectoid.transform.position = (selectionPtA + selectionPtB) / 2f;
            selectoid.SetSize((selectionPtB - selectionPtA).Abs());

            updateSelection();
        }

        if (Input.GetMouseButtonUp(0)) {
            Destroy(selectoid.gameObject);
            selectoid = null;
        }

        if (Input.GetMouseButtonDown(1)) {
            updateAttractionListInAllCells();
        }

        if (Input.GetMouseButton(1)) {
            updateSeekPointInSelectedCells(mousePos);
        }

        if(Input.GetKey(KeyCode.Space))
        {
            ApoptosisSelection();
        }

        updateCameraPosition();
    }

    void updateCameraPosition()
    {
        if (Input.GetKey(KeyCode.LeftShift)) cameraMoveSpeed *= 2;
        if (Input.GetKey(KeyCode.W)) cameraTarget += Vector2.up    * cameraMoveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S)) cameraTarget += Vector2.down  * cameraMoveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.A)) cameraTarget += Vector2.left  * cameraMoveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.D)) cameraTarget += Vector2.right * cameraMoveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftShift)) cameraMoveSpeed /= 2;

        if (bounds != null) {
            if (cameraTarget.x < bounds.bounds.xMin) cameraTarget = cameraTarget.WithX(bounds.bounds.xMin);
            if (cameraTarget.x > bounds.bounds.xMax) cameraTarget = cameraTarget.WithX(bounds.bounds.xMax);
            if (cameraTarget.y < bounds.bounds.yMin) cameraTarget = cameraTarget.WithY(bounds.bounds.yMin);
            if (cameraTarget.y > bounds.bounds.yMax) cameraTarget = cameraTarget.WithY(bounds.bounds.yMax);
        }

        var speed = (new Vector3(cameraTarget.x, cameraTarget.y, transform.position.z) - transform.position) / 10f;
     // var MAX = 1f;
     // if (speed.sqrMagnitude > MAX * MAX) speed = speed.normalized * MAX;
        transform.position += speed;

    }

public static void Shuffle<T>(IList<T> list)  
{  
    int n = list.Count;  
    while (n > 1) {  
        n--;  
        int k = (int)Random.Range(0f, list.Count - 1f);  
        T value = list[k];  
        list[k] = list[n];  
        list[n] = value;  
    }  
}

    void ApoptosisSelection()
    {
        foreach(CellController apoptosisCell in selectedCells)
        {
            foreach(CellController otherCell in AllCells)
            {
                otherCell.attraction.Remove(apoptosisCell);
            }
            apoptosisCell.Apoptosis();
            AllCells.Remove(apoptosisCell);
        }
        cachedSelection.Clear();
        selectedCells.Clear();
        unselectedCells.Clear();
    }

    void updateSelection()
    {
        var rekt = new Rect(VectorExt.Min(selectionPtA, selectionPtB), (selectionPtB - selectionPtA).Abs());
        foreach (var cell in AllCells) {
            cell.selected = cachedSelection.Contains(cell) || rekt.Contains(cell.transform.position);
        }

        selectedCells.Clear();
        unselectedCells.Clear();

        foreach (var cell in AllCells) {
            if (cell.selected) selectedCells.Add(cell);
            else unselectedCells.Add(cell);
        }
    }

    void updateAttractionListInAllCells() 
    {
        foreach (var cell in selectedCells) {
            cell.attraction.Clear();
            cell.attraction.AddRange(selectedCells);
            cell.attraction.Remove(cell);
            Shuffle(cell.attraction);
            if (cell.attraction.Count > maxCellAttractPairs) {
                cell.attraction.RemoveRange(maxCellAttractPairs, cell.attraction.Count - maxCellAttractPairs);
            }
            cell.FindRotoboi();
        }

        foreach (var unselectedCell in unselectedCells) {
            foreach (var selectedCell in selectedCells) {
                unselectedCell.attraction.Remove(selectedCell);
            }
        }
	}

    void updateSeekPointInSelectedCells(Vector2 pos)
    {
    //  var averageSelectedPos = Vector2.zero;
        
        foreach (var cell in selectedCells) {
            cell.seekPoint = pos;
    //      averageSelectedPos += cell.transform.position.AsVector2();
        }
    //  averageSelectedPos /= selectedCells.Count;
    //  cameraTarget = averageSelectedPos;
    }
}