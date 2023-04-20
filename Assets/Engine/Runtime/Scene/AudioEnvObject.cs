#if UNITY_EDITOR
using UnityEditor;
using UnityEngineEditor = UnityEditor.Editor;
#endif

using System;
using CFClient;
using FMODUnity;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine
{
   // [RequireComponent(typeof(StudioEventEmitter))]
    [ExecuteInEditMode]
    public class AudioEnvObject : MonoBehaviour
    {
        private StudioEventEmitter see;

        private void Awake()
        {
            see = GetComponent<StudioEventEmitter>();
            ControlByGameCfg();
        }

        private void ControlByGameCfg()
        {
            EngineContext engineContext = EngineContext.instance;
            if (engineContext == null || engineContext.mainRole == null) return;
            see.enabled = XGlobalConfig.singleton.EnableEnvAudio;
        }


        void OnEnable()
        {
            
        }


#if UNITY_EDITOR
        public float range = 1;
        private Transform t;
        void OnDrawGizmos()
        {
            if (t == null)
            {
                t = this.transform;
            }

            if (see == null)
            {
                see = GetComponent<StudioEventEmitter>();
            }
                

            var c = Gizmos.color;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(t.position, range);
            Handles.Label(t.position + Vector3.up * range, see.Event);
            Gizmos.color = c;
        }
#endif
    }

#if UNITY_EDITOR
    [CanEditMultipleObjects, CustomEditor(typeof(AudioEnvObject))]
    public class AudioEnvObjectEditor : UnityEngineEditor
    {

        void OnEnable()
        {
            var ao = target as AudioEnvObject;

            StudioEventEmitter see = ao.GetComponent<StudioEventEmitter>();
            if (see == null)
            {
                see = ao.gameObject.AddComponent<StudioEventEmitter>();
                see.PlayEvent = EmitterGameEvent.ObjectEnable;
                see.StopEvent = EmitterGameEvent.ObjectDisable;
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
           
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            // if (GUILayout.Button("Play", GUILayout.MaxWidth(80)))
            // {
            //     ao.Play();
            // }
            //
            // if (GUILayout.Button("Stop", GUILayout.MaxWidth(80)))
            // {
            //     ao.Stop();
            // }

            EditorGUILayout.EndHorizontal();
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif

}
