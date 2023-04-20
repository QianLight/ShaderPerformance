using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEngine.Rendering.Universal
{
    [Serializable, VolumeComponentMenu("Post-processing/Atmospher")]
    public class Atmospher : VolumeComponent
    {
        public ClampedFloatParameter fogScatteringScale = new ClampedFloatParameter(1, 0, 1);

        public ClampedFloatParameter molecularDensity = new ClampedFloatParameter(2.545f, 0, 3);
        public ClampedFloatParameter wavelengthR = new ClampedFloatParameter(584.7f, 380f, 740f);
        public ClampedFloatParameter wavelengthG = new ClampedFloatParameter(541.8f, 380f, 740f);
        public ClampedFloatParameter wavelengthB = new ClampedFloatParameter(493.4f, 380f, 740f);
        public ClampedFloatParameter rayleigh = new ClampedFloatParameter(1.5f, 0f, 2f);
        public ClampedFloatParameter mie = new ClampedFloatParameter(1.0f, 0f, 10f);
        public ClampedFloatParameter scattering = new ClampedFloatParameter(0.25f, 0f, 1f);
        public ClampedFloatParameter exposure = new ClampedFloatParameter(2.0f, 0f, 10f);
        public ColorParameter rayleighColor = new ColorParameter(new Color(0.128f, 0.5172f, 1f));
        public ColorParameter mieColor = new ColorParameter(new Color(0.298f, 0.7568f, 1f));
        public ClampedIntParameter scatteringTexWidth = new ClampedIntParameter(256, 64, 1024);
        public ColorParameter fogScatterColor =
            new ColorParameter(new Color(0.06f, 0.98f, 1, 1), true, false, true, true);
        [Serializable]
        private class Uniforms
        {
            internal static readonly int FogScatteringScale = Shader.PropertyToID("_AtmospherFogScatteringScale");

            internal static readonly int ScatteringMode = Shader.PropertyToID("_AtmospherScatteringMode");
            internal static readonly int Rayleigh = Shader.PropertyToID("_AtmospherRayleigh");
            internal static readonly int Mie = Shader.PropertyToID("_AtmospherMie");
            internal static readonly int Scattering = Shader.PropertyToID("_AtmospherScattering");
            internal static readonly int Exposure = Shader.PropertyToID("_AtmospherExposure");
            internal static readonly int RayleighColor = Shader.PropertyToID("_AtmospherRayleighColor");
            internal static readonly int MieColor = Shader.PropertyToID("_AtmospherMieColor");
            internal static readonly int PrecomputeScatterTex = Shader.PropertyToID("_PrecomputeScatterTex");
        }

        private RenderTexture scatterRenderTexture;
        private Material m_PrecomputeMateiral;
        public bool scatterDirty;

        private static bool _isMaterialParamsDirty = true;
        public override void OnFireChange(Camera camera, Transform root, bool enable)
        {
            base.OnFireChange(camera, root, enable);
            _isMaterialParamsDirty = true;
        }
        
        public override void OnOverrideFinish()
        {
            base.OnOverrideFinish();
#if UNITY_EDITOR
            if (!UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset.AADebug)
            {
                _isMaterialParamsDirty = true;
            }
#endif
            if (_isMaterialParamsDirty)
            {
                Apply();
                _isMaterialParamsDirty = false;
            }
            
            scatterDirty = false;
        }

        public void Apply()
        {
            CpuPrecomputeScatter();

            // RenderTexture rt = PrecomputeScatter();
            //Shader.SetGlobalTexture(Uniforms.PrecomputeScatterTex, rt);
            //Shader.SetGlobalFloat(Uniforms.Exposure, exposure.value);
        }

        public void MarkDirty()
        {
            scatterDirty = true;
        }

        private Vector3 ComputeRayleigh()
        {
            Vector3 rayleigh = Vector3.one;
            Vector3 scatteringWavelength = new Vector3(wavelengthR.value, wavelengthG.value,
                wavelengthB.value);
            Vector3 lambda = scatteringWavelength * 1e-9f;
            float n = 1.0003f; // Refractive index of air
            float pn = 0.035f; // Depolarization factor for standard air.
            float n2 = n * n;
            //float N = 2.545E25f;
            float N = molecularDensity.value;
            float temp = (8.0f * Mathf.PI * Mathf.PI * Mathf.PI * ((n2 - 1.0f) * (n2 - 1.0f))) / (3.0f * N * 1E25f) *
                         ((6.0f + 3.0f * pn) / (6.0f - 7.0f * pn));

            rayleigh.x = temp / Mathf.Pow(lambda.x, 4.0f);
            rayleigh.y = temp / Mathf.Pow(lambda.y, 4.0f);
            rayleigh.z = temp / Mathf.Pow(lambda.z, 4.0f);

            return rayleigh;
        }

        private Vector3 ComputeMie()
        {
            Vector3 mie;

            float c = (0.6544f * 5.0f - 0.6510f) * 10f * 1e-9f;
            Vector3 k = new Vector3(686.0f, 678.0f, 682.0f);

            mie.x = (434.0f * c * Mathf.PI * Mathf.Pow((4.0f * Mathf.PI) / wavelengthR.value, 2.0f) * k.x);
            mie.y = (434.0f * c * Mathf.PI * Mathf.Pow((4.0f * Mathf.PI) / wavelengthG.value, 2.0f) * k.y);
            mie.z = (434.0f * c * Mathf.PI * Mathf.Pow((4.0f * Mathf.PI) / wavelengthB.value, 2.0f) * k.z);
            return mie;
        }

        public RenderTexture PrecomputeScatter()
        {
            Shader.SetGlobalFloat(Uniforms.FogScatteringScale, fogScatteringScale.value);
            Shader.SetGlobalVector(Uniforms.Rayleigh, ComputeRayleigh() * rayleigh.value);
            Shader.SetGlobalVector(Uniforms.Mie, ComputeMie() * mie.value);
            Shader.SetGlobalFloat(Uniforms.Scattering, scattering.value * 60.0f);
            Shader.SetGlobalFloat(Uniforms.Exposure, exposure.value);
            Shader.SetGlobalVector(Uniforms.RayleighColor, rayleighColor.value);
            Shader.SetGlobalVector(Uniforms.MieColor, mieColor.value);

            if (!scatterRenderTexture)
            {
                scatterRenderTexture = RenderTexture.GetTemporary(scatteringTexWidth.value, 4, 0,
                    RenderTextureFormat.RGB111110Float,
                    RenderTextureReadWrite.sRGB);

                scatterRenderTexture.wrapMode = TextureWrapMode.Clamp;
            }

            Shader precomputeShader = Shader.Find("Hidden/UrpScatterPrecomputer");
            if (!m_PrecomputeMateiral || m_PrecomputeMateiral.shader != precomputeShader)
            {
                m_PrecomputeMateiral = new Material(precomputeShader);
            }

            Graphics.Blit(null, scatterRenderTexture, m_PrecomputeMateiral);

            return scatterRenderTexture;
        }

        #region Precompute

        // Constants
        public const float INTERNAL_PI = 3.1415926535f;
        public const float Pi316 = 0.0596831f;
        public const float Pi14 = 0.07957747f;
        public static readonly float3 MieG = new float3(0.4375f, 1.5625f, 1.5f);

        // Uniforms
        public float3 _MainLightDir0;
        public float _AtmospherFogScatteringScale;
        public float4x4 _AtmospherUpDirectionMatrix;
        public int _AtmospherScatteringMode;
        public float3 _AtmospherRayleigh;
        public float3 _AtmospherMie;
        public float _AtmospherScattering;
        public float _AtmospherExposure;
        public float4 _AtmospherRayleighColor;
        public float4 _AtmospherMieColor;
        public float _AtmospherMieDepth;
        public float _AtmospherLuminance;
        public float3 _AtmospherMoonDirection;
        public float4 _FogScatterColor;

        private const int FOG_VDL_COUNT = 32;
        private const int FOG_DEPTH_COUNT = 32;
        private const int SKYBOX_VDL_COUNT = 32;
        private const int SKYBOX_HEIGHT_COUNT = 32;

        private static readonly Vector4[] fogVdlData = new Vector4[FOG_VDL_COUNT];
        private static readonly Vector4[] fogDepthData = new Vector4[FOG_DEPTH_COUNT];
        private static readonly Vector4[] skyboxVdlData = new Vector4[SKYBOX_VDL_COUNT];
        private static readonly Vector4[] skyboxHeightData = new Vector4[SKYBOX_HEIGHT_COUNT];

        private static readonly int _ScatterFogVdl = Shader.PropertyToID("_ScatterFogVdl");
        private static readonly int _ScatterFogDepth = Shader.PropertyToID("_ScatterFogDepth");
        private static readonly int _ScatterSkyboxVdl = Shader.PropertyToID("_ScatterSkyboxVdl");
        private static readonly int _ScatterSkyboxHeight = Shader.PropertyToID("_ScatterSkyboxHeight");
        
        private bool mergeFogAndSkyboxVDL = FOG_VDL_COUNT == SKYBOX_VDL_COUNT;
        private bool mergeFogsunDepthAndSkyboxHeight = SKYBOX_VDL_COUNT == SKYBOX_HEIGHT_COUNT;
        private float3[] eSuns = new float3[FOG_DEPTH_COUNT];
        private float3[] fexs = new float3[FOG_DEPTH_COUNT];
        
        public void CpuPrecomputeScatter()
        {
            Vector3 rayleighPrecompute = ComputeRayleigh() * rayleigh.value;
            Vector3 miePrecompute = ComputeMie() * mie.value;
            
            Shader.SetGlobalFloat(Uniforms.FogScatteringScale, fogScatteringScale.value);
            Shader.SetGlobalVector(Uniforms.Rayleigh, rayleighPrecompute);
            Shader.SetGlobalVector(Uniforms.Mie, miePrecompute);
            Shader.SetGlobalFloat(Uniforms.Scattering, scattering.value * 60.0f);
            Shader.SetGlobalFloat(Uniforms.Exposure, exposure.value);
            Shader.SetGlobalVector(Uniforms.RayleighColor, rayleighColor.value);
            
            Shader.SetGlobalVector(Uniforms.MieColor, mieColor.value);
            _FogScatterColor = new float4(fogScatterColor.value.r, fogScatterColor.value.g, fogScatterColor.value.b, fogScatterColor.value.a);
            // Compute context.
            _MainLightDir0 = (Vector3)Shader.GetGlobalVector("_MainLightDir0");
            _AtmospherFogScatteringScale = fogScatteringScale.value;
            _AtmospherRayleigh = rayleighPrecompute;
            _AtmospherMie = miePrecompute;
            _AtmospherScattering = scattering.value * 60.0f;
            _AtmospherExposure = exposure.value;
            _AtmospherRayleighColor = (Vector4)rayleighColor.value;
            _AtmospherMieColor = (Vector4)mieColor.value;
            
            if (mergeFogAndSkyboxVDL)
            {
                for (int i = 0; i < FOG_VDL_COUNT; i++)
                {
                    float vdl = i / (FOG_VDL_COUNT - 1f);
                    vdl = math.sqrt(vdl);
                    vdl = math.sqrt(vdl);
                    vdl = vdl * 2f - 1f;
                    FogSkyboxVdlCommonComputation(in vdl, out float sunRise, out float3 BrmTheta);
                    fogVdlData[i] = (Vector3) ComputeFogSunVdlScatter(sunRise, BrmTheta);
                    skyboxVdlData[i] = (Vector3) ComputeSkyboxVdlScatter(sunRise, BrmTheta);
                }
                Shader.SetGlobalVectorArray(_ScatterFogVdl, fogVdlData);
                Shader.SetGlobalVectorArray(_ScatterSkyboxVdl, skyboxVdlData);
            }

            if (mergeFogsunDepthAndSkyboxHeight)
            {
                for (int i = 0; i < FOG_DEPTH_COUNT; i++)
                {
                    float depth = i / (FOG_DEPTH_COUNT - 1f);
                    FogSunDepthAndSkyboxHeightCommonComputation(in depth, out float3 eSun, out float3 fex);
                    eSuns[i] = eSun;
                    fexs[i] = fex;
                    fogDepthData[i] = (Vector3)ComputeFogSunDepthScatter(eSun, fex, depth);
                    
                    int commonResultIndex = 0;
                    if (i > 15)
                    {
                        commonResultIndex = 2 * i - 31;
                    }
                    skyboxHeightData[i] = (Vector3)ComputeSkyboxHeightScatter(eSuns[commonResultIndex], fexs[commonResultIndex]);
                }
                Shader.SetGlobalVectorArray(_ScatterFogDepth, fogDepthData);
                Shader.SetGlobalVectorArray(_ScatterSkyboxHeight, skyboxHeightData);
            }

            if (!mergeFogAndSkyboxVDL)
            {
                for (int i = 0; i < FOG_VDL_COUNT; i++)
                {
                    float vdl = i / (FOG_VDL_COUNT - 1f);
                    vdl = math.sqrt(vdl);
                    vdl = math.sqrt(vdl);
                    vdl = vdl * 2f - 1f;
                    fogVdlData[i] = (Vector3) ComputeFogSunVdlScatter(vdl);
                }
                Shader.SetGlobalVectorArray(_ScatterFogVdl, fogVdlData);
            }

            if (!mergeFogsunDepthAndSkyboxHeight)
            {
                for (int i = 0; i < FOG_DEPTH_COUNT; i++)
                {
                    float depth = i / (FOG_DEPTH_COUNT - 1f);
                    fogDepthData[i] = (Vector3) ComputeFogSunDepthScatter(depth);
                }
                Shader.SetGlobalVectorArray(_ScatterFogDepth, fogDepthData);
            }

            if (!mergeFogAndSkyboxVDL)
            {
                for (int i = 0; i < SKYBOX_VDL_COUNT; i++)
                {
                    float vdl = i / (SKYBOX_VDL_COUNT - 1f);
                    vdl = math.sqrt(vdl);
                    vdl = math.sqrt(vdl);
                    vdl = vdl * 2f - 1f;
                    skyboxVdlData[i] = (Vector3)ComputeSkyboxVdlScatter(vdl);
                }
                Shader.SetGlobalVectorArray(_ScatterSkyboxVdl, skyboxVdlData);
            }

            if (!mergeFogsunDepthAndSkyboxHeight)
            {
                for (int i = 0; i < SKYBOX_HEIGHT_COUNT; i++)
                {
                    float height = i / (SKYBOX_HEIGHT_COUNT - 1f) * 2 - 1;
                    skyboxHeightData[i] = (Vector3)ComputeSkyboxHeightScatter(height);
                }
                Shader.SetGlobalVectorArray(_ScatterSkyboxHeight, skyboxHeightData);
            }
        }

        void FogSkyboxVdlCommonComputation(in float vdl, out float sunRise, out float3 BrmTheta)
        {
            sunRise = FastSaturate(_MainLightDir0.y * 10);
            float rayPhase = 2.0f + 0.5f * vdl * vdl;
            float miePhase = MieG.x / math.pow(math.max(0f, MieG.y - MieG.z * vdl), 1.5f);
            float3 BrTheta = Pi316 * _AtmospherRayleigh * rayPhase * _AtmospherRayleighColor.xyz;
            float3 BmTheta = Pi14 * _AtmospherMie * miePhase * _AtmospherMieColor.xyz * sunRise;
            BrmTheta = (BrTheta + BmTheta) / (_AtmospherRayleigh + _AtmospherMie);
        }

        float3 ComputeFogSunVdlScatter(float sunRise, float3 BrmTheta)
        {
            float3 inScatter = BrmTheta * _AtmospherScattering * sunRise;
            inScatter = math.lerp(inScatter,
                math.dot(inScatter, new float3(0.2126729f, 0.7151522f, 0.0721750f)) * _FogScatterColor.xyz,
                _FogScatterColor.w);
            return inScatter;
        }
        float3 ComputeSkyboxVdlScatter(float sunRise, float3 BrmTheta)
        {
            float3 scatter = BrmTheta * sunRise;
            return scatter;
        }
        void FogSunDepthAndSkyboxHeightCommonComputation(in float heightOrDepth, out float3 eSun, out float3 fex)
        {
            float zenith = math.acos(FastSaturate(heightOrDepth)) * _AtmospherFogScatteringScale;
            float z = (math.cos(zenith) +
                       0.15f * math.pow(math.max(0, 93.885f - ((zenith * 180.0f) / INTERNAL_PI)), -1.253f));
            float2 srsm = new float2(8400.0f, 1200.0f) / z;
            fex = math.exp(-(_AtmospherRayleigh * srsm.x + _AtmospherMie * srsm.y));
            float sunset = FastClamp( _MainLightDir0.y, 0.0f, 0.5f);
            eSun = math.lerp(fex, (1 - fex), sunset);
        }
        float3 ComputeFogSunDepthScatter(float3 eSun, float3 fex, float depth)
        { 
            float mieDepth = FastSaturate(math.lerp(depth * 4, 1.0f, _AtmospherMieDepth));
            float3 scatter = eSun * (1.0f - fex) * mieDepth;
            return scatter;
        }
        float3 ComputeSkyboxHeightScatter(float3 eSun, float3 fex)
        {
            float3 scatter = eSun * _AtmospherScattering * (1 - fex);
            return scatter;
        }
        
        float3 ComputeFogSunVdlScatter(float vdl)
        {
            float sunRise = FastSaturate(_MainLightDir0.y * 10);
            float rayPhase = 2.0f + 0.5f * vdl * vdl;
            float miePhase = MieG.x / math.pow(math.max(0f, MieG.y - MieG.z * vdl), 1.5f);
            float3 BrTheta = Pi316 * _AtmospherRayleigh * rayPhase * _AtmospherRayleighColor.xyz;
            float3 BmTheta = Pi14 * _AtmospherMie * miePhase * _AtmospherMieColor.xyz * sunRise;
            float3 BrmTheta = (BrTheta + BmTheta) / (_AtmospherRayleigh + _AtmospherMie);
            float3 inScatter = BrmTheta * _AtmospherScattering * sunRise;
            inScatter = math.lerp(inScatter,
                math.dot(inScatter, new float3(0.2126729f, 0.7151522f, 0.0721750f)) * _FogScatterColor.xyz,
                _FogScatterColor.w);
            return inScatter;
        }

        float3 ComputeFogSunDepthScatter(float depth)
        {
            float zenith = math.acos(FastSaturate(-depth)) * _AtmospherFogScatteringScale;
            float z = (math.cos(zenith) +
                       0.15f * math.pow(math.max(0, 93.885f - ((zenith * 180.0f) / INTERNAL_PI)), -1.253f));
            float2 srsm = new float2(8400.0f, 1200.0f) / z;
            float3 fex = math.exp(-(_AtmospherRayleigh * srsm.x + _AtmospherMie * srsm.y));
            float sunset = FastClamp(_MainLightDir0.y, 0.0f, 0.5f);
            float3 Esun = math.lerp(fex, 1.0f - fex, sunset);
            float mieDepth = FastSaturate(math.lerp(depth * 4, 1.0f, _AtmospherMieDepth));
            float3 scatter = Esun * (1.0f - fex) * mieDepth;
            return scatter;
        }

        float3 ComputeSkyboxVdlScatter(float vdl)
        {
            float sunRise = FastSaturate(_MainLightDir0.y * 10);
            float rayPhase = 2.0f + 0.5f * vdl * vdl;
            float miePhase = MieG.x / math.pow(math.max(0, MieG.y - MieG.z * vdl), 1.5f);
            float3 BrTheta = Pi316 * _AtmospherRayleigh * rayPhase * _AtmospherRayleighColor.xyz;
            float3 BmTheta = Pi14 * _AtmospherMie * miePhase * _AtmospherMieColor.xyz * sunRise;
            float3 BrmTheta = (BrTheta + BmTheta) / (_AtmospherRayleigh + _AtmospherMie);
            float3 scatter = BrmTheta * sunRise;
            return scatter;
        }

        float3 ComputeSkyboxHeightScatter(float height)
        {
            float zenith = math.acos(FastSaturate(height)) * _AtmospherFogScatteringScale;
            float z = (math.cos(zenith) +
                       0.15f * math.pow(math.max(0, 93.885f - ((zenith * 180.0f) / INTERNAL_PI)), -1.253f));
            float SR = 8400.0f / z;
            float SM = 1200.0f / z;
            float3 fex = math.exp(-(_AtmospherRayleigh * SR + _AtmospherMie * SM));
            float sunset = FastClamp( _MainLightDir0.y, 0.0f, 0.5f);
            float3 eSun = math.lerp(fex, (1 - fex), sunset);
            float3 scatter = eSun * _AtmospherScattering * (1 - fex);
            return scatter;
        }

        #endregion
        private float FastClamp(float d, float min, float max) {
            float t = d < min ? min : d;
            return t > max ? max : t;
        }

        private float FastSaturate(float d)
        {
            float t = d < 0f ? 0f : d;
            return t > 1f ? 1f : t;
        }
    }
}