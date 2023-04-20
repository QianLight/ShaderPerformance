using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using CFEngine.WorldStreamer;
using UnityEngine.SceneManagement;
using System.IO;

namespace CFEngine.WorldStreamer.Editor
{

    [InitializeOnLoad]
    public class OpenWorldEditor : UnityEditor.Editor
    {
        static OpenWorldEditor()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }
        

      public static void RefreshAllStreamerAuto()
        {
            if (m_Target == null)
            {
                m_Target = FindObjectOfType<OpenWorld>();
                
                if(m_Target)
                LightmapManager.RefreshAllVolumnsInEditor();
            }

            if (m_Target == null)
            {
                return;
            }
            
            m_Target.InitConfig();
            RefreshAllStreamer();
        }


        static void RefreshAllStreamer()
        {
            m_Target.m_StreamerPrefabs = GameObject.FindObjectsOfType<StreamerPrefab>();
        }

        private static OpenWorld m_Target;


        static Rect windowRect = new Rect(20, 20, 150, 50);
        private static void OnSceneGUI(SceneView sceneView)
        {
            RefreshAllStreamerAuto();

            if (m_Target == null) return;

            int windowID = 22333;
            windowRect = GUILayout.Window(windowID, windowRect, DrawOceamWorldWindow, "开放大世界", GUILayout.Width(100));
        }


        public static void DrawOceamWorldWindow(int windowID)
        {
            // GUILayout.BeginHorizontal();
            // if (GUILayout.Button("刷新所有场景配置"))
            // {
            //     RefreshAllStreamer();
            // }
            //
            // if (GUILayout.Button("Debug:" + m_Target.bDebug))
            // {
            //     m_Target.bDebug = !m_Target.bDebug;
            // }
            //
            // GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            m_Target.m_EnumSteamerLODType = (EnumStreamerLODType)EditorGUILayout.EnumPopup("StreamerLOD", m_Target.m_EnumSteamerLODType);
            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            if (GUILayout.Button("加载所有资源"))
            {
                StreamerLoader.IsEditorMonitor = true;
                RefreshAllStreamer();
                m_Target.SendAllStreamerAction(EnumSteamerActionType.Unloading, EnumStreamerLODType.All);
                m_Target.SendAllStreamerAction(EnumSteamerActionType.Loading, m_Target.m_EnumSteamerLODType);
                
                LightmapManager.RefreshAllVolumnsInEditor();
            }

            if (GUILayout.Button("卸载所有资源"))
            {
                StreamerLoader.IsEditorMonitor = true;
                RefreshAllStreamer();
                m_Target.SendAllStreamerAction(EnumSteamerActionType.Unloading, m_Target.m_EnumSteamerLODType);

                StreamerPrefab[] allDatas = GameObject.FindObjectsOfType<StreamerPrefab>();
                for (int i = 0; i < allDatas.Length; i++)
                {
                    allDatas[i].Show(false);
                }

                LightmapManager.RefreshAllVolumnsInEditor();
            }

            GUILayout.EndHorizontal();


            GUILayout.BeginHorizontal();
            if (GUILayout.Button("运行"))
            {
                EditorApplication.isPlaying = true;
            }

            GUILayout.EndHorizontal();

            GUI.DragWindow();
        }
    }
}
