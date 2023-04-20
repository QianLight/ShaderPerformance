using System.IO;


public class GMRTCEnv
{
    public readonly string PATH_RTC_EDITOR;
    public readonly string PATH_RTC_LIBRARYS_IOS;

    static GMRTCEnv instance;

    public static GMRTCEnv Instance
    {
        get { return instance ?? (instance = new GMRTCEnv()); }
    }

    private GMRTCEnv()
    {
        PATH_RTC_EDITOR = GMSDKEnv.Instance.SubEditorPath(@"GMRTC");
        PATH_RTC_LIBRARYS_IOS = Path.Combine(PATH_RTC_EDITOR, @"Librarys/iOS");
    }
}