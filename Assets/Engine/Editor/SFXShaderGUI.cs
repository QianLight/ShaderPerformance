using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SFXShaderGUI : ShaderGUI
{
    private Dictionary<string, MaterialProperty> propertyMap = new Dictionary<string, MaterialProperty>();
    private MaterialProperty[] properties;
    private MaterialEditor editor;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        this.editor = materialEditor;
        this.properties = properties;

        DrawGroup(DrawBase);

        DrawGroup(DrawMainColor);

        DrawGroup(DrawMask);

        DrawGroup(DrawDissolution);

        DrawGroup(DrawDistortion);

        DrawGroup(DrawSoftParticle);

        DrawGroup(DrawDistanceFade);

        DrawGroup(DrawFresnel);

        DrawGroup(DrawRamp);

        DrawGroup(DrawDecal);
    }

    private void DrawDecal()
    {
        DrawToggle("_Param4", 3);
    }

    private void DrawDistortion()
    {
        if (DrawToggle("_Param2", 3, "使用扭曲"))
        {
            DrawProperty("_DistortionTex");
            DrawProperty("_DistortionSpeed");
            DrawCustomDataToggle(0, 3, () => DrawFloat("_Param3", 1, "扭曲强度"));
        }
    }

    private void DrawBase()
    {
        DrawEnum<UnityEngine.Rendering.BlendMode>("_BlendMode");
        DrawEnum<UnityEngine.Rendering.CullMode>("_CullMode");
        DrawEnum<UnityEngine.Rendering.CompareFunction>("_ZTest");
        DrawEnum<DepthMode>("_DepthMode");
        DrawSlider("_Param2", 2, null, 0, 1.01f);
        Material mat = editor.target as Material;
        mat.renderQueue = EditorGUILayout.IntField("RenderQueue", mat.renderQueue);
    }

    private enum DepthMode
    {
        Off,
        On
    }

    private void DrawRamp()
    {
        if (DrawToggle("_Param1", 1))
        {
            using (new GUILayout.VerticalScope("box"))
            {
                GUILayout.Label("颜色分割线");
                DrawSlider("_RampShape", 0, "位置1");
                DrawSlider("_RampShape", 1, "宽度1");
                DrawSlider("_RampShape", 2, "位置2");
                DrawSlider("_RampShape", 3, "宽度2");
            }
            using (new GUILayout.VerticalScope("box"))
            {
                GUILayout.Label("渐变颜色");
                DrawProperty("_RampColor0");
                DrawProperty("_RampColor1");
                DrawProperty("_RampColor2");
            }
        }
    }

    private void DrawFresnel()
    {
        if (DrawToggle("_Param0", 3, "使用边缘光", x => x > 0))
        {
            DrawFloat("_Param0", 3, "过渡", 1e-4f);
            DrawHDRColor("_RimColor");
            DrawToggle("_Param1", 0);
            DrawSlider("_Param3", 0, "偏移", 0, 3);
            DrawToggle("_RimMask", 0, "边缘光R");
            DrawToggle("_RimMask", 1, "边缘光G");
            DrawToggle("_RimMask", 2, "边缘光B");
            DrawToggle("_RimMask", 3, "边缘光A");
        }
    }

    private void DrawHDRColor(string propertyName)
    {
        var colorProperty = GetProperty(propertyName);
        colorProperty.colorValue = EditorGUILayout.ColorField(new GUIContent(colorProperty.displayName), colorProperty.colorValue, true, true, true);
    }

    private void DrawDistanceFade()
    {
        var property = GetProperty("_Param0");
        var value = property.vectorValue;
        ref float fadeIn = ref value.y;
        ref float fadeOut = ref value.z;
        bool enable = fadeOut >= 0 || fadeOut > fadeIn;
        EditorGUI.BeginChangeCheck();
        bool newEnable = EditorGUILayout.Toggle("使用距离淡出", enable);
        bool toggleChanged = EditorGUI.EndChangeCheck();
        EditorGUI.BeginChangeCheck();
        if (toggleChanged)
        {
            fadeIn = newEnable ? 0 : 0;
            fadeOut = newEnable ? 10 : -1;
        }
        if (newEnable)
        {
            fadeIn = EditorGUILayout.FloatField("淡入距离", fadeIn);
            fadeOut = EditorGUILayout.FloatField("淡出距离", fadeOut);
        }
        bool fadesChanged = EditorGUI.EndChangeCheck();
        if (toggleChanged || fadesChanged)
        {
            property.vectorValue = value;
        }
    }

    private void DrawSoftParticle()
    {
        if (DrawToggle("_Param0", 0, "使用软粒子", x => x > 0))
        {
            DrawFloat("_Param0", 0, "淡出距离", 1e-4f);
        }
    }

    private void DrawMask()
    {
        if (DrawToggle("_Param1", 2))
        {
            DrawProperty("_MaskTex");
        }
    }

    private void DrawCustomDataToggle(int customDataIndex, int customDataComponent, Action gui)
    {
        string propertyName = "_UseCustomData" + customDataIndex;
        string displayName = GetDefualtDisplayName(null, propertyName, customDataComponent);
        bool useCustomData = DrawToggle(propertyName, customDataComponent, "粒子控制" + displayName);
        if (useCustomData)
        {
            Color color = GUI.color;
            GUI.color = Color.yellow;
            GUILayout.Label($"使用CustomData{customDataIndex + 1}.{"xyzw"[customDataComponent]}控制UV速度");
            GUI.color = color;
        }
        using (new EditorGUI.DisabledGroupScope(useCustomData))
        {
            gui?.Invoke();
        }
    }

    private void DrawDissolution()
    {
        if (DrawToggle("_DissolutionParam", 0, "使用消融图", x => x < 1, 0.5f, 1))
        {
            DrawProperty("_DissolutionTex", "消融贴图");
            DrawSlider("_DissolutionParam", 1, null, 0, 1);
            Action drawProgress = () => DrawSlider("_DissolutionParam", 0, null, 0, 1 - 1e-4f);
            DrawCustomDataToggle(0, 2, drawProgress);

            var edgeColorProperty = GetProperty("_DissolutionEdgeColor");
            Color edgeColor = edgeColorProperty.colorValue;
            bool edgeEnable = edgeColor.a > 0;
            EditorGUI.BeginChangeCheck();
            bool newEdgeEnable = EditorGUILayout.Toggle("消融边缘", edgeEnable);
            if (EditorGUI.EndChangeCheck())
            {
                edgeColor.a = newEdgeEnable ? 1 : 0;
                edgeColorProperty.colorValue = edgeColor;
            }
            if (newEdgeEnable)
            {
                DrawCustomDataToggle(0, 1, () =>
                {
                    DrawSlider("_DissolutionParam", 2, null, 0, 1);
                });
                DrawSlider("_DissolutionParam", 3, null, 0, 1);
                DrawHDRColor("_DissolutionEdgeColor");
            }
        }
    }

    private void DrawMainColor()
    {
        DrawProperty("_MainTex");
        DrawHDRColor("_FrontColor");
        if (DrawToggle("_Param2", 0, "使用背面颜色"))
        {
            DrawHDRColor("_BackColor");
        }
        DrawToggle("_Param2", 1);
        MaterialProperty legacyParam = GetProperty("_LegacyParam");
        if (legacyParam.vectorValue.x != 0)
        {
            DrawSlider("_Param4", 2, "_Contrast", 0, 10);
            EditorGUILayout.HelpBox("Contrast有性能问题，如果有时间顺便把这个材质调好，并关掉。", MessageType.Error);
            if (GUILayout.Button("关闭<_Contrast>功能"))
            {
                var param4 = GetProperty("_Param4");
                var param4Value = param4.vectorValue;
                param4Value.z = 1;
                param4.vectorValue = param4Value;

                Vector4 legacyVector = legacyParam.vectorValue;
                legacyVector.x = 0;
                legacyParam.vectorValue = legacyVector;
            }
        }
        DrawSlider("_Param4", 0, "Alpha缩放", 0, 5);
        DrawFloat("_Param1", 3);
        DrawToggle("_Param5", 0);

        GUILayout.Box("UV设置", GUILayout.ExpandWidth(true));
        Action drawUVSpeeds = () =>
        {
            DrawFloat("_Param3", 2);
            DrawFloat("_Param3", 3);
        };

        DrawCustomDataToggle(0, 0, drawUVSpeeds);
    }

    private void DrawGroup(Action gui)
    {
        GUILayout.BeginVertical("box");
        gui?.Invoke();
        GUILayout.EndVertical();
    }

    private void DrawEnum<T>(string propertyName)
    {
        Type type = typeof(T);
        MaterialProperty property = GetProperty(propertyName);
        property.floatValue = EditorGUILayout.Popup(property.displayName, (int)property.floatValue, Enum.GetNames(type));
    }

    private float DrawFloat(string propertyName, int component, string displayName = null, float min = float.NaN, float max = float.NaN)
    {
        displayName = GetDefualtDisplayName(displayName, propertyName, component);
        MaterialProperty property = GetProperty(propertyName);
        Vector4 value = property.vectorValue;
        EditorGUI.BeginChangeCheck();
        float result = EditorGUILayout.FloatField(displayName, value[component]);
        if (min != float.NaN)
        {
            result = Mathf.Max(min, result);
        }
        if (max != float.NaN)
        {
            result = Mathf.Min(max, result);
        }
        value[component] = result;
        if (EditorGUI.EndChangeCheck())
        {
            property.vectorValue = value;
        }
        return result;
    }

    private float DrawSlider(string propertyName, int component, string displayName = null, float min = 0, float max = 1)
    {
        displayName = GetDefualtDisplayName(displayName, propertyName, component);
        MaterialProperty property = GetProperty(propertyName);
        Vector4 value = property.vectorValue;
        EditorGUI.BeginChangeCheck();
        float result = EditorGUILayout.Slider(displayName, value[component], min, max);
        value[component] = result;
        if (EditorGUI.EndChangeCheck())
        {
            property.vectorValue = value;
        }
        return result;
    }

    private bool DrawToggle(string propertyName, int component, string displayName = null, Func<float, bool> condition = null, float onValue = 1, float offValue = 0)
    {
        displayName = GetDefualtDisplayName(displayName, propertyName, component);
        MaterialProperty property = GetProperty(propertyName);
        Vector4 value = property.vectorValue;
        EditorGUI.BeginChangeCheck();
        bool inValue = condition != null ? condition(value[component]) : value[component] != 0;
        bool result = EditorGUILayout.Toggle(displayName, inValue);
        value[component] = result ? onValue : offValue;
        if (EditorGUI.EndChangeCheck())
        {
            property.vectorValue = value;
        }
        return result;
    }

    private string GetDefualtDisplayName(string displayName, string propertyName, int component)
    {
        if (displayName == null)
        {
            return GetProperty(propertyName).displayName.Split('|')[component].Trim();
        }
        return displayName;
    }

    private void DrawProperty(string name, string displayName = null)
    {
        MaterialProperty property = GetProperty(name);
        displayName = displayName ?? property.displayName;
        editor.DefaultShaderProperty(property, displayName);
    }

    private MaterialProperty GetProperty(string name)
    {
        if (propertyMap.TryGetValue(name, out MaterialProperty property))
        {
            return property;
        }

        return propertyMap[name] = FindProperty(name, properties);
    }
}
