using System.IO;

public class DeployIOSPush
{
    private static readonly GMPushEnv _env = GMPushEnv.Instance;
    
    public static void Deploy(string projectPath)
    {
        // 获得项目完整路径
        string path = Path.GetFullPath(projectPath);
        
        // 复制framework
        CopyFrameworks(path);
        
        // 插入 xcode 代码(UnityAppController.mm)
        EditorAppControllerCode(path);
    }

    private static void CopyFrameworks(string projectPath)
    {
        string destDir = projectPath + "/GMSDK";
        if (!Directory.Exists(destDir))
        {
            Directory.CreateDirectory(destDir);
        }
        GMSDKUtil.CopyDir(_env.PATH_PUSH_LIBRARYS_IOS + "/Library/GMSDK",destDir,true);
    }
    
    private static void EditorAppControllerCode(string projectPath)
    {
        string ocFile = projectPath + "/Classes/UnityAppController.mm";
        
        StreamReader streamReader = new StreamReader(ocFile);
        string textAll = streamReader.ReadToEnd();
        streamReader.Close();
        if (string.IsNullOrEmpty(textAll))
        {
            return;
        }
        
        // 添加头文件
        GMSDKUtil.WriteBelow(ocFile, GMPushNativeCode.IOS_SRC_HEADER, GMPushNativeCode.IOS_HEADER);
        
        // 插入代码
        GMSDKUtil.WriteBelow(ocFile, GMPushNativeCode.IOS_SRC_FINISH, GMPushNativeCode.IOS_FINISH);
    }
}