using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace MeshLOD
{
    [Serializable]
    public class MeshLODGroup
    {
        public Transform bindTransform;
        public Transform runtimeTransform;

        //LOD
        public Vector3 lodReferencePoint;
        public float quadSize;

        //保存根节点的相关渲染信息，也就是LOD0的copy
        public MeshRenderNode rootMeshRenderNode;
        public List<MeshRenderNode> rootChildMeshRenderNodeList;

        //保存根节点的LOD各个级别信息
        public List<MeshLODData> lodRootMeshLODDataList;

        //保存根节点的子节点LOD各个级别信息
        public List<MeshLODDataGroup> lodRootLODDataGroupList;

        //距离配置
        public List<float> distanceList;

        private int _lodTotalLevel;
        private int _curLODLevel;

        private bool _isNeedBakeVertexColor;

        private List<string> _lodObjNameList;

        //Impostor State
        private bool _isForceStopUpdate;
        private bool _isInImpostorState;

        private LODGroup _lodGroup;

        public MeshLODGroup(LODGroup lodGroup, bool isNeedBakeVertexColor)
        {
            _lodGroup = lodGroup;
            bindTransform = lodGroup.transform;
            _isNeedBakeVertexColor = isNeedBakeVertexColor;

            distanceList = new List<float>();
            rootChildMeshRenderNodeList = new List<MeshRenderNode>();
            lodRootMeshLODDataList = new List<MeshLODData>();
            _lodObjNameList = new List<string>();
            lodRootLODDataGroupList = new List<MeshLODDataGroup>();
        }

        public bool InitData()
        {
            if (!CreateRuntimeObj())
            {
                return false;
            }

            InitMeshRenderNode();
            InitLOD();
            InitVisible();

            _isForceStopUpdate = false;
            _isInImpostorState = false;
            return true;
        }

        public void Init()
        {
            _curLODLevel = 0;
            _lodTotalLevel = distanceList.Count;
            SetLOD(0);
        }

        public void UpdateMeshLOD(Vector3 referencePos, float multiplier)
        {
            if (_isForceStopUpdate)
            {
                return;
            }

            Vector3 viewDir = referencePos - lodReferencePoint;
            float viewDistance = viewDir.magnitude;
            float screenSize = quadSize / (viewDistance * multiplier);
            screenSize = Mathf.Clamp(screenSize, 0, 1);
            
            for (int i = 0; i < distanceList.Count; i++)
            {
                if (screenSize > distanceList[i])
                {
                    SetLOD(i);
                    return;
                }
            }
            
            Culling();
        }

        public bool SetLOD(int lodLevel)
        {
            if (lodLevel < 0 || lodLevel > _lodTotalLevel)
            {
                return false;
            }

            if (lodLevel == _curLODLevel)
            {
                return false;
            }

            _curLODLevel = lodLevel;
            MeshLODData rootMeshLODData = lodRootMeshLODDataList[lodLevel];
            if (rootMeshLODData != null)
            {
                rootMeshRenderNode?.SetRender(rootMeshLODData);
            }

            MeshLODDataGroup meshLODDataGroup = lodRootLODDataGroupList[lodLevel];
            if (rootChildMeshRenderNodeList != null)
            {
                int len = Math.Min(rootChildMeshRenderNodeList.Count, meshLODDataGroup.meshLODDataList.Count);
                for (int i = 0; i < len; i++)
                {
                    
                    rootChildMeshRenderNodeList[i].SetRender(meshLODDataGroup.meshLODDataList[i]);
                }
            }

            return true;
        }

        public void Culling()
        {
            rootMeshRenderNode.Culling();
            if (rootChildMeshRenderNodeList != null)
            {
                for (int i = 0; i < rootChildMeshRenderNodeList.Count; i++)
                {
                    rootChildMeshRenderNodeList[i].Culling();
                }
            }

            _curLODLevel = -1;
        }

        public void EnterImpostor()
        {
            if (_isInImpostorState)
            {
                return;
            }

            _isInImpostorState = true;
            _isForceStopUpdate = true;
            Culling();
        }

        public void ExitImpostor()
        {
            if (!_isInImpostorState)
            {
                return;
            }

            _isInImpostorState = false;
            _isForceStopUpdate = false;
        }

        public void Dispose()
        {
            SetLOD(0);
        }

        private bool CreateRuntimeObj()
        {
            if (bindTransform.childCount <= 0)
            {
                return false;
            }
            Transform lod0Trans = bindTransform.GetChild(0);
            if (lod0Trans == null)
            {
                Debug.Log(bindTransform.name + " LOD0 is Null");
                return false;
            }

            runtimeTransform = lod0Trans;

            if (runtimeTransform.childCount > 0)
            {
                for (int i = 0; i < runtimeTransform.childCount; i++)
                {
                    Transform childTransform = runtimeTransform.GetChild(i);
                    if (childTransform == null)
                    {
                        continue;
                    }
                    
                    _lodObjNameList.Add(childTransform.gameObject.name);
                }
            }
            
            return true;
        }

        private void RecalculateLODData(LODGroup lodGroup)
        {
            if (lodGroup == null)
            {
                return;
            }

            lodGroup.RecalculateBounds();
            lodReferencePoint = bindTransform.TransformPoint(lodGroup.localReferencePoint);
            quadSize = RecalculateQuadSize(lodGroup);
            LOD[] lods = lodGroup.GetLODs();
            for (int i = 0; i < lods.Length; i++)
            {
                distanceList.Add(lods[i].screenRelativeTransitionHeight);
            }
        }
        
        public float RecalculateQuadSize(LODGroup lodGroup)
        {
            if (lodGroup == null)
            {
                return 1;
            }
            
            float tempQuadSize = lodGroup.size;
            return tempQuadSize;
            // Bounds bound = new Bounds();
            // var renderers = lodGroup.GetLODs().SelectMany(lod => lod.renderers); 
            // foreach (Renderer r in renderers)
            // {
            //     if (r == null)
            //         continue;
            //     if (bound.extents == Vector3.zero)
            //         bound = r.bounds;
            //     else
            //         bound.Encapsulate(r.bounds);
            // }
            //
            // tempQuadSize = bound.size.magnitude;
            //
            // return tempQuadSize;
        }
        

        private void InitLOD()
        {
            RecalculateLODData(_lodGroup);

            _curLODLevel = 0;
            _lodTotalLevel = distanceList.Count;

            //处理父节点的LOD数据
            lodRootMeshLODDataList.Clear();

            int childCount = bindTransform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform tempTrans = bindTransform.GetChild(i);
                if (tempTrans == null)
                {
                    continue;
                }

                //处理父节点的LOD数据
                lodRootMeshLODDataList.Add(new MeshLODData(tempTrans, rootMeshRenderNode, (i != 0 && _isNeedBakeVertexColor)));

                //处理子节点的LOD数据
                int childLen = tempTrans.childCount;
                List<MeshLODData> tempMeshLODDataList = new List<MeshLODData>();

                //对齐父节点网格名称
                string lodChildTranName = String.Empty;
                for (int k = 0; k < _lodObjNameList.Count; k++)
                {
                    lodChildTranName = _lodObjNameList[k];

                    if (MeshLODNameMathUtility.Instance.CheckIsSpecialObj(lodChildTranName))
                    {
                        lodChildTranName = MeshLODNameMathUtility.Instance.GetLODName(lodChildTranName, i);
                    }
                    else
                    {
                        if (i != 0) // LOD0 不带后缀，使用原始名称
                        {
                            lodChildTranName += "_LOD" + i;
                        }    
                    }
                    
                    for (int j = 0; j < childLen; j++)
                    {
                        Transform childTrans = tempTrans.GetChild(j);
                        if (childTrans != null && childTrans.gameObject.name == lodChildTranName)
                        {
                            tempMeshLODDataList.Add(new MeshLODData(childTrans, rootChildMeshRenderNodeList[k], _isNeedBakeVertexColor));
                        }
                    }
                }
                
                MeshLODDataGroup lodDataGroup = new MeshLODDataGroup(tempMeshLODDataList);
                lodRootLODDataGroupList.Add(lodDataGroup);
            }
        }

        private void InitMeshRenderNode()
        {
            rootChildMeshRenderNodeList.Clear();

            rootMeshRenderNode = new MeshRenderNode(runtimeTransform);

            int len = runtimeTransform.childCount;
            for (int i = 0; i < len; i++)
            {
                MeshRenderNode tempMeshFilter = new MeshRenderNode(runtimeTransform.GetChild(i));
                rootChildMeshRenderNodeList.Add(tempMeshFilter);
            }
        }

        private void InitVisible()
        {
            if ( _lodGroup != null)
            {
                _lodGroup.enabled = false;
            }
            
            int childCount = bindTransform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var tempTrans = bindTransform.GetChild(i);
                if (tempTrans == null)
                {
                    continue;
                }

                if (tempTrans == runtimeTransform)
                {
                    tempTrans.gameObject.SetActive(true);
                    continue;
                }

                tempTrans.gameObject.SetActive(false);
            }
        }
    }
}