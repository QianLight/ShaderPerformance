/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#define OpenHttpDownloadLog
#if UNITY_EDITOR ||(!UNITY_ANDROID && !UNITY_IOS)
using System.Collections.Generic;
using UnityEngine;
using System.Net;
using System.Threading;
using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Concurrent;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Zeus.Framework.Http.UnityHttpDownloader.Tool;

namespace Zeus.Framework.Http.UnityHttpDownloader.Platform
{

    class UnityDownloader : PlatformDownloader
    {
        #region Class

        private class DownloadUnit
        {
            public const int READ_BYTES_ZERO_RETRY_COUNT_LIMIT = 3;
            string _tempSavePath;
            long _from;
            long _to;
            int _index;
            volatile bool _finish = false;
            volatile bool _success = false;
            int _retryCount = 0; 
            int _readBytesZeroRetryCount = 0; 
            ErrorType _error = ErrorType.None;
            public DownloadUnit(string tempSavePath, long from, long to, int index)
            {
                _tempSavePath = tempSavePath;
                _from = from;
                _to = to;
                _index = index;
                Size = to - from + 1;
                ReadBytesZeroRetryCount = READ_BYTES_ZERO_RETRY_COUNT_LIMIT;
            }

            public string TempSavePath { get { return _tempSavePath; } }
            public long From { get { return _from; } }
            public long To { get { return _to; } }
            public bool Finish { get { return _finish; } set { _finish = value; } }
            public bool Success { get { return _success; } set { _success = value; } }
            public int RetryCount { get { return _retryCount; } set { _retryCount = value; } }
            public int ReadBytesZeroRetryCount { get { return _readBytesZeroRetryCount; } set { _readBytesZeroRetryCount = value; } }
            public int Index { get { return _index; } set { _index = value; } }
            public long Size { get; private set; }

            public ErrorType Error { get { return _error; } set { _error = value; } }

            public FileStream GetReadStream()
            {
                if (File.Exists(TempSavePath))
                {
                    return File.OpenRead(TempSavePath);
                }
                else
                {
                    UnityEngine.Debug.LogError("Try Open Stream,But File Isn't Exists:" + TempSavePath);
                    return null;
                }
            }

            public void DeleteTempFile()
            {
                if (File.Exists(TempSavePath))
                {
                    File.Delete(TempSavePath);
                }
            }

            public void Reset()
            {
                Finish = false;
                Success = false;
                RetryCount = 0;
                ReadBytesZeroRetryCount = READ_BYTES_ZERO_RETRY_COUNT_LIMIT;
            }
        }
        #endregion




        #region Field
        //=======================static==========================
        /// <summary>
        /// 静态字段，只要有一个 downloader 从服务器获取到字节，此值就会更新，则说明网络正常，重置 DownloadUnit 的重试次数为0
        /// </summary>
        private static long _allReceived = 0;


        //=======================private==========================
        private const int UNIT_DEFAULT_RETRY_COUNT_LIMIT = 2;
        private int UNIT_OVERRIDE_RETRY_COUNT_LIMIT = 0;
        private int _unitRetryCountLimit = UNIT_DEFAULT_RETRY_COUNT_LIMIT;
        private long _maxClipSize = 2097152L;//1024 * 1024 * 2 = 2097152
        private static int _maxThreadNum = 12;//Mathf.Min(4, SystemInfo.processorCount);
        private volatile bool _abort = false;
        private long _asyncOperationCount = 0;//为0时Abort操作成功
        private volatile bool _isComplete = false;
        private int _streamBufferSize = 20480;
        private long _realNeedDownloadSize = -1;
        private Stopwatch _stopwatch;
        private List<DownloadUnit> _allDownloadUnitList;
        private ConcurrentQueue<DownloadUnit> _toDownloadUnitQueue;
        private long _totalReceived = 0;
        private ErrorType _error;
        private long unitFailedCount = 0;//最后一次成功下载到现在，中间失败的个数，每次unit下载成功都会重置为0
        private long downloadingCount = 0;//正在进行下载的任务个数
        private int _downloadRetryCount = 0;
        #endregion




        #region Attribute
        //=======================private===========================
        private int MaxThreadNum
        {
            get
            {
                if (Arg.isMultiThread)
                {
                    if (Arg.threadLimit > 0)
                    {
                        return Mathf.Min(_maxThreadNum, Arg.threadLimit);
                    }
                    return _maxThreadNum;
                }
                return 1;
            }
        }

        /// <summary>
        /// 是否达到重试次数上限。
        /// 保证了在当前任务下载失败的情况下，每个url及增加刷新参数的url均会被尝试一次。
        /// </summary>
        private bool IsReachRetryCountLimit
        {
            get
            {
                return _downloadRetryCount >= Arg.MaxUrlCount;
            }
        }


        //=======================public=============================
        /// <summary>
        /// 位枚举
        /// </summary>
        public override int Error
        {
            get
            {
                return (int)_error;
            }
        }

        public override int TopPriorityError
        {
            get
            {
                int bit = 0;
                int error = Error;
                if (error == 0)
                {
                    return 0;
                }
                while ((error /= 2) != 0)
                {
                    bit++;
                }
                return 1 << bit;
            }
        }

        /// <summary>
        /// 每秒下载字节数（平均速度）
        /// </summary>
        public override long AvgDownloadSpeed
        {
            get
            {
                if (RealNeedDownloadSize <= 0 || _stopwatch == null || _stopwatch.ElapsedMilliseconds == 0)
                {
                    return 0;
                }
                return Interlocked.Read(ref _totalReceived) * 1000 / _stopwatch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// 下载耗时
        /// </summary>
        public override long DownloadTime
        {
            get
            {
                if (_stopwatch == null)
                {
                    return 0;
                }
                return _stopwatch.ElapsedMilliseconds;
            }
        }

        public override long TotalReceived
        {
            get
            {
                long temp = Interlocked.Read(ref _totalReceived);
                if (temp > RealNeedDownloadSize)
                {
                    if (RealNeedDownloadSize > 0)
                    {
                        return RealNeedDownloadSize;
                    }
                    else
                    {
                        return Arg.targetSize;
                    }
                }
                return temp;
            }
        }

        public override long RealNeedDownloadSize
        {
            get
            {
                return _realNeedDownloadSize;
            }
        }

        public override bool IsAbort
        {
            get
            {
                return _abort;
            }
        }

        public override bool IsAborting
        {
            get
            {
                if (IsAbort && Interlocked.Read(ref _asyncOperationCount) > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion




        #region Constructor
        public UnityDownloader(DownloadArg arg) : base(arg)
        {
            if (Arg.enableSpeedLimit)
            {
                _streamBufferSize = Mathf.Max(1, Mathf.Min(_streamBufferSize, Convert.ToInt32(LimitSpeed / MaxThreadNum / 10)));
            }
            ResetState();
        }
        #endregion




        #region Method
        public override void StartDownLoad()
        {
            ResetState();

            _stopwatch = new Stopwatch();
            _stopwatch.Start();

            if (Arg.targetSize <= 0)
            {
                // 获取热更资源文件大小
                SendHeadRequest();
            }
            else
            {
                CalcTaskCount(true);
            }
        }

        private void ResetState()
        {
            _error = ErrorType.None;
            unitFailedCount = 0;
            downloadingCount = 0;
            _totalReceived = 0;
            _isComplete = false;
            Interlocked.Exchange(ref _asyncOperationCount, 0);
            _unitRetryCountLimit = UNIT_OVERRIDE_RETRY_COUNT_LIMIT > 0 ? UNIT_OVERRIDE_RETRY_COUNT_LIMIT : UNIT_DEFAULT_RETRY_COUNT_LIMIT;
            _downloadRetryCount = 0;
            if (!IsAbort)
            {
                Arg.ResetIndex();
            }
            _abort = false;
            if (Arg.IsPartialFile)
            {
                if (Arg.toIndex != Arg.fromIndex)
                {
                    Arg.targetSize = Arg.toIndex - Arg.fromIndex + 1;
                }
                else
                {
                    UnityEngine.Debug.LogError(string.Concat("[UnityDownloader] SplitFile but fromindex == toindex: ", Arg.toIndex, " \r\nPath: ", Arg.destPath));
                }
            }
            else
            {
                if (Arg.targetSize > 0)
                {
                    Arg.fromIndex = 0;
                    Arg.toIndex = Arg.targetSize - 1;
                }
            }
        }

        private void SendHeadRequest()
        {
            Interlocked.Increment(ref _asyncOperationCount);
            HttpWebRequest req = HttpWebRequest.CreateHttp(Arg.CurUrl);
            req.Method = "HEAD";
            req.Timeout = 5000;
            req.BeginGetResponse(HandleSizeResponse, req);
        }

        private void HandleSizeResponse(IAsyncResult ar)
        {
            try
            {
                if (!IsAbort)
                {
                    HttpWebRequest req = ar.AsyncState as HttpWebRequest;
                    HttpWebResponse resp = req.EndGetResponse(ar) as HttpWebResponse;
#if OpenHttpDownloadLog
                    UnityEngine.Debug.Log(string.Concat("[UnityDownloader]resultCode:" + resp.StatusCode.ToString() + ", Description:" + resp.StatusDescription));
#endif
                    if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.PartialContent)
                    {
                        string lenStr = resp.Headers[HttpResponseHeader.ContentLength];
#if OpenHttpDownloadLog
                        UnityEngine.Debug.Log("[UnityDownloader] Patch Size Response,PatchSize:" + lenStr);
#endif
                        long.TryParse(lenStr, out Arg.targetSize);
                        if (!Arg.IsPartialFile)
                        {
                            Arg.fromIndex = 0;
                            Arg.toIndex = Arg.targetSize - 1;
                        }
                        CalcTaskCount(true);
                    }
                    else
                    {
                        if (resp.StatusCode == HttpStatusCode.NotFound)
                        {
                            _error = _error | ErrorType.MissingFile;
                        }
                        else
                        {
                            _error = _error | ErrorType.NetError;
                        }
                        CheckHeadRequestRetry();
                    }
                }
            }
            catch (WebException e)
            {
                UnityEngine.Debug.Log(e);
                _error = _error | ErrorType.NetError;
                CheckHeadRequestRetry();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e);
                _error = _error | ErrorType.Exception;
                CheckHeadRequestRetry();
            }
            finally
            {
                Interlocked.Decrement(ref _asyncOperationCount);
            }
        }

        private void CheckHeadRequestRetry()
        {
            if (IsReachRetryCountLimit)
            {
                Finish(false);
            }
            else
            {
                _downloadRetryCount++;
                Arg.SwitchNextUrl();
                SendHeadRequest();
            }
        }

        private void CalcTaskCount(bool calcRealNeedDownloadSize)
        {
            Interlocked.Exchange(ref unitFailedCount, 0);
            FileInfo file = new FileInfo(GetFinalTempPath(Arg.destPath));
            if (file.Exists && file.Length == Arg.targetSize)
            {
                _totalReceived = _realNeedDownloadSize > 0 ? _realNeedDownloadSize : Arg.targetSize;
                ThreadPool.QueueUserWorkItem(CheckFile);
            }
            else
            {
                _maxClipSize = (long)Mathf.Clamp(Arg.targetSize / 10, 1024 * 1024, 1024 * 1024 * 8);

                //计算任务个数
                int taskCount = 1;
                if (Arg.targetSize > _maxClipSize)
                {
                    taskCount = Convert.ToInt32(Math.Ceiling((double)Arg.targetSize / _maxClipSize));
                }

                _allDownloadUnitList = new List<DownloadUnit>(taskCount);
                _toDownloadUnitQueue = new ConcurrentQueue<DownloadUnit>();
                if (calcRealNeedDownloadSize)
                {
                    _realNeedDownloadSize = Arg.targetSize;
                }
                for (int i = 0; i < taskCount; i++)
                {
                    //例：请求前100个字节的 HTTP 协议请求中，from 参数将指定为0，并且 to 参数将指定为99。故 to 要减1。
                    //https://docs.microsoft.com/zh-cn/dotnet/api/system.net.httpwebrequest.addrange?view=netcore-3.1#System_Net_HttpWebRequest_AddRange_System_Int64_System_Int64_
                    long to = Math.Min(Arg.toIndex, _maxClipSize * (i + 1) + Arg.fromIndex - 1);
                    long from = _maxClipSize * i + Arg.fromIndex;

                    string unitTempPath = Arg.destPath + HttpDownloader.TempMarkStr + i.ToString() + HttpDownloader.TempExtension;
                    DownloadUnit unit = new DownloadUnit(unitTempPath, from, to, i);
                    _allDownloadUnitList.Add(unit);

                    if (File.Exists(unitTempPath))
                    {
                        FileInfo fileInfo = new FileInfo(unitTempPath);
                        if (calcRealNeedDownloadSize)
                        {
                            _realNeedDownloadSize -= fileInfo.Length;
                        }
                        if (fileInfo.Length == unit.Size)
                        {
                            unit.Finish = true;
                            unit.Success = true;
                        }
                        else
                        {
                            _toDownloadUnitQueue.Enqueue(unit);
                        }
                    }
                    else
                    {
                        _toDownloadUnitQueue.Enqueue(unit);
                    }
                }

                PrepareDownload();
            }
        }

        private void PrepareDownload(DownloadUnit completeUnit = null)
        {
            Interlocked.Increment(ref _asyncOperationCount);
            lock (this)
            {
                if (!IsAbort && !_isComplete)
                {
                    if (completeUnit != null)
                    {
                        if (completeUnit.Success)
                        {
                            for (int i = 0; i < _allDownloadUnitList.Count; i++)
                            {
                                if (_allDownloadUnitList[i].Finish && !_allDownloadUnitList[i].Success)
                                {
                                    _allDownloadUnitList[i].Reset();
                                    _toDownloadUnitQueue.Enqueue(_allDownloadUnitList[i]);
                                }
                            }
                        }
                        else
                        {
                            _error = _error | completeUnit.Error;
                        }
                    }
                    while (!IsAbort &&
                        _toDownloadUnitQueue.Count > 0 &&
                        Interlocked.Read(ref unitFailedCount) == 0 &&
                        Interlocked.Read(ref downloadingCount) < MaxThreadNum)
                    {
                        DownloadUnit unit;
                        if (_toDownloadUnitQueue.TryDequeue(out unit))
                        {
                            Interlocked.Increment(ref downloadingCount);
                            ThreadPool.QueueUserWorkItem(Download, unit);
                        }
                    }
                    if (Interlocked.Read(ref downloadingCount) == 0)
                    {
                        TryFinish();
                    }
                }
            }
            Interlocked.Decrement(ref _asyncOperationCount);
        }

        static DateTime startTime = DateTime.Now;
        static int limitReadBytes = 0;
        static object limitSpeedLock = new object();
        public static long LimitSpeed = 1024 * 1024;

        private void Download(object obj)
        {
            Interlocked.Increment(ref _asyncOperationCount);
            DownloadUnit unit = obj as DownloadUnit;
            var buffer = new Byte[_streamBufferSize];
            while (!IsAbort && !unit.Finish && unit.RetryCount <= _unitRetryCountLimit)
            {
                long preAllReceive = Interlocked.Read(ref _allReceived);
                unit.RetryCount++;
                if (unit.RetryCount > _unitRetryCountLimit)
                {
                    unit.Finish = true;
                    unit.Success = false;
                }
                else
                {
#if OpenHttpDownloadLog
                    //UnityEngine.Debug.Log("[UnityDownloader] unit.Index:" + unit.Index + ",unit.RetryCount:" + unit.RetryCount);
                    //UnityEngine.Debug.Log("[UnityDownloader] Download Patch File,url:" + unit.CurUrl + ", savePath:" + _destPath);
#endif
                    HttpWebRequest req = null;
                    var readTotalBytes = 0;
                    try
                    {
                        var readBytes = 0;
                        long fileSize = 0;

                        using (var writeStream = CreateFileStream(unit.TempSavePath, out fileSize))
                        {
                            //例：请求前100个字节的 HTTP 协议请求中，from 参数将指定为0，并且 to 参数将指定为99。故 size 要加1。
                            if (fileSize < unit.Size)
                            {
                                req = HttpWebRequest.CreateHttp(Arg.CurUrl);
                                req.ServerCertificateValidationCallback = CheckRemoteCertificateValidation;
                                req.KeepAlive = true;
                                req.ServicePoint.UseNagleAlgorithm = false;
                                req.ServicePoint.Expect100Continue = false;
                                req.ServicePoint.ConnectionLimit = 256;
                                req.AllowWriteStreamBuffering = false;
                                req.Method = "GET";
#if UNITY_IOS && !UNITY_EDITOR
                                req.Timeout = 10 * 1000;
#else
                                req.Timeout = 5 * 1000;
#endif
                                req.ReadWriteTimeout = 5 * 1000;
                                req.Proxy = null;
                                req.AddRange(unit.From + fileSize, unit.To);
                                HttpWebResponse response = null;
                                using (response = req.GetResponse() as HttpWebResponse)
                                {
                                    using (var readStream = response.GetResponseStream())
                                    {
                                        while (!IsAbort)
                                        {
                                            if (Arg.enableSpeedLimit)
                                            {
                                                readBytes = readStream.Read(buffer, 0, buffer.Length / 10);
                                            }
                                            else
                                            {
                                                readBytes = readStream.Read(buffer, 0, buffer.Length);
                                            }
                                            if (readBytes <= 0 || IsAbort)
                                            {
                                                break;
                                            }
                                            readTotalBytes += readBytes;
                                            writeStream.Write(buffer, 0, readBytes);
                                            writeStream.Flush();
                                            Interlocked.Add(ref _totalReceived, readBytes);
                                            Interlocked.Add(ref _allReceived, readBytes);

                                            if (Arg.enableSpeedLimit)
                                            {
                                                lock (limitSpeedLock)
                                                {
                                                    Interlocked.Add(ref limitReadBytes, readBytes);
                                                    if (limitReadBytes > (LimitSpeed / 10))
                                                    {
                                                        int timeSpan = (int)(DateTime.Now - startTime).TotalMilliseconds;
                                                        if (timeSpan < 100)
                                                        {
                                                            int sleepMillis = Mathf.CeilToInt(Mathf.Max(100, limitReadBytes / LimitSpeed - timeSpan));
                                                            Thread.Sleep(sleepMillis);
                                                        }
                                                        Interlocked.Exchange(ref limitReadBytes, 0);
                                                        startTime = DateTime.Now;
                                                    }
                                                }
                                            }
                                        }
                                        writeStream.Flush();
                                        writeStream.Close();
                                        readStream.Close();
                                        if (!IsAbort)
                                        {
                                            //有一定概率会出现readBytes为0但是实际上资源并没下载完的情况，需要重新建立连接继续下载
                                            FileInfo fileInfo = new FileInfo(unit.TempSavePath);
                                            if (fileInfo.Length < unit.Size)
                                            {
                                                if (unit.ReadBytesZeroRetryCount > 0)
                                                {
                                                    unit.RetryCount--;
                                                    unit.ReadBytesZeroRetryCount--;
                                                }
                                                else
                                                {
                                                    _error = _error | ErrorType.NetError;
                                                    unit.Finish = true;
                                                    unit.Success = false;
                                                }
                                            }
                                            else if(fileInfo.Length == unit.Size)
                                            {
                                                unit.Finish = true;
                                                unit.Success = true;
                                            }
                                            else
                                            {
                                                Interlocked.Add(ref _totalReceived, -readTotalBytes);
                                                File.Delete(unit.TempSavePath);
                                            }
                                        }
                                    }
                                    response.Close();
                                }
                            }
                            else if (fileSize == unit.Size)
                            {
                                writeStream.Close();
                                unit.Finish = true;
                                unit.Success = true;
                            }
                            else
                            {
                                writeStream.Close();
                                File.Delete(unit.TempSavePath);
                            }
                        }
                    }
                    catch (WebException e)
                    {
                        if (readTotalBytes > 0 || Interlocked.Read(ref _allReceived) > preAllReceive)
                        {
                            unit.RetryCount = 0;
                        }

                        if (e.Response != null)
                        {
                            HttpWebResponse response = e.Response as HttpWebResponse;
                            if (response != null && response.StatusCode == HttpStatusCode.NotFound)
                            {
                                unit.Error = unit.Error | ErrorType.MissingFile;
                            }
                            else
                            {
                                unit.Error = unit.Error | ErrorType.NetError;
                            }
                        }
                        else
                        {
                            unit.Error = unit.Error | ErrorType.NetError;
                        }

                        UnityEngine.Debug.LogError("[UnityDownloader] WebException:" + e.Status + "," + e.ToString() + " : " + Arg.CurUrl);
                    }
                    catch (IOException e)
                    {
                        if (IsHardDiskFull(e))
                        {
                            unit.Finish = true;
                            unit.Success = false;
                            unit.Error = unit.Error | ErrorType.HardDiskFull;
                        }
                        else
                        {
                            unit.Error = unit.Error | ErrorType.IOException;
                        }
                        UnityEngine.Debug.LogError("[UnityDownloader] IOException:" + e.ToString() + " : " + Arg.CurUrl);
                    }
                    catch (Exception e)
                    {
                        unit.Error = unit.Error | ErrorType.Exception;
                        UnityEngine.Debug.LogError("[UnityDownloader] " + e.ToString());
                    }
                    finally
                    {
                        if (req != null)
                        {
                            req.Abort();
                        }
                    }
                }
            }
            Interlocked.Decrement(ref downloadingCount);
            if (!IsAbort)
            {
                if (unit.Success)
                {
                    Interlocked.Exchange(ref unitFailedCount, 0);
                }
                else
                {
                    Interlocked.Increment(ref unitFailedCount);
                }
                PrepareDownload(unit);
            }
            Interlocked.Decrement(ref _asyncOperationCount);
        }

        private bool CheckRemoteCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        private const int HR_ERROR_HANDLE_DISK_FULL = unchecked((int)0x80070027);
        private const int HR_ERROR_DISK_FULL = unchecked((int)0x80070070);
        /// <summary>
        /// 是否存储空间不足异常
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool IsHardDiskFull(IOException e)
        {
            //存储空间不足
            if (e.HResult == HR_ERROR_HANDLE_DISK_FULL || e.HResult == HR_ERROR_DISK_FULL)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void TryFinish()
        {
            if (Interlocked.Read(ref unitFailedCount) == 0)
            {
                //检测是否结束下载
                for (int i = 0; i < _allDownloadUnitList.Count; i++)
                {
                    if (!_allDownloadUnitList[i].Finish)
                    {
                        return;
                    }
                }
            }
            //检测是否全部成功
            for (int i = 0; i < _allDownloadUnitList.Count; i++)
            {
                if (!_allDownloadUnitList[i].Success)
                {
                    CheckForRetry();
                    return;
                }
            }

            bool combineSuc = CombineFiles();

            if (!IsAbort)
            {
                if (combineSuc)
                {
                    CheckFile(null);
                }
                else
                {
                    CheckForRetry();
                }
            }
        }

        private bool CombineFiles()
        {
            Interlocked.Increment(ref _asyncOperationCount);
            string tempPath = GetFinalTempPath(Arg.destPath);

            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }

            //合并临时文件到目标位置
            long fileSize;
            var buffer = new Byte[_streamBufferSize];
            int readBytes;
            try
            {
                if (_allDownloadUnitList.Count > 1)
                {
                    using (var writeStream = CreateFileStream(tempPath, out fileSize))
                    {
                        for (int i = 0; i < _allDownloadUnitList.Count; i++)
                        {
                            using (FileStream readStream = _allDownloadUnitList[i].GetReadStream())
                            {
                                while (!IsAbort)
                                {
                                    readBytes = readStream.Read(buffer, 0, buffer.Length);
                                    if (readBytes <= 0 || IsAbort)
                                    {
                                        break;
                                    }
                                    writeStream.Write(buffer, 0, readBytes);
                                }
                            }
                            if (IsAbort)
                            {
                                Interlocked.Decrement(ref _asyncOperationCount);
                                return false;
                            }
                            _allDownloadUnitList[i].DeleteTempFile();
                        }
                        writeStream.Flush();
                        writeStream.Close();
                    }
                }
                else
                {
                    File.Move(_allDownloadUnitList[0].TempSavePath, tempPath);
                }
            }
            catch (IOException e)
            {
                if (IsHardDiskFull(e))
                {
                    _error = _error | ErrorType.HardDiskFull;
                }
                else
                {
                    _error = _error | ErrorType.CombineFile;
                }
                UnityEngine.Debug.LogError("[UnityDownloader] IOException:" + e.ToString());
                Interlocked.Decrement(ref _asyncOperationCount);
                return false;
            }
            catch (Exception e)
            {
                _error = _error | ErrorType.CombineFile;
                UnityEngine.Debug.LogError("[UnityDownloader] CombineFileException:" + e.ToString());
                Interlocked.Decrement(ref _asyncOperationCount);
                return false;
            }
            Interlocked.Decrement(ref _asyncOperationCount);
            return true;
        }

        private void CheckFile(object obj)
        {
            Interlocked.Increment(ref _asyncOperationCount);
            if (Arg.checkAlgorithm == null)
            {
                SaveFile();
                _error = ErrorType.None;
                Finish(true);
            }
            else
            {
                bool isMatch = false;
                switch (Arg.checkAlgorithm.checkAlgorithmType)
                {
                    case CheckAlgorithmType.Md5:
                        isMatch = CheckMd5();
                        break;
                    case CheckAlgorithmType.Crc32:
                        isMatch = CheckCrc32();
                        break;
                }
                if (!IsAbort)
                {
                    if (isMatch)
                    {
                        SaveFile();
                        _error = ErrorType.None;
                        Finish(true);
                    }
                    else
                    {
                        _error = _error | ErrorType.CheckFail;
                        File.Delete(GetFinalTempPath(Arg.destPath));
                        _totalReceived = 0;
                        _stopwatch.Stop();
                        _stopwatch.Restart();
                        CheckForRetry();
                    }
                }
            }
            Interlocked.Decrement(ref _asyncOperationCount);
        }

        private bool CheckMd5()
        {
            if (!string.IsNullOrEmpty(Arg.checkAlgorithm.md5))
            {
                string md5 = CheckAlgorithmUtil.GetMD5FromFile(GetFinalTempPath(Arg.destPath));
                return !string.IsNullOrEmpty(md5) && md5.Equals(Arg.checkAlgorithm.md5);
            }
            else
            {
                return true;
            }
        }

        private bool CheckCrc32()
        {
            if (Arg.checkAlgorithm.crc32 != 0)
            {
                int crc32 = CheckAlgorithmUtil.GetCrc32FromFile(GetFinalTempPath(Arg.destPath));
                return crc32 != 0 && crc32 == Arg.checkAlgorithm.crc32;
            }
            return false;
        }

        private static string GetFinalTempPath(string oriPath)
        {
            return oriPath + ".finalTemp";
        }

        private void SaveFile()
        {
            string tempPath = GetFinalTempPath(Arg.destPath);
            if (File.Exists(Arg.destPath))
            {
                File.Delete(Arg.destPath);
            }
            File.Move(tempPath, Arg.destPath);
        }

        private void CheckForRetry()
        {
            if (!IsReachRetryCountLimit && (_error & (ErrorType.CheckFail | ErrorType.NetError | ErrorType.MissingFile | ErrorType.CombineFile)) != 0)
            {
                _downloadRetryCount++;
                Arg.SwitchNextUrl();
                CalcTaskCount(false);
            }
            else
            {
                Finish(false);
            }
        }

        private void Finish(bool sucess)
        {
            _isComplete = true;
            _stopwatch.Stop();
            Arg.OnFinish(sucess);
        }

        private FileStream CreateFileStream(string path, out long fileSize)
        {
            fileSize = 0;
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            FileStream filestream = null;
            if (File.Exists(path))
            {
                filestream = File.OpenWrite(path);
                fileSize = filestream.Length;
                filestream.Seek(fileSize, SeekOrigin.Current);
            }
            else
            {
                filestream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            }

            return filestream;
        }


        /// <summary>
        /// 修改限速值，如果当前downloader正在下载也会立即生效
        /// </summary>
        /// <param name="speed"></param>
        public override void EnableSpeedLimit(bool isLimit)
        {
            Arg.enableSpeedLimit = isLimit;
        }

        public override void SetOverrideRetryCountLimit(int times)
        {
            UNIT_OVERRIDE_RETRY_COUNT_LIMIT = times;
        }

        public override void SetIsMultiThread(bool isMultiThread)
        {
            Arg.isMultiThread = isMultiThread;
        }

        public override void SetThreadLimit(int threadlimit)
        {
            Arg.threadLimit = threadlimit;
        }

        public override void Refresh(DownloadArg arg)
        {
            base.Refresh(arg);
            ResetState();
        }

        public override void Abort()
        {
            if (!_abort)
            {
                _stopwatch.Stop();
                _abort = true;
            }
        }

        public override void SetAllowDownloadInBackground(bool allow) { }

        /// <summary>
        /// 是否允许移动网络下载，编辑器状态不生效
        /// </summary>
        /// <param name="allow"></param>
        public override void SetAllowCarrierDataNetworkDownload(bool allow) { }

        public override void SetSucNotificationStr(string suc) { }

        public override void SetFailNotificationStr(string fail) { }

        /// <summary>
        /// 修改限速值
        /// </summary>
        /// <param name="speed"></param>
        public static void SetLimitSpeed(long limitSpeed)
        {
            LimitSpeed = limitSpeed;
        }

        /// <summary>
        /// 根据存储位置以及目标文件大小，计算剩余下载大小
        /// </summary>
        /// <param name="destPath"></param>
        /// <param name="targetSize"></param>
        /// <returns></returns>
        public static long CalcRealDownloadSize(string destPath, long targetSize)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            int i = 0;
            builder.Append(destPath);
            builder.Append(HttpDownloader.TempMarkStr);
            builder.Append(i);
            builder.Append(HttpDownloader.TempExtension);
            string unitPath = builder.ToString();
            FileInfo file = new FileInfo(unitPath);
            while (file.Exists)
            {
                targetSize -= file.Length;
                i++;

                builder.Clear();
                builder.Append(destPath);
                builder.Append(HttpDownloader.TempMarkStr);
                builder.Append(i);
                builder.Append(HttpDownloader.TempExtension);
                unitPath = builder.ToString();

                file = new FileInfo(unitPath);
            }
            return targetSize;
        }

        public static bool IsCarrierDataNetwork()
        {
            return false;
        }
        #endregion

        public override void Dispose()
        {

        }
    }
}
#endif