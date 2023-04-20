using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GUIFrameWork
{
    public class GUIEx
    {
        public static bool AssetCheckToggle(Rect position, bool value)
        {
            Texture2D texture = value ? GUIExAssets.GetAssetCheckToggleOpen() : GUIExAssets.GetAssetCheckToggleClose();

            GUI.Label(position, texture);

            var e = Event.current;
            if(e.type == EventType.MouseUp)
            {
                if(position.Contains(e.mousePosition))
                {
                    e.Use();
                    return !value;
                }
            }
            return value;
        }

        public static string AssetCheckEditorAndDragPathTextField(Rect position, string value)
        {
            string result = GUI.TextField(position, value);
            var e = Event.current;
            if (position.Contains(e.mousePosition))
            {
                if (e.type == EventType.DragUpdated)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Generic;
                }
                else if (e.type == EventType.DragExited)
                {
                    if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                    {
                        result = GetDirectoryName(DragAndDrop.paths[0]);
                    }
                }
            }            
            return result;
        }

        static string GetDirectoryName(string fileOrDir)
        {
            string filePath = string.Empty;
            if (Directory.Exists(fileOrDir))
            {
                filePath = fileOrDir;
            }
            else if (File.Exists(fileOrDir))
            {
                filePath = Path.GetDirectoryName(fileOrDir);
            }
            return "Assets" + Path.GetFullPath(filePath).Substring(Path.GetFullPath(Application.dataPath).Length).Replace("\\", "/");
        }

        public static bool AssetCheckBeginFoldoutHeaderGroup(Rect position, bool value, string strvalue, GUIStyle style = null)
        {
            GUIContent icon;
            if(value)
            {
                icon = EditorGUIUtility.IconContent("IN foldout act on");
            }
            else
            {
                icon = EditorGUIUtility.IconContent("IN foldout act");
            }
            Rect[] rects = position.SplitTwoHorizontal(position.height);
            GUI.Label(rects[0].GetCenterRect(icon.image.width, icon.image.height), icon);
            if(style == null)
            {
                if (GUI.Button(rects[1], strvalue))
                {
                    return !value;
                }
            }
            else
            {
                if (GUI.Button(rects[1], strvalue, style))
                {
                    return !value;
                }
            }
            return value;
        }

        public static bool Popup(Rect position, ref int selectedIndex, string[] displayedOptions)
        {
            int lastSelect = selectedIndex;
            selectedIndex = EditorGUI.Popup(position, lastSelect, displayedOptions);
            return lastSelect != selectedIndex;
        }
    }
}

