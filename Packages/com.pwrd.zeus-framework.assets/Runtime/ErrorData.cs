/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus.Framework.Asset
{
    public class ErrorData
    {
        AssetLoadErrorType _errorType;
        string _assetPath;

        public AssetLoadErrorType ErrorType { get { return _errorType; } set { _errorType = value; } }
        public string AssetPath { get { return _assetPath; } set { _assetPath = value; } }
    }
}

