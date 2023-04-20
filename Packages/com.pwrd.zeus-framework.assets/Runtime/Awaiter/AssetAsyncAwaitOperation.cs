/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Zeus.Framework.Asset
{
    /*AssetAsyncAwaitOperation 异步方式后去资源，当不在使用资源时需要主动调用Dispose接口*/
    public class AssetAsyncAwaitOperation : AsyncAwaitOperation, IDisposable
    {
        private bool _alreadyDisposed = false;
        private IAssetRef _assetRef = null;
        private AssetLoadErrorType _errorType = AssetLoadErrorType.None;

        public UnityEngine.Object AssetObject
        {
            get
            {
                if (_alreadyDisposed)
                {
                    Debug.LogError("AssetAsyncAwaitOperation has be disposed");
                    return null;
                }

                if (_assetRef != null)
                {
                    return _assetRef.AssetObject;
                }
                return null;
            }
        }


        public void Dispose()
        {
            if (_alreadyDisposed)
                return;

            if (_assetRef != null)
            {
                _assetRef.Release();
                _assetRef = null;
            }
            //set disposed flag:
            _alreadyDisposed = true;
            GC.SuppressFinalize(this);
        }

        internal AssetAsyncAwaitOperation(string assetPath, Type type, bool isUrgent)
        {
            LoadFunction(assetPath, type, isUrgent);
        }

        internal virtual void LoadFunction(string assetPath, Type type, bool isUrgent)
        {
            if (isUrgent)
            {
                AssetManager.LoadAssetUrgent(assetPath, type, OnAssetLoadCallback, null);
            }
            else
            {
                AssetManager.LoadAssetAsync(assetPath, type, OnAssetLoadCallback, null);
            }
        }

        ~AssetAsyncAwaitOperation()
        {
            Dispose();
        }

        internal void OnAssetLoadCallback(IAssetRef assetRef, System.Object param)
        {
            _isDone = true;
            if (_alreadyDisposed)
                return;

            _assetRef = assetRef;
            if (_assetRef != null)
            {
                _assetRef.Retain();
            }
            InvokeComplete();
        }
    }
}
