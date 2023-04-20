using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine.Editor
{

    [CustomEditor (typeof (EnvProfile))]
    sealed class EnvProfileEditor : UnityEngineEditor
    {
        EffectListEditor m_EffectList;

        void OnEnable ()
        {
            m_EffectList = new EffectListEditor (this);
            m_EffectList.Init (target as EnvProfile, serializedObject);
        }

        void OnDisable ()
        {
            if (m_EffectList != null)
                m_EffectList.Clear ();
        }

        public override void OnInspectorGUI ()
        {
            serializedObject.Update ();
            m_EffectList.OnGUI ();
            serializedObject.ApplyModifiedProperties ();
        }
    }
}