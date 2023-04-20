/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using System;
using System.IO;
using System.Collections;
using System.Collections.Concurrent;
using System.Threading;
using Zeus.Framework.Http.UnityHttpDownloader;
using Zeus.Framework.Http.UnityHttpDownloader.Tool;
using UnityEngine;
using Zeus.Framework.KeepAlive;

namespace Zeus.Framework.Http
{
    public class HttpDownloadManager : IDisposable
    {
        #region Class: TaskExecuter
        private class TaskExecuter : IComparable<TaskExecuter>, IDisposable
        {
            public const int UrgentThread = 2;
            public const int NormalThread = 1;

            public enum TaskType
            {
                Normal = 0,//普通任务
                Urgent = 1,//紧急任务
                Normal2Urgent = 2,//开始是普通任务，后来变成紧急任务
            }

            public enum TaskState
            {
                Initial = 0,
                Downloading,
                Finish,
            }
            private Func<string, ErrorType, bool> _callback;
            private List<int> _normalHandler = new List<int>();
            private HttpDownloader _downloader;
            private DownloadArg _arg;
            private TaskType _type;
            private volatile TaskState _state;
            private volatile bool _isSuccess = false;
            private int _priority;
            private long _successReceived = 0;
            private bool _allowDownloadInBackground = true;
            private bool _allowCarrierDataNetworkDownload = true;
            private bool alreadyDisposed = false;

            private HttpDownloader Downloader
            {
                get
                {
                    if (Application.platform == RuntimePlatform.Android)
                    {
                        //懒加载，此处是为了防止安卓低端机崩溃
                        if (_downloader == null && !_isSuccess)
                        {
                            _downloader = new HttpDownloader(_arg);
                            if (IsUrgent)
                            {
                                _downloader.EnableSpeedLimit(false);//紧急任务不限速
                                _downloader.SetOverrideRetryCountLimit(6);//紧急任务最大重试次数为6次
                            }
                            _downloader.SetAllowDownloadInBackground(_allowDownloadInBackground);
                            _downloader.SetAllowCarrierDataNetworkDownload(_allowCarrierDataNetworkDownload);
                        }
                    }
                    return _downloader;
                }
            }
            public int Priority { set { _priority = value; } }

            public long TotalReceived
            {
                get
                {
                    if (_state == TaskState.Initial)
                    {
                        return 0;
                    }
                    if (_isSuccess)
                    {
                        if (_successReceived == 0 && Downloader != null)
                        {
                            _successReceived = Downloader.TotalReceived;
                        }
                        return _successReceived;
                    }
                    return Downloader.TotalReceived;
                }
            }

            public bool IsUrgent { get { return _type != TaskType.Normal; } }
            public string DestPath { get; private set; }
            public CheckAlgorithm CheckAlgorithm
            {
                get
                {
                    if (_arg != null)
                    {
                        return _arg.checkAlgorithm;
                    }
                    return null;
                }
            }
            public TaskType Type { get { return _type; } }
            public bool IsSuccess
            {
                get
                {
                    if (_isSuccess)
                    {
                        if (_successReceived == 0 && Downloader != null)
                        {
                            _successReceived = Downloader.TotalReceived;
                        }
                    }
                    return _isSuccess;
                }
            }
            public bool IsFinish { get { return _state == TaskState.Finish; } }
            public bool IsAbort
            {
                get
                {
                    if (_isSuccess)
                    {
                        return false;
                    }
                    return Downloader.IsAbort;
                }
            }
            public bool IsAborting
            {
                get
                {
                    if (_isSuccess)
                    {
                        return false;
                    }
                    return Downloader.IsAborting;
                }
            }

            public ErrorType TopPriorityError
            {
                get
                {
                    if (_isSuccess)
                    {
                        return ErrorType.None;
                    }
                    return Downloader.TopPriorityError;
                }
            }

            public TaskExecuter(List<string> urlList, long size, CheckAlgorithm checkAlgorithm, string destPath, bool urgent, long from = -1, long to = -1, string sucNotificationStr = null, string failNotificationStr = null)
            {
                _state = TaskState.Initial;
                _type = urgent ? TaskType.Urgent : TaskType.Normal;
                DestPath = destPath;
                if (from >= 0 && to >= 0)
                {
                    _arg = new DownloadArg(DownloadType.PartialFile, sucNotificationStr, failNotificationStr);
                }
                else
                {
                    _arg = new DownloadArg(DownloadType.WholeFile, sucNotificationStr, failNotificationStr);
                }
                _arg.isGroupTask = true;//True：多文件下载时使用，会将很多文件统筹管理；False：单文件下载时使用。
                _arg.sourceUrls = urlList.ToArray();
                _arg.destPath = destPath;
                _arg.isMultiThread = IsUrgent;//紧急任务可以多线程下载
                _arg.threadLimit = IsUrgent ? UrgentThread : NormalThread;
                _arg.targetSize = size;
                _arg.fromIndex = from;
                _arg.toIndex = to;
                _arg.checkAlgorithm = checkAlgorithm;

                if (Application.platform != RuntimePlatform.Android)
                {
                    _downloader = new HttpDownloader(_arg);
                    if (IsUrgent)
                    {
                        _downloader.EnableSpeedLimit(false);//紧急任务不限速
                        _downloader.SetOverrideRetryCountLimit(6);//紧急任务最大重试次数为6次
                    }
                    _downloader.SetAllowDownloadInBackground(_allowDownloadInBackground);
                    _downloader.SetAllowCarrierDataNetworkDownload(_allowCarrierDataNetworkDownload);
                }
            }

            private void OnDownloadComplete(bool suc)
            {
                _arg.callback = null;
                _isSuccess = suc;
                _state = TaskState.Finish;
            }

            public void StartDownload()
            {
                if (_isSuccess)
                {
                    return;
                }
                _successReceived = 0;
                _arg.callback = OnDownloadComplete;
                _state = TaskState.Downloading;
                Downloader.StartDownLoad();
            }

            public void EnableSpeedLimit(bool isLimit)
            {
                if (IsUrgent)
                {
                    //紧急任务不限速
                    return;
                }
                if (_isSuccess)
                {
                    return;
                }
                Downloader.EnableSpeedLimit(isLimit);
            }

            public void TurnToUrgent()
            {
                if (IsUrgent)
                {
                    return;
                }
                if (_isSuccess)
                {
                    return;
                }
                _type = TaskType.Normal2Urgent;
                Downloader.SetThreadLimit(UrgentThread);
                Downloader.SetIsMultiThread(true);
                Downloader.EnableSpeedLimit(false);
                Downloader.SetOverrideRetryCountLimit(6);//紧急任务最大重试次数为6次
                SetAllowDownloadInBackground(true);
                SetAllowCarrierDataNetworkDownload(true);
            }

            public void Abort()
            {
                if (_isSuccess)
                {
                    return;
                }
                _arg.callback = null;
                if (Downloader != null)
                {
                    Downloader.Abort();
                }
            }

            public void SetAllowDownloadInBackground(bool allow)
            {
                //紧急任务不限制网络类型
                if (IsUrgent)
                {
                    allow = true;
                }
                if (_allowDownloadInBackground != allow)
                {
                    _allowDownloadInBackground = allow;
                    if (_downloader != null)
                    {
                        _downloader.SetAllowDownloadInBackground(allow);
                    }
                }

            }

            /// <summary>
            /// 是否允许移动网络下载，编辑器状态不生效
            /// </summary>
            /// <param name="allow"></param>
            public void SetAllowCarrierDataNetworkDownload(bool allow)
            {
                //紧急任务不限制网络类型
                if (IsUrgent)
                {
                    allow = true;
                }
                if (_allowCarrierDataNetworkDownload != allow)
                {
                    _allowCarrierDataNetworkDownload = allow;
                    if (_downloader != null)
                    {
                        _downloader.SetAllowCarrierDataNetworkDownload(allow);
                    }
                }
            }

            public void SetSucNotificationStr(string suc)
            {
                if (_downloader != null)
                {
                    _downloader.SetSucNotificationStr(suc);
                }
                else
                {
                    _arg.sucNotificationStr = suc;
                }
            }

            public void SetFailNotificationStr(string fail)
            {
                if (_downloader != null)
                {
                    _downloader.SetFailNotificationStr(fail);
                }
                else
                {
                    _arg.failNotificationStr = fail;
                }
            }

            /// <summary>
            /// 注册回调
            /// </summary>
            /// <param name="callback">不保证回调会在主线程调用，需调用方自行保证。回调函数的返回值：true表示此回调函数执行一次就删掉，false表示任务在失败后多次重试时可以重复调用此回调函数</param>
            public void RegistCallback(Func<string, ErrorType, bool> callback)
            {
                if (callback != null)
                {
                    _callback -= callback;//去重
                    _callback += callback;
                }
            }

            /// <summary>
            /// 是否是一个无效的任务(不用重试下载的任务)
            /// 如果已经下载完了的任务（成功或者失败），没有了回调函数，说明不需要再重试下载了
            /// 一个初始任务（即还没开始的任务是可以没有回调函数的）
            /// </summary>
            /// <returns></returns>
            public bool IsInvalidTask()
            {
                return _state != TaskState.Initial && _callback == null;
            }

            public void DoCallback()
            {
                ErrorType error = _isSuccess ? ErrorType.None : Downloader.TopPriorityError;
                if (_callback != null)
                {
                    Delegate[] array = _callback.GetInvocationList();
                    for (int i = 0; i < array.Length; i++)
                    {
                        bool result = true;
                        try
                        {
                            result = (bool)array[i].DynamicInvoke(_arg.destPath, error);
                        }
                        catch (Exception e)
                        {
#if UNITY_2018_1_OR_NEWER || DEVELOPMENT_BUILD
                            UnityEngine.Debug.LogError($"Invoke Callback Func Fail,{_arg.destPath},{e.ToString()}");
#endif
                        }
                        if (result)
                        {
                            _callback -= (Func<string, ErrorType, bool>)array[i];
                        }
                    }
                }
            }

            //优先级越大排序越靠后
            public int CompareTo(TaskExecuter other)
            {
                return _priority.CompareTo(other._priority);
            }

            ~TaskExecuter()
            {
                Dispose();
            }

            public void Dispose()
            {
                if (alreadyDisposed)
                    return;

                if (_downloader != null)
                {
                    _downloader.Dispose();
                    _downloader = null;
                }
                //set disposed flag:
                alreadyDisposed = true;
                GC.SuppressFinalize(this);
            }
        }
        #endregion


        #region Enum: State
        public enum State
        {
            None = 0,
            Running = 1,//下载中
            Pause = 2,//在不允许4G下载或者不允许后台下载的情况下，自动暂停（紧急下载不受影响）
            Abort = 3,//主动停止下载任务（紧急下载不受影响）
        }
        /// <summary>
        /// 暂停状态的原因
        /// </summary>
        [Flags]
        public enum PauseStateReason
        {
            None = 0,
            NotAllowCarrierDataNetwork = 1,//不允许移动网络下载，当前处于移动网络状态
            NotAllowBackGround = 1 << 1,//不允许后台下载，当前处于后台状态
        }
        #endregion


        #region Field
        private string _sucNotificationStr = "Download Success";
        private string _failNotificationStr = "Download Fail";
        private string _keepAliveNotificationStr = null;
        private volatile State _curState = State.None;
        private PauseStateReason _pauseReason = PauseStateReason.None;
        private int _threadLimit = 1;
        private const int MAX_THREAD_NUM = 12;
        private long _realNeedDownloadSize;
        private long _finishedTasksReceived;
        private long _completeSize;
        private ConcurrentDictionary<string, long> _taskDownloadedBytesDict = new ConcurrentDictionary<string, long>();
        private List<TaskExecuter> _downloading = new List<TaskExecuter>();

        private List<TaskExecuter> _toDownloadList = new List<TaskExecuter>();
        private bool _needSort = false;

        private Dictionary<string, TaskExecuter> _toDownloadDict = new Dictionary<string, TaskExecuter>();

        private List<TaskExecuter> _failCacheList = new List<TaskExecuter>();
        private List<TaskExecuter> _retryList = new List<TaskExecuter>();

        private int _normalPriority = 0;
        private int _urgentPriority = int.MaxValue;

        private bool _isSpeedLimited = false;

        private LoopCaller _loopCaller;

        private volatile bool _isInBackground = false;
        private bool _LowConsumptionMode = false;
        //后台状态下所有任务处理完之后，会将失败的任务重新放入待下载中并延时5秒再下载
        private const int BACKGROUND_SLEEP_TIME = 1000 * 5;

#if UNITY_EDITOR
        private bool _isQiut = false;
#endif

        /// <summary>
        /// 是否允许后台下载，编辑器状态不生效
        /// </summary>
        private bool _allowDownloadInBackground = true;

        /// <summary>
        /// 是否允许移动网络下载，编辑器状态不生效
        /// </summary>
        private bool _allowCarrierDataNetworkDownload = true;

#if !UNITY_EDITOR && UNITY_ANDROID
        private static int _mainThreadID = int.MinValue;
        private static System.Collections.Concurrent.ConcurrentDictionary<int, int> thread2AttachCount = new System.Collections.Concurrent.ConcurrentDictionary<int, int>();
#endif

        //注：此 mgr 仅会根据外部设置的是否允许后台下载以及是否允许移动网络下载来控制是否开启新的普通任务下载，不会控制当前正在进行中的普通任务暂停或继续

        #endregion


        #region Attribute
        /// <summary>
        /// 每个下载任务都处理完并得到了一个结果，不保证全部下载成功
        /// </summary>
        public bool IsDone { get; private set; }
        public double Progress { get { return (double)CompleteSize / RealNeedDownloadSize; } }
        public long RealNeedDownloadSize { get { return _realNeedDownloadSize; } }
        public long CompleteSize
        {
            get
            {
                return _completeSize;
            }
        }

        public int ThreadLimit { get { return _threadLimit; } }

        /// <summary>
        /// 后台状态下，所有任务下载成功时，如果有值，会将此值作为内容显示一个通知
        /// </summary>
        public string SucNotificationStr
        {
            get
            {
                return _sucNotificationStr;
            }
            set
            {
                _sucNotificationStr = value;
                lock (_downloading)
                {
                    for (int i = 0; i < _downloading.Count; i++)
                    {
                        _downloading[i].SetSucNotificationStr(SucNotificationStr);
                    }
                }
                lock (_toDownloadList)
                {
                    for (int i = 0; i < _toDownloadList.Count; i++)
                    {
                        _toDownloadList[i].SetSucNotificationStr(SucNotificationStr);
                    }
                }
            }
        }
        /// <summary>
        /// 后台状态下，下载失败时，如果有值，会将此值作为内容显示一个通知
        /// </summary>
        public string FailNotificationStr
        {
            get
            {
                return _failNotificationStr;
            }
            set
            {
                _failNotificationStr = value;
                lock (_downloading)
                {
                    for (int i = 0; i < _downloading.Count; i++)
                    {
                        _downloading[i].SetFailNotificationStr(FailNotificationStr);
                    }
                }
                lock (_toDownloadList)
                {
                    for (int i = 0; i < _toDownloadList.Count; i++)
                    {
                        _toDownloadList[i].SetFailNotificationStr(FailNotificationStr);
                    }
                }
            }
        }

        /// <summary>
        /// 当前状态
        /// </summary>
        public State CurState { get { return _curState; } }
        /// <summary>
        /// 位枚举，切换到暂停状态的原因，如果当前状态是 Pause ，此属性有值
        /// APP后台状态导致的的 pause 不应该做处理，切到前台会自动转到 running 状态,
        /// 仅关注移动网络导致的 pause 即可
        /// </summary>
        public PauseStateReason PauseReason { get { return _pauseReason; } }

        public bool IsInBackground
        {
            get
            {
                if (_isInBackground)
                {
                    //移动平台从顶部拉出下拉菜单时，会失去焦点，导致_isInBackground为false，
                    //这种状态不认为是后台，需要调用原生接口确认是否为真正的后台
                    bool result = _isInBackground;
                    AttachCurrentThread();
                    try
                    {
                        if (UnityMobilePlatformKeepAlive.IsTurnOn())
                        {
                            result = UnityMobilePlatformKeepAlive.IsWorking();
                        }
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogError(e.ToString());
                    }
                    finally
                    {
                        DetachCurrentThread();
                    }
                    return result;
                }
                return _isInBackground;
            }
        }
        #endregion


        #region Method
        /// <summary>
        /// 注意：本构造函数需在主线程调用
        /// 其他接口在子线程使用需要时，安卓平台需要 Attach 到 java 虚拟机，需要调用如下函数：
        /// AttachCurrentThread();
        /// DetachCurrentThread();
        /// </summary>
        /// <param name="sucNotificationStr">后台状态下，所有任务下载成功时，如果有值，会将此值作为内容显示一个通知</param>
        /// <param name="failNotificationStr">后台状态下，下载失败时，如果有值，会将此值作为内容显示一个通知</param>
        public HttpDownloadManager(string sucNotificationStr = null, string failNotificationStr = null)
        {
            _sucNotificationStr = sucNotificationStr;
            _failNotificationStr = failNotificationStr;
            IsDone = false;
            _curState = State.None;
#if UNITY_EDITOR
            //用于编辑器环境下取消播放时停止下载
            if (_loopCaller == null)
            {
                var go = GameObject.Find("LoopCaller(HttpDownloadManager)");
                if (go != null)
                {
                    _loopCaller = go.GetComponent<LoopCaller>();
                }
                if (_loopCaller == null)
                {
                    _loopCaller = new GameObject("LoopCaller(HttpDownloadManager)").AddComponent<LoopCaller>();
                }
                _loopCaller.OnQuit = OnQiut;
            }
#elif UNITY_ANDROID || UNITY_IOS
            //用于下载完成后监测是不是处于后台状态，如果是后台状态，发送系统通知
            if (!string.IsNullOrEmpty(_sucNotificationStr) || !string.IsNullOrEmpty(_failNotificationStr))
            {
                var go = GameObject.Find("LoopCaller(HttpDownloadManager)");
                if (go != null)
                {
                    _loopCaller = go.GetComponent<LoopCaller>();
                }
                if (_loopCaller == null)
                {
                    _loopCaller = new GameObject("LoopCaller(HttpDownloadManager)").AddComponent<LoopCaller>();
                }
                if (_loopCaller != null)
                {
                    _loopCaller.OnFocus = OnFocus;
                }
            }
#if UNITY_ANDROID
            HttpDownloader.GenAndroidJavaClass();
            UnityMobilePlatformKeepAlive.GenAndroidJavaClass();
            _mainThreadID = Thread.CurrentThread.ManagedThreadId;
#endif
#endif
        }

        public void Dispose()
        {
            if (_loopCaller != null)
            {
                _loopCaller.WaitDestroy = true;
                _loopCaller = null;
            }
#if !UNITY_EDITOR && UNITY_ANDROID
            AttachCurrentThread();
            try
            {
                HttpDownloader.DisposeAndroidJavaClass();
                UnityMobilePlatformKeepAlive.DisposeAndroidJavaClass();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                DetachCurrentThread();
            }
#endif
        }

        ~HttpDownloadManager()
        {
            Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlList"></param>
        /// <param name="size"></param>
        /// <param name="md5"></param>
        /// <param name="destPath"></param>
        /// <param name="callback">不保证回调会在主线程调用，需调用方自行保证。回调函数的返回值：true表示此回调函数执行一次就删掉，false表示任务在失败后多次重试时可以重复调用此回调函数</param>
        public void Download(List<string> urlList, long size, CheckAlgorithm checkAlgorithm, string destPath, Func<string, ErrorType, bool> callback)
        {
            AttachCurrentThread();
            try
            {
                TaskExecuter executer = null;
                lock (_downloading)
                {
                    for (int i = 0; i < _downloading.Count; i++)
                    {
                        executer = _downloading[i];
                        if (executer.DestPath.Equals(destPath))
                        {
                            Debug.Assert(executer.CheckAlgorithm.Equals(checkAlgorithm), "[HttpDowmloadManager] Add Same DestPath Task But CheckAlgorithm Different, Something Must Be Wrong!");
                            executer.RegistCallback(callback);
                            return;
                        }
                    }
                }
                if (_toDownloadDict.TryGetValue(destPath, out executer))
                {
                    Debug.Assert(executer.CheckAlgorithm.Equals(checkAlgorithm), "[HttpDowmloadManager] Add Same DestPath Task But CheckAlgorithm Different, Something Must Be Wrong!");
                    executer.RegistCallback(callback);
                }
                else
                {
                    bool find = false;
                    if (_retryList.Count > 0)
                    {
                        for (int i = 0; i < _retryList.Count; i++)
                        {
                            executer = _retryList[i];
                            if (executer.DestPath.Equals(destPath))
                            {
                                Debug.Assert(executer.CheckAlgorithm.Equals(checkAlgorithm), "[HttpDowmloadManager] Add Same DestPath Task But CheckAlgorithm Different, Something Must Be Wrong!");
                                find = true;
                                executer.RegistCallback(callback);
                                AddToDownload(executer);
                                break;
                            }
                        }
                        //_retryList.Remove(executer);此处不需要 remove，重试的时候不会重复添加
                    }

                    if (!find && _failCacheList.Count > 0)
                    {
                        for (int i = 0; i < _failCacheList.Count; i++)
                        {
                            executer = _failCacheList[i];
                            if (executer.DestPath.Equals(destPath))
                            {
                                Debug.Assert(executer.CheckAlgorithm.Equals(checkAlgorithm), "[HttpDowmloadManager] Add Same DestPath Task But CheckAlgorithm Different, Something Must Be Wrong!");
                                find = true;
                                executer.RegistCallback(callback);
                                AddToDownload(executer);
                                break;
                            }
                        }
                        //_failCacheList.Remove(executer);此处不需要 remove，重试的时候不会重复添加
                    }

                    if (!find)
                    {
                        executer = new TaskExecuter(urlList, size, checkAlgorithm, destPath, false, -1, -1, SucNotificationStr, FailNotificationStr);
                        executer.RegistCallback(callback);
                        executer.Priority = _normalPriority--;
                        AddToDownload(executer);

                        //此处是为了防止安卓低端机崩溃
#if !UNITY_EDITOR && UNITY_ANDROID
                        ThreadPool.QueueUserWorkItem((obj) =>
                        {
                            AndroidJNI.AttachCurrentThread();
                            long needDownloadSize = HttpDownloader.CalcRealDownloadSize(destPath, size);
                            Interlocked.Add(ref _realNeedDownloadSize, needDownloadSize);
                            long downloadedSize = size - needDownloadSize;
                            Interlocked.Add(ref _finishedTasksReceived, downloadedSize);
                            AndroidJNI.DetachCurrentThread();
                        });
#else
                        long needDownloadSize = HttpDownloader.CalcRealDownloadSize(destPath, size);
                        _realNeedDownloadSize += needDownloadSize;
                        long downloadedSize = size - needDownloadSize;
                        _finishedTasksReceived += downloadedSize;
#endif
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                DetachCurrentThread();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlsList"></param>
        /// <param name="sizeList"></param>
        /// <param name="checkAlgorithmList"></param>
        /// <param name="destPathList"></param>
        /// <param name="callback">不保证回调会在主线程调用，需调用方自行保证。回调函数的返回值：true表示此回调函数执行一次就删掉，false表示任务在失败后多次重试时可以重复调用此回调函数</param>
        public void Download(List<List<string>> urlsList, List<long> sizeList, List<CheckAlgorithm> checkAlgorithmList, List<string> destPathList, Func<string, ErrorType, bool> callback)
        {
            for (int i = 0; i < urlsList.Count; i++)
            {
                Download(urlsList[i], sizeList[i], checkAlgorithmList[i], destPathList[i], callback);
            }
        }

        private void AddToDownload(TaskExecuter executer)
        {
            lock (_toDownloadList)
            {
                AddToDownloadWithoutLock(executer);
            }
        }

        private void AddToDownloadWithoutLock(TaskExecuter executer)
        {
            if (!executer.IsInvalidTask() && !_toDownloadDict.ContainsKey(executer.DestPath))
            {
                executer.SetAllowDownloadInBackground(_allowDownloadInBackground);
                executer.SetAllowCarrierDataNetworkDownload(_allowCarrierDataNetworkDownload);
                _toDownloadDict.Add(executer.DestPath, executer);
                _toDownloadList.Add(executer);
                _needSort = true;
            }
        }

        private void AddListToDownload(List<TaskExecuter> list)
        {
            lock (_toDownloadList)
            {
                AddListToDownloadWithoutLock(list);
            }
        }

        private void AddListToDownloadWithoutLock(List<TaskExecuter> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                AddToDownloadWithoutLock(list[i]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="urlList"></param>
        /// <param name="size"></param>
        /// <param name="checkAlgorithm"></param>
        /// <param name="destPath"></param>
        /// <param name="callback">不保证回调会在主线程调用，需调用方自行保证。回调函数的返回值：true表示此回调函数执行一次就删掉，false表示任务在失败后多次重试时可以重复调用此回调函数</param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void DownloadUrgent(List<string> urlList, long size, CheckAlgorithm checkAlgorithm, string destPath, Func<string, ErrorType, bool> callback, long from = -1, long to = -1)
        {
            AttachCurrentThread();
            try
            {
                TaskExecuter executer = null;
                lock (_downloading)
                {
                    for (int i = 0; i < _downloading.Count; i++)
                    {
                        executer = _downloading[i];
                        if (executer.DestPath.Equals(destPath))
                        {
                            Debug.Assert(executer.CheckAlgorithm.Equals(checkAlgorithm), "[HttpDowmloadManager] Add Same DestPath Task But CheckAlgorithm Different, Something Must Be Wrong!");
                            if (!executer.IsUrgent) 
                            {
                                executer.TurnToUrgent();
                                executer.Priority = _urgentPriority--;
                                TryAbrotNormalTaskForUrgent(TaskExecuter.UrgentThread - TaskExecuter.NormalThread);
                            }
                            executer.RegistCallback(callback);
                            if (CalcDownloadingThreadCount() > MAX_THREAD_NUM)
                            {
                                executer.Abort();
#if DEVELOPMENT_BUILD
                                //Debug.Log("HttpDownloadManager DownloadUrgent start _finishedTasksReceived=" + _finishedTasksReceived);
#endif
                                Interlocked.Add(ref _finishedTasksReceived, executer.TotalReceived);
#if DEVELOPMENT_BUILD
                                //Debug.Log("HttpDownloadManager DownloadUrgent end _finishedTasksReceived=" + _finishedTasksReceived);
#endif
                                AddToDownload(executer);
                                _downloading.Remove(executer);
                            }
                            return;
                        }
                    }
                }
                if (_toDownloadDict.TryGetValue(destPath, out executer))
                {
                    Debug.Assert(executer.CheckAlgorithm.Equals(checkAlgorithm), "[HttpDowmloadManager] Add Same DestPath Task But CheckAlgorithm Different, Something Must Be Wrong!");
                    if (!executer.IsUrgent)
                    {
                        executer.TurnToUrgent();
                        executer.Priority = _urgentPriority--;

                        lock (_toDownloadList)
                        {
                            _needSort = true;//优先级变化，重新排序
                        }
                    }
                    executer.RegistCallback(callback);
                }
                else
                {
                    bool find = false;
                    if (_retryList.Count > 0)
                    {
                        for (int i = 0; i < _retryList.Count; i++)
                        {
                            executer = _retryList[i];
                            if (executer.DestPath.Equals(destPath))
                            {
                                Debug.Assert(executer.CheckAlgorithm.Equals(checkAlgorithm), "[HttpDowmloadManager] Add Same DestPath Task But CheckAlgorithm Different, Something Must Be Wrong!");
                                find = true;
                                if (!executer.IsUrgent)
                                {
                                    executer.TurnToUrgent();
                                    executer.Priority = _urgentPriority--;
                                }
                                executer.RegistCallback(callback);
                                AddToDownload(executer);
                                break;
                            }
                        }
                        //_retryList.Remove(executer); 此处不需要 remove，重试的时候不会重复添加
                    }

                    if (!find && _failCacheList.Count > 0)
                    {
                        for (int i = 0; i < _failCacheList.Count; i++)
                        {
                            executer = _failCacheList[i];
                            if (executer.DestPath.Equals(destPath))
                            {
                                Debug.Assert(executer.CheckAlgorithm.Equals(checkAlgorithm), "[HttpDowmloadManager] Add Same DestPath Task But CheckAlgorithm Different, Something Must Be Wrong!");
                                find = true;
                                if (!executer.IsUrgent)
                                {
                                    executer.TurnToUrgent();
                                    executer.Priority = _urgentPriority--;
                                }
                                executer.RegistCallback(callback);
                                AddToDownload(executer);
                                break;
                            }
                        }
                        //_failCacheList.Remove(executer); 此处不需要 remove，重试的时候不会重复添加
                    }

                    if (!find)
                    {
                        executer = new TaskExecuter(urlList, size, checkAlgorithm, destPath, true, from, to, SucNotificationStr, FailNotificationStr);
                        executer.RegistCallback(callback);
                        executer.Priority = _urgentPriority--;
                        AddToDownload(executer);
                    }
                }

                lock (_downloading)
                {
                    TryAbrotNormalTaskForUrgent(Mathf.CeilToInt((float)TaskExecuter.UrgentThread / TaskExecuter.NormalThread));
                }

                RegisterUpdate();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                DetachCurrentThread();
            }
        }

        /// <summary>
        /// 当有紧急任务添加进来的话
        /// 会尝试将正在下载的两个普通任务放回待下载中去
        /// 以便腾出线程来尽快下载紧急任务
        /// </summary>
        private void TryAbrotNormalTaskForUrgent(int removeTarget)
        {
            for (int i = _downloading.Count - 1; i >= 0; i--)
            {
                if (!_downloading[i].IsUrgent)
                {
                    _downloading[i].Abort();
#if DEVELOPMENT_BUILD
                    Debug.Log("HttpDownloadManager TryAbrotNormalTaskForUrgent start _finishedTasksReceived=" + _finishedTasksReceived);
#endif
                    Interlocked.Add(ref _finishedTasksReceived, _downloading[i].TotalReceived);
#if DEVELOPMENT_BUILD
                    Debug.Log("HttpDownloadManager TryAbrotNormalTaskForUrgent end _finishedTasksReceived=" + _finishedTasksReceived);
#endif
                    AddToDownload(_downloading[i]);
                    _downloading.Remove(_downloading[i]);
                    removeTarget--;
                    if (removeTarget <= 0)
                    {
                        break;
                    }
                }
            }
        }

        public void Start()
        {
            AttachCurrentThread();
            try
            {
                if (_curState != State.Running)
                {
                    _curState = State.Running;
                    IsDone = false;
                    if (_retryList.Count > 0)
                    {
                        AddListToDownload(_retryList);
                        _retryList.Clear();
                    }
                    if (_failCacheList.Count > 0)
                    {
                        AddListToDownload(_failCacheList);
                        _failCacheList.Clear();
                    }
#if DEVELOPMENT_BUILD || UNITY_EDITOR
                    Debug.Log("HttpDownloadManager Start _toDownloadList=" + _toDownloadList.Count);
#endif
                    RegisterUpdate();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                DetachCurrentThread();
            }
        }

        private volatile bool _isRegisterUpdate = false;
        private void RegisterUpdate()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.Log("HttpDownloadManager RegisterUpdate _isRegisterUpdate=" + _isRegisterUpdate + " _allowDownloadInBackground=" + _allowDownloadInBackground);
#endif
            if (!_isRegisterUpdate)
            {
                if (_allowDownloadInBackground)
                {
                    UnityMobilePlatformKeepAlive.TurnOn(_keepAliveNotificationStr);
                }
                _isRegisterUpdate = true;
                ThreadPool.QueueUserWorkItem(Update);
            }
        }

#if UNITY_EDITOR
        private void OnQiut()
        {
            _isQiut = true;
        }
#elif UNITY_ANDROID || UNITY_IOS
        private void OnFocus(bool focus)
        {
            _isInBackground = !focus;
        }
#endif

        private void UnRegisterUpdate()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.Log("HttpDownloadManager UnRegisterUpdate _isRegisterUpdate=" + _isRegisterUpdate + " _allowDownloadInBackground=" + _allowDownloadInBackground);
#endif
            if (_isRegisterUpdate)
            {
                if (_allowDownloadInBackground)
                {
                    UnityMobilePlatformKeepAlive.TurnOff();
                }
                _isRegisterUpdate = false;
            }
        }


        private void Update(object obj)
        {
#if !UNITY_EDITOR && UNITY_ANDROID
            AttachCurrentThread();
#endif
            while (_isRegisterUpdate)
            {
                try
                {
#if UNITY_EDITOR
                    //编辑器环境不进行后台下载，停止运行时即停止下载
                    if (_isQiut)
                    {
                        break;
                    }
#else
#if UNITY_ANDROID || UNITY_IOS
                    //非编辑器环境下要进行网络状态和app前后台状态的检测，并根据外部的设置来自动暂停或者继续普通任务的下载
                    CheckCarrierDataNetworkAndBackground();
#endif
#endif
                    if (_curState == State.Running || _toDownloadList.Count > 0 || _downloading.Count > 0)
                    {
                        CheckDownloadingTaskFinished();

                        TryStartNewTask();
                        Thread.Sleep(_LowConsumptionMode ? 1000 : 100);
                    }
                    else
                    {
                        Thread.Sleep(2000);
                    }
                }
                catch (Exception ex)
                {
#if UNITY_2018_1_OR_NEWER || DEVELOPMENT_BUILD
                    Debug.LogError(ex.ToString());
#endif
                }
            }

#if !UNITY_EDITOR && UNITY_ANDROID
            DetachCurrentThread();
#endif
        }

        private DateTime _carrierDataNetworkCheckTimer = DateTime.Now;
        private void CheckCarrierDataNetworkAndBackground()
        {
            //每2000毫秒检测网络状态和app前后台状态
            //当 _curState != State.Running 时只会在待下载列表中开启新的紧急任务，当前正在下载中的普通任务由 Native 代码维护自动暂停或者继续下载
            if ((DateTime.Now - _carrierDataNetworkCheckTimer).TotalMilliseconds >= 2000)
            {
                _carrierDataNetworkCheckTimer = DateTime.Now;
                if (_allowDownloadInBackground && _allowCarrierDataNetworkDownload)
                {
                    if (_curState == State.Pause)
                    {
                        _curState = State.Running;
                        _pauseReason = PauseStateReason.None;
                    }
                }
                else if (!_allowDownloadInBackground && _allowCarrierDataNetworkDownload)
                {
                    if (_curState == State.Running && IsInBackground)
                    {
                        _curState = State.Pause;
                        _pauseReason = PauseStateReason.NotAllowBackGround;
                    }
                    else if (_curState == State.Pause && !IsInBackground)
                    {
                        _curState = State.Running;
                        _pauseReason = PauseStateReason.None;
                    }
                }
                else if (_allowDownloadInBackground && !_allowCarrierDataNetworkDownload)
                {
                    bool isCarrierDataNetwork = HttpDownloader.IsCarrierDataNetwork();
                    if (_curState == State.Running && isCarrierDataNetwork)
                    {
                        _curState = State.Pause;
                        _pauseReason = PauseStateReason.NotAllowCarrierDataNetwork;
                    }
                    else if (_curState == State.Pause && !isCarrierDataNetwork)
                    {
                        _curState = State.Running;
                        _pauseReason = PauseStateReason.None;
                    }
                }
                else if (!_allowDownloadInBackground && !_allowCarrierDataNetworkDownload)
                {
                    bool isCarrierDataNetwork = HttpDownloader.IsCarrierDataNetwork();
                    if (_curState == State.Running && (IsInBackground || isCarrierDataNetwork))
                    {
                        _curState = State.Pause;
                        _pauseReason = PauseStateReason.NotAllowBackGround | PauseStateReason.NotAllowCarrierDataNetwork;
                    }
                    else if (_curState == State.Pause && !IsInBackground && !isCarrierDataNetwork)
                    {
                        _curState = State.Running;
                        _pauseReason = PauseStateReason.None;
                    }
                }
            }
        }

        List<TaskExecuter> _tempCallbackList = new List<TaskExecuter>();
        private void CheckDownloadingTaskFinished()
        {
            lock (_downloading)
            {
                if (_downloading.Count > 0)
                {
                    if (_tempCallbackList.Count > 0)
                    {
                        _tempCallbackList.Clear();
                    }
                    long downloadingSize = 0;
                    for (int i = _downloading.Count - 1; i >= 0; i--)
                    {
                        TaskExecuter executer = _downloading[i];
                        if (!_taskDownloadedBytesDict.TryAdd(executer.DestPath, executer.TotalReceived))
                        {
                            if(_taskDownloadedBytesDict[executer.DestPath] < executer.TotalReceived)
                            {
                                _taskDownloadedBytesDict[executer.DestPath] = executer.TotalReceived;
                            }
                        }

                        if (!executer.IsFinish)
                        {
                            downloadingSize += executer.TotalReceived;
                            continue;
                        }
                        if (executer.Type == TaskExecuter.TaskType.Normal || executer.Type == TaskExecuter.TaskType.Normal2Urgent)
                        {
#if DEVELOPMENT_BUILD
            //Debug.Log("HttpDownloadManager CheckDownloadingTaskFinished start _finishedTasksReceived=" + _finishedTasksReceived);
#endif
                            Interlocked.Add(ref _finishedTasksReceived, executer.TotalReceived);
#if DEVELOPMENT_BUILD
            //Debug.Log("HttpDownloadManager CheckDownloadingTaskFinished end _finishedTasksReceived=" + _finishedTasksReceived);
#endif
                        }
                        _downloading.Remove(executer);
                        
                        if (executer.IsSuccess)
                        {
                            _tempCallbackList.Add(executer);
                            executer.Dispose();
                            if (_failCacheList.Count > 0)
                            {
                                AddListToDownload(_failCacheList);
                                _failCacheList.Clear();
                            }
                            if (_retryList.Count > 0)
                            {
                                AddListToDownload(_retryList);
                                _retryList.Clear();
                            }
                        }
                        else
                        {
                            _taskDownloadedBytesDict.TryRemove(executer.DestPath, out _);
                            if (executer.TopPriorityError == ErrorType.NetError)
                            {
                                _failCacheList.Add(executer);
                                if (!IsInBackground)
                                {
                                    lock (_toDownloadList)
                                    {
                                        //前台状态累计失败任务个数达到上限则任务不再重试
                                        if ((_failCacheList.Count >= Math.Min(ThreadLimit, 3)) ||
                                            (_toDownloadList.Count == 0 && _downloading.Count == 0))
                                        {
                                            _retryList.AddRange(_failCacheList);
                                            _tempCallbackList.AddRange(_failCacheList);
                                            _failCacheList.Clear();
                                        }
                                    }
                                }
                                else
                                {
                                    bool sleep = false;
                                    lock (_toDownloadList)
                                    {
                                        //后台状态下所有任务处理完之后，会将失败的任务重新放入待下载中并延时5秒再下载
                                        if (_toDownloadList.Count == 0 && _downloading.Count == 0)
                                        {
                                            sleep = true;
                                            AddListToDownloadWithoutLock(_failCacheList);
                                            _failCacheList.Clear();
                                        }
                                    }
                                    if (sleep)
                                    {
                                        int timeLeft = BACKGROUND_SLEEP_TIME;
                                        while (IsInBackground && timeLeft > 0)
                                        {
                                            timeLeft -= 1000;
                                            Thread.Sleep(1000);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Debug.LogError($"[HttpDwonloader] Download :{executer.DestPath} fail. Error: {executer.TopPriorityError}");
                                _retryList.Add(executer);
                                _tempCallbackList.Add(executer);
                            }
                        }
                    }
                    Interlocked.Exchange(ref _completeSize, downloadingSize + Interlocked.Read(ref _finishedTasksReceived));
                }
                if (_tempCallbackList.Count > 0)
                {
                    for (int i = 0; i < _tempCallbackList.Count; i++)
                    {
                        _tempCallbackList[i].DoCallback();
                    }
                    _tempCallbackList.Clear();
                }
            }
        }

        private void TryStartNewTask()
        {
            lock (_downloading)
            {
                lock (_toDownloadList)
                {
                    if (_toDownloadList.Count > 0)
                    {
                        if (_needSort)
                        {
                            _needSort = false;
                            _toDownloadList.Sort();
                        }
                        TaskExecuter executer = _toDownloadList[_toDownloadList.Count - 1];
                        if (!executer.IsAbort || !executer.IsAborting)
                        {
                            int downloadThreadCount = CalcDownloadingThreadCount();
                            if ((executer.IsUrgent && downloadThreadCount <= MAX_THREAD_NUM - TaskExecuter.UrgentThread) ||
                                (!executer.IsUrgent && downloadThreadCount < _threadLimit && _curState == State.Running))
                            {
                                _toDownloadDict.Remove(executer.DestPath);
                                _toDownloadList.Remove(executer);
                                _downloading.Add(executer);
                                executer.EnableSpeedLimit(_isSpeedLimited);
                                executer.StartDownload();
#if UNITY_EDITOR //|| DEVELOPMENT_BUILD
                                //Debug.Log("HttpDownloadManager TryStartNewTask executer.StartDownload() executer.DestPath=" + executer.DestPath + " executer.IsUrgent" + executer.IsUrgent);
#endif
                            }
                        }

                        if (_curState == State.Abort && _downloading.Count == 0)
                        {
                            executer = _toDownloadList[_toDownloadList.Count - 1];
                            if (!executer.IsUrgent)
                            {
                                UnRegisterUpdate();
                            }
                        }
                    }
                    else
                    {
                        if (_downloading.Count == 0)
                        {
                            if (_curState == State.Running)
                            {
                                _curState = State.None;
                            }
                            IsDone = true;
                            UnRegisterUpdate();
#if !UNITY_EDITOR
                            //发送下载完成的系统通知
                            if (IsInBackground)
                            {
                                CancelNotification();
                                if (_retryList.Count == 0 && _failCacheList.Count == 0)
                                {
                                    if (!string.IsNullOrEmpty(_sucNotificationStr))
                                    {
                                        //发送下载成功的系统通知
                                        ShowNotification(_sucNotificationStr);
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(_failNotificationStr))
                                    {
                                        //发送下载失败的系统通知
                                        ShowNotification(_failNotificationStr);
                                    }
                                }
                            }
#endif
                        }
                    }
                }
            }
        }

        private int CalcDownloadingThreadCount()
        {
            int result = 0;
            for (int i = 0; i < _downloading.Count; i++)
            {
                result += _downloading[i].IsUrgent ? TaskExecuter.UrgentThread : TaskExecuter.NormalThread;
            }
            return result;
        }

        //直接结束
        public void Abort()
        {
            AttachCurrentThread();
            try
            {
#if DEVELOPMENT_BUILD
                Debug.Log("HttpDownloadManager Abort start _curState=" + _curState + " _downloading.Count=" + _downloading.Count + "_finishedTasksReceived=" + _finishedTasksReceived);
#endif
                if (_curState != State.Abort)
                {
                    CheckDownloadingTaskFinished();
                    _curState = State.Abort;
                    lock (_downloading)
                    {
                        if (_downloading.Count > 0)
                        {
                            //非紧急任务都停止，并放到失败列表等待重试
                            for (int i = _downloading.Count - 1; i >= 0; i--)
                            {
                                TaskExecuter executer = _downloading[i];
                                if (executer.IsUrgent)
                                {
                                    continue;
                                }
                                executer.Abort();
                                Interlocked.Add(ref _finishedTasksReceived, executer.TotalReceived);
                                _retryList.Add(executer);
                                _downloading.Remove(executer);
                            }
                        }
                    }
                }
#if DEVELOPMENT_BUILD
                Debug.Log("HttpDownloadManager Abort end _finishedTasksReceived=" + _finishedTasksReceived);
#endif
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                DetachCurrentThread();
            }
        }

        //清除所有任务
        public void ClearAllTasks()
        {
            AttachCurrentThread();
            try
            {
#if DEVELOPMENT_BUILD
                Debug.Log("HttpDownloadManager ClearAllTasks _toDownloadList.Count=" + _toDownloadList.Count);
#endif
                Abort();

                lock (_downloading)
                {
                    RemoveNormalTask(_failCacheList);
                    RemoveNormalTask(_retryList);
                }
                lock (_toDownloadList)
                {
                    RemoveNormalTask(_toDownloadList);
                }
                Interlocked.Exchange(ref _completeSize, 0);
                Interlocked.Exchange(ref _finishedTasksReceived, 0);
                _realNeedDownloadSize = 0;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                DetachCurrentThread();
            }
        }

        private void RemoveNormalTask(List<TaskExecuter> list)
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                TaskExecuter task = list[i];
                if (!task.IsUrgent)
                {
                    _toDownloadDict.Remove(task.DestPath);
                    list.Remove(task);
                    task.Dispose();
                }
            }
        }

        public void SetThreadLimit(int threadNum)
        {
            if (threadNum > MAX_THREAD_NUM)
            {
                _threadLimit = MAX_THREAD_NUM;
            }
            else
            {
                _threadLimit = threadNum;
            }
        }

        public void SetIsLimitSpeed(bool isLimit)
        {
            AttachCurrentThread();
            try
            {
                _isSpeedLimited = isLimit;

                lock (_downloading)
                {
                    for (int i = 0; i < _downloading.Count; i++)
                    {
                        if (_downloading[i].IsUrgent)
                        {
                            continue;
                        }
                        _downloading[i].EnableSpeedLimit(isLimit);
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                DetachCurrentThread();
            }
        }

        public void SetAllowDownloadInBackground(bool allow)
        {
            AttachCurrentThread();
            try
            {
                if (_allowDownloadInBackground != allow)
                {
                    _allowDownloadInBackground = allow;
                    if (_allowDownloadInBackground)
                    {
                        if (CurState == State.Running)
                        {
                            UnityMobilePlatformKeepAlive.TurnOn(_keepAliveNotificationStr);
                        }
                    }
                    else
                    {
                        UnityMobilePlatformKeepAlive.TurnOff();
                    }
                    lock (_toDownloadList)
                    {
                        for (int i = 0; i < _toDownloadList.Count; i++)
                        {
                            _toDownloadList[i].SetAllowDownloadInBackground(allow);
                        }
                    }

                    lock (_downloading)
                    {
                        for (int i = 0; i < _downloading.Count; i++)
                        {
                            _downloading[i].SetAllowDownloadInBackground(allow);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                DetachCurrentThread();
            }
        }

        /// <summary>
        /// 是否允许移动网络下载，编辑器状态不生效
        /// </summary>
        /// <param name="allow"></param>
        public void SetAllowCarrierDataNetworkDownload(bool allow)
        {
            AttachCurrentThread();
            try
            {
                if (_allowCarrierDataNetworkDownload != allow)
                {
                    _allowCarrierDataNetworkDownload = allow;

                    lock (_toDownloadList)
                    {
                        for (int i = 0; i < _toDownloadList.Count; i++)
                        {
                            _toDownloadList[i].SetAllowCarrierDataNetworkDownload(allow);
                        }
                    }

                    lock (_downloading)
                    {
                        for (int i = 0; i < _downloading.Count; i++)
                        {
                            _downloading[i].SetAllowCarrierDataNetworkDownload(allow);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                DetachCurrentThread();
            }
        }

        public void SetKeepAliveNotificationStr(string str)
        {
            _keepAliveNotificationStr = str;
        }

        private static void AttachCurrentThread()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_mainThreadID != int.MinValue && Thread.CurrentThread.ManagedThreadId != _mainThreadID)
            {
                int count;
                if (!thread2AttachCount.TryGetValue(Thread.CurrentThread.ManagedThreadId, out count) || count <= 0)
                {
                    thread2AttachCount[Thread.CurrentThread.ManagedThreadId] = 1;
                    AndroidJNI.AttachCurrentThread();
                }
                else
                {
                    count++;
                    thread2AttachCount[Thread.CurrentThread.ManagedThreadId] = count;
                }
            }
#endif
        }

        private static void DetachCurrentThread()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_mainThreadID != int.MinValue && Thread.CurrentThread.ManagedThreadId != _mainThreadID)
            {
                int count;
                if (thread2AttachCount.TryGetValue(Thread.CurrentThread.ManagedThreadId, out count))
                {
                    count--;
                    thread2AttachCount[Thread.CurrentThread.ManagedThreadId] = count;
                    if (count <= 0)
                    {
                        int v;
                        thread2AttachCount.TryRemove(Thread.CurrentThread.ManagedThreadId, out v);
                        AndroidJNI.DetachCurrentThread();
                    }
                }
            }
#endif
        }

        public static void SetLimitSpeed(long speed)
        {
            AttachCurrentThread();
            try
            {
                HttpDownloader.SetLimitSpeed(speed);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                DetachCurrentThread();
            }
        }


        /// <summary>
        /// 修改APP保活模块后台音乐音量，0.0f～1.0f，仅iOS平台生效
        /// </summary>
        /// <param name="volum"></param>
        public static void SetKeepAliveMusicVolum(float volum)
        {
            AttachCurrentThread();
            try
            {
                UnityMobilePlatformKeepAlive.SetMusicVolume(volum);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                DetachCurrentThread();
            }
        }
        /// <summary>
		/// 自定义APP保活模块后台播放的音乐，支持多条音乐，使用“;”分隔，仅iOS平台生效
		/// </summary>
		/// <param name="musicClips"></param>
        public static void SetCustomKeepAliveMusicClips(string clips)
        {
            AttachCurrentThread();
            try
            {
                UnityMobilePlatformKeepAlive.SetCustomMusicClips(clips);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                DetachCurrentThread();
            }
        }

        /// <summary>
        /// 设置推送的图标及其颜色。注：仅安卓平台生效
        /// 原因：在国外某些手机上，app的推送的图标会出现纯白色的异常，原因可见以下网址，故增加smallicon和smallicon的rgb设置接口。 https://blog.csdn.net/SImple_a/article/details/103594842?utm_medium=distribute.wap_relevant.none-task-blog-2%7Edefault%7EBlogCommendFromMachineLearnPai2%7Edefault-1.wap_blog_relevant_pic&depth_1-utm_source=distribute.wap_relevant.none-task-blog-2%7Edefault%7EBlogCommendFromMachineLearnPai2%7Edefault-1.wap_blog_relevant_pic
        /// </summary>
        /// <param name="name">图标名</param>
        /// <param name="type">文件夹</param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static void SetNotificationSmallIcon(string name, string type, int r, int g, int b)
        {
            AttachCurrentThread();
            try
            {
                UnityMobilePlatformKeepAlive.SetNotificationSmallIcon(name, type, r, g, b);
                HttpDownloader.SetNotificationSmallIcon(name, type, r, g, b);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                DetachCurrentThread();
            }
        }

        /// <summary>
        /// 设置用于检测网络状态的url，支持多个url，以分号(;)分割,仅用于iOS平台
        /// </summary>
        /// <param name="urls"></param>
        public static void SetNetworkStatusObseverUrls(string urls)
        {
            AttachCurrentThread();
            try
            {
                HttpDownloader.SetNetworkStatusObseverUrls(urls);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                DetachCurrentThread();
            }
        }

        /// <summary>
        /// 检查是否获取通知权限
        /// </summary>
        /// <param name="callback">回调</param>
        public static void TryCheckNotificationPermission(System.Action<bool> callback)
        {
            AttachCurrentThread();
            try
            {
                HttpDownloader.TryCheckNotificationPermission(callback);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                DetachCurrentThread();
            }
        }

        /// <summary>
        /// 跳转到系统的通知权限设置界面
        /// </summary>
        public static void JumpNotificationPermissionSetting()
        {
            AttachCurrentThread();
            try
            {
                HttpDownloader.JumpNotificationPermissionSetting();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                DetachCurrentThread();
            }
        }
        public static int ShowNotification(string str)
        {
            AttachCurrentThread();
            try
            {
                return HttpDownloader.ShowNotification(str);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
                return 0;
            }
            finally
            {
                DetachCurrentThread();
            }
        }

        public static int ShowNotification(int id, string str)
        {
            AttachCurrentThread();
            try
            {
                return HttpDownloader.ShowNotification(id, str);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
                return 0;
            }
            finally
            {
                DetachCurrentThread();
            }
        }

        public static int ShowProgressNotification(string title, string desc, int max, int progress, bool indeterminate)
        {
            AttachCurrentThread();
            try
            {
                return HttpDownloader.ShowProgressNotification(UnityMobilePlatformKeepAlive.GetKeepAliveNotificationId(), title, desc, max, progress, indeterminate);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
                return 0;
            }
            finally
            {
                DetachCurrentThread();
            }
        }

        public static void CancelNotification(int id)
        {
            AttachCurrentThread();
            try
            {
                HttpDownloader.CancelNotification(id);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                DetachCurrentThread();
            }
        }

        public static void CancelNotification()
        {
            AttachCurrentThread();
            try
            {
                CancelNotification(UnityMobilePlatformKeepAlive.GetKeepAliveNotificationId());
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            finally
            {
                DetachCurrentThread();
            }
        }

        public bool TryGetTaskDownloadedBytes(string destPath, out long size)
        {
            return _taskDownloadedBytesDict.TryGetValue(destPath, out size);
        }

        public bool LowConsumptionMode
        {
            get { return _LowConsumptionMode; }
            set { _LowConsumptionMode = value; }
        }
        #endregion
    }
}

