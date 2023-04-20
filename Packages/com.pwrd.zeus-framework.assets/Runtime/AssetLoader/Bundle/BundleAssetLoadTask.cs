/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;
using System.Collections.Generic;
using System;

namespace Zeus.Framework.Asset
{
    class BundleAssetLoadTask
    {
        public enum LoadStep
        {
            WaitingBundle,
            LoadingAsset,
            WaitingCallback,
            Finish,
        };

        public enum LoadPriority
        {
            Normal = 0,
            High = 5,
            Immediately = 6,
        }

        private BundleRef _bundleRef;
        private AssetBundleRequest _assetBundleRequest;
        private string _assetName;
        private string _bundleName;
        private string _assetPath;
        private Type _type;
        private LoadStep _step;
        private LoadPriority _loadPriority;
        private List<AssetLoadCallbackData> _callbackList;
        private AssetLoadErrorType _errorType;
#if ZEUS_ASSETS_PROFILER_LOG
        private System.DateTime _beginTime;
#endif
        public BundleAssetLoadTask(string assetPath, string pAssetName, Type type, string pBundleName)
        {
            _assetPath = assetPath;
            _bundleName = pBundleName;
            _assetName = pAssetName;
            _type = type;
            _step = LoadStep.WaitingBundle;
            _callbackList = new List<AssetLoadCallbackData>(1);
            _loadPriority = LoadPriority.Normal;
        }

        public BundleRef BundleRef
        {
            get { return _bundleRef; }
            set { _bundleRef = value; }
        }

        public AssetBundleRequest AssetBundleRequest
        {
            get { return _assetBundleRequest; }
            set
            {
                _assetBundleRequest = value;
                _assetBundleRequest.priority = (int)_loadPriority;
#if ZEUS_ASSETS_PROFILER_LOG
                _beginTime = System.DateTime.Now;
#endif
            }
        }

        public void AddCallback(Action<IAssetRef, object> callback, object param)
        {
            _callbackList.Add(new AssetLoadCallbackData(callback, param));
        }

        public void ExecuteCallBack(IAssetRef assetRef)
        {
            for (int i = 0; i < _callbackList.Count; i++)
            {
                try
                {
                    _callbackList[i].Execute((assetRef == null || assetRef.AssetObject == null) ? null : assetRef);
                }
                catch(Exception ex)
                {
                    Debug.LogException(ex);
                }
                if (assetRef != null)
                {
                    assetRef.Release();
                }
            }
            _callbackList.Clear();
        }

        public string AssetName { get { return _assetName; } }
        public string BundleName { get { return _bundleName; } }
        public Type AssetType { get { return _type; } }
        public string AssetPath { get { return _assetPath; } }
        public LoadPriority Priority
        {
            get { return _loadPriority; }
            set
            {
                _loadPriority = value;
                if (_assetBundleRequest != null)
                {
                    _assetBundleRequest.priority = (int)_loadPriority;
                }
            }
        }

        public LoadStep Step
        {
            get { return _step; }
            set { _step = value; }
        }

        public AssetLoadErrorType ErrorType { get { return _errorType; } set { _errorType = value; } }
        public class AssetLoadTaskStepComparer : IComparer<BundleAssetLoadTask>
        {
            public int Compare(BundleAssetLoadTask x, BundleAssetLoadTask y)
            {
                int rtn = x.Step - y.Step;
                return rtn;
            }
        }

        public class AssetLoadTaskPriorityComparer : IComparer<BundleAssetLoadTask>
        {
            public int Compare(BundleAssetLoadTask x, BundleAssetLoadTask y)
            {
                int rtn = y.Priority - x.Priority;
                return rtn;
            }
        }

        public bool IsDone()
        {
            if (_loadPriority == LoadPriority.Immediately)
            {
                return _assetBundleRequest.asset != null;
            }
            else
            {
#if ZEUS_ASSETS_PROFILER_LOG
                if (_assetBundleRequest.isDone)
                {
                    var timespan = System.DateTime.Now - _beginTime;
                    Debug.Log("BundleAssetLoadTask load asset " + _assetName + " from "+ _bundleName + " cost:" + timespan.TotalSeconds);
                }
#endif
                return _assetBundleRequest.isDone;
            }
        }
    }
}