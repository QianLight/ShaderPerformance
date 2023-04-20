using UnityEngine;
using System.IO;
using System.Collections.Generic;
using GMSDKUnityEditor.XCodeEditor;

public class DeployIOSLocation
{
	static GMLocationEnv env = GMLocationEnv.Instance;

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

		GMSDKUtil.CopyDir(env.PATH_Location_LIBRARYS_IOS + "/Library/GMSDK", destDir, true);
    }

}
