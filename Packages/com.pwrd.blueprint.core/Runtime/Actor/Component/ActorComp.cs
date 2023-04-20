using System;
using UnityEngine;


namespace Blueprint.Actor
{
    [Serializable]
    public class ActorComp
    {
        public string Name;

        public string FullName;
        public bool IsAdded;
        
        public bool IsRemoveComp;

        // 仅transform需要赋值
        public Vector3 Position;

        // 仅transform需要赋值
        public Vector3 Rotation;

        // 仅transform需要赋值
        public Vector3 Scale;
    }
}