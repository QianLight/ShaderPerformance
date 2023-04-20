using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if _HLOD_USE_ATHENA_PVS_
using PVS;
#endif

namespace com.pwrd.hlod
{
    [Serializable]
    public class Manifest
    {
        public GameObject go;
        public string name;
#if _HLOD_USE_ATHENA_PVS_
        public VisibilityItemComponent itemCompoenent;
#else
        public Renderer[] renderers;
#endif

        private bool m_lastVisible = true;

        public Manifest(GameObject ctrlNode)
        {
            this.go = ctrlNode;
            this.name = ctrlNode.name;
#if _HLOD_USE_ATHENA_PVS_
            itemCompoenent = go.GetComponent<VisibilityItemComponent>();
            if (itemCompoenent == null)
                itemCompoenent = go.AddComponent<VisibilityItemComponent>();
#else
            var list = new List<Renderer>();
            var lodGruop = go.GetComponentInChildren<LODGroup>();
            if (lodGruop != null)
            {
                foreach (var lod in lodGruop.GetLODs())
                {
                    foreach (var renderer in lod.renderers)
                    {
                        if (renderer != null)
                            list.Add(renderer);
                    }
                }
            }
            else
            {
                var r = go.GetComponentsInChildren<Renderer>();
                list.AddRange(r);
            }

            renderers = list.ToArray();
#endif
        }

        public void Init()
        {
            m_lastVisible = true;
            if (!Application.isPlaying)
                return;

            SetShadowProxy();
        }

        public void SetVisible(bool visible, bool force = false)
        {
            if (m_lastVisible == visible && !force)
            {
                return;
            }

#if _HLOD_USE_ATHENA_PVS_
            if (itemCompoenent != null)
                itemCompoenent.SetVisibilityByMask(ProxyManager.PvsBitMask, visible);
#else
            foreach (var render in renderers)
            {
                if (null != render)
                    render.forceRenderingOff = !visible;
            }
#endif

            m_lastVisible = visible;
        }

        public void SetShadowProxy()
        {
            foreach (var render in renderers)
            {
                if (null != render)
                    render.shadowCastingMode = ProxyManager.Instance.UseShadowProxy ? ShadowCastingMode.Off : ShadowCastingMode.On;
                else
                    HLODDebug.LogWarning($"[HLOD][Manifest] manifest is null. manifest: {name}");
            }
        }
    }
}