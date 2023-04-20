#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class MergeObject : MonoBehaviour
    {
        public List<MaterialGroup> matGroups = new List<MaterialGroup> ();
        public Vector2 debugScroll = Vector2.zero;
    }

    [CanEditMultipleObjects, CustomEditor (typeof (MergeObject))]
    public class MergeObjectEditor : UnityEngineEditor
    {
        public override void OnInspectorGUI ()
        {
            MergeObject mo = target as MergeObject;
            GUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField (string.Format ("MatCount:{0}", mo.matGroups.Count));
            if(GUILayout.Button("Reset",GUILayout.MaxWidth(60)))
            {
                MaterialGroup.currentMG = null;
                MaterialGroup.blockID = -1;
            }
            GUILayout.EndHorizontal ();
            int count = mo.matGroups.Count > 10 ? 10 : mo.matGroups.Count;
            mo.debugScroll = GUILayout.BeginScrollView (mo.debugScroll, GUILayout.MinHeight (count * 20 + 10));
            for (int i = 0; i < mo.matGroups.Count; ++i)
            {
                GUILayout.BeginHorizontal ();
                var mg = mo.matGroups[i];
                EditorGUILayout.ObjectField (mg, typeof (MaterialGroup), true);
                GUILayout.EndHorizontal ();
            }
            GUILayout.EndScrollView ();
        }
    }
}
#endif