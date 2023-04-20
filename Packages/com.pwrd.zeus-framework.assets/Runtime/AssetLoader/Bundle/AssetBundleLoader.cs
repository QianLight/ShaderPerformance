/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Zeus.Core;
using UnityEngine.SceneManagement;
using Zeus.Core.FileSystem;
using Zeus.Framework.Http;
using System.Collections.Concurrent;
using HttpErrorType = Zeus.Framework.Http.UnityHttpDownloader.ErrorType;

namespace Zeus.Framework.Asset
{
    class AssetBundleLoader : IResourceLoader, ISubPackageLoader
    {
        //assetbundle cache dictionary
        private Dictionary<string, BundleRef> _cachedBundleDict;
        private Dictionary<string, BundleAssetRef> _cachedAssetDict;

        private Dictionary<string, BundleRef> _loadingBundleDict;
        private Dictionary<string, BundleAssetRef> _loadingAssetDict;
        private Dictionary<string, BundleAssetLoadTask> _assetLoadTaskDict;
        private Dictionary<string, AssetBundleLoadTask> _bundleLoadTaskDict;
        private Queue<AssetBundleLoadTask> _loadTaskCache;
        private List<AssetBundleLoadTask> _completedloadTaskList;

        private Dictionary<string, SceneLoadTaskBase> _sceneLoadTaskDict;
        private Dictionary<string, BundleRef> _loadedSceneBundleDict;

        private IBundleCollector _bundleGcStrategy;
        private AssetBundleLoaderSetting _setting;
        private DownloadService _downloadService;
        private ErrorData _errorData;
        private Action<AssetLoadErrorType, string> _errorObserver;
        private Action<string, string, int, float, string> _remoteLoadObserver;
        private Queue<SceneCommandBase> _sceneCommandQueue;
        private ConcurrentQueue<string> _cancelDelayReleaseBundleQueue;
        private HashSet<string> _bundleBlacklist;
        private string _level = null;

        public AssetBundleLoader(AssetBundleLoaderSetting setting)
        {
            _errorData = new ErrorData();
            _setting = setting;
            _setting.LoadPlayerPrefs();
            _bundleGcStrategy = new DefaultBundleCollector(setting.defaultBundleCollectorSetting);

            _cachedBundleDict = new Dictionary<string, BundleRef>(512);
            _cachedAssetDict = new Dictionary<string, BundleAssetRef>(1024);

            _loadingBundleDict = new Dictionary<string, BundleRef>(128);
            _loadingAssetDict = new Dictionary<string, BundleAssetRef>(256);

            _assetLoadTaskDict = new Dictionary<string, BundleAssetLoadTask>(256);
            _bundleLoadTaskDict = new Dictionary<string, AssetBundleLoadTask>(128);

            _sceneLoadTaskDict = new Dictionary<string, SceneLoadTaskBase>(2);
            _loadedSceneBundleDict = new Dictionary<string, BundleRef>(2);
            _cancelDelayReleaseBundleQueue = new ConcurrentQueue<string>();

            _sceneCommandQueue = new Queue<SceneCommandBase>();
            _loadTaskCache = new Queue<AssetBundleLoadTask>(256);
            _completedloadTaskList = new List<AssetBundleLoadTask>(256);
            if (_setting.useSubPackage)
            {
                _downloadService = new DownloadService();
                _downloadService.SetCarrierDataNetworkDownloading(_setting.isCarrierDataNetworkDownloadAllowed);
                _downloadService.SetAllowDownloadInBackground(_setting.isBackgroundDownloadAllowed);
                _downloadService.SetIsAutoRetryDownloading(_setting.isAutoRetryDownloading);
            }
            _bundleBlacklist = new HashSet<string>();
            ZeusCore.Instance.RegisterUpdate(this.Update);
        }

        #region Subpackage

        public void DownloadSubpackage(int maxDownloadingTask,
            Action<double, double, double, SubpackageState, SubpackageError> downloadingProgressCallback,
            double limitSpeed,
            bool isBackgroundDownloading,
            string[] tags,
            bool isDownloadAll)
        {
            if (_downloadService == null)
            {
                Debug.LogWarning("The setting \"useSubpackage\" is false, please check it.");
                return;
            }
            _downloadService.DownloadSubpackage(maxDownloadingTask, downloadingProgressCallback, limitSpeed, isBackgroundDownloading, tags, isDownloadAll);
        }

        public void PauseDownloading()
        {
            if (_downloadService == null)
            {
                Debug.LogWarning("The setting \"useSubpackage\" is false, please check it.");
                return;
            }
            _downloadService.Pause();
        }

        public void SetAllowDownloadInBackground(bool isAllowed)
        {
            if (isAllowed && AssetManager.AssetSetting != null && !AssetManager.AssetSetting.bundleLoaderSetting.isSupportBackgroundDownload)
            {
                isAllowed = false;
                Debug.LogError("Cur APP Don't Support Background Download, You Can't Set AllowDownloadInBackground，" +
                    "Otherwise You Can Set isSupportBackgroundDownload Yes In Build Window Or Use Command Line Arg：SUPPORT_BACKGROUND_DOWNLOAD Set isSupportBackgroundDownload Yes");
            }
            AssetBundleLoaderSetting.SetAllowDownloadInBackground(isAllowed);
            if (_downloadService == null)
            {
                Debug.LogWarning("The setting \"useSubpackage\" is false, please check it.");
                return;
            }
            _downloadService.SetAllowDownloadInBackground(isAllowed);
        }

        public void SetSucNotificationStr(string str)
        {
            if (_downloadService == null)
            {
                Debug.LogWarning("The setting \"useSubpackage\" is false, please check it.");
                return;
            }
            _downloadService.SetSucNotificationStr(str);
        }

        public void SetFailNotificationStr(string str)
        {
            if (_downloadService == null)
            {
                Debug.LogWarning("The setting \"useSubpackage\" is false, please check it.");
                return;
            }
            _downloadService.SetFailNotificationStr(str);
        }

        public void SetKeepAliveNotificationStr(string str)
        {
            if (_downloadService == null)
            {
                Debug.LogWarning("The setting \"useSubpackage\" is false, please check it.");
                return;
            }
            _downloadService.SetKeepAliveNotificationStr(str);
        }

        public void SetShowBackgroundDownloadProgress(bool show, string downloadingNotificationStr = null, string carrierDataNetworkNotificationStr = null)
        {
            if (_downloadService == null)
            {
                Debug.LogWarning("The setting \"useSubpackage\" is false, please check it.");
                return;
            }
            _downloadService.SetShowBackgroundDownloadProgress(show, downloadingNotificationStr, carrierDataNetworkNotificationStr);
        }

        public void SetCarrierDataNetworkDownloading(bool isAllowed)
        {
            AssetBundleLoaderSetting.SetCarrierDataNetworkDownloading(isAllowed);
            if (_downloadService == null)
            {
                Debug.LogWarning("The setting \"useSubpackage\" is false, please check it.");
                return;
            }
            _downloadService.SetCarrierDataNetworkDownloading(isAllowed);
        }

        public bool GetCarrierDataNetworkDownloadingAllowed()
        {
            if (AssetManager.AssetSetting != null)
            {
                return AssetManager.AssetSetting.bundleLoaderSetting.isCarrierDataNetworkDownloadAllowed;
            }
            return AssetBundleLoaderSetting.GetCarrierDataNetworkDownloadingAllowed();
        }

        public bool IsBackgroundDownloadAllowed()
        {
            if (AssetManager.AssetSetting != null)
            {
                if (!AssetManager.AssetSetting.bundleLoaderSetting.isSupportBackgroundDownload)
                {
                    return false;
                }
                return AssetManager.AssetSetting.bundleLoaderSetting.isBackgroundDownloadAllowed;
            }
            return AssetBundleLoaderSetting.IsBackgroundDownloadAllowed();
        }

        /// <summary>
        /// 修改二包资源下载Url
        /// </summary>
        /// <param name="hosts">CDN域名，多个域名用';'分割</param>
        public void ReplaceCdnUrlHost(string hosts)
        {
            if (string.IsNullOrEmpty(hosts))
            {
                throw new ArgumentNullException();
            }
            if (_setting.remoteURL.Count < 1 || string.IsNullOrEmpty(_setting.remoteURL[0]))
            {
                throw new Exception("The remote Url list is empty! Please provide at least one subpackage url address while building.");
            }
            string oldUrl = _setting.remoteURL[0];//oldUrl示例：https://zsyjoss1.wmupd.com/subpackage/10/newandroid_cdn_fenbao/subpackage
            int index = oldUrl.IndexOf("subpackage");
            if (index < 0)
            {
                throw new Exception("The first old remote Url list doesn't contain \"subpackage\".");
            }
            string relativeAdr = oldUrl.Substring(index);
            string[] hostArray = hosts.Split(';');
            for (int i = 0; i < hostArray.Length; ++i)
            {
                hostArray[i] = UriUtil.CombineUri(hostArray[i], relativeAdr);
            }
            _setting.remoteURL = new List<string>(hostArray);
            _downloadService.UpdateNetworkStatusObseverUrls();
        }

        /// <summary>
        /// 修改二包资源下载Url
        /// </summary>
        /// <param name="urlsStr">只传入CDN地址，用';'分割多个Url</param>
        public void SetCdnUrl(string urlsStr)
        {
            if (string.IsNullOrEmpty(urlsStr))
            {
                throw new ArgumentNullException();
            }
            string[] urls = urlsStr.Split(';');
            _setting.remoteURL = new List<string>(urls);
            _downloadService.UpdateNetworkStatusObseverUrls();
        }

        /// <summary>
        /// 修改二包下载Url
        /// </summary>
        /// <param name="urls">传入多个完整的分包下载地址</param>
        public void SetCdnUrl(string[] urls)
        {
            if (urls == null || urls.Length < 0)
            {
                Debug.LogError("Urls cannot be null or empty.");
                return;
            }
#if DEVELOPMENT_BUILD
            foreach (string url in urls)
            {
                Debug.Log("URL: " + url);
            }
#endif

            _setting.remoteURL = new List<string>(urls);
            _downloadService.UpdateNetworkStatusObseverUrls();
        }

        public void SetLimitSpeed(double limitSpeed)
        {
            if (_downloadService == null)
            {
                Debug.LogWarning("The setting \"useSubpackage\" is false, please check it.");
                return;
            }
            _downloadService.SetLimitSpeed(limitSpeed);
        }

        public Dictionary<string, double> GetTag2SizeDic()
        {
            if (_downloadService == null)
            {
                Debug.LogWarning("The setting \"useSubpackage\" is false, please check it.");
                return null;
            }
            return _downloadService.GetTag2SizeDic();
        }

        ///modify by cmm
        public void InitProcessedChunkDictAsync()
        {
            if (_downloadService == null)
            {
                return;
            }
            ZeusCore.Instance.StartCoroutine(_downloadService.InitProcessedChunkDictAsync());
        }

        public bool IsSubpackageReady(string tag)
        {
            if (_downloadService == null)
            {
                ///modify by cmm
                //Debug.LogWarning("The setting \"useSubpackage\" is false, please check it.");
                return true;
            }
            return _downloadService.IsSubpackageReady(tag);
        }

        public bool IsHardDiskEnough()
        {
            if (_downloadService == null)
            {
                Debug.LogWarning("The setting \"useSubpackage\" is false, please check it.");
                return true;
            }
            return _downloadService.IsHardDiskEnough();
        }

        public double CalcUnCompleteChunkSizeForTag(string tag)
        {
            if (_downloadService == null)
            {
                Debug.LogWarning("The setting \"useSubpackage\" is false, please check it.");
                return 0;
            }
            return _downloadService.CalcUnCompleteChunkSizeForTag(tag);
        }

        public double GetSizeToDownloadOfTag(string tag)
        {
            if (_downloadService == null)
            {
                Debug.LogWarning("The setting \"useSubpackage\" is false, please check it.");
                return 0;
            }
            return _downloadService.GetSizeToDownloadOfTag(tag);
        }

        public double GetTagSize(string tag)
        {
            if (_downloadService == null)
            {
                Debug.LogWarning("The setting \"useSubpackage\" is false, please check it.");
                return 0;
            }
            return _downloadService.GetTagSize(tag);
        }

        #endregion

        public IAssetRef LoadAsset(string path, Type type)
        {
            //level map
            if (_setting.enableAssetLevel)
                path = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.AssetBundle);

            AssetsLogger.Log("LoadAsset: ", path);
            BundleAssetRef assetRef = null;
            if (_cachedAssetDict.TryGetValue(path, out assetRef))
            {
                return assetRef;
            }

            string abName;
            string assetName;
            if (!AssetBundleUtils.TryGetAssetBundleName(path, out abName, out assetName))
            {
                _errorData.ErrorType = AssetLoadErrorType.AssetNotExist;
                _errorData.AssetPath = path;
#if UNITY_EDITOR
                Debug.LogError("AssetFromBundleLoader:Not Found asset: " + path);
#else
                if (_setting.assetAbsenceLogLevel == AssetAbsenceLogLevel.EditorDevice)
                {
                    Debug.LogError("AssetFromBundleLoader:Not Found asset: " + path);
                }
                else if(_setting.assetAbsenceLogLevel == AssetAbsenceLogLevel.EditorDevelopmentDevice && Debug.isDebugBuild)
                {
                    Debug.LogError("AssetFromBundleLoader:Not Found asset: " + path);
                }
#endif
                return null;
            }

            BundleAssetLoadTask assetLoadTask = null;
            if (_assetLoadTaskDict.TryGetValue(path, out assetLoadTask))
            {
                assetLoadTask.Priority = BundleAssetLoadTask.LoadPriority.Immediately;
                AssetBundleLoadTask bundleLoadTask = null;
                if (_bundleLoadTaskDict.TryGetValue(abName, out bundleLoadTask))
                {
                    if (bundleLoadTask.IsRemoteLoading())
                    {
                        return null;
                    }
                    bundleLoadTask.SetCheckDoneByAssetBundle();
                }
#if UNITY_EDITOR
                Debug.LogWarning("LoadAsset Immediately -> LoadAssetAsync");
#endif
                while (!_cachedAssetDict.TryGetValue(path, out assetRef) && _assetLoadTaskDict.ContainsKey(path))
                {
                    //System.Threading.Thread.Sleep(0);
                    CheckBundleLoadTask();
                    CheckAssetLoadTask();
                }
            }
            else
            {
                try
                {
                    BundleRef bundleRef = LoadBundle(abName, path);
                    assetRef = new BundleAssetRef(path, bundleRef.BundleName, assetName, null, bundleRef);
                    bundleRef.LoadAssetRef(ref assetRef, path, assetName, type);
                    if (assetRef != null)
                    {
                        _cachedAssetDict.Add(assetRef.AssetPath, assetRef);
                        //make sure asset will be checked
                        assetRef.Retain();
                        assetRef.Release();
                    }
                    else
                    {
                        _errorData.ErrorType = AssetLoadErrorType.AssetLoadFailed;
                        _errorData.AssetPath = path;
                    }
                }
                catch (Exception ex)
                {
                    _errorData.ErrorType = AssetLoadErrorType.BundleLoadFailed;
                    _errorData.AssetPath = path;
                    Debug.LogException(ex);
                    Debug.LogError("[zeus asset] Load asset \"" + assetName + "\" failed.");
                }
            }
            return assetRef;
        }

        public void LoadAssetAsync(string path, Type type, Action<IAssetRef, object> callback, object param)
        {
            _LoadAssetAsync(path, type, callback, param, BundleAssetLoadTask.LoadPriority.Normal);
        }

        public void LoadAssetUrgent(string path, Type type, Action<IAssetRef, object> callback, object param)
        {
            _LoadAssetAsync(path, type, callback, param, BundleAssetLoadTask.LoadPriority.High);
        }

        public bool IsAssetCached(string path)
        {
            return _cachedAssetDict.ContainsKey(path);
        }

        private void _LoadAssetAsync(string path, Type type, Action<IAssetRef, object> callback, object param, BundleAssetLoadTask.LoadPriority loadPriority = BundleAssetLoadTask.LoadPriority.Normal)
        {
            if (_setting.enableAssetLevel)
                path = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.AssetBundle);

            AssetsLogger.Log("_LoadAssetAsync", path);
            string abName = null;
            string assetName = null;
            BundleAssetRef assetRef = null;
            BundleAssetLoadTask assetLoadTask = null;
            BundleRef bundleRef = null;
            if (_cachedAssetDict.TryGetValue(path, out assetRef))
            {
                if (_setting.fullAsyncCallback)
                {
                    assetRef.Retain();
                    assetLoadTask = GetBundleAssetLoadTask(path, assetRef.AssetName, type, assetRef.BundleName, loadPriority);
                    assetLoadTask.Step = BundleAssetLoadTask.LoadStep.WaitingCallback;
                }
                else
                {
                    SafeExecuteAction(callback, assetRef, param);
                    return;
                }
            }
            else if (!AssetBundleUtils.TryGetAssetBundleName(path, out abName, out assetName))
            {
#if UNITY_EDITOR
                Debug.LogError("not found asset: " + path);
#else
                if (_setting.assetAbsenceLogLevel == AssetAbsenceLogLevel.EditorDevice)
                {
                    Debug.LogError("AssetFromBundleLoader:Not Found asset: " + path);
                }
                else if(_setting.assetAbsenceLogLevel == AssetAbsenceLogLevel.EditorDevelopmentDevice && Debug.isDebugBuild)
                {
                    Debug.LogError("AssetFromBundleLoader:Not Found asset: " + path);
                }
#endif
                if (_setting.fullAsyncCallback)
                {
                    assetLoadTask = GetBundleAssetLoadTask(path, null, type, null, loadPriority);
                    assetLoadTask.Step = BundleAssetLoadTask.LoadStep.WaitingCallback;
                    assetLoadTask.ErrorType = AssetLoadErrorType.AssetNotExist;
                }
                else
                {
                    _errorData.ErrorType = AssetLoadErrorType.AssetNotExist;
                    _errorData.AssetPath = path;
                    SafeExecuteAction(callback, null, param);
                    return;
                }
            }
            else
            {
                bundleRef = CreateBundleRef(abName);
                if (!_loadingAssetDict.TryGetValue(path, out assetRef))
                {
                    assetRef = new BundleAssetRef(path, abName, assetName, null, bundleRef);
                    _loadingAssetDict.Add(path, assetRef);
                }
                assetRef.Retain();

                assetLoadTask = GetBundleAssetLoadTask(path, assetName, type, abName, loadPriority);
                if (!_cachedBundleDict.ContainsKey(abName))
                {
                    CreateAssetBundleTask(abName, path, loadPriority);
                }
            }
            if (assetLoadTask != null && callback != null)
            {
                assetLoadTask.AddCallback(callback, param);
            }
        }

        private BundleAssetLoadTask GetBundleAssetLoadTask(string assetPath, string assetName, Type type, string abName, BundleAssetLoadTask.LoadPriority loadPriority)
        {
            BundleAssetLoadTask assetLoadTask = null;
            if (_assetLoadTaskDict.TryGetValue(assetPath, out assetLoadTask))
            {
                assetLoadTask.Priority = loadPriority;
            }
            else
            {
                assetLoadTask = new BundleAssetLoadTask(assetPath, assetName, type, abName);
                assetLoadTask.Priority = loadPriority;
                assetLoadTask.Step = BundleAssetLoadTask.LoadStep.WaitingBundle;
                _assetLoadTaskDict.Add(assetPath, assetLoadTask);
            }
            return assetLoadTask;
        }

        public void LoadScene(string path, LoadSceneMode loadMode)
        {
            if (_setting.enableAssetLevel)
                path = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.AssetBundle);

            AssetsLogger.Log("LoadScene: ", path);
            if (_loadedSceneBundleDict.ContainsKey(path))
            {
                Debug.LogWarning("LoadScene scene has been loaeded: " + path);
                return;
            }
            string abName;
            string assetName;
            if (!AssetBundleUtils.TryGetAssetBundleName(path, out abName, out assetName))
            {
                _errorData.ErrorType = AssetLoadErrorType.AssetNotExist;
                _errorData.AssetPath = path;
                throw new Exception("AssetFromBundleLoader:Not Found path in ab:" + path);
            }
            int idx = assetName.LastIndexOf('/');
            assetName = assetName.Substring(idx + 1);
            Debug.Assert(!_cachedBundleDict.ContainsKey(abName));
            Debug.Assert(!_bundleLoadTaskDict.ContainsKey(abName));
            if (loadMode == LoadSceneMode.Single)
            {
                foreach (KeyValuePair<string, BundleRef> pair in _loadedSceneBundleDict)
                {
                    BundleRef sceneBundleRef = pair.Value;
                    _cachedBundleDict.Remove(sceneBundleRef.BundleName);
                    sceneBundleRef.Release(null);
                    sceneBundleRef.UnloadBundle(false);
                }
                _loadedSceneBundleDict.Clear();
            }
            BundleRef bundleRef = LoadBundle(abName, path);
            bundleRef.Retain();
            _loadedSceneBundleDict.Add(path, bundleRef);
            assetName = AssetBundleUtils.RemoveSceneFileExtension(assetName);
            SceneManager.LoadScene(assetName, loadMode);
        }

        public void LoadSceneAsync(string path, LoadSceneMode loadMode, Action<bool, float, object> callback, object param)
        {
            if (_setting.enableAssetLevel)
                path = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.AssetBundle);

            AssetsLogger.Log("LoadSceneAsync: ", path);
            SceneLoadCommand loadOperation = new SceneLoadCommand(path, loadMode, callback, param);
            _sceneCommandQueue.Enqueue(loadOperation);
        }


        public IEnumerator LoadSceneAsync(string path, LoadSceneMode loadMode)
        {
            if (_setting.enableAssetLevel)
                path = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.AssetBundle);

            AssetsLogger.Log("LoadSceneAsync Coroutine", path);

            SceneLoadCommand loadOperation = new SceneLoadCommand(path, loadMode);
            _sceneCommandQueue.Enqueue(loadOperation);

            while (_sceneCommandQueue.Peek() != loadOperation)
            {
                loadOperation.FrameId = Time.frameCount;
                yield return null;
            }

            yield return DoLoadSceneCoroutine(loadOperation);
        }
        private IEnumerator DoLoadSceneCoroutine(SceneLoadCommand loadCommand)
        {
            loadCommand.BeginExecuteCommand();
            if (_loadedSceneBundleDict.ContainsKey(loadCommand.ScenePath))
            {
                Debug.LogWarning("LoadSceneAsync scene has been loaded: " + loadCommand.ScenePath);
                loadCommand.FinishCommand();
                yield break;
            }
            string abName;
            string assetName;
            if (!AssetBundleUtils.TryGetAssetBundleName(loadCommand.ScenePath, out abName, out assetName))
            {
                _errorData.ErrorType = AssetLoadErrorType.AssetNotExist;
                _errorData.AssetPath = loadCommand.ScenePath;
                Debug.LogException(new Exception("Not found scene " + loadCommand.ScenePath));
                loadCommand.FinishCommand();
                yield break; 
            }

            Debug.Assert(!_cachedBundleDict.ContainsKey(abName));
            Debug.Assert(!_bundleLoadTaskDict.ContainsKey(abName));

            BundleRef bundleRef = null;
            if (!_cachedBundleDict.TryGetValue(abName, out bundleRef))
            {
                bundleRef = CreateBundleRef(abName);
                bundleRef.Retain();

                AssetBundleLoadTask bundleLoadTask = CreateAssetBundleTask(abName, loadCommand.ScenePath);
                SceneLoadCoroutineTask sceneLoadTask = new SceneLoadCoroutineTask(loadCommand.SceneName, loadCommand.LoadMode, bundleLoadTask);

                _sceneLoadTaskDict.Add(loadCommand.ScenePath, sceneLoadTask);
                while (!sceneLoadTask.IsBundleReadly())
                {
                    loadCommand.FrameId = Time.frameCount;
                    yield return null;
                }

                loadCommand.FrameId = -1;
                if(sceneLoadTask.ErrorMsg != null)
                {
                    sceneLoadTask.FinishTask();
                    bundleRef.Release(null);
                    loadCommand.FinishCommand();
                    yield break;
                }
                yield return sceneLoadTask.LoadSceneAsync();

                if (sceneLoadTask.ErrorMsg != null)
                {
                    bundleRef.Release(null);
                    sceneLoadTask.FinishTask();
                    loadCommand.FinishCommand();
                    yield break;
                }
            }
            else
            {
                Debug.LogError("[zeus asset] scene bundle has loaded: " + assetName);
            }

            //卸载之前加载的场景
            if (loadCommand.LoadMode == LoadSceneMode.Single)
            {
                foreach (KeyValuePair<string, BundleRef> pair in _loadedSceneBundleDict)
                {
                    BundleRef sceneBundleRef = pair.Value;
                    _cachedBundleDict.Remove(sceneBundleRef.BundleName);
                    sceneBundleRef.Release(null);
                    sceneBundleRef.UnloadBundle();
                }
                _loadedSceneBundleDict.Clear();
            }
            //添加到记录列表里
            _loadedSceneBundleDict.Add(loadCommand.ScenePath, bundleRef);
            loadCommand.FinishCommand();
        }

        public void UnloadSceneAsync(string path, Action<bool, float, object> callback, object param)
        {
            AssetsLogger.Log("UnloadSceneAsync: ", path);
            SceneUnloadCommand command = new SceneUnloadCommand(path, callback, param);
            _sceneCommandQueue.Enqueue(command);
        }

        public IEnumerator UnloadSceneAsync(string path)
        {
            AssetsLogger.Log("UnloadSceneAsync: ", path);
            SceneUnloadCommand command = new SceneUnloadCommand(path);
            _sceneCommandQueue.Enqueue(command);

            while (_sceneCommandQueue.Peek() != command)
            {
                command.FrameId = Time.frameCount;
                yield return null;
            }

            yield return DoUnLoadSceneCoroutine(command);
        }
        public IEnumerator DoUnLoadSceneCoroutine(SceneUnloadCommand command)
        {
            command.BeginExecuteCommand();
            if (!_loadedSceneBundleDict.ContainsKey(command.ScenePath))
            {
                Debug.LogWarning("UnloadSceneAsync scene has not been loaded: " + command.ScenePath);
                command.FinishCommand();
                yield break;
            }
            string abName;
            string assetName;
            if (!AssetBundleUtils.TryGetAssetBundleName(command.ScenePath, out abName, out assetName))
            {
                Debug.LogException(new Exception("AssetFromBundleLoader:Not Found scene path :" + command.ScenePath));
                command.FinishCommand();
                yield break;
            }
            AsyncOperation unloadOperation = null;
            try
            {
                unloadOperation = SceneManager.UnloadSceneAsync(command.SceneName);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                command.FinishCommand();
                yield break;
            }

            command.FrameId = -1;
            yield return unloadOperation;

            try
            {
                BundleRef bundleRef = _loadedSceneBundleDict[command.ScenePath];
                _loadedSceneBundleDict.Remove(command.ScenePath);
                _cachedBundleDict.Remove(abName);
                bundleRef.Release(null);
                bundleRef.UnloadBundle();
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            command.FinishCommand();
        }

        public void UnloadAsset()
        {
            _bundleGcStrategy.CollectAll(_cachedBundleDict, _cachedAssetDict);
        }

        private IEnumerator UnloadSceneCoroutine(AsyncOperation unloadOperation, Action<bool, float, object> callback, object param)
        {
            while (!unloadOperation.isDone)
            {
                if (callback != null)
                {
                    callback(false, unloadOperation.progress, param);
                }
                yield return null;
            }
            if (callback != null)
            {
                callback(true, unloadOperation.progress, param);
            }
        }

        private void Update()
        {
            InternalUpdate();
        }

        private IEnumerator UpdateCoroutine()
        {
            while (Application.isPlaying)
            {
                InternalUpdate();
                yield return new WaitForEndOfFrame();
            }
        }

        private void InternalUpdate(bool gc = true)
        {
            try
            {
                CheckBundleLoadTask();
                CheckAssetLoadTask();
                CheckSceneLoadTask();
                if (gc && Time.frameCount % 10 == 0)
                {
                    _bundleGcStrategy.Collect(_cachedBundleDict, _cachedAssetDict);
                }
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private BundleRef LoadBundle(string abName, string assetPath)
        {
            AssetsLogger.Log("LoadBundle", abName);
            string abPath = AssetBundleUtils.GetAssetBundlePath(abName); //获取到AssetBundlePath
            if (String.IsNullOrEmpty(abPath))
            {
                throw new Exception("AssetBundleLoader:bundle not exits \"" + abName + "\"");
            }
            AssetBundleLoadTask loadTask = null;
            while (_bundleLoadTaskDict.TryGetValue(abName, out loadTask))
            {
                if (loadTask.IsRemoteLoading())
                {
                    throw new Exception(string.Format("[zeus asset] Sync load asset {0}, but bundle {1} remote loading",assetPath, abName));
                }
                loadTask.SetCheckDoneByAssetBundle();
                CheckBundleLoadTask();
            }
            BundleRef bundleRef = null;
            ////
            /// modify by cmm
            if (!_cachedBundleDict.TryGetValue(abName, out bundleRef) || bundleRef.IsBundleUnloaded())
            {
                //获取所有的依赖
                string[] depNames = AssetBundleUtils.GetDirectDependencies(abName);
                AssetBundle bundle = null;
                bool isDelayRelease = false;
                if (!_bundleBlacklist.Contains(abName))
                {
                    string realPath = null;
                    ulong offset = 0;
                    if (InnerPackage.TryGetFileEntry(abPath, out realPath, out offset))
                    {
                        bundle = AssetBundle.LoadFromFile(realPath, 0, offset);
                        if (bundle == null)
                            Debug.LogError(string.Format("[zeus asset] sync Load {0} from inner failed, path: {1} offset: {2}", assetPath, realPath, offset));
                    }
                    else
                    {
                        if (InnerPackage.IsInnerPath(abPath))
                        {
                            bundle = AssetBundle.LoadFromFile(abPath);
                            if (bundle == null)
                                Debug.LogError(string.Format("[zeus asset] sync Load {0} from inner failed, path: {1} ", assetPath, abPath));
                        }
                        else
                        {
                            if (System.IO.File.Exists(abPath))
                            {
                                bundle = AssetBundle.LoadFromFile(abPath);
                                if (bundle == null)
                                    Debug.LogError(string.Format("[zeus asset] sync Load {0} from outer failed, path: {1} ", assetPath, abPath));
                            }
                            else
                            {
                                try
                                {
                                    if (_downloadService != null)
                                    {
                                        Debug.LogError(string.Format("[zeus asset] sync load {0}, Try to download assetBundle {1}",assetPath, abPath));
                                        _downloadService.AddUrgentBundleDownloadTask(abName, this.CheckBundleLoadTask);
                                        isDelayRelease = true;
                                    }
                                    else
                                    {
                                        Debug.LogError(string.Format("[zeus asset] sync Load: {0} failed, bundle: {1} not exit！", assetPath, abPath));
                                    }
                                    if (_errorObserver != null)
                                    {
                                        try
                                        {
                                            var msg = string.Format("[zeus asset] LoadAsset:{0} local bundle file {1} not ready", assetPath, abName);
                                            _errorObserver.Invoke(AssetLoadErrorType.AssetSyncLoadBundleNotReady, msg);
                                        }
                                        catch(Exception ex)
                                        {
                                            Debug.LogException(ex);
                                        }
                                    }
                                }
                                catch
                                {
                                    //do noting
                                }
                            }
                        }
                    }
                    if(bundle == null)
                    {
                        Debug.LogError("Bundle load failed: " + abPath);
                    }
                }

                ////
                /// modify by cmm
                bool newBundleRef = bundleRef == null;
                if (newBundleRef)
                    bundleRef = new BundleRef(abName, bundle, _bundleGcStrategy, depNames.Length);
                else
                {
                    bundleRef.SetAssetBundle(bundle);
                    Debug.LogError("BundleRef allready exist: " + assetPath + 
                        "\nRefCount: " + bundleRef.RefCount + 
                        "\nAssetRefListSize:" + bundleRef.AssetRefList.Count);
                }

                if (isDelayRelease)
                {
                    bundleRef.EnableRemoteLoading(true);
                }
                
                if (newBundleRef) 
                    _cachedBundleDict.Add(abName, bundleRef);

                foreach (string depName in depNames)
                {
                    BundleRef depBundleRef = null;
                    if (!_cachedBundleDict.TryGetValue(depName, out depBundleRef) || bundleRef.IsBundleUnloaded())
                    {
                        depBundleRef = LoadBundle(depName, assetPath);
                    }
                    if (newBundleRef) 
                        bundleRef.AddDepBundle(depBundleRef);
                }
            }
            return bundleRef;
        }

        private void CheckBundleLoadTask(string bundleName, HttpErrorType errorType)
        {
            _cancelDelayReleaseBundleQueue.Enqueue(bundleName);
        }
        

        private List<BundleRef> completeBundleList = new List<BundleRef>();
        private void CheckBundleLoadTask()
        {
            if (_bundleLoadTaskDict.Count > 0)
            {
                completeBundleList.Clear();
                try
                {
                    foreach (KeyValuePair<string, AssetBundleLoadTask> pair in _bundleLoadTaskDict)
                    {
                        AssetBundleLoadTask loadTask = pair.Value;
                        if (loadTask.IsDone())
                        {
                            BundleRef bundleRef = _loadingBundleDict[loadTask.BundleName];
                            bundleRef.SetAssetBundle(loadTask.GetAssetBundle());
                            completeBundleList.Add(bundleRef);
                        }
                    }
                }
                catch(InvalidOperationException ex)
                {
                    Debug.LogError("[zeus asset] InvalidOperationException in foreach in _bundleLoadTaskDict");
                    throw new Exception("[zeus asset] InvalidOperationException in foreach in _bundleLoadTaskDict");
                }
                catch(Exception ex)
                {
                    throw ex;
                }
                
                for (int i = 0; i < completeBundleList.Count; i++)
                {
                    BundleRef bundleRef = completeBundleList[i];
                    var bundleLoadTask = _bundleLoadTaskDict[bundleRef.BundleName];

                    _bundleLoadTaskDict.Remove(bundleRef.BundleName);
                    _loadingBundleDict.Remove(bundleRef.BundleName);
                    _cachedBundleDict.Add(bundleRef.BundleName, bundleRef);
                    if (bundleLoadTask.LoadErrorType != AssetLoadErrorType.None && _errorObserver != null)
                    {
                        try
                        {
                            _errorObserver.Invoke(bundleLoadTask.LoadErrorType, bundleLoadTask.ErrorMessage);
                        }
                        catch
                        {
                            //do nothing
                        }
                    }
                    _completedloadTaskList.Add(bundleLoadTask);
                }
                completeBundleList.Clear();
            }
            else if (_completedloadTaskList.Count > 0)
            {
                foreach (var loadTask in _completedloadTaskList)
                {
                    loadTask.ReSet();
                    _loadTaskCache.Enqueue(loadTask);
                }
                _completedloadTaskList.Clear();
            }

            if (_cancelDelayReleaseBundleQueue.Count > 0)
            {
                string bundleName = null;
                while (_cancelDelayReleaseBundleQueue.TryDequeue(out bundleName))
                {
                    BundleRef bundleRef = null;
                    if (_cachedBundleDict.TryGetValue(bundleName, out bundleRef))
                    {
                        bundleRef.EnableRemoteLoading(false);
                    }
                    else
                    {
                        Debug.LogError("[zeus asset] check and cancel delayRelease not found: " + bundleName);
                    }
                }
            }
        }

        private List<BundleAssetLoadTask> completeAssetList = new List<BundleAssetLoadTask>();
        private BundleAssetLoadTask.AssetLoadTaskPriorityComparer assetLoadPriorityComparer = new BundleAssetLoadTask.AssetLoadTaskPriorityComparer();
        private void CheckAssetLoadTask()
        {
            if (_assetLoadTaskDict.Count > 0)
            {
                completeAssetList.Clear();
                foreach (KeyValuePair<string, BundleAssetLoadTask> pair in _assetLoadTaskDict)
                {
                    BundleAssetLoadTask loadTask = pair.Value;
                    if (loadTask.Step == BundleAssetLoadTask.LoadStep.LoadingAsset)
                    {
                        if (loadTask.IsDone())
                        {
                            //AssetsLogger.Log("CheckAssetLoadTask loadTask Done: ", loadTask.AssetPath);
                            BundleAssetRef assetRef = _loadingAssetDict[loadTask.AssetPath];
                            if (loadTask.AssetBundleRequest.asset == null)
                            {
                                Debug.LogError("[zeus asset] Load asset falied " + loadTask.BundleName + ":" + loadTask.AssetPath);
                            }
                            assetRef.AssetObject = loadTask.AssetBundleRequest.asset;
                            completeAssetList.Add(loadTask);
                            loadTask.Step = BundleAssetLoadTask.LoadStep.Finish;
                            loadTask.BundleRef.AddBundleAssetRef(assetRef);
                            _cachedAssetDict.Add(assetRef.AssetPath, assetRef);
                        }
                    }
                    else if (loadTask.Step == BundleAssetLoadTask.LoadStep.WaitingBundle)
                    {
                        BundleRef bundleRef = null;
                        if (_cachedBundleDict.TryGetValue(loadTask.BundleName, out bundleRef))
                        {
                            if (loadTask.Priority == BundleAssetLoadTask.LoadPriority.Immediately)
                            {
                                BundleAssetRef assetRef = _loadingAssetDict[loadTask.AssetPath];
                                bundleRef.LoadAssetRef(ref assetRef, loadTask.AssetPath, loadTask.AssetName, loadTask.AssetType);
                                loadTask.Step = BundleAssetLoadTask.LoadStep.Finish;
                                if (assetRef.AssetObject == null)
                                {
                                    _cachedAssetDict.Add(assetRef.AssetPath, assetRef);
                                    completeAssetList.Add(loadTask);
                                }
                                else
                                {
                                    _cachedAssetDict.Add(assetRef.AssetPath, assetRef);
                                    completeAssetList.Add(loadTask);
                                }
                            }
                            else
                            {
                                AssetBundleRequest request = bundleRef.LoadAssetAsync(loadTask.AssetName, loadTask.AssetType);
                                if (request == null)
                                {
                                    loadTask.Step = BundleAssetLoadTask.LoadStep.Finish;
                                    BundleAssetRef assetRef = _loadingAssetDict[loadTask.AssetPath];
                                    assetRef.AssetObject = null;
                                    bundleRef.AddBundleAssetRef(assetRef);
                                    completeAssetList.Add(loadTask);
                                    _cachedAssetDict.Add(assetRef.AssetPath, assetRef);
                                    Debug.LogError("[zeus asset] AssetBundleLoader load asset failed : " + loadTask.AssetName);
                                }
                                else
                                {
                                    loadTask.Step = BundleAssetLoadTask.LoadStep.LoadingAsset;
                                    loadTask.AssetBundleRequest = request;
                                    request.priority = (int)loadTask.Priority;
                                    loadTask.BundleRef = bundleRef;
                                }
                            }
                        }
                    }
                    else if (loadTask.Step == BundleAssetLoadTask.LoadStep.WaitingCallback)
                    {
                        completeAssetList.Add(loadTask);
                    }
                }
                if (completeAssetList.Count > 0)
                {
                    completeAssetList.Sort(assetLoadPriorityComparer);
                    for (int i = 0; i < completeAssetList.Count; i++)
                    {
                        BundleAssetLoadTask loadTask = completeAssetList[i];
                        if (_loadingAssetDict.ContainsKey(loadTask.AssetPath))
                            _loadingAssetDict.Remove(loadTask.AssetPath);

                        _assetLoadTaskDict.Remove(loadTask.AssetPath);
                        AssetsLogger.Log("_LoadAssetAsync callback: ", loadTask.AssetPath);
                        BundleAssetRef assetRef = null;
                        _cachedAssetDict.TryGetValue(loadTask.AssetPath, out assetRef);
                        loadTask.ExecuteCallBack(assetRef);
                    }
                    completeAssetList.Clear();
                }
            }
        }

        private List<string> _loadedSceneList = new List<string>();
        private void CheckSceneLoadTask()
        {
            if (_sceneLoadTaskDict.Count > 0)
            {
                _loadedSceneList.Clear();
                foreach (var pair in _sceneLoadTaskDict)
                {
                    SceneLoadTaskBase loadTask = pair.Value;
                    loadTask.UpdateLoadProgress();
                    if (loadTask.IsDone())
                    {
                        _loadedSceneList.Add(pair.Key);
                    }
                }
                for (int i = 0; i < _loadedSceneList.Count; i++)
                {
                    SceneLoadTaskBase loadTask = _sceneLoadTaskDict[_loadedSceneList[i]];
                    _sceneLoadTaskDict.Remove(_loadedSceneList[i]);
                    try
                    {
                        loadTask.ExecuteCallBack();
                    }
                    catch
                    {
                        //do nothing
                    }
                }
                _loadedSceneList.Clear();
            }
            if (_sceneCommandQueue.Count > 0)
            {
                var command = _sceneCommandQueue.Peek();
                if (command.State == SceneCommandBase.CommandState.Pending)
                {
                    if (command.CmdType == SceneCommandBase.CommandType.AsyncLoad)
                    {
                        SceneLoadCommand loadOp = command as SceneLoadCommand;
                        ZeusCore.Instance.StartCoroutine(DoLoadSceneCoroutine(loadOp));
                    }
                    else if (command.CmdType == SceneCommandBase.CommandType.AsyncUnload)
                    {
                        SceneUnloadCommand unloadOp = command as SceneUnloadCommand;
                        ZeusCore.Instance.StartCoroutine(DoUnLoadSceneCoroutine(unloadOp));
                    }
                    else if (command.CmdType == SceneCommandBase.CommandType.CoroutineLoad)
                    {
                        if (command.FrameId < Time.frameCount - 1)
                        {
                            //coroutine interrupt may be monobehaviour has been destoryed
                            Debug.LogWarningFormat("[zeus asset] {0} scene command cancel because coroutine interrupt {1} / {2}", command.SceneName, command.FrameId, Time.frameCount);
                            _sceneCommandQueue.Dequeue();
                        }
                    }
                }
                else if (command.State == SceneCommandBase.CommandState.Doing)
                {
                    if (command.CmdType == SceneCommandBase.CommandType.CoroutineLoad)
                    {
                        if (command.FrameId > 0 && command.FrameId < Time.frameCount - 1)
                        {
                            //coroutine interrupt may be monobehaviour has been destoryed
                            Debug.LogWarningFormat("[zeus asset] {0} scene command interrupt because coroutine interrupt {1} / {2}", command.SceneName, command.FrameId, Time.frameCount);
                            _sceneCommandQueue.Dequeue();
                        }
                    }
                }
                else if (command.State == SceneCommandBase.CommandState.Done)
                {
                    _sceneCommandQueue.Dequeue();
                }
            }
        }

        protected BundleRef CreateBundleRef(string abName)
        {
            BundleRef bundleRef = null;
            if (!_cachedBundleDict.TryGetValue(abName, out bundleRef) && !_loadingBundleDict.TryGetValue(abName, out bundleRef))
            {
                //获取所有的依赖
                string[] depNames = AssetBundleUtils.GetDirectDependencies(abName);

                bundleRef = new BundleRef(abName, _bundleGcStrategy, depNames.Length);
                if (_bundleBlacklist.Contains(abName))
                {
                    _cachedBundleDict.Add(abName, bundleRef);
                }
                else
                {
                    _loadingBundleDict.Add(abName, bundleRef);
                }

                foreach (string depName in depNames)
                {
                    BundleRef depBundleRef = null;
                    depBundleRef = CreateBundleRef(depName);
                    bundleRef.AddDepBundle(depBundleRef);
                }
            }
            return bundleRef;
        }

        private AssetBundleLoadTask CreateAssetBundleTask(string abName, string assetPath, BundleAssetLoadTask.LoadPriority priority = BundleAssetLoadTask.LoadPriority.Normal)
        {
            AssetBundleLoadTask loadTask = null;
            if (!_bundleBlacklist.Contains(abName))
            {
                if (_bundleLoadTaskDict.TryGetValue(abName, out loadTask))
                {
                    return loadTask;
                }
                if (_loadTaskCache.Count > 0)
                {
                    loadTask = _loadTaskCache.Dequeue();
                    loadTask.Init(abName, _downloadService, priority, assetPath, _remoteLoadObserver);
                }
                else
                {
                    loadTask = new AssetBundleLoadTask(abName, _downloadService, priority, assetPath, _remoteLoadObserver);
                }
                _bundleLoadTaskDict.Add(abName, loadTask);
            }

            //Debug.Assert(_loadingBundleDict.ContainsKey(abName), "AssetBundleLoader.CreateAssetBundleTask _loadingBundleDict not Contains bundle: " + abName);
            AssetsLogger.Log("CreateAssetBundleTask", abName);
            string[] deps = AssetBundleUtils.GetDirectDependencies(abName);
            for (int i = 0; i < deps.Length; i++)
            {
                if (!_cachedBundleDict.ContainsKey(deps[i]))
                {
                    AssetBundleLoadTask depTask = CreateAssetBundleTask(deps[i], assetPath, priority);
                    if (depTask != null)
                    {
                        loadTask.AddDepBundleTask(depTask);
                    }
                }
            }
            return loadTask;
        }

        public void UnloadAll(bool unloadAllLoadedObjects)
        {
            _bundleGcStrategy.CollectAll(_cachedBundleDict, _cachedAssetDict);
            foreach (KeyValuePair<string, BundleRef> pair in _cachedBundleDict)
            {
                pair.Value.UnloadBundle(unloadAllLoadedObjects);
            }
            _cachedBundleDict.Clear();
            _cachedAssetDict.Clear();
        }

        public void Dump()
        {
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            strBuilder.Append("_cachedAssetDict.count:");
            strBuilder.Append(_cachedAssetDict.Count);
            strBuilder.AppendLine();
            foreach (KeyValuePair<string, BundleAssetRef> pair in _cachedAssetDict)
            {
                strBuilder.Append(pair.Value.AssetPath);
                strBuilder.Append(" : ");
                strBuilder.Append(pair.Value.RefCount);
                strBuilder.AppendLine();
            }
            strBuilder.Append("_cachedBundleDict.count:");
            strBuilder.Append(_cachedBundleDict.Count);
            strBuilder.AppendLine();
            foreach (KeyValuePair<string, BundleRef> pair in _cachedBundleDict)
            {
                if (!pair.Value.IsBundleUnloaded())
                {
                    strBuilder.Append(pair.Value.BundleName);
                    strBuilder.Append(" : ");
                    strBuilder.Append(pair.Value.RefCount);
                    strBuilder.AppendLine();
                    int assetRefIndex = 0;
                    int assetRefCount = pair.Value.RefCount;
                    foreach (var assetRef in pair.Value.AssetRefList)
                    {
                        strBuilder.AppendLine($"RefIndex: {++assetRefIndex}/{assetRefCount}, AssetPath: {assetRef.AssetPath}, RefCount: {assetRef.RefCount}");
                    }
                }
            }
            strBuilder.Append("_loadedSceneBundleDict.count:");
            strBuilder.Append(_loadedSceneBundleDict.Count);
            strBuilder.AppendLine();
            foreach (KeyValuePair<string, BundleRef> pair in _loadedSceneBundleDict)
            {
                strBuilder.Append(pair.Value.BundleName);
                strBuilder.Append(" : ");
                strBuilder.Append(pair.Value.RefCount);
                strBuilder.AppendLine();
            }

            Debug.Log(strBuilder.ToString());

            AssetDumpHelper.GenerateDumpSnapshotFile(_cachedAssetDict, _cachedBundleDict);
        }

        public LocalAssetStatus GetAssetStatus(string path, Type type)
        {
            string abName;
            string assetName;
            if (!AssetBundleUtils.TryGetAssetBundleName(path, out abName, out assetName))
            {
                return LocalAssetStatus.NonExistent;
            }

            //获取到AssetBundlePath
            string abPath = null;
            bool isBundleExisted = AssetBundleUtils.TryGetAssetBundlePath(abName, out abPath);
            if (!isBundleExisted)
            {
                return LocalAssetStatus.LocalNonExistent;
            }
            if (_bundleLoadTaskDict.ContainsKey(abName))
            {
                return LocalAssetStatus.AsyncLoading;
            }

            //获取所有的依赖
            string[] depNames = AssetBundleUtils.GetAllDependencies(abName);
            foreach (string depName in depNames)
            {
                if (_bundleLoadTaskDict.ContainsKey(depName))
                {
                    return LocalAssetStatus.AsyncLoading;
                }
                isBundleExisted = AssetBundleUtils.TryGetAssetBundlePath(depName, out abPath);
                if (!isBundleExisted)
                {
                    return LocalAssetStatus.LocalNonExistent;
                }
            }
            return LocalAssetStatus.Ready;
        }

        public void GetSubPackageSize(out double totalSize, out double completeSize)
        {
            if (_downloadService == null)
            {
                totalSize = 0;
                completeSize = 0;
            }
            else
            {
                _downloadService.GetSubPackageSize(out totalSize, out completeSize);
            }
        }

        //errorType, errorMessage
        public void SetAssetLoadExceptionObserver(Action<AssetLoadErrorType, string> observer)
        {
            _errorObserver = observer;
        }

        //asssetPath bundleName size druation errorMsg
        public void SetAssetRemoteLoadObserver(Action<string, string, int, float, string> observer)
        {
            _remoteLoadObserver = observer;
        }

        //tag  isComplete
        public void SetTagStatusObserver(Action<string, bool> observer)
        {
            if (_downloadService != null)
            {
                _downloadService.SetTagStatusObserver(observer);
            }
            else
            {
                Debug.LogWarning("Asset Not enable subpackage");
            }
        }


        private void DummyBundleDownloadCallback(string bundleName, Zeus.Framework.Http.UnityHttpDownloader.ErrorType error)
        {
            //do nothing
        }

        private void SafeExecuteAction(Action<IAssetRef, object> callback, IAssetRef assetRef, object param, bool logException = true)
        {
            if(callback != null)
            {
                try 
                {
                    callback(assetRef, param);
                }
                catch(Exception ex)
                {
                    if (logException)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        //检查远端（CDN）资源是否就绪，用于初步检查二包数据是否上传
        public void CheckRemoteFileStatus(Action<ChunkListStatus> remote)
        {
            string url = _setting.remoteURL[0] + "/" + SubPackageBundleInfoContainer.LoadSubPackageInfo().ChunkListName;
            HttpManager.Instance().Head(url, 5000,
                (isGet, error, responseCode, dic) =>
                {
                    if (isGet)
                    {
                        remote(ChunkListStatus.Ready);
                    }
                    else if (responseCode == 404)
                    {
                        remote(ChunkListStatus.MissingFile);
                    }
                    else
                    {
                        remote(ChunkListStatus.NetError);
                    }
                }
            );
        }

        public void SetCacheCapacity(int capactity)
        {
            _bundleGcStrategy.SetCachedBundleLimit(capactity);
        }

        public void SetCacheLifeCycle(float lifeCycle)
        {
            _bundleGcStrategy.SetCachedBundleLifeCycle(lifeCycle);
        }

        public void AddPercentNotification(int percent, string notificationStr)
        {
            if (_downloadService == null)
            {
                Debug.LogWarning("The setting \"useSubpackage\" is false, please check it.");
                return;
            }
            _downloadService.AddPercentNotification(percent, notificationStr);
        }

        public void ClearPercentNotification()
        {
            if (_downloadService == null)
            {
                Debug.LogWarning("The setting \"useSubpackage\" is false, please check it.");
                return;
            }
            _downloadService.ClearPercentNotification();
        }

        public void AddBundleToBlacklist(string abName)
        {
            _bundleBlacklist.Add(abName);
        }

        public void SetAssetLevel(string level)
        {
            if (_setting.enableAssetLevel)
            {
                if (!_setting.enableAssetLevel_GenerateAll && level == null)
                {
                    Debug.LogError("SetAssetLevel level not be null");
                }
                this._level = level;
                AssetLevelManager.Instance.SetLevel(level);
                if (_downloadService != null)
                {
                    _downloadService.SetSubpackageLevel(level);
                }
            }
            else if(!string.IsNullOrEmpty(level))
            {
                //Debug.LogError("Please Enable AssetBundleLoader AssetLevel function first");
            }
        }

        public bool IsSceneLoaded(string path)
        {
            if (_setting.enableAssetLevel)
                path = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.AssetBundle);

            AssetsLogger.Log("IsSceneLoaded ", path);
            string assetName;
            if (!AssetBundleUtils.TryGetAssetBundleName(path, out _, out assetName))
            {
                return false;
            }
            int idx = assetName.LastIndexOf('/');
            assetName = assetName.Substring(idx + 1);

            assetName = AssetBundleUtils.RemoveSceneFileExtension(assetName);
            var targetScene = SceneManager.GetSceneByName(assetName);
            if (targetScene != null && targetScene.isLoaded)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 获取二包网络问题下载失败时是否自动重试
        /// </summary>
        /// <returns></returns>
        public bool GetIsAutoRetryDownloading()
        {
            if (AssetManager.AssetSetting != null&& AssetManager.AssetSetting.bundleLoaderSetting!=null)
            {
                return AssetManager.AssetSetting.bundleLoaderSetting.isAutoRetryDownloading;
            }
            return AssetBundleLoaderSetting.IsAutoRetryDownloading();
        }

        public void SetIsAutoRetryDownloading(bool value)
        {
            AssetBundleLoaderSetting.SetAutoRetryDownloading(value);
            if (_downloadService == null)
            {
                Debug.LogWarning("The setting \"useSubpackage\" is false, please check it.");
                return;
            }
            _downloadService.SetIsAutoRetryDownloading(value);
        }
    }
}
