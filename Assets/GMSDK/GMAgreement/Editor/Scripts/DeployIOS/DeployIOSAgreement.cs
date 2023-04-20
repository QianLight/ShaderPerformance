using UnityEngine;
using System.IO;
using System.Collections.Generic;
using GMSDKUnityEditor.XCodeEditor;

public class DeployIOSAgreement {
	static GMAgreementEnv env = GMAgreementEnv.Instance;

	public static void Deploy(string projectPath)
	{
		string path = Path.GetFullPath(projectPath);

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

		GMSDKUtil.CopyDir(env.PATH_Agreement_LIBRARYS_IOS + "/Library/GMSDK", destDir, true);
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
		GMSDKUtil.ReplaceTextStringWithRegex(env.PATH_Agreement_EDITOR + "/Resources/GMAgreementXcodeConfig.gsdkprojmods", ".*/GMSDK/", "        \"" + pathToBuiltProject + "/GMSDK/");
	}

	private static void EditorPlist(string filePath)
	{
	}

	private static void EditorAppControllerCode(string projectPath)
	{
	}

	private static void generateIOSPlist() 
	{
	}
}
