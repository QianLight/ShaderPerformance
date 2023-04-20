using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CFEngine.Editor
{

    public abstract class AttributeDecorator<S> : AttributeDecorator where S : Attribute
    {
        protected S attr;
        protected SerializedParameter spo;
        protected SerializedProperty sp;
        protected bool overrideToggleChange = false;
        protected bool valueChange = false;
        protected void UndoRecord (string name)
        {
            Undo.RecordObject (sp.serializedObject.targetObject, name);
        }
        protected void UndoRecord (UnityEngine.Object obj, string name)
        {
            Undo.RecordObject (obj, name);
        }
        protected virtual void InitEditValue ()
        {

        }

        protected virtual bool EndOverride (ref byte mask)
        {
            return true;
        }
        public virtual byte GetMaskFlag ()
        {
            return ParamOverride.MaskX;
        }

        public virtual void InnerOnGUI (
            GUIContent title,
            float width,
            uint flag)
        {

        }

        public virtual void InnerResetValue ()
        {

        }

        protected static void SetMask (ref byte byteMask, byte mask, bool add)
        {
            if (add)
            {
                byteMask |= mask;
            }
            else
            {
                byteMask &= (byte) (~(mask));
            }
        }
    }

    public abstract class AttributeDecorator<S, T> : AttributeDecorator<S> where S : Attribute where T : ParamOverride
    {
        protected T profileParam;
        protected T overrideParam;
        protected T editParam;

        protected EnvParam envParam;

        protected bool maskX = false;
        protected bool maskY = false;
        protected bool maskZ = false;
        protected bool maskW = false;
        protected bool mask4 = false;
        protected bool mask5 = false;

        protected override void InitEditValue ()
        {
            if (overrideParam != null && maskX)
            {
                editParam = overrideParam;
            }
            else
            {
                editParam = profileParam;
            }
        }
        protected override bool EndOverride (ref byte mask)
        {
            if (overrideToggleChange)
            {
                SetMask (ref mask, ParamOverride.MaskX, maskX);
                if (!maskX)
                {
                    overrideParam.SetValue (profileParam, false);
                }
            }
            base.EndOverride (ref mask);
            if (valueChange)
            {
                envParam.valueChangeMask = ParamOverride.MaskX;
            }
            return overrideParam != null?valueChange : true;
        }

        protected virtual bool BeginClassSerializeProperty (
            Attribute attribute)
        {
            profileParam = null;
            valueChange = false;
            overrideToggleChange = false;

            sp = spo.value;
            attr = attribute as S;
            if (sp.propertyType != GetSType ())
            {
                return false;
            }
            if (spo is ClassSerializedParameterOverride)
            {
                var cspo = spo as ClassSerializedParameterOverride;
                profileParam = cspo.param as T;
            }
            return true;
        }

        private void CalcMask (byte mask, byte maskFlag, byte maskBit, ref bool maskValue)
        {
            maskValue = false;
            if ((maskFlag & maskBit) != 0)
                maskValue = (mask & maskBit) != 0;
        }
        protected bool MaskToggle (ref ListElementContext lec, ref bool mask)
        {
            if (overrideParam != null)
            {
                EditorGUI.BeginChangeCheck ();
                ToolsUtility.Toggle (ref lec, "", 40, ref mask, true);
                if (EditorGUI.EndChangeCheck ())
                {
                    overrideToggleChange = true;
                    // valueChange = true;
                }
                return false;
            }
            return true;
        }
        protected void MaskToggle (ref bool mask)
        {
            if (overrideParam != null)
            {
                EditorGUILayout.BeginVertical (GUILayout.MaxWidth (40));
                EditorGUI.BeginChangeCheck ();
                mask = EditorGUILayout.Toggle ("", mask, GUILayout.MaxWidth (40));
                if (EditorGUI.EndChangeCheck ())
                {
                    overrideToggleChange = true;
                    // valueChange = true;
                }
                EditorGUILayout.EndVertical ();
            }
        }
        protected void AnimMaskToggle (byte mask)
        {
            if (envParam.hasAnim)
            {
                bool editing = (envParam.animMask & mask) != 0;
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal ();
                if (editing)
                {
                    EditorGUILayout.LabelField ("Editing", GetRedTextSyle (), GUILayout.Width (100));
                    editing = EditorGUILayout.Toggle ("", editing);
                }
                else
                {
                    EditorGUILayout.LabelField ("NotEdit", GUILayout.Width (100));
                    editing = EditorGUILayout.Toggle ("", editing);
                }

                if (editing)
                {
                    envParam.animMask |= mask;
                }
                else
                {
                    envParam.animMask &= (byte) (~(mask));
                }
                EditorGUILayout.EndHorizontal ();
                EditorGUI.indentLevel--;
            }
        }
        private void InitParam ()
        {
            byte maskFlag = GetMaskFlag ();
            CalcMask (envParam.valueMask, maskFlag, ParamOverride.MaskX, ref maskX);
            CalcMask (envParam.valueMask, maskFlag, ParamOverride.MaskY, ref maskY);
            CalcMask (envParam.valueMask, maskFlag, ParamOverride.MaskZ, ref maskZ);
            CalcMask (envParam.valueMask, maskFlag, ParamOverride.MaskW, ref maskW);

            overrideParam = null;
            T param = envParam.param as T;
            if (param != null)
            {
                overrideParam = param;
            }
            InitEditValue ();
        }

        public override bool OnGUI (
            SerializedParameter spo,
            GUIContent title,
            Attribute attribute,
            EnvParam envParam,
            uint flag,
            out bool overrideChange)
        {
            overrideChange = false;
            this.envParam = envParam;
            this.spo = spo;
            if (BeginClassSerializeProperty (attribute))
            {
                InitParam ();
                float w = EditorGUIUtility.currentViewWidth;
                InnerOnPreGUI (w);
                InnerOnGUI (title, w, flag);
                InnerOnPostGUI ();
                if (sp.serializedObject != null && sp.serializedObject.targetObject != null)
                    sp.serializedObject.ApplyModifiedProperties ();
                overrideChange = overrideToggleChange;
                return EndOverride (ref envParam.valueMask);
            }
            return true;
        }
        public virtual void InnerOnPreGUI (float width)
        {
            EditorCommon.BeginGroup ("", new Vector4 (0, 0, width, 50), true);
        }
        public virtual void InnerOnPostGUI ()
        {
            if (overrideParam != null && maskX)
            {
                EditorGUILayout.BeginHorizontal ();
                AnimMaskToggle (ParamOverride.MaskX);
                EditorGUILayout.EndHorizontal ();
            }
            EditorCommon.EndGroup ();
        }
        public override void ResetValue (SerializedParameter spo, Attribute attribute)
        {
            base.ResetValue (spo, attribute);
            sp = spo.value;
            attr = attribute as S;
            InnerResetValue ();
            sp.serializedObject.ApplyModifiedProperties ();
        }
    }

    [CFDecorator (typeof (CFRangeAttribute))]
    public sealed class RangeDecorator : AttributeDecorator<CFRangeAttribute>
    {
        public override bool OnGUI (
            SerializedParameter spo,
            GUIContent title,
            Attribute attribute,
            EnvParam envParam,
            uint flag,
            out bool overrideChange)
        {
            overrideChange = false;
            var attr = (CFRangeAttribute) attribute;
            var property = spo.value;
            if (property.propertyType == SerializedPropertyType.Float)
            {
                property.floatValue = EditorGUILayout.Slider (title, property.floatValue, attr.min, attr.max);
                return true;
            }

            if (property.propertyType == SerializedPropertyType.Integer)
            {
                property.intValue = EditorGUILayout.IntSlider (title, property.intValue, (int) attr.min, (int) attr.max);
                return true;
            }

            return false;
        }
        public override void ResetValue (
            SerializedParameter spo,
            Attribute attribute)
        {
            var attr = (CFRangeAttribute) attribute;
            var property = spo.value;

            if (property.propertyType != SerializedPropertyType.Float)
            {
                return;
            }
            property.floatValue = attr.v;
        }
    }

    [CFDecorator (typeof (CFEnumAttribute))]
    public sealed class EnumDecorator : AttributeDecorator<CFEnumAttribute, EnumParam>
    {
        int editValue = 0;
        public override SerializedPropertyType GetSType ()
        {
            return SerializedPropertyType.Integer;
        }
        protected override bool EndOverride (ref byte mask)
        {
            base.EndOverride (ref mask);
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
                    sp.intValue = editValue;
                }
                return true;
            }
        }
        public override void InnerOnGUI (
            GUIContent title,
            float width,
            uint flag)
        {
            int srcValue = sp.intValue;
            // DebugValue<int, EnumParam> (envParam.runtimeParam, ref srcValue);
            editValue = srcValue;
            if (overrideParam != null)
            {
                editValue = overrideParam.value;
                if (!maskX)
                {
                    editValue = srcValue;
                }
            }
            EditorGUILayout.BeginHorizontal ();
            MaskToggle (ref maskX);
            EditorGUILayout.BeginVertical (GUILayout.MaxWidth (width - 20));
            EditorGUI.BeginChangeCheck ();
            if (profileParam.gui != null)
            {
                editValue = profileParam.gui(editValue);
            }
            // editValue = EditorGUILayout.ColorField (title, editValue, false, attr.showAlpha, attr.hdr, GUILayout.MaxWidth (width - 20));
            if (EditorGUI.EndChangeCheck ())
            {
                valueChange = true;
            }
            EditorGUILayout.EndVertical ();
            EditorGUILayout.EndHorizontal ();
        }
        public override void ResetValue(SerializedParameter spo, Attribute attribute)
        {
            var attr = (CFEnumAttribute)attribute;
            var property = spo.value;

            if (property.propertyType != SerializedPropertyType.Enum)
                return;

            property.intValue = attr.v;
        }
        // public override void InnerResetValue ()
        // {
        //     sp.enumValueIndex = attr.v;
        // }
        // public override void SetInfo (string settingname, string name, Attribute attribute,
        //     EnvParam envParam)
        // {
        //     if (envParam.param is ColorParam)
        //     {
        //         var param = envParam.param as ColorParam;
        //         param.curve.name = name;
        //     }
        // }
    }

    [CFDecorator (typeof (CFObjRefAttribute))]
    public sealed class ObjRefDecorator : AttributeDecorator<CFObjRefAttribute>
    {
        public override bool OnGUI (
            SerializedParameter spo,
            GUIContent title,
            Attribute attribute,
            EnvParam envParam,
            uint flag,
            out bool overrideChange)
        {
            overrideChange = false;
            var attr = (CFObjRefAttribute) attribute;
            var property = spo.value;
            var cspo = spo as ClassSerializedParameterOverride;
            if (property.propertyType != SerializedPropertyType.String)
            {
                return false;
            }
            TransParam rawParam = cspo.param as TransParam;
            if (rawParam != null)
            {
                EditorGUI.BeginChangeCheck ();
                var t = (Transform) EditorGUILayout.ObjectField (attr.name, rawParam.t, typeof (Transform), true);
                if (EditorGUI.EndChangeCheck ())
                {
                    Undo.RecordObject (property.serializedObject.targetObject, property.serializedObject.targetObject.name);
                    string path = EditorCommon.GetSceneObjectPath (t);
                    rawParam.value = path;
                    rawParam.t = t;
                    property.serializedObject.ApplyModifiedProperties ();
                }
            }
            return true;
        }

        public override void ResetValue (SerializedParameter spo, Attribute attribute)
        {
            var cspo = spo as ClassSerializedParameterOverride;
            TransParam rawParam = cspo.param as TransParam;
            if (rawParam != null)
            {
                rawParam.t = null;
                rawParam.value = "";
            }
        }

    }

    [CFDecorator (typeof (CFCustomDrawAttribute))]
    public sealed class CustomDrawDecorator : AttributeDecorator<CFCustomDrawAttribute>
    {
        public override bool OnGUI (
            SerializedParameter spo,
            GUIContent title,
            Attribute attribute,
            EnvParam envParam,
            uint flag,
            out bool overrideChange)
        {
            overrideChange = false;
            var cspo = spo as ClassSerializedParameterOverride;
            if (!(cspo.param is ICustomDraw))
            {
                return false;
            }
            ICustomDraw customDraw = cspo.param as ICustomDraw;
            customDraw.OnDrawInspector ();

            return true;
        }
    }

    [CFDecorator (typeof (CFActiveAttribute))]
    public sealed class CFActiveAttributeDecorator : AttributeDecorator<CFActiveAttribute, ActiveParam>
    {
        bool editValue = false;
        public override SerializedPropertyType GetSType ()
        {
            return SerializedPropertyType.Boolean;
        }

        protected override bool EndOverride (ref byte mask)
        {
            base.EndOverride (ref mask);
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
                    sp.boolValue = editValue;
                }
                return true;
            }
        }

        public override void InnerOnGUI (
            GUIContent title,
            float width,
            uint flag)
        {
            bool srcValue = sp.boolValue;
            DebugValue<bool, ActiveParam> (envParam.runtimeParam, ref srcValue);
            editValue = srcValue;
            if (overrideParam != null)
            {
                editValue = overrideParam.value;
                if (!maskX)
                {
                    editValue = srcValue;
                }
            }

            EditorGUILayout.BeginHorizontal ();
            MaskToggle (ref maskX);
            EditorGUILayout.BeginVertical (GUILayout.MaxWidth (200));
            EditorGUI.BeginChangeCheck ();
            editValue = EditorGUILayout.Toggle (title, editValue);
            if (EditorGUI.EndChangeCheck ())
            {
                valueChange = true;
            }
            EditorGUILayout.EndVertical ();
            EditorGUILayout.EndHorizontal ();
        }
        public override void InnerResetValue ()
        {
            // sp.boolValue = attr.v;
        }

        public override void SetInfo (string settingname, string name, Attribute attribute,
            EnvParam envParam)
        {
            if (envParam.param is ActiveParam)
            {
                var param = envParam.param as ActiveParam;
                param.curve.name = string.Format ("{0}:{1}", settingname, name);
            }
        }
    }

    //ext decorator
    [CFDecorator (typeof (CFParam4Attribute))]
    public sealed class Param4UsageDecorator : AttributeDecorator<CFParam4Attribute, Vector4Param>
    {
        Vector4 editValue = Vector4.zero;
        public override SerializedPropertyType GetSType ()
        {
            return SerializedPropertyType.Vector4;
        }
        public override byte GetMaskFlag ()
        {
            return (byte) (ParamOverride.MaskX | ParamOverride.MaskY | ParamOverride.MaskZ | ParamOverride.MaskW);
        }
        private bool DrawData (ref float v, string vStr, float min, float max, float scale,
            C4DataType datatype, ref bool toggleValue, bool drawToggle, int index, float width)
        {
            bool vchange = false;
            EditorGUILayout.BeginHorizontal ();
            if (drawToggle && datatype != C4DataType.None)
            {
                MaskToggle (ref toggleValue);
            }
            width -= 100;
            EditorGUILayout.BeginVertical (GUILayout.MaxWidth (width));
            EditorGUI.BeginChangeCheck ();
            switch (datatype)
            {
                case C4DataType.None:
                    break;
                case C4DataType.FloatRange:
                    {
                        v = EditorGUILayout.Slider (string.Format ("{0}({1}-{2})", vStr, min * scale, max * scale), v, min, max, GUILayout.Width (width));
                    }
                    break;
                case C4DataType.IntRange:
                    {
                        v = EditorGUILayout.IntSlider (string.Format ("{0}({1}-{2})", vStr, min * scale, max * scale), (int) v, (int) min, (int) max, GUILayout.Width (width));
                    }
                    break;
                case C4DataType.Float:
                    {
                        v = EditorGUILayout.FloatField (vStr, v, GUILayout.Width (width));
                    }
                    break;
                case C4DataType.Int:
                    {
                        v = EditorGUILayout.IntField (vStr, (int) v, GUILayout.Width (width));
                    }
                    break;
                case C4DataType.Bool:
                    {
                        v = EditorGUILayout.Toggle (vStr, v > (min + 0.01f), GUILayout.Width (width)) ? max : min;
                    }
                    break;
            }
            if (EditorGUI.EndChangeCheck ())
            {
                envParam.valueChangeMask = (byte) (1 << index);
                vchange = true;
            }
            EditorGUILayout.EndVertical ();

            EditorGUILayout.EndHorizontal ();
            if (drawToggle && toggleValue)
            {
                byte mask = (byte) (1 << index);
                AnimMaskToggle (mask);
            }
            return vchange;
        }
        protected override bool EndOverride (ref byte mask)
        {
            if (overrideToggleChange)
            {
                SetMask (ref mask, ParamOverride.MaskX, maskX);
                if (!maskX)
                {
                    overrideParam.value.x = profileParam.value.x;
                }
                SetMask (ref mask, ParamOverride.MaskY, maskY);
                if (!maskY)
                {
                    overrideParam.value.y = profileParam.value.y;
                }
                SetMask (ref mask, ParamOverride.MaskZ, maskZ);
                if (!maskZ)
                {
                    overrideParam.value.z = profileParam.value.z;
                }
                SetMask (ref mask, ParamOverride.MaskW, maskW);
                if (!maskW)
                {
                    overrideParam.value.w = profileParam.value.w;
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
                    sp.vector4Value = editValue;
                }
                return true;
            }
        }

        public override void InnerOnGUI (
            GUIContent title,
            float width,
            uint flag)
        {
            Vector4 srcValue = sp.vector4Value;
            DebugValue<Vector4, Vector4Param> (envParam.runtimeParam, ref srcValue);
            editValue = srcValue;
            bool drawToggle = false;
            if (overrideParam != null)
            {
                editValue = overrideParam.value;
                if (!maskX)
                {
                    editValue.x = srcValue.x;
                }
                if (!maskY)
                {
                    editValue.y = srcValue.y;
                }
                if (!maskZ)
                {
                    editValue.z = srcValue.z;
                }
                if (!maskW)
                {
                    editValue.w = srcValue.w;
                }
                drawToggle = true;
            }
            float h = 10;
            if (attr.type0 != C4DataType.None)
                h += 30;
            if (attr.type1 != C4DataType.None)
                h += 30;
            if (attr.type2 != C4DataType.None)
                h += 30;
            if (attr.type3 != C4DataType.None)
                h += 30;

            EditorCommon.BeginGroup (title.text, new Vector4 (0, 0, width, h), true);
            valueChange |= DrawData (ref editValue.x, attr.v0Str, attr.min0, attr.max0, attr.scale0 > 0 ? attr.scale0 : 1, attr.type0, ref maskX, drawToggle, 0, width);
            valueChange |= DrawData (ref editValue.y, attr.v1Str, attr.min1, attr.max1, attr.scale1 > 0 ? attr.scale1 : 1, attr.type1, ref maskY, drawToggle, 1, width);
            valueChange |= DrawData (ref editValue.z, attr.v2Str, attr.min2, attr.max2, attr.scale2 > 0 ? attr.scale2 : 1, attr.type2, ref maskZ, drawToggle, 2, width);
            valueChange |= DrawData (ref editValue.w, attr.v3Str, attr.min3, attr.max3, attr.scale3 > 0 ? attr.scale3 : 1, attr.type3, ref maskW, drawToggle, 3, width);
            EditorCommon.EndGroup ();
        }

        public override void InnerOnPreGUI (float width)
        {

        }
        public override void InnerOnPostGUI ()
        {

        }
        public override void InnerResetValue ()
        {
            sp.vector4Value = new Vector4 (attr.default0, attr.default1, attr.default2, attr.default3);
        }
        public override void SetInfo (string settingname, string name, Attribute attribute,
            EnvParam envParam)
        {
            var attr = attribute as CFParam4Attribute;
            if (attr != null && envParam.param is Vector4Param)
            {
                var param = envParam.param as Vector4Param;
                param.curve0.name = attr.v0Str;
                param.curve1.name = attr.v1Str;
                param.curve2.name = attr.v2Str;
                param.curve3.name = attr.v3Str;
            }
        }

    }

    [CFDecorator (typeof (CFColorUsageAttribute))]
    public sealed class ColorUsageDecorator : AttributeDecorator<CFColorUsageAttribute, ColorParam>
    {
        Color editValue = Color.white;
        public override SerializedPropertyType GetSType ()
        {
            return SerializedPropertyType.Color;
        }
        protected override bool EndOverride (ref byte mask)
        {
            base.EndOverride (ref mask);
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
                    sp.colorValue = editValue;
                }
                return true;
            }
        }
        public override void InnerOnGUI (
            GUIContent title,
            float width,
            uint flag)
        {
            Color srcValue = sp.colorValue;
            DebugValue<Color, ColorParam> (envParam.runtimeParam, ref srcValue);
            editValue = srcValue;
            if (overrideParam != null)
            {
                editValue = overrideParam.value;
                if (!maskX)
                {
                    editValue = srcValue;
                }
            }
            EditorGUILayout.BeginHorizontal ();
            MaskToggle (ref maskX);
            EditorGUILayout.BeginVertical (GUILayout.MaxWidth (width - 20));
            EditorGUI.BeginChangeCheck ();
            editValue = EditorGUILayout.ColorField (title, editValue, false, attr.showAlpha, attr.hdr, GUILayout.MaxWidth (width - 20));
            if (EditorGUI.EndChangeCheck ())
            {
                valueChange = true;
            }
            EditorGUILayout.EndVertical ();
            EditorGUILayout.EndHorizontal ();
        }
        public override void InnerResetValue ()
        {
            sp.colorValue = attr.v;
        }
        public override void SetInfo (string settingname, string name, Attribute attribute,
            EnvParam envParam)
        {
            if (envParam.param is ColorParam)
            {
                var param = envParam.param as ColorParam;
                param.curve.name = name;
            }
        }
    }
}