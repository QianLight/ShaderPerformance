using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class DecalMapData
{
    public int ChunkLength;
    [HideInInspector] public DecalChunkData[] ChunkDatas;
    [HideInInspector] public Bounds[] BoxBounds;
    public BoundingSphere[] BoundingSpheres;

    // private Plane[] _frustumPlanes;
    private CullingGroup _cullingGroup;

    public DecalMapData(DecalChunkData[] chunkDatas)
    {
        if (chunkDatas == null)
        {
            Debug.Log("chunkDatas is Null");
            return;
        }

        ChunkDatas = chunkDatas;
        ChunkLength = chunkDatas.Length;

        CreateBounds(); //用于AABB做剔除
    }

    public void CreateCullingGroup(Camera targetCam)
    {
        BoundingSpheres = new BoundingSphere[ChunkLength];
        int MapRow = DecalConfig.Row;
        int MapCol = DecalConfig.Col;
        int ceilX = DecalConfig.MaxMapX / MapCol;
        int ceilZ = DecalConfig.MaxMapZ / MapCol;
        int halfCeilX = ceilX / 2;
        int halfCeilZ = ceilZ / 2;

        int radius = Mathf.Max(halfCeilX, halfCeilZ);
        Vector3 centerPos = default;
        for (int i = 0; i < ChunkLength; i++)
        {
            var tempRow = i / MapRow;
            var tempCol = i % MapCol;
            centerPos.x = tempRow * ceilX + halfCeilX;
            centerPos.z = tempCol * ceilZ + halfCeilZ;
            BoundingSpheres[i] = new BoundingSphere(centerPos, radius);
        }

        _cullingGroup = new CullingGroup();
        _cullingGroup.enabled = true;
        _cullingGroup.SetBoundingSpheres(BoundingSpheres);
        _cullingGroup.SetBoundingSphereCount(ChunkLength);
        _cullingGroup.SetBoundingDistances(DecalConfig.CullingDistances);
        _cullingGroup.targetCamera = targetCam;
        _cullingGroup.SetDistanceReferencePoint(targetCam.transform);
        _cullingGroup.onStateChanged = OnCullingStateChanged;
    }

    private void OnCullingStateChanged(CullingGroupEvent cullingGroupEvent)
    {
        if (cullingGroupEvent.hasBecomeVisible)
        {
            ChunkDatas[cullingGroupEvent.index].SetActive(true);
        }
        else if (cullingGroupEvent.hasBecomeInvisible)
        {
            ChunkDatas[cullingGroupEvent.index].SetActive(false);
        }
        else
        {
        }
    }

    public void CreateBounds()
    {
        BoxBounds = new Bounds[ChunkLength];

        int MapRow = DecalConfig.Row;
        int MapCol = DecalConfig.Col;
        int ceilX = DecalConfig.MaxMapX / MapCol;
        int ceilZ = DecalConfig.MaxMapZ / MapCol;
        int halfCeilX = ceilX / 2;
        int halfCeilZ = ceilZ / 2;

        Vector3 centerPos = default;
        for (int i = 0; i < ChunkLength; i++)
        {
            var tempRow = i / MapRow;
            var tempCol = i % MapCol;
            centerPos.x = tempRow * ceilX + halfCeilX;
            centerPos.z = tempCol * ceilZ + halfCeilZ;
            BoxBounds[i] = new Bounds(centerPos, new Vector3(ceilX, 1, ceilZ));
        }
    }

    public void Init(Camera targetCam)
    {
        CreateCullingGroup(targetCam);

        for (int i = 0; i < ChunkLength; i++)
        {
            ChunkDatas[i].Init(targetCam);
        }
    }

    public void CancelPreview()
    {
        for (int i = 0; i < ChunkLength; i++)
        {
            ChunkDatas[i].CancelPreview();
        }

        Dispose();
    }

    public void SetVisible(bool isVisible)
    {
        if (ChunkDatas == null)
        {
            return;
        }
        
        for (int i = 0; i < ChunkDatas.Length; i++)
        {
            ChunkDatas[i]?.SetDecalObjVisible(isVisible);
        }
    }

    public void UpdateCulling()
    {
        // UpdateCullingAABB();
    }


    // public void UpdateCullingAABB()
    // {
    //     _frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
    //     for (int i = 0; i < _chunkLength; i++)
    //     {
    //         bool isVisible = GeometryUtility.TestPlanesAABB(_frustumPlanes, BoxBounds[i]);
    //         ChunkDatas[i].SetActive(isVisible);
    //     }
    // }

    public void Dispose()
    {
        if (_cullingGroup != null)
        {
            _cullingGroup.onStateChanged = null;
            _cullingGroup.Dispose();
            _cullingGroup = null;
        }

        if (ChunkDatas != null)
        {
            for (int i = 0; i < ChunkLength; i++)
            {
                ChunkDatas[i].Dispose();
            }
        }

        BoundingSpheres = null;
    }
}