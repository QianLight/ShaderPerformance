using GMSDK;

public class GMReactNativeMgr : GMSDK.ServiceSingleton<GMReactNativeMgr>
{
    private BaseReactNativeSDK m_sdk;
    public BaseReactNativeSDK SDK { get { return this.m_sdk; } }

    private GMReactNativeMgr()
    {
        if (m_sdk == null)
        {
            m_sdk = new BaseReactNativeSDK();
            // 拉取云控
            GMReactUnityMgr.instance.GetKVConfig();
        }
    }
}
