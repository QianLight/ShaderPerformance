using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CFEngine.Editor
{
    [CFDecorator (typeof (CFLightingAttribute))]
    public sealed class LightingDecorator : AttributeDecorator<CFLightingAttribute, LightingParam>
    {
        private LightWrapper lightWrapper;
        private LightingParam editParam1;
        private LightingParam editParam2;
        private static int s_FoldoutHash = "Foldout".GetHashCode ();
        private static readonly GUIContent[] s_XYZLabels = { EditorGUIUtility.TrTextContent ("X"), EditorGUIUtility.TrTextContent ("Y"), EditorGUIUtility.TrTextContent ("Z") };
        public override byte GetMaskFlag ()
        {
            return (byte) (ParamOverride.MaskX | ParamOverride.MaskY | ParamOverride.MaskZ);
        }
        protected override void InitEditValue ()
        {
            if (overrideParam != null)
            {
                editParam = overrideParam;
                editParam1 = overrideParam;
                editParam2 = overrideParam;
                lightWrapper = overrideParam.value.lightWrapper;
                if (!maskX)
                {
                    editParam = profileParam;
                }
                if (!maskY)
                {
                    editParam1 = profileParam;
                }
                if (!maskZ)
                {
                    editParam2 = profileParam;
                }
            }
            else
            {
                lightWrapper = profileParam.value.lightWrapper;
                editParam = profileParam;
                editParam1 = profileParam;
                editParam2 = profileParam;
            }
        }

        protected override bool EndOverride (ref byte mask)
        {
            if (overrideToggleChange)
            {
                SetMask (ref mask, ParamOverride.MaskX, maskX);
                var light = overrideParam.value.lightWrapper != null?overrideParam.value.lightWrapper.light : null;
                if (!maskX)
                {
                    overrideParam.value.lightColor = profileParam.value.lightColor;
                    if (light != null)
                    {
                        light.color = profileParam.value.lightColor;
                    }
                }
                SetMask (ref mask, ParamOverride.MaskY, maskY);
                if (!maskY)
                {
                    overrideParam.value.lightDir.w = profileParam.value.lightDir.w;
                }
                SetMask (ref mask, ParamOverride.MaskZ, maskZ);
                if (!maskZ)
                {
                    var dir = profileParam.value.lightDir;
                    overrideParam.value.lightDir.x = dir.x;
                    overrideParam.value.lightDir.y = dir.y;
                    overrideParam.value.lightDir.z = dir.z;
                }
            }

            return overrideParam != null?valueChange : true;
        }

        public override void InnerOnGUI (
            GUIContent title,
            float width,
            uint flag)
        {
            
            EditorCommon.BeginGroup (title.text, new Vector4 (0, 0, width, 120), true);
            var light = lightWrapper != null?lightWrapper.light : null;
            if (light != null)
            {
                ListElementContext lec = new ListElementContext ();
                lec.draw = true;
                float lineHeight = 21;
                int lineCount = 1 + 1 + 1 + 1;
                lec.rect = GUILayoutUtility.GetRect (1, lineHeight * lineCount);
                ToolsUtility.InitListContext (ref lec, lineHeight);
                EditorGUI.BeginChangeCheck ();
                ToolsUtility.ObjectField (ref lec, "", 0, ref light, typeof (Light), width - 100);
                if (EditorGUI.EndChangeCheck ())
                {
                    if (overrideParam == null)
                    {
                        UndoRecord ("Light");
                        lightWrapper.light = light;
                    }
                }
                //color
                ToolsUtility.NewLine (ref lec, 0);
                bool resetline = MaskToggle (ref lec, ref maskX);
                EditorGUI.BeginChangeCheck ();
                var color = editParam.value.lightColor;
                ToolsUtility.ColorField (ref lec, "", 0, ref color, width - 100, resetline);
                if (EditorGUI.EndChangeCheck ())
                {
                    UndoRecord ("LightColor");
                    // if ((flag & Flag_EditSrcObj) != 0)
                    {
                        UndoRecord (light, "LightColor");
                        light.color = color;
                    }
                    // else
                    // {
                    //     UndoRecord ("LightColor");
                    // }
                    editParam.value.lightColor = color;
                    valueChange = true;
                }

                //intensity
                ToolsUtility.NewLine (ref lec, 0);
                resetline = MaskToggle (ref lec, ref maskY);
                EditorGUI.BeginChangeCheck ();
                float intensity = editParam1.value.lightDir.w;
                ToolsUtility.Slider (ref lec, "", 0, ref intensity, 0, 20, width - 100, resetline);
                if (EditorGUI.EndChangeCheck ())
                {

                    // if ((flag & Flag_EditSrcObj) != 0)
                    {
                        UndoRecord (light, "LightIntensity");
                        light.intensity = intensity;

                    }
                    // else
                    // {
                    //     UndoRecord ("LightIntensity");
                    // }
                    editParam1.value.lightDir.w = intensity;
                    valueChange = true;
                }

                //rot
                //if (overrideParam == null)
                {
                    var rotGui = editParam2.value.lightWrapper != null ?
                        editParam2.value.lightWrapper.rotGui : null;
                    if (rotGui != null)
                    {
                        if (rotGui.BeginGUI ())
                        {
                            ToolsUtility.NewLine (ref lec, 0);
                            resetline = MaskToggle (ref lec, ref maskZ);
                            var context = rotGui.context;

                            GUIContent label = EditorGUI.BeginProperty (lec.rect, EditorGUIUtility.TrTextContent ("Rot"), rotGui.RotationSp);
                            int id = GUIUtility.GetControlID (s_FoldoutHash, FocusType.Keyboard, lec.rect);
                            ToolsUtility.MultiFieldPrefixLabel (ref lec, label, 30, id, 3, resetline);
                            EditorGUI.BeginChangeCheck ();
                            ToolsUtility.MultiFloatField (ref lec, s_XYZLabels, context.EulerFloats, width - 140);

                            if (EditorGUI.EndChangeCheck ())
                            {
                                rotGui.EndGUI ();
                                var dir = light.transform.rotation * -Vector3.forward;
                                editParam2.value.lightDir.x = dir.x;
                                editParam2.value.lightDir.y = dir.y;
                                editParam2.value.lightDir.z = dir.z;                                
                                valueChange = true;
                            }
                            EditorGUI.EndProperty ();
                        }
                    }
                }
            }
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

        }
    }
}