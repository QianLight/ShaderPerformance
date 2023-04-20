using System.Collections.Generic;
using CFEngine;
using CFUtilPoolLib;
using Cinemachine;
using UnityEngine;

public class WanderController : CinemachineController, IWander
{
    CinemachineFreeLook freeLook = null;
    CinemachineFreeLook FreeLook
    {
        get
        {
            if (freeLook != null) return freeLook;
            freeLook = GetComponent<CinemachineFreeLook> ();
            _default_y = freeLook.m_YAxis.Value;
            _default_x = freeLook.m_XAxis.Value;
            return freeLook;
        }
    }
    protected override CinemachineVirtualCameraBase VritualCamera()
    {
        return FreeLook;
    }
    [Header("DragSpeed")]
    public float XDragSpeed;
    public float YDragSpeed;

    public float DragSpeedX { get { return XDragSpeed; } }
    public float DragSpeedY { get { return YDragSpeed; } }

    private float _default_y = 0;
    private float _default_x = 0;

    public override Transform LookAt
    {
        get
        {
            return FreeLook.LookAt;
        }
        set
        {
            FreeLook.LookAt = value;
        }
    }
    public override Transform Follow
    {
        get
        {
            return FreeLook.Follow;
        }
        set
        {
            FreeLook.Follow = value;
        }
    }

    public override bool Enable
    {
        get
        {
            if (FreeLook != null)
                return FreeLook.enabled;
            else return false;
        }
        set
        {
            if (FreeLook != null)
                FreeLook.enabled = value;
            //brain.enabled = value;
        }
    }

    public override void UnInit()
    {
        base.UnInit();

        freeLook.m_YAxis.Value = _default_y;
        freeLook.m_XAxis.Value = _default_x;
    }

    [Header ("TriggerAxis")]
    public float TriggerAxisX = 2;
    public float TriggerAxisY = 2;
    public float xInputAxisValue
    {
        set
        {
            FreeLook.m_InputX = Mathf.Abs (value) < TriggerAxisX ? 0 : (value - (value > 0 ? 1 : -1) * TriggerAxisX);
        }
        get
        {
            return FreeLook.m_InputX;
        }
    }
    public float yInputAxisValue
    {
        set
        {
            if (value == -10000000) guion = !guion;
            else FreeLook.m_InputY = Mathf.Abs(value) < TriggerAxisY ? 0 : (value - (value > 0 ? 1 : -1) * TriggerAxisY);
        }
        get
        {
            return FreeLook.m_InputY;
        }
    }
    public float xAxisValue
    {
        get { return FreeLook.m_XAxis.Value; }
        set { FreeLook.m_XAxis.Value = value; }
    }
    public float yAxisValue
    {
        get { return FreeLook.m_YAxis.Value; }
        set { FreeLook.m_YAxis.Value = value; }
    }
    public bool AxisInputValid
    {
        set
        {
            FreeLook.m_InputValid = value;
        }
    }
    public void InterruptAxisRecenter()
    {
        FreeLook.m_XAxis.InterruptAxisRecenter();
        FreeLook.m_YAxis.InterruptAxisRecenter();
    }
    private bool hasAxisInput = true;
    public bool HasAxisInput { get => hasAxisInput; set => hasAxisInput = value; }

    public override void Init ()
    {
        base.Init ();

        FreeLook.m_InputScriptControl = true;
    }

    private float _rotate_speed;
    private float _start_rotation;
    private float _target_rotation;
    // private float _rotate_current_time;
    private float _rotate_total_time;


    private float deltaAngle = 0;

    protected override void Update ()
    {
        base.Update ();

        if (FreeLook != null)
        {
            if (HostCamera != null)
            {
                if (brain != null && brain.ActiveVirtualCamera != null && brain.ActiveVirtualCamera.VirtualCameraGameObject == this.gameObject)
                    deltaAngle = FreeLook.m_XAxis.Value - hostCamera.transform.localEulerAngles.y;
            }
        }

        //if (Data != null) CopyData(Data);
    }

    #region Fov
    protected override float FieldOfView
    {
        get
        {
            if (FreeLook != null)
                return FreeLook.m_Lens.FieldOfView;
            return 60;
        }
        set
        {
            if (FreeLook != null)
                FreeLook.m_Lens.FieldOfView = value;
        }
    }

    #endregion

    bool guion = false;
    void OnGUI ()
    {
        if (guion)
        {
            GUIStyle style = new GUIStyle ();
            style.normal.textColor = Color.red;
            style.fontSize = 30;

            GUI.color = Color.red;
            GUI.Label (new Rect (Screen.width - 800, 50, 80, 100), new GUIContent ("X_Speed:" + (int) FreeLook.m_XAxis.m_MaxSpeed), style);
            FreeLook.m_XAxis.m_MaxSpeed = GUI.HorizontalSlider (new Rect (Screen.width - 600, 60, 550, 100), FreeLook.m_XAxis.m_MaxSpeed, 0, 500);

            GUI.Label (new Rect (Screen.width - 800, 150, 80, 100), new GUIContent ("Y_Speed:" + ((int) (FreeLook.m_YAxis.m_MaxSpeed * 10)) / 10.0f), style);
            FreeLook.m_YAxis.m_MaxSpeed = GUI.HorizontalSlider (new Rect (Screen.width - 600, 160, 550, 100), FreeLook.m_YAxis.m_MaxSpeed, 0, 10);

            GUI.Label (new Rect (Screen.width - 800, 250, 80, 100), new GUIContent ("X_Trigger:" + ((int) (TriggerAxisX * 10)) / 10.0f), style);
            TriggerAxisX = GUI.HorizontalSlider (new Rect (Screen.width - 600, 260, 550, 100), TriggerAxisX, 0, 10);

            GUI.Label (new Rect (Screen.width - 800, 350, 80, 100), new GUIContent ("Y_Trigger:" + ((int) (TriggerAxisY * 10)) / 10.0f), style);
            TriggerAxisY = GUI.HorizontalSlider (new Rect (Screen.width - 600, 360, 550, 100), TriggerAxisY, 0, 10);

            GUI.Label(new Rect(Screen.width - 800, 450, 80, 100), new GUIContent("X_Accel:" + freeLook.m_XAxis.m_AccelTime), style);
            freeLook.m_XAxis.m_AccelTime = GUI.HorizontalSlider(new Rect(Screen.width - 600, 460, 550, 100), freeLook.m_XAxis.m_AccelTime, 0, 3);

            GUI.Label(new Rect(Screen.width - 800, 550, 80, 100), new GUIContent("X_Decel:" + freeLook.m_XAxis.m_DecelTime), style);
            freeLook.m_XAxis.m_DecelTime = GUI.HorizontalSlider(new Rect(Screen.width - 600, 560, 550, 100), freeLook.m_XAxis.m_DecelTime, 0, 3);

            GUI.Label(new Rect(Screen.width - 800, 650, 80, 100), new GUIContent("Y_Accel:" + freeLook.m_YAxis.m_AccelTime), style);
            freeLook.m_YAxis.m_AccelTime = GUI.HorizontalSlider(new Rect(Screen.width - 600, 660, 550, 100), freeLook.m_YAxis.m_AccelTime, 0, 3);

            GUI.Label(new Rect(Screen.width - 800, 750, 80, 100), new GUIContent("Y_Decel:" + freeLook.m_YAxis.m_DecelTime), style);
            freeLook.m_YAxis.m_DecelTime = GUI.HorizontalSlider(new Rect(Screen.width - 600, 760, 550, 100), freeLook.m_YAxis.m_DecelTime, 0, 3);
        }
    }
}