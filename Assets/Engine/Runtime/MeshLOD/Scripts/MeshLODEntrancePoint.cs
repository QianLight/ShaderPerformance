using System;
using UnityEngine;

namespace MeshLOD
{
    [DefaultExecutionOrder(100)]
    public class MeshLODEntrancePoint : MonoBehaviour
    {
        public Camera referenceCamera;
        public float cameraIntervalDistance = 5f;
        private Vector3 _oldCamPos = Vector3.zero;
        
        private void OnDestroy()
        {
            MeshLODManager.Instance.Dispose();
        }

        private void Start()
        {
            if (referenceCamera == null)
            {
                ResetCamera();
            }
            
            MeshLODManager.Instance.InitData(referenceCamera);
        }

        private void ResetCamera()
        {
            if (Camera.main != null)
            {
                referenceCamera = Camera.main;
                _oldCamPos = referenceCamera.transform.position;
            }
        }

        private void LateUpdate()
        {
            if (referenceCamera == null)
            {
                ResetCamera();
                MeshLODManager.Instance.InitData(referenceCamera);
            }

            Vector3 camPos = referenceCamera.transform.position;
            if (Mathf.Abs(camPos.x - _oldCamPos.x) > cameraIntervalDistance || Mathf.Abs(camPos.y - _oldCamPos.y) > cameraIntervalDistance || Mathf.Abs(camPos.z - _oldCamPos.z) > cameraIntervalDistance )
            {
                _oldCamPos = camPos;
                
                if (referenceCamera != null && MeshLODManager.Instance.IsInit)
                {
                    MeshLODManager.Instance.UpdateMeshLOD(referenceCamera);
                }
            }
        }
    }
}