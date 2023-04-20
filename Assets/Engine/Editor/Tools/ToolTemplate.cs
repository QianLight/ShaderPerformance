using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public class ToolTemplate
    {
        public static bool wantsRepaint = false;
        protected CommonToolTemplate currentTool;
        protected int toolIndex = 0;
        protected List<CommonToolTemplate> tools = new List<CommonToolTemplate> ();
        protected GUIContent[] toolIcons = null;
        private bool init = false;
        public ToolTemplate (EditorWindow editorWindow)
        {
            this.editorWindow = editorWindow;
        }
        protected EditorWindow editorWindow;
        public virtual string ToolName
        {
            get
            {
                return "";
            }
        }
        public virtual void OnEnable ()
        {
            editorWindow.wantsMouseMove = true;
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
            init = true;
        }

        public virtual void OnDisable ()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            init = false;
        }

        public virtual void OnDestroy ()
        {
            SetTool (0);
            foreach (CommonToolTemplate t in tools)
                GameObject.DestroyImmediate (t);
        }

        public virtual void OnGUI (Rect rect)
        {
            if (!init)
            {
                OnEnable ();
            }
            Event e = Event.current;

            GUILayout.Space (8);

            EditorGUI.BeginChangeCheck ();
            int toolbarIndex = toolIndex - 1;
            toolbarIndex = GUILayout.Toolbar (toolbarIndex, toolIcons, "button");
            if (EditorGUI.EndChangeCheck ())
            {
                int newIndex = toolbarIndex + 1;
                SetTool (newIndex == toolIndex ? 0 : newIndex);
            }

            // Call current mode GUI
            if (currentTool != null)
            {
                currentTool.DrawGUI (ref rect);
            }
            else
            {
                GUILayout.BeginVertical ();
                GUILayout.FlexibleSpace ();
                GUILayout.BeginHorizontal ();
                GUILayout.FlexibleSpace ();
                GUILayout.Label ("Select an Edit Mode");
                if (GUILayout.Button ("Select", GUILayout.MaxWidth (100)))
                {
                    toolbarIndex = 0;
                    SetTool (toolbarIndex);
                }
                GUILayout.FlexibleSpace ();
                GUILayout.EndHorizontal ();
                GUILayout.FlexibleSpace ();
                GUILayout.EndVertical ();
            }
            if (wantsRepaint)
            {
                wantsRepaint = false;
                editorWindow.Repaint ();
            }
        }
        public virtual void OnSceneGUI (SceneView sceneView)
        {
            if (currentTool != null)
                currentTool.DrawSceneGUI ();
        }
        public virtual void DrawGizmos ()
        {
            if (currentTool != null)
                currentTool.DrawGizmos ();
        }

        public virtual void Update ()
        {
            if (currentTool != null)
                currentTool.Update ();
        }
        protected void SetTool (int index)
        {
            if (index == toolIndex && currentTool != null)
                return;

            if (currentTool != null)
            {
                currentTool.OnUninit ();
            }
            toolIndex = index;
            if (toolIndex >= 0 && (int) toolIndex < tools.Count)
            {
                currentTool = tools[(int) toolIndex];
            }
            else
            {
                currentTool = null;
            }
            if (currentTool != null)
            {
                Tools.current = Tool.None;
                currentTool.OnInit ();
            }
            editorWindow.Repaint ();
        }
        public static void DoRepaint ()
        {
            wantsRepaint = true;
        }
    }
}