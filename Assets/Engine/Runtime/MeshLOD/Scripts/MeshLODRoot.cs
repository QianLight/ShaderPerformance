using System.Collections.Generic;
using UnityEngine;

namespace MeshLOD
{
    [DisallowMultipleComponent]
    public class MeshLODRoot : MonoBehaviour
    {
        public bool isNeedBakeVertexColor;
        public List<Transform> rootTransforms;
        public List<Transform> filterTransforms;
        [SerializeField] public List<MeshLODGroup> meshLODGroupList;


        private void OnEnable()
        {
            MeshLODManager.Instance.Register(this);
        }

        private void OnDisable()
        {
            MeshLODManager.Instance.UnRegister(this);
        }

        public void CollectionInfo()
        {
            if (meshLODGroupList == null)
            {
                meshLODGroupList = new List<MeshLODGroup>();
            }

            for (int i = 0; i < meshLODGroupList.Count; i++)
            {
                meshLODGroupList[i].Dispose();
            }

            meshLODGroupList.Clear();

            if (rootTransforms == null)
            {
                Debug.Log("请拖入跟节点");
                return;
            }

            for (int k = 0; k < rootTransforms.Count; k++)
            {
                Transform tempRootTransform = rootTransforms[k];

                LODGroup[] lodGroups = tempRootTransform.GetComponentsInChildren<LODGroup>();
                LODGroup tempLODGroup;
                for (int i = 0; i < lodGroups.Length; i++)
                {
                    tempLODGroup = lodGroups[i];
                    if (!tempLODGroup.gameObject.activeSelf)
                    {
                        continue;
                    }

                    bool isfilter = false;
                    for (int j = 0; j < filterTransforms.Count; j++)
                    {
                        if (tempLODGroup.transform == filterTransforms[j])
                        {
                            isfilter = true;
                            break;
                        }
                    }

                    if (isfilter)
                    {
                        continue;
                    }
                    
                    MeshLODGroup tempMeshLODGroup = new MeshLODGroup(tempLODGroup, isNeedBakeVertexColor);
                    if (!tempMeshLODGroup.InitData())
                    {
                        continue;
                    }

                    

                    meshLODGroupList.Add(tempMeshLODGroup);
                }
            }
        }

        public void Clear()
        {
            for (int i = 0; i < meshLODGroupList.Count; i++)
            {
                meshLODGroupList[i].Dispose();
            }

            meshLODGroupList.Clear();
        }

        public void Init()
        {
            if (meshLODGroupList == null)
            {
                return;
            }

            for (int i = 0; i < meshLODGroupList.Count; i++)
            {
                meshLODGroupList[i].Init();
            }
        }

        public void EnterImpostor(GameObject targetObj)
        {
            for (int i = 0; i < meshLODGroupList.Count; i++)
            {
                if (meshLODGroupList[i].bindTransform.gameObject != targetObj)
                {
                    continue;
                }

                meshLODGroupList[i].EnterImpostor();
            }
        }

        public void ExitImpostor(GameObject targetObj, Camera referenceCamera)
        {
            for (int i = 0; i < meshLODGroupList.Count; i++)
            {
                if (meshLODGroupList[i].bindTransform.gameObject != targetObj)
                {
                    continue;
                }

                meshLODGroupList[i].ExitImpostor();

                if (referenceCamera != null && referenceCamera.transform != null)
                {
                    float multiplier = MeshLODUtility.CalculateScreenMultiplier(referenceCamera.fieldOfView, QualitySettings.lodBias);
                    meshLODGroupList[i].UpdateMeshLOD(referenceCamera.transform.position, multiplier);
                }
            }
        }

        public bool CheckHasMeshLOD(GameObject obj)
        {
            if (meshLODGroupList == null)
            {
                return false;
            }

            for (int i = 0; i < meshLODGroupList.Count; i++)
            {
                if (meshLODGroupList[i].bindTransform.gameObject == obj.gameObject)
                {
                    return true;
                }
            }

            return false;
        }

        public void UpdateMeshLOD(Vector3 referencePos, float multiplier)
        {
            if (meshLODGroupList == null)
            {
                return;
            }

            for (int i = 0; i < meshLODGroupList.Count; i++)
            {
                meshLODGroupList[i].UpdateMeshLOD(referencePos, multiplier);
            }
        }

        public void Dispose()
        {
            if (meshLODGroupList == null)
            {
                return;
            }

            for (int i = 0; i < meshLODGroupList.Count; i++)
            {
                meshLODGroupList[i].Dispose();
            }
        }
    }
}