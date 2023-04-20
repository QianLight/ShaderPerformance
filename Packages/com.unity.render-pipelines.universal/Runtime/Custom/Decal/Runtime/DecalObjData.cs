using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class DecalObjData
{
    public Material Material;
    public Transform targetTrans;
    public Renderer targetRender;
    [HideInInspector] public bool IsNeedRender;
    [HideInInspector] public Matrix4x4 LocalToWorld;


    public DecalObjData(GameObject targetObj)
    {
        targetTrans = targetObj.transform;
        targetRender = targetObj.GetComponent<Renderer>();
        if (targetRender != null)
        {
            Material = targetRender.sharedMaterial;
        }

        LocalToWorld = targetTrans.localToWorldMatrix;
    }

    public void SetVisible(bool isVisible)
    {
        if (targetRender != null)
        {
            targetRender.enabled = isVisible;
        }
    }
}