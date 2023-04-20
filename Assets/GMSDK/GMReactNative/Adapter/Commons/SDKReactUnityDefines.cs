using UNBridgeLib.LitJson;
using System.Collections;
using System.Collections.Generic;

namespace GMSDK
{
    public class SDKReactUnityDefines
    {
        // 获取云控配置
        public const string RUGetKVConfig = "RUGetKVConfig";

        public const string RURequest = "RURequest";

        public const string RUOpenUrl = "RUOpenUrl";

        public const string RUGetUserInfo = "RUGetUserInfo";

        public const string RUGetCommonParams = "RUGetCommonParams";

        public const string RUMonitorEvent = "RUMonitorEvent";

        // 跳转打开外部App
        public const string RULinkingCanOpenUrl = "RULinkingCanOpenUrl";
        public const string RULinkingOpenUrl = "RULinkingOpenUrl";

        public const string RUSyncGecko = "syncGecko";
    }

    #region KVConfig

    public class RUGetKVConfigResult : CallbackResult
    {
        public RUKVConfig config;
        public string bundlePrePath;
    }

    public class RUKVConfig
    {
        public bool ruDisable;
        public string preloadUrl;
    }

    #endregion

    public class RURequestResult : CallbackResult
    {
        public string data;
    }

    public class RUOpenUrlResult: CallbackResult
    {

    }

    public class RUGetUserInfoResult: CallbackResult
    {
        public Dictionary<string, object> data;
    }

    public class RUGetCommonParamsResult: CallbackResult
    {
        public Dictionary<string, object> data;
    }

    public class RUMonitorEventResult: CallbackResult
    {

    }

    public class RULinkingCanOpenUrlResult : CallbackResult
    {
        // 是否可以打开
        public bool canOpen;
    }
}
