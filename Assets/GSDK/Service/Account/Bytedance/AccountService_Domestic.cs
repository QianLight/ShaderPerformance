using System;
using System.Collections.Generic;
using GMSDK;
using GSDK;
using UNBridgeLib.LitJson;

namespace GSDK
{
    /// <summary>
    /// 扫描二维码授权PC端登录回调
    /// </summary>
    /// <param name="result">包含错误码的结果</param>
    /// <para>
    ///		SUCCESS: 成功
    ///		AccountInvalidQRCode: 无效二维码。二维码中包含的字符串中无token，或token校验失败
    ///     AccountAuthFailed: 二维码授权失败。二维码过期或已被其他设备授权
    ///     AccountGustNotAuth: 游客禁止授权。不支持游客授权登录能力
    /// </para>
    /// <param name="status">用户授权状态</param>
    /// <param>
    ///		status == 0 : 用户同意授权
    ///		status == 1 ：用户拒绝授权
    /// </param>
    public delegate void QRCodeAuthLoginDelegate(Result result, int status);

    public partial class AccountBytedanceService
    {
        private string _douyinUserTicket = "";
        private string _douyinOpenID = "";

        // （无UI）登录
        public void LoginWithoutUI(AccountType accountType)
        {
            GSDKProfilerTools.BeginSample("Account LoginWithoutUI " + accountType);
            switch (accountType)
            {
                case AccountType.Guest:
                    GuestLogin();
                    break;
                case AccountType.CloudGame:
                    // 云游戏登录，仅在安卓上有效，iOS上的实现为空
                    CloudGameLogin();
                    break;
                case AccountType.CloudGameV2:
                    // 云游戏登录V2，仅在安卓上有效，iOS上的实现为空
                    CloudGameLoginV2();
                    break;
                case AccountType.Auto:
                    GLog.LogError("抱歉，国内暂不支持自动登录");
                    break;
                default:
                    ThirdPartyAuthLogin(accountType);
                    break;
            }
            GSDKProfilerTools.EndSample();
        }

        // （无UI）游客登录
        private void GuestLogin()
        {
            GLog.LogInfo("GuestLogin");
            gsdk.SdkGuestLoginDomesticInterface(ProcessLoginResult);
        }

        // （无UI）第三方账号授权登录
        private void ThirdPartyAuthLogin(AccountType type)
        {
            GLog.LogInfo("ThirdPartyAuthLogin: " + type);
            GMSDK.GMUserType oldType = AccountInnerTools.Convert(type);
            gsdk.SdkAuthLoginInterface(oldType, ProcessLoginResult);
        }


        public void ShowUserCenter(AccountShowUserCenterDelegate showUserCenterDelegate,
            AccountPanelClosedDelegate panelClosedCallback)
        {
            GSDKProfilerTools.BeginSample("Account ShowUserCenter");
            GLog.LogInfo("ShowUserCenter");
            // 打开用户中心， -109800 表示用户未登录，0表示成功
            int retCode = gsdk.SdkShowUserCenter(new UserCenter()
            {
                LogoutAction = OnLogoutCallback,
                RealNameAction = OnRealNameAuthCallback,
                BindIndependentAction = OnShowIndependentBindPhoneCallback,
                PanelClosedAction = () =>
                {
                    if (panelClosedCallback != null)
                    {
                        panelClosedCallback();
                    }
                }
            });
            if (retCode == ErrorCode.AccountNotLoggedIn)
            {
                GLog.LogError("ShowUserCenter Failed");
                OnShowUserCenterCallback(new Result(ErrorCode.AccountNotLoggedIn, "user not logged in"),
                    showUserCenterDelegate);
            }
            else
            {
                GLog.LogInfo("ShowUserCenter Success");
                OnShowUserCenterCallback(new Result(ErrorCode.Success, "Show user center success"),
                    showUserCenterDelegate);
            }
            GSDKProfilerTools.EndSample();
        }

        void OnShowUserCenterCallback(Result result, AccountShowUserCenterDelegate showUserCenterDelegate)
        {
            if (showUserCenterDelegate != null)
            {
                try
                {
                    showUserCenterDelegate(result);
                }
                catch (Exception ex)
                {
                    GLog.LogException(ex);
                }
            }
        }

#if UNITY_STANDALONE_WIN && !GMEnderOn
        [MonoPInvokeCallback(typeof(Callback2P))]
        private static void AuthRealNameWithUICallback(string resultStr, string callbackStr)
        {
            GLog.LogDebug("resultStr:" + resultStr + ",callbackStr:" + callbackStr);
            Result result = JsonMapper.ToObject<Result>(resultStr);
            RealNameAuthResult realNameAuthResult = JsonMapper.ToObject<RealNameAuthResult>(callbackStr);
            AccountBytedanceService accountService = Account.Service as AccountBytedanceService;
            if (accountService.RealNameAuthEvent != null)
            {
                accountService.RealNameAuthEvent(result, realNameAuthResult);
            }
        }
#endif


        public void AuthRealNameWithUI(RealNameAuthType authType)
        {
            GLog.LogInfo("AuthRealNameWithUI");          
#if UNITY_STANDALONE_WIN && !GMEnderOn
            ComplianceAuthRealNameWithUI((int)authType, AuthRealNameWithUICallback);  
#else
            int type = (int) authType;
            Action<GMSDK.RealNameAuthRet> callback = (GMSDK.RealNameAuthRet ret) =>
            {
                //todo 需要修改错误码
                if (ret.code == -1)
                {
                    OnRealNameAuthCallback(InnerTools.ConvertToResult(ret), null);
                    //OnRealNameAuthCallback(new Result(ErrorCode.AccountUserCancelVerification, -1, 0, "User cancel verification"), null);
                }
                else if (ret.code == -2)
                {
                    OnRealNameAuthCallback(InnerTools.ConvertToResult(ret), null);
                    //OnRealNameAuthCallback(new Result(ErrorCode.AccountAuthLowLevel, -1, 0, "Low auth level, no need to verify"), null);
                }
                else
                {
                    OnRealNameAuthCallback(InnerTools.ConvertToResult(ret),
                        AccountInnerTools.Convert(ret));
                }
            };
            // iOS如果已实名会返回一个错误
            // Android如果已实名还是会弹出实名窗口，等到按了实名按钮之后再提示已实名
            // 为了避免双端差异，Android需要先调用是否实名接口进行检测，再调用实名接口
#if UNITY_IOS
            gsdk.SdkRealNameAuth(type, callback);
#elif UNITY_ANDROID
            gsdk.SdkDeviceIsVerifedV2((GMSDK.VerifiedResult verifiedResult) =>
            {
                if (verifiedResult.isVerified)
                {
                    OnRealNameAuthCallback(
                        new Result(ErrorCode.AccountRealNameHasVerifiedError, -1202, 0, "User is already verified!"),
                        null);
                }
                else
                {
                    gsdk.SdkRealNameAuth(type, callback);
                }
            });
#endif
#endif
        }

        void OnRealNameAuthCallback(Result result, RealNameAuthResult realNameAuthResult)
        {
            if (RealNameAuthEvent != null)
            {
                try
                {
                    RealNameAuthEvent(result, realNameAuthResult);
                }
                catch (Exception ex)
                {
                    GLog.LogException(ex);
                }
            }
        }

        public void ShowPhoneBindingView(AccountPanelClosedDelegate panelClosedCallback)
        {
            GSDKProfilerTools.BeginSample("Account ShowPhoneBindingView");
            int retCode = gsdk.SdkShowIndependentBindPhone(
                (GMSDK.BindIndependentResult ret) =>
                {
                    OnShowIndependentBindPhoneCallback(InnerTools.ConvertToResult(ret), ret);
                }, result =>
                {
                    InnerTools.SafeInvoke(() =>
                    {
                        if (panelClosedCallback != null)
                        {
                            panelClosedCallback();
                        }
                    });
                });
            if (retCode == ErrorCode.AccountNotLoggedIn)
            {
                OnShowIndependentBindPhoneCallback(new Result(ErrorCode.AccountNotLoggedIn, "User didn't log in"),
                    null);
            }
            else if(retCode == ErrorCode.AccountOnlyVisitorCanBeBound)
            {
                OnShowIndependentBindPhoneCallback(new Result(ErrorCode.AccountOnlyVisitorCanBeBound, "only visitor can be bound"),
                    null);
            }
            else
            {
                OnShowIndependentBindPhoneCallback(new Result(ErrorCode.AccountGClientError, "client internal error"),
                    null);
            }
            GSDKProfilerTools.EndSample();
        }

        void OnShowIndependentBindPhoneCallback(Result result, GMSDK.BindIndependentResult bindIndependentResult)
        {
            if (BindAccountEvent != null)
            {
                AccountType bindAccountType = AccountType.None;
                BindOrUnbindOperation operation = BindOrUnbindOperation.Bind;
                AccountInnerTools.ConvertBindAccountType(bindIndependentResult.type, ref bindAccountType,
                    ref operation);
                try
                {
                    BindAccountEvent(result, bindAccountType);
                }
                catch (Exception ex)
                {
                    GLog.LogException(ex);
                }
            }
        }

        public bool HasBinded(AccountType type)
        {
            bool ret = false;
            switch (type)
            {
                case AccountType.Phone:
                    ret = gsdk.SdkCheckHasBindPhone();
                    break;
                case AccountType.Awe:
                    ret = gsdk.SdkCheckHasBindDouYin();
                    break;
                case AccountType.TouTiao:
                    ret = gsdk.SdkCheckHasBindTouTiao();
                    break;
                case AccountType.Apple:
                    ret = gsdk.SdkCheckHasBindApple();
                    break;
                case AccountType.HuoShan:
                    ret = gsdk.SdkCheckHasBindHuoShan();
                    break;
                case AccountType.XiGua:
                    ret = gsdk.SdkCheckHasBindXiGua();
                    break;
                case AccountType.TapTap:
                    ret = gsdk.SdkCheckHasBindTapTap();
                    break;
                default:
                    break;
            }

            return ret;
        }

        public void ShowLicense(AccountPanelClosedDelegate panelClosedCallback)
        {
            gsdk.SdkShowLicense(result =>
            {
                InnerTools.SafeInvoke(() =>
                {
                    if (panelClosedCallback != null)
                    {
                        panelClosedCallback();
                    }
                });
            });
        }

        public void CloudGameLogin()
        {
#if UNITY_ANDROID
            GLog.LogInfo("CloudGameLogin");
            gsdk.SdkCloudGameLogin(ProcessLoginResult);
#endif
        }
        
        public void CloudGameLoginV2()
        {
#if UNITY_ANDROID
            GLog.LogInfo("CloudGameLoginV2");
            gsdk.SdkCloudGameLoginV2(ProcessLoginResult);
#endif
        }

        public void QRCodeAuthLogin(string token, QRCodeAuthLoginDelegate qrCodeAuthLoginCallback)
        {
            gsdk.SdkScanQRCodeAuthLogin(token,
                (result => { qrCodeAuthLoginCallback(InnerTools.ConvertToResult(result), result.status); }));
        }

        public void LogoutAndSetTokenExpired()
        {
            GLog.LogInfo("logoutAndSetTokenExpired");
            gsdk.SdkLogoutAndSetTokenExpired((GMSDK.CallbackResult ret) =>
            {
                GLog.LogDebug("LogoutAndSetTokenExpiredResult: " + ret);
                if (ret.IsSuccess())
                {
                    GLog.LogInfo("LogoutAndSetTokenExpiredResult Success.");
                }
                else
                {
                    GLog.LogError("LogoutAndSetTokenExpiredResult Failed:" + ret.ToString());
                }

                CleanLoginedInfo();
                OnLogoutCallback(InnerTools.ConvertToResult(ret));
            });
        }

        public void FetchLinkInfo(AccountLinkInfoDelegate callback)
        {
            GLog.LogInfo("FetchLinkInfo");
            var accountCallbackHandler = new AccountCallbackHandler {AccountLinkInfoDelegate = callback};
            accountCallbackHandler.OnSuccess = accountCallbackHandler.HandleLinkInfo;
            UNBridge.Call(AccountConstants.MethodFetchLinkInfo, null, accountCallbackHandler);
        }

        public void LinkAuth(AccountLinkInfoDelegate callback, ScopeInfo scopeInfo = null)
        {
            GLog.LogInfo("LinkAuth");
            var accountCallbackHandler = new AccountCallbackHandler {AccountLinkInfoDelegate = callback};
            accountCallbackHandler.OnSuccess = accountCallbackHandler.HandleLinkInfo;
            var callData = new JsonData();
            var param = new JsonData();
            param.SetJsonType(JsonType.Object);
            if (scopeInfo != null)
            {
                if (scopeInfo.Permission != null)
                    param["permissions"] = JsonMapper.ToObject(JsonMapper.ToJson(scopeInfo.Permission));
                if (scopeInfo.AdditionalPermissionSelected != null)
                    param["additionalPermissionsSelected"] =
                        JsonMapper.ToObject(JsonMapper.ToJson(scopeInfo.AdditionalPermissionSelected));
                if (scopeInfo.AdditionalPermissionUnSelected != null)
                    param["additionalPermissionsUnSelected"] =
                        JsonMapper.ToObject(JsonMapper.ToJson(scopeInfo.AdditionalPermissionUnSelected));
                if (scopeInfo.Extra != null)
                    param["extraInfo"] = JsonMapper.ToObject(JsonMapper.ToJson(scopeInfo.Extra));
            }

            callData["data"] = param;
            UNBridge.Listen(AccountConstants.EventLinkAuthResult, accountCallbackHandler);
            UNBridge.Call(AccountConstants.MethodLinkAuth, callData);
        }

        public void LinkRelease(AccountLinkInfoDelegate callback)
        {
            GLog.LogInfo("LinkRelease");
            var accountCallbackHandler = new AccountCallbackHandler {AccountLinkInfoDelegate = callback};
            accountCallbackHandler.OnSuccess = accountCallbackHandler.HandleLinkInfo;
            UNBridge.Call(AccountConstants.MethodLinkRelease, null, accountCallbackHandler);
        }

        public ScopeInfo GetDefaultScopeInfo()
        {
            GLog.LogInfo("GetDefaultScopeInfo");
            var res = UNBridge.CallSync(AccountConstants.MethodGetDefaultScopeInfo, null);
            ScopeInfo scopeInfo = new ScopeInfo
            {
                Permission = new List<string>(),
                AdditionalPermissionSelected = new List<string>(),
                AdditionalPermissionUnSelected = new List<string>(),
                Extra = new Dictionary<object, object>()
            };
            GLog.LogInfo("GetDefaultScopeInfo:" + JsonMapper.ToJson(res));
            if (res != null)
            {
                var data = JsonMapper.ToObject(JsonMapper.ToJson(res));
                if (data.ContainsKey("permissions"))
                {
                    var permissions = JsonMapper.ToObject<List<string>>(JsonMapper.ToJson(data["permissions"]));
                    if (permissions != null)
                    {
                        scopeInfo.Permission.AddRange(permissions);
                    }
                }

                if (data.ContainsKey("additionalPermissionsSelected"))
                {
                    var additionalPermissionsSelected =
                        JsonMapper.ToObject<List<string>>(JsonMapper.ToJson(data["additionalPermissionsSelected"]));
                    if (additionalPermissionsSelected != null)
                    {
                        scopeInfo.AdditionalPermissionSelected.AddRange(additionalPermissionsSelected);
                    }
                }

                if (data.ContainsKey("additionalPermissionsUnSelected"))
                {
                    var additionalPermissionsUnSelected =
                        JsonMapper.ToObject<List<string>>(JsonMapper.ToJson(data["additionalPermissionsUnSelected"]));
                    if (additionalPermissionsUnSelected != null)
                    {
                        scopeInfo.AdditionalPermissionUnSelected.AddRange(additionalPermissionsUnSelected);
                    }
                }

                if (data.ContainsKey("extraInfo"))
                {
                    var extraInfo = JsonMapper.ToObject<Dictionary<object, object>>(
                        JsonMapper.ToJson(data["extraInfo"]));
                    if (extraInfo != null)
                    {
                        foreach (var pair in extraInfo)
                        {
                            scopeInfo.Extra.Add(pair.Key, pair.Value);
                        }
                    }
                }
            }

            return scopeInfo;
        }

        private class UserCenter : IUserCenter
        {
            public Action<Result> LogoutAction;
            public Action<Result, RealNameAuthResult> RealNameAction;
            public Action<Result, BindIndependentResult> BindIndependentAction;
            public Action PanelClosedAction;

#region IUserCenter实现

            public void logoutCallback(GMSDK.CallbackResult ret)
            {
                GLog.LogInfo("logoutevent in IUserCenter, result:" + ret);
                InnerTools.SafeInvoke(() => { LogoutAction(InnerTools.ConvertToResult(ret)); });
            }

            public void realNameCallback(GMSDK.RealNameAuthRet ret)
            {
                GLog.LogInfo("Realname in IUserCenter, result: " + ret.IsSuccess() + ", code:" + ret.code +
                             ", message:" +
                             ret.message + ", level:" + ret.authLevel
                             + ", age:" + ret.age + ", anti_addiction_tips = " + ret.anti_addiction_tips);
                InnerTools.SafeInvoke(() =>
                {
                    RealNameAction(InnerTools.ConvertToResult(ret), AccountInnerTools.Convert(ret));
                });
            }

            public void bindCallback(GMSDK.BindIndependentResult ret)
            {
                GLog.LogInfo("Show Independent Bind Panel in IUserCenter, result: " + ret);
                InnerTools.SafeInvoke(() => { BindIndependentAction(InnerTools.ConvertToResult(ret), ret); });
            }

            public void closeCallback(CallbackResult ret)
            {
                GLog.LogInfo("UserCenter Panel Closed in IUserCenter, result: " + ret);
                InnerTools.SafeInvoke(PanelClosedAction);
            }

#endregion
        }
    }
}

