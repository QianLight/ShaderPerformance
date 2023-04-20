#if UNITY_EDITOR
using System;
using CFEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Custom.Decal.Editor
{
    [CustomEditor(typeof(DecalRoot))]
    public class DecalRootEditor : UnityEditor.Editor
    {
        private DecalRoot _decalRoot;
        private SerializedProperty decalDiffMaterialListProperty;
        private SerializedProperty IsDrawCubeProperty;
        private SerializedProperty IsDrawSphereProperty;
        private SerializedProperty IsDrawDecalChunkObj;


        private void OnEnable()
        {
            _decalRoot = target as DecalRoot;
            decalDiffMaterialListProperty = serializedObject.FindProperty("DecalDiffMaterialList");
            IsDrawCubeProperty = serializedObject.FindProperty("IsDrawCube");
            IsDrawSphereProperty = serializedObject.FindProperty("IsDrawSphere");
            IsDrawDecalChunkObj = serializedObject.FindProperty("IsDrawDecalChunkObj");
        }

        private void OnDisable()
        {
            decalDiffMaterialListProperty.Dispose();
            IsDrawCubeProperty.Dispose();
            IsDrawSphereProperty.Dispose();
            IsDrawDecalChunkObj.Dispose();
        }

        public override void OnInspectorGUI()
        {
            if (_decalRoot == null)
            {
                return;
            }

            EditorGUILayout.PropertyField(IsDrawCubeProperty);
            // EditorGUILayout.PropertyField(IsDrawSphereProperty);
            // EditorGUILayout.PropertyField(IsDrawDecalChunkObj);
            serializedObject.ApplyModifiedProperties();

            GUI.enabled = false;
            EditorGUILayout.PropertyField(decalDiffMaterialListProperty);
            GUI.enabled = true;

            if (EngineContext.IsRunning)
            {
                return;
            }
            
            if (GUILayout.Button("收集"))
            {
                _decalRoot.CollectDecal();

                Scene curScene = SceneManager.GetActiveScene();
                EditorSceneManager.MarkSceneDirty(curScene);
                EditorSceneManager.SaveScene(curScene);
            }

            if (!DecalManager.Instance.IsInit)
            {
                if (GUILayout.Button("预览"))
                {
                    DecalManager.Instance.Init(Camera.main);
                    DecalManager.Instance.SetVisible(false);
                }
            }
            else
            {
                if (GUILayout.Button("取消预览"))
                {
                    DecalManager.Instance.CancelPreview();
                }
            }
        }
    }
}
#endif