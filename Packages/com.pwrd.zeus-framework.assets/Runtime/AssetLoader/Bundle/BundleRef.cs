/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Threading;

namespace Zeus.Framework.Asset
{
    public class BundleRef : IBundleRef
    {
        private int _refcount;
        private string _bundleName;
        private AssetBundle _assetBundle;
        private LinkedList<BundleAssetRef> _assetRefList;
        private List<BundleRef> _depBundleList;
        private IBundleCollector _collector;
        private bool _isUnloadble = false;
        private float _cachedTime;
        private int _circularDepCount;
        private static HashSet<string> _circluarCheckedSet = new HashSet<string>();
        private BundleFlag _flag = BundleFlag.None;


        [Flags]
        private enum BundleFlag
        {
            None = 0,
            //远程加载中
            REMOTE_LOADING = 1,
            //音频加载中
            AUDIO_LOADING = 1 << 1,

        }

        public BundleRef(string pBundleName, IBundleCollector collector, int depCount = 0)
        {
            _assetRefList = new LinkedList<BundleAssetRef>();
            _bundleName = pBundleName;
            _assetBundle = null;
            _refcount = 0;
            _circularDepCount = -1;
            if (depCount == 0)
            {
                _depBundleList = null;
            }
            else
            {
                _depBundleList = new List<BundleRef>(depCount);
            }
            _collector = collector;
            collector.OnRefCountToZero(this);
            _isUnloadble = AssetBundleUtils.IsUnloadbleBundle(pBundleName);
            CalculateCircularDepCount();
        }

        public BundleRef(string pBundleName, AssetBundle pAssetBundle, IBundleCollector collector, int depCount = 0)
        {
            _assetRefList = new LinkedList<BundleAssetRef>();
            _bundleName = pBundleName;
            _assetBundle = pAssetBundle;
            _refcount = 0;
            if (depCount == 0)
            {
                _depBundleList = null;
            }
            else
            {
                _depBundleList = new List<BundleRef>(depCount);
            }
            _collector = collector;
            collector.OnRefCountToZero(this);
            _isUnloadble = AssetBundleUtils.IsUnloadbleBundle(pBundleName);
            CalculateCircularDepCount();
        }

        public void AddDepBundle(BundleRef bundleRef)
        {
            bundleRef.Retain();
            _depBundleList.Add(bundleRef);
        }

        public bool CanReleaseCircularRef()
        {
            _circluarCheckedSet.Clear();
            if (CircularDepCount > 0)
            {
                _circluarCheckedSet.Add(BundleName);
                return _CanReleaseCircularRef(_circluarCheckedSet);
            }
            else
            {
                return RefCount <= 0;
            }
        }

        private bool _CanReleaseCircularRef(HashSet<string> checkedSet)
        {
            if (RefCount > CircularDepCount)
            {
                return false;
            }
            for (int i = 0; i < _depBundleList.Count; i++)
            {
                var bundleRef = _depBundleList[i];
                if (checkedSet.Add(bundleRef.BundleName))
                {
                    if (bundleRef.CircularDepCount > 0 && !bundleRef._CanReleaseCircularRef(checkedSet))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void CalculateCircularDepCount()
        {
            _circluarCheckedSet.Clear();
            _circularDepCount = _CalculateCircularDepCount(BundleName, AssetBundleUtils.GetDirectDependencies(BundleName), _circluarCheckedSet);
        }

        private int _CalculateCircularDepCount(string bundleName, string[] depNames, HashSet<string> checkedSet)
        {
            int count = 0;
            for (int i = 0; i < depNames.Length; i++)
            {
                if (depNames[i] == bundleName)
                {
                    count++;
                }
                else
                {
                    if (checkedSet.Add(depNames[i]))
                    {
                        count += _CalculateCircularDepCount(bundleName, AssetBundleUtils.GetDirectDependencies(depNames[i]), checkedSet);
                    }
                }
            }
            return count;
        }

        public string BundleName { get { return _bundleName; } }

        public void Retain()
        {
            Interlocked.Increment(ref _refcount);
            //Debug.Log("[" + bundleName + "  " + refcount + "]");
        }

        public void Release(BundleAssetRef assetRef)
        {
            Interlocked.Decrement(ref _refcount);
            //Debug.Log("Release bundle [" + _bundleName + "  " + _refcount + "]");
            if (_refcount == 0 || (assetRef != null && assetRef.RefCount == 0) || _refcount <= _circularDepCount)
            {
                _collector.OnRefCountToZero(this);
            }
            if (_refcount < 0)
            {
                Debug.LogWarning("Please check, assetRef.Retain() and Release() don't match!");
            }
        }

        public int RefCount { get { return _refcount; } }
        public LinkedList<BundleAssetRef> AssetRefList { get { return _assetRefList; } }

        public bool IsDelayRelease 
        { 
            get 
            {
                if (_flag == BundleFlag.None)
                {
                    return false;
                }
                else if ((_flag & BundleFlag.REMOTE_LOADING) == BundleFlag.REMOTE_LOADING)
                {
                    return true;   
                }
                else if ((_flag & BundleFlag.AUDIO_LOADING) == BundleFlag.AUDIO_LOADING)
                {
                    if (_assetRefList.Count > 0 && (_assetRefList.First.Value.AssetObject as AudioClip).loadState == AudioDataLoadState.Loaded)
                    {
                        _flag &= (~BundleFlag.AUDIO_LOADING);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return false;
                }
            }
        }

        public void EnableRemoteLoading(bool enabled)
        {
            if (enabled)
            {
                _flag |= BundleFlag.REMOTE_LOADING;
            }
            else
            {
                _flag &= (~BundleFlag.REMOTE_LOADING);
            }
        }

        public int CircularDepCount { get { return _circularDepCount; } }
        public void SetAssetBundle(AssetBundle bundle)
        {
            _assetBundle = bundle;
            if(bundle == null)
            {
                Debug.LogError("Bundle async load failed : " + _bundleName);
            }
        }

        public void LoadAssetRef(ref BundleAssetRef bundleAssetRef, string assetPath, string assetName, Type type)
        {
            try
            {
                _assetRefList.AddLast(bundleAssetRef);
                if (_assetBundle != null)
                {
                    UnityEngine.Object assetObject = _assetBundle.LoadAsset(assetName, type);
                    if (assetObject == null)
                    {
                        Debug.LogError("BundleRef " + _bundleName + " not contains asset: " + assetName);
                    }

                    bundleAssetRef.AssetObject = assetObject;

                    if (_isUnloadble)
                    {
                        if(assetObject is AudioClip)
                        {
                            var audioclip = assetObject as AudioClip;
                            //Debug.LogWarning("LoadAssetRef :" + assetPath + " : " + _bundleName + " : " + audioclip.loadType);
                            _flag |= BundleFlag.AUDIO_LOADING;
                        }
                        else
                        {
                            _assetBundle.Unload(false);
                            _assetBundle = null;
                        }
                    }
                }
                else
                {
                    //Debug.LogError("Load asset \"" + assetPath + "\" failed: bundle load failed");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogException(ex);
                Debug.LogError("Load asset \"" + assetPath + "\" failed.");
            }
        }

        public AssetBundleRequest LoadAssetAsync(string assetName, Type type)
        {
            AssetBundleRequest request = null;
            try
            {
                if (_assetBundle != null)
                {
                    request = _assetBundle.LoadAssetAsync(assetName, type);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(e);
                Debug.LogError("Load asset \"" + assetName + "\" failed.");
            }
            return request;
        }

        public void AddBundleAssetRef(BundleAssetRef bundleAssetRef)
        {
            Debug.Assert(bundleAssetRef.BundleName == this._bundleName);
            _assetRefList.AddLast(bundleAssetRef);
            if (_isUnloadble && _assetBundle != null)
            {
                if (bundleAssetRef.AssetObject != null && bundleAssetRef.AssetObject is AudioClip)
                {
                    var audioclip = bundleAssetRef.AssetObject as AudioClip;
                    //Debug.LogWarning("LoadAssetRef :" + bundleAssetRef.AssetPath + " : " + _bundleName + " : " + audioclip.loadType);
                    _flag |= BundleFlag.AUDIO_LOADING;
                }
                else
                {
                    _assetBundle.Unload(false);
                    _assetBundle = null;
                }
            }
        }

        public void UnloadBundle(bool unloadAllLoadedObjects = true)
        {
            if (_assetBundle != null)
            {
                _assetBundle.Unload(unloadAllLoadedObjects);
                _assetBundle = null;
                AssetsLogger.Log("UnloadBundle", _bundleName);
            }
            else
            {
                LinkedListNode<BundleAssetRef> assetNode = _assetRefList.First;
                while (assetNode != null)
                {
                    var assetRef = assetNode.Value;
                    if (assetRef.IsUnloadable())
                    {
                        Resources.UnloadAsset(assetRef.AssetObject);
                        //AssetsLogger.Log("UnloadAsset [", assetRef.AssetPath, "]");
                    }
                    assetNode = assetNode.Next;
                }
            }
            _assetRefList.Clear();
            if (_refcount != 0 && _refcount > _circularDepCount)
            {
                Debug.LogWarning("BundleRef :" + _bundleName + " UnloadBundle refcount not zero but " + _refcount);
            }
            if (_depBundleList != null)
            {
                for (int i = 0; i < _depBundleList.Count; i++)
                {
                    _depBundleList[i].Release(null);
                }
                _depBundleList.Clear();
            }
        }

        public int TryUnloadAsset(Dictionary<string, BundleAssetRef> cachedAssetDict, int maxCount)
        {
            int count = 0;
            LinkedListNode<BundleAssetRef> assetNode = _assetRefList.First;
            bool beDepended = IsBeDepended();
            while (assetNode != null)
            {
                var next = assetNode.Next;
                var assetRef = assetNode.Value;
                if (assetRef.RefCount <= 0)
                {
                    if (assetRef.IsUnloadable() && !beDepended)
                    {
                        Resources.UnloadAsset(assetRef.AssetObject);
                        AssetsLogger.Log("TryUnloadAsset [", assetRef.AssetPath, "]");
                    }
                    _assetRefList.Remove(assetNode);
                    assetRef.AssetObject = null;
                    cachedAssetDict.Remove(assetRef.AssetPath);
                    count++;
                }

                assetNode = next;
                if (maxCount > 0 && count >= maxCount)
                {
                    break;
                }
            }
            if (_isUnloadble && _assetBundle != null && _assetRefList.Count > 0)
            {
                _assetBundle.Unload(false);
                _assetBundle = null;
            }
            return count;
        }

        private bool IsBeDepended()
        {
            return true;
            LinkedListNode<BundleAssetRef> assetNode = _assetRefList.First;
            int assetRefCount = 0;
            while (assetNode != null)
            {
                if (assetNode.Value.RefCount > 0)
                    assetRefCount++;

                assetNode = assetNode.Next;
            }
            return assetRefCount < this.RefCount;
        }

        public bool IsBundleUnloaded()
        {
            return _assetBundle == null;
        }

        public float CachedTime
        {
            get { return _cachedTime; }
            set { _cachedTime = value; }
        }
    }
}