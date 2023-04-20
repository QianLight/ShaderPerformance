using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
namespace CFEngine.Editor
{
    public abstract class CommonToolTemplate : ScriptableObject
    {
        public virtual void OnInit () { }
        public virtual void OnUninit () { }

        public virtual void DrawGUI (ref Rect rect) { }
        public virtual void DrawSceneGUI ()
        {

        }
        public virtual void DrawGizmos ()
        {

        }
        public virtual void Update ()
        {

        }
    }
    public struct TwoOpInfo
    {
        public bool enable;
        public bool hasOp;

        public void OnButtonGUI (string op0, string op1, float width)
        {
            if (GUILayout.Button (op0, GUILayout.MaxWidth (width))) { hasOp = true; enable = true; }
            if (GUILayout.Button (op1, GUILayout.MaxWidth (width))) { hasOp = true; enable = false; }
        }

        public bool NeedProcess ()
        {
            return hasOp;
        }
        public bool Process (ref bool value)
        {
            if (hasOp)
            {
                value = enable;
                return false;
            }
            return true;
        }
    }
    public struct DeleteInfo
    {
        public int removeIndex;
        public void BeginDelete ()
        {
            removeIndex = -1;
        }
        public void RemveButton (int i)
        {
            if (GUILayout.Button ("Delete", GUILayout.MaxWidth (80)))
            {
                removeIndex = i;
            }
        }
        public bool EndDelete (IList list)
        {
            if (removeIndex >= 0)
            {
                list.RemoveAt (removeIndex);
                return true;
            }
            return false;
        }
    }
    public class ToolsUtility
    {
        public delegate void MouseDownCb (bool left);
        public delegate void MouseDragCb ();
        public delegate void MouseUpCb (Vector2Int chunkSrcPoint, Vector2Int chunkEndPoint);
        public delegate void RepaintCb ();

        public class GridContext
        {
            public int hLines;
            public int vLines;
            public Rect gridRect = Rect.zero;
            public Rect innerRect = Rect.zero;
            public RectOffset padding = new RectOffset (10, 10, 10, 10);
            public Color bgColor = new Color (0.15f, 0.15f, 0.15f, 1f);
            public Color rectOutlineColor = new Color (0.8f, 0.8f, 0.8f, 0.5f);
            public Color handleColor = new Color (1f, 1f, 1f, 0.05f);
            public Vector2 textSize = new Vector2 (20, 20);
            public int gridOffsetH;
            public int gridOffsetV;
            public bool doubleClick = false;
            public Vector2 mousePos;
            public Rect dragRect;
            public Vector2Int clickSrcChunk = new Vector2Int (-1, -1);
            public MouseDownCb mouseDownCb;
            public MouseDragCb mouseDragCb;
            public MouseUpCb mouseUpCb;
            public RepaintCb repaintCb;
            public bool receiveEvent = true;

            public bool drawDragRect = true;
        }
        public static void PrepareGrid (GridContext context, int hLines, int vLines, int hSize = 20, int vSize = 20, int padding = 10)
        {
            context.hLines = hLines;
            context.vLines = vLines;
            context.gridRect = GUILayoutUtility.GetAspectRect (2f);
            context.gridRect.width = hLines * hSize + padding * 2;
            context.gridRect.height = vLines * vSize + padding * 2;
            context.padding.left = padding;
            context.padding.right = padding;
            context.padding.top = padding;
            context.padding.bottom = padding;
            context.innerRect = context.padding.Remove (context.gridRect);
            context.gridOffsetH = Mathf.FloorToInt (context.innerRect.width / hLines);
            context.gridOffsetV = Mathf.FloorToInt (context.innerRect.height / vLines);
        }

        public static void DrawGrid (GridContext context)
        {
            // Background                        
            EditorGUI.DrawRect (context.gridRect, context.bgColor);
            // Bounds
            Handles.color = Color.white * (GUI.enabled ? 1f : 0.5f);
            Handles.DrawSolidRectangleWithOutline (context.innerRect, Color.clear, context.rectOutlineColor);

            Vector2 centerPos = context.innerRect.position;
            centerPos.y += context.innerRect.height;
            // Grid setup
            Handles.color = context.handleColor;
            float halfGridOffset0 = context.gridOffsetH * 0.6f;
            float halfGridOffset1 = context.gridOffsetV * 0.8f;

            int gridPadding = ((int) (context.innerRect.width) % context.hLines) / 2;
            for (int i = 1; i < context.hLines; i++)
            {
                float halfGridOffset = i < 11 ? halfGridOffset0 : halfGridOffset1;
                var offset = i * Vector2.right * context.gridOffsetH;
                offset.x += gridPadding;
                Handles.DrawLine (context.innerRect.position + offset, new Vector2 (context.innerRect.x, context.innerRect.yMax - 1) + offset);
                var textoffset = i * Vector2.right * context.gridOffsetH - Vector2.right * halfGridOffset;
                Rect textRect = new Rect (centerPos + textoffset, context.textSize);
                EditorGUI.LabelField (textRect, (i - 1).ToString ());
            }
            var lastTextoffset = context.hLines * Vector2.right * context.gridOffsetH - Vector2.right * halfGridOffset1;
            EditorGUI.LabelField (new Rect (centerPos + lastTextoffset, context.textSize), (context.hLines - 1).ToString ());

            gridPadding = ((int) (context.innerRect.height) % context.vLines) / 2;
            for (int i = 1; i < context.vLines; i++)
            {
                float halfGridOffset = i < 11 ? halfGridOffset0 : halfGridOffset1;
                var offset = i * Vector2.up * context.gridOffsetV;
                offset.y += gridPadding;
                Handles.DrawLine (context.innerRect.position + offset, new Vector2 (context.innerRect.xMax - 1, context.innerRect.y) + offset);
                var textoffset = (i - 1) * Vector2.up * context.gridOffsetV + Vector2.up * halfGridOffset;
                Rect textRect = new Rect (centerPos - textoffset, context.textSize);
                EditorGUI.LabelField (textRect, (i - 1).ToString ());
            }
            lastTextoffset = (context.vLines - 1) * Vector2.up * context.gridOffsetV + Vector2.up * halfGridOffset1;
            EditorGUI.LabelField (new Rect (centerPos - lastTextoffset, context.textSize), (context.vLines - 1).ToString ());
        }

        public static void DrawBlock (GridContext context, int hIndex, int vIndex, Color color, int padding = 5)
        {
            Rect rect = new Rect ();

            rect.xMin = hIndex * context.gridOffsetH + padding;
            rect.yMin = vIndex * context.gridOffsetV + padding;
            rect.width = context.gridOffsetH - padding * 2;
            rect.height = context.gridOffsetV - padding * 2;
            rect.position += context.innerRect.position;
            EditorGUI.DrawRect (rect, color);
        }
        public static Vector2Int CalcGridIndex (GridContext gridContext, Vector2 mousePosition)
        {
            Vector2 pos = mousePosition - gridContext.innerRect.position;
            int xIndex = Mathf.FloorToInt (pos.x / gridContext.gridOffsetH);
            int zIndex = (gridContext.vLines - 1) - Mathf.FloorToInt (pos.y / gridContext.gridOffsetV);
            return new Vector2Int (xIndex, zIndex);
        }

        public static void DrawGrid (GridContext gridContext, int hLines, int vLines, int hSize, int vSize)
        {
            ToolsUtility.PrepareGrid (gridContext, hLines, vLines, hSize, vSize);
            var e = Event.current;
            if (gridContext.receiveEvent)
            {

                if (e.type == EventType.MouseDown)
                {
                    gridContext.doubleClick = e.clickCount == 2;
                    bool leftMouse = e.button == 0;
                    // bool rightMouse = e.button == 1;

                    if (gridContext.innerRect.Contains (e.mousePosition))
                    {
                        gridContext.mousePos = e.mousePosition;
                        gridContext.dragRect = new Rect (0, 0, 0, 0);
                        Vector2Int pos = CalcGridIndex (gridContext, e.mousePosition);
                        if (leftMouse)
                        {
                            gridContext.clickSrcChunk.x = pos.x;
                            gridContext.clickSrcChunk.y = pos.y;
                        }
                        else
                        {
                            gridContext.clickSrcChunk.x = -1;
                            gridContext.clickSrcChunk.y = -1;
                        }
                        if (gridContext.mouseDownCb != null)
                        {
                            gridContext.mouseDownCb (leftMouse);
                        }
                    }
                    ToolTemplate.DoRepaint ();
                }
                else if (e.type == EventType.MouseDrag)
                {
                    if (gridContext.innerRect.Contains (e.mousePosition) && gridContext.clickSrcChunk.x >= 0 && gridContext.clickSrcChunk.y >= 0)
                    {
                        Vector2 pos = e.mousePosition;
                        float xMin = pos.x > gridContext.mousePos.x ? gridContext.mousePos.x : pos.x;
                        float yMin = pos.y > gridContext.mousePos.y ? gridContext.mousePos.y : pos.y;
                        float xMax = pos.x < gridContext.mousePos.x ? gridContext.mousePos.x : pos.x;
                        float yMax = pos.y < gridContext.mousePos.y ? gridContext.mousePos.y : pos.y;
                        gridContext.dragRect = new Rect (xMin, yMin, xMax - xMin, yMax - yMin);
                        ToolTemplate.DoRepaint ();
                    }
                }
                else if (e.type == EventType.MouseUp)
                {
                    bool leftMouse = e.button == 0;
                    if (leftMouse && gridContext.innerRect.Contains (e.mousePosition) && gridContext.clickSrcChunk.x >= 0 && gridContext.clickSrcChunk.y >= 0)
                    {
                        float dist = Vector2.Distance (e.mousePosition, gridContext.mousePos);
                        if (dist > 0.1f || gridContext.doubleClick)
                        {
                            Vector2 pos = e.mousePosition - gridContext.innerRect.position;
                            int xIndex = Mathf.FloorToInt (pos.x / gridContext.gridOffsetH);
                            int zIndex = (gridContext.vLines - 1) - Mathf.FloorToInt (pos.y / gridContext.gridOffsetV);

                            int srcX = xIndex < gridContext.clickSrcChunk.x ? xIndex : gridContext.clickSrcChunk.x;
                            int srcZ = zIndex < gridContext.clickSrcChunk.y ? zIndex : gridContext.clickSrcChunk.y;
                            int endX = xIndex > gridContext.clickSrcChunk.x ? xIndex : gridContext.clickSrcChunk.x;
                            int endZ = zIndex > gridContext.clickSrcChunk.y ? zIndex : gridContext.clickSrcChunk.y;

                            Vector2Int chunkSrcPoint = new Vector2Int (srcX, srcZ);
                            Vector2Int chunkEndPoint = new Vector2Int (endX, endZ);
                            if (gridContext.mouseUpCb != null)
                            {
                                gridContext.mouseUpCb (chunkSrcPoint, chunkEndPoint);
                            }
                        }

                    }
                    gridContext.clickSrcChunk.x = -1;
                    gridContext.clickSrcChunk.y = -1;
                    ToolTemplate.DoRepaint ();
                }
            }
            if (e.type == EventType.Repaint)
            {
                ToolsUtility.DrawGrid (gridContext);
                if (gridContext.repaintCb != null)
                {
                    gridContext.repaintCb ();
                }

                //drag rect
                if (gridContext.drawDragRect && gridContext.clickSrcChunk.x >= 0 && gridContext.clickSrcChunk.y >= 0)
                    Handles.DrawSolidRectangleWithOutline (gridContext.dragRect, Color.white, new Color (1f, 1f, 1f, 1f));
            }
        }

        public static void InitListContext (ref ListElementContext lec, float lineHeight)
        {
            lec.rect.height = lineHeight - 4;
            lec.height = lineHeight;
            lec.lineHeight = lineHeight;
            lec.lineStart = lec.rect.x;
            lec.lineOffset = 0;
            lec.lastWidth = -1;
            if (lec.draw)
                lec.width = lec.rect.width;
        }
        public static void NewLine (ref ListElementContext lec, float xOffset)
        {
            lec.rect.x = lec.lineStart + xOffset;
            lec.rect.y += lec.lineHeight;
            lec.rect.height = lec.lineHeight - 4;
            lec.lineOffset = xOffset;
            lec.height += lec.lineHeight;
        }
        public static void NewLine (ref ListElementContext lec, float xOffset, float yOffset)
        {
            lec.rect.x = lec.lineStart + xOffset;
            lec.rect.y += yOffset;
            lec.rect.height = yOffset - 4;
            lec.lineOffset = xOffset;
            lec.height += yOffset;
        }
        public static void NewLineWithOffset (ref ListElementContext lec)
        {
            lec.rect.x = lec.lineStart + lec.lineOffset;
            lec.rect.y += lec.lineHeight;
            lec.rect.height = lec.lineHeight - 4;
            lec.height += lec.lineHeight;
        }
        public static void LineSpace (ref ListElementContext lec, float h = 2)
        {
            lec.rect.y += h;
            lec.height += h;
        }

        public static void LineOffset (ref ListElementContext lec, float xOffset)
        {
            lec.rect.x = lec.lineStart + xOffset;
            lec.lineOffset = xOffset;
        }
        public static void NewRect (ref ListElementContext context, float width, bool reset = false)
        {
            if (reset)
                context.lastWidth = -1;
            if (context.lastWidth > 0)
                context.rect.x += context.lastWidth + 5;
            context.rect.width = width;
            context.lastWidth = width;
        }

        public static int FixLineCount (ref ListElementContext lec, float width = 140)
        {
            int lineCount = (int) (lec.width / width);
            if (lineCount <= 0)
                lineCount = 1;
            return lineCount;
        }

        public static bool FixPerLine (ref ListElementContext lec, int lineCount, ref int perLineCount)
        {
            bool reset = false;
            if (perLineCount == lineCount)
            {
                ToolsUtility.NewLineWithOffset (ref lec);
                perLineCount = 0;
                reset = true;
            }
            perLineCount++;
            return reset;
        }
        public static void NewRectLabel (ref ListElementContext context, string label, float widthText, float width, bool reset = false)
        {
            if (!string.IsNullOrEmpty (label))
            {
                NewRect (ref context, widthText, reset);
                if (context.draw)
                    EditorGUI.LabelField (context.rect, label);
                NewRect (ref context, width);
            }
            else
            {
                NewRect (ref context, width, reset);
            }

        }
        public static void Foldout (ref ListElementContext context, ref bool folder, string str, float width, bool reset = false)
        {
            NewRect (ref context, width, reset);
            if (context.draw)
            {
                folder = EditorGUI.Foldout (context.rect, folder, str);
            }
        }

        public static bool Foldout (ref ListElementContext context, FolderConfig folder, string path, string str, float width, bool reset = false)
        {
            bool isFolder = folder.IsFolder (path);
            NewRect (ref context, width, reset);
            if (context.draw)
            {
                isFolder = EditorGUI.Foldout (context.rect, isFolder, str);
                folder.SetFolder (path, isFolder);
            }
            return isFolder;
        }
        public static void SHButton (ref ListElementContext context, ref bool folder, bool reset = false)
        {
            NewRect (ref context, 80, reset);
            if (context.draw)
            {
                if (GUI.Button (context.rect, folder? "Hide": "Show"))
                {
                    folder = !folder;
                }
            }
        }

        public static bool SHButton (ref ListElementContext context, FolderConfig folder, string path, bool reset = false)
        {
            bool isFolder = folder.IsFolder (path);
            NewRect (ref context, 80, reset);
            if (context.draw)
            {
                if (GUI.Button (context.rect, isFolder? "Hide": "Show"))
                {
                    isFolder = !isFolder;
                }
                folder.SetFolder (path, isFolder);
            }
            return isFolder;
        }

        public static bool Button (ref ListElementContext context, string str, float width, bool reset = false)
        {
            NewRect (ref context, width, reset);
            if (context.draw)
            {
                if (GUI.Button (context.rect, str))
                {
                    return true;
                }
            }
            return false;
        }

        public static void Label (ref ListElementContext context, string str, float width, bool reset = false)
        {
            NewRect (ref context, width, reset);
            if (context.draw)
            {
                EditorGUI.LabelField (context.rect, str);
            }
        }

        public static void MultiFieldPrefixLabel (ref ListElementContext context, GUIContent str, float width, int id, int columns, bool reset = false)
        {
            NewRect (ref context, width, reset);
            if (context.draw)
            {
                RotEditor.MultiFieldPrefixLabel (context.rect, id, str, columns);
            }
        }

        public static bool TextField (ref ListElementContext context, string label, float texWidth, ref string str, float width, bool reset = false)
        {
            NewRectLabel (ref context, label, texWidth, width, reset);
            if (context.draw)
            {
                EditorGUI.BeginChangeCheck ();
                str = EditorGUI.TextField (context.rect, str);
                if (EditorGUI.EndChangeCheck ())
                {
                    return true;
                }
            }
            return false;
        }
        public static void ColorField (ref ListElementContext context, string label, float texWidth, ref Color color, float width, bool reset = false)
        {
            NewRectLabel (ref context, label, texWidth, width, reset);
            if (context.draw)
            {
                color = EditorGUI.ColorField (context.rect, color);
            }
        }

        public static void ColorField (ref ListElementContext context, string label, float texWidth, ref Color color, bool showAlpha, bool hdr, float width, bool reset = false)
        {
            NewRectLabel (ref context, label, texWidth, width, reset);
            if (context.draw)
            {
                color = EditorGUI.ColorField (context.rect, GUIContent.none, color, false, showAlpha, hdr);
            }
        }
        public static bool Toggle (ref ListElementContext context, string label, float texWidth, ref bool enable, bool reset = false)
        {
            NewRect (ref context, texWidth, reset);
            if (context.draw)
            {
                bool oldEnable = enable;
                enable = EditorGUI.ToggleLeft (context.rect, label, enable);
                return oldEnable != enable;
            }
            return false;
        }

        public static void Popup (ref ListElementContext context, string label, float texWidth, ref int index, string[] text, float width, bool reset = false)
        {
            NewRectLabel (ref context, label, texWidth, width, reset);
            if (context.draw)
            {
                index = EditorGUI.Popup (context.rect, index, text);
            }
        }

        //c# 6
        // public static void EnumPopup (ref ListElementContext context, string label, float texWidth, ref Enum selected, float width, bool reset = false)
        // {
        //     NewRectLabel (ref context, label, texWidth, width, reset);
        //     if (context.draw)
        //     {
        //         selected = EditorGUI.EnumPopup (context.rect, selected);
        //     }
        // }
        //C# 7.3
        public static void EnumPopup<T> (ref ListElementContext context, string label, float texWidth, ref T selected, float width, bool reset = false)
        where T : Enum
        {
            NewRectLabel (ref context, label, texWidth, width, reset);
            if (context.draw)
            {
                selected = (T) EditorGUI.EnumPopup (context.rect, selected);
            }
        }

        public static void EnumFlagsField<T> (ref ListElementContext context, string label, float texWidth, ref T selected, float width, bool reset = false)
            where T : Enum
        {
            NewRectLabel (ref context, label, texWidth, width, reset);
            if (context.draw)
            {
                selected = (T)EditorGUI.EnumFlagsField (context.rect, selected);
            }
        }

        public static void VectorField (ref ListElementContext context, string label, float texWidth, ref Vector3 v, float width, bool reset = false)
        {
            NewRectLabel (ref context, label, texWidth, width, reset);
            if (context.draw)
            {
                v = EditorGUI.Vector3Field (context.rect, "", v);
            }
        }

        public static void VectorField (ref ListElementContext context, string label, float texWidth, ref Vector4 v, float width, bool reset = false)
        {
            NewRectLabel (ref context, label, texWidth, width, reset);
            if (context.draw)
            {
                v = EditorGUI.Vector4Field (context.rect, "", v);
            }
        }
        public static void FloatField (ref ListElementContext context, string label, float texWidth, ref float v, float width, bool reset = false)
        {
            NewRectLabel (ref context, label, texWidth, width, reset);
            if (context.draw)
            {
                v = EditorGUI.FloatField (context.rect, v);
            }
        }

        public static void MultiFloatField (ref ListElementContext context, GUIContent[] labels, float[] floats, float width, bool reset = false)
        {
            NewRect (ref context, width, reset);
            if (context.draw)
            {
                EditorGUI.MultiFloatField (context.rect, labels, floats);
            }
        }

        public static void IntField (ref ListElementContext context, string label, float texWidth, ref int v, float width, bool reset = false)
        {
            NewRectLabel (ref context, label, texWidth, width, reset);
            if (context.draw)
            {
                v = EditorGUI.IntField (context.rect, v);
            }
        }
        public static void IntSlider (ref ListElementContext context, string label, float texWidth, ref int v, int min, int max, float width, bool reset = false)
        {
            NewRectLabel (ref context, label, texWidth, width, reset);
            if (context.draw)
            {
                v = EditorGUI.IntSlider (context.rect, v, min, max);
            }
        }
        public static void Slider (ref ListElementContext context, string label, float texWidth, ref float v, float min, float max, float width, bool reset = false)
        {
            NewRectLabel (ref context, label, texWidth, width, reset);
            if (context.draw)
            {
                v = EditorGUI.Slider (context.rect, v, min, max);
            }
        }
        public static void ObjectField<T> (ref ListElementContext context,
            string str, float texWidth, ref T obj, Type type, float width, bool reset = false)
        where T : UnityEngine.Object
        {
            NewRectLabel (ref context, str, texWidth, width, reset);
            if (context.draw)
            {
                Rect r = context.rect;
                r.height = EditorGUIUtility.singleLineHeight;
                obj = EditorGUI.ObjectField (r, obj, type, false) as T;
            }
        }

        public static void CopyButton<E> (ref ListElementContext context, ref E copyElem, E currentElem)
        where E : BaseCopy<E>, new ()
        {
            NewRect (ref context, 80);
            if (context.draw)
            {
                if (copyElem == currentElem)
                {
                    if (GUI.Button (context.rect, "Copyed"))
                    {
                        copyElem = null;
                    }
                }
                else
                {
                    if (copyElem == null)
                    {
                        if (GUI.Button (context.rect, "Copy"))
                        {
                            copyElem = currentElem;
                        }
                    }
                    else
                    {
                        if (GUI.Button (context.rect, "Paste"))
                        {
                            currentElem.Copy (copyElem);
                            copyElem = null;
                        }
                    }
                }
            }

        }

        public static int BeginDelete ()
        {
            return -1;
        }

        public static void DeleteButton (ref int deleteIndex, int index, bool waringing = false)
        {
            if (GUILayout.Button ("Delete", GUILayout.MaxWidth (80)))
            {
                if (!waringing || EditorUtility.DisplayDialog ("Warining", "IsDelete?", "OK", "Cancel"))
                {
                    deleteIndex = index;
                }
            }
        }

        public static void DeleteButton (ref int deleteIndex, int index, ref ListElementContext context)
        {
            NewRect (ref context, 80);
            if (context.draw)
            {
                if (GUI.Button (context.rect, "Delete"))
                {
                    if (EditorUtility.DisplayDialog ("Warining", "IsDelete?", "OK", "Cancel"))
                    {
                        deleteIndex = index;
                    }
                }
            }

        }

        public static void EndDelete (int deleteIndex, IList list)
        {
            if (deleteIndex >= 0)
                list.RemoveAt (deleteIndex);
        }

        public static void DrawRect (ref ListElementContext context, float width)
        {
            NewRect (ref context, width);
            if (context.draw)
            {
                GUI.Box (context.rect, "");
            }
        }
        public static void DrawRect (ref ListElementContext context, float width, float height)
        {
            if (context.draw)
            {
                Rect r = context.rect;
                r.width = width;
                r.height = height;
                GUI.Box (r, "");
            }
        }
        public static void DrawRect (Rect r, float width, float height)
        {
            r.width = width;
            // r.width -= width * 2;
            // r.y += height;
            r.height = height;
            GUI.Box (r, "");
        }

        public static void DrawLine (ref ListElementContext context, float width, float xoffset = 0, float yoffset = 0, bool dotline = false)
        {
            if (context.draw)
            {

                Vector2 pos0 = context.rect.position + new Vector2 (xoffset, context.rect.height + yoffset);
                Vector2 pos1 = context.rect.position + new Vector2 (width + xoffset, context.rect.height + yoffset);
                if (dotline)
                {
                    Handles.color = Color.black;
                    Handles.DrawDottedLine (pos0, pos1, 1);
                }
                else
                {
                    Handles.color = Color.gray;
                    Handles.DrawLine (pos0, pos1);
                }

            }
        }

        public static void FolderSelect (ref string path, float width = 300)
        {
            EditorGUILayout.LabelField ("Dir", GUILayout.MaxWidth (40));
            path = EditorGUILayout.TextField ("", path, GUILayout.MaxWidth (width));
            EditorGUI.BeginChangeCheck ();
            DefaultAsset asset = null;
            asset = EditorGUILayout.ObjectField ("", asset, typeof (DefaultAsset), false, GUILayout.MaxWidth (50)) as DefaultAsset;
            if (EditorGUI.EndChangeCheck ())
            {
                path = AssetDatabase.GetAssetPath (asset);
            }
        }

        public static void ObjectField<T> (ref T obj, float width = 300) where T : UnityEngine.Object
        {
            obj = EditorGUILayout.ObjectField (obj, typeof (T), false, GUILayout.MaxWidth (width)) as T;
        }
        public static void ComponentField<T> (ref T obj, float width = 300) where T : UnityEngine.Component
        {
            obj = EditorGUILayout.ObjectField (obj, typeof (T), true, GUILayout.MaxWidth (width)) as T;
        }

        public static bool FolderPorperty (SerializedProperty sp, string name)
        {
            bool folder = sp.boolValue;
            EditorGUI.BeginChangeCheck ();
            folder = EditorGUILayout.Foldout (folder, name);
            if (EditorGUI.EndChangeCheck ())
            {
                sp.boolValue = folder;
            }
            return folder;
        }
        public static bool FolderPorperty(SavedBool sb, string name)
        {
            bool folder = sb.Value;
            EditorGUI.BeginChangeCheck();
            folder = EditorGUILayout.Foldout(folder, name);
            if (EditorGUI.EndChangeCheck())
            {
                sb.Value = folder;
            }
            return folder;
        }

        public static void LodSizeGUI(string name, ref LodSize ls)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(name);
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel++;
            GUILayout.BeginHorizontal();
            ls.size = EditorGUILayout.Slider("Size", ls.size, 1, 50, GUILayout.MaxWidth(500));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            ls.dist = EditorGUILayout.Slider("LodDist", ls.dist, 1, 128, GUILayout.MaxWidth(500));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            ls.fade = EditorGUILayout.Slider("CullDist", ls.fade, 1, 128, GUILayout.MaxWidth(500));
            GUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }

        public struct MouseClickContext
        {
            public Vector2 mouseClickPos;
            public bool processedClick;
            public void Reset ()
            {
                mouseClickPos.x = -1;
                mouseClickPos.y = -1;
                processedClick = true;
            }

            public void BeginProcessEvent ()
            {
                Event evt = Event.current;
                switch (evt.type)
                {
                    case EventType.MouseUp:
                        mouseClickPos = evt.mousePosition;
                        processedClick = false;
                        break;
                }
            }
            public void EndProcessEvent ()
            {
                Event evt = Event.current;
                if (evt.type == EventType.Repaint) { }
            }
            public bool Test (ref Rect rect)
            {
                if (!processedClick && rect.Contains (mouseClickPos))
                {
                    processedClick = true;
                    return true;
                }
                return false;
            }
        }
    }
}