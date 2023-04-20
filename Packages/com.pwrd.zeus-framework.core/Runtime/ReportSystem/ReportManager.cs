/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using UnityEngine;
using Zeus.Core.FileSystem;

namespace Zeus.Core.ReportSystem
{
    public class ReportManager
    {
        private static ReportManager _instance;
        public static ReportManager Instance
        {
            get
            {
                if (null == _instance)
                {
                    _instance = new ReportManager();
                }
                return _instance;
            }
        }

        private int _uploadSize;
        public int UploadSize
        {
            get
            {
                return _uploadSize;
            }
            set
            {
                _uploadSize = value > 1 ? value : 1;
            }
        }
        private int _uploadInterval;
        public int UploadInterval
        {
            get
            {
                return _uploadInterval;
            }
            set
            {
                _uploadInterval = value > 1 ? value : 1;
            }
        }

        private volatile bool _running = false;
        private ZeusReportConf _reportConf;
        private ReportMarkCenter _markCenter = null;
        private ReportDataCenter _dataCenter = null;
        private ManualResetEvent _uploadEvent = null;
        private AutoResetEvent _writeEvent = null;
        private Thread _uploadThread = null;
        private Thread _writeThread = null;
        private Dictionary<string, string> _urlMap = new Dictionary<string, string>();
        private ConcurrentQueue<ReportLogWithTopic> _writeQueue = new ConcurrentQueue<ReportLogWithTopic>(); //写入到磁盘的队列
        private int _growId = -1;

        //账号和玩家信息
        private string _userName;
        private string _playerName;
        private ulong _uId;

        #region 对外接口

        /// <summary>
        /// 设置项目名
        /// </summary>
        public void SetProject(string project)
        {
            if (null != _dataCenter)
                _dataCenter.SetProject(project);
        }

        /// <summary>
        /// 设置渠道名
        /// </summary>
        public void SetChannel(string channel)
        {
            if (null != _dataCenter)
                _dataCenter.SetChannel(channel);
        }

        /// <summary>
        /// 当玩家登录或切换账号时调用
        /// </summary>
        /// <param name="uId"> 用户uid </param>
        /// <param name="userName"> 用户名 </param>
        public void SetAccount(ulong uId, string userName)
        {
            _uId = uId;
            _userName = userName;
        }

        /// <summary>
        /// 当一个账号有多角色时，登入或切换角色时调用
        /// </summary>
        /// <param name="playerName"> 角色名 </param>
        public void SetPlayerName(string playerName)
        {
            _playerName = playerName;
        }

        /// <summary>
        /// 上报数据
        /// </summary>
        /// <param name="store"> 仓库 </param>
        /// <param name="topic"> 主题 </param>
        /// <param name="hint"> 数据内容 </param>
        public void Report(string store, string topic, string hint)
        {
            if (!_running)
            {
                return;
            }

            //过滤掉被禁用的store
            if (_reportConf.bannedStores.Contains(store))
            {
                return;
            }

            var logWithTopic = BuildLogWithTopic(store, topic, hint);

            _writeQueue.Enqueue(logWithTopic);
            _writeEvent.Set();
        }

        #endregion

        private ReportManager()
        {
            _reportConf = ReportUtil.LoadConf();

            if (_reportConf.enable)
            {
                Init();
            }
        }

        private void Init()
        {
            _running = true;

            ZeusCore.Instance.RegisterOnApplicationQuit(Abort);

            string reportRoot = InitPath();
            _markCenter = new ReportMarkCenter(reportRoot);
            _dataCenter = new ReportDataCenter(reportRoot, _reportConf.encryptData);

            SetProject(_reportConf.projectName);
            SetChannel(_reportConf.channelName);
            UploadSize = _reportConf.uploadSize;
            UploadInterval = _reportConf.uploadInterval;
            _growId = _markCenter.ReadGrowId();

            //开启写入本地的线程
            _writeEvent = new AutoResetEvent(false);
            _writeThread = new Thread(DoWriteThread);
            _writeThread.Start();
            //开启上报线程
            _uploadEvent = new ManualResetEvent(true);
            _uploadThread = new Thread(DoUploadThread);
            _uploadThread.Start();
        }

        private void Abort()
        {
            _running = false;

            ReportLogWithTopic temp;
            while (!_writeQueue.IsEmpty)
            {
                _writeQueue.TryDequeue(out temp);
            }

            if (null != _dataCenter)
            {
                _dataCenter.Release();
            }

            _urlMap.Clear();

            if (null != _uploadEvent)
            {
                _uploadEvent.Set();
            }

            if (null != _writeEvent)
            {
                _writeEvent.Set();
            }
        }

        private string InitPath()
        {
            string reportRoot = OuterPackage.GetRealPath(ReportConsts.REPORT_ROOT_RELATIVE_PATH);
            if (!Directory.Exists(reportRoot))
                Directory.CreateDirectory(reportRoot);

            return reportRoot;
        }

        private void DoUploadThread()
        {
            bool _needResend = false;

            while (_running)
            {
                _uploadEvent.WaitOne();
                if (0 == UploadInterval || 0 == UploadSize)
                    continue;

                ReportData reportData = null;
                try
                {
                    int reportedId = _markCenter.ReadReportedId();
                    _dataCenter.HandleDeleteQueue(reportedId);

                    if (_needResend)
                    {
                        reportData = _dataCenter.GetResendData();
                    }
                    else
                    {
                        //从上传队列中获取数据
                        reportData = _dataCenter.GetData(UploadSize, reportedId);
                    }

                    if (null != reportData && !reportData.IsEmpty())
                    {
                        _needResend = !Send(reportData);
                    }
                    else
                    {
                        _needResend = false;
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("[ReportManager][DoUploadThread] " + ex);
                }

                //当没有报告时暂停Thread
                if (null == reportData || reportData.IsEmpty())
                {
                    _uploadEvent.Reset();
                    continue;
                }

                if (_needResend)
                    Thread.Sleep(1000 * ReportConsts.RESEND_INTERVAL);
                else
                    Thread.Sleep(1000 * UploadInterval);
            }
        }

        private void DoWriteThread()
        {
            while (_running)
            {
                _writeEvent.WaitOne();

                try
                {
                    int lastWriteId = -1;
                    if (!_writeQueue.IsEmpty)
                    {
                        _dataCenter.WriteToDisk(_writeQueue, ref lastWriteId);
                    }

                    if (-1 != lastWriteId)
                    {
                        _markCenter.WriteGrowId(lastWriteId);
                    }

                    //写完新数据后打开_uploadEvent
                    _uploadEvent.Set();
                }
                catch (Exception ex)
                {
                    Debug.LogError("[ReportManager][DoWriteThread] " + ex);
                }
            }
        }

        private ReportLogWithTopic BuildLogWithTopic(string store, string topic, string hint)
        {
            _growId = _growId >= int.MaxValue ? 1 : ++_growId;

            var log = new ReportLog(store, hint, _growId, _uId, _userName, _playerName);
            var reportLogWithTopic = new ReportLogWithTopic(topic, log);

            return reportLogWithTopic;
        }

        #region 发送

        //向目标服务器发送报告
        private bool Send(ReportData data)
        {
            var sendContent = JsonUtility.ToJson(data);

            bool isSuc = false;
            string url = GetUrl(data);

            using (HttpWebResponse response = Post(sendContent, url))
            {
                if (null != response)
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        int lastReportId = data.GetLastReportId();
                        _markCenter.WriteReportedId(lastReportId);
                        _dataCenter.SetDeleteId(lastReportId);
                        isSuc = true;
                    }
                    else
                    {
                        Debug.LogError("[ReportManager][Send] " + response.StatusCode);
                    }
                }
            }

            return isSuc;
        }

        private HttpWebResponse Post(string data, string url)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data);

            HttpWebRequest request = null;
            HttpWebResponse response = null;

            try
            {
                request = WebRequest.Create(url) as HttpWebRequest;
                request.ProtocolVersion = HttpVersion.Version11;
                request.ContentLength = buffer.Length;
                request.Method = ReportConsts.REPORT_METHOD;
                request.KeepAlive = false;
                request.ServicePoint.MaxIdleTime = 5000;
                request.ServicePoint.ConnectionLimit = 1000;
                request.ServicePoint.Expect100Continue = false;
                request.ConnectionGroupName = Guid.NewGuid().ToString();

                request.Headers.Add(ReportConsts.HEADER_BODYRAWSIZE, buffer.Length.ToString());
                request.Headers.Add(ReportConsts.HEADER_APIVERSION, ReportConsts.HEADER_APIVERSION_VALUE);

                using (Stream reqStream = request.GetRequestStream())
                {
                    reqStream.Write(buffer, 0, buffer.Length);
                }

                response = request.GetResponse() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                HandleWebException(ex);
            }
            finally
            {
                if (null != request)
                    request.Abort();
            }

            return response;
        }

        private void HandleWebException(WebException ex)
        {
            Debug.LogError("[ReportManager][HandleWebException] " + ex);
            if (ex.Status == WebExceptionStatus.ProtocolError && ex.Response != null)
            {
                var resp = (HttpWebResponse)ex.Response;
                var code = resp.StatusCode;
                Debug.LogError("[ReportManager][HandleWebException] " + code);
                if (code == HttpStatusCode.NotFound || code == HttpStatusCode.Forbidden)
                {
                    Abort();
                }
            }
        }

        private string GetUrl(ReportData data)
        {
            string store = data._store;

            string url;
            if (!_urlMap.TryGetValue(store, out url))
            {
                url = string.Format(ReportConsts.REPORT_URL, data.__tags__.pro, _reportConf.endPoint, store);
                _urlMap.Add(store, url);
            }

            return url;
        }

        #endregion
    }
}
