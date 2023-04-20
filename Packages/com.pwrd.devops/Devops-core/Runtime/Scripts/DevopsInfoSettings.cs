using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Devops.Core
{
    public class WebReqSkipCert : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            Debug.Log("devops skip certificate");
            return true;
        }
    }
    public class DevopsInfoSettings
    {
        static DevopsInfoSettings _Instance;
        bool hasInit = false;
        DevopsCoreInfo devopsCoreInfo;
        PerformanceInfo performanceInfo;
        string diskUploadURL = null;

        public static DevopsInfoSettings Instance()
        {
            if (_Instance == null)
            {
                _Instance = new DevopsInfoSettings();
            }
            return _Instance;
        }

        private void Awake()
        {
            _Instance = this;
        }

        void GetInfo()
        {
            if (hasInit)
                return;
            DevopsCoreConfig config = Resources.Load<DevopsCoreConfig>("Configs/DevopsCoreConfig");
            if (config == null)
                return;
            devopsCoreInfo = new DevopsCoreInfo();
            devopsCoreInfo.devopsIpPort = config.DevopsIpPort;
            devopsCoreInfo.versionId = config.VersionId;
            devopsCoreInfo.buildTimestamp = config.BuildTimestamp;
            hasInit = true;
        }

        public void InternalSetDevopsInfo_VersionId(string versionId)
        {
#if Devops_debug
            GetInfo();
            devopsCoreInfo.versionId = versionId;
            _ReGetCoreInfoDatas();
            _ReGetDiskURL();
#endif
        }

        public void InternalSetDevopsInfo_IpPort(string ipAndPort)
        {
#if Devops_debug
            GetInfo();
            devopsCoreInfo.devopsIpPort = ipAndPort;
            _ReGetCoreInfoDatas();
            _ReGetDiskURL();
#endif
        }

        void _ReGetCoreInfoDatas()
        {
            performanceInfo = null;
            GetPerformanceInfo();
        }

        void _ReGetDiskURL()
        {
            diskUploadURL = null;
            GetDiskUploadURL();
        }
        public DevopsCoreInfo GetDevopsInfo()
        {
            if(devopsCoreInfo == null)
                GetInfo();
            return devopsCoreInfo;
        }

        public void ClearDevopsInfo()
        {
            devopsCoreInfo = null;
        }

        [Serializable]
        class DevopsProgramParamJsonDataType
        {
            public int id;
            public int projectId;
            public int varOrder;
            public string varName;
            public string varDesc;
            public int varType;
            public string varValue;
            public string varDefault;
            public string varOpts;
            public int createTime;
        }
        [Serializable]
        class DevopsProjectData
        {
            public int id;
            public List<DevopsProgramParamJsonDataType> projectVarModels;
        }

        [Serializable]
        class DevopsProgramParamJsonType
        {
            public int code;
            public string message;
            public DevopsProjectData data;

            public DevopsProgramParamJsonDataType GetSinglePerformanceJsonData(string name)
            {
                for (int i = 0; i < data.projectVarModels.Count; i++)
                {
                    if (data.projectVarModels[i].varName.Equals(name))
                        return data.projectVarModels[i];
                }
                return null;
            }

            public string GetVarValue(string name)
            {
                for (int i = 0; i < data.projectVarModels.Count; i++)
                {
                    if (data.projectVarModels[i].varName.Equals(name))
                        return data.projectVarModels[i].varValue;
                }
                return "";
            }
        }

        public async Task<PerformanceInfo> GetPerformanceInfo()
        {
            if (performanceInfo != null)
            {
                return performanceInfo;
            }
            GetInfo();
            if (devopsCoreInfo == null)
                return null;
            string urlStr = string.Format($"{devopsCoreInfo.devopsIpPort}/devops/projectVar/listWithVersion/{devopsCoreInfo.versionId}");
            Debug.Log("DevopsCoreInfo url:" + urlStr);
            string strResult = await GetData(urlStr);
            if (string.IsNullOrEmpty(strResult))
                return null;
            Debug.Log("DevopsCoreInfo json:" + strResult);
            performanceInfo = new PerformanceInfo();
            performanceInfo.WebIpPort = devopsCoreInfo.devopsIpPort;
            performanceInfo.VersionId = devopsCoreInfo.versionId;
            performanceInfo.BuildTimestamp = devopsCoreInfo.buildTimestamp;
            DevopsProgramParamJsonType pJsonType = JsonUtility.FromJson<DevopsProgramParamJsonType>(strResult);
            performanceInfo.ServerIp = pJsonType.GetVarValue("UNITY_PERFORMANCE_PROCESS_SERVER_IP");
            performanceInfo.Id = pJsonType.data.id;

            return performanceInfo;
        }

        public async Task<string> GetReportId()
        {
            await GetPerformanceInfo();
            if (performanceInfo == null)
                return null;
            string urlStr = $"{performanceInfo.WebIpPort}/devops/dataRequest/getPerformanceReportId";
            string strResult = await GetData(urlStr);
            Debug.Log("GetReportId:" + strResult);
            return strResult;
        }

        public async Task<string> GetDiskUploadURL()
        {
            if (diskUploadURL != null)
                return diskUploadURL;
            GetInfo();
            if (devopsCoreInfo == null)
                return null;
            string urlStr = string.Format($"{devopsCoreInfo.devopsIpPort}/devops/projectVar/listWithVersion/{devopsCoreInfo.versionId}");
            string strResult = await GetData(urlStr);
            if (string.IsNullOrEmpty(strResult))
                return null;
            Debug.Log("DevopsCoreInfo json:" + strResult);
            DevopsProgramParamJsonType pJsonType = JsonUtility.FromJson<DevopsProgramParamJsonType>(strResult);
            diskUploadURL = pJsonType.GetVarValue("TRIDENT_DISK_UPLOAD_URL");
            return diskUploadURL;
        }

        private async Task<string> GetData(string url)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.certificateHandler = new WebReqSkipCert();
            await request.SendWebRequest();
            if (request.error != null)
            {
                Debug.LogError("GetUrlData:" + request.error);
                return "";
            }
            else
            {
                return request.downloadHandler.text;
            }
        }

        private async Task<string> PostUrl(string url, string postData)
        {
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(new HttpMethod("Post"), url))
                    {
                        request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                        if (postData != null)
                        {
                            request.Content = new StringContent(postData);
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                        }
                        var response = await httpClient.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            var resultStr = await response.Content.ReadAsStringAsync();
                            return resultStr;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("PostUrl error:" + ex.ToString());
            }
            return "";
        }
    }

    public static class ExtensionMethods
    {
        public static TaskAwaiter GetAwaiter(this AsyncOperation asyncOp)
        {
            var tcs = new TaskCompletionSource<object>();
            asyncOp.completed += obj => { tcs.SetResult(null); };
            return ((Task)tcs.Task).GetAwaiter();
        }
    }
}