using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SMath = System.Math;
namespace CFEngine
{
    public enum TonemapperMode
    {
        UnityCustom,
        UnityACES,
    }

    public enum GradingMode
    {
        Realtime,
        CustomLut,
    }
    
    
    [Serializable]
    public sealed class TonemapperParam : EnumParam
    {
#if UNITY_EDITOR
        public TonemapperParam() : base()
        {
            gui = DecoratorGui;
        }

        public static int DecoratorGui(int src)
        {
            TonemapperMode tm = (TonemapperMode) src;
            tm = (TonemapperMode) EditorGUILayout.EnumPopup("Tonemapper Mode", tm);
            return (int) tm;
        }
#endif
    }

    [Serializable]
    public sealed class LutModeParam : EnumParam/*ParamOverride<GradingMode, LutModeParam>*/
    {
        // protected override void InnerInterp(ParamOverride<GradingMode, LutModeParam> from, ParamOverride<GradingMode, LutModeParam> to, float t)
        // {
        //     value = t > 0.01f ? to.value : from.value;
        // }
#if UNITY_EDITOR
        public LutModeParam() : base()
        {
            gui = DecoratorGui;
        }

        public static int DecoratorGui(int src)
        {
            GradingMode gm = (GradingMode) src;
            gm = (GradingMode) EditorGUILayout.EnumPopup("Grading Mode", gm);
            return (int) gm;
        }
#endif
    }

    // TODO: Could use some refactoring, too much duplicated code here
    [Serializable]
    [Env (typeof (ColorGradingModify), "Env/Color Grading")]
    public sealed class ColorGrading : EnvSetting
    {
        [CFResPath (typeof (Texture2D), "", EnvBlock.ResOffset_LutTex, true)]
        public ResParam customLut = new ResParam { value = "" };

        [CFEnum (typeof (TonemapperMode), (int) TonemapperMode.UnityCustom)]
        public TonemapperParam tonemapper = new TonemapperParam { value = (int)TonemapperMode.UnityCustom };

        [CFEnum (typeof (GradingMode), (int)GradingMode.Realtime)]
        public LutModeParam gradingMode = new LutModeParam { value = (int)GradingMode.Realtime };

        [CFParam4("Contribution", 0, 0f, 2f, -1, C4DataType.FloatRange,
            "", 1f, 0f, 3f, -1, C4DataType.None,
            "", 0f, 0f, 1f, -1, C4DataType.None,
            "", 0.5f, 0, 10, -1, C4DataType.None)]
        public Vector4Param customLutParam = new Vector4Param() { value = new Vector4() };

        [CFParam4 ("toneCurveToeStrength", 0, 0f, 1f, -1, C4DataType.FloatRange,
            "toneCurveToeLength", 0.5f, 0f, 1f, -1, C4DataType.FloatRange,
            "toneCurveShoulderStrength", 0f, 0f, 1f, -1, C4DataType.FloatRange,
            "toneCurveShoulderLength", 0.5f, 0, 10, -1, C4DataType.FloatRange)]
        public Vector4Param customTone0 = new Vector4Param { value = new Vector4 (0, 0.5f, 0, 0.5f) };

        [CFParam4 ("toneCurveShoulderAngle", 0, 0f, 1f, -1, C4DataType.FloatRange,
            "toneCurveGamma", 1f, 0.001f, 20f, -1, C4DataType.FloatRange,
            "", 0f, -180f, 180f, -1, C4DataType.None,
            "", 0f, -100f, 100f, -1, C4DataType.None)]
        public Vector4Param customTone1 = new Vector4Param { value = new Vector4 (0, 1, 0, 0) };

        [CFParam4 ("postExposure", 0, -20f, 20f, -1, C4DataType.FloatRange,
            "contrast", 0f, -100f, 100f, -1, C4DataType.FloatRange,
            "hueShift", 0f, -180f, 180f, -1, C4DataType.FloatRange,
            "saturation", 0f, -100f, 100f, -1, C4DataType.FloatRange)]
        public Vector4Param colorAdjustments = new Vector4Param { value = new Vector4 (0, 0, 0, 0) };

        [CFDisplayName ("colorFilter"), CFColorUsage (false, true, 1, 1, 1, 1)]
        public ColorParam colorFilter = new ColorParam { value = Color.white };

        [CFParam4 ("temperature", 0, -100, 100, -1, C4DataType.FloatRange,
            "tint", 0, -100, 100, -1, C4DataType.FloatRange,
            "balance", 0, -100, 100, -1, C4DataType.FloatRange,
            "preColorGrading", 0, 0, 1, -1, C4DataType.None)]
        public Vector4Param whiteBalance = new Vector4Param { value = new Vector4 (0, 0, 0, 0) };

        [CFDisplayName ("shadowColor"), CFColorUsage (false, false, 0.5f, 0.5f, 0.5f, 0.5f)]
        public ColorParam shadowColor = new ColorParam { value = Color.grey };

        [CFDisplayName ("highlights"), CFColorUsage (false, false, 0.5f, 0.5f, 0.5f, 0.5f)]
        public ColorParam highlightColor = new ColorParam { value = Color.grey };

        [CFParam4 ("redOutRedIn", 100f, -200f, 200f, -1, C4DataType.FloatRange,
            "redOutGreenIn", 0f, -200f, 200f, -1, C4DataType.FloatRange,
            "redOutBlueIn", 0f, -200f, 200f, -1, C4DataType.FloatRange,
            "", 0, 0, 1, -1, C4DataType.None)]
        public Vector4Param redInOut = new Vector4Param { value = new Vector4 (100f, 0, 0, 0) };

        [CFParam4 ("greenOutRedIn", 0f, -200f, 200f, -1, C4DataType.FloatRange,
            "greenOutGreenIn", 100f, -200f, 200f, -1, C4DataType.FloatRange,
            "greenOutBlueIn", 0f, -200f, 200f, -1, C4DataType.FloatRange,
            "", 0, 0, 1, -1, C4DataType.None)]
        public Vector4Param greenInOut = new Vector4Param { value = new Vector4 (0, 100f, 0, 0) };

        [CFParam4 ("blueOutRedIn", 0f, -200f, 200f, -1, C4DataType.FloatRange,
            "blueOutGreenIn", 0f, -200f, 200f, -1, C4DataType.FloatRange,
            "blueOutBlueIn", 100f, -200f, 200f, -1, C4DataType.FloatRange,
            "", 0, 0, 1, -1, C4DataType.None)]
        public Vector4Param blueInOut = new Vector4Param { value = new Vector4 (0, 0, 100f, 0) };

        [CFTrackballAttribute (0, 1f, 1f, 1f, 0f)]
        public Vector4Param lift = new Vector4Param { value = new Vector4 (1f, 1f, 1f, 0f) };

        [CFTrackballAttribute (0, 1f, 1f, 1f, 0f)]
        public Vector4Param gamma = new Vector4Param { value = new Vector4 (1f, 1f, 1f, 0f) };

        [CFTrackballAttribute (0, 1f, 1f, 1f, 0f)]
        public Vector4Param gain = new Vector4Param { value = new Vector4 (1f, 1f, 1f, 0f) };

        [CFTrackballAttribute (1, 1f, 1f, 1f, 0f)]
        public Vector4Param shadows = new Vector4Param { value = new Vector4 (1f, 1f, 1f, 0f) };

        [CFTrackballAttribute (1, 1f, 1f, 1f, 0f)]
        public Vector4Param midtones = new Vector4Param { value = new Vector4 (1f, 1f, 1f, 0f) };

        [CFTrackballAttribute (1, 1f, 1f, 1f, 0f)]
        public Vector4Param highlights = new Vector4Param { value = new Vector4 (1f, 1f, 1f, 0f) };

        [CFParam4 ("ShadowsStart", 0, 0f, 1f, -1, C4DataType.FloatRange,
            "ShadowsEnd", 0.3f, 0.3f, 2f, -1, C4DataType.FloatRange,
            "HighlightsStart", 0.55f, 0, 2f, -1, C4DataType.FloatRange,
            "HighlightsEnd", 1, 0f, 2f, -1, C4DataType.FloatRange)]
        public Vector4Param shadowHightlights = new Vector4Param { value = new Vector4 (0, 0.3f, 0.55f, 1) };

        [CFTextureCurve (2, 0, false, -1, 1, 1, 1), CFCustomOverride ()]
        public TextureCurveParam master = new TextureCurveParam () {
            curveType = TextureCurveParam.CurveTyp_Linear01,
            forceLinear01 = true,
        };

        [CFTextureCurve (2, 0, false, -1, 1, 0, 0), CFCustomOverride ()]
        public TextureCurveParam red = new TextureCurveParam () { curveType = TextureCurveParam.CurveTyp_Linear01 };

        [CFTextureCurve (2, 0, false, -1, 0, 1, 0), CFCustomOverride ()]
        public TextureCurveParam green = new TextureCurveParam () { curveType = TextureCurveParam.CurveTyp_Linear01 };

        [CFTextureCurve (2, 0, false, -1, 0, 0.5f, 1), CFCustomOverride ()]
        public TextureCurveParam blue = new TextureCurveParam () { curveType = TextureCurveParam.CurveTyp_Linear01 };

        [CFTextureCurve (0, 1, true, 0, 1, 1, 1), CFCustomOverride ()]
        public TextureCurveParam hueVsHue = new TextureCurveParam () { curveType = TextureCurveParam.CurveTyp_ConstHalf, loop = true };
        [CFTextureCurve (0, 1, true, 0, 1, 1, 1), CFCustomOverride ()]
        public TextureCurveParam hueVsSat = new TextureCurveParam () { curveType = TextureCurveParam.CurveTyp_ConstHalf, loop = true };
        [CFTextureCurve (0, 1, false, 1, 1, 1, 1), CFCustomOverride ()]
        public TextureCurveParam satVsSat = new TextureCurveParam () { curveType = TextureCurveParam.CurveTyp_ConstHalf };
        [CFTextureCurve (0, 1, false, 1, 1, 1, 1), CFCustomOverride ()]
        public TextureCurveParam lumVsSat = new TextureCurveParam () { curveType = TextureCurveParam.CurveTyp_ConstHalf };

#if UNITY_EDITOR
        public static bool breakLUT = false;
#endif

        public override void InitParamaters (ListObjectWrapper<ISceneObject> objects, EnvModify envModify)
        {
            InnerInitParamters (objects, envModify, "ColorGrading");

            CreateParam (ref tonemapper, nameof (tonemapper), objects, envModify);

            CreateParam (ref customTone0, nameof (customTone0), objects, envModify);
            CreateParam (ref customTone1, nameof (customTone1), objects, envModify);
            CreateParam (ref colorAdjustments, nameof (colorAdjustments), objects, envModify);
            CreateParam (ref colorFilter, nameof (colorFilter), objects, envModify);
            CreateParam (ref whiteBalance, nameof (whiteBalance), objects, envModify);
            CreateParam (ref shadowColor, nameof (shadowColor), objects, envModify);
            CreateParam (ref highlightColor, nameof (highlightColor), objects, envModify);

            CreateParam (ref redInOut, nameof (redInOut), objects, envModify);
            CreateParam (ref greenInOut, nameof (greenInOut), objects, envModify);
            CreateParam (ref blueInOut, nameof (blueInOut), objects, envModify);

            CreateParam (ref lift, nameof (lift), objects, envModify);
            CreateParam (ref gamma, nameof (gamma), objects, envModify);
            CreateParam (ref gain, nameof (gain), objects, envModify);

            CreateParam (ref shadows, nameof (shadows), objects, envModify);
            CreateParam (ref midtones, nameof (midtones), objects, envModify);
            CreateParam (ref highlights, nameof (highlights), objects, envModify);

            CreateParam (ref shadowHightlights, nameof (shadowHightlights), objects, envModify);

            CreateParam (ref master, nameof (master), objects, envModify);
            CreateParam (ref red, nameof (red), objects, envModify);
            CreateParam (ref green, nameof (green), objects, envModify);
            CreateParam (ref blue, nameof (blue), objects, envModify);

            CreateParam (ref hueVsHue, nameof (hueVsHue), objects, envModify);
            CreateParam (ref hueVsSat, nameof (hueVsSat), objects, envModify);
            CreateParam (ref satVsSat, nameof (satVsSat), objects, envModify);
            CreateParam (ref lumVsSat, nameof (lumVsSat), objects, envModify);

            CreateParam(ref gradingMode, nameof(gradingMode), objects, envModify);
            CreateParam(ref customLutParam, nameof(customLutParam), objects, envModify);
            CreateParam (ref customLut, nameof(customLut), objects, envModify);

            customLut.Init();
        }

        public override void UninitParamaters()
        {
            customLut.UnInit();
        }

#if UNITY_EDITOR
        public override void InitEditorParamaters (ListObjectWrapper<ISceneObject> objects, EnvModify envModify, bool init)
        {
            if (init)
            {
                master.InitCurve (2, TextureCurveParam.CurveTyp_Linear01);
                red.InitCurve (2, TextureCurveParam.CurveTyp_Linear01);
                green.InitCurve (2, TextureCurveParam.CurveTyp_Linear01);
                blue.InitCurve (2, TextureCurveParam.CurveTyp_Linear01);
                hueVsHue.InitCurve (0, TextureCurveParam.CurveTyp_ConstHalf);
                hueVsSat.InitCurve (0, TextureCurveParam.CurveTyp_ConstHalf);
                satVsSat.InitCurve (0, TextureCurveParam.CurveTyp_ConstHalf);
                lumVsSat.InitCurve (0, TextureCurveParam.CurveTyp_ConstHalf);
            }
            if (envModify != null)
            {
                master.defaultCurveType = TextureCurveParam.CurveTyp_Linear01;
                red.defaultCurveType = TextureCurveParam.CurveTyp_Linear01;
                green.defaultCurveType = TextureCurveParam.CurveTyp_Linear01;
                blue.defaultCurveType = TextureCurveParam.CurveTyp_Linear01;
                hueVsHue.defaultCurveType = TextureCurveParam.CurveTyp_ConstHalf;
                hueVsSat.defaultCurveType = TextureCurveParam.CurveTyp_ConstHalf;
                satVsSat.defaultCurveType = TextureCurveParam.CurveTyp_ConstHalf;
                lumVsSat.defaultCurveType = TextureCurveParam.CurveTyp_ConstHalf;
            }

            customLut.resType = ResObject.Tex_2D_PNG;
            if (init)
            {
                customLut.Init();
            }
        }
#endif

        public override EnvSettingType GetEnvType ()
        {
            return EnvSettingType.PPTonemapping;
        }
        public override void ResetEffect ()
        {
            active.value = true;
        }

        public override EnvSetting Load (CFBinaryReader reader, EngineContext context)
        {
            ColorGrading setting = Load<ColorGrading> ((int) EnvSettingType.PPTonemapping);
            setting.tonemapper.value = (int) reader.ReadByte ();
            reader.ReadVector (ref setting.customTone0.value);
            reader.ReadVector (ref setting.customTone1.value);

            reader.ReadVector (ref setting.colorAdjustments.value);

            reader.ReadVector (ref setting.colorFilter.value);
            reader.ReadVector (ref setting.whiteBalance.value);

            reader.ReadVector (ref setting.shadowColor.value);
            reader.ReadVector (ref setting.highlightColor.value);

            reader.ReadVector (ref setting.redInOut.value);
            reader.ReadVector (ref setting.greenInOut.value);
            reader.ReadVector (ref setting.blueInOut.value);

            reader.ReadVector (ref setting.lift.value);
            reader.ReadVector (ref setting.gamma.value);
            reader.ReadVector (ref setting.gain.value);

            reader.ReadVector (ref setting.shadows.value);
            reader.ReadVector (ref setting.midtones.value);
            reader.ReadVector (ref setting.highlights.value);
            reader.ReadVector (ref setting.shadowHightlights.value);

#if UNITY_EDITOR
            if (context.IsValidResVersion (RenderContext.ResVersionTexCurve, EngineContext.Cmp_GE))
#endif
            {
                setting.master.Load (reader);
                setting.red.Load (reader);
                setting.green.Load (reader);
                setting.blue.Load (reader);
                setting.hueVsHue.Load (reader);
                setting.hueVsSat.Load (reader);
                setting.satVsSat.Load (reader);
                setting.lumVsSat.Load (reader);
            }

#if UNITY_EDITOR
            if (context.IsValidResVersion (RenderContext.ResVersionCustomLut, EngineContext.Cmp_GE))
#endif
            {
                setting.gradingMode.value = (int)reader.ReadByte();
                setting.customLut.Load (reader);
                reader.ReadVector (ref setting.customLutParam.value);
            }

            return setting;
        }

#if UNITY_EDITOR
        public override void Save (BinaryWriter bw)
        {
            bw.Write ((byte)tonemapper.value);
            EditorCommon.WriteVector (bw, customTone0.value);
            EditorCommon.WriteVector (bw, customTone1.value);

            EditorCommon.WriteVector (bw, colorAdjustments.value);
            EditorCommon.WriteVector (bw, colorFilter.value);
            EditorCommon.WriteVector (bw, whiteBalance.value);

            EditorCommon.WriteVector (bw, shadowColor.value);
            EditorCommon.WriteVector (bw, highlightColor.value);

            EditorCommon.WriteVector (bw, redInOut.value);
            EditorCommon.WriteVector (bw, greenInOut.value);
            EditorCommon.WriteVector (bw, blueInOut.value);

            EditorCommon.WriteVector (bw, lift.value);
            EditorCommon.WriteVector (bw, gamma.value);
            EditorCommon.WriteVector (bw, gain.value);

            EditorCommon.WriteVector (bw, shadows.value);
            EditorCommon.WriteVector (bw, midtones.value);
            EditorCommon.WriteVector (bw, highlights.value);

            EditorCommon.WriteVector (bw, shadowHightlights.value);

            master.Save (bw);
            red.Save (bw);
            green.Save (bw);
            blue.Save (bw);
            hueVsHue.Save (bw);
            hueVsSat.Save (bw);
            satVsSat.Save (bw);
            lumVsSat.Save (bw);

            bw.Write ((byte)gradingMode.value);
            EditorCommon.WriteRes (bw, customLut);
            EditorCommon.WriteVector (bw, customLutParam.value);
        }
#endif
    }

    public sealed class ColorGradingModify : EnvModify<ColorGrading>
    {
        public const int k_Lut2DSize = 32;
        public const float k_DefaultLutExposure = 1;
        private RenderTexture internal2DLut = null;
        private RenderTargetIdentifier lutTex;
        //film param
        static readonly int _CustomLut2D = Shader.PropertyToID("_CustomLut2D");
        static readonly int _Lut2D = Shader.PropertyToID ("_Lut2D");
        static readonly int _Lut_Params = Shader.PropertyToID ("_Lut_Params");
        static readonly int _CustomLut_Params = Shader.PropertyToID("_CustomLut_Params");
        static readonly int _LutBake_Params = Shader.PropertyToID ("_LutBake_Params");
        static readonly int _ColorBalance = Shader.PropertyToID ("_ColorBalance");
        static readonly int _HueSatCon = Shader.PropertyToID ("_HueSatCon");
        static readonly int _ColorFilter = Shader.PropertyToID ("_ColorFilter");
        static readonly int _ChannelMixerRed = Shader.PropertyToID ("_ChannelMixerRed");
        static readonly int _ChannelMixerGreen = Shader.PropertyToID ("_ChannelMixerGreen");
        static readonly int _ChannelMixerBlue = Shader.PropertyToID ("_ChannelMixerBlue");
        static readonly int _Lift = Shader.PropertyToID ("_Lift");
        static readonly int _Gamma = Shader.PropertyToID ("_Gamma");
        static readonly int _Gain = Shader.PropertyToID ("_Gain");
        static readonly int _Shadows = Shader.PropertyToID ("_Shadows");
        static readonly int _Midtones = Shader.PropertyToID ("_Midtones");
        static readonly int _Highlights = Shader.PropertyToID ("_Highlights");
        static readonly int _ShaHiLimits = Shader.PropertyToID ("_ShaHiLimits");
        static readonly int _SplitShadows = Shader.PropertyToID ("_SplitShadows");
        static readonly int _SplitHighlights = Shader.PropertyToID ("_SplitHighlights");

        static readonly int _CustomToneCurve = Shader.PropertyToID ("_CustomToneCurve");
        static readonly int _ToeSegmentA = Shader.PropertyToID ("_ToeSegmentA");
        static readonly int _ToeSegmentB = Shader.PropertyToID ("_ToeSegmentB");
        static readonly int _MidSegmentA = Shader.PropertyToID ("_MidSegmentA");
        static readonly int _MidSegmentB = Shader.PropertyToID ("_MidSegmentB");
        static readonly int _ShoSegmentA = Shader.PropertyToID ("_ShoSegmentA");
        static readonly int _ShoSegmentB = Shader.PropertyToID ("_ShoSegmentB");
        static readonly int _CurveMaster = Shader.PropertyToID ("_CurveMaster");
        static readonly int _CurveRed = Shader.PropertyToID ("_CurveRed");
        static readonly int _CurveGreen = Shader.PropertyToID ("_CurveGreen");
        static readonly int _CurveBlue = Shader.PropertyToID ("_CurveBlue");
        static readonly int _CurveHueVsHue = Shader.PropertyToID ("_CurveHueVsHue");
        static readonly int _CurveHueVsSat = Shader.PropertyToID ("_CurveHueVsSat");
        static readonly int _CurveSatVsSat = Shader.PropertyToID ("_CurveSatVsSat");
        static readonly int _CurveLumVsSat = Shader.PropertyToID ("_CurveLumVsSat");

        readonly HableCurve m_HableCurve = new HableCurve ();
#if UNITY_EDITOR
        public override void BeginDump ()
        {
            base.BeginDump ();
            AddKeyName (_LutBake_Params, "_LutBake_Params");
            AddKeyName (_ColorBalance, "_ColorBalance");
            AddKeyName (_ColorFilter, "_ColorFilter");
            AddKeyName (_ChannelMixerRed, "_ChannelMixerRed");
            AddKeyName (_ChannelMixerGreen, "_ChannelMixerGreen");
            AddKeyName (_ChannelMixerBlue, "_ChannelMixerBlue");
            AddKeyName (_HueSatCon, "_HueSatCon");
            AddKeyName (_Lift, "_Lift");
            AddKeyName (_Gamma, "_Gamma");
            AddKeyName (_Gain, "_Gain");
            AddKeyName (_Shadows, "_Shadows");
            AddKeyName (_Midtones, "_Midtones");
            AddKeyName (_Highlights, "_Highlights");
            AddKeyName (_ShaHiLimits, "_ShaHiLimits");
            AddKeyName (_SplitShadows, "_SplitShadows");
            AddKeyName (_SplitHighlights, "_SplitHighlights");
            AddKeyName (_CustomToneCurve, "_CustomToneCurve");
            AddKeyName (_ToeSegmentA, "_ToeSegmentA");
            AddKeyName (_ToeSegmentB, "_ToeSegmentB");
            AddKeyName (_MidSegmentA, "_MidSegmentA");
            AddKeyName (_MidSegmentB, "_MidSegmentB");
            AddKeyName (_ShoSegmentA, "_ShoSegmentA");
            AddKeyName (_ShoSegmentB, "_ShoSegmentB");

            AddKeyName (_Lut2D, "_Lut2D");
            AddKeyName (_Lut_Params, "_Lut_Params");
        }
#endif
        public override void DirtySetting()
        {
            base.DirtySetting();
            EngineContext context = EngineContext.instance;
            context.renderflag.SetFlag(EngineContext.RFlag_PPDirty, true);
        }
        private void LutBake (RenderContext rc, PropertySheet lutSheet, bool aces)
        {
            CheckInternalStripLut ();
            // Prepare data
            var lmsColorBalance = ColorUtilities.ColorBalanceToLMSCoeffs (settings.whiteBalance.value.x, settings.whiteBalance.value.y);
            ref var colorAdjustments = ref settings.colorAdjustments.value;
            var hueSatCon = new Vector4 (
                colorAdjustments.z / 360f,
                colorAdjustments.w / 100f + 1f,
                settings.colorAdjustments.value.y / 100f + 1f,
                aces?1 : 0);
            ref var redInOut = ref settings.redInOut.value;
            ref var greenInOut = ref settings.greenInOut.value;
            ref var blueInOut = ref settings.blueInOut.value;
            var channelMixerR = new Vector4 (redInOut.x / 100f, redInOut.y / 100f, redInOut.z / 100f, 0f);
            var channelMixerG = new Vector4 (greenInOut.x / 100f, greenInOut.y / 100f, greenInOut.z / 100f, 0f);
            var channelMixerB = new Vector4 (blueInOut.x / 100f, blueInOut.y / 100f, blueInOut.z / 100f, 0f);

            //var shadowsHighlightsLimits = settings.shadowHightlights.value;

            var (shadows, midtones, highlights) = ColorUtilities.PrepareShadowsMidtonesHighlights (
                settings.shadows.value,
                settings.midtones.value,
                settings.highlights.value
            );

            var (lift, gamma, gain) = ColorUtilities.PrepareLiftGammaGain (
                settings.lift.value,
                settings.gamma.value,
                settings.gain.value
            );

            var (splitShadows, splitHighlights) = ColorUtilities.PrepareSplitToning (
                settings.shadowColor.value,
                settings.highlightColor.value,
                settings.whiteBalance.value.z
            );
            var tone0 = settings.customTone0.value;
            var tone1 = settings.customTone1.value;
            m_HableCurve.Init (
                tone0.x,
                tone0.y,
                tone0.z,
                tone0.w,
                tone1.x,
                tone1.y
            );

            int lutHeight = k_Lut2DSize;
            int lutWidth = k_Lut2DSize * k_Lut2DSize;
            var lutParameters = new Vector4(lutHeight, 0.5f / lutWidth, 0.5f / lutHeight, lutHeight / (lutHeight - 1f));

            // Fill in constants
            SetShaderValue (_LutBake_Params, lutSheet.properties, ref lutParameters);
            SetShaderValue (_ColorBalance, lutSheet.properties, ref lmsColorBalance);
            SetShaderValue (_ColorFilter, lutSheet.properties, settings.colorFilter.value.linear);
            SetShaderValue (_ChannelMixerRed, lutSheet.properties, ref channelMixerR);
            SetShaderValue (_ChannelMixerGreen, lutSheet.properties, ref channelMixerG);
            SetShaderValue (_ChannelMixerBlue, lutSheet.properties, ref channelMixerB);
            SetShaderValue (_HueSatCon, lutSheet.properties, ref hueSatCon);
            SetShaderValue (_Lift, lutSheet.properties, ref lift);
            SetShaderValue (_Gamma, lutSheet.properties, ref gamma);
            SetShaderValue (_Gain, lutSheet.properties, ref gain);
            SetShaderValue (_Shadows, lutSheet.properties, ref shadows);
            SetShaderValue (_Midtones, lutSheet.properties, ref midtones);
            SetShaderValue (_Highlights, lutSheet.properties, ref highlights);
            SetShaderValue (_ShaHiLimits, lutSheet.properties, ref settings.shadowHightlights.value);
            SetShaderValue (_SplitShadows, lutSheet.properties, ref splitShadows);
            SetShaderValue (_SplitHighlights, lutSheet.properties, ref splitHighlights);

            // YRGB curves
            lutSheet.properties.SetTexture (_CurveMaster, settings.master.GetTexture (rc, 0));
            lutSheet.properties.SetTexture (_CurveRed, settings.red.GetTexture (rc, 1));
            lutSheet.properties.SetTexture (_CurveGreen, settings.green.GetTexture (rc, 2));
            lutSheet.properties.SetTexture (_CurveBlue, settings.blue.GetTexture (rc, 3));

            // Secondary curves
            lutSheet.properties.SetTexture (_CurveHueVsHue, settings.hueVsHue.GetTexture (rc, 4));
            lutSheet.properties.SetTexture (_CurveHueVsSat, settings.hueVsSat.GetTexture (rc, 5));
            lutSheet.properties.SetTexture (_CurveSatVsSat, settings.satVsSat.GetTexture (rc, 6));
            lutSheet.properties.SetTexture (_CurveLumVsSat, settings.lumVsSat.GetTexture (rc, 7));

            // Tonemapping (baked into the lut for HDR)
            rc.preWorkingCmd.BlitFullscreenTriangle (null, ref lutTex, lutSheet, 0);
            // rc.preWorkingCmd.DebugCmd();
        }

        private void UnityCustomTonemapping (RenderContext rc, PropertySheet lutSheet)
        {
            var tone0 = settings.customTone0.value;
            var tone1 = settings.customTone1.value;
            m_HableCurve.Init (
                tone0.x,
                tone0.y,
                tone0.z,
                tone0.w,
                tone1.x,
                tone1.y
            );

            Shader.SetGlobalVector(_CustomToneCurve, m_HableCurve.uniforms.curve);
            Shader.SetGlobalVector(_ToeSegmentA, m_HableCurve.uniforms.toeSegmentA);
            Shader.SetGlobalVector(_ToeSegmentB, m_HableCurve.uniforms.toeSegmentB);
            Shader.SetGlobalVector(_MidSegmentA, m_HableCurve.uniforms.midSegmentA);
            Shader.SetGlobalVector(_MidSegmentB, m_HableCurve.uniforms.midSegmentB);
            Shader.SetGlobalVector(_ShoSegmentA, m_HableCurve.uniforms.shoSegmentA);
            Shader.SetGlobalVector(_ShoSegmentB, m_HableCurve.uniforms.shoSegmentB);
        }
        public override void Update (EngineContext context, IRenderContext renderContext)
        {
            var rc = renderContext as RenderContext;
            if (BeginUpdate ())
            {
                bool customLut = settings.gradingMode == (int)GradingMode.CustomLut;
                bool aces = settings.tonemapper == (int)TonemapperMode.UnityACES;

                var lutSheet = rc.propertySheets.Get (rc.resources.shaders.lutBuilderHdr);

                if (!aces)
                {
                    UnityCustomTonemapping (rc, lutSheet);
                }

                if (!customLut)
                {
                    LutBake (rc, lutSheet, aces);
                }

                EndUpdate ();
            }
        }

        public override void Render (EngineContext context, IRenderContext renderContext)
        {
            var rc = renderContext as RenderContext;

            if (settings.gradingMode ==(int) GradingMode.CustomLut)
            {
                Texture2D lut = settings.customLut.res as Texture2D;
                if (lut)
                {
                    float size = lut.height;
                    Vector4 customLutParam = new Vector4(
                        1f / lut.width,
                        1f / lut.height,
                        lut.height - 1f,
                        settings.customLutParam.value.x
                    );
                    Shader.SetGlobalVector(_CustomLut_Params, customLutParam);
                    Shader.SetGlobalTexture(_CustomLut2D, lut);
                    rc.stateFlag.SetFlag(RenderContext.SFlag_ColorgradingEnable, true);
                    rc.stateFlag.SetFlag(RenderContext.SFlag_CustomLutEnable, true);
                }
            }
            else if (settings.gradingMode == (int)GradingMode.Realtime)
            {
                var uberSheet = rc.uberSheet;
#if UNITY_EDITOR
                if (RenderContext.capturing && ColorGrading.breakLUT)
                {
                    uberSheet.properties.SetVector (_Lut_Params, Vector4.zero);
                    return;
                }
    #endif
                if (internal2DLut != null)
                {
                    // ref var v = ref settings.whiteBalance.value;
                    // if (v.w < 0.5f)
                    {
                        float postExposureLinear = Mathf.Pow (2f, settings.colorAdjustments.value.x);
                        SetShaderValue (_Lut2D, uberSheet.properties, internal2DLut);
                        SetShaderValue (_Lut_Params, uberSheet.properties, new Vector4 (
                            1f / (k_Lut2DSize * k_Lut2DSize),
                            1f / k_Lut2DSize,
                            k_Lut2DSize - 1f,
                            postExposureLinear));
                        rc.stateFlag.SetFlag (RenderContext.SFlag_ColorgradingEnable, true);
                        rc.stateFlag.SetFlag(RenderContext.SFlag_CustomLutEnable, false);
                    }
                    // else
                    // {
                    //     rc.stateFlag.SetFlag (RenderContext.SFlag_ColorgradingEnable, false);
                    // }
                }
            }
        }

        public static RenderTextureFormat GetLutFormat ()
        {
            // Use ARGBHalf if possible, fallback on ARGB2101010 and ARGB32 otherwise
            var format = RenderTextureFormat.ARGBHalf;

            if (!format.IsSupported ())
            {
                format = RenderTextureFormat.ARGB2101010;

                // Note that using a log lut in ARGB32 is a *very* bad idea but we need it for
                // compatibility reasons (else if a platform doesn't support one of the previous
                // format it'll output a black screen, or worse will segfault on the user).
                if (!format.IsSupported ())
                    format = RenderTextureFormat.ARGB32;
            }

            return format;
        }

        private RenderTexture CheckInternalStripLut ()
        {
            // Check internal lut state, (re)create it if needed
            if (internal2DLut == null || !internal2DLut.IsCreated ())
            {
                EngineUtility.Destroy (internal2DLut);

                var format = GetLutFormat ();
                internal2DLut = new RenderTexture (k_Lut2DSize * k_Lut2DSize, k_Lut2DSize, 0, format, RenderTextureReadWrite.Linear)
                {
                    name = "Color Grading Strip Lut",
                    hideFlags = HideFlags.DontSave,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp,
                    anisoLevel = 0,
                    autoGenerateMips = false,
                    useMipMap = false
                };
                internal2DLut.Create ();
                lutTex = internal2DLut;
            }
            return internal2DLut;
        }

        public override void Release (EngineContext context, IRenderContext renderContext)
        {
            if (internal2DLut != null)
            {
                EngineUtility.Destroy (internal2DLut);
                internal2DLut = null;
            }
            var rc = renderContext as RenderContext;
            settings.master.ResetTex (rc, 0, TextureCurveParam.CurveTyp_Linear01);
            settings.red.ResetTex (rc, 1, TextureCurveParam.CurveTyp_Linear01);
            settings.green.ResetTex (rc, 2, TextureCurveParam.CurveTyp_Linear01);
            settings.blue.ResetTex (rc, 3, TextureCurveParam.CurveTyp_Linear01);
            settings.hueVsHue.ResetTex (rc, 4, TextureCurveParam.CurveTyp_ConstHalf);
            settings.hueVsSat.ResetTex (rc, 5, TextureCurveParam.CurveTyp_ConstHalf);
            settings.satVsSat.ResetTex (rc, 6, TextureCurveParam.CurveTyp_ConstHalf);
            settings.lumVsSat.ResetTex (rc, 7, TextureCurveParam.CurveTyp_ConstHalf);
            base.Release (context, renderContext);

        }
    }
}