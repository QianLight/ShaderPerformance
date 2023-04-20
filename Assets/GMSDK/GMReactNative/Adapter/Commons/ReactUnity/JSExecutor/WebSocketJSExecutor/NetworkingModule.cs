using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;

namespace GSDK.RNU
{
    public class NetworkingModule : SimpleUnityModule
    {
        public static string NAME = "Networking";
        
        //TODO 不应该直接使用RNUMainCore.CallJSFunction，而是应该通过context调用
        private RNUMainCore context;


        public NetworkingModule(RNUMainCore rnuContext)
        {
            //no-op
        }
        public override string GetName()
        {
            return NAME;
        }

        [ReactMethod]
        public void abortRequest(int requestId)
        {
            //no-op
        }

        [ReactMethod]
        public void addListener(string eventName)
        {
            //no-op
        }

        [ReactMethod(true)]
        public void clearCookies(Promise promise)
        {
            //no-op
        }

        [ReactMethod]
        public void removeListeners(double count)
        {
            //no-op
        }

        [ReactMethod]
        public void sendRequest(string method,
            string url,
            int requestId,
            ArrayList headers,
            Dictionary<string, object> data,
            string responseType,
            bool useIncrementalUpdates,
            int timeoutAsDouble,
            bool withCredentials)
        {
            if (method.ToUpper() == "POST")
            {
                InnerRequestPost(url, requestId, headers, data, responseType, useIncrementalUpdates, timeoutAsDouble, withCredentials);
                return;
            }

            if (method.ToUpper() == "GET")
            {
                InnerRequestGet(url, requestId, headers, data, responseType, useIncrementalUpdates, timeoutAsDouble, withCredentials);
                return;
            }
        }

        private static void InnerRequestGet(string url,
            int requestId,
            ArrayList headers,
            Dictionary<string, object> data,
            string responseType,
            bool useIncrementalUpdates,
            int timeoutAsDouble,
            bool withCredentials)
        {
            var req = UnityWebRequest.Get(url);
            StaticCommonScript.StaticStartCoroutine(ReqSend(url, requestId, headers, req, 1));
        }

        private static IEnumerator ReqSend(string url, int requestId, ArrayList headers, UnityWebRequest req, int contentLen)
        {
            yield return null;
            SetReqHeaders(req, headers);
            var reqSend = req.SendWebRequest();
            EmitDataSend(requestId, contentLen, contentLen);
            yield return reqSend;
            EmitResponseReceived(requestId, (int) req.responseCode, req.GetResponseHeaders(), url);
            
            if (req.isHttpError || req.isNetworkError)
            {
                EmitRequestError(requestId, req.error);
            }
            else
            {
                var resultData = req.downloadHandler.data;
                var result = Encoding.UTF8.GetString(resultData);
                EmitDataReceived(requestId, result);
                EmitRequestSuccess(requestId);
            }
        }

        private static void InnerRequestPost(string url,
            int requestId,
            ArrayList headers,
            Dictionary<string, object> data,
            string responseType,
            bool useIncrementalUpdates,
            int timeoutAsDouble,
            bool withCredentials)
        {
            var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST);
            
            var byteData = new byte[]{};
            if (data.ContainsKey("string"))
            {
                byteData = Encoding.UTF8.GetBytes((string)data["string"]);
            }
            req.uploadHandler = (UploadHandler) new UploadHandlerRaw(byteData);
            req.downloadHandler = (DownloadHandler) new DownloadHandlerBuffer();
            
            StaticCommonScript.StaticStartCoroutine(ReqSend(url, requestId, headers, req, byteData.Length));
        }

        private static void SetReqHeaders(UnityWebRequest req, ArrayList headers)
        {
            foreach (ArrayList headerCp in headers)
            {
                if (headerCp.Count != 0) continue;
                req.SetRequestHeader((string)headerCp[0], (string)headerCp[1]);
            }
            
        }

        private static void EmitDataReceived(int requestId, string res)
        {
            var args = new ArrayList()
            {
                requestId,
                res
            };
            
            RNUMainCore.CallJSFunction("RCTDeviceEventEmitter", "emit", new ArrayList()
            {
                "didReceiveNetworkData",
                args,
            });
        }
        
        private static void EmitRequestSuccess(int requestId)
        {
            var args = new ArrayList()
            {
                requestId,
            };
            
            RNUMainCore.CallJSFunction("RCTDeviceEventEmitter", "emit", new ArrayList()
            {
                "didCompleteNetworkResponse",
                args,
            });
            
        }

        private static void EmitResponseReceived(int requestId, int statusCode, Dictionary<string, string> headers, string url)
        {
            var args = new ArrayList()
            {
                requestId,
                statusCode,
                new Hashtable(headers),
                url,
            };
            
            RNUMainCore.CallJSFunction("RCTDeviceEventEmitter", "emit", new ArrayList()
            {
                "didReceiveNetworkResponse",
                args,
            });
            
        }

        private static void EmitDataSend(int requestId, int progress, int total)
        {
            var args = new ArrayList()
            {
                requestId,
                progress,
                total
            };
            
            RNUMainCore.CallJSFunction("RCTDeviceEventEmitter", "emit", new ArrayList()
            {
                "didSendNetworkData",
                args,
            });
        }

        private static void EmitRequestError(int requestId, string error)
        {
            var args = new ArrayList()
            {
                requestId,
                error
            };
            
            RNUMainCore.CallJSFunction("RCTDeviceEventEmitter", "emit", new ArrayList()
            {
                "didCompleteNetworkResponse",
                args,
            });
        }
    }
}