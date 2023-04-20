using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using CFEngine.WorldStreamer;
using UnityEngine.SceneManagement;


namespace CFEngine.WorldStreamer.Editor
{

    [CustomEditor(typeof(StreamerManager))]

    public class StreamerManagerEditor : UnityEditor.Editor
    {

        private StreamerManager m_SteamerManager;
        private SerializedProperty m_bIsDebug;
        
        void OnEnable()
        {
            m_SteamerManager = target as StreamerManager;
            m_bIsDebug = serializedObject.FindProperty("bIsDebug");
        }

        public static string StreamerPrefabFolder = "Assets/BundleRes/Prefabs/StreamerWorld/";
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("刷新Bounds", GUILayout.MaxWidth(100)))
            {
                m_SteamerManager.RefreshData();
                PrefabUtility.ApplyPrefabInstance(m_SteamerManager.gameObject, InteractionMode.AutomatedAction);
            }

            EditorGUILayout.PropertyField(m_bIsDebug, new GUIContent("调试", "调试显示lod框框!"), true);
            
            EditorGUILayout.EndHorizontal();


            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_StreamerData"), new GUIContent("场景数据", "高中低显示数据 场景 prefab都可以!"), true);

            if (EditorGUI.EndChangeCheck())
            {
                m_SteamerManager.RefreshData();
                PrefabUtility.ApplyPrefabInstance(m_SteamerManager.gameObject, InteractionMode.AutomatedAction);
            }

            serializedObject.ApplyModifiedProperties();
        }


        public static bool RefreshStreamer()
        {
            StreamerManager sm = FindObjectOfType<StreamerManager>();
            if (sm == null) return false;
            sm.RefreshData();

#if UNITY_EDITOR
            PrefabUtility.ApplyPrefabInstance(sm.gameObject, InteractionMode.AutomatedAction);
#endif

            return true;
        }
        
        
        public static string WorldStreamerFolder = "Assets/BundleRes/WorldStreamer/";

        [MenuItem("GameObject/Streamer/SteamerManager", false, 0)]
        public static void Create()
        {
            Scene activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

            StreamerManager sm = FindObjectOfType<StreamerManager>();
            if (sm != null) return;

            Create(WorldStreamerFolder, "Streamer_" + activeScene.name + ".Prefab");
        }

        public static void Create(string path, string assetName)
        {
            if (string.IsNullOrEmpty(path))
                return;

            string assetPathAndName =(path + assetName);
            
            if (File.Exists(assetPathAndName))
            {

                GameObject obj =
                    PrefabUtility.InstantiatePrefab(AssetDatabase.LoadMainAssetAtPath(assetPathAndName)) as GameObject;
                
                obj.name = "StreamerManager";
                
                return;
            }

            GameObject newObj = new GameObject("StreamerManager");

            StreamerManager sm = newObj.AddComponent<StreamerManager>();
            
            // Scene activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            // UnityEngine.Object sceneObj= AssetDatabase.LoadMainAssetAtPath(activeScene.path);
            // StreamerData newStreamerData = new StreamerData(sceneObj,true);
            // newStreamerData.m_EnumSteamerLODType = EnumSteamerLODType.LOD0;
            //
            // sm.AddSceneStream(newStreamerData);
            
            PrefabUtility.SaveAsPrefabAssetAndConnect(newObj,assetPathAndName, InteractionMode.AutomatedAction);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            //EditorUtility.FocusProjectWindow();
        }


        public static void ReplaceOrAddScenePrefab(GameObject obj, string sceneName)
        {
            string path = StreamerPrefabFolder + sceneName + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(obj, path);
            AssetDatabase.Refresh();

            string assetPathAndName = WorldStreamerFolder + "Streamer_" + sceneName + ".Prefab";

            GameObject SceneObjConfig =
                PrefabUtility.InstantiatePrefab(AssetDatabase.LoadMainAssetAtPath(assetPathAndName)) as GameObject;
            StreamerManager sm = SceneObjConfig.GetComponent<StreamerManager>();

            List<StreamerData> allTmpList = new List<StreamerData>();

            if (sm.m_StreamerData != null)
                allTmpList.AddRange(sm.m_StreamerData);


            StreamerData lastLOD0 = null;
            
            for (int i = allTmpList.Count - 1; i >= 0; i--)
            {
                StreamerData tmp = allTmpList[i];
                if (tmp.m_EnumSteamerLODType == EnumStreamerLODType.LOD0)
                {
                    lastLOD0 = tmp;
                    break;
                }
            }

            UnityEngine.Object sceneObj= AssetDatabase.LoadMainAssetAtPath(path);

            if (lastLOD0 == null)
            {
                lastLOD0 = new StreamerData(sceneObj);
                lastLOD0.m_EnumSteamerLODType = EnumStreamerLODType.LOD0;
                allTmpList.Add(lastLOD0);
            }
            
            lastLOD0.m_AssetObj = sceneObj;
            sm.m_StreamerData = allTmpList.ToArray();
            
            sm.RefreshData();
            PrefabUtility.SaveAsPrefabAssetAndConnect(SceneObjConfig,assetPathAndName, InteractionMode.AutomatedAction);
        }
    }
}

