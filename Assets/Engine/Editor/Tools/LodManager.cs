#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public class LodManager : EditorWindow
    {
        public List<IEditorComponetInitor<IEditorComponet>> initors = new List<IEditorComponetInitor<IEditorComponet>>();
        private List<string> toolNames = new List<string>();
        private int currentTool = -1;
        
        [MenuItem("Tools/场景/Lod Manager")]
        private static void ShowWindow()
        {
            var window = GetWindow<LodManager>();
            window.titleContent = new GUIContent("Lod Manager");
            window.Show();
        }

        private void Awake()
        {
            initors.Add(TreeLodData.initor);
            initors.Add(HouseLodData.initor);
            foreach (var initor in initors)
            {
                initor.Init();
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
            foreach (var initor in initors)
            {
                initor.GetComponent().Destroy();
            }
            GC.Collect();
        }
    }
}
#endif
