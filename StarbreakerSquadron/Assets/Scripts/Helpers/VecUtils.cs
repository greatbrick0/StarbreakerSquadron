using UnityEngine;

public static class VecUtils
{
    public static Vector2 FlattenVec3(this Vector3 vec)
    {
        return new Vector2(vec.x, vec.y);
    }

    public static Vector3 SetZ(this Vector3 vec, float z = 0)
    {
        return new Vector3(vec.x, vec.y, z);
    }

    public static Vector3 SetZ(this Vector2 vec, float z = 0)
    {
        return new Vector3(vec.x, vec.y, z);
    }

    public static Vector2 RotateDegrees(this Vector2 vec, float angle)
    {
        float delta = Mathf.Deg2Rad * angle;
        return new Vector2(
            vec.x * Mathf.Cos(delta) - vec.y * Mathf.Sin(delta),
            vec.x * Mathf.Sin(delta) + vec.y * Mathf.Cos(delta)
        );
    }

    public static Vector3 RotateDegrees(this Vector3 vec, float angle)
    {
        float delta = Mathf.Deg2Rad * angle;
        return new Vector3(
            vec.x * Mathf.Cos(delta) - vec.y * Mathf.Sin(delta),
            vec.x * Mathf.Sin(delta) + vec.y * Mathf.Cos(delta),
            vec.z
        );
    }

    public static Color ChangeAlpha(this Color oldColour, float newAlpha)
    {
        return new Color(oldColour.r, oldColour.g, oldColour.b, newAlpha);
    }

    public static float ModifiedDistance(Vector3 p1, Vector3 p2, float modify)
    {
        return Mathf.Pow((Mathf.Pow(p1.x - p2.x, modify) + Mathf.Pow(p1.y - p2.y, modify)), 1.0f / modify);
    }
}
