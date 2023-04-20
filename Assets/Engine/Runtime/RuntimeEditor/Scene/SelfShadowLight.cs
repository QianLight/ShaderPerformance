#if UNITY_EDITOR
using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEngineEditor = UnityEditor.Editor;
#endif
namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class SelfShadowLight : MonoBehaviour
    {
        private Transform trans;
        void OnDrawGizmos ()
        {
            if (trans == null)
                trans = transform;
            if (trans != null)
            {
                Color c = Handles.color;
                Handles.color = Color.yellow;
                Quaternion rot = trans.rotation;
                Handles.ArrowHandleCap (100, trans.position, rot, 1, EventType.Repaint);
                Handles.color = c;
            }

        }
    }
}
#endif