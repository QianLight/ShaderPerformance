using System;
using System.Collections.Generic;
using System.Reflection;
using GMSDK;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class InnerTools
    {
        /// <summary>
        /// 安全调用。可以用于防止CP意外传入null，造成整个接口崩溃。
        /// </summary>
        /// <param name="action">用lambda表达式将callback.Invoke调用包起来。</param>
        /// <param name="forceAnalyze">是否强制分析回调内容。</param>
        public static void SafeInvoke(Action action, bool forceAnalyze = false)
        {
            try
            {
                if (action == null)
                {
                    return;
                }
                action.Invoke();
                if (forceAnalyze)
                {
                    AnalyzeDelegate(action);
                }
            }
            catch (Exception e)
            {
                GLog.LogException(e);
                AnalyzeDelegate(action);
            }
        }

        /// <summary>
        /// 分析一个Delegate的内容。
        /// </summary>
        /// <param name="action">对应的Delegate</param>
        private static void AnalyzeDelegate(Action action)
        {
            // Target为Delegate的目标执行函数。
            if (Analyze(action.Target))
            {
                // 如果有回调为空，再打印一句提示信息。
                GLog.LogError("A null delegate exists in this callback.");
            }
        }

        /// <summary>
        /// 分析一个Object的内容，打印其的每个域（fields）的名称、值和类型。
        /// </summary>
        /// <param name="obj">分析目标</param>
        /// <returns>是否含有null的Delegate</returns>
        internal static bool Analyze(object obj)
        {
            // 如果LogLevel为None，则不检测，因为检测会影响性能，而且也没法打印出任何实际信息。
            if (GLog.Level == LogLevel.None)
            {
                return false;
            }

            if (obj == null)
            {
                GLog.LogInfo("The object is null.");
                return false;
            }

            // 标记是否有null的Delegate。
            var hasNullDelegate = false;
            // Lambda表达式会将引用到表达式外部的变量作为域（fields）存入。根据编译方式的不同，具体的访问权限也不同，因此
            // 需要获取所有种类的域。
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance |
                                                         BindingFlags.Public | BindingFlags.Static);
            foreach (var field in fields)
            {
                // 打印域的具体信息。
                if (field.GetValue(obj) == null)
                {
                    // C#默认null默认打印换行，在此做一个特殊判断打印null方便调试。
                    var value = "(null)";
                    GLog.LogWarning(string.Format("Name: {0} \n Value: {3} \n Type: {1} \n BaseType: {2}", field.Name,
                        field.FieldType,
                        field.FieldType.BaseType, value));
                }
                else
                {
                    var value = field.GetValue(obj).ToString();
                    GLog.LogInfo(string.Format("Name: {0} \n Value: {3} \n Type: {1} \n BaseType: {2}", field.Name,
                        field.FieldType,
                        field.FieldType.BaseType, value));
                }

                // 判断该参数是否为回调。C#中参数超过一个的回调的基础类型为 System.MulticastDelegate，只有一个的为 System.Delegate。
                if ((field.FieldType.BaseType == typeof(MulticastDelegate) ||
                     field.FieldType.BaseType == typeof(Delegate)) &&
                    field.GetValue(obj) == null)
                {
                    hasNullDelegate = true;
                }

                // Lambda 表达式的类型为 System.Object。
                // 在Lambda套Lambda的情况下，有一部分参数会存在外层Lambda下，传入内层Lambda的变量会存在外层Lambda的域内，
                // 因此如果检测到外层Lambda，我们需要对其进行进一步相关分析。
                if (field.FieldType.BaseType == typeof(Object) && field.FieldType != typeof(String))
                {
                    GLog.LogInfo("The above is an object. \n Check for more details.");
                    // 递归检查。
                    if (Analyze(field.GetValue(obj)))
                    {
                        hasNullDelegate = true;
                    }
                }

                // 检测嵌套CallbackResult
                if (field.FieldType.BaseType == typeof(CallbackResult))
                {
                    GLog.LogInfo("The above is a CallbackResult. \n Check for more details.");
                    // 递归检查。
                    if (Analyze(field.GetValue(obj)))
                    {
                        hasNullDelegate = true;
                    }
                }
            }

            return hasNullDelegate;
        }

        private static Dictionary<int, string> _errorCodeMap = null;

        /// <summary>
        /// 获取错误码在ErrorCode中的名称。
        /// </summary>
        /// <param name="code">错误码数值（int）</param>
        /// <returns>错误码名称（string）</returns>
        public static string ConvertErrorCode(int code)
        {
            if (_errorCodeMap == null)
            {
                _errorCodeMap = new Dictionary<int, string>();
                foreach (FieldInfo f in typeof(ErrorCode).GetFields(BindingFlags.Static | BindingFlags.Public))
                {
                    try
                    {
                        _errorCodeMap[(int) f.GetValue(null)] = f.Name;
                    }
                    catch
                    {
                    }
                }
            }

            if (_errorCodeMap.ContainsKey(code))
            {
                return _errorCodeMap[code];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Native bridge 错误码转换为 Unity 3.x
        /// </summary>
        /// <param name="jsonData">Native bridge 错误码</param>
        /// <returns>Unity 3.x错误码/returns>
        public static Result ConvertJsonToResult(JsonData jsonData)
        {
            if (!jsonData.ContainsKey("code") && !jsonData.ContainsKey("message"))
            {
                GLog.LogWarning("JsonData don't contain \"code\" or \"message\"");
                return null;
            }

            try
            {
                var code = int.Parse(jsonData["code"].ToString());
                var message = jsonData["message"].ToString();
                var extraCode = int.Parse(jsonData.ContainsKey("extraErrorCode")?jsonData["extraErrorCode"].ToString():"0");
                var extraMessage = jsonData.ContainsKey("extraErrorMessage")?jsonData["extraErrorMessage"].ToString():"";
                var additionalInfo = jsonData.ContainsKey("additionalInfo")?jsonData["additionalInfo"].ToString():"";
                var result = new Result(code, message, extraCode, extraMessage, additionalInfo);
                return result;
            }
            catch (FormatException e)
            {
                GLog.LogWarning("code of JsonData isn't Integer, error: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Unity 2.x 错误码结构转换为 Unity 3.x
        /// </summary>
        /// <param name="callbackResult"></param>
        /// <returns></returns>
        public static Result ConvertToResult(CallbackResult callbackResult)
        {
            var ret = new Result(callbackResult.code, callbackResult.message);
            ret.ExtraCode =  callbackResult.extraErrorCode;
            ret.ExtraMessage =  callbackResult.extraErrorMessage;
            ret.AddtionalInfo = callbackResult.additionalInfo;

            return ret;
        }
        
        public static void SdkMonitorEvent(string eventName, JsonData category)
        {
            SdkUtil.SdkMonitorEvent(eventName, category, true);
        }
        
        public static Type GetGSDKType(string typeName)
        {
            return Type.GetType(typeName);
        }
    }
}