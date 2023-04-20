using GMSDK;

public class GPMSDKMgr
{
    public BaseGPMSDK SDK
	{
		get
		{
			return _sdk;
		}
	}

	protected static GPMSDKMgr _instance = null;
	protected static BaseGPMSDK _sdk = null;

	private static object _lock = new object();

	public static GPMSDKMgr instance
	{
		get
		{
			lock (_lock)
			{
				if (_instance == null)
				{
					_instance = new GPMSDKMgr();
					_sdk = new BaseGPMSDK();
				}

				return _instance;
			}
		}
	}

}
