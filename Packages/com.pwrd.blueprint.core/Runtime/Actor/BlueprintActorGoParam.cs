using System;
using System.Collections.Generic;
using UnityEngine;

namespace Blueprint.Actor
{
    [Serializable]
    public class BlueprintActorGoParam
    {

        public GameObject gameObject;

        /// <summary>
        /// 该物体的唯一名称
        /// </summary>
        public string uniqueName;

        public string name;

        public bool isInherit = false;

        /// <summary>
        /// 父物体在数组中的下标，-1表示为根物体
        /// </summary>
        public int parentIndex = -1;

        /// <summary>
        ///  在数组中的下标
        /// </summary>
        public int index;

        /// <summary>
        /// 子物体下标
        /// </summary>
        public List<int> childIndex = new List<int>();

        /// <summary>
        /// 存储该物体下所有Component的InstanceID
        /// </summary>
        public Dictionary<int, bool> commponentInstanceIDs = new Dictionary<int, bool>();

        /// <summary>
        /// 记录该物体Active状态
        /// </summary>
        public bool IsActive;

#if UNITY_EDITOR

        /// <summary>
        /// 该物体的所有组件
        /// </summary>
        public List<ActorComp> components = new List<ActorComp>();
#endif
    }
}