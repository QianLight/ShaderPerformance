using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace UnityEngine.Rendering.Universal
{
    [Serializable, VolumeComponentMenu("Post-processing/Depth Of Field URP")]
    public sealed class DOF : VolumeComponent, IPostProcessComponent
    {
        public BoolParameter EasyMode = new BoolParameter(false);
        public FloatParameter FocusDistance = new FloatParameter(5);
        public FloatParameter BokehRangeNear = new FloatParameter(5);
        public FloatParameter FocusRangeFar = new FloatParameter(30);
        public ClampedFloatParameter BlurRadius = new ClampedFloatParameter(1, 0, 10);
        public ClampedFloatParameter Intensity = new ClampedFloatParameter(1, 0, 2);
        private DOFParam _param = DOFParam.GetDefaultValue();
        private int id = -1;
        public bool IsActive()
        {
            return Intensity.value > 0;
        }

        public bool IsTileCompatible() => false;

        public override void OnFireChange(Camera camera, Transform root, bool enable)
        {
            base.OnFireChange(camera, root, enable);
            UpdateParam();

            if (enable)
            {
                id = URPDepthOfField.instance.AddParam(_param, URPDOFSource.DefualtValue, 0);
            }
            else
            {
                URPDepthOfField.instance.RemoveParam(id);
            }
        }

        private void UpdateParam()
        {
            _param.active = Intensity.value > 0;
            _param.EasyMode = EasyMode.value;
            _param.FocusDistance = FocusDistance.value;
            _param.BokehRangeFar = BokehRangeNear.value;
            _param.FocusRangeFar = FocusRangeFar.value;
            _param.BlurRadius = BlurRadius.value;
            _param.Intensity = Intensity.value;
        }

        public void Update()
        {
            if (id != -1)
            {
                UpdateParam();
                URPDepthOfField.instance.ModifyParam(id, _param);
            }
        #if UNITY_EDITOR
            else
            {
                if (!Application.isPlaying)
                {
                    id = URPDepthOfField.instance.AddParam(_param, URPDOFSource.DefualtValue, 0);
                }
            }
        #endif
        }

        protected override void OnDisable()
        {
            if (id != -1)
            {
                URPDepthOfField.instance.RemoveParam(id);
            }
        }
    }
}