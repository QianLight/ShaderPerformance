using System;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkUtils
{
    public static void Post(string url, string jsonData, Action<string> responseHandler)
    {
        _Post(url, jsonData, responseHandler);
    }

    private static void _Post(string url, string jsonData, Action<string> responseHandler)
    {
        ServicePointManager.ServerCertificateValidationCallback += (p1, p2, p3, p4) => true;
        var webRequest = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new UTF8Encoding().GetBytes(jsonData);
        webRequest.uploadHandler = new UploadHandlerRaw(jsonToSend);
        webRequest.downloadHandler = new DownloadHandlerBuffer();
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.timeout = 90000;
        var asyncRequest = webRequest.SendWebRequest();
        asyncRequest.completed += obj =>
        {
            if (webRequest.isHttpError || webRequest.isNetworkError)
            {
                Debug.Log("request fail:" + webRequest.error);
                responseHandler("");
            }
            else
            {
                Debug.Log("request result:" + webRequest.downloadHandler.text);
                responseHandler(webRequest.downloadHandler.text);
            }
        };
    }
}