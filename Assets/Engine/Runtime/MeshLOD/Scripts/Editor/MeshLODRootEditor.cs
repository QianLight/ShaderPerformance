#if UNITY_EDITOR
using System;
using CFEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MeshLOD
{
    [CustomEditor(typeof(MeshLODRoot))]
    public class MeshLODRootEditor : UnityEditor.Editor
    {
        private MeshLODRoot _meshLODRoot;
        private void OnEnable()
        {
            _meshLODRoot = target as MeshLODRoot;
        }


        public override void OnInspectorGUI()
        {
            if (_meshLODRoot == null)
            {
                return;
            }

            if (GUILayout.Button("收集"))
            {
                _meshLODRoot.CollectionInfo();

                // Scene curScene = SceneManager.GetActiveScene();
                // EditorSceneManager.MarkSceneDirty(curScene);
                // EditorSceneManager.SaveScene(curScene);
            }
            
            if (GUILayout.Button("Clear"))
            {
                _meshLODRoot.Clear();
            }
            
            base.OnInspectorGUI();
        }
    }
}
#endif