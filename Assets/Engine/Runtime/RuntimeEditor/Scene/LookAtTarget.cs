#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngineEditor = UnityEditor.Editor;
namespace CFEngine
{
    [ExecuteInEditMode]
    public class LookAtTarget : MonoBehaviour
    {

        private void Update()
        {
            SceneMiscModify.lookAtTarget = this.transform;
        }

    }
}

#endif