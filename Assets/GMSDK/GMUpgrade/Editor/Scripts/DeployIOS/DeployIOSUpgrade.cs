using System.IO;

public class DeployIOSUpgrade
{
    private static readonly GMUpgradeEnv _env = GMUpgradeEnv.Instance;
    
    public static void Deploy(string projectPath)
    {
        // 获得项目完整路径
        string path = Path.GetFullPath(projectPath);
        
        // 复制framework
        CopyFrameworks(path);
    }

    private static void CopyFrameworks(string projectPath)
    {
        string destDir = projectPath + "/GMSDK";
        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }
        GMSDKUtil.CopyDir(_env.PATH_UPGRADE_LIBRARYS_IOS + "/Library/GMSDK", destDir, true);
    }
}