using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace CFEngine
{
    [Serializable]
    [Env (typeof (SceneMiscModify), "Env/SceneMisc")]
    public sealed class SceneMisc : EnvSetting
    {
        [CFParam4 ("fov", 60, 1, 179, -1, C4DataType.FloatRange,
                "near", 0.1f, 0.01f, 100, -1, C4DataType.FloatRange,
                "far", 300.0f, 0.02f, 1000, -1, C4DataType.FloatRange,
                "Projector", 0, 0, 1, -1, C4DataType.None),
            CFTooltip ("Camera Param.")
        ]
        public Vector4Param cameraParam0 = new Vector4Param { value = new Vector4 (60, 0.1f, 300.0f, 0) };

        [CFParam4 ("WindDir", 0, -180f, 180f, -1, C4DataType.FloatRange,
            "WindSpeed", 0, 0, 20f, -1, C4DataType.FloatRange,
            "WindFrequency", 1, 0, 10f, -1, C4DataType.FloatRange,
            "TreeWindSpeed", 0, 0, 20f, -1, C4DataType.FloatRange)]
        public Vector4Param windParam0 = new Vector4Param { value = new Vector4 (0, 0, 0, 0) };

        [CFParam4 ("WindDirX", 0.001f, -1f, 1f, -1, C4DataType.FloatRange,
            "WindDirZ", 0.001f, -1f, 1f, -1, C4DataType.FloatRange,
            "", 0.13f, 0, 2, -1, C4DataType.None,
            "", 0, 0, 1, -1, C4DataType.None)]
        public Vector4Param windParam1 = new Vector4Param { value = new Vector4 (0.001f, 0.001f, 0.13f, 0.1f) };

        [CFResPath (typeof (Texture2D), "", EnvBlock.ResOffset_WindMap, true)]
        public ResParam ambientWindMap = new ResParam { value = "" };

        [CFColorUsage (true, true, 1, 1, 1, 1)]
        public ColorParam sceneColor = new ColorParam { value = Color.white };

        public static uint[] argArray = new uint[5];

        public override void InitParamaters (ListObjectWrapper<ISceneObject> objects, EnvModify envModify)
        {
            InnerInitParamters (objects, envModify, "SceneMisc");
            CreateParam (ref cameraParam0, nameof (cameraParam0), objects, envModify);
            CreateParam (ref windParam0, nameof (windParam0), objects, envModify);
            CreateParam (ref windParam1, nameof (windParam1), objects, envModify);
            CreateParam (ref ambientWindMap, nameof (ambientWindMap), objects, envModify);
            CreateParam (ref sceneColor, nameof (sceneColor), objects, envModify);
        }
#if UNITY_EDITOR
        public override void InitEditorParamaters (ListObjectWrapper<ISceneObject> objects, EnvModify envModify, bool init)
        {
            if (init)
            {
                ambientWindMap.resOffset = EnvBlock.ResOffset_WindMap;
                if (string.IsNullOrEmpty (ambientWindMap.value))
                {
                    ambientWindMap.value = "AmbientWind_Noise";
                    ambientWindMap.asset = AssetDatabase.LoadAssetAtPath<Texture> ("Assets/Engine/Runtime/Res/Textures/AmbientWind_Noise.tga");
                }

                if (ambientWindMap.resType == ResObject.None)
                {
                    ambientWindMap.resType = ResObject.Tex_2D;
                }
                ambientWindMap.Init ();
            }

        }
#endif
        public override EnvSettingType GetEnvType ()
        {
            return EnvSettingType.SceneMisc;
        }
        public override void ResetEffect ()
        {
            active.value = true;
        }

        // public static void Creator (out EnvSetting setting, out EnvModify modify, bool createModify)
        // {
        //     setting = Create<SceneMisc> ();
        //     if (createModify)
        //         modify = CFAllocator.Allocate<SceneMiscModify> ();
        //     else
        //         modify = null;
        // }

        public override EnvSetting Load (CFBinaryReader reader, EngineContext context)
        {
            SceneMisc setting = Load<SceneMisc> ((int) EnvSettingType.SceneMisc);
            reader.ReadVector (ref setting.cameraParam0.value);
            reader.ReadVector (ref setting.windParam0.value);
            reader.ReadVector (ref setting.windParam1.value);
            setting.ambientWindMap.Load (reader,false);
#if UNITY_EDITOR
            if (context.IsValidResVersion (RenderContext.ResVersionWind, EngineContext.Cmp_GE) &&
                context.IsValidResVersion (RenderContext.ResVersionNewWind, EngineContext.Cmp_L))
            {
                ResParam tmp = new ResParam ();
                tmp.Load (reader);
                tmp.Load (reader);
            }
#endif
            return setting;
        }

        public override void UninitParamaters ()
        {
            base.UninitParamaters ();
            ambientWindMap.UnInit ();

        }
#if UNITY_EDITOR

        public override void Save (BinaryWriter bw)
        {
            EditorCommon.WriteVector (bw, cameraParam0.value);
            EditorCommon.WriteVector (bw, windParam0.value);
            EditorCommon.WriteVector (bw, windParam1.value);
            EditorCommon.WriteRes (bw, ambientWindMap);
        }
#endif
    }

    public sealed class SceneMiscModify : EnvModify<SceneMisc>
    {
        // private RenderTexture roleRT;
        // public static bool cameraParamDirty = true;

        public static readonly int _OutlineScale = Shader.PropertyToID ("_OutlineScale");

        public static readonly int _RoleRamp = Shader.PropertyToID ("_RoleRamp");
        public static readonly int _RimParam = Shader.PropertyToID ("_RimParam");
        public static readonly int _LightFadePram = Shader.PropertyToID ("_LightFadePram");
        public static readonly int _AmbientWindParam = Shader.PropertyToID ("_AmbientWindParam");
        public static readonly int _AmbientWindParam1 = Shader.PropertyToID ("_AmbientWindParam1");
        public static readonly int _WindParam1 = Shader.PropertyToID ("_WindParam1");
        public static readonly int _WindControl = Shader.PropertyToID ("_WindControl");
        public static readonly int _WorldSize = Shader.PropertyToID ("_WorldSize");
        public static readonly int _AmbientWind = Shader.PropertyToID ("_AmbientWind");
        // public static readonly int _WindNoiseLow = Shader.PropertyToID ("_WindNoiseLow");
        // public static readonly int _WindNoiseHigh = Shader.PropertyToID ("_WindNoiseHigh");

        private static int[] multiLayerRTID = new int[]
{
            Shader.PropertyToID ("_MultiLayer0"),
            Shader.PropertyToID ("_MultiLayer1"),
            Shader.PropertyToID ("_MultiLayer2"),
            Shader.PropertyToID ("_MultiLayer3"),
};

#if UNITY_EDITOR
        public static Transform lookAtTarget;
        private static AABB aabb;
        public static readonly int fieldOfView = Shader.PropertyToID ("fieldOfView");
        public static readonly int nearClipPlane = Shader.PropertyToID ("nearClipPlane");
        public static readonly int farClipPlane = Shader.PropertyToID ("farClipPlane");

        private static string[] multiLayerRTName = new string[]
        {
            "_MultiLayer0",
            "_MultiLayer1",
            "_MultiLayer2",
            "_MultiLayer3",
        };

        public override void BeginDump ()
        {
            base.BeginDump ();
            AddKeyName (fieldOfView, "fieldOfView");
            AddKeyName (nearClipPlane, "nearClipPlane");
            AddKeyName (farClipPlane, "farClipPlane");
            AddKeyName (_AmbientWindParam, "_AmbientWindParam");
            AddKeyName (_WindParam1, "_WindParam1");
            AddKeyName (_WindControl, "_WindControl");
            AddKeyName (_WorldSize, "_WorldSize");
            AddKeyName (ShaderManager._ShaderKeyDecalAtlas, "_DecalAtlas");
            AddKeyName (_AmbientWind, "_AmbientWind");

            AddKeyName (ShaderManager._SceneColor, "ShaderManager._SceneColor");
            AddKeyName (_OutlineScale, "_OutlineScale");
            AddKeyName (_RoleRamp, "_RoleRamp");
            AddKeyName (_RimParam, "_RimParam");
            AddKeyName (_RimParam, "_LightFadePram");
        }

        private static void TestFadeEffect (EngineContext context, Transform t)
        {
            if (t.TryGetComponent (out MeshRenderObject mro))
            {
                Bounds bounds = mro.sceneAABB;
                aabb.Init (ref bounds);
                if (EngineUtility.FastTestRayAABB (ref context.cameraPlane, context.cameraPlaneDist, ref aabb) &&
                    EngineUtility.FastTestRayAABB (ref context.cameraPos, ref context.lookAtPos, ref context.camera2Target, ref aabb, out var rayAABB))
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.white;
                }
                Gizmos.DrawWireCube (bounds.center, bounds.size);
            }
            for (int i = 0; i < t.childCount; ++i)
            {
                TestFadeEffect (context, t.GetChild (i));
            }
        }
        public static void OnDrawGizmos (EngineContext context, EnvModify envModify, EnvSetting src)
        {
            SceneMiscModify modify = envModify as SceneMiscModify;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(context.lookAtPos, 0.2f);
            Gizmos.DrawSphere (context.lookAtPos2, 0.1f);
        }

        private void UpdateRole (EngineContext context, RenderContext rc)
        {
            if (lookAtTarget != null)
            {
                Vector3 pos = lookAtTarget.position;
                context.lookAtPos.x = pos.x;
                context.lookAtPos.y = pos.y;
                context.lookAtPos.z = pos.z;
                lookAtTarget = null;
            }
            else
            {
                var forward = context.CameraTransCache.forward;
                float lookAtDistance = 6;
                context.lookAtPos.x = context.cameraPos.x + forward.x * lookAtDistance;
                context.lookAtPos.y = context.cameraPos.y + forward.y * lookAtDistance;
                context.lookAtPos.z = context.cameraPos.z + forward.z * lookAtDistance;
            }
            EngineUtility.Lerp (ref context.lookAtPos, ref context.cachePos, ref context.lookAtPos2, 0.2f);
        }

        
#endif
        public override void Start (EngineContext context, IRenderContext renderContext)
        {
            context.logicflag.SetFlag (EngineContext.Flag_CameraFovDirty, true);
        }

        public override void Update (EngineContext context, IRenderContext renderContext)
        {
            RenderContext rc = renderContext as RenderContext;
#if UNITY_EDITOR
            if (!EngineContext.IsRunning)
            {
                UpdateRole (context, rc);
                // UpdateDecal (context);
                int xx;
                int zz;
                SceneMiscSystem.GetChunkPos(context, ref context.lookAtPos, out xx, out zz);
                Vector2 pos = new Vector2(context.lookAtPos.x, context.lookAtPos.z);
                SceneRoamingSystem.UpdateChunkBlock(context, ref pos, xx, zz, context.lookAtPos.y);
                context.quad32XZ.z = context.Width / 32;
                context.quad32XZ.w = context.Height / 32;
                context.quad64XZ.z = context.Width / 64;
                context.quad64XZ.w = context.Height / 64;
                WorldSystem.UpdateCamera (context);
            }
#endif
//            if (WorldSystem.materialConfig != null && WorldSystem.materialConfig.isMatDirty)
//            {
//                WorldSystem.materialConfig.isMatDirty = false;
//#if UNITY_EDITOR
//                DrawDummyMat(context, WorldSystem.debugDummyMats);
//                DrawDummyMat(context, WorldSystem.debugEffectMats);
//#else
//                DrawDummyMat(context, WorldSystem.materialConfig.dummyMats);
//                DrawDummyMat(context, WorldSystem.materialConfig.effectMats);
//#endif
//            }

            if (BeginUpdate ())
            {
                if (context.logicflag.HasFlag (EngineContext.Flag_CameraFovDirty))
                {
                    context.CameraRef.fieldOfView = settings.cameraParam0.value.x;
                    context.CameraRef.nearClipPlane = settings.cameraParam0.value.y;
                    //context.CameraRef.farClipPlane = settings.cameraParam0.value.z;//这里会给相机FarClip二次赋值，影响Volume工作，所以注释掉了
#if UNITY_EDITOR
                    AddDumpParam (fieldOfView, context.CameraRef.fieldOfView);
                    AddDumpParam (nearClipPlane, context.CameraRef.nearClipPlane);
                    AddDumpParam (farClipPlane, context.CameraRef.farClipPlane);
#endif
                    context.logicflag.SetFlag (EngineContext.Flag_CameraFovDirty, false);
                }

                // Vector4 windParam = settings.windParam0;
                // float rot = windParam.x * Mathf.Deg2Rad;
                // SetShaderValue (_WindParam0,
                //     new Vector4 (
                //         Mathf.Cos (rot),
                //         Mathf.Sin (rot),
                //         windParam.y,
                //         windParam.z));

                // SetShaderValue (_WindParam1, new Vector4 (windParam.w, 0, 0, 0));
                ref Vector4 windParam = ref settings.windParam0.value;
                Quaternion rotY = Quaternion.Euler (0, windParam.x, 0);
                Vector3 windDir = rotY * Vector3.forward;
                SetShaderValue (_AmbientWindParam,
                    new Vector4 (
                        windDir.x, windDir.y, windDir.z,
                        windParam.y));
                ref Vector4 windParam1 = ref settings.windParam1.value;
                SetShaderValue (_AmbientWindParam1,
                    new Vector4 (
                        windParam.z, windParam.w, windParam1.x,
                        windParam1.y));
                Texture2D tex = settings.ambientWindMap.res as Texture2D;
                SetShaderValue (_AmbientWind, tex);


                SetShaderValue (_WorldSize, new Vector4 (context.Width, context.Height, 1.0f / context.Width, 1.0f / context.Height));

                // SetShaderValue (ShaderManager._ShaderKeyDecalAtlas, context.decalTexCache.texCache);
                SetShaderValue (ShaderManager._SceneColor, ref settings.sceneColor.value);

                var oc = WorldSystem.miscConfig;
                if (oc != null)
                {
                    SetShaderValue (_OutlineScale,
                        new Vector4 (oc.defaultDist.minDist, oc.defaultDist.maxDist, oc.defaultDist.minScale, oc.defaultDist.maxScale));
                    SetShaderValue (_RoleRamp,
                        new Vector4 (oc.lightFaceRampOffset, oc.darkFaceRampOffset, 0, 0));
                    SetShaderValue (_RimParam, ref oc.rimParam);
                    SetShaderValue (_LightFadePram, ref oc.lightFadePram);
                }
                EndUpdate ();
            }
            if (context.CameraRef != null)
            {
                if (context.CameraRef.farClipPlane > 2500)
                    context.CameraRef.farClipPlane = 2500;
            }
        }
        private void DrawMultiLayerObject(EngineContext context, RenderContext rc)
        {
            ref var drawCalls = ref context.multiLayerObjects;
            if (drawCalls.count > 0)
            {
                int frame = context.multiLayerFrame - 1;
                if (frame < 0)
                    frame = 3;
                //frame = 0;
                int id = multiLayerRTID[frame];
#if UNITY_EDITOR
                string name = multiLayerRTName[frame];
#else
                string name = "";
#endif
                frame += RenderContext.MultiLayerRT0;
                var rt = rc.GetMultiLayerRT(frame, id, name);
                rc.preWorkingCmd.SetRenderTarget(rt, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
                rc.preWorkingCmd.ClearRenderTarget(false, true, Color.clear);
                Matrix4x4 viewMatrix = context.CameraRef.worldToCameraMatrix;
                Matrix4x4 projMatrix = context.CameraRef.projectionMatrix;
                rc.preWorkingCmd.SetViewProjectionMatrices(viewMatrix, GL.GetGPUProjectionMatrix(projMatrix, false));
                while (drawCalls.Pop<InstanceDrawBatch>(out var db))
                {
                    rc.preWorkingCmd.DrawMeshInstancedIndirect(db.mesh, 0, db.mat, db.passID, db.argBuffer, 0, db.mpbRef);
                    SharedObjectPool<InstanceDrawBatch>.Release(db);
                }
            }  
        }

        public override void Render (EngineContext context, IRenderContext renderContext) 
        {
            DrawMultiLayerObject(context, renderContext as RenderContext);
        }

        public override bool OverrideSetting (EngineContext context, IGetEnvValue getEnvValue)
        {
            int effectType = (int) getEnvValue.GetValue (-1);
            if (effectType == EnvSetting.EnvEffect_SceneMisc_SceneColor)
            {
                //scene color
                SetColor (getEnvValue, settings.sceneColor, ShaderManager._SceneColor);
                return true;
            }
            return false;
        }

        public override bool RecoverSetting (EngineContext context, IGetEnvValue getEnvValue)
        {
            settings.sceneColor.overrideState = false;
            if (getEnvValue != null)
            {
                int effectType = (int) getEnvValue.GetValue (-1);
                if (effectType == EnvSetting.EnvEffect_SceneMisc_SceneColor)
                {
                    Vector4 one = Vector4.one;
                    SetShaderValue (ShaderManager._SceneColor, ref one);
                    return true;
                }
            }
            return false;
        }

        public override void DirtySetting ()
        {
            base.DirtySetting ();
        }

        public override void Release (EngineContext context, IRenderContext renderContext)
        {
            base.Release (context, renderContext);
#if UNITY_EDITOR
            lookAtTarget = null;
#endif
        }
    }
}