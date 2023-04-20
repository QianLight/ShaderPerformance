using System;
using UnityEditor;
using UnityEngine;
#if UNITY_EDITOR
using UnityEngineEditor = UnityEditor.Editor;
#endif
namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class SFXPrefab : MonoBehaviour
    {
        public string animPath;
        public bool startHide;
        public string sfxName;

        private Vector3 pos;

        private Quaternion ro;

        private SFX targetFx;
        public void Start()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
            {
#endif
                pos = transform.localPosition;
                ro = transform.localRotation;
                targetFx = SFXMgr.singleton.Create(sfxName);
                targetFx.flag.SetFlag(SFX.Flag_Follow, true);
                targetFx.SetParent(transform);
                var scale = transform.localScale;
                targetFx.SetScale(ref scale);
                targetFx.Play();

#if UNITY_EDITOR
            }
        }
        public void Save()
        {
            // gameObject.AddComponent<ManualSFXCreate>();
            sfxName = gameObject.name;
            DestroyImmediate(transform.GetChild(0).gameObject);
#endif
        }
    }
#if UNITY_EDITOR
    [CustomEditor (typeof (SFXPrefab))]
    public class SFXPrefabEditor : UnityEngineEditor
    {
        SerializedProperty animPath;
        SerializedProperty startHide;
        void OnEnable ()
        {
            animPath = serializedObject.FindProperty ("animPath");
            startHide = serializedObject.FindProperty ("startHide");
        }
        public override void OnInspectorGUI ()
        {
            var sp = target as SFXPrefab;
            if (EditorCommon.AssetSelect (ref sp.animPath, 500))
            {
                animPath.stringValue = sp.animPath;
            }
            EditorGUILayout.PropertyField(startHide);
            // if (GUILayout.Button("Save"))
            // {
            //     sp.Save();
            // }
            sp.sfxName = EditorGUILayout.TextField("SFXPrefab Name", sp.sfxName);
            serializedObject.ApplyModifiedProperties ();
        }
    }
#endif
}
