using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DomesticConfigSetting))]
public class DomesticSDKConfigEditor : BaseSDKConfigEditor
{
    private bool androidShareShow = true;
    private bool iOSShareShow = true;
    private bool androidLoginFoldoutShow = true;
    protected override void LoginConfigInfoGUI()
    {
        DomesticConfigSetting domesticConfigSetting = (DomesticConfigSetting) configSetting;
        EditorGUILayout.HelpBox(moduleIndex++ + ") " + LoadPanelContent.GetInstance().getText("Login_Title"),
            MessageType.None);
        //头条
        domesticConfigSetting.login_toutiao_enable =
            EditorGUILayout.ToggleLeft(new GUIContent(LoadPanelContent.GetInstance().getText("Login_TouTiao_Enable")),
                domesticConfigSetting.login_toutiao_enable);
        if (domesticConfigSetting.login_toutiao_enable)
        {
            EditorGUILayout.LabelField(
                new GUIContent(LoadPanelContent.GetInstance().getText("Login_TouTiao_PlatformId")));
            domesticConfigSetting.login_toutiao_platform_id =
                EditorGUILayout.TextField(domesticConfigSetting.login_toutiao_platform_id);
            EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
                .getText("Login_TouTiao_PlatformKey")));
            domesticConfigSetting.login_toutiao_platform_key =
                EditorGUILayout.TextField(domesticConfigSetting.login_toutiao_platform_key);
            domesticConfigSetting.login_toutiao_friend_permission = (EditorGUILayout.ToggleLeft(
                new GUIContent(LoadPanelContent.GetInstance().getText("Login_TouTiao_Friend")),
                domesticConfigSetting.login_toutiao_friend_permission));
        }

        LayoutSpecialModuleSpace();
        //抖音
        domesticConfigSetting.login_aweme_enable =
            EditorGUILayout.ToggleLeft(new GUIContent(LoadPanelContent.GetInstance().getText("Login_DouYin_Enable")),
                domesticConfigSetting.login_aweme_enable);
        if (domesticConfigSetting.login_aweme_enable)
        {
            EditorGUILayout.LabelField(
                new GUIContent(LoadPanelContent.GetInstance().getText("Login_DouYin_PlatformId")));
            domesticConfigSetting.login_aweme_platform_id =
                EditorGUILayout.TextField(domesticConfigSetting.login_aweme_platform_id);
            EditorGUILayout.LabelField(
                new GUIContent(LoadPanelContent.GetInstance().getText("Login_DouYin_PlatformKey")));
            domesticConfigSetting.login_aweme_platform_key =
                EditorGUILayout.TextField(domesticConfigSetting.login_aweme_platform_key);
            domesticConfigSetting.login_aweme_friend_permission = (EditorGUILayout.ToggleLeft(
                new GUIContent(LoadPanelContent.GetInstance().getText("Login_DouYin_Friend")),
                domesticConfigSetting.login_aweme_friend_permission));
            domesticConfigSetting.login_aweme_video_list_data_permission = EditorGUILayout.ToggleLeft(
                new GUIContent(LoadPanelContent.GetInstance().getText("Login_DouYin_Video")),
                domesticConfigSetting.login_aweme_video_list_data_permission);
            domesticConfigSetting.login_aweme_relation_follow = EditorGUILayout.ToggleLeft(
                new GUIContent(LoadPanelContent.GetInstance().getText("Login_DouYin_Relation_Follow")),
                domesticConfigSetting.login_aweme_relation_follow);
            domesticConfigSetting.login_aweme_relation_follow_default_check = EditorGUILayout.ToggleLeft(
                new GUIContent(LoadPanelContent.GetInstance().getText("Login_DouYin_Relation_Follow_Default")),
                domesticConfigSetting.login_aweme_relation_follow_default_check);
            domesticConfigSetting.login_aweme_realname_permission = EditorGUILayout.ToggleLeft(
                new GUIContent(LoadPanelContent.GetInstance().getText("Login_DouYin_RealName")),
                domesticConfigSetting.login_aweme_realname_permission);
            domesticConfigSetting.login_aweme_mobile_permission = EditorGUILayout.ToggleLeft(
                new GUIContent(LoadPanelContent.GetInstance().getText("Login_DouYin_Mobile")),
                domesticConfigSetting.login_aweme_mobile_permission);
            domesticConfigSetting.login_aweme_video_create = EditorGUILayout.ToggleLeft(
                new GUIContent(LoadPanelContent.GetInstance().getText("Login_DouYin_Video_Create")),
                domesticConfigSetting.login_aweme_video_create);
            domesticConfigSetting.login_aweme_friend_list = EditorGUILayout.ToggleLeft(
                new GUIContent(LoadPanelContent.GetInstance().getText("Login_DouYin_FriendList")),
                domesticConfigSetting.login_aweme_friend_list);
            domesticConfigSetting.LoginAwemeCardPermission = EditorGUILayout.ToggleLeft(
                new GUIContent(LoadPanelContent.GetInstance().getText("Login_DouYin_Card")),
                domesticConfigSetting.LoginAwemeCardPermission);
            domesticConfigSetting.LoginAwemeRelationUserFollowPermission = EditorGUILayout.ToggleLeft(
                new GUIContent(LoadPanelContent.GetInstance().getText("Login_DouYin_Relation_User_Follow")),
                domesticConfigSetting.LoginAwemeRelationUserFollowPermission);
            domesticConfigSetting.LoginAwemeUserExternalDataPermission = EditorGUILayout.ToggleLeft(
                new GUIContent(LoadPanelContent.GetInstance().getText("Login_DouYin_User_External_Data_Permission")),
                domesticConfigSetting.LoginAwemeUserExternalDataPermission);
        }


        LayoutSpecialModuleSpace();
        // 嗷哩游戏
        domesticConfigSetting.login_aoligame_enable =
            EditorGUILayout.ToggleLeft(new GUIContent(LoadPanelContent.GetInstance().getText("Login_Aoligame_Enable")),
                domesticConfigSetting.login_aoligame_enable);

        if (domesticConfigSetting.login_aoligame_enable)
        {
            EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
                .getText("Login_Aoligame_PlatformId")));
            domesticConfigSetting.login_aoligame_platform_id =
                EditorGUILayout.TextField(domesticConfigSetting.login_aoligame_platform_id);
        }

        //苹果
        LayoutSpecialModuleSpace();
        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_Apple_Title")));
        domesticConfigSetting.login_iOS_apple_enable =
            EditorGUILayout.ToggleLeft(new GUIContent(LoadPanelContent.GetInstance().getText("Login_Apple_Enable")),
                domesticConfigSetting.login_iOS_apple_enable);
        if (domesticConfigSetting.login_iOS_apple_enable)
        {
            EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
                .getText("Login_Apple_PlatformId")));
            domesticConfigSetting.login_iOS_apple_platform_id =
                EditorGUILayout.TextField(domesticConfigSetting.login_iOS_apple_platform_id);
        }

        //抖音火山版
        LayoutSpecialModuleSpace();
        domesticConfigSetting.login_huoshan_enable =
            EditorGUILayout.ToggleLeft(new GUIContent(LoadPanelContent.GetInstance().getText("Login_HuoShan_Enable")),
                domesticConfigSetting.login_huoshan_enable);
        if (domesticConfigSetting.login_huoshan_enable)
        {
            EditorGUILayout.LabelField(
                new GUIContent(LoadPanelContent.GetInstance().getText("Login_HuoShan_PlatformId")));
            domesticConfigSetting.login_huoshan_platform_id =
                EditorGUILayout.TextField(domesticConfigSetting.login_huoshan_platform_id);
            EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
                .getText("Login_HuoShan_PlatformKey")));
            domesticConfigSetting.login_huoshan_platform_key =
                EditorGUILayout.TextField(domesticConfigSetting.login_huoshan_platform_key);
        }

        //西瓜视频
        LayoutSpecialModuleSpace();
        domesticConfigSetting.login_xigua_enable =
            EditorGUILayout.ToggleLeft(new GUIContent(LoadPanelContent.GetInstance().getText("Login_XiGua_Enable")),
                domesticConfigSetting.login_xigua_enable);
        if (domesticConfigSetting.login_xigua_enable)
        {
            EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
                .getText("Login_XiGua_PlatformId")));
            domesticConfigSetting.login_xigua_platform_id =
                EditorGUILayout.TextField(domesticConfigSetting.login_xigua_platform_id);
            EditorGUILayout.LabelField(
                new GUIContent(LoadPanelContent.GetInstance().getText("Login_XiGua_PlatformKey")));
            domesticConfigSetting.login_xigua_platform_key =
                EditorGUILayout.TextField(domesticConfigSetting.login_xigua_platform_key);
        }

        //TapTap(仅android有)
        LayoutSpecialModuleSpace();
        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_TapTap_Title")));
        domesticConfigSetting.login_android_taptap_enable =
            EditorGUILayout.ToggleLeft(new GUIContent(LoadPanelContent.GetInstance().getText("Login_Taptap_Enable")),
                domesticConfigSetting.login_android_taptap_enable);
        if (domesticConfigSetting.login_android_taptap_enable)
        {
            EditorGUILayout.LabelField(new GUIContent("TapTap PlatformId"));
            domesticConfigSetting.login_android_taptap_platform_id =
                EditorGUILayout.TextField(domesticConfigSetting.login_android_taptap_platform_id);
            EditorGUILayout.LabelField(new GUIContent("TapTap PlatformKey"));
            domesticConfigSetting.login_android_taptap_platform_key =
                EditorGUILayout.TextField(domesticConfigSetting.login_android_taptap_platform_key);
            EditorGUILayout.LabelField(new GUIContent("TapTap PlatformSecret"));
            domesticConfigSetting.login_android_taptap_platform_secret =
                EditorGUILayout.TextField(domesticConfigSetting.login_android_taptap_platform_secret);
        }
        
        //云游戏(仅Android有)
        LayoutSpecialModuleSpace();
        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_CloudGame_Title")));
        domesticConfigSetting.login_android_cloudgame_enable =
            EditorGUILayout.ToggleLeft(new GUIContent(LoadPanelContent.GetInstance().getText("Login_CloudGame_Enable")),
                domesticConfigSetting.login_android_cloudgame_enable);
        if (domesticConfigSetting.login_android_cloudgame_enable)
        {
            EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_CloudGame_PlatformId")));
            domesticConfigSetting.login_android_cloudgame_platform_id =
                EditorGUILayout.TextField(domesticConfigSetting.login_android_cloudgame_platform_id);
            EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_CloudGame_Package_Name")));
            domesticConfigSetting.login_android_cloudgame_package_name =
                EditorGUILayout.TextField(domesticConfigSetting.login_android_cloudgame_package_name);
        }
        

        //一键登录
        LayoutSpecialModuleSpace();
        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_OneKey_Title")));
        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_OneKey_CT") + "（iOS）"));
        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_OneKey_CT_AppKey")));
        domesticConfigSetting.login_iOS_ct_app_key =
            EditorGUILayout.TextField(domesticConfigSetting.login_iOS_ct_app_key);
        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_OneKey_CT_AppSecret")));
        domesticConfigSetting.login_iOS_ct_app_secret =
            EditorGUILayout.TextField(domesticConfigSetting.login_iOS_ct_app_secret);
        LayoutSpecialModuleSpace();

        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_OneKey_CT") +
                                                  "（android）"));
        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_OneKey_CT_AppKey")));
        domesticConfigSetting.login_android_ct_app_key =
            EditorGUILayout.TextField(domesticConfigSetting.login_android_ct_app_key);
        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_OneKey_CT_AppSecret")));
        domesticConfigSetting.login_android_ct_app_secret =
            EditorGUILayout.TextField(domesticConfigSetting.login_android_ct_app_secret);
        LayoutSpecialModuleSpace();

        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_OneKey_CM") + "（iOS）"));
        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_OneKey_CM_AppId")));
        domesticConfigSetting.login_iOS_cm_app_id =
            EditorGUILayout.TextField(domesticConfigSetting.login_iOS_cm_app_id);
        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_OneKey_CM_AppKey")));
        domesticConfigSetting.login_iOS_cm_app_key =
            EditorGUILayout.TextField(domesticConfigSetting.login_iOS_cm_app_key);
        LayoutSpecialModuleSpace();

        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_OneKey_CM") +
                                                  "（android）"));
        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_OneKey_CM_AppId")));
        domesticConfigSetting.login_android_cm_app_id =
            EditorGUILayout.TextField(domesticConfigSetting.login_android_cm_app_id);
        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_OneKey_CM_AppKey")));
        domesticConfigSetting.login_android_cm_app_key =
            EditorGUILayout.TextField(domesticConfigSetting.login_android_cm_app_key);
        LayoutSpecialModuleSpace();

        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_OneKey_CU") + "（iOS）"));
        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_OneKey_CU_AppKey")));
        domesticConfigSetting.login_iOS_cu_app_key =
            EditorGUILayout.TextField(domesticConfigSetting.login_iOS_cu_app_key);
        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_OneKey_CU_AppSecret")));
        domesticConfigSetting.login_iOS_cu_app_secret =
            EditorGUILayout.TextField(domesticConfigSetting.login_iOS_cu_app_secret);
        LayoutSpecialModuleSpace();

        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_OneKey_CU") +
                                                  "（android）"));
        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_OneKey_CU_AppKey")));
        domesticConfigSetting.login_android_cu_app_key =
            EditorGUILayout.TextField(domesticConfigSetting.login_android_cu_app_key);
        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_OneKey_CU_AppSecret")));
        domesticConfigSetting.login_android_cu_app_secret =
            EditorGUILayout.TextField(domesticConfigSetting.login_android_cu_app_secret);
        LayoutSpecialModuleSpace();

        //登录面板是否可被关闭
        domesticConfigSetting.login_panel_can_be_closed = EditorGUILayout.ToggleLeft(
            new GUIContent(LoadPanelContent.GetInstance().getText("Login_Panel_Close")),
            domesticConfigSetting.login_panel_can_be_closed);
        LayoutSpecialModuleSpace();
        
        // 登录面板展示顺序优先级本地配置
        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Login_priority")));
        domesticConfigSetting.login_priority = EditorGUILayout.IntField(domesticConfigSetting.login_priority);
        LayoutSpecialModuleSpace();
        
        androidLoginFoldoutShow = EditorGUILayout.Foldout(androidLoginFoldoutShow, LoadPanelContent.GetInstance().getText("Panel_Android_Specific"), getUniqueFoldoutConfigStyle());
        if (androidLoginFoldoutShow)
        {
            //是否显示游客本地兜底
            GUIContent isNeedVisitorDesc =
                new GUIContent(LoadPanelContent.GetInstance().getText("Login_Need_Visitor") + "[?]",
                    LoadPanelContent.GetInstance().getText("Login_Need_Visitor_Tips"));
            domesticConfigSetting.login_android_is_need_visitor = EditorGUILayout.ToggleLeft(isNeedVisitorDesc,
                domesticConfigSetting.login_android_is_need_visitor);
        
            GUIContent userInfoNoDbDefault =
                new GUIContent(LoadPanelContent.GetInstance().getText("Login_User_Info_NO_DB_Default") + "[?]",
                    LoadPanelContent.GetInstance().getText("Common_Settings_Default_Tips"));
            domesticConfigSetting.android_user_info_no_db_default = EditorGUILayout.ToggleLeft(userInfoNoDbDefault,
                domesticConfigSetting.android_user_info_no_db_default);
            
            GUIContent fusionInfoNoDbDefault =
                new GUIContent(LoadPanelContent.GetInstance().getText("Login_Fusion_User_NO_DB_Default") + "[?]",
                    LoadPanelContent.GetInstance().getText("Common_Settings_Default_Tips"));
            domesticConfigSetting.android_fusion_info_no_db_default = EditorGUILayout.ToggleLeft(fusionInfoNoDbDefault,
                domesticConfigSetting.android_fusion_info_no_db_default);
        }
        LayoutSpecialModuleSpace();
    }

    // 分享---国内
    protected override void ShareRegionInfoGUI()
    {
        base.ShareConfigInfoGUI();

        LayoutSpecialModuleSpace();
        iOSShareShow = EditorGUILayout.Foldout(iOSShareShow, LoadPanelContent.GetInstance().getText("Panel_Ios_Specific"), getUniqueFoldoutConfigStyle());
        if (iOSShareShow)
        {
            #region iOS独有
            configSetting.iOS_share_qq_available = EditorGUILayout.ToggleLeft(
                new GUIContent(LoadPanelContent.GetInstance().getText("Share_QQ_Enable")),
                configSetting.iOS_share_qq_available);
            if (configSetting.iOS_share_qq_available)
            {
                EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Share_QQ_Key")));
                configSetting.iOS_share_qq_key = EditorGUILayout.TextField(configSetting.iOS_share_qq_key);
                EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
                    .getText("Share_QQ_Universal")));
                configSetting.iOS_share_qq_universal_link =
                    EditorGUILayout.TextField(configSetting.iOS_share_qq_universal_link);
            }

            configSetting.iOS_share_wx_available = EditorGUILayout.ToggleLeft(
                new GUIContent(LoadPanelContent.GetInstance().getText("Share_WX_Enable")),
                configSetting.iOS_share_wx_available);
            if (configSetting.iOS_share_wx_available)
            {
                EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Share_WX_Key")));
                configSetting.iOS_share_wx_key = EditorGUILayout.TextField(configSetting.iOS_share_wx_key);
                EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
                    .getText("Share_WX_Universal")));
                configSetting.iOS_share_wx_universal_link =
                    EditorGUILayout.TextField(configSetting.iOS_share_wx_universal_link);
            }

            configSetting.iOS_share_weibo_available = EditorGUILayout.ToggleLeft(
                new GUIContent(LoadPanelContent.GetInstance().getText("Share_WeiBo_Enable")),
                configSetting.iOS_share_weibo_available);
            if (configSetting.iOS_share_weibo_available)
            {
                EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Share_WeiBo_Key")));
                configSetting.iOS_share_weibo_key = EditorGUILayout.TextField(configSetting.iOS_share_weibo_key);
                EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
                    .getText("Share_WeiBo_Universal")));
                configSetting.iOS_share_weibo_universal_link =
                    EditorGUILayout.TextField(configSetting.iOS_share_weibo_universal_link);
            }

            configSetting.iOS_share_awe_available = EditorGUILayout.ToggleLeft(
                new GUIContent(LoadPanelContent.GetInstance().getText("Share_DouYin_Enable")),
                configSetting.iOS_share_awe_available);
            if (configSetting.iOS_share_awe_available)
            {
                EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Share_DouYin_Key")));
                configSetting.iOS_share_awe_key = EditorGUILayout.TextField(configSetting.iOS_share_awe_key);
            }

            #endregion
        }

        LayoutSpecialModuleSpace();
        androidShareShow = EditorGUILayout.Foldout(androidShareShow, LoadPanelContent.GetInstance().getText("Panel_Android_Specific"), getUniqueFoldoutConfigStyle());
        if (androidShareShow)
        {
            #region Android独有
            configSetting.android_share_qq_available = EditorGUILayout.ToggleLeft(
                new GUIContent(LoadPanelContent.GetInstance().getText("Share_QQ_Enable")),
                configSetting.android_share_qq_available);
            if (configSetting.android_share_qq_available)
            {
                EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Share_QQ_Key")));
                configSetting.android_share_qq_key = EditorGUILayout.TextField(configSetting.android_share_qq_key);
            }

            configSetting.android_share_wx_available = EditorGUILayout.ToggleLeft(
                new GUIContent(LoadPanelContent.GetInstance().getText("Share_WX_Enable")),
                configSetting.android_share_wx_available);
            if (configSetting.android_share_wx_available)
            {
                EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Share_WX_Key")));
                configSetting.android_share_wx_key = EditorGUILayout.TextField(configSetting.android_share_wx_key);
            }

            configSetting.android_share_weibo_available = EditorGUILayout.ToggleLeft(
                new GUIContent(LoadPanelContent.GetInstance().getText("Share_WeiBo_Enable")),
                configSetting.android_share_weibo_available);
            if (configSetting.android_share_weibo_available)
            {
                EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Share_WeiBo_Key")));
                configSetting.android_share_weibo_key =
                    EditorGUILayout.TextField(configSetting.android_share_weibo_key);
                EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Share_WeiBo_Url")));
                configSetting.android_share_weibo_url =
                    EditorGUILayout.TextField(configSetting.android_share_weibo_url);
            }

            configSetting.android_share_awe_available = EditorGUILayout.ToggleLeft(
                new GUIContent(LoadPanelContent.GetInstance().getText("Share_DouYin_Enable")),
                configSetting.android_share_awe_available);
            if (configSetting.android_share_awe_available)
            {
                EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Share_DouYin_Key")));
                configSetting.android_share_awe_key = EditorGUILayout.TextField(configSetting.android_share_awe_key);
            }

            #endregion
        }
        
        LayoutModuleSpace();
    }
}
