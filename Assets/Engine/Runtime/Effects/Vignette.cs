using System;
using System.IO;
using UnityEngine;
namespace CFEngine
{
    [Serializable]
    [Env (typeof (VignetteModify), "Env/Vignette")]
    public sealed class Vignette : EnvSetting
    {
        [CFColorUsage (false, true, 0, 0, 0, 1), CFTooltip ("Vignette color. Use the alpha channel for transparency.")]
        public ColorParam color = new ColorParam { value = new Color (0f, 0f, 0f, 1f) };

        [CFParam4 ("CenterX", 0.5f, 0, 1, -1, C4DataType.FloatRange,
            "CenterY", 0.5f, 0, 1, -1, C4DataType.FloatRange,
            "", 0, 0, 1, -1, C4DataType.None,
            "", 0, 0, 1, -1, C4DataType.None)]
        public Vector4Param vignetteParam0 = new Vector4Param { value = new Vector4 (0.5f, 0.5f, 0, 0) };

        [CFParam4 ("Intensity", 0, 0f, 1, -1, C4DataType.FloatRange,
            "Smoothness", 0.2f, 0.01f, 1, -1, C4DataType.FloatRange,
            "Roundness", 0, 0, 1, -1, C4DataType.FloatRange,
            "Rounded", 0, 0, 1, -1, C4DataType.Bool)]
        public Vector4Param vignetteParam1 = new Vector4Param { value = new Vector4 (0, 0.2f, 1, 0) };

        public override void InitParamaters (ListObjectWrapper<ISceneObject> objects, EnvModify envModify)
        {
            InnerInitParamters (objects, envModify, "Vignette");
            CreateParam (ref color, nameof (color), objects, envModify);
            CreateParam (ref vignetteParam0, nameof (vignetteParam0), objects, envModify);
            CreateParam (ref vignetteParam1, nameof (vignetteParam1), objects, envModify);
        }

        public override EnvSettingType GetEnvType ()
        {
            return EnvSettingType.PPVignette;
        }

        public override void ResetEffect ()
        {
            active.value = true;
        }

        // public static void Creator (out EnvSetting setting, out EnvModify modify, bool createModify)
        // {
        //     setting = Create<Vignette> ();
        //     if (createModify)
        //         modify = CFAllocator.Allocate<VignetteModify> ();
        //     else
        //         modify = null;
        // }

        public override EnvSetting Load (CFBinaryReader reader, EngineContext context)
        {
            Vignette setting = Load<Vignette> ((int) EnvSettingType.PPVignette);
            reader.ReadVector (ref setting.color.value);
            reader.ReadVector (ref setting.vignetteParam0.value);
            reader.ReadVector (ref setting.vignetteParam1.value);
            return setting;
        }

#if UNITY_EDITOR
        public override void Save (BinaryWriter bw)
        {
            EditorCommon.WriteVector (bw, color.value);
            EditorCommon.WriteVector (bw, vignetteParam0.value);
            EditorCommon.WriteVector (bw, vignetteParam1.value);
        }
#endif
    }

    public sealed class VignetteModify : EnvModify<Vignette>
    {
        public static readonly int _Vignette_Color = Shader.PropertyToID ("_Vignette_Color");
        public static readonly int _Vignette_Center = Shader.PropertyToID ("_Vignette_Center");
        public static readonly int _Vignette_Settings = Shader.PropertyToID ("_Vignette_Settings");

        public override bool OverrideSetting (EngineContext context, IGetEnvValue getEnvValue)
        {
            Vector4 param = settings.vignetteParam1;
            param.x = getEnvValue.GetValue (EnvSetting.EnvEffect_Vignette_Intensity);
            param.y = getEnvValue.GetValue (EnvSetting.EnvEffect_Vignette_Smoothness);
            param.z = getEnvValue.GetValue (EnvSetting.EnvEffect_Vignette_Roundness);
            param.w = getEnvValue.GetValue (EnvSetting.EnvEffect_Vignette_Rounded);
            settings.vignetteParam1.value = param;
            settings.vignetteParam1.overrideState = true;
            return true;
        }

        public override bool RecoverSetting (EngineContext context, IGetEnvValue getEnvValue)
        {
            settings.vignetteParam1.overrideState = false;
            return true;
        }

        public override void Render (EngineContext context, IRenderContext renderContext)
        {
            var rc = renderContext as RenderContext;
            if (BeginUpdate () || rc.flag.HasFlag (RenderContext.Flag_UberClear))
            {

                var sheet = rc.uberSheet;
                sheet.properties.SetColor (_Vignette_Color, settings.color.value);

                sheet.properties.SetVector (_Vignette_Center, settings.vignetteParam0.value);
                var param1 = settings.vignetteParam1.value;
                float roundness = (1f - param1.z) * 6f + param1.z;
                sheet.properties.SetVector (_Vignette_Settings, new Vector4 (param1.x * 3f, param1.y * 5f, roundness, param1.w));
                EndUpdate ();
            }
        }
    }
}