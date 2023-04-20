using UnityEditor;
using UnityEngine;

public class XToggleDrawer : MaterialPropertyDrawer
{
    public override void OnGUI(Rect position, MaterialProperty prop, GUIContent label, MaterialEditor editor)
    {
        bool value = prop.floatValue != 0f;
        EditorGUI.BeginChangeCheck();
        value = EditorGUI.Toggle(position, label, value);
        if (EditorGUI.EndChangeCheck())
        {
            prop.floatValue = value ? 1f : 0f;
        }
    }
}
