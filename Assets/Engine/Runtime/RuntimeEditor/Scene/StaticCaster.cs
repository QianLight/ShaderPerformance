#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngineEditor = UnityEditor.Editor;
namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class StaticCaster : MonoBehaviour
    {

        private Transform t;

        private void Update()
        {
            EngineContext context = EngineContext.instance;
            if(context!=null)
            {
                if (t == null)
                {
                    t = this.transform;
                }
                if (ShadowModify.staticShadowList.IndexOf(t) < 0)
                    ShadowModify.staticShadowList.Add(t);
            }
        }

    }
}

#endif