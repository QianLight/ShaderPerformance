/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

namespace Zeus.Framework.Asset
{
    public enum LocalAssetStatus
    {
        NonExistent, //资源不存在（路径拼写错误等，例如大小写拼写错误）
        Ready, //资源就绪   
        AsyncLoading, //资源或者依赖的bundle在异步加载过程中，此时同步加载会导致游戏卡顿
        LocalNonExistent, //本地不存在，在二包中，还没有下载到
    }

    interface IResourceLoader
    {
        IAssetRef LoadAsset(string path, Type type);

        bool IsAssetCached(string path);

        void LoadAssetAsync(string path, Type type, Action<IAssetRef, object> callback, object param);

        void LoadAssetUrgent(string path, Type type, Action<IAssetRef, object> callback, object param);

        void LoadScene(string path, LoadSceneMode loadMode);

        IEnumerator LoadSceneAsync(string path, LoadSceneMode loadMode);

        void LoadSceneAsync(string path, LoadSceneMode loadMode, Action<bool, float, object> callback, object param);

        void UnloadSceneAsync(string path, Action<bool, float, object> callback, object param);

        bool IsSceneLoaded(string path);

        IEnumerator UnloadSceneAsync(string path);

        void UnloadAsset();

        void UnloadAll(bool unloadAllLoadedObjects);

        void Dump();

        LocalAssetStatus GetAssetStatus(string path, Type type);

        void SetCacheCapacity(int capactity);

        void SetCacheLifeCycle(float lifeCycle);

        void SetAssetLevel(string level);

        void AddBundleToBlacklist(string abName);
    }
}

