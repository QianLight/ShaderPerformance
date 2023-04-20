using System;
using UnityEngine;

public class BuglyComponent : SDKComponent
{
    public override bool on
    {
        get
        {
#if BUGLY
            return true;
#else
            return false;
#endif

        }
    }

    public override string version { get { return "2.5.0"; } }

    public override int type { get { return SDKEnum.BUGLY; } }

    public override void Init ()
    {
//#if UNITY_EDITOR
//        // TO-DO 
//#elif UNITY_ANDROID
//        BuglyAgent.InitWithAppId ("1f7d78a4b8");
//        BuglyAgent.EnableExceptionHandler ();
//#elif UNITY_IOS
//        BuglyAgent.InitWithAppId ("95e714f5db");
//        BuglyAgent.EnableExceptionHandler ();
//#endif
    }

    protected override void OnLogin ()
    {
        base.OnLogin ();
        //BuglyAgent.SetUserId (this.userid);
    }

    public void ManuReport (Exception e)
    {
//        if (on)
//        {
//#if UNITY_EDITOR
//            //to-do
//            Debug.Log ("bugly report=>" + e.Message);
//#else
//            BuglyAgent.ReportException (e, e.Message);
//#endif
//        }
    }

    public void Config (string chanel, string version, string user, long delay)
    {
//        if (on)
//        {
//#if UNITY_EDITOR
////to-do
//#else
//            var asset = Resources.Load<TextAsset> ("packinfo");
//            if (asset != null)
//            {
//                //asset.text include git branch & build date info
//                BuglyAgent.ConfigDefault (chanel, asset.text, user, delay);
//            }
//            else
//            {
//                BuglyAgent.ConfigDefault (chanel, version, user, delay);
//            }
//#endif

//#if BUGLY_LEVEL
//            BuglyAgent.ConfigAutoReportLogLevel(LogSeverity.LogException);
//#endif
//        }
    }
}