namespace GMSDK.GMLocation
{
	public class GMLocationMgr : ServiceSingleton<GMLocationMgr>
	{
		private BaseLocationSDK _locationSDK;
		public BaseLocationSDK SDK
		{
			get
			{
				return _locationSDK;
			}
		}
		
		private GMLocationMgr()
		{
			if (_locationSDK == null)
			{
				_locationSDK = new BaseLocationSDK();
			}
		}
	}
}