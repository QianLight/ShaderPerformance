using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace Devops.Core
{
    public class EditorDevopsInfoSettings : Editor
    {
        static DevopsCoreInfo devopsCoreInfo;
        static LoginInfo devopsLoginInfo;
        static float getLoginTime = 0.0f;
        public static void SetDataDirty()
        {
            devopsCoreInfo = null;
        }
        public static DevopsCoreInfo GetDevopsInfo()
        {
            if (devopsCoreInfo == null)
            {
                DevopsCoreConfig config = AssetDatabase.LoadAssetAtPath<DevopsCoreConfig>(DevopsCoreDefine.DevopsCoreConfigPath);
                if (config == null)
                    return null;
                devopsCoreInfo = new DevopsCoreInfo();
                devopsCoreInfo.devopsIpPort = config.DevopsIpPort;
                devopsCoreInfo.versionId = config.VersionId;
                devopsCoreInfo.buildTimestamp = config.BuildTimestamp;
            }
            return devopsCoreInfo;
        }

        public static async Task<LoginInfo> GetLoginInfo()
        {
            DevopsCoreInfo devopsInfo = GetDevopsInfo();
            if (devopsInfo == null)
                return null;
            if(Time.realtimeSinceStartup - getLoginTime > 30* 60)
            {
                devopsLoginInfo = null;
            }
            if (devopsLoginInfo != null)
                return devopsLoginInfo;
            string url = $"{devopsInfo.devopsIpPort}/center/getClientLoginKey";
            string strResult = await GetData(url);
            if(strResult == null)
            {
                Debug.LogError($"Can not get data from login url:{url}");
                return null;
            }
            LoginKeyJsonDataType loginKeyData = JsonUtility.FromJson<LoginKeyJsonDataType>(strResult);
            devopsLoginInfo = new LoginInfo();
            devopsLoginInfo.loginUrl = $"{loginKeyData.loginUrl}?clientLoginKey={loginKeyData.key}";
            devopsLoginInfo.getTokenUrl = $"{devopsInfo.devopsIpPort}/center/getClientLoginToken?clientLoginKey={loginKeyData.key}";
            devopsLoginInfo.key = loginKeyData.key;
            getLoginTime = Time.realtimeSinceStartup;
            return devopsLoginInfo;
        }

        private static async Task<string> GetData(string url)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            await request.SendWebRequest();
            if (request.error != null)
            {
                Debug.LogError("GetUrlData:" + request.error);
                return null;
            }
            else
            {
                return request.downloadHandler.text;
            }
        }
    }
}