/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR
using System.Reflection;

namespace Zeus
{
    public interface IWrapper {
        
    }
    public interface IWrapperAttributeToInferface<Interface, Attribute> : IWrapper where Interface : class where Attribute : class
    {
        string Info { get; }
        Interface Wrapper(MethodInfo method);
    }
}
#endif