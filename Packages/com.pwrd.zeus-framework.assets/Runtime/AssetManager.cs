/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部  ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;
using Zeus.Core.FileSystem;
using System.Text;
using System.Collections.Generic;
using Zeus.Framework.Http;
using System.Collections;

namespace Zeus.Framework.Asset
{
    public static class AssetManager
    {
        public delegate void LoadAssetHandler(string assetPath);
        public static event LoadAssetHandler OnLoadEvent;
        private static IResourceLoader _bundleAssetLoader;
        private static ISubPackageLoader _subpackageLoader;
        private static ObsoleteAssetAdpter _assetAdpter;
        private static AssetManagerSetting _setting;

        internal static AssetManagerSetting AssetSetting
        {
            get
            {
                return _setting;
            }
        }

        private static AtlasRequestHandler _atlasRequestHandler;

        public static void Init(string level = null)
        {
            if(IsInited())
            {
                //只能初始化一次
                return;
            }
            InitSetting();
            if (_setting.assetLoaderType == AssetLoaderType.AssetBundle)
            {
                AssetBundleUtils.Init();
                _bundleAssetLoader = new AssetBundleLoader(_setting.bundleLoaderSetting);
                _subpackageLoader = (AssetBundleLoader)_bundleAssetLoader;
            }
            else if (_setting.assetLoaderType == AssetLoaderType.Resources)
            {
                _bundleAssetLoader = new ResourceLoader(_setting.resourceLoaderSetting);
#if UNITY_EDITOR
                _subpackageLoader = new EditorSubPackageLoader(level);
#endif
            }
            else
            {
                _bundleAssetLoader = new AssetDataBaseLoader(_setting.assetDatabaseSetting);
                _subpackageLoader = new EditorSubPackageLoader(level);
            }

            if (_bundleAssetLoader != null)
            {
                _bundleAssetLoader.SetAssetLevel(level);
            }

#if USE_SPRITE_ATLAS_MAP
            AtlasAssetHelper.InitSpriteAtlasMap();
#endif
            _assetAdpter = new ObsoleteAssetAdpter(_bundleAssetLoader);
            _atlasRequestHandler = new AtlasRequestHandler();
#if DEVELOPMENT_BUILD
            GC.Collect();
#endif
        }

        public static bool IsInited()
        {
            return _bundleAssetLoader != null;
        }

        private static void InitSetting()
        {
            string content = VFileSystem.ReadAllText("ZeusSetting/ZeusAssetManagerSetting.json", Encoding.UTF8);
            if (string.IsNullOrEmpty(content))
            {
                Debug.LogError("ZeusAssetManagerSetting.json missing!");
                _setting = new AssetManagerSetting();
            }
            else
            {
                _setting = JsonUtility.FromJson<AssetManagerSetting>(content);
                if (_setting == null)
                {
                    Debug.LogError("ZeusAssetManagerSetting.json error!");
                    _setting = new AssetManagerSetting();
                }
            }
        }

        
        #region 加载资源风格1
        ////////////////////////////// 资源管理器接口 style1///////////////////////////////////////////////////////////
        /////////////////// 直接返回资源对象，不再使用该资源调用UnuseAsset函数 该接口已经过时，请勿再继续使用//////////////
        
        [Obsolete]
        public static Object GetAsset(string path)
        {
            return GetAsset(path, typeof(Object));
        }

        [Obsolete]
        public static Object GetAsset(string path, Type type)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "asset path is empty");
            OnLoad(path);
            return _assetAdpter.GetAsset(path, type);
        }

        [Obsolete]
        public static void GetAssetAsync(string path, Action<Object, object> callback, object param)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "asset path is empty");
            _assetAdpter.GetAssetAsync(path, typeof(Object), callback, param);
            OnLoad(path);
        }

        [Obsolete]
        public static void GetAssetAsync(string path, Type type, Action<Object, object> callback, object param)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "asset path is empty");
            _assetAdpter.GetAssetAsync(path, type, callback, param);
            OnLoad(path);
        }

        [Obsolete]
        public static void GetAssetAsyncUrgent(string path, Action<Object, object> callback, object param)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "asset path is empty");
            _assetAdpter.GetAssetAsyncUrgent(path, typeof(Object), callback, param);
            OnLoad(path);
        }

        [Obsolete]
        public static void GetAssetAsyncUrgent(string path, Type type, Action<Object, object> callback, object param)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "asset path is empty");
            _assetAdpter.GetAssetAsyncUrgent(path, type, callback, param);
            OnLoad(path);
        }

        [Obsolete]
        public static void UnuseAsset(Object asset)
        {
            _assetAdpter.UnuseAsset(asset);
        }
        #endregion
        

        #region 资源接口风格2
        //////////////////////////////资源管理器接口 style2////////////////////////////////////////
        ///////////////////////返回资源引用IAssetRef，通过该引用来使用资源/////////////////////
        /// <summary>
        /// 同步方式加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>资源引用</returns>
        public static IAssetRef LoadAsset(string path)
        {
            return LoadAsset(path, typeof(Object));
        }

        /// <summary>
        /// 同步方式加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <returns>资源引用</returns>
        public static IAssetRef LoadAsset(string path, Type type)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "asset path is empty");
            OnLoad(path);
            return _bundleAssetLoader.LoadAsset(path, type);
        }

        public static bool IsAssetCached(string path)
        {
            return _bundleAssetLoader.IsAssetCached(path);
        }

        /// <summary>
        /// 异步方式加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="callback">回调函数</param>
        /// <param name="param">透传参数</param>
        public static void LoadAssetAsync(string path, Action<IAssetRef, object> callback, object param)
        {
            LoadAssetAsync(path, typeof(Object), callback, param);
        }

        /// <summary>
        /// 异步方式加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <param name="callback">回调函数</param>
        /// <param name="param">透传参数</param>
        public static void LoadAssetAsync(string path, Type type, Action<IAssetRef, object> callback, object param)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "asset path is empty");
            _bundleAssetLoader.LoadAssetAsync(path, type, callback, param);
            OnLoad(path);
        }

        /// <summary>
        /// 加急方式加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="callback">回调函数</param>
        /// <param name="param">透传参数</param>
        public static void LoadAssetUrgent(string path, Action<IAssetRef, object> callback, object param)
        {
            LoadAssetUrgent(path, typeof(Object), callback, param);
        }

        /// <summary>
        /// 加急方式加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <param name="callback">回调函数<资源引用，透传参数></param>
        /// <param name="param">透传参数</param>
        public static void LoadAssetUrgent(string path, Type type, Action<IAssetRef, object> callback, object param)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "asset path is empty");
            _bundleAssetLoader.LoadAssetUrgent(path, type, callback, param);
            OnLoad(path);
        }

        #endregion


        #region 加载资源风格3
        /// <summary>
        /// 异步方式加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <param name="param">透传参数</param>
        /// <returns>AssetLoadOperation</returns>
        public static AssetLoadOperation CreateAssetLoadOperation(string path, Type type, Action<AssetLoadOperation> complete)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "asset path is empty");
            AssetLoadOperation operation = new AssetLoadOperation(path,type, complete);
            return operation;
        }

        /// <summary>
        /// 加急方式加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <param name="param">透传参数</param>
        /// <returns>AssetLoadOperationUgent</returns>
        public static AssetLoadOperationUgent CreateAssetLoadOperationUrgent(string path, Type type, Action<AssetLoadOperation> complete)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "asset path is empty");
            AssetLoadOperationUgent operation = new AssetLoadOperationUgent(path, type, complete);
            return operation;
        }

        /// <summary>
        /// 异步实例化prefab, operation.Dispose()/operation.Dispose(true) 释放gameobject和对应资源。operation.Dispose(false) 只释放资源不释放gameobject。
        /// 如果需要持有该实例请一并持有operation直到不需要该实例及资源。
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="position">对象坐标</param>
        /// <param name="rotation">对象旋转</param>
        /// <param name="parent">对象父节点</param>
        /// <param name="complete">实例化回调函数，可以为空</param>
        /// <returns></returns>
        public static InstantiateOperation InstantiatePrefabAsync(string path, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent, bool instantiateInWorldSpace = true, Action < InstantiateOperation> completeAction = null)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "asset path is empty");
            InstantiateOperation operation = new InstantiateOperation(path, position, rotation, scale, parent, instantiateInWorldSpace, completeAction);
            return operation;
        }

        /// <summary>
        /// 异步实例化prefab, operation.Dispose()/operation.Dispose(true) 释放gameobject和对应资源。operation.Dispose(false) 只释放资源不释放gameobject。
        /// 如果需要持有该实例请一并持有operation直到不需要该实例及资源。
        /// </summary>
        /// <param name="path">prefab资源路径</param>
        /// <param name="parent">实例化的对象挂载到parent下</param>
        /// <param name="tranform2Parent">与parent同position rotation scale</param>
        /// <param name="completeAction">实例化完成回调</param>
        /// <returns></returns>
        public static InstantiateOperation InstantiatePrefabAsync(string path, Transform parent, bool tranform2Parent, Action<InstantiateOperation> completeAction = null)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "asset path is empty");
            InstantiateOperation operation = new InstantiateOperation(path, parent, tranform2Parent, completeAction);
            return operation;
        }

        /// <summary>
        /// 异步加载场景方法
        /// </summary>
        /// <param name="path"></param>
        /// <param name="loadMode"></param>
        /// <param name="param"></param>透传参数
        public static SceneLoadOperation LoadSceneAsync(string path, LoadSceneMode loadMode, object param)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "scene path is empty");
            OnLoad(path);
            SceneLoadOperation operation = new SceneLoadOperation(param);
            _bundleAssetLoader.LoadSceneAsync(path, loadMode, operation.OnSceneLoadCallback, null);
            return operation;
        }

        #endregion

        #region 加载资源风格4
        /// <summary>
        /// 异步方式加载资源(支持Async Await语法)
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <returns>AssetAsyncAwaitOperation</returns>
        public static AssetAsyncAwaitOperation CreateAssetAsyncAwaitOperation(string path, Type type)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "asset path is empty");
            AssetAsyncAwaitOperation operation = new AssetAsyncAwaitOperation(path, type, false);
            return operation;
        }

        /// <summary>
        /// 加急方式加载资源(支持Async Await语法)
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <returns>AssetAsyncAwaitOperation</returns>
        public static AssetAsyncAwaitOperation CreateAssetAsyncAwaitOperationUrgent(string path, Type type)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "asset path is empty");
            AssetAsyncAwaitOperation operation = new AssetAsyncAwaitOperation(path, type, true);
            return operation;
        }

        /// <summary>
        /// 异步加载场景方法(支持Async Await语法)
        /// </summary>
        /// <param name="path"></param>
        /// <param name="loadMode"></param>
        /// <returns>SceneAsyncAwaitOperation</returns>
        public static SceneAsyncAwaitOperation CreateSceneAsyncAwaitOperation(string path, LoadSceneMode loadMode)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "scene path is empty");
            SceneAsyncAwaitOperation operation = new SceneAsyncAwaitOperation(path,loadMode);
            return operation;
        }

        /// <summary>
        /// 卸载场景方法(支持Async Await语法)
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns>UnloadSceneAsyncAwaitOperation</returns>
        public static UnloadSceneAsyncAwaitOperation CreateUnloadSceneAsyncAwaitOperation(string path)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "scene path is empty");
            UnloadSceneAsyncAwaitOperation operation = new UnloadSceneAsyncAwaitOperation(path);
            return operation;
        }

        #endregion

        /// <summary>
        /// 同步加载场景方法
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="loadMode">LoadSceneMode</param>
        public static void LoadScene(string path, LoadSceneMode loadMode)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "scene path is empty");
            _bundleAssetLoader.LoadScene(path, loadMode);
            OnLoad(path);
        }

        /// <summary>
        /// 异步加载场景方法
        /// </summary>
        /// <param name="path"></param>
        /// <param name="loadMode"></param>
        /// <param name="callback"></param>Action<是否加载完成, 加载进度, 透传参数>
        /// <param name="param"></param>透传参数
        public static void LoadSceneAsync(string path, LoadSceneMode loadMode, Action<bool, float, object> callback, object param)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "scene path is empty");
            _bundleAssetLoader.LoadSceneAsync(path, loadMode, callback, param);
            OnLoad(path);
        }

        /// <summary>
        /// 异步加载场景方法
        /// </summary>
        /// <param name="path"></param>
        /// <param name="loadMode"></param>
        public static IEnumerator LoadSceneAsync(string path, LoadSceneMode loadMode = LoadSceneMode.Single)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "scene path is empty");
            yield return _bundleAssetLoader.LoadSceneAsync(path, loadMode);
            OnLoad(path);
        }

        /// <summary>
        /// 卸载场景方法
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="callback"> 回调Action<是否完成, 加载进度, 透传参数></param>
        /// <param name="param">透传参数</param>
        public static void UnloadSceneAsync(string path, Action<bool, float, object> callback, object param)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "scene path is empty");
            _bundleAssetLoader.UnloadSceneAsync(path, callback, param);
        }

        /// <summary>
        /// 卸载场景方法
        /// </summary>
        /// <param name="path">资源路径</param>
        public static IEnumerator UnloadSceneAsync(string path)
        {
            Debug.Assert(!string.IsNullOrEmpty(path), "scene path is empty");
            yield return _bundleAssetLoader.UnloadSceneAsync(path);
        }

        /// <summary>
        /// 卸载资源方法，可以在切换场景等场合主动调用
        /// </summary>
        public static void UnloadAsset()
        {
            _bundleAssetLoader.UnloadAsset();
        }

        /// <summary>
        /// 卸载资源方法
        /// unloadAllLoadedObjects false:保留已经加载了的资源， true:卸载所有资源
        /// </summary>
        private static void UnloadAll(bool unloadAllLoadedObjects)
        {
            _bundleAssetLoader.UnloadAll(unloadAllLoadedObjects);
        }

        /// <summary>
        /// 重新初始化bundle相关数据，一般配合热更新逻辑使用
        /// </summary>

        public static void UnloadBundleAndReInit()
        {
            UnloadAll(false);
            AssetBundleUtils.ReInit();
#if USE_SPRITE_ATLAS_MAP
            AtlasAssetHelper.InitSpriteAtlasMap();
#endif
        }

        public static void Dump()
        {
            if (_bundleAssetLoader != null)
            {
                _bundleAssetLoader.Dump();
            }
        }

        private static void OnLoad(string path)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (OnLoadEvent != null)
            {
                OnLoadEvent(path);
            }
#endif
        }

        #region Subpackage

        /// <summary>
        /// 下载分包资源，使用多线程
        /// </summary>
        /// <param name="maxDownloadingCount">最大线程数</param>
        /// <param name="downloadingProgressCallback">回调函数参数分别为已下载大小(Byte)，总大小(Byte)，平均速度(Byte/S)，下载状态，错误类型(当下载状态Abort时，使用该参数定位出错原因)。下载状态为Complete表示下载成功</param>
        /// <param name="tags">指定的标签，为空表示全部按默认顺序下载</param>
        /// <param name="isDownloadAll">是否要按默认顺序下载未指定的标签</param>
        public static void DownloadSubpackageBundles(int maxDownloadingCount = 4, Action<double, double, double, SubpackageState, SubpackageError> downloadingProgressCallback = null, string[] tags = null, bool isDownloadAll = true)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
#if DEVELOPMENT_BUILD
            Debug.Log("AssetManager.DownloadSubpackageBundles maxDownloadingCount=" + maxDownloadingCount + " isDownloadAll=" + isDownloadAll);
#endif
            if (_subpackageLoader != null)
            {
                _subpackageLoader.DownloadSubpackage(maxDownloadingCount, downloadingProgressCallback, -1, false, tags, isDownloadAll);
            }
        }

        /// <summary>
        /// 后台下载分包资源，使用两个线程并限速
        /// </summary>
        /// <param name="limitSpeed">限制最大下载速度，单位为Byte/s，能够限制的值最小为20480(Byte/s)</param>
        /// <param name="downloadingProgressCallback">回调函数参数分别为已下载大小(Byte)，总大小(Byte)，平均速度(Byte/S)，下载状态，错误类型(当下载状态Abort时，使用该参数定位出错原因)。下载状态为Complete表示下载成功</param>
        /// <param name="tags">指定的标签，为空表示全部按默认顺序下载</param>
        /// <param name="isDownloadAll">是否要按默认顺序下载未指定的标签</param>
        public static void DownloadSubpackageInBackground(double limitSpeed, Action<double, double, double, SubpackageState, SubpackageError> downloadingProgressCallback = null, string[] tags = null, bool isDownloadAll = true)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
#if DEVELOPMENT_BUILD
            Debug.Log("AssetManager.DownloadSubpackageInBackground limitSpeed=" + limitSpeed + " isDownloadAll=" + isDownloadAll);
#endif
            if (_subpackageLoader != null)
            {
                _subpackageLoader.DownloadSubpackage(1, downloadingProgressCallback, limitSpeed, true, tags, isDownloadAll);
            }
        }
        ///modify by cmm
        public static void InitProcessedChunkDictAsync()
        {
            if (_subpackageLoader != null)
            {
                (_subpackageLoader as AssetBundleLoader)?.InitProcessedChunkDictAsync();
            }
        }

        /// <summary>
        /// 分包资源是否就绪(已经下载完成)
        /// 返回false需要调用DownloadSubpackageBundles下载分包资源。
        /// 返回true 分包资源已经准备好。
        /// </summary>
        /// <returns></returns>
        public static bool IsSubpackageReady()
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return true;
            }
            if (_subpackageLoader != null)
            {
                return _subpackageLoader.IsSubpackageReady(null);
            }
            return true;
        }

        /// <summary>
        /// 分包资源是否就绪(已经下载完成)
        /// 返回false需要调用DownloadSubpackageBundles下载分包资源。
        /// 返回true 分包资源已经准备好。
        /// <param name="tag">需要判断是否完成的tag</param>
        /// <returns></returns>
        public static bool IsSubpackageReady(string tag)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return true;
            }
            if (_subpackageLoader != null)
            {
                return _subpackageLoader.IsSubpackageReady(tag);
            }
            return true;
        }

        /// <summary>
        /// 暂停下载
        /// </summary>
        public static void PauseDownloading()
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            if (_subpackageLoader != null)
            {
                _subpackageLoader.PauseDownloading();
            }
        }

        //单位 Byte（字节），设置小于0的值，不限速
        public static void SetLimitSpeed(double limitSpeed)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            if (_subpackageLoader != null)
            {
                _subpackageLoader.SetLimitSpeed(limitSpeed);
            }
        }

        /// <summary>
        /// 设置是否允许流量下载
        /// </summary>
        /// <param name="isAllowed"></param>
        public static void SetCarrierDataNetworkDownloading(bool isAllowed)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            if (_subpackageLoader != null)
            {
                _subpackageLoader.SetCarrierDataNetworkDownloading(isAllowed);
            }
        }

        /// <summary>
        /// 获取是否允许流量下载
        /// </summary>
        /// <returns></returns>
        public static bool GetCarrierDataNetworkDownloadingAllowed()
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return false;
            }
            if (_subpackageLoader != null)
            {
                return _subpackageLoader.GetCarrierDataNetworkDownloadingAllowed();
            }
            return false;
        }

        /// <summary>
        /// 修改二包资源下载Url
        /// </summary>
        /// <param name="urlsStr">只传入CDN地址，用';'分割多个Url</param>
        public static void SetCdnUrl(string urlsStr)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            if (_subpackageLoader != null)
            {
                _subpackageLoader.SetCdnUrl(urlsStr);
            }
        }

        /// <summary>
        /// 修改二包下载Url
        /// </summary>
        /// <param name="urls">传入多个完整的分包下载地址</param>
        public static void SetCdnUrl(string[] urls)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            if (_subpackageLoader != null)
            {
                _subpackageLoader.SetCdnUrl(urls);
            }
        }

        /// <summary>
        /// 获取APP是否支持后台下载功能
        /// </summary>
        /// <returns></returns>
        public static bool IsSupportBackgroundDownload()
        {
            if (_setting != null && _setting.bundleLoaderSetting != null)
            {
                return _setting.bundleLoaderSetting.isSupportBackgroundDownload;
            }
            return false;
        }

        /// <summary>
        /// 获取是否允许后台下载,当 IsSupportBackgroundDownload 返回值为true的时候，此值才有意义
        /// </summary>
        /// <returns></returns>
        public static bool IsBackgroundDownloadAllowed()
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return false;
            }
            if (_subpackageLoader != null)
            {
                return _subpackageLoader.IsBackgroundDownloadAllowed();
            }
            return false;
        }

        public static void SetAllowDownloadInBackground(bool isAllowed)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            if (_subpackageLoader != null)
            {
                _subpackageLoader.SetAllowDownloadInBackground(isAllowed);
            }
        }

        /// <summary>
        /// 后台状态下，所有任务下载成功时，如果有值，会将此值作为内容显示一个通知
        /// </summary>
        public static void SetSucNotificationStr(string str)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            if (_subpackageLoader != null)
            {
                _subpackageLoader.SetSucNotificationStr(str);
            }
        }

        /// <summary>
        /// 后台状态下，下载失败时，如果有值，会将此值作为内容显示一个通知
        /// </summary>
        public static void SetFailNotificationStr(string str)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            if (_subpackageLoader != null)
            {
                _subpackageLoader.SetFailNotificationStr(str);
            }
        }

        /// <summary>
        /// 后台状态下开启APP保活模块，某些版本的安卓设备上需要显示一个通知 注：仅安卓设备生效
        /// </summary>
        /// <param name="str">提示内容，可能会被 SetShowBackgroundDownloadProgress 覆盖 </param>
        public static void SetKeepAliveNotificationStr(string str)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            if (_subpackageLoader != null)
            {
                _subpackageLoader.SetKeepAliveNotificationStr(str);
            }
        }

        /// <summary>
        /// 后台下载时是否显示进度 注：当前仅安卓设备生效
        /// </summary>
        /// <param name="show">true：显示进度，会覆盖 SetKeepAliveNotificationStr 的提示内容  
        ///                    false：不显示进度，后台状态下载仍显示 SetKeepAliveNotificationStr 的提示内容</param>
        /// <param name="downloadingNotificationStr">下载过程中的提示内容</param>
        /// <param name="carrierDataNetworkNotificationStr">不允许移动网络下载时，变为移动网络后，暂停下载并等待wifi网络的提示</param>
        public static void SetShowBackgroundDownloadProgress(bool show, string downloadingNotificationStr = null, string carrierDataNetworkNotificationStr = null)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            if (_subpackageLoader != null)
            {
                _subpackageLoader.SetShowBackgroundDownloadProgress(show, downloadingNotificationStr, carrierDataNetworkNotificationStr);
            }
        }

        /// <summary>
        /// 修改APP保活模块后台音乐音量，0.0f～1.0f，仅iOS平台生效
        /// </summary>
        /// <param name="volum"></param>
        public static void SetKeepAliveMusicVolum(float volum)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            DownloadService.SetKeepAliveMusicVolum(volum);
        }
        /// <summary>
        /// 自定义APP保活模块后台播放的音乐，支持多条音乐，使用“;”分隔，仅iOS平台生效
        /// </summary>
        /// <param name="musicClips">StreamingAssets下的完整路径</param>
        public static void SetCustomKeepAliveMusicClips(string clips)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            DownloadService.SetCustomKeepAliveMusicClips(clips);
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
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            DownloadService.SetNotificationSmallIcon(name, type, r, g, b);
        }

        /// <summary>
        /// 设置用于检测网络状态的url，支持多个url，以分号(;)分割,仅用于iOS平台
        /// </summary>
        /// <param name="urls"></param>
        public static void SetNetworkStatusObseverUrls(string urls)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            HttpDownloadManager.SetNetworkStatusObseverUrls(urls);
        }

        /// <summary>
        /// 检查是否获取通知权限
        /// </summary>
        /// <param name="callback">回调</param>
        public static void TryCheckNotificationPermission(System.Action<bool> callback)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                callback(false);
                return;
            }
            HttpDownloadManager.TryCheckNotificationPermission(callback);
        }

        /// <summary>
        /// 跳转到系统的通知权限设置界面
        /// </summary>
        public static void JumpNotificationPermissionSetting()
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            HttpDownloadManager.JumpNotificationPermissionSetting();
        }

        public static Dictionary<string, double> GetTag2SizeDic()
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return null;
            }
            if (_subpackageLoader != null)
            {
                return _subpackageLoader.GetTag2SizeDic();
            }
            return null;
        }

        public static void GetSubPackageSize(out double totalSize, out double completeSize)
        {
            if (!IsUseSubpackage())
            {
                totalSize = 0;
                completeSize = 0;
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            if (_subpackageLoader != null)
            {
                _subpackageLoader.GetSubPackageSize(out totalSize, out completeSize);
            }
            else
            {
                totalSize = 0;
                completeSize = 0;
            }
        }

        /// <summary>
        /// 判断是否有足够磁盘空间下载二包资源
        /// </summary>
        /// <returns></returns>
        public static bool IsHardDiskEnough()
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return true;
            }
            if (_subpackageLoader != null)
            {
                return _subpackageLoader.IsHardDiskEnough();
            }
            return true;
        }

        /// <summary>
        /// 计算某个tag内所有未下载或未分割完的chunk的尺寸
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static double CalcUnCompleteChunkSizeForTag(string tag)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return 0;
            }
            if (_subpackageLoader != null)
            {
                return _subpackageLoader.CalcUnCompleteChunkSizeForTag(tag);
            }
            return 0;
        }

        /// <summary>
        /// 计算该tag需要下载的资源大小。
        /// 注意：尽量减少调用频率，如果UI展示进度使用建议每秒或多帧调用，避免每帧调用
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static double GetSizeToDownloadOfTag(string tag)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return 0;
            }
            if (_subpackageLoader != null)
            {
                return _subpackageLoader.GetSizeToDownloadOfTag(tag);
            }
            return 0;
        }

        /// <summary>
        /// 计算某个tag的资源大小
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static double GetTagSize(string tag)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return 0;
            }
            if (_subpackageLoader != null)
            {
                return _subpackageLoader.GetTagSize(tag);
            }
            return 0;
        }

        /// <summary>
        /// 是否正在记录资源的使用
        /// </summary>
        public static bool IsObservingAsset
        {
            get
            {
#if UNITY_EDITOR
                return PlayerPrefs.GetInt("IsObserving", 0) == 1;
#else
                return false;
#endif
            }
        }

        #endregion

        /// <summary>
        /// 获取资源状态
        /// </summary>
        /// <param name="path"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static LocalAssetStatus GetAssetStatus(string path, Type type)
        {
            return _bundleAssetLoader.GetAssetStatus(path, type);
        }
        
        
        /// <summary>
        /// 获取当前版本对应的ChunkList的MD5值
        /// </summary>
        /// <returns></returns>
        public static string GetChunkListMD5()
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return string.Empty;
            }
            string chunkListName = SubPackageBundleInfoContainer.LoadSubPackageInfo().ChunkListName;
            return chunkListName.Substring(0, chunkListName.IndexOf("_"));
        }
        
        /// <summary>
        /// 检查远端（CDN）资源是否就绪，可判断二包数据是否上传
        /// </summary>
        /// <param name="remote"></param>
        public static void CheckRemoteFileStatus(Action<ChunkListStatus> remote)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            if (_subpackageLoader != null)
            {
                _subpackageLoader.CheckRemoteFileStatus(remote);
            }
        }

        /// <summary>
        /// 设置加载资源失败时回调函数，Action<错误类型, 错误信息> 可以将错误信息log输出或上报
        /// </summary>
        /// <param name="listener"></param>
        public static void SetAssetLoadExceptionObserver(Action<AssetLoadErrorType, string> observer)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            if (_subpackageLoader != null)
            {
                _subpackageLoader.SetAssetLoadExceptionObserver(observer);
            }
        }


        /// <summary>
        /// 设置远程加载资源监控函数
        /// </summary>
        /// <param name="observer"></param>
        /// Action<assetPath, bundleName, bundleSize, duration, errorMsg>
        /// assetPath： 资源路径
        /// bundleName： bundle名称
        /// bundleSize： bundle大小，单位字节
        /// duration： 远程加载耗时，单位秒
        /// errorMsg： 错误信息，为null表明加载过程中没有出错
        public static void SetAssetRemoteLoadObserver(Action<string, string, int, float, string> observer)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            if (_subpackageLoader != null)
            {
                _subpackageLoader.SetAssetRemoteLoadObserver(observer);
            }
        }

        /// <summary>
        /// 获取二包tag开始下载和完成下载的状态，Action<tag名称, 是否完成下载> 注意，同一个tag可能回开启下载多次
        /// 注意开始下载可能会调用多次，需要在log中过滤重复的。
        /// </summary>
        /// <param name="listener"></param>
        public static void SetSubPackageTagStatusObserver(Action<string, bool> observer)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            if (_subpackageLoader != null)
            {
                _subpackageLoader.SetTagStatusObserver(observer);
            }
        }

        public static void SetCacheCapacity(int capactity)
        {
            if (_bundleAssetLoader != null)
            {
                _bundleAssetLoader.SetCacheCapacity(capactity);
            }
        }

        public static void SetCacheLifeCycle(float lifeCycle)
        {
            if (_bundleAssetLoader != null)
            {
                _bundleAssetLoader.SetCacheLifeCycle(lifeCycle);
            }
        }

        /// <summary>
        /// 后台状态下，下载分包到指定千分比时发起推送
        /// </summary>
        /// <param name="percent">1-999</param>
        /// <param name="notificationStr">推送内容</param>
        public static void AddPercentNotification(int percent, string notificationStr)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            if (_subpackageLoader != null)
            {
                _subpackageLoader.AddPercentNotification(percent, notificationStr);
            }
        }
        /// <summary>
        /// 清理通过AddPercentNotification添加的推送数据
        /// </summary>
        public static void ClearPercentNotification()
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            if (_subpackageLoader != null)
            {
                _subpackageLoader.ClearPercentNotification();
            }
        }

        /// <summary>
        /// 可以过滤掉不想加载的bundle,只对assetbundle模式生效
        /// </summary>
        /// <param name="abName"> bundle 名字</param>
        public static void AddBundleToBlacklist(string abName)
        {
            if (_bundleAssetLoader != null)
            {
                _bundleAssetLoader.AddBundleToBlacklist(abName);
            }
        }

        /// <summary>
        /// 判断场景是否已加载
        /// </summary>
        /// <param name="sceneName"></param>
        /// <returns>true：已加载 false：未加载</returns>
        public static bool IsSceneLoaded(string sceneName)
        {
            return _bundleAssetLoader.IsSceneLoaded(sceneName);
        }

        /// <summary>
        /// 设置二包网络问题下载失败时是否自动重试
        /// </summary>
        /// <returns></returns>
        public static void SetIsAutoRetryDownloading(bool value)
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return;
            }
            if (_subpackageLoader != null)
            {
                _subpackageLoader.SetIsAutoRetryDownloading(value);
            }
        }

        /// <summary>
        /// 获取二包网络问题下载失败时是否自动重试
        /// </summary>
        /// <returns></returns>
        public static bool GetIsAutoRetryDownloading()
        {
            if (!IsUseSubpackage())
            {
                Debug.LogWarning("SubPackage is not open,check setting!");
                return false;
            }
            if (_subpackageLoader != null)
            {
                return _subpackageLoader.GetIsAutoRetryDownloading();
            }
            return false;
        }

        /// <summary>
        /// 当前设置是否开启二包
        /// </summary>
        /// <returns></returns>
        private static bool IsUseSubpackage()
        {
            if (_setting == null)
            {
                Debug.LogError("AssetManagerSetting is null.");
                return false;
            }
            if (_setting.bundleLoaderSetting == null)
            {
                Debug.LogError("AssetBundleLoaderSetting is null.");
                return false;
            }
            return _setting.assetLoaderType == AssetLoaderType.AssetBundle && _setting.bundleLoaderSetting.useSubPackage;
        }
    }
}
