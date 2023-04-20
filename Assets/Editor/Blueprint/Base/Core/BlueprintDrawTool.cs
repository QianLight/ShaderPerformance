using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    public class DrawTool
    {
        static Dictionary<GUIStyle, Dictionary<string, Vector2>> SizeCacheDic = new Dictionary<GUIStyle, Dictionary<string, Vector2>>();
        public static Vector2 CalculateTextSize(string text, GUIStyle style)
        {
            if (text == null) return Vector2.zero;
            if (!SizeCacheDic.ContainsKey(style))
                SizeCacheDic.Add(style, new Dictionary<string, Vector2>());
            if (SizeCacheDic[style].Count > 1000) SizeCacheDic[style].Clear();
            if (!SizeCacheDic[style].ContainsKey(text))
                SizeCacheDic[style].Add(text, style.CalcSize(new GUIContent(text)));
            return SizeCacheDic[style][text];
        }

        public static void DrawLabel(Rect rect, string label, GUIStyle style, TextAnchor alignment)
        {
            var oldAlignment = style.alignment;
            style.alignment = alignment;
            GUI.Label(rect, label, style);
            style.alignment = oldAlignment;
        }

        public static void DrawMultiLineText(Rect rect, string lable, GUIStyle style, GUILayoutOption[] param)
        {
            
        }

        public static void DrawStretchBox(Rect scale, GUIStyle style, float offset)
        {
            DrawExpandableBox(scale, style, string.Empty, offset);
        }

        public static void DrawStretchBox(Rect scale, object nodeBackground, Rect offset)
        {
            //var rectOffset = new RectOffset(Mathf.RoundToInt(offset.x), Mathf.RoundToInt(offset.y), Mathf.RoundToInt(offset.width), Mathf.RoundToInt(offset.height));

            var rectOffset = new RectOffset((int)offset.x, (int)offset.y, (int)offset.width, (int)offset.height);

            DrawExpandableBox(scale, (GUIStyle)nodeBackground,
                string.Empty, rectOffset);
        }

        public static void DrawExpandableBox(Rect rect, object style, string text, RectOffset offset)
        {
            var guiStyle = (GUIStyle)style;
            var oldBorder = guiStyle.border;
            GUI.Box(rect, text, guiStyle);
            guiStyle.border = oldBorder;
        }

        public static void DrawExpandableBox(Rect rect, GUIStyle style, string text, float offset = 12)
        {
            var oldBorder = style.border;
            //style.border = new RectOffset((int)offset, (int)offset, (int)offset, (int)offset);
            GUI.Box(rect, text, style);
            style.border = oldBorder;
        }

        public static void DrawNodeHeader(Rect boxRect, GUIStyle style, Texture2D image)
        {
            if (image != null) style.ForNormalState(image);

            DrawStretchBox(boxRect, style, 20f);
        }

        public static void DrawTextbox(Rect rect, string value, GUIStyle style, TextAnchor alignment, Action<string, bool> valueChangedAction)
        {
            GUI.SetNextControlName("EditingField");
            var oldAlignment = style.alignment;
            style.alignment = alignment;
            var newName = EditorGUI.TextField(rect, value, style);
            style.alignment = oldAlignment;

            valueChangedAction(newName, false);

            if (Event.current.keyCode == KeyCode.Return)
            {
                valueChangedAction(value, true);
            }
            EditorGUI.FocusTextInControl("EditingField");
        }

        public static void DrawBezier(Vector3 start, Vector3 end, Color color)
        {
            Vector3 startTangent = start + new Vector3(100f, (end - start).y * 0.01f);
            Vector3 endTangent = end - new Vector3(100f, (end - start).y * 0.01f);

            Handles.DrawBezier(start, end, startTangent, endTangent, color, null, 3);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <param name="lineColor"></param>
        /// <param name="pic"></param>
        /// <param name="widthScale"></param>
        /// <param name="mode">
        /// 模式 1 =》 默认 直线
        /// 模式 2 =》 曲线
        /// </param>
        public static void DrawConnectionLine(Vector3 start, Vector3 end, Color lineColor, Texture2D pic, float widthScale)
        {
            //改为贝塞尔曲线
            float tangentOffset = Mathf.Min(100, Mathf.Abs(start.y - end.y)) * 0.5f;//用纵向插值决定控制点偏移
            Vector3 startTangent = new Vector3(start.x + tangentOffset, start.y, start.z);
            Vector3 endTangent = new Vector3(end.x - tangentOffset, end.y, end.z);
            Handles.DrawBezier(start, end, startTangent, endTangent, lineColor, pic, 2 * widthScale);
        }
        public static void DrawConnectionLine(Vector3 start, Vector3 end, Color lineColor, Texture2D pic, float widthScale, float deltaX, float deltaY)
        {
            deltaX *= widthScale;
            deltaY *= widthScale;

            List<Vector3> points = new List<Vector3>();
            points.Add(start);
            points.Add(new Vector3(start.x + deltaX, start.y));
            if (start.x + deltaX * 2 < end.x)
            {
                points.Add(new Vector3(start.x + deltaX, end.y));
            }
            else
            {
                points.Add(new Vector3(start.x + deltaX, start.y > end.y ? Math.Min(start.y, end.y + deltaY) : Math.Max(start.y, end.y - deltaY)));
                points.Add(new Vector3(end.x - deltaX, start.y > end.y ? Math.Min(start.y, end.y + deltaY) : Math.Max(start.y, end.y - deltaY)));
                points.Add(new Vector3(end.x - deltaX, end.y));
            }
            points.Add(end);
            {
                for (int i = 1; i < points.Count; ++i)
                {
                    if (points[i - 1] == points[i]) continue;
                    Handles.DrawBezier(points[i - 1], points[i], points[i - 1], points[i], lineColor, pic, 2 * widthScale);
                }
            }
        }

        public static void DrawIcon(Rect rect, string textureName)
        {
            GUI.DrawTexture(rect, BlueprintStyles.GetSkinTexture(textureName), ScaleMode.ScaleToFit);
        }
    }
}