#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public class RolesManager : EditorWindow
    {
        public List<IEditorComponetInitor<IEditorComponet>> initors = new List<IEditorComponetInitor<IEditorComponet>>();
        private List<string> toolNames = new List<string>();
        private int currentTool = -1;
        
        [MenuItem("Tools/角色/Roles Manager #&m")]
        private static void ShowWindow()
        {
            var window = GetWindow<RolesManager>();
            window.titleContent = new GUIContent("Roles Manager");
            window.Show();
        }

        private void Awake()
        {
            initors.Add(BandposeStatistics.initor);
            initors.Add(RoleStatistics.initor);
            initors.Add(RoleInSceneStatistics.initor);
            initors.Add(RoleMatStatistics.initor);
            initors.Add(RoleAssetsDisposition.initor);
            foreach (var initor in initors)
            {
                initor.Init();
                initor.GetComponent().GetEditorWindow(this);
                toolNames.Add(initor.GetComponent().Name());
            }
        }

        private void OnGUI()
        {
            GUILayout.BeginVertical();
            DrawToolBar();
            GUILayout.EndVertical();
            GUILayout.Space(5);
            
            GUILayout.BeginVertical();
            DrawSingleTool();
            GUILayout.EndVertical();
        }

        void DrawToolBar()
        {
            currentTool = GUILayout.Toolbar(currentTool,toolNames.ToArray());
        }

        void DrawSingleTool()
        {
            for (int i = 0; i < initors.Count; i++)
            {
                if (currentTool == i)
                {
                    initors[i].GetComponent().DrawGUI();
                }
            }
        }

        private void OnDestroy()
        {
            RoleEditorComponetContext.Clear();
            foreach (var initor in initors)
            {
                initor.GetComponent().Destroy();
            }
            GC.Collect();
        }
    }
}
#endif
