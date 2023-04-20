using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    internal class CustomShaderSelectionDropdown : EditorWindow
    {
        private static class Styles
        {
            public static GUIStyle background = "grey_border";
            public static GUIStyle itemStyle = "DD ItemStyle";
            public static GUIStyle header = "DD HeaderStyle";
            public static GUIStyle checkMark = "DD ItemCheckmark";
            public static GUIStyle lineSeparator = "DefaultLineSeparator";
            public static GUIStyle rightArrow = "ArrowNavigationRight";
            public static GUIStyle leftArrow = "ArrowNavigationLeft";

            public static GUIContent checkMarkContent = new GUIContent ("✔");

            public static readonly GUIContent s_TextImage = new GUIContent ();
        }
        public Material mat;
        public Shader SelectShader;
        private Shader hoverShader;
        public event Action<CustomShaderSelectionDropdown> windowClosed;
        private List<ObjectInfo> shaders = new List<ObjectInfo> ();
        private GUIStyle lineStyle => Styles.itemStyle;
        public void SetShader (Material mat, string[] dir)
        {
            this.mat = mat;
            shaders.Clear ();
            for (int i = 0; i < dir.Length; ++i)
                CommonAssets.GetObjectsInfolder<Shader> (dir[i], shaders, true, "*.shader");
        }
        public void Show (Rect buttonRect, Action<CustomShaderSelectionDropdown> act)
        {
            SelectShader = null;
            hoverShader = null;
            windowClosed = act;
            buttonRect.position = GUIUtility.GUIToScreenPoint (buttonRect.position);
            ShowAsDropDown (buttonRect, new Vector2 (buttonRect.width, shaders.Count * 18));
            Show ();
        }

        private void CloseWindow ()
        {
            if (windowClosed != null)
                windowClosed (this);
            windowClosed = null;
            Close ();
        }

        void OnGUI ()
        {
            GUI.Label (new Rect (0, 0, position.width, position.height), GUIContent.none, Styles.background);
            for (int i = 0; i < shaders.Count; ++i)
            {
                Shader shader = shaders[i].obj as Shader;
                if (shader != null)
                {
                    Styles.s_TextImage.text = shader.name;
                    var rect = GUILayoutUtility.GetRect (Styles.s_TextImage, lineStyle, GUILayout.ExpandWidth (true));

                    if (Event.current.type == EventType.Repaint)
                    {
                        lineStyle.onNormal.textColor = Color.black;
                        lineStyle.onActive.textColor = Color.blue;
                        lineStyle.Draw (rect, Styles.s_TextImage, hoverShader == shader, false, hoverShader == shader, false);
                    }
                    var r = GUILayoutUtility.GetLastRect ();
                    if (Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag)
                    {
                        if (r.Contains (Event.current.mousePosition))
                        {
                            hoverShader = shader;
                            Event.current.Use ();
                        }
                    }
                    if (Event.current.type == EventType.MouseUp && r.Contains (Event.current.mousePosition))
                    {
                        SelectShader = shader;
                        CloseWindow ();
                        GUIUtility.ExitGUI ();

                        Event.current.Use ();
                    }
                }

            }
        }

    }
}