using System;
using GMSDK;
using UNBridgeLib.LitJson;

#if UNITY_ANDROID
namespace GSDK
{
    public partial class ChannelService : IChannelService, IChannelExtraCallbackCenter
    {
        private MainSDK gsdk = GMSDKMgr.instance.SDK;
        public event ChannelLogoutEventHandler LogoutChannelEvent;
        public event ChannelSwitchAccountEventHandler SwitchAccountChannelEvent;
        public event ChannelExitChannelEventHandler ExitChannelEvent;

        public void EnterGameUpload(GameUploadInfo cpUploadInfo)
        {
            GLog.LogInfo("EnterGameUpload");

            gsdk.EnterGameUpload(ChannelInnerTools.Convert(cpUploadInfo));
        }

        public void CreateNewRoleUpload(GameUploadInfo cpUploadInfo)
        {
            GLog.LogInfo("CreateNewRoleUpload");

            gsdk.CreateNewRoleUpload(ChannelInnerTools.Convert(cpUploadInfo));
        }

        public void RoleLevelUpload(GameUploadInfo cpUploadInfo)
        {
            GLog.LogInfo("RoleLevelUpload");

            gsdk.RoleLevelUpload(ChannelInnerTools.Convert(cpUploadInfo));
        }

        public void RoleExitUpload(GameUploadInfo cpUploadInfo)
        {
            GLog.LogInfo("RoleExitUpload");

            gsdk.RoleExitUpload(ChannelInnerTools.Convert(cpUploadInfo));
        }

        public void ExitGame(ChannelExitGameInfoDelegate exitGameCallback)
        {
            GLog.LogInfo("ExitGame");
            gsdk.SdkOnExit((OnExitResult exitResult) =>
            {
                GLog.LogDebug("SdkOnExit result: " + exitResult);
                OnExitGameCallback(InnerTools.ConvertToResult(exitResult),
                    ChannelInnerTools.Convert(exitResult), exitGameCallback);
            });
        }

        public void RegisterAccountStatusChangedListener()
        {
            GLog.LogInfo("RegisterAccountStatusChangedListener");
            gsdk.RegisterAccountStatusChangedListener(this);
        }

        public void IsApiAvailable(string apiName)
        {
            GLog.LogInfo("IsApiAvailable : " + apiName);
            gsdk.SDKIsAvailable(apiName);
        }
        
        public void ChannelGeneralCallback(ChannelGeneralDelegate channelGeneralCallback)
        {
            GLog.LogInfo("ChannelGeneralCallback");
            gsdk.SdkChannelGeneralCallback((ChannelGeneralCallbackResult generalResult) =>
            {
                GLog.LogDebug("ChannelGeneralCallback result: " + generalResult);
                OnChannelGeneralCallback(InnerTools.ConvertToResult(generalResult),
                    ChannelInnerTools.Convert(generalResult), channelGeneralCallback);
            });
        }
        public void ChannelGeneral(JsonData jsonData)
        {
            GLog.LogInfo("ChannelGeneral");
            gsdk.SdkChannelGeneral(jsonData);
        }

        void OnExitGameCallback(Result result, ExitGameInfo exitGameInfo, ChannelExitGameInfoDelegate exitGameCallback)
        {
            if (exitGameCallback != null)
            {
                try
                {
                    exitGameCallback(result, exitGameInfo);
                }
                catch (Exception ex)
                {
                    GLog.LogException(ex);
                }
            }
        }

        void OnChannelGeneralCallback(Result result, GeneralResult generalResult,
            ChannelGeneralDelegate generalCallbackResult)
        {
            if (generalCallbackResult != null)
            {
                try
                {
                    generalCallbackResult(result, generalResult);
                }
                catch (Exception ex)
                {
                    GLog.LogException(ex);
                }
            }
        }

        public void logoutChannelCallback(CallbackResult ret)
        {
            GLog.LogInfo("LogoutChannel in IChannelExtraCallbackCenter, result: " + ret.IsSuccess() + ", code:" +
                         ret.code + ", message:" +
                         ret.message);
            OnLogoutChannelCallback(InnerTools.ConvertToResult(ret));
        }

        public void switchAccountChannelCallback(LoginResultRet ret)
        {
            GLog.LogInfo("SwitchAccountChannel in IChannelExtraCallbackCenter, result: " + ret.IsSuccess() + ", code:" +
                         ret.code + ", message:" +
                         ret.message);
            OnSwitchAccountChannelCallback(ChannelInnerTools.Convert(ret));
        }

        public void onExitChannelCallback(GMSDK.ExitResult ret)
        {
            GLog.LogInfo("ExitChannel in IChannelExtraCallbackCenter, result: " + ret.IsSuccess() + ", code:" +
                         ret.code + ", message:" + ret.message);
            OnExitChannelCallback(ChannelInnerTools.Convert(ret));
        }

        void OnLogoutChannelCallback(Result result)
        {
            if (LogoutChannelEvent != null)
            {
                try
                {
                    LogoutChannelEvent(result);
                }
                catch (Exception ex)
                {
                    GLog.LogException(ex);
                }
            }
        }

        void OnSwitchAccountChannelCallback(LoginResult loginResult)
        {
            if (SwitchAccountChannelEvent != null)
            {
                try
                {
                    SwitchAccountChannelEvent(loginResult);
                }
                catch (Exception ex)
                {
                    GLog.LogException(ex);
                }
            }
        }

        void OnExitChannelCallback(ExitResult exitResult)
        {
            if (ExitChannelEvent != null)
            {
                try
                {
                    ExitChannelEvent(exitResult);
                }
                catch (Exception ex)
                {
                    GLog.LogException(ex);
                }
            }
        }
    }
}
#endif
