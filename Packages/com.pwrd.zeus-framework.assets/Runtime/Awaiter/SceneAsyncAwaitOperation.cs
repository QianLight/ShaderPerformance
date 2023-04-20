/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

namespace Zeus.Framework.Asset
{

    public class SceneAsyncAwaitOperation : AsyncAwaitOperation
    {
        internal SceneAsyncAwaitOperation(string path, LoadSceneMode loadMode)
        {
            AssetManager.LoadSceneAsync(path, loadMode, OnSceneLoadCallback, null);
        }

        internal void OnSceneLoadCallback(bool isDone, float progress, System.Object param)
        {
            if (isDone)
            {
                _isDone = true;
                InvokeComplete();
            }
        }
    }
}
