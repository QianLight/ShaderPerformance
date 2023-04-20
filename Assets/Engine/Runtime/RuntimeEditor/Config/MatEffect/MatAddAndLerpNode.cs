#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
namespace CFEngine
{
    [Node (module = "MatEffect")]
    [Serializable]
    public class MatAddAndLerp : ParamLerpNode
    {
        [Editable("needMainTex")] public bool needMainTex = false;
        [Editable("needNormal")] public bool needNormal = false;
        
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
                    effectType = (int)MatEffectType.AddMatAndLerp,
                };
            }
            met.flag.SetFlag(MatEffectTemplate.Flag_MainTex, needMainTex);
            met.flag.SetFlag(MatEffectTemplate.Flag_NormalTex, needNormal);
            
            met.keyID = Shader.PropertyToID (key);
            met.v = color;
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

        public override void InitRes(ref UnityEngine.Object asset)
        {
            asset = null;
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

        private void RefreshRes (MatEffectData data)
        {
            if (data.asset == null)
            {
                data.path = "";
            }
            else
            {
                string fullpath = AssetsPath.GetAssetFullPath (data.asset, out var ext);
                data.path = AssetsPath.GetDir(fullpath) + "/";
                ext = ext.ToLower ();
                if (ext.EndsWith (ResObject.ResExt_Mat))
                {
                    data.x = ResObject.Mat;
                }
                else
                {
                    DebugLog.AddErrorLog ("only support tga tex");
                    data.asset = null;
                    data.path = "";
                }
            }
        }
        public override void OnGUI (MatEffectData data)
        {
            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("Mat", GUILayout.MaxWidth (100));
            EditorGUI.BeginChangeCheck ();
            var asset = EditorGUILayout.ObjectField ("", data.asset, typeof (Material), false, GUILayout.MaxWidth (300)) as Material;
            if (EditorGUI.EndChangeCheck ())
            {
                data.asset = asset;
                RefreshRes(data);
            }
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField (data.path, GUILayout.MaxWidth (400));
            EditorGUILayout.EndHorizontal ();
            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Refresh", GUILayout.MaxWidth (80)))
            {
                RefreshRes (data);
            }
            EditorGUILayout.EndHorizontal ();
            
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