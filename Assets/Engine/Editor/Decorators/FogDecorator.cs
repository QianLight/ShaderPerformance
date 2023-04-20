using System;
using UnityEditor;
using UnityEngine;

namespace CFEngine.Editor
{
    [CFDecorator(typeof(CFFogAttribute))]
    public sealed class FogDecorator : AttributeDecorator<CFFogAttribute, FogParam>
    {
        private FogInfo editValue = default;

        public override SerializedPropertyType GetSType()
        {
            return SerializedPropertyType.Generic;
        }

        public override byte GetMaskFlag()
        {
            return (byte)(
                ParamOverride.MaskX |
                ParamOverride.MaskY |
                ParamOverride.MaskZ |
                ParamOverride.MaskW |
                ParamOverride.Mask4 |
                ParamOverride.Mask5
            );
        }

        public override void InnerOnGUI(
          GUIContent title,
          float width,
          uint flag)
        {
            var srcValue = new FogInfo()
            {
                start = sp.FindPropertyRelative("start").floatValue,
                end = sp.FindPropertyRelative("end").floatValue,
                intensityMin = sp.FindPropertyRelative("intensityMin").floatValue,
                intensityMax = sp.FindPropertyRelative("intensityMax").floatValue,
                intensityScale = sp.FindPropertyRelative("intensityScale").floatValue,
                fallOff = sp.FindPropertyRelative("fallOff").floatValue,
            };

            editValue = srcValue;
            bool drawToggle = false;
            if (overrideParam != null)
            {
                editValue = overrideParam.value;
                if (!maskX)
                {
                    editValue.start = srcValue.start;
                }
                if (!maskY)
                {
                    editValue.end = srcValue.end;
                }
                if (!maskZ)
                {
                    editValue.intensityMin = srcValue.intensityMin;
                }
                if (!maskW)
                {
                    editValue.intensityMax = srcValue.intensityMax;
                }
                if (!mask4)
                {
                    editValue.intensityScale = srcValue.intensityScale;
                }
                if (!mask5)
                {
                    editValue.fallOff = srcValue.fallOff;
                }
                drawToggle = true;
            }

            EditorCommon.BeginGroup(title.text, new Vector4(0, 0, width, 10 + 30 * 6), true);
            valueChange |= DrawFloat("淡入距离", ref editValue.start, drawToggle, ref maskX, 0, width);
            valueChange |= DrawFloat("淡出距离", ref editValue.end, drawToggle, ref maskY, 0, width);
            valueChange |= DrawSlider("最低强度", ref editValue.intensityMin, 0, 1, drawToggle, ref maskZ, 0, width);
            valueChange |= DrawSlider("最高强度", ref editValue.intensityMax, 0, 1, drawToggle, ref maskW, 0, width);
            valueChange |= DrawFloat("强度缩放", ref editValue.intensityScale, drawToggle, ref mask4, 0, width);
            valueChange |= DrawFloat("淡出效果", ref editValue.fallOff, drawToggle, ref mask5, 0, width);
            EditorCommon.EndGroup();
        }

        protected override bool EndOverride(ref byte mask)
        {
            if (overrideToggleChange)
            {
                SetMask (ref mask, ParamOverride.MaskX, maskX);
                if (!maskX)
                {
                    overrideParam.value.start = profileParam.value.start;
                }
                SetMask (ref mask, ParamOverride.MaskY, maskY);
                if (!maskY)
                {
                    overrideParam.value.end = profileParam.value.end;
                }
                SetMask (ref mask, ParamOverride.MaskZ, maskZ);
                if (!maskZ)
                {
                    overrideParam.value.intensityMin = profileParam.value.intensityMin;
                }
                SetMask (ref mask, ParamOverride.MaskW, maskW);
                if (!maskW)
                {
                    overrideParam.value.intensityMax = profileParam.value.intensityMax;
                }
                SetMask (ref mask, ParamOverride.Mask4, mask4);
                if (!mask4)
                {
                    overrideParam.value.intensityScale = profileParam.value.intensityScale;
                }
                SetMask (ref mask, ParamOverride.Mask5, mask5);
                if (!mask5)
                {
                    overrideParam.value.fallOff = profileParam.value.fallOff;
                }
            }
            if (overrideParam != null)
            {
                if (valueChange)
                {
                    overrideParam.value = editValue;
                }
                return valueChange;
            }
            else
            {
                if (valueChange)
                {
                    sp.FindPropertyRelative("start").floatValue = editValue.start;
                    sp.FindPropertyRelative("end").floatValue = editValue.end;
                    sp.FindPropertyRelative("intensityMin").floatValue = editValue.intensityMin;
                    sp.FindPropertyRelative("intensityMax").floatValue = editValue.intensityMax;
                    sp.FindPropertyRelative("intensityScale").floatValue = editValue.intensityScale;
                    sp.FindPropertyRelative("fallOff").floatValue = editValue.fallOff;
                }
                return true;
            }
        }

        public override void InnerResetValue()
        {
            sp.FindPropertyRelative("start").floatValue = attr.start;
            sp.FindPropertyRelative("end").floatValue = attr.end;
            sp.FindPropertyRelative("intensityMin").floatValue = attr.intensityMin;
            sp.FindPropertyRelative("intensityMax").floatValue = attr.intensityMax;
            sp.FindPropertyRelative("intensityScale").floatValue = attr.intensityScale;
            sp.FindPropertyRelative("fallOff").floatValue = attr.fallOff;

            profileParam.value.start = attr.start;
            profileParam.value.end = attr.end;
            profileParam.value.intensityMin = attr.intensityMin;
            profileParam.value.intensityMax = attr.intensityMax;
            profileParam.value.intensityScale = attr.intensityScale;
            profileParam.value.fallOff = attr.fallOff;
        }

        private bool DrawSlider(string desc, ref float v, float min, float max, bool drawToggle, ref bool toggleValue, int index, float width)
        {
            BeginDraw(drawToggle, ref toggleValue, ref width);

            desc = string.Format("{0}({1}-{2})", desc, min, max);
            v = EditorGUILayout.Slider(desc, v, min, max, GUILayout.Width(width));

            return EndDraw(drawToggle, toggleValue, index);
        }

        private bool DrawFloat(string desc, ref float v, bool drawToggle, ref bool toggleValue, int index, float width)
        {
            BeginDraw(drawToggle, ref toggleValue, ref width);
            v = EditorGUILayout.FloatField(desc, v, GUILayout.Width(width));
            return EndDraw(drawToggle, toggleValue, index);
        }

        private bool EndDraw(bool drawToggle, bool toggleValue, int index)
        {
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            bool vchange = EditorGUI.EndChangeCheck();
            if (vchange)
            {
                envParam.valueChangeMask = (byte)(1 << index);
            }
            if (drawToggle && toggleValue)
            {
                byte mask = (byte)(1 << index);
                AnimMaskToggle(mask);
            }
            return vchange;
        }

        private void BeginDraw(bool drawToggle, ref bool toggleValue, ref float width)
        {
            EditorGUILayout.BeginHorizontal();
            if (drawToggle)
            {
                MaskToggle(ref toggleValue);
            }
            width -= 100;
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(width));
            EditorGUI.BeginChangeCheck();
        }
    }
}