/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;

namespace Zeus.Framework.Asset
{
    class ObsoleteAssetAdpter
    {
        IResourceLoader _loader;
        Dictionary<int, IAssetRef> id2AssetRefDict = new Dictionary<int, IAssetRef>();
        public ObsoleteAssetAdpter(IResourceLoader loader)
        {
            _loader = loader;
        }


        public Object GetAsset(string path)
        {
            return GetAsset(path, typeof(Object));
        }

       
        public Object GetAsset(string path, Type type)
        {
            IAssetRef assetRef = _loader.LoadAsset(path, type);
            AddAsset(assetRef);
            return assetRef.AssetObject;
        }

        
        public void GetAssetAsync(string path, Action<Object, object> callback, object param)
        {
            GetAssetAsync(path, typeof(Object), callback, param);
        }

        
        public void GetAssetAsync(string path, Type type, Action<Object, object> callback, object param)
        {
            _loader.LoadAssetAsync(path, 
                                    type, 
                                    (IAssetRef assetRef, object para) =>
                                    {
                                        AddAsset(assetRef);
                                        if (callback != null) callback(assetRef.AssetObject, param);
                                    }, 
                                    param);
        }

        public void GetAssetAsyncUrgent(string path, Action<Object, object> callback, object param)
        {
            GetAssetAsyncUrgent(path, typeof(Object), callback, param);
        }

        public void GetAssetAsyncUrgent(string path, Type type, Action<Object, object> callback, object param)
        {
            _loader.LoadAssetUrgent(path,
                                    type,
                                    (IAssetRef assetRef, object para) =>
                                    {
                                        AddAsset(assetRef);
                                        if (callback != null) callback(assetRef.AssetObject, param);
                                    },
                                    param);
        }

        public void UnuseAsset(Object asset)
        {
            IAssetRef assetRef = null;
            if (id2AssetRefDict.TryGetValue(asset.GetInstanceID(), out assetRef))
            {
                assetRef.Release();
                if (assetRef.RefCount <= 0)
                {
                    id2AssetRefDict.Remove(asset.GetInstanceID());
                }
            }
            else
            {
                Debug.LogError("UnuseAsset error not found asset");
            }
        }

        private void AddAsset(IAssetRef assetRef)
        {
            int id = assetRef.AssetObject.GetInstanceID();
            if (!id2AssetRefDict.ContainsKey(id))
            {
                id2AssetRefDict.Add(id, assetRef);
            }
            else
            {
                id2AssetRefDict[id] = assetRef;
            }
            assetRef.Retain();
        }

    }
}

