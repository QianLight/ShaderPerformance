using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
#if UNITY_EDITOR
using Ender;
using Ender.LitJson;

namespace Ender
{
    public class EnderCallBackUtils
    {
        private static int callBackId;

        //存储CallBackId 和 Callback的映射
        public static readonly Dictionary<int, IEnderBaseCallBack> CallBackMap = new Dictionary<int, IEnderBaseCallBack>();

        public static void AddCallBack(IEnderBaseCallBack callBack)
        {
            CallBackMap.Add(callBackId++, callBack);
        }

        public static int GetCallBackId()
        {
            return callBackId;
        }

        public static IEnderBaseCallBack GetCallBack(int id)
        {
            if (CallBackMap.ContainsKey(id))
            {
                return CallBackMap[id];
            }

            return null;
        }

        public static void RemoveCallBack(int id)
        {
            if (CallBackMap.ContainsKey(id))
            {
                CallBackMap.Remove(id);
            }
        }

        // 回调Callback
        // {
        // "CallBackId":0,
        // "MethodName":"OnLoginFailed",
        // "Params":[
        //    -1,
        //    "login failed"
        // ]
        // }
        public static void InvokeCallBack(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            JsonData jsonData = JsonMapper.ToObject(json);
            int retCallBackId = GMEnderHelper.GetInt(jsonData, "CallBackId");

            IEnderBaseCallBack callBack = GetCallBack(retCallBackId);

            if (callBack == null)
            {
                Debug.LogError("callback should add first");
                return;
            }

            string retMethodName = GMEnderHelper.GetString(jsonData, "MethodName");
            if (string.IsNullOrEmpty(retMethodName))
            {
                Debug.LogError("method name is null");
                return;
            }

            Type type = callBack.GetType();
            JsonData retParams = GMEnderHelper.GetData(jsonData, "Params");
            if (retParams == null || retParams.Count == 0)
            {
                try
                {
                    MethodInfo methodInfo = type.GetMethod(retMethodName);
                    if (methodInfo == null)
                    {
                        Debug.LogError("can not find method: " + type + retMethodName);
                        return;
                    }

                    methodInfo.Invoke(callBack, null);
                }
                catch (Exception e)
                {
                    Debug.LogError("InvokeCallBack, " + e.ToString());
                }
            }
            else
            {
                Debug.Log(retParams.ToJson());
                int count = retParams.Count;
                Type[] types = new Type[count];
                object[] objects = new object[count];
                for (int i = 0; i < count; i++)
                {
                    types[i] = GMEnderHelper.getDataType(retParams[i]);
                    objects[i] = GMEnderHelper.GetObjectByIndex(retParams[i]);
                }

                try
                {
                    MethodInfo methodInfo = type.GetMethod(retMethodName, types);
                    if (methodInfo == null)
                    {
                        Debug.LogError("can not find method: " + type + retMethodName);
                        return;
                    }

                    methodInfo.Invoke(callBack, objects);
                }
                catch (Exception e)
                {
                    Debug.LogError("InvokeCallBack, " + e.ToString());
                }
            }
        }
    }
}
#endif
