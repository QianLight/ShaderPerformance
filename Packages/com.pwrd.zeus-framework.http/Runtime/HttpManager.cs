/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Net;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Networking;
using Zeus.Core;
#if ZEUS_LUA_SUPPORT
using LuaInterface;
#endif

namespace Zeus.Framework.Http
{
    public class HttpManager
    {
        static private HttpManager _instance = null;
        private List<IHttpRequest> _requestList = new List<IHttpRequest>();

        static public HttpManager Instance()
        {
            if (_instance == null)
            {
                _instance = new HttpManager();
            }
            return _instance;
        }

        private HttpManager()
        {
            ZeusCore.Instance.RegisterUpdate(_Update);
            ZeusCore.Instance.RegisterOnApplicationQuit(_OnApplicationQuit);
        }

#if ZEUS_LUA_SUPPORT
        public void Post(string url, LuaTable ltable, Action<int, string> callBack)
        {
            
        }
#endif

        public void Post(string url, WWWForm form, Action<bool, string, int, string> callBack)
        {
            UnityWebRequest webRequest = new UnityWebRequest(url, "POST");
            SetupPost(webRequest, form);
            //webRequest.downloadHandler = new DownloadHandlerBuffer();
            GetPost_HttpRequest req = new GetPost_HttpRequest(webRequest, callBack);
            req.SendWebRequest();
            _requestList.Add(req);
        }

        private static void SetupPost(UnityWebRequest request, WWWForm formData)
        {
            byte[] array = null;
            if (formData != null)
            {
                array = formData.data;
                if (array.Length == 0)
                {
                    array = null;
                }
            }
            request.uploadHandler = new UploadHandlerRaw(array);
            request.downloadHandler = new DownloadHandlerBuffer();
            if (formData != null)
            {
                Dictionary<string, string> headers = formData.headers;
                foreach (KeyValuePair<string, string> current in headers)
                {
                    request.SetRequestHeader(current.Key, current.Value);
                }
            }
        }

        public void Post(string url, Dictionary<string, string> formDict, Action<bool, string, int, string> callBack)
        {
            UnityWebRequest webRequest = UnityWebRequest.Post(url, formDict);
            //webRequest.downloadHandler = new DownloadHandlerBuffer();
            GetPost_HttpRequest req = new GetPost_HttpRequest(webRequest, callBack);
            req.SendWebRequest();
            _requestList.Add(req);
        }

        public void Get(string url, Action<bool, string, int, string> callBack)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            //webRequest.downloadHandler = new DownloadHandlerBuffer();
            GetPost_HttpRequest req = new GetPost_HttpRequest(webRequest, callBack);
            req.SendWebRequest();
            _requestList.Add(req);
        }

        public void Get(string url, int timeout, Action<bool, string, int, string> callBack)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            webRequest.timeout = timeout;
            //webRequest.downloadHandler = new DownloadHandlerBuffer();
            GetPost_HttpRequest req = new GetPost_HttpRequest(webRequest, callBack);
            req.SendWebRequest();
            _requestList.Add(req);
        }

        public void Head(string url, int timeout, Action<bool, string, int, Dictionary<string, string>> callBack)
        {
            UnityWebRequest webRequest = UnityWebRequest.Head(url);
            webRequest.timeout = timeout;
            Head_HttpRequest req = new Head_HttpRequest(webRequest, callBack);
            req.SendWebRequest();
            _requestList.Add(req);
        }

        /// <summary>
        /// 回调函数返回 UnityWebRequest，使用完之后需要主动调用 UnityWebRequest.Dispose()
        /// </summary>
        /// <param name="url"></param>
        /// <param name="timeout"></param>
        /// <param name="callBack"></param>
        public void Get(string url, int timeout, Action<UnityWebRequest> callBack)
        {
            UnityWebRequest webRequest = UnityWebRequest.Get(url);
            webRequest.timeout = timeout;
            Request_HttpRequest req = new Request_HttpRequest(webRequest, callBack);
            req.SendWebRequest();
            _requestList.Add(req);
        }

        private void _Update()
        {
            _CheckRequest();
        }

        private void _OnApplicationQuit()
        {
            _requestList.Clear();
        }

        private void _CheckRequest()
        {
            for (int i = 0; i < _requestList.Count; i++)
            {
                if (_requestList[i] != null && _requestList[i].IsDone())
                {
                    _requestList[i].DoResponseHandler();
                    _requestList[i].Dispose();
                    _requestList[i] = null;
                }
            }

            //remove null request
            for (int i = _requestList.Count - 1; i >= 0; i--)
            {
                if (_requestList[i] == null)
                {
                    _requestList.RemoveAt(i);
                }
            }
        }
    }
}

