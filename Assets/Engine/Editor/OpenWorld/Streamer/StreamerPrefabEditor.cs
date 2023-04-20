using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine.WorldStreamer.Editor
{

    [CustomEditor(typeof(StreamerPrefab))]

    public class StreamerPrefabEditor : UnityEditor.Editor
    {
        private StreamerPrefab m_TargetData;

        private SerializedProperty m_StreamerManager;
        private SerializedProperty m_AssetObj;
        void OnEnable()
        {
            m_TargetData = target as StreamerPrefab;
            m_StreamerManager = serializedObject.FindProperty("m_StreamerManager");
            
        }

        public static string Hierachy = "EditorScene/Prefabs/MainScene/";

        [MenuItem("GameObject/Streamer/SteamerPrefab", false, 0)]
        public static void Create()
        {
            Transform root = GameObject.Find(Hierachy).transform;

            GameObject newObject = new GameObject("World_" + root.childCount);
            newObject.transform.SetParent(root);
            newObject.AddComponent<StreamerPrefab>();
            
            Selection.activeObject = newObject;
        }
        
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            bool hasShow = m_TargetData.transform.childCount == 0;

            if (GUILayout.Button(hasShow ? "显示" : "隐藏", GUILayout.MaxWidth(100)))
            {
                m_TargetData.Show(hasShow);


            }

            EditorGUILayout.EndHorizontal();
            
            
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(m_StreamerManager, new GUIContent("场景数据", "真正用来显示的数据!"), true);
            if (EditorGUI.EndChangeCheck())
            {
                RefreshData();
            }

            serializedObject.ApplyModifiedProperties();
        }
        
        
        public void RefreshData()
        {


        }

    }
}
