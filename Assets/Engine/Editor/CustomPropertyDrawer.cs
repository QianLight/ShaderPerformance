// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

using System;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    public class CustomPropertyDrawer
    {
        static void DrawSlider (Material mat, ShaderCustomProperty scp, ref Vector4 value, int index)
        {
            if (scp.valid)
            {

                if (scp.toggleType == EShaderCustomValueToggle.Keyword)
                {
                    bool hasKeyword = mat.IsKeywordEnabled (scp.valueName);
                    if (hasKeyword && scp.valueCmpType != EValueCmpType.Equel ||
                        !hasKeyword && scp.valueCmpType == EValueCmpType.Equel)
                    {
                        return;
                    }
                }

                if (!string.IsNullOrEmpty (scp.desc))
                {
                    if (scp.min < scp.max)
                    {
                        bool needIndentation = false;
                        bool valid = true;
                        bool drawDesc = true;
                        float min = scp.min;
                        if (scp.toggleType == EShaderCustomValueToggle.ValueToggle ||
                            scp.toggleType == EShaderCustomValueToggle.Toggle)
                        {
                            var cmpFun = ShaderCustomProperty.cmpFun[(int) scp.valueCmpType];
                            bool isEnable = cmpFun (value[index], scp.thresholdValue);
                            EditorGUILayout.BeginHorizontal ();
                            bool enable = EditorGUILayout.Toggle (scp.desc, isEnable);
                            if (isEnable != enable)
                            {
                                value[index] = enable?scp.enableValue : scp.disableValue;
                            }
                            if (enable)
                            {
                                min = scp.thresholdValue + 0.001f;
                            }
                            valid = enable && scp.toggleType == EShaderCustomValueToggle.ValueToggle;
                            drawDesc = false;
                            EditorGUILayout.EndHorizontal ();
                            needIndentation = true;
                            EditorGUI.indentLevel++;
                        }
                        EditorGUILayout.BeginHorizontal ();
                        if (valid)
                        {
                            if (drawDesc)
                            {
                                if (scp.intValue)
                                {
                                    value[index] = EditorGUILayout.IntSlider (
                                        string.Format ("{0}({1}-{2})", scp.desc, min, scp.max),
                                        (int) value[index], (int) min, (int) scp.max);
                                }
                                else
                                {
                                    value[index] = EditorGUILayout.Slider (
                                        string.Format ("{0}({1}-{2})", scp.desc, min, scp.max),
                                        value[index], min, scp.max);
                                }

                            }
                            else
                            {
                                if (scp.intValue)
                                {
                                    value[index] = EditorGUILayout.IntSlider (
                                        string.Format ("{0}-{1})", min, scp.max),
                                        (int) value[index], (int) min, (int) scp.max);
                                }
                                else
                                {
                                    value[index] = EditorGUILayout.Slider (
                                        string.Format ("{0}-{1})", min, scp.max),
                                        value[index], min, scp.max);
                                }
                            }

                            if (GUILayout.Button ("R", GUILayout.MaxWidth (20)))
                            {
                                value[index] = scp.defaultValue;
                            }
                        }

                        if (needIndentation)
                            EditorGUI.indentLevel--;
                        EditorGUILayout.EndHorizontal ();
                    }
                }
                else
                {
                    value[index] = scp.defaultValue;
                }

            }
        }

        public static void OnGUI (MaterialProperty prop, Material mat, ShaderFeature sf)
        {
            Vector4 value = prop.vectorValue;
            if (prop.type == MaterialProperty.PropType.Color)
            {
                value = prop.colorValue;
            }
            EditorGUI.BeginChangeCheck ();
            for (int i = 0; i < sf.customProperty.Length; ++i)
            {
                var scp = sf.customProperty[i];
                DrawSlider (mat, scp, ref value, scp.index >= 0 ? scp.index : i);
            }
            if (EditorGUI.EndChangeCheck ())
            {
                if (prop.type == MaterialProperty.PropType.Color)
                {
                    prop.colorValue = value;
                }
                else
                    prop.vectorValue = value;
            }
        }

        static void DrawToggle (Material mat, ShaderCustomProperty scp, string name, ref Vector4 value, int index, bool readOnly)
        {
            var cmpFun = ShaderCustomProperty.cmpFun[(int) scp.valueCmpType];
            bool isEnable = cmpFun (value[index], scp.thresholdValue);
            EditorGUILayout.BeginHorizontal ();
            bool enable = EditorGUILayout.Toggle (name, isEnable);
            if (isEnable != enable && !readOnly)
            {
                value[index] = enable?scp.enableValue : scp.disableValue;
            }
            EditorGUILayout.EndHorizontal ();
        }

        public static void OnToggleValueGUI (MaterialProperty prop, Material mat, ShaderFeature sf, bool readOnly)
        {

            Vector4 value = prop.vectorValue;
            if (prop.type == MaterialProperty.PropType.Color)
            {
                value = prop.colorValue;
            }
            else if (prop.type == MaterialProperty.PropType.Float ||
                prop.type == MaterialProperty.PropType.Range)
            {
                value.x = prop.floatValue;
            }
            EditorGUI.BeginChangeCheck ();
            var scp = sf.customProperty[0];
            DrawToggle (mat, scp, sf.name, ref value, scp.index >= 0 ? scp.index : 0, readOnly);
            if (EditorGUI.EndChangeCheck ())
            {
                if (prop.type == MaterialProperty.PropType.Color)
                {
                    prop.colorValue = value;
                }
                else if (prop.type == MaterialProperty.PropType.Float ||
                    prop.type == MaterialProperty.PropType.Range)
                {
                    prop.floatValue = value.x;
                }
                else
                {
                    prop.vectorValue = value;
                }

            }
        }
    }
}