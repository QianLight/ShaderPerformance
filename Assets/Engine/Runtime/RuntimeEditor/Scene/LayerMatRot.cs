#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{

    [DisallowMultipleComponent, ExecuteInEditMode]
    [RequireComponent(typeof(Renderer))]
    public class LayerMatRot : MonoBehaviour
    {
        [Range(0,360)]
        public float rotLayer0;
        [Range(0, 360)]
        public float rotLayer1;
        [Range(0, 360)]
        public float rotLayer2;
        private Material mat;

        public Material GetMat(bool force = false)
        {
            if (mat == null || force)
            {
                if (this.TryGetComponent<Renderer>(out var r))
                {
                    mat = r.sharedMaterial;
                }
            }
            return mat;
        }
    }

    [CustomEditor (typeof (LayerMatRot))]
    public class LayerMatRotEditor : UnityEngineEditor
    {
        SerializedProperty rotLayer0;
        SerializedProperty rotLayer1;
        SerializedProperty rotLayer2;
        private void OnEnable()
        {
            rotLayer0 = serializedObject.FindProperty("rotLayer0");
            rotLayer1 = serializedObject.FindProperty("rotLayer1");
            rotLayer2 = serializedObject.FindProperty("rotLayer2");
        }

        private void LayerRotGUI(SerializedProperty rotLayer,Material mat,int key, bool is01)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(rotLayer);
            if (EditorGUI.EndChangeCheck())
            {
                if(mat.HasProperty(key))
                {
                    Vector4 v = mat.GetVector(key);
                    if(is01)
                    {
                        v.x = Mathf.Cos(Mathf.Deg2Rad * rotLayer.floatValue);
                        v.y = Mathf.Sin(Mathf.Deg2Rad * rotLayer.floatValue);
                    }
                    else
                    {
                        v.z = Mathf.Cos(Mathf.Deg2Rad * rotLayer.floatValue);
                        v.w = Mathf.Sin(Mathf.Deg2Rad * rotLayer.floatValue);
                    }
                    mat.SetVector(key, v);
                }
            }
        }
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            var lmr = target as LayerMatRot;
            var mat = lmr.GetMat();
            if (mat != null)
            {
                LayerRotGUI(rotLayer0, mat, ShaderManager._Rot01, true);
                LayerRotGUI(rotLayer1, mat, ShaderManager._Rot01, false);
                LayerRotGUI(rotLayer2, mat, ShaderManager._Rot23, true);
            }

            serializedObject.ApplyModifiedProperties ();
        }
    }
}
#endif