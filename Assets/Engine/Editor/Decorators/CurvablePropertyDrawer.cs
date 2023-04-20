using UnityEditor;
using UnityEngine;

public abstract class CurvablePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        Rect labelPosition = GetRect(ref position, EditorGUIUtility.labelWidth - 14);
        GUI.Label(labelPosition, label);

        SerializedProperty useCurveProeprty = property.FindPropertyRelative("useCurve");
        Rect useCurveTogglePos = GetRect(ref position, position.height);
        useCurveProeprty.boolValue = EditorGUI.Toggle(useCurveTogglePos, useCurveProeprty.boolValue);

        Rect valuePosition = GetRect(ref position, position.width);
        string valueName = useCurveProeprty.boolValue ? "curve" : "value";
        SerializedProperty valueProperty = property.FindPropertyRelative(valueName);
        EditorGUI.PropertyField(valuePosition, valueProperty, GUIContent.none);

        EditorGUI.EndProperty();
    }

    private Rect GetRect(ref Rect space, float size)
    {
        Rect result = new Rect(space.x, space.y, size, space.height);
        space.x += size;
        space.width -= size;
        return result;
    }
}


[CustomPropertyDrawer(typeof(CFloat))]
public class CurvableFloatDrawer : CurvablePropertyDrawer { }


[CustomPropertyDrawer(typeof(CBool))]
public class CurvableBoolDrawer : CurvablePropertyDrawer { }


[CustomPropertyDrawer(typeof(CColor))]
public class CurvableColorDrawer : CurvablePropertyDrawer { }

[CustomPropertyDrawer(typeof(CFEngine.OverridableCFloat))]
public class OverridableCFloatDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty enableOverride = property.FindPropertyRelative(nameof(CFEngine.OverridableCFloat.enable));
        Rect enableTogglePos = GetRect(ref position, position.height);
        enableOverride.boolValue = EditorGUI.Toggle(enableTogglePos, enableOverride.boolValue);

        GUILayout.Label(property.propertyPath);

        using (new EditorGUI.DisabledGroupScope(!enableOverride.boolValue))
        {
            Rect labelPosition = GetRect(ref position, EditorGUIUtility.labelWidth - 14);
            GUI.Label(labelPosition, label);

            SerializedProperty useCurveProeprty = property.FindPropertyRelative("data.useCurve");
            Rect useCurveTogglePos = GetRect(ref position, position.height);
            useCurveProeprty.boolValue = EditorGUI.Toggle(useCurveTogglePos, useCurveProeprty.boolValue);

            Rect valuePosition = GetRect(ref position, position.width);
            string valueName = useCurveProeprty.boolValue ? "data.curve" : "data.value";
            SerializedProperty valueProperty = property.FindPropertyRelative(valueName);
            EditorGUI.PropertyField(valuePosition, valueProperty, GUIContent.none);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label);
    }

    private Rect GetRect(ref Rect space, float size)
    {
        Rect result = new Rect(space.x, space.y, size, space.height);
        space.x += size;
        space.width -= size;
        return result;
    }
}