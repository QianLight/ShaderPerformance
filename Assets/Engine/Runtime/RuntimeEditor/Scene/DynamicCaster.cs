#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;
using UnityEngineEditor = UnityEditor.Editor;
namespace CFEngine
{
    public enum EShadowCasterType
    {
        None,
        MainCaster,
        ExtraCaster,
    }
    public struct RenderPair
    {
        public Renderer r;
        public Material shadowCaster;
        public MaterialPropertyBlock mpb;
        public bool isHair;
        public bool isFace;
    }
    public class ShadowRender
    {
        public List<RenderPair> renders = new List<RenderPair>();

        private int lastShadowSlot = -1;
        public void Resolve(Transform t)
        {
            if (t != null && t.gameObject.activeInHierarchy)
            {
                if (t.TryGetComponent(out Renderer r))
                {
                    if (r.enabled)
                    {
                        Material mat = r.sharedMaterial;
                        if (mat != null)
                        {
                            //mat.EnableKeyword("_SHADOW_MAP");
                        }

                        MaterialPropertyBlock mpb = CommonObject<MaterialPropertyBlock>.Get();
                        Material shadowCastMat = ShadowCullContext.GetShadowCasterMat(r, mpb);
                        string name = r.name.ToLower();
                        renders.Add(new RenderPair()
                        {
                            r = r,
                            mpb = mpb,
                            shadowCaster = shadowCastMat,
                            isHair = name.EndsWith("_hair"),
                            isFace = name.EndsWith("_face"),
                        });
                        mpb.SetVector(ShaderManager._Param, new Vector4(1, 1, 0, 1));
                    }

                }
                for (int i = 0; i < t.childCount; ++i)
                {
                    Resolve(t.GetChild(i));
                }
            }
        }

        public void Bind(Transform t)
        {
            if (renders.Count > 0)
                Clear();
            Resolve(t);
        }

        public void Clear()
        {
            foreach (var sr in renders)
            {
                if (sr.mpb != null)
                {
                    CommonObject<MaterialPropertyBlock>.Release(sr.mpb);
                }
                if (sr.r != null)
                {
                    sr.r.SetPropertyBlock(null);
                }
            }
            renders.Clear();
        }

        public void Update(Transform t, int slot)
        {
            if (slot != lastShadowSlot)
            {
                lastShadowSlot = slot;
                if (lastShadowSlot == -1)
                {
                    Clear();
                }
                else
                {
                    Bind(t);
                }
            }
            if (slot >= 0 && slot < ShadowModify.selfShadow.Length)
            {
                ShadowModify.selfShadow[slot] = this;
            }
        }
    }


    [DisallowMultipleComponent, ExecuteInEditMode]
    public class DynamicCaster : MonoBehaviour
    {
        public EShadowCasterType dynamicCastType = EShadowCasterType.None;
        [Range(-1, 2)]
        public int selfShadowSlot = -1;
        private ShadowRender shadowRender = new ShadowRender();
        private Transform t;

        private void Update()
        {
            EngineContext context = EngineContext.instance;
            if (context != null)
            {
                if (t == null)
                {
                    t = this.transform;
                }
                if (dynamicCastType == EShadowCasterType.MainCaster)
                {
                    ShadowModify.mainShadowList.Add(t);
                }
                else if (dynamicCastType == EShadowCasterType.ExtraCaster)
                {
                    ShadowModify.extraShadowList.Add(t);
                }
                shadowRender.Update(t, selfShadowSlot);
            }
        }

    }
}

#endif