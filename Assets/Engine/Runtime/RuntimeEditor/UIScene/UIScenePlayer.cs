#if UNITY_EDITOR
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{


    [DisallowMultipleComponent, ExecuteInEditMode]
    public class UIScenePlayer : MonoBehaviour
    {
        public Vector3 pos;
        public Vector3 rot;
        public Vector3 scale = Vector3.one;

        public void Update ()
        {
            var t = this.transform;
            if (t.childCount > 0)
            {
                var child = t.GetChild(0);
                var p = child.localPosition;
                if (!EngineUtility.CompareVector(ref p, ref pos))
                {
                    pos = p;
                }
                var r = child.localEulerAngles;
                if (!EngineUtility.CompareVector(ref r, ref rot))
                {
                    rot = r;
                }
                var s = child.localScale;
                if (!EngineUtility.CompareVector(ref s, ref scale))
                {
                    scale = s;
                }
            }
        }
    }

    [CustomEditor(typeof(UIScenePlayer))]
    public class UIScenePlayerEditor : UnityEngineEditor
    {
        private SerializedProperty pos;
        private SerializedProperty rot;
        private SerializedProperty scale;
        public void OnEnable()
        {
            pos = serializedObject.FindProperty("pos");
            rot = serializedObject.FindProperty("rot");
            scale = serializedObject.FindProperty("scale");
        }

        public override void OnInspectorGUI()
        {
            UIScenePlayer player = target as UIScenePlayer;
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(pos);
            EditorGUILayout.PropertyField(rot);
            EditorGUILayout.PropertyField(scale);
            if (EditorGUI.EndChangeCheck())
            {
                var t = player.transform;
                if(t.childCount>0)
                {
                    var child = t.GetChild(0);
                    child.localPosition = pos.vector3Value;
                    child.localRotation = Quaternion.Euler(rot.vector3Value);
                    child.localScale = scale.vector3Value;
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif