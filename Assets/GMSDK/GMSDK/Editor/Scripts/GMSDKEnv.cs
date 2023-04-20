using UnityEngine;
using System.IO;

public class GMSDKEnv
{
    public readonly string WORKSPACE;
    public readonly string MAIN_GSDK_FOLDER;
    public readonly string PATH_CONFIG_SETTINGS;
    public readonly string PATH_PUGLIN_ANDROID;
    public readonly string PATH_PUGLIN_IOS;
    public readonly string PATH_EDITOR;
    public readonly string PATH_LIBRARYS_IOS;
    public readonly string PATH_RESOURCES;
    public readonly string PATH_TEMP;
    public readonly string PATH_ADAPTER_ANDROID;
    public readonly string PATH_ADAPTER_IOS;
    public readonly string PATH_IOS_PLIST;
    public readonly string PATH_IOS_DEFAULT_INFO_PLIST_STRING;
    public readonly string PATH_IOS_USER_INFO_PLIST_STRING;
    public readonly string PATH_CI;
    public readonly string PATH_ASSETS_GSDK;
    public const int XCodePostProcessPriority = 999;

    static GMSDKEnv instance;

    public static GMSDKEnv Instance
    {
        get
        {
            if (instance == null) {
                instance = new GMSDKEnv();
            }
            return instance;
        }
    }

    bool deploySucceed = true;

    public bool DeploySucceed
    {
        get { return deploySucceed; }
        set { deploySucceed = value; }
    }

    private readonly string unityPackageProduct = @"UnityPackage";
    private readonly string UPMProduct = @"UPM";
    
    private string packageProductType = "";

    public string PackageProductType
    {
        get { return packageProductType; }
    }

    public GMSDKEnv()
    {
        WORKSPACE = Directory.GetCurrentDirectory();

        packageProductType = File.Exists(Path.GetFullPath(Path.Combine(WORKSPACE, @"Packages/com.zxgn.gsdk.unity/package.json"))) ?
            UPMProduct : unityPackageProduct;
        
        MAIN_GSDK_FOLDER = (packageProductType == unityPackageProduct) ?
            Path.GetFullPath(Path.Combine(WORKSPACE, @"Assets/GMSDK")) :
            Path.GetFullPath(Path.Combine(WORKSPACE, @"Packages/com.zxgn.gsdk.unity"));
        
        PATH_ASSETS_GSDK = (packageProductType == unityPackageProduct)
            ? Path.GetFullPath(Path.Combine(WORKSPACE, @"Assets/GSDK"))
            : Path.GetFullPath(Path.Combine(MAIN_GSDK_FOLDER, @"Runtime/GSDK"));

        if (!Directory.Exists(MAIN_GSDK_FOLDER)) {
            Debug.LogError("当前GSDK的默认路径无法找到，请修改GMSDKEnv.cs文件中的MAIN_GSDK_FOLDER参数");
        }
        PATH_CONFIG_SETTINGS = (packageProductType == unityPackageProduct) ?
            Path.GetFullPath(Path.Combine(MAIN_GSDK_FOLDER, @"GMConfigSettings")) :
            Path.GetFullPath(Path.Combine(MAIN_GSDK_FOLDER, @"Runtime/GMConfigSettings"));
        
        PATH_PUGLIN_ANDROID = Path.GetFullPath(Path.Combine(WORKSPACE, @"Assets/Plugins/Android"));
        PATH_PUGLIN_IOS = Path.GetFullPath(Path.Combine(WORKSPACE, @"Assets/Plugins/iOS"));
        PATH_EDITOR = (packageProductType == unityPackageProduct) ?
            Path.GetFullPath(Path.Combine(MAIN_GSDK_FOLDER, @"GMSDK/Editor")) :
            Path.GetFullPath(Path.Combine(MAIN_GSDK_FOLDER, @"Editor/GMSDK"));
        
        PATH_RESOURCES = Path.GetFullPath(Path.Combine(PATH_EDITOR, @"Resources"));
        PATH_TEMP = Path.GetFullPath(Path.Combine(PATH_EDITOR, @"Temp"));
        
        PATH_ADAPTER_ANDROID = (packageProductType == unityPackageProduct) ?
            Path.GetFullPath(Path.Combine(MAIN_GSDK_FOLDER, @"GMSDK/Adapter/Android")) :
            Path.GetFullPath(Path.Combine(MAIN_GSDK_FOLDER, @"Runtime/GMSDK/Adapter/Android"));
        
        PATH_ADAPTER_IOS = (packageProductType == unityPackageProduct) ?
            Path.GetFullPath(Path.Combine(MAIN_GSDK_FOLDER, @"GMSDK/Adapter/iOS")) :
            Path.GetFullPath(Path.Combine(MAIN_GSDK_FOLDER, @"Runtime/GMSDK/Adapter/iOS"));
        
        PATH_IOS_PLIST = Path.GetFullPath(Path.Combine(PATH_EDITOR, @"Resources/GMSDKInfo.plist"));
        PATH_IOS_DEFAULT_INFO_PLIST_STRING =
            Path.GetFullPath(Path.Combine(PATH_EDITOR, @"Scripts/DeployIOS/Localisations/infoPlist"));
        PATH_IOS_USER_INFO_PLIST_STRING = Path.GetFullPath(Path.Combine(PATH_CONFIG_SETTINGS, @"Localisation"));

        PATH_CI = Path.Combine(WORKSPACE, @"CI/");
        PrepareDir(PATH_PUGLIN_ANDROID);
        PrepareDir(PATH_PUGLIN_IOS);

#if UNITY_IOS
        string PATH_LIBRARYS = Path.GetFullPath(Path.Combine(PATH_EDITOR, @"Librarys"));
        if (!Directory.Exists(PATH_LIBRARYS))
        {
            Debug.LogError("Librarys Directory doesn't exist!");
            return;
        }
        DirectoryInfo resourceDirInfo = new DirectoryInfo(PATH_LIBRARYS);
        DirectoryInfo[] iOSDirs = resourceDirInfo.GetDirectories("iOS*");
        // 此目录中应只有一个 iOS 开头的目录
        if (iOSDirs.Length != 1) {
            Debug.LogError("Get iOS Resource Directory Error!");
            return;
        }
        PATH_LIBRARYS_IOS = iOSDirs[0].FullName;
#endif

    }

    // 准备环境
    private void PrepareDir(string dirPath)
    {
        if (!Directory.Exists(dirPath)) {
            Directory.CreateDirectory(dirPath);
        }
    }

    // 打印错误日志，标明布署失败
    public void Error(string msg)
    {
        deploySucceed = false;
        Debug.LogError(msg);
    }

    public string SubEditorPath(string subName) {
        string ret = "";
        if (packageProductType == UPMProduct) {
            ret =  Path.Combine(GMSDKEnv.Instance.MAIN_GSDK_FOLDER, @"Editor/" + subName);
        } else {
            ret = Path.Combine(GMSDKEnv.Instance.MAIN_GSDK_FOLDER, subName + @"/Editor");
        }
        return ret;
    }
}