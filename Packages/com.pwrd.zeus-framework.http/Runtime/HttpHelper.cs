/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.IO;

namespace Zeus.Framework.Http
{
    internal class WebReqState
    {
        public byte[] Buffer;

        public FileStream fs;

        public const int BufferSize = 1024;

        public Stream OrginalStream;

        public HttpWebResponse WebResponse;

        public WebReqState(string path)
        {
            Buffer = new byte[1024];
            //Debug.Log ("create local filestream:" + path);
            try
            {
                fs = new FileStream(path, FileMode.Create);
            }
            catch (Exception ex)
            {
                Debug.Log("error:" + ex.Message);
            }
            //Debug.Log ("create local filestream complete");
        }

    }

    public class HttpHelper
    {

        string path = null;
        string httpUrl = null;
        string tag;
        Action<bool, string> completeFunction;


        public HttpHelper(string localPath, string remoteUrl, string tag, Action<bool, string> nextFunc)
        {
            this.path = localPath;
            this.httpUrl = remoteUrl;
            this.completeFunction = nextFunc;
            this.tag = tag;
        }

        /// <summary>
        /// 检测网址是否可用
        /// </summary>
        /// <returns></returns>
        public bool CheckPathState(string url)
        {
            //Debug.Log("地址：" + url);
            HttpWebRequest myHttpWebRequest = null;
            HttpWebResponse myHttpWebResponse = null;
            bool state = false;
            try
            {
                myHttpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                myHttpWebRequest.UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64; Trident/7.0; rv:11.0) like Gecko";
                myHttpWebRequest.Method = "GET";
                myHttpWebRequest.Timeout = 5000;              //设置网页响应时间长度
                myHttpWebRequest.AllowAutoRedirect = false;//是否允许自动重定向

                myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                if (myHttpWebResponse.StatusCode == HttpStatusCode.OK)
                {
                    state = true;
                }
                else
                {
                    Debug.Log("error code :" + myHttpWebResponse.StatusCode);
                }
            }
            catch (WebException e)
            {
                Debug.Log(e.Message);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message);
            }
            finally
            {
                if (myHttpWebResponse != null)
                    myHttpWebResponse.Close();
            }
            return state;
        }

        public void AsyDownLoad()
        {
            try
            {
                //Debug.Log("server res url="+url);
                string directory = Path.GetDirectoryName(this.path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                HttpWebRequest httpRequest = WebRequest.Create(httpUrl) as HttpWebRequest;
                httpRequest.BeginGetResponse(new AsyncCallback(ResponseCallback), httpRequest);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        void ResponseCallback(IAsyncResult ar)
        {
            HttpWebRequest req = ar.AsyncState as HttpWebRequest;
            HttpWebResponse response = null;
            if (req == null) return;
            try
            {
                response = req.EndGetResponse(ar) as HttpWebResponse;
            }
            catch (Exception e)
            {
                Debug.LogError(this.httpUrl + " : " + e.Message);
                if (completeFunction != null)
                {
                    completeFunction(false, tag);
                }
                return;
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                Debug.LogError("Download error:" + tag + " " + response.StatusCode);
                response.Close();
                if (completeFunction != null)
                {
                    completeFunction(false, tag);
                }
                return;
            }

            WebReqState st = new WebReqState(path);
            st.WebResponse = response;
            Stream responseStream = response.GetResponseStream();
            st.OrginalStream = responseStream;
            responseStream.BeginRead(st.Buffer, 0, WebReqState.BufferSize, new AsyncCallback(ReadDataCallback), st);
        }

        void ReadDataCallback(IAsyncResult ar)
        {
            //Debug.Log("response2");
            WebReqState rs = ar.AsyncState as WebReqState;
            //Debug.Log("response21");
            int read = rs.OrginalStream.EndRead(ar);
            //Debug.Log("response22");
            if (read > 0)
            {
                //Debug.Log("response23");
                rs.fs.Write(rs.Buffer, 0, read);
                //Debug.Log("response24");
                rs.fs.Flush();
                //Debug.Log("response25");
                rs.OrginalStream.BeginRead(rs.Buffer, 0, WebReqState.BufferSize, new AsyncCallback(ReadDataCallback), rs);
                //Debug.Log("response26");
            }
            else
            {
                //Debug.Log("response27");
                rs.fs.Close();
                //Debug.Log("response28");
                rs.OrginalStream.Close();
                rs.WebResponse.Close();
                //Debug.Log(tag+":::: success");
                if (completeFunction != null)
                {
                    completeFunction(true, tag);
                }

            }
        }

        public bool SyncDownLoad()
        {
            bool flag = false;
            FileStream FStream = null;

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                string directory = Path.GetDirectoryName(this.path);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                FStream = new FileStream(path, FileMode.Create);

                //打开网络连接
                HttpWebRequest myRequest = (HttpWebRequest)HttpWebRequest.Create(httpUrl);
                //向服务器请求,获得服务器的回应数据流
                WebResponse myResponse = myRequest.GetResponse();
                Stream myStream = myResponse.GetResponseStream();
                //定义一个字节数据
                byte[] btContent = new byte[512];
                int intSize = 0;
                intSize = myStream.Read(btContent, 0, 512);
                while (intSize > 0)
                {
                    FStream.Write(btContent, 0, intSize);
                    intSize = myStream.Read(btContent, 0, 512);
                }
                //关闭流
                FStream.Close();
                myResponse.Close();

                flag = true;
            }
            catch (Exception ex)
            {
                if (FStream != null)
                {
                    FStream.Close();
                }
                flag = false;
                Debug.LogException(ex);
                Debug.LogError(ex.Message);
            }
            if (completeFunction != null)
            {
                completeFunction(flag, tag);
            }
            return flag;
        }
    }
}
