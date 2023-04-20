#if ANIMATION_OBJECT_V0
#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CFEngine
{
    [System.Serializable]
    public class ActiveObjectData : SceneAnimationData
    {        
        public string animationGroupPath;
        public override void Save (BinaryWriter bw)
        {
            bw.Write (AnimationObject.AnimType_ActiveObject);
            base.Save (bw);
        }
    }
}
#endif
#endif