/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using UnityEngine;
using Zeus.Core;
using System.IO;
using Zeus.Core.FileSystem;
using System;
using Zeus.Framework.Http;
using HttpErrorType = Zeus.Framework.Http.UnityHttpDownloader.ErrorType;
using System.Collections.Concurrent;
using System.Threading;
using System.Collections;
using Zeus.Framework.Http.UnityHttpDownloader;
using Zeus.Framework.Http.UnityHttpDownloader.Tool;

namespace Zeus.Framework.Asset
{
    public enum SubpackageError
    {
        /// <summary>
        /// 没有错误
        /// </summary>
        None = 0,
        /// <summary>
        ///暂停
        Pause,
        /// <summary>
        ///没有开启非WIFI环境自动下载时，WIFI切换到运营商网络会回调这个错误
        /// </summary>
        NetworkChange,
        /// <summary>
        /// 下载的分包资源解压或是分割失败。提示用户重试或联系客服。
        /// </summary>
        DecodeError,
        /// <summary>
        /// 下载的分包资源文件MD5值不正确,校验失败。提示用户重试或联系客服。
        /// </summary>
        CheckFail,
        /// <summary>
        /// 下载分包资源文件失败。提示用户检查网络并重试。
        /// </summary>
        DownloadError,
        /// <summary>
        /// 找不到要下载的文件错误。提示用户联系客服。
        /// </summary>
        HttpStatusCode404Error,
        /// <summary>
        /// 磁盘空间不足错误。提示用户清理磁盘空间。
        /// </summary>
        HardDiskFullError,
        /// <summary>
        /// bundlelist中找不到bundle。(这个错误一般不会出现)
        /// </summary>
        MissingError
    }

    public enum SubpackageState
    {
        //空闲阶段，不用处理
        Idle,
        //准备好开始下载
        Ready,
        //下载阶段，提供进度
        Downloading,
        //WIFI切换到运营商网络的自动暂停状态
        WaitLocalAreaNetwork,
        //错误处理阶段
        DownloadingError,
        //错误处理阶段
        SplitingError,
        //暂停
        Pause,
        //文件分割阶段，提供进度
        Spliting,
        //出现错误，根据SubpackageError参数弹窗提示
        Abort,
        //下载并且分割完成
        Complete
    }

    public class DownloadService
    {
#if UNITY_IOS && !UNITY_EDITOR
        private int THREAD_LIMIT = 6;
#else
        private int THREAD_LIMIT = 24;
#endif
        public const string ChunkExtension = ".bundleChunk";
        const string TempFolder = "_DownloadServiceTemp";
        const string ChunkTempExtension = ".tempChunk";
        const string ReadyTagFolderName = "SubpackageReadyTag";
        const string AllTagReadyMarkFile = "AllTagReadyMarkFile";
        //首包补录模式：该模式下会记录缺失的资源（通过远程加载的资源）到固定文件
        const string RecordLackingAssetsMode = "RecordLackingAssetsMode";
        const string LackingAssetListFileName = "LackingAssetList.txt";

        private bool _recordLackingAssetsMode = false;
        private string _lackingAssetsFilePath = null;

        //Tag资源状态（开始下载/完成下载）记录观察者，一般用于统计tag开始下载和完成下载事件
        private Action<string, bool> _tagStatusObserver = null;
        private HashSet<string> _observedTags = null;
        //level
        private string assetLevel = null;
        //二包是否下载完成
        private bool _isSubpacakgeReady = false;

        string ReadyTagFolderPath
        {
            get
            {
                if (_tagFlagPath == null)
                {
                    string folderPath = VFileSystem.GetZeusSettingPath(ReadyTagFolderName);
                    _tagFlagPath = OuterPackage.GetRealPath(folderPath);
                    if (assetLevel != null)
                    {
                        _tagFlagPath += "_" + assetLevel;
                    }
                }
                return _tagFlagPath;
            }
        }

        HttpDownloadManager _downloadManager;

        internal class UrgentCallback
        {
            public Action<string, HttpErrorType> callback;
            public string bundleName;
            public HttpErrorType error;

            public UrgentCallback(Action<string, HttpErrorType> callback, string bundleName, HttpErrorType error)
            {
                this.callback = callback;
                this.bundleName = bundleName;
                this.error = error;
            }
        }

        SubPackageBundleInfoContainer _bundleInfoContainer;
        public SubPackageBundleInfoContainer BundleInfoContainer
        {
            get
            {
                if (_bundleInfoContainer == null)
                {
                    _bundleInfoContainer = SubPackageBundleInfoContainer.LoadSubPackageInfo(assetLevel);
                }
                return _bundleInfoContainer;
            }
        }
        Hashtable _urgentDownloadingBundles;

        internal DownloadService()
        {
            //使用CDN地址检测网络通断
            UpdateNetworkStatusObseverUrls();
            _downloadManager = new HttpDownloadManager("Download successful", "Download Failed");

            _urgentDownloadingBundles = new Hashtable();
            string recordModeFlag = VFileSystem.GetZeusSettingPath(RecordLackingAssetsMode);
            if (VFileSystem.ExistsFile(recordModeFlag))
            {
                _recordLackingAssetsMode = true;
                string assetlistPath = VFileSystem.GetZeusSettingPath(LackingAssetListFileName);
                _lackingAssetsFilePath = OuterPackage.GetRealPath(assetlistPath);
                Debug.Log("Asset enter RecordLackingAssetsMode");
            }
        }

        /// <summary>
        /// 立即下载需要加载的bundle
        /// </summary>
        /// <param name="bundleName">bundle名称</param>
        /// <param name="callback">回调函数</param>
        public void AddUrgentBundleDownloadTask(string bundleName, Action<string, HttpErrorType> callback)
        {
            SubPackageBundleInfo info;
            if (BundleInfoContainer.TryGetBundleInfo(bundleName, out info))
            {
                if (_recordLackingAssetsMode)
                {
                    AddToLackingAssetsFile(bundleName);
                }
                List<string> urlList = new List<string>();
                foreach (string url in AssetManager.AssetSetting.bundleLoaderSetting.remoteURL)
                {
                    urlList.Add(UriUtil.CombineUri(url, info.ChunkFile));
                }
                string outputPath = AssetBundleUtils.GetAssetBundleOuterPackagePath(bundleName);
                long chunkTo = info.ChunkFrom + info.BundleSize - 1;
                if (callback != null)
                {
                    lock (_urgentDownloadingBundles.SyncRoot)
                    {
                        if (!_urgentDownloadingBundles.ContainsKey(bundleName))
                        {
                            _urgentDownloadingBundles.Add(bundleName, callback);
                        }
                        else
                        {
                            Action<string, HttpErrorType> dele = (Action<string, HttpErrorType>)_urgentDownloadingBundles[bundleName];
                            dele += callback;
                            _urgentDownloadingBundles[bundleName] = dele;
                        }
                    }
                    _downloadManager.DownloadUrgent(urlList, info.BundleSize, new CheckAlgorithm(CheckAlgorithmType.Crc32, info.BundleCrc32), outputPath, DownloadUrgentCallback, info.ChunkFrom, chunkTo);
                }
                else
                {
                    _downloadManager.DownloadUrgent(urlList, info.BundleSize, new CheckAlgorithm(CheckAlgorithmType.Crc32, info.BundleCrc32), outputPath, null, info.ChunkFrom, chunkTo);
                }
            }
            else
            {
                Debug.LogError("DownloadService bundle not in subpackage : " + bundleName);
                callback(bundleName, HttpErrorType.Exception);
            }
        }

        private void AddToLackingAssetsFile(string bundleName)
        {
            try
            {
                //Debug.Log("AddToLackingAssetsFile: " + bundleName);
                using (FileStream fileStream = File.Open(_lackingAssetsFilePath, FileMode.Append, FileAccess.Write))
                {
                    Byte[] bytes = new System.Text.UTF8Encoding(true).GetBytes("bundle=" + bundleName + "\n");
                    fileStream.Write(bytes, 0, bytes.Length);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        private bool DownloadUrgentCallback(string bundlePath, HttpErrorType error)
        {
            lock (_urgentDownloadingBundles.SyncRoot)
            {
                var bundleName = Path.GetFileName(bundlePath);
                Action<string, HttpErrorType> callback = (Action<string, HttpErrorType>)_urgentDownloadingBundles[bundleName];
                if (callback == null)
                {
                    Debug.LogError("Can't find bundle \"" + bundleName + "\" in UrgentDownloadingBundles.");
                }
                else
                {
                    foreach (var del in callback.GetInvocationList())
                    {
                        try
                        {
                            del.DynamicInvoke(bundleName, error);
                        }
                        catch
                        {
                            //do nothing
                        }
                    }
                }
                _urgentDownloadingBundles.Remove(bundleName);
            }
            return true;
        }

        #region 下载子包

        volatile SubpackageState _state = SubpackageState.Idle;
        volatile SubpackageError _subpackageError = SubpackageError.None;
        Action<double, double, double, SubpackageState, SubpackageError> OnDownloading;
        List<string> _downloadSequence; //存储下载顺序
        string[] _currentDownloadingTags;
        bool _isDownloadAll = false;
        ConcurrentQueue<string> _chunkSplitTaskQueue;
        ConcurrentDictionary<string, long> _processedChunkDict;
        ConcurrentDictionary<string, HashSet<string>> _tag2ChunkSet;

        long _totalSize = 0;
        long _avgSpeed;
        long _completedChunksSize;
        long _splitingCompletedSize;
        float _time;
        const int SpeedCalcTimeSpan = 3;
        Queue<long> _lastCompletedSizeQueue; //用于计算平均速度,存储前5秒的已下载大小
        bool _isBackgroundDownloading;
        bool _isAutoRetryDownloading = false;//网络问题下载失败后是否自动重试

        int _splitThreadId = -1;
        bool _isTaskAdded = true;
        volatile bool _isRegisterNeeded = true;

        string _tagFlagPath = null;
        const float BGDownloadUpdateTimeSpan = 2;

        int _alreadyFinishPercent = 0;
        private object _percentNotificationsLock = new object();
        private Dictionary<int, PercentNotification> _percentNotificationsDic = new Dictionary<int, PercentNotification>();
        private List<PercentNotification> _percentNotifications = new List<PercentNotification>();

        private class PercentNotification
        {
            public int Percent;
            public string NotificationStr;
        }

        private bool _showBackgroundProgress = false;
        private string _downloadingNotificationStr = null;
        private string _carrierDataNetworkNotificationStr = null;
        private int _lastBackgroundProgress = 0;
        private HttpDownloadManager.State _lastBackgroundState = HttpDownloadManager.State.None;
        private HttpDownloadManager.PauseStateReason _lastBackgroundPauseStateReason = HttpDownloadManager.PauseStateReason.None;

        #region Update

        private float updateTime = 0;
        public void _Update()
        {
            if (_isBackgroundDownloading)
            {
                updateTime += Time.deltaTime;
                if (updateTime < BGDownloadUpdateTimeSpan)
                {
                    return;
                }
                updateTime = 0;
            }

            switch (_state)
            {
                case SubpackageState.Ready:
                    {
                        if (_downloadSequence.Count == 0)
                        {
                            _totalSize = 1;
                            _splitingCompletedSize = 1;
                            FinishDownload();
                            return;
                        }
                        //判断当前网络状态
                        if (_downloadManager.CurState == HttpDownloadManager.State.Pause
                            && _downloadManager.PauseReason == HttpDownloadManager.PauseStateReason.NotAllowCarrierDataNetwork)
                        {
                            SwitchStateToWaitLocalAreaNetwork();
                        }
                        else
                        {
                            _state = SubpackageState.Downloading;
                            if (!_isTaskAdded)
                            {
                                _isTaskAdded = true;
                                AddDownloadTask();
                            }
                            _downloadManager.Start();
                            long completedSize = _downloadManager.CompleteSize;
                            _lastCompletedSizeQueue.Clear();
                            for (int i = 0; i < SpeedCalcTimeSpan; ++i)
                            {
                                _lastCompletedSizeQueue.Enqueue(completedSize);
                            }
                        }
                        break;
                    }
                case SubpackageState.WaitLocalAreaNetwork:
                    {
                        ProcessWaitLocalAreaNetwork();
                        break;
                    }
                case SubpackageState.DownloadingError:
                case SubpackageState.SplitingError:
                    {
                        CheckError();
                        break;
                    }
                case SubpackageState.Downloading:
                    {
                        UpdateDownloadingInfo();
                        break;
                    }
                case SubpackageState.Spliting:
                    {
                        UpdateSplitingInfo();
                        break;
                    }
                case SubpackageState.Complete:
                    {
                        Finish(true);
                        break;
                    }
                case SubpackageState.Abort:
                    {
                        ProcessAbort();
                        break;
                    }
            }
        }

        //只有在state为Downloading时才会Update
        private void ProcessWaitLocalAreaNetwork()
        {
            if (_downloadManager.CurState == HttpDownloadManager.State.Running)
            {
                _state = SubpackageState.Downloading;
            }
        }

        private void ProcessAbort()
        {
            /// Modify by CMM
            /// Auto Retry
            if (/*_isBackgroundDownloading && */_subpackageError == SubpackageError.DownloadError)
            {
                if (_downloadManager.CurState == HttpDownloadManager.State.Pause
                    && _downloadManager.PauseReason == HttpDownloadManager.PauseStateReason.NotAllowCarrierDataNetwork)
                {
                    SwitchStateToWaitLocalAreaNetwork();
                }
                else if (_downloadManager.CurState == HttpDownloadManager.State.Abort)
                {
                    RetryDownload();
                }
            }
        }

        private void SwitchStateToWaitLocalAreaNetwork()
        {
            _state = SubpackageState.WaitLocalAreaNetwork;
            InvokeOnDownloading(GetCompleteSize(), _totalSize, _avgSpeed, _state, SubpackageError.NetworkChange);
        }

        private void CheckError()
        {
            if (_subpackageError != SubpackageError.None)
            {
                Finish(false);
            }
            else
            {
                ///modify by cmm
                Debug.LogWarning("CheckError SubpackageError is None.");
            }
        }

        private void UpdateDownloadingInfo()
        {
            if (_downloadManager.CurState == HttpDownloadManager.State.Pause
                && _downloadManager.PauseReason == HttpDownloadManager.PauseStateReason.NotAllowCarrierDataNetwork)
            {
                SwitchStateToWaitLocalAreaNetwork();
            }
            else
            {
                GetDownloadAvgSpeed();

                InvokeOnDownloading(GetCompleteSize(), _totalSize, _avgSpeed, SubpackageState.Downloading, SubpackageError.None);
                if (_downloadManager.IsDone)
                {
                    FinishDownload();
                }
            }
        }

        private long GetCompleteSize()
        {
            long completeSize = _downloadManager.CompleteSize + _completedChunksSize;
            completeSize = completeSize > _totalSize ? _totalSize : completeSize;
            return completeSize;
        }

        private void UpdateSplitingInfo()
        {
            if (Interlocked.Read(ref _totalSize) != -1)
            {
                GetSplitAvgSpeed();
                long completedSize = Interlocked.Read(ref _splitingCompletedSize);
                long totalSize = Interlocked.Read(ref _totalSize);
                InvokeOnDownloading(completedSize, totalSize, _avgSpeed, SubpackageState.Spliting, SubpackageError.None);
            }
        }

        long _bgLastCompletedSize = 0;
        float timeRatio = 0.5f;

        private void GetDownloadAvgSpeed()
        {
            if (_isBackgroundDownloading)
            {
                _avgSpeed = (long)((_downloadManager.CompleteSize - _bgLastCompletedSize) / BGDownloadUpdateTimeSpan);
                _bgLastCompletedSize = (long)_downloadManager.CompleteSize;
            }
            else
            {
                _time += Time.deltaTime;
                if (_time > timeRatio)
                {
                    int tempSpeedCalcTimeSpan = _lastCompletedSizeQueue.Count;
                    long lastCompletedSize;
                    lastCompletedSize = _lastCompletedSizeQueue.Dequeue();
                    if (_downloadManager.CompleteSize >= lastCompletedSize)
                    {
                        _avgSpeed = (long)((_downloadManager.CompleteSize - lastCompletedSize) / tempSpeedCalcTimeSpan * 1 / timeRatio);
                    }
                    _lastCompletedSizeQueue.Enqueue((long)_downloadManager.CompleteSize);
                    _time = 0;
                }
            }
        }

        private void GetSplitAvgSpeed()
        {
            if (_isBackgroundDownloading)
            {
                _avgSpeed = (long)((_downloadManager.CompleteSize - _bgLastCompletedSize) / BGDownloadUpdateTimeSpan);
                _bgLastCompletedSize = (long)_downloadManager.CompleteSize;
            }
            else
            {
                _time += Time.deltaTime;
                if (_time > 1)
                {
                    int tempSpeedCalcTimeSpan = _lastCompletedSizeQueue.Count;
                    long lastCompletedSize;
                    lastCompletedSize = _lastCompletedSizeQueue.Dequeue();
                    _avgSpeed = (_splitingCompletedSize - lastCompletedSize) / tempSpeedCalcTimeSpan;
                    _lastCompletedSizeQueue.Enqueue(_splitingCompletedSize);
                    _time = 0;
                }
            }
        }

        void Finish(bool suc)
        {
            if (suc)
            {
                _state = SubpackageState.Idle;
#if UNITY_EDITOR
                ZeusCore.Instance.UnRegisterOnApplicationQuit(_OnQuit);
#endif
                ZeusCore.Instance.UnRegisterUpdate(_Update);
                if (!IsSubpackageReady(null) && CheckSubpackageDataIntegrity())
                {
                    MakeSubpackageReadyMarkFile();
                }
                _isRegisterNeeded = true;

                _chunkSplitTaskQueue = null;
                _lastCompletedSizeQueue = null;
                _downloadSequence = null;
                _currentDownloadingTags = null;
                _processedChunkDict = null;
                _tag2ChunkSet = null;
                InvokeOnDownloading(_totalSize, _totalSize, _avgSpeed, SubpackageState.Complete, SubpackageError.None);
                UnRegisterAllOnDownloadingEvent();
                _bundleInfoContainer = null;
                _observedTags = null;
                _filePath2ChunkName.Clear();
                _ChunkName2filePath.Clear();
                ClearPercentNotification();
            }
            else
            {
                if (_state == SubpackageState.SplitingError)
                {
                    long completedSize = Interlocked.Read(ref _splitingCompletedSize);
                    long totalSize = Interlocked.Read(ref _totalSize);
                    InvokeOnDownloading(completedSize, totalSize, _avgSpeed, SubpackageState.Abort, _subpackageError);
                }
                else
                {
                    if (!_isBackgroundDownloading)
                    {
                        InvokeOnDownloading(GetCompleteSize(), _totalSize, _avgSpeed, SubpackageState.Abort, _subpackageError);
                    }
                    else
                    {
                        if (_subpackageError != SubpackageError.DownloadError)
                        {
                            InvokeOnDownloading(GetCompleteSize(), _totalSize, _avgSpeed, SubpackageState.Abort, _subpackageError);
                        }
                    }
                }
                _state = SubpackageState.Abort;
            }
        }
        #endregion

        public void SetLimitSpeed(double limitSpeed)
        {
            _downloadManager.SetIsLimitSpeed(limitSpeed >= 0);
            HttpDownloadManager.SetLimitSpeed((long)limitSpeed);
        }

        public void DownloadSubpackage(int threadLimit,
            Action<double, double, double, SubpackageState, SubpackageError> downloadingProgressCallback,
            double limitSpeed,
            bool isBackgroundDownloading,
            string[] tags,
            bool isDownloadAll)
        {
#if DEVELOPMENT_BUILD
            string tagStr = string.Empty;
            if (tags != null && tags.Length > 0)
            {
                for (int i = 0; i < tags.Length; i++)
                {
                    tagStr += tags[i];
                    if (i < tags.Length - 1)
                    {
                        tagStr += ";";
                    }
                }
            }
            else
            {
                tagStr = "null or empty";
            }
            Debug.Log("DownloadService DownloadSubpackage threadLimit=" + threadLimit + " limitSpeed=" + limitSpeed +
                " isBackgroundDownloading=" + isBackgroundDownloading + " tags=" + tagStr + " isDownloadAll=" + isDownloadAll);
#endif
            _isBackgroundDownloading = isBackgroundDownloading;
            if (downloadingProgressCallback != null)
            {
                UnRegisterAllOnDownloadingEvent();
                RegisterOnDownloadingEvent(downloadingProgressCallback);
            }
            _downloadManager.SetThreadLimit(Mathf.Clamp(threadLimit, 1, THREAD_LIMIT));
            _downloadManager.LowConsumptionMode = threadLimit > 1 ? false : true;
            SetLimitSpeed(limitSpeed);
            _bgLastCompletedSize = _downloadManager.CompleteSize;
            if (!VFileSystem.ExistsDirectory(ReadyTagFolderPath))
            {
                Directory.CreateDirectory(ReadyTagFolderPath);
            }

            switch (_state)
            {
                case SubpackageState.Idle:
                    Prepare(tags, isDownloadAll, true);
                    break;
                case SubpackageState.Downloading:
                    if (IsTagsChanged(tags, isDownloadAll))
                    {
                        _downloadManager.ClearAllTasks();
                        Prepare(tags, isDownloadAll, true);
                    }
                    break;
                case SubpackageState.Pause:
                case SubpackageState.Abort:
                    if (IsTagsChanged(tags, isDownloadAll))
                    {
                        _downloadManager.ClearAllTasks();
                        Prepare(tags, isDownloadAll, true);
                    }
                    else
                    {
                        Prepare(tags, isDownloadAll, false);
                    }
                    break;
                case SubpackageState.DownloadingError:
                case SubpackageState.SplitingError:
                    CheckError();
                    if (IsTagsChanged(tags, isDownloadAll))
                    {
                        _downloadManager.ClearAllTasks();
                        Prepare(tags, isDownloadAll, true);
                    }
                    break;
                case SubpackageState.Complete:
                    Finish(true);
                    _state = SubpackageState.Idle;
                    Prepare(tags, isDownloadAll, true);
                    break;
                case SubpackageState.Ready:
                case SubpackageState.WaitLocalAreaNetwork:
                    if (IsTagsChanged(tags, isDownloadAll))
                    {
                        _downloadManager.ClearAllTasks();
                        Prepare(tags, isDownloadAll, true);
                    }
                    break;
            }
        }

        private bool IsTagsChanged(string[] tags, bool isDownloadAll)
        {
            if (_currentDownloadingTags == null || (!ArrayEqual(_currentDownloadingTags, GetNewDownloadTags(tags, isDownloadAll))))
            {
                return true;
            }
            return false;
        }

        private string[] GetNewDownloadTags(string[] tags, bool isDownloadAll)
        {
            if (tags == null || tags.Length == 0)
            {
                //tags == null时，不管isDownloadAll是true还是false都表示下载全部
                return BundleInfoContainer.TagSequence.ToArray();
            }
            else
            {
                List<string> taglist = new List<string>();
                foreach (var tag in tags)
                {   //检查tag是否合法
                    if (BundleInfoContainer.ContainsTag(tag))
                    {
                        taglist.Add(tag);
                    }
                    else
                    {
                        Debug.LogError("Tag is invalid:" + tag);
                    }
                }
                //tags不为null时, isDownloadAll为true则自动后面补全没有传入的Tag, 为false就不补全，tags传啥下啥
                if (isDownloadAll)
                {
                    for (int i = 0; i < BundleInfoContainer.TagSequence.Count; i++)
                    {
                        if (!taglist.Contains(BundleInfoContainer.TagSequence[i]))
                        {
                            taglist.Add(BundleInfoContainer.TagSequence[i]);
                        }
                    }
                }
                return taglist.ToArray();
            }
        }

        private bool ArrayEqual(string[] array1, string[] array2)
        {
            if (array1.Length != array2.Length)
            {
                return false;
            }
            for (int i = 0; i < array1.Length; ++i)
            {
                if (array1[i] != array2[i])
                {
                    return false;
                }
            }
            return true;
        }

        private void Prepare(string[] tags, bool isDownloadAll, bool tagChanged = true)
        {
            _currentDownloadingTags = GetNewDownloadTags(tags, isDownloadAll);
            _isDownloadAll = isDownloadAll;
            PrepareDownloadParam prepareParam = new PrepareDownloadParam();
            prepareParam.initDownloadList = tagChanged;
            prepareParam.tags = _currentDownloadingTags;
            ThreadPool.QueueUserWorkItem(PrepareForDownloading, prepareParam);
        }

        private class PrepareDownloadParam
        {
            public bool initDownloadList = false;
            public string[] tags;

        }

        private void PrepareForDownloading(System.Object o)
        {
            var param = o as PrepareDownloadParam;

            lock (this)
            {
                try
                {
#if DEVELOPMENT_BUILD
                    string tagStr = string.Empty;
                    if (param.tags != null && param.tags.Length > 0)
                    {
                        for (int i = 0; i < param.tags.Length; i++)
                        {
                            tagStr += param.tags[i];
                            if (i < param.tags.Length - 1)
                            {
                                tagStr += ";";
                            }
                        }
                    }
                    else
                    {
                        tagStr = "null or empty";
                    }
                    Debug.Log("DownloadService PrepareForDownloading  _currentDownloadingTags=" + tagStr + " _state=" + _state);
#endif
                    if (param.tags != _currentDownloadingTags)
                    {
                        Debug.LogWarning("skip tags");
                        return;
                    }
                    Thread.Sleep(1000);
                    if (_isRegisterNeeded)
                    {
                        ZeusCore.Instance.RegisterUpdate(_Update);
#if UNITY_EDITOR
                        ZeusCore.Instance.RegisterOnApplicationQuit(_OnQuit);
#endif
                        _isRegisterNeeded = false;
                    }
                    if (_chunkSplitTaskQueue == null || _state == SubpackageState.Idle)
                    {
                        _chunkSplitTaskQueue = new ConcurrentQueue<string>();
                    }
                    if (_lastCompletedSizeQueue == null)
                    {
                        _lastCompletedSizeQueue = new Queue<long>();
                    }

                    InitVariable();

                    if (param.initDownloadList)
                    {
                        InitDownloadList(param.tags);
                        _isTaskAdded = false;
                    }
                    if (_state == SubpackageState.Idle)
                    {
                        AddLocalChunkSplitTask();
                    }
                    _state = SubpackageState.Ready;
                    if (_splitThreadId < 0)
                    {
                        _splitThreadId = 0;
                        ThreadPool.QueueUserWorkItem(UpdateSplitQueue);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.ToString());
                }
            }
        }

        private void RetryDownload()
        {
            InitVariable();
            _state = SubpackageState.Downloading;
            if (_splitThreadId < 0)
            {
                _splitThreadId = 0;
                ThreadPool.QueueUserWorkItem(UpdateSplitQueue);
            }
            _downloadManager.Start();
            long completedSize = _downloadManager.CompleteSize;
            _lastCompletedSizeQueue.Clear();
            for (int i = 0; i < SpeedCalcTimeSpan; ++i)
            {
                _lastCompletedSizeQueue.Enqueue(completedSize);
            }
        }

        public void Pause()
        {
#if DEVELOPMENT_BUILD
            Debug.Log("DownloadService Pause _state=" + _state);
#endif
            if (_state == SubpackageState.Downloading)
            {
                SwitchStateToPause();
                _downloadManager.Abort();
            }
        }

        private void SwitchStateToPause()
        {
#if DEVELOPMENT_BUILD
            Debug.Log("SwitchStateToPause _state=" + _state);
#endif
            _state = SubpackageState.Pause;
            InvokeOnDownloading(GetCompleteSize(), _totalSize, _avgSpeed, _state, SubpackageError.Pause);
#if DEVELOPMENT_BUILD
            Debug.Log("SwitchStateToPause _state=" + _state);
#endif
        }

        private void InitVariable()
        {
            _subpackageError = SubpackageError.None;
            _avgSpeed = 0;
            Interlocked.Exchange(ref _splitingCompletedSize, 0);
            _time = 0;
            _lastCompletedSizeQueue.Clear();
        }

        private void InitDownloadList(string[] tags)
        {
            string bundlesOutputPath = AssetBundleUtils.GetAssetBundleOuterPackagePath("");
            bundlesOutputPath = PathUtil.FormatPathSeparator(bundlesOutputPath);
            FileUtil.EnsureFolder(Path.Combine(bundlesOutputPath, "Ensure"));
            InitProcessedChunkDict();
            List<string> chunkSequence = BundleInfoContainer.GetChunkSequence(tags);
            _totalSize = BundleInfoContainer.GetChunksSize(chunkSequence);
            _downloadSequence = GetToDownloadChunkList(chunkSequence);
            _tag2ChunkSet = BundleInfoContainer.GetTagChunkSet(tags, _downloadSequence);
            long toDownloadSize = BundleInfoContainer.GetChunksSize(_downloadSequence);
            //初始化下载相关变量
            _completedChunksSize = _totalSize - toDownloadSize;// + GetDownloadedUnitSize(_downloadSequence);
            _alreadyFinishPercent = Mathf.CeilToInt(_completedChunksSize * 1000f / _totalSize);
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            Debug.Log("DownloadService InitDownloadList _completedChunksSize=" + _completedChunksSize +
                " _totalSize=" + _totalSize + " toDownloadSize=" + toDownloadSize);// + " GetDownloadedUnitSize()=" + GetDownloadedUnitSize(_downloadSequence));
#endif
        }

        ///modify by cmm
        private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        public IEnumerator InitProcessedChunkDictAsync()
        {
            if (_processedChunkDict == null)
            {
                if (_processedChunkDict == null)
                {
                    var chunkDict = new ConcurrentDictionary<string, long>();
                    var chunkSequence = BundleInfoContainer.GetChunkSequence();
                    int index = 0;
                    foreach (var chunk in chunkSequence)
                    {
                        if (CheckChunkDataIntegrity(chunk))
                        {
                            var chunkSize = BundleInfoContainer.GetChunksSize(chunk);
                            chunkDict.TryAdd(chunk, chunkSize);
                        }
                        ++index;
                        if (index % 100 == 0)
                        {
                            if (_processedChunkDict != null)
                                yield break;

                            yield return waitForEndOfFrame;
                        }
                    }
                    if (_processedChunkDict == null)
                        _processedChunkDict = chunkDict;
                }
            }
        }

        private void InitProcessedChunkDict()
        {
            if (_processedChunkDict == null)
            {
                lock (this)
                {
                    if (_processedChunkDict == null)
                    {
                        var chunkDict = new ConcurrentDictionary<string, long>();
                        var chunkSequence = BundleInfoContainer.GetChunkSequence();
                        foreach (var chunk in chunkSequence)
                        {
                            if (CheckChunkDataIntegrity(chunk))
                            {
                                var chunkSize = BundleInfoContainer.GetChunksSize(chunk);
                                chunkDict.TryAdd(chunk, chunkSize);
                            }
                        }
                        _processedChunkDict = chunkDict;
                    }
                }

            }
        }

        private List<string> GetToDownloadChunkList(List<string> chunkSequence)
        {
            List<string> toDownloadChunkList = new List<string>();
            HashSet<string> toDownloadChunkSet = new HashSet<string>();

            //从ChunkSequence获取BundleSequence
            List<string> allBundlesSequence = new List<string>();
            if (chunkSequence == null)
            {
                chunkSequence = BundleInfoContainer.GetChunkSequence();
            }
            GetToDownloadBundleChunkSet(chunkSequence, toDownloadChunkSet);
            GetToDownloadOtherChunkSet(chunkSequence, toDownloadChunkSet);

            foreach (string chunk in chunkSequence)
            {
                if (toDownloadChunkSet.Contains(chunk))
                {
                    toDownloadChunkList.Add(chunk);
                }
            }
            return toDownloadChunkList;
        }

        private void GetToDownloadBundleChunkSet(List<string> chunkSequence, HashSet<string> toDownloadChunkListSet, bool containSplitingChunk = true)
        {
            InitProcessedChunkDict();
            //获取已经下载完但还没有分割为的chunk
            HashSet<string> downloadedChunkSet = new HashSet<string>();
            if (containSplitingChunk)
            {
                downloadedChunkSet = GetDownloadedChunkFromChunkDir();
            }
            foreach (var chunk in chunkSequence)
            {
                if (!downloadedChunkSet.Contains(chunk) && !_processedChunkDict.ContainsKey(chunk) && !toDownloadChunkListSet.Contains(chunk))
                {
                    toDownloadChunkListSet.Add(chunk);
                }
            }
        }

        //根据chunkSequence 获取需要下载的chunk列表
        private void GetToDownloadOtherChunkSet(List<string> chunkSequence, HashSet<string> toDownloadChunkListSet)
        {
            InitProcessedChunkDict();
            foreach (string chunk in chunkSequence)
            {
                if (!_processedChunkDict.ContainsKey(chunk) && !toDownloadChunkListSet.Contains(chunk))
                {
                    toDownloadChunkListSet.Add(chunk);
                }
            }
        }

        private HashSet<string> GetDownloadedChunkFromChunkDir()
        {
            HashSet<string> downloadedChunkSet = new HashSet<string>();
            string chunkOutputDirectory = GetTempChunkPath("");
            if (Directory.Exists(chunkOutputDirectory))
            {
                foreach (string file in Directory.GetFiles(chunkOutputDirectory))
                {
                    string fileName = Path.GetFileName(file);
                    //如果该文件是Chunk
                    if (fileName.EndsWith(ChunkExtension))
                    {
                        SubPackageChunkInfo chunkInfo;
                        if (BundleInfoContainer.TryGetChunkInfo(fileName, out chunkInfo))
                        {
                            downloadedChunkSet.Add(fileName);
                        }
                        else
                        {
                            Debug.LogError("Can't find chunk \"" + fileName + "\" in ChunkInfo.");
                        }
                    }
                }
            }
            return downloadedChunkSet;
        }

        /// <summary>
        /// 获取断点续传已经下载部分的大小
        /// </summary>
        private long GetDownloadedUnitSize(List<string> downloadSequence)
        {
            if (downloadSequence.Count <= 0)
            {
                return 0;
            }
            HashSet<string> downloadSet = new HashSet<string>(downloadSequence);
            long downloadedSize = 0;
            foreach (string chunkFile in downloadSequence)
            {
                SubPackageChunkInfo chunkInfo;
                if (BundleInfoContainer.TryGetChunkInfo(chunkFile, out chunkInfo))
                {
                    string outputPath = GetChunkPath(chunkFile);
                    downloadedSize += HttpDownloader.GetAlreadyDownloadSize(outputPath, chunkInfo.FileSize);
                }
                else
                {
                    Debug.LogError("Can't get info of chunk \"" + chunkFile + "\".");
                }
            }
            return downloadedSize;
        }

#if UNITY_EDITOR
        void _OnQuit()
        {
            _downloadManager.Abort();
            _state = SubpackageState.Complete;
            ZeusCore.Instance.UnRegisterOnApplicationQuit(_OnQuit);
            ZeusCore.Instance.UnRegisterUpdate(_Update);
            _isRegisterNeeded = true;
        }
#endif

        //download file name to chunk name
        ConcurrentDictionary<string, string> _filePath2ChunkName = new ConcurrentDictionary<string, string>();
        ConcurrentDictionary<string, string> _ChunkName2filePath = new ConcurrentDictionary<string, string>();
        private void AddDownloadTask()
        {
            _filePath2ChunkName.Clear();
            foreach (string chunkFile in _downloadSequence)
            {
                SubPackageChunkInfo chunkInfo;
                if (BundleInfoContainer.TryGetChunkInfo(chunkFile, out chunkInfo))
                {
                    List<string> urlList = new List<string>();
                    //get download path
                    string outputPath = GetChunkPath(chunkFile);
                    //download path to chunk name
                    _filePath2ChunkName.TryAdd(outputPath, chunkFile);
                    //chunk name to download path
                    _ChunkName2filePath.TryAdd(chunkFile, outputPath);
                    //generate cdn http url
                    foreach (string url in AssetManager.AssetSetting.bundleLoaderSetting.remoteURL)
                    {
                        urlList.Add(UriUtil.CombineUri(url, chunkFile));
                    }
                    //add to download manager
                    _downloadManager.Download(urlList, chunkInfo.FileSize, new CheckAlgorithm(CheckAlgorithmType.Crc32, chunkInfo.Crc32), outputPath, OnDownloadComplete);
                }
                else
                {
                    Debug.LogError("Can't get info of chunk \"" + chunkFile + "\".");
                }
            }
        }

        private string GetChunkPath(string chunkName)
        {
            string outputPath = null;
            if (BundleInfoContainer.Chunk2Bundles.ContainsKey(chunkName))
            {
                //bundle 数量
                if (BundleInfoContainer.Chunk2Bundles[chunkName].Count == 1)
                {
                    string bundleName = BundleInfoContainer.Chunk2Bundles[chunkName][0];
                    outputPath = AssetBundleUtils.GetAssetBundleOuterPackagePath(bundleName);
                }
                else
                {
                    outputPath = GetTempChunkPath(chunkName);
                }
            }
            else if (BundleInfoContainer.TryGetOtherFilePath(chunkName, ref outputPath))
            {
                //get outputPath in TryGetOtherFilePath
            }
            else
            {
                Debug.Assert(false, "chunk file not bundle or others : " + chunkName);
            }

            return outputPath;
        }

        private void AddLocalChunkSplitTask()
        {
            string chunkOutputPath = GetTempChunkPath("");
            if (Directory.Exists(chunkOutputPath))
            {
                foreach (string chunk in Directory.GetFiles(chunkOutputPath, "*" + ChunkExtension))
                {
                    _chunkSplitTaskQueue.Enqueue(Path.GetFileName(chunk));
                }
            }
        }

        private bool OnDownloadComplete(string filePath, HttpErrorType httpError)
        {
            if (httpError == HttpErrorType.None)
            {
                //将需要分割的chunk文件加入解压队列中
                if (filePath.EndsWith(ChunkExtension))
                {
                    _chunkSplitTaskQueue.Enqueue(Path.GetFileName(filePath));
                }
                else
                {
                    string chunkName = null;
                    if (_filePath2ChunkName.TryGetValue(filePath, out chunkName))
                    {
                        MarkChunkDone(chunkName);
                    }
                    else
                    {
                        Debug.LogError(" OnDownloadComplete not found " + filePath);
                    }
                }
                TriggerPercentNotification();
            }
            else
            {
                Debug.LogError(string.Format("Download file \"{0}\" from remote failed. Error: {1}", filePath, httpError));
                if (_state == SubpackageState.DownloadingError || _state == SubpackageState.Abort)
                {
                    return false;
                }
                _state = SubpackageState.DownloadingError;
                _downloadManager.Abort();
                SetSubpackageError(httpError);
            }
            return false;
        }

        private void SetSubpackageError(HttpErrorType httpError)
        {
            switch (httpError)
            {
                case HttpErrorType.HardDiskFull:
                    {
                        SetSubpackageError(SubpackageError.HardDiskFullError);
                        break;
                    }
                case HttpErrorType.MissingFile:
                    {
                        SetSubpackageError(SubpackageError.HttpStatusCode404Error);
                        break;
                    }
                case HttpErrorType.CheckFail:
                    {
                        SetSubpackageError(SubpackageError.CheckFail);
                        break;
                    }
                default:
                    {
                        SetSubpackageError(SubpackageError.DownloadError);
                        break;
                    }
            }
        }

        private void SetSubpackageError(SubpackageError error)
        {
            if (error > _subpackageError)
            {
                _subpackageError = error;
            }
        }

        private void UpdateSplitQueue(System.Object o)
        {
            _splitThreadId = Thread.CurrentThread.ManagedThreadId;
            while (_state != SubpackageState.Complete)
            {
                string chunkName;
                while ((_state == SubpackageState.Downloading || _state == SubpackageState.Spliting || _state == SubpackageState.WaitLocalAreaNetwork)
                    && _chunkSplitTaskQueue.TryDequeue(out chunkName))
                {
                    if (_processedChunkDict.ContainsKey(chunkName))
                    {
                        continue;
                    }
                    SubPackageChunkInfo info;
                    if (BundleInfoContainer.TryGetChunkInfo(chunkName, out info))
                    {
                        //进入spliting状态，重新设置_totalSize, _splitingCompletedSize
                        if (Interlocked.Read(ref _totalSize) == -1)
                        {
                            Interlocked.Exchange(ref _splitingCompletedSize, 0);
                            Interlocked.Exchange(ref _totalSize, GetSplitTotalSize(info));
                        }
                        try
                        {
                            ProcessChunk(chunkName, info.CompressMethod);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e.ToString());
                            //有Chunk处理错误
                            _state = SubpackageState.SplitingError;
                            _downloadManager.Abort();
                            _chunkSplitTaskQueue.Enqueue(chunkName);
                            if (e is IOException)
                            {
                                if (DiskUtils.IsHardDiskFull((IOException)e))
                                {
                                    SetSubpackageError(SubpackageError.HardDiskFullError);
                                }
                                else
                                {
                                    SetSubpackageError(SubpackageError.DecodeError);
                                }
                            }
                            else
                            {
                                SetSubpackageError(SubpackageError.DecodeError);
                            }
                            break;
                        }
                    }
                    else
                    {
                        Debug.LogError("UpdateSplitQueue chunk not found : " + chunkName);
                    }
                }
                if (_chunkSplitTaskQueue.Count == 0 && _state == SubpackageState.Spliting)
                {
                    _splitThreadId = -1;
                    try
                    {
                        string tempPath = GetTempChunkPath("");
                        if (Directory.Exists(tempPath))
                        {
                            Directory.Delete(tempPath, true);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.ToString());
                    }
                    finally
                    {
                        _state = SubpackageState.Complete;
                    }
                    break;
                }

                CheckBackGroundDownload();
                Thread.Sleep((_state == SubpackageState.Downloading || _state == SubpackageState.Spliting) ? 2000 : 8000);
            }
        }

        private long GetSplitTotalSize(SubPackageChunkInfo info)
        {
            long totalSize = 0;
            foreach (string chunk in _chunkSplitTaskQueue)
            {
                SubPackageChunkInfo chunkInfo;
                if (BundleInfoContainer.TryGetChunkInfo(chunk, out chunkInfo))
                {
                    if (chunkInfo.CompressMethod == BundleCompressMethod.None)
                    {
                        totalSize += chunkInfo.FileSize;
                    }
                    else
                    {
                        foreach (string bundle in BundleInfoContainer.Chunk2Bundles[chunk])
                        {
                            SubPackageBundleInfo bundleInfo;
                            if (BundleInfoContainer.TryGetBundleInfo(bundle, out bundleInfo))
                            {
                                totalSize += bundleInfo.BundleSize;
                            }
                            else
                            {
                                Debug.LogError("Can't get info of bundle \"" + bundle + "\".");
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("[GetSplitTotalSize] Can't get chunk info.");
                }
            }
            if (info.CompressMethod == BundleCompressMethod.None)
            {
                totalSize += info.FileSize;
            }
            else
            {
                foreach (string bundle in BundleInfoContainer.Chunk2Bundles[info.FileName])
                {
                    SubPackageBundleInfo bundleInfo;
                    if (BundleInfoContainer.TryGetBundleInfo(bundle, out bundleInfo))
                    {
                        totalSize += bundleInfo.BundleSize;
                    }
                    else
                    {
                        Debug.LogError("Can't get info of bundle \"" + bundle + "\".");
                    }
                }
            }
            if (totalSize == 0)
            {
                totalSize = 1;
            }
            return totalSize;
        }

        private void ProcessChunk(string chunkName, BundleCompressMethod compressMethod)
        {
            switch (compressMethod)
            {
                case BundleCompressMethod.None:
                    {
                        SplitChunk(chunkName);
                        break;
                    }
            }
        }

        private void SplitChunk(string chunkTempFileName)
        {
            string chunkName = chunkTempFileName.Replace(ChunkTempExtension, "");
            string chunkPath = GetTempChunkPath(chunkTempFileName);
            List<string> bundlesName = BundleInfoContainer.Chunk2Bundles[chunkName];
            Debug.Assert(bundlesName.Count != 1, "This chunk doesn't need to be splited.");
            using (FileStream fsChunk = File.OpenRead(chunkPath))
            {
                SubPackageBundleInfo info;
                byte[] buffer = new byte[1024];
                foreach (string bundle in bundlesName)
                {
                    if (_state == SubpackageState.Idle)
                    {
                        return;
                    }
                    string finalBundleName = AssetBundleUtils.GetAssetBundleOuterPackagePath(bundle);
                    string tempBundleName = finalBundleName + ChunkTempExtension;
                    if (BundleInfoContainer.TryGetBundleInfo(bundle, out info))
                    {
                        if (_urgentDownloadingBundles.ContainsKey(finalBundleName) || File.Exists(finalBundleName))
                        {
                            Interlocked.Add(ref _splitingCompletedSize, info.BundleSize);
                            continue;
                        }
                        FileUtil.EnsureFolder(tempBundleName);
                        int times = 0;
                        while (times < 3)
                        {
                            times++;
                            uint curCRC32 = 0;
                            using (FileStream fsBundle = File.Create(tempBundleName))
                            {
                                fsChunk.Seek(info.ChunkFrom, SeekOrigin.Begin);
                                int byteCount = (int)info.BundleSize;
                                while (byteCount > 0)
                                {
                                    var readNum = byteCount > buffer.Length ? buffer.Length : byteCount;
                                    var readCount = fsChunk.Read(buffer, 0, (int)readNum);
                                    Interlocked.Add(ref _splitingCompletedSize, readCount);
                                    if (readCount == 0)
                                    {
                                        Debug.LogError(string.Format("SplitChunk: read bundle \"{0}\" from chunk failed.", bundle));
                                        break;
                                    }
                                    fsBundle.Write(buffer, 0, readCount);
                                    curCRC32 = Crc32Algorithm.Append(curCRC32, buffer, 0, readCount);
                                    byteCount -= readCount;
                                }
                            }
                            if (info.BundleCrc32 == ((int)curCRC32))
                            {
                                try
                                {
                                    File.Move(tempBundleName, finalBundleName);
                                    break;
                                }
                                catch (IOException e)
                                {
                                    Debug.LogError(e.ToString());
                                }
                            }
                            else
                            {
                                Debug.LogError($"Split Bundle({bundle}) From Chunk({chunkName}) Fail," +
                                    $"CalcCRC32:{(int)curCRC32} OriCRC32:{info.BundleCrc32} TryTimes:{times}");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogError(string.Format("SplitChunk: Can't find file \"{0}\" in bundle list.", bundle));
                    }
                }
            }
            FileUtil.DeleteFile(chunkPath);
            MarkChunkDone(chunkName);
        }

        private void MarkChunkDone(string chunkName)
        {
            try
            {
                _processedChunkDict.TryAdd(chunkName, 0);
                _ChunkName2filePath.TryRemove(chunkName, out _);
                if (_processedChunkDict.Count == BundleInfoContainer.GetChunkCount())
                {
                    if (CheckSubpackageDataIntegrity())
                    {
                        Debug.Log("processed chunk enough, CheckSubpackageDataIntegrity success");
                        MakeSubpackageReadyMarkFile();
                    }
                    else
                    {
                        Debug.LogError("processed chunk enough, but CheckSubpackageDataIntegrity failed");
                    }
                }

                var toCheckSet = _tag2ChunkSet;
                if (toCheckSet != null)
                {
                    lock (toCheckSet)
                    {
                        foreach (var pair in toCheckSet)
                        {
                            if (pair.Value.Contains(chunkName))
                            {
                                if (_tagStatusObserver != null)
                                {
                                    if (_observedTags == null)
                                    {
                                        _observedTags = new HashSet<string>();
                                    }
                                    if (!_observedTags.Contains(pair.Key))
                                    {
                                        _observedTags.Add(pair.Key);
                                        if (!ZeusCore.IsApplicationQuit)
                                        {
                                            ZeusCore.Instance.AddMainThreadTask(() => { _tagStatusObserver(pair.Key, false); });
                                        }
                                    }
                                }

                                pair.Value.Remove(chunkName);
                                if (pair.Value.Count == 0)
                                {
                                    if (CheckTagDataIntegrity(pair.Key))
                                    {
                                        MakeSubpackageReadyMarkFile(pair.Key);
                                    }
                                    else
                                    {
                                        Debug.LogError("processed chunk enough, but CheckTagDataIntegrity failed " + pair.Key);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (!ZeusCore.IsApplicationQuit)
                {
                    Debug.LogError(e.ToString());
                }
            }
        }

        private void MakeSubpackageReadyMarkFile(string tag = null)
        {
            if (string.IsNullOrEmpty(tag))
            {
                string allReadyPath = Path.Combine(ReadyTagFolderPath, AllTagReadyMarkFile);
                if (!File.Exists(allReadyPath))
                {
                    File.WriteAllText(allReadyPath, AllTagReadyMarkFile);
                }
            }
            else
            {
                string tagReadyPath = Path.Combine(ReadyTagFolderPath, tag);
                if (!File.Exists(tagReadyPath))
                {
                    File.WriteAllText(tagReadyPath, tag);
                }
                if (_tagStatusObserver != null && !ZeusCore.IsApplicationQuit)
                {
                    ZeusCore.Instance.AddMainThreadTask(() => { _tagStatusObserver(tag, true); });
                }
            }

        }

        private void FinishDownload()
        {
            InvokeOnDownloading(_totalSize, _totalSize, _avgSpeed, _state, SubpackageError.None);
            _lastCompletedSizeQueue.Clear();
            for (int i = 0; i < SpeedCalcTimeSpan; ++i)
            {
                _lastCompletedSizeQueue.Enqueue(0);
            }
            Interlocked.Exchange(ref _bgLastCompletedSize, 0);
            _state = SubpackageState.Spliting;
            Interlocked.Exchange(ref _totalSize, -1);
        }

        private void UnRegisterAllOnDownloadingEvent()
        {
            OnDownloading = null;
        }

        private void RegisterOnDownloadingEvent(Action<double, double, double, SubpackageState, SubpackageError> callback)
        {
            OnDownloading += callback;
        }

        private void UnRegisterOnDownloadingEvent(Action<double, double, double, SubpackageState, SubpackageError> callback)
        {
            OnDownloading -= callback;
        }

        private void InvokeOnDownloading(double completedSize, double totalSize, double avgDownloadSpeed, SubpackageState state, SubpackageError error)
        {
            if (_isAutoRetryDownloading && _subpackageError == SubpackageError.DownloadError)
            {
                //因网络问题下载失败后自动重试则不再向外报告错误
                return;
            }
            OnDownloading?.Invoke(completedSize, totalSize, avgDownloadSpeed, state, error);
        }

        private string GetTempChunkPath(string fileName)
        {
            return OuterPackage.GetRealPath(Path.Combine(TempFolder, fileName));
        }

        private bool CheckTagDataIntegrity(string tag, bool isChecksum = false)
        {
            List<string> chunks;
            if (BundleInfoContainer.TryGetTag2ChunkNames(tag, out chunks))
            {
                foreach (var chunk in chunks)
                {
                    if (!CheckChunkDataIntegrity(chunk, isChecksum))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private bool CheckChunkDataIntegrity(string chunk, bool isChecksum = false)
        {
            string otherFilePath = null;
            List<string> bundles = null;
            if (BundleInfoContainer.Chunk2Bundles.TryGetValue(chunk, out bundles))
            {
                foreach (string bundle in bundles)
                {
                    string bundlePath = AssetBundleUtils.GetAssetBundleOuterPackagePath(bundle);
                    if (!File.Exists(bundlePath))
                    {
                        //Debug.Log("CheckChunkDataIntegrity " + chunk + " " + bundlePath);
                        return false;
                    }
                }
            }
            else if (BundleInfoContainer.TryGetOtherFilePath(chunk, ref otherFilePath))
            {
                if (!File.Exists(otherFilePath))
                {
                    //Debug.Log("CheckChunkDataIntegrity " + chunk + " " + otherFilePath);
                    return false;
                }
            }
            else
            {
                Debug.LogErrorFormat("CheckChunkDataIntegrity chunk {0} is invalid", chunk);
            }
            return true;
        }

        private bool CheckSubpackageDataIntegrity(bool isChecksum = false)
        {
            List<string> chunks = BundleInfoContainer.GetChunkSequence();
            var bundleMap = BundleInfoContainer.GetBundleInfoDic();
            SubPackageBundleInfo bundleInfo = null;
            foreach (var chunk in chunks)
            {
                List<string> bundles;
                if (BundleInfoContainer.Chunk2Bundles.TryGetValue(chunk, out bundles))
                {
                    foreach (string bundle in bundles)
                    {
                        string bundlePath = AssetBundleUtils.GetAssetBundleOuterPackagePath(bundle);
                        if (!File.Exists(bundlePath))
                        {
                            return false;
                        }
                        else if (isChecksum)
                        {
                            if (bundleMap.TryGetValue(bundle, out bundleInfo))
                            {
                                int crc32 = CheckAlgorithmUtil.GetCrc32FromFile(bundlePath);
                                if (crc32 != bundleInfo.BundleCrc32)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return true;
        }

        public void SetCarrierDataNetworkDownloading(bool isAllowed)
        {
            _downloadManager.SetAllowCarrierDataNetworkDownload(isAllowed);
        }

        public void SetAllowDownloadInBackground(bool isAllowed)
        {
            _downloadManager.SetAllowDownloadInBackground(isAllowed);
        }

        public void SetSucNotificationStr(string str)
        {
            _downloadManager.SucNotificationStr = str;
        }

        public void SetFailNotificationStr(string str)
        {
            _downloadManager.FailNotificationStr = str;
        }

        public void SetKeepAliveNotificationStr(string str)
        {
            _downloadManager.SetKeepAliveNotificationStr(str);
        }

        /// <summary>
        /// 修改APP保活模块后台音乐音量，0.0f～1.0f，仅iOS平台生效
        /// </summary>
        /// <param name="volum"></param>
        public static void SetKeepAliveMusicVolum(float volum)
        {
            HttpDownloadManager.SetKeepAliveMusicVolum(volum);
        }
        /// <summary>
        /// 自定义APP保活模块后台播放的音乐，支持多条音乐，使用“;”分隔，仅iOS平台生效
        /// </summary>
        /// <param name="musicClips">StreamingAssets下的完整路径</param>
        public static void SetCustomKeepAliveMusicClips(string clips)
        {
            HttpDownloadManager.SetCustomKeepAliveMusicClips(clips);
        }

        /// <summary>
        /// 设置推送的图标及其颜色。注：仅安卓平台生效
        /// 原因：在国外某些手机上，app的推送的图标会出现纯白色的异常，原因可见以下网址，故增加smallicon和smallicon的rgb设置接口。 https://blog.csdn.net/SImple_a/article/details/103594842?utm_medium=distribute.wap_relevant.none-task-blog-2%7Edefault%7EBlogCommendFromMachineLearnPai2%7Edefault-1.wap_blog_relevant_pic&depth_1-utm_source=distribute.wap_relevant.none-task-blog-2%7Edefault%7EBlogCommendFromMachineLearnPai2%7Edefault-1.wap_blog_relevant_pic
        /// </summary>
        /// <param name="name">图标名</param>
        /// <param name="type">Gradle工程下Res文件夹内的子文件夹名</param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static void SetNotificationSmallIcon(string name, string type, int r, int g, int b)
        {
            HttpDownloadManager.SetNotificationSmallIcon(name, type, r, g, b);
        }

        public Dictionary<string, double> GetTag2SizeDic()
        {
            return BundleInfoContainer.GetTag2Size();
        }

        public bool IsSubpackageReady(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                string tagFlagFile = Path.Combine(ReadyTagFolderPath, AllTagReadyMarkFile);
                return File.Exists(tagFlagFile);
            }
            else
            {
                string tagFlagFile = Path.Combine(ReadyTagFolderPath, tag);
                if (File.Exists(tagFlagFile))
                {
                    return true;
                }
                else
                {
                    List<string> chunks;
                    InitProcessedChunkDict();
                    if (BundleInfoContainer.TryGetTag2ChunkNames(tag, out chunks))
                    {
                        foreach (var chunk in chunks)
                        {
                            if (!_processedChunkDict.ContainsKey(chunk))
                            {
                                return false;
                            }
                        }
                    }
                }
                if (!VFileSystem.ExistsDirectory(ReadyTagFolderPath))
                {
                    Directory.CreateDirectory(ReadyTagFolderPath);
                }
                if (!File.Exists(tagFlagFile))
                {
                    File.Create(tagFlagFile);
                }
                return true;
            }
        }


        public bool IsHardDiskEnough()
        {
            long AvailableSpaceBytes = DiskUtils.CheckAvailableSpaceBytes();
            List<string> downloadSequence = GetToDownloadChunkList(null);
            long requrieSize = BundleInfoContainer.GetChunksSize(downloadSequence);
            requrieSize -= GetDownloadedUnitSize(downloadSequence);
            //因为下载、分割文件的时候会出现临时文件，额外增加100MB空间需求
            requrieSize += 100L * 1024L * 1024L;
            return AvailableSpaceBytes >= requrieSize;
        }

        /// <summary>
        /// 计算某个tag内所有未下载分割完的chunk的尺寸
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public double CalcUnCompleteChunkSizeForTag(string tag)
        {
            long chunksize = 0;
            if (!IsSubpackageReady(tag))
            {
                List<string> chunks;
                if (BundleInfoContainer.TryGetTag2ChunkNames(tag, out chunks))
                {
                    HashSet<string> toDownloadChunkSet = new HashSet<string>();
                    GetToDownloadBundleChunkSet(chunks, toDownloadChunkSet, false);
                    GetToDownloadOtherChunkSet(chunks, toDownloadChunkSet);

                    chunksize = BundleInfoContainer.GetChunksSize(new List<string>(toDownloadChunkSet));
                }
            }
            return chunksize;
        }

        /// <summary>
        /// 计算某个tag需要下载的大小
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public double GetSizeToDownloadOfTag(string tag)
        {
            long totalSize = 0;
            if (!IsSubpackageReady(tag))
            {
                List<string> chunks;
                if (BundleInfoContainer.TryGetTag2ChunkNames(tag, out chunks))
                {
                    InitProcessedChunkDict();
                    foreach (var chunk in chunks)
                    {
                        if (!_processedChunkDict.ContainsKey(chunk))
                        {
                            long chunksize = BundleInfoContainer.GetChunksSize(chunk);
                            totalSize += chunksize;
                            string chunkPath = null;
                            if (_ChunkName2filePath.TryGetValue(chunk, out chunkPath))
                            {
                                long downloadedBytes = 0;
                                if (_downloadManager.TryGetTaskDownloadedBytes(chunkPath, out downloadedBytes))
                                {
                                    totalSize -= downloadedBytes;
                                }
                            }
                        }
                    }
                }
            }
            return totalSize;
        }

        /// <summary>
        /// 计算某个tag内所有未下载分割完的chunk的尺寸
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public double GetTagSize(string tag)
        {
            double result;
            if (GetTag2SizeDic().TryGetValue(tag, out result))
            {
                return result;
            }
            return 0;
        }

        public void GetSubPackageSize(out double totalSize, out double completeSize)
        {
            totalSize = BundleInfoContainer.GetChunkTotalSize();
            var downloadSequence = GetToDownloadChunkList(null);
            long toDownloadSize = BundleInfoContainer.GetChunksSize(downloadSequence);
            //初始化下载相关变量
            completeSize = totalSize - toDownloadSize;
        }

        public void SetTagStatusObserver(Action<string, bool> observer)
        {
            _tagStatusObserver = observer;
        }

        public void AddPercentNotification(int percent, string notificationStr)
        {
            if (percent <= 0 || percent >= 1000)
            {
                Debug.LogError("invalid percent(1-999):" + percent);
                return;
            }
            if (string.IsNullOrEmpty(notificationStr))
            {
                Debug.LogError("invalid notificationStr");
                return;
            }
            lock (_percentNotificationsLock)
            {
                PercentNotification temp;
                if (!_percentNotificationsDic.TryGetValue(percent, out temp))
                {
                    temp = new PercentNotification();
                    temp.Percent = percent;
                    _percentNotificationsDic[percent] = temp;
                    _percentNotifications.Add(temp);
                    //由大到小排列
                    _percentNotifications.Sort((a, b) => { return -a.Percent.CompareTo(b.Percent); });
                }
                temp.NotificationStr = notificationStr;
            }
        }

        private void TriggerPercentNotification()
        {
#if !UNITY_EDITOR
            if (_downloadManager.IsInBackground)
            {
                lock (_percentNotificationsLock)
                {
                    if (_percentNotifications.Count > 0)
                    {
                        int curPercent = Mathf.FloorToInt(GetCompleteSize() * 1000f / _totalSize);
                        PercentNotification notification = null;
                        for (int i = _percentNotifications.Count - 1; i >= 0; i--)
                        {
                            if (_percentNotifications[i].Percent <= _alreadyFinishPercent)
                            {
                                _percentNotificationsDic.Remove(_percentNotifications[i].Percent);
                                _percentNotifications.Remove(_percentNotifications[i]);
                            }
                            else if (_percentNotifications[i].Percent <= curPercent)
                            {
                                //如果一次完成了多个进度推送，只显示进度值最大的一个
                                if (notification == null || _percentNotifications[i].Percent > notification.Percent)
                                {
                                    notification = _percentNotifications[i];
                                }
                                _percentNotificationsDic.Remove(_percentNotifications[i].Percent);
                                _percentNotifications.Remove(_percentNotifications[i]);
                            }
                        }
                        if (notification != null)
                        {
                            HttpDownloader.ShowNotification(notification.NotificationStr);
                        }
                    }
                }
            }
#endif
        }

        public void ClearPercentNotification()
        {
            lock (_percentNotificationsLock)
            {
                _percentNotificationsDic.Clear();
                _percentNotifications.Clear();
            }
        }

        public static int ShowProgressNotification(string title, string desc, int max, int progress, bool indeterminate)
        {
            return HttpDownloadManager.ShowProgressNotification(title, desc, max, progress, indeterminate);
        }

        public void SetShowBackgroundDownloadProgress(bool show, string downloadingNotificationStr = null, string carrierDataNetworkNotificationStr = null)
        {
            _showBackgroundProgress = show;
            if (_showBackgroundProgress)
            {
                _downloadingNotificationStr = downloadingNotificationStr;
                _carrierDataNetworkNotificationStr = carrierDataNetworkNotificationStr;
            }
            else
            {
                _downloadingNotificationStr = null;
                _carrierDataNetworkNotificationStr = null;
            }
        }

        private void CheckBackGroundDownload()
        {
            try
            {
                if (_showBackgroundProgress && !_downloadManager.IsDone && _downloadManager.IsInBackground)
                {
                    long completedSize = GetCompleteSize();
                    if (completedSize < _totalSize)
                    {
                        string title = string.Empty;
                        switch (_downloadManager.CurState)
                        {
                            case HttpDownloadManager.State.Running:
                                title = _downloadingNotificationStr;
                                break;
                            case HttpDownloadManager.State.Pause:
                                if (_downloadManager.PauseReason == HttpDownloadManager.PauseStateReason.NotAllowCarrierDataNetwork)
                                {
                                    title = _carrierDataNetworkNotificationStr;
                                }
                                break;
                            case HttpDownloadManager.State.Abort:
                                if (_lastBackgroundState != HttpDownloadManager.State.Abort)
                                {
                                    _lastBackgroundState = _downloadManager.CurState;
                                    _lastBackgroundPauseStateReason = _downloadManager.PauseReason;
                                    HttpDownloadManager.CancelNotification();
                                }
                                break;
                        }
                        if (!string.IsNullOrEmpty(title))
                        {
                            int progress = Mathf.Min(Mathf.Max(Mathf.CeilToInt(completedSize * 10000f / _totalSize), 0), 10000);
                            if (_lastBackgroundProgress != progress ||
                                _lastBackgroundState != _downloadManager.CurState ||
                                _lastBackgroundPauseStateReason != _downloadManager.PauseReason)
                            {
                                ShowProgressNotification(title, null, 10000, progress, false);
                                _lastBackgroundProgress = progress;
                                _lastBackgroundState = _downloadManager.CurState;
                                _lastBackgroundPauseStateReason = _downloadManager.PauseReason;
                            }
                        }
                    }
                    else
                    {
                        if (_lastBackgroundProgress != 0 ||
                                _lastBackgroundState != HttpDownloadManager.State.None ||
                                _lastBackgroundPauseStateReason != HttpDownloadManager.PauseStateReason.None)
                        {
                            _lastBackgroundProgress = 0;
                            _lastBackgroundState = HttpDownloadManager.State.None;
                            _lastBackgroundPauseStateReason = HttpDownloadManager.PauseStateReason.None;
                            HttpDownloadManager.CancelNotification();
                        }
                    }
                }
                else
                {
                    if (_lastBackgroundProgress != 0 ||
                            _lastBackgroundState != HttpDownloadManager.State.None ||
                            _lastBackgroundPauseStateReason != HttpDownloadManager.PauseStateReason.None)
                    {
                        _lastBackgroundProgress = 0;
                        _lastBackgroundState = HttpDownloadManager.State.None;
                        _lastBackgroundPauseStateReason = HttpDownloadManager.PauseStateReason.None;
                        HttpDownloadManager.CancelNotification();
                    }
                }

            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
        }

        public void UpdateNetworkStatusObseverUrls()
        {
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                string observerAddress = "";
                foreach (string url in AssetManager.AssetSetting.bundleLoaderSetting.remoteURL)
                {
                    Uri uri = new Uri(url);
                    if (string.IsNullOrEmpty(observerAddress))
                    {
                        observerAddress = uri.Host;
                    }
                    else
                    {
                        observerAddress += ";" + uri.Host;
                    }
                }
                Debug.Log("-->update observer : " + observerAddress);
                HttpDownloadManager.SetNetworkStatusObseverUrls(observerAddress);
            }
        }

        public void SetSubpackageLevel(string level)
        {
            assetLevel = level;
        }

        public void SetIsAutoRetryDownloading(bool value)
        {
            _isAutoRetryDownloading = value;
        }

        public bool GetIsAutoRetryDownloading()
        {
            return _isAutoRetryDownloading;
        }
    }

    #endregion
}