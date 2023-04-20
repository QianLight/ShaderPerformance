#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class NaviPointBehaviour : MonoBehaviour
{
    public delegate void GameObjectDestroyed(GameObject go);

    public GameObjectDestroyed destroyCallback = null;

    public void OnDestroy()
    {
        if(destroyCallback != null)
        {
            destroyCallback(gameObject);
        }
    }
}

#endif