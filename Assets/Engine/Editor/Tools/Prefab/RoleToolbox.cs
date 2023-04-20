#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using A;

namespace CFEngine.Editor
{
    public class RoleToolbox : EditorWindow
    {
        public List<IEditorComponetInitorSelectObject<IEditorComponet>> initors = new List<IEditorComponetInitorSelectObject<IEditorComponet>>();
        private List<IEditorComponet> _components = new List<IEditorComponet>();
        private Vector2 _scrollPosition;

        [MenuItem("Tools/角色/BandposeEditor #&b")]
        public static void ShowWindow()
        {
            var window = GetWindow<RoleToolbox>();
            window.titleContent = new GUIContent("RoleToolbox");
            window.maxSize = new Vector2(450,1200);
            window.minSize = new Vector2(450,650);
            window.Show();
        }

        private void Awake()
        {
            Init();
        }
        void Init()
        {
            GameObject ob;
            if (Selection.gameObjects.Length > 0)
            {
                ob = Selection.gameObjects[0];
                
                initors.Add(BandposeEditorCatcher.initor);
                initors.Add(BoundsRuler.initor);
                initors.Add(RolePhotographer.initor);
                
                foreach (var componetInitor in initors)
                {
                    IEditorComponet component = componetInitor.Init(ob);
                    _components.Add(component);
                }
            }
        }
        private void OnDisable()
        {
            Clear();
        }

        void Clear()
        {
            foreach (var component in _components)
            {
                component.Destroy();
            }
            _components.Clear();
            initors.Clear();
        }
        void Refresh()
        {
            Clear();
            Init();
        }
        private void OnGUI()
        {
            if (Selection.gameObjects.Length != 1)
            {
                EditorGUILayout.HelpBox("请选择单个物体", MessageType.Warning);
            }
            else
            {
                _scrollPosition = GUILayout.BeginScrollView(_scrollPosition,GUILayout.Width(0),GUILayout.Height(0));
                foreach (var component in _components)
                {
                    GUILayout.Space(10);
                    GUILayout.Button(component.Name(),"dockareaStandalone");
                    GUILayout.Space(5);
                    component.DrawGUI();
                }
                GUILayout.EndScrollView();
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh",GUILayout.Width(100)))
            {
                Refresh();
            }
        }
    }
}
#endif
