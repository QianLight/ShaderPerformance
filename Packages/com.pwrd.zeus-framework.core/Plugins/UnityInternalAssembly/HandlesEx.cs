/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor
{
    public class HandlesEx
    {
        public static Color lineTransparency = new Color(1f, 1f, 1f, 0.75f);

        private static Color backColor = Handles.color;
        public static void BeginColor(Color color)
        {
            backColor = UnityEditor.Handles.color;
            UnityEditor.Handles.color = color;
        }
        public static void EndColor()
        {
            UnityEditor.Handles.color = backColor;
        }

        public static bool BeginLineDrawing(Matrix4x4 matrix, bool dottedLines, int mode)
        {
            bool result;
            if (Event.current.type != EventType.Repaint)
            {
                result = false;
            }
            else
            {
                Color c = Handles.color * lineTransparency;
                if (dottedLines)
                {
                    HandleUtility.ApplyDottedWireMaterial(Handles.zTest);
                }
                else
                {
                    HandleUtility.ApplyWireMaterial(Handles.zTest);
                }
                GL.PushMatrix();
                GL.MultMatrix(matrix);
                GL.Begin(mode);
                GL.Color(c);
                result = true;
            }
            return result;
        }

        public static void EndLineDrawing()
        {
            GL.End();
            GL.PopMatrix();
        }

        public static void DrawRectangleOutline(Rect rect, Color color)
        {
            BeginColor(color);
            if (BeginLineDrawing(Handles.matrix, false, 1))
            {
                var points = new Vector2[]
                {
                    rect.position,
                    new Vector2(rect.x + rect.width, rect.y),
                    rect.position + rect.size,
                    new Vector3(rect.x, rect.y + rect.height)
                };

                int count = points.Length;
                for (int i = 0; i < count; i++)
                {
                    GL.Vertex(points[i]);
                    GL.Vertex(points[(i + 1) % count]);
                }

                EndLineDrawing();
            }
            EndColor();
        }

        public static void DrawGrid(Rect rect, float spacing, Color color)
        {
            DrawGrid(rect, spacing, spacing, color);
        }

        public static void DrawGrid(Rect rect, float hSpacing, float vSpacing, Color color)
        {
            if (vSpacing > 0 && hSpacing > 0 && color.a > 0)
            {
                BeginColor(color);
                if (BeginLineDrawing(Handles.matrix, false, 1))
                {
                    //  竖线
                    if (rect.width > hSpacing)
                    {
                        for (float x = rect.x; x < rect.x + rect.width; x += hSpacing)
                        {
                            GL.Vertex(new Vector2(x, rect.y));
                            GL.Vertex(new Vector2(x, rect.y + rect.height));
                        }
                    }

                    //  横线
                    if (rect.height > vSpacing)
                    {
                        for (float y = rect.y; y < rect.y + rect.height; y += vSpacing)
                        {
                            GL.Vertex(new Vector2(rect.x, y));
                            GL.Vertex(new Vector2(rect.x + rect.width, y));
                        }
                    }
                    EndLineDrawing();
                }
                EndColor();
            }
        }

        private static Vector2 _Internal_GetTrangent(Vector2 from, Vector2 to)
        {
            Vector2 directionn = to - from;

            float offset = directionn.magnitude / 3;

            if (Mathf.Abs(directionn.x) < Mathf.Abs(directionn.y))
            {
                directionn.x = 0;
            }
            else
            {
                directionn.y = 0;
            }

            directionn = directionn.normalized;

            return directionn * offset + from;
        }

        private static Vector2 _Internal_GetArowwDirection(Vector2 from, Vector2 to)
        {
            Vector2 directionn = to - from;
            float xValue = Mathf.Abs(directionn.x);
            float yValue = Mathf.Abs(directionn.y);

            if ( xValue< yValue)
            {
                directionn.x = 0;
                directionn.y /= (yValue + float.Epsilon);
            }
            else
            {
                directionn.y = 0;
                directionn.x /= (xValue + float.Epsilon);
            }
            return directionn;
        }

        public static void DrawBezier(Vector2 start, Vector2 end, Color lineColor, float lineWidth = 1, bool widthArrow = true)
        {
            if (start == end) return;   //  两个点重合，跳过绘制

            lineColor *= lineTransparency;
            var startTrangent = _Internal_GetTrangent(start, end);
            var endTrangent = _Internal_GetTrangent(end, start);
            Handles.DrawBezier(start, end, startTrangent, endTrangent, lineColor, Texture2D.whiteTexture, lineWidth);
            if (widthArrow)
            {
                BeginColor(lineColor);
                DrawArrow(end, _Internal_GetArowwDirection(start, end), lineWidth);
                EndColor();
            }
        }

        public static void DrawArrow(Vector2 position, Vector2 direction, float lineWidth = 1)
        {
            direction = -direction.normalized;
            Vector2 v1 = new Vector2(direction.x * 0.866f - direction.y * 0.4f, direction.x * 0.4f + direction.y * 0.866f) * 10;
            Vector2 v2 = new Vector2(direction.x * 0.866f + direction.y * 0.4f, direction.x * -0.4f + direction.y * 0.866f) * 10;
            Handles.DrawAAPolyLine(lineWidth * 2, position + v1, position, position + v2);
        }

        public static void DrawSliderTexture(Rect rect, float value, Color fillColor, Color outlineColor)
        {
            var outlineRect = rect;
            rect.width *= Mathf.Clamp01(value);
            GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 1, fillColor, 0, 0);
            DrawRectangleOutline(outlineRect, outlineColor);
        }
    }
}
#endif
