using System.Collections;
using System.Collections.Generic;
using CFEngine;
using UnityEngine;
using UnityEngine.Rendering;

public class DecalManager
{
    private static DecalManager _instance;

    public static DecalManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new DecalManager();
            }

            return _instance;
        }
    }


    private Mesh _decalMesh;

    private Mesh DecalMesh
    {
        get
        {
            if (_decalMesh == null)
                _decalMesh = CoreUtils.CreateCubeMesh(new Vector4(-0.5f, -0.5f, -0.5f, 1.0f), new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
            return _decalMesh;
        }
    }

    public bool IsInit;

    private List<Material> _diffMaterialList;
    private List<DecalBatchData> _decalBatchDataNewList;
    private List<DecalRoot> _decalRootList;

    private Camera _curCamera;
    private bool _isSupport;

    public DecalManager()
    {
        _diffMaterialList = new List<Material>();
        _decalBatchDataNewList = new List<DecalBatchData>();
        _decalRootList = new List<DecalRoot>();
        IsInit = false;
        _isSupport = SystemInfo.supportsInstancing;
    }

    ~DecalManager()
    {
        _decalRootList?.Clear();
        _decalRootList = null;
        Dispose();
    }

    public void Register(DecalRoot decalRoot)
    {
        if (_decalRootList == null)
        {
            Debug.Log("_decalRootList is Null");
            return;
        }

        if (decalRoot == null)
        {
            return;
        }

        if (_decalRootList.Contains(decalRoot))
        {
            return;
        }

        _decalRootList.Add(decalRoot);
    }

    public void UnRegister(DecalRoot decalRoot)
    {
        if (_decalRootList == null)
        {
            Debug.Log("_decalRootList is Null");
            return;
        }

        if (decalRoot == null)
        {
            return;
        }

        if (!_decalRootList.Contains(decalRoot))
        {
            return;
        }

        _decalRootList.Remove(decalRoot);
    }

    public void DrawDecal(CommandBuffer cmd)
    {
        if (!_isSupport)
        {
            return;
        }
        
        CheckCameraChange();
        UpdateDecalBatch();
        Mesh instanceMesh = DecalConfig.InstanceMesh;

        if (_decalBatchDataNewList != null)
        {
            DecalBatchData decalBatchData;
            for (int i = 0; i < _decalBatchDataNewList.Count; i++)
            {
                decalBatchData = _decalBatchDataNewList[i];
                if (decalBatchData == null || decalBatchData.Material == null || decalBatchData.Count <= 0)
                {
                    continue;
                }
                cmd.DrawMeshInstanced(instanceMesh, 0, decalBatchData.Material, 0, decalBatchData.Matrix4X4s, decalBatchData.Count);
            }
        }
    }

    public bool GetIsInit()
    {
        return IsInit;
    }

    public void Init(Camera targetCam)
    {
        if (targetCam == null)
        {
            return;
        }
        _curCamera = targetCam;
        
        for (int i = 0; i < _decalRootList.Count; i++)
        {
            _decalRootList[i].Init(targetCam);
            AddMaterials(_decalRootList[i].DecalDiffMaterialList);
        }

        IsInit = true;
    }

    public void SetVisible(bool isVisible)
    {
        for (int i = 0; i < _decalRootList.Count; i++)
        {
            _decalRootList[i].SetVisible(isVisible);
        }
    }

    public void CancelPreview()
    {
        IsInit = false;
        for (int i = 0; i < _decalRootList.Count; i++)
        {
            _decalRootList[i].CancelPreview();
        }
    }

    public void Dispose()
    {
        ClearDecalBatchData();

        _instance = null;
        IsInit = false;
    }

    private void UpdateDecalBatch()
    {
        for (int i = 0; i < _decalBatchDataNewList.Count; i++)
        {
            if (_decalBatchDataNewList[i] == null)
            {
                continue;
            }
            _decalBatchDataNewList[i].ResetMatrixCount();
        }

        DecalMapData tempDecalMapData;
        DecalChunkData tempDecalChunkData;
        DecalRoot tempDecalRoot;
        for (int i = 0; i < _decalRootList.Count; i++)
        {
            tempDecalRoot = _decalRootList[i];
            if (tempDecalRoot == null)
            {
                return;
            }

            tempDecalMapData = tempDecalRoot._decalMapData;
            if (tempDecalMapData == null)
            {
                continue;
            }

            if (tempDecalMapData.ChunkDatas == null)
            {
                continue;
            }

            for (int j = 0; j < tempDecalMapData.ChunkDatas.Length; j++)
            {
                tempDecalChunkData = tempDecalMapData.ChunkDatas[j];
                if (!tempDecalChunkData.IsActive)
                {
                    continue;
                }

                FillNeedDecalObjsNew(tempDecalChunkData);
            }
        }
    }

    private void FillNeedDecalObjsNew(DecalChunkData decalChunkData)
    {
        if (_decalBatchDataNewList == null || _diffMaterialList == null || decalChunkData == null || decalChunkData.DecalObjDic == null)
        {
            return;
        }

        if (_decalBatchDataNewList.Count != _diffMaterialList.Count)
        {
            return;
        }

        List<int> tempDecalObjIndexList;
        DecalObjData tempDecalObjData;
        int len = 0;
        Material material;
        for (int i = 0; i < _diffMaterialList.Count; i++)
        {
            material = _diffMaterialList[i];

            if (material == null)
            {
                continue;
            }
            
            if (!decalChunkData.DecalObjDic.ContainsKey(material))
            {
                continue;
            }
            
            tempDecalObjIndexList = decalChunkData.DecalObjDic[material];
            len = tempDecalObjIndexList.Count;
            for (int j = 0; j < len; j++)
            {
                tempDecalObjData = decalChunkData.DecalObjDataList[tempDecalObjIndexList[j]];
                if (!tempDecalObjData.IsNeedRender)
                {
                    continue;
                }

                _decalBatchDataNewList[i].AddOne(tempDecalObjData.LocalToWorld);
            }
        }
    }

    private void AddMaterials(List<Material> decalDiffMaterialList)
    {
        if (decalDiffMaterialList == null || decalDiffMaterialList.Count <= 0)
        {
            return;
        }

        Material tempMaterial;
        for (int i = 0; i < decalDiffMaterialList.Count; i++)
        {
            tempMaterial = decalDiffMaterialList[i];
            
            if (tempMaterial == null)
            {
                continue;
            }
            
            if (_diffMaterialList.Contains(tempMaterial))
            {
                continue;
            }

            _diffMaterialList.Add(tempMaterial);
            _decalBatchDataNewList.Add(new DecalBatchData(tempMaterial));
        }
    }

    private void ClearDecalBatchData()
    {
        for (int i = 0; i < _decalBatchDataNewList.Count; i++)
        {
            _decalBatchDataNewList[i].Dispose();
        }
        _diffMaterialList.Clear();
        _decalBatchDataNewList.Clear();
    }

    private void CheckCameraChange()
    {
        if (EngineContext.IsRunning)
        {
            if (EngineContext.instance.CameraRef != null && _curCamera != EngineContext.instance.CameraRef)
            {
                Init(EngineContext.instance.CameraRef);
            }
        }
    }
}