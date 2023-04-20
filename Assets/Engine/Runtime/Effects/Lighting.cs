using System;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
#endif
namespace CFEngine
{
    [Serializable]
    [Env (typeof (LightingModify), "Env/Lighting")]
    public sealed class Lighting : EnvSetting
    {
        [CFLighting ("MainLight")]
        public LightingParam mainLightInfo = new LightingParam { value = LightInfo.CreateEmpty () };

        [CFLighting ("AddLight")]
        public LightingParam addLightInfo = new LightingParam { value = LightInfo.CreateEmpty () };
        
        [CFColorUsage (false, true, 1, 1, 1, 1)]
        public ColorParam roleLightColor = new ColorParam { value = Color.white };

        [CFColorUsage (false, true, 1.931336f, 1.931336f, 1.931336f, 1f)]
        public ColorParam roleLightColorV2 = new ColorParam { value = new Color(1.931336f, 1.931336f, 1.931336f, 1f) };

        [CFParam4 ("MinRoleHAngle", -20f, -45f, 0f, -1, C4DataType.FloatRange,
            "MaxRoleHAngle", 20f, -80f, 45f, -1, C4DataType.FloatRange,
            "AddRoleLight", 0, 0, 1, -1, C4DataType.Bool,
            "RoleLightRotOffset", 0, 0, 360, -1, C4DataType.FloatRange)]
        public Vector4Param lightParam = new Vector4Param { value = new Vector4 (-0.1f, 0.1f, 0, 0) };


        [CFParam4("LeftRightControl", 1f, -5f, 5f, 1, C4DataType.FloatRange,
         "UpDownControl", 0f, -5f, 5f,1, C4DataType.FloatRange,
         "", 0, 0, 360, -1, C4DataType.None,
         "", 0, -90, 90, -1, C4DataType.None)]
        public Vector4Param roleLightDirParam = new Vector4Param { value = new Vector4(1, 0, 0, 0) };


        [CFParam4 ("SimpleLightIntensity", 1, 0.1f, 10f, -1, C4DataType.FloatRange,
            "AddLightSpecScope", 0.5f, 0, 1, -1, C4DataType.FloatRange,
            "", 0, 0, 1, -1, C4DataType.None,
            "", 0, 0, 360, -1, C4DataType.None)]
        public Vector4Param lightParam1 = new Vector4Param { value = new Vector4 (1, 0.5f, 0, 0) };

        [CFColorUsage (false, true, 1, 1, 1, 1)]
        public ColorParam rimLightColor = new ColorParam { value = Color.white };

        [CFLighting ("WaterLight")]
        public LightingParam waterLightInfo = new LightingParam { value = LightInfo.CreateEmpty () };

#if UNITY_EDITOR

        public static DrawMainLightHandlerMode drawMainLightHandlerMode = DrawMainLightHandlerMode.Disable;
        public enum DrawMainLightHandlerMode
        {
            Disable,
            LegacyRole,
            RoleRenderV2,
        }
#endif

        public override void InitParamaters (ListObjectWrapper<ISceneObject> objects, EnvModify envModify)
        {
            InnerInitParamters (objects, envModify, "Lighting");

            CreateParam (ref mainLightInfo, nameof (mainLightInfo), objects, envModify);
            CreateParam (ref addLightInfo, nameof (addLightInfo), objects, envModify);
            CreateParam (ref roleLightColor, nameof (roleLightColor), objects, envModify);
            CreateParam (ref roleLightColorV2, nameof (roleLightColorV2), objects, envModify);
            CreateParam (ref lightParam1, nameof (lightParam1), objects, envModify);
            CreateParam (ref lightParam, nameof (lightParam), objects, envModify);
            CreateParam(ref roleLightDirParam, nameof(roleLightDirParam), objects, envModify);
            CreateParam (ref rimLightColor, nameof (rimLightColor), objects, envModify);
            CreateParam (ref waterLightInfo, nameof (waterLightInfo), objects, envModify);
        }

#if UNITY_EDITOR

        public static string mainLightName = "mainLight";
        public static string addLightName = "addLight";
        public static string waterLightName = "waterLight";
        public override void InitEditorParamaters (ListObjectWrapper<ISceneObject> objects, EnvModify envModify, bool init)
        {
            CreateLights(forceOverride);
        }

        public void CreateLights (bool create = false)
        {
            RuntimeUtilities.PrepareLight (ref mainLightInfo.value, mainLightName + key, create);
            RuntimeUtilities.PrepareLight (ref addLightInfo.value, addLightName + key, create);
            RuntimeUtilities.PrepareLight (ref waterLightInfo.value, waterLightName + key, create);
        }
#endif
        public override EnvSettingType GetEnvType ()
        {
            return EnvSettingType.Lighting;
        }

        public override void ResetEffect ()
        {
            active.value = true;
        }

        // public static void Creator (out EnvSetting setting, out EnvModify modify, bool createModify)
        // {
        //     setting = Create<Lighting> ();
        //     if (createModify)
        //     {
        //         modify = CFAllocator.Allocate<LightingModify> ();
        //     }

        //     else
        //         modify = null;
        // }

        public override EnvSetting Load (CFBinaryReader reader, EngineContext context)
        {
            Lighting setting = Load<Lighting> ((int) EnvSettingType.Lighting);
            setting.mainLightInfo.Load (reader);
            setting.addLightInfo.Load (reader);
            reader.ReadVector (ref setting.roleLightColor.value);
#if UNITY_EDITOR
            if (context.IsValidResVersion(RenderContext.ResVersionRoleLightingV2, EngineContext.Cmp_GE))
#endif
            {
                reader.ReadVector(ref setting.roleLightColorV2.value);
            }

            reader.ReadVector (ref setting.lightParam.value);
            reader.ReadVector (ref setting.lightParam1.value);
#if UNITY_EDITOR
            if (context.IsValidResVersion(RenderContext.ResVersionRoleShadow, EngineContext.Cmp_GE))
#endif
            {
                reader.ReadVector(ref setting.roleLightDirParam.value);
            }

            //     reader.ReadVector (ref setting.roleFaceShaodowDirParam.value);
            reader.ReadVector (ref setting.rimLightColor.value);
            setting.waterLightInfo.Load (reader);
            return setting;
        }

        public override void SwitchEnvArea (EngineContext context,
            ref ListObjectWrapper<ISceneObject> objects, bool recover)
        {
            base.SwitchEnvArea (context, ref objects, recover);
            context.renderflag.SetFlag (EngineContext.RFlag_SimpleLightDirty, true);
        }

#if UNITY_EDITOR
        public override void Save (BinaryWriter bw)
        {
            mainLightInfo.Save (bw);
            addLightInfo.Save (bw);
            EditorCommon.WriteVector (bw, roleLightColor.value);
            EditorCommon.WriteVector (bw, roleLightColorV2.value);
            EditorCommon.WriteVector (bw, lightParam.value);
            EditorCommon.WriteVector (bw, lightParam1.value);
            EditorCommon.WriteVector (bw, roleLightDirParam.value);
            EditorCommon.WriteVector (bw, rimLightColor.value);
            waterLightInfo.Save (bw);
        }
#endif
    }

    public sealed class LightingModify : EnvModify<Lighting>
    {
        private int lightKernel = -1;

        public static readonly int _MainLightDir0 = Shader.PropertyToID ("_MainLightDir0");
        public static readonly int _ShadowDir = Shader.PropertyToID("_ShadowDir");
        public static readonly int _MainLightColor0 = Shader.PropertyToID ("_MainLightColor0");

        public static readonly int _AddLightDir0 = Shader.PropertyToID ("_AddLightDir0");
        public static readonly int _AddLightColor0 = Shader.PropertyToID ("_AddLightColor0");
        
        public static readonly int _MainLightRoleColor = Shader.PropertyToID ("_MainLightRoleColor");
        public static readonly int _MainLightRoleColorV2 = Shader.PropertyToID ("_MainLightRoleColorV2");

        public static readonly int _RimLightColor = Shader.PropertyToID ("_RimLightColor");

        public static readonly int _WaterLightDir = Shader.PropertyToID ("_WaterLightDir");

        public static readonly int _DyanmicPointLightPos = Shader.PropertyToID ("_DyanmicPointLightPos");
        public static readonly int _DyanmicPointLightColor = Shader.PropertyToID ("_DyanmicPointLightColor");
        public static readonly int _DyanmicPointLightParam = Shader.PropertyToID ("_DyanmicPointLightParam");


        public static readonly int _GlobalLightParam = Shader.PropertyToID("_GlobalLightParam");
        //cs param
        public static readonly int _LightInfos = Shader.PropertyToID ("_LightInfos");

        public static readonly int _VoxelLightParam = Shader.PropertyToID ("_VoxelLightParam");

        //static&dynamic light
        public static readonly int _StaticLightParam = Shader.PropertyToID ("_StaticLightParam");

        public static readonly int _StaticLightTex = Shader.PropertyToID ("_StaticLightTex");

        public static readonly int _Output = Shader.PropertyToID ("_Output");

        public static readonly int _GameViewWorldSpaceCameraPos = Shader.PropertyToID ("_GameViewWorldSpaceCameraPos");
        public static readonly int _DepthShadow = Shader.PropertyToID("_DepthShadow");

#if UNITY_EDITOR
        public static EnvAreaProfile uiSceneLights;
        public static EnvAreaProfile envSceneLights;

        public static RenderTexture staticLightRT = null;
        public static readonly int shadowProjDir = Shader.PropertyToID ("shadowProjDir");
        public static readonly int invSin = Shader.PropertyToID ("invSin");

        public static SavedBool showLightGizmos;
        public static bool forceUpdateVoxelLight = false;
        int maxLightCount = 128;

        public static SavedFloat updateFps;
#endif

#if UNITY_EDITOR

        public void UpdateDebug()
        {
            if (showLightGizmos == null)
            {
                showLightGizmos = new SavedBool($"{EngineContext.sceneNameLower}.{nameof(showLightGizmos)}", false);
            }
            else
            {
                showLightGizmos.Reset($"{EngineContext.sceneNameLower}.{nameof(showLightGizmos)}");
            }

            if (updateFps == null)
            {
                updateFps = new SavedFloat($"{EngineContext.sceneNameLower}.{nameof(updateFps)}", 2f);
            }
            else
            {
                updateFps.Reset($"{EngineContext.sceneNameLower}.{nameof(updateFps)}");
            }
        }

        public override void BeginDump ()
        {
            base.BeginDump ();
#if !PIPELINE_URP
            AddKeyName (_MainLightColor0, "_MainLightColor0");
            AddKeyName (_MainLightDir0, "_MainLightDir0");
            AddKeyName (_MainLightRoleColor, "_MainLightRoleColor");
            AddKeyName (ShaderManager._MainLightDir1, "_MainLightDir1");
            AddKeyName (_RimLightColor, "_RimLightColor");
            AddKeyName (_WaterLightDir, "_WaterLightDir");
            AddKeyName (shadowProjDir, "shadowProjDir");
            AddKeyName (invSin, "invSin");
            AddKeyName (_GlobalLightParam, "_GlobalLightParam");

            AddKeyName (_DyanmicPointLightPos, "_DyanmicPointLightPos");
            AddKeyName (_DyanmicPointLightColor, "_DyanmicPointLightColor");
            AddKeyName (_DyanmicPointLightParam, "_DyanmicPointLightParam");

            AddKeyName (_GameViewWorldSpaceCameraPos, "_GameViewWorldSpaceCameraPos");
#endif
        }

        public static void OnDrawGizmos (EngineContext context, EnvModify envModify, EnvSetting src)
        {
            LightingModify modify = envModify as LightingModify;

            //if (modify.settings.debugLight)
            //{
            //    ref var lc = ref context.staticLightContext;
            //    //var pos = new Vector3 (
            //    //    lc.lightRTSize + lc.worldOffset.x,
            //    //    (lc.height.x + lc.height.y) * 0.5f,
            //    //    lc.lightRTSize + lc.worldOffset.y);
            //    Gizmos.color = Color.yellow;
            //    Gizmos.DrawWireCube (context.lightingClipBox, new Vector3 (lc.lightRTSize * 2, 4, lc.lightRTSize * 2));
            //}
            if (Lighting.drawMainLightHandlerMode == Lighting.DrawMainLightHandlerMode.Disable)
            {
                Color temp = Handles.color;
                if (SceneView.lastActiveSceneView != null &&
                    SceneView.lastActiveSceneView.camera != null &&
                    showLightGizmos != null &&
                    showLightGizmos.Value &&
                    context != null)
                {
                    Color roleColor = modify.settings.roleLightColor;
                    roleColor.a = 1;
                    Vector3 lightDir = -EngineContext.roleLightDir;
                    Transform t = SceneView.lastActiveSceneView.camera.transform;
                    Vector3 pos = t.position + t.forward * 10;
                    RuntimeUtilities.DrawLightHandle (t, ref pos, ref lightDir, ref roleColor, -4, 3, modify.settings.mainLightInfo, "MainLight");
                }
                Handles.color = temp;
            }
        }

        private void SetLights(List<LightingInfo> lights,
            ref LightContext lc,ref NativeListWrap<LightingInfo> li)
        {
            for (int i = 0; i < lights.Count; ++i)
            {
                var l = lights[i];
                if (lc.lightCount < maxLightCount)
                {
                    li.Set(lc.lightCount, l);
                    lc.lightCount++;
                }
            }

        }
        private void FindLights (EngineContext context)
        {
            bool fpsCheck = !EngineContext.IsRunning && (int)(Time.time * updateFps.Value) != (int)((Time.time - Time.deltaTime) * updateFps.Value);
            if (fpsCheck)
            {
                ref var lc = ref context.staticLightContext;
                ref var li = ref lc.lightInfo;

                lc.lightCount = 0;
                if (li.Length == 0)
                {
                    li.Create(maxLightCount, "EditorLightInfo");
                }
                for (int i = 0; i < context.dynamicLightCount; ++i)
                {
                    ref var posRange = ref context.dynamicLightPos[i];
                    ref var colorInvRange = ref context.dynamicLightColor[i];
                    ref var param = ref context.dynamicLightParam[i];
                    li.Set(i, new LightingInfo()
                    {
                        posCoverage = posRange,
                        color = colorInvRange,
                        param = param,
                    });
                    lc.lightCount++;
                }

                if (uiSceneLights != null)
                {
                    SetLights(uiSceneLights.lights, ref lc, ref li);
                }
                else if (envSceneLights != null)
                {
                    SetLights(envSceneLights.lights, ref lc, ref li);
                }
                else
                {
                    var lightGroup = LightGroup.lightGroup;
                    if (lightGroup != null)
                    {
                        for (int i = 0; i < lightGroup.staticLights.Count; ++i)
                        {
                            var ld = lightGroup.staticLights[i];
                            if (lc.lightCount < maxLightCount&&ld != null && ld.IsValid)
                            {
                                var posRange = ld.GetPosRange();
                                if (EngineUtility.IsSphereInBox(ref context.lightingClipBox,
                                            ref posRange))
                                {
                                    li.Set(lc.lightCount, ld.GetLigthInfo());
                                    lc.lightCount++;
                                }
                            }
                        }
                    }
                }
            }
        }
#endif
        public override void Start (EngineContext context, IRenderContext renderContext)
        {
            //context.renderflag.SetFlag (EngineContext.RFlag_SimpleLightDirty, true);
            Shader.SetGlobalVector(_StaticLightParam, Vector4.zero);
            Vector4 lightParam = new Vector4(
                                EngineContext.LightGridSize,
                                0,
                                0,
                                0);
            Shader.SetGlobalVector(_VoxelLightParam, lightParam);

        }

        private void UpdateRoleLightDir (EngineContext context, ref Vector3 dir)
        {

            if (context.roleLightMode == RoleLightMode.WorldSpace)
            {
                ref Vector4 lightParam = ref settings.lightParam.value;
                Quaternion rotY = Quaternion.Euler(0, lightParam.w, 0);
                dir = rotY * dir;
                float minSin = (float)System.Math.Sin(lightParam.x * Mathf.Deg2Rad);
                float maxSin = (float)System.Math.Sin(lightParam.y * Mathf.Deg2Rad);
                dir.y = Mathf.Clamp(dir.y, minSin, maxSin);
                dir = Vector3.Normalize(dir);
                EngineContext.roleLightDir = -dir;
                SetShaderValueV3(ShaderManager._MainLightDir1, ref dir);
                UpdateRoleShadowDir(context);
            }
            //switch (lightMode)
            //{

            //        break;
            //    case RoleLightMode.CameraSpace:
            //        {
            //            ref Vector4 rolelightParam = ref settings.roleLightDirParam.value;
            //            var cameraForward = context.CameraTransCache.forward;
            //            Quaternion rotY = Quaternion.Euler(0, rolelightParam.z, 0);
            //            var f = rotY * cameraForward;
            //            f.y = (float)System.Math.Sin(rolelightParam.w * Mathf.Deg2Rad);
            //            f = Vector3.Normalize(f);
            //            context.roleLightDir = -f;
            //            SetShaderValueV3(ShaderManager._MainLightDir1, ref f);
            //        }
            //        break;
            //    case RoleLightMode.RoleSpace:
            //        break;
            //}

        }
       
        private void UpdateRoleShadowDir (EngineContext context)
        {
            ref Vector4 roleFaceShaodowDirParam = ref settings.roleLightDirParam.value;
            roleFaceShaodowDirParam.w = context.renderflag.HasFlag(EngineContext.RFlag_UISceneShadow) ? 1 : 0;
            SetShaderValue(_DepthShadow, ref roleFaceShaodowDirParam);
        }

        private void UpdateLightParam (EngineContext context, RenderContext rc)
        {
#if !PIPELINE_URP
            
            if (BeginUpdate ())
            {
                //main light
                LightingParam mainLight = settings.mainLightInfo;

                Vector3 dir = mainLight.value.lightDir;
                EngineContext.mainLightDir = dir;

                //shadow
                context.shadowProjDir = -settings.mainLightInfo.value.lightDir;
#if UNITY_EDITOR
                AddDumpParam (shadowProjDir, context.shadowProjDir);
#endif
                if (!context.renderflag.HasFlag(EngineContext.RFlag_DisableCalcCaluceShadow))
                {
                    float delta = (context.shadowProjDir - context.lastShadowProjDir).sqrMagnitude;
                    if (delta >= 0.01f)
                    {
                        context.renderflag.SetFlag(EngineContext.RFlag_ShadowDirty, true);
                        if (context.shadowUpdateTime < 0)
                        {
                            context.shadowUpdateTime = EngineContext.MinShadowUpdateTime;
                        }
                    }
                }


                context.invSin = context.shadowProjDir.y != 0 ? Mathf.Abs (1 / context.shadowProjDir.y) : 100000;
#if UNITY_EDITOR
                AddDumpParam (invSin, context.invSin);
#endif
                var lightDir = new Vector4(dir.x, dir.y, dir.z, context.invSin);
                SetShaderValue (_MainLightDir0, lightDir);
                if (!context.renderflag.HasFlag(EngineContext.RFlag_DisableCalcCaluceShadow))
                    SetShaderValue(_ShadowDir, lightDir);

                float intensity = mainLight.value.lightDir.w;
                Vector4 lightColorIntensity = new Vector4 (
                    mainLight.value.lightColor.r * intensity,
                    mainLight.value.lightColor.g * intensity,
                    mainLight.value.lightColor.b * intensity, intensity);
                SetShaderValue (_MainLightColor0, ref lightColorIntensity);

                //role light

                UpdateRoleLightDir (context, ref dir);
                ref Vector4 lightParam = ref settings.lightParam.value;
                RuntimeUtilities.SetLightInfo (settings.addLightInfo, _AddLightDir0, _AddLightColor0, lightParam.z);
                SetShaderValue (_MainLightRoleColor, ref settings.roleLightColor.value);
                SetShaderValue (_MainLightRoleColorV2, ref settings.roleLightColorV2.value);

                context.lightIntensityScale = settings.lightParam1.value.x;

                SetShaderValue (_GlobalLightParam, ref settings.lightParam1.value);

                //other light
                SetShaderValue (_RimLightColor, ref settings.rimLightColor.value);

                SetShaderValue(_WaterLightDir, ref settings.waterLightInfo.value.lightDir);

                EndUpdate();
            }
            VoxelLightingSystem.UpdateRoleLightDir(context, context.roleLightMode, ref EngineContext.roleLightDir);
            if (context.CameraTransCache != null)
            {
                SetShaderValue(_GameViewWorldSpaceCameraPos, context.CameraTransCache.position);
            }
#endif
        }

        private void UpdateVoxelLight (EngineContext context, RenderContext rc,
            ref LightContext lc)
        {
            if (lc.lightInfo.Length > 0)
            {
                var cmd = rc.preWorkingCmd;
                var cs = rc.resources.shaders.lightCulling;
                if (cs != null)
                {
                    if (lightKernel == -1)
                        lightKernel = cs.FindKernel ("VoxelLightIndexCS");
                    if (lightKernel != -1)
                    {
                        if (Application.platform == RuntimePlatform.Android) { }
                        else
                        {
                            Vector4 lightParam = new Vector4 (
                                EngineContext.LightGridSize,
                                0,
                                0,
                                0);
                            cmd.SetComputeVectorParam (cs, _VoxelLightParam, lightParam);
                            Shader.SetGlobalVector (_VoxelLightParam, lightParam);

                            Vector4 param = new Vector4 (
                                lc.lightCount,
                                lc.worldOffset.x,
                                lc.worldOffset.y,
                                0.5f / lc.lightRTSize);
                            cmd.SetComputeVectorParam (cs, lc.paramShaderID, param);
                            Shader.SetGlobalVector (lc.paramShaderID, param);

                            if (lc.lightInfoCb == null || lc.lightInfoCb.count != lc.lightInfo.Length)
                            {
                                lc.lightInfoCb = new ComputeBuffer (lc.lightInfo.Length, LightingInfo.GetSize ());
                            }
                            lc.lightInfo.CopyTo (lc.lightInfoCb);
                            cmd.SetComputeBufferParam (cs, lightKernel, _LightInfos, lc.lightInfoCb);
                            Shader.SetGlobalBuffer (lc.lightInfoShaderID, lc.lightInfoCb);

                            cmd.SetComputeTextureParam (cs, lightKernel, _Output, lc.rt);
                            Shader.SetGlobalTexture (lc.rtID, lc.rt);
                            int dispatchCount = lc.lightRTSize / 8;
                            cmd.DispatchCompute (cs, lightKernel, dispatchCount, dispatchCount, 1);
                        }
                    }
                }
            }
            else
            {
                Vector4 param = new Vector4 (
                    lc.lightInfo.Length,
                    lc.worldOffset.x,
                    lc.worldOffset.y,
                    0);
                Shader.SetGlobalVector (lc.paramShaderID, param);
            }
        }

        private void UpdateDynamicLight (EngineContext context, RenderContext rc)
        {
#if UNITY_EDITOR
            if (!EngineContext.IsRunning)
            {
                var qs = QualitySettingData.current;
                if (qs.flag.HasFlag (QualitySet.Flag_EnableDynamicLight))
                {
                    context.renderflag.SetFlag (EngineContext.RFlag_DynamicLightDirty, true);
                    FindLights (context);
                }
                //if (settings.debugLight && rc.debugCmd != null)
                //{
                //    Mesh mesh = AssetsConfig.instance.lightIndex;
                //    Material mat = AssetsConfig.instance.DrawLightIndex;
                //    if (mesh != null && mat != null)
                //    {
                //        ref var lc = ref context.staticLightContext;
                //        var pos = new Vector3 (lc.worldOffset.x,
                //            context.y,
                //            lc.worldOffset.y);
                //        Matrix4x4 matrix = Matrix4x4.TRS (pos, Quaternion.identity, Vector3.one);
                //        rc.debugCmd.DrawMesh (mesh, matrix, mat, 0, 0);
                //    }
                //}
                context.renderflag.SetFlag (EngineContext.RFlag_SimpleLightDirty, true);
            }
            if(forceUpdateVoxelLight)
            {
                context.renderflag.SetFlag(EngineContext.RFlag_DynamicLightDirty, true);
            }

#endif
            //if (context.simpleLightCount > 0)
            //{
            //    SetShaderValue(_DyanmicPointLightPos, context.dynamicLightPos);
            //    SetShaderValue(_DyanmicPointLightColor, context.dynamicLightColor);
            //    SetShaderValue(_DyanmicPointLightParam, context.dynamicLightParam);
            //    SetShaderValue(ShaderManager._SimpleLightParam,
            //        new Vector4(context.lightIntensityScale * 2, 0, 0, 0));
            //}
            //else
            //{
            //    SetShaderValue(ShaderManager._SimpleLightParam, Vector4.zero);
            //}
            if (context.renderflag.HasFlag (EngineContext.RFlag_DynamicLightDirty))
            {
                context.renderflag.SetFlag (EngineContext.RFlag_DynamicLightDirty, false);
#if UNITY_IOS || UNITY_EDITOR && !UNITY_ANDROID
                ref var lc = ref context.staticLightContext;
                if (lc.lightRTSize > 0)
                {
                    lc.rt = rc.GetBakeLightRT(lc.lightRTSize, lc.lightRTSize);
                    lc.paramShaderID = _StaticLightParam;
                    lc.lightInfoShaderID = ShaderManager._StaticLightInfos;
                    lc.rtID = _StaticLightTex;
                    UpdateVoxelLight(context, rc, ref lc);
                }
#endif

            }
        }

        public override void Update (EngineContext context, IRenderContext renderContext)
        {
            var rc = renderContext as RenderContext;
#if UNITY_EDITOR
            UpdateDebug();
#endif
            UpdateLightParam (context, rc);
            UpdateDynamicLight (context, rc);
        }
        public override bool OverrideSetting (EngineContext context, IGetEnvValue getEnvValue)
        {
            int effectType = (int) getEnvValue.GetValue (-1);
            if (effectType == EnvSetting.EnvEffect_Lighting_Angle)
            {
                ref Vector4 param = ref settings.lightParam.value;
                param.x = getEnvValue.GetValue (EnvSetting.EnvEffect_Lighting_MinAngle);
                param.y = getEnvValue.GetValue (EnvSetting.EnvEffect_Lighting_MaxAngle);
                param.w = getEnvValue.GetValue (EnvSetting.EnvEffect_Lighting_LightRotOffset);
                LightingParam mainLight = settings.mainLightInfo;
                Vector3 dir = mainLight.value.lightDir;
                context.roleLightRot.x = param.w;
                context.roleLightRot.y = param.y;
                UpdateRoleLightDir (context, ref dir);
                settings.lightParam.overrideState = true;
                return true;
            }
            else if (effectType == EnvSetting.EnvEffect_Lighting_RoleLightColor)
            {
                SetColor (getEnvValue, settings.roleLightColor, _MainLightRoleColor);
                SetColor (getEnvValue, settings.roleLightColorV2, _MainLightRoleColorV2);
                return true;
            }
            else if (effectType == EnvSetting.EnvEffect_Lighting_RimLightColor)
            {
                SetColor (getEnvValue, settings.rimLightColor, _RimLightColor);
                return true;
            }
            return false;
        }

        public override bool RecoverSetting (EngineContext context, IGetEnvValue getEnvValue)
        {
            settings.lightParam.ResetOverride ();
            settings.roleLightColor.ResetOverride ();
            settings.roleLightColorV2.ResetOverride ();
            settings.rimLightColor.ResetOverride ();
            if (getEnvValue != null)
            {
                int effectType = (int) getEnvValue.GetValue (-1);
                if (effectType == EnvSetting.EnvEffect_Lighting_Angle)
                {
                    settings.lightParam.ResetOverride ();
                    context.roleLightMode = RoleLightMode.WorldSpace;
                    return true;
                }
                else if (effectType == EnvSetting.EnvEffect_Lighting_RoleLightColor)
                {
                    settings.roleLightColor.ResetOverride ();
                    settings.roleLightColorV2.ResetOverride ();
                    return true;
                }
                else if (effectType == EnvSetting.EnvEffect_Lighting_RimLightColor)
                {
                    settings.rimLightColor.ResetOverride ();
                    settings.roleLightColorV2.ResetOverride ();
                    return true;
                }
            }
            else
            {
                settings.lightParam.ResetOverride ();
                settings.roleLightColor.ResetOverride ();
                settings.rimLightColor.ResetOverride ();
            }
            return false;
        }
        public override void Release (EngineContext context, IRenderContext renderContext)
        {
            base.Release (context, renderContext);

#if UNITY_EDITOR
            if (!EngineContext.IsRunning)
            {
                context.staticLightContext.Reset ();
            }
#endif
        }
    }
}