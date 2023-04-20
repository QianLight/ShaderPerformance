using System.Runtime.InteropServices;
using GMSDK;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class ETService : IETService
    {
        private readonly MainSDK _mainSDK;
        
        public ETService()
        {
            _mainSDK = GMSDKMgr.instance.SDK;
        }

        public void ReportTrackEvent(string eventName, string jsonParams)
        {
            if (string.IsNullOrEmpty(eventName) || string.IsNullOrEmpty(jsonParams))
            {
                GLog.LogError("eventName or jsonParams is null in ReportTrackEvent.");
            }
            GSDKProfilerTools.BeginSample("ET ReportTrackEvent");
#if UNITY_STANDALONE_WIN && !GMEnderOn
            TrackEvent(eventName, jsonParams);
#else
            var jsonData = JsonMapper.ToObject(jsonParams);
            _mainSDK.SdkTrackEvent(eventName, jsonData);
#endif
            GSDKProfilerTools.EndSample();
        }

        public void ReportLaunchEvent()
        {
            GSDKProfilerTools.BeginSample("ET ReportLaunchEvent");
            _mainSDK.SDKLaunchEvent();
            GSDKProfilerTools.EndSample();
        }
        
        
#if UNITY_STANDALONE_WIN && !GMEnderOn
        [DllImport(PluginName.GSDK)]
        private static extern void TrackEvent(string eventName, string jsonParams);
        
#endif      
        
    }
}