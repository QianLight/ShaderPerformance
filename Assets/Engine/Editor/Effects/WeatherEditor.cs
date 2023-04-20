using UnityEditor;

namespace CFEngine.Editor
{
    // [EnvEditor(typeof(Weather))]
    // public sealed class WeatherEditor : EnvEffectEditor<Weather>
    // {
    //     SerializedParameterOverride KindOfWeather;
    //     SerializedParameterOverride DayNight;
    //     SerializedParameterOverride RainValue;
    //     SerializedParameterOverride HasWeatherEffect;
    //     SerializedParameterOverride HasThunder;
    //     SerializedParameterOverride HasRainbow;

    //     public override void OnEnable()
    //     {
    //         KindOfWeather = FindParameterOverride(x => x.KindOfWeather);
    //         DayNight = FindParameterOverride(x => x.DayNight);
    //         RainValue = FindParameterOverride(x => x.RainValue);
    //         HasWeatherEffect = FindParameterOverride(x => x.HasWeatherEffect);
    //         HasThunder = FindParameterOverride(x => x.HasThunder);
    //         HasRainbow = FindParameterOverride(x => x.HasRainbow);
    //     }

    //     public override void OnInspectorGUI()
    //     {
    //         EditorUtilities.DrawHeaderLabel("Weather");
            
    //         PropertyField(KindOfWeather);
    //         PropertyField(DayNight);
    //         PropertyField(RainValue);
    //         PropertyField(HasWeatherEffect);
    //         PropertyField(HasThunder);
    //         PropertyField(HasRainbow);
    //     }
    //}
}
