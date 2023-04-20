/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Runtime.CompilerServices;

namespace Zeus.Framework.Asset
{

    public class UnloadSceneAsyncAwaitOperation : AsyncAwaitOperation
    {
        internal UnloadSceneAsyncAwaitOperation(string path)
        {
            AssetManager.UnloadSceneAsync(path, OnSceneUnLoadCallback, null);
        }

        internal void OnSceneUnLoadCallback(bool isDone, float progress, System.Object param)
        {
            if (isDone)
            {
                _isDone = true;
                InvokeComplete();
            }
        }
    }
}
