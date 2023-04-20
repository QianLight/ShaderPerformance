/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Net;
using System;

namespace Zeus.Framework
{

    public enum NetType
    {
        FTP = 1,
        HTTP = 2,
    }

    public class WebManager:Singleton<WebManager>
    {
        private static readonly Encoding encoding = Encoding.UTF8;

        private string _userName = "";
        private string _password = "";

        private List<RequestState> _allRequestState = new List<RequestState>();

        public interface DownloadCallback
        {
            bool OnPrepare(long length, object param);

            void OnProcess(byte[] buffer, object param);

            void OnComplate(object param);

            void OnError(object param, Exception e);
        }

        public class RequestState
        {
            // This class stores the State of the request.
            public const int BUFFER_SIZE = 1024 * 64;
            public byte[] m_bufferRead;
            public WebRequest m_request;
            public WebResponse m_response;
            public Stream m_streamResponse;
            public DownloadCallback m_downloadCallback;
            public string m_remotePath;
            public object m_param;
            public NetType m_netType;
            public RequestState()
            {
                m_bufferRead = new byte[BUFFER_SIZE];
                m_request = null;
                m_streamResponse = null;
            }
            public void Stop()
            {
                if (m_request != null)
                {
                    m_request.Abort();
                    m_request = null;
                }

                if (m_response != null)
                {
                    m_response.Close();
                    m_response = null;
                }

                if (m_streamResponse != null)
                    m_streamResponse.Close();

                m_streamResponse = null;
            }
        }

        public void SetCredentials(string username, string password)
        {
            _userName = username;
            _password = password;
        }


        public void BeginDownLoadFolder(string uri)
        {

        }


        public void BeginDownload(string uri, DownloadCallback callback, object param,NetType netType = NetType.FTP)
        {
            RequestState requestState = new RequestState();
            requestState.m_downloadCallback = callback;
            requestState.m_param = param;
            requestState.m_netType = netType;
            _allRequestState.Add(requestState);

            try
            {
                WebRequest request = _CreateGetRequest(uri, netType);
                requestState.m_request = request;
                requestState.m_remotePath = uri;
                IAsyncResult result = request.BeginGetResponse(_ResponseCallback, requestState);
            }
            catch (Exception e)
            {
                requestState.Stop();
                callback.OnError(param, e);
                _allRequestState.Remove(requestState);
            }
        }

        private void _ResponseCallback(IAsyncResult ar)
        {

            RequestState requestState = ar.AsyncState as RequestState;
            try
            {
                WebRequest request = requestState.m_request;

                long length = 0;
                if (requestState.m_netType == NetType.FTP)
                {
                    requestState.m_response = request.EndGetResponse(ar) as FtpWebResponse;
                    FtpWebRequest tempRequest = _CreateFtpRequest(new Uri(requestState.m_remotePath), "SIZE");
                    length = tempRequest.GetResponse().ContentLength;
                }
                else
                {
                    requestState.m_response = request.EndGetResponse(ar) as HttpWebResponse;
                    length = requestState.m_response.ContentLength;
                }
                
                Stream stream = requestState.m_response.GetResponseStream();

                //准备阶段出现了错误
                if(!requestState.m_downloadCallback.OnPrepare(length, requestState.m_param))
                {
                    requestState.Stop();
                    _allRequestState.Remove(requestState);
                    return;
                }
               
                requestState.m_streamResponse = stream;
                stream.BeginRead(requestState.m_bufferRead, 0, RequestState.BUFFER_SIZE, _ReadCallback, requestState);

            }
            catch (Exception e)
            {
                requestState.Stop();
                requestState.m_downloadCallback.OnError(requestState.m_param, e);
                _allRequestState.Remove(requestState);
            }
        }
        

        private void _ReadCallback(IAsyncResult ar)
        {
            RequestState requestState = ar.AsyncState as RequestState;
            try
            {
                int readLen = requestState.m_streamResponse.EndRead(ar);
                if (readLen > 0)
                {
                    byte[] buffer = new byte[readLen];
                    Array.Copy(requestState.m_bufferRead, 0, buffer, 0, readLen);

                    requestState.m_downloadCallback.OnProcess(buffer, requestState.m_param);
                    requestState.m_streamResponse.BeginRead(requestState.m_bufferRead, 0, RequestState.BUFFER_SIZE, _ReadCallback, requestState);
                }
                else
                {
                    requestState.Stop();
                    requestState.m_downloadCallback.OnComplate(requestState.m_param);
                    _allRequestState.Remove(requestState);
                }
            }
            catch (Exception e)
            {
                requestState.Stop();
                requestState.m_downloadCallback.OnError(requestState.m_param, e);
                _allRequestState.Remove(requestState);
            }
        }

        private WebRequest _CreateGetRequest(string uri,NetType netType)
        {
            try
            {
                WebRequest webRequest = null;

                if (netType == NetType.HTTP)
                {
                    HttpWebRequest request = WebRequest.Create(uri) as HttpWebRequest;
                    request.Method = "GET";
                    request.Timeout = 10000;
                    request.ReadWriteTimeout = 10000;
                    webRequest = request;
                }
                else
                {
                    FtpWebRequest request = _CreateFtpRequest(new Uri(uri), "RETR");
                    webRequest = request;
                }

                return webRequest;
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
                return null;
            }
        }

        private FtpWebRequest _CreateFtpRequest(Uri uri, string ftpMethod)
        {
            try
            {
                FtpWebRequest request = FtpWebRequest.Create(uri) as FtpWebRequest;
                request.Method = ftpMethod;
                
                //用户名 密码.
                request.Credentials = new NetworkCredential(_userName, _password);
                request.Timeout = 10000;
                request.ReadWriteTimeout = 10000;
                request.UseBinary = true;
                return request;
            }
            catch (WebException ex)
            {
                Debug.LogError(ex.Message);
                return null;
            }
        }


        public void StopAllDownload()
        {
            foreach(var requestState in _allRequestState)
            {
                requestState.Stop();
            }

            _allRequestState = new List<RequestState>();
        }
    }
}
