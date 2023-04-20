using System;

namespace UnityEngine.Rendering.Universal
{
    public enum SkyboxMode
    {
        Atmospher,
        Custom,
    }
    
    [Serializable]
    public sealed class SkyboxModeParameter : VolumeParameter<SkyboxMode>
    {
        public SkyboxModeParameter(SkyboxMode value, bool overrideState = false) : base(value,
            overrideState)
        {
        }
    }
    
    [Serializable, VolumeComponentMenu("Post-processing/Skybox")]
    public class Skybox : VolumeComponent
    {
        public SkyboxModeParameter mode = new SkyboxModeParameter(SkyboxMode.Atmospher);
        public MaterialParameter customSkybox = new MaterialParameter(null);
        public BoolParameter customBackground = new BoolParameter(false);
        
        public ColorParameter cloudDarkColor =  new ColorParameter(new Color(1.0f, 1.0f, 1.0f, 1f), true, true, true);
        
        public ColorParameter cloudLightColor =  new ColorParameter(new Color(1.0f, 1.0f, 1.0f, 1f), true, true, true);
        
        public CubemapParameter baseTex = new CubemapParameter(null);

        public CubemapParameter maskTexture = new CubemapParameter(null);

        //public ClampedFloatParameter lightingRotX   = new ClampedFloatParameter(1f, -360f, 360f);
        //public ClampedFloatParameter lightingRotY   = new ClampedFloatParameter(1f, -360f, 360f);
        //public ClampedFloatParameter lightingPTSize = new ClampedFloatParameter(0.2f, 0f, 1.0f);

        //public ClampedFloatParameter flashFrequency = new ClampedFloatParameter(2.0f, 1f, 10.0f);
        //public ColorParameter lightingColor = new ColorParameter(new Color(1.0f, 1.0f, 1.0f, 1f), true, true, true);
       
        public ColorParameter borderColor = new ColorParameter(new Color(1.8338f, 1.6317f, 1.0968f, 1f),true,true,true);

        public ClampedFloatParameter borderEmissionRange = new ClampedFloatParameter(0.1f,0.01f,1f);

        public ClampedFloatParameter borderRange = new ClampedFloatParameter(1.0f,0.1f,1f,true);
        public ClampedFloatParameter scatterFade = new ClampedFloatParameter(1f, 1e-4f, 2);
        public ClampedFloatParameter alphaFade = new ClampedFloatParameter(1f, 1e-4f, 2);
        [HideInInspector]
        public ClampedFloatParameter fogFade = new ClampedFloatParameter(1f, 1e-4f, 2);
        public ClampedFloatParameter sssDirFade = new ClampedFloatParameter(1f, 1e-4f, 2);

        public ClampedFloatParameter fogStart = new ClampedFloatParameter(1f, -1f, 1f);
        public ClampedFloatParameter fogEnd = new ClampedFloatParameter(1f, -1f, 1f);

        public ClampedFloatParameter scatterStart = new ClampedFloatParameter(1f, -1f, 1f);
        public ClampedFloatParameter scatterEnd = new ClampedFloatParameter(1f, -1f, 1f);
        public ClampedFloatParameter sunScatterFalloff = new ClampedFloatParameter(1.0f, 0.01f, 10f);

        public ClampedFloatParameter sunScale = new ClampedFloatParameter(1e-4f, 0f, 0.01f);
        public ClampedFloatParameter rotateSpeed = new ClampedFloatParameter(1e-4f, 0f, 10f);

        public ColorParameter sunColor = new ColorParameter(new Color(0.74902f, 0.607843f, 0.376471f, 1f), true, true, true);
        public ColorParameter tintColor = new ColorParameter(new Color(0.698039f, 0.698039f, 0.698039f, 1f), true, true, true);
        public ColorParameter sssColor = new ColorParameter(new Color(0.74902f, 0.74902f, 0.321569f, 1f), true, true, true);
        public ColorParameter sunflareColor = new ColorParameter(new Color(1, 1, 1, 0.25f), true, true, true, true);
        public ClampedFloatParameter sunflareFalloff = new ClampedFloatParameter(0.6f, 0.001f, 1f);
        
        public ColorParameter skyUpperColor = new ColorParameter(new Color(0.52f, 0.8f, 1f, 1f), true, true, true, true);
        public ColorParameter skyLowerColor = new ColorParameter(new Color(0.2f, 0.3f, 0.8f, 1f), true, true, true, true);
        public FloatParameter skyHorizon = new FloatParameter(0f);
        public ClampedFloatParameter skyHorizonTilt = new ClampedFloatParameter(0f, -1f, 1f);

        public ClampedFloatParameter skyFinalExposure = new ClampedFloatParameter(1f, 0f, 10f);
        
        private Material material;
        public const string skyboxShaderName = "URP/Scene/Skybox";

        private ReciprocalCurve sssRemapper = ReciprocalCurve.Create(1f, false, false);
        private ReciprocalCurve fogRemapper = ReciprocalCurve.Create(1f, false, true);
        private ReciprocalCurve sssDirectionalRemapper = ReciprocalCurve.Create(1f, false, true);
        private ReciprocalCurve scatterRemappter = ReciprocalCurve.Create(1f, false, true);
        private Cubemap _defaultCubemap;
        #if UNITY_EDITOR
        public static Material EditorOverrideMaterial { get; set; }
        #endif

        private static class Uniforms
        {
            public static readonly int _BaseTex = Shader.PropertyToID("_BaseTex");
            public static readonly int _MaskTex = Shader.PropertyToID("_MaskTex");

            //public static readonly int _LightingPTRotX = Shader.PropertyToID("_LightingPTRotX");
            //public static readonly int _LightingPTRotY = Shader.PropertyToID("_LightingPTRotY");
            //public static readonly int _LightingPTSize = Shader.PropertyToID("_LightingPTSize");
            //public static readonly int _TexTiling = Shader.PropertyToID("_TexTiling");
            //public static readonly int _FlashFrequency = Shader.PropertyToID("_FlashFrequency");
            //public static readonly int _LightingColor = Shader.PropertyToID("_LightingColor");

            public static readonly int _SkyboxParams0 = Shader.PropertyToID("_SkyboxParams0");
            public static readonly int _SkyboxParams1 = Shader.PropertyToID("_SkyboxParams1");
            public static readonly int _SkyboxSunColor = Shader.PropertyToID("_SkyboxSunColor");
            public static readonly int _SssColor = Shader.PropertyToID("_SssColor");
            public static readonly int _Tint = Shader.PropertyToID("_Tint");
            public static readonly int _CurvesA = Shader.PropertyToID("_CurvesA");
            public static readonly int _CurvesB = Shader.PropertyToID("_CurvesB");
            public static readonly int _CurvesC = Shader.PropertyToID("_CurvesC");
            public static readonly int _SunFlareFalloff = Shader.PropertyToID("_SunFlareFalloff");
            public static readonly int _SunFlareColor = Shader.PropertyToID("_SunFlareColor");

            public static readonly int _SkyLowerColor = Shader.PropertyToID("_SkyLowerColor");
            public static readonly int _SkyUpperColor = Shader.PropertyToID("_SkyUpperColor");
            public static readonly int _SkyHorizon = Shader.PropertyToID("_SkyHorizon");
            public static readonly int _SkyHorizonTilt = Shader.PropertyToID("_SkyHorizonTilt");
            public static readonly int _SkyScatterFalloff = Shader.PropertyToID("_SkyScatterFalloff");
            public static readonly int _SkyFinalExposure = Shader.PropertyToID("_SkyFinalExposure");
        }

        private static bool _isMaterialParamsDirty = true;
        public override void OnFireChange(Camera camera, Transform root, bool enable)
        {
            base.OnFireChange(camera, root, enable);
            _isMaterialParamsDirty = true;
        }

        public override void OnOverrideFinish()
        {
            base.OnOverrideFinish();
            Update();
        }

        public void Atmospher2Custom()
        {
            mode.value = SkyboxMode.Custom;
        }

        public void Update()
        {
            #if UNITY_EDITOR
            if (EditorOverrideMaterial)
            {
                RenderSettings.skybox = EditorOverrideMaterial;
                return;
            }
            #endif

            if (mode.value == SkyboxMode.Custom)
            {
                if (customSkybox.value)
                {
                    RenderSettings.skybox = customSkybox.value;
                }
                else if (Application.isPlaying)
                {
                    Debug.LogWarning($"渲染天空盒失败，自定义天空盒不存在。");
                }
            }
            else if (mode.value == SkyboxMode.Atmospher)
            {
                if (!material)
                {
                    Shader shader = Shader.Find(skyboxShaderName);
                    if (!shader)
                    {
                        Debug.LogError($"渲染天空盒失败，找不到天空盒Shader。");
                        return;
                    }

                    material = new Material(shader);
                }

                if (maskTexture.value == null)
                {
                    if (_defaultCubemap == null)
                    {
                        _defaultCubemap = new Cubemap(1, TextureFormat.RGBA32, 1);
                        _defaultCubemap.SetPixel(CubemapFace.PositiveX, 0, 0, new Color(0f, 0f, 0f, 0f));
                        _defaultCubemap.SetPixel(CubemapFace.NegativeX, 0, 0, new Color(0f, 0f, 0f, 0f));
                        _defaultCubemap.SetPixel(CubemapFace.NegativeY, 0, 0, new Color(0f, 0f, 0f, 0f));
                        _defaultCubemap.SetPixel(CubemapFace.PositiveY, 0, 0, new Color(0f, 0f, 0f, 0f));
                        _defaultCubemap.SetPixel(CubemapFace.NegativeZ, 0, 0, new Color(0f, 0f, 0f, 0f));
                        _defaultCubemap.SetPixel(CubemapFace.PositiveZ, 0, 0, new Color(0f, 0f, 0f, 0f));
                        _defaultCubemap.Apply();
                    }

                    material.SetTexture(Uniforms._MaskTex, _defaultCubemap);
                }
                else
                {
                    material.SetTexture(Uniforms._MaskTex, maskTexture.value); // maskTexture.value);
                }

                material.SetTexture(Uniforms._BaseTex, baseTex.value);

#if UNITY_EDITOR
                if (!UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset.AADebug)
                {
                    _isMaterialParamsDirty = true;
                }
#endif

                if (_isMaterialParamsDirty)
                {
                    SetMaterialParams();
                    _isMaterialParamsDirty = false;
                    RenderSettings.skybox = material;
                }
 
            }
            else
            {
                Debug.LogError($"渲染天空盒失败，未定义的天空盒渲染类型。");
            }
        }

        void SetMaterialParams()
        {
            sssRemapper.Curveness = alphaFade.value;
            sssDirectionalRemapper.Curveness = sssDirFade.value;
            fogRemapper.Curveness = fogFade.value;
            scatterRemappter.Curveness = scatterFade.value;
            material.SetFloat("_AtmosStrength", scatterFade.value * 0.5f);
            Vector4 a = new Vector4(sssRemapper.A, sssDirectionalRemapper.A, fogRemapper.A, scatterRemappter.A);
            Vector4 b = new Vector4(sssRemapper.B, sssDirectionalRemapper.B, fogRemapper.B, scatterRemappter.B);
            Vector4 c = new Vector4(sssRemapper.C, sssDirectionalRemapper.C, fogRemapper.C, scatterRemappter.C);
            material.SetVector(Uniforms._CurvesA, a);
            material.SetVector(Uniforms._CurvesB, b);
            material.SetVector(Uniforms._CurvesC, c);
            //material.EnableKeyword("_LIGHTING_ON");
            //material.SetFloat(Uniforms._LightingPTRotX, lightingRotX.value);
            //material.SetFloat(Uniforms._LightingPTRotY, lightingRotY.value);
            //material.SetFloat(Uniforms._LightingPTSize, lightingPTSize.value);
            //material.SetFloat(Uniforms._FlashFrequency, flashFrequency.value);
            //material.SetColor("_LightingColor", lightingColor.value);

            material.SetColor("_BorderColor",borderColor.value);
            material.SetFloat("_BorderEmissionRange", 1f / borderEmissionRange.value);
            material.SetFloat("_BorderRange", borderRange.value);
            material.SetColor("_CloudDarkColor",cloudDarkColor.value);
            material.SetColor("_CloudLightColor",cloudLightColor.value);
            if (customBackground.value)
            {
                material.EnableKeyword("_CUSTOM_BACKGROUND");
            }
            else
            {
                material.DisableKeyword("_CUSTOM_BACKGROUND");
            }
            Vector4 skyboxParams0 = new Vector4(
                fogStart.value,
                fogEnd.value,
                scatterStart.value,
                scatterEnd.value
            );

            Vector4 skyboxParams1 = new Vector4(
                sunScale.value,
                rotateSpeed.value,
                0,
                0
            );

            material.SetVector(Uniforms._SkyboxParams0, skyboxParams0);
            material.SetVector(Uniforms._SkyboxParams1, skyboxParams1);
            material.SetVector(Uniforms._SkyboxSunColor, sunColor.value);
            material.SetVector(Uniforms._SssColor, sssColor.value);
            material.SetVector(Uniforms._Tint, tintColor.value);
            material.SetFloat(Uniforms._SkyScatterFalloff, sunScatterFalloff.value);
            material.SetFloat(Uniforms._SunFlareFalloff, sunflareFalloff.value);
            material.SetColor(Uniforms._SunFlareColor, sunflareColor.value);
            
            material.SetColor(Uniforms._SkyUpperColor, skyUpperColor.value);
            material.SetColor(Uniforms._SkyLowerColor, skyLowerColor.value);
            material.SetFloat(Uniforms._SkyHorizon, skyHorizon.value);
            material.SetFloat(Uniforms._SkyHorizonTilt, skyHorizonTilt.value);
            
            material.SetFloat(Uniforms._SkyFinalExposure, skyFinalExposure.value);
        }
    }
}
