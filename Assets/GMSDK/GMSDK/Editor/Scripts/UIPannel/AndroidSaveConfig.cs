using System.Collections.Generic;

public class AndroidSaveConfig
{
    private string appChannel = "bsdk";



    public DomesticConfig androidSaveConfigToPluginAssets(SDKConfigModule sdkConfigModule)
    {
        //将_sdkConfigModule转换成DomesticConfig
        DomesticConfig domesticConfig = null;
        if (sdkConfigModule != null)
        {
            domesticConfig = new DomesticConfig();
            //通用配置
            domesticConfig.app = initAppConfig(sdkConfigModule);
            domesticConfig.channel = initDomesticChannelConfig(sdkConfigModule.loginConfigModule);
        }

        return domesticConfig;
    }

    private AppConfig initAppConfig(SDKConfigModule sdkConfigModule)
    {
        AppConfig appConfig = new AppConfig();
        appConfig.app_id = ConfigSettingParamHandleUtils.GetAndroidStringConfigValue(sdkConfigModule.commonModule.app_id);
        appConfig.channel = appChannel;
        appConfig.gsdk_init_timeout = ConfigSettingParamHandleUtils.GetAndroidLongConfigValue(sdkConfigModule.commonModule.init_timeout);
        appConfig.device_register_optimization = sdkConfigModule.commonModule.drOptimization;
        appConfig.device_register_retry_count = sdkConfigModule.commonModule.drRetryCount;
        appConfig.gsdk_account_init_timeout = ConfigSettingParamHandleUtils.GetAndroidLongConfigValue(sdkConfigModule.commonModule.account_init_timeout);
        appConfig.screen_orientation = sdkConfigModule.commonModule.android_screen_orientation;
        appConfig.package_name = sdkConfigModule.commonModule.package_name;
        appConfig.main_activity = sdkConfigModule.commonModule.android_main_activity;
        appConfig.use_packet_channel = sdkConfigModule.commonModule.use_packet_channel;
        appConfig.gsdk_request_cloud_no_db_default = sdkConfigModule.commonModule.android_request_cloud_no_db_default;
        appConfig.open_launch_trace = sdkConfigModule.commonModule.android_open_launch_trace;
        appConfig.app_name = ConfigSettingParamHandleUtils.GetAndroidStringConfigValue(sdkConfigModule.commonModule.app_name);
        appConfig.download_source = sdkConfigModule.commonModule.download_source;
        appConfig.has_splash = sdkConfigModule.commonModule.android_has_splash;
        appConfig.splash_interval = sdkConfigModule.commonModule.android_splash_interval;
        appConfig.is_need_privacy_protection =
            sdkConfigModule.privacySecurityConfigModule.android_is_need_privacy_protection;
        appConfig.is_need_anti_addiction = sdkConfigModule.commonModule.is_need_anti_addiction;
        appConfig.is_need_service_mouth = sdkConfigModule.commonModule.is_need_service_mouth;
        appConfig.is_need_visitor = sdkConfigModule.loginConfigModule.android_is_need_visitor;
        appConfig.skin = sdkConfigModule.commonModule.skin;
        appConfig.clean_app = sdkConfigModule.commonModule.android_clean_app;
        appConfig.is_show_toast = sdkConfigModule.privacySecurityConfigModule.is_show_permission_toast;
        appConfig.is_debug = sdkConfigModule.commonModule.android_is_debug;


        //share
        appConfig.share.panelId = sdkConfigModule.shareConfigModule.android_share_panel_id;
        if (sdkConfigModule.shareConfigModule.share_dim_amount_available)
        {
            appConfig.share.dimAmount = ConfigSettingParamHandleUtils.GetAndroidStringConfigValue(sdkConfigModule.shareConfigModule.share_dim_amount);
        }
        else
        {
            appConfig.share.dimAmount = "";
        }
        if (sdkConfigModule.shareConfigModule.android_wx_available)
        {
            appConfig.share.share_keys.wechat = sdkConfigModule.shareConfigModule.android_wx_key;
        }
        else
        {
            appConfig.share.share_keys.wechat = "";
        }

        if (sdkConfigModule.shareConfigModule.android_qq_available)
        {
            appConfig.share.share_keys.qq = sdkConfigModule.shareConfigModule.android_qq_key;
        }
        else
        {
            appConfig.share.share_keys.qq = "";
        }

        if (sdkConfigModule.shareConfigModule.android_awe_available)
        {
            appConfig.share.share_keys.douyin = sdkConfigModule.shareConfigModule.android_awe_key;
        }
        else
        {
            appConfig.share.share_keys.douyin = "";
        }

        if (sdkConfigModule.shareConfigModule.android_weibo_available)
        {
            appConfig.share.share_keys.weibo.key = sdkConfigModule.shareConfigModule.android_weibo_key;
            appConfig.share.share_keys.weibo.direct_url =
                sdkConfigModule.shareConfigModule.android_weibo_url;
        }
        else
        {
            appConfig.share.share_keys.weibo.key = "";
            appConfig.share.share_keys.weibo.direct_url = "";
        }

        //push
        if (sdkConfigModule.pushConfigModule.push_enable)
        {
            appConfig.push.enable = sdkConfigModule.pushConfigModule.push_enable;
            appConfig.push.push_app_name = sdkConfigModule.pushConfigModule.android_push_app_name;
        }
        else
        {
            appConfig.push.enable = false;
            appConfig.push.push_app_name = null;
        }
        
        //IM
        if (sdkConfigModule.imConfigModule.enable)
        {
            appConfig.im.enable = sdkConfigModule.imConfigModule.enable;
            appConfig.im.aid = ConfigSettingParamHandleUtils.GetAndroidIntConfigValue(sdkConfigModule.imConfigModule.aid);
            appConfig.im.method = ConfigSettingParamHandleUtils.GetAndroidIntConfigValue(sdkConfigModule.imConfigModule.method);
            appConfig.im.service = ConfigSettingParamHandleUtils.GetAndroidIntConfigValue(sdkConfigModule.imConfigModule.service);
            appConfig.im.wsHostBoe = ConfigSettingParamHandleUtils.GetAndroidStringConfigValue(sdkConfigModule.imConfigModule.ws_host_boe);
            appConfig.im.wsHost = ConfigSettingParamHandleUtils.GetAndroidStringConfigValue(sdkConfigModule.imConfigModule.ws_host);
            appConfig.im.inboxes = sdkConfigModule.imConfigModule.inboxes;
            appConfig.im.fpid = ConfigSettingParamHandleUtils.GetAndroidIntConfigValue(sdkConfigModule.imConfigModule.fpid);
            appConfig.im.httpHost = ConfigSettingParamHandleUtils.GetAndroidStringConfigValue(sdkConfigModule.imConfigModule.http_host);
            appConfig.im.httpHostBoe = ConfigSettingParamHandleUtils.GetAndroidStringConfigValue(sdkConfigModule.imConfigModule.http_host_boe);
            appConfig.im.appKey = ConfigSettingParamHandleUtils.GetAndroidStringConfigValue(sdkConfigModule.imConfigModule.app_key);
        }
        else
        {
            appConfig.im.enable = false;
            appConfig.im.aid = 0;
            appConfig.im.method = 0;
            appConfig.im.service = 0;
            appConfig.im.wsHostBoe = "";
            appConfig.im.wsHost = "";
            appConfig.im.inboxes = null;
            appConfig.im.fpid = 0;
            appConfig.im.httpHost = "";
            appConfig.im.httpHostBoe = "";
            appConfig.im.appKey = "";
        }


        //liveConfig
        if (sdkConfigModule.liveConfigModule.enable)
        {
            appConfig.live.enable = sdkConfigModule.liveConfigModule.enable;
            appConfig.live.douyin_key = sdkConfigModule.liveConfigModule.android_douyin_key;
        }
        else
        {
            appConfig.live.enable = false;
            appConfig.live.douyin_key = null;
        }
        
        //deeplinkConfig
        if (sdkConfigModule.deepLinkConfigModule.enable)
        {
            appConfig.deeplink.enable = sdkConfigModule.deepLinkConfigModule.enable;
            appConfig.deeplink.schemaListSize = sdkConfigModule.deepLinkConfigModule.schema_listsize;
            appConfig.deeplink.schemaList = sdkConfigModule.deepLinkConfigModule.schema_list;
            appConfig.deeplink.delayMillis = sdkConfigModule.deepLinkConfigModule.android_delay_millis;
            appConfig.deeplink.hostListSize = sdkConfigModule.deepLinkConfigModule.android_host_listsize;
            appConfig.deeplink.hostList = sdkConfigModule.deepLinkConfigModule.android_host_list;
            appConfig.deeplink.baseUrlForFission =
                ConfigSettingParamHandleUtils.GetAndroidStringConfigValue(sdkConfigModule.deepLinkConfigModule
                    .baseurl_for_fission);
            appConfig.deeplink.defaultTokenRegex = sdkConfigModule.deepLinkConfigModule.android_default_token_regex;
        }
        else
        {
            appConfig.deeplink.enable = false;
            appConfig.deeplink.schemaListSize = 0;
            appConfig.deeplink.schemaList = new List<string>();
            appConfig.deeplink.delayMillis = 0;
            appConfig.deeplink.hostListSize = 0;
            appConfig.deeplink.hostList = new List<string>();
            appConfig.deeplink.baseUrlForFission = null;
            appConfig.deeplink.defaultTokenRegex = new List<string>();
        }


        //thanos
        appConfig.thanos.saveSubChannel = sdkConfigModule.thanosConfigModule.android_save_sub_channel;


        //rtc
        if (sdkConfigModule.rtcConfigModule.rtc_available)
        {
            appConfig.rtc.enable = sdkConfigModule.rtcConfigModule.rtc_available;
            appConfig.rtc.rtcAppId = ConfigSettingParamHandleUtils.GetAndroidStringConfigValue(sdkConfigModule.rtcConfigModule.rtc_app_id);
        }
        else
        {
            appConfig.rtc.enable = false;
            appConfig.rtc.rtcAppId = null;
        }


        //ironsource
        //pangle穿山甲
        if (sdkConfigModule.adConfigModule.pangleConfigModule.enable)
        {
            appConfig.pangle.enable = sdkConfigModule.adConfigModule.pangleConfigModule.enable;
            appConfig.pangle.key = ConfigSettingParamHandleUtils.GetAndroidStringConfigValue(sdkConfigModule.adConfigModule.pangleConfigModule.pangle_appid);
        }
        else
        {
            appConfig.pangle.enable = false;
            appConfig.pangle.key = null;
        }

        
        return appConfig;
    }



    private DomesticChannelConfig initDomesticChannelConfig(LoginConfigModule loginConfigModule)
    {
        DomesticChannelConfig domesticChannelConfig = new DomesticChannelConfig();
        if (loginConfigModule.aweme_login_enable)
        {
            domesticChannelConfig.aweme_platform_id = ConfigSettingParamHandleUtils.GetAndroidStringConfigValue(loginConfigModule.aweme_platform_id);
            domesticChannelConfig.aweme_platform_key = ConfigSettingParamHandleUtils.GetAndroidStringConfigValue(loginConfigModule.aweme_platform_key);
            domesticChannelConfig.aweme_friend_permission = loginConfigModule.aweme_friend_permission;
            domesticChannelConfig.aweme_video_list_data_permission =
                loginConfigModule.aweme_video_list_data_permission;
            domesticChannelConfig.aweme_relation_follow = loginConfigModule.aweme_relation_follow;
            domesticChannelConfig.aweme_relation_follow_default_check =
                loginConfigModule.aweme_relation_follow_default_check;
            domesticChannelConfig.aweme_dy_real_name_permission =
                loginConfigModule.aweme_dy_real_name_permission;
            domesticChannelConfig.aweme_mobile_permission =
                loginConfigModule.aweme_mobile_permission;
            domesticChannelConfig.aweme_video_create = loginConfigModule.aweme_video_create;
            domesticChannelConfig.aweme_friend_list_permission = loginConfigModule.aweme_friend_list;
            domesticChannelConfig.aweme_card_permission = loginConfigModule.aweme_card_permission;
            domesticChannelConfig.aweme_relation_user_follow_permission = loginConfigModule.aweme_relation_user_follow_permission;
            domesticChannelConfig.aweme_user_external_data_permission =
                loginConfigModule.aweme_user_external_data_permission;
        }
        else
        {
            domesticChannelConfig.aweme_platform_id = "";
            domesticChannelConfig.aweme_platform_key = "";
            domesticChannelConfig.aweme_friend_permission = false;
            domesticChannelConfig.aweme_video_list_data_permission = false;
            domesticChannelConfig.aweme_relation_follow = false;
            domesticChannelConfig.aweme_relation_follow_default_check = false;
            domesticChannelConfig.aweme_dy_real_name_permission = false;
            domesticChannelConfig.aweme_mobile_permission = false;
            domesticChannelConfig.aweme_video_create = false;
            domesticChannelConfig.aweme_friend_list_permission = false;
        }

        if (loginConfigModule.toutiao_login_enable)
        {
            domesticChannelConfig.toutiao_platform_id = ConfigSettingParamHandleUtils.GetAndroidStringConfigValue(loginConfigModule.toutiao_platform_id);
            domesticChannelConfig.toutiao_platform_key = ConfigSettingParamHandleUtils.GetAndroidStringConfigValue(loginConfigModule.toutiao_platform_key);
            domesticChannelConfig.toutiao_friend_permission =
                loginConfigModule.toutiao_friend_permission;
        }
        else
        {
            domesticChannelConfig.toutiao_platform_id = "";
            domesticChannelConfig.toutiao_platform_key = "";
            domesticChannelConfig.toutiao_friend_permission = false;
        }

        if (loginConfigModule.android_aoligame_login_enable)
        {
            domesticChannelConfig.aoligame_platform_id = loginConfigModule.android_aoligame_platform_id;
        }
        else
        {
            domesticChannelConfig.aoligame_platform_id = "";
        }

        if (loginConfigModule.huoshan_login_enable)
        {
            domesticChannelConfig.huoshan_platform_id = ConfigSettingParamHandleUtils.GetAndroidStringConfigValue(loginConfigModule.huoshan_platform_id);
            domesticChannelConfig.huoshan_platform_key = ConfigSettingParamHandleUtils.GetAndroidStringConfigValue(loginConfigModule.huoshan_platform_key);
        }
        else
        {
            domesticChannelConfig.huoshan_platform_id = "";
            domesticChannelConfig.huoshan_platform_key = "";
        }

        if (loginConfigModule.xigua_login_enable)
        {
            domesticChannelConfig.xigua_platform_id = ConfigSettingParamHandleUtils.GetAndroidStringConfigValue(loginConfigModule.xigua_platform_id);
            domesticChannelConfig.xigua_platform_key = ConfigSettingParamHandleUtils.GetAndroidStringConfigValue(loginConfigModule.xigua_platform_key);
        }
        else
        {
            domesticChannelConfig.xigua_platform_id = "";
            domesticChannelConfig.xigua_platform_key = "";
        }

        if (loginConfigModule.android_taptap_login_enable)
        {
            domesticChannelConfig.taptap_platform_id =
                loginConfigModule.android_taptap_platform_id;
            domesticChannelConfig.taptap_platform_key =
                loginConfigModule.android_taptap_platform_key;
            domesticChannelConfig.taptap_platform_secret =
                loginConfigModule.android_taptap_platform_secret;
        }
        else
        {
            domesticChannelConfig.taptap_platform_id = "";
            domesticChannelConfig.taptap_platform_key = "";
            domesticChannelConfig.taptap_platform_secret = "";
        }

        if (loginConfigModule.android_cloudgame_login_enable)
        {
            domesticChannelConfig.cloudgame_platform_id =
                loginConfigModule.android_cloudgame_platform_id;
            domesticChannelConfig.cloudgame_package_name =
                loginConfigModule.android_cloudgame_package_name;
        }
        else
        {
            domesticChannelConfig.cloudgame_platform_id = "";
            domesticChannelConfig.cloudgame_package_name = "";
        }


        domesticChannelConfig.cm_app_id = loginConfigModule.android_cm_app_id;
        domesticChannelConfig.cm_app_key = loginConfigModule.android_cm_app_key;
        domesticChannelConfig.cu_app_id = loginConfigModule.android_cu_app_key;
        domesticChannelConfig.cu_app_key = loginConfigModule.android_cu_app_secret;
        domesticChannelConfig.ct_app_id = loginConfigModule.android_ct_app_key;
        domesticChannelConfig.ct_app_key = loginConfigModule.android_ct_app_secret;
        domesticChannelConfig.login_panel_can_be_closed = loginConfigModule.login_panel_can_be_closed;
        domesticChannelConfig.gsdk_user_info_no_db_default = loginConfigModule.android_user_info_no_db_default;
        domesticChannelConfig.gsdk_fusion_user_no_db_default = loginConfigModule.android_fusion_user_no_db_default;
        domesticChannelConfig.login_priority = loginConfigModule.login_priority.ToString();
        return domesticChannelConfig;
    }
}