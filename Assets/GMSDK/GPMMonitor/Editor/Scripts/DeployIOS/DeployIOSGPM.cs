using UnityEngine;
using System.IO;
using System.Collections.Generic;
using GMSDKUnityEditor.XCodeEditor;

public class DeployIOSGPM
{
	static GPMEnv env = GPMEnv.Instance;

    public static void Deploy(string projectPath)
    {
        string path = Path.GetFullPath(projectPath);

		//动态修改 GMVoiceXcodeConfig.projmod 文件的配置
        CopyFrameworks(path);
        CopyOtherFiles(path);

        EditorMod(path);

        // 修改 plist 文件
        EditorPlist(path);

        // 修改 xcode 代码(UnityAppController.mm)
        EditorAppControllerCode(path);
    }
    private static void CopyFrameworks(string pathToBuiltProject)
    {
		string destDir = pathToBuiltProject + "/GMSDK";
        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

		GMSDKUtil.CopyDir(env.PATH_GPM_LIBRARYS_IOS + "/Library/GMSDK", destDir, true);
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

    }

    private static void EditorMod(string pathToBuiltProject)
    {
		GMSDKUtil.ReplaceTextStringWithRegex(env.PATH_GPM_EDITOR + "/Resources/GPMMonitorXcodeConfig.gsdkprojmods", ".*/GMSDK/", "        \"" + pathToBuiltProject + "/GMSDK/");
    }

    private static void EditorPlist(string filePath)
    {
//		generateIOSPlist();
//        XCPlistSelf list = new XCPlistSelf(filePath);
//        string plistAdd = File.ReadAllText(iosConfigPath);
//        list.AddKey(plistAdd);
//        list.Save();
    }

    private static void EditorAppControllerCode(string projectPath)
    {
//        string ocFile = projectPath + "/Classes/UnityAppController.mm";
//
//        StreamReader streamReader = new StreamReader(ocFile);
//        string text_all = streamReader.ReadToEnd();
//        streamReader.Close();
//        if (string.IsNullOrEmpty(text_all))
//        {
//            return;
//        }
//
//        //添加头文件
//		GMSDKUtil.WriteBelow(ocFile, GMVoiceNativeCode.IOS_SRC_HEADER, GMVoiceNativeCode.IOS_HEADER);
//
//		GMSDKUtil.WriteBelow(ocFile, GMVoiceNativeCode.IOS_SRC_FINISH, GMVoiceNativeCode.IOS_GMZ_FINISH);
//
//		GMSDKUtil.WriteUplow(ocFile, GMVoiceNativeCode.IOS_SRC_REGISTER, GMVoiceNativeCode.IOS_GMA_OPTIONS);


    }

	private static void generateIOSPlist() {
//		ConfigModel configModel = ConfigSettings.Instance.configModel;
//		string PlistContent = "";
//
//		File.WriteAllText(iosConfigPath, PlistContent);
	}
}
