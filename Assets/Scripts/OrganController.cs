using UnityEngine;

public class OrganController : MonoBehaviour 
{
    static public OrganController Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public void GetRekt()
    {
        Destroy(gameObject);
        FadeCameraController.Win();
    }
}