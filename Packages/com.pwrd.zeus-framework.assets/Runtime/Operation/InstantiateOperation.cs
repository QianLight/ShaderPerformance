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
    /*InstantiateOperation 异步方式后去资源，当不在使用资源时需要主动调用Dispose接口*/
    public class InstantiateOperation : CustomYieldInstruction, IDisposable
    {
        [Flags]
        enum TransformFlag
        {
            None = 0,
            Position = 1,
            Rotation = 2,
            Scale = 3,
            inWorld = 4
        }

        public event Action<InstantiateOperation> Completed;
        private bool _alreadyDisposed = false;
        private IAssetRef _assetRef = null;
        private bool _isDone = false;
        private AssetLoadErrorType _errorType = AssetLoadErrorType.None;
        private GameObject _gameObj = null;
        private Transform _parent = null;
        private Vector3 _position;
        private Quaternion _rotation;
        private Vector3 _scale;
        private TransformFlag _transformFlag;
        

        public bool IsDone { get { return _isDone; } }

        public override bool  keepWaiting
        {
            get { return !_isDone; }
        }

        public GameObject GameObject
        {
            get
            {
                if (_alreadyDisposed)
                {
                    Debug.LogError("InstantiateOperation has be disposed");
                    return null;
                }

                return _gameObj;
            }
        }

        public void Dispose() 
        {
            Dispose(true);
        }

        public void Dispose(bool destoryObj)
        {
            if (_alreadyDisposed)
                return;

            if (_assetRef != null)
            {
                _assetRef.Release();
                _assetRef = null;
            }
            if(destoryObj && _gameObj != null)
            {
                GameObject.Destroy(_gameObj);
            }
            _gameObj = null;
            //set disposed flag:
            _alreadyDisposed = true;
            GC.SuppressFinalize(this);
        }

        internal void ReSet()
        {
            Completed = null;
            _assetRef = null;
            _gameObj = null;
            _parent = null;
            _transformFlag = TransformFlag.None;
        }

        internal InstantiateOperation(string assetPath, Transform parent, bool tranform2Parent, Action<InstantiateOperation> complete)
        {
            _parent = parent;
            if (parent != null && tranform2Parent)
            {
                _position = Vector3.zero;
                _scale = Vector3.one;
                _rotation = Quaternion.Euler(0, 0, 0);
                _transformFlag = TransformFlag.Position | TransformFlag.Rotation | TransformFlag.Scale;
            }
            else
            {
                _transformFlag = TransformFlag.inWorld;
            }
            SetComplete(complete);
            LoadFunction(assetPath, typeof(GameObject));
        }

        internal InstantiateOperation(string assetPath, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent, bool instantiateInWorldSpace = true, Action<InstantiateOperation> complete = null)
        {
            _position = position;
            _rotation = rotation;
            _scale = scale;
            _parent = parent;
            _transformFlag =  TransformFlag.Position | TransformFlag.Rotation | TransformFlag.Scale;
            if (instantiateInWorldSpace)
            {
                _transformFlag |= TransformFlag.inWorld;
            }
            SetComplete(complete);
            LoadFunction(assetPath, typeof(GameObject));
        }

        internal InstantiateOperation(string assetPath, Vector3 position, Quaternion rotation, Vector3 scale, Action<InstantiateOperation> complete)
        {
            _position = position;
            _rotation = rotation;
            _scale = scale;
            _transformFlag = TransformFlag.inWorld | TransformFlag.Position | TransformFlag.Rotation | TransformFlag.Scale;
            SetComplete(complete);
            LoadFunction(assetPath, typeof(GameObject));
        }

        internal virtual void SetComplete(Action<InstantiateOperation> complete)
        {
            Completed += complete;
        }

        internal virtual void LoadFunction(string assetPath, Type type)
        {
            AssetManager.LoadAssetAsync(assetPath, type, OnAssetLoadCallback, null);
        }

        ~InstantiateOperation()
        {
            if (_gameObj != null)
            {
                Zeus.Core.ZeusCore.Instance.AddMainThreadTask(DestoryObject, _gameObj);
            }
            Dispose(false);
        }

        private static void DestoryObject(object target)
        {
            var obj = target as GameObject;
            GameObject.Destroy(obj);
        }

        internal void OnAssetLoadCallback(IAssetRef assetRef, System.Object param)
        {
            _isDone = true;
            if (_alreadyDisposed)
                return;

            if(assetRef != null && assetRef.AssetObject != null)
            {
                _assetRef = assetRef;
                _assetRef.Retain();

                _gameObj = (GameObject)GameObject.Instantiate(_assetRef.AssetObject);
                if (_gameObj)
                {
                    
                    if (_transformFlag.HasFlag(TransformFlag.inWorld))
                    {
                        if (_transformFlag.HasFlag(TransformFlag.Position))
                        {
                            _gameObj.transform.position = _position;
                        }
                        if (_transformFlag.HasFlag(TransformFlag.Rotation))
                        {
                            _gameObj.transform.rotation = _rotation;
                        }
                        if (_transformFlag.HasFlag(TransformFlag.Scale))
                        {
                            _gameObj.transform.localScale = _scale;
                        }
                    }
                    if (_parent != null)
                    {
                        _gameObj.transform.parent = _parent;
                    }
                    if (!_transformFlag.HasFlag(TransformFlag.inWorld))
                    {
                        if (_transformFlag.HasFlag(TransformFlag.Position))
                        {
                            _gameObj.transform.localPosition = _position;
                        }
                        if (_transformFlag.HasFlag(TransformFlag.Rotation))
                        {
                            _gameObj.transform.localRotation = _rotation;
                        }
                        if (_transformFlag.HasFlag(TransformFlag.Scale))
                        {
                            _gameObj.transform.localScale = _scale;
                        }
                    }
                }
            }
            if (Completed != null)
            {
                Completed(this);
            }
        }
    }
}
