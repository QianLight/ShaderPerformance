using CFEngine;
using CFUtilPoolLib;
using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class BlendListController : CinemachineController, ICinemachine
{
    CinemachineVirtualCameraBase blendListCamera = null;
    CinemachineVirtualCameraBase BlendListCamera
    {
        get
        {
            if (blendListCamera != null) return blendListCamera;
            return blendListCamera = GetComponent<CinemachineBlendListCamera>();
        }
    }
    protected override CinemachineVirtualCameraBase VritualCamera()
    {
        return BlendListCamera;
    }
    public override void Init()
    {
        base.Init();
    }
    public override bool Enable
    {
        get
        {
            if (BlendListCamera != null)
                return BlendListCamera.enabled;
            else return false;
        }
        set
        {
            if (BlendListCamera != null)
                BlendListCamera.enabled = value;
        }
    }

    public override Transform LookAt
    {
        get
        {
            return BlendListCamera.LookAt;
        }
        set
        {
            BlendListCamera.LookAt = value;
        }
    }
    public override Transform Follow
    {
        get
        {
            return BlendListCamera.Follow;
        }
        set
        {
            BlendListCamera.Follow = value;
        }
    }
}
