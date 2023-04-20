#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
namespace CFEngine
{
    [Node (module = "MatEffect")]
    [Serializable]
    public class ShaderRT : MatEffectNode
    {

        public override MatEffectTemplate GetEffectTemplate ()
        {
            if (met == null)
            {
                met = new MatEffectTemplate ()
                {
                effectType = (int) MatEffectType.RT
                };
            }
            FillData(met);
            return met;
        }

        public override void OnGUI (MatEffectData data)
        {
            
        }
    }
}
#endif