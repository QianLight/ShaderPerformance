using System.Collections.Generic;using System.Runtime.InteropServices;

public class SDKConfigModule
{
    // 通用配置
    public CommonModule commonModule = new CommonModule();
    // 隐私安全配置
    public PrivacySecurityConfigModule privacySecurityConfigModule = new PrivacySecurityConfigModule();
    // 登录配置
    public LoginConfigModule loginConfigModule = new LoginConfigModule();
    // 分享配置
    public ShareConfigModule shareConfigModule = new ShareConfigModule();
    // push配置
    public PushConfigModule pushConfigModule = new PushConfigModule();
    // 应用商店评价配置
    public RatingConfigModule ratingConfigModule = new RatingConfigModule();
    // DeepLink配置
    public DeepLinkConfigModule deepLinkConfigModule = new DeepLinkConfigModule();
    // IM配置
    public IMConfigModule imConfigModule = new IMConfigModule();
    // 直播配置
    public LiveConfigModule liveConfigModule = new LiveConfigModule();
    // RTC配置
    public RTCConfigModule rtcConfigModule = new RTCConfigModule();

    // 广告付费配置（广告变现、IronSource、穿山甲）
    public ADConfigModule adConfigModule = new ADConfigModule();

    // Thanos配置
    public ThanosConfigModule thanosConfigModule = new ThanosConfigModule();
    //面板语言
    public PannelSettingsModule pannelSettingsModule = new PannelSettingsModule();
}

// 通用配置
public class CommonModule
{
    public bool needModifyInfoPlist = true; // 需要gsdk修改info.plist
    public bool modifyInfoPlistReplaceWhenMetSame = true; // 修改info.plist时，如果碰到已经存在的key时，直接替换还是忽略本次
    public string app_id = ""; //appId
    public string app_name = ""; //appName
    public string package_name = ""; //packageName(android)
    public string iOS_bundleId = ""; //bundleID(iOS)
    public string init_timeout = "30000"; //初始化超时时间
    public bool drOptimization = false; //初始化非阻塞优化，默认关
    public string account_init_timeout = "5000"; //初始化非阻塞优化打开时，帐号初始化did的超时时间
    public int drRetryCount = 100; //内部设备注册重试次数
    public bool use_packet_channel = true; //游戏安全第二通道
    public string download_source = "";
    public string skin = "purple"; //换肤
    public bool is_need_anti_addiction; //是否开启防沉迷
    public bool is_need_service_mouth; //实名弹窗是否需要展示游戏客服链接入口
    public bool android_has_splash; //是否需要闪屏                                   
    public int android_splash_interval; //闪屏展示时间

    #region Android独有

    public string android_screen_orientation = "sensorLandscape"; //横竖屏配置
    public string android_main_activity = ""; //mainActivity
    public bool android_open_launch_trace; //launchTrace
    public bool android_is_debug = false; 
    public bool android_clean_app; //纯净版app
    public bool android_request_cloud_no_db_default = false;
    
    #endregion

    
    #region iOS独有
    
    public string iOS_app_display_name = ""; //appDisplayName
    public bool iOS_import_magicbox = false; //导入MagicBox，仅用于测试，上线前必须要移除(iOS)
    public bool iOS_dsym_auto_upload = true; //开启自动上传dsym符号表文件，会导致打包流程变长（iOS）
    public string iOS_dsym_upload_key = ""; //开启自动上传dsym符号表文件，会导致打包流程变长（iOS）
    public string iOS_project_target_name = "Unity-iPhone"; //Unity工程名称，如没修改过可无视（iOS）
    public string iOS_framework_target_name = "UnityFramework"; //Unity动态库名称，如有则填写（iOS）
    public int iOS_associated_domains_list_size; //Xcode工程中的Associated Domains List Size(用于Universal Link拉起) （iOS），可填写的数量
    public List<string> iOS_associated_domains_list; //Xcode工程中的Associated Domains List （用于Universal Link拉起）（iOS）
    public int iOS_own_url_scheme_list_size;  // 外部唤起通用URL schemes list size，可填写的数量
    public List<string> iOS_own_url_scheme_list;    // 外部唤起通用URL schemes list
    public int iOS_application_queries_schemes_list_size;  // 允许探测的APPSchemes list size，可填写的数量
    public List<string> iOS_application_queries_schemes_list;    // 允许探测的APPSchemes list
    public bool iOS_cert_success = false;   // XCodePostProcess文件里需要
    public string iOS_provisioning_guid = "不自动修改"; //Provisioning GUID, 根据文件自动生成
    public string iOS_team_ID = "不自动修改"; //Team ID，根据文件自动生成
    public string iOS_cert_name = "不自动修改"; //证书名称，根据文件自动生成
    public bool iOS_debug_cert_success = false;   // XCodePostProcess文件里需要
    public string iOS_debug_provisioning_guid = "不自动修改"; //[Debug环境] Provisioning GUID, 根据文件自动生成
    public string iOS_debug_team_ID = "不自动修改"; //[Debug环境] Team ID，根据文件自动生成
    public string iOS_debug_cert_name = "不自动修改"; //[Debug环境] 证书名称，根据文件自动生成
    
    #endregion
}

// 隐私安全配置
public class PrivacySecurityConfigModule
{
    public bool is_show_permission_toast = true;    // 是否允许GSDK权限弹窗

    #region Android独有

     public bool android_is_need_privacy_protection = false;    // 是否开启个人信息保护指引
    #endregion

    #region iOS独有

    public bool iOS_privacy_microphone_usage_available = false;
    public string iOS_privacy_microphone_usage_desc = "申请麦克风权限，用于发送语音消息";
    public bool iOS_privacy_photo_usage_available = true;
    public string iOS_privacy_photo_usage_desc = "申请访问相册，用于社交分享";
    public bool iOS_privacy_photo_additions_usage_available = true;
    public string iOS_privacy_photo_additions_usage_desc = "申请存储图片到相册，用于社交分享";
    public bool iOS_privacy_camera_usage_available = false;
    public string iOS_privacy_camera_usage_desc = "申请拍照权限，用于社交分享";
    public bool iOS_privacy_contacts_usage_available = false;
    public string iOS_privacy_contacts_usage_desc = "需要您的通讯录权限，进行好友推荐";
    public bool iOS_privacy_app_tracking_usage_available = true;
    public string iOS_privacy_app_tracking_usage_desc = "请放心，我们无法获取你在其他应用内的隐私，开启权限仅用于标识设备并保障服务安全与提升游戏体验";
    public bool iOS_privacy_location_usage_available = true;
    public string iOS_privacy_location_usage_desc = "申请定位权限，用于必要情况下获取粗略定位";
    public bool iOS_privacy_location_always_usage_available = false;
    public string iOS_privacy_location_always_usage_desc = "申请定位权限，用于必要情况下获取连续定位";
    public bool iOS_privacy_calendars_usage_available = false;
    public string iOS_privacy_calendars_usage_desc = "申请日历权限，用于设置活动日程";
    public bool iOS_privacy_reminders_usage_available = false;
    public string iOS_privacy_reminders_usage_desc = "申请备忘录权限，用于设置活动提醒";
    public bool iOS_privacy_speech_recognition_usage_available = false;
    public string iOS_privacy_speech_recognition_usage_desc = "申请语音识别权限，以启用系统的语音识别功能";
    public bool iOS_privacy_siri_usage_available = false;
    public string iOS_privacy_siri_usage_desc = "申请Siri权限，以启用Siri辅助功能";
    public bool iOS_privacy_face_ID_usage_available = false;
    public string iOS_privacy_face_ID_usage_desc = "申请Face ID权限，以启用Face ID功能";
    
    
    #region ATT idfa弹窗

    public bool iOS_is_ATT_available = false;    // 默认能力已接入，是否需要自定义ATT弹窗能力
    public bool iOS_is_need_DIY_call;    // 自定义弹窗时机
    public bool iOS_is_use_CP_DIY_view;    // 使用开发者自定义的引导弹窗
    public bool iOS_is_need_gsdk_defalut_view;    // 自定义GSDK提供的默认引导弹窗UI
    public string iOS_gsdk_defalut_view_cancel_title_color = "";
    public string iOS_gsdk_defalut_view_cancel_color = "";
    public string iOS_gsdk_defalut_view_cancel_title = "";
    public string iOS_gsdk_defalut_view_confirm_title_color = "";
    public string iOS_gsdk_defalut_view_confirm_color = "";
    public string iOS_gsdk_defalut_view_confirm_title = "";
    public string iOS_gsdk_defalut_view_content = "";
    public string iOS_gsdk_defalut_view_title = "";

    #endregion
    
    #endregion
}

// 登录配置
public class LoginConfigModule
{
    public bool toutiao_login_enable = false;

    // 头条授权登录的id
    public string toutiao_platform_id = "";

    // 头条授权登录的key
    public string toutiao_platform_key = "";

    //是否开启头条好友权限
    public bool toutiao_friend_permission = false;


    public bool aweme_login_enable = false;

    // 抖音授权登录的id
    public string aweme_platform_id = "";

    // 抖音授权登录的key
    public string aweme_platform_key = "";
    
    //是否开启抖音好友权限
    public bool aweme_friend_permission = false;

    //是否需要开启抖音短视频访问权限
    public bool aweme_video_list_data_permission = false;

    //是否需要开启抖音登录或绑定时授权关注抖音官方号
    public bool aweme_relation_follow = false;

    //抖音登录或绑定时授权是否默认勾选关注抖音官方号
    public bool aweme_relation_follow_default_check = false;

    //是否需要开启抖音实名授权功能
    public bool aweme_dy_real_name_permission = false;

    //是否需要开启抖音同步手机号功能
    public bool aweme_mobile_permission = false;

    //是否需要开启抖音创建视频功能，抖音静默分享依赖此功能
    public bool aweme_video_create = false;

    //是否需要开启查询互关好友功能权限
    public bool aweme_friend_list = false;
    
    //是否需要开启抖音名片权限
    public bool aweme_card_permission = false;
    
    //是否需要开启抖音关注指定用户权限
    public bool aweme_relation_user_follow_permission = false;

    //是否开启抖音粉丝数量权限
    public bool aweme_user_external_data_permission = false;

    public bool huoshan_login_enable = false;

    //抖音火山版platformId
    public string huoshan_platform_id = "";

    //抖音火山版platformKey
    public string huoshan_platform_key = "";
    
    public bool xigua_login_enable = false;
    
    //西瓜视频platformId
    public string xigua_platform_id = "";
    //西瓜视频platformKey
    public string xigua_platform_key = "";

    #region Android独有

    public bool android_taptap_login_enable = false;
    public string android_taptap_platform_id = "";
    public string android_taptap_platform_key = "";
    public string android_taptap_platform_secret = "";

    public bool android_cloudgame_login_enable = false;
    public string android_cloudgame_platform_id = "";
    public string android_cloudgame_package_name = "";
    
    //移动
    public string android_cm_app_id = "";
    public string android_cm_app_key = "";
    //联通
    public string android_cu_app_key = "";
    public string android_cu_app_secret = "";

    public bool android_is_need_visitor = true; //是否需要显示游客，默认显示
    public string android_ct_app_key = "";
    public string android_ct_app_secret = "";
    // 是否开启嗷哩游戏登录(云游戏登录)
    public bool android_aoligame_login_enable = false;
    // 嗷哩游戏的平台id
    public string android_aoligame_platform_id = "";

    #endregion
    

    #region iOS独有

    public bool iOS_apple_login_enable = false;
    public string iOS_apple_platform_id = "";
    //移动
    public string iOS_cm_app_id = "";
    public string iOS_cm_app_key = "";
    //联通
    public string iOS_cu_app_key = "";
    public string iOS_cu_app_secret = "";
    //电信 iOS
    public string iOS_ct_app_key = "";
    public string iOS_ct_app_secret = "";
    
    #endregion
    
    // 登录面板展示顺序优先级本地配置
    public int login_priority = 0;



    public bool login_panel_can_be_closed = true;
    
    #region Android独有

    public bool android_user_info_no_db_default = false;
    public bool android_fusion_user_no_db_default = false;

    #endregion
}


// 分享配置
public class ShareConfigModule
{
    public bool share_dim_amount_available = false;
    public string share_dim_amount = "";
    public string android_share_panel_id = "";
    public string iOS_share_panel_id = "";

    #region Android独有
    // qq分享
    public bool android_qq_available = false;
    public string android_qq_key = "";
    // 微信分享
    public bool android_wx_available = false;
    public string android_wx_key = "";
    // 微博分享
    public bool android_weibo_available = false;
    public string android_weibo_key = "";
    public string android_weibo_url = "";
    // 抖音分享
    public bool android_awe_available = false;
    public string android_awe_key = "";

    #endregion
    
    #region iOS独有
    // qq分享
    public bool iOS_qq_available = false;
    public string iOS_qq_key = "";
    public string iOS_qq_universal_link = "";
    // 微信分享
    public bool iOS_wx_available = false;
    public string iOS_wx_key = "";
    public string iOS_wx_universal_link = "";
    // 微博分享
    public bool iOS_weibo_available = false;
    public string iOS_weibo_key = "";
    public string iOS_weibo_universal_link = "";
    // 抖音分享
    public bool iOS_awe_available = false;
    public string iOS_awe_key = "";
    
    #endregion

}

// push配置
public class PushConfigModule
{
    public bool push_enable = false;

    #region Android独有

    public string android_push_app_name = "";

    public string android_push_huawei_appid = ""; //com.huawei.hms.client.appid
    public string android_push_mi_app_id = ""; //ttgame_push_mi_app_id
    public string android_push_mi_app_key = ""; //ttgame_push_mi_app_key
    public string android_push_meizu_app_id = ""; //ttgame_push_meizu_app_id
    public string android_push_meizu_app_key = ""; //ttgame_push_meizu_app_key
    public string android_push_oppo_app_key = ""; //ttgame_push_oppo_app_key
    public string android_push_oppo_app_secret = ""; //ttgame_push_oppo_app_secret
    public string android_push_umeng_app_key = ""; //ttgame_push_umeng_app_key
    public string android_push_umeng_app_secret = ""; //ttgame_push_umeng_app_secret
    #endregion
}


//FireBaseConfigModule配置
public class FirebaseConfigModule
{
    #region iOS独有
    public string iOS_googleservice_info_plist_url = ""; //GoogleService-Info.plist文件下载地址
    #endregion
}

// 应用商店评价配置
public class RatingConfigModule
{
    #region iOS独有

    public string iOS_rating_app_store_id = "";

    #endregion
}

// DeepLink配置
public class DeepLinkConfigModule
{
    public bool enable = false;
    public int schema_listsize = 0;
    public List<string> schema_list= new List<string>();
    public string baseurl_for_fission = "";

    #region Android独有
    public int android_host_listsize = 0;
    public List<string> android_host_list= new List<string>();
    public int android_delay_millis = 0;
    public List<string> android_default_token_regex = new List<string>();

    #endregion
}


// IM配置
public class IMConfigModule
{
    public bool enable = false;
    public string aid = "";
    public string method = "";
    public string service = "";
    public string ws_host_boe = "";
    public string ws_host = "";
    public List<int> inboxes = null;

    public string fpid = "";
    public string http_host = "";
    public string http_host_boe = "";
    public string app_key = "";
}

// 直播配置
public class LiveConfigModule
{
    public bool enable = false;
    #region Android独有
    public string android_douyin_key;
    #endregion

    #region iOS独有
    public string iOS_live_scheme = "";
    public string iOS_kh_id = "";
    public string iOS_kh_scheme = "";
    #endregion
}

// RTC配置
public class RTCConfigModule
{
    public bool rtc_available = false;
    public string rtc_app_id = "";
}


// 广告付费配置（广告变现、IronSource、穿山甲）
public class ADConfigModule
{
    
    //穿山甲广告配置
    public PangleConfigModule pangleConfigModule = new PangleConfigModule();
}

// 广告变现配置
public class ADPayConfigModule
{
    #region iOS独有

    public bool iOS_enable = false;
    public string iOS_applovin_sdk_key = "";
    public string iOS_google_ad_mob_identifier = "";

    #endregion
}

// IronSource配置
public class IronSourceConfigModule
{
    public bool enable = false;

    #region Android独有

    public string android_app_key = "";

    #endregion

    #region iOS独有

    public string iOS_app_key = "";

    #endregion
}

public class PangleConfigModule
{
    public bool enable = false;
    public string pangle_appid = "";
}


// Thanos配置
public class ThanosConfigModule
{
    #region Android独有
    
    public bool android_save_sub_channel = false;

    #endregion
}

public class PannelSettingsModule
{
    public string panel_language = "";
}

public enum Skin
{
    purple,
    red
}

public enum AndroidScreenOrientation
{
    sensorPortrait,

    sensorLandscape
}
