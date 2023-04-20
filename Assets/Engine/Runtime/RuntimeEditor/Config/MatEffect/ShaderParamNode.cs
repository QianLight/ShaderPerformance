#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
namespace CFEngine
{
    [Node (module = "MatEffect")]
    [Serializable]
    public class ShaderParam : ParamNode
    {
        [Editable("hdr")] public bool copySrcParam;
        public override MatEffectTemplate GetEffectTemplate ()
        {
            if (met == null)
            {
                met = new MatEffectTemplate ()
                {
                effectType = (int) MatEffectType.Param,

                };
            }
            FillData(met);
            met.flag.SetFlag(MatEffectTemplate.Flag_CopySrcParam, copySrcParam);
            return met;
        }

        public override void InitData (ref float x, ref float y, ref float z, ref float w, ref string path, ref uint param)
        {
            x = X;
            y = Y;
            z = Z;
            w = W;
        }

        public override void OnGUI (MatEffectData data)
        {
            ParamGUI(data);
        }
    }
}
#endif