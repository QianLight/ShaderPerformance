using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
[ExecuteInEditMode]
public class FPDebugObjectItem : MonoBehaviour
{
    public int RemoteID = 0;
    [NonSerialized]
    public List<VL> VL;
    void OnEnable()
    {
        objectHandle(RemoteID, true);
    }
    void OnDisable()
    {
        objectHandle(RemoteID, false);
    }
    private void OnDestroy()
    {
        VL = null;
    }
    void objectHandle(int id, bool enalbe)
    {
        if (ObjectHandle != null && id != 0)
        {
            int parentId = 0;
            if (transform.parent != null)
            {
                FPDebugObjectItem parentItem = transform.parent.gameObject.GetComponent<FPDebugObjectItem>();
                if (parentItem != null)
                {
                    parentId = parentItem.RemoteID;
                }
            }
            ObjectHandle(parentId, id, enalbe);
        }
    }
    public void PostHandleAction(string post, bool enable)
    {
        if (ObjectHandle != null && RemoteID != 0)
        {
            PostHandle(RemoteID, post, enable);
        }
    }
    public void ParameterHandleAction(string post, string para, bool enable, string value)
    {
        if (ParameterHandle != null && RemoteID != 0)
        {
            ParameterHandle(RemoteID, post, para, enable, value);
        }
    }
    public static ObjectAction ObjectHandle = null;
    public static PostAction PostHandle = null;
    public static ParameterAction ParameterHandle = null;
}
public delegate void ObjectAction(int parent, int id, bool enable);
public delegate void PostAction(int id, string post, bool enable);
public delegate void ParameterAction(int id, string post, string para, bool enable, string value);

