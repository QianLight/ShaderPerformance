using System;
using System.IO;
using System.Net;
using System.Text;
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Networking;

public class EnderHttpUtils
{
    private const string DOMAIN = "https://gdev.bytedance.com/gsdk-hub/ender/invoke/";
    private static readonly string WORKSPACE = Directory.GetCurrentDirectory();

    private static readonly string CONFIG_FILE_Path =
        Path.GetFullPath(Path.Combine(WORKSPACE, @"Assets/Plugins/Android/assets/config.json"));

    public static void UploadAndroidConfigFile(Action<string> responseHandler)
    {
        string URL = DOMAIN + "uploadAndroidConfig";
        if (!File.Exists(CONFIG_FILE_Path))
        {
            Debug.Log("[Ender] config.json not exist");
            responseHandler("");
            return;
        }

        string content = File.ReadAllText(CONFIG_FILE_Path);
        byte[] jsonToSend = new UTF8Encoding().GetBytes(content);
        ServicePointManager.ServerCertificateValidationCallback += (p1, p2, p3, p4) => true;
        var webRequest = new UnityWebRequest(URL, "POST")
        {
            uploadHandler = new UploadHandlerRaw(jsonToSend),
            downloadHandler = new DownloadHandlerBuffer(),
            timeout = 9000
        };
        webRequest.SetRequestHeader("Content-Type", "application/json");
        var asyncRequest = webRequest.SendWebRequest();
        asyncRequest.completed += obj =>
        {
            if (webRequest.isHttpError || webRequest.isNetworkError)
            {
                Debug.Log("[Ender] request fail:" + webRequest.error);
                responseHandler("");
            }
            else
            {
                Debug.Log("[Ender] request result:" + webRequest.downloadHandler.text);
                responseHandler(webRequest.downloadHandler.text);
            }
        };
    }
}
#endif