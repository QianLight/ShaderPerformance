using System;
using UnityEngine;
using System.Reflection;

namespace com.pwrd.hlod.editor
{
    public class Reflection
    {
        public static T GetField<T>(string typeName, string fieldName)
        {
            return GetField<T>(null, typeName, fieldName);
        }
 
        public static T GetField<T>(object instance, string typeName, string fieldName)
        {
            bool isStatic = null == instance;
            Type type = Type.GetType(typeName);
            T defaultValue = default(T);
            if(null == type) return defaultValue;
        
            BindingFlags flag = (isStatic ? BindingFlags.Static : BindingFlags.Instance);
            FieldInfo field = type.GetField(fieldName, flag | BindingFlags.Public | BindingFlags.NonPublic);
            if(null == field)
            {
                HLODDebug.LogError(string.Format("field {0} does not exist!",fieldName));
                return defaultValue;
            }
            object result = field.GetValue(instance);
            if(null == result)
                return defaultValue;
            if(!(result is T))
            {
                HLODDebug.LogError(string.Format("field {0} cast failed!",fieldName));
                return defaultValue;
            }
            return (T)result;
        }

        public static void SetField(object instance, string typeName, string fieldName, object value)
        {
            bool isStatic = null == instance;
            Type type = Type.GetType(typeName);
            if(null == type) return;
        
            BindingFlags flag = (isStatic ? BindingFlags.Static : BindingFlags.Instance);
            FieldInfo field = type.GetField(fieldName, flag | BindingFlags.Public | BindingFlags.NonPublic);
            if(null == field)
            {
                HLODDebug.LogError(string.Format("field {0} does not exist!",fieldName));
                return;
            }
           field.SetValue(instance, value);
        }
        
        public static void Call(string typeName, string methodName, params object[] args)
        {
            Call<object>(typeName, methodName, args);
        }
 
        public static T Call<T>(string typeName, string methodName, params object[] args)
        {
            Type type = Type.GetType(typeName);
            T defaultValue = default(T);
            if(null == type) return defaultValue;
        
            Type[] argTypes = new Type[args.Length];
            for(int i=0, count = args.Length; i< count; ++i)
            {
                argTypes[i] = null != args[i] ? args[i].GetType() : null;
            }
            MethodInfo method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, argTypes, null);
            if(null == method)
            {
                HLODDebug.LogError(string.Format("method {0} does not exist!",methodName));
                return defaultValue;
            }
            object result = method.Invoke(null, args);
            if(null == result)
                return defaultValue;
            if(!(result is T))
            {
                HLODDebug.LogError(string.Format("method {0} cast failed!",methodName));
                return defaultValue;
            }
            return (T)result;
        }
    }
}