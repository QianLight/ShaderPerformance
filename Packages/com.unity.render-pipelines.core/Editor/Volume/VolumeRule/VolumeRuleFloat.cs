using System;
using UnityEditor;

namespace UnityEngine.Rendering
{
    [VolumeRule("数值大小限制", SerializedPropertyType.Float)]
    public class VolumeRuleFloat : VolumeRule
    {
        public enum Type
        {
            Min,
            Max,
        }

        [Header("规范类型")]
        public Type type; 
        [Header("对比值")]
        public float threshold;

        public override bool IsValid(SerializedProperty property)
        {
            switch (type)
            {
                case Type.Min:
                    return property.floatValue >= threshold;
                case Type.Max:
                    return property.floatValue <= threshold;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void GUI(SerializedObject serializedObject, SerializedProperty property)
        {
            SerializedProperty typeSp = serializedObject.FindProperty(nameof(type));
            GUILayoutOption width = GUILayout.Width(EditorGUIUtility.labelWidth);
            typeSp.enumValueIndex = (int)(Type)EditorGUILayout.EnumPopup((Type)typeSp.enumValueIndex, width);
            
            SerializedProperty thresholdSp = serializedObject.FindProperty(nameof(threshold));
            thresholdSp.floatValue = EditorGUILayout.FloatField(thresholdSp.floatValue);
        }
    }
}