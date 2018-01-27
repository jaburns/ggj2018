using UnityEngine;

static public class VectorExt
{
    static public Vector2 Abs(this Vector2 vec)
    {
        return new Vector2(Mathf.Abs(vec.x), Mathf.Abs(vec.y));
    }

    static public Vector2 Min(Vector2 a, Vector2 b) 
    {
        return new Vector2(
            Mathf.Min(a.x, b.x),
            Mathf.Min(a.y, b.y)
        );
    }

    static public Vector2 Max(Vector2 a, Vector2 b) 
    {
        return new Vector2(
            Mathf.Max(a.x, b.x),
            Mathf.Max(a.y, b.y)
        );
    }
}
