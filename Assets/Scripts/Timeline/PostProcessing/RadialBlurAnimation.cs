using System;
using UnityEngine.Rendering.Universal;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine
{
    [ExecuteInEditMode]
    public class RadialBlurAnimation:MonoBehaviour
    {
        public RadialBlurParam param;
        private int _id;

        private void Reset()
        {
            param = RadialBlurParam.GetDefualtValue();
        }

        private void OnEnable()
        {
            _id = URPRadialBlur.instance.AddParam(param, URPRadialBlurSource.Timeline, 0);
        }

        void Update()
        {
            URPRadialBlur.instance.ModifyParam(_id, param);
        }

        private void OnDisable()
        {
            URPRadialBlur.instance.RemoveParam(_id);
            _id = -1;
        }    
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(RadialBlurAnimation))]
    public class RadialBlurAnimationEditor : Editor
    {
        private RadialBlurAnimation _target;
        private SerializedProperty _param;

        private void OnEnable()
        {
            _target = target as RadialBlurAnimation;
            _param = serializedObject.FindProperty("param");
        }

        public override void OnInspectorGUI()
        {
            // base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_param.FindPropertyRelative("active"));
            EditorGUILayout.PropertyField(_param.FindPropertyRelative("center"));
            EditorGUILayout.LabelField("size ", "1");
            EditorGUILayout.PropertyField(_param.FindPropertyRelative("innerRadius"));
            EditorGUILayout.PropertyField(_param.FindPropertyRelative("innerFadeOut"));
            EditorGUILayout.PropertyField(_param.FindPropertyRelative("outerRadius"));
            EditorGUILayout.LabelField("Outer Fade out ", "0");
            EditorGUILayout.PropertyField(_param.FindPropertyRelative("intensity"));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed RadialBlurAnimation");
                _target.param.active = _param.FindPropertyRelative("active").boolValue;
                _target.param.center = _param.FindPropertyRelative("center").vector3Value;
                _target.param.size = 1;
                _target.param.innerRadius = Mathf.Clamp(_param.FindPropertyRelative("innerRadius").floatValue, 0f, 3f);
                _target.param.innerFadeOut = Mathf.Clamp(_param.FindPropertyRelative("innerFadeOut").floatValue, 0f, 5f);
                _target.param.outerRadius = Mathf.Clamp(_param.FindPropertyRelative("outerRadius").floatValue, 0f, 5f);
                _target.param.outerFadeOut = 0;
                _target.param.intensity = Mathf.Clamp(_param.FindPropertyRelative("intensity").floatValue, -1f, 1f);
                _target.param.useScreenPos = true;
            }
        }
    }
#endif
}