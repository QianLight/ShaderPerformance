/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using UnityEngine.U2D;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Zeus.Core;

namespace Zeus.Framework.Asset
{
    public class AssetDataBaseLoader : IResourceLoader
    {
        string pathPrefix = "";
        AssetDataBaseLoaderSetting _setting;
        Dictionary<string, EditorAssetRef> _assetDict;

        private class AsyncLoadParam
        {
            string _path;
            Type _type;
            Action<IAssetRef, object> _callback;
            object _param;
            public AsyncLoadParam(string path, Type type, Action<IAssetRef, object> callback, object param)
            {
                this._path = path;
                this._type = type;
                this._callback = callback;
                this._param = param;
            }

            public string Path { get { return _path; } }
            public Type Type { get { return _type; } }
            public Action<IAssetRef, object> Callback { get { return _callback; } }
            public object Param { get { return _param; } }
        }

        internal AssetDataBaseLoader(AssetDataBaseLoaderSetting setting)
        {
            this._setting = setting;
            _assetDict = new Dictionary<string, EditorAssetRef>();
            if (Application.isPlaying)
            {
                ZeusCore.Instance.RegisterUpdate(this.Update);
            }
        }

        public bool IsAssetCached(string path)
        {
            return true;
        }

        private int frameCount = 0;
        public void Update()
        {
            frameCount++;
            if(frameCount % 3 == 0)
            {
                CollectAsset();
            }
            if (Input.GetKeyUp(KeyCode.F1))
            {
                Debug.Log("AssetDataBaseLoader GC");
                GameObjectHelper.ClearAllInvalidCache();
                Resources.UnloadUnusedAssets();
                GC.Collect();
            }
        }

        private List<string> _unloadList = new List<string>();
        private void CollectAsset()
        {
            _unloadList.Clear();
            foreach (var pair in _assetDict)
            {
                if(pair.Value.RefCount <= 0)
                {
                    _unloadList.Add(pair.Key);
                    if (pair.Value.RefCount < 0)
                    {
                        Debug.LogWarningFormat("AssetDataBaseLoader.CollectAsset: {0} refCount = {1}", pair.Value.AssetName, pair.Value.RefCount);
                    }
                }
            }
            foreach(var assetPath in _unloadList)
            {
                //Debug.Log("AssetDataBaseLoader CollectAsset: " + assetPath);
                _assetDict.Remove(assetPath);
            }
        }

        public IAssetRef LoadAsset(string path, Type type)
        {
            if (_setting.enableAssetLevel)
            {
                var newPath = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.AssetDatabase);
                var assetRef = _LoadAsset(newPath, type, false);
                if (assetRef != null)
                {
                    return assetRef;
                }
                else
                {
                    return _LoadAsset(path, type, true);
                }
            }
            else
            {
                return _LoadAsset(path, type, true);
            }
        }

        private IAssetRef _LoadAsset(string path, Type type, bool log)
        {
#if UNITY_EDITOR
            EditorAssetRef assetRef = null;
            if (_assetDict.TryGetValue(path, out assetRef))
            {
                return assetRef;
            }
            var realPath = TryAddPrefixPath(path);
            realPath = realPath.Replace('\\', '/');
            var ext = AssetExtHelper.GetFileExt(realPath, type, log);
            realPath = realPath + ext;
            
            var obj = AssetDatabase.LoadAssetAtPath(realPath, type);
            
            if (obj == null)
            {
                if (log)
                {
                    Debug.LogWarning("AssetManager Not found asset: " + realPath);
                }
                return null;
            }
            else
            {
                var assetPath = AssetDatabase.GetAssetPath(obj);
                //传入的参数不带后缀名,则路径不区分后缀名，将后缀名统一
                if(!string.IsNullOrEmpty(ext))
                {
                    var extension = Path.GetExtension(assetPath);
                    assetPath = assetPath.Substring(0, assetPath.Length - extension.Length);
                    assetPath = assetPath + ext;
                }
                //对比路径，确保大小写一致
//                 if(assetPath != realPath)
//                 {
//                     if (log)
//                     {
//                         Debug.LogError("资源加载失败，请检查大小写拼写错误 " + path + " : " + assetPath);
//                     }
//                     return null;
//                 }
//                 else
                {
                    assetRef = new EditorAssetRef(path, path, obj);;
                    _assetDict.Add(path, assetRef);
                    return assetRef;
                }
            }
#else
            return null;
#endif
        }

        public void LoadAssetAsync(string path, Type type, Action<IAssetRef, object> callback, object param)
        {
            Zeus.Core.ZeusCore.Instance.StartCoroutine(FakeLoadAssetAsync(new AsyncLoadParam(path, type, callback, param)));
        }

        private IEnumerator FakeLoadAssetAsync(AsyncLoadParam param)
        {
            yield return null;
            var newPath = _setting.enableAssetLevel ? AssetLevelManager.Instance.GetMappedPath(param.Path, AssetLoaderType.AssetDatabase) : param.Path;
            if (newPath != param.Path)
            {
                var assetRef = _LoadAsset(newPath, param.Type, false);
                if (assetRef != null && assetRef.AssetObject != null)
                {
                    if (param.Callback != null) param.Callback(assetRef, param.Param);
                }
                else
                {
                    assetRef = _LoadAsset(param.Path, param.Type, true);
                    if (param.Callback != null) param.Callback(assetRef, param.Param);
                }
            }
            else
            {
                var assetRef = _LoadAsset(param.Path, param.Type, true);
                if (param.Callback != null) param.Callback(assetRef, param.Param);
            }
        }

        public void LoadAssetUrgent(string path, Type type, Action<IAssetRef, object> callback, object param)
        {
            this.LoadAssetAsync(path, type, callback, param);
        }

        public void LoadScene(string path, LoadSceneMode loadMode)
        {
            path = TryAddPrefixPath(path);
            path = TryRemoveAssetsPrefix(path);
            path = AssetBundleUtils.RemoveSceneFileExtension(path);
            if (_setting.enableAssetLevel)
            {
                var newPath = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.AssetDatabase);
                if (newPath != path && SceneUtility.GetBuildIndexByScenePath(newPath) > 0)
                {
                    path = newPath;
                }
            }
            SceneManager.LoadScene(path, loadMode);
        }

        public void LoadSceneAsync(string path, LoadSceneMode loadMode, Action<bool, float, object> callback, object param)
        {
            path = TryAddPrefixPath(path);
            path = TryRemoveAssetsPrefix(path);
            path = AssetBundleUtils.RemoveSceneFileExtension(path);
            if (_setting.enableAssetLevel)
            {
                var newPath = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.AssetDatabase);
                if (newPath != path && SceneUtility.GetBuildIndexByScenePath(newPath) > 0)
                {
                    path = newPath;
                }
            }
            AsyncOperation loadOperation = SceneManager.LoadSceneAsync(path, loadMode);
            ZeusCore.Instance.StartCoroutine(SceneProcessCoroutine(loadOperation, callback, param));
        }

        public IEnumerator LoadSceneAsync(string path, LoadSceneMode loadMode)
        {
            path = TryAddPrefixPath(path);
            path = TryRemoveAssetsPrefix(path);
            path = AssetBundleUtils.RemoveSceneFileExtension(path);
            if (_setting.enableAssetLevel)
            {
                var newPath = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.AssetDatabase);
                if (newPath != path && SceneUtility.GetBuildIndexByScenePath(newPath) > 0)
                {
                    path = newPath;
                }
            }
            yield return SceneManager.LoadSceneAsync(path, loadMode);
        }

        public void UnloadSceneAsync(string path, Action<bool, float, object> callback, object param)
        {
            path = TryAddPrefixPath(path);
            path = TryRemoveAssetsPrefix(path);
            path = AssetBundleUtils.RemoveSceneFileExtension(path);
            if (_setting.enableAssetLevel)
            {
                var newPath = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.AssetDatabase);
                if (newPath != path && SceneUtility.GetBuildIndexByScenePath(newPath) > 0)
                {
                    path = newPath;
                }
            }

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
            path = TryAddPrefixPath(path);
            path = TryRemoveAssetsPrefix(path);
            path = AssetBundleUtils.RemoveSceneFileExtension(path);
            if (_setting.enableAssetLevel)
            {
                var newPath = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.AssetDatabase);
                if (newPath != path && SceneUtility.GetBuildIndexByScenePath(newPath) > 0)
                {
                    path = newPath;
                }
            }
            yield return SceneManager.UnloadSceneAsync(path);
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

        public void UnloadAsset()
        {

        }

        public void UnloadAll(bool unloadAllLoadedObjects)
        {

        }

        private string TryAddPrefixPath(string path)
        {
            if (_setting.AddAssetPathPrefix && !path.StartsWith("Packages") && !path.StartsWith("Assets"))
            {
                path = _setting.AssetPathPrefix + path;
            }
            return path;
        }

        private string TryRemoveAssetsPrefix(string path)
        {
            if(path.StartsWith("Assets/"))
            {
                int prefixLength = "Assets/".Length;
                path = path.Substring(prefixLength, path.Length - prefixLength);
            }
            return path;
        }

        public void Dump()
        {
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            strBuilder.Append("_cachedAssetDict.count:");
            strBuilder.Append(_assetDict.Count);
            strBuilder.AppendLine();
            foreach (KeyValuePair<string, EditorAssetRef> pair in _assetDict)
            {
                strBuilder.Append(pair.Value.AssetPath);
                strBuilder.Append(" : ");
                strBuilder.Append(pair.Value.RefCount);
                strBuilder.AppendLine();
            }

            Debug.Log(strBuilder.ToString());

            AssetDumpHelper.GenerateDumpSnapshotFile(_assetDict);
        }

        public LocalAssetStatus GetAssetStatus(string path, Type type)
        {
            var assetRef = _LoadAsset(path, type, false);
            if(assetRef == null)
            {
                return LocalAssetStatus.NonExistent;
            }
            return LocalAssetStatus.Ready;
        }

        public void SetCacheCapacity(int capactity)
        {
            //do nothing
        }

        public void SetCacheLifeCycle(float lifeCycle)
        {
            //do nothing
        }

        public void SetAssetLevel(string level)
        {
            if (_setting.enableAssetLevel)
            {
                AssetLevelManager.Instance.SetLevel(level);
            }
            else
            {
                //Debug.LogError("Please Enable AssetDataBaseLoader AssetLevel function first");
            }
        }

        public void AddBundleToBlacklist(string abName)
        {
            //do nothing
        }

        public bool IsSceneLoaded(string path)
        {
            path = TryAddPrefixPath(path);
            path = TryRemoveAssetsPrefix(path);
            path = AssetBundleUtils.RemoveSceneFileExtension(path);
            if (_setting.enableAssetLevel)
            {
                var newPath = AssetLevelManager.Instance.GetMappedPath(path, AssetLoaderType.AssetDatabase);
                if (newPath != path && SceneUtility.GetBuildIndexByScenePath(newPath) > 0)
                {
                    path = newPath;
                }
            }
            var targetScene = SceneManager.GetSceneByPath(path);
            if(targetScene != null && targetScene.isLoaded)
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
