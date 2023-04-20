using System.Collections;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using System.Threading;
using System.Reflection;
#if UNITY_EDITOR
using Ender.LitJson;

namespace Ender
{
    public enum GMCallEnderType
    {
        UNBridge = 0,
        Universal = 1,
        Aloha = 2,
    }

    //pointer不支持返回值；pointer参数仅支持基础类型
    public enum GMEnderValueType
    {
        type_null = -1,
        type_void = 0,
        type_string,
        type_int,
        type_uint,
        type_float,
        type_bool,
        type_longlong,
        type_double,
        type_byte,
        type_array_pointer,
        type_pointer,
    }

    public class GMEnderCFuncParam
    {
        public GMEnderValueType type;
        public object value;
        public MethodInfo methodInfo;
        public List<GMEnderValueType> listPointerParams;
        public GMEnderValueType elementType;
        //public GMEnderValueType pointerReturnType;

        public GMEnderCFuncParam(GMEnderValueType type, object value, MethodInfo method = null, List<GMEnderValueType>listPtrParams = null)
        {
            this.type = type;
            this.value = value;
            this.methodInfo = method;
            this.listPointerParams = listPtrParams;
            //this.pointerReturnType = ptrReturnType;
        }

        public GMEnderCFuncParam(GMEnderValueType type, object value, GMEnderValueType elementType)
        {
            this.type = type;
            this.value = value;
            this.elementType = elementType;
        }

        public string getStringValue()
        {
            switch (type)
            {
                case GMEnderValueType.type_void:
                    return "";
                case GMEnderValueType.type_string:
                    return (string)value;
                case GMEnderValueType.type_int:
                    return value.ToString();
                case GMEnderValueType.type_uint:
                    return value.ToString();
                case GMEnderValueType.type_float:
                    return value.ToString();
                case GMEnderValueType.type_bool:
                    return value.ToString();
                case GMEnderValueType.type_longlong:
                    return value.ToString();
                case GMEnderValueType.type_double:
                    return value.ToString();
                case GMEnderValueType.type_array_pointer:
                    {
                        JsonData packet = new JsonData();
                        packet["arrayPointer"] = JsonMapper.ToObject(JsonMapper.ToJson(value));
                        packet["arrayPointerElementType"] = (int)elementType;
                        return packet.ToJson();
                    }
                case GMEnderValueType.type_pointer:
                    {
                        string pointerID = GMEnderMgr.instance.registerUniversalPointer(this);
                        JsonData packet = new JsonData();

                        List<int> types = new List<int>();
                        types.Add((int)GMEnderValueType.type_void);
                        for (int i = 0; i < listPointerParams.Count; i++)
                        {
                            types.Add((int)listPointerParams[i]);
                        }

                        packet["methodPointerTypes"] = JsonMapper.ToObject(JsonMapper.ToJson(types));
                        packet["methodPointerID"] = pointerID;
                        packet["methodName"] = methodInfo.Name;
                        return packet.ToJson();
                    }
            }

            return "";
        }
    }

    public class GMEnderCFunction
    {
        public GMEnderValueType returnType;
        public string methodName;
        public List<GMEnderCFuncParam> methodParams;

        public GMEnderCFunction(GMEnderValueType returnType, string methodName)
        {
            this.returnType = returnType;
            this.methodName = methodName;
            this.methodParams = new List<GMEnderCFuncParam>();
        }

        public GMEnderCFunction(GMEnderValueType returnType, string methodName, List<GMEnderCFuncParam> methodParams)
        {
            this.returnType = returnType;
            this.methodName = methodName;
            this.methodParams = methodParams;
        }

        public JsonData packJsonData()
        {
            try
            {
                JsonData packet = new JsonData();
                packet["methodName"] = methodName;

                List<string> paramString = new List<string>();
                List<int> types = new List<int>();
                types.Add((int)returnType);
                for (int i = 0; i < methodParams.Count; i++)
                {
                    GMEnderCFuncParam param = methodParams[i];
                    types.Add((int)param.type);
                    paramString.Add(param.getStringValue());
                }
                packet["methodTypes"] = JsonMapper.ToObject(JsonMapper.ToJson(types));
                packet["methodParams"] = JsonMapper.ToObject(JsonMapper.ToJson(paramString));

                return packet;
            }
            catch (Exception e)
            {
                Debug.LogError("GMEnderCFunc methodName: " + methodName + ", packJsonData error: " + e.Message);
            }

            return null;
        }
    }
}
#endif