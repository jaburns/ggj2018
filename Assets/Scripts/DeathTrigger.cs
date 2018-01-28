using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathTrigger : MonoBehaviour 
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        var cell = collision.gameObject.GetComponent<CellController>();
        if (cell != null) {
            cell.Apoptosis();
        }
    }
}
