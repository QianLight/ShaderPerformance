/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using UnityEngine;

namespace Zeus.Framework
{
    public class GameObjectPool
    {
        private int _maxSize;
        private int _size;
        private string _name;
        private GameObject _prefabObject;
        private List<GameObject> _availableObjects = new List<GameObject>();

        public GameObjectPool(string poolName, int initSize, int maxSize, GameObject prefabObject)
        {
            _name = poolName;
            _maxSize = maxSize;
            _size = initSize;
            _prefabObject = prefabObject;

            for (int initIdx = 0; initIdx < _size; initIdx++)
            {
                AddObjectToPool(AllocateNewObject());
            }
        }

        // Create a new instance using prefab.
        private GameObject AllocateNewObject()
        {
            return GameObject.Instantiate(_prefabObject) as GameObject;
        }

        // Add new object to the pool.
        private void AddObjectToPool(GameObject go)
        {
            go.SetActive(false);
            _availableObjects.Add(go);
        }

        // Get the Object from available object pool.
        public GameObject GetObjectFromPool()
        {
            GameObject go = null;
            if (_availableObjects.Count > 0)
            {
                go = _availableObjects[0];
                _availableObjects.RemoveAt(0);
            }
            else
            {
                if (_maxSize > _size)
                {
                    // ToDo - Maybe no need to increase size to MAX.
                    int addSize = _maxSize - _size;
                    for (int initIdx = 0; initIdx < addSize; initIdx++)
                    {
                        AddObjectToPool(AllocateNewObject());
                    }
                    go = _availableObjects[0];
                    _availableObjects.RemoveAt(0);
                }
                else
                {
                    Debug.LogError("Get pool object failed. Not enough room on pool - " + _name);
                }
            }

            go.SetActive(true);

            return go;
        }

        // Return the object to the pool. Make it available again.
        public void ReturnObjectToPool(GameObject goToReturn)
        {
            goToReturn.SetActive(false);

            // !!ToDo - Check whether the returned object is belong this pool.
            _availableObjects.Add(goToReturn);
        }
    }
}