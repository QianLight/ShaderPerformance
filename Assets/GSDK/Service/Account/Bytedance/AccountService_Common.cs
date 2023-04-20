using System;
using GSDK;
using System.Runtime.InteropServices;
using UNBridgeLib.LitJson;
using System.Collections.Generic;

namespace GSDK
{
    public partial class AccountBytedanceService : IAccountBytedanceService
    {

        // gsdk实例
        private GMSDK.MainSDK gsdk;

        public bool HasLogined
        {
            get
            {
#if UNITY_STANDALONE_WIN && !GMEnderOn 
                return false;
#else
                GSDKProfilerTools.BeginSample("Account HasLogined");
                var res = _accountInfo != null &&
                          !string.IsNullOrEmpty(_accountInfo.OpenID) && !string.IsNullOrEmpty(_accountInfo.UserID) &&
                          !string.IsNullOrEmpty(_accountInfo.Token)
                          && gsdk.SdkGetLoginState();
                GSDKProfilerTools.EndSample();
                return res;
#endif
            }
        }
        
        AccountInfo _accountInfo = new AccountInfo();
        
        void CleanLoginedInfo()
        {
            _accountInfo = new AccountInfo();
        }

        public event AccountLoginEventHandler LoginEvent;
        public event AccountLogoutEventHandler LogoutEvent;

        public event AccountRealNameAuthEventHandler RealNameAuthEvent;
        public event AccountBindAccountEventHandler BindAccountEvent;
        public event AccountUnbindAccountEventHandler UnbindAccountEvent;

        public AccountBytedanceService()
        {
            gsdk = GMSDKMgr.instance.SDK;
        }
        
#if UNITY_STANDALONE_WIN && !GMEnderOn
        [MonoPInvokeCallback(typeof(Callback2P))]
        private static void LoginWithUICallback(string resultStr, string callbackStr)
        {
            GLog.LogDebug("resultStr:" + resultStr + ",callbackStr:" + callbackStr);
            Result result = JsonMapper.ToObject<Result>(resultStr);
            AccountInfo accountInfo = JsonMapper.ToObject<AccountInfo>(callbackStr);

            accountInfo.AccountType = AccountInnerTools.Convert((GMSDK.GMUserType)accountInfo.AccountType);
            AccountBytedanceService accountService = Account.Service as AccountBytedanceService;
            if (accountService.LoginEvent != null)
            {
                accountService.LoginEvent(result, accountInfo);
            }
        }
        [MonoPInvokeCallback(typeof(Callback1P))]
        private static void LogoutCallback(string resultStr)
        {
            GLog.LogDebug("resultStr:" + resultStr);
            Result result = JsonMapper.ToObject<Result>(resultStr);
            AccountBytedanceService accountService = Account.Service as AccountBytedanceService;
            if (accountService.LogoutEvent != null)
            {
                accountService.LogoutEvent(result);
            }
        }
#endif
        
        public void LoginWithUI(AccountPanelClosedDelegate accountPanelClosedDelegate)
        {
            GSDKProfilerTools.BeginSample("Account LoginWithUI");
            GLog.LogInfo("LoginWithUI");
#if UNITY_STANDALONE_WIN && !GMEnderOn
            AccountSetLoginCallback(LoginWithUICallback);
            AccountLoginWithUI();
#else
            gsdk.SdkLogin(ProcessLoginResult, result =>
            {
                InnerTools.SafeInvoke(() =>
                {
                    if (accountPanelClosedDelegate != null)
                    {
                        accountPanelClosedDelegate();
                    }
                });
            });
#endif
            GSDKProfilerTools.EndSample();
        }

        void OnLoginedCallback(Result result, AccountInfo loginInfo)
        {
            if (LoginEvent != null)
            {
                try
                {
                    LoginEvent(result, loginInfo);
                }
                catch (Exception ex)
                {
                    GLog.LogException(ex);
                }
            }
        }


        void ProcessLoginResult(GMSDK.LoginResultRet ret)
        {
            GLog.LogInfo("LoginResult: " + ret);
            if (ret.IsSuccess())
            {
                if (ret.userInfo == null)
                {
                    GLog.LogError("Login success, but ret.userInfo == null");

                    // Todo: optimize the detail error code.
                    OnLoginedCallback(new Result(ErrorCode.AccountUserInfoNullError, "Login success, but ret.userInfo == null"),
                        null);
                    return;
                }
                
                GLog.LogDebug("Login Success. UserID:" + ret.userInfo.userId + " OpenID:" + gsdk.SdkGetSdkOpenId());
                GLog.LogDebug("==userInfo==: {isGuest:" + ret.userInfo.isGuest + ", userType:" +
                              ret.userInfo.userType
                              + ", isVerified:"  + ", channel:" + ret.userInfo.channel
                              + ", channelId:" + ret.userInfo.channelId + ", Token:" + ret.token + "}");
                
                _accountInfo = AccountInnerTools.Convert(ret);
                // 此时返回的openID只是Native端本地存的，需要再次验证
                _accountInfo.OpenID = gsdk.SdkGetSdkOpenId();
                OnLoginedCallback(new Result(ErrorCode.Success, "Login Success!"), _accountInfo);

            }
            else
            {
                GLog.LogError("Login Failed");
                OnLoginedCallback(InnerTools.ConvertToResult(ret), null);
            }
        }

        public AccountInfo GetLoginRecord()
        {
#if UNITY_STANDALONE_WIN && !GMEnderOn
                return null;
#else
            return _accountInfo;
#endif
        }

        public void Logout()
        {
            GSDKProfilerTools.BeginSample("Account Logout");
            GLog.LogInfo("Logout");
#if UNITY_STANDALONE_WIN && !GMEnderOn
            AccountSetLogoutCallback(LogoutCallback);
            AccountLogout();
#else
            gsdk.SdkLogoutAccount((GMSDK.CallbackResult ret) =>
            {
                GLog.LogDebug("LogoutResult: " + ret);
                if (ret.IsSuccess())
                {
                    GLog.LogInfo("Logout Success.");
                }
                else
                {
                    GLog.LogError("Logout Failed.");
                }
                CleanLoginedInfo();
                OnLogoutCallback(InnerTools.ConvertToResult(ret));
            });
#endif
            GSDKProfilerTools.EndSample();
        }

        void OnLogoutCallback(Result result)
        {
            if (LogoutEvent != null)
            {
                try
                {
                    LogoutEvent(result);
                }
                catch (Exception ex)
                {
                    GLog.LogException(ex);
                }
            }
        }

        public bool IsProtocolUpdated()
        {
#if UNITY_STANDALONE_WIN && !GMEnderOn
            return ComplianceSdkIsProtocolVersionUpdated();
#else
            return gsdk.SdkIsProtocolVersionUpdated();
#endif

        }


        public void ResetGuest(AccountResetGuestDelegate resetGuestCallback)
        {
            gsdk.SdkResetGuest((GMSDK.ResetGuestResult ret) =>
            {
                InnerTools.SafeInvoke(() =>
                {
                    resetGuestCallback(InnerTools.ConvertToResult(ret), ret.needLogout);
                });
            });
        }
        
        /**
         * 判断是否为云游戏环境，true为是
         */
        public bool IsCloudRuntime()
        {
            var res = false;
#if UNITY_ANDROID
            GLog.LogInfo("IsCloudRuntime");
            res = gsdk.SdkIsCloudRuntime();
#endif
            return res;
        }



#if UNITY_STANDALONE_WIN && !GMEnderOn        
        [DllImport(PluginName.GSDK)]
        private static extern void AccountSetLoginCallback(Callback2P callback);

        [DllImport(PluginName.GSDK)]
        private static extern void AccountSetLogoutCallback(Callback1P callback);
        
        [DllImport(PluginName.GSDK)]
        private static extern void AccountLoginWithUI();

        [DllImport(PluginName.GSDK)]
        private static extern void AccountLoginWithoutUI(int accountType);

        [DllImport(PluginName.GSDK)]
        private static extern void AccountLogout();

        [DllImport(PluginName.GSDK)]
        private static extern bool ComplianceSdkIsProtocolVersionUpdated();

        [DllImport(PluginName.GSDK)]
        private static extern void ComplianceAuthRealNameWithUI(int type, Callback2P callback);

#endif

    }
}