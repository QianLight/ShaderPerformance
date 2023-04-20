using System;
using System.Collections.Generic;
using UnityEngine;
#if _HLOD_USE_ATHENA_PVS_
using PVS;
#endif

namespace com.pwrd.hlod
{
    public class ProxyManager : MonoBehaviour
    {
        private static ProxyManager instance;
        public static ProxyManager Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = GameObject.FindObjectOfType<ProxyManager>();
                    if (instance == null)
                    {
                        GameObject gg = new GameObject("HLODRoot");
                        GameObject.DontDestroyOnLoad(gg);
                        instance = gg.AddComponent<ProxyManager>();
                    }
                }
                return instance;
            }
        }
        
        // runtime
        public float minVisibleDistance = 150;
        public float maxVisibleDistance = 300;
        public float LODBias = 1.0f;
        public bool UseShadowProxy = false;
        private bool LastUseShadowProxy = false;
        private bool enableUpdate = true;

        public List<Proxy> proxies = new List<Proxy>();
        public int PvsBitMask = -1;
        
        private Camera m_MainCamera;
        private Transform m_MainCameraTransform;

        public void AddProxy(Proxy item)
        {
            proxies.Add(item);
        }
        
        public void RemoveProxy(Proxy item)
        {
            proxies.Remove(item);
        }

        public void Clear()
        {
            proxies.Clear();
        }

        /// <summary>
        /// 启用刷新
        /// </summary>
        public void EnableUpdate(bool enable)
        {
            this.enableUpdate = enable;
        }
        
        /// <summary>
        /// 开启并刷新 强制显隐当前节点（将替换原有距离判定，后续新建节点不受此影响）
        /// </summary>
        public void EnableOrUpdateForceVisible(Func<Proxy, bool> customForceShowFunc)
        {
            if (customForceShowFunc == null) return;
            foreach (var proxy in proxies)
            {
                var show = customForceShowFunc(proxy);
                proxy.enableForceVisible = true;
                proxy.forceVisible = show;
            }
        }
        
        /// <summary>
        /// 关闭强制显隐
        /// </summary>
        public void DisableForceVisible()
        {
            foreach (var proxy in proxies)
            {
                proxy.enableForceVisible = false;
            }
        }
        
        private void Awake()
        {
            proxies.RemoveAll(s => s == null);
#if _HLOD_USE_ATHENA_PVS_
            PvsBitMask = VisibilityItemComponent.AllocVisibleBitMask();
#endif
            HLODDebug.Log("[HLDO][ProxyManager] Started!!");

            m_MainCamera = Camera.main;

            if (m_MainCamera == null)
            {
                HLODDebug.Log("[HLDO][ProxyManager] Error  The Camera.main is null!!");
                return;
            }

            ;
            m_MainCameraTransform = m_MainCamera.transform;
        }
#if _HLOD_USE_ATHENA_PVS_
        private void OnDestroy()
        {
            if(PvsBitMask >= 0)
                VisibilityItemComponent.RecycleVisibleBitMask(PvsBitMask);
            PvsBitMask = -1;
        }
#endif

        private void Update()
        {
            if (!enableUpdate) return;
            
            if (m_MainCamera == null)
            {
                m_MainCamera = Camera.main;
                if (m_MainCamera != null)
                {
                    m_MainCameraTransform = m_MainCamera.transform;
                }
                else
                {
                    return;
                }
            }

            Vector3 cameraPos = m_MainCameraTransform.position;
            float fieldOfView = m_MainCamera.fieldOfView;
            float screenRelativeMetric = CalcScreenRelativeMetric(fieldOfView);
            foreach (var proxy in proxies)
            {
                proxy.UpdateVisibility(cameraPos, screenRelativeMetric, minVisibleDistance * minVisibleDistance, maxVisibleDistance * maxVisibleDistance);
                if (LastUseShadowProxy ^ UseShadowProxy)
                    proxy.SwitchShadowProxy();
            }

            LastUseShadowProxy = UseShadowProxy;
        }

        private float CalcScreenRelativeMetric(float fieldOfView)
        {
            float halfAngle = Mathf.Tan(Mathf.Deg2Rad * fieldOfView * 0.5f);
            float screenRelativeMetric = 2.0f * halfAngle / LODBias;

            return screenRelativeMetric;
        }
        
#if UNITY_EDITOR
        [ContextMenu("Recovery Origin (Force)")]
        private void DisableProxy()
        {
            foreach (var proxy in proxies)
                proxy.SetVisibilityRecursive(false, true, true);

            var others = GameObject.FindObjectsOfType<Proxy>();
            foreach (var proxy in others)
                proxy.SetVisibilityRecursive(false, true, true);
        }
        
        [ContextMenu("Show Proxy (Force)")]
        private void EnableProxy()
        {
            foreach (var proxy in proxies)
                proxy.SetVisibilityRecursive(true, true, true);

            var others = GameObject.FindObjectsOfType<Proxy>();
            foreach (var proxy in others)
                proxy.SetVisibilityRecursive(true, true, true);
        }
#endif
    }
}