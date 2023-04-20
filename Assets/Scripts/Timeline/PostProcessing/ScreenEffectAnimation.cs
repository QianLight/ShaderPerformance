using System;
using CFEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace UnityEngine
{
    [ExecuteInEditMode]
    [Serializable]
    public class ScreenEffectAnimation: MonoBehaviour
    {
        public static MaterialPropertyBlock mpb;
        public ScreenEffect.ScreenEffectType type;
        public bool overrideProperty = false;
        
        [Space(10)]
        [Header("CartoonShine")]
        [Range(0,1)]public float cartoonShine_Min;
        public bool cartoonShine_Flip;
        [Range(0,1)] public float cartoonShine_Strength;
        
        [Header("MaskLUT")] [ColorUsage(false, true)]public Color maskLUT_Color;
        
        public float transition_duration;
        
        private Camera _camera;

        private ScreenEffect inst;

#if UNITY_EDITOR
        private void Reset()
        {
            var old = GameObject.Find("TimelineScreenEffect");
            if (old != null)
            {
                DebugLog.AddErrorLog("Timeline中已经存在ScreenEffect组件");                
                DestroyImmediate(this.gameObject);
            }
            this.gameObject.name = "TimelineScreenEffect";
            type = ScreenEffect.ScreenEffectType.OldFilm;
        }
#endif
        private void OnEnable()
        {
            inst = ScreenEffect.Instance();
            Rendering.RenderPipelineManager.beginFrameRendering -= UpdateData;
            Rendering.RenderPipelineManager.beginFrameRendering += UpdateData;
        }
        private void UpdateData(ScriptableRenderContext arg1, Camera[] arg2)
        {
            if (inst == null) return;
            inst.SetState(type);
            inst.OverrideProperty(overrideProperty);
            if (overrideProperty)
            {
                SetProperty(type);
            }     
            // else
            // {
            //     ScreenEffect.Instance().type = ScreenEffect.ScreenEffectType.None;
            // }
        }
        

        public void SetProperty(ScreenEffect.ScreenEffectType type)
        {
            if (inst.mpb == null) inst.mpb = new MaterialPropertyBlock();
            switch (type)
            {
                case ScreenEffect.ScreenEffectType.OldFilm : return;
                case ScreenEffect.ScreenEffectType.CartoonShine:
                {
                    inst.mpb.SetFloat(ScreenEffect.CartoonShineMin, cartoonShine_Min);
                    inst.mpb.SetFloat(ScreenEffect.CartoonShineFlip, cartoonShine_Flip ? 1 : 0);
                    inst.mpb.SetFloat(ScreenEffect.CartoonShineStrength, cartoonShine_Strength);
                    break;
                }
                case ScreenEffect.ScreenEffectType.MaskLUT:
                {
                    inst.mpb.SetColor(ScreenEffect.MaskLUTColor, maskLUT_Color);
                    break;
                }
                case ScreenEffect.ScreenEffectType.GrabTransition:
                {
                    inst.mpb.SetFloat(ScreenEffect.TransitionTransparent, transition_duration);
                    break;
                }
            }
        }

        public void Grab()
        {
            _camera = Camera.main;
            float scale = UniversalRenderPipeline.asset.renderScale;
            int _srcW = (int)(_camera.pixelWidth * scale);
            int _srcH = (int)(_camera.pixelHeight * scale);
            if (inst.transitionState == 0)
            {
                ScreenEffect.ReleaseRT();
                ScreenEffect.SetRT(_srcW, _srcH);
                inst.transitionState = 1;
            }
        }

        public void Release()
        {
            if (inst.transitionState == 2)
            {
                ScreenEffect.ReleaseRT();
                inst.transitionState = 0;
            }
        }
        

        private void OnDisable()
        {
            ScreenEffect.ReleaseRT();
            Rendering.RenderPipelineManager.beginFrameRendering -= UpdateData;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ScreenEffectAnimation))]
    public class ScreenEffectAnimationEditor : Editor
    {
        public ScreenEffectAnimation _target;
        private SerializedProperty _type;
        private SerializedProperty _override;
        private SerializedProperty _cartoonShine_Min;
        private SerializedProperty _cartoonShine_Flip;
        private SerializedProperty _cartoonShine_Strength;
        private SerializedProperty _maskLUT_Color;
        private SerializedProperty _transition_Duration;
        // private SerializedProperty _transition_Tex;
        private void OnEnable()
        {
            _target = target as ScreenEffectAnimation;
            _type = serializedObject.FindProperty("type");
            _override = serializedObject.FindProperty("overrideProperty");
            _cartoonShine_Min = serializedObject.FindProperty("cartoonShine_Min");
            _cartoonShine_Flip = serializedObject.FindProperty("cartoonShine_Flip");
            _cartoonShine_Strength = serializedObject.FindProperty("cartoonShine_Strength");
            _maskLUT_Color = serializedObject.FindProperty("maskLUT_Color");
            _transition_Duration = serializedObject.FindProperty("transition_duration");
            // _transition_Tex = serializedObject.FindProperty("_screenCopy");
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_type);
            EditorGUILayout.PropertyField(_override);
            switch (_target.type)
            {
                case ScreenEffect.ScreenEffectType.OldFilm: break;
                case ScreenEffect.ScreenEffectType.CartoonShine:
                {
                    if (_override.boolValue)
                    {
                        DrawRange(ref _cartoonShine_Min, 0, 1);
                        EditorGUILayout.PropertyField(_cartoonShine_Flip);
                        DrawRange(ref _cartoonShine_Strength, 0, 1);
                    }
                    break;
                }
                case ScreenEffect.ScreenEffectType.MaskLUT:
                {
                    if (_override.boolValue)
                    {
                        EditorGUILayout.PropertyField(_maskLUT_Color);
                    }
                    break;
                }
                case ScreenEffect.ScreenEffectType.GrabTransition:
                {
                    // EditorGUILayout.PropertyField(_transition_Tex);
                    if(GUILayout.Button("Release"))_target.Release();
                    if (_override.boolValue)
                    {
                        // if (GUILayout.Button("Grab")) _target.Grab();
                        DrawRange(ref _transition_Duration, 0, 1);
                    }
                    break;
                }
            }
            

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Change ScreenEffect");
                _target.type = (ScreenEffect.ScreenEffectType)(_type.enumValueIndex+1);
                _target.overrideProperty = _override.boolValue;
                _target.cartoonShine_Min = _cartoonShine_Min.floatValue;
                _target.cartoonShine_Flip = _cartoonShine_Flip.boolValue;
                _target.cartoonShine_Strength = _cartoonShine_Strength.floatValue;
                _target.maskLUT_Color = _maskLUT_Color.colorValue;
                _target.transition_duration = _transition_Duration.floatValue;
            }
        }


        void DrawRange(ref SerializedProperty floatProp, float min, float max)
        {
            EditorGUILayout.PropertyField(floatProp);
            float value = floatProp.floatValue;
            value = Mathf.Max(min, value);
            value = Mathf.Min(max, value);
            floatProp.floatValue = value;
        }
    }
#endif
}