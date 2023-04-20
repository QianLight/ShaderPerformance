using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CFEngine.WorldStreamer
{
    public class StreamerHelper
    {
        public static Bounds TransformBounds(Vector3 center,Vector3 size, Transform tf)
        {
            Bounds newBounds = new Bounds(center,size);
            TransformBounds(ref newBounds, tf);
            newBounds.center = new Vector3(newBounds.center.x, 0, newBounds.center.z);
            return newBounds;
        }

        private static void TransformBounds(ref Bounds bounds, Transform tf)
        {
            Matrix4x4 matrix = tf.localToWorldMatrix;
            var xa = matrix.GetColumn(0) * bounds.min.x;
            var xb = matrix.GetColumn(0) * bounds.max.x;

            var ya = matrix.GetColumn(1) * bounds.min.y;
            var yb = matrix.GetColumn(1) * bounds.max.y;

            var za = matrix.GetColumn(2) * bounds.min.z;
            var zb = matrix.GetColumn(2) * bounds.max.z;

            var col4Pos = matrix.GetColumn(3);

            Vector3 min = new Vector3();
            min.x = Mathf.Min(xa.x, xb.x) + Mathf.Min(ya.x, yb.x) + Mathf.Min(za.x, zb.x) + col4Pos.x;
            min.y = Mathf.Min(xa.y, xb.y) + Mathf.Min(ya.y, yb.y) + Mathf.Min(za.y, zb.y) + col4Pos.y;
            min.z = Mathf.Min(xa.z, xb.z) + Mathf.Min(ya.z, yb.z) + Mathf.Min(za.z, zb.z) + col4Pos.z;

            Vector3 max = new Vector3();
            max.x = Mathf.Max(xa.x, xb.x) + Mathf.Max(ya.x, yb.x) + Mathf.Max(za.x, zb.x) + col4Pos.x;
            max.y = Mathf.Max(xa.y, xb.y) + Mathf.Max(ya.y, yb.y) + Mathf.Max(za.y, zb.y) + col4Pos.y;
            max.z = Mathf.Max(xa.z, xb.z) + Mathf.Max(ya.z, yb.z) + Mathf.Max(za.z, zb.z) + col4Pos.z;
            
            bounds.SetMinMax(min, max);
        }
    }
}
