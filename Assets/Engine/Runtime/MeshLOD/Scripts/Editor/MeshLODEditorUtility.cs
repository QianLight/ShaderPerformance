#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace MeshLOD
{
    public class MeshLODCreateEditor : Editor
    {
        [MenuItem("GameObject/MeshLOD/CreateMeshLODConfig", false, 0)]
        public static void CreateDecal()
        {
            GameObject editorSceneObj = GameObject.Find("EditorScene");
            if (editorSceneObj == null)
            {
                Debug.Log("未找的 EditorScene 节点");
                return;
            }

            Transform meshLODRootTransform = editorSceneObj.transform.Find("MeshLODRoot");
            if (meshLODRootTransform == null)
            {
                meshLODRootTransform  = new GameObject("MeshLODRoot").transform;
                meshLODRootTransform.SetParent(editorSceneObj.transform);
                meshLODRootTransform.localPosition = Vector3.zero;
                meshLODRootTransform.localRotation = Quaternion.identity;
                meshLODRootTransform.localScale = Vector3.one;
            }

            MeshLODRoot meshLODRoot = meshLODRootTransform.GetComponent<MeshLODRoot>();
            if (meshLODRoot == null)
            {
                meshLODRoot = meshLODRootTransform.gameObject.AddComponent<MeshLODRoot>();
                
                Transform staticObjTransform = editorSceneObj.transform.Find("StaticPrefabs");
            }
            
            MeshLODEntrancePoint meshLODEntrancePoint = meshLODRootTransform.GetComponent<MeshLODEntrancePoint>();
            if (meshLODEntrancePoint == null)
            {
                meshLODEntrancePoint = meshLODRootTransform.gameObject.AddComponent<MeshLODEntrancePoint>();
                meshLODEntrancePoint.referenceCamera = Camera.main;
            }

            Selection.activeObject = meshLODRootTransform.gameObject;
        }
    }
    
    
}
#endif