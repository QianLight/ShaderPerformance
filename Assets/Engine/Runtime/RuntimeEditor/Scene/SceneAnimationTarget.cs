#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngineEditor = UnityEditor.Editor;

namespace CFEngine
{

    [DisallowMultipleComponent, ExecuteInEditMode]
    public class SceneAnimationTarget : MonoBehaviour
    {
        private Transform t;
        void OnDrawGizmos ()
        {
            if (t == null)
            {
                t = this.transform;
            }
            Handles.PositionHandle (t.position, t.rotation);
        }
    }
}

#endif