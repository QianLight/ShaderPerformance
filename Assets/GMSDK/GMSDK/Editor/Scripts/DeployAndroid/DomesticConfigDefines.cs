/// <summary>
/// 国内头条渠道的配置
/// </summary>
public class DomesticConfig
{
    public AppConfig app = new AppConfig();

    public DomesticChannelConfig channel = new DomesticChannelConfig();
}

/// <summary>
/// 国内头条渠道
/// </summary>
public class DomesticChannelConfig : BaseAccountConfig
{
    // 嗷哩游戏授权登录的id
    public string aoligame_platform_id = "";
    // 抖音授权登录的id
    public string aweme_platform_id = "";
    // 抖音授权登录的key
    public string aweme_platform_key = "";
    // 头条授权登录的id
    public string toutiao_platform_id = "";
    // 头条授权登录的key
    public string toutiao_platform_key = "";
    //火山登录的id
    public string huoshan_platform_id = "";
    //火山登录的key
    public string huoshan_platform_key = "";
    //西瓜登录的id
    public string xigua_platform_id = "";
    //西瓜登录的key
    public string xigua_platform_key = "";
    //TapTap登录的id
    public string taptap_platform_id = "";
    //TapTap登录的key
    public string taptap_platform_key = "";
    //TapTap登录的secret
    public string taptap_platform_secret = "";
    //云游戏登录的id
    public string cloudgame_platform_id = "";
    //云游戏应用包名
    public string cloudgame_package_name = "";
    //是否开启头条好友权限
    public bool toutiao_friend_permission;
    //是否开启抖音好友权限
    public bool aweme_friend_permission;
    //是否开启抖音短视频访问权限
    public bool aweme_video_list_data_permission;
    //是否需要开启抖音登录或绑定时授权关注抖音官方号
	public bool aweme_relation_follow;
    //抖音登录或绑定时授权是否默认勾选关注抖音官方号
    public bool aweme_relation_follow_default_check;
    //是否需要开启抖音实名授权功能
    public bool aweme_dy_real_name_permission;
    //是否需要开启抖音同步手机号功能
    public bool aweme_mobile_permission;
    //是否需要开启抖音创建视频功能，抖音静默分享依赖此功能
    public bool aweme_video_create;
    //是否开启抖音互关好友权限
    public bool aweme_friend_list_permission;
    //是否开启抖音名片权限
    public bool aweme_card_permission;
    //是否开启抖音关注指定用户权限
    public bool aweme_relation_user_follow_permission;
    //是否开启抖音粉丝数量权限
    public bool aweme_user_external_data_permission;
    // 一键登录功能，移动appId
    public string cm_app_id = "";
    // 一键登录功能，移动appKey
    public string cm_app_key = "";
    // 一键登录功能，联通appKey
    public string cu_app_id = "";
    // 一键登录功能，联通appSecret
    public string cu_app_key = "";
    // 一键登录功能，电信appKey
    public string ct_app_id = "";
    // 一键登录功能，电信appSecret
    public string ct_app_key = "";
    // 登录面板展示顺序优先级本地配置
    public string login_priority = "";
}
