using CFEngine;
using ClientEcsData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class GlobalShaderEffects
{
    public static bool IsChangeSceneColor;
    
    public static void Update()
    {
        if (!initialized)
        {
            SkillHelper.onOverride += OnOverride;
            SkillHelper.onRecover += OnRecover;
            SkillHelper.onOverride += OverrideSkillHelper;
            SkillHelper.onRecover += RecoverSkillHelper;
            VolumeManager.instance.onOverrideFinish += ApplyVolume;
            initialized = true;
        }

        Shader.SetGlobalColor(ShaderManager._SceneColor, sceneColor);
    }

    private static void OnOverride(EnvEffectType type, IGetEnvValue getEnvValue)
    {
        if (type == EnvEffectType.SceneColor)
        {
            IsChangeSceneColor = true;
            ApplySceneColor(getEnvValue);
        }
        else if (type == EnvEffectType.Vignette)
        {
            ApplyVignette(getEnvValue);
        }
    }


    private static void OnRecover(EnvEffectType type)
    {
        if (type == EnvEffectType.SceneColor)
        {
            IsChangeSceneColor = false;
            RecoverSceneColor();
        }
        else if (type == EnvEffectType.Vignette)
        {
            RecoverVignette();
        }
    }

    private static void ApplyVolume(VolumeStack stack)
    {
        ApplyVignetteToVolume(stack);
    }

    #region RadialBlur

    private static int skillHelperID = -1;

    private static void OverrideSkillHelper(EnvEffectType type, IGetEnvValue getEnvValue)
    {
        if (type != EnvEffectType.RadialBlur)
            return;

        RadialBlurParam value = RadialBlurParam.GetDefualtValue();

        Vector3 position;
        float x = getEnvValue.GetValue(EnvSetting.EnvEffect_RadialBlur_V2_ScreenSpaceCenterX);
        float y = getEnvValue.GetValue(EnvSetting.EnvEffect_RadialBlur_V2_ScreenSpaceCenterY);
        float z = getEnvValue.GetValue(EnvSetting.EnvEffect_RadialBlur_V2_ScreenSpaceCenterZ);
        if (getEnvValue.GetValue(EnvSetting.EnvEffect_RadialBlur_V2_UseScreenSpace) > 0)
        {
            position = new Vector3(x, y, 10);
            value.useScreenPos = true;
        }
        else if (getEnvValue.GetPos(out position))
        {
            value.useScreenPos = false;
            position += new Vector3(x, y, z);
        }
        else
        {
            return;
        }

        value.center = position;
        value.size = getEnvValue.GetValue(EnvSetting.EnvEffect_RadialBlur_V2_Scale);
        value.innerRadius = getEnvValue.GetValue(EnvSetting.EnvEffect_RadialBlur_V2_InnerRadius);
        value.innerFadeOut = getEnvValue.GetValue(EnvSetting.EnvEffect_RadialBlur_V2_InnerFadeout);
        value.outerRadius = getEnvValue.GetValue(EnvSetting.EnvEffect_RadialBlur_V2_OuterRadius);
        value.outerFadeOut = getEnvValue.GetValue(EnvSetting.EnvEffect_RadialBlur_V2_OuterFadeout);
        value.intensity = getEnvValue.GetValue(EnvSetting.EnvEffect_RadialBlur_V2_Intensity);
        value.active = true;

        if (skillHelperID < 0)
            skillHelperID = URPRadialBlur.instance.AddParam(value, URPRadialBlurSource.SkillHelper, 0);
        else
            URPRadialBlur.instance.ModifyParam(skillHelperID, value);
    }

    private static void RecoverSkillHelper(EnvEffectType type)
    {
        if (type != EnvEffectType.RadialBlur)
            return;

        if (skillHelperID >= 0)
        {
            if (!URPRadialBlur.instance.RemoveParam(skillHelperID))
            {
                Debug.LogError(
                    "URPRadialBlur.RecoverSkillHelper: Try to recover setting that never override or already removed.");
            }

            skillHelperID = -1;
        }
    }

    #endregion

    #region Vignette

    private static bool vignetteOverride;
    private static float vignetteIntensity;
    private static float vignetteSmoothness;
    private static bool vignetteRounded;
    private static float vignetteRoundness;

    private static void ApplyVignette(IGetEnvValue getEnvValue)
    {
        vignetteOverride = true;
        vignetteIntensity = getEnvValue.GetValue(EnvSetting.EnvEffect_Vignette_Intensity);
        vignetteSmoothness = getEnvValue.GetValue(EnvSetting.EnvEffect_Vignette_Smoothness);
        vignetteRoundness = getEnvValue.GetValue(EnvSetting.EnvEffect_Vignette_Roundness);
        vignetteRounded = getEnvValue.GetValue(EnvSetting.EnvEffect_Vignette_Rounded) > 0.5f;
    }

    private static void RecoverVignette()
    {
        vignetteOverride = false;
    }
    
    private static void ApplyVignetteToVolume(VolumeStack stack)
    {
        if (vignetteOverride)
        {
            var vignette = stack.GetComponent<Vignette>();
            vignette.intensity.value = vignetteIntensity;
            vignette.smoothness.value = vignetteSmoothness;
            vignette.rounded.value = vignetteRounded;
            vignette.roundness.value = vignetteRoundness;
            vignette.active = true;
        }
    }

    #endregion

    #region SceneColor

    private static Color sceneColor = Color.white;
    private static bool initialized;

    private static void ApplySceneColor(IGetEnvValue getEnvValue)
    {
        sceneColor.r = getEnvValue.GetValue(EnvSetting.EnvEffect_ColorR);
        sceneColor.g = getEnvValue.GetValue(EnvSetting.EnvEffect_ColorG);
        sceneColor.b = getEnvValue.GetValue(EnvSetting.EnvEffect_ColorB);
        sceneColor.a = getEnvValue.GetValue(EnvSetting.EnvEffect_ColorA);
    }

    private static void RecoverSceneColor()
    {
        sceneColor = Color.white;
    }

    #endregion
}