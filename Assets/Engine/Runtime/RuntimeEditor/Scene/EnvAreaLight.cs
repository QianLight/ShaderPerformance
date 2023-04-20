// 区域灯代码，暂时没地方用到。
// #define ENABLE_AREA_LIGHT

//#if ENABLE_AREA_LIGHT

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;


namespace CFEngine
{
    //[Serializable]
    public class EnvAreaLight : MonoBehaviour
    {
        public Color color;
        public float range;
        public float specLerp;

        private void OnDrawGizmosSelected()
        {
            Color c = Gizmos.color;
            Gizmos.color = color;
            Gizmos.DrawWireSphere(transform.position, range);
            Gizmos.color = c;
        }
    }

    
}

#endif

//#endif