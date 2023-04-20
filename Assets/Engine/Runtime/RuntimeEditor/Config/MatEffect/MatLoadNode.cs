#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
namespace CFEngine
{
    [Node (module = "MatEffect")]
    [Serializable]
    public class MatLoad : MatEffectNode
    {
        [Editable("needMainTex")] public bool needMainTex = false;
        [Editable("needNormal")] public bool needNormal = false;
        [Editable("copyFromSrc")] public bool copyFromSrc = false;
        [Editable("defaultMat")] public Material defaultMat = null;
        [Editable("matCanEdit")] public bool matCanEdit = false;
        public override MatEffectTemplate GetEffectTemplate ()
        {
            if (met == null)
            {
                met = new MatEffectTemplate ()
                {
                    effectType = (int)MatEffectType.MatLoad,
                };
            }
            met.flag.SetFlag(MatEffectTemplate.Flag_MainTex, needMainTex);
            met.flag.SetFlag(MatEffectTemplate.Flag_NormalTex, needNormal);
            met.flag.SetFlag(MatEffectTemplate.Flag_CopyFromSrc, copyFromSrc);
            FillData(met);
            return met;
        }
        public override void InitData(ref float x, ref float y, ref float z, ref float w, ref string path, ref uint param)
        {
            if (defaultMat != null)
            {
                string fullpath = AssetsPath.GetAssetFullPath(defaultMat, out var ext);
                path = AssetsPath.GetDir(fullpath) + "/";
                ext = ext.ToLower();
                if (ext.EndsWith(ResObject.ResExt_Mat))
                {
                    x = ResObject.Mat;
                }
                else
                {
                    DebugLog.AddErrorLog("only support tga tex");
                    path = "";
                }
            }
        }
        public override void InitRes(ref UnityEngine.Object asset)
        {
            asset = defaultMat;
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

        public override void RestoreRes(MatEffectData data)
        {
            if (!string.IsNullOrEmpty(data.path))
            {
                data.asset = AssetDatabase.LoadAssetAtPath(data.path + ".mat", typeof(Material)) as Material;
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
                if(matCanEdit)
                {
                    data.asset = asset;
                    RefreshRes(data);
                }               
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
        }
    }
}
#endif