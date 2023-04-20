using UnityEngine;
using System.IO;
using System.Collections.Generic;
using GMSDKUnityEditor.XCodeEditor;

public class DeployIOSMagicBox
{
    static GMMagicBoxEnv env = GMMagicBoxEnv.Instance;
    public static void Deploy(string projectPath)
    {
        string path = Path.GetFullPath(projectPath);

        //动态修改 GMMagicBoxXcodeConfig.projmod 文件的配置
        CopyFrameworks(path);
        CopyOtherFiles(path);

    }
    private static void CopyFrameworks(string pathToBuiltProject)
    {
        string destDir = pathToBuiltProject + "/GMSDK";
        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }

        GMSDKUtil.CopyDir(env.PATH_MAGICBOX_LIBRARYS_IOS + "/Library/GMSDK", destDir, true);
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



}
