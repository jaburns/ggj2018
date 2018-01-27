using UnityEngine;

public class SelectoidController : MonoBehaviour
{
    [SerializeField] GameObject inside;
    [SerializeField] GameObject left;
    [SerializeField] GameObject right;
    [SerializeField] GameObject top;
    [SerializeField] GameObject bottom;

    public Vector2 Size { get; private set; }

    public void SetSize(Vector2 size)
    {
        Size = size;

        inside.transform.localScale = new Vector3(Size.x, Size.y, 1f);

        left.transform.localPosition = Vector3.left * Size.x / 2f;
        right.transform.localPosition = Vector3.right * Size.x / 2f;
        top.transform.localPosition = Vector3.up * Size.y / 2f;
        bottom.transform.localPosition = Vector3.down * Size.y / 2f;

        bottom.transform.localScale = 
            top.transform.localScale = new Vector3(
                Size.x + 0.1f,
                bottom.transform.localScale.y,
                bottom.transform.localScale.z
            );
        
        left.transform.localScale = 
            right.transform.localScale = new Vector3(
                left.transform.localScale.x,
                Size.y,
                left.transform.localScale.z
            );
	}
}