using CFEngine;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RadialBlurDataV2))]
public class RadialBlurDataV2Editor : Editor
{
    private static readonly SavedBool fadeInFolder = new SavedBool($"{nameof(RadialBlurDataV2Editor)}.{nameof(fadeInFolder)}");
    private static readonly SavedBool loopFolder = new SavedBool($"{nameof(RadialBlurDataV2Editor)}.{nameof(loopFolder)}");
    private static readonly SavedBool fadeOutFolder = new SavedBool($"{nameof(RadialBlurDataV2Editor)}.{nameof(fadeOutFolder)}");

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        using (new GUILayout.VerticalScope("box"))
        {
            using (new GUILayout.VerticalScope("box"))
            {
                GUILayout.Box("基础设置", GUILayout.ExpandWidth(true));
                Draw(nameof(RadialBlurDataV2.useScreenSpace));
                Draw(nameof(RadialBlurDataV2.positionX));
                Draw(nameof(RadialBlurDataV2.positionY));
                Draw(nameof(RadialBlurDataV2.positionZ));
                Draw(nameof(RadialBlurDataV2.intensity));
                Draw(nameof(RadialBlurDataV2.rangeScale));
            }

            using (new GUILayout.VerticalScope("box"))
            {
                DrawGroup("fadeIn", "淡入阶段", fadeInFolder);

                DrawGroup("loop", "循环阶段", loopFolder);

                DrawGroup("fadeOut", "淡出阶段", fadeOutFolder);
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawGroup(string name, string displayName, SavedBool folder)
    {
        using (new GUILayout.VerticalScope("box"))
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            folder.Value = EditorGUILayout.Foldout(folder.Value, displayName);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            GUILayout.BeginVertical();
            if (folder.Value)
            {
                DrawStageParams(name);
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }
    }

    private void DrawStageParams(string name)
    {
        string templateName = name + "Template";
        SerializedProperty template = Find(templateName);
        SerializedProperty custom = Find(name);

        EditorGUILayout.PropertyField(template);
        DrawProperty(template, custom, nameof(RadialBlurStage.positionOffsetX));
        DrawProperty(template, custom, nameof(RadialBlurStage.positionOffsetY));
        DrawProperty(template, custom, nameof(RadialBlurStage.scale));
        DrawProperty(template, custom, nameof(RadialBlurStage.innerRadius));
        DrawProperty(template, custom, nameof(RadialBlurStage.innerFadeOut));
        DrawProperty(template, custom, nameof(RadialBlurStage.outerRadius));
        DrawProperty(template, custom, nameof(RadialBlurStage.outerFadeOut));
        DrawProperty(template, custom, nameof(RadialBlurStage.intensity));
    }

    private void DrawProperty(SerializedProperty template, SerializedProperty customData, string name)
    {
        using (new GUILayout.HorizontalScope())
        {
            var templateObject = template == null ? null : template.objectReferenceValue;
            var t = templateObject ? new SerializedObject(templateObject).FindProperty(name) : null;
            var c = customData.FindPropertyRelative(name);
            var useCustomDataSp = c.FindPropertyRelative(nameof(OverridableCFloat.enable));
            float toggleWidth = EditorGUIUtility.singleLineHeight;
            if (templateObject)
            {
                useCustomDataSp.boolValue = EditorGUILayout.Toggle(new GUIContent(string.Empty, "使用自定义参数"), useCustomDataSp.boolValue, GUILayout.Width(toggleWidth));
            }
            bool useCustomData = !templateObject || useCustomDataSp.boolValue;
            var source = useCustomData ? c.FindPropertyRelative("data") : t;
            var useCurve = source.FindPropertyRelative("useCurve");
            string displayName = useCustomData ? c.displayName : t.displayName;
            var value = source.FindPropertyRelative(useCurve.boolValue ? "curve" : "value");
            using (new EditorGUI.DisabledGroupScope(templateObject && !useCustomDataSp.boolValue))
            {
                float displayNameWidth = templateObject
                    ? EditorGUIUtility.labelWidth - toggleWidth - 6
                    : EditorGUIUtility.labelWidth - 3;
                GUILayout.Label(displayName, GUILayout.Width(displayNameWidth));
                useCurve.boolValue = EditorGUILayout.Toggle(useCurve.boolValue, GUILayout.Width(toggleWidth));
                EditorGUILayout.PropertyField(value, GUIContent.none);
            }
        }
    }

    private SerializedProperty Find(string propertyName)
    {
        return serializedObject.FindProperty(propertyName);
    }

    private void Draw(string propertyName)
    {
        EditorGUILayout.PropertyField(Find(propertyName));
    }
}
