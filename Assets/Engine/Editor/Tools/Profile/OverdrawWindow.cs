using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CFEngine.Editor
{
    public class OverdrawWindow : EditorWindow
    {
        public static OverdrawWindow instance;

        public bool opaqueOverdraw;
        public bool transparentOverdraw;
        private GUIStyle _titleStyle;
        
        public static void ShowWindow()
        {
            if(!instance)instance = GetWindow<OverdrawWindow>("Overdraw模式");
            instance.Focus();
        }
        private void OnEnable()
        {
            _titleStyle = new GUIStyle() {fontSize = 12, normal = new GUIStyleState() {textColor = Color.white}};
            opaqueOverdraw = true;
            transparentOverdraw = true;
        }

        private void OnGUI()
        {
            if (OverdrawState.gameOverdrawViewMode || OverdrawState.sceneOverdrawViewMode)
            {
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.Space(20);
                    opaqueOverdraw = GUILayout.Toggle(opaqueOverdraw, "Opaque Overdraw");
                    OverdrawState.opaqueOverdraw = opaqueOverdraw;
                    GUILayout.FlexibleSpace();
                    transparentOverdraw = GUILayout.Toggle(transparentOverdraw, "Transparent Overdraw");
                    OverdrawState.transparentOverdraw = transparentOverdraw;
                }
            }

           
        }
    }
}