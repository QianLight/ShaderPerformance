using System;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

namespace CFEngine
{
    [Serializable]
    [Env (typeof (MotionBlurModify), "Env/Motion Blur", false)]
    public sealed class MotionBlur : EnvSetting
    {
        [CFParam4 ("blurDiverge", 1, 0, 3, -1, C4DataType.FloatRange,
            "intensity", 0.2f, 1, 100f, -1, C4DataType.FloatRange,
            "", 2, 0, 10, -1, C4DataType.None,
            "blurByCamera", 0, 0, 1, -1, C4DataType.Bool)]
        public Vector4Param param = new Vector4Param () { value = new Vector4 (1, 20, 2, 0) };

        public override void InitParamaters (ListObjectWrapper<ISceneObject> objects, EnvModify envModify)
        {
            InnerInitParamters (objects, envModify, "MotionBlur");
            CreateParam (ref param, nameof (param), objects, envModify);
        }

        public override EnvSettingType GetEnvType ()
        {
            return EnvSettingType.PPMotionBlur;
        }

        public override void ResetEffect ()
        {
            active.value = false;
        }

        // public static void Creator (out EnvSetting setting, out EnvModify modify, bool createModify)
        // {
        //     setting = Create<MotionBlur> ();
        //     if (createModify)
        //         modify = CFAllocator.Allocate<MotionBlurModify> ();
        //     else
        //         modify = null;
        // }

        public override EnvSetting Load (CFBinaryReader reader, EngineContext context)
        {
            MotionBlur setting = Load<MotionBlur> ((int) EnvSettingType.PPMotionBlur);
#if UNITY_EDITOR
            if (context.IsValidResVersion (RenderContext.ResVersionTexCurve, EngineContext.Cmp_L))

            {
                reader.ReadVector (ref setting.param.value);
            }
#endif
            return setting;
        }

#if UNITY_EDITOR
        public override void Save (BinaryWriter bw)
        { }
#endif
    }

    public sealed class MotionBlurModify : EnvModify<MotionBlur>, IPreEffect
    {
        private int motionState = Effect_Disable;
        private Matrix4x4 previousViewProjectionMatrix;
        public static readonly int _GhostBlurParam = Shader.PropertyToID ("_GhostBlurParam");
        public static readonly int _CurrentVPInverse = Shader.PropertyToID ("_CurrentVPInverse");
        public static readonly int _PreviousVP = Shader.PropertyToID ("_PreviousVP");
        public override void Start (EngineContext context, IRenderContext renderContext)
        {
            motionState = Effect_Disable;
        }

        public override void Release (EngineContext context, IRenderContext renderContext)
        {
            motionState = Effect_Disable;
        }
        public void PostRender (EngineContext engineContext, RenderContext context, CommandBuffer cmd) { }
        public override bool OverrideSetting (EngineContext context, IGetEnvValue getEnvValue)
        {
            ref Vector4 param = ref settings.param.value;
            param.x = getEnvValue.GetValue (EnvSetting.EnvEffect_MotionBlur_BlurDiverge);
            param.y = getEnvValue.GetValue (EnvSetting.EnvEffect_MotionBlur_Intensity);
            param.w = getEnvValue.GetValue (EnvSetting.EnvEffect_MotionBlur_BlurByCamera);
            if(param.w > 0.5f)
            {
                var camera = context.CameraRef;
                previousViewProjectionMatrix = camera.projectionMatrix * camera.worldToCameraMatrix;
            }
            settings.param.overrideState = true;
            motionState = Effect_Running;
            context.renderflag.SetFlag (EngineContext.RFlag_PrePPDirty, true);
            return true;
        }

        public override bool RecoverSetting (EngineContext context, IGetEnvValue getEnvValue)
        {
            settings.param.overrideState = false;
            settings.active.value = false;
            motionState = Effect_Disable;
            context.renderflag.SetFlag (EngineContext.RFlag_PrePPDirty, true);
            return true;
        }
#if UNITY_EDITOR
        public override void EditorDirty()
        {
            motionState = Effect_Init;
        }
#endif
        public override void DirtySetting()
        {
            base.DirtySetting();
            EngineContext context = EngineContext.instance;
            context.renderflag.SetFlag(EngineContext.RFlag_PrePPDirty, true);
        }
        public void Render (EngineContext engineContext, RenderContext context, CommandBuffer cmd, PropertySheet sheet)
        {
            Vector4 param = new Vector4 ();
            Vector4 shakeParam = settings.param;
            Blur (engineContext, ref shakeParam, ref param, sheet);
            sheet.properties.SetVector(_GhostBlurParam, param);
            if (motionState == Effect_Disable)
            {
                PreEffect.EnablePreffect(PreEffect.PreEffect_MotionBlur, 0);
                PreEffect.EnablePreffect(PreEffect.PreEffect_GhostBlur, 0);
            }
            engineContext.renderflag.SetFlag(EngineContext.RFlag_PrePPDirty, true);
        }

        private void UpdateShakeBlur(EngineContext engineContext, float blurDiverge, float intensity, ref float velocityX, ref float velocityY)
        {
            Vector2 pos = UnityEngine.Random.insideUnitCircle * intensity * 0.05f;
            velocityX = pos.x * blurDiverge;
            velocityY = pos.y * blurDiverge;
            PreEffect.EnablePreffect(PreEffect.PreEffect_GhostBlur);
        }

        private void UpdateMotionBlur(EngineContext engineContext, float intensity, ref float blurSize, PropertySheet sheet)
        {
            blurSize = intensity * 0.01f; //blur size
            var camera = engineContext.CameraRef;
            Matrix4x4 currentViewProjectionMatrix = camera.projectionMatrix * camera.worldToCameraMatrix;
            Matrix4x4 currentViewProjectionInverseMatrix = currentViewProjectionMatrix.inverse;

            sheet.properties.SetMatrix (_CurrentVPInverse, currentViewProjectionInverseMatrix);
            sheet.properties.SetMatrix (_PreviousVP, previousViewProjectionMatrix);
            previousViewProjectionMatrix = currentViewProjectionMatrix;
            PreEffect.EnablePreffect (PreEffect.PreEffect_MotionBlur);
        }

        private void Blur (EngineContext engineContext, ref Vector4 shakeParam, ref Vector4 param, PropertySheet sheet)
        {
            bool shakeBlur = shakeParam.w < 0.5f;
            if (shakeBlur)
            {
                UpdateShakeBlur (engineContext, shakeParam.x, shakeParam.y, ref param.x, ref param.y);
            }
            else
            {
                UpdateMotionBlur(engineContext, shakeParam.y, ref param.x, sheet);
            }
        }

    }
}