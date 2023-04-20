using UnityEngine;
using System.IO;
using System.Collections.Generic;
using GMSDKUnityEditor.XCodeEditor;

public class DeployIOSIM
{
	static GMIMEnv env = GMIMEnv.Instance;

    public static void Deploy(string projectPath)
    {
        string path = Path.GetFullPath(projectPath);

        CopyFrameworks(path);

    }
    private static void CopyFrameworks(string pathToBuiltProject)
    {
		string destDir = pathToBuiltProject + "/GMSDK";
		if (!Directory.Exists(destDir))
		{
			Directory.CreateDirectory(destDir);
		}

		GMSDKUtil.CopyDir(env.PATH_IM_LIBRARYS_IOS + "/Library/GMSDK", destDir, true);
    }

}
