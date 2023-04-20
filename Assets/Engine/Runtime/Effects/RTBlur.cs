using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine
{
    [Serializable]
    [Env (typeof (RTBlurModify), "Env/RTBlur")]
    public sealed class RTBlur : EnvSetting
    {
        public override void InitParamaters (ListObjectWrapper<ISceneObject> objects, EnvModify envModify)
        {
            InnerInitParamters (objects, envModify, "RTBlur");
        }
        public override EnvSettingType GetEnvType ()
        {
            return EnvSettingType.PPRTBlur;
        }
        public override void ResetEffect ()
        {
            active.value = false;
        }

        public override EnvSetting Load (CFBinaryReader reader, EngineContext context)
        {
            RTBlur setting = Load<RTBlur> ((int) EnvSettingType.PPRTBlur);
            return setting;
        }

#if UNITY_EDITOR
        public override void Save (BinaryWriter bw) { }
#endif
    }

    public sealed class RTBlurModify : EnvModify<RTBlur>
    {
        const int Pass_DownSample = 0;
        const int Pass_Vertical = 1;
        const int Pass_Horizontal = 2;
        public static readonly int _SceneBlurRT = Shader.PropertyToID ("_SceneBlurRT");
        public static readonly int _DownSampleNum = Shader.PropertyToID ("_DownSampleNum");

        public static readonly int _BackgroundTex = Shader.PropertyToID ("_BackgroundTex");
        public static readonly int _BackgroundUV = Shader.PropertyToID ("_BackgroundUV");
        public static readonly int _BackgroundColor = Shader.PropertyToID ("_BackgroundColor");

        public override void Render (EngineContext context, IRenderContext renderContext)
        {
            var rc = renderContext as RenderContext;
            var miscConfig =  WorldSystem.miscConfig;

            int downSampleNum = miscConfig.miscParam.x;
            float blurSpreadSize =  miscConfig.miscParam.y;
            int blurIterations = miscConfig.miscParam.z;

            var cmd = rc.prePPCommand;

            RuntimeUtilities.BeginProfile (cmd, "RTBlur");
            var sheet = rc.propertySheets.Get (rc.resources.shaders.rtBlur);

            //==============================DownSample==============================
            var blurRT = rc.GetBlurRT ();
            RenderTargetIdentifier srcTex = context.blurTex != null?context.blurTex : rc.currentRT;
            cmd.BlitFullscreenTriangle (ref srcTex, blurRT, sheet, Pass_DownSample);

            //==============================Blur==============================
            float widthMod = 1.0f / (1.0f * (1 << downSampleNum));
            rc.GetTmpRT (cmd, ref RenderContext._TmpRT0, RenderContext.RTSize_Half);
            for (int i = 0; i < blurIterations; i++)
            {
                float iterationOffs = (i * 1.0f);
                sheet.properties.SetFloat (_DownSampleNum, blurSpreadSize * widthMod + iterationOffs);

                cmd.BlitFullscreenTriangle (blurRT, ref RenderContext._TmpRT0.rtID, sheet, Pass_Vertical);
                cmd.BlitFullscreenTriangle (ref RenderContext._TmpRT0.rtID, blurRT, sheet, Pass_Horizontal);
            }
            cmd.SetGlobalTexture (_SceneBlurRT, blurRT);
            rc.ReleaseTmpRT (cmd, ref RenderContext._TmpRT0);
            RuntimeUtilities.EndProfile (cmd, "RTBlur");

            context.backGroundTex = blurRT;
            context.logicflag.SetFlag (EngineContext.Flag_UIBGReady, true);

            settings.active.value = false;
            settings.active.overrideState = false;
            context.renderflag.SetFlag (EngineContext.RFlag_PrePPDirty, true);
        }
        public static void PostRender (EngineContext context, RenderContext rc, PropertySheet sheet)
        {
            if (context.backGroundTex != null)
            {
                sheet.properties.SetTexture (_BackgroundTex, context.backGroundTex);

                float texHalfWidth = context.backGroundTex.width * 0.5f;
                float texHalfHeight = context.backGroundTex.height * 0.5f;
                float realHalfWidth = rc.halfHeight / texHalfHeight * texHalfWidth;
                Vector3 backUV = new Vector3 (0, 1, 0);
                if (realHalfWidth > rc.halfWidth)
                {
                    backUV.x = (realHalfWidth - rc.halfWidth) / realHalfWidth * 0.5f;
                    backUV.y = rc.halfWidth / realHalfWidth * 0.5f + 0.5f;
                }
                backUV.z = context.renderflag.HasFlag (EngineContext.RFlag_BGBlack) ? 1 : 0;
                sheet.properties.SetVector (_BackgroundUV, backUV);
                sheet.properties.SetColor (_BackgroundColor, context.backGroundColor);
            }
        }

    }
}