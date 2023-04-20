using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
namespace CFEngine
{
    [Serializable]
    [Env (typeof (DistortionScreen), "Env/Distortion Screen", false)]

    public sealed class DistortionScreen : EnvSetting
    {
        [CFParam4 ("Intensity", 0.05f, 0, 2, -1, C4DataType.FloatRange,
            "Speed", 2, 0, 10, -1, C4DataType.FloatRange,
            "Tiling", 1.1f, 0.001f, 4, -1, C4DataType.FloatRange,
            "", 0.5f, 0, 1, -1, C4DataType.None)]
        public Vector4Param param = new Vector4Param () { value = new Vector4 (0.05f, 2, 1.1f, 0) };

        public override void InitParamaters (ListObjectWrapper<ISceneObject> objects, EnvModify envModify)
        {
            InnerInitParamters (objects, envModify, "DistortionScreen");
            CreateParam (ref param, nameof (param), objects, envModify);
        }
        public override EnvSettingType GetEnvType ()
        {
            return EnvSettingType.PPDistortion;
        }
        public override void ResetEffect ()
        {
            active.value = false;
        }

        public override EnvSetting Load (CFBinaryReader reader, EngineContext context)
        {
            DistortionScreen setting = Load<DistortionScreen> ((int) EnvSettingType.PPDistortion);
            return setting;
        }

#if UNITY_EDITOR
        public override void Save (BinaryWriter bw) { }
#endif
    }

    public sealed class DistortionScreenModify : EnvModify<DistortionScreen>, IPreEffect
    {
        public static readonly int _DistortionPPParam = Shader.PropertyToID ("_DistortionPPParam");
        public static readonly int _DistortionPPTex0 = Shader.PropertyToID ("_DistortionPPTex0");
        public static readonly int _DistortionPPTex1 = Shader.PropertyToID ("_DistortionPPTex1");
        public void PostRender (EngineContext engineContext, RenderContext context, CommandBuffer cmd) { }

        public override bool OverrideSetting (EngineContext context, IGetEnvValue getEnvValue)
        {
            if (getEnvValue != null)
            {
                ref Vector4 param = ref settings.param.value;
                param.x = getEnvValue.GetValue (0);
                param.y = getEnvValue.GetValue (1);
                param.z = getEnvValue.GetValue (2);
            }
            settings.param.overrideState = true;
            context.renderflag.SetFlag (EngineContext.RFlag_PrePPDirty, true);
            return true;
        }

        public override bool RecoverSetting (EngineContext context, IGetEnvValue getEnvValue)
        {
            settings.param.overrideState = false;
            settings.active.value = false;
            context.renderflag.SetFlag (EngineContext.RFlag_PrePPDirty, true);
            return true;
        }

        public void Render (EngineContext engineContext, RenderContext context, CommandBuffer cmd, PropertySheet sheet)
        {
            SetShaderValue (_DistortionPPParam, sheet.properties, ref settings.param.value);
            var noise0 = context.resources.distortion0;
            if (noise0 != null)
            {
                SetShaderValue (_DistortionPPTex0, sheet.properties, noise0);
            }
            var noise1 = context.resources.distortion1;
            if (noise1 != null)
            {
                SetShaderValue (_DistortionPPTex1, sheet.properties, noise1);
            }
        }
    }
}