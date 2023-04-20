using System.IO;

public class GMUpgradeEnv
{
    public readonly string PATH_UPGRADE_EDITOR;
    public readonly string PATH_UPGRADE_LIBRARYS_IOS;

    private static GMUpgradeEnv instance;
    
    public static GMUpgradeEnv Instance
    {
        get { return instance ?? (instance = new GMUpgradeEnv()); }
    }

    private GMUpgradeEnv()
    {
        PATH_UPGRADE_EDITOR = GMSDKEnv.Instance.SubEditorPath(@"GMUpgrade");
        PATH_UPGRADE_LIBRARYS_IOS = Path.Combine(PATH_UPGRADE_EDITOR, @"Librarys/iOS");
    }
}