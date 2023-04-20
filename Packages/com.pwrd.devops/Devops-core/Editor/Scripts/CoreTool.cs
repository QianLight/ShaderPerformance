using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Devops.Core
{
    public static class CoreTool
    {
        public class WebReqSkipCert : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                Debug.Log("devops skip certificate");
                return true;
            }
        }
        public static async Task<string> GetUrlData(string url)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.certificateHandler = new WebReqSkipCert();
            await request.SendWebRequest();
            if (request.error != null)
            {
                //Debug.LogError("GetUrlData:" + request.error);
                return string.Empty;
            }
            else
            {
                return request.downloadHandler.text;
            }
        }
    }
}