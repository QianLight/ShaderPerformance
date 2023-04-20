using CFEngine;
using CFUtilPoolLib;
using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class MotionController : CinemachineController, ICinemachine
{
    CinemachineVirtualCamera motionCamera = null;
    CinemachineVirtualCamera MotionCamera
    {
        get
        {
            if (motionCamera != null) return motionCamera;
            return motionCamera = GetComponent<CinemachineVirtualCamera>();
        }
    }
    protected override CinemachineVirtualCameraBase VritualCamera()
    {
        return motionCamera;
    }
    public override bool Enable
    {
        get
        {
            if (MotionCamera != null)
                return MotionCamera.enabled;
            else return false;
        }
        set
        {
            if (MotionCamera != null)
                MotionCamera.enabled = value;
        }
    }

    public override float FarClipPlane
    {
        set
        {
            if (MotionCamera != null)
                MotionCamera.m_Lens.FarClipPlane = value;
        }
    }

    public override void Init()
    {
        base.Init();
    }

    public override void UnInit()
    {
        base.UnInit();
    }
    public void SetMotionCamera(Vector3 position, Quaternion rotation)
    {
        if (MotionCamera != null && Enable)
        {
            MotionCamera.transform.position = position;
            MotionCamera.transform.rotation = rotation;
        }
    }

    #region Fov
    protected override float FieldOfView
    {
        get
        {
            if (MotionCamera != null)
                return MotionCamera.m_Lens.FieldOfView;
            return 60;
        }
        set
        {
            if (MotionCamera != null)
                MotionCamera.m_Lens.FieldOfView = value;
        }
    }
    #endregion
}
