using System;

namespace GSDK
{
    /// <summary>
    /// 版本信息
    /// </summary>
    public static class VersionCode
    {
        /// <summary>
        /// 当前Unity版本
        /// </summary>
        public const string UnityVersion = "3.15.2.0";
    }

    public static class PluginName
    {
        public const string GSDK = "gsdk";
    }

    /// <summary>
    /// 通用
    /// </summary>
    public static partial class ErrorCode
    {
        /// <summary>
        /// 成功
        /// </summary>
        public const int Success = 0;
    }

    
    /// <summary>
    /// 调用结果的数据结构
    /// </summary>
    [Serializable]
    public class Result
    {
        /// <summary>
        /// 一级错误码
        /// </summary>
        public int Error;

        /// <summary>
        ///  一级错误码信息
        /// </summary>
        public string Message;

        /// <summary>
        /// 二级错误码
        /// </summary>
        public int ExtraCode;

        /// <summary>
        /// 二级错误码信息
        /// </summary>
        public string ExtraMessage;

        /// <summary>
        /// 额外的错误信息，用于定位问题
        /// </summary>
        public string AddtionalInfo;

        /// <summary>
        /// 请求是否成功。
        /// </summary>
        public bool IsSuccess
        {
            get { return Error == ErrorCode.Success; }
        }

        #region Constructor

        public Result()
        {
        }

        public Result(int errorCode)
        {
            Error = errorCode;
            AddtionalInfo = "";
        }

        public Result(int errorCode, string message, string addtionalInfo = "")
        {
            Error = errorCode;
            Message = message;
            AddtionalInfo = addtionalInfo;
        }

        public Result(int error, string message, int extraCode, string extraMessage, string addtionalInfo = "")
        {
            Error = error;
            Message = message;
            ExtraCode = extraCode;
            ExtraMessage = extraMessage;
            AddtionalInfo = addtionalInfo;
        }

        public Result(int errorCode, int extraCode, int optionalCode, string message, string extraMessage = "",
            string optionalMessage = "")
        {
            Error = errorCode;
            Message = message;
            ExtraCode = extraCode;
            ExtraMessage = extraMessage;
            if (!string.IsNullOrEmpty(optionalMessage))
            {
                AddtionalInfo = string.Format("ExtraCode2: {0}, ExtraMessage2: {1}", optionalCode, optionalMessage);
            }
        }


        #endregion

        public static Result ResultFromJson(UNBridgeLib.LitJson.JsonData jsonData)
        {
            if (!jsonData.ContainsKey("code") && !jsonData.ContainsKey("message"))
            {
                GLog.LogWarning("JsonData don't contain \"code\" or \"message\"");
                return null;
            }

            int extraErrorCode = 0;
            string extraErrorMessage = "";
            string additionalInfo = "";
            if (jsonData.ContainsKey("extraErrorCode"))
            {
                extraErrorCode = int.Parse(jsonData["extraErrorCode"].ToString());
            }
            if (jsonData.ContainsKey("extraErrorMessage"))
            {
                extraErrorMessage = jsonData["extraErrorMessage"].ToString();
            }
            if (jsonData.ContainsKey("additionalInfo"))
            {
                additionalInfo = jsonData["additionalInfo"].ToString();
            }
            return new Result(int.Parse(jsonData["code"].ToString()), jsonData["message"].ToString(), extraErrorCode, extraErrorMessage, additionalInfo);
        }

        #region Detail

        public override string ToString()
        {
            return string.Format(
                "ErrorCode:{0}, Message:{1}, ExtraCode:{2}, ExtraMessage:{3}, AddtionalInfo:{4}",
                Error,
                Message,
                ExtraCode,
                ExtraMessage,
                AddtionalInfo);
        }

        /// <summary>
        /// [Debug Only]转换为包含有错误码对应变量名称的的字符串。第一次使用需要约2s时间初始化，后续内存开销较大，建议仅在调试环境下使用。
        /// </summary>
        /// <returns>错误码的具体信息，包含有错误码对应变量名称。</returns>
        public string ToDetailedString()
        {
            return string.Format(
                "ErrorCode:{0}[{1}], Message:{2}, ExtraCode:{3}, ExtraMessage:{4}, AddtionalInfo:{5}",
                Error,
                InnerTools.ConvertErrorCode(Error),
                Message,
                ExtraCode,
                ExtraMessage,
                AddtionalInfo);
        }

        #endregion
    }
}