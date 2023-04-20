using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UNBridgeLib.LitJson;

namespace GMSDK
{

    public enum ErrorCode
    {
        Success = 0,
        NetworkError,


        IMError = 8000,

        // Common Errors
        IMOtherError,
        IMInvalidArguments,
        IMDBError,

        // Errors from Server
        IMTokenInvalid = 8010,
        IMTokenExpired,
        IMTicketInvalid,
        IMReqeustInvalid,
        IMCMDInvalid,
        IMServerException,
        IMNotEnoughPemission,
        IMServerInnerError,
        IMRequestDemoted,
        IMCancelTimeout,

        // Conversation Errors
        IMConversationNotExist = 8020,
        IMConversationCheckFailed,
        IMUserNotInConversation,

        // Message Errors
        IMMessageNotExist = 8030,
        IMMessageIsAlreadySending,
        IMMessageCheckFailed,
        IMMessageCheckFailedButVisibleToSender,
        IMMessageSendErrorFileUplaodFailed,

        EmoticonError = 23000,
        EmoticonOtherError,
        EmoticonNetworkError,
    }

    public class SDKMethodName
    {
        public const string SDKInit = "requestSDKInit";
        public const string SDKUpdateConfig = "setConfigurationDictionary";
        public const string SDKTriggerBridgeInit = "requestTriggerInit";
        public const string SDKLogin = "requestLogin";
        public const string SDKAuthLogin = "authLoginInterface";
        public const string SDKCloudGameLogin = "requestCloudGameLogin";
        public const string SDKCloudGameLoginV2 = "requestCloudGameLoginV2";
        public const string SDKIsCloudRuntime = "isCloudRuntime";
        public const string SDKResetGuest = "requestReleaseGuest";
        public const string SDKGetDeviceId = "requestDeviceId";
        public const string SDKGetChannel = "getChannel";
        public const string SDKGetChannelOp = "getChannelOp";
        public const string SDKGetPayChannelOp = "getPayChannelOp";
        public const string SDKGetAppId = "requestAppId";
        public const string SDKGetSDKVersion = "getGSDKVersion";
        public const string SDKGetSdkOpenId = "requestSdkOpenId";
        public const string SDKGetInstallId = "requestInstallId";
        public const string SDKGetDownloadSource = "requestDownloadSource";
        public const string SDKLogout = "requestLogout";
        public const string SDKLogoutAndSetTokenExpired = "logoutAndSetTokenExpired";
        public const string SDKRequestPermissions = "requestPermission";
        public const string SDKLoginState = "requestLoginState";
        public const string SDKProtocolVersion = "checkProtocolVersion";
        public const string SDKShowUserCenter = "requestShowUserCenter";
        public const string SDKSetDebugMode = "requestSetDebugMode";
        public const string SDKRealNameAuth = "requestRealNameAuth";
        public const string SDKRealNameAuthWithoutUI = "requestRealNameAuthWithoutUI";
        public const string SDKIsVerified = "requestIsVerified";
        public const string SDKShowWebViewController = "requestShowWebViewController";
        public const string SDKShowWebViewWithCallbackController = "requestShowWebViewWithCallbackController";
        public const string SDKCloseWebView = "requestCloseWebView";
        public const string SDKGetBulletins = "requestBulletins";
        public const string SDKRequestProduct = "requestProduct";
        public const string SDKPay = "requestPay";
        public const string SDKCompensate = "requestCompensate";
        public const string SDKRequestAppTracking = "gsdkRequestTrackingAuthorization";
        public const string SDKGetLoginResultSync = "getLoginResultSync";     // 同步返回用户登录信息
        public const string SDKGetLoginResult = "getLoginResult";

        public const string SDKCheckUnverifiedTransactionIfNeeded = "checkUnverifiedTransactionIfNeeded";
        public const string SDKConfigShowErrorTip = "configShowErrorTip";

        public const string SdkCheckLeftOrder = "requestCheckLeftOrder";
        public const string SdkTrackEvent = "requestTrackEvent";
        public const string SdkTrackSDKEvent = "requestTrackSDKEvent";
        public const string SdkRegisterDeepLink = "requestRegisterDeepLink";
        public const string SdkGetSecureHeader = "requestSecureHeader";
        public const string SdkGetLocation = "requestGetLocation";
        public const string SdkGetLocationByIp = "requestGetLocationByIp";
        public const string SdkRegisterExperiment = "requestRegisterExperiment";
        public const string SdkGetExperiment = "requestGetExperiment";
        public const string SdkCheckMethodEnable = "checkMethodEnable";
        public const string SdkShowFloating = "requestShowFloating";
        public const string SdkHideFloating = "requestHideFloating";
        public const string SdkGMLog = "requestGMLog";
        public const string SdkGMLogException = "requestGMLogException";
        public const string SdkGMLogExceptionSyn = "requestGMLogExceptionSyn";
        public const string SdkGMUploadFile = "requestGMUploadFile";
        public const string SdkGMAddTags = "requestGMAddTags";
        public const string SdkGMAddTagsByJson = "requestGMAddTagsByJson";
        public const string SdkGMAddCustomInfo = "requestGMAddCustomInfo";
        public const string SdkGMAddCustomInfoByJson = "requestGMAddCustomInfoByJson";
        public const string SdkGMLogEvent = "requestGMLogEvent";
        public const string SdkMonitorEvent = "requestSdkMonitorEvent";
        public const string SdkGMEnableSystemLogUpload = "enableSystemLogUpload";
        public const string SdkGMOutputSystemLog = "outputSystemLog";
        public const string SdkGMSystemLogUploadEvent = "onSystemLogUploadEvent";
        public const string SdkZoneList = "requestZoneList";
        public const string SdkZoneListWithExtraInfo = "requestZoneListWithExtraInfo";
        public const string SdkRoleList = "requestRoleList";
        public const string SdkServerAndRole = "requestServerAndRole";
        public const string SdkServerAndRoleWithExtraInfo = "requestServerAndRoleWithExtraInfo";
        public const string SdkPing = "requestPing";
        public const string SdkZoneListWithTimeout = "requestZoneListWithTimeout";
        public const string SdkZoneListWithTimeoutAndExtraInfo = "requestZoneListWithTimeoutAndExtraInfo";
        public const string SdkRoleListWithTimeout = "requestRoleListWithTimeout";
        public const string SdkServerAndRoleWithTimeout = "requestServerAndRoleWithTimeout";
        public const string SdkServerAndRoleWithTimeoutAndExtraInfo = "requestServerAndRoleWithTimeoutAndExtraInfo";
        public const string SdkPingWithTimeout = "requestPingWithTimeout";
        public const string SdkGetCDKey = "requestCDKey";
        public const string SdkIsEmulator = "requestIsEmulator";
        public const string SdkSetLanguage = "requestSetLanguage";
        public const string SDKRegisterAccountStatusChangedListener = "requestRegisterAccountStatusChangedListener";
        public const string SDKEnterGameUpload = "requestEnterGameUpload";
        public const string SDKCreateNewRoleUpload = "requestCreateNewRoleUpload";
        public const string SDKRoleLevelUpload = "requestRoleLevelUp";
        public const string SDKRoleExitUpload = "requestRoleExitUp";
        public const string SdkOnExit = "requestOnExit";
        public const string SDKIsAvaliable = "requestiIsAvailable";
        public const string SDKIsVerifiedV2 = "requestIsVerifyV2";
        public const string SDKReadPacket = "readPacketText";
        public const string SDKWritePacket = "writePacketText";
        public const string SDKFetchNetState = "requestCurrentNetState";
        public const string SdkIsFirstLogin = "isFirstLogin";
        public const string SDKStartRecyclePing = "requestRecyclePing";
        public const string SDKSetPriority = "setPriority";
        public const string SDKSetUserInfo = "setUserInfo";
        public const string SDKSetPatchVersion = "setGMPatchVersion";
        public const string SDKClearDidAndIid = "requestClearDidAndIid";
        public const string SDKGetSDKURLType = "getSDKURLType";
        public const string SDKSetGameInfo = "setGameInfo";
        public const string SDKAutoLoginPoorNetwork = "autoLoginWithPoorNetworkInterface";
        public const string SDKLaunchEvent = "requestLaunchEvent";

        public const string SDKStartTrace = "startTrace";
        public const string SDKStartSpan = "startSpan";
        public const string SDKEndSpan = "endSpan";
        public const string SDKEndTrace = "endTrace";
        public const string SDKCancelTrace = "cancelTrace";
        public const string SDKReportThreadCount = "reportThreadCount";
        public const string SDKStartCollectCurrent = "startCollectCurrent";
        public const string SDKStopCollectCurrent = "stopCollectCurrent";

        public const string SDKFriendChainInit = "registerFriendChainModule";
        public const string SdkHasOpenFriendChainPermission = "hasOpenFriendChainPermission";
        public const string SdkFetchFriendChainInfo = "showFriendChain";

        public const string SdkIsBsdkAccount = "requestIsBsdkAccount";
        public const string SdkIsBsdkPay = "requestIsBsdkPay";
        public const string SdkCrashInit = "registerCrash";


        // ------ 设备信息 ------
        public const string SdkBatteryRegister = "batteryRegisterReceiver";
        public const string SdkBatteryUnregister = "batteryUnregisterReciver";
        public const string SdkIsCharging = "isCharging";
        public const string SdkBatteryLevel = "getBatteryLevel";
        public const string SdkHeadsetRegister = "headsetRegisterReceiver";
        public const string SdkHeadsetUnregister = "headsetUnregisterReciver";
        public const string SdkIsHeadsetPlugged = "isHeadsetPlugged";
        public const string SdkScreenBrightness = "getScreenBrightness";
        public const string SdkSetScreenBrightness = "setScreenBrightness";
        public const string SdkSetCurrentWindowBrightness = "setCurrentWindowBrightness";
        public const string SdkGetCurrentWindowBrightness = "getCurrentWindowBrightness";
        public const string SdkIsScreenAnomalous = "getScreenIsAnomalous";
        // ----------------------

        // ------ debug,boe,沙盒接口 ------
        public const string SdksetDebug = "setDebug";
        public const string SdksetBOEEnable = "setBOEEnable";
        public const string SdksetSandboxEnable = "setSandboxEnable";
        public const string SdkIsDebugEnable = "SdkIsDebugEnable";
        public const string SdkIsBOEEnable = "SdkIsBOEEnable";
        public const string SdkIsSandboxEnable = "SdkIsSandboxEnable";
        public const string SdksetRNDebugEnable = "setRNDebugEnable";
        public const string SdkIsRNDebugEnable = "SdkIsRNDebugEnable";


        public const string SDKShowPrivacy = "requestShowPrivacy";
        public const string SDKShowPrivacyWithContent = "requestShowPrivacyWithContent";
        public const string SDKCheckHasBindPhone = "requestCheckHasBindPhone";
        public const string SDKCheckHasBindDouYin = "requestCheckHasBindDouYin";
        public const string SDKCheckHasBindToutiao = "requestCheckHasBindToutiao";
        public const string SDKCheckHasBindApple = "requestCheckHasBindApple";
        public const string SDKCheckHasBindHuoShan = "requestCheckHasBindHuoShan";
        public const string SDKCheckHasBindXiGua = "requestCheckHasBindXiGua";
        public const string SDKCheckHasBindTapTap = "requestCheckHasBindTapTap";
        public const string SDKIndependentBindPhone = "requestIndependentBindPhoneMethod";
        public const string SdkGuestLoginDomestic = "guestLoginDomesticInterface";
        public const string SDKChannelGeneralAction = "channelGeneralAction";
        public const string SDKCheckRealNameResult = "checkRealNameResult";
        public const string SDKCheckHasAgreePrivacy = "requestCheckHasAgreePrivacy";
        public const string SDKShowLicensePanel = "requestShowLicense";
        public const string SDKRegisterChannelGeneralCallback = "registerChannelGeneralCallback";
        public const string SdkProtocolAddress = "requestProtocolAddress";
        public const string SdkQRCodeAuthLogin = "requestScanQRCodeLogin";

    }

    public class SDKResultName
    {
        public const string SDKRequestPermissionsResult = "requestPermissionsResult";
        public const string SDKPrivacyResult = "privacyResult";
        public const string SDKLoginResult = "requestLoginResult";
        public const string SDKAuthLoginResult = "authLoginInterfaceResult";
        public const string SDKLogoutResult = "requestLogoutResult";
        public const string SDKRealNameResult = "requestRealNameResult";
        public const string SdkPingResult = "requestPingResult";
        public const string SDKBuyProductResult = "requestBuyProductResult";
        public const string SDKChannelLogoutResult = "requestChannelLogoutResult";
        public const string SDKChannelSwitchAccountResult = "requestChannelSwitchAccountResult";
        public const string SDKChannelBindSuccessResult = "requestChannelBindSuccessResult";
        public const string SDKChannelOnExitResult = "requestChannelExitResult";
        public const string SDKWebViewExitResult = "requestExitResult";
        public const string SDKWebViewCustomParamEvent = "requestCustomCallbackResult";
        public const string SDKCheckRealNameResult = "requestCheckRealNameResult";
        public const string SDKBindInUserCenterResult = "bindInUserCenterResult";

        public const string SdkBatteryRegisterResult = "batteryRegisterResult";
        public const string SdkHeadsetRegisterResult = "headsetRegisterResult";

        public const string SDKLoginPanelClosedResult = "requestLoginPanelClosedResult";
        public const string SDKManagementPanelClosedResult = "requestManagementPanelClosedResult";

        public const string SDKIndependentBindPhoneResult = "requestindependentBindPhoneResult";
        public const string SDKShowPrivacyResult = "requestShowPrivacyResult";
        public const string ChannelGeneralCallbackResult = "channelGeneralCallbackResult";
        public const string QRCodeAuthResult = "scanQRCodeLoginResult";
    }

    public class InitResultRet : CallbackResult
    {
        public int success;
        public int errorCode;
        public string errorMessage;
    }

    public enum GMUserType
    {
        GMUserTypeNone = 0,
        GMUserTypeGuest = 1,
        GMUserTypePhone = 2,
        GMUserTypeTT = 3,
        GMUserTypeAwe = 4,
        GMUserTypeGoogle = 5,
        GMUserTypeFacebook = 6,
        GMUserTypeTwitter = 7,
        GMUserTypeLine = 8,
        GMUserTypeKakaoTalk = 9,
        GMUserTypeVK = 10,
        GMUsertypeApple = 11,
        GMUserTypeHuoShan = 13, /// 因Android端 AOLIGAME = 12 已使用
        GMUsertypeXiGua = 14,
        GMUserTypeTikTok = 15,
        GMUsertypeTapTap = 16,
        GMUsertypeEmail = 17,
        GMUserTypeGooglePlay = 19,
        GMUsertypePassword = 100,  /// 游客或三方可生成帐密类型，目前仅无UI使用
    }

    public class ChannelMethod
    {
        //切换账号接口
        public static string GSDK_API_SWITCH_LOGIN = "gsdk_api_switch_login";
        //用户中心
        public static string GSDK_API_USER_CENTER = "gsdk_api_open_user_center";
        //设置密码
        public static string GSDK_API_SETTING_PASSWORD = "gsdk_api_set_password";
        //打开实名认证界面
        public static string GSDK_API_REAL_NAME_VERIFY = "gsdk_api_real_name_verfiy";
        //是否实名
        public static string GSDK_API_IS_VERIFY = "gsdk_api_is_verify";
        ////查询实名认证结果
        public static string GSDK_API_CHECK_REALNAME_RESULT = "gsdk_api_check_realname_result";

    }

    public class GMUserInfo
    {
        public GMUserType userType;
        public bool isGuest;
        public string userId;
        public string channel;
        public string channelId;
        public List<GMConnectInfo> connectInfos;
        public string conflictUserId;
        public string loginTime;
        public string accountCode;
        public bool cancelLogoff;
    }

    public class GMConnectInfo
    {
        public Int64 puid;
        public GMUserType user_type;
        public string nickname;
        public string avatar_url;
    }

    public class LoginResultRet : CallbackResult
    {
        public string token;
        public GMUserInfo userInfo;
    }

    public class ResetGuestResult : CallbackResult
    {
        public bool needLogout;
    }

    public class LoginResultFullRet
    {
        public string token;
        public string sdkOpenId;
        public GMUserInfo userInfo;
    }

    public class BindLoginResultRet : CallbackResult
    {
        public string token;
        public GMUserInfo userInfo;
        public int bindType;
        public int bindStatus;
    }

    public class LoginWithPoorNetworkResultRet : CallbackResult
    {
        public string token;
        public string sdkOpenId;
        public GMUserInfo userInfo;
    }

    public class LogoutRet : CallbackResult
    {
        public int success;
        public int errorCode;
        public string errorMessage;
    }

    public class HasOpenFriendChainPermissionResultRet : CallbackResult
    {
        public bool havePermission;
    }

    public class FriendChainSDKResultRet : CallbackResult
    {
        public JsonData jsonObj;
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .Append("code=").Append(code)
                .Append(", message='").Append(message).Append("'")
                .Append(", jsonData='").Append(jsonObj.ToJson()).Append("'")
                .Append("}");
            return sb.ToString();
        }
    }


    public enum RealNameAuthLevel
    {
        RealNameAuthLevelDefault = 0,
        RealNameAuthLevelLow = 1,
        RealNameAuthLevelMedium = 2,
        RealNameAuthLevelHigh = 3,
    }

    //errorCode:
    //-1:Cancel
    //-2:LowLevel
    //-1202:AlreadyAuth
    public class RealNameAuthRet : CallbackResult
    {
        public RealNameAuthLevel authLevel;
        public int age;
        public string anti_addiction_tips;
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .Append("code=").Append(code)
                .Append(", message='").Append(message).Append("'")
                .Append(", RealNameAuthLevel='").Append(authLevel).Append("'")
                .Append(", age='").Append(age).Append("'")
                .Append(", anti_addiction_tips='").Append(anti_addiction_tips).Append("'")
                .Append("}");
            return sb.ToString();
        }
    }

    public class RealNameResultRet : CallbackResult
    {
        public bool hasRealName;// 是否实名
        public int age;// 年龄，仅外渠存在
        public string identityNumber;// 脱敏身份证，仅官渠存在
        public string realName;// 脱敏姓名，仅官渠存在
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .Append("code=").Append(code)
                .Append(", message='").Append(message).Append("'")
                .Append(", has_realname='").Append(hasRealName).Append("'")
                .Append(", age='").Append(age).Append("'")
                .Append("}");
            return sb.ToString();
        }
    }

    public class EmulatorResult : CallbackResult
    {
        public Boolean isEmulator;
        public String emulatorBrand;
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .Append("code=").Append(code)
                .Append(", message='").Append(message).Append("'")
                .Append(", isEmulator='").Append(isEmulator).Append("'")
                .Append(", EmulatorBrand='").Append(emulatorBrand).Append("'")
                .Append("}");
            return sb.ToString();
        }
    }

    public class VerifiedResult : CallbackResult
    {
        public Boolean isVerified;
        public Boolean needParentVerify;
    }

    public class ExitResult : CallbackResult
    {
        public Boolean isExit;
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .Append("code=").Append(code)
                .Append(", message='").Append(message).Append("'")
                .Append(", isExit='").Append(isExit).Append("'")
                .Append("}");
            return sb.ToString();
        }
    }

    public class OnExitResult : CallbackResult
    {
        public Boolean hasDialog;
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .Append("code=").Append(code)
                .Append(", message='").Append(message).Append("'")
                .Append(", hasDialog='").Append(hasDialog).Append("'")
                .Append("}");
            return sb.ToString();
        }
    }

    public class OnWebViewCustomParamResult : CallbackResult
    {
        public string data;
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .Append("data=").Append(data)
                .Append("}");
            return sb.ToString();
        }
    }


    public class BulletinRet : CallbackResult
    {
        public GMBulletin bulletin;
    }

    public class PayRequestProductResult : CallbackResult
    {
        public GMRequestProductsModel product;
    }

    public class PayBuyProductResult : CallbackResult
    {
        public string orderId;
        public string productId;
    }

    public class ProtocolAddressResult : CallbackResult
    {
        public string userAgreementUrl;
        public string privacyPolicy;
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .Append("code=").Append(code)
                .Append(", message='").Append(message).Append("'")
                .Append(", userAgreementUrl='").Append(userAgreementUrl).Append("'")
                .Append(", privacyPolicy='").Append(privacyPolicy).Append("'")
                .Append("}");
            return sb.ToString();
        }
    }


    /// <summary>
    /// 公告回调
    /// </summary>
    public class GMBulletin
    {
        // 公告数组
        public List<GMBulletinItem> bulletinItems;
        // 总共页数
        public int totalPage;
        // 当前页数
        public int currentPage;
        // 每页的数量
        public int pageSize;
        // 筛选条件，cp透传进来的
        public UNBridgeLib.LitJson.JsonData filters;
    }

    /// <summary>
    /// 公告回调参数
    /// </summary>
    public class GMBulletinItem
    {
        // 公告ID
        public string bID;
        // 公告展示语言
        public string language;
        // 公告类型
        public int scene;
        // 公告的标题
        public string title;
        // 公告的内容
        public string content;
        // 跳转链接
        public string targetURL;
        // 优先级，置顶:0, 置底:-1,自定义:大于0的自然数
        public int priority;
        // 图片url
        public string imageURL;
        // 富文本编码格式
        public string encoding;
        // 活动公告按钮文字
        public string buttonText;
        // 公告起始时间
        public long startTime;
        // 公告过期时间
        public long expireTime;
        // 跑马灯滚动频率(仅用于跑马灯场景)
        public int frequency;
        // 频率 0:每次，-1:仅展示一次, >0:前n次展示
        public int times;
        // tab文案
        public string tab;
        // 图片信息
        public string imageInfoJson;
        // 公告图片数组
        public List<GMImageItem> imageList;
        // 公告tab标签
        public string tabLabel;
        // 红点提醒开关，开：true、关：false
        public bool badgeSwitch;
        // 额外信息
        public string extra;
    }

    /// <summary>
    /// 公告图片信息
    /// </summary>
    public class GMImageItem
    {
        //图片本身的链接
        public string imageLink;
        //图片跳转链接
        public string imageJumpLink;
        //图片参数
        public string imageInfoJson;
    }

    /// <summary>
    /// 累计赠送的每一个档位描述信息
    /// </summary>
    public class GMProductAccumulationDetail
    {
        /* 赠送游戏币数量 */
        public int coinNumber;
        /* 赠送道具id */
        public string itemId;
        /* 赠送道具数量 */
        public int itemNumber;
        /* 满足赠送条件的数量 */
        public int levelNumber;
    }
    /// <summary>
    /// 累计赠送虚拟币活动
    /// </summary>
    public class GMProductAccumulation
    {
        /* 累计计算中玩家当前有效值 */
        public long currentSum;
        /* 当前版本，服务端验证 */
        public int version;
        /**
 * 礼物描述类型
 * @"coin"，游戏币
 * @"item"，道具
 * @"combo"，组合
 */
        public string giftType;
        /* 开始时间,以秒为单位 */
        public long startTime;
        /* 结束时间 以秒为单位 */
        public long endTime;
        /* 唯一标志 */
        public long accId;
        /* 累计赠送细节 */
        public List<GMProductAccumulationDetail> details;
        /* 描述 */
        public string desc;
    }
    /// <summary>
    /// 虚拟币活动
    /// </summary>
    public class GMProductActivity
    {
        /* 唯一标志 */
        public long activityId;
        /**
 * 活动描述类型
 * @"First"，首冲
 * @"Limit"，道具
 * @"combo"，组合
 */
        public string activityType;
        /**
 * 礼物描述类型
 * @"coin"，游戏币
 * @"item"，道具
 * @"combo"，组合
 */
        public string giftType;
        /* 开始时间 */
        public long startTime;
        /* 结束时间 */
        public long endTime;
        /* 当前版本，服务端验证 */
        public int version;
        /* 是否生效 */
        public bool active;
        /* 赠送游戏币数量 */
        public int coinNumber;
        /* 赠送道具id */
        public string itemId;
        /* 赠送道具数量 */
        public int itemNumber;
        /* 描述 */
        public string desc;
    }

    /// <summary>
    /// 一个商品的信息
    /// </summary>
    public class GMProductModel
    {
        public string appid;  // sdk服务器分配的id
        public string productId;   //游戏服务器分配的商品id
        public string productName;  //游戏定义的商品名称
        public long price; //商品价格
        public string priceDesc;  //商品价格描述
        public string currencyType; //币种
        public int status; //商品状态（上架，下架）
        public long createTime;  //商品在SDK服务器的创建时间
        public long updateTime;  //商品在SDK服务器的更新时间
        public int goodsType; //商品类型
        public string extra; // 折扣信息

        public List<GMProductActivity> activities; //当前活动
        public string activityJSONStringForCreateOrder; //下单的activity字段
    }

    public class GMRequestProductsModel
    {
        public List<GMProductModel> products;//商品列表
        public List<GMProductAccumulation> accumulations; //累计列表

    }

    /// <summary>
    /// 定位信息
    /// </summary>
    public class GMLocationModel
    {
        public string country;
        public string countryAsci;
        public string countryCode;
        public string administrativeArea;
        public string administrativeAreaAsci;
        public string city;
        public string cityAsci;
        public string district;
        public string districtAsci;
        public double latitude;
        public double longitude;
        public long countryGeoNameId;
        public long administrativeAreaGeoNameId;
        public long cityGeoNameId;
        public long districtGeoNameId;
        public bool isDisputed;
        public int code;
        public string message;

        public GMLocationModel()
        {
            country = "";
            countryCode = "";
            administrativeArea = "";
            city = "";
            administrativeAreaAsci = "";
            cityAsci = "";
            district = "";
            countryAsci = "";
            districtAsci = "";
            latitude = 0;
            longitude = 0;
            cityGeoNameId = 0;
            countryGeoNameId = 0;
            districtGeoNameId = 0;
            administrativeAreaGeoNameId = 0;
            code = 0;
            message = "";
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .Append("country= ").Append(country)
                .Append(", countryCode= ").Append(countryCode)
                .Append(", administrativeArea= ").Append(administrativeArea)
                .Append(", city= ").Append(city)
                .Append(", administrativeAreaAsci= ").Append(administrativeAreaAsci)
                .Append(", cityAsci= ").Append(cityAsci)
                .Append(", district= ").Append(district)
                .Append(", countryAsci= ").Append(countryAsci)
                .Append(", districtAsci= ").Append(districtAsci)
                .Append(", latitude= ").Append(latitude)
                .Append(", longitude= ").Append(longitude)
                .Append(", cityGeoNameId= ").Append(cityGeoNameId)
                .Append(", countryGeoNameId= ").Append(countryGeoNameId)
                .Append(", districtGeoNameId= ").Append(districtGeoNameId)
                .Append(", administrativeAreaGeoNameId= ").Append(administrativeAreaGeoNameId)
                .Append(", isDisputed= ").Append(isDisputed)
                .Append("}");
            return sb.ToString();
        }
    }

    /// <summary>
    /// 定位回调
    /// </summary>
    public class LocationResult : CallbackResult
    {
        public GMLocationModel Location;

        public LocationResult(GMLocationModel location)
        {
            Location = location;
            code = location.code;
            message = location.message;
        }
    }

    public class ZonesListResult : CallbackResult
    {
        public List<GMZoneModel> zoneList;
    }

    public class RolesListResult : CallbackResult
    {
        public List<GMRoleModel> roleList;
    }

    public class ServersAndRolesResult : CallbackResult
    {
        public GMServerAndRoleModel serverAndRole;
    }

    public class PingServersResult : CallbackResult
    {
        public GMServerModel server;
        public bool finish;
    }

    /// <summary>
    /// 一个区服信息
    /// </summary>
    public class GMZoneModel
    {
        public string zoneName;
        public int zoneId;
        public string channelId;
        public string extraInfo;
        public Dictionary<string, string> extraKV;
        public List<GMServerModel> servers;
    }

    public class GMServerModel
    {
        public int serverId;
        public string serverName;
        public int serverType;
        public string serverEntry;
        public int opStatus;
        public int realServerId;
        public bool isMerged;
        public int onlineLoad;
        public string extraInfo;
        public List<GMServerTagModel> tags;
        public int openTimestamp;
        public Dictionary<string, string> extraKV;
        public string pingAddr;
        public int time;
    }

    public class GMServerTagModel
    {
        public string tagName;
        public int tagValue;
    }

    public class GMRoleModel
    {
        public string roleId;
        public string roleName;
        public string roleLevel;
        public int realServerId;
        public string serverName;
        public long loginTime;
        public string avatarUrl;
        public string job;
        public string extra;
    }

    public class GMServerAndRoleModel
    {
        public List<GMZoneModel> zones;
        public List<GMRoleModel> roles;
    }

    public enum GMDevLogLevel
    {
        GMDevLogLevelWarn,              //cp_warn
        GMDevLogLevelError,             //cp_error
        GMDevLogLevelException,         //cp_exception
        GMDevLogLevelAssert,            //cp_assert
        GMDevLogLevelProcessInfo,       //cp_processInfo
    }

    public interface IUserCenter
    {
        void logoutCallback(CallbackResult ret);
        void realNameCallback(RealNameAuthRet ret);
        void bindCallback(BindIndependentResult ret);
        void closeCallback(CallbackResult ret);
    }

    public class GMExeption
    {
        public Exception exception;
    }

    public interface GMExeptionInterface
    {
        void onGMExceptionCallback(GMExeption e);
    }

    public interface IChannelExtraCallbackCenter
    {
        void logoutChannelCallback(CallbackResult ret);
        void switchAccountChannelCallback(LoginResultRet ret);
        void onExitChannelCallback(ExitResult ret);
    }

    public class CpUploadInfo
    {
        public List<string> balanceids; //余额货币种类id list

        public List<string> balancenames; //余额货币种类name list

        public List<string> balancenums; // 余额货币数额 list

        public List<string> roleids;  // 关系角色id list

        public List<string> intimacys; // 亲密度 list

        public List<string> nexusids; // 关系id list

        public List<string> nexusnames; // 关系name list

        public List<string> listids; // 榜单id list

        public List<string> listnames; //榜单name list

        public List<string> nums; //榜单排名name list

        public List<string> coins; //排名指标name list

        public List<string> costs; //排名指标名称 list

        public string type;  //（必填）角色状态（enterServer（登录），levelUp（升级），createRole（创建角色），exitServer（退出））

        public string zoneid;  //（必填）游戏区服ID

        public string zonename;  //（必填）游戏区服名称

        public string roleid;  //（必填）玩家角色ID

        public string rolename;  //（必填）玩家角色名

        public string professionid;  //（必填）职业ID

        public string profession;  //（必填）职业名称

        public string gender;  //（必填）性别

        public string professionroleid;  //（选填）职业称号ID

        public string professionrolename;  //（选填）职业称号

        public string rolelevel;  //（必填）玩家角色等级

        public string power;  //（必填）战力数值

        public string vip;  //（必填）当前用户VIP等级

        public string balance;  //（必填）帐号余额

        public string partyid;  //（必填）所属帮派帮派ID

        public string partyname;  //（必填）所属帮派名称

        public string partyroleid;  //（必填）帮派称号ID

        public string partyrolename;  //（必填）帮派称号名称

        public string friendlist;  //（必填）好友关系

        public string ranking;  //（选填）排行榜列表

        public string chapter; //游戏关卡

        public string serverId; //服区Id

        public string serverName; //服区name
    }

    // 权限申请的回调
    public interface IRequestPermissionCallback
    {
        // 权限申请回调结果
        void OnPermissionResult(PermissionRequestResult result);
        // 隐私检测结果
        void onPrivacyResult(PrivacyResult result);
    }

    //请求权限回调结果
    public class PermissionRequestResult : CallbackResult
    {
        public bool isAllGranted;
        public List<String> permissions;
        public List<int> grantResults;
    }

    //请求权限隐私权限回调
    public class PrivacyResult : CallbackResult
    {
        public bool isNeedVerify;
        public bool verifyResult;
    }

    //请求获取用户隐私权限回调
    public class GMShowPrivacyResult : CallbackResult
    {
        public bool hasAgree; // 用户是否同意过
    }

    public class BindIndependentResult : CallbackResult
    {
        public int type;
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .Append("code=").Append(code)
                .Append(", message='").Append(message).Append("'")
                .Append(", type='").Append(type).Append("'")
                .Append("}");
            return sb.ToString();
        }
    }

    public class BatteryInfoCallbackResult : CallbackResult
    {
        public bool status;
    }

    public class HeadsetInfoCallbackResult : CallbackResult
    {
        public bool status;
    }


    public class ChannelGeneralCallbackResult : CallbackResult
    {
        public JsonData extraData;
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("{")
                .Append("code=").Append(code)
                .Append(", message='").Append(message).Append("'")
                .Append(", jsonData='").Append(extraData.ToJson()).Append("'")
                .Append("}");
            return sb.ToString();
        }
    }
    
    public class QRCodeAuthResult : CallbackResult
    {
        public int status;
    }

}

