/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using Zeus.Core.Collections;

namespace Zeus.Framework.Asset
{
    class DefaultBundleCollector : IBundleCollector
    {
        DefaultBundleCollectorSetting _setting;
        private ConcurrentQueue<string> _toCheckBundleQueue;
        private Queue<string> _delayCheckBundleQueue;
        private LRUCache<string, BundleRef> _bundleCache;

        private int _cacheBundleCount;
        private float _cacheBundleLifeCycle;

        public DefaultBundleCollector(DefaultBundleCollectorSetting setting)
        {
            _setting = setting;
            _cacheBundleCount = _setting.cacheBundleCount;
            _cacheBundleLifeCycle = _setting.cacheBundleLifeCycle;
            _toCheckBundleQueue = new ConcurrentQueue<string>();
            _delayCheckBundleQueue = new Queue<string>();
            _bundleCache = new LRUCache<string, BundleRef>(_cacheBundleCount);
        }

        public void CollectAll(Dictionary<string, BundleRef> cachedBundleDict, Dictionary<string, BundleAssetRef> cachedAssetDict)
        {
            this.CollectInteral(cachedBundleDict, cachedAssetDict, int.MaxValue, true);
        }

        public void Collect(Dictionary<string, BundleRef> cachedBundleDict, Dictionary<string, BundleAssetRef> cachedAssetDict)
        {
            this.CollectInteral(cachedBundleDict, cachedAssetDict, _setting.maxGcNumPerFrame, false);
        }

        public void SetCachedBundleLifeCycle(float lifeCycle)
        {
            if (lifeCycle > 0)
            {
                _cacheBundleLifeCycle = lifeCycle;
                Debug.Log("SetCachedBundleLifeCycle: " + lifeCycle);
            }
            else
            {
                UnityEngine.Debug.LogWarning("cached bundle life cycley must > 0");
            }
        }

        public void SetCachedBundleLimit(int limit)
        {
            if (limit >= 0)
            {
                _cacheBundleCount = limit;
                _bundleCache.SetMaxCapacity(_cacheBundleCount);
                Debug.Log("SetCachedBundleLimit: " + _cacheBundleCount);
            }
            else
            {
                UnityEngine.Debug.LogWarning("cached bundle limit must > 0");
            }
        }

        List<BundleRef> tempBundleList = new List<BundleRef>();
        private void CollectInteral(Dictionary<string, BundleRef> cachedBundleDict, Dictionary<string, BundleAssetRef> cachedAssetDict, int maxCollect, bool clearCache)
        {
            int count = 0;
            _delayCheckBundleQueue.Clear();
            string bundleName = null;
            bool releaseAll = false;
            if (_bundleCache.Count > 0)
            {
                _bundleCache.Remove(this.FilterReferencedBundle);
            }

            while (_toCheckBundleQueue.TryDequeue(out bundleName))
            {
                //Debug.Log("CollectInteral check bundle: " + bundleName);
                BundleRef bundleRef = null;
                if (cachedBundleDict.TryGetValue(bundleName, out bundleRef))
                {
                    if (bundleRef.IsDelayRelease)
                    {
                        _delayCheckBundleQueue.Enqueue(bundleName);
                        continue;
                    }
                    if (bundleRef.RefCount == 0 || bundleRef.CanReleaseCircularRef())
                    {
                        //循环依赖的bundle不做缓存策略
                        if (_cacheBundleCount > 0 && bundleRef.CircularDepCount <= 0)
                        {
                            if (!_bundleCache.ContainsKey(bundleRef.BundleName))
                            {
                                BundleRef outBundleRef = null;
                                bundleRef.CachedTime = Time.realtimeSinceStartup;
                                if (_bundleCache.Add(bundleRef.BundleName, bundleRef, out outBundleRef))
                                {
                                    UnloadBundle(outBundleRef, cachedBundleDict, cachedAssetDict);
                                    //Debug.Log("-->LRU cached bundle unload " + outBundleRef.BundleName + " " + (Time.realtimeSinceStartup - outBundleRef.CachedTime) + " COUNT: " + _bundleCache.Count);
                                    count++;
                                }
                            }
                        }
                        else
                        {
                            //有循环依赖的情况需要一次将循环内的bundle都卸载完成，避免出现部分卸载情况
                            if (bundleRef.CircularDepCount > 0)
                                releaseAll = true;

                            UnloadBundle(bundleRef, cachedBundleDict, cachedAssetDict);
                            count++;
                        }
                    }
                    else
                    {
                        var unloadCount = bundleRef.TryUnloadAsset(cachedAssetDict, maxCollect - count);
                        if (unloadCount > 0)
                        {
                            count += unloadCount;
                        }
                    }
                }

                if (!releaseAll && maxCollect > 0 && count >= maxCollect)
                {
                    break;
                }
            }
            while (_delayCheckBundleQueue.Count > 0)
            {
                _toCheckBundleQueue.Enqueue(_delayCheckBundleQueue.Dequeue());
            }

            if (_bundleCache.Count > 0)
            {
                if (clearCache)
                {
                    foreach (KeyValuePair<string, BundleRef> pair in _bundleCache)
                    {
                        if (pair.Value.RefCount <= 0 || pair.Value.CanReleaseCircularRef())
                        {
                            UnloadBundle(pair.Value, cachedBundleDict, cachedAssetDict);
                        }
                    }
                    _bundleCache.Clear();
                }
                else
                {
                    tempBundleList.Clear();
                    foreach (KeyValuePair<string, BundleRef> pair in _bundleCache)
                    {
                        if (pair.Value.RefCount <= 0 || pair.Value.CanReleaseCircularRef())
                        {
                            if (Time.realtimeSinceStartup - pair.Value.CachedTime > _cacheBundleLifeCycle)
                            {
                                tempBundleList.Add(pair.Value);
                            }
                        }
                        if (maxCollect > 0 && tempBundleList.Count >= maxCollect)
                        {
                            break;
                        }
                    }
                    foreach (var bundleRef in tempBundleList)
                    {
                        UnloadBundle(bundleRef, cachedBundleDict, cachedAssetDict);
                        _bundleCache.Remove(bundleRef.BundleName);
                    }
                    tempBundleList.Clear();
                }
            }
        }


        private bool FilterReferencedBundle(BundleRef bundleRef)
        {
            return bundleRef.RefCount > 0;
        }

        private void UnloadBundle(BundleRef bundleRef, Dictionary<string, BundleRef> cachedBundleDict, Dictionary<string, BundleAssetRef> cachedAssetDict)
        {
            if ((bundleRef.RefCount <= 0 || bundleRef.CanReleaseCircularRef()) && cachedBundleDict.ContainsKey(bundleRef.BundleName))
            {
                //Debug.Log("DefaultBundleGcStrategy unload bundle: " + bundleRef.BundleName);
                cachedBundleDict.Remove(bundleRef.BundleName);
                foreach (BundleAssetRef assetRef in bundleRef.AssetRefList)
                {
                    //Debug.Log("unload asset: " + assetRef.AssetPath);
                    cachedAssetDict.Remove(assetRef.AssetPath);
                }
                bundleRef.UnloadBundle();
            }
        }

        public void OnRefCountToZero(BundleRef bundleRef)
        {
            if (bundleRef != null && !string.IsNullOrEmpty(bundleRef.BundleName))
            {
                _toCheckBundleQueue.Enqueue(bundleRef.BundleName);
            }
        }
    }
}

