using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CFEngine
{
    [DisallowMultipleComponent, ExecuteInEditMode]
    public class DynamicSelfShadow : MonoBehaviour
    {
        public DynamicSelfShadowCurve m_curve;
        [HideInInspector]
        public int m_roleIndex;
        public System.Action<int, int> m_selfShadowCb;
#if UNITY_EDITOR
        private ShadowRender shadowRender = new ShadowRender();
        private Transform t;
#endif

        private void LateUpdate()
        {
            if (EngineContext.IsRunning)
            {
                if (m_selfShadowCb != null)
                {
                    m_selfShadowCb.Invoke(m_roleIndex, m_curve.m_slotID);
                }
            }
#if UNITY_EDITOR
            else
            {
                EngineContext context = EngineContext.instance;
                if (context != null)
                {
                    if (t == null)
                    {
                        t = this.transform;
                    }
                    if(m_curve != null)
                    {
                        shadowRender.Update(t, m_curve.m_slotID);
                    }
                }
            }
#endif
        }
    }
}
