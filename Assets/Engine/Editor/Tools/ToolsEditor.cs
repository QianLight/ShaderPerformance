using System;
using System.Collections.Generic;
using CFEngine;
using UnityEditor;
using UnityEngine;
namespace CFEngine.Editor
{

    public partial class ToolsEditor : EditorWindow
    {
        // public enum EToolsType
        // {
        //     Common,
        //     Scene,
        //     Character,            
        //     Prefab,
        //     Fx,            
        // }

        private GUIContent[] toolnames = null;
        private int toolType = 0;
        private ToolTemplate tool = null;
        private List<ToolTemplate> tools = new List<ToolTemplate> ();
        private OnDrawGizmoCb drawGizmoCb;

        public static List<ToolTemplate> externalTools = new List<ToolTemplate> ();

        [MenuItem ("Tools/Tools Window %T", false, 20)]
        public static void MenuInitEditorWindow ()
        {
            ToolsEditor window = EditorWindow.GetWindow<ToolsEditor> ("Tools", true);
            window.name = "ToolsEditor";
            window.Show ();
        }
        private void GetExternalTool ()
        {
            EditorCommon.CallInternalFunction (typeof (ToolsEditorExternal), "Init", true, false, false, null, new object[] { this });
        }

        void OnEnable ()
        {
            // this.titleContent = new GUIContent("ToolsEditor", z_IconUtility.GetIcon("Icon/window_icon"));
            GetExternalTool ();
            toolnames = new GUIContent[5 + externalTools.Count];
            toolnames[0] = new GUIContent ("Common");
            toolnames[1] = new GUIContent ("Scene");
            toolnames[2] = new GUIContent ("Character");
            toolnames[3] = new GUIContent ("Prefab");
            toolnames[4] = new GUIContent ("Fx");

            tools.Add (new CommonTool (this));
            tools.Add (new SceneTool (this));
            tools.Add (new CharacterTool (this));
            tools.Add (new PrefabTool (this));
            tools.Add (new SFXTool (this));
            for (int i = 0; i < externalTools.Count; ++i)
            {
                var et = externalTools[i];
                toolnames[5 + i] = new GUIContent (et.ToolName);
                tools.Add (et);
            }

            SetTool (0);
            drawGizmoCb = DrawGizmos;
            EnvironmentExtra.RegisterDrawGizmo (drawGizmoCb);
        }

        void OnDisable ()
        {
            EnvironmentExtra.UnRegisterDrawGizmo (drawGizmoCb);
        }

        void OnDestroy ()
        {
            for (int i = 0; i < tools.Count; ++i)
            {
                if (tools[i] != null)
                {
                    tools[i].OnDestroy ();
                }
            }
        }

        void OnGUI ()
        {
            Event e = Event.current;

            EditorGUI.BeginChangeCheck ();
            int toolbarIndex = toolType;
            toolbarIndex = GUILayout.Toolbar (toolbarIndex, toolnames, "button");
            if (EditorGUI.EndChangeCheck ())
            {
                SetTool (toolbarIndex);
            }
            if (tool != null)
            {
                tool.OnGUI (this.position);
            }
        }

        void OnSceneGUI (SceneView sceneView)
        {
            if (tool != null)
            {
                tool.OnSceneGUI (sceneView);
            }
        }

        void Update ()
        {
            if (tool != null)
            {
                tool.Update ();
            }
        }
        public void DrawGizmos ()
        {
            if (tool != null)
            {
                tool.DrawGizmos ();
            }
        }

        void SetTool (int toolType)
        {
            if (this.toolType == toolType && tool != null)
                return;

            this.toolType = toolType;
            if (tool != null)
            {
                tool.OnDisable ();
            }
            tool = tools[(int) toolType];
            if (tool != null)
                tool.OnEnable ();

            Repaint ();
        }
    }
}