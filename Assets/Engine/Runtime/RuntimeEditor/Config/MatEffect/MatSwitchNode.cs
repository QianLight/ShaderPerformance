#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
namespace CFEngine
{
    [Node (module = "MatEffect")]
    [Serializable]
    public class MatSwitch : ParamNode
    {
        [Editable ("effectMat")] public EEffectMaterial effectMat;
        [Editable ("canEdit")] public bool canEdit = true;
        [Editable("needMainTex")] public bool needMainTex = false;
        [Editable("needNormal")] public bool needNormal = false;
       
        public override MatEffectTemplate GetEffectTemplate ()
        {
            if (met == null)
            {
                met = new MatEffectTemplate ()
                {
                effectType = (int) MatEffectType.MatSwitch,
                };
            }
            FillData(met);
            met.param = (uint) effectMat;
            met.flag.SetFlag(MatEffectTemplate.Flag_MainTex, needMainTex);
            met.flag.SetFlag(MatEffectTemplate.Flag_NormalTex, needNormal);
            return met;
        }
        public override void InitData (ref float x, ref float y, ref float z, ref float w, ref string path, ref uint param)
        {
            param = (uint) effectMat;
            x = X;
            y = Y;
            z = Z;
            w = W;
        }

        public override void OnGUI (MatEffectData data)
        {
            EditorGUILayout.BeginHorizontal ();
            EditorGUILayout.LabelField ("EffectMat", GUILayout.MaxWidth (100));
            EEffectMaterial mat = (EEffectMaterial) data.param;
            uint v = (uint) (EEffectMaterial) EditorGUILayout.EnumPopup (mat, GUILayout.MaxWidth (300));
            if (canEdit)
            {
                data.param = v;
            }
            EditorGUILayout.EndHorizontal ();
            ParamGUI(data);
        }
    }
}
#endif