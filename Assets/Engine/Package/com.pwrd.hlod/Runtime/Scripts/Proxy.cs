using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if _HLOD_USE_ATHENA_PVS_
using PVS;
#endif


namespace com.pwrd.hlod
{
    public class Proxy : MonoBehaviour
    {
        public List<Proxy> children = new List<Proxy>();
#if _HLOD_USE_ATHENA_PVS_
        public VisibilityItemComponent itemCompoenent;
#endif
        public Renderer proxyRenderer;
        public Manifest[] mainfests;

        public Bounds bounds;
        public float sqrVisibleDistance;
        
        //强制显隐
        public bool enableForceVisible = false;
        public bool forceVisible = true;
        
        private bool m_curproxyVisible;
        private bool m_lastproxyVisible;

        private void OnEnable()
        {
            ProxyManager.Instance.AddProxy(this);
        }

        private void OnDisable()
        {
            ProxyManager.Instance.RemoveProxy(this);
        }

        private void Start()
        {
            //Debug.Log("[HLDO][Proxy] Started name:" + gameObject.name);
            foreach (var mainfest in mainfests)
            {
                mainfest.Init();
            }

            Init();
        }

        public void CalculateVisibleDistance(float screenPercent)
        {
            float largestAxis = 0;
            largestAxis = Mathf.Abs(bounds.size.x);
            largestAxis = Mathf.Max(largestAxis, Mathf.Abs(bounds.size.y));
            largestAxis = Mathf.Max(largestAxis, Mathf.Abs(bounds.size.z));
            sqrVisibleDistance = (largestAxis / screenPercent) * (largestAxis / screenPercent);
        }

        public bool GetVisibility()
        {
            return m_lastproxyVisible;
        }
        
        public void SetVisibility(bool proxyVisible, bool originVisible, bool force = false)
        {
            if (m_lastproxyVisible != proxyVisible || force)
            {
#if _HLOD_USE_ATHENA_PVS_
                if (null != itemCompoenent)
                    itemCompoenent.SetVisibilityByMask(ProxyManager.PvsBitMask, proxyVisible);
#else
                if (ProxyManager.Instance.UseShadowProxy)
                {
                    proxyRenderer.shadowCastingMode = proxyVisible ? ShadowCastingMode.On : ShadowCastingMode.ShadowsOnly;
                }                    
                else
                {
                    proxyRenderer.forceRenderingOff = !proxyVisible;
                }
#endif
                m_lastproxyVisible = proxyVisible;
                //Debug.Log(string.Format("[HLDO][Proxy] proxy {0} visiable changed to:{1}", gameObject.name, proxyVisible));
            }

            foreach (var mainfest in mainfests)
                mainfest.SetVisible(originVisible, force);
        }

#if UNITY_EDITOR
        public void SetProxyRenderEnabled(bool visible)
        {
            proxyRenderer.enabled = visible;
            foreach (var mainfest in mainfests)
                mainfest.SetVisible(visible);
        }
#endif

        public void SetVisibilityRecursive(bool proxyVisible, bool originVisible, bool force = false)
        {
            SetVisibility(proxyVisible, originVisible, force);
            foreach (var child in children)
                child.SetVisibilityRecursive(proxyVisible, originVisible, force);
        }

        static float CalculatePerspectiveDistance(Vector3 objectPos, Vector3 cameraPos, float screenRelativeMetric)
        {
            var sqrDist = (objectPos - cameraPos).sqrMagnitude;

            return sqrDist * screenRelativeMetric * screenRelativeMetric;
        }

        public void UpdateVisibility(Vector3 cameraPos, float screenRelativeMetric, float sqrMinVisibleDistance, float sqrMaxVisibleDistance)
        {
            float sqrDistance = CalculatePerspectiveDistance(bounds.center, cameraPos, screenRelativeMetric);
            var checkSqrDistance = Mathf.Clamp(sqrVisibleDistance, sqrMinVisibleDistance, sqrMaxVisibleDistance);
            if (sqrDistance < checkSqrDistance)
            {
                m_curproxyVisible = enableForceVisible ? forceVisible : false;
                SetVisibility(m_curproxyVisible, !m_curproxyVisible);
                foreach (var child in children)
                    child.UpdateVisibility(cameraPos, screenRelativeMetric, sqrMinVisibleDistance, sqrMaxVisibleDistance);
            }
            else
            {
                m_curproxyVisible = enableForceVisible ? forceVisible : true;
                SetVisibility(m_curproxyVisible, !m_curproxyVisible);
                foreach (var child in children)
                    child.SetVisibilityRecursive(false, false);
            }
        }

        public void Init()
        {
            m_lastproxyVisible = false;

#if _HLOD_USE_ATHENA_PVS_
            if (null == proxyRenderer)
                Debug.LogError("VisibilityItemComponent is null!!! proxy: " + this.name);

            if (null == itemCompoenent)
                itemCompoenent = proxyRenderer.GetComponent<VisibilityItemComponent>();
            if (null == itemCompoenent)
                itemCompoenent = proxyRenderer.gameObject.AddComponent<VisibilityItemComponent>();
            
#else
            SetShadowProxy();
#endif
        }

        public void SwitchShadowProxy()
        {
            SetShadowProxy();
            foreach (var child in children)
                child.SwitchShadowProxy();
        }

        public void SetShadowProxy()
        {
#if _HLOD_USE_ATHENA_PVS_
            if (null != itemCompoenent)
                itemCompoenent.m_UseShadowCastMode = ProxyManager.UseShadowProxy;
#else
            if (ProxyManager.Instance.UseShadowProxy)
            {
                proxyRenderer.forceRenderingOff = false;
                proxyRenderer.shadowCastingMode = m_lastproxyVisible ? ShadowCastingMode.On : ShadowCastingMode.ShadowsOnly;
            }
            else
            {
                proxyRenderer.forceRenderingOff = !m_lastproxyVisible;
                proxyRenderer.shadowCastingMode = ShadowCastingMode.On;
            }
#endif

            foreach (var mainfest in mainfests)
                mainfest.SetShadowProxy();
        }
    }
}
