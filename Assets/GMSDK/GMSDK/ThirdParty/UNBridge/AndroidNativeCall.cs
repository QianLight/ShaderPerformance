#if UNITY_EDITOR || UNITY_ANDROID
using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR && GMEnderOn
using Ender;
#endif

namespace UNBridgeLib
{
    public class AndroidNativeCall
    {
        private static readonly Dictionary<string, IBaseCallAgent> CallObjectMap =
            new Dictionary<string, IBaseCallAgent>();

        private static readonly Dictionary<string, IBaseStaticCallAgent> StaticCallObjectMap =
            new Dictionary<string, IBaseStaticCallAgent>();


        /**
         * 调用Native方法
         * @param className 被调用Native类的完整类名
         * @param methodName 方法名
         * @param args  方法参数
         */
        public static void Call(string className, string methodName, params object[] args)
        {
            GetCallObject(className).Call(methodName, args);
        }

        /**
         * 调用Native同步方法
         * @param className 被调用Native类的完整类名
         * @param methodName 方法名
         * @param args  方法参数
         * @ReturnType 返回值类型
         */
        public static ReturnType CallSync<ReturnType>(string className, string methodName, params object[] args)
        {
            return GetCallObject(className).Call<ReturnType>(methodName, args);
        }

        /**
         * 调用Native静态方法
         * @param className 被调用Native类的完整类名
         * @param methodName 方法名
         * @param args  方法参数
         */
        public static void CallStatic(string className, string methodName, params object[] args)
        {
            GetStaticCallObject(className).CallStatic(methodName, args);
        }

        /**
         * 调用Native静态同步方法
         * @param className 被调用Native类的完整类名
         * @param methodName 方法名
         * @param args  方法参数
         * @ReturnType 返回值类型
         */
        public static ReturnType CallStaticSync<ReturnType>(string className, string methodName, params object[] args)
        {
            return GetStaticCallObject(className).CallStatic<ReturnType>(methodName, args);
        }


        private static IBaseCallAgent GetCallObject(string className)
        {
            if (CallObjectMap.ContainsKey(className))
            {
                return CallObjectMap[className];
            }
#if UNITY_EDITOR && GMEnderOn
            IBaseCallAgent agent = new EnderCallAgent(className);
#else
            IBaseCallAgent agent = new NativeCall(className);

#endif
            CallObjectMap[className] = agent;
            return agent;
        }

        private static IBaseStaticCallAgent GetStaticCallObject(string className)
        {
            if (StaticCallObjectMap.ContainsKey(className))
            {
                return StaticCallObjectMap[className];
            }
#if UNITY_EDITOR && GMEnderOn
            IBaseStaticCallAgent agent = new EnderCallAgent(className);
#else
            IBaseStaticCallAgent agent = new NativeStaticCall(className);

#endif
            StaticCallObjectMap[className] = agent;
            return agent;
        }


        private sealed class NativeCall : IBaseCallAgent
        {
            private readonly AndroidJavaObject _javaObject;

            public NativeCall(string className)
            {
                using (var jc = new AndroidJavaClass(className))
                {
                    _javaObject =
                        jc.CallStatic<AndroidJavaObject>("getInstance");
                }
            }

            public void Call(string methodName, params object[] args)
            {
                _javaObject.Call(methodName, args);
            }

            public ReturnType Call<ReturnType>(string methodName, params object[] args)
            {
                return _javaObject.Call<ReturnType>(methodName, args);
            }
        }

        private sealed class NativeStaticCall : IBaseStaticCallAgent
        {
            private readonly AndroidJavaClass _javaClass;

            public NativeStaticCall(string className)
            {
                _javaClass = new AndroidJavaClass(className);
            }

            public ReturnType CallStatic<ReturnType>(string methodName, params object[] args)
            {
                return _javaClass.CallStatic<ReturnType>(methodName, args);
            }

            public void CallStatic(string methodName, params object[] args)
            {
                _javaClass.CallStatic(methodName, args);
            }
        }
    }
}
#endif
