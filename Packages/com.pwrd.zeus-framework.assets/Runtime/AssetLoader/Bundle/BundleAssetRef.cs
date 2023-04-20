/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using UnityEngine;
using System.Threading;

namespace Zeus.Framework.Asset
{
    //for lua wrap reasion, set public
    public class BundleAssetRef : IAssetRef
    {
        private IBundleRef _bundleRef;
        private string _bundleName;
        private string _assetName;
        private string _assetPath;
        private int _refCount;
        private UnityEngine.Object _assetObject;

        public BundleAssetRef(string assetPath, string pBundleName, string pAssetName, UnityEngine.Object pAssetObject, IBundleRef pBundleRef)
        {
            _assetPath = assetPath;
            _bundleName = pBundleName;
            _assetName = pAssetName;
            _assetObject = pAssetObject;
            _bundleRef = pBundleRef;
            _refCount = 0;
        }

        public void Retain()
        {
            Interlocked.Increment(ref _refCount);
            //Debug.Log("Retain asset [" + _assetPath + "  " + _refCount + "]");
            if (_refCount == 1)
            {
                _bundleRef.Retain();
            }
        }

        public void Release()
        {
            Interlocked.Decrement(ref _refCount);
            //Debug.Log("Release asset [" + _assetPath + "  " + _refCount + "]");
            if (_refCount == 0)
            {
                _bundleRef.Release(this);
            }
        }

        public int RefCount { get { return _refCount; } }
        public UnityEngine.Object AssetObject { get { return _assetObject; } set { _assetObject = value; } }
        public string AssetName { get { return _assetName; } }
        public string AssetPath { get { return _assetPath; } }
        public IBundleRef BundleRef { get { return _bundleRef; } }
        public String BundleName { get { return _bundleName; } }

        public bool IsUnloadable()
        {
            if(_assetObject != null)
            {
                var assetType = _assetObject.GetType();
                if (assetType == typeof(GameObject) ||
                        assetType == typeof(AudioClip) ||
                    assetType == typeof(MonoBehaviour) ||
                    assetType == typeof(Component) ||
                    assetType == typeof(Behaviour))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                //Debug.LogWarning("IsUnloadable asset is null " + _bundleName + " : " + _assetName);
                return false;
            }
            
        }
    }
}

