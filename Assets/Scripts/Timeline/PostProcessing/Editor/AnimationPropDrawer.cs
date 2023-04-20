using UnityEditor;

namespace UnityEngine.Editor
{
    // public abstract class AnimationPropDrawer : PropertyDrawer
    // {
    //     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //     {
    //         // float width = position.width * 0.25f;
    //         // Rect rect = new Rect(position);
    //         // rect.width = width;
    //         // EditorGUI.LabelField(rect, label);
    //         // rect.x += width;
    //         // float labelWidthTmp = EditorGUIUtility.labelWidth;
    //         
    //         // EditorGUIUtility.labelWidth = 24f; // your label width (8) x3
    //         // Using BeginProperty / EndProperty on the parent property means that
    //         // prefab override logic works on the entire property.
    //         EditorGUI.BeginProperty(position, label, property);
    //         // Draw label
    //         position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
    //         GUI(position, property);
    //         EditorGUI.EndProperty();
    //         
    //         // EditorGUIUtility.labelWidth = labelWidthTmp;
    //
    //     }
    //
    //     public virtual void GUI(Rect position, SerializedProperty property)
    //     {
    //        
    //         // Don't make child fields be indented
    //         var indent = EditorGUI.indentLevel;
    //         EditorGUI.indentLevel = 0;
    //         // Calculate rects
    //         var overrideStateRect = new Rect(position.x, position.y, 25, EditorGUI.GetPropertyHeight(property.FindPropertyRelative("overrideState")));
    //         // var unitRect = new Rect(position.x + 35, position.y, 50, position.height);
    //         // var nameRect = new Rect(position.x + 90, position.y, position.width - 90, position.height);
    //         // Draw fields - passs GUIContent.none to each so they are drawn without labels
    //         EditorGUI.PropertyField(overrideStateRect, property.FindPropertyRelative("overrideState"), GUIContent.none);
    //         // EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("unit"), GUIContent.none);
    //         // EditorGUI.PropertyField(nameRect, property.FindPropertyRelative("name"), GUIContent.none);
    //         DrawData(position, property);
    //         // Set indent back to what it was
    //         EditorGUI.indentLevel = indent;
    //     }
    //     public abstract void DrawData(Rect position, SerializedProperty property);
    // }
    
    // [CustomPropertyDrawer(typeof(BoolAnimation))]
    // public class BoolAnimationDrawer : AnimationPropDrawer
    // {
    //     public override void DrawData(Rect position, SerializedProperty property)
    //     {
    //         var valueRect = new Rect(position.x + 30, position.y, 50, EditorGUI.GetPropertyHeight(property.FindPropertyRelative("value")));
    //         EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("value"), GUIContent.none);
    //     }
    // }
    // [CustomPropertyDrawer(typeof(FloatAnimation))]
    // public class FloatAnimationDrawer : AnimationPropDrawer
    // {
    //     public override void DrawData(Rect position, SerializedProperty property)
    //     {
    //         var valueRect = new Rect(position.x + 30, position.y, 100, EditorGUI.GetPropertyHeight(property.FindPropertyRelative("value")));
    //         // EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("value"), GUIContent.none);
    //         property.FindPropertyRelative("value").floatValue = EditorGUI.FloatField(valueRect, property.FindPropertyRelative("value").floatValue, EditorStyles.numberField);
    //     }
    // }
    // [CustomPropertyDrawer(typeof(ColorAnimation))]
    // public class ColorAnimationDrawer : AnimationPropDrawer
    // {
    //     public override void DrawData(Rect position, SerializedProperty property)
    //     {
    //         var valueRect = new Rect(position.x + 30, position.y, 100, EditorGUI.GetPropertyHeight(property.FindPropertyRelative("color")));
    //         // property.FindPropertyRelative("color").vector4Value = EditorGUI.ColorField(valueRect, "", property.FindPropertyRelative("color").vector4Value);
    //         // EditorGUILayout.PropertyField(property.FindPropertyRelative("color"), true);
    //         EditorGUI.PropertyField(valueRect, property.FindPropertyRelative("color"), GUIContent.none);
    //     }
    // }
    // [CustomPropertyDrawer(typeof(LightAnimation))]
    // public class LightAnimationDrawer : AnimationPropDrawer
    // {
    //     public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    //     {
    //         Rect newp = position;
    //         // newp.y *= 4;
    //         // newp.height *= 4;
    //         // Using BeginProperty / EndProperty on the parent property means that
    //         // prefab override logic works on the entire property.
    //         EditorGUI.BeginProperty(newp, label, property);
    //         // Draw label
    //         position = EditorGUI.PrefixLabel(newp, GUIUtility.GetControlID(FocusType.Passive), label);
    //         GUI(position, property);
    //         EditorGUI.EndProperty();
    //     }
    //     
    //     public override void DrawData(Rect position, SerializedProperty property)
    //     {
    //         EditorGUI.indentLevel++;
    //         var lightRect = new Rect(position.x+30, position.y, 100, position.height);
    //         EditorGUI.PropertyField(lightRect, property.FindPropertyRelative("light"), GUIContent.none);
    //     
    //         var lightInfo = property.FindPropertyRelative("info");
    //         var colorRect = new Rect(position.x+30, position.y + position.height, 100, position.height);
    //         EditorGUI.PropertyField(colorRect, lightInfo.FindPropertyRelative("color"), GUIContent.none);
    //         
    //         var hRect = new Rect(position.x+30, position.y + position.height * 2, 300, position.height);
    //         EditorGUI.PropertyField(hRect, lightInfo.FindPropertyRelative("horizontal"), GUIContent.none);
    //         var vRect = new Rect(position.x+30, position.y + position.height * 3, 300, position.height);
    //         EditorGUI.PropertyField(vRect, lightInfo.FindPropertyRelative("vertical"), GUIContent.none);
    //
    //     }
    // }

}