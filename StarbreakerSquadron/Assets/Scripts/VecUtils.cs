using UnityEngine;

public class VecUtils
{
    public static Vector2 FlattenVec3(Vector3 vec)
    {
        return new Vector2(vec.x, vec.y);
    }

    public static Vector3 SetZ(Vector3 vec, float z = 0)
    {
        return new Vector3(vec.x, vec.y, z);
    }

    public static Vector3 SetZ(Vector2 vec, float z = 0)
    {
        return new Vector3(vec.x, vec.y, z);
    }
}
