using System;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace UnityEngine
{

    public class BloomAnimation :VolumeAnimation
    {
        public FloatAnimation intensity;
        public FloatAnimation threshold;

        private Bloom _volumeBloom;

        protected override void OnEnable()
        {
            base.OnEnable();
            Search(ref _volumeBloom);
        }
        public override void RefreshData()
        {
            _volumeBloom.intensity.overrideState = intensity.overrideState;
            if (intensity.overrideState) _volumeBloom.intensity.value = intensity.value;

            _volumeBloom.threshold.overrideState = threshold.overrideState;
            if (threshold.overrideState) _volumeBloom.directThreshold.value = threshold.value;
        }
    }
    
    #if UNITY_EDITOR
    [CustomEditor(typeof(BloomAnimation))]
    public class BloomAnimationEditor : VolumeAnimationEditor
    {
        public BloomAnimation _target;
        private SerializedProperty _intensity;
        private SerializedProperty _threshold;

        protected override void OnEnable()
        {
            base.OnEnable();
            _target = target as BloomAnimation;
            _intensity = serializedObject.FindProperty("intensity");
            _threshold = serializedObject.FindProperty("threshold");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            DrawMinFloatAnimation(ref _intensity, "Intensity", 0);
            DrawMinFloatAnimation(ref _threshold, "Threshold", 0);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed BloomAnimation");
                SetFloatAnimation(ref _target.intensity, ref _intensity);
                SetFloatAnimation(ref _target.threshold, ref _threshold);
            }
        }
    }
    #endif
}