using CFUtilPoolLib;
using Cinemachine;
using System.Collections.Generic;
using CFEngine;
using UnityEngine;

public class CinemachineController : MonoBehaviour, ICinemachine
{
    public bool IgnoreTableData = false;

    protected GameObject hostCamera = null;
    protected CinemachineBrain brain = null;

    public GameObject HostCamera
    {
        get
        {
            if (hostCamera == null)
            {
                hostCamera = EngineContext.instance?.CameraRef?.gameObject;
                if (hostCamera == null)
                    hostCamera = GameObject.Find("Main Camera");
                brain = hostCamera.GetComponent<CinemachineBrain>();
                if (brain == null) brain = hostCamera.AddComponent<CinemachineBrain>();
                brain.enabled = true;
            }

            return hostCamera;
        }
    }

    public bool IsBlending
    {
        get
        {
            if (HostCamera != null && brain != null)
            {
                return brain.IsBlending;
            }
            return false;
        }
    }

    public bool IsInBlending
    {
        get
        {
            return IsBlending && brain.ActiveBlend.Uses(VritualCamera());
        }
    }
    protected virtual CinemachineVirtualCameraBase VritualCamera()
    {
        throw new System.NotFiniteNumberException();
    }
    public virtual void SpeInit()
    {
        throw new System.NotImplementedException();
    }
    public float BlendTime
    {
        set
        {
            if (HostCamera != null && brain != null)
            {
                brain.m_DefaultBlend.m_Time = value;
            }
        }
    }

    public int BlendStyle
    {
        set
        {
            if (HostCamera != null && brain != null)
            {
                brain.m_DefaultBlend.m_Style = (CinemachineBlendDefinition.Style)value;
            }
        }
    }

    public virtual float FarClipPlane { set { } }

    public virtual Transform LookAt { get; set; }
    public virtual Transform Follow { get; set; }
    public virtual bool Enable { get; set; }
    public bool Deprecated { get; set; }

    public virtual void Init()
    {
        if (Instances.IndexOf(this) < 0)
            Instances.Add(this);
        hostCamera = EngineContext.instance?.CameraRef?.gameObject;
        if (hostCamera == null)
            hostCamera = GameObject.Find("Main Camera");
        brain = hostCamera.GetComponent<CinemachineBrain>();
        if (brain == null) brain = hostCamera.AddComponent<CinemachineBrain>();
        brain.enabled = true;

        InitHosterAlpha();
    }

    public virtual void UnInit()
    {
        int index = Instances.IndexOf(this);
        if (index >= 0)
            Instances.RemoveAt(index);
        
        ResetDamping(0);
        ResetFov(0);
        hostCamera = null;
        brain = null;
        Follow = null;
        LookAt = null;
    }

    protected virtual void Update()
    {
        var deltaTime = Time.deltaTime*TimeScale;
        UpdateDamping(deltaTime);
        UpdateFov(deltaTime);
        UpdateDummyLookAt();
    }

    public virtual void SetTableParamData(float[] param) { }


    #region LookAt
    private Transform _dummyLookAt;
    public Transform DummyLookAt => _dummyLookAt;
    protected float[] targetParam = null;
    virtual protected float[] TargetParam()
    {
        return null;
    }
    public void ChangeLookat(float[] cameraParam)
    {
        if (this is FreeLookController && (this as FreeLookController).ForSpecialSetting) return;//freelook特殊相机特殊处理
        if (this is SoloModeCameraController && (this as SoloModeCameraController).SetScreenYByHeight == false) return;//solo状态不对巨型怪物修改

        Transform dummy;
        if (LookAt == null)
            return;

        if (LookAt.name == "dummyLookat")
            dummy = LookAt;
        else
            dummy = LookAt.Find("dummyLookat");
        if (dummy == null)
        {
            dummy = CFEngine.XGameObject.GetGameObject().transform;
            dummy.name = "dummyLookat";
            dummy.transform.SetParent(LookAt);
        }
        _dummyLookAt = dummy;
        LookAt = dummy;

        if (cameraParam != null)
        {
            if (cameraParam.Length > 0)
            {
                dummy.localPosition = cameraParam[0] * Vector3.up;
            }
            if (cameraParam.Length > 9)
            {
                float battleHeightOffset = 0;
                if (this is FreeLookController)
                {
                    targetParam = new float[9] {
                        cameraParam[1],
                        cameraParam[2],
                        cameraParam[3],
                        cameraParam[4],
                        cameraParam[5],
                        cameraParam[6],
                        cameraParam[7],
                        cameraParam[8],
                        cameraParam[9],
                    };
                    (this as FreeLookController).ChangeParam2Taregt();
                }
            }
        }
    }

    public void SetCameraAcoidGroup(GameObject player)
    {
        if (player != null && player.TryGetComponent<CameraBlockGroup>(out var blockGroup))
        {
            blockGroup.enabled = false;
        }
    }
    private void UpdateDummyLookAt()
    {
        if (_dummyLookAt == null) return;
    }
    #endregion

    #region HosterAlphaControl
    [Header("HosterAlpha")]
    [SerializeField, Range(0, 1)]
    float _min_alpha = 1.0f;
    [SerializeField]
    float _min_alpha_range = 0.0f;
    [SerializeField]
    float _max_alpha_range = 1.0f;

    float _alpha_param;
    float _alpha_delta_range;
    protected void InitHosterAlpha()
    {
        _alpha_param = 1.0f - _min_alpha;
        _alpha_delta_range = _max_alpha_range - _min_alpha_range + 0.00001f;
    }

    public float GetHosterAlpha()
    {
        if (_alpha_delta_range <= 0) return 2.0f;

        if (Follow != null && HostCamera != null)
        {
            Vector3 tempPos = hostCamera.transform.position;
            tempPos.y = Follow.position.y;
            float dis = Vector3.Distance(Follow.position, tempPos);
            if (dis < _min_alpha_range)
            {
                return _min_alpha;
            }

            if (dis > _max_alpha_range)
            {
                return 2f;
            }

            return _min_alpha + Mathf.Min(1.0f, (dis -_min_alpha_range) / _alpha_delta_range) * _alpha_param;
        }

        return 2.0f;
    }
    #endregion

    /// <summary>
    /// 是否正在混合中
    /// </summary>
    public bool BrainIsBlending()
    {
        if (brain != null && brain.isActiveAndEnabled)
            return brain.IsBlending;
        return false;
    }
    public bool OpenAvoidBlcok
    {
        get
        {
            return openavoidblock;
        }
        set
        {

            openavoidblock = value;
            if (HostCamera != null && HostCamera.TryGetComponent<CameraAvoidBlock>(out var cab))
            {
                cab.enabled = value;
            }
        }
    }
    private bool openavoidblock = true;

    #region shake

    public int SetImpulse(string path, float amplitudeGain, float frequencyGain, float time, float attackTime, float decayTime, Vector3 pos, float radius)
    {
        return CinemachineShake.singleton.SetImpulse(path, amplitudeGain, frequencyGain, time, attackTime, decayTime, pos, radius);
    }

    public void CancelImpulse(int id)
    {
        CinemachineShake.singleton.Cancel(id);
    }
    #endregion

    #region damping 
    private float _damping_current_time;
    private float _damping_total_time;
    AnimationCurve _dampingCurve;
    private float _default_damping = 0f;
    private float _itr_damping;
    protected virtual float Damping { set; get; }
    public void SetDamping(float time,AnimationCurve curve=null)
    {
        if (time != 0)
        {
            _dampingCurve = curve;
            _damping_current_time = 0;
            _damping_total_time = time;
        }
        if (curve != null)
            Damping = curve.Evaluate(0);
    }
    public void ResetDamping(float time)
    {
        if (time == 0)
        {
            _damping_total_time = 0;
            Damping = _default_damping;
        }
        else
            SetDamping(time);
    }
    public void UpdateDamping(float delt)
    {
        if (_damping_total_time == 0) return;
        _damping_current_time += delt;
        var percent = _damping_current_time / _damping_total_time;
        if (percent > 1)
        {
            _damping_total_time = 0;
            percent = 1;
        }
        if (_dampingCurve == null)
            _itr_damping = _default_damping+(1-percent)*_itr_damping;//中断后拟合
        else
            _itr_damping = _dampingCurve.Evaluate(percent);//曲线拟合
        Damping = _itr_damping;
        if (percent == 1) ResetDamping(0.5f);
    }
    #endregion

    #region Fov
    private float _fov_current_time=-1;
    private float _fov_fadein_time;
    private AnimationCurve fadeInCurve;
    private float _fov_last_time;
    private float _fov_fadeout_time;//数值为负表示不会返回默认值
    private AnimationCurve fadeOutCurve;//数值为负表示不会返回默认值
    [Header("FOV")]
    public float _fov_default=55;
    private float _fov_target;
    private float _fov_init;
    public static List<CinemachineController> Instances { get; } = new List<CinemachineController>();
    protected virtual float FieldOfView{ get; set; }
    public void SetFov(float fov, float fadeinTime, AnimationCurve fadeInCurve, float lastTime, float fadeoutTime, AnimationCurve fadeOutCurve)
    {
        _fov_target = fov;
        _fov_init = FieldOfView;
        _fov_current_time = 0;
        _fov_fadein_time = fadeinTime;
        this.fadeInCurve = fadeInCurve;
        _fov_last_time = lastTime;
        _fov_fadeout_time = fadeoutTime;
        this.fadeOutCurve = fadeOutCurve;
        UpdateFov(0);
    }
    public void ResetFov(float resetTime)
    {
        if (resetTime == 0)
            FieldOfView = _fov_default;
        //FieldOfView = _fov_init != 0 ? _fov_init : _default_fov;
        else
            SetFov(_fov_default, resetTime, null, 0, -1, null);
    }

    public void UpdateFov(float deltaTime)
    {
        if (_fov_current_time < 0) return;
        _fov_current_time += deltaTime;
        if (_fov_current_time < _fov_fadein_time)
        {
            if (fadeInCurve == null) 
            {
                float percent = _fov_current_time / _fov_fadein_time;
                FieldOfView = _fov_init * (1 - percent) + _fov_target * percent;
            }
            else
            {
                float percent = _fov_current_time / _fov_fadein_time;
                percent = fadeInCurve.Evaluate(percent);
                FieldOfView = _fov_init * (1 - percent) + _fov_target * percent;
            }
        }
        else if (_fov_current_time < _fov_fadein_time + _fov_last_time)
        {
            FieldOfView = _fov_target;
        }
        else if (_fov_fadeout_time < 0)//不会返回默认值的情况
        {
            FieldOfView = _fov_target;
            _fov_current_time = -1;
        }
        else if(_fov_current_time < _fov_fadein_time + _fov_last_time+_fov_fadeout_time)
        {
            if (fadeOutCurve == null)
            {
                float percent = (_fov_current_time - (_fov_fadein_time + _fov_last_time)) / _fov_fadeout_time;
                FieldOfView = _fov_init * percent + _fov_target * (1 - percent);
            }
            else
            {
                float percent = (_fov_current_time - (_fov_fadein_time + _fov_last_time)) / _fov_fadeout_time;
                percent = fadeOutCurve.Evaluate(percent);
                FieldOfView = _fov_init * percent + _fov_target * (1 - percent);
            }
        }
        else
        {
            FieldOfView = _fov_init;
            _fov_current_time = -1;
        }
    }
    #endregion
    #region TimeScale
    private float timescale =1;
    public float TimeScale { get => timescale; set =>timescale=value; }
    #endregion

    #region CameraGUI
    private static bool GUIEnable = false;
    Rect mCamGUIWindow = new Rect(20, 20, 300, 600);
    public void EnableGUI()
    {
        GUIEnable = !GUIEnable;
    }
    private void OnGUI()
    {
        if (GUIEnable && brain!=null && brain.isActiveAndEnabled)
        {
            ICinemachineCamera live = null;
            if (brain.mCurrentLiveCameras.CamA == null)
                if (brain.mCurrentLiveCameras.CamB == null)
                    return;
                else
                    live = brain.mCurrentLiveCameras.CamB;
            else
                live = brain.mCurrentLiveCameras.CamA;
            if (live?.VirtualCameraGameObject == null) return;
            string name = live.Name;
            var followName = live.Follow == null ? "" : live.Follow.name;
            var lookatName = live.LookAt == null ? "" : live.LookAt.name;
            var camPos = brain.transform.position;
            var cameraForward = brain.transform.forward;cameraForward.y = 0;
            var isBlending = brain.IsBlending;

            Ray cameraRay = Camera.main.ScreenPointToRay(new Vector2(Screen.width, Screen.height) / 2);
            float yAngle = 90 - Vector3.Angle(cameraRay.direction, Vector3.down); 

            float camDir=-1, distance=-1;
            if (live.LookAt != null)
            {
                camDir = Vector3.SignedAngle(live.LookAt.forward, cameraForward, Vector3.up);
                distance = Vector3.Distance(camPos, live.LookAt.position);
            }
            GUIStyle style = new GUIStyle();
            style.fontSize = 20;
            style.normal.textColor = Color.white;

            GUI.TextArea(new Rect(20, 20, 200, 50), "相机名称：" + name,style);
            GUI.TextArea(new Rect(20, 50, 200, 50), "相机位置：" + camPos, style);
            GUI.TextArea(new Rect(20, 80, 200, 50), "是否拟合中：" + isBlending, style);
            GUI.TextArea(new Rect(20, 110, 200, 50), "镜头与XZ平面夹角：" + yAngle, style);
            GUI.TextArea(new Rect(20, 140, 200, 50), "镜头与玩家朝向夹角：" + camDir, style);
            GUI.TextArea(new Rect(20, 170, 200, 50), "镜头与玩家距离：" + distance, style);
            GUI.TextArea(new Rect(20, 200, 200, 50), "镜头跟随点：" + followName, style);
            GUI.TextArea(new Rect(20, 230, 200, 50), "镜头看向点：" + lookatName, style);
        }
    }
    #endregion
}