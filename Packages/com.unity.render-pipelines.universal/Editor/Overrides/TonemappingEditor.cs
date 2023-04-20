using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEditor.Rendering.Universal
{
    [VolumeComponentEditor(typeof(Tonemapping))]
    sealed class TonemappingEditor : VolumeComponentEditor
    {
        SerializedDataParameter m_Mode;
        SerializedDataParameter toneCurveToeStrength;
        SerializedDataParameter toneCurveToeLength;
        SerializedDataParameter toneCurveShoulderStrength;
        SerializedDataParameter toneCurveShoulderLength;
        SerializedDataParameter toneCurveShoulderAngle;
        SerializedDataParameter toneCurveGamma;

        const int k_CustomToneCurveResolution = 1024;
        const float k_CustomToneCurveRangeY = 1.025f;
        readonly Vector3[] m_RectVertices = new Vector3[4];
        readonly Vector3[] m_LineVertices = new Vector3[2];
        readonly Vector3[] m_CurveVertices = new Vector3[k_CustomToneCurveResolution];
        Rect m_CustomToneCurveRect;
        readonly HableCurve m_HableCurve = new HableCurve();

        public override void OnEnable()
        {
            var o = new PropertyFetcher<Tonemapping>(serializedObject);

            m_Mode = Unpack(o.Find(x => x.mode));
            toneCurveToeStrength = Unpack(o.Find(x => x.toneCurveToeStrength));
            toneCurveToeLength = Unpack(o.Find(x => x.toneCurveToeLength));
            toneCurveShoulderStrength = Unpack(o.Find(x => x.toneCurveShoulderStrength));
            toneCurveShoulderLength = Unpack(o.Find(x => x.toneCurveShoulderLength));
            toneCurveShoulderAngle = Unpack(o.Find(x => x.toneCurveShoulderAngle));
            toneCurveGamma = Unpack(o.Find(x => x.toneCurveGamma));
        }

        public override void OnInspectorGUI()
        {
            PropertyField(m_Mode);

            if ((TonemappingMode)m_Mode.value.enumValueIndex == TonemappingMode.Custom)
            {
                CustomTone();
            }

            // Display a warning if the user is trying to use a tonemap while rendering in LDR
            var asset = UniversalRenderPipeline.asset;
            if (asset != null && !asset.supportsHDR)
            {
                EditorGUILayout.HelpBox("Tonemapping should only be used when working in HDR.", MessageType.Warning);
                return;
            }
        }

        private void CustomTone()
        {
            DrawCustomToneCurve();
            PropertyField(toneCurveToeStrength);
            PropertyField(toneCurveToeLength);
            PropertyField(toneCurveShoulderStrength);
            PropertyField(toneCurveShoulderLength);
            PropertyField(toneCurveShoulderAngle);
            PropertyField(toneCurveGamma);
        }

        void DrawCustomToneCurve()
        {
            EditorGUILayout.Space();

            // Reserve GUI space
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(EditorGUI.indentLevel * 15f);
                m_CustomToneCurveRect = GUILayoutUtility.GetRect(128, 80);
            }

            if (Event.current.type != EventType.Repaint)
                return;

            m_HableCurve.Init(
                toneCurveToeStrength.value.floatValue,
                toneCurveToeLength.value.floatValue,
                toneCurveShoulderStrength.value.floatValue,
                toneCurveShoulderLength.value.floatValue,
                toneCurveShoulderAngle.value.floatValue,
                toneCurveGamma.value.floatValue
            );

            float endPoint = m_HableCurve.whitePoint;

            // Background
            m_RectVertices[0] = PointInRect(0f, 0f, endPoint);
            m_RectVertices[1] = PointInRect(endPoint, 0f, endPoint);
            m_RectVertices[2] = PointInRect(endPoint, k_CustomToneCurveRangeY, endPoint);
            m_RectVertices[3] = PointInRect(0f, k_CustomToneCurveRangeY, endPoint);
            Handles.DrawSolidRectangleWithOutline(m_RectVertices, Color.white * 0.1f, Color.white * 0.4f);

            // Vertical guides
            if (endPoint < m_CustomToneCurveRect.width / 3)
            {
                int steps = Mathf.CeilToInt(endPoint);
                for (var i = 1; i < steps; i++)
                    DrawLine(i, 0, i, k_CustomToneCurveRangeY, 0.4f, endPoint);
            }

            // Label
            Handles.Label(m_CustomToneCurveRect.position + Vector2.right, "Custom Tone Curve", EditorStyles.miniLabel);

            // Draw the acual curve
            var vcount = 0;
            while (vcount < k_CustomToneCurveResolution)
            {
                float x = endPoint * vcount / (k_CustomToneCurveResolution - 1);
                float y = m_HableCurve.Eval(x);

                if (y < k_CustomToneCurveRangeY)
                {
                    m_CurveVertices[vcount++] = PointInRect(x, y, endPoint);
                }
                else
                {
                    if (vcount > 1)
                    {
                        // Extend the last segment to the top edge of the rect.
                        var v1 = m_CurveVertices[vcount - 2];
                        var v2 = m_CurveVertices[vcount - 1];
                        var clip = (m_CustomToneCurveRect.y - v1.y) / (v2.y - v1.y);
                        m_CurveVertices[vcount - 1] = v1 + (v2 - v1) * clip;
                    }

                    break;
                }
            }

            if (vcount > 1)
            {
                Handles.color = Color.white * 0.9f;
                Handles.DrawAAPolyLine(2f, vcount, m_CurveVertices);
            }
        }

        Vector3 PointInRect(float x, float y, float rangeX)
        {
            x = Mathf.Lerp(m_CustomToneCurveRect.x, m_CustomToneCurveRect.xMax, x / rangeX);
            y = Mathf.Lerp(m_CustomToneCurveRect.yMax, m_CustomToneCurveRect.y, y / k_CustomToneCurveRangeY);
            return new Vector3(x, y, 0);
        }

        void DrawLine(float x1, float y1, float x2, float y2, float grayscale, float rangeX)
        {
            m_LineVertices[0] = PointInRect(x1, y1, rangeX);
            m_LineVertices[1] = PointInRect(x2, y2, rangeX);
            Handles.color = Color.white * grayscale;
            Handles.DrawAAPolyLine(2f, m_LineVertices);
        }
    }
}