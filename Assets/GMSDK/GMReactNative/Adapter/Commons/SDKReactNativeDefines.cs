using System;
using System.Collections.Generic;

namespace GMSDK
{
    public class ReactNativeMethodName
    {
        public const string Init = "registerReactNative";
        public const string FetchActivityUrlWithId = "fetchActivityUrlWithId";
        public const string OpenUrl = "openUrl";
        public const string GameHomeDidLoadWithRoleId = "gameHomeDidLoadWithRoleId";
        public const string WarDidFinish = "warDidFinish";
        public const string SetUpOrientation = "setUpOrientation";
        public const string ListenNativeNotification = "ListenNativeNotification";
        public const string QueryConfigValueByKey = "queryConfigValueByKey";
        public const string QueryActivityNotify = "queryActivityNotify";
        public const string invitationPreBind = "invitationPreBind";
        public const string getSenceUrl = "getSenceUrl";
        public const string updateGameConfig = "updateGameConfig";
        public const string notifyIconClickSceneDid = "notifyIconClickSceneDid";
        public const string initBundlePackages = "initBundlePackages";
        public const string getSceneData = "getSceneData";
        public const string queryActivityNotifyByType = "queryActivityNotifyByType";
        public const string queryActivityNotifyById = "queryActivityNotifyById";
        public const string openPage = "openPage";
        public const string openFaceVerify = "openFaceVerify";
        public const string queryActivityNotifyDataByType = "queryActivityNotifyDataByType";
        public const string queryActivityNotifyDataById = "queryActivityNotifyDataById";
        public const string showPage = "showPage";
        public const string hidePage = "hidePage";
        public const string closePage = "closePage";
        public const string sendMessageToPage = "sendMessageToPage";
        public const string startBindPage = "startBindPage";
        public const string setRNDebug = "setRNDebug";
        public const string getRNDebug = "getRNDebug";
        public const string getRNPages = "getRNPages";
        public const string closeAllPages = "closeAllPages";
        public const string showTestPage = "showTestPage";

        public const string syncGecko = "syncGecko";
    }

    public class ReactNativeResultName
    {
        public const string GMRNUnityNotification = "GMRNUnityNotification";
        public const string GMGumihoEngineNotification = "GMGumihoEngineNotification";
    }

    public class SceneData
    {
        public string activityUrl;
        public string activityId;
        public string inGameId;
        public string type;
        public string count;
        public string custom;
    }

    public class NotifyData
    {
        public string id;
        public string inGameId;
        public Notify notify;
    }

    public class Notify
    {
        public string type;
        public string count;
        public string custom;
    }

    public class openFaceData
    {
        public string activityUrl;
        public string activityId;
        public string inGameId;
    }

    public class getSceneDataRet : CallbackResult
    {
        public List<SceneData> list;
    }

    public class queryActivityNotifyByTypeRet : CallbackResult
    {
        public List<NotifyData> list;
    }

    public class queryActivityNotifyByIdRet : CallbackResult
    {
        public List<NotifyData> list;
    }

    public class openPageRet : CallbackResult
    {
        public int type;
        public string windowId;
    }

    public class PageCloseResult : CallbackResult
    {
        public int type;
        public string windowId;
        public string inGameId;
        public int pageType;
    }

    public class openFaceVerifyRet : CallbackResult
    {
        public List<openFaceData> list;
    }

    public class initBundlePackagesRet : CallbackResult
    {
    }

    public class invitationPreBindRet : CallbackResult
    {
    }

    public class getSenceUrlRet : CallbackResult
    {
        public string url;
    }

    public class updateGameConfigRet : CallbackResult
    {
    }

    public class notifyIconClickSceneDidRet : CallbackResult
    {
    }

    public class FetchActivityUrlWithIdResultRet : CallbackResult
    {
        public string activityUrl;
    }

    public class QueryConfigValueResultRet : CallbackResult
    {
        public string values;
    }

    public class QueryActivityNotifyRet : CallbackResult
    {
        public String count;
        public String type;
        public String custom;
    }

    public class OpenUrlResultRet : CallbackResult
    {
    }

    public class HomeDidLoadResultRet : CallbackResult
    {
    }

    public class WarDidEndResultRet : CallbackResult
    {
    }

    public class SetOrientationResultRet : CallbackResult
    {
    }

    public class OperatePageRet : CallbackResult
    {
        public bool status;
    }

    public class GetRNDebugRet : CallbackResult
    {
        public bool status;
    }

    public class queryActivityNotifyDataRet : CallbackResult
    {
        public List<NotifyDataBean> data;
    }

    public class NotifyDataBean
    {
        public string id;
        public string inGameId;
        public NotifyBean notify;
    }

    public class NotifyBean
    {
        public int type;
        public int count;
        public string custom;
    }

    public class RNPage
    {
        public bool isShowing;  //是否显示
        public string inGameId; //活动标识
        public string url;      //活动url
        public int type;        //活动类型
        public string windowId; //活动页面id
    }

    public class ShowTestPageRet : CallbackResult
    {
    }
}