#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{

    [DisallowMultipleComponent, ExecuteInEditMode]
    public class ObjectRender : MonoBehaviour
    {
        public LightRender light0 = null;
        public LightRender light1 = null;
        public bool manualSet = false;
    }

    [CustomEditor (typeof (ObjectRender))]
    public class ObjectRenderEditor : UnityEngineEditor
    {
        // public override void OnInspectorGUI ()
        // {
        //     serializedObject.Update ();
        //     // LightRender lr = target as LightRender;
        //     // lr.rangeBias = EditorGUILayout.Slider ("RangeBias", lr.rangeBias, 0.1f, 10);
        //     // lr.priority = EditorGUILayout.IntField ("Priority", lr.priority);
        //     // serializedObject.ApplyModifiedProperties ();
        // }
    }
}
#endif