using System;
using System.Collections.Generic;
using UNBridgeLib;
using UNBridgeLib.LitJson;
using UnityEngine;

namespace GMSDK
{
    public class GMUNCallbackHandler : BridgeCallBack
    {
        public Action<CallbackResult> initCallback;
        public Action<CallbackResult> triggerInitCallback;
        public Action<LoginResultRet> loginCallback;
        public Action<ResetGuestResult> resetGuestCallback;
        public Action<BindLoginResultRet> bindLoginCallback;
        public Action<LoginWithPoorNetworkResultRet> loginWithPoorNetworkCallback;
        public Action<CallbackResult> logoutCallback;
        public Action<CallbackResult> logoutAndSetTokenExpiredCallback;
        public Action<BindIndependentResult> bindCallback;
        public Action<RealNameAuthRet> realNameCallback;
        public Action<BulletinRet> bulletinCallBack;
        public Action<CallbackResult> webViewExitCallback;
        public Action<CallbackResult> showWebViewCallback;
        public Action<OnWebViewCustomParamResult> customParamWebviewCallback;
        public Action<EmulatorResult> isEmulatorCallBack;
        public Action<BindIndependentResult> bindPhoneCallback;
        public Action<BindIndependentResult> bindAuthCallback;

        public Action<HasOpenFriendChainPermissionResultRet> hasOpenFriendChainPermissionCallback;
        public Action<FriendChainSDKResultRet> fetchFriendChainCallBack;

        public Action<VerifiedResult> isVerifiedCallBack;
        public Action<CallbackResult> realNameAuthCallBack;

        public Action<OnExitResult> onExitCallBack;

        public Action<PayRequestProductResult> requestProductCallBack;
        public Action<PayBuyProductResult> buyProductCallBack;
        public Action<LocationResult> LocationCallBack;
        public Action<ZonesListResult> zoneListCallBack;
        public Action<RolesListResult> roleListCallBack;
        public Action<ServersAndRolesResult> serverAndRoleCallBack;
        public Action<PingServersResult> pingCallBack;
        public Action<CallbackResult> cdKeyCallBack;
        public Action<CallbackResult> uploadFileCallBack;
        public Action<GMShowPrivacyResult> showPrivacyCallBack;
        public IUserCenter userCenter = null;

        public IChannelExtraCallbackCenter channelExtraCallbackCenter = null;
        public IRequestPermissionCallback permissionRequestCallback = null;
        public Action<RealNameResultRet> checkRealNameCallback;

        public Action<BatteryInfoCallbackResult> batteryCallbackResult;
        public Action<HeadsetInfoCallbackResult> headsetCallbackResult;
        public Action<Dictionary<string,string>> SystemLogUploadCallback;

        public Action<CallbackResult> panelClosedResult;
        
		public Action<ChannelGeneralCallbackResult> channelGeneralCallback;
		public Action<ProtocolAddressResult> protocolAddressCallBack;
		public Action<QRCodeAuthResult> qrcodeAuthCallback;

        public GMUNCallbackHandler()
        {
            this.OnFailed = new OnFailedDelegate(OnFailCallBack);
            this.OnTimeout = new OnTimeoutDelegate(OnTimeoutCallBack);
        }

        /// <summary>
        /// 初始化回调
        /// </summary>
        /// <param name="data"></param>
        public void OnInitCallBack(JsonData jd)
        {
            LogUtils.D("OnInitCallBack: " , jd.ToJson());
            CallbackResult ret = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
            if (ret.IsSuccess())
            {
	            // slardar事件埋点
	            SdkUtil.SdkMonitorEvent("gsdk_unity_sdk_init", null);
	            
	            // 玄武埋点
	            JsonData para = new JsonData();
	            para["platform"] = "unity";
	            para["component_name"] = "unity";
	            para["component_version"] = MainSDK.Version;
	            para["game_version"] = Application.version;
	            SdkUtil.SdkTrackSDKEvent("gsdk_version_upload", para);
            }
            SdkUtil.InvokeAction<CallbackResult>(initCallback, ret);
        }
        
        /// <summary>
        /// 触发Native Bridge初始化回调
        /// 用来凑数的，基本不用加任何东西进去
        /// </summary>
        /// <param name="data"></param>
        public void OnTriggerBridgeInitCallBack(JsonData jd)
        {
	        LogUtils.D("OnTriggerBridgeInitCallBack: " , jd.ToJson());
	        CallbackResult ret = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
	        SdkUtil.InvokeAction<CallbackResult>(triggerInitCallback, ret);
        }

        public void OnPermissionCallBack(JsonData jd)
        {
            LogUtils.D("OnPermissionCallBack: " , jd.ToJson());
            PermissionRequestResult ret = SdkUtil.ToObject<PermissionRequestResult>(jd.ToJson());
            if (permissionRequestCallback != null)
            {
                SdkUtil.InvokeAction<PermissionRequestResult>(permissionRequestCallback.OnPermissionResult, ret);
            }
        }

        public void OnPrivacyCallBack(JsonData jd)
        {
            LogUtils.D("OnPrivacyCallBack: " , jd.ToJson());
            PrivacyResult ret = SdkUtil.ToObject<PrivacyResult>(jd.ToJson());

            if (permissionRequestCallback != null)
            {
                SdkUtil.InvokeAction<PrivacyResult>(permissionRequestCallback.onPrivacyResult, ret);
            }
        }

        /// <summary>
        /// 登录回调
        /// </summary>
        /// <param name="data"></param>
        public void OnLoginCallBack(JsonData jd)
        {
            LogUtils.D("OnLoginCallBack:" , jd.ToJson());
            LoginResultRet ret = SdkUtil.ToObject<LoginResultRet>(jd.ToJson());
            SdkUtil.InvokeAction<LoginResultRet>(loginCallback, ret);
        }

        /// <summary>
        /// 面板关闭回调
        /// </summary>
        /// <param name="jd"></param>
        public void OnPanelClosedCallBack(JsonData jd)
        {
	        String json = jd.ToJson();
	        LogUtils.D("OnPanelClosedCallBack:" , json);
	        CallbackResult ret = SdkUtil.ToObject<CallbackResult>(json);
	        SdkUtil.InvokeAction<CallbackResult>(panelClosedResult, ret);
        }
        
        		

        public void OnAuthLoginInterfaceCallBack(JsonData jd)
        {
	        OnLoginInterfaceCallBack (jd);
	        UNBridge.UnListen (SDKResultName.SDKAuthLoginResult);
        }
        

        /// <summary>
        /// 登出回调
        /// </summary>
        /// <param name="data"></param>
        public void OnLogoutCallBack(JsonData jd)
        {
            LogUtils.D("OnLogoutCallBack:" , jd.ToJson());
            CallbackResult ret = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
            SdkUtil.InvokeAction<CallbackResult>(logoutCallback, ret);
            if (userCenter != null)
            {
                SdkUtil.InvokeAction<CallbackResult>(userCenter.logoutCallback, ret);
            }
            UNBridge.UnListen(SDKResultName.SDKLogoutResult);
        }
        
        /// <summary>
        /// 登出回调,token过期
        /// </summary>
        /// <param name="data"></param>
        public void OnLogoutAndSetTokenExpiredCallBack(JsonData jd)
        {
	        LogUtils.D("OnLogoutAndSetTokenExpiredCallBack:" , jd.ToJson());
	        CallbackResult ret = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
	        SdkUtil.InvokeAction<CallbackResult>(logoutAndSetTokenExpiredCallback, ret);
        }
        
        
        /// <summary>
        /// 重置遊客信息回調
        /// </summary>
        /// <param name="data"></param>
        public void OnResetGuestCallback(JsonData jd)
        {
	        LogUtils.D("OnResetGuestCallback:" , jd.ToJson());
	        ResetGuestResult ret = SdkUtil.ToObject<ResetGuestResult>(jd.ToJson());
	        SdkUtil.InvokeAction<ResetGuestResult>(resetGuestCallback, ret);
        }
        
        /// <summary>
        /// 有UI实名认证回调
        /// </summary>
        /// <param name="data"></param>
        public void OnRealNameAuthCallBack(JsonData jd)
        {
            LogUtils.D("OnRealNameAuthCallBack:" , jd.ToJson());
            RealNameAuthRet ret = SdkUtil.ToObject<RealNameAuthRet>(jd.ToJson());
            if (userCenter != null)
            {
                SdkUtil.InvokeAction<RealNameAuthRet>(userCenter.realNameCallback, ret);
            }
            SdkUtil.InvokeAction<RealNameAuthRet>(realNameCallback, ret);
        }
	    
        public void OnUserCenterClosedCallBack(JsonData jd)
        {
	        String json = jd.ToJson();
	        LogUtils.D("OnUserCenterClosedCallBack:" , json);
	        CallbackResult ret = SdkUtil.ToObject<CallbackResult>(json);
	        if (userCenter != null)
	        {
		        SdkUtil.InvokeAction(userCenter.closeCallback, ret);
	        }
        }

        /// <summary>
        /// 查询实名认证结果回调
        /// </summary>
        /// <param name="data"></param>
        public void OnCheckRealNameResultCallBack(JsonData jd)
        {
            LogUtils.D("OnCheckRealNameResultCallBack:" , jd.ToJson());
            RealNameResultRet ret = SdkUtil.ToObject<RealNameResultRet>(jd.ToJson());
            SdkUtil.InvokeAction<RealNameResultRet>(checkRealNameCallback, ret);
            UNBridge.UnListen(SDKResultName.SDKCheckRealNameResult);
        }


        /// <summary>
        /// 渠道登出回调
        /// </summary>
        /// <param name="data"></param>
        public void OnChannelLogoutCallBack(JsonData jd)
        {
            LogUtils.D("OnChannelLogoutCallBack:" , jd.ToJson());
            CallbackResult ret = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
            if (channelExtraCallbackCenter != null)
            {
                SdkUtil.InvokeAction<CallbackResult>(channelExtraCallbackCenter.logoutChannelCallback, ret); //调用mainScene实现的callback
            }
            // UNBridge.UnListen(SDKResultName.SDKChannelLogoutResult); 这里应该不用解除监听，而是要持续监听
        }


        /// <summary>
        /// 渠道切换账号回调
        /// </summary>
        /// <param name="data"></param>
        public void OnChannelSwicthAccountCallBack(JsonData jd)
        {
            LogUtils.D("OnChannelSwitchAccountCallBack:" , jd.ToJson());
            LoginResultRet ret = SdkUtil.ToObject<LoginResultRet>(jd.ToJson());
            if (channelExtraCallbackCenter != null)
            {
                SdkUtil.InvokeAction<LoginResultRet>(channelExtraCallbackCenter.switchAccountChannelCallback, ret); //调用mainScene实现的callback
            }
        }

        /// <summary>
        /// 退出游戏回调
        /// </summary>
        /// <param name="data"></param>
        public void OnChannelExitCallBack(JsonData jd)
        {
            LogUtils.D("OnChannelBindSuccessCallBack:" , jd.ToJson());
            ExitResult ret = SdkUtil.ToObject<ExitResult>(jd.ToJson());
            if (channelExtraCallbackCenter != null)
            {
                SdkUtil.InvokeAction<ExitResult>(channelExtraCallbackCenter.onExitChannelCallback, ret); //调用mainScene实现的callback
            }
        }



        /// <summary>
        /// 获取是否是模拟器
        /// </summary>
        /// <param name="data"></param>
        public void OnIsEmulatorCallBack(JsonData jd)
        {
            LogUtils.D("OnIsEmulatorCallBack --- ");
            EmulatorResult result = SdkUtil.ToObject<EmulatorResult>(jd.ToJson());
            SdkUtil.InvokeAction<EmulatorResult>(isEmulatorCallBack, result);
        }

        /// <summary>
        /// 获取是否实名
        /// </summary>
        /// <param name="data"></param>
        public void OnIsVerifiedCallBack(JsonData jd)
        {
            LogUtils.D("OnIsVerifiedCallBack --- ");
            VerifiedResult result = SdkUtil.ToObject<VerifiedResult>(jd.ToJson());
            SdkUtil.InvokeAction<VerifiedResult>(isVerifiedCallBack, result);
        }
	    
        public void OnRealNameAuthCallback(JsonData jd)
        {
	        LogUtils.D("OnRealNameAuthCallback --- ");
	        CallbackResult result = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
	        SdkUtil.InvokeAction(realNameAuthCallBack, result);
        }

        /// <summary>
        /// 多渠道注册通用回调
        /// </summary>
        /// <param name="data"></param>
        public void OnChannelGeneralCallback(JsonData jd)
        {
			LogUtils.D("OnChannelGeneralCallback --- ");
			ChannelGeneralCallbackResult result = SdkUtil.ToObject<ChannelGeneralCallbackResult>(jd.ToJson());
			SdkUtil.InvokeAction<ChannelGeneralCallbackResult> (channelGeneralCallback, result);
			UNBridge.UnListen (SDKResultName.ChannelGeneralCallbackResult);
        }
        /// <summary>
        /// 退出游戏
        /// </summary>
        /// <param name="data"></param>
        public void OnExitCallBack(JsonData jd)
        {
            LogUtils.D("OnExitCallBack --- ");
            OnExitResult result = SdkUtil.ToObject<OnExitResult>(jd.ToJson());
            SdkUtil.InvokeAction<OnExitResult>(onExitCallBack, result);
        }

        /// <summary>
        /// 用户中心账号绑定回调
        /// </summary>
        /// <param name="data"></param>
        public void OnBindInUserCenterCallBack(JsonData jd)
        {
            LogUtils.D("OnBindInUserCenterCallBack:" , jd.ToJson());
            BindIndependentResult ret = SdkUtil.ToObject<BindIndependentResult>(jd.ToJson());
            if (userCenter != null)
            {
                SdkUtil.InvokeAction<BindIndependentResult>(userCenter.bindCallback, ret);
            }
        }

        /// <summary>
        /// 获取公告回调
        /// </summary>
        /// <param name="data"></param>
        public void OnRequestBulletinCallBack(JsonData jd)
        {
            LogUtils.D("OnRequestBulletinCallBack --- " + jd);
            BulletinRet ret = SdkUtil.ToObject<BulletinRet>(jd.ToJson());
            SdkUtil.InvokeAction<BulletinRet>(bulletinCallBack, ret);
        }

        /// <summary>
        /// 获取购买商品回调
        /// </summary>
        /// <param name="data"></param>
        public void OnRequestProductCallBack(JsonData jd)
        {
            LogUtils.D("OnRequestProductCallBack --- ");
            PayRequestProductResult result = SdkUtil.ToObject<PayRequestProductResult>(jd.ToJson());
            SdkUtil.InvokeAction<PayRequestProductResult>(requestProductCallBack, result);
        }

        /// <summary>
        /// 购买商品回调
        /// </summary>
        /// <param name="data"></param>
        public void OnBuyProductCallBack(JsonData jd)
        {
            LogUtils.D("OnBuyProductCallBack --- ");
            PayBuyProductResult result = SdkUtil.ToObject<PayBuyProductResult>(jd.ToJson());
            SdkUtil.InvokeAction<PayBuyProductResult>(buyProductCallBack, result);
        }

        /// <summary>
        /// 定位信息回调
        /// </summary>
        /// <param name="jd"></param>
        public void OnLocationCallBack(JsonData jd)
        {
            LogUtils.D("OnLocationCallBack --- ");
            GMLocationModel location = SdkUtil.ToObject<GMLocationModel>(jd.ToJson());
            LocationResult result = new LocationResult(location);
            SdkUtil.InvokeAction(LocationCallBack, result);
        }

        public void OnGetZoneListCallBack(JsonData jd)
        {
            LogUtils.D("OnGetZoneListCallBack");
            ZonesListResult ret = SdkUtil.ToObject<ZonesListResult>(jd.ToJson());
            SdkUtil.InvokeAction<ZonesListResult>(zoneListCallBack, ret);
        }

        public void OnGetRoleListCallBack(JsonData jd)
        {
            LogUtils.D("OnGetRoleListCallBack");
            RolesListResult ret = SdkUtil.ToObject<RolesListResult>(jd.ToJson());
            SdkUtil.InvokeAction<RolesListResult>(roleListCallBack, ret);
        }

        public void OnServersAndRolesListCallBack(JsonData jd)
        {
            LogUtils.D("OnServersAndRolesListCallBack");
            ServersAndRolesResult ret = SdkUtil.ToObject<ServersAndRolesResult>(jd.ToJson());
            SdkUtil.InvokeAction<ServersAndRolesResult>(serverAndRoleCallBack, ret);
        }

        public void OnPingSeversListCallBack(JsonData jd)
        {
            LogUtils.D("OnPingSeversListCallBack");
            PingServersResult ret = SdkUtil.ToObject<PingServersResult>(jd.ToJson());
            SdkUtil.InvokeAction<PingServersResult>(pingCallBack, ret);
        }

        public void OnCDKInfoCallBack(JsonData jd)
        {
            CallbackResult ret = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
            SdkUtil.InvokeAction<CallbackResult>(cdKeyCallBack, ret);
        }

        public void OnUploadFileCallBack(JsonData jd)
        {
            CallbackResult ret = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
            SdkUtil.InvokeAction<CallbackResult>(uploadFileCallBack, ret);
        }

        public void OnSystemLogUploadCallBack(JsonData jd)
        {
	        LogUtils.D("OnPingSeversListCallBack");
	        Dictionary<string,string> ret = SdkUtil.ToObject<Dictionary<string,string>>(jd.ToJson());
	        SdkUtil.InvokeAction<Dictionary<string,string>>(SystemLogUploadCallback, ret);
        }

        public void OnShowPrivacyCallBack(JsonData jd)
        {
            GMShowPrivacyResult ret = SdkUtil.ToObject<GMShowPrivacyResult>(jd.ToJson());
            SdkUtil.InvokeAction<GMShowPrivacyResult>(showPrivacyCallBack, ret);
        }
        /// <summary>
        /// 退出WebView回调
        /// </summary>
        public void OnWebViewExitCallback(JsonData jd)
        {
            CallbackResult ret = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
            SdkUtil.InvokeAction<CallbackResult>(webViewExitCallback, ret);
        }
        
        /// <summary>
        /// 调用结果回调
        /// </summary>
        public void OnWebViewCallback(JsonData jd)
        {
	        CallbackResult ret = SdkUtil.ToObject<CallbackResult>(jd.ToJson());
	        if (showWebViewCallback == null)
	        {
		        showWebViewCallback = result =>
		        {

		        };
	        }
	        SdkUtil.InvokeAction<CallbackResult>(showWebViewCallback, ret);
        }
        
        /// <summary>
        /// JSbridge自定义参数回调
        /// </summary>
        /// <param name="jd"></param>
        public void onCustomParamWebviewCallback(JsonData jd)
        {
	        OnWebViewCustomParamResult ret = SdkUtil.ToObject<OnWebViewCustomParamResult>(jd.ToJson());
	        SdkUtil.InvokeAction<OnWebViewCustomParamResult>(customParamWebviewCallback,ret);
        }


        /// <summary>
        /// 登录回调
        /// </summary>
        /// <param name="data"></param>
        public void OnLoginInterfaceCallBack(JsonData jd)
        {
            LogUtils.D("OnLoginCallBack:" , jd.ToJson());
            LoginResultRet ret = SdkUtil.ToObject<LoginResultRet>(jd.ToJson());
            SdkUtil.InvokeAction<LoginResultRet>(loginCallback, ret);
        }

        /// <summary>
        /// 绑定回调
        /// </summary>
        /// <param name="data"></param>
        /// 
        public void OnBindInterfaceCallBack(JsonData jd)
        {
            LogUtils.D("OnLoginCallBack:" , jd.ToJson());
            BindLoginResultRet ret = SdkUtil.ToObject<BindLoginResultRet>(jd.ToJson());
            SdkUtil.InvokeAction<BindLoginResultRet>(bindLoginCallback, ret);
        }

        /// <summary>
        /// 登录回调
        /// </summary>
        /// <param name="data"></param>
        public void OnLoginWithpoorNetworkInterfaceCallBack(JsonData jd)
        {
            LogUtils.D("OnLoginpoorNetworkCallBack:" + jd.ToJson());
            LoginWithPoorNetworkResultRet ret = SdkUtil.ToObject<LoginWithPoorNetworkResultRet>(jd.ToJson());
            SdkUtil.InvokeAction<LoginWithPoorNetworkResultRet>(loginWithPoorNetworkCallback, ret);
        }

        /// <summary>
        /// 独立绑定手机号回调
        /// </summary>
        /// <param name="data"></param>
        public void OnIndependentBindPhoneCallBack(JsonData jd)
        {
            LogUtils.D("OnIndependentBindPhoneCallBack:" , jd.ToJson());
            BindIndependentResult ret = SdkUtil.ToObject<BindIndependentResult>(jd.ToJson());
            SdkUtil.InvokeAction<BindIndependentResult>(bindPhoneCallback, ret);
        }

        /// <summary>
        /// 独立打开账号管理面板绑定解绑回调
        /// </summary>
        /// <param name="data"></param>
        public void OnIndependentAccountManagementCallBack(JsonData jd)
        {
            LogUtils.D("OnIndependentAccountManagementCallBack:" , jd.ToJson());
            BindIndependentResult ret = SdkUtil.ToObject<BindIndependentResult>(jd.ToJson());
            SdkUtil.InvokeAction<BindIndependentResult>(bindAuthCallback, ret);
        }

        /// <summary>
        /// 是否开启平台好友关系链权限，限头条（GMAuthTypeTT）、抖音（GMAuthTypeAwe）、Facebook（GMAuthTypeFacebook）回调
        /// </summary>
        /// <param name="data"></param>
        public void OnHasOpenFriendChainPermissionCallback(JsonData jd)
        {
            LogUtils.D("OnHasOpenFriendChainPermissionCallback:" + jd.ToJson());
            HasOpenFriendChainPermissionResultRet ret = SdkUtil.ToObject<HasOpenFriendChainPermissionResultRet>(jd.ToJson());
            SdkUtil.InvokeAction<HasOpenFriendChainPermissionResultRet>(hasOpenFriendChainPermissionCallback, ret);
        }

        /// <summary>
        /// sdk封装查询平台好友关系链，限头条（GMAuthTypeTT）、抖音（GMAuthTypeAwe）、Facebook（GMAuthTypeFacebook）回调
        /// </summary>
        /// <param name="data"></param>
        public void OnFriendChainInfoCallback(JsonData jd)
        {
            LogUtils.D("OnFriendChainInfoCallback:" , jd.ToJson());
            FriendChainSDKResultRet ret = SdkUtil.ToObject<FriendChainSDKResultRet>(jd.ToJson());
            SdkUtil.InvokeAction<FriendChainSDKResultRet>(fetchFriendChainCallBack, ret);
        }

        /// <summary>
        /// 获取协议地址
        /// </summary>
        /// <param name="data"></param>
        public void OnProtocolAddressCallBack(JsonData jd)
        {
			LogUtils.D("OnProtocolAddressCallBack --- ");
			ProtocolAddressResult result = SdkUtil.ToObject<ProtocolAddressResult>(jd.ToJson());
			SdkUtil.InvokeAction<ProtocolAddressResult> (protocolAddressCallBack, result);
        }
       
	    // 扫码授权PC登录回调
        public void OnQRCodeAuthCallBack(JsonData jd)
        {
	        LogUtils.D("OnQRCodeAuthCallBack:" , jd.ToJson());
	        QRCodeAuthResult ret = SdkUtil.ToObject<QRCodeAuthResult>(jd.ToJson());
	        SdkUtil.InvokeAction<QRCodeAuthResult>(qrcodeAuthCallback, ret);
        }




        /// <summary>
        /// 是否在充电回调
        /// </summary>
        /// <param name="data"></param>
        public void OnBatteryInfoCallBack(JsonData jd)
        {
            LogUtils.D("OnBatteryInfoCallBack:" , jd.ToJson());
            BatteryInfoCallbackResult ret = SdkUtil.ToObject<BatteryInfoCallbackResult>(jd.ToJson());
            SdkUtil.InvokeAction<BatteryInfoCallbackResult>(batteryCallbackResult, ret);
        }

        /// <summary>
        /// 是否插入耳机
        /// </summary>
        /// <param name="data"></param>
        public void OnHeadsetInfoCallBack(JsonData jd)
        {
            LogUtils.D("OnHeadsetInfoCallBack:" , jd.ToJson());
            HeadsetInfoCallbackResult ret = SdkUtil.ToObject<HeadsetInfoCallbackResult>(jd.ToJson());
            SdkUtil.InvokeAction<HeadsetInfoCallbackResult>(headsetCallbackResult, ret);
        }

        /// <summary>
        /// 失败统一回调，用于调试接口
        /// </summary>
        public void OnFailCallBack(int code, string failMsg)
        {
            LogUtils.E("接口访问失败 " + code.ToString() + " " + failMsg);
        }
        /// <summary>
        /// 超时统一回调
        /// </summary>
        public void OnTimeoutCallBack()
        {
            JsonData jd = new JsonData();
            jd["code"] = -321;
            jd["message"] = "UN - request time out";
            if (this.OnSuccess != null)
            {
                this.OnSuccess(jd);
            }
        }
    }
}

