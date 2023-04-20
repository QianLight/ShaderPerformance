using CFEngine;
using CFUtilPoolLib;
using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class TrackController : CinemachineController, ICinemachine, ICameraAxis,ITrackEventTrigger
{
    public CinemachineVirtualCamera virtualCamera;
    public CinemachineVirtualCamera POVCamera;
    private CinemachinePOV POV;
    public CinemachineVirtualCamera OtherCamera;
    public float POVHorizontalValueMinRange = -180;
    public float POVHorizontalValueMaxRange = 180;

    protected override CinemachineVirtualCameraBase VritualCamera()
    {
        return virtualCamera;
    }
    public CinemachineBrain.UpdateMethod updateMethod = CinemachineBrain.UpdateMethod.SmartUpdate;
    public override bool Enable
    {
        get
        {
            if (virtualCamera != null)
                return virtualCamera.enabled;
            else return false;
        }
        set
        {
            if (virtualCamera != null)
                virtualCamera.enabled = value;
        }
    }

    public override Transform LookAt
    {
        get
        {
            return virtualCamera.LookAt;
        }
        set
        {
            virtualCamera.LookAt = value;
            if (OtherCamera != null) OtherCamera.LookAt = value;
        }
    }
    public override Transform Follow
    {
        get
        {
            return virtualCamera.Follow;
        }
        set
        {
            virtualCamera.Follow = value;
        }
    }

    protected override float FieldOfView 
    {
        get
        {
            if (POVCamera != null)
                return POVCamera.m_Lens.FieldOfView;
            if (virtualCamera != null)
                return virtualCamera.m_Lens.FieldOfView;
            return 60;
        }
        set
        {
            if (POVCamera != null)
                POVCamera.m_Lens.FieldOfView = value;
            if (virtualCamera != null)
                virtualCamera.m_Lens.FieldOfView = value;
        }
    }

    public override void Init()
    {
        base.Init();

        if (POVCamera != null) POV = POVCamera.GetCinemachineComponent<CinemachinePOV>();
        if (HostCamera != null && brain != null)
        {
            brain.m_UpdateMethod = updateMethod;
        }
        InitDollyCart();
    }

    protected override void Update()
    {
        base.Update();

        UpdateEvent();

        UpdatePOV();
    }

    #region POV
    public float xAxisValue 
    {
        get
        {
            if (POV != null)
                return POV.m_HorizontalAxis.m_InputAxisValue;
            else
                return 0;
        }
        set
        {
            if (POV != null)
                POV.m_HorizontalAxis.m_InputAxisValue = value;
        }
    }
    public float yAxisValue 
    {
        get
        {
            if (POV != null)
                return POV.m_VerticalAxis.m_InputAxisValue;
            else
                return 0;
        }
        set
        {
            if (POV != null)
                POV.m_VerticalAxis.m_InputAxisValue = value;
        }
    }
    public float xInputAxisValue { get => xAxisValue; set => xAxisValue = value; }
    public float yInputAxisValue { get => yAxisValue; set => yAxisValue = value; }
    private bool hasAxisInput = true;
    public bool HasAxisInput { get => hasAxisInput; set => hasAxisInput = value; }
    public bool AxisInputValid
    {
        set
        {
            POV.m_VerticalAxis.m_InputAxisValueValid = value;
            POV.m_HorizontalAxis.m_InputAxisValueValid = value;
        }
    }
    public void InterruptAxisRecenter()
    {
        POV.m_VerticalAxis.InterruptAxisRecenter();
        POV.m_HorizontalAxis.InterruptAxisRecenter();
    }
    private void UpdatePOV()
    {
        if (POV == null) return;

        var target = POV.GetRecenterTarget();
        while (target.x < POV.m_HorizontalAxis.m_MinValue)
        {
            POV.m_HorizontalAxis.Value -= 360;
            POV.m_HorizontalAxis.m_MinValue -= 360;
            POV.m_HorizontalAxis.m_MaxValue -= 360;
        }
        while (target.x > POV.m_HorizontalAxis.m_MaxValue)
        {

            POV.m_HorizontalAxis.Value += 360;
            POV.m_HorizontalAxis.m_MinValue += 360;
            POV.m_HorizontalAxis.m_MaxValue += 360;
        }

        POV.m_HorizontalAxis.m_MinValue = target.x + POVHorizontalValueMinRange;
        POV.m_HorizontalAxis.m_MaxValue = target.x + POVHorizontalValueMaxRange;
    }
#endregion

    #region DollyCart
    public CinemachineDollyCart cart;
    public enum TrackEvent
    {
        Speed,
        Shoot,
        FOV,
        Camera,
    }
    [System.Serializable]
    public class TrackEventObj
    {
        public float pos;
        public TrackEvent triggers;
        public float value;
        public float value2;
    }
    [Tooltip("Speed:\tvalue (速度)\n" +
        "Shoot:\tvalue (UI显隐 1.显示 0.隐藏)\n\tvalue2 (速度)\n" +
        "FOV:\tvaule (FOV值)\n\tvalue2 (时间)\n" +
        "Camera:\tvalue(0.默认相机 1.other相机)\n\tvalue2 (拟合时间)\n")]
    public List<TrackEventObj> events = new List<TrackEventObj>();
    public int eventIndex = 0;
    private float _last_cart_pos;
    private Queue<TrackEventObj> _event_queue = new Queue<TrackEventObj>();
    private void InitDollyCart()
    {
        if (cart != null && events.Count != 0)
        {
            _last_cart_pos = GetCartPathUnits;
            for (int i = 0; i < events.Count; ++i)
            {
                if (events[i].pos >= GetCartPathUnits)
                    _event_queue.Enqueue(events[i]);
            }
        }
    }

    private float GetCartPathUnits => cart.m_Path.ToNativePathUnits(cart.m_Position, cart.m_PositionUnits);

    private void UpdateEvent()
    {
        if (cart != null && events.Count != 0)
        {
            if (_last_cart_pos > GetCartPathUnits)
            {
                while (_event_queue.Count > 0)
                    TriggerEvent(_event_queue.Dequeue());
                for (int i = 0; i < events.Count; ++i)
                    _event_queue.Enqueue(events[i]);
            }
            while (_event_queue.Count > 0 && 
                _event_queue.Peek().pos < GetCartPathUnits)
            {
                TriggerEvent(_event_queue.Dequeue());
            }
            
            _last_cart_pos = GetCartPathUnits;
        }
    }

    private TrackEventObj _track_event_obj = new TrackEventObj();
    public void TriggerTrackEvent(int type, List<float> values)
    {
        _track_event_obj.triggers = (TrackEvent)type;
        _track_event_obj.value = values.Count > 0 ? values[0] : 0;
        _track_event_obj.value2 = values.Count > 1 ? values[1] : 0;
        TriggerEvent(_track_event_obj);
    }

    private void TriggerEvent(TrackEventObj obj)
    {
        switch(obj.triggers)
        {
            case TrackEvent.Speed:
                cart.m_Speed = obj.value;
                break;
            case TrackEvent.Shoot:
                var view = CFClient.UI.XUITools.GetSpecificViewInCache(CFClient.UI.XUIHosts.ShootScene) as CFClient.UI.CFShootSceneView;
                view.SetActive(obj.value != 0);
                view.SetShootStatus(obj.value != 0);

                cart.m_Speed = obj.value2;
                break;
            case TrackEvent.FOV:
                SetFov(obj.value, obj.value2, null, 0, -1, null);
                break;
            case TrackEvent.Camera:
                POVCamera.gameObject.SetActive(obj.value == 0);
                OtherCamera.gameObject.SetActive(obj.value == 1);
                BlendTime = obj.value2;
                break;
        }
    }
    #endregion
}
