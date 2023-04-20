/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Zeus.Framework.Asset
{
    class AssetLoadCallbackData
    {
        private Action<IAssetRef, object> _callback;
        private object _param;

        public AssetLoadCallbackData(Action<IAssetRef, object> callback, object param)
        {
            _callback = callback;
            _param = param;
        }

        public void Execute(IAssetRef assetRef)
        {
            try
            {
                _callback.Invoke(assetRef, _param);
            }
            catch (System.Exception ex)
            {
                Debug.LogErrorFormat("exception in Load asset callback : {0}  {1}" ,ex.Message, ex.StackTrace);
            }
        }
    }
}

