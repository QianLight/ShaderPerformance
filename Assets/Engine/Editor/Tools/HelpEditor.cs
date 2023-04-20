using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    internal class HelpDropdown : EditorWindow
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
        private GUIStyle lineStyle => Styles.itemStyle;

        public void Show (Vector2 pos,Rect buttonRect)
        {
            Vector2 screenPos = GUIUtility.GUIToScreenPoint (pos);
            buttonRect.position = screenPos;
            buttonRect.yMax = screenPos.y;
            ShowAsDropDown (buttonRect, new Vector2 (buttonRect.width, 100));
            Show ();
        }

        private void CloseWindow ()
        {
            Close ();
        }

        void OnGUI ()
        {
            GUI.Label (new Rect (0, 0, position.width, position.height), GUIContent.none, Styles.background);            
        }

    }
}