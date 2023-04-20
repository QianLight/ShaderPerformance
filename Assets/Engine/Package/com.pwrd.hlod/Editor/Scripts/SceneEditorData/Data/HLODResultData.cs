#if UNITY_EDITOR
using System;
using UnityEngine;

namespace com.pwrd.hlod.editor
{
    [Serializable]
    public class HLODResultData
    {
        public  Cluster cluster { get { return param.cluster; } }
        public AggregateParam param;
        public GameObject prefab;
        public GameObject instance;
    }
}
#endif