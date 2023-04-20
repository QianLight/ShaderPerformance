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
    /*AssetLoadOperation 异步方式后去资源，当不在使用资源时需要主动调用Dispose接口*/
    public class AssetLoadOperation : CustomYieldInstruction, IDisposable
    {
        public event Action<AssetLoadOperation> Completed;
        private bool _alreadyDisposed = false;
        private IAssetRef _assetRef = null;
        private bool _isDone = false;
        private AssetLoadErrorType _errorType = AssetLoadErrorType.None;

        public bool IsDone { get { return _isDone; } }

        public override bool  keepWaiting
        {
            get { return !_isDone; }
        }

        public UnityEngine.Object AssetObject
        {
            get
            {
                if (_alreadyDisposed)
                {
                    Debug.LogError("AssetLoadOperation has be disposed");
                    return null;
                }

                if(_assetRef != null)
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

        internal AssetLoadOperation(string assetPath, Type type, Action<AssetLoadOperation> complete)
        {
            SetComplete(complete);
            LoadFunction(assetPath, type);
        }

        internal virtual void SetComplete(Action<AssetLoadOperation> complete)
        {
            Completed += complete;
        }

        internal virtual void LoadFunction(string assetPath, Type type)
        {
            AssetManager.LoadAssetAsync(assetPath,type,OnAssetLoadCallback, null);
        }

        ~AssetLoadOperation()
        {
            Dispose();
        }

        internal void OnAssetLoadCallback(IAssetRef assetRef, System.Object param)
        {
            _isDone = true;
            if (_alreadyDisposed)
                return;

            _assetRef = assetRef;
            if(_assetRef != null)
            {
                _assetRef.Retain();
            }
            if (Completed != null)
            {
                Completed(this);
            }
        }
    }

    public class AssetLoadOperationUgent: AssetLoadOperation
    {
        internal AssetLoadOperationUgent(string assetPath, Type type, Action<AssetLoadOperation> complete): base(assetPath, type, complete)
        {

        }

        internal override void SetComplete(Action<AssetLoadOperation> complete)
        {
            Completed += complete;
        }

        internal override void LoadFunction(string assetPath, Type type)
        {
            AssetManager.LoadAssetUrgent(assetPath, type, OnAssetLoadCallback, null);
        }
    }
}
