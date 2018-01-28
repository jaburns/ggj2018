using UnityEngine;

public class CameraBounds : MonoBehaviour 
{
    public Rect bounds;

    void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(bounds.center.AsVector3(0), bounds.size.AsVector3(0));
    }
}
