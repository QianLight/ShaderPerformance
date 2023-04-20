using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using GMSDK;
using GSDK;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

public abstract class BaseSDKConfigEditor : Editor
{
	protected BaseSDKConfigSetting configSetting;
	protected static int moduleIndex;
	private static string privacySecuritySaveResult = "";
	private static string googleServiceInfoPlistSaveResult = "";
	private bool androidCommonFoldoutShow = true;
	private bool iOSCommonFoldoutShow = true;
	private bool androidPrivacySecuritFoldoutShow = true;
	private bool iOSPrivacySecuritFoldoutShow = true;
	private bool androidPushFoldoutShow = true;
	private bool androidDeepLinkFoldoutShow = true;
	private bool iOSAppsFlyerFoldoutShow = true;
	private bool androidAppsFlyerFoldoutShow = true;
	private static bool IsPullingParam;
	private static PullResult pullResult;

	public override void OnInspectorGUI()
    {
	    configSetting = (BaseSDKConfigSetting) target;
        ConfigInfoGUI();
    }

    protected void ConfigInfoGUI()
    {
	    moduleIndex = 1;
	    
	    //拉取GDev上的参数
	    GetParametersOnGDev();
	    
        //1. 通用配置
        CommonConfigInfoGUI();

        //2. 隐私安全配置
        PrivacySecurityConfigInfoGUI();
        
        //3. 登录
        LoginConfigInfoGUI();
        
        //4. 支付
        PayConfigInfoGUI();
        
        //5. share 
        ShareRegionInfoGUI();
        
        //6. push
        PushConfigInfoGUI();
        
        //7. 广告归因(Adjust AppsFlyer Firebase)
        AdAttributionConfigInfoGUI();

        //8. 应用商店评价
        RatingConfigInfoGUI();

        //9. DeepLink
        DeepLinkConfigInfoGUI();

        //12 IM
        IMConfigInfoGUI();

        //13 直播
        LiveConfigInfoGUI();

        //14 RTC
        RTCConfigInfoGUI();

        //15 广告
        ADConfigInfoGUI();

        //16 Thanos
        ThanosConfigInfoGUI();
    }

    // 通用配置
    private void CommonConfigInfoGUI()
    {
	    EditorGUI.BeginDisabledGroup(true);
	    GUILayout.TextArea(LoadPanelContent.GetInstance().getText("Common_Param_Declaration"));
	    EditorGUI.EndDisabledGroup();
	    EditorGUILayout.HelpBox(moduleIndex++ + ") "+LoadPanelContent.GetInstance().getText("Common_Title"), MessageType.None);
        GUIStyle guiStyle = GUI.skin.GetStyle("HelpBox");
        guiStyle.normal.textColor = Color.black;
        guiStyle.fontSize = 15;
        
        EditorGUILayout.LabelField(new GUIContent("AppID"));
        configSetting.app_id = EditorGUILayout.TextField(configSetting.app_id);

        EditorGUILayout.LabelField(new GUIContent("AppName[?]", LoadPanelContent.GetInstance().getText("Common_App_English_Name")));
        configSetting.app_name = EditorGUILayout.TextField(configSetting.app_name);

        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Common_App_Package_Name")));
        configSetting.android_package_name = EditorGUILayout.TextField(configSetting.android_package_name);
        
        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Common_App_BundleID")));
        configSetting.iOS_bundleId = EditorGUILayout.TextField(configSetting.iOS_bundleId);

        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Common_Init_Timeout")));
        configSetting.init_timeout = EditorGUILayout.TextField(configSetting.init_timeout);

		GUIContent drOptimizationComponentDesc = new GUIContent(LoadPanelContent.GetInstance().getText("Common_Init_DROptimization"));
		configSetting.init_optimization_enable =
			EditorGUILayout.ToggleLeft(drOptimizationComponentDesc, configSetting.init_optimization_enable);

		if (configSetting.init_optimization_enable)
		{
			EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Common_Account_Init_Timeout")));
			configSetting.account_init_timeout = EditorGUILayout.TextField(configSetting.account_init_timeout);
		}

		EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Common_Init_DRCount")));
		configSetting.init_dr_retryCount = EditorGUILayout.IntField(configSetting.init_dr_retryCount);

		GUIContent securityComponentDesc = new GUIContent(LoadPanelContent.GetInstance().getText("Common_Game_Security"));
        configSetting.SecurityUsePacketChannelEnable =
            EditorGUILayout.ToggleLeft(securityComponentDesc, configSetting.SecurityUsePacketChannelEnable);

		GUIContent modifyInfoContent = new GUIContent(LoadPanelContent.GetInstance().getText("Common_Need_Modify_Info_Plist"));
		configSetting.needGSDKModifyInfoPlist =
			EditorGUILayout.ToggleLeft(modifyInfoContent, configSetting.needGSDKModifyInfoPlist);

		GUIContent overrideInfo = new GUIContent(LoadPanelContent.GetInstance().getText("Common_Override_Plist"));
		configSetting.gsdkModifyInfoPlistReplaceWhenMetSame =
			EditorGUILayout.ToggleLeft(overrideInfo, configSetting.gsdkModifyInfoPlistReplaceWhenMetSame);


        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Common_Skin")));
        configSetting.skin = (Skin) EditorGUILayout.EnumPopup(configSetting.skin);
        GUIContent isNeedAntiAddictionDesc = new GUIContent(LoadPanelContent.GetInstance().getText("Common_Need_Antiaddiction"));
        configSetting.is_need_anti_addiction = EditorGUILayout.ToggleLeft(isNeedAntiAddictionDesc, configSetting.is_need_anti_addiction);
        GUIContent isNeedServiceMouthDesc = new GUIContent(LoadPanelContent.GetInstance().getText("Common_Need_Service_Mouth"));
        configSetting.is_need_service_mouth = EditorGUILayout.ToggleLeft(isNeedServiceMouthDesc, configSetting.is_need_service_mouth);

	    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Common_Download_From")));
	    configSetting.download_source = EditorGUILayout.TextField(configSetting.download_source);

	    
        LayoutSpecialModuleSpace();

        androidCommonFoldoutShow = EditorGUILayout.Foldout(androidCommonFoldoutShow, LoadPanelContent.GetInstance().getText("Panel_Android_Specific"), getUniqueFoldoutConfigStyle());
        if (androidCommonFoldoutShow)
        {
	        #region Android独有
	        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Common_Screen_Orientation"))); 
	        AndroidScreenOrientation orientation = (AndroidScreenOrientation) EditorGUILayout.EnumPopup(configSetting.screenOrientation);
	        configSetting.screenOrientation = orientation;

	        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Common_Main_Activity") + "[?]", LoadPanelContent.GetInstance().getText("Common_Main_Activity_Tips")));
	        configSetting.android_main_activity = EditorGUILayout.TextField(configSetting.android_main_activity);
			GUIContent hasSplashDesc = new GUIContent(LoadPanelContent.GetInstance().getText("Common_Has_Splash"));
			configSetting.android_has_splash = EditorGUILayout.ToggleLeft(hasSplashDesc, configSetting.android_has_splash);

			EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Common_Splash_Interval")));
			configSetting.android_splash_interval = EditorGUILayout.IntField(configSetting.android_splash_interval);
	        GUIContent openLaunchTrace = new GUIContent(LoadPanelContent.GetInstance().getText("Common_Launch_Trace") + "[?]", LoadPanelContent.GetInstance().getText("Common_Launch_Trace_Tips"));
	        configSetting.android_open_launch_trace =
		        EditorGUILayout.ToggleLeft(openLaunchTrace, configSetting.android_open_launch_trace);

			GUIContent cleanAppDesc = new GUIContent(LoadPanelContent.GetInstance().getText("Common_Clean")+ "[?]", LoadPanelContent.GetInstance().getText("Common_Clean_Tips"));
			configSetting.android_clean_app = EditorGUILayout.ToggleLeft(cleanAppDesc, configSetting.android_clean_app);
	        GUIContent requestCloudNoDbDefault =
		        new GUIContent(LoadPanelContent.GetInstance().getText("Common_Request_Cloud_NO_DB_Default") + "[?]",
			        LoadPanelContent.GetInstance().getText("Common_Settings_Default_Tips"));
	        configSetting.android_request_cloud_no_db_default = EditorGUILayout.ToggleLeft(requestCloudNoDbDefault,
		        configSetting.android_request_cloud_no_db_default);

	        #endregion
        }
        
	    
        LayoutSpecialModuleSpace();
        iOSCommonFoldoutShow = EditorGUILayout.Foldout(iOSCommonFoldoutShow, LoadPanelContent.GetInstance().getText("Panel_Ios_Specific"), getUniqueFoldoutConfigStyle());
        if (iOSCommonFoldoutShow)
        {
	        #region iOS独有
	        
	        EditorGUILayout.LabelField(new GUIContent("AppDisplayName[?]", "App Name"));
	        configSetting.iOS_app_display_name = EditorGUILayout.TextField(configSetting.iOS_app_display_name);

	        GUIContent importMagicBoxDesc = new GUIContent(LoadPanelContent.GetInstance().getText("Common_MagicBox"));
	        configSetting.iOS_import_magicbox =
		        EditorGUILayout.ToggleLeft(importMagicBoxDesc, configSetting.iOS_import_magicbox);

	        GUIContent dsymAutoUploadDesc =
		        new GUIContent(LoadPanelContent.GetInstance().getText("Common_Dsym_Auto_Upload"));
	        configSetting.iOS_dsym_auto_upload =
		        EditorGUILayout.ToggleLeft(dsymAutoUploadDesc, configSetting.iOS_dsym_auto_upload);
	        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
		        .getText("Common_Dsym_Upload_Key")));
	        configSetting.iOS_dsym_upload_key = EditorGUILayout.TextField(configSetting.iOS_dsym_upload_key);

	        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
		        .getText("Common_Project_Target_Name")));
	        configSetting.iOS_project_target_name = EditorGUILayout.TextField(configSetting.iOS_project_target_name);

	        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
		        .getText("Common_FrameWork_Target_Name")));
	        configSetting.iOS_framework_target_name =
		        EditorGUILayout.TextField(configSetting.iOS_framework_target_name);

	        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
		        .getText("Common_Associated_Domains_List_Size")));
	        configSetting.iOS_associated_domains_list_size =
		        EditorGUILayout.IntField(configSetting.iOS_associated_domains_list_size);
	        if (configSetting.iOS_associated_domains_list_size > 0)
	        {
		        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
			        .getText("Common_Associated_Domains_List")));
		        var list = new List<string>();
		        for (int i = 0; i < configSetting.iOS_associated_domains_list_size; i++)
		        {
			        string associatedDomain;
			        try
			        {
				        associatedDomain = EditorGUILayout.TextField(configSetting.iOS_associated_domains_list[i]);
			        }
			        catch
			        {
				        associatedDomain = EditorGUILayout.TextField("");
			        }

			        list.Add(associatedDomain);
		        }

		        configSetting.iOS_associated_domains_list = list;
	        }
	        else
	        {
		        configSetting.iOS_associated_domains_list = null;
	        }

	        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
		        .getText("Common_OwnUrl_Scheme_List_Size")));
	        configSetting.iOS_own_url_scheme_list_size =
		        EditorGUILayout.IntField(configSetting.iOS_own_url_scheme_list_size);
	        if (configSetting.iOS_own_url_scheme_list_size > 0)
	        {
		        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
			        .getText("Common_OwnUrl_Scheme_List")));
		        var list = new List<string>();
		        for (int i = 0; i < configSetting.iOS_own_url_scheme_list_size; i++)
		        {
			        string ownUrlScheme;
			        try
			        {
				        ownUrlScheme = EditorGUILayout.TextField(configSetting.iOS_own_url_scheme_list[i]);
			        }
			        catch
			        {
				        ownUrlScheme = EditorGUILayout.TextField("");
			        }

			        list.Add(ownUrlScheme);
		        }

		        configSetting.iOS_own_url_scheme_list = list;
	        }
	        else
	        {
		        configSetting.iOS_own_url_scheme_list = null;
	        }

	        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
		        .getText("Common_Application_Queries_Schemes_List_Size")));
	        configSetting.iOS_application_queries_schemes_list_size =
		        EditorGUILayout.IntField(configSetting.iOS_application_queries_schemes_list_size);
	        if (configSetting.iOS_application_queries_schemes_list_size > 0)
	        {
		        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
			        .getText("Common_Application_Queries_Schemes_List")));
		        var list = new List<string>();
		        for (int i = 0; i < configSetting.iOS_application_queries_schemes_list_size; i++)
		        {
			        string ownUrlScheme;
			        try
			        {
				        ownUrlScheme = EditorGUILayout.TextField(configSetting.iOS_application_queries_schemes_list[i]);
			        }
			        catch
			        {
				        ownUrlScheme = EditorGUILayout.TextField("");
			        }

			        list.Add(ownUrlScheme);
		        }

		        configSetting.iOS_application_queries_schemes_list = list;
	        }
	        else
	        {
		        configSetting.iOS_application_queries_schemes_list = null;
	        }

	        LayoutSpecialModuleSpace();

	        if (GUILayout.Button(LoadPanelContent.GetInstance().getText("Common_Mobile_Provision")))
	        {
		        string path = EditorUtility.OpenFilePanel("选取mobileProvision文件", "", "mobileprovision");
		        if (path != "")
		        {
			        //计算GUID与证书名称
			        Process proc = new Process
			        {
				        StartInfo =
				        {
					        //					cms -D -i " + path + " -o /Users/Cliffe/Code/Unity/BDGameSDKUnityPlugin/CI/p.plist
					        FileName = "sh",
					        Arguments = Path.Combine(GMSDKEnv.Instance.SubEditorPath("GMSDK"),
						        "Scripts/DeployiOS/findProvisioningGUID.sh") + " " + path,
					        CreateNoWindow = false,
					        WindowStyle = ProcessWindowStyle.Normal,
					        UseShellExecute = false,
					        ErrorDialog = false,
					        RedirectStandardInput = false,
					        RedirectStandardOutput = true,
					        RedirectStandardError = false,
					        WorkingDirectory = null
				        }
			        };

			        proc.Start();
			        proc.WaitForExit();
			        var sr = proc.StandardOutput;
			        string str = sr.ReadToEnd();
			        proc.Close();

			        string[] res = str.Split('\n');
			        string provisioningGUID = "";
			        string certName = "";
			        string teamID = "";
			        if (res.Length > 0)
			        {
				        provisioningGUID = res[0];
				        UnityEngine.Debug.Log("provisioning GUID:" + provisioningGUID);
			        }
			        else
			        {
				        //解析出错
				        configSetting.iOS_provisioning_guid = "解析出错";
			        }

			        if (res.Length > 1)
			        {
				        teamID = res[1];
				        UnityEngine.Debug.Log("team ID:" + teamID);
			        }
			        else
			        {
				        //解析出错
				        configSetting.iOS_team_ID = "解析出错";
			        }

			        if (res.Length > 2)
			        {
				        certName = res[2];
				        UnityEngine.Debug.Log("cert name:" + certName);
			        }
			        else
			        {
				        //解析出错
				        configSetting.iOS_cert_name = "解析出错";
			        }

			        if (provisioningGUID.Length > 0 && certName.Length > 0 && teamID.Length > 0)
			        {
				        configSetting.iOS_cert_success = true;
				        configSetting.iOS_provisioning_guid = provisioningGUID;
				        configSetting.iOS_team_ID = teamID;
				        configSetting.iOS_cert_name = certName;
			        }
			        else
			        {
				        configSetting.iOS_cert_success = false;
			        }
		        }
	        }

	        EditorGUILayout.LabelField(
		        new GUIContent(LoadPanelContent.GetInstance().getText("Common_Provisioning_Guid")));
	        EditorGUILayout.TextField(configSetting.iOS_provisioning_guid);
	        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Common_TeamID")));
	        EditorGUILayout.TextField(configSetting.iOS_team_ID);
	        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Common_Cert_Name")));
	        EditorGUILayout.TextField(configSetting.iOS_cert_name);

	        LayoutSpecialModuleSpace();

	        if (GUILayout.Button(LoadPanelContent.GetInstance().getText("Common_Debug_Mobile_Provision")))
	        {
		        string path = EditorUtility.OpenFilePanel("[Debug环境]选取mobileProvision文件", "", "mobileprovision");
		        if (path != "")
		        {
			        //计算GUID与证书名称
			        Process proc = new Process
			        {
				        StartInfo =
				        {
					        //					cms -D -i " + path + " -o /Users/Cliffe/Code/Unity/BDGameSDKUnityPlugin/CI/p.plist
					        FileName = "sh",
					        Arguments = Path.Combine(GMSDKEnv.Instance.SubEditorPath("GMSDK"),
						        "Scripts/DeployIOS/findProvisioningGUID.sh") + " " + path,
					        CreateNoWindow = false,
					        WindowStyle = ProcessWindowStyle.Normal,
					        UseShellExecute = false,
					        ErrorDialog = false,
					        RedirectStandardInput = false,
					        RedirectStandardOutput = true,
					        RedirectStandardError = false,
					        WorkingDirectory = null
				        }
			        };

			        proc.Start();
			        proc.WaitForExit();
			        var sr = proc.StandardOutput;
			        string str = sr.ReadToEnd();
			        proc.Close();

			        string[] res = str.Split('\n');
			        string provisioningGUID = "";
			        string certName = "";
			        string teamID = "";
			        if (res.Length > 0)
			        {
				        provisioningGUID = res[0];
				        UnityEngine.Debug.Log("provisioning GUID:" + provisioningGUID);
			        }
			        else
			        {
				        //解析出错
				        configSetting.iOS_debug_provisioning_guid = "解析出错";
			        }

			        if (res.Length > 1)
			        {
				        teamID = res[1];
				        UnityEngine.Debug.Log("team ID:" + teamID);
			        }
			        else
			        {
				        //解析出错
				        configSetting.iOS_debug_team_ID = "解析出错";
			        }

			        if (res.Length > 2)
			        {
				        certName = res[2];
				        UnityEngine.Debug.Log("cert name:" + certName);
			        }
			        else
			        {
				        //解析出错
				        configSetting.iOS_debug_cert_name = "解析出错";
			        }

			        if (provisioningGUID.Length > 0 && certName.Length > 0 && teamID.Length > 0)
			        {
				        configSetting.iOS_debug_cert_success = true;
				        configSetting.iOS_debug_provisioning_guid = provisioningGUID;
				        configSetting.iOS_debug_team_ID = teamID;
				        configSetting.iOS_debug_cert_name = certName;
			        }
			        else
			        {
				        configSetting.iOS_debug_cert_success = false;
			        }
		        }
	        }

	        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Common_Debug") +
	                                                  LoadPanelContent.GetInstance()
		                                                  .getText("Common_Provisioning_Guid")));
	        EditorGUILayout.TextField(configSetting.iOS_debug_provisioning_guid);
	        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Common_Debug") +
	                                                  LoadPanelContent.GetInstance().getText("Common_TeamID")));
	        EditorGUILayout.TextField(configSetting.iOS_debug_team_ID);
	        EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Common_Debug") +
	                                                  LoadPanelContent.GetInstance().getText("Common_Cert_Name")));
	        EditorGUILayout.TextField(configSetting.iOS_debug_cert_name);

	        #endregion 
        }
        
        LayoutModuleSpace();
    }

    // 隐私安全配置
    private void PrivacySecurityConfigInfoGUI()
    {
	    EditorGUILayout.HelpBox(moduleIndex++ + ") "+LoadPanelContent.GetInstance().getText("Privacy_Title"), MessageType.None);

	    configSetting.privacySecurity_is_show_permission_toast = EditorGUILayout.ToggleLeft(LoadPanelContent.GetInstance().getText("Privacy_Permission_Pop"),
		    configSetting.privacySecurity_is_show_permission_toast);

	    LayoutSpecialModuleSpace();
	    androidPrivacySecuritFoldoutShow = EditorGUILayout.Foldout(androidPrivacySecuritFoldoutShow, LoadPanelContent.GetInstance().getText("Panel_Android_Specific"), getUniqueFoldoutConfigStyle());
	    if (androidPrivacySecuritFoldoutShow)
	    {
		    #region Android独有
		    configSetting.android_privacySecurity_is_need_privacy_protection =
			    EditorGUILayout.ToggleLeft(new GUIContent(LoadPanelContent.GetInstance().getText("Privacy_Need_Protection")), configSetting.android_privacySecurity_is_need_privacy_protection);
	    
		    #endregion 
	    }
	    
	    LayoutSpecialModuleSpace();

	    
	    iOSPrivacySecuritFoldoutShow = EditorGUILayout.Foldout(iOSPrivacySecuritFoldoutShow, LoadPanelContent.GetInstance().getText("Panel_Ios_Specific"), getUniqueFoldoutConfigStyle());
	    if (iOSPrivacySecuritFoldoutShow)
	    {
		    #region iOS独有

		    configSetting.iOS_privacy_microphone_usage_available = EditorGUILayout.ToggleLeft(
			    new GUIContent(LoadPanelContent.GetInstance().getText("Privacy_Microphone_Permisson")),
			    configSetting.iOS_privacy_microphone_usage_available);
		    EditorGUILayout.LabelField(new GUIContent("NSMicrophoneUsageDescription"));
		    if (configSetting.iOS_privacy_microphone_usage_available)
		    {
			    configSetting.iOS_privacy_microphone_usage_desc =
				    EditorGUILayout.TextField(configSetting.iOS_privacy_microphone_usage_desc);
		    }

		    configSetting.iOS_privacy_photo_usage_available = EditorGUILayout.ToggleLeft(
			    new GUIContent(LoadPanelContent.GetInstance().getText("Privacy_Photo_Library")),
			    configSetting.iOS_privacy_photo_usage_available);
		    EditorGUILayout.LabelField(new GUIContent("NSPhotoLibraryUsageDescription"));
		    if (configSetting.iOS_privacy_photo_usage_available)
		    {
			    configSetting.iOS_privacy_photo_usage_desc =
				    EditorGUILayout.TextField(configSetting.iOS_privacy_photo_usage_desc);
		    }

		    configSetting.iOS_privacy_photo_additions_usage_available = EditorGUILayout.ToggleLeft(
			    new GUIContent(LoadPanelContent.GetInstance().getText("Privacy_Stored_Pictures")),
			    configSetting.iOS_privacy_photo_additions_usage_available);
		    EditorGUILayout.LabelField(new GUIContent("NSPhotoLibraryAddUsageDescription"));
		    if (configSetting.iOS_privacy_photo_additions_usage_available)
		    {
			    configSetting.iOS_privacy_photo_additions_usage_desc =
				    EditorGUILayout.TextField(configSetting.iOS_privacy_photo_additions_usage_desc);
		    }

		    configSetting.iOS_privacy_camera_usage_available = EditorGUILayout.ToggleLeft(
			    new GUIContent(LoadPanelContent.GetInstance().getText("Privacy_Camera")),
			    configSetting.iOS_privacy_camera_usage_available);
		    EditorGUILayout.LabelField(new GUIContent("NSCameraUsageDescription"));
		    if (configSetting.iOS_privacy_camera_usage_available)
		    {
			    configSetting.iOS_privacy_camera_usage_desc =
				    EditorGUILayout.TextField(configSetting.iOS_privacy_camera_usage_desc);
		    }

		    configSetting.iOS_privacy_contacts_usage_available = EditorGUILayout.ToggleLeft(
			    new GUIContent(LoadPanelContent.GetInstance().getText("Privacy_Contacts")),
			    configSetting.iOS_privacy_contacts_usage_available);
		    EditorGUILayout.LabelField(new GUIContent("NSContactsUsageDescription"));
		    if (configSetting.iOS_privacy_contacts_usage_available)
		    {
			    configSetting.iOS_privacy_contacts_usage_desc =
				    EditorGUILayout.TextField(configSetting.iOS_privacy_contacts_usage_desc);
		    }

		    configSetting.iOS_privacy_app_tracking_usage_available = EditorGUILayout.ToggleLeft(
			    new GUIContent(LoadPanelContent.GetInstance().getText("Privacy_Tracking")),
			    configSetting.iOS_privacy_app_tracking_usage_available);
		    EditorGUILayout.LabelField(new GUIContent("NSUserTrackingUsageDescription"));
		    if (configSetting.iOS_privacy_app_tracking_usage_available)
		    {
			    configSetting.iOS_privacy_app_tracking_usage_desc =
				    EditorGUILayout.TextField(configSetting.iOS_privacy_app_tracking_usage_desc);
		    }

		    configSetting.iOS_privacy_location_usage_available = EditorGUILayout.ToggleLeft(
			    new GUIContent(LoadPanelContent.GetInstance().getText("Privacy_Location_Inuse_Usage")),
			    configSetting.iOS_privacy_location_usage_available);
		    EditorGUILayout.LabelField(new GUIContent("NSLocationWhenInUseUsageDescription"));
		    if (configSetting.iOS_privacy_location_usage_available)
		    {
			    configSetting.iOS_privacy_location_usage_desc =
				    EditorGUILayout.TextField(configSetting.iOS_privacy_location_usage_desc);
		    }

		    configSetting.iOS_privacy_location_always_usage_available = EditorGUILayout.ToggleLeft(
			    new GUIContent(LoadPanelContent.GetInstance().getText("Privacy_Location_Always_Usage")),
			    configSetting.iOS_privacy_location_always_usage_available);
		    EditorGUILayout.LabelField(new GUIContent("NSLocationAlwaysUsageDescription"));
		    if (configSetting.iOS_privacy_location_always_usage_available)
		    {
			    configSetting.iOS_privacy_location_always_usage_desc =
				    EditorGUILayout.TextField(configSetting.iOS_privacy_location_always_usage_desc);
		    }

		    configSetting.iOS_privacy_calendars_usage_available = EditorGUILayout.ToggleLeft(
			    new GUIContent(LoadPanelContent.GetInstance().getText("Privacy_Calendars")),
			    configSetting.iOS_privacy_calendars_usage_available);
		    EditorGUILayout.LabelField(new GUIContent("NSCalendarsUsageDescription"));
		    if (configSetting.iOS_privacy_calendars_usage_available)
		    {
			    configSetting.iOS_privacy_calendars_usage_desc =
				    EditorGUILayout.TextField(configSetting.iOS_privacy_calendars_usage_desc);
		    }

		    configSetting.iOS_privacy_reminders_usage_available = EditorGUILayout.ToggleLeft(
			    new GUIContent(LoadPanelContent.GetInstance().getText("Privacy_Reminders")),
			    configSetting.iOS_privacy_reminders_usage_available);
		    EditorGUILayout.LabelField(new GUIContent("NSRemindersUsageDescription"));
		    if (configSetting.iOS_privacy_reminders_usage_available)
		    {
			    configSetting.iOS_privacy_reminders_usage_desc =
				    EditorGUILayout.TextField(configSetting.iOS_privacy_reminders_usage_desc);
		    }

		    configSetting.iOS_privacy_speech_recognition_usage_available = EditorGUILayout.ToggleLeft(
			    new GUIContent(LoadPanelContent.GetInstance().getText("Privacy_Speech_Recognition")),
			    configSetting.iOS_privacy_speech_recognition_usage_available);
		    EditorGUILayout.LabelField(new GUIContent("NSSpeechRecognitionUsageDescription"));
		    if (configSetting.iOS_privacy_speech_recognition_usage_available)
		    {
			    configSetting.iOS_privacy_speech_recognition_usage_desc =
				    EditorGUILayout.TextField(configSetting.iOS_privacy_speech_recognition_usage_desc);
		    }

		    configSetting.iOS_privacy_siri_usage_available = EditorGUILayout.ToggleLeft(
			    new GUIContent(LoadPanelContent.GetInstance().getText("Privacy_Siri")),
			    configSetting.iOS_privacy_siri_usage_available);
		    EditorGUILayout.LabelField(new GUIContent("NSSiriUsageDescription"));
		    if (configSetting.iOS_privacy_siri_usage_available)
		    {
			    configSetting.iOS_privacy_siri_usage_desc =
				    EditorGUILayout.TextField(configSetting.iOS_privacy_siri_usage_desc);
		    }

		    configSetting.iOS_privacy_face_ID_usage_available = EditorGUILayout.ToggleLeft(
			    new GUIContent(LoadPanelContent.GetInstance().getText("Privacy_FaceID")),
			    configSetting.iOS_privacy_face_ID_usage_available);
		    EditorGUILayout.LabelField(new GUIContent("NSFaceIDUsageDescription"));
		    if (configSetting.iOS_privacy_face_ID_usage_available)
		    {
			    configSetting.iOS_privacy_face_ID_usage_desc =
				    EditorGUILayout.TextField(configSetting.iOS_privacy_face_ID_usage_desc);
		    }


		    #region ATT idfa弹窗

		    LayoutSpecialModuleSpace();

		    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Privacy_Idfa")));
		    configSetting.iOS_is_ATT_available = EditorGUILayout.ToggleLeft(
			    new GUIContent(LoadPanelContent.GetInstance().getText("Privacy_Idfa_Enable")),
			    configSetting.iOS_is_ATT_available);
		    if (configSetting.iOS_is_ATT_available)
		    {
			    configSetting.iOS_ATT_is_need_DIY_call = EditorGUILayout.ToggleLeft(
				    new GUIContent(LoadPanelContent.GetInstance().getText("Privacy_Idfa_Pop")),
				    configSetting.iOS_ATT_is_need_DIY_call);
			    configSetting.iOS_ATT_is_use_CP_DIY_view = EditorGUILayout.ToggleLeft(
				    new GUIContent(LoadPanelContent.GetInstance().getText("Privacy_Idfa_Pop_Style")),
				    configSetting.iOS_ATT_is_use_CP_DIY_view);
			    configSetting.iOS_ATT_is_need_gsdk_defalut_view = EditorGUILayout.ToggleLeft(
				    new GUIContent(LoadPanelContent.GetInstance().getText("Privacy_Idfa_Pop_Ui")),
				    configSetting.iOS_ATT_is_need_gsdk_defalut_view);
			    if (configSetting.iOS_ATT_is_need_gsdk_defalut_view)
			    {
				    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
					    .getText("Privacy_Idfa_Cancel_Color")));
				    configSetting.iOS_ATT_gsdk_defalut_view_cancel_title_color =
					    EditorGUILayout.TextField(configSetting.iOS_ATT_gsdk_defalut_view_cancel_title_color);
				    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
					    .getText("Privacy_Idfa_Cancel_Button_Color")));
				    configSetting.iOS_ATT_gsdk_defalut_view_cancel_color =
					    EditorGUILayout.TextField(configSetting.iOS_ATT_gsdk_defalut_view_cancel_color);
				    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
					    .getText("Privacy_Idfa_Cancel_Button_Text")));
				    configSetting.iOS_ATT_gsdk_defalut_view_cancel_title =
					    EditorGUILayout.TextField(configSetting.iOS_ATT_gsdk_defalut_view_cancel_title);

				    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
					    .getText("Privacy_Idfa_Confirm_Color")));
				    configSetting.iOS_ATT_gsdk_defalut_view_confirm_title_color =
					    EditorGUILayout.TextField(configSetting.iOS_ATT_gsdk_defalut_view_confirm_title_color);
				    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
					    .getText("Privacy_Idfa_Confirm_Button_Color")));
				    configSetting.iOS_ATT_gsdk_defalut_view_confirm_color =
					    EditorGUILayout.TextField(configSetting.iOS_ATT_gsdk_defalut_view_confirm_color);
				    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
					    .getText("Privacy_Idfa_Confirm_Text")));
				    configSetting.iOS_ATT_gsdk_defalut_view_confirm_title =
					    EditorGUILayout.TextField(configSetting.iOS_ATT_gsdk_defalut_view_confirm_title);

				    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
					    .getText("Privacy_Idfa_Text")));
				    configSetting.iOS_ATT_gsdk_defalut_view_content =
					    EditorGUILayout.TextField(configSetting.iOS_ATT_gsdk_defalut_view_content);
				    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
					    .getText("Privacy_Idfa_Title")));
				    configSetting.iOS_ATT_gsdk_defalut_view_title =
					    EditorGUILayout.TextField(configSetting.iOS_ATT_gsdk_defalut_view_title);
			    }
		    }

		    #endregion

		    #endregion
	    }
	    LayoutModuleSpace();
    }

    protected abstract void LoginConfigInfoGUI();

    protected void PayConfigInfoGUI()
    {
	    
	    LayoutModuleSpace();
    }

    protected void ShareConfigInfoGUI()
    {
	    EditorGUILayout.HelpBox(moduleIndex++ + ") "+LoadPanelContent.GetInstance().getText("Share_Title"), MessageType.None);
        
	    EditorGUILayout.LabelField(new GUIContent("SharePanelID（android）"));
	    configSetting.andriod_share_panel_id = EditorGUILayout.TextField(configSetting.andriod_share_panel_id);
	    EditorGUILayout.LabelField(new GUIContent("SharePanelID（iOS）"));
	    configSetting.iOS_share_panel_id = EditorGUILayout.TextField(configSetting.iOS_share_panel_id);
	    configSetting.share_dim_amount_available = EditorGUILayout.ToggleLeft(new GUIContent(LoadPanelContent.GetInstance().getText("Share_Dim_Enable")), configSetting.share_dim_amount_available);
	    if (configSetting.share_dim_amount_available)
	    {
		    EditorGUILayout.LabelField (new GUIContent(LoadPanelContent.GetInstance().getText("Share_Dim") + "[?]", LoadPanelContent.GetInstance().getText("Share_Dim_Tips")));
		    configSetting.share_dim_amount = EditorGUILayout.TextField(configSetting.share_dim_amount);
	    }
    }

    protected abstract void ShareRegionInfoGUI();

    private void PushConfigInfoGUI()
    {
	    EditorGUILayout.HelpBox(moduleIndex++ + ") "+LoadPanelContent.GetInstance().getText("Push_Title"), MessageType.None);

	    GUIContent pushComponentDesc = new GUIContent(LoadPanelContent.GetInstance().getText("Push_Enable"));
	    configSetting.push_enable = EditorGUILayout.ToggleLeft(pushComponentDesc, configSetting.push_enable);
	    if (configSetting.push_enable)
	    {
		    LayoutSpecialModuleSpace();
		    androidPushFoldoutShow = EditorGUILayout.Foldout(androidPushFoldoutShow, LoadPanelContent.GetInstance().getText("Panel_Android_Specific"), getUniqueFoldoutConfigStyle());
		    if (androidPushFoldoutShow)
		    {
			    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Push_AppName")));
			    configSetting.android_push_app_name = EditorGUILayout.TextField(configSetting.android_push_app_name);

			    if (!string.IsNullOrEmpty(configSetting.android_push_huawei_appid))
			    {
				    EditorGUILayout.LabelField(new GUIContent("com.huawei.hms.client.appid" + LoadPanelContent
					    .GetInstance()
					    .getText("Common_Manifest_tips")));
				    EditorGUILayout.TextField("appid=" + configSetting.android_push_huawei_appid);
			    }

			    if (!string.IsNullOrEmpty(configSetting.android_push_mi_app_id))
			    {
				    EditorGUILayout.LabelField(new GUIContent("ttgame_push_mi_app_id" + LoadPanelContent.GetInstance()
					    .getText("Common_Manifest_tips")));
				    EditorGUILayout.TextField("\\" + configSetting.android_push_mi_app_id);
			    }

			    if (!string.IsNullOrEmpty(configSetting.android_push_mi_app_key))
			    {
				    EditorGUILayout.LabelField(new GUIContent("ttgame_push_mi_app_key" + LoadPanelContent.GetInstance()
					    .getText("Common_Manifest_tips")));
				    EditorGUILayout.TextField("\\" + configSetting.android_push_mi_app_key);
			    }

			    if (!string.IsNullOrEmpty(configSetting.android_push_meizu_app_id))
			    {
				    EditorGUILayout.LabelField(new GUIContent("ttgame_push_meizu_app_id" + LoadPanelContent
					    .GetInstance()
					    .getText("Common_Manifest_tips")));
				    EditorGUILayout.TextField("\\" + configSetting.android_push_meizu_app_id);
			    }

			    if (!string.IsNullOrEmpty(configSetting.android_push_meizu_app_key))
			    {
				    EditorGUILayout.LabelField(new GUIContent("ttgame_push_meizu_app_key" + LoadPanelContent
					    .GetInstance()
					    .getText("Common_Manifest_tips")));
				    EditorGUILayout.TextField(configSetting.android_push_meizu_app_key);
			    }

			    if (!string.IsNullOrEmpty(configSetting.android_push_oppo_app_key))
			    {
				    EditorGUILayout.LabelField(new GUIContent("ttgame_push_oppo_app_key" + LoadPanelContent
					    .GetInstance()
					    .getText("Common_Manifest_tips")));
				    EditorGUILayout.TextField(configSetting.android_push_oppo_app_key);
			    }

			    if (!string.IsNullOrEmpty(configSetting.android_push_oppo_app_secret))
			    {
				    EditorGUILayout.LabelField(new GUIContent("ttgame_push_oppo_app_secret" + LoadPanelContent
					    .GetInstance()
					    .getText("Common_Manifest_tips")));
				    EditorGUILayout.TextField(configSetting.android_push_oppo_app_secret);
			    }

			    if (!string.IsNullOrEmpty(configSetting.android_push_umeng_app_key))
			    {
				    EditorGUILayout.LabelField(new GUIContent("ttgame_push_umeng_app_key" + LoadPanelContent
					    .GetInstance()
					    .getText("Common_Manifest_tips")));
				    EditorGUILayout.TextField(configSetting.android_push_umeng_app_key);
			    }

			    if (!string.IsNullOrEmpty(configSetting.android_push_umeng_app_secret))
			    {
				    EditorGUILayout.LabelField(new GUIContent("ttgame_push_umeng_app_secret" + LoadPanelContent
					    .GetInstance()
					    .getText("Common_Manifest_tips")));
				    EditorGUILayout.TextField(configSetting.android_push_umeng_app_secret);
			    }

		    }
	    }
	    
	    LayoutModuleSpace();
    }

    // 广告归因（Adjust、AppsFlyer、Firebase）
    private void AdAttributionConfigInfoGUI()
    {
    }


    // 应用商店评价
    private void RatingConfigInfoGUI()
    {
	    EditorGUILayout.HelpBox(moduleIndex++ + ") "+LoadPanelContent.GetInstance().getText("AppStore_Title"), MessageType.None);
	    
	    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("AppStore_ID")));
	    configSetting.iOS_rating_app_store_id = EditorGUILayout.TextField(configSetting.iOS_rating_app_store_id);
	    
	    LayoutModuleSpace();
    }

    private void DeepLinkConfigInfoGUI()
    {
	    EditorGUILayout.HelpBox(moduleIndex++ + ") "+LoadPanelContent.GetInstance().getText("DeepLink_Title"), MessageType.None);
	    GUIContent deepLinkComponentDesc = new GUIContent(LoadPanelContent.GetInstance().getText("DeepLink_Enable"));
	    configSetting.deeplink_enable = EditorGUILayout.ToggleLeft(deepLinkComponentDesc, configSetting.deeplink_enable);
	    if (configSetting.deeplink_enable)
	    {
		    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("DeepLink_ShemeList_Size")));
		    configSetting.deeplink_schema_listsize = EditorGUILayout.IntField(configSetting.deeplink_schema_listsize);
		    if (configSetting.deeplink_schema_listsize > 0)
		    {
			    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("DeepLink_ShemeList")));
			    var count = 1;
			    var newList = new List<string>();
			    if (configSetting.deeplink_schema_list != null)
			    {
				    foreach (var schema in configSetting.deeplink_schema_list)
				    {
					    var newValue=EditorGUILayout.TextField(schema);
					    newList.Add(newValue);
					    count++;
					    if (count > configSetting.deeplink_schema_listsize)
					    {
						    break;
					    }
				    }
			    }
			    for ( ;count <= configSetting.deeplink_schema_listsize; count++)
			    {
				    var newValue = EditorGUILayout.TextField("");
				    newList.Add(newValue);
			    }
			    configSetting.deeplink_schema_list = newList;
		    }
		    else
		    {
			    configSetting.deeplink_schema_list = new List<string>();
		    }
		    
		    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("DeepLink_Url")));
		    configSetting.deeplink_baseurl_for_fission =
			    EditorGUILayout.TextField(configSetting.deeplink_baseurl_for_fission);

		    LayoutSpecialModuleSpace();
		    androidDeepLinkFoldoutShow = EditorGUILayout.Foldout(androidDeepLinkFoldoutShow, LoadPanelContent.GetInstance().getText("Panel_Android_Specific"), getUniqueFoldoutConfigStyle());
		    if (androidDeepLinkFoldoutShow)
		    {
			    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
				    .getText("DeepLink_Host_ListSize")));
			    configSetting.android_deeplink_host_listsize =
				    EditorGUILayout.IntField(configSetting.android_deeplink_host_listsize);
			    if (configSetting.android_deeplink_host_listsize > 0)
			    {
				    EditorGUILayout.LabelField(new GUIContent(
					    LoadPanelContent.GetInstance().getText("DeepLink_Host_List") + "[?]",
					    LoadPanelContent.GetInstance().getText("DeepLink_Host_List_Tips")));
				    var count = 1;
				    var newList = new List<string>();
				    if (configSetting.android_deeplink_host_list != null)
				    {
					    foreach (var host in configSetting.android_deeplink_host_list)
					    {
						    var newValue = EditorGUILayout.TextField(host);
						    newList.Add(newValue);
						    count++;
						    if (count > configSetting.android_deeplink_host_listsize)
						    {
							    break;
						    }
					    }
				    }
		    
				    for (; count <= configSetting.android_deeplink_host_listsize; count++)
				    {
					    var newValue = EditorGUILayout.TextField("");
					    newList.Add(newValue);
				    }
		    
				    configSetting.android_deeplink_host_list = newList;
			    }
			    else
			    {
				    configSetting.android_deeplink_host_list = new List<string>();
			    }
		    
			    EditorGUILayout.LabelField(new GUIContent(
				    LoadPanelContent.GetInstance().getText("DeepLink_Delay_Millis") + "[?]",
				    LoadPanelContent.GetInstance().getText("DeepLink_Delay_Value")));
			    var delayMillis = EditorGUILayout.IntField(configSetting.android_deeplink_delay_millis);
			    if (delayMillis >= 0)
			    {
				    configSetting.android_deeplink_delay_millis = delayMillis;
			    }
			    else
			    {
				    Debug.LogError("DeepLink Delay Millis只支持正数");
			    }
		    
			    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
				    .getText("DeepLink_Regex_ListSize")));
			    var token_regex_size = 0;
			    if (configSetting.android_default_token_regex != null)
			    {
				    token_regex_size = configSetting.android_default_token_regex.Count;
			    }
		    
			    token_regex_size = EditorGUILayout.IntField(token_regex_size);
			    if (token_regex_size > 0)
			    {
				    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance()
					    .getText("DeepLink_Regex_List")));
				    var count = 1;
				    var newList = new List<string>();
				    if (configSetting.android_default_token_regex != null)
				    {
					    foreach (var host in configSetting.android_default_token_regex)
					    {
						    var newValue = EditorGUILayout.TextField(host);
						    newList.Add(newValue);
						    count++;
						    if (count > token_regex_size)
						    {
							    break;
						    }
					    }
				    }
		    
				    for (; count <= token_regex_size; count++)
				    {
					    var newValue = EditorGUILayout.TextField("");
					    newList.Add(newValue);
				    }
		    
				    configSetting.android_default_token_regex = newList;
			    }
			    else
			    {
				    configSetting.android_default_token_regex = new List<string>();
			    }
		    }
	    }
	    
	    LayoutModuleSpace();
    }
    
    private void IMConfigInfoGUI()
    {
	    EditorGUILayout.HelpBox(moduleIndex++ + ") "+LoadPanelContent.GetInstance().getText("IM_Title"), MessageType.None);
	    GUIContent imComponentDesc = new GUIContent(LoadPanelContent.GetInstance().getText("IM_Enable"));
	    configSetting.im_enable = EditorGUILayout.ToggleLeft(imComponentDesc, configSetting.im_enable);
	    if (configSetting.im_enable)
	    {
		    EditorGUILayout.LabelField(new GUIContent("IMAppID" + LoadPanelContent.GetInstance().getText("IM_Enter_Number")));
		    configSetting.im_appid = EditorGUILayout.TextField(configSetting.im_appid);
		    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("IM_MethodId") +
		                                              LoadPanelContent.GetInstance().getText("IM_Enter_Number")));
		    configSetting.im_frontier_method_key = EditorGUILayout.TextField(configSetting.im_frontier_method_key);
		    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("IM_ServiceId") +
		                                              LoadPanelContent.GetInstance().getText("IM_Enter_Number")));
		    configSetting.im_frontier_service_key = EditorGUILayout.TextField(configSetting.im_frontier_service_key);
		    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("IM_Frontier_BoeUrl")));
		    configSetting.im_frontier_boe_baseurl = EditorGUILayout.TextField(configSetting.im_frontier_boe_baseurl);
		    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("IM_Frontier_Url")));
		    configSetting.im_frontier_baseurl = EditorGUILayout.TextField(configSetting.im_frontier_baseurl);
		    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("IM_Inbox_Size")));
		    var inboxes_size = 0;
		    if (configSetting.im_inboxes != null)
		    {
			    inboxes_size = configSetting.im_inboxes.Count;
		    }
		    inboxes_size = EditorGUILayout.IntField(inboxes_size);
		    if (inboxes_size > 0)
		    {
			    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("IM_Inbox")));
			    var count = 1;
			    var newList = new List<int>();
			    if (configSetting.im_inboxes != null)
			    {
				    foreach (var host in configSetting.im_inboxes)
				    {
					    var newValue = EditorGUILayout.IntField(host);
					    newList.Add(newValue);
					    count++;
					    if (count > inboxes_size)
					    {
						    break;
					    }
				    }
			    }

			    for (; count <= inboxes_size; count++)
			    {
				    var newValue = EditorGUILayout.IntField(0);
				    newList.Add(newValue);
			    }
			    configSetting.im_inboxes = newList;
		    }
		    else
		    {
			    configSetting.im_inboxes = new List<int>();
		    }

		    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("IM_FrontierId") +
		                                              LoadPanelContent.GetInstance().getText("IM_Enter_Number")));
		    configSetting.im_frontier_id = EditorGUILayout.TextField(configSetting.im_frontier_id);
		    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("IM_Http_Url")));
		    configSetting.im_http_baseurl = EditorGUILayout.TextField(configSetting.im_http_baseurl);
		    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("IM_Http_BoeUrl")));
		    configSetting.im_http_boe_baseurl = EditorGUILayout.TextField(configSetting.im_http_boe_baseurl);
		    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("IM_AppKey")));
		    configSetting.im_frontier_app_key = EditorGUILayout.TextField(configSetting.im_frontier_app_key);
	    }
	    
	    LayoutModuleSpace();
    }

    private void LiveConfigInfoGUI()
    {
	    EditorGUILayout.HelpBox(moduleIndex++ + ") "+LoadPanelContent.GetInstance().getText("Live_Title"), MessageType.None);
	    GUIContent liveRecordComponentDesc = new GUIContent(LoadPanelContent.GetInstance().getText("Live_Enable"));
	    configSetting.live_enable = EditorGUILayout.ToggleLeft(liveRecordComponentDesc, configSetting.live_enable);
	    if (configSetting.live_enable)
	    {
		    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Live_DouYin_Key")));
		    configSetting.android_douyin_key = EditorGUILayout.TextField(configSetting.android_douyin_key);

		    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Live_Scheme")));
		    configSetting.iOS_live_scheme = EditorGUILayout.TextField(configSetting.iOS_live_scheme);
		    
		    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Live_UrlType_Id")));
		    configSetting.iOS_kh_id = EditorGUILayout.TextField(configSetting.iOS_kh_id);
		    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("Live_UrlType_Schema")));
		    configSetting.iOS_kh_schema = EditorGUILayout.TextField(configSetting.iOS_kh_schema);
	    }

	    LayoutModuleSpace();
    }

    private void RTCConfigInfoGUI()
    {
	    EditorGUILayout.HelpBox(moduleIndex++ + ") RTC", MessageType.None);

	    configSetting.rtc_available = EditorGUILayout.ToggleLeft(new GUIContent(LoadPanelContent.GetInstance().getText("RTC_Title")), configSetting.rtc_available);
	    if (configSetting.rtc_available)
	    {
		    EditorGUILayout.LabelField(new GUIContent("RTC AppID"));
		    configSetting.rtc_app_id = EditorGUILayout.TextField(configSetting.rtc_app_id);
	    }
	    
	    LayoutModuleSpace();
    }

    private void ADConfigInfoGUI()
    {
	    EditorGUILayout.HelpBox(moduleIndex++ + ") "+LoadPanelContent.GetInstance().getText("AD_Title"), MessageType.None);

	    EditorGUILayout.LabelField(new GUIContent(LoadPanelContent.GetInstance().getText("AD_Pangle_Title")));
	    configSetting.pangle_enable =
		    EditorGUILayout.ToggleLeft(new GUIContent(LoadPanelContent.GetInstance().getText("AD_Pangle_Enable")), configSetting.pangle_enable);
	    if (configSetting.pangle_enable)
	    {
		    EditorGUILayout.LabelField(new GUIContent("Pangle AppID"));
		    configSetting.pangle_appid = EditorGUILayout.TextField(configSetting.pangle_appid);
	    }
	    LayoutModuleSpace();
	    
    }

    private void ThanosConfigInfoGUI()
    {
	    EditorGUILayout.HelpBox(moduleIndex++ + ") "+LoadPanelContent.GetInstance().getText("Thanos_Title"), MessageType.None);
	    GUIContent ThanosSaveSubChannelDesc =
			    new GUIContent(LoadPanelContent.GetInstance().getText("Thanos_SaveSubChannel"));
		configSetting.android_save_sub_channel = EditorGUILayout.ToggleLeft(ThanosSaveSubChannelDesc, configSetting.android_save_sub_channel);
	    
	    
	    LayoutModuleSpace();
    }

    public void AndroidSaveConfigToPluginAssetsGUI()
    {
	    if (GUILayout.Button("保存配置信息到Plugin/assets目录下(android)"))
	    {
		    configSetting.androidSaveConfigToPluginAssets();
	    }
    }

    // 大模块之间间隔
    protected void LayoutModuleSpace()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }
    
    // 特殊模块之间间隔
    protected void LayoutSpecialModuleSpace()
    {
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }
    protected GUIStyle getUniqueFoldoutConfigStyle()
    {
	    GUIStyle uniqueConfigStyle = new GUIStyle(EditorStyles.foldout);
	    uniqueConfigStyle.fontSize = 13;
	    uniqueConfigStyle.fontStyle = FontStyle.Bold;
	    return uniqueConfigStyle;
    }

    private void GetParametersOnGDev()
    {
	    GUIStyle btnTipStyle = new GUIStyle(UnityEditor.EditorStyles.label);
	    btnTipStyle.fontSize = 15;
	    btnTipStyle.normal.textColor = Color.red;
	    if (IsPullingParam)
	    {
		    GUILayout.Label(LoadPanelContent.GetInstance().getText("Common_Param_Pulling"), btnTipStyle,
			    GUILayout.Height(30));
		    pullResult = null;
	    }

	    if (pullResult != null)
	    {
		    string errorMsg;
		    if (pullResult.errorCode == 0)
		    {
			    errorMsg = LoadPanelContent.GetInstance().getText("Common_Param_Pull_Success");
		    }
		    else
		    {
			    errorMsg = LoadPanelContent.GetInstance().getText("Common_Param_Pull_Error") + pullResult.errorMsg;
		    }

		    GUILayout.Label(errorMsg, btnTipStyle, GUILayout.Height(30));
	    }
	    GUIStyle btnStyle = new GUIStyle("Button");
	    btnStyle.fontSize = 20;
	    if (GUILayout.Button(LoadPanelContent.GetInstance().getText("Common_Pull_Param_Gdev"), btnStyle,
		    GUILayout.Height(40)))
	    {
		    Debug.Log("IsPullingParam:" + IsPullingParam);
		    GUI.FocusControl(null);
		    if (!IsPullingParam && EditorUtility.DisplayDialog(LoadPanelContent.GetInstance().getText("Common_Tips"),
			    LoadPanelContent.GetInstance().getText("Common_Conditions_Use"),
			    LoadPanelContent.GetInstance().getText("Common_Pull"),
			    LoadPanelContent.GetInstance().getText("Common_Cancel")))
		    {
			    IsPullingParam = true;
			    Debug.Log("Parameter pulling");
			    configSetting.getParametersOnGDev((PullResult result) =>
			    {
				    pullResult = result;
				    if (result != null && result.errorCode == 0)
				    {
					    Debug.Log("Start refreshing parameters");
					    CombinParamManager.ParamCombin(result.paramsString, configSetting._sdkConfigModule);
					    BaseSDKConfigSetting.Instance._sdkConfigModule = configSetting._sdkConfigModule;
					    configSetting.SaveConfigSettings();
					    Repaint();
					    Debug.Log("Parameters refreshed successfully");
				    }
				    else
				    {
					    Debug.Log("isNeedUpdateUi is false");
				    }
				    IsPullingParam = false;
			    });
		    }
	    }
    }
}