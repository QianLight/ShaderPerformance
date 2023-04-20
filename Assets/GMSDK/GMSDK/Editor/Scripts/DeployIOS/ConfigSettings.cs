using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UNBridgeLib.LitJson;

public class ConfigModel
{
    public string bundleId = "";
    public string appId = "";
    public string appDisplayName = "";
    public string appName = "";
    public bool importMagicBox = false;
    public double gsdk_init_timeout = 15;
    public bool dsymAutoUpload = true;
    public string dsymUploadKey = "";

    public bool needModifyInfoPlist = true;

	public bool modifyInfoPlistReplaceWhenMetSame = true;

    public bool usePacketChannel = true;

    public string projectTargetName = "Unity-iPhone";
    public string frameworkTargetName = "UnityFramework";

    public int AssociatedDomainsListSize;
    public List<string> AssociatedDomainsList;

    public bool iOSCertSuccess = false;
    public string iOSProvisioningGUID = "";
    public string iOSCertName = "";
    public string iOSTeamID = "";

    public bool iOSDebugCertSuccess = false;
    public string iOSDebugProvisioningGUID = "";
    public string iOSDebugCertName = "";
    public string iOSDebugTeamID = "";

    public bool toutiaoLoginAvailable = false;
    public string toutiaoLoginPlatformId = "";
    public string toutiaoLoginKey = "";
    public bool toutiaoFriendPermissionAvailable = false;

    public bool douyinLoginAvailable = false;
    public string douyinLoginPlatformId = "";
    public string douyinLoginKey = "";
    public bool douyinFriendPermissionAvailable = false;
    public bool douyinRelationFollowPermissionAvailable = false;
    public bool douyinRelationFollowDefaultCheckPermissionAvailable = false;
    public bool douyinVideoPermissionAvailable = false;
    public bool douyinRealNameAuthPermissionAvailable = false;
    public bool douyinMobilePermissionAvailable = false;
    public bool douyinFriendListPermissionAvailable = false;

    public bool appleLoginAvailable = false;
    public string appleLoginPlatformId = "";

    public bool huoShanLoginAvailable = false;
    public string huoShanLoginPlatformId = "";
    public string huoShanLoginKey = "";

    public bool xiGuaLoginAvailable = false;
    public string xiGuaLoginPlatformId = "";
    public string xiGuaLoginKey = "";
    
    public string phoneMobileKey = "";
    public string phoneMobileSecret = "";

    public string phoneTelecomKey = "";
    public string phoneTelecomSecret = "";

    public string phoneUnionKey = "";
    public string phoneUnionSecret = "";

    public bool pushAvailable = false;

    public bool privacyMicrophoneUsageAvailable = false;
    public bool privacyLocationUsageAvailable = true;
    public bool privacyPhotoUsageAvailable = true;
    public bool privacyPhotoAdditionsAvailable = true;
    public bool privacyCameraUsageAvailable = false;
    public bool privacyContactsUsageAvailable = false;
    public bool privacyAppTrackingUsageAvailable = true;
    public bool privacyLocationAlwaysUsageAvailable = false;
    public bool privacyCalendarsUsageAvailable = false;
    public bool privacyRemindersUsageaAvailable = false;
    public bool privacySpeechRecognitionUsageAvailable = false;
    public bool privacySiriUsageAvailable = false;
    public bool privacyFaceIDUsageAvailable = false;

    public string privacyMicrophoneUsageDesc = "申请麦克风权限，用于发送语音消息";
    public string privacyLocationUsageDesc = "申请定位权限，用于必要情况下获取粗略定位";
    public string privacyPhotoUsageDesc = "申请访问相册，用于社交分享";
    public string privacyPhotoAdditionsDesc = "申请存储图片到相册，用于社交分享";
    public string privacyCameraUsageDesc = "申请拍照权限，用于社交分享";
    public string privacyContactsUsageDesc = "需要您的通讯录权限，进行好友推荐";
    public string privacyAppTrackingUsageDesc = "请放心，我们无法获取你在其他应用内的隐私，开启权限仅用于标识设备并保障服务安全与提升游戏体验";
    public string privacyLocationAlwaysUsageDesc = "申请定位权限，用于必要情况下获取连续定位";
    public string privacyCalendarsUsageDesc = "申请日历权限，用于设置活动日程";
    public string privacyRemindersUsageDesc = "申请备忘录权限，用于设置活动提醒";
    public string privacySpeechRecognitionUsageDesc = "申请语音识别权限，以启用系统的语音识别功能";
    public string privacySiriUsageDesc = "申请Siri权限，以启用Siri辅助功能";
    public string privacyFaceIDUsageDesc = "申请Face ID权限，以启用Face ID功能";

    public string sharePanelID = "";
    public bool shareDimAmountAvailable = false;
    public string shareDimAmount = "";
    public bool shareQQAvailable = false;
    public string shareQQKey = "";
    public string shareQQUniversalLink = "";
    public bool shareQZoneAvailable = false;
    public string shareQZoneKey = "";
    public string shareQZoneUniversalLink = "";
    public bool shareWeChatAvailable = false;
    public string shareWeChatKey = "";
    public string shareWeChatUniversalLink = "";
    public bool shareWeChatTLAvailable = false;
    public string shareWeChatTLKey = "";
    public string shareWeChatTLUniversalLink = "";
    public bool shareWeiboAvailable = false;
    public string shareWeiboKey = "";
    public bool shareAWEAvailable = false;
    public string shareAWEKey = "";
    public bool shareAWEIMAvailable = false;
    public string shareAWEIMKey = "";


    public bool replayClientProcessAvailable = false;
    public string replayAppId = "";
    public string replayLicense = "";
    public string ownURLScheme = "";

    public string IMAppID = "";
    public string IMFrontierMethodKey = "";
    public string IMFrontierServiceKey = "";
    public string IMFrontierBOEBaseURL = "";
    public string IMFrontierBaseURL = "";
    public string IMInboxes = "";
    public string IMFrontierID = "";
    public string IMHTTPBaseURL = "";
    public string IMHTTPBOEBaseURL = "";
    public string IMFrontierAppKey = "";

    public bool liveAvailable = false;
    public string liveScheme = "";
    public string khId = "";
    public string khSchema = "";

    public bool DeepLinkAvailable;
    public int DeepLinkSchemeListSize;
    public List<string> DeepLinkSchemeList;
    public string DeepLinkHost;
    public string RatingAppStoreID;

    public bool RTCAvailable = false;
    public string RTCAppId = "";

    public bool EnableSDKAuthPoping = true;

    public string Skin = "purple";

    public bool IsNeedAntiAddiction = true;
    
    public bool IsNeedServiceMouth = true;
#region ATT

    public bool ATTAvailable;
	public bool needCustomDefalutView;
    public bool needCustomCoverView;
	public bool needCustomCall;
	public bool useCPPopup;
    public string coverContentColor;
    public string coverContent;
    public string CancelTitleColor;
    public string CancelColor;
    public string CancelTitle;
    public string ConfirmTitleColor;
    public string ConfirmColor;
    public string ConfirmTitle;
    public string Content;
    public string Title;
    public bool needCover;
    public float coverAlpha;


#endregion

#region AppScheme

    public string AppSchemeArray;

#endregion

#region AdPangle

    public bool IsNeedAdPangle = true;
    public string PangleAppID = "";

#endregion
}

[InitializeOnLoad]
public class ConfigSettings : ScriptableObject
{

    const string sdkSettingsAssetName = "ConfigSettings";
    const string sdkSettingsAssetExtension = ".json";

    static GMSDKEnv env = GMSDKEnv.Instance;
    //string androidConfigPath = env.PATH_PUGLIN_ANDROID + "/assets/msdkconfig.ini";

    public ConfigModel configModel = new ConfigModel();

    private static ConfigSettings _instance;

    public static ConfigSettings Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = CreateInstance<ConfigSettings>();

                string properPath = env.PATH_CONFIG_SETTINGS;
                if (!Directory.Exists(properPath))
                {
                    Directory.CreateDirectory(properPath);
                }

                string fullPath = Path.Combine(env.PATH_CONFIG_SETTINGS,
                    sdkSettingsAssetName + sdkSettingsAssetExtension
                );
                if (System.IO.File.Exists(fullPath))
                {
                    Debug.Log("configsettings file exist");
                    string configSettingJson = File.ReadAllText(fullPath);
                    _instance.configModel = JsonMapper.ToObject<ConfigModel>(configSettingJson);
                }
                else
                {
                    Debug.Log("configsettings file not exist, read default settings");
                }
            }
            return _instance;
        }
    }


#region Config Settings
    //--- Common Settings ---



    [SerializeField]

    public string BundleId
    {
        get { return Instance.configModel.bundleId; }
    }

    public void SetBundleId(string aBundleId)
    {
        if (Instance.configModel.bundleId != aBundleId)
        {
            Instance.configModel.bundleId = aBundleId;
            DirtyEditor();
            saveSettings();
        }
    }

    public string AppId
    {
        get { return Instance.configModel.appId; }
    }

    public void SetAppId(string aAppid)
    {
        if (Instance.configModel.appId != aAppid)
        {
            Instance.configModel.appId = aAppid;
            DirtyEditor();
            saveSettings();
        }
    }

    public string AppName
    {
        get { return Instance.configModel.appName; }
    }

    public void SetAppName(string aAppName)
    {
        if (Instance.configModel.appName != aAppName)
        {
            Instance.configModel.appName = aAppName;
            DirtyEditor();
            saveSettings();
        }
    }

    public string AppDisplayName
    {
        get { return Instance.configModel.appDisplayName; }
    }

    public void SetAppDisplayName(string aAppDisplayName)
    {
        if (Instance.configModel.appDisplayName != aAppDisplayName)
        {
            Instance.configModel.appDisplayName = aAppDisplayName;
            DirtyEditor();
            saveSettings();
        }
    }


    public bool ImportMagicBox
    {
        get { return Instance.configModel.importMagicBox; }
    }

    public void setImportMagicBox(bool importMagicBox)
    {
        if (Instance.configModel.importMagicBox != importMagicBox)
        {
            Instance.configModel.importMagicBox = importMagicBox;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool DsymAutoUpload
    {
        get { return Instance.configModel.dsymAutoUpload; }
    }

    public bool SecurityUsePacketChannelEnable
    {
        get
        {
            return Instance.configModel.usePacketChannel;
        }
        set
        {
            if (Instance.configModel.usePacketChannel != value)
            {
                Instance.configModel.usePacketChannel = value;
                DirtyEditor();
                saveSettings();
            }
        }
    }

    public bool needGSDKModifyInfoPlist
	{
		get
		{
			return Instance.configModel.needModifyInfoPlist;
		}
		set
		{
			if (Instance.configModel.needModifyInfoPlist != value)
			{
				Instance.configModel.needModifyInfoPlist = value;
				DirtyEditor();
				saveSettings();
			}
		}
	}

    public bool gsdkModifyInfoPlistReplaceWhenMetSame
	{
		get
		{
			return Instance.configModel.modifyInfoPlistReplaceWhenMetSame;
		}
		set
		{
			if (Instance.configModel.modifyInfoPlistReplaceWhenMetSame != value)
			{
				Instance.configModel.modifyInfoPlistReplaceWhenMetSame = value;
				DirtyEditor();
				saveSettings();
			}
		}
	}
 

    public void setDsymAutoUpload(bool autoUpload)
    {
        if (Instance.configModel.dsymAutoUpload != autoUpload)
        {
            Instance.configModel.dsymAutoUpload = autoUpload;
            DirtyEditor();
            saveSettings();
        }
    }

    public string DsymUploadKey
    {
        get { return Instance.configModel.dsymUploadKey; }
    }

    public void setDsymUploadKey(string key)
    {
        if (Instance.configModel.dsymUploadKey != key)
        {
            Instance.configModel.dsymUploadKey = key;
            DirtyEditor();
            saveSettings();
        }
    }

    public double SdkInitTimeout
    {
        get { return Instance.configModel.gsdk_init_timeout * 1000.0; }
    }

    public void SetSdkInitTimeout(double sdkInitTimeout)
    {
        if (Instance.configModel.gsdk_init_timeout != (sdkInitTimeout / 1000))
        {
            Instance.configModel.gsdk_init_timeout = (sdkInitTimeout / 1000);
            DirtyEditor();
            saveSettings();
        }
    }

    public string ProjectTargetName
    {
        get { return Instance.configModel.projectTargetName; }
    }

    public void SetProjectTargetName(string aProjectTargetName)
    {
        if (Instance.configModel.projectTargetName != aProjectTargetName)
        {
            Instance.configModel.projectTargetName = aProjectTargetName;
            DirtyEditor();
            saveSettings();
        }
    }

    public string FrameworkTargetName
    {
        get { return Instance.configModel.frameworkTargetName; }
    }

    public void SetFrameworkTargetName(string aFrameworkTargetName)
    {
        if (Instance.configModel.frameworkTargetName != aFrameworkTargetName)
        {
            Instance.configModel.frameworkTargetName = aFrameworkTargetName;
            DirtyEditor();
            saveSettings();
        }
    }

    public int AssociatedDomainsListSize
    {
        get
        {
            return Instance.configModel.AssociatedDomainsListSize;
        }
        set
        {
            if (Instance.configModel.AssociatedDomainsListSize != value)
            {
                Instance.configModel.AssociatedDomainsListSize = value;
                DirtyEditor();
                saveSettings();
            }
        }
    }

    public List<string> AssociatedDomainsList
    {
        get
        {
            return Instance.configModel.AssociatedDomainsList;
        }
        set
        {
            Instance.configModel.AssociatedDomainsList = value;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool IOSCertSuccess
    {
        get { return Instance.configModel.iOSCertSuccess; }
    }

    public void SetIOSCertSuccess(bool aIOSCertSuccess)
    {
        if (Instance.configModel.iOSCertSuccess != aIOSCertSuccess)
        {
            Instance.configModel.iOSCertSuccess = aIOSCertSuccess;
            DirtyEditor();
            saveSettings();
        }
    }

    public string IOSProvisioniongGUID
    {
        get { return Instance.configModel.iOSProvisioningGUID; }
    }

    public void SetIOSProvisioniongGUID(string aIOSProvisioniongGUID)
    {
        if (Instance.configModel.iOSProvisioningGUID != aIOSProvisioniongGUID)
        {
            Instance.configModel.iOSProvisioningGUID = aIOSProvisioniongGUID;
            DirtyEditor();
            saveSettings();
        }
    }

    public string IOSCertName
    {
        get { return Instance.configModel.iOSCertName; }
    }

    public void SetIOSCertName(string aIOSCertName)
    {
        if (Instance.configModel.iOSCertName != aIOSCertName)
        {
            Instance.configModel.iOSCertName = aIOSCertName;
            DirtyEditor();
            saveSettings();
        }
    }

    public string IOSTeamID
    {
        get { return Instance.configModel.iOSTeamID; }
    }

    public void SetIOSTeamID(string aIOSTeamID)
    {
        if (Instance.configModel.iOSTeamID != aIOSTeamID)
        {
            Instance.configModel.iOSTeamID = aIOSTeamID;
            DirtyEditor();
            saveSettings();
        }
    }
    
    public bool IOSDebugCertSuccess
    {
        get { return Instance.configModel.iOSDebugCertSuccess; }
    }

    public void SetIOSDebugCertSuccess(bool aIOSDebugCertSuccess)
    {
        if (Instance.configModel.iOSDebugCertSuccess != aIOSDebugCertSuccess)
        {
            Instance.configModel.iOSDebugCertSuccess = aIOSDebugCertSuccess;
            DirtyEditor();
            saveSettings();
        }
    }

    public string IOSDebugProvisioniongGUID
    {
        get { return Instance.configModel.iOSDebugProvisioningGUID; }
    }

    public void SetIOSDebugProvisioniongGUID(string aIOSDebugProvisioniongGUID)
    {
        if (Instance.configModel.iOSDebugProvisioningGUID != aIOSDebugProvisioniongGUID)
        {
            Instance.configModel.iOSDebugProvisioningGUID = aIOSDebugProvisioniongGUID;
            DirtyEditor();
            saveSettings();
        }
    }

    public string IOSDebugCertName
    {
        get { return Instance.configModel.iOSDebugCertName; }
    }

    public void SetIOSDebugCertName(string aIOSDebugCertName)
    {
        if (Instance.configModel.iOSDebugCertName != aIOSDebugCertName)
        {
            Instance.configModel.iOSDebugCertName = aIOSDebugCertName;
            DirtyEditor();
            saveSettings();
        }
    }

    public string IOSDebugTeamID
    {
        get { return Instance.configModel.iOSDebugTeamID; }
    }

    public void SetIOSDebugTeamID(string aIOSDebugTeamID)
    {
        if (Instance.configModel.iOSDebugTeamID != aIOSDebugTeamID)
        {
            Instance.configModel.iOSDebugTeamID = aIOSDebugTeamID;
            DirtyEditor();
            saveSettings();
        }
    }
    
    public bool ToutiaoLoginAvailable
    {
        get { return Instance.configModel.toutiaoLoginAvailable; }
    }

    public void SetToutiaoLoginAvailable(bool aToutiaoLoginAvailable)
    {
        if (Instance.configModel.toutiaoLoginAvailable != aToutiaoLoginAvailable)
        {
            Instance.configModel.toutiaoLoginAvailable = aToutiaoLoginAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public string ToutiaoLoginPlatformId
    {
        get { return Instance.configModel.toutiaoLoginPlatformId; }
    }

    public void SetToutiaoLoginPlatformId(string aToutiaoLoginPlatformId)
    {
        if (Instance.configModel.toutiaoLoginPlatformId != aToutiaoLoginPlatformId)
        {
            Instance.configModel.toutiaoLoginPlatformId = aToutiaoLoginPlatformId;
            DirtyEditor();
            saveSettings();
        }
    }

    public string ToutiaoLoginKey
    {
        get { return Instance.configModel.toutiaoLoginKey; }
    }

    public void SetToutiaoLoginKey(string aToutiaoLoginKey)
    {
        if (Instance.configModel.toutiaoLoginKey != aToutiaoLoginKey)
        {
            Instance.configModel.toutiaoLoginKey = aToutiaoLoginKey;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool ToutiaoFriendPermissionAvailable
    {
        get { return Instance.configModel.toutiaoFriendPermissionAvailable; }
    }

    public void SetToutiaoFriendPermissionAvailable(bool aToutiaoFriendPermissionAvailable)
    {
        if (Instance.configModel.toutiaoFriendPermissionAvailable != aToutiaoFriendPermissionAvailable)
        {
            Instance.configModel.toutiaoFriendPermissionAvailable = aToutiaoFriendPermissionAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool DouyinLoginAvailable
    {
        get { return Instance.configModel.douyinLoginAvailable; }
    }

    public void SetDouyinLoginAvailable(bool aDouyinLoginAvailable)
    {
        if (Instance.configModel.douyinLoginAvailable != aDouyinLoginAvailable)
        {
            Instance.configModel.douyinLoginAvailable = aDouyinLoginAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public string DouyinLoginPlatformId
    {
        get { return Instance.configModel.douyinLoginPlatformId; }
    }

    public void SetDouyinLoginPlatformId(string aDouyinLoginPlatformId)
    {
        if (Instance.configModel.douyinLoginPlatformId != aDouyinLoginPlatformId)
        {
            Instance.configModel.douyinLoginPlatformId = aDouyinLoginPlatformId;
            DirtyEditor();
            saveSettings();
        }
    }

    public string DouyinLoginKey
    {
        get { return Instance.configModel.douyinLoginKey; }
    }

    public void SetDouyinLoginKey(string aDouyinLoginKey)
    {
        if (Instance.configModel.douyinLoginKey != aDouyinLoginKey)
        {
            Instance.configModel.douyinLoginKey = aDouyinLoginKey;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool DouyinFriendPermissionAvailable
    {
        get { return Instance.configModel.douyinFriendPermissionAvailable; }
    }

    public void SetDouyinFriendPermissionAvailable(bool aDouyinFriendPermissionAvailable)
    {
        if (Instance.configModel.douyinFriendPermissionAvailable != aDouyinFriendPermissionAvailable)
        {
            Instance.configModel.douyinFriendPermissionAvailable = aDouyinFriendPermissionAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool DouyinRelationFollowPermissionAvailable
    {
        get { return Instance.configModel.douyinRelationFollowPermissionAvailable; }
    }

    public void SetDouyinRelationFollowPermissionAvailable(bool aDouyinRelationFollowPermissionAvailable)
    {
        if (Instance.configModel.douyinRelationFollowPermissionAvailable != aDouyinRelationFollowPermissionAvailable)
        {
            Instance.configModel.douyinRelationFollowPermissionAvailable = aDouyinRelationFollowPermissionAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool DouyinRelationFollowDefaultCheckPermissionAvailable
    {
        get { return Instance.configModel.douyinRelationFollowDefaultCheckPermissionAvailable; }
    }

    public void SetDouyinRelationFollowDefaultCheckPermissionAvailable(bool aDouyinRelationFollowDefaultCheckPermissionAvailable)
    {
        if (Instance.configModel.douyinRelationFollowDefaultCheckPermissionAvailable != aDouyinRelationFollowDefaultCheckPermissionAvailable)
        {
            Instance.configModel.douyinRelationFollowDefaultCheckPermissionAvailable = aDouyinRelationFollowDefaultCheckPermissionAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool DouyinVideoPermissionAvailable
    {
        get { return Instance.configModel.douyinVideoPermissionAvailable; }
    }

    public void SetDouyinVideoPermissionAvailable(bool aDouyinVideoPermissionAvailable)
    {
        if (Instance.configModel.douyinVideoPermissionAvailable != aDouyinVideoPermissionAvailable)
        {
            Instance.configModel.douyinVideoPermissionAvailable = aDouyinVideoPermissionAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool DouyinRealNameAuthPermissionAvailable
    {
        get { return Instance.configModel.douyinRealNameAuthPermissionAvailable; }
    }

    public void SetDouyinRealNameAuthPermissionAvailable(bool aDouyinRealNameAuthPermissionAvailable)
    {
        if (Instance.configModel.douyinRealNameAuthPermissionAvailable != aDouyinRealNameAuthPermissionAvailable)
        {
            Instance.configModel.douyinRealNameAuthPermissionAvailable = aDouyinRealNameAuthPermissionAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool DouyinMobilePermissionAvailable
    {
        get { return Instance.configModel.douyinMobilePermissionAvailable; }
    }

    public void SetDouyinMobilePermissionAvailable(bool aDouyinMobilePermissionAvailable)
    {
        if (Instance.configModel.douyinMobilePermissionAvailable != aDouyinMobilePermissionAvailable)
        {
            Instance.configModel.douyinMobilePermissionAvailable = aDouyinMobilePermissionAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool DouyinFriendListPermissionAvailable
    {
        get { return Instance.configModel.douyinFriendListPermissionAvailable; }
    }

    public void SetDouyinFriendListPermissionAvailable(bool aDouyinFriendListPermissionAvailable)
    {
        if (Instance.configModel.douyinFriendListPermissionAvailable != aDouyinFriendListPermissionAvailable)
        {
            Instance.configModel.douyinFriendListPermissionAvailable = aDouyinFriendListPermissionAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool AppleLoginAvailable
    {
        get { return Instance.configModel.appleLoginAvailable; }
    }

    public void SetAppleLoginAvailable(bool aAppleLoginAvailable)
    {
        if (Instance.configModel.appleLoginAvailable != aAppleLoginAvailable)
        {
            Instance.configModel.appleLoginAvailable = aAppleLoginAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public string AppleLoginPlatformId
    {
        get { return Instance.configModel.appleLoginPlatformId; }
    }

    public void SetAppleLoginPlatformId(string aAppleLoginPlatformId)
    {
        if (Instance.configModel.appleLoginPlatformId != aAppleLoginPlatformId)
        {
            Instance.configModel.appleLoginPlatformId = aAppleLoginPlatformId;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool HuoShanLoginAvailable
    {
        get { return Instance.configModel.huoShanLoginAvailable; }
    }

    public void SetHuoShanLoginAvailable(bool aHuoShanLoginAvailable)
    {
        if (Instance.configModel.huoShanLoginAvailable != aHuoShanLoginAvailable)
        {
            Instance.configModel.huoShanLoginAvailable = aHuoShanLoginAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public string HuoShanLoginPlatformId
    {
        get { return Instance.configModel.huoShanLoginPlatformId; }
    }

    public void SetHuoShanLoginPlatformId(string aHuoShanLoginPlatformId)
    {
        if (Instance.configModel.huoShanLoginPlatformId != aHuoShanLoginPlatformId)
        {
            Instance.configModel.huoShanLoginPlatformId = aHuoShanLoginPlatformId;
            DirtyEditor();
            saveSettings();
        }
    }

    public string HuoShanLoginKey
    {
        get { return Instance.configModel.huoShanLoginKey; }
    }

    public void SetHuoShanLoginKey(string aHuoShanLoginKey)
    {
        if (Instance.configModel.huoShanLoginKey != aHuoShanLoginKey)
        {
            Instance.configModel.huoShanLoginKey = aHuoShanLoginKey;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool XiGuaLoginAvailable
    {
        get { return Instance.configModel.xiGuaLoginAvailable; }
    }

    public void SetXiGuaLoginAvailable(bool aXiGuaLoginAvailable)
    {
        if (Instance.configModel.xiGuaLoginAvailable != aXiGuaLoginAvailable)
        {
            Instance.configModel.xiGuaLoginAvailable = aXiGuaLoginAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public string XiGuaLoginPlatformId
    {
        get { return Instance.configModel.xiGuaLoginPlatformId; }
    }

    public void SetXiGuaLoginPlatformId(string aXiGuaLoginPlatformId)
    {
        if (Instance.configModel.xiGuaLoginPlatformId != aXiGuaLoginPlatformId)
        {
            Instance.configModel.xiGuaLoginPlatformId = aXiGuaLoginPlatformId;
            DirtyEditor();
            saveSettings();
        }
    }

    public string XiGuaLoginKey
    {
        get { return Instance.configModel.xiGuaLoginKey; }
    }

    public void SetXiGuaLoginKey(string aXiGuaLoginKey)
    {
        if (Instance.configModel.xiGuaLoginKey != aXiGuaLoginKey)
        {
            Instance.configModel.xiGuaLoginKey = aXiGuaLoginKey;
            DirtyEditor();
            saveSettings();
        }
    }

    public string PhoneMobileKey
    {
        get { return Instance.configModel.phoneMobileKey; }
    }

    public void SetPhoneMobileKey(string aPhoneMobileKey)
    {
        if (Instance.configModel.phoneMobileKey != aPhoneMobileKey)
        {
            Instance.configModel.phoneMobileKey = aPhoneMobileKey;
            DirtyEditor();
            saveSettings();
        }
    }

    public string PhoneMobileSecret
    {
        get { return Instance.configModel.phoneMobileSecret; }
    }

    public void SetPhoneMobileSecret(string aPhoneMobileSecret)
    {
        if (Instance.configModel.phoneMobileSecret != aPhoneMobileSecret)
        {
            Instance.configModel.phoneMobileSecret = aPhoneMobileSecret;
            DirtyEditor();
            saveSettings();
        }
    }

    public string PhoneTelecomKey
    {
        get { return Instance.configModel.phoneTelecomKey; }
    }

    public void SetPhoneTelecomKey(string aPhoneTelecomKey)
    {
        if (Instance.configModel.phoneTelecomKey != aPhoneTelecomKey)
        {
            Instance.configModel.phoneTelecomKey = aPhoneTelecomKey;
            DirtyEditor();
            saveSettings();
        }
    }

    public string PhoneTelecomSecret
    {
        get { return Instance.configModel.phoneTelecomSecret; }
    }

    public void SetPhoneTelecomSecret(string aPhoneTelecomSecret)
    {
        if (Instance.configModel.phoneTelecomSecret != aPhoneTelecomSecret)
        {
            Instance.configModel.phoneTelecomSecret = aPhoneTelecomSecret;
            DirtyEditor();
            saveSettings();
        }
    }

    public string PhoneUnionKey
    {
        get { return Instance.configModel.phoneUnionKey; }
    }

    public void SetPhoneUnionKey(string aPhoneUnionKey)
    {
        if (Instance.configModel.phoneUnionKey != aPhoneUnionKey)
        {
            Instance.configModel.phoneUnionKey = aPhoneUnionKey;
            DirtyEditor();
            saveSettings();
        }
    }

    public string PhoneUnionSecret
    {
        get { return Instance.configModel.phoneUnionSecret; }
    }

    public void SetPhoneUnionSecret(string aPhoneUnionSecret)
    {
        if (Instance.configModel.phoneUnionSecret != aPhoneUnionSecret)
        {
            Instance.configModel.phoneUnionSecret = aPhoneUnionSecret;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool PushAvailable
    {
        get { return Instance.configModel.pushAvailable; }
    }

    public void SetPushAvailable(bool aPushAvailable)
    {
        if (Instance.configModel.pushAvailable != aPushAvailable)
        {
            Instance.configModel.pushAvailable = aPushAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool PrivacyMicrophoneUsageAvailable
    {
        get { return Instance.configModel.privacyMicrophoneUsageAvailable; }
    }

    public void SetPrivacyMicrophoneUsageAvailable(bool aPrivacyMicrophoneUsageAvailable)
    {
        if (Instance.configModel.privacyMicrophoneUsageAvailable != aPrivacyMicrophoneUsageAvailable)
        {
            Instance.configModel.privacyMicrophoneUsageAvailable = aPrivacyMicrophoneUsageAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool PrivacyLocationUsageAvailable
    {
        get { return Instance.configModel.privacyLocationUsageAvailable; }
    }

    public void SetPrivacyLocationUsageAvailable(bool aPrivacyLocationUsageAvailable)
    {
        if (Instance.configModel.privacyLocationUsageAvailable != aPrivacyLocationUsageAvailable)
        {
            Instance.configModel.privacyLocationUsageAvailable = aPrivacyLocationUsageAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool PrivacyPhotoUsageAvailable
    {
        get { return Instance.configModel.privacyPhotoUsageAvailable; }
    }

    public void SetPrivacyPhotoUsageAvailable(bool aPrivacyPhotoUsageAvailable)
    {
        if (Instance.configModel.privacyPhotoUsageAvailable != aPrivacyPhotoUsageAvailable)
        {
            Instance.configModel.privacyPhotoUsageAvailable = aPrivacyPhotoUsageAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool PrivacyPhotoAdditionsAvailable
    {
        get { return Instance.configModel.privacyPhotoAdditionsAvailable; }
    }

    public void SetPrivacyPhotoAdditionsAvailable(bool aPrivacyPhotoAdditionsAvailable)
    {
        if (Instance.configModel.privacyPhotoAdditionsAvailable != aPrivacyPhotoAdditionsAvailable)
        {
            Instance.configModel.privacyPhotoAdditionsAvailable = aPrivacyPhotoAdditionsAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool PrivacyCameraUsageAvailable
    {
        get { return Instance.configModel.privacyCameraUsageAvailable; }
    }

    public void SetPrivacyCameraUsageAvailable(bool aPrivacyCameraUsageAvailable)
    {
        if (Instance.configModel.privacyCameraUsageAvailable != aPrivacyCameraUsageAvailable)
        {
            Instance.configModel.privacyCameraUsageAvailable = aPrivacyCameraUsageAvailable;
            DirtyEditor();
            saveSettings();
        }
    }
    public bool PrivacyContactsUsageAvailable
    {
        get { return Instance.configModel.privacyContactsUsageAvailable; }
    }

    public void SetPrivacyContactsUsageAvailable(bool aPrivacyContactsUsageAvailable)
    {
        if (Instance.configModel.privacyContactsUsageAvailable != aPrivacyContactsUsageAvailable)
        {
            Instance.configModel.privacyContactsUsageAvailable = aPrivacyContactsUsageAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool PrivacyAppTrackingUsageAvailable
    {
        get { return Instance.configModel.privacyAppTrackingUsageAvailable; }
    }

    public void SetPrivacyAppTrackingUsageAvailable(bool aPrivacyAppTrackingUsageAvailable)
    {
        if (Instance.configModel.privacyAppTrackingUsageAvailable != aPrivacyAppTrackingUsageAvailable)
        {
            Instance.configModel.privacyAppTrackingUsageAvailable = aPrivacyAppTrackingUsageAvailable;
            DirtyEditor();
            saveSettings();
        }
    }
    public bool PrivacyLocationAlwaysUsageAvailable
    {
        get { return Instance.configModel.privacyLocationAlwaysUsageAvailable; }
    }

    public void SetPrivacyLocationAlwaysUsageAvailable(bool aPrivacyLocationAlwaysUsageAvailable)
    {
        if (Instance.configModel.privacyLocationAlwaysUsageAvailable != aPrivacyLocationAlwaysUsageAvailable)
        {
            Instance.configModel.privacyLocationAlwaysUsageAvailable = aPrivacyLocationAlwaysUsageAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool PrivacyCalendarsUsageAvailable
    {
        get { return Instance.configModel.privacyCalendarsUsageAvailable; }
    }

    public void SetPrivacyCalendarsUsageAvailable(bool aPrivacyCalendarsUsageAvailable)
    {
        if (Instance.configModel.privacyCalendarsUsageAvailable != aPrivacyCalendarsUsageAvailable)
        {
            Instance.configModel.privacyCalendarsUsageAvailable = aPrivacyCalendarsUsageAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool PrivacyRemindersUsageaAvailable
    {
        get { return Instance.configModel.privacyRemindersUsageaAvailable; }
    }

    public void SetPrivacyRemindersUsageaAvailable(bool aPrivacyRemindersUsageaAvailable)
    {
        if (Instance.configModel.privacyRemindersUsageaAvailable != aPrivacyRemindersUsageaAvailable)
        {
            Instance.configModel.privacyRemindersUsageaAvailable = aPrivacyRemindersUsageaAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool PrivacySpeechRecognitionUsageAvailable
    {
        get { return Instance.configModel.privacySpeechRecognitionUsageAvailable; }
    }

    public void SetPrivacySpeechRecognitionUsageAvailable(bool aPrivacySpeechRecognitionUsageAvailable)
    {
        if (Instance.configModel.privacySpeechRecognitionUsageAvailable != aPrivacySpeechRecognitionUsageAvailable)
        {
            Instance.configModel.privacySpeechRecognitionUsageAvailable = aPrivacySpeechRecognitionUsageAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool PrivacySiriUsageAvailable
    {
        get { return Instance.configModel.privacySiriUsageAvailable; }
    }

    public void SetPrivacySiriUsageAvailable(bool aPrivacySiriUsageAvailable)
    {
        if (Instance.configModel.privacySiriUsageAvailable != aPrivacySiriUsageAvailable)
        {
            Instance.configModel.privacySiriUsageAvailable = aPrivacySiriUsageAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool PrivacyFaceIDUsageAvailable
    {
        get { return Instance.configModel.privacyFaceIDUsageAvailable; }
    }

    public void SetPrivacyFaceIDUsageAvailable(bool aPrivacyFaceIDUsageAvailable)
    {
        if (Instance.configModel.privacyFaceIDUsageAvailable != aPrivacyFaceIDUsageAvailable)
        {
            Instance.configModel.privacyFaceIDUsageAvailable = aPrivacyFaceIDUsageAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public string PrivacyMicrophoneUsageDesc
    {
        get { return Instance.configModel.privacyMicrophoneUsageDesc; }
    }

    public void SetPrivacyMicrophoneUsageDesc(string aPrivacyMicrophoneUsageDesc)
    {
        if (Instance.configModel.privacyMicrophoneUsageDesc != aPrivacyMicrophoneUsageDesc)
        {
            Instance.configModel.privacyMicrophoneUsageDesc = aPrivacyMicrophoneUsageDesc;
            DirtyEditor();
            saveSettings();
        }
    }

    public string PrivacyLocationUsageDesc
    {
        get { return Instance.configModel.privacyLocationUsageDesc; }
    }

    public void SetPrivacyLocationUsageDesc(string aPrivacyLocationUsageDesc)
    {
        if (Instance.configModel.privacyLocationUsageDesc != aPrivacyLocationUsageDesc)
        {
            Instance.configModel.privacyLocationUsageDesc = aPrivacyLocationUsageDesc;
            DirtyEditor();
            saveSettings();
        }
    }

    public string PrivacyPhotoUsageDesc
    {
        get { return Instance.configModel.privacyPhotoUsageDesc; }
    }

    public void SetPrivacyPhotoUsageDesc(string aPrivacyPhotoUsageDesc)
    {
        if (Instance.configModel.privacyPhotoUsageDesc != aPrivacyPhotoUsageDesc)
        {
            Instance.configModel.privacyPhotoUsageDesc = aPrivacyPhotoUsageDesc;
            DirtyEditor();
            saveSettings();
        }
    }

    public string PrivacyPhotoAdditionsDesc
    {
        get { return Instance.configModel.privacyPhotoAdditionsDesc; }
    }

    public void SetPrivacyPhotoAdditionsDesc(string aPrivacyPhotoAdditionsDesc)
    {
        if (Instance.configModel.privacyPhotoAdditionsDesc != aPrivacyPhotoAdditionsDesc)
        {
            Instance.configModel.privacyPhotoAdditionsDesc = aPrivacyPhotoAdditionsDesc;
            DirtyEditor();
            saveSettings();
        }
    }

    public string PrivacyCameraUsageDesc
    {
        get { return Instance.configModel.privacyCameraUsageDesc; }
    }

    public void SetPrivacyCameraUsageDesc(string aPrivacyCameraUsageDesc)
    {
        if (Instance.configModel.privacyCameraUsageDesc != aPrivacyCameraUsageDesc)
        {
            Instance.configModel.privacyCameraUsageDesc = aPrivacyCameraUsageDesc;
            DirtyEditor();
            saveSettings();
        }
    }

    public string privacyContactsUsageDesc
    {
        get { return Instance.configModel.privacyContactsUsageDesc; }
    }

    public void SetPrivacyContactsUsageDesc(string aPrivacyContactsUsageDesc)
    {
        if (Instance.configModel.privacyContactsUsageDesc != aPrivacyContactsUsageDesc)
        {
            Instance.configModel.privacyContactsUsageDesc = aPrivacyContactsUsageDesc;
            DirtyEditor();
            saveSettings();
        }
    }

    public string privacyAppTrackingUsageDesc
    {
        get { return Instance.configModel.privacyAppTrackingUsageDesc; }
    }

    public void SetPrivacyAppTrackingUsageDesc(string aPrivacyAppTrackingUsageDesc)
    {
        if (Instance.configModel.privacyAppTrackingUsageDesc != aPrivacyAppTrackingUsageDesc)
        {
            Instance.configModel.privacyAppTrackingUsageDesc = aPrivacyAppTrackingUsageDesc;
            DirtyEditor();
            saveSettings();
        }
    }

    public string privacyLocationAlwaysUsageDesc
    {
        get { return Instance.configModel.privacyLocationAlwaysUsageDesc; }
    }

    public void SetPrivacyLocationAlwaysUsageDesc(string aPrivacyLocationAlwaysUsageDesc)
    {
        if (Instance.configModel.privacyLocationAlwaysUsageDesc != aPrivacyLocationAlwaysUsageDesc)
        {
            Instance.configModel.privacyLocationAlwaysUsageDesc = aPrivacyLocationAlwaysUsageDesc;
            DirtyEditor();
            saveSettings();
        }
    }

    public string privacyCalendarsUsageDesc
    {
        get { return Instance.configModel.privacyCalendarsUsageDesc; }
    }

    public void SetPrivacyCalendarsUsageDesc(string aPrivacyCalendarsUsageDesc)
    {
        if (Instance.configModel.privacyCalendarsUsageDesc != aPrivacyCalendarsUsageDesc)
        {
            Instance.configModel.privacyCalendarsUsageDesc = aPrivacyCalendarsUsageDesc;
            DirtyEditor();
            saveSettings();
        }
    }

    public string privacyRemindersUsageDesc
    {
        get { return Instance.configModel.privacyRemindersUsageDesc; }
    }

    public void SetPrivacyRemindersUsageDesc(string aPrivacyRemindersUsageDesc)
    {
        if (Instance.configModel.privacyRemindersUsageDesc != aPrivacyRemindersUsageDesc)
        {
            Instance.configModel.privacyRemindersUsageDesc = aPrivacyRemindersUsageDesc;
            DirtyEditor();
            saveSettings();
        }
    }

    public string privacySpeechRecognitionUsageDesc
    {
        get { return Instance.configModel.privacySpeechRecognitionUsageDesc; }
    }

    public void SetPrivacySpeechRecognitionUsageDesc(string aPrivacySpeechRecognitionUsageDesc)
    {
        if (Instance.configModel.privacySpeechRecognitionUsageDesc != aPrivacySpeechRecognitionUsageDesc)
        {
            Instance.configModel.privacySpeechRecognitionUsageDesc = aPrivacySpeechRecognitionUsageDesc;
            DirtyEditor();
            saveSettings();
        }
    }
    public string privacySiriUsageDesc
    {
        get { return Instance.configModel.privacySiriUsageDesc; }
    }

    public void SetPrivacySiriUsageDesc(string aPrivacySiriUsageDesc)
    {
        if (Instance.configModel.privacySiriUsageDesc != aPrivacySiriUsageDesc)
        {
            Instance.configModel.privacySiriUsageDesc = aPrivacySiriUsageDesc;
            DirtyEditor();
            saveSettings();
        }
    }
    public string privacyFaceIDUsageDesc
    {
        get { return Instance.configModel.privacyFaceIDUsageDesc; }
    }

    public void SetPrivacyFaceIDUsageDesc(string aPrivacyFaceIDUsageDesc)
    {
        if (Instance.configModel.privacyFaceIDUsageDesc != aPrivacyFaceIDUsageDesc)
        {
            Instance.configModel.privacyFaceIDUsageDesc = aPrivacyFaceIDUsageDesc;
            DirtyEditor();
            saveSettings();
        }
    }

    public string SharePanelID
    {
        get { return Instance.configModel.sharePanelID; }
    }

    public void SetSharePanelID(string aSharePanelID)
    {
        if (Instance.configModel.sharePanelID != aSharePanelID)
        {
            Instance.configModel.sharePanelID = aSharePanelID;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool ShareDimAmountAvailable
    {
        get { return Instance.configModel.shareDimAmountAvailable; }
    }

    public void SetShareDimAmountAvailable(bool aShareDimAmountAvailable)
    {
        if (Instance.configModel.shareDimAmountAvailable != aShareDimAmountAvailable)
        {
            Instance.configModel.shareDimAmountAvailable = aShareDimAmountAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public string ShareDimAmount
    {
        get { return Instance.configModel.shareDimAmount; }
    }

    public void SetShareDimAmount(string aDimAmount)
    {
        if (Instance.configModel.shareDimAmount != aDimAmount)
        {
            Instance.configModel.shareDimAmount = aDimAmount;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool ShareQQAvailable
    {
        get { return Instance.configModel.shareQQAvailable; }
    }

    public void SetShareQQAvailable(bool aShareQQAvailable)
    {
        if (Instance.configModel.shareQQAvailable != aShareQQAvailable)
        {
            Instance.configModel.shareQQAvailable = aShareQQAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public string ShareQQKey
    {
        get { return Instance.configModel.shareQQKey; }
    }

    public void SetShareQQKey(string aShareQQKey)
    {
        if (Instance.configModel.shareQQKey != aShareQQKey)
        {
            Instance.configModel.shareQQKey = aShareQQKey;
            DirtyEditor();
            saveSettings();
        }
    }

    public string ShareQQUniversalLink
    {
        get { return Instance.configModel.shareQQUniversalLink; }
    }

    public void SetShareQQUniversalLink(string aShareQQUniversalLink)
    {
        if (Instance.configModel.shareQQUniversalLink != aShareQQUniversalLink)
        {
            Instance.configModel.shareQQUniversalLink = aShareQQUniversalLink;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool ShareQZoneAvailable
    {
        get { return Instance.configModel.shareQZoneAvailable; }
    }

    public void SetShareQZoneAvailable(bool aShareQZoneAvailable)
    {
        if (Instance.configModel.shareQZoneAvailable != aShareQZoneAvailable)
        {
            Instance.configModel.shareQZoneAvailable = aShareQZoneAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public string ShareQZoneKey
    {
        get { return Instance.configModel.shareQZoneKey; }
    }

    public void SetaShareQZoneKey(string aShareQZoneKey)
    {
        if (Instance.configModel.shareQZoneKey != aShareQZoneKey)
        {
            Instance.configModel.shareQZoneKey = aShareQZoneKey;
            DirtyEditor();
            saveSettings();
        }
    }


    public string ShareQZoneUniversalLink
    {
        get { return Instance.configModel.shareQZoneUniversalLink; }
    }

    public void SetShareQZoneUniversalLink(string aShareQZoneUniversalLink)
    {
        if (Instance.configModel.shareQZoneUniversalLink != aShareQZoneUniversalLink)
        {
            Instance.configModel.shareQZoneUniversalLink = aShareQZoneUniversalLink;
            DirtyEditor();
            saveSettings();
        }
    }


    public bool ShareWeChatAvailable
    {
        get { return Instance.configModel.shareWeChatAvailable; }
    }

    public void SetShareWeChatAvailable(bool aShareWeChatAvailable)
    {
        if (Instance.configModel.shareWeChatAvailable != aShareWeChatAvailable)
        {
            Instance.configModel.shareWeChatAvailable = aShareWeChatAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public string ShareWeChatKey
    {
        get { return Instance.configModel.shareWeChatKey; }
    }

    public void SetShareWeChatKey(string aShareWeChatKey)
    {
        if (Instance.configModel.shareWeChatKey != aShareWeChatKey)
        {
            Instance.configModel.shareWeChatKey = aShareWeChatKey;
            DirtyEditor();
            saveSettings();
        }
    }

    public string ShareWeChatUniversalLink
    {
        get { return Instance.configModel.shareWeChatUniversalLink; }
    }

    public void SetShareWeChatUniversalLink(string aShareWeChatUniversalLink)
    {
        if (Instance.configModel.shareWeChatUniversalLink != aShareWeChatUniversalLink)
        {
            Instance.configModel.shareWeChatUniversalLink = aShareWeChatUniversalLink;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool ShareWeChatTLAvailable
    {
        get { return Instance.configModel.shareWeChatTLAvailable; }
    }

    public void SetShareWeChatTLAvailable(bool aShareWeChatTLAvailable)
    {
        if (Instance.configModel.shareWeChatTLAvailable != aShareWeChatTLAvailable)
        {
            Instance.configModel.shareWeChatTLAvailable = aShareWeChatTLAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public string ShareWeChatTLKey
    {
        get { return Instance.configModel.shareWeChatTLKey; }
    }

    public void SetShareWeChatTLKey(string aShareWeChatTLKey)
    {
        if (Instance.configModel.shareWeChatTLKey != aShareWeChatTLKey)
        {
            Instance.configModel.shareWeChatTLKey = aShareWeChatTLKey;
            DirtyEditor();
            saveSettings();
        }
    }

    public string ShareWeChatTLUniversalLink
    {
        get { return Instance.configModel.shareWeChatTLUniversalLink; }
    }

    public void SetShareWeChatTLUniversalLink(string aShareWeChatTLUniversalLink)
    {
        if (Instance.configModel.shareWeChatTLUniversalLink != aShareWeChatTLUniversalLink)
        {
            Instance.configModel.shareWeChatTLUniversalLink = aShareWeChatTLUniversalLink;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool ShareWeiboAvailable
    {
        get { return Instance.configModel.shareWeiboAvailable; }
    }

    public void SetShareWeiboAvailable(bool aShareWeiboAvailable)
    {
        if (Instance.configModel.shareWeiboAvailable != aShareWeiboAvailable)
        {
            Instance.configModel.shareWeiboAvailable = aShareWeiboAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public string ShareWeiboKey
    {
        get { return Instance.configModel.shareWeiboKey; }
    }

    public void SetShareWeiboKey(string aShareWeiboKey)
    {
        if (Instance.configModel.shareWeiboKey != aShareWeiboKey)
        {
            Instance.configModel.shareWeiboKey = aShareWeiboKey;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool ShareAWEAvailable
    {
        get { return Instance.configModel.shareAWEAvailable; }
    }

    public void SetShareAWEAvailable(bool aShareAWEAvailable)
    {
        if (Instance.configModel.shareAWEAvailable != aShareAWEAvailable)
        {
            Instance.configModel.shareAWEAvailable = aShareAWEAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool ShareAWEIMAvailable
    {
        get { return Instance.configModel.shareAWEIMAvailable; }
    }

    public void SetShareAWEIMAvailable(bool aShareAWEIMAvailable)
    {
        if (Instance.configModel.shareAWEIMAvailable != aShareAWEIMAvailable)
        {
            Instance.configModel.shareAWEIMAvailable = aShareAWEIMAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public string ShareAWEKey
    {
        get { return Instance.configModel.shareAWEKey; }
    }

    public void SetShareAWEKey(string aShareAWEKey)
    {
        if (Instance.configModel.shareAWEKey != aShareAWEKey)
        {
            Instance.configModel.shareAWEKey = aShareAWEKey;
            DirtyEditor();
            saveSettings();
        }
    }

    public string ShareAWEIMKey
    {
        get { return Instance.configModel.shareAWEIMKey; }
    }

    public void SetShareAWEIMKey(string aShareAWEIMKey)
    {
        if (Instance.configModel.shareAWEIMKey != aShareAWEIMKey)
        {
            Instance.configModel.shareAWEIMKey = aShareAWEIMKey;
            DirtyEditor();
            saveSettings();
        }
    }

    public string OwnURLScheme
    {
        get { return Instance.configModel.ownURLScheme; }
    }

    public void SetOwnURLScheme(string ownURLScheme)
    {
        if (Instance.configModel.ownURLScheme != ownURLScheme)
        {
            Instance.configModel.ownURLScheme = ownURLScheme;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool ReplayClientProcessAvailable
    {
        get { return Instance.configModel.replayClientProcessAvailable; }
    }

    public void setReplayClientProcessAvailable(bool replayClientProcessAvailable)
    {
        if (Instance.configModel.replayClientProcessAvailable != replayClientProcessAvailable)
        {
            Instance.configModel.replayClientProcessAvailable = replayClientProcessAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public string ReplayAppId
    {
        get { return Instance.configModel.replayAppId; }
    }

    public void setReplayAppId(string replayAppId)
    {
        if (Instance.configModel.replayAppId != replayAppId)
        {
            Instance.configModel.replayAppId = replayAppId;
            DirtyEditor();
            saveSettings();
        }
    }

    public string ReplayLicense
    {
        get { return Instance.configModel.replayLicense; }
    }

    public void setReplayLicense(string replayLicense)
    {
        if (Instance.configModel.replayLicense != replayLicense)
        {
            Instance.configModel.replayLicense = replayLicense;
            DirtyEditor();
            saveSettings();
        }
    }

    public string IMAppID
    {
        get { return Instance.configModel.IMAppID; }
    }

    public void setIMAppID(string IMAppID)
    {
        if (Instance.configModel.IMAppID != IMAppID)
        {
            Instance.configModel.IMAppID = IMAppID;
            DirtyEditor();
            saveSettings();
        }
    }

    public string IMFrontierMethodKey
    {
        get { return Instance.configModel.IMFrontierMethodKey; }
    }

    public void setIMFrontierMethodKey(string IMFrontierMethodKey)
    {
        if (Instance.configModel.IMFrontierMethodKey != IMFrontierMethodKey)
        {
            Instance.configModel.IMFrontierMethodKey = IMFrontierMethodKey;
            DirtyEditor();
            saveSettings();
        }
    }

    public string IMFrontierServiceKey
    {
        get { return Instance.configModel.IMFrontierServiceKey; }
    }

    public void setIMFrontierServiceKey(string IMFrontierServiceKey)
    {
        if (Instance.configModel.IMFrontierServiceKey != IMFrontierServiceKey)
        {
            Instance.configModel.IMFrontierServiceKey = IMFrontierServiceKey;
            DirtyEditor();
            saveSettings();
        }
    }

    public string IMFrontierBOEBaseURL
    {
        get { return Instance.configModel.IMFrontierBOEBaseURL; }
    }

    public void setIMFrontierBOEBaseURL(string IMFrontierBOEBaseURL)
    {
        if (Instance.configModel.IMFrontierBOEBaseURL != IMFrontierBOEBaseURL)
        {
            Instance.configModel.IMFrontierBOEBaseURL = IMFrontierBOEBaseURL;
            DirtyEditor();
            saveSettings();
        }
    }

    public string IMFrontierBaseURL
    {
        get { return Instance.configModel.IMFrontierBaseURL; }
    }

    public void setIMFrontierBaseURL(string IMFrontierBaseURL)
    {
        if (Instance.configModel.IMFrontierBaseURL != IMFrontierBaseURL)
        {
            Instance.configModel.IMFrontierBaseURL = IMFrontierBaseURL;
            DirtyEditor();
            saveSettings();
        }
    }

    public string IMInboxes
    {
        get { return Instance.configModel.IMInboxes; }
    }

    public void setIMInboxes(string IMInboxes)
    {
        if (Instance.configModel.IMInboxes != IMInboxes)
        {
            Instance.configModel.IMInboxes = IMInboxes;
            DirtyEditor();
            saveSettings();
        }
    }

    public string IMFrontierID
    {
        get { return Instance.configModel.IMFrontierID; }
    }

    public void setIMFrontierID(string IMFrontierID)
    {
        if (Instance.configModel.IMFrontierID != IMFrontierID)
        {
            Instance.configModel.IMFrontierID = IMFrontierID;
            DirtyEditor();
            saveSettings();
        }
    }

    public string IMHTTPBaseURL
    {
        get { return Instance.configModel.IMHTTPBaseURL; }
    }

    public void setIMHTTPBaseURL(string IMHTTPBaseURL)
    {
        if (Instance.configModel.IMHTTPBaseURL != IMHTTPBaseURL)
        {
            Instance.configModel.IMHTTPBaseURL = IMHTTPBaseURL;
            DirtyEditor();
            saveSettings();
        }
    }

    public string IMHTTPBOEBaseURL
    {
        get { return Instance.configModel.IMHTTPBOEBaseURL; }
    }

    public void setIMHTTPBOEBaseURL(string IMHTTPBOEBaseURL)
    {
        if (Instance.configModel.IMHTTPBOEBaseURL != IMHTTPBOEBaseURL)
        {
            Instance.configModel.IMHTTPBOEBaseURL = IMHTTPBOEBaseURL;
            DirtyEditor();
            saveSettings();
        }
    }

    public string IMFrontierAppKey
    {
        get { return Instance.configModel.IMFrontierAppKey; }
    }

    public void setIMFrontierAppKey(string IMFrontierAPPKey)
    {
        if (Instance.configModel.IMFrontierAppKey != IMFrontierAPPKey)
        {
            Instance.configModel.IMFrontierAppKey = IMFrontierAPPKey;
            DirtyEditor();
            saveSettings();
        }
    }


    public bool liveAvailable
    {
        get { return Instance.configModel.liveAvailable; }
    }

    public void setLiveAvailable(bool aLiveAvailable)
    {
        if (Instance.configModel.liveAvailable != aLiveAvailable)
        {
            Instance.configModel.liveAvailable = aLiveAvailable;
            DirtyEditor();
            saveSettings();
        }
    }

    public string liveScheme
    {
        get { return Instance.configModel.liveScheme; }
    }

    public void setLiveScheme(string aLiveScheme)
    {
        if (Instance.configModel.liveScheme != aLiveScheme)
        {
            Instance.configModel.liveScheme = aLiveScheme;
            DirtyEditor();
            saveSettings();
        }
    }
    
    public string khId
    {
        get { return Instance.configModel.khId; }
    }

    public void setKhId(string aKhId)
    {
        if (Instance.configModel.khId != aKhId)
        {
            Instance.configModel.khId = aKhId;
            DirtyEditor();
            saveSettings();
        }
    }
    
    public string khSchema
    {
        get { return Instance.configModel.khSchema; }
    }

    public void setKhSchema(string aKhSchema)
    {
        if (Instance.configModel.khSchema != aKhSchema)
        {
            Instance.configModel.khSchema = aKhSchema;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool DeepLinkAvailable
    {
        get
        {
            return Instance.configModel.DeepLinkAvailable;
        }
        set
        {
            if (Instance.configModel.DeepLinkAvailable != value)
            {
                Instance.configModel.DeepLinkAvailable = value;
                DirtyEditor();
                saveSettings();
            }
        }
    }

    public int DeepLinkSchemeListSize
    {
        get
        {
            return Instance.configModel.DeepLinkSchemeListSize;
        }
        set
        {
            if (Instance.configModel.DeepLinkSchemeListSize != value)
            {
                Instance.configModel.DeepLinkSchemeListSize = value;
                DirtyEditor();
                saveSettings();
            }
        }
    }

    public List<string> DeepLinkSchemeList
    {
        get
        {
            return Instance.configModel.DeepLinkSchemeList;
        }
        set
        {
            Instance.configModel.DeepLinkSchemeList = value;
            DirtyEditor();
            saveSettings();
        }
    }

    public string DeepLinkHost
    {
        get
        {
            return Instance.configModel.DeepLinkHost;
        }
        set
        {
            Instance.configModel.DeepLinkHost = value;
            DirtyEditor();
            saveSettings();
        }
    }

#region ATT

   public bool ATTAvailable
	{
		get
		{
			return Instance.configModel.ATTAvailable;
		}
		set
		{
			Instance.configModel.ATTAvailable = value;
			DirtyEditor();
			saveSettings();
		}
	}

	public bool needCustomDefalutView
	{
		get
		{
			return Instance.configModel.needCustomDefalutView;
		}
		set
		{
			if (needCustomDefalutView)
			{
				this.useCPPopup = false;
			}
			Instance.configModel.needCustomDefalutView = value;
			DirtyEditor();
			saveSettings();
		}
	}
	
	public bool needCustomCoverView
	{
		get
		{
			return Instance.configModel.needCustomCoverView;
		}
		set
		{
			Instance.configModel.needCustomCoverView = value;
			DirtyEditor();
			saveSettings();
		}
	}

	public bool needCustomCall
	{
		get
		{
			return Instance.configModel.needCustomCall;
		}
		set
		{
			Instance.configModel.needCustomCall = value;
			DirtyEditor();
			saveSettings();
		}
	}
	
	public bool useCPPopup
	{
		get
		{
			return Instance.configModel.useCPPopup;
		}
		set
		{
			if (value)
			{
				this.needCustomDefalutView = false;
				this.needCustomCall = true;
			}
			Instance.configModel.useCPPopup = value;
			DirtyEditor();
			saveSettings();
		}
	}

    public string coverContentColor
    {
        get
        {
            return Instance.configModel.coverContentColor;
        }
        set
        {
            Instance.configModel.coverContentColor = value;
            DirtyEditor();
            saveSettings();
        }
    }

    public string coverContent
    {
        get
        {
            return Instance.configModel.coverContent;
        }
        set
        {
            Instance.configModel.coverContent = value;
            DirtyEditor();
            saveSettings();
        }
    }

    public float coverAlpha
    {
        get
        {
            return Instance.configModel.coverAlpha;
        }
        set
        {
            Instance.configModel.coverAlpha = value;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool needCover
    {
        get
        {
            return Instance.configModel.needCover;
        }
        set
        {
            Instance.configModel.needCover = value;
            DirtyEditor();
            saveSettings();
        }
    }

    public string CancelTitleColor
    {
        get
        {
            return Instance.configModel.CancelTitleColor;
        }
        set
        {
            Instance.configModel.CancelTitleColor = value;
            DirtyEditor();
            saveSettings();
        }
    }

    public string CancelColor
    {
        get
        {
            return Instance.configModel.CancelColor;
        }
        set
        {
            Instance.configModel.CancelColor = value;
            DirtyEditor();
            saveSettings();
        }
    }

    public string CancelTitle
    {
        get
        {
            return Instance.configModel.CancelTitle;
        }
        set
        {
            Instance.configModel.CancelTitle = value;
            DirtyEditor();
            saveSettings();
        }
    }

    public string ConfirmTitleColor
    {
        get
        {
            return Instance.configModel.ConfirmTitleColor;
        }
        set
        {
            Instance.configModel.ConfirmTitleColor = value;
            DirtyEditor();
            saveSettings();
        }
    }

	public bool EnableSDKAuthPoping
	{
		get
		{
			return Instance.configModel.EnableSDKAuthPoping;
		}
		set
		{
			Instance.configModel.EnableSDKAuthPoping = value;
			DirtyEditor();
			saveSettings();
		}
	}

    public string ConfirmColor
    {
        get
        {
            return Instance.configModel.ConfirmColor;
        }
        set
        {
            Instance.configModel.ConfirmColor = value;
            DirtyEditor();
            saveSettings();
        }
    }

    public string ConfirmTitle
    {
        get
        {
            return Instance.configModel.ConfirmTitle;
        }
        set
        {
            Instance.configModel.ConfirmTitle = value;
            DirtyEditor();
            saveSettings();
        }
    }

    public string Content
    {
        get
        {
            return Instance.configModel.Content;
        }
        set
        {
            Instance.configModel.Content = value;
            DirtyEditor();
            saveSettings();
        }
    }

    public string Title
    {
        get
        {
            return Instance.configModel.Title;
        }
        set
        {
            Instance.configModel.Title = value;
            DirtyEditor();
            saveSettings();
        }
    }

#endregion

#region AppScheme

    public string AppSchemeArray
    {
        get
        {
            return Instance.configModel.AppSchemeArray;
        }
        set
        {
            Instance.configModel.AppSchemeArray = value;
            DirtyEditor();
            saveSettings();
        }
    }

#endregion

    public string RatingAppStoreID
    {
        get
        {
            return Instance.configModel.RatingAppStoreID;
        }
        set
        {
            Instance.configModel.RatingAppStoreID = value;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool RTCAvailable
    {
        get
        {
            return Instance.configModel.RTCAvailable;
        }
        set
        {
            if (Instance.configModel.RTCAvailable == value) return;
            Instance.configModel.RTCAvailable = value;
            DirtyEditor();
            saveSettings();
        }
    }

    public string RTCAppId
    {
        get
        {
            return Instance.configModel.RTCAppId;
        }
        set
        {
            if (Instance.configModel.RTCAppId == value) return;
            Instance.configModel.RTCAppId = value;
            DirtyEditor();
            saveSettings();
        }
    }

    public string Skin
    {
        get
        {
            return Instance.configModel.Skin;
        }
        set
        {
            Instance.configModel.Skin = value;
            DirtyEditor();
            saveSettings();
        }
    }

    public bool IsNeedAntiAddiction
    {
        get
        {
            return Instance.configModel.IsNeedAntiAddiction;
        }
        set
        {
            Instance.configModel.IsNeedAntiAddiction = value;
            DirtyEditor();
            saveSettings();
        }
    }
    
    public bool IsNeedServiceMouth
    {
        get
        {
            return Instance.configModel.IsNeedServiceMouth;
        }
        set
        {
            Instance.configModel.IsNeedServiceMouth = value;
            DirtyEditor();
            saveSettings();
        }
    }
    
    public bool IsNeedAdPangle
    {
        get
        {
            return Instance.configModel.IsNeedAdPangle;
        }
        set
        {
            Instance.configModel.IsNeedAdPangle = value;
            DirtyEditor();
            saveSettings();
        }
    }
    
    public string PangleAppID
    {
        get
        {
            return Instance.configModel.PangleAppID;
        }
        set
        {
            Instance.configModel.PangleAppID = value;
            DirtyEditor();
            saveSettings();
        }
    }

    private void saveSettings()
    {
        string properPath = env.PATH_CONFIG_SETTINGS;
        if (!Directory.Exists(properPath))
        {
            Directory.CreateDirectory(properPath);
        }
        
        string fullPath = Path.Combine(env.PATH_CONFIG_SETTINGS,
            sdkSettingsAssetName + sdkSettingsAssetExtension
        );

        JsonWriter writer = new JsonWriter()
        {
            PrettyPrint = true,
            IndentValue = 4
        };
        
        JsonMapper.ToJson(Instance.configModel, writer);
        string json = writer.ToString();
        File.WriteAllText(fullPath, json);
    }

    private string b2s(bool value)
    {
        return value ? "true" : "false";
    }

    private void DirtyEditor()
    {
        EditorUtility.SetDirty(Instance);
    }
#endregion
}
