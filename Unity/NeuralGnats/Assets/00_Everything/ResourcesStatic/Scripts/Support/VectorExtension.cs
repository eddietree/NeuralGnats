using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtension {

    public static Vector3 Lerp(Vector3 a, Vector3 b, Vector3 coeff)
    {
        Vector3 result;
        result.x = Mathf.Lerp(a.x, b.x, coeff.x);
        result.y = Mathf.Lerp(a.y, b.y, coeff.y);
        result.z = Mathf.Lerp(a.z, b.z, coeff.z);

        return result;
    }

    public static Vector3 Div(this Vector3 a, Vector3 b)
    {
        a.x = a.x / b.x;
        a.y = a.y / b.y;
        a.z = a.z / b.z;

        return a;
    }

    public static Vector3 Invert(this Vector3 a)
    {
        a = new Vector3(1.0f/a.x, 1.0f/a.y, 1.0f/a.z);

        return a;
    }

    public static Vector3 FlattenXZ(this Vector3 a)
    {
        a.y = 0.0f;

        return a;
    }

    public static Vector3 Clamp(this Vector3 a, Vector3 minVal, Vector3 maxVal)
    {
        a.x = Mathf.Clamp(a.x, minVal.x, maxVal.x);
        a.y = Mathf.Clamp(a.y, minVal.y, maxVal.y);
        a.z = Mathf.Clamp(a.z, minVal.z, maxVal.z);

        return a;
    }

    public static Vector3 Clamp01(this Vector3 a)
    {
        return a.Clamp(Vector3.zero, Vector3.one);
    }
}
