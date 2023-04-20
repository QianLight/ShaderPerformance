using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ZeusAssetBuild
{
    public enum BundleType
    {
        ExplicitBundle,
        ExtractSharedBundle
    }
    public class ZeusBundleNode
    {
        private string _bundleName;
        private BundleType _bundleType;
        private List<ZeusAssetNode> _assetList;

        public ZeusBundleNode(string bundleName, List<ZeusAssetNode> assetList)
        {
            this._bundleName = bundleName;
            this._assetList = assetList;
        }

        public BundleType BundleType
        {
            get { return _bundleType; }
            set { _bundleType = value; }
        }

        public List<ZeusAssetNode> AssetList { get { return _assetList; } }
        public string BundleName { get { return _bundleName; } }
    }
}

