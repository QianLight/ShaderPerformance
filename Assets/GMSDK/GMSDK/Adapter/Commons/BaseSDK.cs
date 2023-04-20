using System.Collections.Generic;
using UNBridgeLib;
using UNBridgeLib.LitJson;
using GMSDK;
using System;
using System.Runtime.InteropServices;
using System.Text;
using GSDK;
using UnityEngine;

namespace GMSDK
{

    public class MainSDK
    {
        public const string Version = VersionCode.UnityVersion;

        // 初始化
        public void SdkInit(Action<CallbackResult> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                initCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnInitCallBack);
            UNBridge.Call(SDKMethodName.SDKInit, null, unCallBack);
            //TODO:Native上 ios还没拆出来，Android已经拆出来了，因此在此处先注册WebView，后续拆出来
#if UNITY_ANDROID       
			UNBridge.Call("registerWebView", null);
            UNBridge.Call(SDKMethodName.SdkCrashInit, null);
            UNBridge.Call(SDKMethodName.SDKFriendChainInit, null);
#endif
        }

        // 更新sdk 配置项，需要在sdk初始化前调用
        public void SDKUpdateConfig(Dictionary<String, System.Object> extraParams)
        {
            LogUtils.D("Sdk -- Unity -- SDKUpdateConfig");
            var jsonData = new JsonData();
            jsonData["config"] = JsonMapper.ToObject(JsonMapper.ToJson(extraParams));

            UNBridge.Call(SDKMethodName.SDKUpdateConfig, jsonData);
        }

        // 触发Bridge层初始化的一个空实现
        // 目的是为了触发补单逻辑
        // 只有iOS有
#if UNITY_IOS
        public void SDKTriggerBridgeInit(Action<CallbackResult> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                triggerInitCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnTriggerBridgeInitCallBack);

            UNBridge.Call(SDKMethodName.SDKTriggerBridgeInit, null, unCallBack);
        }
#endif

        // 获取DeviceId
        public string SdkGetDeviceId()
        {
            GSDKProfilerTools.BeginSample("Main DeviceID");
            object res = UNBridge.CallSync(SDKMethodName.SDKGetDeviceId, null);
            GSDKProfilerTools.EndSample();
            return res != null ? res.ToString() : "";
        }

        //获取当前渠道
        public string SdkGetChannel()
        {
            LogUtils.D("Sdk -- Unity -- SdkGetChannel");
            object ret = UNBridge.CallSync(SDKMethodName.SDKGetChannel, null);
            return ret != null ? (string)ret : "";
        }

        //获取当前渠道商
        public string SdkGetChannelOp()
        {
            LogUtils.D("Sdk -- Unity -- SdkGetChannelOp");
            object ret = UNBridge.CallSync(SDKMethodName.SDKGetChannelOp, null);
            return ret != null ? (string)ret : "";
        }

        //获取当前支付渠道商
        public string SdkGetPayChannelOp()
        {
            LogUtils.D("Sdk -- Unity -- SdkGetPayChannelOp");
#if UNITY_ANDROID
            object ret = UNBridge.CallSync(SDKMethodName.SDKGetPayChannelOp, null);
#else 
            object ret = "bsdkintl";
#endif
            return ret != null ? (string)ret : "";
        }
        
        //获取AppId
        public string SdkGetAppId()
        {
            LogUtils.D("Sdk -- Unity -- SdkGetAppId");
            object ret = UNBridge.CallSync(SDKMethodName.SDKGetAppId, null);
            return ret != null ? (string)ret : "";
        }

        //获取当前Native SDK版本
        public string SdkGetSDKVersion()
        {
            LogUtils.D("Sdk -- Unity -- SdkGetSDKVersion");
            object ret = UNBridge.CallSync(SDKMethodName.SDKGetSDKVersion, null);
            return ret != null ? (string)ret : "";
        }

        // 获取SdkOpenId
        public string SdkGetSdkOpenId()
        {
            object res = UNBridge.CallSync(SDKMethodName.SDKGetSdkOpenId, null);
            return res != null ? res.ToString() : "";
        }
        
        
        // 获取DownLoadSource
        public string SDKGetDownloadSource()
        {
            object res = UNBridge.CallSync(SDKMethodName.SDKGetDownloadSource, null);
            return res != null ? res.ToString() : "";
        }

        public void SDKRequestPermission(List<String> permissions, IRequestPermissionCallback callback)
        {
            LogUtils.D("SDKRequestPermission");
            JsonData data = new JsonData();
            foreach (string permission in permissions)
            {
                data.Add(permission);
            }
            JsonData finalParam = new JsonData();
            finalParam["permissions"] = data;
            //权限申请结果
            ListenPermissionResult(callback);
            //隐私检测结果
            ListenPrivacyResult(callback);
            UNBridge.Call(SDKMethodName.SDKRequestPermissions, finalParam);
        }

        public void SDKRequestAppTracking()
        {
            LogUtils.D("SDKRequestAppTracking");
            UNBridge.Call(SDKMethodName.SDKRequestAppTracking);
        }

        // 监听权限结果
        void ListenPermissionResult(IRequestPermissionCallback callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler();
            unCallBack.permissionRequestCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnPermissionCallBack);
            UNBridge.Listen(SDKResultName.SDKRequestPermissionsResult, unCallBack);
        }

        // 监听隐私结果
        void ListenPrivacyResult(IRequestPermissionCallback callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler();
            unCallBack.permissionRequestCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnPrivacyCallBack);
            UNBridge.Listen(SDKResultName.SDKPrivacyResult, unCallBack);
        }

        // 获取InstallId
        public string SdkGetInstallId()
        {
            object res = UNBridge.CallSync(SDKMethodName.SDKGetInstallId, null);
            return res != null ? res.ToString() : "";
        }
        #region 登录模块
        // 登录
        public void SdkLogin(Action<LoginResultRet> callback, Action<CallbackResult> panelClosedCallback = null)
        {
            // 监听登录事件
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                loginCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnLoginCallBack);
            UNBridge.Listen(SDKResultName.SDKLoginResult, unCallBack);
            
            // 监听面板关闭事件
            GMUNCallbackHandler panelClosedUnCallback = new GMUNCallbackHandler()
            {
                panelClosedResult = panelClosedCallback,
            };
            panelClosedUnCallback.OnSuccess = panelClosedUnCallback.OnPanelClosedCallBack;
            
            UNBridge.Listen(SDKResultName.SDKLoginPanelClosedResult, panelClosedUnCallback);
            UNBridge.Call(SDKMethodName.SDKLogin);
        }

        // 授权登录
        // 从 2.11.0.0 开始，同时支持国内的三方授权登录
        public void SdkAuthLoginInterface(GMUserType type, Action<LoginResultRet> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                loginCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnAuthLoginInterfaceCallBack);
            JsonData param = new JsonData();
            param["type"] = (int)type;
            UNBridge.Listen(SDKResultName.SDKAuthLoginResult, unCallBack);
            UNBridge.Call(SDKMethodName.SDKAuthLogin, param);
        }

        // 扫码授权PC端登录
        // 从 3.2.0.0 开始，仅支持国内扫描PC端二维码，通过移动端授权PC端登录
        public void SdkScanQRCodeAuthLogin(string token, Action<QRCodeAuthResult> callback)
        {
            GMUNCallbackHandler unCallback = new GMUNCallbackHandler()
            {
                qrcodeAuthCallback = callback
            };
            unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnQRCodeAuthCallBack);
            JsonData param = new JsonData();
            param["token"] = token;
            UNBridge.Listen(SDKResultName.QRCodeAuthResult, unCallback);
            UNBridge.Call(SDKMethodName.SdkQRCodeAuthLogin, param);
        }

        // 登出
        public void SdkLogoutAccount(Action<CallbackResult> callback)
        {
            LogUtils.D("Sdk -- Unity -- Logout");
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                logoutCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnLogoutCallBack);
            UNBridge.Call(SDKMethodName.SDKLogout, null, unCallBack);
        }

        //登出token过期
        public void SdkLogoutAndSetTokenExpired(Action<CallbackResult> callback)
        {
            LogUtils.D("Sdk -- Unity -- SdkLogoutAndSetTokenExpired");
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                logoutAndSetTokenExpiredCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnLogoutAndSetTokenExpiredCallBack);
            UNBridge.Call(SDKMethodName.SDKLogoutAndSetTokenExpired, null, unCallBack);
        }

        // 获取登录状态
        public bool SdkGetLoginState()
        {
            object res = UNBridge.CallSync(SDKMethodName.SDKLoginState, null);
            return res != null ? (bool)res : false;
        }

        // 重置遊客
        public void SdkResetGuest(Action<ResetGuestResult> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                resetGuestCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnResetGuestCallback);
            UNBridge.Call(SDKMethodName.SDKResetGuest, null, unCallBack);
        }

        // 检测协议是否更新
        public bool SdkIsProtocolVersionUpdated()
        {
            object res = UNBridge.CallSync(SDKMethodName.SDKProtocolVersion, null);
            return res != null ? (bool)res : false;
        }


#if UNITY_IPHONE
        // 获取当前是否是沙盒环境 0线上环境 1线上BOE 2沙盒环境 3沙盒BOE
        public int SdkGetSDKURLType()
        {
            object res = UNBridge.CallSync(SDKMethodName.SDKGetSDKURLType, null);
            LogUtils.D("SdkGetSDKURLType res:" + res);
            return res != null ? (int)res : 0;
        }
#endif
        //支持弱网自动登录
        public void SDKAutoLoginPoorNetworkInterface(Action<LoginWithPoorNetworkResultRet> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                loginWithPoorNetworkCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnLoginWithpoorNetworkInterfaceCallBack);

            UNBridge.Call(SDKMethodName.SDKAutoLoginPoorNetwork, null, unCallBack);
        }

        // 是否开启平台好友关系链权限，限头条（GMAuthTypeTT）、抖音（GMAuthTypeAwe）、Facebook（GMAuthTypeFacebook）
        public void SdkHasOpenFriendChainPermission(GMUserType type, Action<HasOpenFriendChainPermissionResultRet> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                hasOpenFriendChainPermissionCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnHasOpenFriendChainPermissionCallback);
            JsonData param = new JsonData();
            param["type"] = (int)type;
            UNBridge.Call(SDKMethodName.SdkHasOpenFriendChainPermission, param, unCallBack);
        }

        // sdk查询平台好友关系链权限，限头条（GMAuthTypeTT）、抖音（GMAuthTypeAwe）、Facebook（GMAuthTypeFacebook）
        public void SdkFetchFriendChainInfo(GMUserType type, bool needRoleInfo, Action<FriendChainSDKResultRet> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                fetchFriendChainCallBack = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnFriendChainInfoCallback);
            JsonData param = new JsonData();
            param["type"] = (int)type;
            param["needRoleInfo"] = needRoleInfo;
            UNBridge.Call(SDKMethodName.SdkFetchFriendChainInfo, param, unCallBack);
        }

#if UNITY_ANDROID
        // 是否是云游戏环境
        public bool SdkIsCloudRuntime()
        {
            object res = UNBridge.CallSync(SDKMethodName.SDKIsCloudRuntime, null);
            return res != null ? (bool)res : false;
        }
        
        // 云游戏登录
        public void SdkCloudGameLogin(Action<LoginResultRet> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                loginCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnLoginCallBack);
            UNBridge.Listen(SDKResultName.SDKLoginResult, unCallBack);

            UNBridge.Call(SDKMethodName.SDKCloudGameLogin);
        }
        
        // 云游戏登录
        public void SdkCloudGameLoginV2(Action<LoginResultRet> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                loginCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnLoginCallBack);
            UNBridge.Listen(SDKResultName.SDKLoginResult, unCallBack);

            UNBridge.Call(SDKMethodName.SDKCloudGameLoginV2);
        }
#endif
        

        // 打开用户中心， -1表示用户未登录，0表示成功
        public int SdkShowUserCenter(IUserCenter iHandler)
        {
            ListenLogoutEvent(iHandler);
            ListenRealNameAuth(iHandler);
            ListenBindInUserCenterAuth(iHandler);
            ListenPanelClosed(iHandler);
            object res = UNBridge.CallSync(SDKMethodName.SDKShowUserCenter, null);
            return res != null ? (int)res : 0;
        }

        // 游客登录
        public void SdkGuestLoginDomesticInterface(Action<LoginResultRet> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                loginCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnLoginInterfaceCallBack);

            UNBridge.Call(SDKMethodName.SdkGuestLoginDomestic, null, unCallBack);
        }

        // 监听退出登录的事件
        void ListenLogoutEvent(IUserCenter callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler();
            unCallBack.userCenter = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnLogoutCallBack);
            UNBridge.Listen(SDKResultName.SDKLogoutResult, unCallBack);
        }

        // 监听用户中心的绑定事件
        void ListenBindInUserCenterAuth(IUserCenter callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler();
            unCallBack.userCenter = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnBindInUserCenterCallBack);
            UNBridge.Listen(SDKResultName.SDKBindInUserCenterResult, unCallBack);
        }

        // 监听实名认证事件
        void ListenRealNameAuth(IUserCenter callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler();
            unCallBack.userCenter = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnRealNameAuthCallBack);
            UNBridge.Listen(SDKResultName.SDKRealNameResult, unCallBack);
        }
        
        // 监听用户中心面板关闭事件
        void ListenPanelClosed(IUserCenter callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                userCenter = callback
            };
            unCallBack.OnSuccess = unCallBack.OnUserCenterClosedCallBack;
            UNBridge.Listen(SDKResultName.SDKManagementPanelClosedResult, unCallBack);
        }

        /*此接口只针对android生效*/
        //渠道监听回调
        // 注册渠道所需callback
        public void RegisterAccountStatusChangedListener(IChannelExtraCallbackCenter iHandler)
        {
            ListenChannelLogoutEvent(iHandler);
            ListenChannelSwitchAccountEvent(iHandler);
            ListenChannelOnExitEvent(iHandler);
            UNBridge.Call(SDKMethodName.SDKRegisterAccountStatusChangedListener, null);
        }

        // 监听渠道退出事件
        void ListenChannelLogoutEvent(IChannelExtraCallbackCenter callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler();
            unCallBack.channelExtraCallbackCenter = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnChannelLogoutCallBack);
            UNBridge.Listen(SDKResultName.SDKChannelLogoutResult, unCallBack);
        }

        // 监听渠道切换帐号事件
        void ListenChannelSwitchAccountEvent(IChannelExtraCallbackCenter callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler();
            unCallBack.channelExtraCallbackCenter = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnChannelSwicthAccountCallBack);
            UNBridge.Listen(SDKResultName.SDKChannelSwitchAccountResult, unCallBack);
        }

        // 监听游戏退出
        void ListenChannelOnExitEvent(IChannelExtraCallbackCenter callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler();
            unCallBack.channelExtraCallbackCenter = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnChannelExitCallBack);
            UNBridge.Listen(SDKResultName.SDKChannelOnExitResult, unCallBack);
        }

        /*此接口只针对android生效*/
        public void EnterGameUpload(CpUploadInfo cpUploadInfo)
        {
            JsonData param = createUserInfoJson(cpUploadInfo);
            JsonData finalParam = new JsonData();
            finalParam["cpUploadInfo"] = param;
            UNBridge.Call(SDKMethodName.SDKEnterGameUpload, finalParam);
        }

        /*此接口只针对android生效*/
        public void CreateNewRoleUpload(CpUploadInfo cpUploadInfo)
        {
            JsonData param = createUserInfoJson(cpUploadInfo);
            JsonData finalParam = new JsonData();
            finalParam["cpUploadInfo"] = param;
            UNBridge.Call(SDKMethodName.SDKCreateNewRoleUpload, finalParam);
        }

        /*此接口只针对android生效*/
        public void RoleLevelUpload(CpUploadInfo cpUploadInfo)
        {
            JsonData param = createUserInfoJson(cpUploadInfo);
            JsonData finalParam = new JsonData();
            finalParam["cpUploadInfo"] = param;
            UNBridge.Call(SDKMethodName.SDKRoleLevelUpload, finalParam);
        }

        /*此接口只针对android生效*/
        public void RoleExitUpload(CpUploadInfo cpUploadInfo)
        {
            JsonData param = createUserInfoJson(cpUploadInfo);
            JsonData finalParam = new JsonData();
            finalParam["cpUploadInfo"] = param;
            UNBridge.Call(SDKMethodName.SDKRoleExitUpload, finalParam);
        }

        private JsonData createUserInfoJson(CpUploadInfo cpUploadInfo)
        {
            JsonData param = new JsonData();
            param["type"] = cpUploadInfo.type;
            param["zoneid"] = cpUploadInfo.zoneid;
            param["zonename"] = cpUploadInfo.zonename;
            param["roleid"] = cpUploadInfo.roleid;
            param["rolename"] = cpUploadInfo.rolename;
            param["professionid"] = cpUploadInfo.professionid;
            param["profession"] = cpUploadInfo.profession;
            param["gender"] = cpUploadInfo.gender;
            param["professionroleid"] = cpUploadInfo.professionroleid;
            param["professionrolename"] = cpUploadInfo.professionrolename;
            param["rolelevel"] = cpUploadInfo.rolelevel;
            param["power"] = cpUploadInfo.power;
            param["vip"] = cpUploadInfo.vip;
            param["balance"] = cpUploadInfo.balance;
            param["partyid"] = cpUploadInfo.partyid;
            param["partyname"] = cpUploadInfo.partyname;
            param["partyroleid"] = cpUploadInfo.partyroleid;
            param["partyrolename"] = cpUploadInfo.partyrolename;
            param["friendlist"] = cpUploadInfo.friendlist;
            param["ranking"] = cpUploadInfo.ranking;
            param["chapter"] = cpUploadInfo.chapter;
            param["serverId"] = cpUploadInfo.serverId;
            param["serverName"] = cpUploadInfo.serverName;
            param["balanceids"] = JsonMapper.ToJson(getNoneNullValue(cpUploadInfo.balanceids));
            param["balancenames"] = JsonMapper.ToJson(getNoneNullValue(cpUploadInfo.balancenames));
            param["balancenums"] = JsonMapper.ToJson(getNoneNullValue(cpUploadInfo.balancenums));
            param["roleids"] = JsonMapper.ToJson(getNoneNullValue(cpUploadInfo.roleids));
            param["intimacys"] = JsonMapper.ToJson(getNoneNullValue(cpUploadInfo.intimacys));
            param["nexusids"] = JsonMapper.ToJson(getNoneNullValue(cpUploadInfo.nexusids));
            param["nexusnames"] = JsonMapper.ToJson(getNoneNullValue(cpUploadInfo.nexusnames));
            param["listids"] = JsonMapper.ToJson(getNoneNullValue(cpUploadInfo.listids));
            param["listnames"] = JsonMapper.ToJson(getNoneNullValue(cpUploadInfo.listnames));
            param["nums"] = JsonMapper.ToJson(getNoneNullValue(cpUploadInfo.nums));
            param["coins"] = JsonMapper.ToJson(getNoneNullValue(cpUploadInfo.coins));
            param["costs"] = JsonMapper.ToJson(getNoneNullValue(cpUploadInfo.costs));
            return param;
        }

        public List<string> getNoneNullValue(List<string> param)
        {
            if (param == null)
            {
                param = new List<string>();
            }
            return param;
        }

        /// <summary>
        /// 实名认证（SDK自有界面）认证等级根据type来定
        /// </summary>
        /// <param name="type">
        /// 0：认证等级走网络配置 
        /// 2：认证等级不走网络配置,可以关闭实名认证页面，跳过认证 
        /// 3：认证等级不走网络配置,强制实名认证 </param>
        public void SdkRealNameAuth(int type, Action<RealNameAuthRet> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                realNameCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnRealNameAuthCallBack);
            UNBridge.Listen(SDKResultName.SDKRealNameResult, unCallBack);

            JsonData param = new JsonData();
            param["type"] = type;
            UNBridge.Call(SDKMethodName.SDKRealNameAuth, param);
        }

        /// <summary>
        /// 用户实名认证的接口（无UI界面）, Android 特有其他联运渠道使用
        /// </summary>
        /// <param name="sdkOpenId">xxxxxxx</param>
        /// <param name="name">xxx</param>
        /// <param name="idCardNum">610xxxxxx</param>
        /// <param name="callback">回调</param>
        public void SdkRealNameAuth(string sdkOpenId, string name, string idCardNum, Action<RealNameAuthRet> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                realNameCallback = callback
            };
            JsonData param = new JsonData();
            param["sdkOpenId"] = sdkOpenId;
            param["name"] = name;
            param["idCardNum"] = idCardNum;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnRealNameAuthCallBack);
            UNBridge.Call(SDKMethodName.SDKRealNameAuthWithoutUI, param, unCallBack);
        }
        
        /// <summary>
        /// 展示隐私协议页面
        /// </summary>
        /// <param name="callback">回调</param>
        public void SDKShowPrivacy(Action<GMShowPrivacyResult> callback)
        {
            GMUNCallbackHandler unCallback = new GMUNCallbackHandler()
            {
                showPrivacyCallBack = callback
            };
            unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnShowPrivacyCallBack);
            // 监听隐私协议回调
            UNBridge.Listen(SDKResultName.SDKShowPrivacyResult, unCallback);
            UNBridge.Call(SDKMethodName.SDKShowPrivacy);
        }

        /// <summary>
        /// 展示隐私协议页面
        /// </summary>
        /// <param name="callback">回调</param>
        public void SDKShowPrivacy(string content, Action<GMShowPrivacyResult> callback)
        {
            GMUNCallbackHandler unCallback = new GMUNCallbackHandler()
            {
                showPrivacyCallBack = callback
            };
            unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnShowPrivacyCallBack);
            // 监听隐私协议回调
            UNBridge.Listen(SDKResultName.SDKShowPrivacyResult, unCallback);
            JsonData param = new JsonData();
            param["content"] = content;
            UNBridge.Call(SDKMethodName.SDKShowPrivacyWithContent, param);
        }

        /// <summary>
        /// 获取当前用户是否绑定了手机号
        /// </summary>
        /// <return> bool </return>
        public bool SdkCheckHasBindPhone()
        {
            object res = UNBridge.CallSync(SDKMethodName.SDKCheckHasBindPhone, null);
            return res != null ? (bool)res : false;
        }

        /// <summary>
        /// 获取当前用户是否绑定了抖音
        /// </summary>
        /// <return> bool </return>
        public bool SdkCheckHasBindDouYin()
        {
            object res = UNBridge.CallSync(SDKMethodName.SDKCheckHasBindDouYin, null);
            return res != null ? (bool)res : false;
        }

        /// <summary>
        /// 获取当前用户是否绑定了头条
        /// </summary>
        /// <return> bool </return>
        public bool SdkCheckHasBindTouTiao()
        {
            object res = UNBridge.CallSync(SDKMethodName.SDKCheckHasBindToutiao, null);
            return res != null ? (bool)res : false;
        }

        /// <summary>
        /// 获取当前用户是否绑定了Apple
        /// </summary>
        /// <return> bool </return>
        public bool SdkCheckHasBindApple()
        {
            object res = UNBridge.CallSync(SDKMethodName.SDKCheckHasBindApple, null);
            return res != null ? (bool)res : false;
        }

        /// <summary>
        /// 获取当前用户是否绑定了抖音火山版
        /// </summary>
        /// <return> bool </return>
        public bool SdkCheckHasBindHuoShan()
        {
            object res = UNBridge.CallSync(SDKMethodName.SDKCheckHasBindHuoShan, null);
            return res != null ? (bool)res : false;
        }

        /// <summary>
        /// 获取当前用户是否绑定了西瓜视频
        /// </summary>
        /// <return> bool </return>
        public bool SdkCheckHasBindXiGua()
        {
            object res = UNBridge.CallSync(SDKMethodName.SDKCheckHasBindXiGua, null);
            return res != null ? (bool)res : false;
        }

        /// <summary>
        /// 获取当前用户是否绑定了TapTap
        /// </summary>
        /// <return> bool </return>
        public bool SdkCheckHasBindTapTap()
        {
            object res = UNBridge.CallSync(SDKMethodName.SDKCheckHasBindTapTap, null);
            return res != null ? (bool)res : false;
        }

        /// <summary>
        /// 查询实名认证结果，以方便营销做实名认证激励
        /// </summary>
        public void SdkCheckRealNameResult(Action<RealNameResultRet> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                checkRealNameCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnCheckRealNameResultCallBack);
            UNBridge.Listen(SDKResultName.SDKCheckRealNameResult, unCallBack);
            UNBridge.Call(SDKMethodName.SDKCheckRealNameResult);
        }

        public void SdkShowLicense(Action<CallbackResult> panelClosedCallback = null)
        {
            LogUtils.D("Sdk -- Unity -- SdkShowLicense");
            GMUNCallbackHandler panelClosedUnCallBack = new GMUNCallbackHandler()
            {
                panelClosedResult = panelClosedCallback
            };
            panelClosedUnCallBack.OnSuccess = panelClosedUnCallBack.OnPanelClosedCallBack;
            UNBridge.Listen(SDKResultName.SDKManagementPanelClosedResult, panelClosedUnCallBack);
            UNBridge.Call(SDKMethodName.SDKShowLicensePanel);
        }

        public bool SdkCheckHasAgreePrivacy()
        {
            object res = UNBridge.CallSync(SDKMethodName.SDKCheckHasAgreePrivacy, null);
            return res != null ? (bool)res : false;
        }

        public void SdkChannelGeneral(JsonData json)
        {
            LogUtils.D("Sdk -- Unity -- SdkChannelGeneral");
            JsonData param = new JsonData();
            param["generalData"] = json;
            UNBridge.Call(SDKMethodName.SDKChannelGeneralAction, param);
        }

        public void SdkChannelGeneralCallback(Action<ChannelGeneralCallbackResult> callback)
        {
            LogUtils.D("Sdk -- Unity -- SdkChannelGeneralCallback");
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                channelGeneralCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnChannelGeneralCallback);
            UNBridge.Listen(SDKResultName.ChannelGeneralCallbackResult, unCallBack);
            UNBridge.Call(SDKMethodName.SDKRegisterChannelGeneralCallback);

        }


        // 独立打开手机号绑定面板， -1表示用户未登录，0表示成功，回调绑定事件
        public int SdkShowIndependentBindPhone(Action<BindIndependentResult> callback, Action<CallbackResult> panelClosedCallback = null)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                bindPhoneCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnIndependentBindPhoneCallBack);
            UNBridge.Listen(SDKResultName.SDKIndependentBindPhoneResult, unCallBack);
            GMUNCallbackHandler panelClosedUnCallBack = new GMUNCallbackHandler()
            {
                panelClosedResult = panelClosedCallback
            };
            panelClosedUnCallBack.OnSuccess = panelClosedUnCallBack.OnPanelClosedCallBack;
            UNBridge.Listen(SDKResultName.SDKManagementPanelClosedResult, panelClosedUnCallBack);
            object res = UNBridge.CallSync(SDKMethodName.SDKIndependentBindPhone, null);
            return res != null ? (int)res : -1;
        }

        //获取协议地址
        public void SdkProtocolAddress(Action<ProtocolAddressResult> callback)
        {
            LogUtils.D("Sdk -- Unity -- SdkProtocolAddress");
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                protocolAddressCallBack = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnProtocolAddressCallBack);
            UNBridge.Call(SDKMethodName.SdkProtocolAddress, null, unCallBack);
        }
        
        #endregion


        /// <summary>
        /// 打开内置webview,回调不为null时在webview关闭时处理回调
        ///
        /// Webview 一级错误码使用：GSDK.ErrorCode.WebviewXXX
        /// 具体内容请参考 Assets/GSDK/Service/Webview/IWebviewService.cs 中的 ErrorCode 部分
        /// </summary>
        /// <param name="parameter">打开WebView的参数</param>
        /// <param name="exitCallback">退出webview返回游戏回调,默认不传，不传时无回调</param>
        /// <param name="showCallback">webview调用是否成功的回调</param>
        /// <param name="paramCallback">JSbridge自定义参数回调</param>
        public void SdkShowWebViewController(WebviewParameter parameter, 
            Action<CallbackResult> exitCallback = null, Action<CallbackResult> showCallback = null,
            Action<OnWebViewCustomParamResult> paramCallback = null)
        {
            JsonData p = new JsonData();
            p["title"] = parameter.Title;
            p["url"] = parameter.URL;
            p["fromSource"] = parameter.Source;
            p["orientation"] = (int)parameter.Orientation;
            p["customsize_screen"] = parameter.CustomsizeScreen;
            p["x"] = parameter.LocationX;
            p["y"] = parameter.LocationY;
            p["height"] = parameter.Height;
            p["width"] = parameter.Width;
            p["horizontal_scrollbar_hidden"] = parameter.HorizontalScrollbarHidden;
            p["vertical_scrollbar_hidden"] = parameter.VerticalScrollbarHidden;
            JsonData finalParam = new JsonData();
            finalParam["data"] = p;

            // 调用是否成功的回调
            GMUNCallbackHandler unCallback = new GMUNCallbackHandler
            {
                showWebViewCallback = showCallback
            };
            unCallback.OnSuccess = unCallback.OnWebViewCallback;

            if (exitCallback != null || paramCallback != null)
            {
                if (exitCallback != null)
                {
                    GMUNCallbackHandler unExitCallback = new GMUNCallbackHandler
                    {
                        webViewExitCallback = exitCallback
                    };
                    unExitCallback.OnSuccess = unExitCallback.OnWebViewExitCallback;
                    // 监听WebView退出回调
                    UNBridge.Listen(SDKResultName.SDKWebViewExitResult, unExitCallback);
                }
                if (paramCallback != null)
                {
                    GMUNCallbackHandler customParamCallback = new GMUNCallbackHandler()
                    {
                        customParamWebviewCallback = paramCallback
                    };
                    customParamCallback.OnSuccess = customParamCallback.onCustomParamWebviewCallback;
                    UNBridge.Listen(SDKResultName.SDKWebViewCustomParamEvent, customParamCallback);
                }
                UNBridge.Call(SDKMethodName.SDKShowWebViewWithCallbackController, finalParam, unCallback);
            }
            else
            {
                UNBridge.Call(SDKMethodName.SDKShowWebViewController, finalParam, unCallback);
            }
        }

        /// <summary>
        /// 关闭WebView
        /// </summary>
        public void SdkCloseWebView()
        {
            UNBridge.Call(SDKMethodName.SDKCloseWebView,null);
        }
        /// <summary>
        /// 打开内置webview,回调不为null时在webview关闭时处理回调
        ///
        /// Webview 一级错误码使用：GSDK.ErrorCode.WebviewXXX
        /// 具体内容请参考 Assets/GSDK/Service/Webview/IWebviewService.cs 中的 ErrorCode 部分
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="url">url地址</param>
        /// <param name="source">来源</param>
        /// <param name="orientation">0 autorotate, 1 landscape, 2 portrait</param>
        /// <param name="exitCallback">退出webview返回游戏回调,默认不传，不传时无回调</param>
        /// <param name="showCallback">webview调用是否成功的回调</param>
        /// <param name="paramCallback">JSbridge自定义参数回调</param>
        public void SdkShowWebViewController(string title, string url, string source = "", int orientation = 0,
            Action<CallbackResult> exitCallback = null, Action<CallbackResult> showCallback = null,
            Action<OnWebViewCustomParamResult> paramCallback = null)
        {
            JsonData p = new JsonData();
            p["title"] = title;
            p["url"] = url;
            p["fromSource"] = source;
            p["orientation"] = orientation;
            JsonData finalParam = new JsonData();
            finalParam["data"] = p;

            // 调用是否成功的回调
            GMUNCallbackHandler unCallback = new GMUNCallbackHandler
            {
                showWebViewCallback = showCallback
            };
            unCallback.OnSuccess = unCallback.OnWebViewCallback;

            if (exitCallback != null || paramCallback != null)
            {
                if (exitCallback != null)
                {
                    GMUNCallbackHandler unExitCallback = new GMUNCallbackHandler
                    {
                        webViewExitCallback = exitCallback
                    };
                    unExitCallback.OnSuccess = unExitCallback.OnWebViewExitCallback;
                    // 监听WebView退出回调
                    UNBridge.Listen(SDKResultName.SDKWebViewExitResult, unExitCallback);
                }
                if (paramCallback != null)
                {
                    GMUNCallbackHandler customParamCallback = new GMUNCallbackHandler()
                    {
                        customParamWebviewCallback = paramCallback
                    };
                    customParamCallback.OnSuccess = customParamCallback.onCustomParamWebviewCallback;
                    UNBridge.Listen(SDKResultName.SDKWebViewCustomParamEvent, customParamCallback);
                }
                UNBridge.Call(SDKMethodName.SDKShowWebViewWithCallbackController, finalParam, unCallback);
            }
            else
            {
                UNBridge.Call(SDKMethodName.SDKShowWebViewController, finalParam, unCallback);
            }
        }

        /// <summary>
        /// 获取公告
        /// </summary>
        /// <param name="openId">用户的渠道唯一id</param>
        /// <param name="language">游戏当前语言，必传</param>
        /// <param name="region">用户所在地区，必传</param>
        /// <param name="scene">公告场景，必传</param>
        /// <param name="callback">完成后的回调</param>
        /// <param name="serverId">游戏的服务器区服标记，非必传</param>
        /// <param name="zoneId">区id</param>
        /// <param name="roleId">角色id</param>
        /// <param name="extraInfo">额外扩展字段</param>
		public void SdkGetBulletins(string openId, string language, string region, int scene, Action<BulletinRet> callback, string serverId = null, string zoneId = null, string roleId = null, JsonData extraInfo = null)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                bulletinCallBack = callback
            };
            JsonData p = new JsonData();
            if (openId != null)
            {
                p["openId"] = openId;
            }

            if (serverId != null)
            {
                p["serverId"] = serverId;
            }
            if (zoneId != null)
            {
                p["zoneId"] = zoneId;
            }

            if (language != null)
            {
                p["language"] = language;
            }

            p["scene"] = scene;
            if (region != null)
            {
                p["region"] = region;
            }
            if (extraInfo != null)
            {
                p["extraInfo"] = extraInfo.ToJson();
            }
            if (roleId != null)
            {
                p["roleId"] = roleId;
            }

            JsonData finalParam = new JsonData();
            finalParam["data"] = p;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnRequestBulletinCallBack);
            UNBridge.Call(SDKMethodName.SDKGetBulletins, finalParam, unCallBack);
        }

        /// <summary>
        /// 此接口即将废弃，若要使用功能，转用SdksetDebug接口
        /// </summary>
        /// <param name="debug">开启debug</param>
        public void SdkSetDebugMode(bool debug)
        {
            JsonData param = new JsonData();
            param["debug"] = debug;
            UNBridge.Call(SDKMethodName.SDKSetDebugMode, param);
        }

        //是否是模拟器
        public void SdkDeviceIsEmulator(Action<EmulatorResult> callback)
        {
            GSDKProfilerTools.BeginSample("DeviceInfo CheckEmulator");
            LogUtils.D("Sdk -- Unity -- IsEmulator");
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                isEmulatorCallBack = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnIsEmulatorCallBack);
            UNBridge.Call(SDKMethodName.SdkIsEmulator, null, unCallBack);
            GSDKProfilerTools.EndSample();
        }

        //是否已实名认证
        public void SdkDeviceIsVerifedV2(Action<VerifiedResult> callback)
        {
            LogUtils.D("Sdk -- Unity -- IsVerifedV2");
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                isVerifiedCallBack = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnIsVerifiedCallBack);
            UNBridge.Call(SDKMethodName.SDKIsVerifiedV2, null, unCallBack);
        }

        /*此接口只针对android生效*/
        //退出游戏过程
        public void SdkOnExit(Action<OnExitResult> callback)
        {
            LogUtils.D("Sdk -- Unity -- onExit");
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                onExitCallBack = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnExitCallBack);
            UNBridge.Call(SDKMethodName.SdkOnExit, null, unCallBack);
        }

        /*此接口只针对android生效*/
        public bool SDKIsAvailable(string apiName)
        {
            LogUtils.D("Sdk -- Unity --IsAvaliable");
            JsonData param = new JsonData();
            param["apiName"] = apiName;
            object res = UNBridge.CallSync(SDKMethodName.SDKIsAvaliable, param);
            return res != null ? (bool)res : false;
        }


        /*此接口只针对android生效*/
        public void SDKSetGameInfo(JsonData gameInfo)
        {
            LogUtils.D("Sdk -- Unity --SDKSetGameInfo");
            JsonData param = new JsonData();
            param["gameInfo"] = gameInfo;
            UNBridge.Call(SDKMethodName.SDKSetGameInfo, param);
        }


        #region 支付模块

        /// <summary>
        /// 获取商品信息
        /// </summary>
        /// <param name="sdkOpenId">登陆完成后获得的sdk_open_id</param>
        /// <param name="serverId">游戏服务器的大区id</param>
        /// <param name="roleId">角色id</param>
        /// <param name="callback">回调</param>
        public void SdkRequestProduct(string sdkOpenId, string serverId, string roleId, Action<PayRequestProductResult> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                requestProductCallBack = callback
            };
            JsonData p = new JsonData();
            p["sdkOpenId"] = sdkOpenId;
            p["gameServerId"] = serverId;
            p["roleId"] = roleId;

            JsonData finalParam = new JsonData();
            finalParam["data"] = p;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnRequestProductCallBack);
            UNBridge.Call(SDKMethodName.SDKRequestProduct, finalParam, unCallBack);
        }

        /// <summary>
        /// 获取商品信息，默认会进行商品货币本地化
        /// </summary>
        /// <param name="sdkOpenId">登陆完成后获得的sdk_open_id</param>
        /// <param name="serverId">游戏服务器的大区id</param>
        /// <param name="roleId">角色id</param>
        /// <param name="doCurrencyAdapt">是否商品货币本地化</param>
        /// <param name="callback">回调</param>
        public void SdkRequestProduct(string sdkOpenId, string serverId, string roleId, bool doCurrencyAdapt, Action<PayRequestProductResult> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                requestProductCallBack = callback
            };
            JsonData p = new JsonData();
            p["sdkOpenId"] = sdkOpenId;
            p["gameServerId"] = serverId;
            p["roleId"] = roleId;
            p["doCurrencyAdapt"] = doCurrencyAdapt;
            JsonData finalParam = new JsonData();
            finalParam["data"] = p;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnRequestProductCallBack);
            UNBridge.Call(SDKMethodName.SDKRequestProduct, finalParam, unCallBack);
        }

        /// <summary>
        /// 支付
        /// </summary>
        /// <param name="sdkOpenId">登陆完成后获得的sdk_open_id(必传)</param>
        /// <param name="productId">聚合sdk服务器分配的唯一商品id(必传)</param>
        /// <param name="serverId">游戏服务器的大区id(必传)</param>
        /// <param name="currency">货币类型</param>
        /// <param name="roleId">角色id</param>
        /// <param name="roleName">角色名称</param>
        /// <param name="roleLevel">角色等级</param>
        /// <param name="roleVipLevel">角色vip等级</param>
        /// <param name="extraInfo">透传信息</param>
        /// <param name="activity">活动信息</param>
        /// <param name="callback">回调</param>
        /// <param name="goodsType">商品类型</param>
        public void SdkPay(string sdkOpenId, string productId, string serverId, string currency, string roleId, string roleName,
            string roleLevel, int roleVipLevel, string extraInfo, string activity, int goodsType, Action<PayBuyProductResult> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                buyProductCallBack = callback
            };
            JsonData p = new JsonData();
            p["sdkOpenId"] = sdkOpenId;
            p["productId"] = productId;
            p["serverId"] = serverId;
            p["currency"] = currency;
            p["roleId"] = roleId;
            p["roleName"] = roleName;
            p["roleLevel"] = roleLevel;
            p["roleVipLevel"] = roleVipLevel;
            p["extraInfo"] = extraInfo;
            p["activity"] = activity;
            p["goodsType"] = goodsType;

            JsonData finalParam = new JsonData();
            finalParam["data"] = p;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnBuyProductCallBack);
            UNBridge.Listen(SDKResultName.SDKBuyProductResult, unCallBack);
            UNBridge.Call(SDKMethodName.SDKPay, finalParam);
        }

        // 检测有无历史遗留订单，目前没有用到，SDK内部会自动调用
        public void SdkCheckLeftOrder()
        {
            UNBridge.Call(SDKMethodName.SdkCheckLeftOrder);
        }


#if UNITY_IOS
        /// <summary>
        /// 支付
        /// </summary>
        /// 检测是否存在有 购买过程中因各种原因中断的定单， 如果有，则重新启动定单支付和验证流程。 可以在合适的时机调用。
        /// 前是已调用的时间有： 1.sdk启动时会调用该方法检测一次   2. 每一次购买商品时会触发检测（看之前的购买是否有定单未完成）。
        public void SdkCheckUnverifiedTransactionIfNeeded()
        {
            UNBridge.Call(SDKMethodName.SDKCheckUnverifiedTransactionIfNeeded);
        }

        /// <summary>
        /// 支付
        /// </summary>
        /// 购买商品发生错误后，是否需要启用sdk自带的错误提示.
        /// showErrorTip true:启用sdk自带的错误提示  false:不启用sdk自带的错误提示，由游戏方自己处理。
        public void SdkConfigShowErrorTip(bool showErrorTip)
        {
            JsonData finalParam = new JsonData();
            finalParam["showErrorTip"] = showErrorTip;
            UNBridge.Call(SDKMethodName.SDKConfigShowErrorTip, finalParam);
        }

#endif
        // 账号登陆组件-无UI接口
        // 获取用户登陆信息接口

        public LoginResultFullRet SdkGetLoginResult()
        {
            JsonData param = new JsonData();
#if UNITY_IOS
            object res = UNBridge.CallSync(SDKMethodName.SDKGetLoginResult, param);
#elif UNITY_ANDROID
            object res = UNBridge.CallSync(SDKMethodName.SDKGetLoginResultSync, param);
#else
	        object res = null;
#endif
            LoginResultFullRet ret = null;
            if (res != null)
            {
                ret = SdkUtil.ToObject<LoginResultFullRet>(((JsonData)res).ToJson());
            }

            if (ret == null)
            {
                ret = new LoginResultFullRet();
            }
            return ret;
        }

        //是否是首次登录
        public bool SdkIsFirstLogin()
        {
            object res = UNBridge.CallSync(SDKMethodName.SdkIsFirstLogin, null);
            return res != null ? (bool)res : false;
        }



        #endregion

        // 统计
        public void SdkTrackEvent(string eventName, JsonData eventParams)
        {
            JsonData param = new JsonData();
            param["eventName"] = eventName;
            if (eventParams != null)
            {
                param["eventParams"] = eventParams;
            }
            UNBridge.Call(SDKMethodName.SdkTrackEvent, param);
        }

        public void SdkRegisterDeepLink(string url)
        {
            JsonData param = new JsonData();
            param["url"] = url;
            UNBridge.Call(SDKMethodName.SdkRegisterDeepLink, param);
        }

        /// <summary>
        /// 获取加密的Header
        /// </summary>
        /// <returns>The get secure headers.</returns>
        public string SdkGetSecureHeaders()
        {
            object res = UNBridge.CallSync(SDKMethodName.SdkGetSecureHeader, null);
            return res != null ? res.ToString() : "";
        }

        /// <summary>
        /// 获取缓存中位置信息 - 同步
        /// </summary>
        public GMLocationModel SdkGetLocation()
        {
            string loc = "";
            object res = UNBridge.CallSync(SDKMethodName.SdkGetLocation, null);
            if (res != null)
            {
                loc = res.ToString();
            }
            GMLocationModel locationModel = SdkUtil.ToObject<GMLocationModel>(loc);
            return locationModel;
        }

        /// <summary>
        /// IP定位获取信息 - 异步
        /// </summary>
        /// <param name="callback">定位回调</param>
        public void SdkGetLocationByIp(Action<LocationResult> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                LocationCallBack = callback
            };

            unCallBack.OnSuccess = unCallBack.OnLocationCallBack;
            UNBridge.Call(SDKMethodName.SdkGetLocationByIp, new JsonData(), unCallBack);
        }

        #region AB实验模块
        /// <summary>
        /// 注册实验
        /// </summary>
        /// <param name="key">实验key，必须与libra平台的配置名字保持一致</param>
        /// <param name="description">实验的说明</param>
        /// <param name="owner">实验的负责人</param>
        /// <param name="defaultValue">实验的默认值string、number、array、dictionary的json字符串</param>
        /// <returns>true表示解析成功，false表示defaultValue解析失败</returns>
        public bool SdkRegisterExperiment(string key, string description, string owner, string defaultValue)
        {
            JsonData p = new JsonData();
            p["key"] = key;
            p["description"] = description;
            p["owner"] = owner;
            p["defaultValue"] = defaultValue;

            JsonData finalParam = new JsonData();
            finalParam["data"] = p;
            object res = UNBridge.CallSync(SDKMethodName.SdkRegisterExperiment, finalParam);
            return res != null ? (bool)res : false;
        }

        /// <summary>
        /// 注册实验
        /// </summary>
        /// <param name="key">实验key，必须与libra平台的配置名字保持一致</param>
        /// <param name="description">实验的说明</param>
        /// <param name="owner">实验的负责人</param>
        /// <param name="defaultValue">实验的默认值string、number、array、dictionary的json字符串</param>
        /// <param name="isBindUser"></param>
        /// <returns>true表示解析成功，false表示defaultValue解析失败</returns>
        public bool SdkRegisterExperiment(string key, string description, string owner, string defaultValue, bool isBindUser)
        {
            JsonData p = new JsonData();
            p["key"] = key;
            p["description"] = description;
            p["owner"] = owner;
            p["defaultValue"] = defaultValue;
            p["isBindUser"] = isBindUser;

            JsonData finalParam = new JsonData();
            finalParam["data"] = p;
            var res = UNBridge.CallSync(SDKMethodName.SdkRegisterExperiment, finalParam);
            return res != null ? (bool)res : false;
        }

        /// <summary>
        /// 实验取值
        /// </summary>
        /// <returns>The get experiment value.</returns>
        /// <param name="key">实验key，必须与libra平台的"配置参数"名字保持一致</param>
        /// <param name="withExposure">取值的同时是否触发曝光，仅对iOS设置有效，安卓永远触发曝光</param>
        /// <return>获取到的实验结果json字符串</return>
        public string SdkGetExperimentValue(string key, bool withExposure)
        {
            JsonData param = new JsonData();
            param["key"] = key;
            param["withExposure"] = withExposure;
            object res = UNBridge.CallSync(SDKMethodName.SdkGetExperiment, param);
            return res != null ? res.ToString() : "";
        }
        #endregion

        #region 悬浮窗模块
        /// <summary>
        /// 显示悬浮窗(已废弃)
        /// </summary>
        public void SdkShowFloatingView()
        {
            UNBridge.Call(SDKMethodName.SdkShowFloating);
        }

        /// <summary>
        /// 隐藏悬浮窗（已废弃）
        /// </summary>
        public void SdkHideFloatingView()
        {
            UNBridge.Call(SDKMethodName.SdkHideFloating);
        }
        #endregion

        /// <summary>
        /// 调用native端记录开发日志
        /// </summary>
        public void SdkLog(GMDevLogLevel level, string content)
        {
            JsonData param = new JsonData();
            param["level"] = (int)level;
            param["content"] = content;
            UNBridge.Call(SDKMethodName.SdkGMLog, param);
        }

        #region 异常上报模块
        /// <summary>
        /// 调用native端上报自定义异常
        /// </summary>
        public void SdkLogException(string name, string reason, string stack, JsonData filter = null, JsonData extraParams = null)
        {
            JsonData param = new JsonData();
            param["name"] = name;
            param["reason"] = reason;
            param["stack"] = stack;
            if (filter != null)
            {
                param["filters"] = filter;
            }
            if (extraParams != null)
            {
                param["params"] = extraParams;
            }
            UNBridge.Call(SDKMethodName.SdkGMLogException, param);
        }


        /// <summary>
        /// 仅限iOS端
        /// 注意：这里主要应对开启[Script Call Optimization = Fast but no Exception]的情景下AppDomain.CurrentDomain.UnhandledException中使用，其他情况，请勿使用
        /// </summary>
        public void SdkLogExceptionSyn(string name, string reason, string stack)
        {
            JsonData param = new JsonData();
            param["name"] = name;
            param["reason"] = reason;
            param["stack"] = stack;
            UNBridge.Call(SDKMethodName.SdkGMLogExceptionSyn, param);
        }

        /// <summary>
        /// 用于异常列表的自定义过滤项设置
        /// 请尽量在SDK初始化之前完成设置，否则启动阶段的上报可能无法携带
        /// </summary>
        /// <param name="tagInfo">标签信息.</param>
        public void SdkAddTags(Dictionary<string, string> tagInfo)
        {
            if (tagInfo == null || tagInfo.Count == 0)
            {
                return;
            }
            JsonData p = new JsonData();
            foreach (KeyValuePair<string, string> item in tagInfo)
            {
                p[item.Key] = item.Value;
            }
            JsonData finalParam = new JsonData();
            finalParam["data"] = p;
            UNBridge.Call(SDKMethodName.SdkGMAddTags, finalParam);
        }
        
        /// <summary>
        /// 用于异常列表的自定义过滤项设置，作用和SdkAddTags的相同
        /// 与SdkAddTags的区别在于传入的是json字符串格式，如果传入非json字符串格式，接口无效
        /// json字符串的格式固定，key和value均为string类型，且不支持嵌套，例如"{\"data\":\"test_123\"}"
        /// 请尽量在SDK初始化之前完成设置，否则启动阶段的上报可能无法携带
        /// </summary>
        /// <param name="tagInfo">标签信息.</param>
        public void SdkAddTagsByJson(string tagInfoJsonStr)
        {
            if (string.IsNullOrEmpty(tagInfoJsonStr) || tagInfoJsonStr.Length==0)
            {
                return;
            }
            JsonData finalParam = new JsonData();
            finalParam["data"] = tagInfoJsonStr;
            UNBridge.Call(SDKMethodName.SdkGMAddTagsByJson, finalParam);
        }

        /// <summary>
        /// 用于异常列表的自定义信息展示
        /// 与上面tag的区别在于，这部分信息只能用于展示，不能用于过滤筛选
        /// 请尽量在SDK初始化之前完成设置，否则启动阶段的上报可能无法携带
        /// </summary>
        /// <param name="customInfo">自定义信息</param>
        public void SdkAddCustomInfo(Dictionary<string, string> customInfo)
        {
            if (customInfo == null || customInfo.Count == 0)
            {
                return;
            }
            JsonData p = new JsonData();
            foreach (KeyValuePair<string, string> item in customInfo)
            {
                p[item.Key] = item.Value;
            }
            JsonData finalParam = new JsonData();
            finalParam["data"] = p;
            UNBridge.Call(SDKMethodName.SdkGMAddCustomInfo, finalParam);
        }
        
        /// <summary>
        /// 用于异常列表的自定义信息展示，作用和SdkAddCustomInfo的相同
        /// 与SdkAddCustomInfo的区别在于传入的是json字符串格式，如果传入非json字符串格式，接口无效
        /// json字符串的格式固定，key和value均为string类型，且不支持嵌套，例如"{\"data\":\"test_123\"}"
        /// 请尽量在SDK初始化之前完成设置，否则启动阶段的上报可能无法携带
        /// </summary>
        /// <param name="customInfoJsonStr">自定义信息,json字符串格式</param>
        public void SdkAddCustomInfoByJson(string customInfoJsonStr)
        {
            if (string.IsNullOrEmpty(customInfoJsonStr) || customInfoJsonStr.Length==0)
            {
                return;
            }
            JsonData finalParam = new JsonData();
            finalParam["data"] = customInfoJsonStr;
            UNBridge.Call(SDKMethodName.SdkGMAddCustomInfoByJson, finalParam);
        }
        #endregion

        /// <summary>
        /// Sdk the upload file.
        /// </summary>
        /// <param name="path">文件路径.</param>
        /// <param name="type">文件类型.</param>
        /// <param name="scene">文件上报时的场景，如崩溃或者用户主动反馈.</param>
        /// <param name="extraInfo">通用参数，前端可视化时候展示.</param>
        /// <param name="callback">上报完成的回调.</param>
        public void SdkUploadFile(string path, string type, string scene, JsonData extraInfo, Action<CallbackResult> callback)
        {
            GMUNCallbackHandler unCallback = new GMUNCallbackHandler()
            {
                uploadFileCallBack = callback
            };
            JsonData p = new JsonData();
            p["path"] = path;
            if (type == null)
            {
                type = "";
            }
            p["type"] = type;
            if (scene == null)
            {
                scene = "";
            }
            p["scene"] = scene;
            if (extraInfo == null)
            {
                extraInfo = "";
            }
            p["params"] = extraInfo;

            JsonData finalParam = new JsonData();
            finalParam["data"] = p;
            unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnUploadFileCallBack);
            UNBridge.Call(SDKMethodName.SdkGMUploadFile, finalParam, unCallback);
        }

        /// <summary>
        /// 上报开发者定义的事件，可用于监控
        /// </summary>
        /// <param name="eventName">类型的名称，需要在slardar的【上报配置/事件埋点配置/allow_service_name】下添加采样，注意采样率的设置.</param>
        /// <param name="metric">字典必须是key-value形式，不可嵌套，是数值类型的信息，对应 Slardar 的 metric.</param>
        /// <param name="category">字典必须是key-value形式，不可嵌套，是维度信息，对应 Slardar 的 category.</param>
        /// <param name="extraValue">额外信息，方便追查问题使用，Slardar 平台不会进行展示，hive 中可以查询.</param>
        public void SdkLogEvent(string eventName, JsonData metric, JsonData category, JsonData extraValue)
        {
            JsonData p = new JsonData();
            p["event"] = eventName;
            p["metric"] = metric;
            p["category"] = category;
            p["extraValue"] = extraValue;

            JsonData finalParam = new JsonData();
            finalParam["data"] = p;
            UNBridge.Call(SDKMethodName.SdkGMLogEvent, finalParam);
        }

#if UNITY_IOS
        /// <summary>
        /// 开启系统日志上报功能（只有iOS有）
        /// </summary>
        /// <param name="enable">是否开启</param>
        public void SdkEnableSystemLogUpload(bool enable)
        {

            JsonData p = new JsonData();
            p["enable"] = enable;
            UNBridge.Call(SDKMethodName.SdkGMEnableSystemLogUpload, p);
        }

        /// <summary>
        /// 输出系统NSLog日志（只有iOS有）
        /// </summary>
        /// <param name="contents">日志内容</param>
        public void OutputSystemLogUpload(List<string> contents)
        {
            JsonData p = new JsonData();
            p["contents"] = JsonMapper.ToObject(JsonMapper.ToJson(contents));
            UNBridge.Call(SDKMethodName.SdkGMOutputSystemLog, p);
        }

        /// <summary>
        /// 监听日志埋点上报事件（只有iOS有）
        /// </summary>
        /// <param name="callback">埋点内容</param>
        public void ListenSystemLogEvent(Action<Dictionary<string, string>> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                SystemLogUploadCallback = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnSystemLogUploadCallBack);
            UNBridge.Listen(SDKMethodName.SdkGMSystemLogUploadEvent, unCallBack);
        }

        /// <summary>
        /// 取消监听日志埋点上报事件（只有iOS有）
        /// </summary>
        public void unListenSystemLogEvent()
        {
            UNBridge.UnListen(SDKMethodName.SdkGMSystemLogUploadEvent);
        }
#endif

        #region 区服信息模块

        /// <summary>
        /// 获取区服列表（带超时控制）
        /// </summary>
        /// <param name="accessToken">令牌</param>
        /// <param name="gameVersion">游戏版本号</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="callback">回调</param>
        /// <param name="extraInfo">附加参数，例如区服tag，需用JSON字符串</param>
        public void SdkGetZonesList(string accessToken, string gameVersion, Action<ZonesListResult> callback, double timeout = double.NaN, string extraInfo = "")
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                zoneListCallBack = callback
            };
            JsonData p = new JsonData();
            p["accessToken"] = accessToken;
            p["gameVersion"] = gameVersion;
            if (!double.IsNaN(timeout))
            {
                p["timeout"] = timeout;
            }
            p["extraInfo"] = extraInfo;
            
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnGetZoneListCallBack);
            if (double.IsNaN(timeout))
            {
                if (string.IsNullOrEmpty(extraInfo))
                {
                    UNBridge.Call(SDKMethodName.SdkZoneList, p, unCallBack);
                }
                else
                {
                    UNBridge.Call(SDKMethodName.SdkZoneListWithExtraInfo, p, unCallBack);
                }
                
            }
            else
            {
                if (string.IsNullOrEmpty(extraInfo))
                {
                    UNBridge.Call(SDKMethodName.SdkZoneListWithTimeout, p, unCallBack);
                }
                else
                {
                    UNBridge.Call(SDKMethodName.SdkZoneListWithTimeoutAndExtraInfo, p, unCallBack);
                }
            }
        }

        /// <summary>
        /// 获取角色列表（带超时控制）
        /// </summary>
        /// <param name="accessToken">令牌</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="callback">回调</param>
        public void SdkGetRolesList(string accessToken, Action<RolesListResult> callback, double timeout = double.NaN)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                roleListCallBack = callback
            };
            JsonData p = new JsonData();
            p["accessToken"] = accessToken;
            if (!double.IsNaN(timeout))
            {
                p["timeout"] = timeout;
            }

            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnGetRoleListCallBack);
            if (double.IsNaN(timeout))
            {
                UNBridge.Call(SDKMethodName.SdkRoleList, p, unCallBack);
            }
            else
            {
                UNBridge.Call(SDKMethodName.SdkRoleListWithTimeout, p, unCallBack);
            }
        }

        /// <summary>
        /// 获取区服和角色列表（带超时控制）
        /// </summary>
        /// <param name="accessToken">令牌</param>
        /// <param name="gameVersion">游戏版本号</param>
        /// <param name="timeout">超时时间</param>
        /// <param name="callback">回调</param>
        /// <param name="extraInfo">附加参数，例如区服tag，需用JSON字符串</param>
        public void SdkGetServersAndRolesList(string accessToken, string gameVersion, Action<ServersAndRolesResult> callback, double timeout = double.NaN, string extraInfo = "")
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                serverAndRoleCallBack = callback
            };
            JsonData p = new JsonData();
            p["accessToken"] = accessToken;
            p["gameVersion"] = gameVersion;
            if (!double.IsNaN(timeout))
            {
                p["timeout"] = timeout;
            }
            p["extraInfo"] = extraInfo;
            
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnServersAndRolesListCallBack);
            if (double.IsNaN(timeout))
            {
                if (string.IsNullOrEmpty(extraInfo))
                {
                    UNBridge.Call(SDKMethodName.SdkServerAndRole, p, unCallBack);
                }
                else
                {
                    UNBridge.Call(SDKMethodName.SdkServerAndRoleWithExtraInfo, p, unCallBack);
                }
                
            }
            else
            {
                if (string.IsNullOrEmpty(extraInfo))
                {
                    UNBridge.Call(SDKMethodName.SdkServerAndRoleWithTimeout, p, unCallBack);
                }
                else
                {
                    UNBridge.Call(SDKMethodName.SdkServerAndRoleWithTimeoutAndExtraInfo, p, unCallBack);
                }
                
            }
        }

        /// <summary>
        /// 获取区服列表延迟（带超时控制）
        /// </summary>
        /// <param name="servers">区服列表</param>m'do'e'l
        /// <param name="timeout">超时时间</param>
        /// <param name="callback">回调</param>
        public void SdkPingServerList(List<GMServerModel> servers, Action<PingServersResult> callback, double timeout = double.NaN)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                pingCallBack = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnPingSeversListCallBack);
            UNBridge.Listen(SDKResultName.SdkPingResult, unCallBack);

            JsonData param = new JsonData();
            param["servers"] = JsonMapper.ToJson(servers);
            if (!double.IsNaN(timeout))
            {
                param["timeout"] = timeout;
                UNBridge.Call(SDKMethodName.SdkPingWithTimeout, param);
            }
            else
            {
                UNBridge.Call(SDKMethodName.SdkPing, param);
            }
        }

        #endregion

        /// <summary>
        /// 礼包码兑换
        /// </summary>
        /// <param name="roleId">角色id</param>
        /// <param name="serverId">区id</param>
        /// <param name="cdKey">礼包码</param>
        /// <param name="callback">回调</param>
        public void SdkGetCDKeyInfo(string roleId, string serverId, string cdKey, Action<CallbackResult> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                cdKeyCallBack = callback
            };
            JsonData p = new JsonData();
            p["role_id"] = roleId;
            p["server_id"] = serverId;
            p["cdKey"] = cdKey;

            JsonData finalParam = new JsonData();
            finalParam["data"] = p;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnCDKInfoCallBack);
            UNBridge.Call(SDKMethodName.SdkGetCDKey, finalParam, unCallBack);
        }

        // 第二数据通道
        public string SdkReadPacketText()
        {
            object res = UNBridge.CallSync(SDKMethodName.SDKReadPacket, null);
            return res != null ? res.ToString() : "";
        }

        /// <summary>
        /// 上传数据到服务端
        /// 
        /// Security 一级错误码使用：GSDK.ErrorCode.SecurityXXX
        /// 具体内容请参考 Assets/GSDK/Service/Security/ISecurityService.cs 中的 ErrorCode 部分
        /// </summary>
        /// <param name="data"></param>
        /// <returns>调用结果</returns>
        public CallbackResult SdkWritePacketText(string data)
        {
            JsonData param = new JsonData();
            param["data"] = data;
            object res = UNBridge.CallSync(SDKMethodName.SDKWritePacket, param);
            CallbackResult result = JsonMapper.ToObject<CallbackResult>((String)res);
            return result;
        }

        // 性能消耗监控
        public void SetPriority(int priority)
        {
            JsonData param = new JsonData();
            param["data"] = priority;
            UNBridge.Call(SDKMethodName.SDKSetPriority, param);
        }

        //上报用户信息
        public void SetUserInfo(int userType, string roleId, string serverId)
        {
            JsonData param = new JsonData();
            param["userType"] = userType;
            param["roleId"] = roleId;
            param["serverId"] = serverId;
            UNBridge.Call(SDKMethodName.SDKSetUserInfo, param);
        }

        //开启定时网络探测
        public void StartRecyclePing(string host, string serverType)
        {
            JsonData param = new JsonData();
            param["host"] = host;
            param["serverType"] = serverType;
            UNBridge.Call(SDKMethodName.SDKStartRecyclePing, param);
        }

        //上报游戏热更版本号
        public void SetGMPatchVersion(string version)
        {
            JsonData param = new JsonData();
            param["version"] = version;
            UNBridge.Call(SDKMethodName.SDKSetPatchVersion, param);
        }

        /// <summary>
        /// 获取当前设备网络状态 WIFI、2G、3G、4G、5G和未连接
        /// 返回值 -1:未知 0:未连接 1:WIFI 2:2G 3:3G 4:4G 5:5G
        /// </summary>
        public int SdkFetchNetState()
        {
            GSDKProfilerTools.BeginSample("DeviceInfo NetworkState");
            object res = UNBridge.CallSync(SDKMethodName.SDKFetchNetState, null);
            GSDKProfilerTools.EndSample();
            return res != null ? (int)res : -1;
        }



        /// <summary>
        /// 启动行为埋点
        /// </summary>
        public void SDKLaunchEvent()
        {
            UNBridge.Call(SDKMethodName.SDKLaunchEvent, null);
        }

        /// ------------ 性能监控(Slardar) Start ---------------------

        /// <summary>
        /// 开始进行启动监控
        /// 一次应用启动应该只有一次调用
        /// startTrace() 必须保证 和 endTrace() 在同一线程，且配对出现
        /// </summary>
        public void SDKStartTrace()
        {
            UNBridge.Call(SDKMethodName.SDKStartTrace, null);
        }

        /// <summary>
        /// 必须保证已调用 startTrace()
        /// 同一个module可以在不同的线程
        /// 同一个module 的 taskName需要在同一个线程
        /// startSpan 和 endSpan 配对使用
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <param name="taskName">任务名称</param>
        public void SDKStartSpan(string moduleName, string taskName)
        {
            JsonData final = new JsonData();
            final["moduleName"] = moduleName;
            final["taskName"] = taskName;
            UNBridge.Call(SDKMethodName.SDKStartSpan, final);
        }

        /// <summary>
        /// 同一个span,必须保证和 startSpan 在同一个线程
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <param name="taskName">任务名称</param>
        public void SDKEndSpan(string moduleName, string taskName)
        {
            JsonData final = new JsonData();
            final["moduleName"] = moduleName;
            final["taskName"] = taskName;
            UNBridge.Call(SDKMethodName.SDKEndSpan, final);
        }

        /// <summary>
        /// 标记结束启动监控，一次启动过程只应该调用一次
        /// 和cancelTrace互斥，在cancelTrace之后调用endTrace无效
        /// </summary>
        /// <param name="launchTag">启动监控的Tag，业务可以自定义扩展</param>
        /// <param name="endActivityName">启动结束时的Activity名字</param>
        /// <param name="maxTimeMs"> 最大时长约束阈值(推荐2000ms),超过时长则取消上报</param>
        public void SDKEndTrace(string launchTag, string endActivityName, long maxTimeMs)
        {
            JsonData final = new JsonData();
            final["launchTag"] = launchTag;
            final["endActivityName"] = endActivityName;
            final["maxTimeMs"] = maxTimeMs;
            UNBridge.Call(SDKMethodName.SDKEndTrace, final);
        }

        /// <summary>
        /// 取消启动监控
        /// </summary>
        public void SDKCancelTrace()
        {
            UNBridge.Call(SDKMethodName.SDKCancelTrace, null);
        }

        /// <summary>
        /// 获取线程数量并上报（不主动上报，也会在启动20分钟后开始周期性监控）
        /// </summary>
        /// <param name="scene">场景值</param>
        public void SDKReportTrace(string scene)
        {
            JsonData final = new JsonData();
            final["scene"] = scene;
            UNBridge.Call(SDKMethodName.SDKReportThreadCount, final);
        }

        /// <summary>
        /// 开始采集指定场景的电流值（不主动上报，也会定时采集）
        /// </summary>
        /// <param name="scene">场景值</param>
        public void SDKStartCollectCurrent(string scene)
        {
            JsonData final = new JsonData();
            final["scene"] = scene;
            UNBridge.Call(SDKMethodName.SDKStartCollectCurrent, final);
        }

        /// <summary>
        /// 结束该场景电流值的采集
        /// </summary>
        /// <param name="scene">场景值</param>
        public void SDKStopCollectCurrent(string scene)
        {
            JsonData final = new JsonData();
            final["scene"] = scene;
            UNBridge.Call(SDKMethodName.SDKStopCollectCurrent, final);
        }
        /// ------------ 性能监控(Slardar)  End  ---------------------
        #region 海外账号模块
        #endregion

        #region 设备信息
        /// <summary>
        /// 充电状态监听
        /// </summary>
        /// <param name="callback"></param>
        public void SdkBatteryRegisterReceiver(Action<BatteryInfoCallbackResult> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                batteryCallbackResult = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnBatteryInfoCallBack);
            UNBridge.Listen(SDKResultName.SdkBatteryRegisterResult, unCallBack);

            UNBridge.Call(SDKMethodName.SdkBatteryRegister);
        }
        /// <summary>
        /// 充电状态取消监听
        /// </summary>
        public void SdkBatteryUnregisterReceiver()
        {
            UNBridge.Call(SDKMethodName.SdkBatteryUnregister);
            UNBridge.UnListen(SDKResultName.SdkBatteryRegisterResult);
        }
        /// <summary>
        /// 是否正在充电
        /// </summary>
        /// <returns></returns>
        public bool SdkIsCharging()
        {
            GSDKProfilerTools.BeginSample("DeviceInfo Charging");
            object res = UNBridge.CallSync(SDKMethodName.SdkIsCharging, null);
            GSDKProfilerTools.EndSample();
            return res != null ? (bool)res : false;
        }
        /// <summary>
        /// 获取设备电量
        /// </summary>
        /// <returns></returns>
        public double SdkBatteryLevel()
        {   
            GSDKProfilerTools.BeginSample("DeviceInfo Electricity");
            object res = UNBridge.CallSync(SDKMethodName.SdkBatteryLevel, null);
            GSDKProfilerTools.EndSample();
            return res != null ? Convert.ToDouble(res) : 0.0;
        }

        /// <summary>
        /// 耳机插入状态监听
        /// </summary>
        /// <param name="callback"></param>
        public void SdkHeadsetRegisterReceiver(Action<HeadsetInfoCallbackResult> callback)
        {
            GMUNCallbackHandler unCallBack = new GMUNCallbackHandler()
            {
                headsetCallbackResult = callback
            };
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnHeadsetInfoCallBack);
            UNBridge.Listen(SDKResultName.SdkHeadsetRegisterResult, unCallBack);

            UNBridge.Call(SDKMethodName.SdkHeadsetRegister);
        }

        /// <summary>
        /// 耳机插入状态取消监听
        /// </summary>
        public void SdkHeadsetUnregisterReceiver()
        {
            UNBridge.Call(SDKMethodName.SdkHeadsetUnregister);
            UNBridge.UnListen(SDKResultName.SdkHeadsetRegisterResult);
        }
        /// <summary>
        /// 是否插入耳机
        /// </summary>
        /// <returns></returns>
        public bool SdkIsHeadsetPlugged()
        {
            GSDKProfilerTools.BeginSample("DeviceInfo HeadsetPlugging");
            object res = UNBridge.CallSync(SDKMethodName.SdkIsHeadsetPlugged, null);
            GSDKProfilerTools.EndSample();
            return res != null ? (bool)res : false;
        }
        /// <summary>
        /// 获取屏幕亮度
        /// </summary>
        /// <returns></returns>
        public double SdkScreenBrightness()
        {
            GSDKProfilerTools.BeginSample("DeviceInfo ScreenBrightness");
            object res = UNBridge.CallSync(SDKMethodName.SdkScreenBrightness, null);
            GSDKProfilerTools.EndSample();
            return res != null ? Convert.ToDouble(res) : 0.0;
        }
        /// <summary>
        /// 设置屏幕亮度（0.0 - 1.0）
        /// </summary>
        /// <param name="brightness"></param>
        public void SdkSetScreenBrightness(float brightness)
        {
            GSDKProfilerTools.BeginSample("DeviceInfo ScreenBrightness");
            JsonData data = new JsonData();
            data["brightness"] = brightness;
            UNBridge.Call(SDKMethodName.SdkSetScreenBrightness, data);
            GSDKProfilerTools.EndSample();
        }

        /// <summary>
        /// 设置当前窗口亮度（0.0 - 1.0）
        /// </summary>
        /// <param name="brightness"></param>
        public void SdkSetCurrentWindowBrightness(float brightness)
        {
            JsonData data = new JsonData();
            data["brightness"] = brightness;
            UNBridge.Call(SDKMethodName.SdkSetCurrentWindowBrightness, data);
        }

        /// <summary>
        /// 获取当前窗口亮度
        /// </summary>
        /// <returns></returns>
        public double SdkCurrentWindowBrightness()
        {
            object res = UNBridge.CallSync(SDKMethodName.SdkGetCurrentWindowBrightness, null);
            return res != null ? Convert.ToDouble(res) : 0.0;
        }

        /// <summary>
        /// 判断是否为异形屏
        /// </summary>
        /// <returns>
        /// -1：未知
        /// 0 ：非异形屏
        /// 1 ：异形屏
        /// </returns>
        public int SdkIsScreenAnomalous()
        {
            GSDKProfilerTools.BeginSample("DeviceInfo ScreenType");
            object res = UNBridge.CallSync(SDKMethodName.SdkIsScreenAnomalous, null);
            GSDKProfilerTools.EndSample();
            return res != null ? Convert.ToInt16(res) : 0;
        }

        #endregion

        #region 
        //设置debug模式
        public void SdksetDebug(bool enable)
        {
            JsonData param = new JsonData();
            param["enable"] = enable;
            UNBridge.Call(SDKMethodName.SdksetDebug, param);
        }
        
        //设置RNDebug模式
        public void SdksetRnDebug(bool enable)
        {
            #if UNITY_ANDROID
            JsonData param = new JsonData();
            param["enable"] = enable;
            UNBridge.Call(SDKMethodName.SdksetRNDebugEnable, param);
            #endif
        }
        
        //设置BOE环境,boeHeader默认为prod
        public void SdksetBOEEnable(bool enable, string boeHeader = "prod")
        {
            JsonData param = new JsonData();
            param["enable"] = enable;
            param["boeHeader"] = boeHeader;
            UNBridge.Call(SDKMethodName.SdksetBOEEnable, param);
        }
        //设置沙箱环境
        public void SdksetSandboxEnable(bool enable)
        {
            JsonData param = new JsonData();
            param["enable"] = enable;
            UNBridge.Call(SDKMethodName.SdksetSandboxEnable, param);
        }
        //获得当前debug开关状态
        public bool SdkIsDebugEnable()
        {
            object res = UNBridge.CallSync(SDKMethodName.SdkIsDebugEnable, null);
            return res != null ? (bool)res : false;
        }
        
        //获得当前debug开关状态
        public bool SdkIsRNDebugEnable()
        {
            object res = UNBridge.CallSync(SDKMethodName.SdkIsRNDebugEnable, null);
            return res != null ? (bool)res : false;
        }
        
        //获得当前BOE开关状态
        public bool SdkIsBOEEnable()
        {
            object res = UNBridge.CallSync(SDKMethodName.SdkIsBOEEnable, null);
            return res != null ? (bool)res : false;
        }
        //获得当前沙箱开关状态
        public bool SdkIsSandboxEnable()
        {
            object res = UNBridge.CallSync(SDKMethodName.SdkIsSandboxEnable, null);
            return res != null ? (bool)res : false;
        }
        #endregion


    }
}
