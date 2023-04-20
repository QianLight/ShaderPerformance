using System;

namespace UnityEngine.Rendering.Universal
{
    [Serializable, VolumeComponentMenu("Post-processing/Environment Color Adjust")]
    public class EnvironmentColorAdjust : VolumeComponent, IPostProcessComponent
    {
        private Color _skyColor, _equatorColor, _groundColor;

        public ColorParameter skyColor = new ColorParameter(Color.gray, true, false, true, false);
        public ColorParameter equatorColor = new ColorParameter(Color.gray, true, false, true, false);
        public ColorParameter groundColor = new ColorParameter(Color.gray, true, false, true, false);
        
        public bool IsActive()
        {
            return skyColor.overrideState | equatorColor.overrideState | groundColor.overrideState;
        }

        public bool IsTileCompatible()
        {
            return true;
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            RecordOriginColor();
            InitEnvironmentParams();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _skyColor = Color.gray;
            _equatorColor = Color.gray;
            _groundColor = Color.gray;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void OnFireChange(Camera camera, Transform root, bool enable)
        {
            base.OnFireChange(camera, root, enable);
            
            if (enable)
            {
                ApplyEnvironmentColor();
            }
            else
            {
                RevertEnvironmentColor();
            }
        }

        public override void OnOverrideFinish()
        {
            base.OnOverrideFinish();
            ApplyEnvironmentColor();
        }

        public void SetAllOverrideState(bool isOverrideState)
        {
            skyColor.overrideState = isOverrideState;
            equatorColor.overrideState = isOverrideState;
            groundColor.overrideState = isOverrideState;
        }
        
        private void RecordOriginColor()
        {
            _skyColor = RenderSettings.ambientSkyColor;
            _equatorColor = RenderSettings.ambientEquatorColor;
            _groundColor = RenderSettings.ambientGroundColor;
        }

        private void InitEnvironmentParams()
        {
            if (!skyColor.overrideState)
            {
                skyColor.value = _skyColor;
            }

            if (!equatorColor.overrideState)
            {
                equatorColor.value = _equatorColor;
            }

            if (!groundColor.overrideState)
            {
                groundColor.value = _groundColor;
            }
        }

        private void ApplyEnvironmentColor()
        {
            if (skyColor.overrideState)
            {
                RenderSettings.ambientSkyColor = skyColor.value;
            }

            if (equatorColor.overrideState)
            {
                RenderSettings.ambientEquatorColor = equatorColor.value;
            }

            if (groundColor.overrideState)
            {
                RenderSettings.ambientGroundColor = groundColor.value;
            }
        }

        private void RevertEnvironmentColor()
        {
            RenderSettings.ambientSkyColor = _skyColor;
            RenderSettings.ambientEquatorColor = _equatorColor;
            RenderSettings.ambientGroundColor = _groundColor;
        }
    }
}