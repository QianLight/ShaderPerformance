using System;
using CFClient;
using UnityEditor;

namespace UnityEngine.Rendering
{
    [VolumeRule("通道值大小限制", SerializedPropertyType.Vector4)]
    public class VolumeRuleVector4Component : VolumeRule
    {
        public enum Type
        {
            Min,
            Max,
        }

        public enum Component
        {
            x,
            y,
            z,
            w
        }

        [Header("规范类型")]
        public Type type;

        public Component component;
        [Header("对比值")]
        public float threshold;
        
        public override bool IsValid(SerializedProperty property)
        {
            switch (type)
            {
                case Type.Min:
                    return property.vector4Value[(int) component] > threshold;
                case Type.Max:
                    return property.vector4Value[(int) component] < threshold;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void GUI(SerializedObject serializedObject, SerializedProperty property)
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            Rect typeRect = GUILayoutUtility.GetRect(1f, 17f);
            Rect componentRect = GUILayoutUtility.GetRect(1f, 17f);
            Rect thresholdRect = GUILayoutUtility.GetRect(1f, 17f);
            
            SerializedProperty typeSp = serializedObject.FindProperty(nameof(type));
            typeSp.enumValueIndex = (int)(Type)EditorGUI.EnumPopup(typeRect, (Type)typeSp.enumValueIndex);
            SerializedProperty componentSp = serializedObject.FindProperty(nameof(component));
            componentSp.enumValueIndex = (int)(Component)EditorGUI.EnumPopup(componentRect, (Component)componentSp.enumValueIndex);
            SerializedProperty thresholdSp = serializedObject.FindProperty(nameof(threshold));
            thresholdSp.floatValue = EditorGUI.FloatField(thresholdRect, thresholdSp.floatValue);
            EditorGUILayout.EndHorizontal();

            Vector4 value = property.vector4Value;
            Rect valueRect = GUILayoutUtility.GetRect(1f, 17f);
            EditorGUI.LabelField(valueRect, $"当前值: {value.x:0.00},{value.y:0.00},{value.z:0.00},{value.w:0.00}");
            EditorGUILayout.EndVertical();
        }
    }
}