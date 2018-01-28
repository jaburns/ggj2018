using UnityEngine;

public class Pulse : MonoBehaviour 
{
    Vector3 startScale;

	void Start () 
    {
        startScale = transform.localScale;
	}

	void Update() 
    {
        transform.localScale = startScale * (1f + .1f * Mathf.Sin(2 * Time.time));
	}
}
