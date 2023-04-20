using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine
{
    [Serializable]
    [Env (typeof (BloomModify), "Env/Bloom")]
    public sealed class Bloom : EnvSetting
    {
        #region Parameters

        [CFParam4("High,Medium,Low", 3, 0, 3, -1, C4DataType.IntRange,
          "", 64, 1, 64, -1, C4DataType.None,
          "", 128, 1, 128, -1, C4DataType.None,
          "", 4, 1, 8, -1, C4DataType.None), CFTooltip("CSM Param.")]
        public Vector4Param _QualitySet = new Vector4Param { value = new Vector4(3, 32, 64, 4) };

        [CFParam4 ("Threshold", 0.65f, 0, 1, -1, C4DataType.FloatRange,
            "Intensity", 2f, 0, 5, -1, C4DataType.FloatRange,
            "Range", 1, 0.5f, 2, -1, C4DataType.None,
            "", 0, 0, 1, -1, C4DataType.None)]
        public Vector4Param bloomParam = new Vector4Param { value = new Vector4 (0.65f, 1.06f, 0, 0) };

        public static readonly Vector4 defaultBlend = new Vector4 (0.24f, 0.24f, 0.28f, 0.225f);
        public static readonly Vector4 defaultBlend1 = new Vector4(0.3f, 0.3f, 0.35f, 0f);
        public static readonly Vector4 defaultBlend2 = new Vector4(0.5f, 0.5f, 0f, 0f);
        public static readonly Vector4 defaultBlend3 = new Vector4(1f, 0f, 0f, 0f);
        [CFColorUsage (false, true, 1, 1, 1, 1)]
        public ColorParam color = new ColorParam { value = Color.white };


        public float Range => bloomParam.value.z;

        #endregion

        public override bool IsEnabledAndSupported ()
        {
            return bloomParam.value.y > 0f &&
                color.value.r + color.value.g + color.value.b > 0;
        }

        public override void InitParamaters (ListObjectWrapper<ISceneObject> objects, EnvModify envModify)
        {
            InnerInitParamters (objects, envModify, "Bloom");
            CreateParam(ref _QualitySet, nameof(_QualitySet), objects, envModify);
            CreateParam (ref bloomParam, nameof (bloomParam), objects, envModify);
            CreateParam (ref color, nameof (color), objects, envModify);
        }

        public override EnvSettingType GetEnvType ()
        {
            return EnvSettingType.PPBloom;
        }

        public override void ResetEffect ()
        {
            active.value = true;
        }

        public override EnvSetting Load (CFBinaryReader reader, EngineContext context)
        {
            Bloom setting = Load<Bloom> ((int) EnvSettingType.PPBloom);
            reader.ReadVector (ref setting.bloomParam.value);
#if UNITY_EDITOR
            if (context.IsValidResVersion (RenderContext.ResVersionBloom, EngineContext.Cmp_L))
            {
                Vector4 v = Vector4.zero;
                reader.ReadVector (ref v);
            }
#endif
            reader.ReadVector (ref setting.color.value);
            return setting;
        }

#if UNITY_EDITOR
        public override void Save (BinaryWriter bw)
        {
            EditorCommon.WriteVector (bw, bloomParam.value);
            EditorCommon.WriteVector (bw, color.value);
        }
#endif
    }

    public sealed class BloomModify : EnvModify<Bloom>
    {
        private static class Uniforms
        {
            public static readonly int _BloomParam = Shader.PropertyToID ("_BloomParam");
            public static readonly int _BloomColor = Shader.PropertyToID ("_BloomColor");
            public static readonly int _Blend = Shader.PropertyToID ("_Blend");
            public static readonly int _BloomQuality = Shader.PropertyToID("_BloomQuality");
            public static readonly int _Axis = Shader.PropertyToID ("_Axis");
            public static readonly int _BloomTex = Shader.PropertyToID ("_BloomTex");
            public static readonly int _MainTex0 = Shader.PropertyToID ("_MainTex0");
            public static readonly int _MainTex1 = Shader.PropertyToID ("_MainTex1");
            public static readonly int _MainTex2 = Shader.PropertyToID ("_MainTex2");
            public static readonly int _MainTex3 = Shader.PropertyToID ("_MainTex3");
        }
        Vector4 horizontal = Vector4.zero;
        Vector4 vertical = new Vector4(0, 1, 0, 0);
      //  Vector4 _Qualityvalue = Vector4.one;
        private static class Pass
        {
            public const int DownSample = 0;
            public const int DownSampleAndFilter = 1;
            public const int Blur0 = 2;
            public const int FastDownSample = 3;
            public const int Blur1 = 4;
            public const int Blur2 = 5;
            public const int Blur3 = 6;
            public const int UpsampleH = 7;
            public const int UpsampleM = 8;
            public const int UpsampleL = 9;
        
        }

        private static class RT
        {
            public static RenderTargetHandle rt8x   = new RenderTargetHandle ("_RT_8x  ");
            public static RenderTargetHandle rt16x0 = new RenderTargetHandle ("_RT_16x_0");
            public static RenderTargetHandle rt16x1 = new RenderTargetHandle ("_RT_16x_1");
            public static RenderTargetHandle rt32x0 = new RenderTargetHandle ("_RT_32x_0");
            public static RenderTargetHandle rt32x1 = new RenderTargetHandle ("_RT_32x_1");
            public static RenderTargetHandle rt64x0 = new RenderTargetHandle ("_RT_64x_0");
            public static RenderTargetHandle rt64x1 = new RenderTargetHandle ("_RT_64x_1");
        }

#if UNITY_EDITOR
        public override void BeginDump ()
        {
            base.BeginDump ();
            AddKeyName (Uniforms._BloomParam, "_BloomParam");
            AddKeyName (Uniforms._BloomColor, "_BloomColor");
        }
#endif
        public override void DirtySetting()
        {
            base.DirtySetting();
            EngineContext context = EngineContext.instance;
            context.renderflag.SetFlag(EngineContext.RFlag_PPDirty, true);
        }
        public override void Update (EngineContext context, IRenderContext renderContext)
        {
            if (BeginUpdate ())
            {
                context.renderflag.SetFlag (EngineContext.RFlag_PPDirty, true);
                EndUpdate ();
            }
        }

        public override void Render (EngineContext context, IRenderContext renderContext)
        {
#if UNITY_EDITOR
            SetBloomLevel();
#endif
            QualityLevel bloomquality = QualitySettingData.current.bloomQuality;
            RenderContext rc = renderContext as RenderContext;
            CommandBuffer cmd = rc.ppCommand;
            PropertySheet sheet = rc.propertySheets.Get (rc.resources.shaders.bloom);

            RuntimeUtilities.BeginProfile (cmd, "Bloom");

            SetShaderValue (Uniforms._BloomParam, sheet.properties, settings.bloomParam.value);
            
            //============================== Down Sample ==============================
            int width = rc.pixelWidth >> 2;
            int height = rc.pixelHeight >> 2;
            if (!rc.stateFlag.HasFlag(RenderContext.SFlag_DofEnable))
            {
                GetRT (rc, cmd, ref RenderContext._DofRT, width, height);
                cmd.BlitFullscreenTriangle (ref rc.currentRT, ref RenderContext._DofRT.rtID, sheet, Pass.DownSample);
            }
            horizontal.x = Screen.height / (float)Screen.width;

            //============================== POT >> 0 ==============================
            // Downsample & filter
            width >>= 1;
            height >>= 1;
            GetRT (rc, cmd, ref RT.rt8x, width, height);
            GetRT (rc, cmd, ref RenderContext._BloomRT, width, height);
            SetShaderValue (Uniforms._BloomColor, sheet.properties, settings.color.value);
            cmd.BlitFullscreenTriangle (ref RenderContext._DofRT.rtID, ref RT.rt8x.rtID, sheet, Pass.DownSampleAndFilter);
            // Blur
            SetShaderValue (Uniforms._Axis, sheet.properties, horizontal);
            cmd.BlitFullscreenTriangle (ref RT.rt8x.rtID, ref RenderContext._BloomRT.rtID, sheet, Pass.Blur0);
            SetShaderValue (Uniforms._Axis, sheet.properties, vertical);
            cmd.BlitFullscreenTriangle (ref RenderContext._BloomRT.rtID, ref RT.rt8x.rtID, sheet, Pass.Blur0);
    
            if (bloomquality== QualityLevel.High)
            {
                SetShaderValue(Uniforms._Blend, sheet.properties, Bloom.defaultBlend);
                //16
                RenderBloomBlur(sheet, cmd, rc, Pass.FastDownSample, Pass.Blur1, horizontal, vertical, ref width, ref height, ref RT.rt8x, ref RT.rt16x0, ref RT.rt16x1);
                //32
                RenderBloomBlur(sheet, cmd, rc, Pass.FastDownSample, Pass.Blur2, horizontal, vertical, ref width, ref height, ref RT.rt16x0, ref RT.rt32x0, ref RT.rt32x1);
                //64
                RenderBloomBlur(sheet, cmd, rc, Pass.FastDownSample, Pass.Blur3, horizontal, vertical, ref width, ref height, ref RT.rt32x0, ref RT.rt64x0, ref RT.rt64x1);
                cmd.SetGlobalTexture(Uniforms._MainTex0, RT.rt8x.rtID);
                cmd.SetGlobalTexture(Uniforms._MainTex1, RT.rt16x0.rtID);
                cmd.SetGlobalTexture(Uniforms._MainTex2, RT.rt32x0.rtID);
                cmd.SetGlobalTexture(Uniforms._MainTex3, RT.rt64x0.rtID);
                cmd.BlitFullscreenTriangle(null, ref RenderContext._BloomRT.rtID, sheet, Pass.UpsampleH);

            }
            else if (bloomquality == QualityLevel.Medium)
            {
                SetShaderValue(Uniforms._Blend, sheet.properties, Bloom.defaultBlend1);
                //16
                RenderBloomBlur(sheet, cmd, rc, Pass.FastDownSample, Pass.Blur1, horizontal, vertical, ref width, ref height, ref RT.rt8x, ref RT.rt16x0, ref RT.rt16x1);
                //32
                RenderBloomBlur(sheet, cmd, rc, Pass.FastDownSample, Pass.Blur2, horizontal, vertical, ref width, ref height, ref RT.rt16x0, ref RT.rt32x0, ref RT.rt32x1);
                cmd.SetGlobalTexture(Uniforms._MainTex0, RT.rt8x.rtID);
                cmd.SetGlobalTexture(Uniforms._MainTex1, RT.rt16x0.rtID);
                cmd.SetGlobalTexture(Uniforms._MainTex2, RT.rt32x0.rtID);
                cmd.BlitFullscreenTriangle(null, ref RenderContext._BloomRT.rtID, sheet, Pass.UpsampleM);
            }
            else if (bloomquality == QualityLevel.Low)
            {
                cmd.SetGlobalTexture(Uniforms._MainTex0, RT.rt8x.rtID);
                cmd.BlitFullscreenTriangle(null, ref RenderContext._BloomRT.rtID, sheet, Pass.UpsampleL);
            }
     
            // ============================== Setting global prameters ==============================
            cmd.SetGlobalTexture (Uniforms._BloomTex, RenderContext._BloomRT.rtID);
            rc.stateFlag.SetFlag (RenderContext.SFlag_BloomEnable, true);

            // ============================== Clear ==============================
            if(bloomquality == QualityLevel.High)
            {
                rc.ReleaseTmpRT(cmd, ref RT.rt8x);
                rc.ReleaseTmpRT(cmd, ref RT.rt16x0);
                rc.ReleaseTmpRT(cmd, ref RT.rt16x1);
                rc.ReleaseTmpRT(cmd, ref RT.rt32x0);
                rc.ReleaseTmpRT(cmd, ref RT.rt32x1);
                rc.ReleaseTmpRT(cmd, ref RT.rt64x0);
                rc.ReleaseTmpRT(cmd, ref RT.rt64x1);
            }else if (bloomquality == QualityLevel.Medium)
            {
                rc.ReleaseTmpRT(cmd, ref RT.rt8x);
                rc.ReleaseTmpRT(cmd, ref RT.rt16x0);
                rc.ReleaseTmpRT(cmd, ref RT.rt16x1);
                rc.ReleaseTmpRT(cmd, ref RT.rt32x0);
                rc.ReleaseTmpRT(cmd, ref RT.rt32x1);
            }else 
            {
                rc.ReleaseTmpRT(cmd, ref RT.rt8x);
            }

            if (!rc.stateFlag.HasFlag(RenderContext.SFlag_DofEnable))
                rc.ReleaseTmpRT(cmd, ref RenderContext._DofRT);

#if UNITY_EDITOR
            RenderContext._BloomRT.autoRelease = true;
#endif

            RuntimeUtilities.EndProfile (cmd, "Bloom");
        }

        private void SetBloomLevel()
        {
#if UNITY_EDITOR
            if (!EngineContext.IsRunning)
            {
                if (settings._QualitySet.value.x == 3)
                {
                    QualitySettingData.current.bloomQuality = QualityLevel.High;
                }
                else if (settings._QualitySet.value.x == 2)
                {
                    QualitySettingData.current.bloomQuality = QualityLevel.Medium;
                }
                else if (settings._QualitySet.value.x == 1)
                {
                    QualitySettingData.current.bloomQuality = QualityLevel.Low;
                }
            }
#endif
        }

        private void RenderBloomBlur(
            PropertySheet sheet, 
            CommandBuffer cmd,
            RenderContext rc,
            int pass,
            int pass1,
            Vector4 horizontal,
            Vector4 vertical,
            ref int width,
            ref int height,
            ref RenderTargetHandle source,
             ref RenderTargetHandle des0,
              ref RenderTargetHandle des1
            )
        {
            width >>= 1;
            height >>= 1;
            GetRT(rc, cmd, ref des0, width, height);
            GetRT(rc, cmd, ref des1, width, height);
            // Down sample
            cmd.BlitFullscreenTriangle(ref source.rtID, ref des0.rtID, sheet, pass);
            // Blur
            SetShaderValue(Uniforms._Axis, sheet.properties, horizontal);
            cmd.BlitFullscreenTriangle(ref des0.rtID, ref des1.rtID, sheet, pass1);
            SetShaderValue(Uniforms._Axis, sheet.properties, vertical);
            cmd.BlitFullscreenTriangle(ref des1.rtID, ref des0.rtID, sheet, pass1);
        }



        private void GetRT (RenderContext rc, CommandBuffer cmd, ref RenderTargetHandle rt, int width, int height)
        {
            rc.GetTmpRT(cmd, rt.id, width, height, 0);
        }
    }
}