/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;

namespace Zeus.Framework.Http
{
    public abstract class IHttpRequest
    {
        protected UnityWebRequest _request;
#if UNITY_2018 || UNITY_2017
        private int _timeout;
        private DateTime _startTime;
        protected bool _isTimeout;
#endif
        public IHttpRequest(UnityWebRequest request)
        {
            this._request = request;
        }


        public void SendWebRequest()
        {
#if UNITY_2018 || UNITY_2017
            this._timeout = this._request.timeout * 1000;
            this._startTime = System.DateTime.Now;
            this._isTimeout = false;
#endif
            this._request.SendWebRequest();
        }

        public bool IsDone()
        {
#if UNITY_2018 || UNITY_2017
            if (this._timeout > 0 && !this._request.isDone && (System.DateTime.Now - this._startTime).TotalMilliseconds > this._timeout)
            {
                Debug.Log("Http Request Timeout: " + this._request.url);
                this._request.Abort();
                this._isTimeout = true;
                return true;
            }
#endif
            return _request.isDone;
        }

        public abstract void DoResponseHandler();
        public virtual void Dispose()
        {
            this._request.Dispose();
        }
    }



    internal class GetPost_HttpRequest : IHttpRequest
    {
        private Action<bool, string, int, string> _handler;

        public GetPost_HttpRequest(UnityWebRequest request, Action<bool, string, int, string> handler) : base(request)
        {
            this._handler = handler;
        }

        public override void DoResponseHandler()
        {
            string content = null;
            if (_request.isHttpError || _request.isNetworkError)
            {
                if (_handler != null)
                    _handler.Invoke(false, _request.error, (int)_request.responseCode, content);
                Debug.Log("_request.responseCode : " + _request.responseCode +
                            " isHttpError: " + _request.isHttpError +
                            " isNetworkError： " + _request.isNetworkError +
                            " _request.error" + _request.error);
            }
#if UNITY_2018 || UNITY_2017
            else if(_isTimeout)
            {
                if (_handler != null)
                    _handler.Invoke(false, "timeout", 0, content);
                Debug.Log("_request.responseCode : 0 isHttpError: False isNetworkError： True _request.errorRequest timeout");
            }
#endif
            else
            {
                content = DownloadHandlerBuffer.GetContent(_request);
                //Debug.Log("content : " + content);
                if (_handler != null)
                    _handler.Invoke(true, string.Empty, (int)_request.responseCode, content);
            }
        }
    }

    internal class Head_HttpRequest : IHttpRequest
    {
        private Action<bool, string, int, Dictionary<string, string>> _handler;

        public Head_HttpRequest(UnityWebRequest request, Action<bool, string, int, Dictionary<string, string>> handler) : base(request)
        {
            this._handler = handler;
        }

        public override void DoResponseHandler()
        {
            Dictionary<string, string> dic = null;
            if (_request.isHttpError || _request.isNetworkError)
            {
                if (_handler != null)
                    _handler.Invoke(false, _request.error, (int)_request.responseCode, dic);
                Debug.Log("_request.responseCode : " + _request.responseCode +
                            " isHttpError: " + _request.isHttpError +
                            " isNetworkError： " + _request.isNetworkError +
                            " _request.error" + _request.error);
            }
#if UNITY_2018 || UNITY_2017
            else if (_isTimeout)
            {
                if (_handler != null)
                    _handler.Invoke(false, "timeout", 0, dic);
                Debug.Log("_request.responseCode : 0 isHttpError: False isNetworkError： True _request.errorRequest timeout");
            }
#endif
            else
            {
                dic = _request.GetResponseHeaders();
                if (_handler != null)
                    _handler.Invoke(true, string.Empty, (int)_request.responseCode, dic);
            }
        }
    }

    internal class Request_HttpRequest : IHttpRequest
    {
        private Action<UnityWebRequest> _handler;
        public Request_HttpRequest(UnityWebRequest request, Action<UnityWebRequest> handler) : base(request)
        {
            this._handler = handler;
        }

        public override void DoResponseHandler()
        {
            if (_request.isHttpError || _request.isNetworkError)
            {
                Debug.Log("_request.responseCode : " + _request.responseCode +
                            " isHttpError: " + _request.isHttpError +
                            " isNetworkError： " + _request.isNetworkError +
                            " _request.error" + _request.error +
                            " url: " + _request.url);
            }
#if UNITY_2018 || UNITY_2017
            else if (_isTimeout)
            {
                Debug.Log("_request.responseCode : 0 isHttpError: False isNetworkError： True _request.errorRequest timeout");
            }
#endif
            if (_handler != null)
                _handler.Invoke(_request);
        }

        public override void Dispose()
        {
            //为适应调用方缓存 _request 引用并异步处理的情况，需要调用方在使用完 _request 之后再主动释放，故此处为空函数。
        }
    }
}

