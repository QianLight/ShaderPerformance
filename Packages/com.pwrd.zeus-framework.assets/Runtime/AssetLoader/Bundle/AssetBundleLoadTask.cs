/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;
using System.Collections.Generic;
using System;
using Zeus.Framework.Http.UnityHttpDownloader;
using Zeus.Core.FileSystem;

namespace Zeus.Framework.Asset
{
    class AssetBundleLoadTask
    {
        private string _bundleName;
        private List<AssetBundleLoadTask> _depLoadTask;
        private AssetBundleCreateRequest _createRequest;
        private bool _isDone;
        private bool _bundleIsDone;
        private bool _checkDoneByAssetBundle;
        private Dictionary<string, AssetBundleLoadTask> _taskDict = new Dictionary<string, AssetBundleLoadTask>();
        private AssetLoadErrorType _loadErrorType;
        private string _errorMessage = null;
        private System.DateTime _beginTime;
        private float _druation;
        private string _assetPath;
        private int _bundleSize;
        private Action<string, string, int, float, string> _remoteLoadObserver;
        private bool _notifiedToRemoteLoadObserver = false;
        private volatile bool _isRemoteLoading = false;
        private static HashSet<string> _checkedSet = new HashSet<string>();

        #region DownloadBundle

        private enum LoadStep
        {
            WaitingBundle,
            WaitToCreateBundle,
            LoadingBundle,
            Done,
        }
        private volatile LoadStep _loadStep = LoadStep.LoadingBundle;
        DownloadService _downloadService;

        #endregion
        public AssetBundleLoadTask(string bundleName, DownloadService downloadService, BundleAssetLoadTask.LoadPriority priority, string assetPath, Action<string, string, int, float, string> observer)
        {
            _taskDict = new Dictionary<string, AssetBundleLoadTask>();
            _depLoadTask = new List<AssetBundleLoadTask>();
            Init(bundleName, downloadService, priority, assetPath, observer);
        }

        public void Init(string bundleName, DownloadService downloadService, BundleAssetLoadTask.LoadPriority priority, string assetPath, Action<string, string, int, float, string> observer)
        {
            AssetsLogger.Log("AssetBundleLoadTask", bundleName);
            _beginTime = System.DateTime.Now;
            _bundleName = bundleName;
            _isDone = false;
            _bundleIsDone = false;
            _checkDoneByAssetBundle = false;
            _loadErrorType = AssetLoadErrorType.None;
            _assetPath = assetPath;
            string abPath;
            bool isBundleExisted = AssetBundleUtils.TryGetAssetBundlePath(bundleName, out abPath);
            if (!isBundleExisted && downloadService == null)
            {
                Debug.LogError("AssetBundleLoadTask bundle not exist: " + bundleName);
                _loadStep = LoadStep.Done;
                _isDone = true;
            }
            else if (!isBundleExisted && downloadService != null)
            {
                _downloadService = downloadService;
                _loadStep = LoadStep.WaitingBundle;
                _remoteLoadObserver = observer;
                _downloadService.AddUrgentBundleDownloadTask(bundleName, FinishDownload);
                _isRemoteLoading = true;
                Debug.LogError(string.Format("[zeus asset] async load {0}, Try to download Bundle: {1}", _assetPath, abPath));
            }
            else
            {
                if (string.IsNullOrEmpty(abPath))
                {
                    throw new Exception("AssetBundleLoader:No Exits \"" + bundleName + "\"");
                }
                _loadStep = LoadStep.LoadingBundle;
                string realPath = null;
                ulong offset = 0;
                if (InnerPackage.TryGetFileEntry(abPath, out realPath, out offset))
                {
                    _createRequest = Encrypt.EncryptAssetBundle.LoadABOffsetAsync(realPath, 0, offset);
                }
                else
                {
                    _createRequest = Encrypt.EncryptAssetBundle.LoadABOffsetAsync(abPath);
                }
                if (_createRequest != null)
                {
                    _createRequest.priority = (int)priority;
                }
                else
                {
                    Debug.LogError("AssetBundleLoadTask AssetBundle.LoadFromFileAsync failed! " + bundleName);
                }
            }
        }

        public void ReSet()
        {
            _bundleName = null;
            _depLoadTask.Clear();
            _createRequest = null;
            _isDone = false;
            _bundleIsDone = false;
            _checkDoneByAssetBundle = false;
            _taskDict.Clear();
            _loadErrorType = AssetLoadErrorType.None;
            _errorMessage = null;
            _druation = 0.0f;
            _assetPath = null;
            _bundleSize = 0;
            _remoteLoadObserver = null;
            _isRemoteLoading = false;
            _loadStep = LoadStep.LoadingBundle;
            _downloadService = null;
        }

        public string BundleName { get { return _bundleName; } }


        public void AddDepBundleTask(AssetBundleLoadTask depTask)
        {
            _depLoadTask.Add(depTask);
        }

        public void SetCheckDoneByAssetBundle()
        {
            _checkDoneByAssetBundle = true;
        }

        public bool IsDone()
        {
            try
            {
                if (_isDone)
                {
                    return true;
                }
                else
                {
                    if (!CheckSelfIsDone(_checkDoneByAssetBundle))
                    {
                        return false;
                    }
                    _taskDict.Clear();
                    GetDepBundleTask(_taskDict);
                    foreach (var pair in _taskDict)
                    {
                        if (!pair.Value.CheckSelfIsDone(_checkDoneByAssetBundle))
                        {
                            return false;
                        }
                    }
                    _taskDict.Clear();
                    _isDone = true;
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError("AssetBundleLoadTask IsDone " + _bundleName);
                Debug.LogException(ex);
                return true;
            }
        }

        public void GetDepBundleTask(Dictionary<string, AssetBundleLoadTask> taskDict)
        {
            //check depend bundle
            for (int i = 0; i < _depLoadTask.Count; i++)
            {
                if (!taskDict.ContainsKey(_depLoadTask[i].BundleName))
                {
                    taskDict.Add(_depLoadTask[i].BundleName, _depLoadTask[i]);
                    _depLoadTask[i].GetDepBundleTask(taskDict);
                }
            }
        }

        private bool CheckSelfIsDone(bool isCheckAssetBundle = false)
        {
            if (_isDone) return _isDone;


            //check self bundle
            if (!_bundleIsDone)
            {
                if (_loadStep == LoadStep.WaitingBundle)
                {
                    return false;
                }
                else if (_loadStep == LoadStep.WaitToCreateBundle)
                {
                    _createRequest = Encrypt.EncryptAssetBundle.LoadABOffsetAsync(AssetBundleUtils.GetAssetBundlePath(BundleName));
                    _loadStep = LoadStep.LoadingBundle;
                    return false;
                }
                else if (isCheckAssetBundle)
                {
                    //本地加载
                    if (_createRequest != null && _createRequest.assetBundle == null)
                    {
                        return false;
                    }
                }
                else if (_createRequest != null && !_createRequest.isDone)
                {
                    return false;
                }
#if ZEUS_ASSETS_PROFILER_LOG
                var timespan = System.DateTime.Now - _beginTime;
                Debug.Log("AssetBundleLoadTask load bundle " + _bundleName + " cost:" + timespan.TotalSeconds);
#endif
                _bundleIsDone = true;
            }
            if (_remoteLoadObserver != null && !_notifiedToRemoteLoadObserver)
            {
                _notifiedToRemoteLoadObserver = true;
                Zeus.Core.ZeusCore.Instance.AddMainThreadTask(this.NotifyRemoteLoadObserver);
            }
                
            return true;
        }

        private void NotifyRemoteLoadObserver()
        {
            if (_remoteLoadObserver != null)
            {
                try
                {
                    _remoteLoadObserver.Invoke(_assetPath, _bundleName, _bundleSize, _druation, _errorMessage);
                }
                catch
                {
                    //do nothing
                }
                _remoteLoadObserver = null;
            }
        }

        private bool CheckIsDone(bool isCheckAssetBundle = false)
        {
            if (_isDone) return _isDone;
            //check self bundle
            if (!_bundleIsDone)
            {
                if (isCheckAssetBundle)
                {
                    if (_createRequest.assetBundle == null)
                    {
                        return false;
                    }
                }
                else if (!_createRequest.isDone)
                {
                    return false;
                }
                _bundleIsDone = true;
            }

            //check depend bundle
            for (int i = 0; i < _depLoadTask.Count; i++)
            {
                if (!_depLoadTask[i].CheckIsDone(isCheckAssetBundle))
                {
                    return false;
                }
            }
            _isDone = true;
            return _isDone;
        }

        public AssetBundle GetAssetBundle()
        {
            if (_createRequest == null)
            {
                return null;
            }
            return _createRequest.assetBundle;
        }

        public float LoadProgress()
        {
            _taskDict.Clear();
            GetDepBundleTask(_taskDict);
            int totalCount = _taskDict.Count + 1;
            float progress = 0;
            progress = this.RawProgress() / totalCount;

            foreach (var pair in _taskDict)
            {
                progress += pair.Value.RawProgress() / totalCount;
            }
            _taskDict.Clear();
            return progress;
        }

        public AssetLoadErrorType LoadErrorType { get { return _loadErrorType; } }

        public bool IsRemoteLoading(HashSet<string> checkedSet = null)
        {
            if(checkedSet == null)
            {
                _checkedSet.Clear();
                checkedSet = _checkedSet;
            }
            if (!checkedSet.Add(BundleName))
                return false;

            if (_isRemoteLoading)
                return true;

            for (int i = 0; i < _depLoadTask.Count; i++)
            {
                if (_depLoadTask[i].IsRemoteLoading(checkedSet))
                {
                    return true;
                }
            }
            
            return false;

        }

        public string ErrorMessage { get { return _errorMessage; } }

        protected float RawProgress()
        {
            if (_createRequest != null)
            {
                return _createRequest.progress;
            }
            else
            {
                return 0.0f;
            }
        }

        private void FinishDownload(string bundleName, ErrorType error)
        {
            try
            {
                _isRemoteLoading = false;
                SubPackageBundleInfo bundleInfo = null;
                if (_downloadService.BundleInfoContainer.TryGetBundleInfo(BundleName, out bundleInfo))
                {
                    _bundleSize = (int)bundleInfo.BundleSize;
                }
                _druation = (float)(System.DateTime.Now - _beginTime).TotalSeconds;
                if (error == ErrorType.None)
                {
                    _loadStep = LoadStep.WaitToCreateBundle;
                }
                else
                {
                    _bundleIsDone = true;
                    _loadStep = LoadStep.Done;

                    _loadErrorType = (AssetLoadErrorType)error;
                    if (bundleInfo != null)
                    {
                        _errorMessage = string.Format("{0} in Chunk:{1} download failed. HttpError: {2}", BundleName, bundleInfo.ChunkFile, error.ToString());
                        Debug.LogError(_errorMessage);
                    }
                    else
                    {
                        _errorMessage = string.Format("{0} in download failed, because it not in subpackage", BundleName);
                        Debug.LogError(_errorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
    }
}