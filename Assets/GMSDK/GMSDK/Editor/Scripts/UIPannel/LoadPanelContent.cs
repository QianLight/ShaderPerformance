using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using GSDK;
using UnityEngine;

public class LoadPanelContent
{
    private static LoadPanelContent _instance;
    private static Dictionary<string, string> _contents = new Dictionary<string, string>();
    public static string Language_Chinese = "Chinese";
    public static string Language_English = "English";

    public static LoadPanelContent GetInstance()
    {
        if (_instance == null)
        {
            _instance = new LoadPanelContent();
        }

        return _instance;
    }

    private LoadPanelContent()
    {
        ChangeLanguage("");
    }

    public void ChangeLanguage(string languageName)
    {
        if (string.IsNullOrEmpty(languageName))
        {
            languageName = BaseSDKConfigSetting.Instance.panel_language;
            if (string.IsNullOrEmpty(languageName))
            {
                languageName = getSystemLanguage();
            }
        }
        else
        {
            //持久化用户选择的语言
            BaseSDKConfigSetting.Instance.panel_language = languageName;
        }

        _contents.Clear();
        InitDictionary(languageName);
    }

    private void InitDictionary(string languageName)
    {
        string properPath = GMSDKEnv.Instance.PATH_RESOURCES;
        if (!Directory.Exists(properPath))
        {
            Debug.Log("Directory not Exists:" + properPath);
            DefaultStrategy();
            return;
        }

        string fullPath = Path.Combine(properPath,
            Language_Chinese.Equals(languageName) ? "GMPanelContent_CN.xml" : "GMPanelContent_EN.xml");
        if (!File.Exists(fullPath))
        {
            Debug.Log("File not Exists:" + fullPath);
            DefaultStrategy();
            return;
        }

        try
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(fullPath);
            XmlElement rootElement = xmlDocument.DocumentElement;
            if (rootElement != null)
            {
                XmlNodeList xmlNodeList = rootElement.GetElementsByTagName("string");
                foreach (XmlNode node in xmlNodeList)
                {
                    if (node != null && node.Attributes != null)
                    {
                        string nodeKey = node.Attributes["name"].Value;
                        string nodeValue = node.InnerText;
                        if (!string.IsNullOrEmpty(nodeValue))
                        {
                            nodeValue = nodeValue.Replace("\n", "");
                            nodeValue = System.Text.RegularExpressions.Regex.Replace(nodeValue, @"\s+", " ");
                        }

                        AddDictionary(nodeKey, nodeValue);
                    }
                }
            }
        }
        catch (Exception exception)
        {
            Debug.Log("parse xml fail:" + exception.Message);
            DefaultStrategy();
        }
    }

    private void AddDictionary(string nodeKey, String nodeValue)
    {
        if (!_contents.ContainsKey(nodeKey))
        {
            _contents.Add(nodeKey, nodeValue);
        }
        else
        {
            _contents[nodeKey] = nodeValue;
        }
    }

    private string getSystemLanguage()
    {
        try
        {
            string language = Application.systemLanguage.ToString();
            if (string.IsNullOrEmpty(language) ||
                language.IndexOf(Language_Chinese, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Language_Chinese;
            }

            return Language_English;
        }
        catch (Exception e)
        {
            Debug.Log("getSystemLanguage error:" + e.Message);
        }

        return Language_Chinese;
    }

    public string getText(string key)
    {
        if (_contents.ContainsKey(key))
        {
            return _contents[key];
        }

        return "";
    }

    //xml文件不存在情况的默认兜底策略
    private void DefaultStrategy()
    {
        AddDictionary("Common_Param_Declaration",
            "以下参数配置中有android或iOS特殊说明的表示是单端参数,无特殊说明表示是双端共用参数,针对双端共用参数出现值不一致的情况可以使用'|'特殊字符来将双端参数放到一个输入框中,'|'左边是android端参数,右边是iOS端参数。如xx|xx");
        AddDictionary("Common_App_Display_Name_Tips", "App Name");
        AddDictionary("Common_Title", "通用配置");
        AddDictionary("Common_App_English_Name", "应用在应用云平台的英文名");
        AddDictionary("Common_App_Package_Name", "android包名(packageName)");
        AddDictionary("Common_App_BundleID", "iOS包名(bundleID)");
        AddDictionary("Common_Init_Timeout", "初始化超时时间单位毫秒(init_timeout)");
        AddDictionary("Common_Init_DROptimization", "初始化非阻塞优化，默认关");
        AddDictionary("Common_Account_Init_Timeout", "初始化非阻塞优化打开时，帐号初始化设备Id的超时时间(单位:毫秒)");
        AddDictionary("Common_Init_DRCount", "初始化内部重试次数");
        AddDictionary("Common_Game_Security", "游戏安全第二通道");
        AddDictionary("Common_Need_Modify_Info_Plist", "gsdk自动根据config配置info.plist，默认打开");
        AddDictionary("Common_Override_Plist", "gsdk修改info.plist时，碰到相同的内容，打开时覆盖 / 关闭时忽略");
        AddDictionary("Common_Server_Region", "服务部署区域");
        AddDictionary("Common_Server_Region_Tips", "10：部署在新加坡机房，20：部署在美东机房");
        AddDictionary("Common_Skin", "界面主题颜色, 默认紫色(skin)");
        AddDictionary("Common_Need_Antiaddiction", "启用客户端防沉迷(isNeedAntiAddiction)");
        AddDictionary("Common_Need_Service_Mouth", "实名弹窗需要展示游戏客服链接入口(isNeedServiceMouth)");
        AddDictionary("Common_Screen_Orientation", "游戏屏幕方向)");
        AddDictionary("Common_Main_Activity", "游戏主activity(mainActivity)");
        AddDictionary("Common_Main_Activity_Tips", "游戏入口activity的完整包名");
        AddDictionary("Common_Has_Splash", "是否需要闪屏(hasSplash)");
        AddDictionary("Common_Splash_Interval", "闪屏展示时间单位毫秒(splashInterval)");
        AddDictionary("Common_Launch_Trace", "开启性能监控(launchTrace)");
        AddDictionary("Common_Launch_Trace_Tips", "是否开启默认方案的启动耗时分析，默认不开启");
        AddDictionary("Common_Clean", "纯净版");
        AddDictionary("Common_Clean_Tips", "开启纯净版将去掉字节系登录方式");
        AddDictionary("Common_Dsym_Auto_Upload", "开启自动上传dsym符号表文件，会导致打包流程变长");
        AddDictionary("Common_Dsym_Upload_Key", "外网CP，需要在GPM平台下获取Key");
        AddDictionary("Common_Project_Target_Name", "Unity工程名称，如没修改过，可无视");
        AddDictionary("Common_FrameWork_Target_Name", "Unity动态库名称，如有则填写");
        AddDictionary("Common_Associated_Domains_List_Size",
            "Xcode工程中的Associated Domains List Size（用于UniversalLink拉起）");
        AddDictionary("Common_Associated_Domains_List", "Xcode工程中的Associated Domains List（用于Universal Link拉起）");
        AddDictionary("Common_OwnUrl_Scheme_List_Size", "外部唤起通用URL schemes list size（CFBundleURLType）");
        AddDictionary("Common_OwnUrl_Scheme_List", "外部唤起通用URL schemes list（CFBundleURLType）");
        AddDictionary("Common_Application_Queries_Schemes_List_Size",
            "允许探测的APPSchemes list size（LSApplicationQueriesSchemes）");
        AddDictionary("Common_Application_Queries_Schemes_List",
            "允许探测的APPSchemes list（LSApplicationQueriesSchemes）");
        AddDictionary("Common_Mobile_Provision", " 选取mobileProvision文件，在mac下操作");
        AddDictionary("Common_Debug_Mobile_Provision", "[Debug环境]选取mobileProvision文件，在mac下操作");
        AddDictionary("Common_Provisioning_Guid", "provisioning GUID，根据文件自动生成");
        AddDictionary("Common_TeamID", "Team ID，根据文件自动生成");
        AddDictionary("Common_Cert_Name", "证书名称，根据文件自动生成");
        AddDictionary("Common_Debug", "[Debug环境]");
        AddDictionary("Common_Download_From", "下载源");
        AddDictionary("Common_MagicBox", "导入MagicBox，仅用于测试，上线前必须要移除");
        AddDictionary("Common_Skad_Endpoint_Url", "设置SK Adnetwork的归因回传URL（NSAdvertisingAttributionReportEndpoint）");
        AddDictionary("Common_Skad_Endpoint_Url_Tips", "苹果会将SK Adnetwork的归因数据直接发给这个域名，可提高SKAN归因数据的聚合程度，并降低作弊风险。");
        AddDictionary("Common_Param_Pulling","参数拉取中，请稍后");
        AddDictionary("Common_Param_Pull_Success", "参数拉取成功");
        AddDictionary("Common_Param_Pull_Error", "参数拉取失败");
        AddDictionary("Common_Tips", "温馨提示");
        AddDictionary("Common_Conditions_Use", "推荐未完成参数配置的新游戏使用该功能，已经配置过参数的游戏请忽略该功能");
        AddDictionary("Common_Pull", "拉取");
        AddDictionary("Common_Cancel", "取消");
        AddDictionary("Common_Pull_Param_Gdev", "拉取GDev上的配置参数");
        AddDictionary("Common_Manifest_tips", "(由Gdev平台提供，需按照接入文档配置到AndroidManifest.xml中)");
        AddDictionary("Common_Doc_tips", "(由Gdev平台提供，需按照接入文档配置)");
        AddDictionary("Common_Request_Cloud_NO_DB_Default", "云控信息是否存储在luffy中，默认值为false");
        AddDictionary("Common_Settings_Default_Tips", "settings拉取失败情况下的兜底配置");
        AddDictionary("Login_Google_Services_Json", "google-services.json下载地址");
        AddDictionary("Login_Google_Services_Plist", "GoogleService-Info.plist下载地址");
        AddDictionary("Appsflyer_Link", "App Links配置内容");
        AddDictionary("Panel_Ios_Specific", "以下为iOS特有配置");
        AddDictionary("Panel_Android_Specific", "以下为android特有配置");
        AddDictionary("Privacy_Title", "隐私安全配置(请勾选需要的权限并填写相应的描述文字) ");
        AddDictionary("Privacy_Permission_Pop", "是否允许GSDK权限弹窗");
        AddDictionary("Privacy_Need_Protection", "开启个人信息保护指引(isNeedPrivacyProtection)");
        AddDictionary("Privacy_Microphone_Permisson", "获取麦克风隐私权限");
        AddDictionary("Privacy_Photo_Library", "获取系统图片隐私权限");
        AddDictionary("Privacy_Stored_Pictures", "获取存储图片隐私权限");
        AddDictionary("Privacy_Camera", "获取摄像头隐私权限");
        AddDictionary("Privacy_Contacts", "获取通讯录隐私权限");
        AddDictionary("Privacy_Tracking", "获取AppTracking隐私权限");
        AddDictionary("Privacy_Location_Inuse_Usage", "获取仅App使用时获取位置隐私权限");
        AddDictionary("Privacy_Location_Always_Usage", "获取持续使用位置隐私权限");
        AddDictionary("Privacy_Calendars", "获取日历读取隐私权限");
        AddDictionary("Privacy_Reminders", "获取添加提醒隐私权限");
        AddDictionary("Privacy_Speech_Recognition", "获取语音识别隐私权限");
        AddDictionary("Privacy_Siri", "获取Siri模块隐私权限");
        AddDictionary("Privacy_FaceID", "获取FaceID使用隐私权限");
        AddDictionary("Privacy_Idfa", "IDFA弹窗");
        AddDictionary("Privacy_Idfa_Enable", "启用ATT弹窗");
        AddDictionary("Privacy_Idfa_Pop", "自定义ATT弹窗时机");
        AddDictionary("Privacy_Idfa_Pop_Style", "自定义ATT弹窗样式");
        AddDictionary("Privacy_Idfa_Pop_Ui", "自定义GSDK提供的默认引导弹窗UI");
        AddDictionary("Privacy_Idfa_Cancel_Color", "取消按钮文字颜色");
        AddDictionary("Privacy_Idfa_Cancel_Button_Color", "取消按钮颜色");
        AddDictionary("Privacy_Idfa_Cancel_Button_Text", "取消按钮文案");
        AddDictionary("Privacy_Idfa_Confirm_Color", "确认按钮文字颜色");
        AddDictionary("Privacy_Idfa_Confirm_Button_Color", "确认按钮颜色");
        AddDictionary("Privacy_Idfa_Confirm_Text", "确认按钮文案");
        AddDictionary("Privacy_Idfa_Text", "正文");
        AddDictionary("Privacy_Idfa_Title", "标题");
        AddDictionary("Privacy_System_Pop_Enable", "是否需要启用系统弹窗的cover");
        AddDictionary("Privacy_Custom_Cover", "是否需要自定义cover样式");
        AddDictionary("Privacy_Globalization_Enable", "是否启用权限描述国际化");
        AddDictionary("Privacy_Globalization_Enable_Tips",
            "权限描述国际化支持语言，启用后自定义描述文本需 Assets/GMSDK/GMConfigSettings/Localisation下修改");
        AddDictionary("Privacy_ar", "阿拉伯语 ar");
        AddDictionary("Privacy_de", "德语 de");
        AddDictionary("Privacy_en", "英语 en");
        AddDictionary("Privacy_es", "西班牙语 es");
        AddDictionary("Privacy_fr", "法语 fr");
        AddDictionary("Privacy_id", "印尼语 id");
        AddDictionary("Privacy_it", "意大利语 it");
        AddDictionary("Privacy_ja", "日语 ja");
        AddDictionary("Privacy_ko", "韩语 ko");
        AddDictionary("Privacy_pt", "葡萄牙语 pt");
        AddDictionary("Privacy_ru", "俄语 ru");
        AddDictionary("Privacy_th", "泰语 th");
        AddDictionary("Privacy_tr", "土耳其语 tr");
        AddDictionary("Privacy_vi", "越南语 vi");
        AddDictionary("Privacy_ZHHans", "简体中文 zh-Hans");
        AddDictionary("Privacy_ZHHant", "繁体中文 zh-Hant");
        AddDictionary("Privacy_Save_Globalization", "保存权限描述国际化到Assets/GMSDK/GMConfigSettings/Localisation目录下");
        AddDictionary("Privacy_Save_Globalization_Result", "权限描述国际化存储结果：");
        AddDictionary("Login_Title", "登录配置");
        AddDictionary("Login_TouTiao_Enable", "是否接入头条登录功能");
        AddDictionary("Login_TouTiao_PlatformId", "头条PlatformId");
        AddDictionary("Login_TouTiao_PlatformKey", "头条PlatformKey");
        AddDictionary("Login_TouTiao_Friend", "开启头条好友关系权限(toutiaoFriendPermission)");
        AddDictionary("Login_DouYin_Enable", "是否接入抖音登录");
        AddDictionary("Login_DouYin_PlatformId", "抖音PlatformId");
        AddDictionary("Login_DouYin_PlatformKey", "抖音PlatformKey");
        AddDictionary("Login_DouYin_Friend", "开启抖音好友关系权限(awemeFriendPermission)");
        AddDictionary("Login_DouYin_Video", "开启抖音登录或绑定时授权访问抖音短视频内容权限(awemeVideoListDataPermission)");
        AddDictionary("Login_DouYin_Relation_Follow", "开启关注抖音官方号权限(awemeRelationFollow)");
        AddDictionary("Login_DouYin_Relation_Follow_Default", "关注抖音官方号是否需要默认勾选(awemeRelationFollowDefaultCheck)");
        AddDictionary("Login_DouYin_RealName", "开启抖音实名授权(awemeDyRealNamePermission)");
        AddDictionary("Login_DouYin_Mobile", "开启抖音同步手机号功能(awemeMobilePermission)");
        AddDictionary("Login_DouYin_Video_Create", "开启抖音创建视频功能，抖音静默分享依赖此权限(awemeVideoCreate)(android)");
        AddDictionary("Login_DouYin_FriendList", "开启抖音互关好友(awemeFriendListPermission)");
        AddDictionary("Login_DouYin_Card", "开启抖音名片权限(awemeCardPermission)");
        AddDictionary("Login_DouYin_Relation_User_Follow", "开启关注指定用户权限(awemeRelationUserFollowPermission)");
        AddDictionary("Login_DouYin_User_External_Data_Permission", "开启抖音粉丝数量权限(awemeUserExternalDataPermission)");
        AddDictionary("Login_Apple_Title", "苹果授权登录仅支持iOS");
        AddDictionary("Login_Apple_Enable", "是否接入苹果登录");
        AddDictionary("Login_Apple_PlatformId", "苹果PlatformId");
        AddDictionary("Login_HuoShan_Enable", "是否接入抖音火山版登录");
        AddDictionary("Login_HuoShan_PlatformId", "抖音火山版PlatformId");
        AddDictionary("Login_HuoShan_PlatformKey", "抖音火山版PlatformKey");
        AddDictionary("Login_XiGua_Enable", "是否接入西瓜视频登录");
        AddDictionary("Login_XiGua_PlatformId", "西瓜视频PlatformId");
        AddDictionary("Login_XiGua_PlatformKey", "西瓜视频PlatformKey");
        AddDictionary("Login_TapTap_Title", "TapTap登录仅支持android");
        AddDictionary("Login_Taptap_Enable", "是否接入TapTap登录");
        AddDictionary("Login_CloudGame_Title", "云游戏登录仅支持Android");
        AddDictionary("Login_CloudGame_Enable", "是否接入云游戏登录");
        AddDictionary("Login_CloudGame_PlatformId", "云游戏PlatformId");
        AddDictionary("Login_CloudGame_Package_Name", "云游戏应用包名");
        AddDictionary("Login_OneKey_Title", "手机号一键登录配置");
        AddDictionary("Login_OneKey_CT", "电信运营商配置");
        AddDictionary("Login_OneKey_CT_AppKey", "电信登录AppKey");
        AddDictionary("Login_OneKey_CT_AppSecret", "电信登录AppSecret");
        AddDictionary("Login_OneKey_CM", "移动运营商配置");
        AddDictionary("Login_OneKey_CM_AppId", "移动登录AppId");
        AddDictionary("Login_OneKey_CM_AppKey", "移动登录AppKey");
        AddDictionary("Login_OneKey_CU", "联通运营商配置");
        AddDictionary("Login_OneKey_CU_AppKey", "联通登录AppKey");
        AddDictionary("Login_OneKey_CU_AppSecret", "联通登录AppSecret");
        AddDictionary("Login_Need_Visitor", "显示游客本地兜底");
        AddDictionary("Login_Need_Visitor_Tips", "服务器请求失败时是否需要显示游客登录入口，默认显示");
        AddDictionary("Login_Google_Enable", "是否接入google登录功能");
        AddDictionary("Login_Facebook_Enable", "是否接入facebook登录功能");
        AddDictionary("Login_Facebook_Friend_Tips", "是否需要开启facebook好友权限");
        AddDictionary("Login_Line_Enable", "是否接入line登录功能");
        AddDictionary("Login_Kakao_Enable", "是否接入kakao登录功能");
        AddDictionary("Login_VK_Enable", "是否接入vk登录功能");
        AddDictionary("Login_Twitter_Enable", "是否接入twitter登录功能");
        AddDictionary("Login_Tiktok_Enable", "是否接入tiktok登录功能");
        AddDictionary("Login_GooglePlay_Enable", "是否接入GooglePlayGames登录功能");
        AddDictionary("Login_Account_Sort", "帐号显示顺序");
        AddDictionary("Login_Account_Sort_Tips", "帐号显示顺序，使用英文逗号分隔");
        AddDictionary("Login_Panel_Close", "登录面板可被关闭");
        AddDictionary("Login_Aoligame_Enable", "是否接入嗷哩游戏登录(云游戏 android)");
        AddDictionary("Login_Aoligame_PlatformId", "嗷哩游戏PlatformId");
        AddDictionary("Login_User_Info_NO_DB_Default", "自研层用户信息是否存储在luffy中，默认值为false");
        AddDictionary("Login_Fusion_User_NO_DB_Default", "聚合层用户信息是否存储在luffy中，默认值为false");
        AddDictionary("Login_priority", "登录面板展示顺序优先级本地配置(默认值为0，[0、1、2、3、4])");
        AddDictionary("Pay_Title", "支付配置");
        AddDictionary("Pay_Tips", "Google支付的iapKey");
        AddDictionary("Share_Title", "分享配置");
        AddDictionary("Share_Dim_Enable", "分享面板透明度");
        AddDictionary("Share_Dim", "配置透明度");
        AddDictionary("Share_Dim_Tips", "取值：0~1");
        AddDictionary("Share_QQ_Enable", "QQ分享");
        AddDictionary("Share_QQ_Key", "QQ分享Key");
        AddDictionary("Share_QQ_Universal", "QQ分享Universal link");
        AddDictionary("Share_WX_Enable", "微信分享");
        AddDictionary("Share_WX_Key", "微信分享Key");
        AddDictionary("Share_WX_Universal", "微信分享Universal link");
        AddDictionary("Share_WeiBo_Enable", "微博分享");
        AddDictionary("Share_WeiBo_Key", "微博分享Key");
        AddDictionary("Share_WeiBo_Universal", "微博分享Universal link");
        AddDictionary("Share_WeiBo_Url", "微博分享url");
        AddDictionary("Share_DouYin_Enable", "抖音分享");
        AddDictionary("Share_DouYin_Key", "抖音分享Key");
        AddDictionary("Share_Tiktok_Enable", "Tiktok分享");
        AddDictionary("Share_Tiktok_Key", "Tiktok分享Key");
        AddDictionary("Share_Facebook_Enable", "Facebook分享");
        AddDictionary("Share_Whatsapp_Enable", "Whatsapp分享");
        AddDictionary("Share_Instagram_Enable", "Instagram分享");
        AddDictionary("Share_Link_Enable", "Line分享");
        AddDictionary("Share_Kakao_Enable", "Kakao分享");
        AddDictionary("Share_VK_Enable", "VK分享");
        AddDictionary("Share_VKKey", "VK分享Key");
        AddDictionary("Share_Messenger_Enable", "Messenger分享");
        AddDictionary("Share_Twitter_Enable", "Twitter分享");
        AddDictionary("Share_Twitter_Key", "Twitter分享Key");
        AddDictionary("Share_Twitter_Secret", "Twitter分享Secret");
        AddDictionary("Push_Title", "push配置");
        AddDictionary("Push_Enable", "是否接入push推送功能");
        AddDictionary("Push_AppName", "推送在云平台创建的应用的英文名称，必须配置");
        AddDictionary("AppStore_Title", "应用商店评价配置（iOS）");
        AddDictionary("AppStore_ID", "苹果商店应用id");
        AddDictionary("DeepLink_Title", "DeepLink配置");
        AddDictionary("DeepLink_Enable", "是否接入DeepLink功能");
        AddDictionary("DeepLink_ShemeList_Size", "scheme数量");
        AddDictionary("DeepLink_ShemeList", "配置应用的Schemelist，用于识别剪切板内容");
        AddDictionary("DeepLink_Url", "获取邀请码的请求url");
        AddDictionary("DeepLink_Host_ListSize", "AppLink短链域名数量");
        AddDictionary("DeepLink_Host_List", "应用配置的AppLink短链域名");
        AddDictionary("DeepLink_Host_List_Tips", "配置应用在zlink平台上配置的AppLink短链的域名，用于判断AppLink调起是否由zlink引起的");
        AddDictionary("DeepLink_Delay_Millis", "应用前后台切换时，处理剪切板内容的延时时间(单位毫秒)");
        AddDictionary("DeepLink_Delay_Value", "推荐值:500");
        AddDictionary("DeepLink_Regex_ListSize", "设置口令的正则规则数量");
        AddDictionary("DeepLink_Regex_List", "设置口令的正则规则，分享裂变功能用到");
        AddDictionary("IM_Title", "IM配置");
        AddDictionary("IM_Enable", "是否接入IM功能");
        AddDictionary("IM_MethodId", "frontier必需的服务method id");
        AddDictionary("IM_ServiceId", "frontier必需的服务service id");
        AddDictionary("IM_Frontier_BoeUrl", "IM长链boe环境的地址");
        AddDictionary("IM_Frontier_Url", "IM长链的地址");
        AddDictionary("IM_Inbox_Size", "IM支持的收件箱数量");
        AddDictionary("IM_Inbox", "IM支持的收件箱");
        AddDictionary("IM_FrontierId", "frontier必需的产品线id");
        AddDictionary("IM_Http_Url", "IM短链的地址");
        AddDictionary("IM_Http_BoeUrl", "IM短链boe地址");
        AddDictionary("IM_AppKey", "frontier必需的appKey");
        AddDictionary("IM_Enter_Number", "（请输入纯数字）");
        AddDictionary("Live_Title", "直播配置");
        AddDictionary("Live_Enable", "是否接入直播功能");
        AddDictionary("Live_DouYin_Key", "douyin_key（android）");
        AddDictionary("Live_Scheme", "直播scheme(iOS)");
        AddDictionary("Live_UrlType_Id", "开黑id (iOS)");
        AddDictionary("Live_UrlType_Schema", "开黑scheme (iOS)");
        AddDictionary("RTC_Title", "接入RTC");
        AddDictionary("Adattribution_Title", "广告归因（Adjust、AppsFlyer 、Firebase）配置");
        AddDictionary("Adjust_Title", "Adjust配置");
        AddDictionary("Adjust_Enable", "接入Adjust");
        AddDictionary("Appsflyer_Title", "AppsFlyer配置");
        AddDictionary("Appsflyer_Enable", "接入AppsFlyer");
        AddDictionary("Firebase_Title", "Firebase配置（iOS）");
        AddDictionary("Firebase_Google_Service_File", "选取GoogleService - Info.plist文件，在mac下操作");
        AddDictionary("Firebase_Google_Service_File_Save_Result", "选取GoogleService-Info.plist文件存储结果：");
        AddDictionary("Firebase_Google_File_Cancel", "已取消选择文件");
        AddDictionary("Firebase_Google_File_Saved", "已存储");
        AddDictionary("AD_Title", "广告付费相关配置");
        AddDictionary("AD_Pangle_Title", "穿山甲聚合广告配置");
        AddDictionary("AD_Pangle_Enable", "是否接入穿山甲聚合广告功能");
        AddDictionary("AD_IronSource_Title", "IronSource配置");
        AddDictionary("AD_IronSource_Enable", "是否接入IronSource功能");
        AddDictionary("AD_AppLovin_Title", "AppLovin配置(iOS)");
        AddDictionary("AD_AppLovin_Enable", "是否接入AppLovin功能");
        AddDictionary("Thanos_Title", "Thanos配置（android）");
        AddDictionary("Thanos_SaveSubChannel", "更新后保留子渠道信息(SaveSubChannel)");
    }
}