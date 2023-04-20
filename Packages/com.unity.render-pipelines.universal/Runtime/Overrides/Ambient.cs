using System;
using CFEngine;

namespace UnityEngine.Rendering.Universal
{
    public enum AmbientType
    {
        Flat,
        Trilight,
        SkyBox,
    }

    [Serializable]
    public struct SHInfo
    {
        public Vector4 shAr;
        public Vector4 shAg;
        public Vector4 shAb;
        public Vector4 shBr;
        public Vector4 shBg;
        public Vector4 shBb;
        public Vector4 shC;
        public AmbientType ambientMode;
        public Color flatColor;
        public Color skyColor;
        public Color equatorColor;
        public Color groundColor;
        public Cubemap skyCube;
        public float skyIntensity;
        public bool debug;
    }

    [Serializable]
    public sealed class SHParameter : VolumeParameter<SHInfo>
    {
        public SHParameter(SHInfo value, bool overrideState = false) : base(value, overrideState)
        {
        }
    }

    [Serializable, VolumeComponentMenu("Post-processing/Ambient")]
    public sealed class Ambient : VolumeComponent
    {
        public SHParameter sceneSH = new SHParameter(default);
        public SHParameter roleSH = new SHParameter(default);
        public ClampedFloatParameter lightmapIntensity = new ClampedFloatParameter(0.5f, 0f, 5f);
        public ClampedFloatParameter lightmapDefault = new ClampedFloatParameter(0, 0, 1);
        public ClampedFloatParameter ambientLightScale = new ClampedFloatParameter(1f, 0f, 5f);
        public ClampedFloatParameter ambientDarkScale = new ClampedFloatParameter(1f, 0f, 5f);
        public ClampedFloatParameter envHdr = new ClampedFloatParameter(1.0f, 0f, 100f);
        public BoolParameter envGamma = new BoolParameter(false);

        public ClampedFloatParameter ambietnMax = new ClampedFloatParameter(1f, 0f, 5f);
        public ClampedFloatParameter roleAmbietnMax = new ClampedFloatParameter(1f, 0f, 5f);

        // TODO: Make sure if contrast light using.
        public BoolParameter contrastLight = new BoolParameter(false);
        public ClampedFloatParameter contrastIntensity = new ClampedFloatParameter(1f, 0f, 20f);
        public ColorParameter roleShadowColor = new ColorParameter(new Color(0, 0, 0, 1));

        #region Uniforms

        public static readonly int _AmbientParam = Shader.PropertyToID("_AmbientParam");
        public static readonly int _AmbientParam1 = Shader.PropertyToID("_AmbientParam1");
        public static readonly int _AmbientParam2 = Shader.PropertyToID("_AmbientParam2");
        public static readonly int _Scene_SHAr = Shader.PropertyToID("_Scene_SHAr");
        public static readonly int _Scene_SHAg = Shader.PropertyToID("_Scene_SHAg");
        public static readonly int _Scene_SHAb = Shader.PropertyToID("_Scene_SHAb");
        public static readonly int _Scene_SHBr = Shader.PropertyToID("_Scene_SHBr");
        public static readonly int _Scene_SHBg = Shader.PropertyToID("_Scene_SHBg");
        public static readonly int _Scene_SHBb = Shader.PropertyToID("_Scene_SHBb");
        public static readonly int _Scene_SHC = Shader.PropertyToID("_Scene_SHC");
        public static readonly int _Custom_SHAr = Shader.PropertyToID("_Custom_SHAr");
        public static readonly int _Custom_SHAg = Shader.PropertyToID("_Custom_SHAg");
        public static readonly int _Custom_SHAb = Shader.PropertyToID("_Custom_SHAb");
        public static readonly int _Custom_SHBr = Shader.PropertyToID("_Custom_SHBr");
        public static readonly int _Custom_SHBg = Shader.PropertyToID("_Custom_SHBg");
        public static readonly int _Custom_SHBb = Shader.PropertyToID("_Custom_SHBb");
        public static readonly int _Custom_SHC = Shader.PropertyToID("_Custom_SHC");
        public static readonly int _RoleShadowColor = Shader.PropertyToID("_RoleShadowColor");
        
        #endregion

        public override void OnOverrideFinish()
        {
            base.OnOverrideFinish();
            Update();
        }

        public void Update()
        {
            Vector4 ambientParam = new Vector4(
                contrastLight.value ? 0 : ambietnMax.value,
                contrastLight.value ? 0 : roleAmbietnMax.value,
#if (!UNITY_ANDROID && !UNITY_IOS)
                1,
#else
                lightmapIntensity.value,
#endif
                lightmapDefault.value
            );

            SHInfo roleSH = this.roleSH.value;
            Shader.SetGlobalVector(_Custom_SHAr, roleSH.shAr);
            Shader.SetGlobalVector(_Custom_SHAg, roleSH.shAg);
            Shader.SetGlobalVector(_Custom_SHAb, roleSH.shAb);
            Shader.SetGlobalVector(_Custom_SHBr, roleSH.shBr);
            Shader.SetGlobalVector(_Custom_SHBg, roleSH.shBg);
            Shader.SetGlobalVector(_Custom_SHBb, roleSH.shBb);
            Shader.SetGlobalVector(_Custom_SHC, roleSH.shC);

            SHInfo sceneSH = this.sceneSH.value;
            Shader.SetGlobalVector(_Scene_SHAr, sceneSH.shAr);
            Shader.SetGlobalVector(_Scene_SHAg, sceneSH.shAg);
            Shader.SetGlobalVector(_Scene_SHAb, sceneSH.shAb);
            Shader.SetGlobalVector(_Scene_SHBr, sceneSH.shBr);
            Shader.SetGlobalVector(_Scene_SHBg, sceneSH.shBg);
            Shader.SetGlobalVector(_Scene_SHBb, sceneSH.shBb);
            Shader.SetGlobalVector(_Scene_SHC, sceneSH.shC);

            Vector4 ambientParam1 = new Vector4(
                envHdr.value,
                ambientLightScale.value,
                ambientDarkScale.value,
                contrastLight.value ? 1 : 0
            );

            Vector4 ambientParam2 = new Vector4(
                contrastLight.value ? 0 : envHdr.value,
                envGamma.value ? 2.2f : 1,
                0,
                contrastIntensity.value
            );

            Shader.SetGlobalVector(_AmbientParam1, ambientParam1);
            Shader.SetGlobalVector(_AmbientParam, ambientParam);
            Shader.SetGlobalVector(_AmbientParam2, ambientParam2);
            Shader.SetGlobalVector(_RoleShadowColor, roleShadowColor.value);
        }
    }
}