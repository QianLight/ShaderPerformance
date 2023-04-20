using System.IO;

public class DeployIOSRTC
{
    private static readonly GMRTCEnv _env = GMRTCEnv.Instance;
    
    public static void Deploy(string projectPath)
    {
        string path = Path.GetFullPath(projectPath);
        
        CopyFrameworks(path);
        
        //动态修改 GMRTCXcodeConfig.projmod 文件的配置
        EditorMod(path);
    }
    
    
    private static void CopyFrameworks(string pathToBuildProject)
    {
        string destDir = pathToBuildProject + "/GMSDK";
        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }
        GMSDKUtil.CopyDir(_env.PATH_RTC_LIBRARYS_IOS + "/Library/GMSDK",destDir,true);
    }

    private static void EditorMod(string pathToBuildProject)
    {
        GMSDKUtil.ReplaceTextStringWithRegex(_env.PATH_RTC_EDITOR + "/Resources/GMRTCXcodeConfig.gsdkprojmods", ".*/GMSDK/", "        \"" + pathToBuildProject + "/GMSDK/");
    }
}