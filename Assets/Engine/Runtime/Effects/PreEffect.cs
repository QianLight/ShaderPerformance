using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine
{
    public interface IPreEffect
    {
        void Render (EngineContext engineContext, RenderContext context, CommandBuffer cmd, PropertySheet sheet);
        void PostRender (EngineContext engineContext, RenderContext context, CommandBuffer cmd);
    }

    public sealed class PreEffect
    {
        public List<IPreEffect> preEffects = new List<IPreEffect> ();
        // private int noiseTextureIndex = 0;
        private static Vector4 param = Vector4.zero;
        public static byte PreEffect_GhostBlur = 0;
        public static byte PreEffect_MotionBlur = 1;
        public static byte PreEffect_RadialBlur = 2;
        public static byte PreEffect_Distortion = 3;

        const int Pass_PreEffect = 0;
        static readonly int _PreffectParam = Shader.PropertyToID ("_PreffectParam");
        static readonly int _NoiseTex = Shader.PropertyToID ("_NoiseTex");
        static readonly int _NoiseUV = Shader.PropertyToID ("_NoiseUV");
        public static void EnablePreffect (byte effect, int enable = 1)
        {
            if (effect == PreEffect_GhostBlur)
            {
                param.x = enable;
            }
            else if (effect == PreEffect_MotionBlur)
            {
                param.y = enable;
            }
            else if (effect == PreEffect_RadialBlur)
            {
                param.z = enable;
            }
            else if (effect == PreEffect_Distortion)
            {
                param.w = enable;
            }
        }

        public void Init (RenderContext context)
        {

#if UNITY_IOS||UNITY_EDITOR
            var sheet = context.propertySheets.Get (context.resources.shaders.preEffects);
            sheet.EnableKeyword ("_HIGH_QUALITY");
#endif
        }
        private void RenderPreEffect (EngineContext context, RenderContext rc, EnvSettingType envEffectType)
        {
            var modify = EnvHelp.GetEnvEffect (context, rc, envEffectType);

            if (modify == null)
                return;

            var preEffect = modify as IPreEffect;
            if (preEffect != null)
            {
                preEffects.Add (preEffect);
            }
        }

        public void PreRenderAfterOpaque (EngineContext engineContext, RenderContext context)
        {
            var cmd = context.afterOpaqueCmd;
            if (RenderPipelineManager.renderPipeline == OPRenderPipeline.LegacySRP)
            {
                if (cmd != null)
                {
#if !(UNITY_IOS ||UNITY_ANDROID)||UNITY_EDITOR

                    if (engineContext.renderflag.HasFlag (EngineContext.RFlag_NeedGrabPass))
                    {

                        if (context.flag.HasFlag (RenderContext.Flag_SupportGrabRT))
                        {
                            context.GetTmpRT (cmd, ref RenderContext._MainRT2, 0);
                            context.flag.SetFlag (RenderContext.Flag_MainRT2Create, true);

                            cmd.CopyRT (ref context.currentRT, ref RenderContext._MainRT2.rtID);
                            cmd.SetGlobalTexture (RenderContext._GrabTex, RenderContext._MainRT2.rtID);
                            // EnvHelp.RenderEffect (engineContext, context, EnvSettingType.Fog);
                            cmd.SetRenderTarget (context.currentRT, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
                        }
                    }
                    //else
                    //{
                    //    cmd.SetRenderTarget(context.currentRT, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
                    //}

#else
                    //cmd.SetRenderTarget (context.currentRT, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store);
#endif    
                    engineContext.renderflag.SetFlag (EngineContext.RFlag_NeedGrabPass, false);
                }

                EnvHelp.RenderEffect (engineContext, context, EnvSettingType.Fog);

                if(context.flag.HasFlag(RenderContext.Flag_MainRT2Create))
                {
                    context.ReleaseTmpRT(cmd, ref RenderContext._MainRT2);
                    context.flag.SetFlag(RenderContext.Flag_MainRT2Create, false);
                }
            }
//            if (cmd != null)
//            {
//                if (engineContext.HasFlag (EngineContext.UIBGReady) &&
//                    engineContext.HasFlag (EngineContext.OpaqueUIBG))
//                {
//                    var sheet = context.propertySheets.Get (context.resources.shaders.blurMesh);
//#if UNITY_EDITOR
//                    if (!EngineContext.IsRunning)
//                        UISceneSystem.UpdateUIMeshMatrix (engineContext);
//#endif
//                    RTBlurModify.PostRender (engineContext, context, sheet);
//                    cmd.DrawMesh (RuntimeUtilities.fullscreenTriangle,
//                        engineContext.uiMeshMatrix, sheet.material, 0, 0, sheet.properties);
//                }
//            }
        }

        public void LogicRender (EngineContext engineContext, RenderContext context)
        {
            if (RenderPipelineManager.renderPipeline == OPRenderPipeline.LegacySRP)
            {
                context.des = RenderPipelineManager.renderPipeline == OPRenderPipeline.LegacySRP ? RenderContext._MainRT1.rtID : context.ppTmpRT;
            }
            else
            {
                context.des = context.ppTmpRT;
            }

            context.SwitchRT ();
        }
        public void Render (EngineContext engineContext, RenderContext context, CommandBuffer cmd)
        {
            if (RenderPipelineManager.renderPipeline == OPRenderPipeline.LegacySRP)
            {
                if(!context.flag.HasFlag(RenderContext.Flag_MainRT1Create))
                {
                    context.GetTmpRT(cmd, RenderContext._MainRT1.id, 0);
                    context.flag.SetFlag(RenderContext.Flag_MainRT1Create, true);
                }                    
                context.des = RenderContext._MainRT1.rtID;
               
#if UNITY_EDITOR
                RenderContext._MainRT1.autoRelease = true;
#endif
            }
            else
            {
                context.des = context.ppTmpRT;
            }
            RenderPreEffect (engineContext, context, EnvSettingType.PPDistortion);
            RenderPreEffect (engineContext, context, EnvSettingType.PPMotionBlur);
            int passID = Pass_PreEffect;
            if (preEffects.Count > 0)
            {
                param = Vector4.zero;
                var sheet = context.propertySheets.Get (context.resources.shaders.preEffects);
                for (int i = 0; i < preEffects.Count; ++i)
                {
                    preEffects[i].Render (engineContext, context, cmd, sheet);
                }
                sheet.properties.SetVector (_PreffectParam, param);
                cmd.BlitFullscreenTriangle (ref context.currentRT, ref context.des, sheet, passID);
                for (int i = 0; i < preEffects.Count; ++i)
                {
                    preEffects[i].PostRender (engineContext, context, cmd);
                }
                preEffects.Clear ();
            }
            else
            {
                //main rt => rt 1
                context.FirstBlit (cmd);
            }
            context.SwitchRT ();
            var qs = QualitySettingData.current;
            if (qs.flag.HasFlag (QualitySet.Flag_EnableDOF))
            {
                EnvHelp.RenderEffect (engineContext, context, EnvSettingType.PPDepthOfField);
            }
            EnvHelp.RenderEffect (engineContext, context, EnvSettingType.PPRTBlur);
#if UNITY_EDITOR
            RenderContext._TmpQuarterRT0.autoRelease = false;
#endif

            EnvHelp.RenderEffect (engineContext, context, EnvSettingType.PPRadialBlur);

            context.stateFlag.SetFlag (RenderContext.SFlag_GodRayEnable, false);
            EnvHelp.RenderEffect (engineContext, context, EnvSettingType.PPVolumLight);
        }

        public void PostRender (EngineContext engineContext, RenderContext context)
        {
            //bool ppUIBG = false;
            var uberSheet = context.uberSheet;
            //if (engineContext.HasFlag (EngineContext.UIBGReady) &&
            //    !engineContext.HasFlag (EngineContext.OpaqueUIBG))
            //{
            //    RTBlurModify.PostRender (engineContext, context, uberSheet);
            //    ppUIBG = true;
            //}
            Vector4 globalSettings = Vector4.zero;
            globalSettings.x = context.stateFlag.HasFlag (RenderContext.SFlag_BloomEnable) ? 1 : 0;
            globalSettings.y = context.stateFlag.HasFlag (RenderContext.SFlag_GodRayEnable) ? 1 : 0;
            globalSettings.z = context.stateFlag.HasFlag (RenderContext.SFlag_ColorgradingEnable) &&
                !engineContext.renderflag.HasFlag (EngineContext.RFlag_CamertRT) ? 1 : 0;
            var qs = QualitySettingData.current;
             globalSettings.w = qs.flag.HasFlag(QualitySet.Flag_EnableFXAA) ? 1 : 0;
            uberSheet.properties.SetVector (RenderContext._Global_Setting, globalSettings);

            Vector4 globalSettings2 = Vector4.zero;
            globalSettings2.x = context.stateFlag.HasFlag(RenderContext.SFlag_DofEnable) ? 1 : 0;
            globalSettings2.y = context.stateFlag.HasFlag(RenderContext.SFlag_DistortionEnable) ? 1 : 0;
            globalSettings2.z = context.stateFlag.HasFlag(RenderContext.SFlag_RadialBlurEnable) ? 1 : 0;
            globalSettings2.w = context.stateFlag.HasFlag(RenderContext.SFlag_CustomLutEnable) ? 1 : 0;
            uberSheet.properties.SetVector (RenderContext._Global_Setting2, globalSettings2);

            context.stateFlag.SetFlag(RenderContext.SFlag_DistortionEnable, false);
        }
    }
}