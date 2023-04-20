#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Custom.Decal.Editor
{
    public class DecalCreateEditor : UnityEditor.Editor
    {
        private static Shader decalShader;

        private void OnEnable()
        {
            decalShader = Shader.Find("URP/Scene/Decal");
        }

        private void OnDestroy()
        {
            decalShader = null;
        }

        [MenuItem("GameObject/CreateDecal/DecalRoot", false, 0)]
        public static void CreateDecal()
        {
            GameObject editorSceneObj = GameObject.Find("EditorScene");
            if (editorSceneObj == null)
            {
                Debug.Log("未找的 EditorScene 节点");
                return;
            }

            if (editorSceneObj.transform.Find("Decal") != null)
            {
                return;
            }

            GameObject decalRootObj = new GameObject("Decal");
            decalRootObj.AddComponent<DecalRoot>();
            SetParent(decalRootObj.transform, editorSceneObj.transform);

            GameObject mainSceneObj = new GameObject("MainScene");
            SetParent(mainSceneObj.transform, decalRootObj.transform);
            Selection.activeGameObject = decalRootObj;
        }

        [MenuItem("GameObject/CreateDecal/DecalCube", false, 0)]
        public static void CreateDecalCube()
        {
            Transform decalCubeParent = FindDecalCubeParent();
            GameObject decalObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            decalObj.name = "DecalCube";
            if (decalCubeParent != null)
            {
                SetParent(decalObj.transform, decalCubeParent);
            }

            Collider tempCollider = decalObj.GetComponent<Collider>();
            if (tempCollider != null)
            {
                DestroyImmediate(tempCollider);
            }

            Renderer renderer = decalObj.GetComponent<Renderer>();
            renderer ??= decalObj.AddComponent<MeshRenderer>();
            if (decalShader == null)
            {
                decalShader = Shader.Find("URP/Scene/Decal");
                renderer.sharedMaterial = new Material(decalShader);
                renderer.sharedMaterial.enableInstancing = true;
            }

            Selection.activeGameObject = decalObj;
        }

        private static Transform FindDecalCubeParent()
        {
            GameObject editorSceneObj = GameObject.Find("EditorScene");
            if (editorSceneObj == null)
            {
                return null;
            }

            return editorSceneObj.transform.Find("Decal/MainScene");
        }

        private static void SetParent(Transform targetTrans, Transform parent)
        {
            if (parent != null)
            {
                targetTrans.parent = parent;
            }

            targetTrans.position = Vector3.zero;
            targetTrans.rotation = Quaternion.Euler(Vector3.zero);
            targetTrans.localScale = Vector3.one;
        }
    }
}

#endif