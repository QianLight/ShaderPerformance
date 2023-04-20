using System.IO;

public class GMPushEnv
{
    public readonly string PATH_PUSH_EDITOR;
    public readonly string PATH_PUSH_LIBRARYS_IOS;

    private static GMPushEnv instance;
    
    public static GMPushEnv Instance
    {
        get { return instance ?? (instance = new GMPushEnv()); }
    }

    private GMPushEnv()
    {
        PATH_PUSH_EDITOR = GMSDKEnv.Instance.SubEditorPath(@"GMPush");
        PATH_PUSH_LIBRARYS_IOS = Path.Combine(PATH_PUSH_EDITOR, @"Librarys/iOS");
    }
}