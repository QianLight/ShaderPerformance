#if UNITY_EDITOR
using System;
using UnityEngine;

namespace CFEngine
{
    [Node (module = "MatEffect")]
    [Serializable]
    public class EffectTemplate : AbstractNode
    {
        [Editable("effectName")] public string effectName;
        [Editable("groupID")] public int groupID;
        [Editable("valid")] public bool valid = true;
        [Editable("applyOnReplaceMat")] public bool applyOnReplaceMat = false;
        [Editable("applyOnAddMat")] public bool applyOnAddMat = false;
        [Editable("applyOnShadow")] public bool hideShadow = false;
        [Output] public int next;
        
        public override object OnRequestValue (Port port)
        {
            return next + 1;
        }
    }
}
#endif