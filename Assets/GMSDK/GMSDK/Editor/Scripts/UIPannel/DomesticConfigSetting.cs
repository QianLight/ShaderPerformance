using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GSDK;
using UNBridgeLib.LitJson;
using UnityEngine;

public class DomesticConfigSetting : BaseSDKConfigSetting
{
    private DomesticChannelConfig androidChannelConfig = null;
    internal DomesticConfigSetting()
    {
        initPanelData();
    }

    protected override void parseAndroidConfigJsonData(string oldAndroidFullPath)
    {
        if (System.IO.File.Exists(oldAndroidFullPath))
        {
            //解析android端内容
            string configJson = File.ReadAllText(oldAndroidFullPath);
            DomesticConfig config = JsonMapper.ToObject<DomesticConfig>(configJson);
            androidAppConfig = config.app;
            androidChannelConfig = config.channel;
        }
    }

    protected override void initConfigModule()
    {
        base.initConfigModule();
        initThanosModule(androidAppConfig);
        initDeepLinkModule(androidAppConfig, iOSConfigModel);
        initLiveModule(androidAppConfig, iOSConfigModel);
        initShareConfigDomesticModule(androidAppConfig, iOSConfigModel);
        initLoginConfigModule(androidAppConfig, androidChannelConfig, iOSConfigModel);
        initADConfigModule(androidAppConfig, iOSConfigModel);
    }

    public override void androidSaveConfigToPluginAssets()
    {
        return;   
        DomesticConfig domesticConfig = new AndroidSaveConfig().androidSaveConfigToPluginAssets(_sdkConfigModule);
        string dir = Path.Combine(GMSDKEnv.Instance.PATH_PUGLIN_ANDROID, "assets");
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string destFile = Path.Combine(dir, "config.json");
        if (domesticConfig != null)
        {
            StringBuilder sb = new StringBuilder();
            JsonWriter writer = new JsonWriter(sb);
            writer.PrettyPrint = true;
            writer.IndentValue = 4;
            JsonMapper.ToJson(domesticConfig, writer);
            File.WriteAllText(destFile, sb.ToString());
        }
        else
        {
            Debug.Log("尚未添加配置，无法保存到指定目录下。");
        }
    }

    private void initThanosModule(AppConfig appConfig)
    {
        if (appConfig != null && appConfig.thanos != null)
        {
            _sdkConfigModule.thanosConfigModule.android_save_sub_channel = appConfig.thanos.saveSubChannel;
        }
    }

    private void initDeepLinkModule(AppConfig appConfig, ConfigModel iOSConfigModel)
    {
        //通用
        bool enable = _sdkConfigModule.deepLinkConfigModule.enable;
        string baseurlForFission = _sdkConfigModule.deepLinkConfigModule.baseurl_for_fission;
        List<string> schemaList = _sdkConfigModule.deepLinkConfigModule.schema_list;
        if (appConfig != null && iOSConfigModel != null)
        {
            enable = appConfig.deeplink.enable;
            baseurlForFission = universalValueOpe(appConfig.deeplink.baseUrlForFission, iOSConfigModel.DeepLinkHost);
        }
        else if (appConfig != null || iOSConfigModel != null)
        {
            enable = appConfig != null ? appConfig.deeplink.enable : iOSConfigModel.DeepLinkAvailable;
            baseurlForFission = appConfig != null ? appConfig.deeplink.baseUrlForFission : iOSConfigModel.DeepLinkHost;
        }

        List<string> androidSchemaList = new List<string>();
        List<string> iOSSchemaList = new List<string>();
        if (appConfig != null && appConfig.deeplink.schemaList != null)
        {
            androidSchemaList = appConfig.deeplink.schemaList;
        }

        if (iOSConfigModel != null && iOSConfigModel.DeepLinkSchemeList != null)
        {
            iOSSchemaList = iOSConfigModel.DeepLinkSchemeList;
        }
        
        schemaList = androidSchemaList.Union(iOSSchemaList).ToList<string>();
        _sdkConfigModule.deepLinkConfigModule.enable = enable;
        _sdkConfigModule.deepLinkConfigModule.baseurl_for_fission = baseurlForFission;
        _sdkConfigModule.deepLinkConfigModule.schema_listsize = schemaList.Count;
        _sdkConfigModule.deepLinkConfigModule.schema_list = schemaList;

        if (appConfig != null && appConfig.deeplink != null)
        {
            //android特有
            _sdkConfigModule.deepLinkConfigModule.android_host_listsize = appConfig.deeplink.hostListSize;
            _sdkConfigModule.deepLinkConfigModule.android_host_list = appConfig.deeplink.hostList;
            _sdkConfigModule.deepLinkConfigModule.android_delay_millis = appConfig.deeplink.delayMillis;
            _sdkConfigModule.deepLinkConfigModule.android_default_token_regex = appConfig.deeplink.defaultTokenRegex;
        }
    }

    private void initLiveModule(AppConfig androidAppConfig, ConfigModel iOSConfigModel)
    {
        //通用
        if (androidAppConfig != null || iOSConfigModel != null)
        {
            _sdkConfigModule.liveConfigModule.enable = androidAppConfig != null
                ? androidAppConfig.live.enable
                : iOSConfigModel.liveAvailable;
        }
        if (androidAppConfig != null && androidAppConfig.live != null)
        {
            //android特有
            _sdkConfigModule.liveConfigModule.android_douyin_key = androidAppConfig.live.douyin_key;
        }

        if (iOSConfigModel != null)
        {
            //iOS特有
            _sdkConfigModule.liveConfigModule.iOS_live_scheme = iOSConfigModel.liveScheme;
            _sdkConfigModule.liveConfigModule.iOS_kh_id = iOSConfigModel.khId;
            _sdkConfigModule.liveConfigModule.iOS_kh_scheme = iOSConfigModel.khSchema;
        }
    }

    // 分享---国内独有
    private void initShareConfigDomesticModule(AppConfig androidAppConfig, ConfigModel iOSConfigModel)
    {
        if (androidAppConfig != null)
        {
            _sdkConfigModule.shareConfigModule.android_qq_key = androidAppConfig.share.share_keys.qq;
            _sdkConfigModule.shareConfigModule.android_qq_available = !string.IsNullOrEmpty(_sdkConfigModule.shareConfigModule.android_qq_key);
            _sdkConfigModule.shareConfigModule.android_wx_key = androidAppConfig.share.share_keys.wechat;
            _sdkConfigModule.shareConfigModule.android_wx_available = !string.IsNullOrEmpty(_sdkConfigModule.shareConfigModule.android_wx_key);
            _sdkConfigModule.shareConfigModule.android_weibo_key = androidAppConfig.share.share_keys.weibo.key;
            _sdkConfigModule.shareConfigModule.android_weibo_url = androidAppConfig.share.share_keys.weibo.direct_url;
            _sdkConfigModule.shareConfigModule.android_weibo_available = !string.IsNullOrEmpty(_sdkConfigModule.shareConfigModule.android_weibo_key);
            _sdkConfigModule.shareConfigModule.android_awe_key = androidAppConfig.share.share_keys.douyin;
            _sdkConfigModule.shareConfigModule.android_awe_available = !string.IsNullOrEmpty(_sdkConfigModule.shareConfigModule.android_awe_key);
        }

        if (iOSConfigModel != null)
        {
            _sdkConfigModule.shareConfigModule.iOS_qq_available = iOSConfigModel.shareQQAvailable;
            _sdkConfigModule.shareConfigModule.iOS_qq_key = iOSConfigModel.shareQQKey;
            _sdkConfigModule.shareConfigModule.iOS_qq_universal_link = iOSConfigModel.shareQQUniversalLink;
            _sdkConfigModule.shareConfigModule.iOS_wx_available = iOSConfigModel.shareWeChatAvailable;
            _sdkConfigModule.shareConfigModule.iOS_wx_key = iOSConfigModel.shareWeChatKey;
            _sdkConfigModule.shareConfigModule.iOS_wx_universal_link = iOSConfigModel.shareWeChatUniversalLink;
            _sdkConfigModule.shareConfigModule.iOS_weibo_available = iOSConfigModel.shareWeiboAvailable;
            _sdkConfigModule.shareConfigModule.iOS_weibo_key = iOSConfigModel.shareWeiboKey;
            _sdkConfigModule.shareConfigModule.iOS_awe_available = iOSConfigModel.shareAWEAvailable;
            _sdkConfigModule.shareConfigModule.iOS_awe_key = iOSConfigModel.shareAWEKey;
        }
    }

    // 老model转新model，新增面板配置时无需在此添加
    private void initLoginConfigModule(AppConfig androidAppConfig, DomesticChannelConfig androidChannelConfig,
        ConfigModel iOSConfigModel)
    {
        //通用
        bool toutiaoLoginEnable = _sdkConfigModule.loginConfigModule.toutiao_login_enable;
        string toutiaoPlatformId = _sdkConfigModule.loginConfigModule.toutiao_platform_id;
        string toutiaoPlatformKey = _sdkConfigModule.loginConfigModule.toutiao_platform_key;
        bool toutiaoFriendPermission = _sdkConfigModule.loginConfigModule.toutiao_friend_permission;
        bool awemeLoginEnable = _sdkConfigModule.loginConfigModule.aweme_login_enable;
        string awemePlatformId = _sdkConfigModule.loginConfigModule.aweme_platform_id;
        string awemePlatformKey = _sdkConfigModule.loginConfigModule.aweme_platform_key;
        bool awemeFriendPermission = _sdkConfigModule.loginConfigModule.aweme_friend_permission;
        bool awemeVideoListDataPermission = _sdkConfigModule.loginConfigModule.aweme_video_list_data_permission;
        bool awemeRelationFollow = _sdkConfigModule.loginConfigModule.aweme_relation_follow;
        bool awemeRelationFollowDefaultCheck = _sdkConfigModule.loginConfigModule.aweme_relation_follow_default_check;
        bool awemeDyRealNamePermission = _sdkConfigModule.loginConfigModule.aweme_dy_real_name_permission;
        bool awemeMobilePermission = _sdkConfigModule.loginConfigModule.aweme_mobile_permission;
        bool awemeFriendListPermission = _sdkConfigModule.loginConfigModule.aweme_friend_list;
        bool awemeCardPermission = _sdkConfigModule.loginConfigModule.aweme_card_permission;
        bool awemeUserFollowPermission = _sdkConfigModule.loginConfigModule.aweme_relation_user_follow_permission;
        bool huoshanLoginEnable = _sdkConfigModule.loginConfigModule.huoshan_login_enable;
        string huoshanPlatformId = _sdkConfigModule.loginConfigModule.huoshan_platform_id;
        string huoshanPlatformKey = _sdkConfigModule.loginConfigModule.huoshan_platform_key;
        bool xiguaLoginEnable = _sdkConfigModule.loginConfigModule.xigua_login_enable;
        string xiguaPlatformId = _sdkConfigModule.loginConfigModule.xigua_platform_id;
        string xiguaPlatformKey = _sdkConfigModule.loginConfigModule.xigua_platform_key;

        bool aoligameLoginEnable = _sdkConfigModule.loginConfigModule.android_aoligame_login_enable;
        string aoligamePlatformId = _sdkConfigModule.loginConfigModule.android_aoligame_platform_id;

        if (androidChannelConfig != null && iOSConfigModel != null)
        {
            toutiaoLoginEnable = !string.IsNullOrEmpty(androidChannelConfig.toutiao_platform_id);
            toutiaoPlatformId = universalValueOpe(androidChannelConfig.toutiao_platform_id,
                iOSConfigModel.toutiaoLoginPlatformId);
            toutiaoPlatformKey =
                universalValueOpe(androidChannelConfig.toutiao_platform_key, iOSConfigModel.toutiaoLoginKey);
            toutiaoFriendPermission = androidChannelConfig.toutiao_friend_permission;
            awemeLoginEnable = !string.IsNullOrEmpty(androidChannelConfig.aweme_platform_id);
            awemePlatformId = universalValueOpe(androidChannelConfig.aweme_platform_id,
                iOSConfigModel.douyinLoginPlatformId);
            awemePlatformKey =
                universalValueOpe(androidChannelConfig.aweme_platform_key, iOSConfigModel.douyinLoginKey);
            awemeFriendPermission = androidChannelConfig.aweme_friend_permission;
            awemeVideoListDataPermission = androidChannelConfig.aweme_video_list_data_permission;
            awemeRelationFollow = androidChannelConfig.aweme_relation_follow;
            awemeRelationFollowDefaultCheck = androidChannelConfig.aweme_relation_follow_default_check;
            awemeDyRealNamePermission = androidChannelConfig.aweme_dy_real_name_permission;
            awemeMobilePermission = androidChannelConfig.aweme_mobile_permission;
            huoshanLoginEnable = !string.IsNullOrEmpty(androidChannelConfig.huoshan_platform_id);
            huoshanPlatformId = universalValueOpe(androidChannelConfig.huoshan_platform_id, iOSConfigModel.huoShanLoginPlatformId);
            huoshanPlatformKey =
                universalValueOpe(androidChannelConfig.huoshan_platform_key, iOSConfigModel.huoShanLoginKey);
            xiguaLoginEnable = !string.IsNullOrEmpty(androidChannelConfig.xigua_platform_id);
            xiguaPlatformId =
                universalValueOpe(androidChannelConfig.xigua_platform_id, iOSConfigModel.xiGuaLoginPlatformId);
            xiguaPlatformKey = universalValueOpe(androidChannelConfig.xigua_platform_key, iOSConfigModel.xiGuaLoginKey);
        }
        else if (androidChannelConfig != null || iOSConfigModel != null)
        {
            toutiaoLoginEnable = androidChannelConfig != null
                ? !string.IsNullOrEmpty(androidChannelConfig.toutiao_platform_id)
                : iOSConfigModel.toutiaoLoginAvailable;
            toutiaoPlatformId = androidChannelConfig != null
                ? androidChannelConfig.toutiao_platform_id
                : iOSConfigModel.toutiaoLoginPlatformId;
            toutiaoPlatformKey = androidChannelConfig != null
                    ? androidChannelConfig.toutiao_platform_key
                    : iOSConfigModel.toutiaoLoginKey;
            toutiaoFriendPermission = androidChannelConfig != null
                ? androidChannelConfig.toutiao_friend_permission
                : iOSConfigModel.toutiaoFriendPermissionAvailable;
            awemeLoginEnable = androidChannelConfig != null
                ? !string.IsNullOrEmpty(androidChannelConfig.aweme_platform_id)
                : iOSConfigModel.douyinLoginAvailable;
            awemePlatformId = androidChannelConfig != null
                ? androidChannelConfig.aweme_platform_id
                : iOSConfigModel.douyinLoginPlatformId;
            awemePlatformKey = androidChannelConfig != null
                ? androidChannelConfig.aweme_platform_key
                : iOSConfigModel.douyinLoginKey;
            awemeFriendPermission = androidChannelConfig != null
                ? androidChannelConfig.aweme_friend_permission
                : iOSConfigModel.douyinFriendPermissionAvailable;
            awemeVideoListDataPermission = androidChannelConfig != null
                ? androidChannelConfig.aweme_video_list_data_permission
                : iOSConfigModel.douyinVideoPermissionAvailable;
            awemeRelationFollow = androidChannelConfig != null
                ? androidChannelConfig.aweme_relation_follow
                : iOSConfigModel.douyinRelationFollowPermissionAvailable;
            awemeRelationFollowDefaultCheck = androidChannelConfig != null
                ? androidChannelConfig.aweme_relation_follow_default_check
                : iOSConfigModel.douyinRelationFollowDefaultCheckPermissionAvailable;
            awemeDyRealNamePermission = androidChannelConfig != null
                ? androidChannelConfig.aweme_dy_real_name_permission
                : iOSConfigModel.douyinRealNameAuthPermissionAvailable;
            awemeMobilePermission = androidChannelConfig != null
                ? androidChannelConfig.aweme_mobile_permission
                : iOSConfigModel.douyinMobilePermissionAvailable;
            awemeFriendListPermission = androidChannelConfig != null ?
                androidChannelConfig.aweme_friend_list_permission
                : iOSConfigModel.douyinFriendListPermissionAvailable;
            huoshanLoginEnable = androidChannelConfig != null
                ? !string.IsNullOrEmpty(androidChannelConfig.huoshan_platform_id)
                : iOSConfigModel.huoShanLoginAvailable;
            huoshanPlatformId = androidChannelConfig != null
                ? androidChannelConfig.huoshan_platform_id
                : iOSConfigModel.huoShanLoginPlatformId;
            huoshanPlatformKey = androidChannelConfig != null
                ? androidChannelConfig.huoshan_platform_key
                : iOSConfigModel.huoShanLoginKey;
            xiguaLoginEnable = androidChannelConfig != null
                ? !string.IsNullOrEmpty(androidChannelConfig.xigua_platform_id)
                : iOSConfigModel.xiGuaLoginAvailable;
            xiguaPlatformId = androidChannelConfig != null
                ? androidChannelConfig.xigua_platform_id
                : iOSConfigModel.xiGuaLoginPlatformId;
            xiguaPlatformKey = androidChannelConfig != null
                ? androidChannelConfig.xigua_platform_key
                : iOSConfigModel.xiGuaLoginKey;

        }
        //头条
        _sdkConfigModule.loginConfigModule.toutiao_login_enable = toutiaoLoginEnable;
        _sdkConfigModule.loginConfigModule.toutiao_platform_id = toutiaoPlatformId;
        _sdkConfigModule.loginConfigModule.toutiao_platform_key = toutiaoPlatformKey;
        _sdkConfigModule.loginConfigModule.toutiao_friend_permission = toutiaoFriendPermission;
        //抖音
        _sdkConfigModule.loginConfigModule.aweme_login_enable = awemeLoginEnable;
        _sdkConfigModule.loginConfigModule.aweme_platform_id = awemePlatformId;
        _sdkConfigModule.loginConfigModule.aweme_platform_key = awemePlatformKey;
        _sdkConfigModule.loginConfigModule.aweme_friend_permission = awemeFriendPermission;
        _sdkConfigModule.loginConfigModule.aweme_video_list_data_permission = awemeVideoListDataPermission;
        _sdkConfigModule.loginConfigModule.aweme_relation_follow = awemeRelationFollow;
        _sdkConfigModule.loginConfigModule.aweme_relation_follow_default_check = awemeRelationFollowDefaultCheck;
        _sdkConfigModule.loginConfigModule.aweme_dy_real_name_permission = awemeDyRealNamePermission;
        _sdkConfigModule.loginConfigModule.aweme_mobile_permission = awemeMobilePermission;
        //抖音火山版
        _sdkConfigModule.loginConfigModule.huoshan_login_enable = huoshanLoginEnable;
        _sdkConfigModule.loginConfigModule.huoshan_platform_id = huoshanPlatformId;
        _sdkConfigModule.loginConfigModule.huoshan_platform_key = huoshanPlatformKey;
        //西瓜视频
        _sdkConfigModule.loginConfigModule.xigua_login_enable = xiguaLoginEnable;
        _sdkConfigModule.loginConfigModule.xigua_platform_id = xiguaPlatformId;
        _sdkConfigModule.loginConfigModule.xigua_platform_key = xiguaPlatformKey;
        // 嗷哩游戏
        _sdkConfigModule.loginConfigModule.android_aoligame_login_enable = aoligameLoginEnable;
        _sdkConfigModule.loginConfigModule.android_aoligame_platform_id = aoligamePlatformId;

        
        if (androidChannelConfig != null)
        {
            
            _sdkConfigModule.loginConfigModule.aweme_video_create = androidChannelConfig.aweme_video_create;
            //TapTap
            _sdkConfigModule.loginConfigModule.android_taptap_login_enable = !string.IsNullOrEmpty(androidChannelConfig.taptap_platform_id);
            _sdkConfigModule.loginConfigModule.android_taptap_platform_id = androidChannelConfig.taptap_platform_id;
            _sdkConfigModule.loginConfigModule.android_taptap_platform_key = androidChannelConfig.taptap_platform_key;
            _sdkConfigModule.loginConfigModule.android_taptap_platform_secret = androidChannelConfig.taptap_platform_secret;
            
            //云游戏
            _sdkConfigModule.loginConfigModule.android_cloudgame_login_enable = !string.IsNullOrEmpty(androidChannelConfig.cloudgame_platform_id);
            _sdkConfigModule.loginConfigModule.android_cloudgame_platform_id = androidChannelConfig.cloudgame_platform_id;
            _sdkConfigModule.loginConfigModule.android_cloudgame_package_name = androidChannelConfig.cloudgame_package_name;

            //手机号一键登录--移动
            _sdkConfigModule.loginConfigModule.android_cm_app_id = androidChannelConfig.cm_app_id;
            _sdkConfigModule.loginConfigModule.android_cm_app_key = androidChannelConfig.cm_app_key;

            //手机号一键登录--联通
            _sdkConfigModule.loginConfigModule.android_cu_app_key = androidChannelConfig.cu_app_id;
            _sdkConfigModule.loginConfigModule.android_cu_app_secret = androidChannelConfig.cu_app_key;

            //手机号一键登录--电信
            _sdkConfigModule.loginConfigModule.android_ct_app_key = androidChannelConfig.ct_app_id;
            _sdkConfigModule.loginConfigModule.android_ct_app_secret = androidChannelConfig.ct_app_key;
        }

        if (androidAppConfig != null)
        {
            //是否显示游客
            _sdkConfigModule.loginConfigModule.android_is_need_visitor = androidAppConfig.is_need_visitor;
        }

        if (iOSConfigModel != null)
        {
            //苹果
            _sdkConfigModule.loginConfigModule.iOS_apple_login_enable = iOSConfigModel.appleLoginAvailable;
            _sdkConfigModule.loginConfigModule.iOS_apple_platform_id = iOSConfigModel.appleLoginPlatformId;

            //手机号一键登录--移动
            _sdkConfigModule.loginConfigModule.iOS_cm_app_id = iOSConfigModel.phoneMobileKey;
            _sdkConfigModule.loginConfigModule.iOS_cm_app_key = iOSConfigModel.phoneMobileSecret;

            //手机号一键登录--联通
            _sdkConfigModule.loginConfigModule.iOS_cu_app_key = iOSConfigModel.phoneUnionKey;
            _sdkConfigModule.loginConfigModule.iOS_cu_app_secret = iOSConfigModel.phoneUnionSecret;
            
            //手机号一键登录--电信
            _sdkConfigModule.loginConfigModule.iOS_ct_app_key = iOSConfigModel.phoneTelecomKey;
            _sdkConfigModule.loginConfigModule.iOS_ct_app_secret = iOSConfigModel.phoneTelecomSecret;
        }
    }

    private void initADConfigModule(AppConfig androidAppConfig, ConfigModel iOSConfigModel)
    {
        //穿山甲广告
        string pangleAppId = _sdkConfigModule.adConfigModule.pangleConfigModule.pangle_appid;
        bool enable = _sdkConfigModule.adConfigModule.pangleConfigModule.enable;
        if (androidAppConfig != null && iOSConfigModel != null)
        {
            pangleAppId = universalValueOpe(androidAppConfig.pangle.key, iOSConfigModel.PangleAppID);
            enable = androidAppConfig.pangle.enable;
        }
        else if (androidAppConfig != null || iOSConfigModel != null)
        {
            pangleAppId = androidAppConfig != null ? androidAppConfig.pangle.key : iOSConfigModel.PangleAppID;
            enable = androidAppConfig != null ? androidAppConfig.pangle.enable : iOSConfigModel.IsNeedAdPangle;
        }
        _sdkConfigModule.adConfigModule.pangleConfigModule.enable = enable;
        _sdkConfigModule.adConfigModule.pangleConfigModule.pangle_appid = pangleAppId;
    }


    #region 登录配置

    public bool login_toutiao_enable
    {
        get { return _sdkConfigModule.loginConfigModule.toutiao_login_enable; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.toutiao_login_enable != value)
            {
                _sdkConfigModule.loginConfigModule.toutiao_login_enable = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_toutiao_platform_id
    {
        get { return _sdkConfigModule.loginConfigModule.toutiao_platform_id; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.toutiao_platform_id != value)
            {
                _sdkConfigModule.loginConfigModule.toutiao_platform_id = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_toutiao_platform_key
    {
        get { return _sdkConfigModule.loginConfigModule.toutiao_platform_key; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.toutiao_platform_key != value)
            {
                _sdkConfigModule.loginConfigModule.toutiao_platform_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool login_toutiao_friend_permission
    {
        get { return _sdkConfigModule.loginConfigModule.toutiao_friend_permission; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.toutiao_friend_permission != value)
            {
                _sdkConfigModule.loginConfigModule.toutiao_friend_permission = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }


    public bool login_aoligame_enable
    {
        get { return _sdkConfigModule.loginConfigModule.android_aoligame_login_enable; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.android_aoligame_login_enable != value)
            {
                _sdkConfigModule.loginConfigModule.android_aoligame_login_enable = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }


    public string login_aoligame_platform_id
    {
        get { return _sdkConfigModule.loginConfigModule.android_aoligame_platform_id; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.android_aoligame_platform_id != value)
            {
                _sdkConfigModule.loginConfigModule.android_aoligame_platform_id = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool login_aweme_enable
    {
        get { return _sdkConfigModule.loginConfigModule.aweme_login_enable; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.aweme_login_enable != value)
            {
                _sdkConfigModule.loginConfigModule.aweme_login_enable = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_aweme_platform_id
    {
        get { return _sdkConfigModule.loginConfigModule.aweme_platform_id; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.aweme_platform_id != value)
            {
                _sdkConfigModule.loginConfigModule.aweme_platform_id = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_aweme_platform_key
    {
        get { return _sdkConfigModule.loginConfigModule.aweme_platform_key; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.aweme_platform_key != value)
            {
                _sdkConfigModule.loginConfigModule.aweme_platform_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool login_aweme_friend_permission
    {
        get { return _sdkConfigModule.loginConfigModule.aweme_friend_permission; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.aweme_friend_permission != value)
            {
                _sdkConfigModule.loginConfigModule.aweme_friend_permission = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool login_aweme_video_list_data_permission
    {
        get { return _sdkConfigModule.loginConfigModule.aweme_video_list_data_permission; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.aweme_video_list_data_permission != value)
            {
                _sdkConfigModule.loginConfigModule.aweme_video_list_data_permission = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool login_aweme_relation_follow
    {
        get { return _sdkConfigModule.loginConfigModule.aweme_relation_follow; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.aweme_relation_follow != value)
            {
                _sdkConfigModule.loginConfigModule.aweme_relation_follow = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool login_aweme_relation_follow_default_check
    {
        get { return _sdkConfigModule.loginConfigModule.aweme_relation_follow_default_check; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.aweme_relation_follow_default_check != value)
            {
                _sdkConfigModule.loginConfigModule.aweme_relation_follow_default_check = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool login_aweme_realname_permission
    {
        get { return _sdkConfigModule.loginConfigModule.aweme_dy_real_name_permission; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.aweme_dy_real_name_permission != value)
            {
                _sdkConfigModule.loginConfigModule.aweme_dy_real_name_permission = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool login_aweme_mobile_permission
    {
        get { return _sdkConfigModule.loginConfigModule.aweme_mobile_permission; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.aweme_mobile_permission != value)
            {
                _sdkConfigModule.loginConfigModule.aweme_mobile_permission = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool login_aweme_video_create
    {
        get { return _sdkConfigModule.loginConfigModule.aweme_video_create; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.aweme_video_create != value)
            {
                _sdkConfigModule.loginConfigModule.aweme_video_create = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool login_aweme_friend_list
    {
        get { return _sdkConfigModule.loginConfigModule.aweme_friend_list; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.aweme_friend_list != value)
            {
                _sdkConfigModule.loginConfigModule.aweme_friend_list = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool LoginAwemeCardPermission
    {
        get
        {
            return _sdkConfigModule.loginConfigModule.aweme_card_permission;
        }
        set
        {
            if (_sdkConfigModule.loginConfigModule.aweme_card_permission != value)
            {
                _sdkConfigModule.loginConfigModule.aweme_card_permission = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool LoginAwemeRelationUserFollowPermission
    {
        get
        {
            return _sdkConfigModule.loginConfigModule.aweme_relation_user_follow_permission;
        }
        set
        {
            if (_sdkConfigModule.loginConfigModule.aweme_relation_user_follow_permission != value)
            {
                _sdkConfigModule.loginConfigModule.aweme_relation_user_follow_permission = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool LoginAwemeUserExternalDataPermission
    {
        get { return _sdkConfigModule.loginConfigModule.aweme_user_external_data_permission; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.aweme_user_external_data_permission != value)
            {
                _sdkConfigModule.loginConfigModule.aweme_user_external_data_permission = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool login_huoshan_enable
    {
        get { return _sdkConfigModule.loginConfigModule.huoshan_login_enable; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.huoshan_login_enable != value)
            {
                _sdkConfigModule.loginConfigModule.huoshan_login_enable = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_huoshan_platform_id
    {
        get { return _sdkConfigModule.loginConfigModule.huoshan_platform_id; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.huoshan_platform_id != value)
            {
                _sdkConfigModule.loginConfigModule.huoshan_platform_id = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_huoshan_platform_key
    {
        get { return _sdkConfigModule.loginConfigModule.huoshan_platform_key; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.huoshan_platform_key != value)
            {
                _sdkConfigModule.loginConfigModule.huoshan_platform_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool login_xigua_enable
    {
        get { return _sdkConfigModule.loginConfigModule.xigua_login_enable; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.xigua_login_enable != value)
            {
                _sdkConfigModule.loginConfigModule.xigua_login_enable = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_xigua_platform_id
    {
        get { return _sdkConfigModule.loginConfigModule.xigua_platform_id; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.xigua_platform_id != value)
            {
                _sdkConfigModule.loginConfigModule.xigua_platform_id = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_xigua_platform_key
    {
        get { return _sdkConfigModule.loginConfigModule.xigua_platform_key; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.xigua_platform_key != value)
            {
                _sdkConfigModule.loginConfigModule.xigua_platform_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public int login_priority
    {
        get { return _sdkConfigModule.loginConfigModule.login_priority; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.login_priority != value)
            {
                _sdkConfigModule.loginConfigModule.login_priority = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    #region Android独有

    public bool login_android_taptap_enable
    {
        get { return _sdkConfigModule.loginConfigModule.android_taptap_login_enable; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.android_taptap_login_enable != value)
            {
                _sdkConfigModule.loginConfigModule.android_taptap_login_enable = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_android_taptap_platform_id
    {
        get { return _sdkConfigModule.loginConfigModule.android_taptap_platform_id; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.android_taptap_platform_id != value)
            {
                _sdkConfigModule.loginConfigModule.android_taptap_platform_id = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_android_taptap_platform_key
    {
        get { return _sdkConfigModule.loginConfigModule.android_taptap_platform_key; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.android_taptap_platform_key != value)
            {
                _sdkConfigModule.loginConfigModule.android_taptap_platform_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_android_taptap_platform_secret
    {
        get
        {
            return _sdkConfigModule.loginConfigModule.android_taptap_platform_secret;
        }
        set
        {
            if (_sdkConfigModule.loginConfigModule.android_taptap_platform_secret != value)
            {
                _sdkConfigModule.loginConfigModule.android_taptap_platform_secret = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    
    public bool login_android_cloudgame_enable
    {
        get { return _sdkConfigModule.loginConfigModule.android_cloudgame_login_enable; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.android_cloudgame_login_enable != value)
            {
                _sdkConfigModule.loginConfigModule.android_cloudgame_login_enable = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_android_cloudgame_platform_id
    {
        get { return _sdkConfigModule.loginConfigModule.android_cloudgame_platform_id; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.android_cloudgame_platform_id != value)
            {
                _sdkConfigModule.loginConfigModule.android_cloudgame_platform_id = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public string login_android_cloudgame_package_name
    {
        get { return _sdkConfigModule.loginConfigModule.android_cloudgame_package_name; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.android_cloudgame_package_name != value)
            {
                _sdkConfigModule.loginConfigModule.android_cloudgame_package_name = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_android_cm_app_id
    {
        get { return _sdkConfigModule.loginConfigModule.android_cm_app_id; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.android_cm_app_id != value)
            {
                _sdkConfigModule.loginConfigModule.android_cm_app_id = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_android_cm_app_key
    {
        get { return _sdkConfigModule.loginConfigModule.android_cm_app_key; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.android_cm_app_key != value)
            {
                _sdkConfigModule.loginConfigModule.android_cm_app_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_android_cu_app_key
    {
        get { return _sdkConfigModule.loginConfigModule.android_cu_app_key; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.android_cu_app_key != value)
            {
                _sdkConfigModule.loginConfigModule.android_cu_app_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_android_cu_app_secret
    {
        get { return _sdkConfigModule.loginConfigModule.android_cu_app_secret; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.android_cu_app_secret != value)
            {
                _sdkConfigModule.loginConfigModule.android_cu_app_secret = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool login_android_is_need_visitor
    {
        get { return _sdkConfigModule.loginConfigModule.android_is_need_visitor; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.android_is_need_visitor != value)
            {
                _sdkConfigModule.loginConfigModule.android_is_need_visitor = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_android_ct_app_key
    {
        get
        {
            return _sdkConfigModule.loginConfigModule.android_ct_app_key;
        }
        set
        {
            if (_sdkConfigModule.loginConfigModule.android_ct_app_key != value)
            {
                _sdkConfigModule.loginConfigModule.android_ct_app_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public string login_android_ct_app_secret
    {
        get
        {
            return _sdkConfigModule.loginConfigModule.android_ct_app_secret;
        }
        set
        {
            if (_sdkConfigModule.loginConfigModule.android_ct_app_secret != value)
            {
                _sdkConfigModule.loginConfigModule.android_ct_app_secret = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public bool android_user_info_no_db_default
    {
        get { return _sdkConfigModule.loginConfigModule.android_user_info_no_db_default; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.android_user_info_no_db_default != value)
            {
                _sdkConfigModule.loginConfigModule.android_user_info_no_db_default = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public bool android_fusion_info_no_db_default
    {
        get { return _sdkConfigModule.loginConfigModule.android_fusion_user_no_db_default; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.android_fusion_user_no_db_default != value)
            {
                _sdkConfigModule.loginConfigModule.android_fusion_user_no_db_default = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    #endregion

    #region iOS独有

    public bool login_iOS_apple_enable
    {
        get { return _sdkConfigModule.loginConfigModule.iOS_apple_login_enable; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.iOS_apple_login_enable != value)
            {
                _sdkConfigModule.loginConfigModule.iOS_apple_login_enable = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_iOS_apple_platform_id
    {
        get { return _sdkConfigModule.loginConfigModule.iOS_apple_platform_id; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.iOS_apple_platform_id != value)
            {
                _sdkConfigModule.loginConfigModule.iOS_apple_platform_id = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_iOS_cm_app_id
    {
        get { return _sdkConfigModule.loginConfigModule.iOS_cm_app_id; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.iOS_cm_app_id != value)
            {
                _sdkConfigModule.loginConfigModule.iOS_cm_app_id = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_iOS_cm_app_key
    {
        get { return _sdkConfigModule.loginConfigModule.iOS_cm_app_key; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.iOS_cm_app_key != value)
            {
                _sdkConfigModule.loginConfigModule.iOS_cm_app_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_iOS_cu_app_key
    {
        get { return _sdkConfigModule.loginConfigModule.iOS_cu_app_key; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.iOS_cu_app_key != value)
            {
                _sdkConfigModule.loginConfigModule.iOS_cu_app_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_iOS_cu_app_secret
    {
        get { return _sdkConfigModule.loginConfigModule.iOS_cu_app_secret; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.iOS_cu_app_secret != value)
            {
                _sdkConfigModule.loginConfigModule.iOS_cu_app_secret = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }
    
    public string login_iOS_ct_app_key
    {
        get { return _sdkConfigModule.loginConfigModule.iOS_ct_app_key; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.iOS_ct_app_key != value)
            {
                _sdkConfigModule.loginConfigModule.iOS_ct_app_key = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }

    public string login_iOS_ct_app_secret
    {
        get { return _sdkConfigModule.loginConfigModule.iOS_ct_app_secret; }
        set
        {
            if (_sdkConfigModule.loginConfigModule.iOS_ct_app_secret != value)
            {
                _sdkConfigModule.loginConfigModule.iOS_ct_app_secret = value;
                DirtyEditor();
                SaveConfigSettings();
            }
        }
    }


    #endregion

    #endregion

}
