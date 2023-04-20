using System.Collections.Generic;
using System.Linq;
using GMSDK;
using NotImplementedException = System.NotImplementedException;

namespace GSDK
{
    public class GPMMonitorService : IGPMMonitorService
    {
        #region Variables

        private readonly BaseGPMSDK _sdk;

        #endregion
        
        #region Methods

        public GPMMonitorService()
        {
            _sdk = GPMSDKMgr.instance.SDK;
            GLog.LogInfo("GPMMonitorService Initialize");
        }

        public void OnSceneStart(string sceneName)
        {
            _sdk.LogSceneStart(sceneName);
            GLog.LogInfo(string.Format("OnSceneStart,sceneName:{0}", sceneName));
        }

        public void OnSceneLoadFinish()
        {
            _sdk.LogSceneLoaded();
            GLog.LogInfo("OnSceneLoadFinish");
        }

        public void OnSceneEnd(bool isUpload = true)
        {
            _sdk.LogSceneEnd(isUpload);
            GLog.LogInfo("OnSceneEnd");
        }

        public void AddSceneInfo(string key, string value)
        {
            _sdk.LogSceneInfo(key, value);
            GLog.LogInfo(string.Format("AddSceneInfo:{{key:{0}, value:{1}}}", key, value));
        }

        public void AddSceneInfo(string key, int value)
        {
            _sdk.LogSceneInfo(key, value);
            GLog.LogInfo(string.Format("AddSceneInfo:{{key:{0}, value:{1}}}", key, value));
        }

        public void AddGlobalInfo(string key, string value)
        {
            _sdk.LogGlobalInfo(key, value);
            GLog.LogInfo(string.Format("AddGlobalInfo:{{key:{0}, value:{1}}}", key, value));
        }

        public void AddGlobalInfo(string key, int value)
        {
            _sdk.LogGlobalInfo(key, value);
            GLog.LogInfo(string.Format("AddGlobalInfo:{{key:{0}, value:{1}}}", key, value));
        }

        public int GetGraphicLevel()
        {
            var graphicLevel = _sdk.GraphicLevel();
            GLog.LogInfo(string.Format("GetGraphicLevel:{0}", graphicLevel));
            return graphicLevel;
        }

        #endregion
    }
}