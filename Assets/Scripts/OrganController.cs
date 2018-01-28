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
        Audio.Play("squishBig");
        Destroy(gameObject);
        FadeCameraController.Win();
    }
}