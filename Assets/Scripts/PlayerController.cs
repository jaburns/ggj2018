using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour 
{
    static public CellController[] AllCells { get; private set; }

    [SerializeField] GameObject cellPrefab;

    void Awake()
    {
        AllCells = generateCells();
    }

    CellController[] generateCells()
    {
        var cellList = new List<CellController>();
        for (int i = 0; i < 100; ++i) {
            var cellObj = Instantiate(cellPrefab, transform.position + Vector3.right * Random.Range(-10f, 10f) + Vector3.up * Random.Range(-10f, 10f), Quaternion.identity) as GameObject;
            cellList.Add(cellObj.GetComponent<CellController>());
        }
        return cellList.ToArray();
    }

    void Update() 
    {
        bool hitCell = false;

        RaycastHit2D[] hits = Physics2D.RaycastAll(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero, 0f, 1 << LayerMask.NameToLayer("MouseInteract"));
        foreach (var hit in hits) {
            if (hit.collider != null) {
                var cell = hit.collider.transform.parent.GetComponent<CellController>();
                if (cell != null) {
                    hitCell = true;
                    cell.NotifyMouseOver();
                }
            }
        }

        var selectedCells = new List<CellController>();
        var unselectedCells = new List<CellController>();

        if (Input.GetMouseButtonDown(0) && !hitCell) {
            foreach (var cell in AllCells) {
                if (cell.Selected) selectedCells.Add(cell);
                else unselectedCells.Add(cell);
            }

            foreach (var cell in selectedCells) {
                cell.attraction.Clear();
                cell.attraction.AddRange(selectedCells);
                cell.attraction.Remove(cell);

                cell.Selected = false;

                cell.seekPoint = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }

            foreach (var cell in unselectedCells) {
                cell.PurgeFromAttraction(selectedCells);

                cell.seekPoint = null;
            }
        }
	}
}
