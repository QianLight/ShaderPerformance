
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;

// [ExcludeFromPreset]
public class ScreenEffect /*:ScriptableRendererFeature*/
{
    public ScreenEffectType type;
    public bool additivePass;
    public bool overrideProperty = false;
    private Material mat;


    public MaterialPropertyBlock mpb;
    public int transitionState = 0;//0:disable 1:needCapture 2:showing
    
    public static bool enabled = false;
    private static RenderTexture temp;
    public enum ScreenEffectType
    {
        // None = 0,
        OldFilm = 1,
        CartoonShine = 2,
        MaskLUT = 3,
        GrabTransition = 4
    }

    private static ScreenEffect singleton;
    public static readonly int CartoonShineMin = Shader.PropertyToID("_CartoonShineMin");
    public static readonly int CartoonShineFlip = Shader.PropertyToID("_CartoonShineFlip");
    public static readonly int CartoonShineStrength = Shader.PropertyToID("_CartoonShineStrength");
    public static readonly int MaskLUTColor = Shader.PropertyToID("_MaskGrayColor");
    public static readonly int TransitionTex = Shader.PropertyToID("_ScreenCopyTexture");
    public static readonly int TransitionTransparent = Shader.PropertyToID("_TransitionTransparent");


    public ScreenEffect()
    {
        // singleton = this;
        InitMat();
        mpb = new MaterialPropertyBlock();
    }
    public static ScreenEffect Instance()
    {
        if (singleton == null) singleton = new ScreenEffect();
        return singleton;
    }

    public Material GetMat()
    {
        if (mat == null) InitMat();
        return mat;
    }
    public void InitMat()
    {
        var shader = Shader.Find("URP/SFX/ScreenEffect");
        if(shader == null) shader = CFEngine.ZeusAssetManager.singleton.GetAsset("AssetRes/URP_ScreenEffect.shader", false, typeof(Shader)) as Shader;
        mat = new Material(shader);
    }
    public void SetState(ScreenEffectType type)
    {
        this.type = type;
        if (type == ScreenEffectType.MaskLUT)
        {
            additivePass = true;
        }
        else
        {
            additivePass = false;
        }
    }

    public void OverrideProperty(bool isOverride)
    {
        overrideProperty = isOverride;
        enabled = isOverride;
    }

    // public override void Create()
    // {
    //
    // }
    //
    // public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    // {
    //
    // }

    public static void SetRT(int w, int h)
    {
        temp = RenderTexture.GetTemporary(w,h, 24, GraphicsFormat.B10G11R11_UFloatPack32, 1, RenderTextureMemoryless.None);
        temp.name = "ScreenEffectTemp";
    }

    public static void ReleaseRT()
    {
        if (temp != null)
        {
            temp.Release();
            temp = null;
        }
    }

    public static RenderTexture GetTempTex()
    {
        return temp;
    }
    
}