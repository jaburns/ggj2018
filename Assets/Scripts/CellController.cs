using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class CellController : MonoBehaviour 
{
    int hoverFrames;
    Rigidbody2D rb;

    public List<CellController> attraction = new List<CellController>();
    public bool Selected { get; set; }
    public Vector3? seekPoint;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        attraction = (PlayerController.AllCells.Clone() as CellController[]).ToList();
        attraction.Remove(this);
    }

    public void NotifyMouseOver()
    {
        if (Input.GetMouseButton(0)) {
            Selected = true;
        } else {
            hoverFrames = 3;
        }
    }

    public void PurgeFromAttraction(List<CellController> cells) 
    {
        foreach (var cell in cells) {
            attraction.Remove(cell);
        }
    }

    void Update()
    {
        if (hoverFrames > 0) {
            hoverFrames--;
        }

        if (Selected) {
            GetComponentInChildren<Renderer>().material.color = Color.red;
        } else if (hoverFrames > 0) {
            GetComponentInChildren<Renderer>().material.color = Color.yellow;
        } else {
            GetComponentInChildren<Renderer>().material.color = Color.white;
        }
    }

    static Vector2 forceBetweenCells(CellController thisCell, CellController otherCell)
    {
        var ds = otherCell.transform.position - thisCell.transform.position;
        return ds.normalized / ds.magnitude;
    }

    void FixedUpdate()
    {
        foreach (var cell in attraction) {
            rb.AddForce(forceBetweenCells(this, cell));
        }

        if (seekPoint.HasValue) {
            rb.AddForce(1f * (seekPoint.Value - transform.position).normalized);
        }
    }
}