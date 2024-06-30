using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Utility
{
    public static void Debugging()
    {
    }
    public static void Debugging(object param)
    {
    }
    public static void Debugging(object param1, object param2)
    {
        Vector2 v1 = (Vector2)param1;
        Vector2 v2 = (Vector2)param2;
        Debug.DrawLine(Vector2.zero, v1, Color.red);
        Debug.DrawLine(Vector2.zero, v2, Color.blue);
        float dot = Vector2.Dot(v1, v2);
        if (Mathf.Abs(dot) > 10f)
        {
            decimal k = (decimal)(10f - dot);
            decimal mag = (decimal)v2.magnitude;
            k /= mag;
            Debug.DrawLine(Vector2.zero, v1 + (float)k * v2, Color.green);
        }
        else
            Debug.DrawLine(Vector2.zero, v1 + v1 + v2, Color.green);
    }
    public static void Debugging(object param1, object param2, object param3)
    {
    }
    /// <summary>
    /// Get a vector's project on another one
    /// </summary>
    /// <param name="vector">Vector to project</param>
    /// <param name="bottom">The direction of the returned value</param>
    /// <returns></returns>
    public static Vector2 Project(Vector2 vector, Vector2 bottom)
    {
        return Vector2.Dot(vector, bottom) * bottom.normalized;
    }
    public static float DirectionAngle(Vector2 from, Vector2 to)
    {
        Vector2 v2 = (to - from).normalized;
        float angle = Mathf.Atan2(v2.y, v2.x) * Mathf.Rad2Deg;
        if (angle < 0)
            angle = 360 + angle;
        return angle;
    }
    public static float SignedDirectionAngle(Vector2 from, Vector2 to)
    {
        Vector2 v2 = (to - from).normalized;
        float angle = Mathf.Atan2(v2.y, v2.x) * Mathf.Rad2Deg;
        return angle;
    }
}
