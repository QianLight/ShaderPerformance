// using System;
// using UnityEngine;
// using UnityEngine.Rendering;
// namespace CFEngine
// {

//     public enum WeatherKind
//     {
//         FineDay = 0,
//         RainDay = 1,
//     }

//     [Serializable]
//     public sealed class WeatherKindParameter : ParamOverride<WeatherKind> { }

//     [Serializable]
//     [PostProcess(typeof(WeatherRenderer), "Unity/Weather")]
//     public sealed class Weather : EnvSetting
//     {

//         [CFDisplayName("Kind"), CFTooltip("Select a Weather Type.")]
//         public WeatherKindParameter KindOfWeather = new WeatherKindParameter { value = WeatherKind.FineDay };

//         [CFDisplayName("DayNight"), CFRange(-1f, 1f), CFTooltip("")]
//         public FloatParam DayNight = new FloatParam { value = 0f };

//         [CFDisplayName("RainValue"), CFRange(-1f, 1f), CFTooltip("")]
//         public FloatParam RainValue = new FloatParam { value = 0f };

//         [CFTooltip("Weather Effect")]
//         public BoolParam HasWeatherEffect = new BoolParam { value = false };

//         [CFTooltip("Thunder Effect")]
//         public BoolParam HasThunder = new BoolParam { value = false };
//         [CFTooltip("Rainbow Effect")]
//         public BoolParam HasRainbow = new BoolParam { value = false };

//         public override bool IsEnabledAndSupported(RenderContext context, bool isInSceneView)
//         {
//             return enabled.value;
//         }

//         public override void SetEnable(bool enable)
//         {
//             base.SetEnable(enable);
//             if (!enable)
//             {
//                 RuntimeUtilities.EnableKeyword(ShaderIDs.Weather_ThunderKeyWord, false);
//                 RuntimeUtilities.EnableKeyword(ShaderIDs.Weather_RainbowKeyWord, false);
//                 RuntimeUtilities.EnableKeyword(ShaderIDs.Weather_RainEffectKeyWord, false);
//                 RuntimeUtilities.EnableKeyword(ShaderIDs.Weather_StarKeyWord, false);
//             }
//         }

//         public override void InitParamaters()
//         {
//             effectType = (int)Effects.EWeather;
//             parameters = new ParamOverride[7];
//             parameters[0] = enabled;
//             parameters[1] = KindOfWeather;
//             parameters[2] = DayNight;
//             parameters[3] = RainValue;
//             parameters[4] = HasWeatherEffect;
//             parameters[5] = HasThunder;
//             parameters[6] = HasRainbow;
//         }
//         public static EnvModify Creator()
//         {
//             return new WeatherRenderer();
//         }
//     }
//     public sealed class WeatherRenderer : EnvModify<Weather>, IPreEffect
//     {

//         public override void Render(RenderContext context)
//         {

//         }

//         public void Render(RenderContext context, PropertySheet sheet)
//         {

//             PreEffect.param.x = 1;
//             // RuntimeUtilities.EnableKeyword(ShaderIDs.Weather_WeatherKeyWord, settings.HasWeatherEffect);
//             if (settings.KindOfWeather == WeatherKind.RainDay)
//             {

//                 RuntimeUtilities.EnableKeyword(ShaderIDs.Weather_ThunderKeyWord, settings.HasThunder);
//                 RuntimeUtilities.EnableKeyword(ShaderIDs.Weather_ThunderKeyWord, settings.HasThunder);
//                 RuntimeUtilities.EnableKeyword(ShaderIDs.Weather_RainbowKeyWord, settings.HasRainbow);
//                 RuntimeUtilities.EnableKeyword(ShaderIDs.Weather_RainEffectKeyWord, true);
//                 RuntimeUtilities.EnableKeyword(ShaderIDs.Weather_StarKeyWord, false);
//                 sheet.EnableKeyword(ShaderIDs.Weather_RainDay);
//                 Shader.SetGlobalFloat(ShaderIDs.Weather_RainFact, settings.RainValue);
//                 sheet.properties.SetVector(ShaderIDs.Weather_ViewDirection, context.camera.transform.forward);

//             }
//             else if (settings.KindOfWeather == WeatherKind.FineDay)
//             {
//                 RuntimeUtilities.EnableKeyword(ShaderIDs.Weather_ThunderKeyWord, false);
//                 RuntimeUtilities.EnableKeyword(ShaderIDs.Weather_RainbowKeyWord, false);
//                 RuntimeUtilities.EnableKeyword(ShaderIDs.Weather_RainEffectKeyWord, false);
//                 RuntimeUtilities.EnableKeyword(ShaderIDs.Weather_StarKeyWord, true);
//                 sheet.DisableKeyword(ShaderIDs.Weather_RainDay);
//                 Shader.SetGlobalFloat(ShaderIDs.Weather_RainFact, settings.DayNight);
//             }
//         }
//     }
// }