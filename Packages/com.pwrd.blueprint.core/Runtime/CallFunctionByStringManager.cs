using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class CallFunctionByStringManager
{
    public static bool ParameterIsEqual(object[] paramters, List<bool> isRefs, MethodInfo methodInfo)
    {
        if (methodInfo == null)
            return false;
        ParameterInfo[] infos = methodInfo.GetParameters();
        if (infos.Length != paramters.Length)
            return false;
        for (int i = 0; i < paramters.Length; i++)
        {
            if (paramters[i] == null)
                continue;
            var parameterType = paramters[i].GetType();
            var infoType = infos[i].ParameterType;
            var parameterRef = isRefs[i];
            var infoRef = infoType.IsByRef;
            if (infoRef && !infoType.ToString().Contains(parameterType.ToString()))
            {
                return false;
            }
            if (!infoRef && parameterType != infoType || parameterRef != infoRef)
            {
                return false;
            }
        }
        return true;
    }
    public static bool ReturnIsParameterEqual(object paramters, MethodInfo methodInfo)
    {
        if (methodInfo == null)
            return false;
        var infos = methodInfo.ReturnType;
        var rr = paramters.GetType();
        return rr == infos;
    }
}
