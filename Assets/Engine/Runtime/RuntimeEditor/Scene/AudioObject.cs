#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{
    public delegate EngineAudio AddAudioComponent (GameObject go);
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class AudioObject : MonoBehaviour
    {
        public string eventName = "";
        public float range = 1;

        
        public bool autoPlay = false;

        private Transform t;
        private EngineAudio engineAudio;
        [System.NonSerialized]
        public bool dirty = true;
        public static AddAudioComponent add;
        public void Init ()
        {
            if (add != null)
            {
                engineAudio = add (this.gameObject);
            }
        }
        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        void OnEnable ()
        {
            dirty = true;
        }
        void OnDisable() 
        {
            Stop();
        }
        public void Stop ()
        {
            if (engineAudio != null)
            {
                engineAudio.StopAudio ();
            }
        }
        void Update ()
        {
            if (t == null)
            {
                t = this.transform;
            }
            if (engineAudio != null&&autoPlay)
            {
                if (!string.IsNullOrEmpty (eventName))
                {
                    if (dirty)
                    {
                        engineAudio.StartAudio (eventName);
                        dirty = false;
                    }
                }
            }
            else
            {
                Init ();
            }
        }

        void OnDrawGizmos ()
        {
            if (t == null)
            {
                t = this.transform;
            }
            var c = Gizmos.color;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere (t.position, range);
            Handles.Label (t.position + Vector3.up * range, eventName);
            Gizmos.color = c;
        }
    }

    [CanEditMultipleObjects, CustomEditor (typeof (AudioObject))]
    public class AudioObjectEditor : UnityEngineEditor
    {
        SerializedProperty eventName;
        SerializedProperty range;
        SerializedProperty autoPlay;
        void OnEnable ()
        {
            eventName = serializedObject.FindProperty ("eventName");
            range = serializedObject.FindProperty ("range");
            autoPlay = serializedObject.FindProperty ("autoPlay");
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update ();
            var ao = target as AudioObject;
            EditorGUILayout.PropertyField (autoPlay);
            EditorGUI.BeginChangeCheck ();
            EditorGUILayout.PropertyField (eventName);
            if (EditorGUI.EndChangeCheck ())
            {
                ao.dirty = true;
            }
            EditorGUILayout.PropertyField (range);
            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Play", GUILayout.MaxWidth (80)))
            {
                ao.dirty = true;
            }
            if (GUILayout.Button ("Stop", GUILayout.MaxWidth (80)))
            {
                ao.Stop ();
            }
            EditorGUILayout.EndHorizontal ();
            serializedObject.ApplyModifiedProperties ();
        }
    }
}
#endif