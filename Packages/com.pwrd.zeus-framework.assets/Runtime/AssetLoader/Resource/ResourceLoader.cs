/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;
using Zeus.Core;

namespace Zeus.Framework.Asset
{
    class ResourceLoader : IResourceLoader
    {
        private Dictionary<string, ResourceAssetRef> _cachedAssetDict;
        private Dictionary<string, ResourceAssetLoadTask> _assetLoadTaskDict;
        private Queue<ResourceAssetLoadTask> _assetToLoadTaskQueue;
        private int _parallelLoadCount;
        private IResourceCollector _resourceCollector;
        private ResourceLoaderSetting _setting;

        public ResourceLoader(ResourceLoaderSetting setting)
        {
            _cachedAssetDict = new Dictionary<string, ResourceAssetRef>();
            _assetLoadTaskDict = new Dictionary<string, ResourceAssetLoadTask>();
            _assetToLoadTaskQueue = new Queue<ResourceAssetLoadTask>();
            _parallelLoadCount = 0;
            _resourceCollector = new DefaultResourceCollector();
            ZeusCore.Instance.RegisterUpdate(this.Update);
            _setting = setting;
        }

        public bool IsAssetCached(string path)
        {
            return true;
        }

        public IAssetRef LoadAsset(string path, Type type)
        {
            path = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.Resources);
            ResourceAssetRef assetRef = null;
            if (_cachedAssetDict.TryGetValue(path, out assetRef))
            {
                return assetRef;
            }
            Object obj = Resources.Load(path, type);
            if(obj == null)
            {
                Debug.LogError("Asset not found! Please check asset path: " + path);
                return null;
            }
            assetRef = new ResourceAssetRef(path, path, obj);
            _cachedAssetDict.Add(assetRef.AssetPath, assetRef);
            return assetRef;
        }

        public void LoadAssetAsync(string path, Type type, Action<IAssetRef, object> callback, object param)
        {
            path = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.Resources);
            ResourceAssetRef assetRef = null;
            if (_cachedAssetDict.TryGetValue(path, out assetRef))
            {
                if (callback != null) callback(assetRef, param);
                return;
            }

            ResourceAssetLoadTask task = null;
            if (_assetLoadTaskDict.TryGetValue(path, out task))
            {
                task.AddCallback(callback, param);
            }
            else
            {
                task = new ResourceAssetLoadTask(path, type);
                task.AddCallback(callback, param);
                _assetLoadTaskDict.Add(path, task);
                if (_setting.MaxLoadingNum <= 0 || _parallelLoadCount <= _setting.MaxLoadingNum)
                {
                    _parallelLoadCount++;
                    task.Start();
                }
                else
                {
                    _assetToLoadTaskQueue.Enqueue(task);
                }
            }
        }

        public void LoadAssetUrgent(string path, Type type, Action<IAssetRef, object> callback, object param)
        {
            this.LoadAssetAsync(path, type, callback, param);
        }

        public void LoadScene(string path, LoadSceneMode loadMode)
        {
            path = TryAddPathPrefix(path);
            path = AssetBundleUtils.RemoveSceneFileExtension(path);
            path = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.Resources);
            SceneManager.LoadScene(path, loadMode);
        }

        public void LoadSceneAsync(string path, LoadSceneMode loadMode, Action<bool, float, object> callback, object param)
        {
            path = TryAddPathPrefix(path);
            path = AssetBundleUtils.RemoveSceneFileExtension(path);
            path = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.Resources);
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(path, loadMode);
            ZeusCore.Instance.StartCoroutine(SceneProcessCoroutine(loadOperation, callback, param));
        }

        public IEnumerator LoadSceneAsync(string path, LoadSceneMode loadMode)
        {
            path = TryAddPathPrefix(path);
            path = AssetBundleUtils.RemoveSceneFileExtension(path);
            path = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.Resources);
            yield return SceneManager.LoadSceneAsync(path, loadMode);
        }

        public void UnloadSceneAsync(string path, Action<bool, float, object> callback, object param)
        {
            path = TryAddPathPrefix(path);
            path = AssetBundleUtils.RemoveSceneFileExtension(path);
            path = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.Resources);
            AsyncOperation asyncOperation = SceneManager.UnloadSceneAsync(path);
            if (asyncOperation == null)
            {
                if (callback != null) callback(true, 1.0f, param);
            }
            else
            {
                ZeusCore.Instance.StartCoroutine(SceneProcessCoroutine(asyncOperation, callback, param));
            }
        }

        public IEnumerator UnloadSceneAsync(string path)
        {
            path = TryAddPathPrefix(path);
            path = AssetBundleUtils.RemoveSceneFileExtension(path);
            path = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.Resources);
            yield return SceneManager.UnloadSceneAsync(path);
        }

        public void UnloadAsset()
        {
            _resourceCollector.CollectAll(_cachedAssetDict);
            Resources.UnloadUnusedAssets();
        }

        public void UnloadAll(bool unloadAllLoadedObjects)
        {

        }

        public LocalAssetStatus GetAssetStatus(string path, Type type)
        {
            var assetRef = LoadAsset(path, type);
            if(assetRef == null)
            {
                return LocalAssetStatus.NonExistent;
            }
            return LocalAssetStatus.Ready;
        }

        private IEnumerator SceneProcessCoroutine(AsyncOperation asyncOperation, Action<bool, float, object> callback, object param)
        {
            while (!asyncOperation.isDone)
            {
                if (callback != null)
                {
                    callback(false, asyncOperation.progress, param);
                }
                yield return null;
            }
            if (callback != null)
            {
                callback(true, asyncOperation.progress, param);
            }
        }

        private void Update()
        {
            CheckAssetLoadTask();
            _resourceCollector.Collect(_cachedAssetDict);
        }

        private List<ResourceAssetLoadTask> _tempTaskList = new List<ResourceAssetLoadTask>();
        private void CheckAssetLoadTask()
        {
            if (_assetLoadTaskDict.Count > 0)
            {
                _tempTaskList.Clear();
                foreach (KeyValuePair<string, ResourceAssetLoadTask> pair in _assetLoadTaskDict)
                {
                    ResourceAssetLoadTask task = pair.Value;
                    if (task.IsDone())
                    {
                        _tempTaskList.Add(task);
                    }
                }

                for (int i = 0; i < _tempTaskList.Count; i++)
                {
                    ResourceAssetLoadTask task = _tempTaskList[i];
                    _assetLoadTaskDict.Remove(task.AssetPath);
                    if(task.AssetObject != null)
                    {
                    ResourceAssetRef assetRef = null;
                    if (!_cachedAssetDict.TryGetValue(task.AssetPath, out assetRef))
                    {
                        assetRef = new ResourceAssetRef(task.AssetPath, task.AssetPath, task.AssetObject);
                        _cachedAssetDict.Add(assetRef.AssetPath, assetRef);
                    }
                    task.ExecuteCallBack(assetRef);
                }
                    else
                    {
                        task.ExecuteCallBack(null);
                    }
                }
                _parallelLoadCount = _parallelLoadCount - _tempTaskList.Count;
                if (_tempTaskList.Count > 0 && _assetToLoadTaskQueue.Count > 0)
                {
                    while(_parallelLoadCount < _setting.MaxLoadingNum && _assetToLoadTaskQueue.Count > 0)
                    {
                        ResourceAssetLoadTask loadTask = _assetToLoadTaskQueue.Dequeue();
                        loadTask.Start();
                        _parallelLoadCount++;
                    }
                }
            }
        }

        private string TryAddPathPrefix(string path)
        {
            if (_setting.AddScenePathPrefix)
            {
                path = _setting.ScenePathPrefix + path;
            }
            return path;
        }

        public void Dump()
        {
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            strBuilder.Append("_cachedAssetDict.count:");
            strBuilder.Append(_cachedAssetDict.Count);
            strBuilder.AppendLine();
            foreach (KeyValuePair<string, ResourceAssetRef> pair in _cachedAssetDict)
            {
                strBuilder.Append(pair.Value.AssetPath);
                strBuilder.Append(" : ");
                strBuilder.Append(pair.Value.RefCount);
                strBuilder.AppendLine();
            }

            Debug.Log(strBuilder.ToString());

            AssetDumpHelper.GenerateDumpSnapshotFile(_cachedAssetDict);
        }

        public void SetCacheCapacity(int capactity) 
        {
            //do nothing
        }

        public void SetCacheLifeCycle(float lifeCycle) 
        {
            //do nothing
        }

        public void AddBundleToBlacklist(string abName)
        {
            //do nothing
        }

        public void SetAssetLevel(string level)
        {
            AssetLevelManager.Instance.SetLevel(level);
        }

        public bool IsSceneLoaded(string path)
        {
            path = TryAddPathPrefix(path);
            path = AssetBundleUtils.RemoveSceneFileExtension(path);
            path = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.Resources);
            var targetScene = SceneManager.GetSceneByPath(path);
            if (targetScene != null && targetScene.isLoaded)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

