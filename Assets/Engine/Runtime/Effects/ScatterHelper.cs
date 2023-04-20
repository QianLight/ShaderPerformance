using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine
{
    public interface IScatterInfo
    {
        Texture Lut { get; }
        Vector4 Param { get; }
    }

    public class PreviewScatterInfo : IScatterInfo
    {
        public RenderTexture lutRT;
        public Vector4 param;

        public Texture Lut => lutRT;
        public Vector4 Param => param;
    }

    [System.Serializable]
    public struct SavedScatterInfo : IScatterInfo
    {
        public Texture2D lut;
        public Vector4 param;

        public Texture Lut => lut;
        public Vector4 Param => param;
    }

    public static class ScatterHelper
    {
        private const int LUT_WIDTH = 512;
        private const int LUT_HEIGHT = 4;

        private static class Uniforms
        {
            public static readonly int _PrecomputeScatterTex = Shader.PropertyToID("_PrecomputeScatterTex");
            public static readonly int _ScatteringSunDirection = Shader.PropertyToID("_ScatteringSunDirection");
            public static readonly int _ScatteringExposure = Shader.PropertyToID("_ScatteringExposure");
        }

        public static Vector3 CurrentSunDir => Shader.GetGlobalVector(Uniforms._ScatteringSunDirection);
        public static float CurrentExposure => Shader.GetGlobalFloat(Uniforms._ScatteringExposure);

        public static void Apply(IScatterInfo info)
        {
            Shader.SetGlobalTexture(Uniforms._PrecomputeScatterTex, info.Lut);
            Shader.SetGlobalVector(Uniforms._ScatteringSunDirection, info.Param);
            Shader.SetGlobalFloat(Uniforms._ScatteringExposure, info.Param.w);
        }

        public static void Bake(RenderContext rc, ref PreviewScatterInfo info)
        {
            if (!info.lutRT)
            {
                RenderTextureFormat format = RenderTextureFormat.ARGBHalf;

                if (!format.IsSupported())
                {
                    format = RenderTextureFormat.ARGB2101010;

                    // Note that using a log lut in ARGB32 is a *very* bad idea but we need it for
                    // compatibility reasons (else if a platform doesn't support one of the previous
                    // format it'll output a black screen, or worse will segfault on the user).
                    if (!format.IsSupported())
                        format = RenderTextureFormat.ARGB32;
                }

                info.lutRT = new RenderTexture(LUT_WIDTH, LUT_HEIGHT, 0, format, RenderTextureReadWrite.Linear)
                {
                    name = "Precomputed Scatter RT",
                    hideFlags = HideFlags.DontSave,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    anisoLevel = 0,
                    autoGenerateMips = false,
                    useMipMap = false
                };
                info.lutRT.Create();
            }

            Vector4 sunDir = Shader.GetGlobalVector(Uniforms._ScatteringSunDirection);
            float exposure = Shader.GetGlobalFloat(Uniforms._ScatteringExposure);
            info.param = new Vector4(sunDir.x, sunDir.y, sunDir.z, exposure);

            PropertySheet sheet = rc.propertySheets.Get(rc.resources.shaders.scatter);
            CommandBuffer cmd = rc.afterOpaqueCmd;
            RenderTargetIdentifier identify = info.lutRT;
            rc.preWorkingCmd.BlitFullscreenTriangle(null, ref identify, sheet, 0);
            cmd.SetGlobalTexture(Uniforms._PrecomputeScatterTex, identify);
        }
    }
}
