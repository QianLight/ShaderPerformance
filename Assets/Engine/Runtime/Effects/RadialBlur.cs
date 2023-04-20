using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine
{
    [Serializable]
    [Env(typeof(RadialBlurModify), "Env/Radial Blur", false)]
    public sealed class RadialBlur : EnvSetting
    {
        [CFParam4("Center.x", 0, 0, 2, -1, C4DataType.Float,
            "Center.y", 0.06f, -0.1f, 0.1f, -1, C4DataType.Float,
            "Center.z", 0.5f, 0, 1, -1, C4DataType.Float,
            "Size (米 | 屏幕高度)", 0.5f, 0, 1, -1, C4DataType.Float)]
        public Vector4Param param0 = new Vector4Param() { value = new Vector4(0, 0, 0, 0) };

        [CFParam4("Inner Radius", 0.2f, 0, 1, -1, C4DataType.FloatRange,
            "Inner Fadeout", 0.2f, 0, 1, -1, C4DataType.FloatRange,
            "Outer Radius", 0.5f, 0, 1, -1, C4DataType.FloatRange,
            "Outer Fadeout", 0.1f, 0, 1, -1, C4DataType.FloatRange)]
        public Vector4Param param1 = new Vector4Param() { value = new Vector4(0.2f, 0.2f, 0.5f, 0.1f) };

        [CFParam4("Intensity", 0.02f, -5, 5, -1, C4DataType.FloatRange,
            "UseScreenPos", 0, 0, 1, -1, C4DataType.Bool,
            "ForceEnable", 0, 0, 1, -1, C4DataType.Bool,
            "", 0, 0, 0, -1, C4DataType.None)]
        public Vector4Param param2 = new Vector4Param() { value = new Vector4(0.02f, 0, 0, 0) };

#if UNITY_EDITOR
        public static readonly SavedBool preview = new SavedBool($"{nameof(RadialBlur)}.{nameof(preview)}");
#endif

        public Vector3 Center => param0.value;
        public float WorldSize => param0.value.w;
        public float InnerRadius => param1.value.x;
        public float InnerFadeout => param1.value.y;
        public float OuterRadius => param1.value.z;
        public float OuterFadeout => param1.value.w;
        public float Intensity => param2.value.x;
        public bool UseScreenPos
        {
            get { return param2.value.y > 0; }
            set
            {
                ref Vector4 p = ref param2.value;
                p.y = value ? 1 : 0;
                param2.value = p;
            }
        }
        public bool forceEnable => param2.value.z > 0;

        public override bool IsEnabledAndSupported()
        {
            return QualitySettingData.current.radialBlurQuality.enable;
        }

        public override void InitParamaters(ListObjectWrapper<ISceneObject> objects, EnvModify envModify)
        {
            InnerInitParamters(objects, envModify, "RadialBlur");
            CreateParam(ref param0, nameof(param0), objects, envModify);
            CreateParam(ref param1, nameof(param1), objects, envModify);
            CreateParam(ref param2, nameof(param2), objects, envModify);
        }

        public override EnvSettingType GetEnvType()
        {
            return EnvSettingType.PPRadialBlur;
        }

        public override void ResetEffect()
        {
            active.value = false;
        }

        public override EnvSetting Load(CFBinaryReader reader, EngineContext context)
        {
            RadialBlur setting = Load<RadialBlur>((int)EnvSettingType.PPRadialBlur);
#if UNITY_EDITOR
            if (context.IsValidResVersion(RenderContext.ResVersionTexCurve, EngineContext.Cmp_L))
            {
                Vector4 temp = default;
                reader.ReadVector(ref temp);
            }
#endif
            return setting;
        }

#if UNITY_EDITOR
        public override void Save(BinaryWriter bw) { }
#endif

    }

    public sealed class RadialBlurModify : EnvModify<RadialBlur>
    {
        private static class Uniforms
        {
            public static readonly int params0 = Shader.PropertyToID("_Params0");
            public static readonly int params1 = Shader.PropertyToID("_Params1");
            public static readonly int params2 = Shader.PropertyToID("_Params2");
            public static readonly int radialBlurTex = Shader.PropertyToID("_RadialBlurTex");
            public static readonly int radialBlurDebugTex = Shader.PropertyToID("_RadialBlurDebugTex");
        }

        private static class Pass
        {
            public const int Downsample = 0;
            public const int MaskAndBlur = 1;
            public const int Blur = 2;
            public const int DrawDebugColor = 3;
        }

        private static class TempRT
        {
            public static RenderTargetHandle tempBlurRt = new RenderTargetHandle("_RT_RADIAL_BLUR_TEMP_0");
        }

        private const int BLUR_SAMPLE_COUNT = 4;
        private int motionState = Effect_Disable;
        public static readonly int _RadialBlurParam = Shader.PropertyToID("_RadialBlurParam");

        public override void Start(EngineContext context, IRenderContext renderContext)
        {
            motionState = Effect_Disable;
        }

        public override void Release(EngineContext context, IRenderContext renderContext)
        {
            motionState = Effect_Disable;
        }

        public override bool OverrideSetting(EngineContext context, IGetEnvValue getEnvValue)
        {
            Vector3 position;
            if (getEnvValue.GetValue(EnvSetting.EnvEffect_RadialBlur_V2_UseScreenSpace) > 0)
            {
                float x = getEnvValue.GetValue(EnvSetting.EnvEffect_RadialBlur_V2_ScreenSpaceCenterX);
                float y = getEnvValue.GetValue(EnvSetting.EnvEffect_RadialBlur_V2_ScreenSpaceCenterY);
                position = new Vector3(x, y, 10);
                settings.UseScreenPos = true;
            }
            else
            {
                if (!getEnvValue.GetPos(out position))
                {
                    RecoverSetting(context, getEnvValue);
                    return false;
                }
                else
                {
                    settings.UseScreenPos = false;
                }
            }

            float size = getEnvValue.GetValue(EnvSetting.EnvEffect_RadialBlur_V2_Scale);
            float innerRadius = getEnvValue.GetValue(EnvSetting.EnvEffect_RadialBlur_V2_InnerRadius);
            float innerFadeOut = getEnvValue.GetValue(EnvSetting.EnvEffect_RadialBlur_V2_InnerFadeout);
            float outerRadius = getEnvValue.GetValue(EnvSetting.EnvEffect_RadialBlur_V2_OuterRadius);
            float outerFadeOut = getEnvValue.GetValue(EnvSetting.EnvEffect_RadialBlur_V2_OuterFadeout);
            float intensity = getEnvValue.GetValue(EnvSetting.EnvEffect_RadialBlur_V2_Intensity);

            settings.param0.value = new Vector4(
                position.x,
                position.y,
                position.z,
                size
            );

            settings.param1.value = new Vector4(
                innerRadius,
                innerFadeOut,
                outerRadius,
                outerFadeOut
            );

            settings.param2.value = new Vector4(
                intensity,
                settings.param2.value.y,
                0,
                0
            );

            settings.param0.overrideState = true;
            settings.param1.overrideState = true;
            settings.param2.overrideState = true;

            settings.active.overrideState = true;
            settings.active.value = true;

            motionState = Effect_Init;
            context.renderflag.SetFlag(EngineContext.RFlag_PrePPDirty, true);
            return true;
        }

        public override bool RecoverSetting(EngineContext context, IGetEnvValue getEnvValue)
        {
            settings.param0.overrideState = false;
            settings.param1.overrideState = false;
            settings.param2.overrideState = false;
            settings.active.value = false;
            context.renderflag.SetFlag(EngineContext.RFlag_PrePPDirty, true);
            return true;
        }

#if UNITY_EDITOR
        private static bool IsShowAreaColor()
        {
            if (AssetsConfig.shaderPPDebugContext.debugNames == null)
            {
                return false;
            }

            string debugModeName = AssetsConfig.shaderPPDebugContext.debugNames[GlobalContex.ee.ppDebugMode];
            return debugModeName == "RadialBlur/Area";
        }

        public override void BeginDump()
        {
            base.BeginDump();
            AddKeyName(Uniforms.params0, "_Params0");
            AddKeyName(Uniforms.params1, "_Params1");
            AddKeyName(Uniforms.params2, "_Params2");
        }
#endif
        public override void DirtySetting()
        {
            base.DirtySetting();
            EngineContext context = EngineContext.instance;
            context.renderflag.SetFlag(EngineContext.RFlag_PrePPDirty, true);
#if UNITY_EDITOR
            if (!EngineContext.IsRunning)
            {
                motionState = Effect_Init;
            }
#endif
        }
        public override void Render(EngineContext engineContext, IRenderContext renderContext)
        {
            RenderContext rc = renderContext as RenderContext;
            if (motionState == Effect_Init)
            {
                // Setting sheet proeprties
                motionState = Effect_Running;
                engineContext.renderflag.SetFlag(EngineContext.RFlag_PrePPDirty, true);
            }
            bool render = motionState == Effect_Running || settings.forceEnable;

#if UNITY_EDITOR
            render |= RadialBlur.preview.Value;
#endif
            if (render)
            {
#if UNITY_EDITOR
                if (!EngineContext.IsRunning)
                {
                    if (RenderContext.pausePostProcess)
                        return;
                }
#endif

                ref PPRadialBlurQuality quality = ref QualitySettingData.current.radialBlurQuality;

                Camera camera = engineContext.CameraRef;
                Vector3 cameraPos = engineContext.cameraPos;
                Vector3 screenPos;
                float screenSize;
                if (settings.UseScreenPos)
                {
                    Vector3 center = settings.Center;
                    screenPos = new Vector3(Screen.width * center.x, Screen.height * center.y, center.z);
                    screenSize = settings.WorldSize;
                }
                else
                {
                    screenPos = camera.WorldToScreenPoint(settings.Center);
                    if (Culling(ref screenPos, ref cameraPos, ref quality))
                    {
                        return;
                    }
                    Vector3 screenOffsetPos = camera.WorldToScreenPoint(settings.Center + Vector3.up * settings.WorldSize);
                    screenSize = -0.5f * (screenOffsetPos - screenPos).magnitude / Screen.height;
                }

                #region Prepare parameters


                Vector4 params0 = default;
                params0.x = Screen.width / (float)Screen.height / screenSize;
                params0.y = 1 / screenSize;
                params0.z = Screen.height / (float)Screen.width;
                params0.w = 1;

                float fadeOutProgress = Mathf.Clamp01(Vector3.Distance(settings.Center, cameraPos) / quality.cullDistance);
                float distanceFadeout = quality.fadeOutCurve.Evaluate(fadeOutProgress);
                float innerInvFadeout = 1 / (settings.InnerFadeout + 0.0001f);
                float outerInvFadeout = 1 / (settings.OuterFadeout + 0.0001f);
                Vector4 params1 = new Vector4(
                    innerInvFadeout,
                    -outerInvFadeout,
                    -settings.InnerRadius * innerInvFadeout,
                    settings.OuterRadius * outerInvFadeout
                );

                Vector4 params2x0 = new Vector4(
                    -screenPos.x / Screen.height / screenSize,
                    -screenPos.y / Screen.height / screenSize,
                    settings.Intensity / screenPos.z,
                    0
                );

                Vector4 params2x1 = params2x0;
                params2x1.z /= BLUR_SAMPLE_COUNT;

                #endregion

                #region Rendering

                CommandBuffer cmd = rc.prePPCommand;

                RuntimeUtilities.BeginProfile(cmd, "Radial Blur");

                PropertySheet radialBlurSheet = rc.propertySheets.Get(rc.resources.shaders.radialBlur);

                SetShaderValue(Uniforms.params0, radialBlurSheet.properties, params0);
                SetShaderValue(Uniforms.params1, radialBlurSheet.properties, params1);
                SetShaderValue(Uniforms.params2, radialBlurSheet.properties, params2x0);

                bool lowLod = screenSize < quality.lodSize;
                bool blurOnce = quality.times == PPRadialBlurTimes.One || lowLod;
                bool useDofRt = rc.stateFlag.HasFlag(RenderContext.SFlag_DofEnable);
                bool needTempRt = !useDofRt || !blurOnce;
                int width = rc.pixelWidth >> quality.downSample;
                int height = rc.pixelHeight >> quality.downSample;

                rc.GetTmpRT(cmd, RenderContext._RadialBlurRt.id, width, height, 0, RenderTextureFormat.ARGBHalf);
                if (needTempRt)
                {
                    rc.GetTmpRT(cmd, TempRT.tempBlurRt.id, width, height, 0, RenderTextureFormat.ARGBHalf);
                }

                if (blurOnce)
                {
                    if (useDofRt)
                    {
                        DrawDebugRT(rc, cmd, ref RenderContext._DofRT.rtID, width, height, radialBlurSheet);
                        cmd.BlitFullscreenTriangle(ref RenderContext._DofRT.rtID, ref RenderContext._RadialBlurRt.rtID, radialBlurSheet, Pass.MaskAndBlur);
                    }
                    else
                    {
                        DrawDebugRT(rc, cmd, ref rc.currentRT, width, height, radialBlurSheet);
                        cmd.BlitFullscreenTriangle(ref rc.currentRT, ref TempRT.tempBlurRt.rtID, radialBlurSheet, Pass.Downsample);
                        cmd.BlitFullscreenTriangle(ref TempRT.tempBlurRt.rtID, ref RenderContext._RadialBlurRt.rtID, radialBlurSheet, Pass.MaskAndBlur);
                    }
                }
                else
                {
                    if (useDofRt)
                    {
                        DrawDebugRT(rc, cmd, ref RenderContext._DofRT.rtID, width, height, radialBlurSheet);
                        cmd.BlitFullscreenTriangle(ref RenderContext._DofRT.rtID, ref TempRT.tempBlurRt.rtID, radialBlurSheet, Pass.MaskAndBlur);
                    }
                    else
                    {
                        DrawDebugRT(rc, cmd, ref rc.currentRT, width, height, radialBlurSheet);
                        cmd.BlitFullscreenTriangle(ref rc.currentRT, ref RenderContext._RadialBlurRt.rtID, radialBlurSheet, Pass.Downsample);
                        cmd.BlitFullscreenTriangle(ref RenderContext._RadialBlurRt.rtID, ref TempRT.tempBlurRt.rtID, radialBlurSheet, Pass.MaskAndBlur);
                    }
                    SetShaderValue(Uniforms.params2, radialBlurSheet.properties, params2x1);
                    cmd.BlitFullscreenTriangle(ref TempRT.tempBlurRt.rtID, ref RenderContext._RadialBlurRt.rtID, radialBlurSheet, Pass.Blur);
                }
                cmd.SetGlobalTexture(Uniforms.radialBlurTex, RenderContext._RadialBlurRt.rtID);

                if (needTempRt)
                {
                    rc.ReleaseTmpRT(cmd, ref TempRT.tempBlurRt);
                }

                PreEffect.EnablePreffect(PreEffect.PreEffect_RadialBlur);
                engineContext.renderflag.SetFlag(EngineContext.RFlag_PrePPDirty, true);
                rc.stateFlag.SetFlag(RenderContext.SFlag_RadialBlurEnable, true);

                RuntimeUtilities.EndProfile(cmd, "Radial Blur");

#if UNITY_EDITOR
                RenderContext._RadialBlurRt.autoRelease = true;
#endif

                #endregion
            }
            else
            {
                settings.active.value = false;
                settings.active.overrideState = false;
                PreEffect.EnablePreffect(PreEffect.PreEffect_RadialBlur, 0);
            }
        }

        private void DrawDebugRT(RenderContext rc, CommandBuffer cmd, ref RenderTargetIdentifier src, int width, int height, PropertySheet radialBlurSheet)
        {
#if UNITY_EDITOR
            if (IsShowAreaColor())
            {
                rc.GetTmpRT(cmd, RenderContext._RadialBlurDebugRt.id, width, height, 0, RenderTextureFormat.ARGBHalf);
                cmd.BlitFullscreenTriangle(ref src, ref RenderContext._RadialBlurDebugRt.rtID, radialBlurSheet, Pass.DrawDebugColor);
                cmd.SetGlobalTexture(Uniforms.radialBlurDebugTex, RenderContext._RadialBlurDebugRt.rtID);
                rc.stateFlag.SetFlag(RenderContext.SFlag_RadialBlurDebugEnable, true);
                RenderContext._RadialBlurDebugRt.autoRelease = true;
            }
#endif
        }

        private bool Culling(ref Vector3 screenPos, ref Vector3 cameraPos, ref PPRadialBlurQuality quality)
        {
            // 基本参数剔除
            if (settings.Intensity == 0 || settings.InnerRadius > settings.OuterRadius)
            {
                return true;
            }

            // 距离剔除
            float sqrDistance = Vector3.SqrMagnitude(settings.Center - cameraPos);
            if (sqrDistance > quality.cullDistance * quality.cullDistance)
            {
                return true;
            }

            // 视锥剔除 z
            if (screenPos.z < 0)
            {
                return true;
            }

            // 视锥剔除 x
            float width = settings.OuterRadius * Screen.height / Screen.width * settings.WorldSize / screenPos.z;
            float x01 = screenPos.x / Screen.width;
            if (x01 - width > 1f || x01 + width < 0f)
            {
                return true;
            }

            // 视锥剔除 y
            float height = settings.OuterRadius * settings.WorldSize / screenPos.z;
            float y01 = screenPos.y / Screen.height;
            if (y01 - height > 1f || y01 + height < 0f)
            {
                return true;
            }

            return false;
        }
    }
}
