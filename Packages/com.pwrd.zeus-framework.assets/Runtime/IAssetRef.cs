/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using UnityEngine;

namespace Zeus.Framework.Asset
{
	/// <summary>
    /// 资源引用接口，通过该接口使用资源
    /// </summary>
    public interface IAssetRef
	{
        /// <summary>
        /// 需要持有该资源时，调用该函数
        /// </summary>
        void Retain();

        /// <summary>
        /// 不在持有该资源时，调用该函数
        /// </summary>
        void Release();

        /// <summary>
        /// 引用该资源的个数
        /// </summary>
        int RefCount{get;}

		/// <summary>
        /// 获取资源对象
        /// </summary>
        UnityEngine.Object AssetObject{get;}

        /// <summary>
        /// 资源名称
        /// </summary>
        string AssetName{get;}

        /// <summary>
        /// 资源路径
        /// </summary>
        string AssetPath{get;}
	}
}

