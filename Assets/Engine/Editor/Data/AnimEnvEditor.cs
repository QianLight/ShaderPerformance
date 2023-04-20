#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Experimental.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine.Editor
{
    [CanEditMultipleObjects, CustomEditor (typeof (AnimEnv))]
    public sealed class AnimEnvEditor : UnityEngineEditor
    {
        private void OnEnable ()
        {
            var ae = target as AnimEnv;
            if (ae.animEnvProfile != null)
            {
                ae.animEnvProfile.Init (ae.transform);
            }
        }

        public override void OnInspectorGUI ()
        {
            var ae = target as AnimEnv;
            var animEnvProfile = ae.animEnvProfile;
            animEnvProfile.OnInspectorGUI();
            serializedObject.ApplyModifiedProperties ();
        }
        private void OnSceneGUI ()
        {

        }
    }
}
#endif