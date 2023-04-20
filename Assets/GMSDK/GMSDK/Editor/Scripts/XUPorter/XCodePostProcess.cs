using UnityEngine;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using GMSDKUnityEditor.XCodeEditor;
#endif
using System;
using System.IO;
using System.Diagnostics;
using UNBridgeLib.LitJson;
using System.Collections.Generic;
using System.Collections;
using GMSDK;


public static class XCodePostProcessSDK
{
	private static SDKConfigModule _sdkConfigModule = loadSDKConfigModule();
	

#if UNITY_EDITOR && UNITY_IOS
	[PostProcessBuild(GMSDKEnv.XCodePostProcessPriority)]
	public static void OnPostProcessBuild( BuildTarget target, string pathToBuiltProject )
	{
		
#if UNITY_5 || UNITY_2017 || UNITY_2017_1_OR_NEWER
		if (target == BuildTarget.iOS)
        {
#else
        if (target == BuildTarget.iPhone) {
#endif
			UnityEngine.Debug.Log("Run XCodePostProcess to Config Xcode project.");
        }
        else
        {
            return;
        }

        // Create a new project object from build target
        XCProject project = new XCProject( pathToBuiltProject );

        DeployIOSPlist.Deploy(pathToBuiltProject);

		string projmodFilePath;
		projmodFilePath = GMSDKEnv.Instance.PATH_EDITOR + "/Resources/GMSDKXcodeConfig.gsdkprojmods";
		UnityEngine.Debug.Log("ProjMod File: " + projmodFilePath);
		project.ApplyMod( projmodFilePath );

        project.overwriteBuildSetting("ARCHS", "arm64");

        // Finally save the xcode project
        project.Save();

//        ChangePlist(pathToBuiltProject);

    }

	[PostProcessBuild(GMSDKEnv.XCodePostProcessPriority+1001)]
	public static void OnPostProcessBuildProjsMod( BuildTarget target, string pathToBuiltProject )
	{
#if UNITY_5 || UNITY_2017 || UNITY_2017_1_OR_NEWER
		if (target == BuildTarget.iOS)
		{
#else
	if (target == BuildTarget.iPhone) {
#endif
			UnityEngine.Debug.Log("Run XCodePostProcess to Config Xcode project.");
		}
		else
		{
			return;
		}

		string path = Path.GetFullPath(pathToBuiltProject);
		string destDir = path + "/GMSDK";
		if (!Directory.Exists(destDir))
		{
			UnityEngine.Debug.LogError("GMSDK folder not exists");
			return;
		}

		//生成json
		string sdkConfigString;
		//系统库
		List<string> systemFrameworkArray = new List<string>();
		systemFrameworkArray.Add("libz.tbd");
		systemFrameworkArray.Add("libxml2.tbd");
		systemFrameworkArray.Add("libc++.tbd");
		systemFrameworkArray.Add("libc++abi.tbd");
		systemFrameworkArray.Add("libresolv.tbd");
		systemFrameworkArray.Add("libbz2.tbd");
		systemFrameworkArray.Add("CoreTelephony.framework");
		systemFrameworkArray.Add("SystemConfiguration.framework");
		systemFrameworkArray.Add("UIKit.framework");
		systemFrameworkArray.Add("Foundation.framework");
		systemFrameworkArray.Add("CoreGraphics.framework");
		systemFrameworkArray.Add("MobileCoreServices.framework");
		systemFrameworkArray.Add("StoreKit.framework");
		systemFrameworkArray.Add("CFNetwork.framework");
		systemFrameworkArray.Add("CoreData.framework");
		systemFrameworkArray.Add("Security.framework");
		systemFrameworkArray.Add("CoreLocation.framework");
		systemFrameworkArray.Add("ImageIO.framework");
		systemFrameworkArray.Add("CoreText.framework");
		systemFrameworkArray.Add("QuartzCore.framework");
		systemFrameworkArray.Add("AdSupport.framework");
		systemFrameworkArray.Add("WebKit.framework");
		systemFrameworkArray.Add("MessageUI.framework");
		systemFrameworkArray.Add("OpenAL.framework");
		systemFrameworkArray.Add("Accelerate.framework");
		systemFrameworkArray.Add("VideoToolBox.framework");
		systemFrameworkArray.Add("MetalKit.framework");
		systemFrameworkArray.Add("JavaScriptCore.framework");
		systemFrameworkArray.Add("libcompression.tbd");
		systemFrameworkArray.Add("ReplayKit.framework");
		systemFrameworkArray.Add("MetalPerformanceShaders.framework");
		systemFrameworkArray.Add("GLKit.framework");
		systemFrameworkArray.Add("PhotosUI.framework");

		//weak
		List<string> weakSystemFrameworkArray = new List<string>();
		weakSystemFrameworkArray.Add("AuthenticationServices.framework");
		//weakSystemFrameworkArray.Add("AppTrackingTransparency.framework");
        weakSystemFrameworkArray.Add("iAd.framework");
		weakSystemFrameworkArray.Add("CoreML.framework");
		weakSystemFrameworkArray.Add("UserNotifications.framework");

        //Shell

        Hashtable shell2 = new Hashtable ();
		shell2.Add ("name", "GMShellSgmpreaction");
		shell2.Add ("script", "sh ./GMSDK/gsdk/JNiuNiu.bundle/sgmpre.sh nopod");
		shell2.Add ("top", false);

		Hashtable shell3 = new Hashtable ();
		shell3.Add ("name", "GMShelldSYMUpload");
		if (string.IsNullOrEmpty(_sdkConfigModule.commonModule.iOS_dsym_upload_key))
		{
			shell3.Add("script", "sh ./GMSDK/gsdk/dSYMUpload.sh CN " + _sdkConfigModule.commonModule.app_id);
		}
		else
		{
			shell3.Add("script", "sh ./GMSDK/gsdk/dSYMUpload.sh CN " + _sdkConfigModule.commonModule.app_id + " " + _sdkConfigModule.commonModule.iOS_dsym_upload_key);
		}
		shell3.Add ("top", false);

		Hashtable shell4 = new Hashtable ();
		shell4.Add ("name", "GMPostBuild");
		shell4.Add ("script", "sh ./GMSDK/gsdk/post_build.sh");
		shell4.Add ("top", false);

		List<Hashtable> ShellPhaseArray = new List<Hashtable>();
		ShellPhaseArray.Add(shell2);
        if (_sdkConfigModule.commonModule.iOS_dsym_auto_upload)
        {
            ShellPhaseArray.Add(shell3);
        }
		if (File.Exists(destDir + "/post_build.sh"))
		{
            ShellPhaseArray.Add(shell4);
		}

        List<Hashtable> BuildSettingsArray = new List<Hashtable>();
		if (_sdkConfigModule.commonModule.iOS_cert_success)
		{
 			Hashtable setting1 = new Hashtable();
			setting1.Add("targetName", _sdkConfigModule.commonModule.iOS_project_target_name);
			setting1.Add("settingsName", "CODE_SIGN_IDENTITY");
			setting1.Add("settingsValue", _sdkConfigModule.commonModule.iOS_cert_name);
			setting1.Add("configuration", "all");

			Hashtable setting11 = new Hashtable();
			setting11.Add("targetName", _sdkConfigModule.commonModule.iOS_project_target_name);
			setting11.Add("settingsName", "CODE_SIGN_IDENTITY[sdk=iphoneos*]");
			setting11.Add("settingsValue", _sdkConfigModule.commonModule.iOS_cert_name);
			setting11.Add("configuration", "all");

			Hashtable setting2 = new Hashtable();
			setting2.Add("targetName", _sdkConfigModule.commonModule.iOS_project_target_name);
			setting2.Add("settingsName", "PROVISIONING_PROFILE");
			setting2.Add("settingsValue", _sdkConfigModule.commonModule.iOS_provisioning_guid);
			setting2.Add("configuration", "all");

			Hashtable setting21 = new Hashtable();
			setting21.Add("targetName", _sdkConfigModule.commonModule.iOS_project_target_name);
			setting21.Add("settingsName", "PROVISIONING_PROFILE_SPECIFIER");
			setting21.Add("settingsValue", _sdkConfigModule.commonModule.iOS_provisioning_guid);
			setting21.Add("configuration", "all");

			Hashtable setting3 = new Hashtable();
			setting3.Add("targetName", _sdkConfigModule.commonModule.iOS_project_target_name);
			setting3.Add("settingsName", "CODE_SIGN_STYLE");
			setting3.Add("settingsValue", "Manual");
			setting3.Add("configuration", "all");

			Hashtable setting4 = new Hashtable();
			setting4.Add("targetName", _sdkConfigModule.commonModule.iOS_project_target_name);
			setting4.Add("settingsName", "DEVELOPMENT_TEAM");
			setting4.Add("settingsValue", _sdkConfigModule.commonModule.iOS_team_ID);
			setting4.Add("configuration", "all");

			BuildSettingsArray.Add(setting1);
			BuildSettingsArray.Add(setting11);
			BuildSettingsArray.Add(setting2);
			BuildSettingsArray.Add(setting21);
			BuildSettingsArray.Add(setting3);
			BuildSettingsArray.Add(setting4);
		}

		if (_sdkConfigModule.commonModule.iOS_debug_cert_success)
		{
			Hashtable setting1 = new Hashtable();
			setting1.Add("targetName", _sdkConfigModule.commonModule.iOS_project_target_name);
			setting1.Add("settingsName", "CODE_SIGN_IDENTITY");
			setting1.Add("settingsValue", _sdkConfigModule.commonModule.iOS_debug_cert_name);
			setting1.Add("configuration", "Debug");

			Hashtable setting11 = new Hashtable();
			setting11.Add("targetName", _sdkConfigModule.commonModule.iOS_project_target_name);
			setting11.Add("settingsName", "CODE_SIGN_IDENTITY[sdk=iphoneos*]");
			setting11.Add("settingsValue", _sdkConfigModule.commonModule.iOS_debug_cert_name);
			setting11.Add("configuration", "Debug");

			Hashtable setting2 = new Hashtable();
			setting2.Add("targetName", _sdkConfigModule.commonModule.iOS_project_target_name);
			setting2.Add("settingsName", "PROVISIONING_PROFILE");
			setting2.Add("settingsValue", _sdkConfigModule.commonModule.iOS_debug_provisioning_guid);
			setting2.Add("configuration", "Debug");

			Hashtable setting21 = new Hashtable();
			setting21.Add("targetName", _sdkConfigModule.commonModule.iOS_project_target_name);
			setting21.Add("settingsName", "PROVISIONING_PROFILE_SPECIFIER");
			setting21.Add("settingsValue", _sdkConfigModule.commonModule.iOS_debug_provisioning_guid);
			setting21.Add("configuration", "Debug");

			Hashtable setting4 = new Hashtable();
			setting4.Add("targetName", _sdkConfigModule.commonModule.iOS_project_target_name);
			setting4.Add("settingsName", "DEVELOPMENT_TEAM");
			setting4.Add("settingsValue", _sdkConfigModule.commonModule.iOS_debug_team_ID);
			setting4.Add("configuration", "Debug");
			
			Hashtable setting3 = new Hashtable();
			setting3.Add("targetName", _sdkConfigModule.commonModule.iOS_project_target_name);
			setting3.Add("settingsName", "CODE_SIGN_STYLE");
			setting3.Add("settingsValue", "Manual");
			setting3.Add("configuration", "Debug");

			BuildSettingsArray.Add(setting1);
			BuildSettingsArray.Add(setting11);
			BuildSettingsArray.Add(setting2);
			BuildSettingsArray.Add(setting21);
			BuildSettingsArray.Add(setting3);
			BuildSettingsArray.Add(setting4);
		}

		if (_sdkConfigModule.pushConfigModule.push_enable  || _sdkConfigModule.loginConfigModule.iOS_apple_login_enable)
		{
			Hashtable entitlementSetting = new Hashtable();
			entitlementSetting.Add("targetName", _sdkConfigModule.commonModule.iOS_project_target_name);
			entitlementSetting.Add("settingsName", "CODE_SIGN_ENTITLEMENTS");
			entitlementSetting.Add("settingsValue", "GMSDK/app.entitlements");
			entitlementSetting.Add("configuration", "all");

			BuildSettingsArray.Add(entitlementSetting);
		}
#if UNITY_2019_3_OR_NEWER
		List<Hashtable> ExposeHeadersArray = new List<Hashtable>();
		Hashtable UnityFrameworkHeader = new Hashtable();
		UnityFrameworkHeader.Add("targetName", _sdkConfigModule.commonModule.iOS_framework_target_name);
		UnityFrameworkHeader.Add("headerFile", "UnityFramework.h");
		ExposeHeadersArray.Add(UnityFrameworkHeader);
#endif

		Hashtable ht = new Hashtable ();
		ht.Add ("SystemFrameworks", systemFrameworkArray);
		ht.Add ("WeakSystemFrameworks", weakSystemFrameworkArray);
		ht.Add ("ShellPhase", ShellPhaseArray);
		ht.Add ("BuildSettings", BuildSettingsArray);
#if UNITY_2019_3_OR_NEWER
		ht.Add ("ExposeHeaders", ExposeHeadersArray);
#endif

		sdkConfigString = JsonMapper.ToJson(ht);	

		string configFilePath;
		configFilePath = destDir + "/GMSDKConfig.json";

		StreamWriter streamWriter = new StreamWriter(configFilePath);
		streamWriter.Write(sdkConfigString);
		streamWriter.Close();

		runRubyEnv(pathToBuiltProject);
	}

	// [MenuItem("GMSDK/Gen Xcode Projecttest", false, 3)]
	// public static void BuildIOS1()

	public static void runRubyEnv(string pathToBuiltProject)
	{
		// GMSDKEnv env = GMSDKEnv.Instance;
		// string XCODE_EXPORTPATH = env.WORKSPACE + "/xcode";
		// string pathToBuiltProject = XCODE_EXPORTPATH;
		string path = Path.GetFullPath(pathToBuiltProject);
		string destDir = path + "/GMSDK";

        string projPath = UnityEditor.iOS.Xcode.PBXProject.GetPBXProjectPath(path);

		string args = Path.GetFullPath(GMSDKEnv.Instance.PATH_EDITOR + "/Scripts/DeployIOS/GMConfig.sh") + " " + projPath + " " + _sdkConfigModule.commonModule.iOS_project_target_name + " " + destDir;
#if UNITY_2019_3_OR_NEWER
		args = args + " " + _sdkConfigModule.commonModule.iOS_framework_target_name;
#endif

		Process proc = new Process
		{
			StartInfo =
			{
				FileName = "/bin/sh",
				Arguments = args,
				CreateNoWindow = false,
				WindowStyle = ProcessWindowStyle.Normal,
				UseShellExecute = false,
				ErrorDialog =  false,
				RedirectStandardInput = false ,
				RedirectStandardOutput = true,
				RedirectStandardError = false,
				WorkingDirectory =  null
			}
		};
		// UnityEngine.Debug.LogError(Path.Combine(GMSDKEnv.Instance.MAIN_GSDK_FOLDER, "GMSDK/Editor/Scripts/DeployIOS/GMConfig.sh") + " " + projPath + " Unity-iPhone" + " " + destDir);
		proc.Start();
		proc.WaitForExit();
		var sr = proc.StandardOutput;
		string str = sr.ReadToEnd();
		proc.Close();

		if (str.Contains("GMConfig project success")) {
			//成功
		} else {
			//失败
			UnityEngine.Debug.LogError("导出Xcode过程中，ruby执行失败:");
			UnityEngine.Debug.LogError(str);
		}
	}

#endif

    public static void Log(string message)
    {
        UnityEngine.Debug.Log("PostProcess: " + message);
    }

    private static SDKConfigModule loadSDKConfigModule()
    {
	    string fullPath = Path.Combine(GMSDKEnv.Instance.PATH_CONFIG_SETTINGS,
		    BaseSDKConfigSetting.SDKSettingConfigName);

	    if (System.IO.File.Exists(fullPath))
	    {
		    string configSettingJson = File.ReadAllText(fullPath);
		    return JsonMapper.ToObject<SDKConfigModule>(configSettingJson);
	    }
	    else
	    {
		    return BaseSDKConfigSetting.Instance._sdkConfigModule;
	    }
    }
}
