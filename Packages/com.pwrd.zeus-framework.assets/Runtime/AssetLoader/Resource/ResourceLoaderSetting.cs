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
    class ResourceLoaderSetting
    {

        public int MaxLoadingNum = 10;
        public bool AddScenePathPrefix = false;
        public string ScenePathPrefix;
        public ResourceLoaderSetting()
        {
            MaxLoadingNum = 10;
            AddScenePathPrefix = false;
            ScenePathPrefix = "Resources/";
        }
    }
}

