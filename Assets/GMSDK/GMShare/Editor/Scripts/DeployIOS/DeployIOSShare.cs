using UnityEngine;
using System.IO;
using System.Collections.Generic;
using GMSDKUnityEditor.XCodeEditor;

public class DeployIOSShare
{
	static GMShareEnv env = GMShareEnv.Instance;
	static string iosConfigPath = GMShareEnv.Instance.PATH_SHARE_IOS_PLIST;
    public static void Deploy(string projectPath)
    {
        string path = Path.GetFullPath(projectPath);

		//动态修改 GMShareXcodeConfig.projmod 文件的配置
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

		GMSDKUtil.CopyDir(env.PATH_SHARE_LIBRARYS_IOS + "/Library/GMSDK", destDir, true);
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
		GMSDKUtil.ReplaceTextStringWithRegex(env.PATH_SHARE_EDITOR + "/Resources/GMShareXcodeConfig.gsdkprojmods", ".*/GMSDK/", "        \"" + pathToBuiltProject + "/GMSDK/");
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
        string ocFile = projectPath + "/Classes/UnityAppController.mm";

        StreamReader streamReader = new StreamReader(ocFile);
        string text_all = streamReader.ReadToEnd();
        streamReader.Close();
        if (string.IsNullOrEmpty(text_all))
        {
            return;
        }

        //添加头文件
		GMSDKUtil.WriteBelow(ocFile, GMShareNativeCode.IOS_SRC_HEADER, GMShareNativeCode.IOS_HEADER);

//		GMSDKUtil.WriteBelow(ocFile, GMShareNativeCode.IOS_SRC_FINISH, GMShareNativeCode.IOS_GMZ_FINISH);
//
//		GMSDKUtil.WriteUplow(ocFile, GMShareNativeCode.IOS_SRC_REGISTER, GMShareNativeCode.IOS_GMA_OPTIONS);

		GMSDKUtil.WriteBelow(ocFile, GMShareNativeCode.IOS_SRC_OPENURL, GMShareNativeCode.IOS_HANDLE_URL);

		GMSDKUtil.ReplaceLineBelow(ocFile, GMShareNativeCode.IOS_SRC_OPENURL_OPTION, "return NO", GMShareNativeCode.IOS_GMA_OPTIONS);
		GMSDKUtil.WriteBelowWithAlterBelow(ocFile, GMSDKNativeCode.IOS_CONTINUEACTIVITY_HEAD, GMSDKNativeCode.IOS_UnitySetAbsoluteURL, GMShareNativeCode.IOS_CONTINUEACTIVITY_SHARE);
    }

	private static void generateIOSPlist() {
		string PlistContent = "";

		File.WriteAllText(iosConfigPath, PlistContent);
	}
}
