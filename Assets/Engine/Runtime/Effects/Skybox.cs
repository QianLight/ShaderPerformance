using System;
using System.IO;
using UnityEngine;

namespace CFEngine
{
    [Serializable]
    [Env(typeof(SkyboxModify), "Env/Skybox")]
    public class Skybox : EnvSetting
    {
        [CFResPath(typeof(Material), "", EnvBlock.ResOffset_SkyBox, false)]
        public ResParam skyBoxMat = new ResParam { value = "" };

        [CFResPath(typeof(Texture2D), "", EnvBlock.ResOffset_ScatterTex, false)]
        public ResParam scatterTex = new ResParam() { value = "" };

        [CFParam4("SunDirection.x", 1.0f, 0, 100, -1, C4DataType.Float,
            "SunDirection.y", 0, 0, 1, -1, C4DataType.Float,
            "SunDirection.z", 1, 0, 5, -1, C4DataType.Float,
            "Exposure", 5, 0, 100, -1, C4DataType.Float)]
        public Vector4Param scatterParams = new Vector4Param() { value = new Vector4() };

        [CFParam4("Scatter Fade", 0.5f, ReciprocalBasedCurve.MIN_CURVENESS, 2, -1, C4DataType.FloatRange,
            "Sss Alpha Fade", 0.5f, ReciprocalBasedCurve.MIN_CURVENESS, 2, -1, C4DataType.FloatRange,
            "Fog Fade", 2f, ReciprocalBasedCurve.MIN_CURVENESS, 2, -1, C4DataType.FloatRange,
            "Sss Dir Fade", 3.5f, ReciprocalBasedCurve.MIN_CURVENESS, 20, -1, C4DataType.FloatRange)]
        public Vector4Param skyboxParams0 = new Vector4Param() { value = new Vector4(0.5f, 0.5f, 2f, 3.5f) };

        [CFParam4("Fog Fade Start", 0.08f, -1, 1, -1, C4DataType.FloatRange,
                  "Fog Fade End", 0.49f, -1, 1, -1, C4DataType.FloatRange,
                  "Scatter Fade Start", 0.2f, -1, 1, -1, C4DataType.FloatRange,
                  "Scatter Fade End", 0.38f, -1, 1, -1, C4DataType.FloatRange)]
        public Vector4Param skyboxParams1 = new Vector4Param() { value = new Vector4(0.08f, 0.49f, 0.2f, 0.38f) };

        [CFParam4("SunScale", 0.0005f, 0, 0.01f, -1, C4DataType.FloatRange,
            "RotateSpeed", 0.6f, 0, 10, -1, C4DataType.Float,
            "Sun Bloom", 1, 0, 5, -1, C4DataType.FloatRange,
            "Cloud Bloom", 1, 0, 5, -1, C4DataType.FloatRange)]
        public Vector4Param skyboxParams2 = new Vector4Param() { value = new Vector4(0.0005f, 0.6f, 0f, 0f) };

        [CFColorUsage(true, true, 1, 1, 1, 1)]
        public ColorParam sunColor = new ColorParam() { value = new Color(0.74902f, 0.607843f, 0.376471f, 1f) };

        [CFColorUsage(true, true, 1, 1, 1, 1)]
        public ColorParam tintColor = new ColorParam() { value = new Color(0.698039f, 0.698039f, 0.698039f, 1f) };

        [CFColorUsage(true, true, 1, 1, 1, 1)]
        public ColorParam sssColor = new ColorParam() { value = new Color(0.74902f, 0.74902f, 0.321569f, 1f) };

        public float ScatterFade => skyboxParams0.value.x;
        public float SssFade => skyboxParams0.value.y;
        public float FogFade => skyboxParams0.value.z;
        public float SssDirectionalFade => skyboxParams0.value.w;
        public float SunScale => skyboxParams2.value.x;
        public float RotateSpeed => skyboxParams2.value.y;
        public Vector3 ScatterSunDir => scatterParams.value;
        public float ScatterExposure => scatterParams.value.w;

        public override void InitParamaters(ListObjectWrapper<ISceneObject> objects, EnvModify envModify)
        {
            InnerInitParamters(objects, envModify, "Skybox");
            CreateParam(ref skyBoxMat, nameof(skyBoxMat), objects, envModify);
            CreateParam(ref scatterTex, nameof(scatterTex), objects, envModify);
            CreateParam(ref scatterParams, nameof(scatterParams), objects, envModify);
            CreateParam(ref sunColor, nameof(sunColor), objects, envModify);
            CreateParam(ref skyboxParams0, nameof(skyboxParams0), objects, envModify);
            CreateParam(ref skyboxParams1, nameof(skyboxParams1), objects, envModify);
            CreateParam(ref skyboxParams2, nameof(skyboxParams2), objects, envModify);
            CreateParam(ref sssColor, nameof(sssColor), objects, envModify);
            CreateParam(ref tintColor, nameof(tintColor), objects, envModify);
        }

        public override void ResetEffect()
        {
            active.value = true;
        }

        public override bool IsEnabledAndSupported()
        {
            return !string.IsNullOrEmpty(skyBoxMat.value);
        }

#if UNITY_EDITOR
        public override void InitEditorParamaters(ListObjectWrapper<ISceneObject> objects, EnvModify envModify, bool init)
        {
            skyBoxMat.resOffset = EnvBlock.ResOffset_SkyBox;
            skyBoxMat.resType = ResObject.Mat;
            scatterTex.resOffset = EnvBlock.ResOffset_ScatterTex;
            scatterTex.resType = ResObject.Tex_2D_EXR;
            if (init)
            {
                skyBoxMat.Init();
                scatterTex.Init();
            }
        }
#endif
        public override EnvSettingType GetEnvType()
        {
            return EnvSettingType.Skybox;
        }

        public override void UninitParamaters()
        {
            base.UninitParamaters();
            skyBoxMat.UnInit();
            scatterTex.UnInit();
        }

        public override EnvSetting Load(CFBinaryReader reader, EngineContext context)
        {
            Skybox setting = Load<Skybox>((int)EnvSettingType.Skybox);
            setting.skyBoxMat.Load(reader,false);
            setting.scatterTex.Load(reader, false);
            reader.ReadVector(ref setting.scatterParams.value);
            reader.ReadVector(ref setting.sunColor.value);
            reader.ReadVector(ref setting.skyboxParams0.value);
            reader.ReadVector(ref setting.skyboxParams1.value);
            reader.ReadVector(ref setting.skyboxParams2.value);
            reader.ReadVector(ref setting.sssColor.value);
            reader.ReadVector(ref setting.tintColor.value);
            return setting;
        }

#if UNITY_EDITOR
        public override void Save(BinaryWriter bw)
        {
            EditorCommon.WriteRes(bw, skyBoxMat, false);
            EditorCommon.WriteRes(bw, scatterTex);
            EditorCommon.WriteVector(bw, scatterParams.value);
            EditorCommon.WriteVector(bw, sunColor.value);
            EditorCommon.WriteVector(bw, skyboxParams0.value);
            EditorCommon.WriteVector(bw, skyboxParams1.value);
            EditorCommon.WriteVector(bw, skyboxParams2.value);
            EditorCommon.WriteVector(bw, sssColor.value);
            EditorCommon.WriteVector(bw, tintColor.value);
        }
#endif

    }

    public sealed class SkyboxModify : EnvModify<Skybox>
    {
        private static class Uniforms
        {
            public static readonly int _Skybox = Shader.PropertyToID("_Skybox");
            public static readonly int _SkyMat = Shader.PropertyToID("_SkyMat");
            public static readonly int _ScatterTex = Shader.PropertyToID("_ScatterTex");
            public static readonly int _CloudTex = Shader.PropertyToID("_CloudTex");
            public static readonly int _CloudUVScale = Shader.PropertyToID("_CloudUVScale");
            public static readonly int _CloudUVOffset = Shader.PropertyToID("_CloudUVOffset");
            public static readonly int _CloudLightColor = Shader.PropertyToID("_CloudLightColor");
            public static readonly int _CloudColor = Shader.PropertyToID("_CloudColor");
            public static readonly int _CloudAmbient = Shader.PropertyToID("_CloudAmbient");
            public static readonly int _CloudParams0 = Shader.PropertyToID("_CloudParams0");
            public static readonly int _CloudParams1 = Shader.PropertyToID("_CloudParams1");
            public static readonly int _Tex = Shader.PropertyToID("_Tex");
            public static readonly int _SkyboxParams0 = Shader.PropertyToID("_SkyboxParams0");
            public static readonly int _SkyboxParams1 = Shader.PropertyToID("_SkyboxParams1");
            public static readonly int _SkyboxSunColor = Shader.PropertyToID("_SkyboxSunColor");
            public static readonly int _SssColor = Shader.PropertyToID("_SssColor");
            public static readonly int _Tint = Shader.PropertyToID("_Tint");
            public static readonly int _CurvesA = Shader.PropertyToID("_CurvesA");
            public static readonly int _CurvesB = Shader.PropertyToID("_CurvesB");
            public static readonly int _CurvesC = Shader.PropertyToID("_CurvesC");
        }

        private PreviewScatterInfo previewScatterInfo = new PreviewScatterInfo();
        private ResLoadCb processResCb = ProcessResCb;
        private Vector4 uvOffset = Vector4.zero;
        private AssetHandler switchSky;
        private Cubemap SkyBox;
        public static bool autoRefreshSystemValue = true;

        private ReciprocalBasedCurve sssRemapper = ReciprocalBasedCurve.Create(1f, false, false);
        private ReciprocalBasedCurve fogRemapper = ReciprocalBasedCurve.Create(1f, false, true);
        private ReciprocalBasedCurve sssDirectionalRemapper = ReciprocalBasedCurve.Create(1f, false, true);
        private ReciprocalBasedCurve scatterRemappter = ReciprocalBasedCurve.Create(1f, false, true);

#if UNITY_EDITOR
        public override void BeginDump()
        {
            base.BeginDump();
            AddKeyName(Uniforms._Skybox, "_Skybox");
            AddKeyName(Uniforms._SkyMat, "_SkyMat");
            AddKeyName(Uniforms._ScatterTex, "_ScatterTex");
        }
#endif

        public override void Update(EngineContext context, IRenderContext renderContext)
        {
            UpdateSkybox();

            UpdateScatterTex(renderContext);
        }

        private void UpdateScatterTex(IRenderContext renderContext)
        {
            RenderContext rc = renderContext as RenderContext;
            SavedScatterInfo info = new SavedScatterInfo()
            {
                lut = settings.scatterTex.res as Texture2D,
                param = settings.scatterParams.value
            };

            if (Application.isPlaying)
            {
                if (info.Lut)
                {
                    ScatterHelper.Apply(info);
                }
                else
                {
                    ScatterHelper.Apply(rc.resources.defaultScatterInfo);
                }
            }
            else
            {
                ScatterHelper.Bake(rc, ref previewScatterInfo);
                ScatterHelper.Apply(previewScatterInfo);
            }
        }

        private void UpdateSkybox()
        {
            Material sky = null;
            SkyBox = null;
            if (!AssetHandler.IsValid(switchSky))
            {
                sky = settings.skyBoxMat.res as Material;
                if (sky != null)
                {
                    SkyBox = sky.GetTexture(Uniforms._Tex) as Cubemap;
                }
            }
#if UNITY_EDITOR
            if (autoRefreshSystemValue)
#endif
            {
                RenderSettings.skybox = sky;
            }
#if UNITY_EDITOR
            AddDumpParam(Uniforms._SkyMat, sky);
#endif
            SetShaderValue(Uniforms._Skybox, SkyBox);

            sssRemapper.Curveness = settings.SssFade;
            sssDirectionalRemapper.Curveness = settings.SssDirectionalFade;
            fogRemapper.Curveness = settings.FogFade;
            scatterRemappter.Curveness = settings.ScatterFade;
            Vector4 a = new Vector4(sssRemapper.A, sssDirectionalRemapper.A, fogRemapper.A, scatterRemappter.A);
            Vector4 b = new Vector4(sssRemapper.B, sssDirectionalRemapper.B, fogRemapper.B, scatterRemappter.B);
            Vector4 c = new Vector4(sssRemapper.C, sssDirectionalRemapper.C, fogRemapper.C, scatterRemappter.C);
            SetShaderValue(Uniforms._CurvesA, a);
            SetShaderValue(Uniforms._CurvesB, b);
            SetShaderValue(Uniforms._CurvesC, c);

            SetShaderValue(Uniforms._SkyboxParams0, settings.skyboxParams1);
            SetShaderValue(Uniforms._SkyboxParams1, settings.skyboxParams2);
            SetShaderValue(Uniforms._SkyboxSunColor, settings.sunColor.value);
            SetShaderValue(Uniforms._SssColor, settings.sssColor.value);
            SetShaderValue(Uniforms._Tint, settings.tintColor.value);
        }

        public override void Release(EngineContext context, IRenderContext renderContext)
        {
            base.Release(context, renderContext);
            LoadMgr.singleton.Destroy(ref switchSky);
            SkyBox = null;
        }

        private static void ProcessResCb(AssetHandler ah, LoadInstance li)
        {
            var modify = li.loadHolder as SkyboxModify;
            if (modify != null)
            {
                RenderSettings.skybox = ah.obj as Material;
#if UNITY_EDITOR
                modify.AddDumpParam(Uniforms._SkyMat, RenderSettings.skybox);
#endif
            }
        }

        public void SwitchSkyBox(string path)
        {
            LoadMgr.singleton.Destroy(ref switchSky);
            if (string.IsNullOrEmpty(path))
            {
                Material mat = settings.skyBoxMat.res as Material;
                RenderSettings.skybox = mat;
#if UNITY_EDITOR
                AddDumpParam(Uniforms._SkyMat, mat);
#endif
            }
            else
            {

                LoadMgr.GetAssetHandler(ref switchSky, path, ResObject.ResExt_Mat);
                LoadMgr.loadContext.Init(processResCb,this, LoadMgr.AsyncLoad);
                LoadMgr.singleton.LoadAsset<Material>(switchSky, ResObject.ResExt_Mat);
            }
        }

    }
}
