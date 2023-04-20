#if UNITY_EDITOR
using System;
using UnityEngine;
using UnityEditor;

namespace CFEngine
{
    public enum ValueModifyType
    {
        Const,
        ConstModify,
        Range,
        Toggle,
        IntRange,
    }

    [Serializable]
    public abstract class MatEffectNode : AbstractNode
    {
        [Input] public int pre;
        [Output] public int next;

        [Editable("RunOnDisable")] public bool runOnDisable;

        protected MatEffectTemplate met;

        public static GUIContent emptyTitle = new GUIContent("");
        public override object OnRequestValue(Port port)
        {
            return pre + 1;
        }

        public virtual MatEffectTemplate GetEffectTemplate()
        {
            return met;
        }
        public virtual void InitData(ref float x, ref float y, ref float z, ref float w, 
            ref string path, ref uint param)
        {

        }
        public virtual void InitRes(ref UnityEngine.Object asset)
        {

        }
        public virtual void FillData(MatEffectTemplate met)
        {
            met.flag.SetFlag(MatEffectTemplate.Flag_RunOnDisable, runOnDisable);
        }
        public virtual void OnGUI(MatEffectData data)
        {

        }

        public virtual void RestoreRes(MatEffectData data)
        {

        }

        public virtual void UpdateEffect(IEntityHandler e, MatEffectData med)
        {

        }
    }

    [Serializable]
    public abstract class ParamLerpNode : MatEffectNode
    {
        [Editable("key")] public string key;
        [Editable("lerpType")] public LerpType lerpType = LerpType.None;

        [Editable("maskLerpType")]
        [Mask(new string[] { "MaskX", "MaskY", "MaskZ", "MaskW" })]
        public uint maskLerpType;
        [Editable("fadeInType")]
        [Mask(new string[] { "FadeIn_0V", "FadeIn_V0", "FadeIn_1V", "FadeIn_V1" })]
        public uint fadeInType;
        [Editable("fadeOutType")]
        [Mask(new string[] { "FadeOut_0V", "FadeOut_V0", "FadeOut_1V", "FadeOut_V1" })]
        public uint fadeOutType;

        public static uint ConvertMask(uint maskLerp, uint fadeIn, uint fadeOut)
        {
            uint mask = (maskLerp & 0xfu) | ((fadeIn & 0xfu) << 16) | ((fadeOut & 0xfu) << 20);
            return mask;
        }
    }

    [Serializable]
    public abstract class ParamNode : ParamLerpNode
    {
        [Editable("maskX")] public ValueModifyType maskX;
        [Editable("xDesc")] public string xDesc;
        [Editable("X")] public float X;
        [Editable("minX")] public float minX;
        [Editable("maxX")] public float maxX;
        [Editable("maskY")] public ValueModifyType maskY;
        [Editable("yDesc")] public string yDesc;
        [Editable("Y")] public float Y;
        [Editable("minY")] public float minY;
        [Editable("maxY")] public float maxY;
        [Editable("maskZ")] public ValueModifyType maskZ;
        [Editable("zDesc")] public string zDesc;
        [Editable("Z")] public float Z;
        [Editable("minZ")] public float minZ;
        [Editable("maxZ")] public float maxZ;
        [Editable("maskW")] public ValueModifyType maskW;
        [Editable("wDesc")] public string wDesc;
        [Editable("W")] public float W;
        [Editable("minW")] public float minW;
        [Editable("maxW")] public float maxW;

        public override void FillData(MatEffectTemplate met)
        {
            base.FillData(met);
            met.keyID = Shader.PropertyToID(key);
            met.v = new Vector4(X, Y, Z, W);
            //met.lerpType = (byte)lerpType;
            met.shaderKey = key;
            if (lerpType == LerpType.Lerp01)
            {
                met.lerpMask = ConvertMask(maskLerpType, fadeInType, fadeOutType);
            }
            else
            {
                met.lerpMask = 0;
            }
        }

        public void ValueGUI(ref float v,
            ValueModifyType mask, float defaultV, float min, float max, string desc)
        {
            switch (mask)
            {
                case ValueModifyType.Const:
                    v = defaultV;
                    break;
                case ValueModifyType.ConstModify:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(desc, GUILayout.MaxWidth(200));
                    v = EditorGUILayout.FloatField(v, GUILayout.MaxWidth(300));
                    EditorGUILayout.EndHorizontal();
                    break;
                case ValueModifyType.Range:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(desc, GUILayout.MaxWidth(200));
                    v = EditorGUILayout.Slider(v, min, max, GUILayout.MaxWidth(300));
                    EditorGUILayout.EndHorizontal();
                    break;
                case ValueModifyType.Toggle:
                    bool enable = v > ((min + max) * 0.5f);
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(desc, GUILayout.MaxWidth(200));
                    enable = EditorGUILayout.Toggle(enable, GUILayout.MaxWidth(300));
                    EditorGUILayout.EndHorizontal();
                    v = enable ? max : min;
                    break;
                case ValueModifyType.IntRange:
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(desc, GUILayout.MaxWidth(200));
                    v = EditorGUILayout.IntSlider((int)v, (int)min, (int)max, GUILayout.MaxWidth(300));
                    EditorGUILayout.EndHorizontal();
                    break;
            }
        }

        public void ParamGUI(MatEffectData data)
        {
            ValueGUI(ref data.x, maskX, X, minX, maxX, xDesc);
            ValueGUI(ref data.y, maskY, Y, minY, maxY, yDesc);
            ValueGUI(ref data.z, maskZ, Z, minZ, maxZ, zDesc);
            ValueGUI(ref data.w, maskW, W, minW, maxW, wDesc);
        }
    }

}
#endif