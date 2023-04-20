using System;
using UnityEngine;
using System.IO;
using GMSDKUnityEditor.XCodeEditor;
using UNBridgeLib.LitJson;
using UnityEditor;

public class DeployIOSPlist
{
    static GMSDKEnv env = GMSDKEnv.Instance;
    static string iosConfigPath = env.PATH_IOS_PLIST;

    private static SDKConfigModule _sdkConfigModule = BaseSDKConfigSetting.Instance._sdkConfigModule;
    
    public static void Deploy(string projectPath)
    {
	    string fullPath = Path.Combine(env.PATH_CONFIG_SETTINGS, BaseSDKConfigSetting.SDKSettingConfigName); 

	    if (System.IO.File.Exists(fullPath))
	    {
		    string configSettingJson = File.ReadAllText(fullPath);
		    _sdkConfigModule = JsonMapper.ToObject<SDKConfigModule>(configSettingJson);
	    }
	    
        string path = Path.GetFullPath(projectPath);

		ModifyEntitlement (path);

		//动态修改 GMSDKXcodeConfig.projmod 文件的配置
        CopyFrameworks(path);
        CopyOtherFiles(path);

        EditorMod(path);

        // 修改 plist 文件
        EditorPlist(path);

        // 修改 xcode 代码(UnityAppController.mm)
        EditorAppControllerCode(path);
    }

	private static void ModifyEntitlement(string pathToBuiltProject)
	{
		string entitlementPath = env.PATH_RESOURCES + "/app.entitlements";

		string entitlementContent = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" +
			"<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\n" +
			"<plist version=\"1.0\">\n" +
			"<dict>\n";
		if (_sdkConfigModule.pushConfigModule.push_enable) {
			entitlementContent += "\t<key>aps-environment</key>\n" +
				"\t<string>development</string>\n";
		}
		if (_sdkConfigModule.loginConfigModule.iOS_apple_login_enable) {
			entitlementContent += "\t<key>com.apple.developer.applesignin</key>\n" +
				"\t<array>\n" +
				"\t\t<string>Default</string>\n" +
				"\t</array>\n";
		}

		if (_sdkConfigModule.commonModule.iOS_associated_domains_list_size > 0)
		{
			entitlementContent += "\t<key>com.apple.developer.associated-domains</key>\n";
			entitlementContent += "\t<array>\n";
			foreach (var deepLinkAssociatedDomain in _sdkConfigModule.commonModule.iOS_associated_domains_list)
			{
				entitlementContent += "\t\t<string>"+ deepLinkAssociatedDomain +"</string>\n";
			}
			entitlementContent += "\t</array>\n";
		}
		
		entitlementContent += "</dict>\n" + 
		                      "</plist>";

		File.WriteAllText (entitlementPath, entitlementContent);
	}

    private static void CopyFrameworks(string pathToBuiltProject)
    {
		string destDir = pathToBuiltProject + "/GMSDK";
        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }
		GMSDKUtil.RunCmdSync("unzip", "gsdk.zip -d " + destDir, env.PATH_LIBRARYS_IOS + "/Library", false);
		
		// GMSDKUtil.CopyDir(env.PATH_LIBRARYS_IOS + "/Library/GMSDK", destDir, true);
		GMSDKUtil.CopyFile(env.PATH_RESOURCES + "/app.entitlements",
				destDir + "/app.entitlements", false);
		GMSDKUtil.ReplaceDir(env.PATH_RESOURCES + "/app.entitlements",
				destDir + "/app.entitlements");

		var buildInfoFile = Path.Combine(env.PATH_ASSETS_GSDK, "gsdk.json");
		if (File.Exists(buildInfoFile))
		{
			GMSDKUtil.CopyFileWithOverwrite(buildInfoFile, destDir + "/gsdk.json");
		}
    }

    private static void CopyOtherFiles(string pathToBuiltProject)
    {
        if (!GMSDKUtil.isUnityEarlierThan("5.0"))
        {
            return;
        }

		string destDir = pathToBuiltProject + "/GMSDK";
        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

		GMSDKUtil.CopyFile(env.PATH_RESOURCES + "/GMConfig.rb", pathToBuiltProject + "/GMConfig.rb", false);
		GMSDKUtil.CopyFile(env.PATH_RESOURCES + "/ruby.zip", pathToBuiltProject + "/ruby.zip", false);

    }

    private static void EditorMod(string pathToBuiltProject)
    {
		GMSDKUtil.ReplaceTextStringWithRegex(env.PATH_EDITOR + "/Resources/GMSDKXcodeConfig.gsdkprojmods", ".*/GMSDK/", "        \"" + pathToBuiltProject + "/GMSDK/");
    }

    private static void EditorPlist(string filePath)
    {
	    generateIOSPlist();
        XCPlistSelf list = new XCPlistSelf(filePath);
        string plistAdd = File.ReadAllText(iosConfigPath);
        list.AddKey(plistAdd);
        list.Save();
    }

    private static void EditorAppControllerCode(string projectPath)
    {
        string ocFile = projectPath + "/Classes/UnityAppController.mm";

        StreamReader streamReader = new StreamReader(ocFile);
        string text_all = streamReader.ReadToEnd();
        streamReader.Close();
        if (string.IsNullOrEmpty(text_all))
        {
            return;
        }

        //添加头文件
        GMSDKUtil.WriteBelow(ocFile, GMSDKNativeCode.IOS_SRC_HEADER, GMSDKNativeCode.IOS_HEADER);

        GMSDKUtil.WriteBelow(ocFile, GMSDKNativeCode.IOS_SRC_FINISH, GMSDKNativeCode.IOS_GMZ_FINISH);

		if (!GMSDKUtil.ExistLine(ocFile, GMSDKNativeCode.IOS_SRC_OPENURL))
		{
			GMSDKUtil.WriteUplow(ocFile, GMSDKNativeCode.IOS_SRC_REGISTER, GMSDKNativeCode.IOS_SRC_OPENURL + "\n\n    return YES;\n}\n");
		}

		GMSDKUtil.ReplaceLineBelow(ocFile, GMSDKNativeCode.IOS_SRC_OPENURL, "return ", GMSDKNativeCode.IOS_HANDLE_URL);

		if (GMSDKUtil.ExistLine(ocFile, GMSDKNativeCode.IOS_SRC_OPENURL_IOS9))
		{
			GMSDKUtil.ReplaceLineBelow(ocFile, GMSDKNativeCode.IOS_SRC_OPENURL_IOS9, "return ", GMSDKNativeCode.IOS_GMA_OPTIONS_CONTENT);
		}
		else
		{
			GMSDKUtil.WriteUplow(ocFile, GMSDKNativeCode.IOS_SRC_OPENURL, GMSDKNativeCode.IOS_GMA_OPTIONS);
		}

		if (!GMSDKUtil.ExistLine (ocFile, GMSDKNativeCode.IOS_CONTINUEACTIVITY_HEAD_EXIST_CHECK)) {
			GMSDKUtil.WriteUplow(ocFile, GMSDKNativeCode.IOS_SRC_OPENURL, GMSDKNativeCode.IOS_CONTINUEACTIVITY_HEAD + GMSDKNativeCode.IOS_CONTINUEACTIVITY_CONTENT + GMSDKNativeCode.IOS_CONTINUEACTIVITY_END);
		}    
	}

    // 生成plist文件
    private static void generateIOSPlist() {

		//UnityEngine.Debug.LogError(_sdkConfigModule.commonModule.needModifyInfoPlist ? "needModifyInfoPlist == yes" : "needModifyInfoPlist == no");
		if (_sdkConfigModule.commonModule.needModifyInfoPlist == false)
        {
			File.WriteAllText(iosConfigPath, "");
			return;
        }


	    string PlistContent = "";

	    if (PlayerSettings.statusBarHidden)
	    {
		    PlistContent += "<key>UIViewControllerBasedStatusBarAppearance</key>\n <false/>\n";
	    }

	    if (_sdkConfigModule.commonModule.iOS_bundleId.Length > 0)
        {
			PlistContent += "<key>CFBundleIdentifier</key>\n    <string>" + _sdkConfigModule.commonModule.iOS_bundleId + "</string>\n";
		}
        if (_sdkConfigModule.commonModule.iOS_app_display_name.Length > 0) {
			PlistContent += "<key>CFBundleDisplayName</key>\n    <string>" + _sdkConfigModule.commonModule.iOS_app_display_name + "</string>\n";
		}

        PlistContent += "<key>CFBundleURLTypes</key>\n";
        PlistContent += "<array>\n";

	    if (_sdkConfigModule.loginConfigModule.toutiao_login_enable) {
			PlistContent += "\t\t<dict>\n\t\t\t<key>CFBundleTypeRole</key>\n\t\t\t<string>Editor</string>\n\t\t\t<key>CFBundleURLName</key>\n\t\t\t<string>toutiao</string>\n\t\t\t<key>CFBundleURLSchemes</key>\n\t\t\t<array>\n\t\t\t\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.loginConfigModule.toutiao_platform_key) + "</string>\n\t\t\t</array>\n\t\t</dict>\n";
		}
		if (_sdkConfigModule.loginConfigModule.aweme_login_enable || _sdkConfigModule.shareConfigModule.iOS_awe_available) {
			PlistContent += "\t\t<dict>\n\t\t\t<key>CFBundleTypeRole</key>\n\t\t\t<string>Editor</string>\n\t\t\t<key>CFBundleURLName</key>\n\t\t\t<string>douyin</string>\n\t\t\t<key>CFBundleURLSchemes</key>\n\t\t\t<array>\n\t\t\t\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.loginConfigModule.aweme_platform_key) + "</string>\n\t\t\t</array>\n\t\t</dict>\n";
		}
		if (_sdkConfigModule.loginConfigModule.huoshan_login_enable) {
			PlistContent += "\t\t<dict>\n\t\t\t<key>CFBundleTypeRole</key>\n\t\t\t<string>Editor</string>\n\t\t\t<key>CFBundleURLName</key>\n\t\t\t<string>huoshan</string>\n\t\t\t<key>CFBundleURLSchemes</key>\n\t\t\t<array>\n\t\t\t\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.loginConfigModule.huoshan_platform_key) + "</string>\n\t\t\t</array>\n\t\t</dict>\n";
		}
		if (_sdkConfigModule.loginConfigModule.xigua_login_enable) {
			PlistContent += "\t\t<dict>\n\t\t\t<key>CFBundleTypeRole</key>\n\t\t\t<string>Editor</string>\n\t\t\t<key>CFBundleURLName</key>\n\t\t\t<string>xiguaopen</string>\n\t\t\t<key>CFBundleURLSchemes</key>\n\t\t\t<array>\n\t\t\t\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.loginConfigModule.xigua_platform_key) + "</string>\n\t\t\t</array>\n\t\t</dict>\n";
		}

		if (_sdkConfigModule.shareConfigModule.iOS_qq_available) {
			PlistContent += "\t\t<dict>\n\t\t\t<key>CFBundleTypeRole</key>\n\t\t\t<string>Editor</string>\n\t\t\t<key>CFBundleURLName</key>\n\t\t\t<string>tencentApiIdentifier</string>\n\t\t\t<key>CFBundleURLSchemes</key>\n\t\t\t<array>\n\t\t\t\t<string>tencent" + _sdkConfigModule.shareConfigModule.iOS_qq_key + "</string>\n\t\t\t</array>\n\t\t</dict>\n";
		}
		if (_sdkConfigModule.shareConfigModule.iOS_wx_available) {
			PlistContent += "\t\t<dict>\n\t\t\t<key>CFBundleTypeRole</key>\n\t\t\t<string>Editor</string>\n\t\t\t<key>CFBundleURLName</key>\n\t\t\t<string>weixin</string>\n\t\t\t<key>CFBundleURLSchemes</key>\n\t\t\t<array>\n\t\t\t\t<string>" + _sdkConfigModule.shareConfigModule.iOS_wx_key + "</string>\n\t\t\t</array>\n\t\t</dict>\n";
		}
		if (_sdkConfigModule.shareConfigModule.iOS_weibo_available) {
			PlistContent += "\t\t<dict>\n\t\t\t<key>CFBundleTypeRole</key>\n\t\t\t<string>Editor</string>\n\t\t\t<key>CFBundleURLName</key>\n\t\t\t<string>weibo</string>\n\t\t\t<key>CFBundleURLSchemes</key>\n\t\t\t<array>\n\t\t\t\t<string>wb" + _sdkConfigModule.shareConfigModule.iOS_weibo_key + "</string>\n\t\t\t</array>\n\t\t</dict>\n";
		}
	    
	    if (_sdkConfigModule.liveConfigModule.enable)
		{
			PlistContent += "\t\t<dict>\n\t\t\t<key>CFBundleTypeRole</key>\n\t\t\t<string>Editor</string>\n\t\t\t<key>CFBundleURLName</key>\n\t\t\t<string>own</string>\n\t\t\t<key>CFBundleURLSchemes</key>\n\t\t\t<array>\n\t\t\t\t<string>" + _sdkConfigModule.liveConfigModule.iOS_live_scheme + "</string>\n\t\t\t</array>\n\t\t</dict>\n";
			PlistContent += "\t\t<dict>\n\t\t\t<key>CFBundleTypeRole</key>\n\t\t\t<string>Editor</string>\n\t\t\t<key>CFBundleURLName</key>\n\t\t\t<string>"+ _sdkConfigModule.liveConfigModule.iOS_kh_id + "</string>\n\t\t\t<key>CFBundleURLSchemes</key>\n\t\t\t<array>\n\t\t\t\t<string>" + _sdkConfigModule.liveConfigModule.iOS_kh_scheme + "</string>\n\t\t\t</array>\n\t\t</dict>\n";
		}

		if (_sdkConfigModule.commonModule.iOS_own_url_scheme_list_size > 0)
		{
			for (int i = 0; i < _sdkConfigModule.commonModule.iOS_own_url_scheme_list_size; i++)
			{
				PlistContent += "\t\t<dict>\n\t\t\t<key>CFBundleTypeRole</key>\n\t\t\t<string>Editor</string>\n\t\t\t<key>CFBundleURLName</key>\n\t\t\t<string>own</string>\n\t\t\t<key>CFBundleURLSchemes</key>\n\t\t\t<array>\n\t\t\t\t<string>" + _sdkConfigModule.commonModule.iOS_own_url_scheme_list[i] + "</string>\n\t\t\t</array>\n\t\t</dict>\n";
			}
		}

		if (!string.IsNullOrEmpty(ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.commonModule.app_id)))
		{
			string appurl = "snssdk" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.commonModule.app_id);
			PlistContent += "\t\t<dict>\n\t\t\t<key>CFBundleTypeRole</key>\n\t\t\t<string>Editor</string>\n\t\t\t<key>CFBundleURLName</key>\n\t\t\t<string>own</string>\n\t\t\t<key>CFBundleURLSchemes</key>\n\t\t\t<array>\n\t\t\t\t<string>" + appurl + "</string>\n\t\t\t</array>\n\t\t</dict>\n";
		}

	    
	    PlistContent += "</array>\n";

		PlistContent += "<key>LSApplicationQueriesSchemes</key>\n<array>\n";
		if (_sdkConfigModule.commonModule.iOS_application_queries_schemes_list_size > 0) {
			for (int i = 0; i < _sdkConfigModule.commonModule.iOS_application_queries_schemes_list_size; i++)
			{
				PlistContent += "\t<string>" + _sdkConfigModule.commonModule.iOS_application_queries_schemes_list[i] + "</string>\n";
			}
		}

	    if (_sdkConfigModule.loginConfigModule.toutiao_login_enable) {
			PlistContent += "\t\t<string>toutiaoopensdk</string>\n";
		}
		if (_sdkConfigModule.loginConfigModule.aweme_login_enable) {
			PlistContent += "\t\t<string>douyinopensdk</string>\n";
			PlistContent += "\t\t<string>douyinopensdksm</string>\n";
			PlistContent += "\t\t<string>douyinv1opensdk</string>\n";
		}
		if (_sdkConfigModule.loginConfigModule.huoshan_login_enable) {
			PlistContent += "\t\t<string>huoshanopensdk</string>\n";
		}
		if (_sdkConfigModule.loginConfigModule.xigua_login_enable) {
			PlistContent += "\t\t<string>xiguaopensdk</string>\n";
			PlistContent += "\t\t<string>snssdk32</string>\n";
		}
		if (_sdkConfigModule.shareConfigModule.iOS_wx_available) {
			PlistContent += "\t\t<string>wechat</string>\n";
			PlistContent += "\t\t<string>weixin</string>\n";
			PlistContent += "\t\t<string>weixinULAPI</string>\n";
		}
		if (_sdkConfigModule.shareConfigModule.iOS_weibo_available) {
			PlistContent += "\t\t<string>sinaweibosso.com.ss.iphone.article.news</string>\n";
			PlistContent += "\t\t<string>sinaweibosso.ss.essay.joke</string>\n";
			PlistContent += "\t\t<string>sinaweibosso</string>\n";
			PlistContent += "\t\t<string>sinaweibo</string>\n";
			PlistContent += "\t\t<string>sinaweibohd</string>\n";
			PlistContent += "\t\t<string>sinaweibohdsso</string>\n";
			PlistContent += "\t\t<string>weibosdk</string>\n";
			PlistContent += "\t\t<string>weibosdk2.5</string>\n";
            PlistContent += "\t\t<string>weibosdk3.3</string>\n";
			PlistContent += "\t\t<string>weibo</string>\n";
		}
		if (_sdkConfigModule.shareConfigModule.iOS_awe_available) {
			PlistContent += "\t\t<string>douyinsharesdk</string>\n";
			PlistContent += "\t\t<string>awemesso</string>\n";
            PlistContent += "\t\t<string>snssdk1128</string>\n";
			if (!_sdkConfigModule.loginConfigModule.aweme_login_enable) {
				PlistContent += "\t\t<string>douyinopensdk</string>\n";
			}
		}
		if (_sdkConfigModule.shareConfigModule.iOS_qq_available) {
			PlistContent += "\t\t<string>mqqsecure</string>\n";
			PlistContent += "\t\t<string>wtloginmqq2</string>\n";
			PlistContent += "\t\t<string>mqqopensdkapiV3</string>\n";
			PlistContent += "\t\t<string>mqqwpa</string>\n";
			PlistContent += "\t\t<string>mqqOpensdkSSoLogin</string>\n";
			PlistContent += "\t\t<string>mqq</string>\n";
			PlistContent += "\t\t<string>mqqapi</string>\n";
			PlistContent += "\t\t<string>mqqopensdkapiV2</string>\n";
			PlistContent += "\t\t<string>mqzoneopensdk</string>\n";
			PlistContent += "\t\t<string>mqzoneopensdkapi</string>\n";
			PlistContent += "\t\t<string>mqzoneopensdkapi19</string>\n";
			PlistContent += "\t\t<string>mqzoneopensdkapiV2</string>\n";
			PlistContent += "\t\t<string>mqzone</string>\n";
		}


	    PlistContent += "</array>\n";

	    
	    
	    
	    
		PlistContent += "<key>GameConfig</key>\n<dict>\n";
		if (!String.IsNullOrEmpty(_sdkConfigModule.commonModule.download_source))
		{
			PlistContent +=
				"\t\t\t\t<key>download_source</key>\n" +
				"\t\t\t\t<string>" + _sdkConfigModule.commonModule.download_source + "</string>\n";
		}
		PlistContent +=
			"\t\t\t\t<key>accountLoginPanelCanBeClosed</key>\n\t\t\t\t" +
			(_sdkConfigModule.loginConfigModule.login_panel_can_be_closed ? "<true/>" : "<false/>");
		PlistContent += "\t\t\t\t<key>usePacketChannel</key>\n\t\t\t\t<" + _sdkConfigModule.commonModule.use_packet_channel.ToString().ToLower() + "/>\n";
		
		if (ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.commonModule.app_id).Length > 0)
        {
			PlistContent += "\t\t<key>appId</key>\n\t\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.commonModule.app_id) + "</string>\n";
		}
		if (ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.commonModule.app_name).Length > 0)
        {
			PlistContent += "\t\t<key>appName</key>\n\t\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.commonModule.app_name) + "</string>\n";
		}
		
		if (ConfigSettingParamHandleUtils.GetIOSLongConfigValue(_sdkConfigModule.commonModule.init_timeout) > 0)
        {
			PlistContent += "\t\t<key>gsdk_init_timeout</key>\n\t\t<real>" + (ConfigSettingParamHandleUtils.GetIOSLongConfigValue(_sdkConfigModule.commonModule.init_timeout) / 1000).ToString() + "</real>\n";
		}


		string enableDR = _sdkConfigModule.commonModule.drOptimization ? "<true/>" : "<false/>";
        PlistContent += "\t\t<key>device_register_optimization</key>\n\t\t" + enableDR + "\n";
        PlistContent += "\t\t<key>device_register_retry_count</key>\n\t\t<integer>" + _sdkConfigModule.commonModule.drRetryCount + "</integer>\n";
        
        if (ConfigSettingParamHandleUtils.GetIOSLongConfigValue(_sdkConfigModule.commonModule.account_init_timeout) > 0)
        {
	        PlistContent += "\t\t<key>gsdk_account_init_timeout</key>\n\t\t<real>" + (ConfigSettingParamHandleUtils.GetIOSLongConfigValue(_sdkConfigModule.commonModule.account_init_timeout)) + "</real>\n";
        }
        
		
		PlistContent += "\t\t<key>LoginItems</key>\n\t\t<array>\n";

	    if (_sdkConfigModule.loginConfigModule.toutiao_login_enable) {
			PlistContent += "\t\t\t<dict>\n\t\t\t\t<key>key</key>\n\t\t\t\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.loginConfigModule.toutiao_platform_key) + "</string>\n\t\t\t\t<key>platformId</key>\n\t\t\t\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.loginConfigModule.toutiao_platform_id) + "</string>\n\t\t\t\t<key>type</key>\n\t\t\t\t<string>tt</string>\n";
			if (_sdkConfigModule.loginConfigModule.toutiao_friend_permission)
			{
				PlistContent += "\t\t\t\t<key>friend_permission</key>\n\t\t\t\t<true/>\n";
			}
			PlistContent += "\t\t\t</dict>\n";
		}
		if (_sdkConfigModule.loginConfigModule.aweme_login_enable) {
			PlistContent += "\t\t\t<dict>\n\t\t\t\t<key>key</key>\n\t\t\t\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.loginConfigModule.aweme_platform_key) + "</string>\n\t\t\t\t<key>platformId</key>\n\t\t\t\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.loginConfigModule.aweme_platform_id) + "</string>\n\t\t\t\t<key>type</key>\n\t\t\t\t<string>awe</string>\n";
			if (_sdkConfigModule.loginConfigModule.aweme_friend_permission)
			{
				PlistContent += "\t\t\t\t<key>friend_permission</key>\n\t\t\t\t<true/>\n";
			}
			if (_sdkConfigModule.loginConfigModule.aweme_relation_follow)
			{
				PlistContent += "\t\t\t\t<key>relationFollow_permission</key>\n\t\t\t\t<true/>\n";
			}
			if (_sdkConfigModule.loginConfigModule.aweme_relation_follow_default_check)
			{
				PlistContent += "\t\t\t\t<key>aweme_relation_follow_default_check</key>\n\t\t\t\t<true/>\n";
			}
			if (_sdkConfigModule.loginConfigModule.aweme_video_list_data_permission)
			{
				PlistContent += "\t\t\t\t<key>video_permission</key>\n\t\t\t\t<true/>\n";
			}
			if (_sdkConfigModule.loginConfigModule.aweme_dy_real_name_permission)
			{
				PlistContent += "\t\t\t\t<key>realNameAuth_permission</key>\n\t\t\t\t<true/>\n";
			}
			if (_sdkConfigModule.loginConfigModule.aweme_mobile_permission)
			{
				PlistContent += "\t\t\t\t<key>mobile_permission</key>\n\t\t\t\t<true/>\n";
			}
			if (_sdkConfigModule.loginConfigModule.aweme_friend_list)
            {
				PlistContent += "\t\t\t\t<key>friend_list_permission</key>\n\t\t\t\t<true/>\n";
            }
			if (_sdkConfigModule.loginConfigModule.aweme_card_permission)
			{
				PlistContent += "\t\t\t\t<key>userInfoCard_permission</key>\n\t\t\t\t<true/>\n";
			}

			if (_sdkConfigModule.loginConfigModule.aweme_relation_user_follow_permission)
			{
				PlistContent += "\t\t\t\t<key>relationUserFollow_permission</key>\n\t\t\t\t<true/>\n";
			}
			
			if (_sdkConfigModule.loginConfigModule.aweme_user_external_data_permission)
			{
				PlistContent += "\t\t\t\t<key>user_external_data_permission</key>\n\t\t\t\t<true/>\n";
			}

			PlistContent += "\t\t\t</dict>\n";
		}
		if (_sdkConfigModule.loginConfigModule.huoshan_login_enable) {
			PlistContent += "\t\t\t<dict>\n\t\t\t\t<key>key</key>\n\t\t\t\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.loginConfigModule.huoshan_platform_key) + "</string>\n\t\t\t\t<key>platformId</key>\n\t\t\t\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.loginConfigModule.huoshan_platform_id) + "</string>\n\t\t\t\t<key>type</key>\n\t\t\t\t<string>huoshan</string>\n";
			PlistContent += "\t\t\t</dict>\n";
		}
		if (_sdkConfigModule.loginConfigModule.xigua_login_enable) {
			PlistContent += "\t\t\t<dict>\n\t\t\t\t<key>key</key>\n\t\t\t\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.loginConfigModule.xigua_platform_key) + "</string>\n\t\t\t\t<key>platformId</key>\n\t\t\t\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.loginConfigModule.xigua_platform_id) + "</string>\n\t\t\t\t<key>type</key>\n\t\t\t\t<string>ixigua</string>\n";
			PlistContent += "\t\t\t</dict>\n";
		}

		
		if (_sdkConfigModule.loginConfigModule.iOS_apple_login_enable) {
			PlistContent += "\t\t\t<dict>\n\t\t\t\t<key>platformId</key>\n\t\t\t\t<string>" + _sdkConfigModule.loginConfigModule.iOS_apple_platform_id + "</string>\n\t\t\t\t<key>type</key>\n\t\t\t\t<string>apple</string>\n\t\t\t</dict>\n";
		}
		PlistContent += "\t\t</array>\n";

	    if (_sdkConfigModule.loginConfigModule.login_priority >= 0)
	    {
		    PlistContent += "\t\t<key>login_priority</key>\n\t\t<integer>" + _sdkConfigModule.loginConfigModule.login_priority + "</integer>";
	    }
	    
	    PlistContent += "\t\t<key>phoneOperator</key>\n\t\t<array>\n";
        PlistContent += "\t\t\t<dict>\n\t\t\t\t<key>key</key>\n\t\t\t\t<string>" + _sdkConfigModule.loginConfigModule.iOS_cm_app_id + "</string>\n\t\t\t\t<key>secret</key>\n\t\t\t\t<string>" + _sdkConfigModule.loginConfigModule.iOS_cm_app_key + "</string>\n\t\t\t\t<key>name</key>\n\t\t\t\t<string>mobile</string>\n\t\t\t</dict>\n";
        PlistContent += "\t\t\t<dict>\n\t\t\t\t<key>key</key>\n\t\t\t\t<string>" + _sdkConfigModule.loginConfigModule.iOS_ct_app_key + "</string>\n\t\t\t\t<key>secret</key>\n\t\t\t\t<string>" + _sdkConfigModule.loginConfigModule.iOS_ct_app_secret + "</string>\n\t\t\t\t<key>name</key>\n\t\t\t\t<string>telecom</string>\n\t\t\t</dict>\n";
        PlistContent += "\t\t\t<dict>\n\t\t\t\t<key>key</key>\n\t\t\t\t<string>" + _sdkConfigModule.loginConfigModule.iOS_cu_app_key + "</string>\n\t\t\t\t<key>secret</key>\n\t\t\t\t<string>" + _sdkConfigModule.loginConfigModule.iOS_cu_app_secret + "</string>\n\t\t\t\t<key>name</key>\n\t\t\t\t<string>union</string>\n\t\t\t</dict>\n";
        PlistContent += "\t\t</array>\n";

	    if (_sdkConfigModule.deepLinkConfigModule.enable)
	    {
		    PlistContent += "<key>DeepLink</key><dict><key>host</key><string>";
		    PlistContent += ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.deepLinkConfigModule.baseurl_for_fission) + "</string>";
		    if (_sdkConfigModule.deepLinkConfigModule.schema_list != null)
		    {
			    PlistContent += "<key>schemeList</key><array>";
			    foreach (var scheme in _sdkConfigModule.deepLinkConfigModule.schema_list)
			    {
				    PlistContent += "<string>";
				    PlistContent += scheme;
				    PlistContent += "</string>";
			    }
			    PlistContent += "</array>";
		    }
		    PlistContent+="</dict>";
	    }
        

        if (_sdkConfigModule.rtcConfigModule.rtc_available)
        {
	        PlistContent += "<key>rtc_init_appId</key>\n<string>";
	        PlistContent += ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.rtcConfigModule.rtc_app_id);
	        PlistContent += "</string>\n";
        }
        
#region ATT

        if (_sdkConfigModule.privacySecurityConfigModule.iOS_is_ATT_available)
        {
	        PlistContent += "\t<key>ATTPopUpConfig</key>\n\t" + "<dict>\n";
	        PlistContent += "\t\t\t<key>useCustomCallTiming</key>\n" + (_sdkConfigModule.privacySecurityConfigModule.iOS_is_need_DIY_call ? "\t\t\t<true/>\n" : "\t\t\t<false/>\n");
	        PlistContent += "\t\t\t<key>useFullCustomView</key>\n" + (_sdkConfigModule.privacySecurityConfigModule.iOS_is_use_CP_DIY_view ? "\t\t\t<true/>\n" : "\t\t\t<false/>\n");

	        if (_sdkConfigModule.privacySecurityConfigModule.iOS_is_need_gsdk_defalut_view)
	        {
		        PlistContent += "\t\t<key>DialogView</key>\n\t" + "\t\t<dict>\n";
		        PlistContent += "\t\t\t<key>Cancel</key>\n\t" + "\t\t\t<dict>\n";
		        PlistContent += "\t\t\t\t<key>CancelTitleColor</key>\n\t\t\t\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_cancel_title_color + "</string>\n";
		        PlistContent += "\t\t\t\t<key>CancelColor</key>\n\t\t\t\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_cancel_color + "</string>\n";
		        PlistContent += "\t\t\t\t<key>CancelTitle</key>\n\t\t\t\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_cancel_title + "</string>\n";
		        PlistContent += "\t\t\t</dict>\n";

		        PlistContent += "\t\t\t<key>Confirm</key>\n" + "\t\t\t<dict>\n";
		        PlistContent += "\t\t\t\t<key>ConfirmTitleColor</key>\n\t\t\t\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_confirm_title_color + "</string>\n";
		        PlistContent += "\t\t\t\t<key>ConfirmColor</key>\n\t\t\t\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_confirm_color + "</string>\n";
		        PlistContent += "\t\t\t\t<key>ConfirmTitle</key>\n\t\t\t\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_confirm_title + "</string>\n";
		        PlistContent += "\t\t\t</dict>\n";
			
		        PlistContent += "\t\t\t<key>Content</key>\n\t\t\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_content + "</string>\n";
		        PlistContent += "\t\t\t<key>Title</key>\n\t\t\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_gsdk_defalut_view_title + "</string>\n";
		        PlistContent += "\t\t</dict>\n";
	        }
	        PlistContent += "\t</dict>\n";
        }

#endregion
        if (_sdkConfigModule.shareConfigModule.iOS_share_panel_id.Length > 0)
        {
			PlistContent += "\t\t<key>sharePanelID</key>\n\t\t<string>" + _sdkConfigModule.shareConfigModule.iOS_share_panel_id + "</string>";
		}
		
		if (_sdkConfigModule.shareConfigModule.share_dim_amount_available)
		{
			PlistContent += "\t\t<key>sharePanelCoverAlpha</key>\n\t\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.shareConfigModule.share_dim_amount) + "</string>";
		}

	 if (_sdkConfigModule.shareConfigModule.iOS_qq_available || _sdkConfigModule.shareConfigModule.iOS_wx_available || _sdkConfigModule.shareConfigModule.iOS_weibo_available || _sdkConfigModule.shareConfigModule.iOS_awe_available)
        {
			PlistContent += "\t\t<key>ShareItems</key>\n\t\t<array>\n";
			if (_sdkConfigModule.shareConfigModule.iOS_qq_available)
			{
				PlistContent += "\t\t\t<dict>\n\t\t\t\t<key>key</key>\n\t\t\t\t<string>" + _sdkConfigModule.shareConfigModule.iOS_qq_key + "</string>\n\t\t\t\t<key>source</key>\n\t\t\t\t<string>qq</string>\n\t\t\t\t<key>universal</key>\n\t\t\t\t<string>" + _sdkConfigModule.shareConfigModule.iOS_qq_universal_link + "</string>\n\t\t\t</dict>\n";
				PlistContent += "\t\t\t<dict>\n\t\t\t\t<key>key</key>\n\t\t\t\t<string>" + _sdkConfigModule.shareConfigModule.iOS_qq_key + "</string>\n\t\t\t\t<key>source</key>\n\t\t\t\t<string>qzone</string>\n\t\t\t\t<key>universal</key>\n\t\t\t\t<string>" + _sdkConfigModule.shareConfigModule.iOS_qq_universal_link + "</string>\n\t\t\t</dict>\n";
			}
			if (_sdkConfigModule.shareConfigModule.iOS_wx_available)
			{
				PlistContent += "\t\t\t<dict>\n\t\t\t\t<key>key</key>\n\t\t\t\t<string>" + _sdkConfigModule.shareConfigModule.iOS_wx_key + "</string>\n\t\t\t\t<key>source</key>\n\t\t\t\t<string>wechat</string>\n\t\t\t\t<key>universal</key>\n\t\t\t\t<string>" + _sdkConfigModule.shareConfigModule.iOS_wx_universal_link + "</string>\n\t\t\t</dict>\n";
				PlistContent += "\t\t\t<dict>\n\t\t\t\t<key>key</key>\n\t\t\t\t<string>" + _sdkConfigModule.shareConfigModule.iOS_wx_key + "</string>\n\t\t\t\t<key>source</key>\n\t\t\t\t<string>wechatTimeline</string>\n\t\t\t\t<key>universal</key>\n\t\t\t\t<string>" + _sdkConfigModule.shareConfigModule.iOS_wx_universal_link + "</string>\n\t\t\t</dict>\n";
			}
			if (_sdkConfigModule.shareConfigModule.iOS_weibo_available)
			{
				PlistContent += "\t\t\t<dict>\n\t\t\t\t<key>key</key>\n\t\t\t\t<string>" + _sdkConfigModule.shareConfigModule.iOS_weibo_key + "</string>\n\t\t\t\t<key>source</key>\n\t\t\t\t<string>weibo</string>\n\t\t\t\t<key>universal</key>\n\t\t\t\t<string>" + _sdkConfigModule.shareConfigModule.iOS_weibo_universal_link + "</string>\n\t\t\t</dict>\n";
				PlistContent += "\t\t\t<dict>\n\t\t\t\t<key>key</key>\n\t\t\t\t<string>" + _sdkConfigModule.shareConfigModule.iOS_weibo_key + "</string>\n\t\t\t\t<key>source</key>\n\t\t\t\t<string>weibo_sg</string>\n\t\t\t\t<key>universal</key>\n\t\t\t\t<string>" + _sdkConfigModule.shareConfigModule.iOS_weibo_universal_link + "</string>\n\t\t\t</dict>\n";
			}
			if (_sdkConfigModule.shareConfigModule.iOS_awe_available)
			{
				PlistContent += "\t\t\t<dict>\n\t\t\t\t<key>key</key>\n\t\t\t\t<string>" + _sdkConfigModule.shareConfigModule.iOS_awe_key + "</string>\n\t\t\t\t<key>source</key>\n\t\t\t\t<string>awe</string>\n\t\t\t</dict>\n";
				PlistContent += "\t\t\t<dict>\n\t\t\t\t<key>key</key>\n\t\t\t\t<string>" + _sdkConfigModule.shareConfigModule.iOS_awe_key + "</string>\n\t\t\t\t<key>source</key>\n\t\t\t\t<string>aweim</string>\n\t\t\t</dict>\n";
			}
			PlistContent += "\t\t</array>\n";
		}

	    

	    
		if (_sdkConfigModule.imConfigModule.enable) {
			PlistContent += "<key>IMConfig</key>\n\t" + "<dict>\n";
			PlistContent += "<key>IMAppID</key>\n\t<string>" + ConfigSettingParamHandleUtils.GetIOSIntConfigValue(_sdkConfigModule.imConfigModule.aid) + "</string>\n";
			PlistContent += "<key>IMFrontierMethodKey</key>\n\t<integer>" + ConfigSettingParamHandleUtils.GetIOSIntConfigValue(_sdkConfigModule.imConfigModule.method) + "</integer>\n";
			PlistContent += "<key>IMFrontierServiceKey</key>\n\t<integer>" + ConfigSettingParamHandleUtils.GetIOSIntConfigValue(_sdkConfigModule.imConfigModule.service) + "</integer>\n";
			PlistContent += "<key>IMFrontierBOEBaseURL</key>\n\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.imConfigModule.ws_host_boe) + "</string>\n";
			PlistContent += "<key>IMFrontierBaseURL</key>\n\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.imConfigModule.ws_host) + "</string>\n";
			PlistContent += "<key>IMFrontierID</key>\n\t<string>" + ConfigSettingParamHandleUtils.GetIOSIntConfigValue(_sdkConfigModule.imConfigModule.fpid) + "</string>\n";
			PlistContent += "<key>IMHTTPBaseURL</key>\n\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.imConfigModule.http_host) + "</string>\n";
			PlistContent += "<key>IMHTTPBOEBaseURL</key>\n\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.imConfigModule.http_host_boe) + "</string>\n";
			PlistContent += "<key>IMFrontierAppKey</key>\n\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.imConfigModule.app_key) + "</string>\n";
			PlistContent += "<key>IMInboxes</key>\n\t" +"<array>\n\t";
			foreach (int inbox in _sdkConfigModule.imConfigModule.inboxes) {
				PlistContent += "<string>" + inbox + "</string>\n";
			}
			PlistContent += "</array>\n" +"    </dict>\n";
		}

		if (!string.IsNullOrEmpty(_sdkConfigModule.ratingConfigModule.iOS_rating_app_store_id))
		{
			PlistContent += "    <key>appleStoreAppId</key>\n    <string>" + _sdkConfigModule.ratingConfigModule.iOS_rating_app_store_id + "</string>\n";
		}

	    

	    if (!string.IsNullOrEmpty(_sdkConfigModule.commonModule.skin))
	    {
		    PlistContent += "    <key>skin</key>\n    <string>" + _sdkConfigModule.commonModule.skin + "</string>\n";
	    }

	    string isNeedAntiAddiction = _sdkConfigModule.commonModule.is_need_anti_addiction ? "<true/>" : "<false/>";
	    PlistContent += "\t\t\t\t<key>isNeedAntiAddiction</key>\n\t\t\t\t" + isNeedAntiAddiction + "\n";
	    
	    string isNeedServiceMouth = _sdkConfigModule.commonModule.is_need_service_mouth ? "<true/>" : "<false/>";
	    PlistContent += "\t\t\t\t<key>isNeedServiceMouth</key>\n\t\t\t\t" + isNeedServiceMouth + "\n";

	    string enableSDKPopup = _sdkConfigModule.privacySecurityConfigModule.is_show_permission_toast ? "<true/>" : "<false/>";
		PlistContent += "\t\t\t\t<key>enableSDKAuthPoping</key>\n\t\t\t\t" + enableSDKPopup + "\n";

	    if (_sdkConfigModule.adConfigModule.pangleConfigModule.enable)
	    {
		    PlistContent += "\t\t<key>AdConfig</key>\n\t\t<dict>\n";
		    PlistContent += "\t\t\t<key>PangleAppID</key>\n\t\t\t<string>" + ConfigSettingParamHandleUtils.GetIOSStringConfigValue(_sdkConfigModule.adConfigModule.pangleConfigModule.pangle_appid) + "</string>\n";
		    PlistContent += "\t\t</dict>\n";
	    }
		
		PlistContent += "\t\t</dict>\n";


		

	    if (_sdkConfigModule.adConfigModule.pangleConfigModule.enable)
	    {
		    PlistContent += "<key>SKAdNetworkItems</key>\n<array>\n\t<dict>\n\t\t<key>SKAdNetworkIdentifier</key>\n\t\t<string>238da6jt44.skadnetwork</string>\n\t</dict>\n\t<dict>\n\t\t<key>SKAdNetworkIdentifier</key>\n\t\t<string>x2jnk7ly8j.skadnetwork</string>\n\t</dict>\n\t<dict>\n\t\t<key>SKAdNetworkIdentifier</key>\n\t<string>f7s53z58qe.skadnetwork</string>\n\t</dict>\n</array>";	
	    }

	    if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_microphone_usage_available && _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_microphone_usage_desc.Length > 0) {
			PlistContent += "<key>NSMicrophoneUsageDescription</key>\n\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_microphone_usage_desc + "</string>\n";
		}
	    if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_usage_available && _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_usage_desc.Length > 0) {
			PlistContent += "<key>NSPhotoLibraryUsageDescription</key>\n\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_usage_desc + "</string>\n";
		}
		if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_additions_usage_available && _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_additions_usage_desc.Length > 0) {
			PlistContent += "<key>NSPhotoLibraryAddUsageDescription</key>\n\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_photo_additions_usage_desc + "</string>\n";
		}
		if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_camera_usage_available && _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_camera_usage_desc.Length > 0) {
			PlistContent += "<key>NSCameraUsageDescription</key>\n\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_camera_usage_desc + "</string>\n";
		}
		if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_contacts_usage_available && _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_contacts_usage_desc.Length > 0) {
			PlistContent += "<key>NSContactsUsageDescription</key>\n\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_contacts_usage_desc + "</string>\n";
		}
		if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_app_tracking_usage_available && _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_app_tracking_usage_desc.Length > 0)
		{
			PlistContent += "<key>NSUserTrackingUsageDescription</key>\n\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_app_tracking_usage_desc + "</string>\n";
		}
		if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_usage_available && _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_usage_desc.Length > 0) {
			PlistContent += "<key>NSLocationWhenInUseUsageDescription</key>\n\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_usage_desc + "</string>\n";
		}
		if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_always_usage_available && _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_always_usage_desc.Length > 0)
		{
			PlistContent += "<key>NSLocationAlwaysUsageDescription</key>\n\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_location_always_usage_desc + "</string>\n";
		}
		if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_calendars_usage_available && _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_calendars_usage_desc.Length > 0)
		{
			PlistContent += "<key>NSCalendarsUsageDescription</key>\n\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_calendars_usage_desc + "</string>\n";
		}
		if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_reminders_usage_available && _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_reminders_usage_desc.Length > 0)
		{
			PlistContent += "<key>NSRemindersUsageDescription</key>\n\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_reminders_usage_desc + "</string>\n";
		}
		if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_speech_recognition_usage_available && _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_speech_recognition_usage_desc.Length > 0)
		{
			PlistContent += "<key>NSSpeechRecognitionUsageDescription</key>\n\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_speech_recognition_usage_desc + "</string>\n";
		}
		if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_siri_usage_available && _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_siri_usage_desc.Length > 0)
		{
			PlistContent += "<key>NSSiriUsageDescription</key>\n\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_siri_usage_desc + "</string>\n";
		}
		if (_sdkConfigModule.privacySecurityConfigModule.iOS_privacy_face_ID_usage_available && _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_face_ID_usage_desc.Length > 0)
		{
			PlistContent += "<key>NSFaceIDUsageDescription</key>\n\t<string>" + _sdkConfigModule.privacySecurityConfigModule.iOS_privacy_face_ID_usage_desc + "</string>\n";
		}
		
		string fontFolderPath = Application.streamingAssetsPath + "/fonts";
		if(Directory.Exists(fontFolderPath)){
			// 增加自定义字体
			string fontsArrayString = string.Empty;
			DirectoryInfo assetFolder = new DirectoryInfo(fontFolderPath);
			//遍历文件
			foreach (FileInfo file in assetFolder.GetFiles())
			{
				if (file.Name.EndsWith(".ttf"))
				{
					fontsArrayString += "\t\t<string>Data/Raw/fonts/" + file.Name + "</string>\n";
				}
			}
			if(fontsArrayString.Length > 0)
			{
				PlistContent += "<key>UIAppFonts</key>\n\t<array>\n" + fontsArrayString + "\t</array>\n";
			}
		}

		File.WriteAllText(iosConfigPath, PlistContent);
	}
}