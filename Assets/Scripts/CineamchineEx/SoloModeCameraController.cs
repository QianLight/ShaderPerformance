using System.Collections.Generic;
using CFEngine;
using CFUtilPoolLib;
using Cinemachine;
using UnityEngine;

public class SoloModeCameraController : CinemachineController, ISoloModeCamera
{
    public bool SetScreenYByHeight = true;
    public bool ShowGizmos = false;
    [Range (0, 0.99f)]
    public float ResumeParam = 0.98f;
    public float DeadRange = 4;
    public List<float> Range = new List<float> ();

    private Transform DeadLookAt;
    private Vector3 DeadPos;

    public bool ignoreY = false;
    public bool IgnoreY
    {
        get { return ignoreY; }
    }

    public Transform follow = null;
    public override Transform Follow
    {
        get
        {
            return follow;
        }
        set
        {
            follow = value;
            for (int i = 0; i < Range.Count; ++i)
                mixCamera.ChildCameras[i].Follow = follow;
        }
    }
    public Transform lookat = null;
    public override Transform LookAt
    {
        get
        {
            return lookat;
        }
        set
        {
            lookat = value;
            for (int i = 1; i < Range.Count; ++i)
                mixCamera.ChildCameras[i].LookAt = lookat;
        }
    }

    public override bool Enable
    {
        get
        {
            if (mixCamera != null)
                return mixCamera.enabled;
            else return false;
        }
        set
        {
            if (mixCamera != null)
            {
                mixCamera.enabled = value;
                for (int i = 1; i < Range.Count; ++i)
                    mixCamera.ChildCameras[i].enabled = value;
            }
            //brain.enabled = value;
        }
    }

    public override void UnInit ()
    {
        base.UnInit ();
        CinemachineFramingTransposer cinemachineFramingTransposer;
        for (int i = 0; i < Range.Count; i++)
        {
            cinemachineFramingTransposer = (mixCamera.ChildCameras[i] as CinemachineVirtualCamera).GetCinemachineComponent<CinemachineFramingTransposer>();
            cinemachineFramingTransposer.m_XDamping = soloDamping[i * 3 + 0];
            cinemachineFramingTransposer.m_YDamping = soloDamping[i * 3 + 1];
            cinemachineFramingTransposer.m_ZDamping = soloDamping[i * 3 + 2];
        }
        soloDamping = null;

        CFEngine.XGameObject.ReturnGameObject (ref DeadLookAt);
        if (mixCamera != null && Range.Count > 0) mixCamera.ChildCameras[0].LookAt = null;
    }

    private CinemachineMixingCamera _mix_camera = null;
    public CinemachineMixingCamera mixCamera
    {
        get
        {
            if (_mix_camera != null) return _mix_camera;
            return _mix_camera = GetComponent<CinemachineMixingCamera> ();
        }
    }

    protected override CinemachineVirtualCameraBase VritualCamera()
    {
        return mixCamera;
    }
    //public bool IsAimingPOV()
    //{
    //    return mixCamera.
    //}
    private bool m_InheritPosition
    {
        set
        {
            for (int i = 0; i < Range.Count; ++i)
                (mixCamera.ChildCameras[i] as CinemachineVirtualCamera).m_Transitions.m_InheritPosition = value;
        }
    }

    private float[] soloDamping = null;
    public override void Init ()
    {
        base.Init ();
        if (soloDamping == null)
        {
            soloDamping = new float[Range.Count * 3];
            CinemachineFramingTransposer cinemachineFramingTransposer;
            for (int i = 0; i < Range.Count; i++)
            {
                cinemachineFramingTransposer = (mixCamera.ChildCameras[i] as CinemachineVirtualCamera).GetCinemachineComponent<CinemachineFramingTransposer>();
                soloDamping[i * 3 + 0] = cinemachineFramingTransposer.m_XDamping;
                soloDamping[i * 3 + 1] = cinemachineFramingTransposer.m_YDamping;
                soloDamping[i * 3 + 2] = cinemachineFramingTransposer.m_ZDamping;
            }
        }

        m_InheritPosition = false;
        InheritPosition = false;

        for (int i = 1; i < Range.Count; ++i)
        {
            mixCamera.ChildCameras[i].Follow = Follow;
            mixCamera.ChildCameras[i].LookAt = LookAt;
            mixCamera.SetWeight(i, i == (Range.Count - 1) ? 1 : 0);
        }

        mixCamera.SetWeight (0, 0);
        mixCamera.ChildCameras[0].Follow = Follow;
        //(mixCamera.ChildCameras[0] as CinemachineVirtualCamera).isFirst = true;//

        DeadLookAt = CFEngine.XGameObject.GetGameObject ().transform;
        DeadLookAt.name = "DeadLookAt";
        if (mixCamera.ChildCameras[0].LookAt != null)
        {
            DeadLookAt.position = mixCamera.ChildCameras[0].LookAt.position;
        }
        mixCamera.ChildCameras[0].LookAt = DeadLookAt;
        flag = false;
    }

    public override void SpeInit()
    {
        Update();
    }

    public void SetInheritPosition()
    {
        if (mixCamera != null)
        {
            m_InheritPosition = true;
            InheritPosition = true;
        }
    }

    private bool InheritPosition = false;

    bool flag = false;
    float beginRange = 0;
    protected override void Update ()
    {
        base.Update ();

        if (Follow == null) return;
        if (LookAt == null) return;
        if (mixCamera == null) return;
        if (DeadLookAt == null) return;

        m_InheritPosition = InheritPosition;
        InheritPosition = false;

        float Dis = Vector3.Distance (Follow.position, LookAt.position);

        if (Dis <= DeadRange && !flag)
        {
            DeadPos += (LookAt.position - Follow.position).normalized * DeadRange;

            flag = true;

            
            for (int i = 0; i < Range.Count; ++i)
            {
                mixCamera.SetWeight (i, i == 0 ? 1 : 0);
            }
        }
        else if (Dis > DeadRange && flag)
        {
            flag = false;

            if (Vector3.Distance (Follow.position, mixCamera.ChildCameras[0].transform.position) >
                Vector3.Distance (LookAt.position, mixCamera.ChildCameras[0].transform.position) || DeadPos != Vector3.zero)
            {
                bool clockwise = XCommon.singleton.Clockwise (LookAt.position - Follow.position, LookAt.position - mixCamera.ChildCameras[0].transform.position);
                DeadPos += Follow.position - LookAt.position;
                beginRange = Vector3.Distance (DeadPos, Vector3.zero);
                DeadPos = Vector3.Distance (DeadPos, Vector3.zero) * DeadPos.normalized;
            }
            else DeadPos = Vector3.zero;
        }
        else if (!flag)
        {
            DeadLookAt.position = LookAt.position;
        }

        DeadLookAt.position = (flag ? Follow.position : LookAt.position) + DeadPos;
        if (!flag)
        {
            bool clockwise = XCommon.singleton.Clockwise (LookAt.position - Follow.position, LookAt.position - mixCamera.ChildCameras[0].transform.position);
            float precent = (beginRange == 0 ? 0 : Mathf.Max (0, Vector3.Distance (DeadPos, Vector3.zero) / beginRange)) * ResumeParam;
            DeadPos = beginRange * precent * (DeadPos.normalized * precent + XCommon.singleton.HorizontalRotateVetor3 (Follow.position - LookAt.position, clockwise ? -90 : 90) * (1 - precent)).normalized;
            
            for (int i = 0; i < Range.Count; ++i)
            {
                mixCamera.SetWeight (i, 0);
                if (Dis < Range[i])
                {
                    if (i == 0)
                        mixCamera.SetWeight (i, 1);
                    else if (Dis > Range[i - 1])
                    {
                        precent = (Dis - Range[i - 1]) / (Range[i] - Range[i - 1]);
                        mixCamera.SetWeight (i, precent);
                    }
                }
                else if (Dis >= Range[i])
                {
                    if (i == Range.Count - 1)
                    {
                        mixCamera.SetWeight (i, 1);
                    }
                    else if (Dis < Range[i + 1])
                    {
                        precent = (Dis - Range[i]) / (Range[i + 1] - Range[i]);
                        mixCamera.SetWeight (i, 1 - precent);
                    }
                }
            }
        }
    }

    #region TableParamData
    public override void SetTableParamData(float[] param)
    {
        if (IgnoreTableData || param == null || param.Length == 0)
        {
            ResetTableParamData();
            return;
        }

        if (mixCamera != null)
        {
            int index = 0;
            for (int i = 3; i < param.Length && index < mixCamera.ChildCameras.Length; i += 3, ++index)
            {
                CinemachineVirtualCamera vc = (mixCamera.ChildCameras[index] as CinemachineVirtualCamera);
                if (vc == null) continue;
                CinemachineFramingTransposer cft = vc.GetCinemachineComponent<CinemachineFramingTransposer>();
                if (cft == null) continue;
                CinemachineComposer cc = vc.GetCinemachineComponent<CinemachineComposer>();
                if (cc == null) continue;
                cft.SetCustomScale(param[i - 2], param[i]);
                cc.SetCustomScale(param[i - 1]);
            }
        }
    }
    private void ResetTableParamData()
    {
        if (mixCamera != null)
        {
            int index = 0;
            for (;index < mixCamera.ChildCameras.Length; ++index)
            {
                CinemachineVirtualCamera vc = (mixCamera.ChildCameras[index] as CinemachineVirtualCamera);
                if (vc == null) continue;
                CinemachineFramingTransposer cft = vc.GetCinemachineComponent<CinemachineFramingTransposer>();
                if (cft == null) continue;
                CinemachineComposer cc = vc.GetCinemachineComponent<CinemachineComposer>();
                if (cc == null) continue;
                cft.SetCustomScale(1f, 1f);
                cc.SetCustomScale(1f);
            }
        }
    }
    #endregion

    #region damping
    protected override float Damping
    {
        set
        {
            CinemachineFramingTransposer cinemachineFramingTransposer;
            for (int i = 0; i < Range.Count; i++)
            {
                cinemachineFramingTransposer = (mixCamera.ChildCameras[i] as CinemachineVirtualCamera).GetCinemachineComponent<CinemachineFramingTransposer>();
                cinemachineFramingTransposer.m_XDamping = value;
                cinemachineFramingTransposer.m_YDamping = value;
                cinemachineFramingTransposer.m_ZDamping = value;
            }
        }
    }
    #endregion

    #region Fov
    protected override float FieldOfView
    {
        get
        {
            if (mixCamera != null)
                return (mixCamera.ChildCameras[0] as CinemachineVirtualCamera).m_Lens.FieldOfView;
            return 60;
        }
        set
        {
            if (mixCamera != null)
            {
                for (int i = 0; i < Range.Count; ++i)
                {
                    (mixCamera.ChildCameras[i] as CinemachineVirtualCamera).m_Lens.FieldOfView = value;
                }
            }
        }
    }
    #endregion

    #region input
    public bool HasPOV()
    {
        if (mSelectedPOV == null)
            return false;
        mSelectedPOV.m_VerticalAxis.m_InputAxisName = "";
        mSelectedPOV.m_HorizontalAxis.m_InputAxisName = "";//设为空后不会自动刷新
        return true;
    }

    [Header("TriggerAxis")]
    public float TriggerAxisX = 2;
    public float TriggerAxisY = 2;

    private CinemachinePOV mSelectedPOV
    {
        get
        {
            CinemachinePOV pov = null;
            if (_selectedIndex > -1 && _selectedIndex < Range.Count
                && mixCamera.IsLiveChild(mixCamera.ChildCameras[_selectedIndex])
                && (pov = (mixCamera.ChildCameras[_selectedIndex] as CinemachineVirtualCamera).GetCinemachineComponent<CinemachinePOV>()) != null)
                return pov;

            CinemachineVirtualCamera virtualCamera;
            for (int i = 0; i < Range.Count; i++)
            {
                if (mixCamera.IsLiveChild(mixCamera.ChildCameras[i]))
                {
                    virtualCamera = (mixCamera.ChildCameras[i] as CinemachineVirtualCamera);
                    pov = virtualCamera.GetCinemachineComponent<CinemachinePOV>();
                    if (pov != null)
                    {
                        virtualCamera.LockPosY = false;
                        _selectedIndex = i;
                        return pov;
                    }
                }
            }
            return pov;
        }
    }
    private int _selectedIndex = -1;
    //private CinemachinePOV _selectedPOV;

    public float xInputAxisValue { get => xAxisValue; set => xAxisValue = value; }
    public float yInputAxisValue { get => yAxisValue; set => yAxisValue = value; }
    public float xAxisValue {
        get { 
            if (mSelectedPOV != null)
                return mSelectedPOV.m_HorizontalAxis.m_InputAxisValue;
            else
                return 0; 
        }
        set
        {
            if (mSelectedPOV != null)
                mSelectedPOV.m_HorizontalAxis.m_InputAxisValue = value;
        }
    }
    public float yAxisValue
    {
        get
        {
            if (mSelectedPOV != null)
                return mSelectedPOV.m_VerticalAxis.m_InputAxisValue;
            else
                return 0;
        }
        set
        {
            if (mSelectedPOV != null)
                mSelectedPOV.m_VerticalAxis.m_InputAxisValue = value;
        }
    }
    public bool AxisInputValid
    {
        set
        {
            mSelectedPOV.m_VerticalAxis.m_InputAxisValueValid = value;
            mSelectedPOV.m_HorizontalAxis.m_InputAxisValueValid = value;
        }
    }
    public void InterruptAxisRecenter()
    {
        mSelectedPOV.m_VerticalAxis.InterruptAxisRecenter();
        mSelectedPOV.m_HorizontalAxis.InterruptAxisRecenter();
    }
    private bool hasAxisInput = true;
    public bool HasAxisInput { get => hasAxisInput; set => hasAxisInput = value; }
    #endregion

    #region gizmos
    private void OnDrawGizmos ()
    {
        if (ShowGizmos)
        {
            Gizmos.color = Color.red;
            if (LookAt == null) return;

            DrawCircle (LookAt.position, DeadRange);
            for (int i = 0; i < Range.Count; ++i)
            {
                int c = i + 2;
                Gizmos.color = new Color ((c & 1) != 0 ? 1f : 0f, (c & 2) != 0 ? 1f : 0f, (c & 4) != 0 ? 1f : 0f);
                DrawCircle (LookAt.position, Range[i]);
            }
        }
    }

    public static void DrawCircle (Vector3 _center, float _radius, int _lineNum = 60)
    {
        Vector3 forwardLine = Vector3.forward * _radius;
        Vector3 curPos = _center + forwardLine;
        Vector3 prePos = curPos;

        for (int i = 0; i < _lineNum; i++)
        {
            forwardLine = _radius * XCommon.singleton.HorizontalRotateVetor3 (forwardLine, 360f / _lineNum);
            curPos = forwardLine + _center;
            Gizmos.DrawLine (prePos, curPos);
            prePos = curPos;
        }
    }

    public void ResetParam(float[] param)
    {
        if (param == null || param.Length < 10)
        {
            XDebug.singleton.AddErrorLog2("参数是null 或 数量小于10");
            return;
        }
        CinemachineVirtualCameraBase[] virtualCameras = mixCamera.ChildCameras;
        if (virtualCameras == null || virtualCameras.Length < 2)
        {
            XDebug.singleton.AddErrorLog2("相机是null 或 数量小于2");
            return;
        }
        for (int i = 0; i < 2; ++i)
        {
            CinemachineVirtualCamera virtualCamera = virtualCameras[i] as CinemachineVirtualCamera;
            if (virtualCamera != null)
            {
                virtualCamera.PosY = param[0 + 5 * i];
                virtualCamera.RotX = param[1 + 5 * i];
                virtualCamera.MinYBasedOnFollow = param[2 + 5 * i];
                virtualCamera.MaxYBasedOnFollow = param[3 + 5 * i];
                CinemachineComponentBase component = virtualCamera.GetCinemachineComponent(CinemachineCore.Stage.Body);
                if (component is CinemachineFramingTransposer)
                {
                    (component as CinemachineFramingTransposer).CameraDistance = param[4 + 5 * i];
                }
            }
        }
    }
    #endregion
}