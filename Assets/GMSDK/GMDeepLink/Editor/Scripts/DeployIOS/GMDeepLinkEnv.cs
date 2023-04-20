using System.IO;

public class GMDeepLinkEnv
{
    public readonly string PATH_DEEPLINK_EDITOR;
    public readonly string PATH_DEEPLINK_LIBRARYS_IOS;

    private static GMDeepLinkEnv instance;
    
    public static GMDeepLinkEnv Instance
    {
        get { return instance ?? (instance = new GMDeepLinkEnv()); }
    }

    private GMDeepLinkEnv()
    {
        PATH_DEEPLINK_EDITOR = GMSDKEnv.Instance.SubEditorPath(@"GMDeepLink");
        PATH_DEEPLINK_LIBRARYS_IOS = Path.Combine(PATH_DEEPLINK_EDITOR, @"Librarys/iOS");
    }
}