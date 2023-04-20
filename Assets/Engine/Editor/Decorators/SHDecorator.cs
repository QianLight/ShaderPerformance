using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CFEngine.Editor
{
    [CFDecorator (typeof (CFSHAttribute))]
    public sealed class SHDecorator : AttributeDecorator<CFSHAttribute, SHParam>
    {
        public override void InnerOnGUI (
            GUIContent title,
            float width,
            uint flag)
        {
            ListElementContext lec = new ListElementContext ();
            lec.draw = true;
            float lineHeight = 21;
            int lineCount = 1 + 2; //head debug
            if (editParam.value.ambientMode == AmbientType.Flat)
            {
                lineCount += 1;
            }
            else if (editParam.value.ambientMode == AmbientType.Trilight)
            {
                lineCount += 3;
            }
            else if (editParam.value.ambientMode == AmbientType.SkyBox)
            {
                lineCount += 2;
            }
            if (editParam.value.debug || DebugCurrentParamValue)
            {
                lineCount += 3;
            }

            lec.rect = GUILayoutUtility.GetRect (1, lineHeight * lineCount);

            ToolsUtility.InitListContext (ref lec, lineHeight);
            var resetline = MaskToggle (ref lec, ref maskX);
            EditorGUI.BeginChangeCheck ();
            ToolsUtility.Label (ref lec, title.text, 100, resetline);

            ToolsUtility.NewLine (ref lec, 10);
            if (editParam.value.ambientMode == AmbientType.Flat)
            {
                ToolsUtility.ColorField (ref lec, "FlatColor", 100, ref editParam.value.flatColor, false, true, 120, true);
            }
            else if (editParam.value.ambientMode == AmbientType.Trilight)
            {
                ToolsUtility.ColorField (ref lec, "SkyColor", 100, ref editParam.value.skyColor, false, true, 120, true);
                ToolsUtility.NewLineWithOffset (ref lec);
                ToolsUtility.ColorField (ref lec, "EquatorColor", 100, ref editParam.value.equatorColor, false, true, 120, true);
                ToolsUtility.NewLineWithOffset (ref lec);
                ToolsUtility.ColorField (ref lec, "GroundColor", 100, ref editParam.value.groundColor, false, true, 120, true);
            }
            else if (editParam.value.ambientMode == AmbientType.SkyBox)
            {
                lec.height = 18;
                ToolsUtility.ObjectField (ref lec, "SkyCube", 100, ref editParam.value.skyCube, typeof (Cubemap), 32, true);
                lec.height = lineHeight;
                ToolsUtility.Label (ref lec, EditorCommon.GetAssetPath (editParam.value.skyCube, true), 500);
                ToolsUtility.NewLineWithOffset (ref lec);
                ToolsUtility.Slider (ref lec, "Intensity", 100, ref editParam.value.skyIntensity, 0.1f, 5, 200, true);
            }
            if (EditorGUI.EndChangeCheck ())
            {
                bool bake = true;
                if (editParam.value.ambientMode == AmbientType.SkyBox &&
                    editParam.value.skyCube == null)
                {
                    bake = false;
                }
                if (bake)
                {
#if !UNITY_ANDROID
                    EnviromentSHBakerHelper.singleton.BakeSkyBoxSphericalHarmonics (ref editParam.value);
#endif
                }

                valueChange = true;
            }
            ToolsUtility.NewLine (ref lec, 10);
            ToolsUtility.EnumPopup (ref lec, "AmbientMode", 120, ref editParam.value.ambientMode, 100, true);
            if (ToolsUtility.Button (ref lec, "Bake", 80))
            {
                Debug.LogError("安卓平台下不支持ComputeShader，无法烘焙SH。");
#if !UNITY_ANDROID
                EnviromentSHBakerHelper.singleton.BakeSkyBoxSphericalHarmonics (ref editParam.value);
#endif
                valueChange = true;
            }
            ToolsUtility.NewLine (ref lec, 10);
            ToolsUtility.Toggle (ref lec, "Debug", 100, ref editParam.value.debug, true);
            if (editParam.value.debug || DebugCurrentParamValue)
            {
                ToolsUtility.NewLine (ref lec, 10);
                string shAr = EditorCommon.Vec4Str ("shAr", ref editParam.value.shAr);
                string shAg = EditorCommon.Vec4Str ("shAg", ref editParam.value.shAg);
                string shAb = EditorCommon.Vec4Str ("shAb", ref editParam.value.shAb);
                ToolsUtility.Label (ref lec, string.Format ("{0} {1} {2}", shAr, shAg, shAb), 500, true);
                ToolsUtility.NewLine (ref lec, 10);
                string shBr = EditorCommon.Vec4Str ("shBr", ref editParam.value.shBr);
                string shBg = EditorCommon.Vec4Str ("shBg", ref editParam.value.shBg);
                string shBb = EditorCommon.Vec4Str ("shBb", ref editParam.value.shBb);
                ToolsUtility.Label (ref lec, string.Format ("{0} {1} {2}", shBr, shBg, shBb), 500, true);
                ToolsUtility.NewLine (ref lec, 10);
                string shC = EditorCommon.Vec4Str ("shC", ref editParam.value.shC);
                ToolsUtility.Label (ref lec, shC, 200, true);
            }
            if(valueChange)
            {
                editParam.data.Copy(ref editParam.value);
            }
        }
        public override void InnerResetValue ()
        {
            if (profileParam.value.ambientMode == AmbientType.Flat)
            {
                profileParam.value.flatColor = attr.flatColor;
            }
            else if (profileParam.value.ambientMode == AmbientType.Trilight)
            {
                profileParam.value.skyColor = attr.skyColor;
                profileParam.value.equatorColor = attr.equatorColor;
                profileParam.value.groundColor = attr.groundColor;
            }
            else if (profileParam.value.ambientMode == AmbientType.SkyBox)
            {
                profileParam.value.skyIntensity = attr.skyIntensity;
                profileParam.value.skyCube = null;
            }
        }

        public override void SetInfo(string settingname, string name, Attribute attribute,
            EnvParam envParam)
        {
            if (envParam.param is SHParam)
            {
                var param = envParam.param as SHParam;
                //param.curve.name = name;
            }
        }
    }
}