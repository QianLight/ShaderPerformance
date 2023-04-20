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
    public class AssetSequenceLoader
    {
        private static AssetSequenceLoader mLoader = null;
        private Queue<AssetSequenceItem> _queue = new Queue<AssetSequenceItem>();
        private Queue<AssetSequenceItem> _pool = new Queue<AssetSequenceItem>();

        private AssetSequenceLoader() { }

        public static AssetSequenceLoader GetInstacne()
        {
            if(mLoader == null)
            {
                mLoader = new AssetSequenceLoader();
            }
            return mLoader;
        }

        private class AssetSequenceItem
        {
            IAssetRef _assetRef;
            Action<IAssetRef, object> _callback;
            object _param;
            bool _isReady = false;

            public bool IsReady { get { return _isReady; } set { _isReady = false; } }
            public object Param { set { _param = value; } }
            public Action<IAssetRef, object> Callback { set { _callback = value; } }

            public AssetSequenceItem(Action<IAssetRef, object> callback, object param)
            {
                this._callback = callback;
                this._param = param;
            }

            public void SetAssetRef(IAssetRef assetRef)
            {
                _isReady = true;
                _assetRef = assetRef;
                if(_assetRef != null)
                {
                    _assetRef.Retain();
                }
            }

            public void DoCallback()
            {
                try
                {
                    this._callback(_assetRef, this._param);
                }
                finally
                {
                    if (_assetRef != null)
                    {
                        _assetRef.Release();
                    }
                }
            }
        }

        public void LoadAssetAsync(string path, Type type, Action<IAssetRef, object> callback, object param)
        {

            AssetSequenceItem seqItem = null;
            if(_pool.Count == 0)
            {
                seqItem = new AssetSequenceItem(callback, param);
            }
            else
            {
                seqItem = _pool.Dequeue();
                seqItem.Callback = callback;
                seqItem.Param = param;
            }
            _queue.Enqueue(seqItem);
            AssetManager.LoadAssetAsync(path, type, this.OnAssetReady, seqItem);
        }

        public void OnAssetReady(IAssetRef assetRef, object param)
        {
            Debug.Assert(_queue.Count > 0, "OnAssetReady _queue should not be empty");
            var seqItem = param as AssetSequenceItem;
            seqItem.SetAssetRef(assetRef);
            while (_queue.Count > 0)
            {
                seqItem = _queue.Peek();
                
                if(seqItem != null && seqItem.IsReady)
                {
                    _queue.Dequeue();
                    seqItem.DoCallback();
                    seqItem.Callback = null;
                    seqItem.Param = null;
                    seqItem.IsReady = false;
                    _pool.Enqueue(seqItem);
                }
                else
                {
                    break;
                }
            }
        }
    }
}

