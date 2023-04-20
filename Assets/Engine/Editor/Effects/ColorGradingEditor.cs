using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    [EnvEditor (typeof (ColorGrading))]
    public sealed class ColorGradingEditor : EnvEffectEditor<ColorGrading>
    {
        SerializedParameterOverride tonemapper;
        SerializedParameterOverride lutMode;
        //
        SerializedParameterOverride customTone0;
        SerializedParameterOverride customTone1;

        SerializedParameterOverride colorAdjustments;
        SerializedParameterOverride colorFilter;
        SerializedParameterOverride whiteBalance;
        SerializedParameterOverride shadowColor;
        SerializedParameterOverride highlightColor;

        SerializedParameterOverride redInOut;
        SerializedParameterOverride greenInOut;
        SerializedParameterOverride blueInOut;

        SerializedParameterOverride lift;
        SerializedParameterOverride gamma;
        SerializedParameterOverride gain;

        SerializedParameterOverride shadows;
        SerializedParameterOverride midtones;
        SerializedParameterOverride highlights;
        SerializedParameterOverride shadowHightlights;

        SerializedParameterOverride master;
        SerializedParameterOverride red;
        SerializedParameterOverride green;
        SerializedParameterOverride blue;
        SerializedParameterOverride hueVsHue;
        SerializedParameterOverride hueVsSat;
        SerializedParameterOverride satVsSat;
        SerializedParameterOverride lumVsSat;

        ClassSerializedParameterOverride customLut;
        SerializedParameterOverride customLutParam;

        static GUIContent[] s_Curves = {
            new GUIContent ("Master"),
            new GUIContent ("Red"),
            new GUIContent ("Green"),
            new GUIContent ("Blue"),
            new GUIContent ("Hue Vs Hue"),
            new GUIContent ("Hue Vs Sat"),
            new GUIContent ("Sat Vs Sat"),
            new GUIContent ("Lum Vs Sat")
        };
        static SerializedParameterOverride[] s_CurvesProperty = new SerializedParameterOverride[8];
        const int k_CustomToneCurveResolution = 48;
        const float k_CustomToneCurveRangeY = 1.025f;
        readonly Vector3[] m_RectVertices = new Vector3[4];
        readonly Vector3[] m_LineVertices = new Vector3[2];
        readonly Vector3[] m_CurveVertices = new Vector3[k_CustomToneCurveResolution];
        Rect m_CustomToneCurveRect;
        readonly HableCurve m_HableCurve = new HableCurve ();

        //debug
        SavedInt colorMixChannel;
        SavedBool channelMixerFolder;
        SavedBool liftGammaGainFolder;
        SavedBool shadowsMidtonesHighlightsFolder;
        SavedInt selectCurve;
        SavedBool colorCurveFolder;
        public override void OnEnable ()
        {
            var colorGrading = target as ColorGrading;
            tonemapper = FindClassParameterOverride (x => x.tonemapper, colorGrading.tonemapper);
            lutMode = FindClassParameterOverride (x => x.gradingMode, colorGrading.gradingMode);
            //
            customTone0 = FindParameterOverride (x => x.customTone0);
            customTone1 = FindParameterOverride (x => x.customTone1);
            colorAdjustments = FindParameterOverride (x => x.colorAdjustments);
            colorFilter = FindParameterOverride (x => x.colorFilter);

            whiteBalance = FindParameterOverride (x => x.whiteBalance);

            shadowColor = FindParameterOverride (x => x.shadowColor);
            highlightColor = FindParameterOverride (x => x.highlightColor);

            colorMixChannel = new SavedInt($"{EngineContext.sceneNameLower}.{nameof(colorMixChannel)}", 0);
            channelMixerFolder = new SavedBool($"{EngineContext.sceneNameLower}.{nameof(channelMixerFolder)}", true);
            redInOut = FindParameterOverride (x => x.redInOut);
            greenInOut = FindParameterOverride (x => x.greenInOut);
            blueInOut = FindParameterOverride (x => x.blueInOut);

            liftGammaGainFolder = new SavedBool($"{EngineContext.sceneNameLower}.{nameof(liftGammaGainFolder)}", true);
            lift = FindParameterOverride (x => x.lift);
            gamma = FindParameterOverride (x => x.gamma);
            gain = FindParameterOverride (x => x.gain);

            shadowsMidtonesHighlightsFolder = new SavedBool($"{EngineContext.sceneNameLower}.{nameof(shadowsMidtonesHighlightsFolder)}", true);
            shadows = FindParameterOverride (x => x.shadows);
            midtones = FindParameterOverride (x => x.midtones);
            highlights = FindParameterOverride (x => x.highlights);

            shadowHightlights = FindParameterOverride (x => x.shadowHightlights);

            master = FindClassParameterOverride (x => x.master, colorGrading.master);
            s_CurvesProperty[0] = master;
            red = FindClassParameterOverride (x => x.red, colorGrading.red);
            s_CurvesProperty[1] = red;
            green = FindClassParameterOverride (x => x.green, colorGrading.green);
            s_CurvesProperty[2] = green;
            blue = FindClassParameterOverride (x => x.blue, colorGrading.blue);
            s_CurvesProperty[3] = blue;

            hueVsHue = FindClassParameterOverride (x => x.hueVsHue, colorGrading.hueVsHue);
            s_CurvesProperty[4] = hueVsHue;
            hueVsSat = FindClassParameterOverride (x => x.hueVsSat, colorGrading.hueVsSat);
            s_CurvesProperty[5] = hueVsSat;
            satVsSat = FindClassParameterOverride (x => x.satVsSat, colorGrading.satVsSat);
            s_CurvesProperty[6] = satVsSat;
            lumVsSat = FindClassParameterOverride (x => x.lumVsSat, colorGrading.lumVsSat);
            s_CurvesProperty[7] = lumVsSat;

            selectCurve = new SavedInt($"{EngineContext.sceneNameLower}.{nameof(selectCurve)}", 0);
            colorCurveFolder = new SavedBool($"{EngineContext.sceneNameLower}.{nameof(colorCurveFolder)}", true);

            customLut = FindClassParameterOverride (x=> x.customLut, colorGrading.customLut);
            customLutParam = FindParameterOverride(x => x.customLutParam);
        }

        public override void OnInspectorGUI ()
        {
            PropertyField (tonemapper);
            PropertyField (lutMode);

            if (tonemapper.value.intValue == (int) TonemapperMode.UnityCustom)
            {
                CustomTone ();
            }

            if (lutMode.value.intValue == (int) GradingMode.Realtime)
            {
                PropertyField (colorAdjustments);
                PropertyField (colorFilter);
                PropertyField (whiteBalance);
                PropertyField (shadowColor);
                PropertyField (highlightColor);
                ChannelMixGUI ();
                LiftGammaGainGUI ();
                ShadowsMidtonesHighlightsGUI ();
                ColorCurveGUI ();
            }
            else if (lutMode.value.intValue == (int) GradingMode.CustomLut)
            {
                GUILayout.Space(10);
                PropertyField (customLut);
                PropertyField (customLutParam);
            }
        }

        void DrawCustomToneCurve ()
        {
            EditorGUILayout.Space ();

            // Reserve GUI space
            using (new GUILayout.HorizontalScope ())
            {
                GUILayout.Space (EditorGUI.indentLevel * 15f);
                m_CustomToneCurveRect = GUILayoutUtility.GetRect (128, 80);
            }

            if (Event.current.type != EventType.Repaint)
                return;

            // Prepare curve data
            var tone0 = customTone0.value.vector4Value;
            var tone1 = customTone1.value.vector4Value;
            float toeStrength = tone0.x;
            float toeLength = tone0.y;
            float shoulderStrength = tone0.z;
            float shoulderLength = tone0.w;
            float shoulderAngle = tone1.x;
            float gamma = tone1.y;
            m_HableCurve.Init (
                toeStrength,
                toeLength,
                shoulderStrength,
                shoulderLength,
                shoulderAngle,
                gamma
            );

            float endPoint = m_HableCurve.whitePoint;

            // Background
            m_RectVertices[0] = PointInRect (0f, 0f, endPoint);
            m_RectVertices[1] = PointInRect (endPoint, 0f, endPoint);
            m_RectVertices[2] = PointInRect (endPoint, k_CustomToneCurveRangeY, endPoint);
            m_RectVertices[3] = PointInRect (0f, k_CustomToneCurveRangeY, endPoint);
            Handles.DrawSolidRectangleWithOutline (m_RectVertices, Color.white * 0.1f, Color.white * 0.4f);

            // Vertical guides
            if (endPoint < m_CustomToneCurveRect.width / 3)
            {
                int steps = Mathf.CeilToInt (endPoint);
                for (var i = 1; i < steps; i++)
                    DrawLine (i, 0, i, k_CustomToneCurveRangeY, 0.4f, endPoint);
            }

            // Label
            Handles.Label (m_CustomToneCurveRect.position + Vector2.right, "Custom Tone Curve", EditorStyles.miniLabel);

            // Draw the acual curve
            var vcount = 0;
            while (vcount < k_CustomToneCurveResolution)
            {
                float x = endPoint * vcount / (k_CustomToneCurveResolution - 1);
                float y = m_HableCurve.Eval (x);

                if (y < k_CustomToneCurveRangeY)
                {
                    m_CurveVertices[vcount++] = PointInRect (x, y, endPoint);
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
                Handles.DrawAAPolyLine (2f, vcount, m_CurveVertices);
            }
        }

        void DrawLine (float x1, float y1, float x2, float y2, float grayscale, float rangeX)
        {
            m_LineVertices[0] = PointInRect (x1, y1, rangeX);
            m_LineVertices[1] = PointInRect (x2, y2, rangeX);
            Handles.color = Color.white * grayscale;
            Handles.DrawAAPolyLine (2f, m_LineVertices);
        }

        Vector3 PointInRect (float x, float y, float rangeX)
        {
            x = Mathf.Lerp (m_CustomToneCurveRect.x, m_CustomToneCurveRect.xMax, x / rangeX);
            y = Mathf.Lerp (m_CustomToneCurveRect.yMax, m_CustomToneCurveRect.y, y / k_CustomToneCurveRangeY);
            return new Vector3 (x, y, 0);
        }

        private void CustomTone ()
        {
            DrawCustomToneCurve ();
            PropertyField (customTone0);
            PropertyField (customTone1);
        }

        private void ChannelMixGUI ()
        {
            bool folder = ToolsUtility.FolderPorperty (channelMixerFolder, "ChannelMixer");
            if (folder)
            {
                int currentChannel = colorMixChannel.Value;

                EditorGUI.BeginChangeCheck ();
                {
                    using (new EditorGUILayout.HorizontalScope ())
                    {
                        if (GUILayout.Toggle (currentChannel == 0, EditorGUIUtility.TrTextContent ("Red", "Red output channel."), EditorStyles.miniButtonLeft)) currentChannel = 0;
                        if (GUILayout.Toggle (currentChannel == 1, EditorGUIUtility.TrTextContent ("Green", "Green output channel."), EditorStyles.miniButtonMid)) currentChannel = 1;
                        if (GUILayout.Toggle (currentChannel == 2, EditorGUIUtility.TrTextContent ("Blue", "Blue output channel."), EditorStyles.miniButtonRight)) currentChannel = 2;
                    }
                }
                if (EditorGUI.EndChangeCheck ())
                    GUI.FocusControl (null);

                colorMixChannel.Value = currentChannel;

                if (currentChannel == 0)
                {
                    PropertyField (redInOut);
                }
                else if (currentChannel == 1)
                {
                    PropertyField (greenInOut);
                }
                else
                {
                    PropertyField (blueInOut);
                }
            }

        }

        private void ResetTrackBall (SerializedParameterOverride property)
        {
            if (GUILayout.Button ("R", GUILayout.MaxWidth (20)))
            {
                if (property.decorator != null)
                {
                    property.decorator.ResetValue (property, property.decoratorAttr);
                }
            }
        }

        private void TrackBall (SerializedParameterOverride value, string str, Func<Vector4, Vector3> computeFunc)
        {
            var param = AttributeDecorator.DebugRuntimeParam (value);
            var v = value.value.vector4Value;
            AttributeDecorator.DebugValue<Vector4, Vector4Param> (param, ref v);
            var overrideState = value.overrideState.boolValue;
            TrackballDecorator.trackballUIDrawer.OnGUI (ref v, ref overrideState, EditorGUIUtility.TrTextContent (str), computeFunc, true);
            value.value.vector4Value = v;
            value.overrideState.boolValue = overrideState;
            ResetTrackBall (value);
        }

        private void LiftGammaGainGUI ()
        {
            bool folder = ToolsUtility.FolderPorperty (liftGammaGainFolder, "LiftGammaGain");
            if (folder)
            {
                using (new EditorGUILayout.HorizontalScope ())
                {
                    TrackBall (lift, "Lift", TrackballDecorator.GetLiftValue);
                    GUILayout.Space (4f);
                    TrackBall (gamma, "Gamma", TrackballDecorator.GetLiftValue);
                    GUILayout.Space (4f);
                    TrackBall (gain, "Gain", TrackballDecorator.GetLiftValue);
                }
            }
        }

        private void ShadowsMidtonesHighlightsGUI ()
        {
            bool folder = ToolsUtility.FolderPorperty (shadowsMidtonesHighlightsFolder, "ShadowsMidtonesHighlights");
            if (folder)
            {
                using (new EditorGUILayout.HorizontalScope ())
                {
                    TrackBall (shadows, "Shadows", TrackballDecorator.GetWheelValue);
                    GUILayout.Space (4f);
                    TrackBall (midtones, "Midtones", TrackballDecorator.GetWheelValue);
                    GUILayout.Space (4f);
                    TrackBall (highlights, "Highlights", TrackballDecorator.GetWheelValue);
                }
            }
            EditorGUILayout.Space ();
            // using (new GUILayout.HorizontalScope ())
            // {
            //     GUILayout.Space (EditorGUI.indentLevel * 15f);
            //     m_CurveRect = GUILayoutUtility.GetRect (128, 80);
            // }

            // EditorGUILayout.Space ();
            PropertyField (shadowHightlights);
        }

        private void ColorCurveGUI ()
        {
            float w = EditorGUIUtility.currentViewWidth;
            EditorGUILayout.BeginHorizontal ();
            bool folder = ToolsUtility.FolderPorperty (colorCurveFolder, "Color Curves");
            EditorGUI.BeginChangeCheck ();
            var curveEditingId = EditorGUILayout.Popup (selectCurve.Value, s_Curves, GUILayout.MaxWidth (200f));
            if (EditorGUI.EndChangeCheck ())
            {
                selectCurve.Value = curveEditingId;
            }
            EditorGUILayout.EndHorizontal ();
            if (folder)
            {
                EditorCommon.BeginGroup ("", true, w, 120);
                var property = s_CurvesProperty[curveEditingId];
                if (property != null)
                    PropertyField (property);
                EditorCommon.EndGroup ();

            }

        }
    }
}