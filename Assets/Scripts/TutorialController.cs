using UnityEngine;

public class TutorialController : MonoBehaviour 
{
    [SerializeField] GameObject tutorial2;

	void Update () {
        if (Camera.main.transform.position.y > 12.5f) {
            tutorial2.SetActive(true);
            Destroy(gameObject);
        }
	}
}
