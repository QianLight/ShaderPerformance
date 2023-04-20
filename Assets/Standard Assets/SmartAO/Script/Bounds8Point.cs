

#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct Bounds8Point
{
    public static void DrawGizmos(Bounds8Point p)
    {
        Gizmos.DrawLine(p.P0, p.P1);
        Gizmos.DrawLine(p.P1, p.P2);
        Gizmos.DrawLine(p.P2, p.P3);
        Gizmos.DrawLine(p.P3, p.P0);
        Gizmos.DrawLine(p.P0, p.P4);
        Gizmos.DrawLine(p.P1, p.P5);
        Gizmos.DrawLine(p.P2, p.P6);
        Gizmos.DrawLine(p.P3, p.P7);
        Gizmos.DrawLine(p.P4, p.P5);
        Gizmos.DrawLine(p.P5, p.P6);
        Gizmos.DrawLine(p.P6, p.P7);
        Gizmos.DrawLine(p.P7, p.P4);
    }
    /// <summary>
    /// Center 位相对坐标
    /// </summary>
    public Vector3 LocalCenter, LocalExtents;
    public Vector3 P0, P1, P2, P3, P4, P5, P6, P7;
}
#endif