using UnityEngine;
using System.Collections.Generic;

public class Ground : MonoBehaviour
{
    public List<Vector2> Nodes;

    public bool KeepDebugMesh = true;
    public bool UseCoolGenerator;

    public bool UseJags;
    public float JaggySize = 0.5f;
    public float JagStepSize = 1.5f;
    public bool JagFlipNormals;

    void Awake()
    {
        if (!KeepDebugMesh) {
            var rend = GetComponent<MeshRenderer>();
            if (rend != null) Destroy(rend);
            var filt = GetComponent<MeshFilter>();
            if (filt != null) Destroy(filt);
        }
    }
}
