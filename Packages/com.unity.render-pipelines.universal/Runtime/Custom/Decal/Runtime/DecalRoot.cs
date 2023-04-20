using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
[ExecuteInEditMode]
public class DecalRoot : MonoBehaviour
{
    public bool IsDrawCube;
    public bool IsDrawSphere;
    public bool IsDrawDecalChunkObj;

    public List<Material> DecalDiffMaterialList = new List<Material>();
    public DecalMapData _decalMapData;

    private void OnEnable()
    {
        DecalManager.Instance.Register(this);
    }

    private void OnDisable()
    {
        DecalManager.Instance.UnRegister(this);

        CancelPreview();
    }

    public void CollectDecal()
    {
        Transform[] tempTransforms = transform.GetComponentsInChildren<Transform>();
        if (tempTransforms == null || tempTransforms.Length < 2)
        {
            Debug.Log("childCount 的数量为 0");
            return;
        }

        DecalDiffMaterialList ??= new List<Material>();
        DecalDiffMaterialList.Clear();

        int len = DecalConfig.Col * DecalConfig.Row;
        DecalChunkData[] decalChunkDatas = new DecalChunkData[len];
        for (int i = 0; i < len; i++)
        {
            decalChunkDatas[i] = new DecalChunkData(i);
        }

        Transform tempTrans;
        Vector3 tempPos;
        Renderer tempRenderer;
        int tempRow, tempCol;
        int ceilX = DecalConfig.MaxMapX / DecalConfig.Col;
        int ceilZ = DecalConfig.MaxMapZ / DecalConfig.Row;
        int childCount = tempTransforms.Length;
        for (int i = 0; i < childCount; i++)
        {
            tempTrans = tempTransforms[i];
            tempRenderer = tempTrans.GetComponent<Renderer>();
            tempPos = tempTrans.position;

            if (tempPos.x > DecalConfig.MaxMapX || tempPos.y > DecalConfig.MaxMapY || tempPos.z > DecalConfig.MaxMapZ)
            {
                continue;
            }

            if (tempPos.x < 0 || tempPos.y < 0 || tempPos.z < 0)
            {
                continue;
            }

            if (tempRenderer == null || tempRenderer.sharedMaterial == null || tempRenderer.sharedMaterial.shader == null)
            {
                continue;
            }

            if (tempRenderer.sharedMaterial.shader.name != DecalConfig.DecalShaderName)
            {
                continue;
            }

            if (!DecalDiffMaterialList.Contains(tempRenderer.sharedMaterial))
            {
                var sharedMaterial = tempRenderer.sharedMaterial;
                sharedMaterial.enableInstancing = true;
                DecalDiffMaterialList.Add(sharedMaterial);
            }

            tempRow = Mathf.FloorToInt(tempPos.x / ceilX);
            tempCol = Mathf.FloorToInt(tempPos.z / ceilZ);

            decalChunkDatas[tempRow * DecalConfig.Row + tempCol].AddDecalObj(tempTrans.gameObject);
        }

        _decalMapData = new DecalMapData(decalChunkDatas);
    }

    public void Init(Camera targetCam)
    {
        _decalMapData?.Init(targetCam);
    }

    public void CancelPreview()
    {
        _decalMapData?.CancelPreview();
    }

    public void SetVisible(bool isVisible)
    {
        _decalMapData?.SetVisible(isVisible);
    }

    private void OnDrawGizmos()
    {
        if (_decalMapData == null || _decalMapData.ChunkDatas == null)
        {
            return;
        }

        if (_decalMapData.BoxBounds != null && IsDrawCube)
        {
            for (int i = 0; i < _decalMapData.BoxBounds.Length; i++)
            {
                if (!_decalMapData.ChunkDatas[i].IsActive)
                {
                    continue;
                }

                Gizmos.DrawWireCube(_decalMapData.BoxBounds[i].center, _decalMapData.BoxBounds[i].size);
            }
        }

        if (_decalMapData.BoundingSpheres != null && IsDrawSphere && _decalMapData.BoundingSpheres != null)
        {
            for (int i = 0; i < _decalMapData.BoundingSpheres.Length; i++)
            {
                if (!_decalMapData.ChunkDatas[i].IsActive)
                {
                    continue;
                }

                Gizmos.DrawWireSphere(_decalMapData.BoundingSpheres[i].position, _decalMapData.BoundingSpheres[i].radius);
            }
        }

        if (IsDrawDecalChunkObj)
        {
            DecalChunkData tempChunkData;
            for (int i = 0; i < _decalMapData.ChunkDatas.Length; i++)
            {
                tempChunkData = _decalMapData.ChunkDatas[i];
                if (!tempChunkData.IsActive || tempChunkData.BoundingSpheres == null)
                {
                    continue;
                }

                for (int j = 0; j < tempChunkData.DecalObjDataList.Count; j++)
                {
                    if (!tempChunkData.DecalObjDataList[j].IsNeedRender)
                    {
                        continue;
                    }

                    Gizmos.DrawWireSphere(tempChunkData.BoundingSpheres[j].position, tempChunkData.BoundingSpheres[j].radius);
                }
            }
        }
    }
}