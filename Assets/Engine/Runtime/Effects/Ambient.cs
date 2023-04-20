using System;
using System.IO;
using CFEngine.SRP;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
#endif

namespace CFEngine
{
    [Serializable]
    [Env(typeof(AmbientModify), "Env/Ambient")]
    public sealed class Ambient : EnvSetting
    {
        [CFSH(54, 58, 66,
            54, 58, 66,
            29, 32, 34,
            12, 11, 9,
            1)]
        public SHParam sceneSH = new SHParam() { value = SHInfo.CreateEmpty() };

        [CFSH(54, 58, 66,
            54, 58, 66,
            29, 32, 34,
            12, 11, 9,
            1)]
        public SHParam roleSH = new SHParam() { value = SHInfo.CreateEmpty() };

        [CFSH(1f, 1f, 1f,
            54, 58, 66,
            29, 32, 34,
            12, 11, 9,
            1)]
        public SHParam roleSHV2 = new SHParam()
        { 
            value = new SHInfo()
            {
                #if UNITY_EDITOR
                    flatColor = new Color(1f, 1f, 1f, 1f),
                    skyColor = new Color32(54, 58, 66, 255),
                    equatorColor = new Color32(29, 32, 34, 255),
                    groundColor = new Color32(12, 11, 9, 255),
                    skyIntensity = 1,
                #endif
            }
        };

        [CFParam4("AmbientMax", 1, 0, 5f, -1, C4DataType.FloatRange,
            "RoleAmbientMax", 0.5f, 0, 5f, -1, C4DataType.FloatRange,
            "LightmapIntensity", 4.6f, 0, 10, -1, C4DataType.FloatRange,
            "LightmapDefault", 0, 0, 1, -1, C4DataType.FloatRange)]
        public Vector4Param ambientParam = new Vector4Param { value = new Vector4 (1, 0.5f, 4.6f, 0) };

        [CFResPath(typeof(Material), "", EnvBlock.ResOffset_SkyBox, false)]
        public ResParam skyBoxMat = new ResParam { value = "" };

        [CFResPath(typeof(Cubemap), "", EnvBlock.ResOffset_EnvCube, true)]
        public ResParam envCube = new ResParam { value = "" };

        [CFParam4("EnvHdr", 1.0f, 0, 100, -1, C4DataType.None,
            "AmbientLightScale", 1, 0, 5, -1, C4DataType.FloatRange,
            "AmbientDarkScale", 1, 0, 5, -1, C4DataType.FloatRange,
            "ContrastLight", 0, 0, 1, -1, C4DataType.Bool), CFTooltip("IBL Param.")]
        public Vector4Param ambientParam1 = new Vector4Param { value = new Vector4(1.0f, 1.0f, 0.5f, 0) };

        [CFParam4("EnvHdr", 1.0f, 0, 100, -1, C4DataType.FloatRange,
            "EnvGamma", 0, 0, 1, -1, C4DataType.Bool,
            "", 1, 0, 5, -1, C4DataType.None,
            "", 5, 0, 100, -1, C4DataType.None)]
        public Vector4Param ambientParam2 = new Vector4Param { value = new Vector4(5.0f, 1.0f, 0, 1) };

        [CFParam4("ContrastLight", 0, 0, 1, -1, C4DataType.Bool,
            "ContrastIntensity", 1, 0, 20, -1, C4DataType.FloatRange,
            "", 1, 0, 5, -1, C4DataType.None,
            "", 5, 0, 100, -1, C4DataType.None)]
        public Vector4Param ambientParam3 = new Vector4Param { value = new Vector4(0, 1, 0, 0) };

#if UNITY_EDITOR
        public static Dictionary<int,SHColorDebug> shColorDebug = new Dictionary<int, SHColorDebug>();
#endif
        public override void InitParamaters(ListObjectWrapper<ISceneObject> objects, EnvModify envModify)
        {
            InnerInitParamters(objects, envModify, "Ambient");
            CreateParam(ref sceneSH, nameof(sceneSH), objects, envModify);
            CreateParam(ref roleSH, nameof(roleSH), objects, envModify);
            CreateParam(ref roleSHV2, nameof(roleSHV2), objects, envModify);
            CreateParam(ref ambientParam, nameof(ambientParam), objects, envModify);
            CreateParam(ref ambientParam1, nameof(ambientParam1), objects, envModify);
            CreateParam(ref ambientParam2, nameof(ambientParam2), objects, envModify);
            CreateParam(ref ambientParam3, nameof(ambientParam3), objects, envModify);
            CreateParam(ref skyBoxMat, nameof(skyBoxMat), objects, envModify);
            CreateParam(ref envCube, nameof(envCube), objects, envModify);
        }

#if UNITY_EDITOR
        public override void InitEditorParamaters(ListObjectWrapper<ISceneObject> objects, EnvModify envModify, bool init)
        {
            skyBoxMat.resOffset = EnvBlock.ResOffset_SkyBox;
            skyBoxMat.resType = ResObject.Mat;
            envCube.resOffset = EnvBlock.ResOffset_EnvCube;
            if (init)
            {
                if (envCube.resType == ResObject.None)
                {
                    envCube.resType = ResObject.Tex_Cube;
                }
                skyBoxMat.Init();
                envCube.Init();
            }
        }
#endif

        public override EnvSettingType GetEnvType()
        {
            return EnvSettingType.Ambient;
        }

        public override void ResetEffect()
        {
            active.value = true;
        }

        public override void UninitParamaters()
        {
            base.UninitParamaters();
            skyBoxMat.UnInit();
            envCube.UnInit();
        }

        // public static void Creator (out EnvSetting setting, out EnvModify modify)
        // {
        //     Create<Ambient, AmbientModify> (out setting, out modify);
        // }

        public override EnvSetting Load(CFBinaryReader reader, EngineContext context)
        {
            Ambient setting = Load<Ambient>((int)EnvSettingType.Ambient);
            setting.sceneSH.Load(reader, 10000);
            setting.roleSH.Load(reader, 10100);
#if UNITY_EDITOR
            if (context.IsValidResVersion(RenderContext.ResVersionRoleLightingV2, EngineContext.Cmp_GE))
#endif
            {
                setting.roleSHV2.Load(reader, 10200);
            }
            reader.ReadVector(ref setting.ambientParam.value);
            setting.skyBoxMat.Load(reader, false);
            setting.envCube.Load(reader, false);
            reader.ReadVector(ref setting.ambientParam1.value);
            reader.ReadVector(ref setting.ambientParam2.value);
            return setting;
        }

#if UNITY_EDITOR
        public override void Save(BinaryWriter bw)
        {
            sceneSH.Save(bw);
            SHParam.AddSHColorDebug(sceneSH, 10000);
            roleSH.Save(bw);
            SHParam.AddSHColorDebug(roleSH, 10100);
            roleSHV2.Save(bw);
            SHParam.AddSHColorDebug(roleSHV2, 10200);
            EditorCommon.WriteVector(bw, ambientParam.value);
            EditorCommon.WriteRes(bw, skyBoxMat, false);
            EditorCommon.WriteRes(bw, envCube);
            EditorCommon.WriteVector(bw, ambientParam1.value);
            var v = ambientParam2.value;
            //v.z = (int)sceneSH.value.ambientMode;
            //v.w = (int)roleSH.value.ambientMode;
            EditorCommon.WriteVector(bw, v);
        }
#endif
    }

    public sealed class AmbientModify : EnvModify<Ambient>
    {
        private Cubemap SkyBox;
        private AssetHandler switchSky;
        private ResLoadCb processResCb = ProcessResCb;
        public static readonly int _AmbientParam = Shader.PropertyToID("_AmbientParam");
        public static readonly int _AmbientParam1 = Shader.PropertyToID("_AmbientParam1");
        public static readonly int _AmbientParam2 = Shader.PropertyToID("_AmbientParam2");
        public static readonly int role_SHAr = Shader.PropertyToID("role_SHAr");
        public static readonly int role_SHAg = Shader.PropertyToID("role_SHAg");
        public static readonly int role_SHAb = Shader.PropertyToID("role_SHAb");
        public static readonly int role_SHBr = Shader.PropertyToID("role_SHBr");
        public static readonly int role_SHBg = Shader.PropertyToID("role_SHBg");
        public static readonly int role_SHBb = Shader.PropertyToID("role_SHBb");
        public static readonly int role_SHC = Shader.PropertyToID("role_SHC");
        public static readonly int scene_SHAr = Shader.PropertyToID("scene_SHAr");
        public static readonly int scene_SHAg = Shader.PropertyToID("scene_SHAg");
        public static readonly int scene_SHAb = Shader.PropertyToID("scene_SHAb");
        public static readonly int scene_SHBr = Shader.PropertyToID("scene_SHBr");
        public static readonly int scene_SHBg = Shader.PropertyToID("scene_SHBg");
        public static readonly int scene_SHBb = Shader.PropertyToID("scene_SHBb");
        public static readonly int scene_SHC = Shader.PropertyToID("scene_SHC");

        public static readonly int _EnvCube = Shader.PropertyToID("_EnvCube");

        public static readonly int _Skybox = Shader.PropertyToID("_Skybox");

        public static readonly int Env_SkyCubeTex = Shader.PropertyToID("_Tex");

#if UNITY_EDITOR
        public static bool autoRefreshSystemValue = true;

        public static readonly int ambientMode = Shader.PropertyToID("ambientMode");

        public static readonly int flatColor = Shader.PropertyToID("flatColor");

        public static readonly int skyColor = Shader.PropertyToID("skyColor");

        public static readonly int equatorColor = Shader.PropertyToID("equatorColor");
        public static readonly int groundColor = Shader.PropertyToID("groundColor");
        public static readonly int skyIntensity = Shader.PropertyToID("skyIntensity");
        public static readonly int _SkyMat = Shader.PropertyToID("_SkyMat");

        private PreviewScatterInfo previewScatterInfo = new PreviewScatterInfo();

        public static IBLConfig GetBLConfig(Ambient ambient, bool create = false)
        {
            var cube = ambient.envCube.res as Cubemap;
            if (cube != null)
            {
                string path = AssetDatabase.GetAssetPath(cube);
                int index = path.LastIndexOf(".");
                path = path.Substring(0, index);
                path += ".asset";
                if (System.IO.File.Exists(path))
                {
                    return AssetDatabase.LoadAssetAtPath<IBLConfig>(path);
                }
                else if (create)
                {
                    IBLConfig config = IBLConfig.CreateInstance<IBLConfig>();
                    config.name = cube.name;
                    return EditorCommon.CreateAsset<IBLConfig>(path, ".asset", config);
                }
            }
            return null;
        }

        public override void BeginDump()
        {
            base.BeginDump();
            
#if PIPELINE_URP
            return;
#endif
            
            AddKeyName(_AmbientParam, "_AmbientParam");
            AddKeyName(_AmbientParam1, "_AmbientParam1");
            AddKeyName(_AmbientParam2, "_AmbientParam2");
            AddKeyName(role_SHAr, "role_SHAr");
            AddKeyName(role_SHAg, "role_SHAg");
            AddKeyName(role_SHAb, "role_SHAb");

            AddKeyName(role_SHBr, "role_SHBr");
            AddKeyName(role_SHBg, "role_SHBg");
            AddKeyName(role_SHBb, "role_SHBb");

            AddKeyName(role_SHC, "role_SHC");

            AddKeyName(scene_SHAr, "scene_SHAr");
            AddKeyName(scene_SHAg, "scene_SHAg");
            AddKeyName(scene_SHAb, "scene_SHAb");

            AddKeyName(scene_SHBr, "scene_SHBr");
            AddKeyName(scene_SHBg, "scene_SHBg");
            AddKeyName(scene_SHBb, "scene_SHBb");

            AddKeyName(scene_SHC, "scene_SHC");

            AddKeyName(ambientMode, "ambientMode");

            AddKeyName(flatColor, "flatColor");
            AddKeyName(skyColor, "skyColor");
            AddKeyName(equatorColor, "equatorColor");
            AddKeyName(groundColor, "groundColor");
            AddKeyName(skyIntensity, "skyIntensity");

            AddKeyName(_EnvCube, "_EnvCube");
            AddKeyName(_Skybox, "_Skybox");
            AddKeyName(_SkyMat, "_SkyMat");

            AddKeyName(ShaderManager.custom_SHAr, "custom_SHAr");
            AddKeyName(ShaderManager.custom_SHAg, "custom_SHAg");
            AddKeyName(ShaderManager.custom_SHAb, "custom_SHAb");

            AddKeyName(ShaderManager.custom_SHBr, "custom_SHBr");
            AddKeyName(ShaderManager.custom_SHBg, "custom_SHBg");
            AddKeyName(ShaderManager.custom_SHBb, "custom_SHBb");

            AddKeyName(ShaderManager.custom_SHC, "custom_SHC");

        }

#endif
        public override void Update(EngineContext context, IRenderContext renderContext)
        {
#if PIPELINE_URP
            return;
#endif

            if (BeginUpdate())
            {
                RenderContext rc = renderContext as RenderContext;
                ref var ambientParam3 = ref settings.ambientParam3.value;
                ref var ambientParam = ref settings.ambientParam.value;
                bool isContrastLight = ambientParam3.x > 0.5f;
#if (!UNITY_ANDROID && !UNITY_IOS)
                ambientParam.z = 1;
#endif
                if(isContrastLight)
                {
                    ambientParam.x = 0;
                    ambientParam.y = 0;
                }
                SetShaderValue(_AmbientParam, ref ambientParam);

                ref SHInfo roleSH = ref settings.roleSH.value;
                SetShaderValue(ShaderManager.custom_SHAr, ref roleSH.shAr);
                SetShaderValue(ShaderManager.custom_SHAg, ref roleSH.shAg);
                SetShaderValue(ShaderManager.custom_SHAb, ref roleSH.shAb);
                SetShaderValue(ShaderManager.custom_SHBr, ref roleSH.shBr);
                SetShaderValue(ShaderManager.custom_SHBg, ref roleSH.shBg);
                SetShaderValue(ShaderManager.custom_SHBb, ref roleSH.shBb);
                SetShaderValue(ShaderManager.custom_SHC, ref roleSH.shC);
                ref var globalRoleSH = ref context.roleSH;
                globalRoleSH.shAr = roleSH.shAr;
                globalRoleSH.shAg = roleSH.shAg;
                globalRoleSH.shAb = roleSH.shAb;
                globalRoleSH.shBr = roleSH.shBr;
                globalRoleSH.shBg = roleSH.shBg;
                globalRoleSH.shBb = roleSH.shBb;
                globalRoleSH.shC = roleSH.shC;

                ref SHInfo roleSHV2 = ref settings.roleSHV2.value;
                SetShaderValue(ShaderManager.custom_ShV2Ar, ref roleSHV2.shAr);
                SetShaderValue(ShaderManager.custom_ShV2Ag, ref roleSHV2.shAg);
                SetShaderValue(ShaderManager.custom_ShV2Ab, ref roleSHV2.shAb);
                SetShaderValue(ShaderManager.custom_ShV2Br, ref roleSHV2.shBr);
                SetShaderValue(ShaderManager.custom_ShV2Bg, ref roleSHV2.shBg);
                SetShaderValue(ShaderManager.custom_ShV2Bb, ref roleSHV2.shBb);
                SetShaderValue(ShaderManager.custom_ShV2C, ref roleSHV2.shC);
                ref var globalRoleSHV2 = ref context.roleShV2;
                globalRoleSHV2.shAr = roleSHV2.shAr;
                globalRoleSHV2.shAg = roleSHV2.shAg;
                globalRoleSHV2.shAb = roleSHV2.shAb;
                globalRoleSHV2.shBr = roleSHV2.shBr;
                globalRoleSHV2.shBg = roleSHV2.shBg;
                globalRoleSHV2.shBb = roleSHV2.shBb;
                globalRoleSHV2.shC = roleSHV2.shC;

                ref SHInfo sceneSH = ref settings.sceneSH.value;
                SetShaderValue(scene_SHAr, ref sceneSH.shAr);
                SetShaderValue(scene_SHAg, ref sceneSH.shAg);
                SetShaderValue(scene_SHAb, ref sceneSH.shAb);
                SetShaderValue(scene_SHBr, ref sceneSH.shBr);
                SetShaderValue(scene_SHBg, ref sceneSH.shBg);
                SetShaderValue(scene_SHBb, ref sceneSH.shBb);
                SetShaderValue(scene_SHC, ref sceneSH.shC);

#if UNITY_EDITOR
                if (autoRefreshSystemValue)
                {
                    AddDumpParam(ambientMode, sceneSH.ambientMode.ToString());
                    if (sceneSH.ambientMode == AmbientType.Flat)
                    {
                        RenderSettings.ambientLight = sceneSH.flatColor;
                        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
                        AddDumpParam(flatColor, sceneSH.flatColor);
                    }
                    else if (sceneSH.ambientMode == AmbientType.Trilight)
                    {
                        RenderSettings.ambientSkyColor = sceneSH.skyColor;
                        RenderSettings.ambientEquatorColor = sceneSH.equatorColor;
                        RenderSettings.ambientGroundColor = sceneSH.groundColor;
                        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
                        AddDumpParam(skyColor, sceneSH.skyColor);
                        AddDumpParam(equatorColor, sceneSH.equatorColor);
                        AddDumpParam(groundColor, sceneSH.groundColor);
                    }
                    else if (sceneSH.ambientMode == AmbientType.SkyBox)
                    {
                        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
                        RenderSettings.ambientIntensity = sceneSH.skyIntensity;
                        AddDumpParam(skyIntensity, sceneSH.skyIntensity);
                    }
                }
#endif
                var ambientParam1 = settings.ambientParam1.value;
                
                ambientParam1.w = ambientParam3.x;//ContrastLight
                //ambientParam1.w = 6;
                Cubemap cube = settings.envCube.res as Cubemap;
                //if (cube != null)
                //{
                //    float maxMipmap = cube.mipmapCount;
                //    ambientParam1.w = maxMipmap;
                //}
                SetShaderValue(_EnvCube, cube);
                SetShaderValue(_AmbientParam1, ref ambientParam1);
                var ambientParam2 = settings.ambientParam2.value;
                ambientParam2.y = (ambientParam2.y > 0.5f) ? (2.2f) : 1;
                ambientParam2.w = ambientParam3.y;//ContrastIntensity
                if (isContrastLight)
                {
                    ambientParam2.x = 0;
                }

#if UNITY_EDITOR
#if !UNITY_ANDROID && !UNITY_IOS
                IBLConfig iblConfig = GetBLConfig (settings);
                if (iblConfig != null)
                {
                    ambientParam2.x = iblConfig.pcHDR;
                    ambientParam2.y = iblConfig.pcGamma?1 : 0;

                    if (EngineContext.IsRunning)
                        DebugLog.AddEngineLog2 ("Set EnvCube {2},hdr:{0} gamma:{1}", ambientParam2.x, ambientParam2.y, cube != null?cube.name: "empty");
                }
                else
                {
                    ambientParam2.x = 1;
                    ambientParam2.y = 0;
                    if (EngineContext.IsRunning)
                        DebugLog.AddEngineLog2 ("Set EnvCube {0},config not find", cube != null?cube.name: "empty");
                }
#endif
#endif
                    SetShaderValue(_AmbientParam2, ref ambientParam2);
                Material sky = null;
                SkyBox = null;
                if (!AssetHandler.IsValid(switchSky))
                {
                        sky = settings.skyBoxMat.res as Material;
                        if (sky != null)
                        {
                            SkyBox = sky.GetTexture(Env_SkyCubeTex) as Cubemap;
                        }
                }
#if UNITY_EDITOR
                if (autoRefreshSystemValue && sky)
#endif
                {
                    RenderSettings.skybox = sky;
                }
#if UNITY_EDITOR
                AddDumpParam(_SkyMat, sky);
#endif

                SetShaderValue(_Skybox, SkyBox);

                EndUpdate();
            }
        }

        private static void ProcessResCb(AssetHandler ah, LoadInstance li)
        {
            var modify = li.loadHolder as AmbientModify;
            if (modify != null)
            {
                RenderSettings.skybox = ah.obj as Material;
#if UNITY_EDITOR
                modify.AddDumpParam(_SkyMat, RenderSettings.skybox);
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
                AddDumpParam(_SkyMat, mat);
#endif
            }
            else
            {

                LoadMgr.GetAssetHandler(ref switchSky, path, ResObject.ResExt_Mat);
                LoadMgr.loadContext.Init(processResCb, this);
                LoadMgr.loadContext.flag = LoadMgr.AsyncLoad;
                LoadMgr.singleton.LoadAsset<Material>(switchSky, ResObject.ResExt_Mat);
            }
        }

        public override void Release(EngineContext context, IRenderContext renderContext)
        {
            base.Release(context, renderContext);
            LoadMgr.singleton.Destroy(ref switchSky);
            SkyBox = null;
        }
    }
}
