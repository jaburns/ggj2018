using UnityEngine;

public interface IGroundGenerator
{
    void Generate(GameObject targetObject, Vector2[] points);
}
