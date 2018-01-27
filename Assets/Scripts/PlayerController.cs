using UnityEngine;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour 
{
    static public List<CellController> AllCells { get; private set; }

    [SerializeField] GameObject cellPrefab;
    [SerializeField] GameObject selectoidPrefab;

    [SerializeField] float cameraMoveSpeed;

    SelectoidController selectoid;
    Vector2 selectionPtA, selectionPtB;
    Vector2 cameraTarget;

    List<CellController> cachedSelection = new List<CellController>();
    List<CellController> selectedCells = new List<CellController>();
    List<CellController> unselectedCells = new List<CellController>();

    void Awake()
    {
        AllCells = generateCells();
        cameraTarget = transform.position;
    }

    List<CellController> generateCells()
    {
        var cellList = new List<CellController>();
        for (int i = 0; i < 100; ++i) {
            var cellObj = Instantiate(cellPrefab, Vector3.right * Random.Range(-10f, 10f) + Vector3.up * Random.Range(-10f, 10f), Quaternion.identity) as GameObject;
            cellList.Add(cellObj.GetComponent<CellController>());
        }
        return cellList;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.W)) cameraTarget += Vector2.up * cameraMoveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.S)) cameraTarget += Vector2.down * cameraMoveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.A)) cameraTarget += Vector2.left * cameraMoveSpeed * Time.deltaTime;
        if (Input.GetKey(KeyCode.D)) cameraTarget += Vector2.right * cameraMoveSpeed * Time.deltaTime;

        transform.position += (new Vector3(cameraTarget.x,cameraTarget.y,transform.position.z) - transform.position) / 10f;

        if (Input.GetMouseButtonDown(0)) {
            cachedSelection.Clear();
            if (Input.GetKey(KeyCode.LeftShift)) {
                cachedSelection.AddRange(selectedCells);
            }

            selectoid = (Instantiate(selectoidPrefab, null) as GameObject).GetComponent<SelectoidController>();
            selectionPtA = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0)) {
            selectionPtB = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            selectoid.transform.position = (selectionPtA + selectionPtB) / 2f;
            selectoid.SetSize((selectionPtB - selectionPtA).Abs());

            updateSelection();
        }

        if (Input.GetMouseButtonUp(0)) {
            Destroy(selectoid.gameObject);
            selectoid = null;
        }

        if (Input.GetMouseButtonDown(1)) {
            triggerMove();
        }

        if(Input.GetKey(KeyCode.R))
        {
            ApoptosisSelection();
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

    void triggerMove() 
    {
        foreach (var cell in selectedCells) {
            cell.attraction.Clear();
            cell.attraction.AddRange(selectedCells);
            cell.attraction.Remove(cell);
            cell.seekPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }

        foreach (var unselectedCell in unselectedCells) {
            foreach (var selectedCell in selectedCells) {
                unselectedCell.attraction.Remove(selectedCell);
            }
        }
	}
}