#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CFEngine
{
    public class IBLConfig : ScriptableObject
    {
        public float HDR = 1;
        public bool gamma = true;
        public float pcHDR = 1;
        public bool pcGamma = true;
    }
}

#endif