using CFEngine;
using CFUtilPoolLib;
using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class LevelCameraController : CinemachineController,ILevelCamera
{
    CinemachineVirtualCamera virtualCamera = null;
    public CinemachineVirtualCamera VirtualCamera
    {
        get
        {
            if (virtualCamera != null) return virtualCamera;
            virtualCamera = GetComponent<CinemachineVirtualCamera>();
            return virtualCamera;
        }
    }
    protected override CinemachineVirtualCameraBase VritualCamera()
    {
        return VirtualCamera;
    }
    public override Transform LookAt { get => VirtualCamera.LookAt; set => VirtualCamera.LookAt=value; }
    public override Transform Follow { get => VirtualCamera.Follow; set => VirtualCamera.Follow = value; }

    public override bool Enable
    {
        get
        {
            if (VirtualCamera != null)
                return VirtualCamera.enabled;
            else 
                return false;
        }
        set
        {
            if(VirtualCamera!=null)
                VirtualCamera.enabled = value;
        }
    }

    public float xAxisValue
    {
        get
        {
            CinemachineComponentBase component = VirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Aim);
            if (component is CinemachinePOV)
            {
                return (component as CinemachinePOV).m_HorizontalAxis.Value;
            }
            return 0;
        }
        set
        {
            CinemachineComponentBase component = VirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Aim);
            if (component is CinemachinePOV)
            {
                (component as CinemachinePOV).m_HorizontalAxis.Value = value;
            }
        }
    }
    public float yAxisValue
    {
        get
        {
            CinemachineComponentBase component = VirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Aim);
            if (component is CinemachinePOV)
            {
                return (component as CinemachinePOV).m_VerticalAxis.Value;
            }
            return 0;
        }
        set
        {
            CinemachineComponentBase component = VirtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Aim);
            if (component is CinemachinePOV)
            {
                (component as CinemachinePOV).m_VerticalAxis.Value = value;
            }
        }
    }
    public float xInputAxisValue
    {
        set
        {
            if (IsBlending || value > 10000)
            {
                VirtualCamera.m_InputX = 0;
                AxisInputValid = false;
                return;
            }
            VirtualCamera.m_InputX = value;
        }
        get
        {
            return VirtualCamera.m_InputX;
        }
    }
    public float yInputAxisValue
    {
        set
        {
            if (brain.IsBlending || value > 10000)
            {
                VirtualCamera.m_InputY = 0;
                AxisInputValid = false;
                return;
            }
            VirtualCamera.m_InputY = value;
        }
        get
        {
            return VirtualCamera.m_InputY;
        }
    }
    public bool HasAxisInput { get; set; }
    public bool AxisInputValid
    {
        set
        {
            VirtualCamera.m_InputValid = value;
        }
    }

    public void InterruptAxisRecenter()
    {
        VirtualCamera.InterruptAxisRecenter();
    }

    public override void SetTableParamData(float[] param)
    {
        if (VirtualCamera == null) return;
        if (IgnoreTableData || param == null || param.Length == 0)
            return;
        SetCamParam(param);
    }
    private void SetCamParam(float[] param)
    {
        //
    }
    public override void Init()
    {
        base.Init();
    }
    public override void UnInit()
    {
        base.UnInit();
    }
    protected override void Update()
    {
        base.Update();
    }
}
