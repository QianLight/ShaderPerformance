using System;
using UnityEngine;

namespace CFEngine.Editor
{
    [System.Serializable]
    public class HeightGradient
    {
        public bool enable = false;
        public float bottomHeight = 0.0f;
        public float topHeight = 1.5f;
        [Range(0, 1)]
        public float fade = 2f;
        public Color color = new Color(0, 0, 0, 0);

        public void CopyTo(HeightGradient another)
        {
            another.enable = enable;
            another.bottomHeight = bottomHeight;
            another.topHeight = topHeight;
            another.fade = fade;
            another.color = color;
        }
    }

    public class BandposePreview : MonoBehaviour
    {
        public bool previewHeightGradient;

        public static Action<BandposePreview> runOnceOnReset;
        public Func<HeightGradient> heightGradientGetter;
        public const float invSubLineLength = 1f / 0.1f;

        private void Reset()
        {
            if (runOnceOnReset != null)
            {
                runOnceOnReset(this);
                runOnceOnReset = null;
            }
        }

        private void OnDrawGizmos()
        {
            if (previewHeightGradient && heightGradientGetter != null)
            {
                Renderer[] renderers = transform.GetComponentsInChildren<Renderer>();

                Bounds bounds = GetBounds(renderers);

                DrawBounds(bounds);
            }
        }

        private void DrawBounds(Bounds bounds)
        {
            HeightGradient gradient = heightGradientGetter();

            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            Vector3 lb = new Vector3(min.x, 0, min.z);
            Vector3 rb = new Vector3(max.x, 0, min.z);
            Vector3 lf = new Vector3(min.x, 0, max.z);
            Vector3 rf = new Vector3(max.x, 0, max.z);

            Vector3 begin = Vector3.up * min.y;
            Vector3 end = Vector3.up * max.y;

            Color gizmosColorBackup = Gizmos.color;
            // vertical lines
            DrawLine(ref gradient, begin + lb, end + lb);
            DrawLine(ref gradient, begin + rb, end + rb);
            DrawLine(ref gradient, begin + lf, end + lf);
            DrawLine(ref gradient, begin + rf, end + rf);
            // bottom lines
            DrawLine(ref gradient, begin + lb, begin + rb);
            DrawLine(ref gradient, begin + rb, begin + rf);
            DrawLine(ref gradient, begin + rf, begin + lf);
            DrawLine(ref gradient, begin + lf, begin + lb);
            // top lines
            DrawLine(ref gradient, end + lb, end + rb);
            DrawLine(ref gradient, end + rb, end + rf);
            DrawLine(ref gradient, end + rf, end + lf);
            DrawLine(ref gradient, end + lf, end + lb);
            Gizmos.color = gizmosColorBackup;
        }

        private Bounds GetBounds(Renderer[] renderers)
        {
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            for (int i = 0; i < renderers.Length; i++)
            {
                Renderer renderer = renderers[i];
                if (renderer.enabled)
                {
                    Bounds b = renderer.bounds;
                    min.x = Mathf.Min(min.x, b.min.x);
                    min.y = Mathf.Min(min.y, b.min.y);
                    min.z = Mathf.Min(min.z, b.min.z);
                    max.x = Mathf.Max(max.x, b.max.x);
                    max.y = Mathf.Max(max.y, b.max.y);
                    max.z = Mathf.Max(max.z, b.max.z);
                }
            }
            return new Bounds((min + max) * 0.5f, max - min);
        }

        private void DrawLine(ref HeightGradient gradient, Vector3 begin, Vector3 end)
        {
            float lineCount = Mathf.FloorToInt(Vector3.Distance(begin, end) * invSubLineLength);
            for (int i = 0; i < lineCount; i++)
            {
                Vector3 lineStart = Vector3.Lerp(begin, end, i / lineCount);
                Vector3 lineEnd = Vector3.Lerp(begin, end, (i + 1) / lineCount);
                float lineHegiht = Mathf.Lerp(lineStart.y, lineEnd.y, i / (lineCount - 1));
                float gradientTime = Mathf.InverseLerp(gradient.bottomHeight, gradient.topHeight, lineHegiht - transform.position.y);
                gradientTime = Mathf.Clamp01(Mathf.Pow(gradientTime, gradient.fade));
                Color dstColor = new Color(gradient.color.r, gradient.color.g, gradient.color.b, 0);
                Gizmos.color = Color.Lerp(gradient.color, dstColor, gradientTime);
                Gizmos.DrawLine(lineStart, lineEnd);
            }
        }
    }
}
