#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{

    [DisallowMultipleComponent, ExecuteInEditMode]
    public class SFXManager : MonoBehaviour
    {
        [NonSerialized]
        public GameObject target;
        [NonSerialized]
        public SFX sfx;
        private void OnDrawGizmos ()
        {
            SFX.OnDrawGizmo ();
        }
    }

    [CustomEditor (typeof (SFXManager))]
    public class SFXManagerEditor : UnityEngineEditor
    {
        private void OnEnable () { }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update ();
            var sfxManager = SFXMgr.singleton;
            var sm = target as SFXManager;
            sm.target = EditorGUILayout.ObjectField ("Debug", sm.target, typeof (GameObject), true) as GameObject;
            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Play", GUILayout.MaxWidth (80)))
            {
                if (sm.target != null)
                {
                    if (sm.sfx != null)
                    {
                        sfxManager.Destroy (ref sm.sfx, true);
                    }
                    sm.sfx = sfxManager.Create (sm.target.name, 0);
                    var t = sm.transform;
                    var pos = t.position;
                    sm.sfx.SetGlobalPos(ref pos);
                    var rot = t.rotation;
                    sm.sfx.SetGlobalRot(ref rot);
                    sm.sfx.Play ();

                }
            }
            if (GUILayout.Button ("Stop", GUILayout.MaxWidth (80)))
            {
                if (sm.sfx != null)
                {
                    sfxManager.Destroy (ref sm.sfx, true);
                }
            }
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.Separator ();
            EditorGUILayout.LabelField (string.Format ("SFX Count:{0}", sfxManager.sfxPool.Count));
            var it = sfxManager.sfxPool.GetEnumerator ();
            while (it.MoveNext ())
            {
                var sfx = it.Current.Value;

                if (sfx.config != null)
                {
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.LabelField (string.Format ("SFX {0}", sfx.config.name));
                    EditorGUILayout.EndHorizontal ();
                    EditorGUI.indentLevel++;
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.ObjectField ("", sfx.GetTrans (), typeof (Transform), false);
                    EditorGUILayout.EndHorizontal ();
                    EditorGUI.indentLevel--;
                }
                
            }
            if (GUILayout.Button ("Clear", GUILayout.MaxWidth (80)))
            {
                sfxManager.OnLeaveScene (EngineContext.instance);
            }
            SFX.debugName = EditorGUILayout.TextField ("sfx debug", SFX.debugName);
            if (GUILayout.Button ("ClearDebugPos"))
            {
                SFX.debugPos.Clear ();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif