using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ZeusAssetBuild
{
    [Flags]
    public enum AssetFlag
    {
        None    = 0X00,
        ExplicitAsset = 0x01,
        ExplicitSharedAsset = 0x02,
        SharedAsset = 0x04,
    }
    public class ZeusAssetNode
    {
        private string _assetPath;
        private AssetFlag _flag = AssetFlag.None;
        private string _bundleName;
        private SortedSet<string> _depBundleSet = new SortedSet<string>();
        private SortedSet<string> _explicitSharedBundle = new SortedSet<string>();

        public ZeusAssetNode(string assetPath)
        {
            _assetPath = assetPath;
        }

        public ZeusAssetNode(string assetPath, string bundleName)
        {
            _assetPath = assetPath;
            _bundleName = bundleName;
        }

        public string AssetPath { get { return _assetPath; } }
        public string BundleName { get { return _bundleName; }  set { _bundleName = value; } }
        public AssetFlag Flag { get { return _flag; } set { _flag = value; } }
        public SortedSet<string> DepBundleSet { get { return _depBundleSet; } }

        public SortedSet<string> ExplicitSharedBundle { get { return _explicitSharedBundle; } }

        public bool IsExplicitSharedAsset() 
        {
            if (_flag.HasFlag(AssetFlag.ExplicitAsset))
                return false;
            if (_flag.HasFlag(AssetFlag.ExplicitSharedAsset))
            {
                return _explicitSharedBundle.IsSupersetOf(_depBundleSet);
            }
            return false;
        }

        public bool IsExplicitAsset()
        {
            return _flag.HasFlag(AssetFlag.ExplicitAsset);
        }
    }

}
