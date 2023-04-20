using System;
using System.Collections.Generic;
using UnityEngine;
using CFUtilPoolLib;
using CFUtilPoolLib.GSDK;
using GSDK;

/// <summary>
/// GSDK接口集合
/// </summary>
public partial class GSDKSystem : XSingleton<GSDKSystem>, IGSDKSystem
{
    public bool Deprecated { get; set; }
    private CFUtilPoolLib.GSDK.AccountLoginEventHandler m_loginCallback;
    private CFUtilPoolLib.GSDK.AccountLogoutEventHandler m_logoutCallback;
    private CFUtilPoolLib.GSDK.FetchZonesListDelegate m_fetchZonesListCallback;
    private CFUtilPoolLib.GSDK.FetchRolesListDelegate m_fetchRolesListCallback;
    private CFUtilPoolLib.GSDK.FetchZonesAndRolesListDelegate m_fetchZonesAndRolesListCallback;
    private CFUtilPoolLib.GSDK.CdKeyDelegate m_CdKeyDelegate;
    private CFUtilPoolLib.GSDK.AccountShowUserCenterDelegate m_AccountShowUserCenterDelegate;

    //webview回调函数
    private CFUtilPoolLib.GSDK.WebviewShowDelegate m_webviewShowCallback;
    private CFUtilPoolLib.GSDK.WebviewExitDelegate m_webviewExitCallback;

    //老实时语音回调函数
    private CFUtilPoolLib.GSDK.JoinRoomDelegate m_joinRoomCallback;
    private CFUtilPoolLib.GSDK.LeaveRoomEventHandler m_leaveRoomEventCallback;
    private CFUtilPoolLib.GSDK.OtherUserJoinEventHandler m_otherUserJoinEventCallback;
    private CFUtilPoolLib.GSDK.OtherUserLeaveEventHandler m_otherUserLeaveEventCallback;
    private CFUtilPoolLib.GSDK.AudioVolumeIndicationEventHandler m_audioVolumeIndicationEventCallback;

    //判断是否是模拟器
    private CFUtilPoolLib.GSDK.SystemCheckEmulatorDelegate m_systemCheckEmulatorCallback;

    //获取用户协议和隐私政策
    private Action<CFUtilPoolLib.GSDK.ProtocolAddressResult> m_protocolAddressResultCallback;

    //扫码
    private CFUtilPoolLib.GSDK.ScanQRCodeDelegate m_scanQRCodeCallback;

    private bool m_CheckPrivate = false;
    private bool m_hasAddLoginEvent = false;

    //sdk初始化的回调函数
    private CFUtilPoolLib.GSDK.GSDKInitializeDelegate m_sdkInitializeCallback;

    private IAgreementService m_agreementService;

    public bool m_callAfterClickAgree = false;//pc上使用，当玩家点击隐私协议同意或者拒绝之后调用，这个是子线程中，所以必须在主线程中调用
    public bool m_hasAgreePrivacy = false; //pc上使用
    private CFUtilPoolLib.GSDK.Result m_initSDKResult; //pc上使用


#if UNITY_ANDROID
    private GameUploadInfo cpUploadInfo;
#endif

    private CFUtilPoolLib.GSDK.FetchRealNameStateDelegate m_accountFetchRealNameStateCallback;
    private CFUtilPoolLib.GSDK.ComplianceRealNameAuthResultDelegate m_accountRealNameAuthCallback;
    private CFUtilPoolLib.GSDK.AntiAddictionStatusEventHandler m_antiAddictionCallback;

    /// <summary>
    /// windows平台，先初始化sdk，后检测隐私协议
    /// 移动平台要先检测隐私协议，后初始化sdk
    /// </summary>
    /// <param name="callback"></param>
    public void Initialize(GSDKInitializeDelegate callback)
    {
        XDebug.singleton.AddGreenLog("InitializeGSDK platform=" + Application.platform);
        m_sdkInitializeCallback = callback;
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            GameSDK.Instance.Initialize(SDKInitedCallback);
        }
        else
        {
            InitializeForMobilePlatform();
        }
    }

    private void InitializeForMobilePlatform()
    {
        bool hasAgreePrivacy = Compliance.Service.Privacy.HasAgreePrivacy;
#if UNITY_EDITOR
        hasAgreePrivacy = true;
#endif
        XDebug.singleton.AddGreenLog("hasAgreePrivacy=" + hasAgreePrivacy);
#if UNITY_ANDROID
        if (cpUploadInfo == null)
        {
            cpUploadInfo = new GameUploadInfo();
            InitRoleInfo();
        }
#endif
        if (hasAgreePrivacy)
        {
            GameSDK.Instance.Initialize(SDKInitedCallback);
        }
        else
        {
            XDebug.singleton.AddGreenLog("SDKShowPrivacy");
            Compliance.Service.Privacy.PrivacyShownEvent += (agree) =>
            {
                if (agree)
                {
                    GameSDK.Instance.Initialize(SDKInitedCallback);
                }
                else
                {
                    CFUtilPoolLib.INativeInfo nativeInfo = CFUtilPoolLib.XInterfaceMgr.singleton.GetInterface<CFUtilPoolLib.INativeInfo>(XInterfaceMgr.XNativeID);
                    if (nativeInfo != null)
                    {
                        nativeInfo.CloseApplication();
                    }
                }
            };
            Compliance.Service.Privacy.ShowPrivacy();
        }
    }

    private void SDKInitedCallback(GSDK.Result result)
    {
        if (m_sdkInitializeCallback == null) return;
        XUpdater.XUpdater.ToSyncTrack(GSDKRecordWord.PRELOGIN_SDK_PRIVACY);
        m_initSDKResult = GetGSDKResult(result);
        XDebug.singleton.AddGreenLog("ResInitSDK=" + result.ToDetailedString());
        if (Application.platform == RuntimePlatform.WindowsPlayer)
        {
            if (!m_initSDKResult.IsSuccess) //未初始化成功，则直接回调，让其再次初始化sdk
            {
                m_sdkInitializeCallback(m_initSDKResult);
            }
            else //否则进行隐私协议检测
            {
                CheckHasAgreePrivacy();
            }
        }
        else
        {
            m_sdkInitializeCallback(m_initSDKResult);
        }
    }

    private void CheckHasAgreePrivacy()
    {
        m_hasAgreePrivacy = Compliance.Service.Privacy.HasAgreePrivacy;
        XDebug.singleton.AddGreenLog("CheckHasAgreePrivacy hasAgreePrivacy=" + m_hasAgreePrivacy);
        if (m_hasAgreePrivacy)
        {
            m_sdkInitializeCallback(m_initSDKResult);
        }
        else
        {
            XDebug.singleton.AddGreenLog("SDKShowPrivacy");
            Compliance.Service.Privacy.PrivacyShownEvent += (agree) =>
            {
                m_callAfterClickAgree = true; //主线程中检测此变量，若为true，则在主线中执行回调
                m_hasAgreePrivacy = agree;
            };
            Compliance.Service.Privacy.ShowPrivacy();
        }
    }

    /// <summary>
    /// pc上主线程监测此变量，然后执行回调函数
    /// </summary>
    public void UpdateCallAfterClickAgree()
    {
        if (!m_callAfterClickAgree) return;
        XDebug.singleton.AddGreenLog("UpdateCallAfterClickAgree m_hasAgreePrivacy=" + m_hasAgreePrivacy + ",m_callAfterClickAgree = " + m_callAfterClickAgree);
        m_callAfterClickAgree = false;

        if (m_hasAgreePrivacy)
        {
            m_sdkInitializeCallback(m_initSDKResult);
        }
        else
        {
            Application.Quit();
        }
    }

    public void SdksetForceUpdateCheck()
    {
        Upgrade.Service.CheckForceUpgrade((GSDK.Result result, UpgradeInfo upgradeInfo) =>
        {
            if (result.IsSuccess)
            {
#if UNITY_ANDROID

#elif UNITY_IOS
              
#endif
            }
        }, true);
    }

    public void GetBulletin(string language = "zh", string region = "CN", int Scene = 6, string serverId = null, string openId = null, string zoneId = null, string roleId = null, CFUtilPoolLib.FetchBulletinsDelegate fetchBulletin = null)
    {
        GSDK.BulletinConfig bulletinConfig = new GSDK.BulletinConfig(1);
        bulletinConfig.Language = language;
        bulletinConfig.Region = region;
        bulletinConfig.Scene = Scene;
        bulletinConfig.ServerId = serverId;
        bulletinConfig.OpenId = openId;
        bulletinConfig.RoleId = roleId;
        Bulletin.Service.FetchBulletins(bulletinConfig, (result, bulletinInfo) =>
        {
            if (fetchBulletin != null)
            {
                CFUtilPoolLib.GSDK.Result resultSelf = GetGSDKResult(result);
                CFUtilPoolLib.BulletinInfo bulletInfoSelf = new CFUtilPoolLib.BulletinInfo();
                bulletInfoSelf.BulletinItems = new List<CFUtilPoolLib.BulletinItem>();
                for (int i = 0; bulletinInfo.BulletinItems != null && i < bulletinInfo.BulletinItems.Count; i++)
                {
                    if (bulletinInfo.BulletinItems[i] == null) continue;
                    CFUtilPoolLib.BulletinItem item = new CFUtilPoolLib.BulletinItem();
                    item.BID = bulletinInfo.BulletinItems[i].BID;
                    item.Language = bulletinInfo.BulletinItems[i].Language;
                    item.Scene = bulletinInfo.BulletinItems[i].Scene;
                    item.Title = bulletinInfo.BulletinItems[i].Title;
                    item.Content = bulletinInfo.BulletinItems[i].Content;
                    item.TargetURL = bulletinInfo.BulletinItems[i].TargetURL;
                    item.Priority = bulletinInfo.BulletinItems[i].Priority;
                    item.ImageURL = bulletinInfo.BulletinItems[i].ImageURL;
                    item.Encoding = bulletinInfo.BulletinItems[i].Encoding;
                    item.ButtonText = bulletinInfo.BulletinItems[i].ButtonText;
                    item.StartTime = bulletinInfo.BulletinItems[i].StartTime;
                    item.ExpireTime = bulletinInfo.BulletinItems[i].ExpireTime;
                    item.Tab = bulletinInfo.BulletinItems[i].Tab;
                    item.Extra = bulletinInfo.BulletinItems[i].Extra;
                    item.ImageInfoJson = bulletinInfo.BulletinItems[i].ImageInfoJson;
                    item.imageList = new List<CFUtilPoolLib.GMImageItem>();
                    for (int k = 0; k < bulletinInfo.BulletinItems[i].ImageList.Count; k++)
                    {
                        if (bulletinInfo.BulletinItems[i].ImageList[k] == null) continue;
                        CFUtilPoolLib.GMImageItem ii = new CFUtilPoolLib.GMImageItem();
                        ii.imageLink = bulletinInfo.BulletinItems[i].ImageList[k].imageLink;
                        ii.imageJumpLink = bulletinInfo.BulletinItems[i].ImageList[k].imageJumpLink;
                        ii.imageInfoJson = bulletinInfo.BulletinItems[i].ImageList[k].imageInfoJson;
                        item.imageList.Add(ii);
                    }
                    bulletInfoSelf.BulletinItems.Add(item);
                }
                bulletInfoSelf.TotalPage = bulletinInfo.TotalPage;
                bulletInfoSelf.CurrentPage = bulletinInfo.CurrentPage;
                bulletInfoSelf.PageSize = bulletinInfo.PageSize;
                bulletInfoSelf.Filters = bulletinInfo.Filters;
                fetchBulletin.Invoke(resultSelf, bulletInfoSelf);
            }
        });
    }



    public string SdkGetDeviceId()
    {
        //return GMSDKMgr.instance.SDK.SdkGetDeviceId();
        return App.Service.DeviceID; //新接口
    }

    public string SdkGetChannel()
    {
        return GMSDKMgr.instance.SDK.SdkGetChannel();
    }

    public string SdkGetChannelOp()
    {
        return GMSDKMgr.instance.SDK.SdkGetChannelOp();
    }


    /// <summary>
    /// login with ui
    /// </summary>
    public void LoginWithUI(CFUtilPoolLib.GSDK.AccountLoginEventHandler loginCallback, CFUtilPoolLib.GSDK.AccountLogoutEventHandler logoutCallback)
    {
        m_loginCallback = loginCallback;
        m_logoutCallback = logoutCallback;
        if (!m_hasAddLoginEvent)
        {
            Account.Service.LoginEvent += LoginEvent;  //https://gdev.nvsgames.com/docs/5180?language=zh#LoginEvent
            Account.Service.LogoutEvent += LogoutEvent;
            //AntiAddiction.Service.AntiAddictionStatusEvent += AntiAddictionService_AntiAddictionStatusEvent;
            m_hasAddLoginEvent = true;
        }
        Account.Service.LoginWithUI(AccountPanelClosedCallback);
    }

    /// <summary>
    /// 不做任何事情，仅仅为了不要报空引用错误
    /// </summary>
    private void AccountPanelClosedCallback()
    {
        //do nothing
    }

    private void LoginEvent(GSDK.Result result, GSDK.AccountInfo accountInfo)
    {
        if (m_loginCallback != null)
        {
            CFUtilPoolLib.GSDK.Result gsdkResult = GetGSDKResult(result);
            CFUtilPoolLib.GSDK.AccountInfo gsdkAccountInfo = GetGSDKAccountInfo(accountInfo);
            m_loginCallback(gsdkResult, gsdkAccountInfo);

            RegisterAccountStatusChangedListener();
        }
    }

    public void Logout()
    {
        Account.Service.Logout();
    }

    private void LogoutEvent(GSDK.Result result)
    {
        if (m_logoutCallback != null)
        {
            //Account.Service.LogoutEvent -= LogoutEvent;
            CFUtilPoolLib.GSDK.Result gsdkResult = GetGSDKResult(result);
            m_logoutCallback(gsdkResult);
        }
    }

    public void SdksetDebug(bool enable)
    {
        GMSDKMgr.instance.SDK.SdksetDebug(enable);
    }

    public void SdksetPrivateCheck(bool enable)
    {
        m_CheckPrivate = enable;
    }



    #region track scene event

    public void OnServiceSceneStart(string sceneName)
    {
        GSDK.GPMMonitor.Service.OnSceneStart(sceneName);
    }

    public void OnServiceSceneLoadFinish()
    {
        GSDK.GPMMonitor.Service.OnSceneLoadFinish();
    }

    public void OnServiceSceneEnd()
    {
        GSDK.GPMMonitor.Service.OnSceneEnd();
    }

    public void OnServiceAddSceneInfo(string key, string value)
    {
        GSDK.GPMMonitor.Service.AddSceneInfo(key, value);
    }

    public void OnServiceAddSceneInfo(string key, int value)
    {
        GSDK.GPMMonitor.Service.AddSceneInfo(key, value);
    }

    public void OnServiceAddGlobalInfo(string key, string value)
    {
        GSDK.GPMMonitor.Service.AddGlobalInfo(key, value);

    }

    public void OnServiceAddGlobalInfo(string key, int value)
    {
        GSDK.GPMMonitor.Service.AddGlobalInfo(key, value);
    }

    public int GetServiceGraphicLevel()
    {
        //int level2 = GPMSDKMgr.instance.SDK.GraphicLevel();
        //XDebug.singleton.AddGreenLog("GSDKSystemGetServiceGraphicLevel level2=" + level2);

        int level = GSDK.GPMMonitor.Service.GetGraphicLevel();
        XDebug.singleton.AddGreenLog("GSDKSystemGetServiceGraphicLevel level=" + level);
        return level;
    }

    #endregion



    public void FetchZonesList(string gameVersion, CFUtilPoolLib.GSDK.FetchZonesListDelegate fetchZonesListCallback, double timeout = double.NaN)
    {
        m_fetchZonesListCallback = fetchZonesListCallback;
        GSDK.Role.Service.FetchZonesList(gameVersion, FetchZonesListCallback, timeout);
    }

    public void FetchRolesList(CFUtilPoolLib.GSDK.FetchRolesListDelegate fetchRolesListCallback, double timeout = double.NaN)
    {
        m_fetchRolesListCallback = fetchRolesListCallback;
        GSDK.Role.Service.FetchRolesList(FetchRolesListCallback);
    }

    private void FetchRolesListCallback(GSDK.Result result, List<GSDK.RoleInfo> roles)
    {
        if (m_fetchRolesListCallback != null)
        {
            CFUtilPoolLib.GSDK.Result gResult = GetGSDKResult(result);
            List<CFUtilPoolLib.GSDK.RoleInfo> gsdkRoles = GetGSDKRolesList(roles);
            m_fetchRolesListCallback(gResult, gsdkRoles);
        }
    }

    /// <summary>
    /// 监修服列表：11.0.0
    /// 正式服列表：1.0.0
    /// 测试服列表：10.10.10
    /// </summary>
    /// <param name="gameVersion"></param>
    /// <param name="fetchZonesAndRolesListCallback"></param>
    /// <param name="timeout"></param>
    public void FetchZonesAndRolesList(string gameVersion, CFUtilPoolLib.GSDK.FetchZonesAndRolesListDelegate fetchZonesAndRolesListCallback, double timeout = double.NaN)
    {
        m_fetchZonesAndRolesListCallback = fetchZonesAndRolesListCallback;
        gameVersion = Resources.Load<TextAsset>("zoneversion").text;
        gameVersion = gameVersion.Replace("\n", "").Replace("\t", "").Replace("\r", "");
        XDebug.singleton.AddGreenLog("FetchZonesAndRolesList start" + gameVersion + "xxx");
        GSDK.Role.Service.FetchZonesAndRolesList(gameVersion, FetchZonesAndRolesListCallback, timeout);
    }


    public void ReportUserInfo(CFUtilPoolLib.GSDK.AccountType userType, string roleId, string serverId)
    {
        SecurityService securityService = (GSDK.Security.Service as SecurityService);
        if (securityService != null)
        {
            securityService.ReportUserInfo((GSDK.AccountType)userType, roleId, serverId);
        }
    }

    private void FetchZonesAndRolesListCallback(GSDK.Result result, List<GSDK.ZoneInfo> zones, List<GSDK.RoleInfo> roles)
    {
        if (m_fetchZonesAndRolesListCallback != null)
        {
            CFUtilPoolLib.GSDK.Result gsdkResult = GetGSDKResult(result);
            List<CFUtilPoolLib.GSDK.ZoneInfo> gsdkZones = GetGSDKZonesList(zones);
            List<CFUtilPoolLib.GSDK.RoleInfo> gsdkRoles = GetGSDKRolesList(roles);
            m_fetchZonesAndRolesListCallback(gsdkResult, gsdkZones, gsdkRoles);
            string str = "FetchZonesAndRolesListCallback";
            str += result.IsSuccess + "  result.Error=" + result.Error + "=";
            if (zones != null) str += "zone count = " + zones.Count + "  ";
            if (roles != null) str += "role count = " + roles.Count + "  ";
            XDebug.singleton.AddGreenLog(str);
            if (zones != null)
            {
                foreach (var item in zones)
                {
                    if (item != null && item.Servers != null)
                    {
                        foreach (var item2 in item.Servers)
                        {
                            XDebug.singleton.AddGreenLog("serverinfoServerId=" + item2.ServerId + "==ServerName==" + item2.ServerName + "==ServerStatus==" + item2.ServerStatus +
                                "==OnlineLoad==" + item2.OnlineLoad + "==ServerType==" + item2.ServerType + "==RealServerId==" + item2.RealServerId);
                        }
                    }
                }
            }
        }
    }

    private void FetchZonesListCallback(GSDK.Result result, List<GSDK.ZoneInfo> zones)
    {
        if (m_fetchZonesListCallback != null)
        {
            CFUtilPoolLib.GSDK.Result gsdkResult = GetGSDKResult(result);
            List<CFUtilPoolLib.GSDK.ZoneInfo> gsdkZones = GetGSDKZonesList(zones);
            m_fetchZonesListCallback(gsdkResult, gsdkZones);
        }
    }

    private CFUtilPoolLib.GSDK.RealNameState GetRealNameState(GSDK.RealNameState realNameState)
    {
        CFUtilPoolLib.GSDK.RealNameState gRealNameState = new CFUtilPoolLib.GSDK.RealNameState();
        gRealNameState.IsVerified = realNameState.IsVerified;
        gRealNameState.NeedParentVerify = realNameState.NeedParentVerify;
        return gRealNameState;
    }

    private CFUtilPoolLib.GSDK.Result GetGSDKResult(GSDK.Result result)
    {
        CFUtilPoolLib.GSDK.Result gsdkResult = new CFUtilPoolLib.GSDK.Result();
        gsdkResult.Error = result.Error;
        gsdkResult.Message = result.Message;
        gsdkResult.ExtraCode = result.ExtraCode;
        gsdkResult.ExtraMessage = result.ExtraMessage;
        gsdkResult.AddtionalInfo = result.AddtionalInfo;
        return gsdkResult;
    }

    private CFUtilPoolLib.GSDK.RealNameAuthResult GetRealNameAuthResult(GSDK.RealNameAuthResult realNameAuthResult)
    {
        if (realNameAuthResult == null) return null;
        CFUtilPoolLib.GSDK.RealNameAuthResult gRealNameAuthResult = new CFUtilPoolLib.GSDK.RealNameAuthResult();
        gRealNameAuthResult.AuthLevel = (CFUtilPoolLib.GSDK.RealNameAuthLevel)realNameAuthResult.AuthLevel;
        gRealNameAuthResult.Age = realNameAuthResult.Age;
        gRealNameAuthResult.AntiAdditionTips = realNameAuthResult.AntiAdditionTips;
        return gRealNameAuthResult;
    }

    private CFUtilPoolLib.GSDK.AccountInfo GetGSDKAccountInfo(GSDK.AccountInfo accountInfo)
    {
        CFUtilPoolLib.GSDK.AccountInfo gsdkAccountInfo = new CFUtilPoolLib.GSDK.AccountInfo();
        if (accountInfo != null)
        {
            gsdkAccountInfo.AccountType = (CFUtilPoolLib.GSDK.AccountType)accountInfo.AccountType;
            gsdkAccountInfo.OpenID = accountInfo.OpenID;
            gsdkAccountInfo.Token = accountInfo.Token;
            gsdkAccountInfo.UserID = accountInfo.UserID;
            gsdkAccountInfo.IsGuest = accountInfo.IsGuest;
            gsdkAccountInfo.LoginTime = accountInfo.LoginTime;
            gsdkAccountInfo.CancelLogoff = accountInfo.CancelLogoff;
            if (accountInfo.BindedUserInfoCollection != null)
            {
                gsdkAccountInfo.BindedUserInfoCollection = new List<CFUtilPoolLib.GSDK.UserDetailInfo>();
                foreach (var item in accountInfo.BindedUserInfoCollection)
                {
                    CFUtilPoolLib.GSDK.UserDetailInfo userDetailInfo = new CFUtilPoolLib.GSDK.UserDetailInfo();
                    userDetailInfo.PassportUserID = item.PassportUserID;
                    userDetailInfo.AccountType = (CFUtilPoolLib.GSDK.AccountType)item.AccountType;
                    userDetailInfo.NickName = item.NickName;
                    userDetailInfo.AvatarUrl = item.AvatarUrl;
                    gsdkAccountInfo.BindedUserInfoCollection.Add(userDetailInfo);
                }
            }
        }
        return gsdkAccountInfo;
    }

    private List<CFUtilPoolLib.GSDK.ZoneInfo> GetGSDKZonesList(List<GSDK.ZoneInfo> zones)
    {
        List<CFUtilPoolLib.GSDK.ZoneInfo> gsdkZones = null;
        if (zones != null)
        {
            gsdkZones = new List<CFUtilPoolLib.GSDK.ZoneInfo>();
            foreach (var item in zones)
            {
                CFUtilPoolLib.GSDK.ZoneInfo zoneInfo = new CFUtilPoolLib.GSDK.ZoneInfo();
                zoneInfo.ZoneName = item.ZoneName;
                zoneInfo.ZoneID = item.ZoneID;
                zoneInfo.ChannelID = item.ChannelID;
                zoneInfo.ExtraInfo = item.ExtraInfo;
                zoneInfo.ExtraKV = null;
                if (item.ExtraKV != null)
                {
                    zoneInfo.ExtraKV = new Dictionary<string, string>();
                    foreach (var item2 in item.ExtraKV)
                    {
                        zoneInfo.ExtraKV.Add(item2.Key, item2.Value);
                    }
                }
                zoneInfo.Servers = null;
                if (item.Servers != null)
                {
                    zoneInfo.Servers = new List<CFUtilPoolLib.GSDK.ServerInfo>();
                    foreach (var item2 in item.Servers)
                    {
                        if (item2 != null)
                        {
                            CFUtilPoolLib.GSDK.ServerInfo serverInfo = GetSDKServerInfo(item2);
                            zoneInfo.Servers.Add(serverInfo);
                        }
                    }
                }
                gsdkZones.Add(zoneInfo);
            }
        }
        return gsdkZones;
    }

    private CFUtilPoolLib.GSDK.ServerInfo GetSDKServerInfo(GSDK.ServerInfo serverInfo)
    {
        CFUtilPoolLib.GSDK.ServerInfo gsdkServerInfo = null;
        if (serverInfo != null)
        {
            gsdkServerInfo = new CFUtilPoolLib.GSDK.ServerInfo();
            gsdkServerInfo.ServerId = serverInfo.ServerId;
            gsdkServerInfo.ServerName = serverInfo.ServerName;
            gsdkServerInfo.ServerType = (CFUtilPoolLib.GSDK.ServerType)serverInfo.ServerType;
            gsdkServerInfo.ServerEntry = serverInfo.ServerEntry;
            gsdkServerInfo.RealServerId = serverInfo.RealServerId;
            gsdkServerInfo.IsMerged = serverInfo.IsMerged;
            gsdkServerInfo.ServerStatus = (CFUtilPoolLib.GSDK.ServerStatus)serverInfo.ServerStatus;
            gsdkServerInfo.OnlineLoad = (CFUtilPoolLib.GSDK.ServerOnlineLoad)serverInfo.OnlineLoad;
            gsdkServerInfo.ExtraInfo = serverInfo.ExtraInfo;
            gsdkServerInfo.Tags = null;
            if (serverInfo.Tags != null)
            {
                gsdkServerInfo.Tags = new List<CFUtilPoolLib.GSDK.ServerTag>();
                foreach (var item in serverInfo.Tags)
                {
                    CFUtilPoolLib.GSDK.ServerTag serverTag = new CFUtilPoolLib.GSDK.ServerTag();
                    serverTag.TagName = item.TagName;
                    serverTag.TagValue = item.TagValue;
                    gsdkServerInfo.Tags.Add(serverTag);
                }
            }
            gsdkServerInfo.OpenTimestamp = serverInfo.OpenTimestamp;
            gsdkServerInfo.ExtraKV = null;
            if (serverInfo.ExtraKV != null)
            {
                gsdkServerInfo.ExtraKV = new Dictionary<string, string>();
                foreach (var item in serverInfo.ExtraKV)
                {
                    gsdkServerInfo.ExtraKV.Add(item.Key, item.Value);
                }
            }
            gsdkServerInfo.PingAddr = serverInfo.PingAddr;
            gsdkServerInfo.Time = serverInfo.Time;
        }
        return gsdkServerInfo;
    }

    private List<CFUtilPoolLib.GSDK.RoleInfo> GetGSDKRolesList(List<GSDK.RoleInfo> roles)
    {
        List<CFUtilPoolLib.GSDK.RoleInfo> gsdkRoles = null;
        if (roles != null)
        {
            gsdkRoles = new List<CFUtilPoolLib.GSDK.RoleInfo>();
            foreach (var item in roles)
            {
                if (item != null)
                {
                    CFUtilPoolLib.GSDK.RoleInfo roleInfo = GetSDKRoleInfo(item);
                    gsdkRoles.Add(roleInfo);
                }
            }
        }
        return gsdkRoles;
    }

    private CFUtilPoolLib.GSDK.RoleInfo GetSDKRoleInfo(GSDK.RoleInfo roleInfo)
    {
        CFUtilPoolLib.GSDK.RoleInfo gsdkRoleInfo = null;
        if (roleInfo != null)
        {
            gsdkRoleInfo = new CFUtilPoolLib.GSDK.RoleInfo();
            gsdkRoleInfo.RoleId = roleInfo.RoleId;
            gsdkRoleInfo.RoleName = roleInfo.RoleName;
            gsdkRoleInfo.RoleLevel = roleInfo.RoleLevel;
            gsdkRoleInfo.RealServerId = roleInfo.RealServerId;
            gsdkRoleInfo.ServerName = roleInfo.ServerName;
            gsdkRoleInfo.LoginTime = roleInfo.LoginTime;
            gsdkRoleInfo.AvatarUrl = roleInfo.AvatarUrl;
            gsdkRoleInfo.Job = roleInfo.Job;
            gsdkRoleInfo.ExtraInfo = roleInfo.ExtraInfo;
        }
        return gsdkRoleInfo;
    }

    private CFUtilPoolLib.GSDK.FinishRecordInfo GetFinishRecordInfo(GSDK.IMVoice.FinishRecordInfo finishRecordInfo)
    {
        CFUtilPoolLib.GSDK.FinishRecordInfo gFinishRecordInfo = new CFUtilPoolLib.GSDK.FinishRecordInfo();
        gFinishRecordInfo.VoiceID = finishRecordInfo.VoiceID;
        gFinishRecordInfo.VoiceDuration = finishRecordInfo.VoiceDuration;
        return gFinishRecordInfo;
    }

    private CFUtilPoolLib.GSDK.PlayInfo GetPlayInfo(GSDK.IMVoice.PlayInfo playInfo)
    {
        CFUtilPoolLib.GSDK.PlayInfo gPlayInfo = new CFUtilPoolLib.GSDK.PlayInfo();
        gPlayInfo.VoiceID = playInfo.VoiceID;
        return gPlayInfo;
    }

    private CFUtilPoolLib.GSDK.TranscribeInfo GetTranscribeInfo(GSDK.IMVoice.TranscribeInfo transcribeInfo)
    {
        CFUtilPoolLib.GSDK.TranscribeInfo gTranscribeInfo = new CFUtilPoolLib.GSDK.TranscribeInfo();
        gTranscribeInfo.VoiceID = transcribeInfo.VoiceID;
        gTranscribeInfo.VoiceContent = transcribeInfo.VoiceContent;
        return gTranscribeInfo;
    }

    private CFUtilPoolLib.GSDK.VoiceFileInfo GetVoiceFileInfo(GSDK.IMVoice.VoiceFileInfo voiceFileInfo)
    {
        CFUtilPoolLib.GSDK.VoiceFileInfo gVoiceFileInfo = new CFUtilPoolLib.GSDK.VoiceFileInfo();
        gVoiceFileInfo.VoiceID = voiceFileInfo.VoiceID;
        gVoiceFileInfo.VoiceLocalFilePath = voiceFileInfo.VoiceLocalFilePath;
        return gVoiceFileInfo;
    }

    private CFUtilPoolLib.GSDK.JoinRoomInfo GetJoinRoomInfo(GSDK.JoinRoomInfo joinRoomInfo)
    {
        CFUtilPoolLib.GSDK.JoinRoomInfo gJoinRoomInfo = new CFUtilPoolLib.GSDK.JoinRoomInfo();
        gJoinRoomInfo.RoomID = joinRoomInfo.RoomID;
        gJoinRoomInfo.UserID = joinRoomInfo.UserID;
        gJoinRoomInfo.Elapsed = joinRoomInfo.Elapsed;
        return gJoinRoomInfo;
    }

    private CFUtilPoolLib.GSDK.OtherUserJoinInfo GetOtherUserJoinInfo(GSDK.OtherUserJoinInfo otherUserJoinInfo)
    {
        CFUtilPoolLib.GSDK.OtherUserJoinInfo gOtherUserJoinInfo = new CFUtilPoolLib.GSDK.OtherUserJoinInfo();
        gOtherUserJoinInfo.UserID = otherUserJoinInfo.UserID;
        gOtherUserJoinInfo.Elapsed = otherUserJoinInfo.Elapsed;
        return gOtherUserJoinInfo;
    }

    private CFUtilPoolLib.GSDK.OtherUserLeaveInfo GetOtherUserLeaveInfo(GSDK.OtherUserLeaveInfo otherUserLeaveInfo)
    {
        CFUtilPoolLib.GSDK.OtherUserLeaveInfo gOtherUserLeaveInfo = new CFUtilPoolLib.GSDK.OtherUserLeaveInfo();
        gOtherUserLeaveInfo.UserID = otherUserLeaveInfo.UserID;
        gOtherUserLeaveInfo.Reason = (CFUtilPoolLib.GSDK.LeaveRoomReason)(otherUserLeaveInfo.Reason);
        return gOtherUserLeaveInfo;
    }

    private CFUtilPoolLib.GSDK.AudioVolumeIndicationInfo GetAudioVolumeIndicationInfo(GSDK.AudioVolumeIndicationInfo audioVolumeIndicationInfo)
    {
        CFUtilPoolLib.GSDK.AudioVolumeIndicationInfo gAudioVolumeIndicationInfo = new CFUtilPoolLib.GSDK.AudioVolumeIndicationInfo();
        gAudioVolumeIndicationInfo.Speakers = new List<CFUtilPoolLib.GSDK.SpeakerInfo>();
        if (audioVolumeIndicationInfo.Speakers != null)
        {
            for (int i = 0; i < audioVolumeIndicationInfo.Speakers.Count; ++i)
            {
                CFUtilPoolLib.GSDK.SpeakerInfo gSpeakerInfo = new CFUtilPoolLib.GSDK.SpeakerInfo();
                gSpeakerInfo.UserID = audioVolumeIndicationInfo.Speakers[i].UserID;
                gSpeakerInfo.Volume = audioVolumeIndicationInfo.Speakers[i].Volume;
                gAudioVolumeIndicationInfo.Speakers.Add(gSpeakerInfo);
            }
        }
        gAudioVolumeIndicationInfo.TotalVolume = audioVolumeIndicationInfo.TotalVolume;
        return gAudioVolumeIndicationInfo;
    }

    private CFUtilPoolLib.GSDK.ProtocolAddressResult GetProtocolAddressResult(GSDK.ProtocolAddressResult result)
    {
        CFUtilPoolLib.GSDK.ProtocolAddressResult gResult = new CFUtilPoolLib.GSDK.ProtocolAddressResult();
        gResult.userAgreementUrl = result.userAgreementUrl;
        gResult.privacyPolicy = result.privacyPolicy;
        gResult.code = result.code;
        gResult.message = result.message;
        return gResult;
    }

    private CFUtilPoolLib.GSDK.updateGameConfigRet GetUpdateGameConfigRet(GMSDK.updateGameConfigRet result)
    {
        CFUtilPoolLib.GSDK.updateGameConfigRet gResult = new CFUtilPoolLib.GSDK.updateGameConfigRet();
        gResult.code = result.code;
        gResult.message = result.message;
        return gResult;
    }

    private CFUtilPoolLib.GSDK.openFaceVerifyRet GetOpenFaceVerifyRet(GMSDK.openFaceVerifyRet result)
    {
        CFUtilPoolLib.GSDK.openFaceVerifyRet gResult = new CFUtilPoolLib.GSDK.openFaceVerifyRet();
        gResult.code = result.code;
        gResult.message = result.message;
        gResult.list = new List<CFUtilPoolLib.GSDK.openFaceData>();
        XDebug.singleton.AddGreenLog("GetOpenFaceVerifyRet info=" + result.ToString());
        if (result.list != null)
        {
            XDebug.singleton.AddGreenLog("GetOpenFaceVerifyRet count=" + result.list.Count);
            for (int i = 0; i < result.list.Count; ++i)
            {
                if (result.list[i] != null)
                {
                    CFUtilPoolLib.GSDK.openFaceData data = new CFUtilPoolLib.GSDK.openFaceData();
                    data.activityUrl = result.list[i].activityUrl;
                    data.activityId = result.list[i].activityId;
                    data.inGameId = result.list[i].inGameId;
                    gResult.list.Add(data);
                }
                else
                {
                    XDebug.singleton.AddGreenLog("GetOpenFaceVerifyRet i=" + i + " is null");
                }
            }
        }
        return gResult;
    }

    private CFUtilPoolLib.GSDK.openPageRet GetOpenPageRet(GMSDK.openPageRet result)
    {
        CFUtilPoolLib.GSDK.openPageRet gResult = new CFUtilPoolLib.GSDK.openPageRet();
        gResult.code = result.code;
        gResult.message = result.message;
        gResult.windowId = result.windowId;
        return gResult;
    }

    private CFUtilPoolLib.GSDK.PageCloseResult GetClosePageRet(GMSDK.PageCloseResult result)
    {
        CFUtilPoolLib.GSDK.PageCloseResult gResult = new CFUtilPoolLib.GSDK.PageCloseResult();
        gResult.code = result.code;
        gResult.type = result.type;
        gResult.windowId = result.windowId;
        gResult.inGameId = result.inGameId;
        gResult.pageType = result.pageType;
        return gResult;
    }

    private CFUtilPoolLib.GSDK.queryActivityNotifyDataRet GetQueryActivityNotifyDataRet(GMSDK.queryActivityNotifyDataRet result)
    {
        CFUtilPoolLib.GSDK.queryActivityNotifyDataRet gResult = new CFUtilPoolLib.GSDK.queryActivityNotifyDataRet();
        gResult.code = result.code;
        gResult.message = result.message;
        gResult.data = new List<CFUtilPoolLib.GSDK.NotifyDataBean>();
        if (result.data != null)
        {
            for (int i = 0; i < result.data.Count; ++i)
            {
                if (result.data[i] != null)
                {
                    CFUtilPoolLib.GSDK.NotifyDataBean item = new CFUtilPoolLib.GSDK.NotifyDataBean();
                    item.id = result.data[i].id;
                    item.inGameId = result.data[i].inGameId;
                    if (result.data[i].notify != null)
                    {
                        item.notify = new CFUtilPoolLib.GSDK.NotifyBean();
                        item.notify.type = result.data[i].notify.type;
                        item.notify.count = result.data[i].notify.count;
                        item.notify.custom = result.data[i].notify.custom;
                        XDebug.singleton.AddGreenLog("GetQueryActivityNotifyDataRet:" + i + "&" + item.notify.type + "&" + item.notify.count + "&" + item.notify.custom);
                    }
                    gResult.data.Add(item);
                }
            }
        }
        return gResult;
    }

    private CFUtilPoolLib.GSDK.GetRNDebugRet GetRNDebugRet(GMSDK.GetRNDebugRet result)
    {
        CFUtilPoolLib.GSDK.GetRNDebugRet gResult = new CFUtilPoolLib.GSDK.GetRNDebugRet();
        gResult.code = result.code;
        gResult.message = result.message;
        gResult.status = result.status;
        return gResult;
    }

    private void ShowUserCenterDelegateCallBack(GSDK.Result result)
    {
        if (m_AccountShowUserCenterDelegate != null)
        {
            CFUtilPoolLib.GSDK.Result gsdkResult = GetGSDKResult(result);
            m_AccountShowUserCenterDelegate(gsdkResult);
        }
    }

    public CFUtilPoolLib.GSDK.AccountInfo GetSDKAccountInfo()
    {
        GSDK.AccountInfo accountInfo = Account.Service.GetLoginRecord();
        return GetGSDKAccountInfo(accountInfo);
    }

    public void ShowUserCenter(CFUtilPoolLib.GSDK.AccountShowUserCenterDelegate showUserCenterDelegate)
    {
        m_AccountShowUserCenterDelegate = showUserCenterDelegate;
        Account.Service.ShowUserCenter(ShowUserCenterDelegateCallBack);
    }

    public void OpenWeb(string url, string title, CFUtilPoolLib.GSDK.WebviewShowDelegate webviewShowDelegate = null, CFUtilPoolLib.GSDK.WebviewExitDelegate webviewExitDelegate = null, CFUtilPoolLib.GSDK.WebviewCustomParamDelegate webviewCustomParamDelegate = null)
    {
        m_webviewShowCallback = webviewShowDelegate;
        m_webviewExitCallback = webviewExitDelegate;
        GSDK.WebviewParameter webviewParameter = new GSDK.WebviewParameter(title, url);
        webviewParameter.Orientation = GSDK.WebOrientation.Landscape; //强制横屏模式
        Webview.Service.Open(webviewParameter, WebviewShowCallback, WebviewExitCallback);
    }

    private void WebviewShowCallback(GSDK.Result result)
    {
        if (m_webviewShowCallback != null)
        {
            CFUtilPoolLib.GSDK.Result gResult = GetGSDKResult(result);
            m_webviewShowCallback(gResult);
            m_webviewShowCallback = null;
        }

        if (result.IsSuccess)
        {
            XDebug.singleton.AddGreenLog("Show WebView Success");
        }
        else if (result.Error == GSDK.ErrorCode.WebviewParameterError)
        {
            XDebug.singleton.AddGreenLog("Webview Parameter Error");
        }
        else if (result.Error == GSDK.ErrorCode.WebviewURLEmptyError || result.Error == GSDK.ErrorCode.WebviewURLError)
        {
            XDebug.singleton.AddGreenLog("Webview URL Error");
        }
        else
        {
            XDebug.singleton.AddGreenLog("Web view Show Error");
        }
    }

    private void WebviewExitCallback()
    {
        if (m_webviewExitCallback != null)
        {
            m_webviewExitCallback();
            m_webviewExitCallback = null;
        }
    }


    #region 实名认证

    public void FetchRealNameState(CFUtilPoolLib.GSDK.FetchRealNameStateDelegate callback)
    {
        m_accountFetchRealNameStateCallback = callback;
        RealName.Service.FetchRealNameState(CheckHasAuthRealNameStateCallback);
    }

    public void AuthRealNameWithUI(CFUtilPoolLib.GSDK.RealNameAuthType authType, CFUtilPoolLib.GSDK.ComplianceRealNameAuthResultDelegate callback)
    {
        m_accountRealNameAuthCallback = callback;
        RealName.Service.ComplianceRealNameAuth((code, message) =>
        {
            m_accountRealNameAuthCallback(code, message);
        });
    }

    private void CheckHasAuthRealNameStateCallback(GSDK.Result result, GSDK.RealNameState realNameState)
    {
        if (m_accountFetchRealNameStateCallback != null)
        {
            CFUtilPoolLib.GSDK.Result gResult = GetGSDKResult(result);
            CFUtilPoolLib.GSDK.RealNameState gRealNameState = GetRealNameState(realNameState);
            m_accountFetchRealNameStateCallback(gResult, gRealNameState);
        }
    }

    void IGSDKSystem.FetchServiceAntiAddictionStatus(CFUtilPoolLib.GSDK.AntiAddictionStatusEventHandler callback)
    {
        m_antiAddictionCallback = callback;
        AntiAddiction.Service.AntiAddictionStatusEvent += AntiAddictionStatusEventHandler; //全局的防沉迷回调，不需要-=，sdk系统每隔一定时间会触发
        AntiAddiction.Service.FetchServiceAntiAddictionStatus(AntiAddictionStatusEventHandler);
    }

    private void AntiAddictionStatusEventHandler(GSDK.AntiAddictionInfo antiAddictionInfo)
    {
        //AntiAddiction.Service.AntiAddictionStatusEvent -= AntiAddictionStatusEventHandler; //不要-=，这是全局的防沉迷回调函数
        if (m_antiAddictionCallback != null)
        {
            CFUtilPoolLib.GSDK.AntiAddictionInfo gAntiAddictionInfo = new CFUtilPoolLib.GSDK.AntiAddictionInfo();
            gAntiAddictionInfo.operation = (CFUtilPoolLib.GSDK.AntiAddictionOperation)antiAddictionInfo.operation;
            gAntiAddictionInfo.message = antiAddictionInfo.message;

            if (Application.platform == RuntimePlatform.WindowsPlayer) //临时写法，周三注释掉
            {
                gAntiAddictionInfo.operation = CFUtilPoolLib.GSDK.AntiAddictionOperation.Ignore;
                gAntiAddictionInfo.message = string.Empty;
            }
            m_antiAddictionCallback(gAntiAddictionInfo);
        }
    }
    #endregion
    public void SdkTrackEvent(string eventName, JsonData eventParams, bool includeCommonParams = true)
    {
        UNBridgeLib.LitJson.JsonData jsonData = new UNBridgeLib.LitJson.JsonData();
        foreach (var item in eventParams.m_commonParams)
        {
            object o = ConvertValueType(item.Value);
            UNBridgeLib.LitJson.JsonData tempJson = new UNBridgeLib.LitJson.JsonData(o);
            jsonData[item.Key] = tempJson;
        }
        foreach (var item in eventParams.m_params)
        {
            object o = ConvertValueType(item.Value);
            UNBridgeLib.LitJson.JsonData tempJson = new UNBridgeLib.LitJson.JsonData(o);
            jsonData[item.Key] = tempJson;
        }
        GMSDKMgr.instance.SDK.SdkTrackEvent(eventName, jsonData);
    }

    public object ConvertValueType(object obj)
    {
        if (obj == null)
        {
            XDebug.singleton.AddErrorLog("ConvertValueType obj is null");
        }

        if (obj is Boolean)
        {
            return (bool)obj;
        }

        if (obj is Double)
        {
            return (double)obj;
        }

        if (obj is Int32)
        {
            return (int)obj;
        }

        if (obj is Int64)
        {
            return (long)obj;
        }

        if (obj is String)
        {
            return (string)obj;
        }

        // add other types
        if (obj is float)
        {
            obj = double.Parse(obj.ToString());
            return obj;
        }

        if (obj is UInt32 || obj is Char || obj is Byte)
        {
            obj = int.Parse(obj.ToString());
            return obj;
        }

        if (obj is UInt64)
        {
            obj = long.Parse(obj.ToString());
            return obj;
        }

        throw new ArgumentException("Unable to wrap the given object with JsonData");
    }
    #region 兑换
    public void RedeemGift(string roleId, int serverId, string cdKey, CFUtilPoolLib.GSDK.CdKeyDelegate callback)
    {
        m_CdKeyDelegate = callback;
        GSDK.CdKey.Service.RedeemGift(roleId, serverId, cdKey, CdKeyDelegateFunc);
    }
    private void CdKeyDelegateFunc(GSDK.Result result)
    {
        if (m_CdKeyDelegate != null)
        {
            CFUtilPoolLib.GSDK.Result gsdkResult = GetGSDKResult(result);
            m_CdKeyDelegate(gsdkResult);
        }

    }

    public bool GetAgreePrivacy()
    {
        return GMSDKMgr.instance.SDK.SdkCheckHasAgreePrivacy();
    }

    public void EnableSandboxMode(bool enable)
    {
        GMSDKMgr.instance.SDK.SdksetSandboxEnable(enable);
    }

    public bool SdkIsSandboxEnable()
    {
        return GMSDKMgr.instance.SDK.SdkIsSandboxEnable();
    }
    #endregion


    public int InitializeAgreement(byte[] configurationData, uint configurationLength, CFUtilPoolLib.GSDK.AgreementCryptoType agreementCryptoMethod)
    {
        if (m_agreementService == null) m_agreementService = Agreement.Service;
        return m_agreementService.Initialize(configurationData, configurationLength, (GSDK.AgreementCryptoType)agreementCryptoMethod);
    }

    public void ReleaseAgreement()
    {
        if (m_agreementService != null)
        {
            m_agreementService.Release();
        }
    }

    public int Encrypt(byte[] inputData, uint inputLength, byte[] outputBuffer, ref uint outputBufferMaxSizes)
    {
        if (m_agreementService == null) m_agreementService = Agreement.Service;
        return m_agreementService.Encrypt(inputData, inputLength, outputBuffer, ref outputBufferMaxSizes);
    }

    public int Decrypt(byte[] inputData, uint inputLength, byte[] outputBuffer, ref uint outputBufferMaxSizes)
    {
        if (m_agreementService == null) m_agreementService = Agreement.Service;
        return m_agreementService.Decrypt(inputData, inputLength, outputBuffer, ref outputBufferMaxSizes);
    }

    public void SdkLogException(string name, string reason, string stack)
    {
        GMSDKMgr.instance.SDK.SdkLogException(name, reason, stack);
    }

    #region 聚合SDK

    public void RegisterAccountStatusChangedListener()
    {
#if UNITY_ANDROID
        Channel.Service.RegisterAccountStatusChangedListener();
        //登出回调事件       
        Channel.Service.LogoutChannelEvent += ChannelService_LogoutChannelEvent;

        Channel.Service.SwitchAccountChannelEvent += ChannelService_SwitchChannelEvent;
        //游戏退出事件
        Channel.Service.ExitChannelEvent += ChannelService_ExitChannelEvent;
#endif
    }

    private void ChannelService_LogoutChannelEvent(GSDK.Result result)
    {
        if (result.IsSuccess)
        {
            //登出账号
            GSDKManager.singleton.LogoutChannelCallback();
        }
    }

#if UNITY_ANDROID

    private void ChannelService_SwitchChannelEvent(GSDK.LoginResult result)
    {
        //切换账号
        GSDKManager.singleton.SwitchAccountChannelCallback();
    }

    private void ChannelService_ExitChannelEvent(GSDK.ExitResult result)
    {
        if (result.IsExit)
        {
            //退出游戏
            GSDKManager.singleton.OnExitChannelCallback();
        }
    }

#endif

    public void SdkOnExit()
    {
#if UNITY_ANDROID
        Channel.Service.ExitGame(this.ChannelService_ExitGameEvent);
#endif
    }

#if UNITY_ANDROID
    private void ChannelService_ExitGameEvent(GSDK.Result result, GSDK.ExitGameInfo exitGameInfo)
    {
        GSDKManager.singleton.SdkOnExitCallBack(exitGameInfo.HasDialog);
    }
#endif

    //创建角色
    public void CreateNewRoleUpload(string type, string zoneid, string zonename, string roleid, string rolename, string level)
    {
#if UNITY_ANDROID
        if (cpUploadInfo == null)
        {
            cpUploadInfo = new GameUploadInfo();
            InitRoleInfo();
        }

        cpUploadInfo.Type = type;
        cpUploadInfo.ZoneId = zoneid;
        cpUploadInfo.ZoneName = zonename;
        cpUploadInfo.RoleId = roleid;
        cpUploadInfo.RoleLevel = level;
        Channel.Service.CreateNewRoleUpload(cpUploadInfo);
#endif

    }

    //角色升级
    public void RoleLevelUpload(string type, string zoneid, string zonename, string roleid, string rolename, string level)
    {
#if UNITY_ANDROID
        if (cpUploadInfo == null)
        {
            cpUploadInfo = new GameUploadInfo();
            InitRoleInfo();
        }
        cpUploadInfo.Type = type;
        cpUploadInfo.ZoneId = zoneid;
        cpUploadInfo.ZoneName = zonename;
        cpUploadInfo.RoleId = roleid;
        cpUploadInfo.RoleLevel = level;
        Channel.Service.RoleLevelUpload(cpUploadInfo);
#endif
    }

    //进入游戏
    public void EnterGameUpload(string type, string zoneid, string zonename, string roleid, string rolename, string level)
    {
#if UNITY_ANDROID
        if (cpUploadInfo == null)
        {
            cpUploadInfo = new GameUploadInfo();
            InitRoleInfo();
        }
        cpUploadInfo.Type = type;
        cpUploadInfo.ZoneId = zoneid;
        cpUploadInfo.ZoneName = zonename;
        cpUploadInfo.RoleId = roleid;
        cpUploadInfo.RoleLevel = level;

        Channel.Service.EnterGameUpload(cpUploadInfo);
#endif
    }

    //角色退出
    public void RoleExitUpload(string type, string zoneid, string zonename, string roleid, string rolename, string level)
    {
#if UNITY_ANDROID
        if (cpUploadInfo == null)
        {
            cpUploadInfo = new GameUploadInfo();
            InitRoleInfo();
        }
        cpUploadInfo.Type = type;
        cpUploadInfo.ZoneId = zoneid;
        cpUploadInfo.ZoneName = zonename;
        cpUploadInfo.RoleId = roleid;
        cpUploadInfo.RoleLevel = level;
        Channel.Service.RoleExitUpload(cpUploadInfo);
#endif
    }

    private void InitRoleInfo()
    {
#if UNITY_ANDROID

        this.cpUploadInfo.Type = "enterServer"; //（必填）角色状态（enterServer（登录），levelUp（升级），createRole（创建角色），exitServer（退出））
        this.cpUploadInfo.ZoneId = "2";  //（必填）游戏区服ID
        this.cpUploadInfo.ZoneName = "测试服";  //（必填）游戏区服名称
        this.cpUploadInfo.RoleId = "123456";  //（必填）玩家角色ID
        this.cpUploadInfo.RoleName = "冷雨夜风";  //（必填）玩家角色名
        this.cpUploadInfo.ProfessionId = "1";  //（必填）职业ID
        this.cpUploadInfo.Profession = "战士";  //（必填）职业名称
        this.cpUploadInfo.Gender = "男";  //（必填）性别
        this.cpUploadInfo.ProfessionRoleId = "0";  //（选填）职业称号ID
        this.cpUploadInfo.ProfessionRoleName = "无";  //（选填）职业称号
        this.cpUploadInfo.RoleLevel = "30";  //（必填）玩家角色等级
        this.cpUploadInfo.Power = "120000";  //（必填）战力数值
        this.cpUploadInfo.Vip = "6";  //（必填）当前用户VIP等级
        this.cpUploadInfo.PartyId = "100";  //（必填）所属帮派帮派ID
        this.cpUploadInfo.PartyName = "王者依旧";  //（必填）所属帮派名称
        this.cpUploadInfo.PartyRoleId = "1";  //（必填）帮派称号ID
        this.cpUploadInfo.PartyRoleName = "会长";  //（必填）帮派称号名称
        this.cpUploadInfo.Chapter = "001"; //(必填)游戏关卡
        this.cpUploadInfo.ServerId = "0001"; //服务器Id(小米渠道需要)
        this.cpUploadInfo.ServerName = "国服1"; //服务器Id名称(小米渠道需要)
#endif
    }

    public bool IsApiAvailable(string apiName)
    {
        return GMSDKMgr.instance.SDK.SDKIsAvailable(apiName);
    }

    #endregion


    #region 老实时语音模块

    public void RTCInitialize(CFUtilPoolLib.GSDK.ClientRole clientRole = CFUtilPoolLib.GSDK.ClientRole.Broadcaster)
    {
        RTC.Service.Initialize((GSDK.ClientRole)clientRole);
    }

    public void RTCJoinRoom(string roomID, string userID, string token, CFUtilPoolLib.GSDK.JoinRoomDelegate joinRoomCallback)
    {
        m_joinRoomCallback = joinRoomCallback;
        RTC.Service.JoinRoom(roomID, userID, token, RTCJoinRoomDelegateCallback);
    }

    private void RTCJoinRoomDelegateCallback(GSDK.Result result, GSDK.JoinRoomInfo joinRoomInfo)
    {
        if (m_joinRoomCallback != null)
        {
            CFUtilPoolLib.GSDK.Result gResult = GetGSDKResult(result);
            CFUtilPoolLib.GSDK.JoinRoomInfo gJoinRoomInfo = GetJoinRoomInfo(joinRoomInfo);
            m_joinRoomCallback(gResult, gJoinRoomInfo);
        }
    }

    public void RTCLeaveRoom()
    {
        RTC.Service.LeaveRoom();
    }

    public void RTCMuteRecvAudioStream(string userID, bool mute)
    {
        RTC.Service.MuteRecvAudioStream(userID, mute);
    }

    public void RTCMuteSendAudioStream(bool mute)
    {
        RTC.Service.MuteSendAudioStream(mute);
    }

    public void RTCMuteAllRecvAudioStreams(bool mute)
    {
        RTC.Service.MuteAllRecvAudioStreams(mute);
    }

    public void RTCRegisterLeaveRoomEvent(CFUtilPoolLib.GSDK.LeaveRoomEventHandler leaveRoomEventHandler)
    {
        m_leaveRoomEventCallback = leaveRoomEventHandler;
        RTC.Service.LeaveRoomEvent += LeaveRoomEventCallback;
    }

    private void LeaveRoomEventCallback()
    {
        if (m_leaveRoomEventCallback != null)
        {
            m_leaveRoomEventCallback();
        }
    }

    public void RTCRegisterOtherUserJoinEventHandler(CFUtilPoolLib.GSDK.OtherUserJoinEventHandler otherUserJoinEventHandler)
    {
        m_otherUserJoinEventCallback = otherUserJoinEventHandler;
        RTC.Service.OtherUserJoinEvent += OtherUserJoinEventCallback;
    }

    private void OtherUserJoinEventCallback(GSDK.OtherUserJoinInfo otherUserJoinInfo)
    {
        if (m_otherUserJoinEventCallback != null)
        {
            CFUtilPoolLib.GSDK.OtherUserJoinInfo gOtherUserJoinInfo = GetOtherUserJoinInfo(otherUserJoinInfo);
            m_otherUserJoinEventCallback(gOtherUserJoinInfo);
        }
    }

    public void RTCRegisterOtherUserLeaveEventHandler(CFUtilPoolLib.GSDK.OtherUserLeaveEventHandler otherUserLeaveEventHandler)
    {
        m_otherUserLeaveEventCallback = otherUserLeaveEventHandler;
        RTC.Service.OtherUserLeaveEvent += OtherUserLeaveEventCallback;
    }

    public void RTCRegisterAudioVolumeIndicationEventHandler(CFUtilPoolLib.GSDK.AudioVolumeIndicationEventHandler audioVolumeIndicationEventHandler)
    {
        m_audioVolumeIndicationEventCallback = audioVolumeIndicationEventHandler;
        RTC.Service.AudioVolumeIndicationEvent += AudioVolumeIndicationEventHandler;
    }

    private void AudioVolumeIndicationEventHandler(GSDK.AudioVolumeIndicationInfo audioVolumeIndicationInfo)
    {
        if (m_audioVolumeIndicationEventCallback != null)
        {
            CFUtilPoolLib.GSDK.AudioVolumeIndicationInfo gAudioVolumeIndicationInfo = GetAudioVolumeIndicationInfo(audioVolumeIndicationInfo);
            m_audioVolumeIndicationEventCallback(gAudioVolumeIndicationInfo);
        }
    }

    private void OtherUserLeaveEventCallback(GSDK.OtherUserLeaveInfo otherUserLeaveInfo)
    {
        if (m_otherUserLeaveEventCallback != null)
        {
            CFUtilPoolLib.GSDK.OtherUserLeaveInfo gOtherUserLeaveInfo = GetOtherUserLeaveInfo(otherUserLeaveInfo);
            m_otherUserLeaveEventCallback(gOtherUserLeaveInfo);
        }
    }

    public void RTCUnRegisterLeaveRoomEvent()
    {
        m_leaveRoomEventCallback = null;
        RTC.Service.LeaveRoomEvent -= LeaveRoomEventCallback;
    }

    public void RTCUnRegisterOtherUserJoinEventHandler()
    {
        m_otherUserJoinEventCallback = null;
        RTC.Service.OtherUserJoinEvent -= OtherUserJoinEventCallback;
    }

    public void RTCUnRegisterOtherUserLeaveEventHandler()
    {
        m_otherUserLeaveEventCallback = null;
        RTC.Service.OtherUserLeaveEvent -= OtherUserLeaveEventCallback;
    }

    public void RTCUnRegisterAudioVolumeIndicationEventHandler()
    {
        m_audioVolumeIndicationEventCallback = null;
        RTC.Service.AudioVolumeIndicationEvent -= AudioVolumeIndicationEventHandler;
    }

    #endregion

    #region 本地推送

    public void SendNotificationOnce(CFUtilPoolLib.GSDK.PushContent content, CFUtilPoolLib.GSDK.PushDate pushDate, CFUtilPoolLib.GSDK.PushTime pushTime)
    {
        CFUtilPoolLib.XDebug.singleton.AddLog("SendNotificationOnce");
        GSDK.PushContent cont = new GSDK.PushContent(content.Title, content.Content);
        cont.SubTitle = content.SubTitle;
        cont.Identifier = content.Identifier;
        cont.UserInfo = content.UserInfo;

        GSDK.PushDate date = new GSDK.PushDate(pushDate.Year, pushDate.Month, pushDate.Day);
        GSDK.PushTime time = new GSDK.PushTime(pushTime.Hour, pushTime.Minute, pushTime.Second);

        var notification = OneTimeNotification.CreateOncePush(cont, date, time);
        Push.Service.SendNotification(notification, (result, identifier) =>
        {
            CFUtilPoolLib.XDebug.singleton.AddLog("SendNotificationOnce result.IsSuccess:" + result.IsSuccess);

            if (result.IsSuccess)
            {
                // 推送设置成功
            }
            else
            {
                // 请参考错误码汇总
            }
        });
    }

    /// <summary>
    /// 重复推送
    /// </summary>
    /// <param name="type">推送类型</param>
    /// <param name="content">内容</param>
    /// <param name="pushDate">日期</param>
    /// <param name="pushTime">时间</param>
    /// <param name="weekDayCount">RepeatType.WeekDayOfMonth时使用，表示第几周</param>
    /// <param name="dayCount">RepeatType.Weekly时为每周的第几天，RepeatType.DayOfMonth时为每月的第几天，RepeatType.WeekDayOfMonth时为每个月的第几周的第几天</param>
    /// <param name="timeInterval">RepeatType.Once时使用，表示重复间隔的秒数</param>
    /// <param name="isRepeat">RepeatType.Once时使用，表示是否重复</param>
    public void SendNotificationRepeat(CFUtilPoolLib.GSDK.RepeatType type, CFUtilPoolLib.GSDK.PushContent content, CFUtilPoolLib.GSDK.PushDate pushDate, CFUtilPoolLib.GSDK.PushTime pushTime, int weekDayCount = 0, int dayCount = 0, int timeInterval = 10, bool isRepeat = false)
    {
        GSDK.PushContent cont = new GSDK.PushContent(content.Title, content.Content);
        cont.SubTitle = content.SubTitle;
        cont.Identifier = content.Identifier;
        cont.UserInfo = content.UserInfo;

        GSDK.PushDate date = new GSDK.PushDate(pushDate.Year, pushDate.Month, pushDate.Day);
        GSDK.PushTime time = new GSDK.PushTime(pushTime.Hour, pushTime.Minute, pushTime.Second);

        RepeatNotification notification = null;

        switch (type)
        {
            case CFUtilPoolLib.GSDK.RepeatType.Daily:
                //每天重复
                notification = RepeatNotification.CreateDailyRepeat(cont, time);
                break;
            case CFUtilPoolLib.GSDK.RepeatType.Weekly:
                //每周第几天重复
                notification = RepeatNotification.CreateWeeklyRepeat(cont, dayCount, time);
                break;
            case CFUtilPoolLib.GSDK.RepeatType.DayOfMonth:
                //每个月的第几天重复
                notification = RepeatNotification.CreateMonthlyRepeat(cont, dayCount, time);
                break;
            case CFUtilPoolLib.GSDK.RepeatType.WeekDayOfMonth:
                //每个月的第几周的第几天重复
                notification = RepeatNotification.CreateMonthlyRepeat(cont, weekDayCount, dayCount, time);
                break;
            case CFUtilPoolLib.GSDK.RepeatType.Once:
                notification = RepeatNotification.CreateIntervalRepeat(cont, timeInterval, isRepeat);
                break;
        }


        Push.Service.SendNotification(notification, (result, identifier) =>
        {
            if (result.IsSuccess)
            {
                // 推送设置成功
            }
            else
            {
                // 请参考错误码汇总
            }
        });
    }

    public void RemoveNotification(string identifier)
    {
        Push.Service.RemoveNotification(identifier);
    }

    public void RemoveAllNotification()
    {
        Push.Service.RemoveAllNotification();
    }

    #endregion

    #region 位置

    public void FetchLocationInfoByIp(CFUtilPoolLib.GSDK.LocationFetchLocationInfoByIpDelegate fetchLocationInfoByIpCallback)
    {
        // 通过回调返回定位信息
        Location.Service.FetchLocationInfoByIp((result, location) =>
        {
            if (result.IsSuccess)
            {
                // 返回正确，可直接使用locationInfo的定位数据
                var newInfo = ConvertLocationInfo(location);
                fetchLocationInfoByIpCallback.Invoke(newInfo);
            }
            else
            {
                // IP定位出错，可能是无网络问题  请联系技术接口人排查
                CFUtilPoolLib.XDebug.singleton.AddLog(string.Format("FetchLocationInfoByIp result.Error : {0}  result.Message:{1}", result.Error.ToString(), result.Message));
            }
        });
    }

    public void FetchAccurateLocation(CFUtilPoolLib.GSDK.LocationFetchAccurateLocationDelegate callback)
    {
        CFUtilPoolLib.XDebug.singleton.AddLog("FetchAccurateLocation1");
        // 通过回调返回定位信息
        Location.Service.FetchAccurateLocation((result, info) =>
        {
            if (result.IsSuccess)
            {
                CFUtilPoolLib.XDebug.singleton.AddLog("FetchAccurateLocation2");
                // 返回正确，可直接使用locationInfo的定位数据
                var newInfo = ConvertLocationInfo(info);
                callback.Invoke(newInfo);
            }
            else
            {
                // 若出现，可将result.Error和result.Message打印出来用于定位问题，详情见本文「错误码汇总」
                CFUtilPoolLib.XDebug.singleton.AddLog(string.Format("FetchAccurateLocation result.Error : {0}  result.Message:{1}", result.Error.ToString(), result.Message));
            }
        });
    }

    private CFUtilPoolLib.GSDK.LocationInfo ConvertLocationInfo(GSDK.LocationInfo info)
    {
        CFUtilPoolLib.GSDK.LocationInfo location = new CFUtilPoolLib.GSDK.LocationInfo();
        location.Country = info.Country;
        location.CountryASCII = info.CountryASCII;
        location.CountryCode = info.CountryCode;
        location.AdministrativeArea = info.AdministrativeArea;
        location.AdministrativeAreaASCII = info.AdministrativeAreaASCII;
        location.City = info.City;
        location.District = info.District;
        location.DistrictASCII = info.DistrictASCII;
        location.Latitude = info.Latitude;
        location.Longitude = info.Longitude;
        location.CountryGeoNameId = info.CountryGeoNameId;
        location.AdministrativeAreaGeoNameId = info.AdministrativeAreaGeoNameId;
        location.CityGeoNameId = info.CityGeoNameId;
        location.DistrictGeoNameId = info.DistrictGeoNameId;
        location.IsDisputed = info.IsDisputed;

        return location;
    }

    #endregion


    public void CheckEmulator(CFUtilPoolLib.GSDK.SystemCheckEmulatorDelegate emulatorDelegate)
    {
        m_systemCheckEmulatorCallback = emulatorDelegate;
#if UNITY_ANDROID
        GSDK.System.Service.CheckEmulator(SystemCheckEmulatorDelegate);
#endif
    }

    private void SystemCheckEmulatorDelegate(GSDK.Result result, bool isEmulator, string emulatorBrand)
    {
        if (m_systemCheckEmulatorCallback != null)
        {
            CFUtilPoolLib.GSDK.Result resultSelf = GetGSDKResult(result);
            m_systemCheckEmulatorCallback(resultSelf, isEmulator, emulatorBrand);
            CFUtilPoolLib.XDebug.singleton.AddLog(string.Format("CheckEmulator result.Error : {0}  result.Message:{1} isEmulator:{2} emulatorBrand:{3}",
                result.Error.ToString(),
                result.Message,
                isEmulator.ToString(),
                emulatorBrand));
        }
    }

    public void TestOpenUserAndPrivacyPanel()
    {
        XDebug.singleton.AddGreenLog("TestOpenUserAndPrivacyPanel" + Compliance.Service.Privacy.HasAgreePrivacy);
        Privacy.Service.ShowPrivacy();
    }

    public void ReportPatchVersion(string version)
    {
        XDebug.singleton.AddGreenLog("ReportPatchVersion version=" + version);
        Report.Service.Monitor.ReportPatchVersion(version);
    }

    /// <summary>
    /// 获取用户协议和隐私协议是否更新
    /// </summary>
    /// <returns></returns>
    public bool IsProtocolUpdated()
    {
        bool update = Compliance.Service.Protocol.IsProtocolUpdated();
        XDebug.singleton.AddGreenLog("IsProtocolUpdated update=" + update);
        return update;
    }

    /// <summary>
    /// 获取用户协议和隐私协议的最新url
    /// </summary>
    /// <param name="callback"></param>
    public void SdkProtocolAddress(Action<CFUtilPoolLib.GSDK.ProtocolAddressResult> callback)
    {
        m_protocolAddressResultCallback = callback;
        Compliance.Service.Protocol.SdkProtocolAddress(SdkProtocolAddressCallback);
    }

    public void SdkProtocolAddressCallback(GSDK.ProtocolAddressResult result)
    {
        XDebug.singleton.AddGreenLog("SdkProtocolAddressCallback=" + result.ToString());
        if (m_protocolAddressResultCallback != null)
        {
            CFUtilPoolLib.GSDK.ProtocolAddressResult gProtocolAddressResult = GetProtocolAddressResult(result);
            m_protocolAddressResultCallback(gProtocolAddressResult);
        }
    }

    /// <summary>
    /// 扫码功能
    /// </summary>
    /// <param name="type"></param>
    /// <param name="callback"></param>
    public void Scan(CFUtilPoolLib.GSDK.QRCodeType type, CFUtilPoolLib.GSDK.ScanQRCodeDelegate callback)
    {
        m_scanQRCodeCallback = callback;
        QRCode.Service.Scan((GSDK.QRCodeType)type, ScanQRCodeCallback);
    }

    public void ScanQRCodeCallback(GSDK.Result result, string scanResult)
    {
        if (m_scanQRCodeCallback != null)
        {
            CFUtilPoolLib.GSDK.Result gResult = GetGSDKResult(result);
            m_scanQRCodeCallback(gResult, scanResult);
        }
    }
}
