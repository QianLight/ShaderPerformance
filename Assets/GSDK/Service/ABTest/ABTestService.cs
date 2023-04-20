using GMSDK;

namespace GSDK
{
    public class ABTestService : IABTestService
    {
        private readonly MainSDK _gsdk;

        public ABTestService()
        {
            _gsdk = GMSDKMgr.instance.SDK;
        }

        public bool RegisterExperiment(string key, string defaultValue, string owner, string description, bool isBindUser = false)
        {
            return _gsdk.SdkRegisterExperiment(key, description, owner, defaultValue, isBindUser);
        }

        public string GetExperimentValue(string key, bool withExposure)
        {
            if (string.IsNullOrEmpty(key))
            {
                GLog.LogError("Experiment key is null.");
            }
            return _gsdk.SdkGetExperimentValue(key, withExposure);
        }
    }
}