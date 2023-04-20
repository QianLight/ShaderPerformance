/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus.Framework.Asset
{
    public class GameObjectHelper
    {
        private static Dictionary<GameObject, IAssetRef> _ObjectPool = new Dictionary<GameObject, IAssetRef>();

        public static GameObject Instantiate(IAssetRef assetRef)
        {
            if(assetRef == null || assetRef.AssetObject == null)
            {
                Debug.LogError("[GameObjectHelper.Instantiate] assetRef is invalid");
                return null;
            }
            GameObject obj = assetRef.AssetObject as GameObject;
            if(obj == null)
            {
                Debug.LogError("[GameObjectHelper.Instantiate] assetRef.AssetObject type is invalid");
                return null;
            }

            GameObject newObj = GameObject.Instantiate<GameObject>(obj);
            if(newObj == null)
            {
                Debug.LogError("[GameObjectHelper.Instantiate] Instantiate failed");
                return null;
            }
            _ObjectPool.Add(newObj, assetRef);
            assetRef.Retain();
            return newObj;
        }

        public static GameObject InstantiateObjToParent(IAssetRef assetRef, Transform parentTransform, string name = null)
        {
            if (null == assetRef || null == assetRef.AssetObject || null == parentTransform)
            {
                return null;
            }

            GameObject newObj = Instantiate(assetRef);
            if (null == newObj)
            {
                return null;
            }

            if (!SetObjParent(newObj, parentTransform))
            {
                return null;
            }

            if (null != name)
            {
                newObj.name = name;
            }
            return newObj;
        }

        public static bool SetObjParent(GameObject child, Transform parent)
        {
            if (null == child || null == parent)
            {
                return false;
            }
            child.transform.parent = parent;
            child.transform.localPosition = Vector3.zero;
            child.transform.localScale = Vector3.one;
            child.transform.localRotation = Quaternion.Euler(0, 0, 0);
            return true;
        }

        public static void Destroy(GameObject obj)
        {
            if (obj != null)
            {
                _Destroy(obj);
            }
        }

        private static void _Destroy(GameObject obj)
        {
            IAssetRef assetRef = null;
            if (_ObjectPool.TryGetValue(obj, out assetRef))
            {
                _ObjectPool.Remove(obj);
                if (obj)
                {
                    GameObject.Destroy(obj);
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning("Destory obj has been destoryed, maybe asset leak : " + assetRef.AssetPath);
#endif
                }
                assetRef.Release();
            }
            else
            {
                Debug.LogError("[GameObjectHelper.Destory] not found target obj : " + (obj ? obj.name : ""));
            }
        }

        static List<GameObject> _disposedObjectList = new List<GameObject>();
        public static void ClearAllInvalidCache()
        {
            _disposedObjectList.Clear();
            foreach (var pair in _ObjectPool)
            {
                if(!pair.Key)
                {
                    _disposedObjectList.Add(pair.Key);
                }
            }
            for(int i = 0; i < _disposedObjectList.Count; i++)
            {
                _Destroy(_disposedObjectList[i]);
            }
            _disposedObjectList.Clear();
        }

        public static void DestoryAll()
        {
            foreach(var pair in _ObjectPool)
            {
                if(pair.Key)
                {
                    GameObject.Destroy(pair.Key);
                }
                pair.Value.Release();
            }
            _ObjectPool.Clear();
        }
    }
}

