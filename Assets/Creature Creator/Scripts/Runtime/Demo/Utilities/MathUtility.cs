using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathUtility
{
    public static Vector3 Clamp(this Vector3 value, float minValue, float maxValue)
    {
        return new Vector3(Mathf.Clamp(value.x, minValue, maxValue), Mathf.Clamp(value.y, minValue, maxValue), Mathf.Clamp(value.z, minValue, maxValue));
    }
}
