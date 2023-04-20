using GMSDK;
using GSDK;


public class GMReactUnityMgr : GMSDK.ServiceSingleton<GMReactUnityMgr>
{
    private BaseReactUnitySDK m_sdk;
    public BaseReactUnitySDK SDK { get { return this.m_sdk; } }

    private static string GECKO_ONLINE_KEY = "d4f4979ca4fa9774d25c3ec3a49fbded";
    private static string GECKO_SANDBOX_KEY = "783b79a961a58fea81c271851de9f948";
    

    private GMReactUnityMgr()
    {
        if (m_sdk == null)
        {
            m_sdk = new BaseReactUnitySDK();
            GetKVConfig();
        }
    }

    private RUKVConfig _kvconfig;
    public RUKVConfig config
    {
        get
        {
            if (_kvconfig == null)
            {
                this.GetKVConfig();
            }
            return _kvconfig != null ? _kvconfig : new RUKVConfig()
            {
                ruDisable = false
            };
        }
    }

    public string preBundlePath { get; private set; }
    
    public void GetKVConfig()
    {
        this.m_sdk.getKVConfig(result =>
        {
            if (result.IsSuccess())
            {
                this._kvconfig = result.config;

                if (string.IsNullOrEmpty(this.preBundlePath)) // 只设置一遍
                {
                    string path = string.IsNullOrEmpty(result.bundlePrePath) ? "" : result.bundlePrePath;

                    if (App.Service.EnableSandboxMode)
                    {
#if UNITY_ANDROID
                    this.preBundlePath = path + "/" + GECKO_SANDBOX_KEY + "/";
#elif UNITY_IOS
                        this.preBundlePath = path + "/ReactNative/" + GECKO_SANDBOX_KEY + "/";
#else
                    this.preBundlePath = path;
#endif
                    }
                    else
                    {
#if UNITY_ANDROID
                    this.preBundlePath = path + "/" + GECKO_ONLINE_KEY + "/";
#elif UNITY_IOS
                        this.preBundlePath = path + "/ReactNative/" + GECKO_ONLINE_KEY + "/";
#else
                    this.preBundlePath = path;
#endif
                    }
                }
            }
        });
    }

}