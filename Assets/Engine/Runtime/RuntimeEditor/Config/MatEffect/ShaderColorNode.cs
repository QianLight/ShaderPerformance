#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
namespace CFEngine
{
    [Node (module = "MatEffect")]
    [Serializable]
    public class ShaderColor : ParamLerpNode
    {
        [Editable ("color")] public Color color = Color.white;
        [Editable ("hdr")] public bool hdr;
        [Editable("hdr")] public bool copySrcParam;
        [Editable ("alpha")] public bool alpha;
        [Editable ("alphaDesc")] public string alphaDesc;
        [Editable ("alphaModifyType")] public ValueModifyType alphaModifyType = ValueModifyType.Const;
        [Editable ("defaultAlpha")] public float defaultAlpha;

        [Editable ("minA")] public float minA = 0;
        [Editable ("maxA")] public float maxA = 1;

        public override MatEffectTemplate GetEffectTemplate ()
        {
            if (met == null)
            {
                met = new MatEffectTemplate ()
                {
                effectType = (int) MatEffectType.Param
                };
            }

            met.keyID = Shader.PropertyToID (key);
            met.v = color;
            //met.lerpType = (byte) lerpType;
            met.shaderKey = key;
            if (lerpType == LerpType.Lerp01)
            {
                met.lerpMask = ConvertMask(maskLerpType, fadeInType, fadeOutType);
            }
            else
            {
                met.lerpMask = 0;
            }
            met.flag.SetFlag (MatEffectTemplate.Flag_IsColor, true);
            met.flag.SetFlag(MatEffectTemplate.Flag_IsHdr, hdr);
            met.flag.SetFlag(MatEffectTemplate.Flag_CopySrcParam, copySrcParam);
            
            FillData(met);
            
            return met;
        }
        public override void InitData (ref float x, ref float y, ref float z, ref float w, ref string path, ref uint param)
        {
            x = color.r;
            y = color.g;
            z = color.b;
            w = color.a;
            if (alpha)
            {
                w = defaultAlpha;
            }
        }

        public override void OnGUI (MatEffectData data)
        {
            EditorGUILayout.BeginHorizontal ();
            Color c = new Color (data.x, data.y, data.z, data.w);
            EditorGUILayout.LabelField ("Color", GUILayout.MaxWidth (100));
            c = EditorGUILayout.ColorField (emptyTitle, c, false, alpha, hdr, GUILayout.MaxWidth (300));
            data.x = c.r;
            data.y = c.g;
            data.z = c.b;
            data.w = c.a;
            EditorGUILayout.EndHorizontal ();
            switch (alphaModifyType)
            {
                case ValueModifyType.Const:
                    data.w = defaultAlpha;
                    break;
                case ValueModifyType.ConstModify:
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.LabelField (alphaDesc, GUILayout.MaxWidth (100));
                    data.w = EditorGUILayout.FloatField(data.w, GUILayout.MaxWidth (300));
                    EditorGUILayout.EndHorizontal ();
                    break;
                case ValueModifyType.Range:
                    EditorGUILayout.BeginHorizontal ();
                    EditorGUILayout.LabelField (alphaDesc, GUILayout.MaxWidth (100));
                    data.w = EditorGUILayout.Slider (data.w, minA, maxA, GUILayout.MaxWidth (300));
                    EditorGUILayout.EndHorizontal ();
                    break;
            }
        }
    }
}
#endif