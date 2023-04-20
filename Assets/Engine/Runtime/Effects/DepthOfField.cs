using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine
{
    [Serializable]
    [Env (typeof (DepthOfFieldModify), "Env/Depth of Field")]
    public sealed class DepthOfField : EnvSetting
    {
        [CFParam4 ("EasyMode", 1, 0, 1, -1, C4DataType.Bool,
            "Focus Distance", 5, 0, 40f, -1, C4DataType.Float,
            "Bokeh Range Near", 5, 0.1f, 100f, -1, C4DataType.Float,
            "Focus Range Far", 30, 0.1f, 1000f, -1, C4DataType.Float)]
        public Vector4Param param = new Vector4Param { value = new Vector4 (10, 30, 1, 0) };
        [CFParam4("High,Medium,Low", 3, 0, 3, -1, C4DataType.IntRange,
         "", 64, 1, 64, -1, C4DataType.None,
         "", 128, 1, 128, -1, C4DataType.None,
         "", 4, 1, 8, -1, C4DataType.None), CFTooltip("CSM Param.")]
        public Vector4Param _QualitySet = new Vector4Param { value = new Vector4(3, 32, 64, 4) };



#if UNITY_EDITOR
        public static SavedBool showFocusPlane = new SavedBool($"{nameof(DepthOfField)}.{nameof(showFocusPlane)}", false);
#endif

        public override void InitParamaters (ListObjectWrapper<ISceneObject> objects, EnvModify envModify)
        {
            InnerInitParamters (objects, envModify, "DepthOfField");
            CreateParam(ref param, nameof(param), objects, envModify);
            CreateParam (ref _QualitySet, nameof (_QualitySet), objects, envModify);
        }
        
        public override void ResetEffect ()
        {
            active.value = false;
        }
#if UNITY_EDITOR
        public override void InitEditorParamaters (ListObjectWrapper<ISceneObject> objects, EnvModify envModify, bool init)
        {
        }
#endif

        public override EnvSettingType GetEnvType ()
        {
            return EnvSettingType.PPDepthOfField;
        }

        public override EnvSetting Load (CFBinaryReader reader, EngineContext context)
        {
            DepthOfField setting = Load<DepthOfField> ((int) EnvSettingType.PPDepthOfField);
#if UNITY_EDITOR
            if (!context.IsValidResVersion (RenderContext.ResVersionDof, EngineContext.Cmp_GE))
            {
                Vector4 tmp = default;
                reader.ReadByte ();
                reader.ReadVector (ref tmp);
                reader.ReadVector (ref tmp);
                reader.ReadVector (ref tmp);
            }
            else
#endif
            {
                reader.ReadVector (ref param.value);
            }

            return setting;
        }

#if UNITY_EDITOR
        public override void Save (BinaryWriter bw)
        {
            EditorCommon.WriteVector (bw, param.value);
        }
#endif
    }

    public sealed class DepthOfFieldModify : EnvModify<DepthOfField>
    {
        private static class Uniforms
        {
            public static readonly int _DofParams = Shader.PropertyToID ("_DofParams");
            public static readonly int _DofTex = Shader.PropertyToID ("_DofTex");
            public static readonly int _Kernel = Shader.PropertyToID ("kernel");
        }

        private static class Pass
        {
            public const int preFilterPass = 0;
            public const int bokehPass = 1;
            public const int postFilterPass = 2;
        }

        private static class TempRT
        {
            public static RenderTargetHandle downSampleX4 = new RenderTargetHandle ("_RT_DOF_TEMP");
        }

#if UNITY_EDITOR
        
        private static void DrawFocusPlane (ref Vector3 focus, ref Vector3 right, float size)
        {
            Vector3 p0 = focus - right * size - Vector3.up * size;
            Vector3 p1 = focus + right * size - Vector3.up * size;
            Vector3 p2 = focus + right * size + Vector3.up * size;
            Vector3 p3 = focus - right * size + Vector3.up * size;
            Gizmos.DrawLine (p0, p1);
            Gizmos.DrawLine (p1, p2);
            Gizmos.DrawLine (p2, p3);
            Gizmos.DrawLine (p3, p0);
        }

        public static void OnDrawGizmos (EngineContext context, EnvModify envModify, EnvSetting src)
        {
            DepthOfField setting = src as DepthOfField;
            if (setting != null && DepthOfField.showFocusPlane.Value && context.CameraTransCache != null)
            {
                ref var param = ref setting.param.value;
                var cameraPos = context.CameraTransCache.position;
                var forward = context.CameraTransCache.forward;
                float farStart = param.x - param.y;
                float farEnd = param.y + param.y;

                Vector3 right = Vector3.Cross (Vector3.up, forward);
                Gizmos.color = Color.yellow;
                var near = cameraPos + forward * farStart;
                DrawFocusPlane (ref near, ref right, farStart);
                Gizmos.color = Color.blue;
                var far = cameraPos + forward * farEnd;
                DrawFocusPlane (ref far, ref right, farEnd);
            }
        }

        public override void BeginDump ()
        {
            base.BeginDump ();
            AddKeyName (Uniforms._DofParams, "_DofParams");
        }
#endif
        public override void DirtySetting()
        {
            base.DirtySetting();
            EngineContext context = EngineContext.instance;
            context.renderflag.SetFlag(EngineContext.RFlag_PrePPDirty, true);
            context.renderflag.SetFlag(EngineContext.RFlag_PPDirty, true);
        }

        public override void Update (EngineContext context, IRenderContext renderContext)
        {
            if (BeginUpdate ())
            {
                context.renderflag.SetFlag (EngineContext.RFlag_PrePPDirty, true);
                EndUpdate ();
            }
        }

        private void GetRT (RenderContext rc, CommandBuffer cmd, ref RenderTargetHandle rt, int width, int height)
        {
            rc.GetTmpRT (cmd, rt.id, width, height, 0, RenderTextureFormat.ARGBHalf);
        }

        public override void Render (EngineContext context, IRenderContext renderContext)
        {
#if UNITY_EDITOR
            SetdofLevel();
#endif
            QualityLevel quality = QualitySettingData.current.dofQuality;
          
            RenderContext rc = renderContext as RenderContext;
            CommandBuffer cmd = rc.prePPCommand;

            RuntimeUtilities.BeginProfile (cmd, "Depth Of Field");

            //============================== Shader Enviroment ==============================
            PropertySheet sheet = rc.propertySheets.Get (rc.resources.shaders.depthOfField);
            Vector4 param = settings.param.value;
            if (quality == QualityLevel.Low)
                param.z = Mathf.Min (param.z, param.x);
            SetShaderValue (Uniforms._DofParams, sheet.properties, param);
            cmd.SetGlobalVector (Uniforms._DofParams, param);

            //============================== Getting RenderTextures ==============================
            int width = rc.pixelWidth >> 2;
            int height = rc.pixelHeight >> 2;
            GetRT (rc, cmd, ref TempRT.downSampleX4, width, height);
            GetRT (rc, cmd, ref RenderContext._DofRT, width, height);

            //============================== Drawing ==============================
            if (quality == QualityLevel.Low)
            {
                cmd.BlitFullscreenTriangle (ref rc.currentRT, ref TempRT.downSampleX4.rtID, sheet, Pass.preFilterPass);
                cmd.BlitFullscreenTriangle (ref TempRT.downSampleX4.rtID, ref RenderContext._DofRT.rtID, sheet, Pass.bokehPass);
            }
            else
            {
                cmd.BlitFullscreenTriangle (ref rc.currentRT, ref RenderContext._DofRT.rtID, sheet, Pass.preFilterPass);
                cmd.BlitFullscreenTriangle (ref RenderContext._DofRT.rtID, ref TempRT.downSampleX4.rtID, sheet, Pass.bokehPass);
                cmd.BlitFullscreenTriangle (ref TempRT.downSampleX4.rtID, ref RenderContext._DofRT.rtID, sheet, Pass.postFilterPass);
            }

            cmd.SetGlobalTexture (Uniforms._DofTex, RenderContext._DofRT.rtID);
            rc.stateFlag.SetFlag (RenderContext.SFlag_DofEnable, true);

            // ============================== Clear ==============================
            rc.ReleaseTmpRT (cmd, ref TempRT.downSampleX4);

#if UNITY_EDITOR
            RenderContext._DofRT.autoRelease = true;
#endif

            RuntimeUtilities.EndProfile (cmd, "Depth Of Field");
        }

        private void SetdofLevel()
        {
            if (settings._QualitySet.value.x == 3)
            {
                QualitySettingData.current.dofQuality = QualityLevel.High;
            }
            else if (settings._QualitySet.value.x == 2)
            {
                QualitySettingData.current.dofQuality = QualityLevel.High;
            }
            else if (settings._QualitySet.value.x == 1)
            {
                QualitySettingData.current.dofQuality = QualityLevel.Low;
            }
        }

    }
}