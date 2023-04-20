using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DecalChunkData
{
    public BoundingSphere[] BoundingSpheres;
    public Dictionary<Material, List<int>> DecalObjDic;
    public int ChunkIndex;
    [HideInInspector] public bool IsActive;
    public List<DecalObjData> DecalObjDataList;

    private CullingGroup _cullingGroup;

    public DecalChunkData(int chunkIndex)
    {
        ChunkIndex = chunkIndex;
        DecalObjDataList = new List<DecalObjData>();

        IsActive = false;
    }

    public void AddDecalObj(GameObject decalObj)
    {
        DecalObjData decalObjData = new DecalObjData(decalObj);
        if (decalObjData.Material == null || decalObjData.Material.shader == null || decalObjData.Material.shader.name != DecalConfig.DecalShaderName)
        {
            Debug.Log(decalObj.gameObject.name + "材质为空");
            return;
        }

        if (DecalObjDataList.Contains(decalObjData))
        {
            return;
        }

        DecalObjDataList.Add(decalObjData);
    }

    public void Init(Camera targetCam)
    {
        InitDecalDicData();
        InitCullingData(targetCam);
    }

    public void CancelPreview()
    {
        for (int i = 0; i < DecalObjDataList.Count; i++)
        {
            DecalObjDataList[i].SetVisible(true);
        }
        
        Dispose();
    }

    private void InitDecalDicData()
    {
        List<int> tempDecalObjList = null;
        DecalObjData tempDecalObj;
        DecalObjDic = new Dictionary<Material, List<int>>();
        for (int i = 0; i < DecalObjDataList.Count; i++)
        {
            tempDecalObjList = null;
            tempDecalObj = DecalObjDataList[i];

            if (tempDecalObj == null || tempDecalObj.Material == null)
            {
                continue;
            }
            
            if (DecalObjDic.ContainsKey(tempDecalObj.Material))
            {
                tempDecalObjList = DecalObjDic[tempDecalObj.Material];
            }

            if (tempDecalObjList == null)
            {
                tempDecalObjList = new List<int>();
                tempDecalObjList.Add(i);
                DecalObjDic[tempDecalObj.Material] = tempDecalObjList;
                continue;
            }

            if (tempDecalObjList.Contains(i))
            {
                continue;
            }

            tempDecalObjList.Add(i);
        }
    }

    private void InitCullingData(Camera targetCam)
    {
        BoundingSpheres = new BoundingSphere[DecalObjDataList.Count];
        Transform tempTrans;
        Vector3 tempScale;
        for (int i = 0; i < DecalObjDataList.Count; i++)
        {
            tempTrans = DecalObjDataList[i].targetTrans;
            tempScale = tempTrans.localScale;
            BoundingSpheres[i] = new BoundingSphere(tempTrans.position, Mathf.Max(tempScale.x, tempScale.z) * 0.5f);
        }

        _cullingGroup = new CullingGroup();
        _cullingGroup.enabled = true;
        _cullingGroup.targetCamera = targetCam;
        _cullingGroup.onStateChanged = OnCullingStateChanged;
        _cullingGroup.SetBoundingSpheres(BoundingSpheres);
        _cullingGroup.SetBoundingSphereCount(BoundingSpheres.Length);
        _cullingGroup.SetDistanceReferencePoint(targetCam.transform);
        _cullingGroup.SetBoundingDistances(DecalConfig.CullingDistances);
    }

    public void SetDecalObjVisible(bool isVisible)
    {
        if (DecalObjDataList == null)
        {
            return;
        }

        for (int i = 0; i < DecalObjDataList.Count; i++)
        {
            DecalObjDataList[i]?.SetVisible(isVisible);
        }
    }


    private void OnCullingStateChanged(CullingGroupEvent cullingGroupEvent)
    {
        if (cullingGroupEvent.hasBecomeVisible)
        {
            DecalObjDataList[cullingGroupEvent.index].IsNeedRender = true;
        }
        else if (cullingGroupEvent.hasBecomeInvisible)
        {
            DecalObjDataList[cullingGroupEvent.index].IsNeedRender = false;
        }
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
        _cullingGroup.enabled = isActive;
    }

    public void Dispose()
    {
        if (_cullingGroup != null)
        {
            _cullingGroup.onStateChanged = null;
            _cullingGroup.Dispose();
            _cullingGroup = null;
        }

        BoundingSpheres = null;
    }
}