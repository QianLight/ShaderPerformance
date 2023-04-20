using System.Collections.Generic;
using Impostors;
using Impostors.Managers;
using UnityEngine;

namespace MeshLOD
{
    public class MeshLODManager
    {
        private static MeshLODManager _intance;

        public static MeshLODManager Instance
        {
            get { return _intance ??= new MeshLODManager(); }
        }

        private MeshLODManager()
        {
            _meshLODRootList = new List<MeshLODRoot>();
        }

        ~MeshLODManager()
        {
            Dispose();
        }

        private List<MeshLODRoot> _meshLODRootList;
        private Camera _referenceCamera;

        public bool IsInit = false;

        public void Register(MeshLODRoot meshLODRoot)
        {
            if (_meshLODRootList == null)
            {
                Debug.Log("_meshLODRootList is null");
                return;
            }

            if (_meshLODRootList.Contains(meshLODRoot))
            {
                return;
            }

            _meshLODRootList.Add(meshLODRoot);
        }

        public void UnRegister(MeshLODRoot meshLODRoot)
        {
            if (_meshLODRootList == null)
            {
                Debug.Log("_meshLODRootList is null");
                return;
            }

            if (!_meshLODRootList.Contains(meshLODRoot))
            {
                return;
            }

            _meshLODRootList.Remove(meshLODRoot);
        }

        public void InitData(Camera referenceCamera)
        {
            Init();
            AddEvent();
            
            _referenceCamera = referenceCamera;
            IsInit = true;
        }
        
        public void UpdateMeshLOD(Camera referenceCamera)
        {
            if (_meshLODRootList == null)
            {
                return;
            }

            if (referenceCamera == null)
            {
                return;
            }

            float multiplier = MeshLODUtility.CalculateScreenMultiplier(referenceCamera.fieldOfView, QualitySettings.lodBias);
            for (int i = 0; i < _meshLODRootList.Count; i++)
            {
                _meshLODRootList[i].UpdateMeshLOD(referenceCamera.transform.position, multiplier);
            }
        }

        public void UpdateMeshLOD()
        {
            if (_referenceCamera == null)
            {
                return;
            }
            
            UpdateMeshLOD(_referenceCamera);
        }
        
        public bool CheckHasMeshLOD(GameObject obj)
        {
            if (_meshLODRootList == null)
            {
                return false;
            }

            for (int i = 0; i < _meshLODRootList.Count; i++)
            {
                if (_meshLODRootList[i].CheckHasMeshLOD(obj))
                {
                    return true;
                }
            }

            return false;
        }
        
        private void Init()
        {
            if (_meshLODRootList == null)
            {
                return;
            }

            for (int i = 0; i < _meshLODRootList.Count; i++)
            {
                _meshLODRootList[i].Init();
            }
        }

        private void AddEvent()
        {
            if (ImpostorableObjectsManager._instance == null)
            {
                return;
            }
            ImpostorableObjectsManager._instance.OnObjGoToImpostorEvent += OnObjGoToImpostorEvent;
            ImpostorableObjectsManager._instance.OnObjGoToNormalEvent += OnObjGoToNormalEvent;
        }

        private void RemoveEvent()
        {
            if (ImpostorableObjectsManager._instance == null)
            {
                return;
            }
            ImpostorableObjectsManager._instance.OnObjGoToImpostorEvent -= OnObjGoToImpostorEvent;
            ImpostorableObjectsManager._instance.OnObjGoToNormalEvent -= OnObjGoToNormalEvent;
        }

        private void OnObjGoToImpostorEvent(ImpostorLODGroup impostorLODGroup)
        {
            if (_meshLODRootList == null)
            {
                return;
            }
            
            for (int i = 0; i < _meshLODRootList.Count; i++)
            {
                _meshLODRootList[i].EnterImpostor(impostorLODGroup.gameObject);
            }
        }

        private void OnObjGoToNormalEvent(ImpostorLODGroup impostorLODGroup)
        {
            if (_meshLODRootList == null)
            {
                return;
            }
            
            for (int i = 0; i < _meshLODRootList.Count; i++)
            {
                _meshLODRootList[i].ExitImpostor(impostorLODGroup.gameObject, _referenceCamera);
            }
        }

        

        public void Dispose()
        {
            IsInit = false;
            RemoveEvent();

            if (_meshLODRootList != null)
            {
                for (int i = 0; i < _meshLODRootList.Count; i++)
                {
                    _meshLODRootList[i].Dispose();
                }

                _meshLODRootList.Clear();
                _meshLODRootList = null;
            }

            _referenceCamera = null;

            IsInit = false;
            _intance = null;
        }
    }
}