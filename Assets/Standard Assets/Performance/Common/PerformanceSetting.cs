using System.Collections;
using CFClient;
using CFEngine;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using ShadowQuality = UnityEngine.ShadowQuality;
using ShadowResolution = UnityEngine.ShadowResolution;

public class PerformanceSetting
{
    private UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urpAsset;
    private static bool isTheEmulator = false;
    public PerformanceSetting()
    {
        urpAsset = UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset as UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset;
        RenderLevelManager.onImposterLevelChanged += new System.Action<RenderQualityLevel>(SetImpostLevelAction);
        //RenderLevelManager.onResolutionLevelChanged += OnResolutionLevelChanged;
    }
    public static float GetResolutionScale(UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urpAsset, string platform)
    {
        //if(platform.Contains("Editor"))
        //    return urpAsset.renderScale;
        //else
        //    return Screen.height / PerformanceSetting.DefaultResolution.y;
        return lastResolutionScale;
    }
    private static float lastResolutionScale = 0;
    private static int defaultWidth = 0, defaultHeight = 0;
    public static void SetResolution(UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset urpAsset, float realScale)
    {
        if (defaultWidth == 0)
        {
            defaultWidth = Screen.width;
            defaultHeight = Screen.height;
        }

        int width = 100;
        int height = 100;
        if (defaultHeight >= MaxHeight)
        {
            lastResolutionScale = (MaxHeight * realScale) / defaultHeight;

            width = (int)(defaultWidth * lastResolutionScale);
            height = (int)(defaultHeight * lastResolutionScale);
        }
        else
        {
            //模拟器高度达不到1080
            //手机的极速模式开启只有720P
            if (isTheEmulator)
            {
                lastResolutionScale = 1;
                width = defaultWidth;
                height = defaultHeight;
            }
            else
            {
                lastResolutionScale = realScale;
                width = (int)(defaultWidth * lastResolutionScale);
                height = (int)(defaultHeight * lastResolutionScale);
            }
        }
#if UNITY_EDITOR
        urpAsset.renderScale = realScale;
#else
        urpAsset.renderScale = 1;
        if (Screen.height != height || Screen.width != width)
        {
            Screen.SetResolution(width, height, true);
        }
        Debug.LogWarning("SetResolution to " + width + "," + height + "," + realScale + "," + lastResolutionScale);
#endif

    }

    //private void OnResolutionLevelChanged(RenderQualityLevel level)
    //{
    //float hardScale = Screen.height > MaxHeight ? MaxHeight / Screen.height : 1f;
    //float realScale = hardScale * RenderLevelManager.GetCurrentResolutionScale();
    //if (DefaultResolution.y == 0)
    //{
    //    DefaultResolution.x = Screen.width;
    //    DefaultResolution.y = Screen.height;
    //}
    //SetResolution(urpAsset, realScale);


    //}

    public IEnumerator SetConfEnumerator(SettingInfo info)
    {
        SetResolutionLevel(info.Level);
        yield return new WaitForSeconds(0.1f);

        SetMatLevel(info.MatLevel);
        SetShadowLevel(info.ShadowLevel);
        SetSFXLevel(info.SFXLevel);
        SetTextureLevel(info.TextureLevel);
        SetAfterEffectLevel(info.AfterEffectLevel);
        SetAAEnable(info.AAEnable);
        //SetTessEnable(info.TessEnable);
        SetRoleShader();
    }

    public void SetConf(SettingInfo info)
    {
        SetResolutionLevel(info.Level);
        SetMatLevel(info.MatLevel);
        SetShadowLevel(info.ShadowLevel);
        SetSFXLevel(info.SFXLevel);
        SetTextureLevel(info.TextureLevel);
        SetAfterEffectLevel(info.AfterEffectLevel);
        SetAAEnable(info.AAEnable);
        //SetTessEnable(info.TessEnable);
        SetRoleShader();
    }
    void SetRoleShader()
    {
        EntityExtSystem.SetRoleFeaturesConfig(EngineContext.instance);
        EntityExtSystem.ResetRoleMaterialSituation();
    }
    public void ifEmulator(bool isEmulator)
    {
        isTheEmulator = isEmulator;
    }

    public SettingInfo GetDefault(RenderQualityLevel level)
    {
        SettingInfo result = new SettingInfo();
        result.Level = level;
        result.MatLevel = level;
        result.ShadowLevel = level;
        result.SFXLevel = level;
        result.TextureLevel = level;
        result.AfterEffectLevel = level;
        result.AAEnable = level;
        //result.TessEnable = level;
        return result;
    }

    public const float MaxHeight = 1080;

    void SetResolutionLevel(RenderQualityLevel level)
    {
        GameQualitySetting.ResolutionLevel = level;
        //Debug.LogWarning("SetResolutionLevel.Level:" + level);
        SetResolutionLevelAction(level);
        RenderLevelManager.SetRoleRenderLevel(level);
    }
    void SetScreen()
    {

        /*
         * 
        AndroidJavaClass jc = new AndroidJavaClass(“com.unity3d.player.UnityPlayer”);
        var java = jc.GetStatic(“currentActivity”);
        java.Call(“SetConfiguration”, scaleWidth, scaleHeight); // 缩放宽高，自己计算
        Screen.SetResolution(scaleWidth, scaleHeight, true, 30);

        android代码
public void SetConfiguration(int fW, int fH)
        {
            requestWindowFeature(Window.FEATURE_NO_TITLE);

            getWindow().takeSurface(null);
            setTheme(android.R.style.Theme_NoTitleBar_Fullscreen);
            getWindow().setFormat(PixelFormat.RGB_565);

            mUnityPlayer = new UnityPlayer(this);
            if (mUnityPlayer.getSettings().getBoolean(“hide_status_bar”, true))
                getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN,
                WindowManager.LayoutParams.FLAG_FULLSCREEN);

            setContentView(mUnityPlayer);
            mUnityPlayer.requestFocus();

            Configuration config = getResources().getConfiguration();

            //在这里强制设置一下屏幕的宽和高
            mUnityPlayer.getView().getLayoutParams().width = fW;
            mUnityPlayer.getView().getLayoutParams().height = fH;
            mUnityPlayer.configurationChanged(config);
            //ShowToast(“width=” + width + “, height=” + height);
        }
        */
    }
    public void TimelineEnter()
    {
        QualitySettings.skinWeights = SkinWeights.FourBones;
    }
    public void TimelineLeave()
    {
        if (GameQualitySetting.MatLevel <= RenderQualityLevel.Medium)
        {
            QualitySettings.skinWeights = SkinWeights.TwoBones;
        }
    }

    void SetImpostLevelAction(RenderQualityLevel level)
    {
        QualitySettings.lodBias = RenderLevelManager.GetCurrentImposterScale();
    }

    void SetResolutionLevelAction(RenderQualityLevel level)
    {
        urpAsset.roleScreenSpaceRim = level == RenderQualityLevel.High || level == RenderQualityLevel.Ultra;//Add by:Takeshi
        //RenderLevelManager.SetResolutionLevel(level);
        Debug.LogWarning("SetResolutionLevelAction.Level:" + level);
        float scale = 1;
        switch (level)
        {
            case RenderQualityLevel.VeryLow:
                {
                    //540
                    scale = 0.5f;
                    if (urpAsset.supportsHDR)
                    {
                        urpAsset.supportsHDR = false;
                    }
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                    break;
                }
            case RenderQualityLevel.Low:
                {
                    //720
                    scale = 0.66667f;
                    if (urpAsset.supportsHDR)
                    {
                        urpAsset.supportsHDR = false;
                    }
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                    break;
                }
            case RenderQualityLevel.Medium:
                {
                    //756
                    scale = 0.66667f;
                    if (!urpAsset.supportsHDR)
                    {
                        urpAsset.supportsHDR = true;
                    }
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                    break;
                }
            case RenderQualityLevel.High:
                {
                    //972
                    scale = 0.9f;
                    if (!urpAsset.supportsHDR)
                    {
                        urpAsset.supportsHDR = true;
                    }
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
                    break;
                }
            case RenderQualityLevel.Ultra:
                {
                    scale = 1.0f;
                    if (!urpAsset.supportsHDR)
                    {
                        urpAsset.supportsHDR = true;
                    }
                    QualitySettings.anisotropicFiltering = AnisotropicFiltering.Enable;
                    break;
                }
        }
        SetResolution(urpAsset, scale);
    }

    public void EnterUI()
    {
        RenderQualityLevel level = GameQualitySetting.ResolutionLevel + 1;
        if (level > RenderQualityLevel.Ultra)
        {
            level = RenderQualityLevel.Ultra;
        }
        SetResolutionLevelAction(level);
    }

    public void EnterWar()
    {
        // 场合需求改成RenderLevelManager了。
        //SetResolutionLevel(GameQualitySetting.ResolutionLevel);
        RenderQualityLevel level = GameQualitySetting.ResolutionLevel;
        SetResolutionLevelAction(level);
    }

    void SetMatLevel(RenderQualityLevel level)
    {
        Debug.LogWarning("SetMatLevel:" + level);
        GameQualitySetting.MatLevel = level;
        Shader.DisableKeyword("_SHADER_LEVEL_VERY_HIGH");
        Shader.DisableKeyword("_SHADER_LEVEL_HIGH");
        Shader.DisableKeyword("_SHADER_LEVEL_MEDIUM");
        Shader.DisableKeyword("_SHADER_LEVEL_LOW");
        Shader.DisableKeyword("_SHADER_LEVEL_VERY_LOW");
        //Shader.EnableKeyword("_MAIN_LIGHT_SHADOWS");
        
        if (level == RenderQualityLevel.VeryLow)
        {
            QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
            urpAsset.supportsMainLightShadows = false;
            urpAsset.supportsAdditionalLightShadows = false;
            urpAsset.maxAdditionalLightsCount = 0;
            //Shader.DisableKeyword("_MAIN_LIGHT_SHADOWS");
            Shader.EnableKeyword("_SHADER_LEVEL_VERY_LOW");
            QualitySettings.skinWeights = SkinWeights.TwoBones;
        }
        else if (level == RenderQualityLevel.Low)
        {
            QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
            urpAsset.supportsMainLightShadows = true;
            urpAsset.shadowCascadeCount = 1;
            urpAsset.supportsAdditionalLightShadows = false;
            urpAsset.maxAdditionalLightsCount = 0;
            Shader.EnableKeyword("_SHADER_LEVEL_LOW");
            QualitySettings.skinWeights = SkinWeights.TwoBones;
        }
        else if (level == RenderQualityLevel.Medium)
        {
            QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
            urpAsset.supportsMainLightShadows = true;
            urpAsset.shadowCascadeCount = 1;
            urpAsset.supportsAdditionalLightShadows = false;
            urpAsset.maxAdditionalLightsCount = 0;
            Shader.EnableKeyword("_SHADER_LEVEL_MEDIUM");
            QualitySettings.skinWeights = SkinWeights.TwoBones;
        }
        else if (level == RenderQualityLevel.High)
        {
            QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
            urpAsset.supportsMainLightShadows = true;
            urpAsset.shadowCascadeCount = 1;
            urpAsset.supportsAdditionalLightShadows = false;
            urpAsset.maxAdditionalLightsCount = 0;
            Shader.EnableKeyword("_SHADER_LEVEL_MEDIUM");
            QualitySettings.skinWeights = SkinWeights.FourBones;
        }
        else
        {
            //为了FPS60
//#if UNITY_EDITOR
//            QualitySettings.shadowmaskMode = ShadowmaskMode.DistanceShadowmask;
//            Shader.EnableKeyword("_SHADER_LEVEL_VERY_HIGH");
//#else
            QualitySettings.shadowmaskMode = ShadowmaskMode.Shadowmask;
            Shader.EnableKeyword("_SHADER_LEVEL_HIGH");
//#endif
            urpAsset.supportsMainLightShadows = true;
            urpAsset.shadowCascadeCount = 1;
            urpAsset.supportsAdditionalLightShadows = true;
            urpAsset.maxAdditionalLightsCount = 3;
            QualitySettings.skinWeights = SkinWeights.FourBones;
        }
        GameQualitySetting.SetMixFuncKeyword(GameQualitySetting.TerrainFeature);
        RenderLevelManager.SetImposterRenderLevel(level);
    }

    void SetShadowLevel(RenderQualityLevel level)
    {
        GameQualitySetting.ShadowLevel = level;
        if (level == RenderQualityLevel.VeryLow)
        {
            urpAsset.mainLightShadowmapResolution = 256;
        }
        else if (level == RenderQualityLevel.Low)
        {
            urpAsset.mainLightShadowmapResolution = 512;
        }
        else if (level == RenderQualityLevel.Medium)
        {
            urpAsset.mainLightShadowmapResolution = 1024;
        }
        else if (level == RenderQualityLevel.High)
        {
            urpAsset.mainLightShadowmapResolution = 2048;
        }
        else
        {
            urpAsset.mainLightShadowmapResolution = 2048;
        }
    }

    void SetSFXLevel(RenderQualityLevel level)
    {
        urpAsset.RequireCopyDepth = level == RenderQualityLevel.High || level == RenderQualityLevel.Ultra;
        Debug.LogWarning("SetSFXLevel:" + level);
        GameQualitySetting.SFXLevel = level;
        switch (GameQualitySetting.SFXLevel)
        {
            case RenderQualityLevel.VeryLow:
                Shader.EnableKeyword("_FX_LEVEL_LOW");
                Shader.DisableKeyword("_FX_LEVEL_MEDIUM");
                Shader.DisableKeyword("_FX_LEVEL_HIGH");
                SFXMgr.performanceLevel = 3;
                break;
            case RenderQualityLevel.Low:
                Shader.EnableKeyword("_FX_LEVEL_LOW");
                Shader.DisableKeyword("_FX_LEVEL_MEDIUM");
                Shader.DisableKeyword("_FX_LEVEL_HIGH");
                SFXMgr.performanceLevel = 2;
                break;
            case RenderQualityLevel.Medium:
                Shader.DisableKeyword("_FX_LEVEL_LOW");
                Shader.EnableKeyword("_FX_LEVEL_MEDIUM");
                Shader.DisableKeyword("_FX_LEVEL_HIGH");
                SFXMgr.performanceLevel = 1;
                break;
            case RenderQualityLevel.High:
                Shader.DisableKeyword("_FX_LEVEL_LOW");
                Shader.DisableKeyword("_FX_LEVEL_MEDIUM");
                Shader.EnableKeyword("_FX_LEVEL_HIGH");
                SFXMgr.performanceLevel = 0;
                break;
            case RenderQualityLevel.Ultra:
                Shader.DisableKeyword("_FX_LEVEL_LOW");
                Shader.DisableKeyword("_FX_LEVEL_MEDIUM");
                Shader.EnableKeyword("_FX_LEVEL_HIGH");
                SFXMgr.performanceLevel = 0;
                break;
        }
        if (SceneSFXManager.SetSceneSfxByLevel != null) SceneSFXManager.SetSceneSfxByLevel(SFXMgr.performanceLevel);
        RenderLevelManager.SetSfxRenderLevel(level);
    }

    void SetTextureLevel(RenderQualityLevel level)
    {
        QualitySettings.globalTextureMipmapLimit = level <= RenderQualityLevel.Medium ? 1 : 0;
    }

    void SetAfterEffectLevel(RenderQualityLevel level)
    {
        QualitySettings.softParticles = false;
        Debug.LogWarning("SetAfterEffectLevel:" + level);
        UnityEngine.Rendering.VolumeManager.instance.ChangeColorGradingLUT(true, true);
        RenderLevelManager.SetPostProcessRenderLevel(level);
        switch (RenderLevelManager.PostProcessRenderLevel)
        {
            case RenderQualityLevel.VeryLow:
                GameQualitySetting.Depth = false;
                GameQualitySetting.Bloom = false;
                GameQualitySetting.DepthOfField = false;
                GameQualitySetting.RadialBlur = false;
                GameQualitySetting.LensFlares = false;
                GameQualitySetting.ToneMapping = false;
                GameQualitySetting.DrawGrass = false;
                GameQualitySetting.GodRay = false;
                GameQualitySetting.UniversalForwardHigh = false;
                GameQualitySetting.UniversalForwardLow = true;
                break;
            case RenderQualityLevel.Low:
                GameQualitySetting.Depth = false;
                GameQualitySetting.Bloom = false;
                GameQualitySetting.DepthOfField = false;
                GameQualitySetting.RadialBlur = false;
                GameQualitySetting.LensFlares = false;
                GameQualitySetting.ToneMapping = true;
                GameQualitySetting.DrawGrass = true;
                GameQualitySetting.GodRay = false;
                GameQualitySetting.UniversalForwardHigh = false;
                GameQualitySetting.UniversalForwardLow = false;
                break;
            case RenderQualityLevel.Medium:
                GameQualitySetting.Depth = false;
                GameQualitySetting.Bloom = true;
                GameQualitySetting.DepthOfField = false;
                GameQualitySetting.RadialBlur = false;
                GameQualitySetting.LensFlares = false;
                GameQualitySetting.ToneMapping = true;
                GameQualitySetting.DrawGrass = true;
                GameQualitySetting.GodRay = false;
                GameQualitySetting.UniversalForwardHigh = false;
                GameQualitySetting.UniversalForwardLow = false;
                break;
            case RenderQualityLevel.High:
                GameQualitySetting.Depth = true;
                GameQualitySetting.Bloom = true;
                GameQualitySetting.DepthOfField = false;
                GameQualitySetting.RadialBlur = true;
                GameQualitySetting.LensFlares = false;
                GameQualitySetting.ToneMapping = true;
                GameQualitySetting.DrawGrass = true;
                GameQualitySetting.GodRay = false;
                GameQualitySetting.UniversalForwardHigh = true;
                GameQualitySetting.UniversalForwardLow = false;
                break;
            case RenderQualityLevel.Ultra:
                GameQualitySetting.Depth = true;
                GameQualitySetting.Bloom = true;
                GameQualitySetting.DepthOfField = true;
                GameQualitySetting.RadialBlur = true;
                GameQualitySetting.LensFlares = true;
                GameQualitySetting.ToneMapping = true;
                GameQualitySetting.DrawGrass = true;
                GameQualitySetting.GodRay = true;
                GameQualitySetting.UniversalForwardHigh = true;
                GameQualitySetting.UniversalForwardLow = false;
                break;
        }
    }

    void SetTessEnable(RenderQualityLevel enable)
    {
        Debug.LogWarning("SetTessEnable:" + enable);

        if ((int)enable == 1)
            Shader.EnableKeyword("_TESSELLATION_ON");
        else
            Shader.DisableKeyword("_TESSELLATION_ON");
    }

    void SetAAEnable(RenderQualityLevel enable)
    {
        Debug.LogWarning("SetAAEnable:" + enable);
        //EngineContext.aaEnabled = enable;
        //if (enable)
        //UniversalRenderPipeline.asset.Antialiasing = AntialiasingMode.FastApproximateAntialiasing;
        //else
        //UniversalRenderPipeline.asset.Antialiasing = AntialiasingMode.None;
#if UNITY_EDITOR
        if (UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset.AADebug)
        {
            setAA(enable);
        }
        else
        {
            UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset.Antialiasing = AntialiasingMode.None;
        }
#else
        setAA(enable);
        //UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset.SetFeature("TAA", enable);
#endif
    }

    void setAA(RenderQualityLevel level)
    {
        if (isTheEmulator)
        {
            if (level >= RenderQualityLevel.Low)
            {
                UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset.Antialiasing = AntialiasingMode.FXAA;
            }
            else
            {
                UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset.Antialiasing = AntialiasingMode.None; 
            }
        }
        else
        {
            switch (level)
            {
                case RenderQualityLevel.Ultra:
                    {
                        UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset.Antialiasing = AntialiasingMode.MSAA;
                        break;
                    }
                case RenderQualityLevel.High:
                    {
                        UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset.Antialiasing = AntialiasingMode.TAA;
                        break;
                    }
                case RenderQualityLevel.Medium:
                    {
                        UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset.Antialiasing = AntialiasingMode.SMAA;
                        break;
                    }
                case RenderQualityLevel.Low:
                    {
                        UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset.Antialiasing = AntialiasingMode.FXAA;
                        break;
                    }
                default:
                    {
                        UnityEngine.Rendering.Universal.UniversalRenderPipeline.asset.Antialiasing = AntialiasingMode.None;
                        break;
                    }
            }
        }
    }

    public class SettingInfo
    {
        public RenderQualityLevel Level;
        public RenderQualityLevel MatLevel;
        public RenderQualityLevel ShadowLevel;
        public RenderQualityLevel SFXLevel;
        public RenderQualityLevel TextureLevel;
        public RenderQualityLevel AfterEffectLevel;
        public RenderQualityLevel AAEnable;
        //public RenderQualityLevel TessEnable;
    }
}