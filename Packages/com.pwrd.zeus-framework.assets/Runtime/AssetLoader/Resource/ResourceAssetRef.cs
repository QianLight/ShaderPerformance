/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus.Framework.Asset
{
    //public for lua wrap file
    public class ResourceAssetRef : IAssetRef
    {
        private UnityEngine.Object _assetObject;
        private string _name;
        private string _assetPath;
        private int _refCount;

        public ResourceAssetRef(string name, string assetPath, Object assetObject)
        {
            _name = name;
            _assetPath = assetPath;
            _assetObject = assetObject;
            _refCount = 0;
        }

        /// <summary>
        /// 需要持有该资源时，调用该函数
        /// </summary>
        public void Retain()
        {
            _refCount++;
        }

        /// <summary>
        /// 不在持有该资源时，调用该函数
        /// </summary>
        public void Release()
        {
            _refCount--;
        }

        /// <summary>
        /// 获取资源对象
        /// </summary>
        public UnityEngine.Object AssetObject
        {
            get { return _assetObject; }
        }

        /// <summary>
        /// 资源名称
        /// </summary>
        public string AssetName { get { return _name; } }

        /// <summary>
        /// 资源路径
        /// </summary>
        public string AssetPath { get { return _assetPath; } }

        public int RefCount { get { return _refCount; } }
    }
}

