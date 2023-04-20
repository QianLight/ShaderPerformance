#if UNITY_EDITOR
using System;
namespace CFEngine
{

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class CFDecoratorAttribute : Attribute
    {
        public readonly Type attributeType;

        public CFDecoratorAttribute(Type attributeType)
        {
            this.attributeType = attributeType;
        }
    }
}
#endif