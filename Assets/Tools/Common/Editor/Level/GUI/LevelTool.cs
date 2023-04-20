using System;
using System.Collections.Generic;
using CFEngine.Editor;
using UnityEditor;
using UnityEngine;

namespace XEditor.Level
{
    class LevelTool : ToolTemplate
    {
        public LevelGridTool m_LevelGridTool = new LevelGridTool ();

        // private bool bMouseDownEventUsed = false;
        //public static bool wantsRepaint = false;

        public LevelTool (EditorWindow editorWindow) : base (editorWindow)
        {

        }
        public override string ToolName
        {
            get
            {
                return "Level";
            }
        }

        public override void OnEnable ()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
            m_LevelGridTool.OnEnable ();
        }

        public override void OnDisable ()
        {
            m_LevelGridTool.OnDisable ();
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif          
        }

        public override void OnDestroy ()
        {
            m_LevelGridTool.OnDestroy ();
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
        }

        public override void OnSceneGUI (SceneView sceneView)
        {
            Event e = Event.current;
            int controlID = GUIUtility.GetControlID (FocusType.Passive);
            UnityEditor.HandleUtility.AddDefaultControl (controlID);

            switch (e.GetTypeForControl (controlID))
            {
                case EventType.MouseDown:
                    if (Event.current.button == 0)
                    {
                        if (e.clickCount == 1)
                            m_LevelGridTool.OnMouseClick (sceneView);
                        else if (e.clickCount == 2)
                            m_LevelGridTool.OnDoubleClick (sceneView);
                    }

                    break;
                case EventType.ScrollWheel:
                    break;
                case EventType.MouseMove:
                    m_LevelGridTool.OnMouseMove (sceneView);
                    break;
                case EventType.MouseDrag:
                    if (e.button == 0)
                    {
                        if (m_LevelGridTool.OnMouseDrag (sceneView))
                            Event.current.Use ();
                    }
                    break;
                case EventType.KeyDown:
                    {
                        if (m_LevelGridTool.OnKeyDown (sceneView))
                            Event.current.Use ();
                    }
                    break;

            }

            m_LevelGridTool.OnSceneGUI ();
        }

        public override void OnGUI (Rect rect)
        {
            m_LevelGridTool.OnGUI ();
        }

        public override void Update ()
        {
            m_LevelGridTool.OnUpdate ();

            if (wantsRepaint)
            {
                wantsRepaint = false;
                editorWindow.Repaint ();
            }
        }
    }
}