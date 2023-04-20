#if UNITY_EDITOR
using System;
using UnityEngine;

namespace com.pwrd.hlod.editor
{

    //在这个节点下的所有Render都会被强制合并成一个Cluster
    [ExecuteInEditMode]
    public class HLODClusterNode : MonoBehaviour
    {
        private void Awake()
        {
        }
    }
}
#endif