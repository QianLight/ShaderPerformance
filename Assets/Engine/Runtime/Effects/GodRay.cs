using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine
{
    [Serializable]
    [Env (typeof (GodRayModify), "Env/GodRay")]
    public sealed class GodRay : EnvSetting
    {
        [CFParam4 ("Use Scatter Dir", 0.8f, 0, 1, -1, C4DataType.Bool,
            "", 0, 0, 1, -1, C4DataType.None,
            "Power", 1, 0.1f, 4, -1, C4DataType.FloatRange,
            "Radius", 0.25f, 0, 5, -1, C4DataType.FloatRange), CFTooltip ("Depth Param.")]
        public Vector4Param godRayParam0 = new Vector4Param { value = new Vector4 (0.8f, 0, 1, 0.25f) };

        [CFParam4 ("Offset", 0.02f, 0, 0.1f, -1, C4DataType.FloatRange,
            "Blur Times", 0, 0, 3, -1, C4DataType.IntRange,
            "Scale", 0, 0, 2, -1, C4DataType.FloatRange,
            "Bias", 0, -1, 1, -1, C4DataType.FloatRange), CFTooltip ("Depth Param.")]
        public Vector4Param godRayParam1 = new Vector4Param { value = new Vector4 (90, 0, 0, 0) };

        [CFColorUsage (false, true, 1, 1, 1, 1), CFTooltip ("Global tint of the bloom filter.")]
        public ColorParam color = new ColorParam { value = Color.white };
#if UNITY_EDITOR
        public static SavedBool debug = new SavedBool($"{nameof(GodRay)}.{nameof(debug)}", false);
#endif

        public bool UseScatterDir => godRayParam0.value.x > 0;
        public float Power => godRayParam0.value.z;
        public float Radius => godRayParam0.value.w;
        public float Offset => godRayParam1.value.x;
        public int BlurTimes => (int)godRayParam1.value.y;
        public float Scale => godRayParam1.value.z;
        public float Bias => godRayParam1.value.w;

        public override bool IsEnabledAndSupported ()
        {
            return Scale > 0;
        }

        public override void InitParamaters (ListObjectWrapper<ISceneObject> objects, EnvModify envModify)
        {
            InnerInitParamters (objects, envModify, "GodRay");

            CreateParam (ref godRayParam0, nameof (godRayParam0), objects, envModify);
            CreateParam (ref godRayParam1, nameof (godRayParam1), objects, envModify);
            CreateParam (ref color, nameof (color), objects, envModify);
        }

#if UNITY_EDITOR
        public override void InitEditorParamaters (ListObjectWrapper<ISceneObject> objects, EnvModify envModify, bool init)
        {
        }
#endif
        public override EnvSettingType GetEnvType ()
        {
            return EnvSettingType.PPVolumLight;
        }

        public override void ResetEffect ()
        {
            active.value = true;
        }

        public override EnvSetting Load (CFBinaryReader reader, EngineContext context)
        {
            GodRay setting = Load<GodRay> ((int) EnvSettingType.PPVolumLight);
            setting.godRayParam0.value = reader.ReadVector4 ();
            setting.godRayParam1.value = reader.ReadVector4 ();
            setting.color.value = reader.ReadVector4 ();
            return setting;
        }

#if UNITY_EDITOR

        public override void Save (BinaryWriter bw)
        {
            EditorCommon.WriteVector (bw, godRayParam0.value);
            EditorCommon.WriteVector (bw, godRayParam1.value);
            EditorCommon.WriteVector (bw, color.value);
        }
#endif
    }

    public sealed class GodRayModify : EnvModify<GodRay>
    {
        public static bool hasEffect = false;
        public static float angle = Mathf.Cos (70 * Mathf.Deg2Rad); //cos(30)

        public static Vector3 sunDir;

        const int Pass_Threshold = 0;
        const int Pass_Blur = 1;
        private static readonly int _GodrayParam = Shader.PropertyToID ("_GodrayParam");
        private static readonly int _Camera2World = Shader.PropertyToID ("_Camera2World");
        private static readonly int _ViewPortLightPos = Shader.PropertyToID ("_ViewPortLightPos");
        private static readonly int _Offset = Shader.PropertyToID ("_Offset");
        private static readonly int _GodRayColor = Shader.PropertyToID ("_GodRayColor");
        public static readonly int _GodRayTex = Shader.PropertyToID ("_GodRayTex");

#if UNITY_EDITOR
        public static Vector3 debugSunPos = Vector3.zero;
        public override void BeginDump ()
        {
            base.BeginDump ();
            AddKeyName (_GodrayParam, "_GodrayParam");
            AddKeyName (_ViewPortLightPos, "_ViewPortLightPos");
        }
#endif

        public override void Start (EngineContext context, IRenderContext renderContext)
        {
            base.Start(context, renderContext);
            hasEffect = false;
        }
        public override void DirtySetting()
        {
            base.DirtySetting();
            EngineContext context = EngineContext.instance;
            context.renderflag.SetFlag(EngineContext.RFlag_PrePPDirty, true);
        }
        public override void Update (EngineContext context, IRenderContext renderContext)
        {
            RenderContext rc = renderContext as RenderContext;

            sunDir = settings.UseScatterDir ? ScatterHelper.CurrentSunDir : -context.shadowProjDir;
            sunDir.y += settings.godRayParam1.value.w;
            sunDir.Normalize();

#if UNITY_EDITOR
            debugSunPos = sunDir * 10 + context.cameraPos;
#endif
            var invLook = -context.CameraTransCache.forward;
            float lookLightAngle = EngineUtility.Dot (ref invLook, -sunDir.x, -sunDir.y, -sunDir.z);

            bool hasGodray = lookLightAngle > 0 && !context.renderflag.HasFlag(EngineContext.RFlag_CamertRT);
            if (hasEffect != hasGodray)
            {
                hasEffect = hasGodray;
                context.renderflag.SetFlag (EngineContext.RFlag_PrePPDirty, true);
            }
            rc.flag.SetFlag (RenderContext.Flag_GodrayEnable, hasEffect);
            if (hasEffect)
            {
                Matrix4x4 camera2World = context.CameraRef.cameraToWorldMatrix;
                Vector3 viewPortLightPos = context.CameraRef.WorldToViewportPoint (sunDir * 1e8f);
                Shader.SetGlobalMatrix (_Camera2World, camera2World);
                SetShaderValueV3 (_ViewPortLightPos, ref viewPortLightPos);
            }
        }

        public override void Render (EngineContext context, IRenderContext renderContext)
        {
            var rc = renderContext as RenderContext;
            if (hasEffect)
            {

                var cmd = rc.prePPCommand;
                var sheet = rc.propertySheets.Get (rc.resources.shaders.godRay);
                RuntimeUtilities.BeginProfile (cmd, "GodRay");

                SetShaderValue (_GodrayParam, sheet.properties, settings.godRayParam0.value);
                sheet.properties.SetVector(_GodRayColor, settings.color.value);

                ref var rt0 = ref RenderContext._TmpQuarterRT0;
                rc.GetTmpRT (cmd, ref rt0, RenderContext.RTSize_Quarter);

                cmd.BlitFullscreenTriangle (ref rc.currentRT, ref rt0.rtID, sheet, Pass_Threshold);
                rc.DebugRT ("GodRay/Threshold", ref rt0.rtID, cmd);
                ref var godRayParam1 = ref settings.godRayParam1.value;
                int count = settings.BlurTimes;
                if (count > 0)
                {
                    ref var rt1 = ref RenderContext._TmpQuarterRT1;
                    rc.GetTmpRT (cmd, ref rt1, RenderContext.RTSize_Quarter);
                    for (int i = 0; i < count; i++)
                    {
                        float offset = settings.Offset * (i * 2 + 1);
                        sheet.properties.SetVector (_Offset, new Vector4 (offset, offset, 0, 0));
                        cmd.BlitFullscreenTriangle (ref rt0.rtID, ref rt1.rtID, sheet, Pass_Blur);

                        offset = settings.Offset * (i * 2 + 2);
                        sheet.properties.SetVector (_Offset, new Vector4 (offset, offset, 0, 0));
                        cmd.BlitFullscreenTriangle (ref rt1.rtID, ref rt0.rtID, sheet, Pass_Blur);
                    }
                    rc.ReleaseTmpRT (cmd, ref rt1);
                }

                cmd.SetGlobalTexture (_GodRayTex, rt0.id);
                rc.stateFlag.SetFlag (RenderContext.SFlag_GodRayEnable, true);

#if UNITY_EDITOR
                RenderContext._TmpQuarterRT0.autoRelease = true;
#endif
                RuntimeUtilities.EndProfile (cmd, "GodRay");
            }
        }

        public override void Release (EngineContext context, IRenderContext renderContext)
        {
            hasEffect = false;
        }
    }
}