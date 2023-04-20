using System;
using UNBridgeLib;
using UnityEngine;
#if UNITY_EDITOR
using Ender.LitJson;

namespace Ender
{
    public class EnderCallAgent : IBaseCallAgent, IBaseStaticCallAgent
    {
        private readonly string className;

        public EnderCallAgent(string className)
        {
            this.className = className;
        }

        public void Call(string methodName, params object[] args)
        {
            JsonData packet = this.PacketMsg(methodName, false, args);
            GMEnderMgr.instance.callEnder(packet, false, GMCallEnderType.Universal);
        }

        public ReturnType Call<ReturnType>(string methodName, params object[] args)
        {
            JsonData packet = this.PacketMsg(methodName, false, args);
            string result = GMEnderMgr.instance.callEnder(packet, true, GMCallEnderType.Universal);
            JsonData resultJson = JsonMapper.ToObject(result);
            if (resultJson == null)
            {
                Debug.LogError(methodName + " return result is null");
                return default(ReturnType);
            }

            try
            {
                object value = GMEnderHelper.GetObject(resultJson, "value");
                if (value == null)
                {
                    return default(ReturnType);
                }

                return (ReturnType)value;
            }
            catch (Exception e)
            {
                Debug.LogError("Call, " + e.ToString());
                return default(ReturnType);
            }
        }

        public ReturnType CallStatic<ReturnType>(string methodName, params object[] args)
        {
            JsonData packet = this.PacketMsg(methodName, true, args);
            string result = GMEnderMgr.instance.callEnder(packet, true, GMCallEnderType.Universal);
            JsonData resultJson = JsonMapper.ToObject(result);
            if (resultJson == null)
            {
                Debug.LogError(methodName + " return result is null");
                return default(ReturnType);
            }

            object value = GMEnderHelper.GetObject(resultJson, "value");
            if (value == null)
            {
                return default(ReturnType);
            }

            return (ReturnType)value;
        }

        public void CallStatic(string methodName, params object[] args)
        {
            JsonData packet = this.PacketMsg(methodName, true, args);
            GMEnderMgr.instance.callEnder(packet, false, GMCallEnderType.Universal);
        }

        private JsonData PacketMsg(string methodName, bool isStatic, params object[] args)
        {
            JsonData param = new JsonData();
            param["ClassName"] = this.className;
            param["MethodName"] = methodName;
            param["IsStatic"] = isStatic;
            if (args != null && args.Length > 0)
            {
                int index = 0;
                foreach (object arg in args)
                {
                    if (arg is IEnderBaseCallBack)
                    {
                        if (!param.ContainsKey("CallBack"))
                        {
                            param["CallBack"] = new JsonData();
                        }

                        JsonData callback = new JsonData();

                        callback["CallBackClassName"] = ((IEnderBaseCallBack)arg).GetCallBackClassName();
                        callback["CallBackId"] = EnderCallBackUtils.GetCallBackId();
                        callback["ParamIndex"] = index;
                        EnderCallBackUtils.AddCallBack((IEnderBaseCallBack)arg);
                        param["CallBack"].Add(callback);
                    }
                    else
                    {
                        if (!param.ContainsKey("Params"))
                        {
                            param["Params"] = new JsonData();
                        }

                        if (arg == null)
                        {
                            param["Params"].Add("");
                        }
                        else
                        {
                            param["Params"].Add(arg);
                        }
                    }

                    index++;
                }
            }

            return param;
        }
    }
}
#endif
