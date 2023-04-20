using System;
using System.IO;
using Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.LWRP;
using UnityEngine.Rendering.Universal;

namespace CFEngine
{
    [Serializable]
    public sealed class FogModeParam : ParamOverride<FogMode, FogModeParam>
    {
        protected override void InnerInterp(ParamOverride<FogMode, FogModeParam> from,
            ParamOverride<FogMode, FogModeParam> to, float t)
        {
            value = t > 0.001f ? to.value : from.value;
        }
    }

    [Serializable]
    [Env(typeof(FogModify), "Env/Fog")]
    public sealed class Fog : EnvSetting
    {
        [CFParam4("High,Medium,Low", 3, 0, 3, -1, C4DataType.IntRange,
             "", 64, 1, 64, -1, C4DataType.None,
             "", 128, 1, 128, -1, C4DataType.None,
             "", 4, 1, 8, -1, C4DataType.None), CFTooltip("CSM Param.")]
        public Vector4Param _QualitySet = new Vector4Param {value = new Vector4(3, 32, 64, 4)};


        [CFFog(0, 500, 0, 1, 1, 1)] public FogParam baseDistance = new FogParam() {value = FogInfo.CreateEmpty()};

        [CFFog(0, 500, 0, 1, 1, 1)] public FogParam baseHeight = new FogParam() {value = FogInfo.CreateEmpty()};

        [CFFog(0, 500, 0, 1, 1, 1)] public FogParam noiseDistance = new FogParam() {value = FogInfo.CreateEmpty()};

        [CFFog(0, 500, 0, 1, 1, 1)] public FogParam noiseHeight = new FogParam() {value = FogInfo.CreateEmpty()};

        [CFParam4("Noise Offset.x", 0, 500, 100000, 0, C4DataType.Float,
            "Noise Offset.y", 0, 500, 100000, 0, C4DataType.Float,
            "Noise Offset.z", 0, 500, 100000, 0, C4DataType.Float,
            "Noise Scale", 10, 500, 100000, 0, C4DataType.Float)]
        public Vector4Param noiseScaleOffset = new Vector4Param() {value = new Vector4(0, 0, 0, 10)};

        [CFParam4("Noise Enable", 1, 0, 1, 0, C4DataType.Bool,
            "Wind.x", 0, 0, 0, 0, C4DataType.Float,
            "Wind.y", 0, 0, 0, 0, C4DataType.Float,
            "Wind.z", 0, 0, 0, 0, C4DataType.Float)]
        public Vector4Param noiseParams = new Vector4Param() {value = new Vector4(1, 0, 0, 0)};

        [CFParam4("Scatter Scale", 1, 0, 1, 0, C4DataType.Float,
            "Noise Density", 0, 0, 1, 0, C4DataType.FloatRange,
            "", 0, 0, 0, 0, C4DataType.None,
            "", 0, 0, 0, 0, C4DataType.None)]
        public Vector4Param scatterParams = new Vector4Param() {value = new Vector4(1, 0, 0, 0)};

        [CFColorUsage(true, true, 1, 1, 1, 1), CFTooltip("Fog Start Color")]
        public ColorParam startColor = new ColorParam {value = Color.white};

        [CFColorUsage(true, true, 1, 1, 1, 1), CFTooltip("Fog End Color")]
        public ColorParam endColor = new ColorParam {value = Color.white};

        [CFColorUsage(true, true, 1, 1, 1, 1), CFTooltip("Scatter Color")]
        public ColorParam scatterColor = new ColorParam {value = new Color(1f, 0.91f, 0.7f, 1f)};

        public override void InitParamaters(ListObjectWrapper<ISceneObject> objects, EnvModify envModify)
        {
            InnerInitParamters(objects, envModify, "Fog");

            CreateParam(ref noiseScaleOffset, nameof(noiseScaleOffset), objects, envModify);
            CreateParam(ref noiseParams, nameof(noiseParams), objects, envModify);
            CreateParam(ref startColor, nameof(startColor), objects, envModify);
            CreateParam(ref endColor, nameof(endColor), objects, envModify);
            CreateParam(ref scatterParams, nameof(scatterParams), objects, envModify);
            CreateParam(ref scatterColor, nameof(scatterColor), objects, envModify);

            CreateParam(ref baseDistance, nameof(baseDistance), objects, envModify);
            CreateParam(ref baseHeight, nameof(baseHeight), objects, envModify);
            CreateParam(ref noiseDistance, nameof(noiseDistance), objects, envModify);
            CreateParam(ref noiseHeight, nameof(noiseHeight), objects, envModify);
            CreateParam(ref _QualitySet, nameof(_QualitySet), objects, envModify);
        }

        public override EnvSettingType GetEnvType()
        {
            return EnvSettingType.Fog;
        }

        public override void ResetEffect()
        {
            active.value = true;
        }

        public override bool IsEnabledAndSupported()
        {
            EngineContext context = EngineContext.instance;
            return base.IsEnabledAndSupported() &&
                   !context.renderflag.HasFlag(EngineContext.RFlag_CamertRT) &&
                   !context.renderflag.HasFlag(EngineContext.RFlag_DummyCameraRender) &&
                   !context.renderflag.HasFlag(EngineContext.RFlag_Capture);
        }

        public override EnvSetting Load(CFBinaryReader reader, EngineContext context)
        {
            Fog setting = Load<Fog>((int) EnvSettingType.Fog);
#if UNITY_EDITOR
            if (context.IsValidResVersion(RenderContext.ResVersionScatteredFog, EngineContext.Cmp_E))
            {
                Vector4 _ = default;
                reader.ReadVector(ref _);
                reader.ReadVector(ref _);
                reader.ReadVector(ref _);
                reader.ReadVector(ref _);
                reader.ReadVector(ref _);
                reader.ReadVector(ref _);
                reader.ReadVector(ref setting.noiseScaleOffset.value);
                reader.ReadVector(ref setting.noiseParams.value);
                reader.ReadVector(ref setting.startColor.value);
                reader.ReadVector(ref setting.endColor.value);
                reader.ReadVector(ref setting.scatterParams.value);
                reader.ReadVector(ref setting.scatterColor.value);
            }
            else if (context.IsValidResVersion(RenderContext.ResVersionOptimizedFog2, EngineContext.Cmp_E))
            {
                Vector4 _ = default;
                reader.ReadVector(ref _);
                reader.ReadVector(ref _);
                reader.ReadVector(ref _);
                reader.ReadVector(ref _);
                reader.ReadVector(ref _);
                reader.ReadVector(ref _);
                reader.ReadVector(ref setting.noiseScaleOffset.value);
                reader.ReadVector(ref setting.noiseParams.value);
                reader.ReadVector(ref setting.startColor.value);
                reader.ReadVector(ref setting.endColor.value);
            }
            else if (context.IsValidResVersion(RenderContext.ResVersionOptimizedFog, EngineContext.Cmp_E))
            {
                Vector4 _ = default;
                reader.ReadVector(ref _);
                reader.ReadVector(ref _);
                reader.ReadVector(ref _);
                reader.ReadVector(ref _);
                reader.ReadVector(ref setting.noiseScaleOffset.value);
                reader.ReadVector(ref setting.noiseParams.value);
                reader.ReadVector(ref setting.startColor.value);
                reader.ReadVector(ref setting.endColor.value);
            }
            else if (context.IsValidResVersion(RenderContext.ResVersionOptimizedFog, EngineContext.Cmp_L))
            {
                // legacy fogMode
                reader.ReadByte();

                Vector4 temp = default;
                // legacy fogColor
                reader.ReadVector(ref temp);
                // legacy fogLight
                reader.ReadVector(ref temp);
                // legacy exp2FogHorizontal
                reader.ReadVector(ref temp);
                // legacy fogVertical
                reader.ReadVector(ref temp);
                // legacy skybox
                reader.ReadVector(ref temp);
                // legacy farColor
                reader.ReadVector(ref temp);
                // legacy midColor
                reader.ReadVector(ref temp);
                // legacy ppColor
                reader.ReadVector(ref temp);

                if (context.IsValidResVersion(RenderContext.ResVersionStart, EngineContext.Cmp_GE))
                {
                    // legacy ppFog2
                    reader.ReadVector(ref temp);
                    if (context.IsValidResVersion(RenderContext.ResVersionFogNoise3D, EngineContext.Cmp_GE))
                    {
                        // legacy ppFog3
                        reader.ReadVector(ref temp);
                    }

                    // legacy inscatterColor
                    reader.ReadVector(ref temp);
                }
            }
            else
#endif
            {
                setting.baseDistance.Load(reader);
                setting.baseHeight.Load(reader);
                setting.noiseDistance.Load(reader);
                setting.noiseHeight.Load(reader);
                reader.ReadVector(ref setting.noiseScaleOffset.value);
                reader.ReadVector(ref setting.noiseParams.value);
                reader.ReadVector(ref setting.startColor.value);
                reader.ReadVector(ref setting.endColor.value);
                reader.ReadVector(ref setting.scatterParams.value);
                reader.ReadVector(ref setting.scatterColor.value);
            }

            return setting;
        }

#if UNITY_EDITOR
        public override void Save(BinaryWriter bw)
        {
            baseDistance.Save(bw);
            baseHeight.Save(bw);
            noiseDistance.Save(bw);
            noiseHeight.Save(bw);
            EditorCommon.WriteVector(bw, noiseScaleOffset.value);
            EditorCommon.WriteVector(bw, noiseParams.value);
            EditorCommon.WriteVector(bw, startColor.value);
            EditorCommon.WriteVector(bw, endColor.value);
            EditorCommon.WriteVector(bw, scatterParams.value);
            EditorCommon.WriteVector(bw, scatterColor.value);
        }
#endif
    }

    public sealed class FogModify : EnvModify<Fog>
    {
        public static class Uniforms
        {
            public static readonly int _Noise3DTex = Shader.PropertyToID("_Noise3DTex");
            public static readonly int _FogStartParams = Shader.PropertyToID("_FogStartParams");
            public static readonly int _FogEndParams = Shader.PropertyToID("_FogEndParams");
            public static readonly int _FogIntensityMin = Shader.PropertyToID("_FogIntensityMin");
            public static readonly int _FogIntensityMax = Shader.PropertyToID("_FogIntensityMax");
            public static readonly int _FogIntensityScale = Shader.PropertyToID("_FogIntensityScale");
            public static readonly int _FogFalloffParams = Shader.PropertyToID("_FogFalloffParams");
            public static readonly int _FogNoiseScaleOffset = Shader.PropertyToID("_FogNoiseScaleOffset");
            public static readonly int _FogNoiseParams = Shader.PropertyToID("_FogNoiseParams");
            public static readonly int _FogStartColor = Shader.PropertyToID("_FogStartColor");
            public static readonly int _FogEndColor = Shader.PropertyToID("_FogEndColor");
            public static readonly int _ScatterParams = Shader.PropertyToID("_ScatterParams");
            public static readonly int _FogScatterColor = Shader.PropertyToID("_FogScatterColor");
            public static readonly int _GlobalFog = Shader.PropertyToID("_GlobalFog");
        }

        public static string _FRAMEBUFFER_FETCH = "_FRAMEBUFFER_FETCH";
        public Vector4 _GlobalFogvalue = Vector4.zero;
#if UNITY_EDITOR
        public override void BeginDump()
        {
            base.BeginDump();
            AddKeyName(Uniforms._Noise3DTex, "_Noise3DTex");
            AddKeyName(Uniforms._FogStartParams, "_FogStartParams");
            AddKeyName(Uniforms._FogEndParams, "_FogEndParams");
            AddKeyName(Uniforms._FogIntensityMin, "_FogIntensityMin");
            AddKeyName(Uniforms._FogIntensityMax, "_FogIntensityMax");
            AddKeyName(Uniforms._FogIntensityScale, "_FogIntensityScale");
            AddKeyName(Uniforms._FogFalloffParams, "_FogFalloffParams");
            AddKeyName(Uniforms._FogNoiseScaleOffset, "_FogNoiseScaleOffset");
            AddKeyName(Uniforms._FogNoiseParams, "_FogNoiseParams");
            AddKeyName(Uniforms._FogStartColor, "_FogStartColor");
            AddKeyName(Uniforms._FogEndColor, "_FogEndColor");
            AddKeyName(Uniforms._ScatterParams, "_ScatterParams");
        }
#endif

        public override void Update(EngineContext context, IRenderContext renderContext)
        {
            if (context.renderflag.HasFlag(EngineContext.RFlag_CamertRT))
                return;
            // Prepare wind offset vector
            Vector4 noiseParams = settings.noiseParams.value;
            Vector3 wind = new Vector3(
                noiseParams.y * Time.time,
                noiseParams.z * Time.time,
                noiseParams.w * Time.time
            );
            noiseParams = new Vector4(noiseParams.x, wind.x, wind.y, wind.z);

            Vector4 starts = new Vector4(
                settings.baseDistance.value.start,
                settings.baseHeight.value.end,
                settings.noiseDistance.value.start,
                settings.noiseHeight.value.end
            );

            Vector4 ends = new Vector4(
                settings.baseDistance.value.end,
                settings.baseHeight.value.start,
                settings.noiseDistance.value.end,
                settings.noiseHeight.value.start
            );

            Vector4 intensityMin = new Vector4(
                settings.baseDistance.value.intensityMin,
                settings.baseHeight.value.intensityMin,
                settings.noiseDistance.value.intensityMin,
                settings.noiseHeight.value.intensityMin
            );

            Vector4 intensityMax = new Vector4(
                settings.baseDistance.value.intensityMax,
                settings.baseHeight.value.intensityMax,
                settings.noiseDistance.value.intensityMax,
                settings.noiseHeight.value.intensityMax
            );

            Vector4 intensityScale = new Vector4(
                settings.baseDistance.value.intensityScale,
                settings.baseHeight.value.intensityScale,
                settings.noiseDistance.value.intensityScale,
                settings.noiseHeight.value.intensityScale
            );

            Vector4 fallOff = new Vector4(
                settings.baseDistance.value.fallOff,
                settings.baseHeight.value.fallOff,
                settings.noiseDistance.value.fallOff,
                settings.noiseHeight.value.fallOff
            );

            RenderContext rc = renderContext as RenderContext;
            SetShaderValue(Uniforms._Noise3DTex, rc.resources.fogNoise3d);
            SetShaderValue(Uniforms._FogStartParams, ref starts);
            SetShaderValue(Uniforms._FogEndParams, ref ends);
            SetShaderValue(Uniforms._FogIntensityMin, ref intensityMin);
            SetShaderValue(Uniforms._FogIntensityMax, ref intensityMax);
            SetShaderValue(Uniforms._FogIntensityScale, ref intensityScale);
            SetShaderValue(Uniforms._FogFalloffParams, ref fallOff);
            SetShaderValue(Uniforms._FogNoiseScaleOffset, ref settings.noiseScaleOffset.value);
            SetShaderValue(Uniforms._FogNoiseParams, ref noiseParams);
            SetShaderValue(Uniforms._FogStartColor, ref settings.startColor.value);
            SetShaderValue(Uniforms._FogEndColor, ref settings.endColor.value);
            SetShaderValue(Uniforms._ScatterParams, ref settings.scatterParams.value);
            Shader.SetGlobalColor(Uniforms._FogScatterColor, settings.scatterColor.value);
        }


        public override void Render(EngineContext context, IRenderContext renderContext)
        {
            if (context.renderflag.HasFlag(EngineContext.RFlag_CamertRT))
                return;
            var qs = QualitySettingData.current;
            QualityLevel fogquality = qs.fogQuality;
            if (fogquality != QualityLevel.Low)
            {
                _GlobalFogvalue.x = 0f;
                Shader.SetGlobalVector(Uniforms._GlobalFog, _GlobalFogvalue);
#if UNITY_EDITOR
                context.renderflag.SetFlag(EngineContext.RFlag_NeedGrabPass, true);
#endif
                RenderContext rc = renderContext as RenderContext;
                PropertySheet sheet = rc.propertySheets.Get(rc.resources.shaders.fog);
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
                    sheet.EnableKeyword (_FRAMEBUFFER_FETCH);
#endif
                rc.afterOpaqueCmd.DrawMesh(RuntimeUtilities.fullscreenTriangle,
                    Matrix4x4.identity, sheet.material, 0, 0, sheet.properties);
            }
            else
            {
                _GlobalFogvalue.x = 0f;
                Shader.SetGlobalVector(Uniforms._GlobalFog, _GlobalFogvalue);
            }
        }

        private void Setfogquality()
        {
            if (settings._QualitySet.value.x == 3)
            {
                QualitySettingData.current.fogQuality = QualityLevel.High;
            }
            else if (settings._QualitySet.value.x == 2)
            {
                QualitySettingData.current.fogQuality = QualityLevel.Medium;
            }
            else
            {
                QualitySettingData.current.fogQuality = QualityLevel.Low;
            }
        }

        // public override void DirtySetting()
        // {
        //     base.DirtySetting();
        //     
        //     FogData fogData = new FogData()
        //     {
        //         // base
        //         startColor = settings.startColor,
        //         endColor = settings.endColor,
        //         baseDistance = TransferFogParam(settings.baseDistance),
        //         baseHeight = TransferFogParam(settings.baseHeight),
        //
        //         // noise
        //         noiseEnable = settings.noiseParams.value.x > 0.5f,
        //         noise3d = RenderContext.singleton.resources.fogNoise3d,
        //         noiseDistance = TransferFogParam(settings.noiseDistance),
        //         noiseHeight = TransferFogParam(settings.noiseHeight),
        //         windDirection = new Vector3(
        //             settings.noiseParams.value.y,
        //             settings.noiseParams.value.z,
        //             settings.noiseParams.value.w
        //         ),
        //
        //         // scatter
        //         scatterColor = settings.scatterColor,
        //         scatterScale = settings.scatterParams.value.x,
        //         scatterLerp = settings.scatterParams.value.z,
        //     };
        //     UniversalRenderPipeline.SetFog(fogData);
        // }

        // private UFogParam TransferFogParam(FogParam param)
        // {
        //     return new UFogParam()
        //     {
        //         start = param.value.start,
        //         end = param.value.end,
        //         intensityMin = param.value.intensityMin,
        //         intensityMax = param.value.intensityMax,
        //         intensityScale = param.value.intensityScale,
        //         fallOff = param.value.fallOff,
        //     };
        // }
    }
}