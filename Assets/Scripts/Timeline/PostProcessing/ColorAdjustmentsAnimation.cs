#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace UnityEngine
{
    public class ColorAdjustmentsAnimation:VolumeAnimation
    {
        public BoolAnimation enable;
        public FloatAnimation postExposure;
        public FloatAnimation sceneExposure;
        public FloatAnimation contrast;
        // public ColorParameter ColorFilter;
        // public FloatParameter HueShift;
        public FloatAnimation saturation;
        private ColorAdjustments _volumeCA;

        protected override void OnEnable()
        {
            base.OnEnable();
            Search(ref _volumeCA);
        }

        public override void RefreshData()
        {
            if (enable.overrideState)_volumeCA.active = enable.value;
            _volumeCA.postExposure.overrideState = postExposure.overrideState;
            if (postExposure.overrideState) _volumeCA.postExposure.value = postExposure.value;
            _volumeCA.sceneExposure.overrideState = sceneExposure.overrideState;
            if (sceneExposure.overrideState) _volumeCA.sceneExposure.value = sceneExposure.value;
            _volumeCA.contrast.overrideState = contrast.overrideState;
            if (contrast.overrideState) _volumeCA.contrast.value = contrast.value;
            _volumeCA.saturation.overrideState = saturation.overrideState;
            if (saturation.overrideState) _volumeCA.saturation.value = saturation.value;
        }
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(ColorAdjustmentsAnimation))]
    public class ColorAdjustmentsAnimationEditor : VolumeAnimationEditor
    {
        public ColorAdjustmentsAnimation _target;
        private SerializedProperty _enable;
        private SerializedProperty _postExposure;
        private SerializedProperty _sceneExposure;
        private SerializedProperty _contrast;
        private SerializedProperty _saturation;

        protected override void OnEnable()
        {
            base.OnEnable();
            _target = target as ColorAdjustmentsAnimation;
            _enable = serializedObject.FindProperty("enable");
            _postExposure = serializedObject.FindProperty("postExposure");
            _sceneExposure = serializedObject.FindProperty("sceneExposure");
            _contrast = serializedObject.FindProperty("contrast");
            _saturation = serializedObject.FindProperty("saturation");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            DrawBoolAnimation(ref _enable, "Active");
            DrawFloatAnimation(ref _postExposure, "PostExposure");
            DrawFloatAnimation(ref _sceneExposure, "SceneExposure");
            DrawFloatAnimation(ref _contrast, "Contrast");
            DrawFloatAnimation(ref _saturation, "Saturation");
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed BloomAnimation");
                SetBoolAnimation(ref _target.enable, ref _enable);
                SetFloatAnimation(ref _target.postExposure, ref _postExposure);
                SetFloatAnimation(ref _target.sceneExposure, ref _sceneExposure);
                SetFloatAnimation(ref _target.contrast, ref _contrast);
                SetFloatAnimation(ref _target.saturation, ref _saturation);
            }
        }
    }
#endif
}