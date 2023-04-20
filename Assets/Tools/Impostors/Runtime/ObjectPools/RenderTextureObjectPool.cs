using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Impostors.ObjectPools
{
    [Serializable]
    public class RenderTextureObjectPool : ObjectPool<RenderTexture>
    {
        public int TextureSize;
        public int Depth { get; }
        public bool UseMipMap { get; }
        public float MipMapBias { get; }
        public RenderTextureFormat RenderTextureFormat { get; }

        public RenderTextureObjectPool(int initialCapacity, int textureSize, int depth, bool useMipMap,
            float mipMapBias, RenderTextureFormat renderTextureFormat) : base(initialCapacity)
        {
            TextureSize = textureSize;
            Depth = depth;
            UseMipMap = useMipMap;
            MipMapBias = mipMapBias;
            RenderTextureFormat = renderTextureFormat;
        }

        public override RenderTexture Get()
        {
            var rt = base.Get();
            rt.Create();
            return rt;
        }

        private static float memoryUnit = 6.0f / 1024;
        public float CaculateUseMemory()
        {
            float used = memoryUnit * TextureSize * _usedObjects.Count;
            
            return used;
        }

        protected override RenderTexture CreateObjectInstance()
        {
            var result = new RenderTexture(TextureSize, TextureSize, Depth, RenderTextureFormat);
            result.useMipMap = UseMipMap;
            result.autoGenerateMips = false;
            result.mipMapBias = MipMapBias;
            result.vrUsage = VRTextureUsage.None;
            result.Create();
            return result;
        }

        public void Destory()
        {
            for (int i = 0; i < _usedObjects.Count; i++)
            {
                RenderTexture rt = _usedObjects[i];
                if (rt == null) continue;
                ProcessReturnedInstance(rt);
            }

            for (int i = 0; i < _availableObjects.Count; i++)
            {
                RenderTexture rt = _availableObjects[i];
                if (rt == null) continue;
                ProcessReturnedInstance(rt);
            }
        }

        protected override void ProcessReturnedInstance(RenderTexture instance)
        {
            instance.DiscardContents();
            instance.Release();
        }
    }
}