using UnityEngine;

public class VecUtils
{
    public static Vector2 FlattenVec3(Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }

    public static Vector3 SetZ(Vector3 v, float z = 0)
    {
        return new Vector3(v.x, v.y, z);
    }

    public static Vector3 SetZ(Vector2 v, float z = 0)
    {
        return new Vector3(v.x, v.y, z);
    }
}
