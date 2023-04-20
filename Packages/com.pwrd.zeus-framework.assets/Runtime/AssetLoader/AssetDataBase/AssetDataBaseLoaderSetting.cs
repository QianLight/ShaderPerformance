/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus.Framework.Asset
{
    [System.Serializable]
    class AssetDataBaseLoaderSetting
    {
        public bool AddAssetPathPrefix = true;
        public string AssetPathPrefix;
        public bool enableAssetLevel = false;

        public AssetDataBaseLoaderSetting()
        {
            AddAssetPathPrefix = true;
            AssetPathPrefix = "Resources/";
            enableAssetLevel = false;
        }
    }
}

