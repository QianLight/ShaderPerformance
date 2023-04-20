using System;
using UnityEditor;

namespace UnityEngine.Rendering
{
    [AttributeUsage(AttributeTargets.Class)]
    public class VolumeRuleAttribute : Attribute
    {
        public readonly string displayName;
        public readonly SerializedPropertyType type;
        
        public VolumeRuleAttribute(string displayName, SerializedPropertyType type)
        {
            this.displayName = displayName;
            this.type = type;
        }
    }
}