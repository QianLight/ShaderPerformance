using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
#if UNITY_EDITOR
using Ender.LitJson;
using System.Security.Cryptography.X509Certificates;

namespace Ender
{
    public class EnderHttpRequestUtils
    {
        private const string DOMAIN = "https://gdev.bytedance.com/gsdk-hub/ender/invoke/";
        private const int MAX_RETRY_COUNT = 10;
        private static long deployKey;

        public static void GetRtcToken(string roomId, string uid, Action<String> onResponse)
        {
            if (String.IsNullOrEmpty(roomId))
            {
                Debug.Log("channel id is empty");
            }

            if (String.IsNullOrEmpty(uid))
            {
                Debug.Log("uid is empty");
            }

            string URL = "https://gsdk.dailygn.com/gs/media/realtime_token";
            string param = String.Format("sdk_open_id={0}&room_id={1}", uid, roomId);
            Debug.Log("get remote token param: " + param);
            HttpPost(URL, param, true, (result) =>
            {
                if (String.IsNullOrEmpty(result))
                {
                    return;
                }

                JsonData json = JsonMapper.ToObject(result);
                if (json != null && json.Keys.Contains("data") && json["data"] != null)
                {
                    JsonData data = json["data"];
                    string token = data["realtime_token"].ToString();
                    onResponse(token);
                }
                else
                {
                    if (json != null)
                    {
                        Debug.LogError("GetRemoteToken, code is: " + json["code"]);
                    }

                    Debug.LogError("GetRemoteToken failed");
                }
            });
        }

        private static int GetRequestPlatform(EnderRemoteConstants.EnderPlatform platform)
        {
            return platform == EnderRemoteConstants.EnderPlatform.Android
                ? EnderRemoteConstants.PlatformRequestConstants.PlatformAndroid
                : EnderRemoteConstants.PlatformRequestConstants.PlatformIOS;
        }

        public static void QueryEnderRemoteToken(Action<string> onResponse)
        {
            string URL = DOMAIN + "queryEnderRemoteToken";
            HttpPost(URL, "", false, (result) =>
            {
                if (String.IsNullOrEmpty(result))
                {
                    Debug.LogError("QueryEnderRemoteToken result is empty");
                    return;
                }

                JsonData json = JsonMapper.ToObject(result);
                int code = int.Parse(json["code"].ToString());
                if (code == 0)
                {
                    onResponse(json["data"].ToString());
                }
                else
                {
                    Debug.LogError("QueryEnderRemoteToken, message is " + json["data"]);
                }
            });
        }

        public static void OccupyEnderRemoteDevice(EnderRemoteConstants.EnderPlatform platform,
            Action<string> onResponse)
        {
            string URL = DOMAIN + "occupyEnderRemoteDeivces";
            Dictionary<string, object> dc = new Dictionary<string, object>();
            deployKey = DateTime.Now.Ticks;
            dc["platform"] = GetRequestPlatform(platform);
            dc["deployKey"] = deployKey;
            HttpPost(URL, JsonMapper.ToJson(dc), false, (result) =>
            {
                if (String.IsNullOrEmpty(result))
                {
                    Debug.LogError("OccupyEnderRemoteDevice result is empty");
                    return;
                }

                JsonData json = JsonMapper.ToObject(result);
                int code = int.Parse(json["code"].ToString());
                if (code == 0)
                {
                    onResponse(JsonMapper.ToJson(json["data"]));
                }
                else
                {
                    onResponse("");
                    Debug.LogError("OccupyEnderRemoteDevice, message is " + json["data"]);
                }
            });
        }

        public static void QueryWhiteDevice(string appId, string packageUrl, Action<string> onResponse)
        {
            string URL = DOMAIN + "queryRemoteWhiteDevice";
            Dictionary<string, object> dc = new Dictionary<string, object>();
            deployKey = DateTime.Now.Ticks;
            dc["appId"] = appId;
            dc["packageUrl"] = packageUrl;
            dc["deployKey"] = deployKey;
            HttpPost(URL, JsonMapper.ToJson(dc), false, (result) =>
            {
                if (String.IsNullOrEmpty(result))
                {
                    Debug.LogError("QueryWhiteDevice result is empty");
                    return;
                }

                JsonData json = JsonMapper.ToObject(result);
                int code = int.Parse(json["code"].ToString());
                if (code == 0)
                {
                    onResponse(JsonMapper.ToJson(json["data"]));
                }
                else
                {
                    onResponse("");
                    Debug.Log("QueryWhiteDevice: " + json["data"]);
                }
            });
        }

        public static void AddDeviceWhiteList(string appId, string serial, Action<bool> onResponse)
        {
            string URL = DOMAIN + "addWhiteDeviceList";
            Dictionary<string, string> dc = new Dictionary<string, string>();
            dc["appId"] = appId;
            dc["serial"] = serial;
            HttpPost(URL, JsonMapper.ToJson(dc), false, (result) =>
            {
                if (String.IsNullOrEmpty(result))
                {
                    Debug.LogError("AddDeviceWhiteList result is empty");
                    return;
                }

                JsonData json = JsonMapper.ToObject(result);
                int code = int.Parse(json["code"].ToString());
                if (code == 0)
                {
                    onResponse(true);
                }
                else
                {
                    onResponse(false);
                    Debug.Log("AddDeviceWhiteList, message is: " + json["data"]);
                }
            });
        }

        public static void QueryRemotePackageIsInstalled(string appId, string serial, string packageUrl,
            Action<bool> onResponse)
        {
            string URL = DOMAIN + "queryRemotePackageIsInstalled";
            Dictionary<string, string> dc = new Dictionary<string, string>();
            dc["appId"] = appId;
            dc["serial"] = serial;
            dc["packageUrl"] = packageUrl;
            HttpPost(URL, JsonMapper.ToJson(dc), false, (result) =>
            {
                if (String.IsNullOrEmpty(result))
                {
                    Debug.LogError("QueryRemotePackageIsInstalled result is empty");
                    return;
                }

                JsonData json = JsonMapper.ToObject(result);
                int code = int.Parse(json["code"].ToString());
                if (code == 0)
                {
                    onResponse(true);
                }
                else
                {
                    onResponse(false);
                    Debug.Log("QueryRemotePackageIsInstalled, message is " + json["data"]);
                }
            });
        }

        public static void ReleaseEnderRemoteDevice(string serial)
        {
            string URL = DOMAIN + "releaseEnderRemoteDevices";
            Dictionary<string, string> dc = new Dictionary<string, string>();
            dc["serial"] = serial;
            HttpPost(URL, JsonMapper.ToJson(dc), false,
                (result) => { Debug.Log("ReleaseEnderRemoteDevice, " + result); });
        }

        //platform  1: Android  2:iOS， 不传默认Android
        public static void QueryEnderRemoteInstallPackage(string appId, string sdkVersion,
            EnderRemoteConstants.EnderPlatform platform, Action<string> onResponse)
        {
            string URL = DOMAIN + "queryEnderRemoteInstallPackage";
            Dictionary<string, object> dc = new Dictionary<string, object>();
            dc["appId"] = appId;
            dc["sdkVersion"] = sdkVersion;
            dc["platform"] = GetRequestPlatform(platform);
            HttpPost(URL, JsonMapper.ToJson(dc), false, (result) =>
            {
                if (String.IsNullOrEmpty(result))
                {
                    Debug.LogError("QueryEnderRemoteInstallPackage result is empty");
                    return;
                }

                JsonData json = JsonMapper.ToObject(result);
                int code = int.Parse(json["code"].ToString());
                if (code == 0)
                {
                    onResponse(JsonMapper.ToJson(json["data"]));
                }
                else
                {
                    onResponse("");
                    Debug.LogError("QueryEnderRemoteInstallPackage, error message is " + json["data"]);
                }
            });
        }

        public static void InstallPackage(string packageUrl, string serial, Action<bool> onResponse)
        {
            string URL = DOMAIN + "installEnderRemotePackage";
            Dictionary<string, string> dc = new Dictionary<string, string>();
            dc["serial"] = serial;
            dc["packageUrl"] = packageUrl;
            HttpPost(URL, JsonMapper.ToJson(dc), false, (result) =>
            {
                if (String.IsNullOrEmpty(result))
                {
                    Debug.LogError("InstallPackage result is empty");
                    return;
                }

                JsonData json = JsonMapper.ToObject(result);
                int code = int.Parse(json["code"].ToString());
                if (code == 0)
                {
                    onResponse(true);
                }
                else
                {
                    onResponse(false);
                    Debug.LogError("InstallPackage, error message is " + json["data"]);
                }
            });
        }

        public static void LaunchApp(string appid, string serial, Action<bool> onResponse)
        {
            string URL = DOMAIN + "launchEnderRemotePackage";
            Dictionary<string, object> dc = new Dictionary<string, object>();
            dc["serial"] = serial;
            dc["appId"] = appid;
            if (deployKey > 0)
            {
                dc["deployKey"] = deployKey;
            }
            HttpPost(URL, JsonMapper.ToJson(dc), false, (result) =>
            {
                if (String.IsNullOrEmpty(result))
                {
                    Debug.LogError("LaunchApp result is empty");
                    return;
                }

                JsonData json = JsonMapper.ToObject(result);
                int code = int.Parse(json["code"].ToString());
                if (code == 0)
                {
                    onResponse(true);
                }
                else
                {
                    onResponse(false);
                    Debug.LogError("LaunchApp, error message is " + json["data"]);
                }
            });
        }

        public static void AddDeviceTime(string serial, Action<bool> onResponse)
        {
            string URL = DOMAIN + "addEnderRemoteDeviceTime";
            Dictionary<string, string> dc = new Dictionary<string, string>();
            dc["serial"] = serial;
            HttpPost(URL, JsonMapper.ToJson(dc), false, (result) =>
            {
                if (String.IsNullOrEmpty(result))
                {
                    Debug.LogError("AddDeviceTime result is empty");
                    return;
                }

                JsonData json = JsonMapper.ToObject(result);
                int code = int.Parse(json["code"].ToString());
                if (code == 0)
                {
                    onResponse(true);
                }
                else
                {
                    onResponse(false);
                    Debug.Log("AddDeviceTime, error message is " + json["data"]);
                }
            });
        }

        public static void QueryInstallStatus(string packageUrl, string serial, Action<bool> onResponse)
        {
            string URL = DOMAIN + "queryRemotePackageInstallStatus";
            Dictionary<string, string> dc = new Dictionary<string, string>();
            dc["packageUrl"] = packageUrl;
            dc["serial"] = serial;
            HttpPost(URL, JsonMapper.ToJson(dc), false, (result) =>
            {
                if (String.IsNullOrEmpty(result))
                {
                    Debug.LogError("QueryInstallStatus result is empty");
                    return;
                }

                JsonData json = JsonMapper.ToObject(result);
                int code = int.Parse(json["code"].ToString());
                if (code == 0)
                {
                    int installResult = int.Parse(json["data"].ToString());
                    onResponse(installResult == 0);
                }
                else
                {
                    Debug.Log("QueryInstallStatus, message is: " + json["data"]);
                }
            });
        }

        public static void QueryDeviceJumpToken(Action<string> onResponse)
        {
            string URL = DOMAIN + "queryEnderRemoteTempToken";
            HttpPost(URL, "", false, (result) =>
            {
                if (String.IsNullOrEmpty(result))
                {
                    Debug.LogError("QueryDeviceJumpToken result is empty");
                    return;
                }

                JsonData json = JsonMapper.ToObject(result);
                int code = int.Parse(json["code"].ToString());
                if (code == 0)
                {
                    onResponse(json["data"].ToString());
                }
                else
                {
                    onResponse("");
                    Debug.LogError("QueryDeviceJumpToken, error message is " + json["data"]);
                }
            });
        }

        public static void QueryEnderRemoteDeviceIsOccupied(string serial, Action<bool> onResponse)
        {
            string URL = DOMAIN + "queryEnderRemoteDeviceIsOccupied";
            Dictionary<string, string> dc = new Dictionary<string, string>();
            dc["serial"] = serial;
            HttpPost(URL, JsonMapper.ToJson(dc), false, (result) =>
            {
                if (String.IsNullOrEmpty(result))
                {
                    Debug.LogError("QueryEnderRemoteDeviceIsOccupied result is empty");
                    return;
                }

                JsonData json = JsonMapper.ToObject(result);
                int code = int.Parse(json["code"].ToString());
                if (code == 0)
                {
                    onResponse(bool.Parse(json["data"].ToString()));
                }
                else
                {
                    onResponse(false);
                    Debug.Log("QueryEnderRemoteDeviceIsOccupied, error message is " + json["data"]);
                }
            });
        }

        public static void QueryEnderRemoteDeviceInfo(string serial, Action<string> onResponse)
        {
            string URL = DOMAIN + "queryEnderRemoteDeviceInfo";
            Dictionary<string, string> dc = new Dictionary<string, string>();
            dc["serial"] = serial;
            HttpPost(URL, JsonMapper.ToJson(dc), false, (result) =>
            {
                if (String.IsNullOrEmpty(result))
                {
                    Debug.LogError("QueryEnderRemoteDeviceInfo result is empty");
                    return;
                }

                JsonData json = JsonMapper.ToObject(result);
                int code = int.Parse(json["code"].ToString());
                if (code == 0)
                {
                    onResponse(JsonMapper.ToJson(json["data"]));
                }
                else
                {
                    onResponse("");
                    Debug.Log("QueryEnderRemoteDeviceInfo, error message is " + json["data"]);
                }
            });
        }

        public static void AddDeviceBlackList(string serial, Action<bool> onResponse)
        {
            string URL = DOMAIN + "addDeviceBlackList";
            Dictionary<string, string> dc = new Dictionary<string, string>();
            dc["serial"] = serial;
            HttpPost(URL, JsonMapper.ToJson(dc), false, (result) =>
            {
                if (String.IsNullOrEmpty(result))
                {
                    Debug.LogError("AddDeviceBlackList result is empty");
                    return;
                }

                JsonData json = JsonMapper.ToObject(result);
                int code = int.Parse(json["code"].ToString());
                if (code == 0)
                {
                    onResponse(true);
                }
                else
                {
                    onResponse(false);
                    Debug.Log("AddDeviceBlackList, error message is " + json["data"]);
                }
            });
        }

        public static void IsRemoteDeviceInstallFailed(string serial, Action<bool> onResponse)
        {
            string URL = DOMAIN + "isRemoteDeviceInstallFailed";
            Dictionary<string, string> dc = new Dictionary<string, string>();
            dc["serial"] = serial;
            HttpPost(URL, JsonMapper.ToJson(dc), false, (result) =>
            {
                if (String.IsNullOrEmpty(result))
                {
                    Debug.LogError("IsRemoteDeviceInstallFailed result is empty");
                    return;
                }

                JsonData json = JsonMapper.ToObject(result);
                onResponse(bool.Parse(json["data"].ToString()));
            });
        }

        static void HttpPost(string url, string data, bool isForm, Action<string> onResponse, int retryCount = 0)
        {
            if (retryCount > MAX_RETRY_COUNT)
            {
                Debug.LogError("Maximum number of failures reached, url: " + url);
                onResponse("");
                return;
            }

            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    ServicePointManager.ServerCertificateValidationCallback += MyRemoteCertificateValidationCallback;
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3;
                    ServicePointManager.DefaultConnectionLimit = 100;
                    HttpWebRequest httpWebRequest = (HttpWebRequest) WebRequest.Create(url);
                    byte[] bs = Encoding.UTF8.GetBytes(data);
                    httpWebRequest.ContentType = isForm ? "application/x-www-form-urlencoded" : "application/json";
                    httpWebRequest.ContentLength = bs.Length;
                    httpWebRequest.Method = "POST";
                    httpWebRequest.Timeout = 20000;
                    httpWebRequest.GetRequestStream().Write(bs, 0, bs.Length);
                    HttpWebResponse httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse();
                    StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8);
                    string responseContent = streamReader.ReadToEnd();
                    onResponse(responseContent);
                    streamReader.Close();
                    httpWebResponse.Close();
                    httpWebRequest.Abort();
                }
                catch (Exception e)
                {
                    if (!url.EndsWith("queryRemotePackageInstallStatus")) //queryRemotePackageInstallStatus为轮询接口
                    {
                        if (retryCount >= 5)
                        {
                            Dictionary<string, object> dc = new Dictionary<string, object>();
                            dc["code"] = -1;
                            dc["data"] = "request exception, url: " + url;
                            onResponse(JsonMapper.ToJson(dc));
                            return;
                        }

                        retryCount += 1;
                        Debug.LogError("request failed, will retry, retryCount: " + retryCount + ", URL: " + url +
                                       ", Exception: " + e);
                        Thread.Sleep(1000);
                        HttpPost(url, data, isForm, onResponse, retryCount); //retry
                    }
                }
            });
        }

        static public bool MyRemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate,
            X509Chain chain, System.Net.Security.SslPolicyErrors sslPolicyErrors)
        {
            bool isOk = true;
            // If there are errors in the certificate chain,
            // look at each error to determine the cause.
            if (sslPolicyErrors != System.Net.Security.SslPolicyErrors.None)
            {
                for (int i = 0; i < chain.ChainStatus.Length; i++)
                {
                    if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown)
                    {
                        continue;
                    }

                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                    chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                    bool chainIsValid = chain.Build((X509Certificate2) certificate);
                    if (!chainIsValid)
                    {
                        isOk = false;
                        break;
                    }
                }
            }

            return isOk;
        }
    }
}
#endif