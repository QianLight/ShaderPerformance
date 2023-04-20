using System.Runtime.InteropServices;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public partial class LocationService : ILocationService
    {
        public void FetchLocationInfoByIp(LocationFetchLocationInfoByIpDelegate fetchLocationInfoByIpCallback)
        {
#if UNITY_STANDALONE_WIN && !GMEnderOn
            FetchLocationInfoByIpCallback = fetchLocationInfoByIpCallback;
            FetchLocationInfoByIp(FetchLocationInfoByIpFunc);
#else
            GMSDKMgr.instance.SDK.SdkGetLocationByIp(locationCallback =>
            {
                var locationInfo = LocationUtils.Convert(locationCallback.Location);
                var result = InnerTools.ConvertToResult(locationCallback);

                InnerTools.SafeInvoke((() => { fetchLocationInfoByIpCallback.Invoke(result, locationInfo); }));
            });
#endif
        }

        public LocationInfo GetLocalLocationInfo()
        {
            var gmLocation = GMSDKMgr.instance.SDK.SdkGetLocation();
            return LocationUtils.Convert(gmLocation);
        }


        public LocationFetchLocationInfoByIpDelegate FetchLocationInfoByIpCallback;

        [MonoPInvokeCallback(typeof(LocationFetchLocationInfoByIpDelegate))]
        private static void FetchLocationInfoByIpFunc(string resultStr, string callbackStr)
        {
            GLog.LogDebug("callbackstr:" + resultStr);
            Result result = JsonMapper.ToObject<Result>(resultStr);
            LocationInfo locationInfo = JsonMapper.ToObject<LocationInfo>(callbackStr);
            LocationService service = Location.Service as LocationService;

            if (service.FetchLocationInfoByIpCallback != null)
            {
                service.FetchLocationInfoByIpCallback(result, locationInfo);
            }
        }
        [DllImport(PluginName.GSDK)]
        private static extern void FetchLocationInfoByIp(Callback2P callback);

    }
}