using System.Collections.Generic;
using CFUtilPoolLib;
using Cinemachine;
using UnityEngine;
using CFClient;
using CFEngine;
using System;

public class FreeLookController : CinemachineController, IFreeLook
{
    /// <summary>
    /// 用于存放读表对象 ， 不想把引入对应类型 ， 用object记录他的引用
    /// </summary>
    public object tableClass;
    protected CinemachineFreeLook freeLook = null;
    public CinemachineFreeLook FreeLook
    {
        get
        {
            if (freeLook != null) return freeLook;
            freeLook = GetComponent<CinemachineFreeLook>();
            //_default_y = freeLook.m_YAxis.Value;
            return freeLook;
        }
    }
    protected override CinemachineVirtualCameraBase VritualCamera()
    {
        return FreeLook;
    }
    private float _default_y = 0.5f;

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
            {
                FreeLook.enabled = value;
            }
            if (value == true)
            {
                if(FormerYAxis!=-999)
                    FreeLook.m_YAxis.Value = FormerYAxis;
                else
                    FreeLook.m_YAxis.Value = _default_y;

                if (backToFollowImm)
                    BackToFollow(0);
            }
            else
            {
                FormerYAxis= FreeLook.m_YAxis.Value;
            }
            //brain.enabled = value;
        }
    }

    public bool SimpleFollowState
    {
        set
        {
            if (FreeLook != null)
                FreeLook.m_FollowFlag = value;
        }
    }

    private GameObject m_followObj;
    public GameObject Follow_Obj
    {
        set { m_followObj = value; }
        get { return m_followObj; }
    }

    public override void UnInit()
    {
        base.UnInit();
        //FormerYAxis = freeLook.m_YAxis.Value;
        //freeLook.m_YAxis.Value = _default_y;
    }
    public override void Init()
    {
        base.Init();

        //FreeLook.m_Transitions.m_InheritPosition = false;
        //InheritPosition = false;

        FreeLook.m_InputScriptControl = true;
        //FreeLook.MinYBasedOnFollow = LookAt.position.y * 0.4f;

        //if (originParam != null) SetParamHardly(ref originParam);//由于镜头衔接问题先关闭功能
        //if (battleParam != null && ForSpecialSetting == false) SetParamHardly(ref battleParam);//根据非战时状态设置参数
    }

    private float _rotate_speed;
    private float _start_rotation;
    private float _target_rotation;
    // private float _rotate_current_time;
    private float _rotate_total_time;
    [Header("InheritPosition")]
    public bool DisableInheritPosition = false;
    private bool InheritPosition = false;
    private bool backToFollowImm = false;

    public float screenYOffset = 0f;//由于lookat点偏移带来的参数调整

    public bool ForSpecialSetting = false;

    private bool resetYOnly = false;
    private static float FormerYAxis = -999;
    public void BackToFollow(float time)
    {
        //resetYOnly = resetY;
        if (FreeLook != null && HostCamera != null && Follow != null)
        {
            if (time <= 0)
            {
                //if (resetY)
                //{
                //    if(FormerYAxis!=-999)
                //        FreeLook.m_YAxis.Value = FormerYAxis;
                //    forceConcentrate = true;
                //    backToFollowImm = false;
                //}
                //else
                {
                    backToFollowImm = true;
                    float dis = Follow.localEulerAngles.y - FreeLook.State.RawOrientation.eulerAngles.y;
                    if (dis < -180) dis += 360;
                    else if (dis > 180) dis -= 360;
                    /// 临时解决射击关相机追背问题
                    if (FreeLook.m_BindingMode == CinemachineTransposer.BindingMode.LockToTargetOnAssign)
                        dis = 0;
                    FreeLook.m_XAxis.Value = dis;
                    FreeLook.m_YAxis.Value = _default_y;
                    _rotate_total_time = 0;
                    FreeLook.UpdateCameraState(Vector3.up, 0);
                    if (CinemachineCore.Instance.IsLive(FreeLook))
                    {
                        backToFollowImm = false;
                    }
                }
            }
            else
            {
                _start_rotation = FreeLook.State.RawOrientation.eulerAngles.y;
                _target_rotation = Follow.localEulerAngles.y;
                ///临时解决射击关相机追背问题
                if (FreeLook.m_BindingMode == CinemachineTransposer.BindingMode.LockToTargetOnAssign)
                    _target_rotation = 0;
                ///

                _rotate_speed = 0;
                _rotate_total_time = time * 0.5f;
                // _rotate_current_time = 0;

            }
        }
        else backToFollowImm = false;
    }
    private bool SetAsFormer = false;
    public void SetInheritPosition(bool resetY)
    {
        if (FreeLook != null && HostCamera != null && !DisableInheritPosition)
        {
            resetYOnly = resetY;
            SetAsFormer = true;
            FreeLook.m_Transitions.m_InheritPosition = true;
            InheritPosition = true;

            FreeLook.inherateY = resetY==false? FormerYAxis:-999;

            //float dis = Follow.localEulerAngles.y;
            //if (dis < -180) dis += 360;
            //else if (dis > 180) dis -= 360;
            //FreeLook.m_XAxis.Value = dis;

            //backToFollowImm = false;
        }
    }

    int chosen = -1;
    bool[] hits = new bool[4];
    float angle2forward = 0;
    private bool isFirstFrame = true;
    protected bool isFreelook = true;//freelook仅有的功能，用于区分freelook和他的继承

    protected override void Update()
    {
        base.Update();

        float deltaTime = Time.deltaTime * TimeScale;

        if (FreeLook != null)
        {
            if (backToFollowImm)
            {
                if (CinemachineCore.Instance.IsLive(FreeLook))
                {
                    BackToFollow(0);
                }
            }

            if (InheritPosition )
            {
                if (SetAsFormer)
                {
                    FreeLook.m_YAxis.Value = FormerYAxis;
                    SetAsFormer = false;
                }
                else if(resetYOnly)
                    forceConcentrate = true;
            }

            if (FreeLook.LookAt != null && isFreelook)
            {
                Vector3 forward = FreeLook.LookAt.forward.normalized;
                forward.y = 0;
                angle2forward = Vector3.SignedAngle(Vector3.forward, forward, Vector3.up);
                angle2forward += 180;


                if (cooldownTimer >= 0)
                {
                    cooldownTimer += deltaTime;
                    if (cooldownTimer > TURNING_COOLDOWN)
                        cooldownTimer = -1;
                }
                if (startRotating)
                {
                    UpdateRotation(deltaTime);
                }
                else
                {
                    if (isInBattleTurning && IsFighting && AttackRotation) //战斗转向    
                    {

                        if (HasAxisInput)
                            CloseBattleTurn();
                        else
                            BattleTurnUpdate(deltaTime);
                    }
                }


                UpdateConcentrateY(deltaTime);
                UpdateByBattle(deltaTime);
                UpdateParamForEntity(deltaTime);
            }

            //FreeLook.m_Transitions.m_InheritPosition = false;
            //InheritPosition = false;
        }
        //if (Data != null) CopyData(Data);
    }

    #region Input
    private bool hasAxisInput = true;
    public bool HasAxisInput { get => hasAxisInput; set => hasAxisInput = value; }
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

    [Header("TriggerAxis")]
    public float TriggerAxisX = 2;
    public float TriggerAxisY = 2;
    public float xInputAxisValue
    {
        set
        {
            if (IsBlending || value > 10000)
            {
                FreeLook.m_InputX = 0;
                AxisInputValid = false;
                FreeLook.m_ShouldResetSpeed = true;
                return;
            }
            FreeLook.m_InputX = Mathf.Abs(value) < TriggerAxisX ? 0 : (value - (value > 0 ? 1 : -1) * TriggerAxisX);
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
            if (IsBlending || value > 10000)
            {
                FreeLook.m_InputY = 0;
                AxisInputValid = false;
                FreeLook.m_ShouldResetSpeed = true;
                return;
            }

            if (value == -10000000)
                guion = !guion;
            else FreeLook.m_InputY = Mathf.Abs(value) < TriggerAxisY ? 0 : (value - (value > 0 ? 1 : -1) * TriggerAxisY);

            if (FreeLook.m_InputX == 0 && FreeLook.m_InputY == 0)
                FreeLook.m_FollowParam = followparamvalue;                   
            else
                FreeLook.m_FollowParam = 0.99f;
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

    bool guion = false;
    #endregion

    #region 镜头设置+赋值
    private float cameraDistance = 1;
    private float cameraHeight = 1;
    private bool attackRotation=true;
    private bool moveRotation=true;
    private bool autoResetY = true;
    public bool AttackRotation { get => attackRotation; set => attackRotation = value; }
    public bool MoveRotation { get => moveRotation;
        set
        {
            moveRotation = value;
            followparamvalue = !IsFighting && MoveRotation ? 0.5f : 0.99f;
        }
    }
    public bool AutoResetY { get => autoResetY; set => autoResetY=value; }

    public float CameraDistance
    {
        get => cameraDistance;
        set
        {
            cameraDistance = value;
            ChangeParam2Taregt();
        }
    }
    public float CameraHeight
    {
        get => cameraHeight;
        set
        {
            cameraHeight = value;
            ChangeParam2Taregt();
        }
    }
    #endregion

    #region 战斗状态
    //public void EnterBattleState(object isFighting)//战斗状态改变
    //{
    //    IsFighting = (bool)isFighting;
    //    if (IsFighting && MoveRotation)
    //        FreeLook.m_FollowParam = 0.9f;
    //    else
    //        FreeLook.m_FollowParam = 0.5f;

    //    if (IsFighting)
    //        mCamBattleState = CamBattleState.ENTER_BATTLE;
    //    else
    //        mCamBattleState = CamBattleState.LEAVE_BATTLE;
    //    _battleStateTimer = 0;
    //    return;//由于镜头衔接问题，这个版本先关闭功能

    //    //battleOn = true;
    //}

    private float[,] freelookparam = new float[,] {
        { 4, 2f,0.5f,3,3,2,0.5f,0.5f,0.5f },
        { 4, 2f,0.5f,5,5,4,0.5f,0.5f,0.5f },
        { 4, 2f,0.5f,6,6,4,0.5f,0.5f,0.5f }
    }; //格式：//orbit0.height,orbit1.height,orbit2.height,//orbit0.radius,orbit1.radius,orbit2.radius,//orbit0.screenY,orbit1.screenY,orbit2.screenY,                
    public void FreelookChange(int index)
    {
        //float[] param = new float[9];
        //for (int i = 0; i < 9; i++)
        //    param[i] = freelookparam[index, i];
        //SetParamHardly(ref param);
    }
    private float followparamvalue = 0.5f;
    public bool IsFighting
    {
        get => isFighting;
        set
        {
            if (IsFighting != value)
            {
                battlePercent = 0;
                battleTimer = 0;
                isFighting = value;
                ChangeParam2TargetSmoothly();
            }

            isFighting = value;
            //FreeLook.m_FollowParam = !IsFighting && MoveRotation?0f:0.9f ;
            followparamvalue = !IsFighting && MoveRotation ? 0.5f : 0.99f;

            //if (HostCamera != null && HostCamera.TryGetComponent<CameraAvoidBlock>(out var cab))
            //{
            //    cab.enabled = value;
            //}
            mCamBattleState = IsFighting ? CamBattleState.ENTER_BATTLE : CamBattleState.LEAVE_BATTLE;
            //_battleStateTimer = 0;
        }
    }
    private float battlePercent = -1;
    private float battleTimer = 0;
    private bool isFighting = false;
    private CamBattleState mCamBattleState = CamBattleState.NOT_BATTLE;
    private enum CamBattleState
    {
        NOT_BATTLE,
        ENTER_BATTLE,
        ON_BATTLE,
        LEAVE_BATTLE,
    }
    #endregion

    #region 回正Concentrate
    private float originY = -1;
    private float concenTimer = 0;
    private readonly float CONCENTRATE_TIME = 2;//回正时间
    private float concenWaitTimer = 0;
    private readonly float CONCENTRATE_WAIT_TIME = 2f;//回正等待时间
    private bool forceConcentrate = false;
    private void UpdateConcentrateY(float deltaTime)
    {
        if (forceConcentrate)
        {
            ConcentrateY(deltaTime);
            return;
        }

        if (!IsFighting
            && AutoResetY
            && EnableParamChange)//回正
        {
            if (HasAxisInput)
            {
                CloseConcentrate();
            }
            else
            {
                concenWaitTimer += deltaTime;
                if (concenWaitTimer > CONCENTRATE_WAIT_TIME)
                    ConcentrateY(deltaTime);
            }
        }
    }
    private void ConcentrateY(float deltaTime)
    {
        if (originY == -1)
            originY = yAxisValue;

        concenTimer += deltaTime;
        var percent = Mathf.Clamp01(concenTimer / CONCENTRATE_TIME);
        yAxisValue = originY * (1 - percent) + _default_y * percent;
        if (percent == 1)
            CloseConcentrate();
    }

    private void CloseConcentrate()
    {
        concenTimer = 0;
        concenWaitTimer = 0;
        originY = -1;
        forceConcentrate = false;

    }
    #endregion

    #region  BattleTurn
    bool isInBattleTurning = false;
    float turningTimer = 0f;

    [Header("BattleTurning")]
    public float detectDistance = 3;
    public float TURNING_COOLDOWN = 0;
    private float cooldownTimer = -1;
    private float ROT_DELAY_TIME = 0.001f;
    private float delayTimer = 0;
    private int turningDataIndex;
    [Tooltip("旋转角度取值范围:[-180,180]")]
    public TurningData[] TurningDatas = new TurningData[] {
        new TurningData(new float[] { 45, -45 }, new float[] {45,135,-135,-45 },5,0.5f,0.3f),
    };

    [Serializable]
    public class TurningData {
        public float[] targetAngles;
        public float[] exclusiveAngles;
        public float maxRange;
        public float rotTime = 0.5f;
        [Range(0.001f, 1)] public float rotAcceleration = 0.3f;
        public TurningData(float[] targetAngles, float[] exclusiveAngles, float maxRange, float rotTime, float rotAcceleration)
        {
            this.targetAngles = targetAngles;
            this.exclusiveAngles = exclusiveAngles;
            this.maxRange = maxRange;
            this.rotTime = rotTime;
            this.rotAcceleration = rotAcceleration;
        }
    }

    private Vector3 battleTurningDirection = Vector3.zero;
    public void BattleTurn(bool isOpen,Vector3 dir)
    {
        if (!EnableParamChange)
            return;

        if (0 < cooldownTimer && cooldownTimer < TURNING_COOLDOWN) return;
        else cooldownTimer = 0;

        isInBattleTurning = isOpen;
        chosen = -1;
        turningTimer = 0;
        delayTimer = 0;
        isFirstFrame = true;

        HasAxisInput = true;
        concenTimer = 0;
        concenWaitTimer = 0;
        forceConcentrate = false;
        originY = -1;

        turningDataIndex = -1;
        var dis = Vector3.Magnitude(dir);
        if (dis < 0.1f || !isOpen)
        {
            battleTurningDirection = Vector3.zero;
            return;
        }
        for (int i = 0; i < TurningDatas.Length; i++)
        {
            if (dis < TurningDatas[i].maxRange)
            {
                turningDataIndex = i;
                battleTurningDirection = dir;
                break;
            }            
        }
    }
    private void BattleTurnUpdate(float deltaTime)
    {
        if (delayTimer < ROT_DELAY_TIME)
        {
            delayTimer += deltaTime;
            return;
        }
        //没有正确的旋转
        if (turningDataIndex < 0 || turningDataIndex >= TurningDatas.Length) return;


        TurningData data = TurningDatas[turningDataIndex];

        if (battleTurningDirection == Vector3.zero)
        {
            CloseBattleTurn();
            return;
        }

        if (turningTimer < data.rotTime)
        {
            turningTimer += deltaTime;

            Vector3 tarDir = battleTurningDirection;
            tarDir.y = 0;tarDir.Normalize();

            var camDir = FreeLook.LookAt.position - transform.position;

            var camDirXZ = camDir;
            camDirXZ.y = 0;

            var angle2tar = Vector3.SignedAngle(tarDir, camDirXZ, Vector3.up);

            if (chosen == -1)
            {
                if (data.exclusiveAngles.Length != 0 && data.exclusiveAngles.Length % 2 == 0)
                {
                    for (int i = 0; i <= data.exclusiveAngles.Length / 2; i += 2)
                    {
                        if (data.exclusiveAngles[i] < angle2tar && angle2tar < data.exclusiveAngles[i + 1])
                            return;//不旋转的情况
                    }
                }

                float closest = float.MaxValue;
                float offset;
                for (int i = 0; i < data.targetAngles.Length; i++)
                {
                    offset = Mathf.Abs(angle2tar - data.targetAngles[i] );
                    if (offset > 180)
                        offset = 360 - offset;

                    hits[i] = Physics.Raycast(
                        new Ray(FreeLook.LookAt.position, new Vector3(Mathf.Sin((data.targetAngles[i] + angle2forward) * Mathf.Deg2Rad),
                        0, Mathf.Cos((data.targetAngles[i] + angle2forward) * Mathf.Deg2Rad))), detectDistance, (1 << 9) | (1 << 25));

                    if (offset < closest && !hits[i])
                    {
                        closest = offset;
                        chosen = i;
                    }
                }
            }
            if (chosen != -1)
            {

                float xResult = data.targetAngles[chosen] - angle2tar;
                if (Mathf.Abs(xResult) > 180)
                    xResult = angle2tar > 0 ? (xResult + 360) : (xResult - 360);

                if (Mathf.Abs(xResult) < 1f)
                    CloseBattleTurn();//跳出

                var result = xResult * data.rotAcceleration;
                if (Mathf.Abs(result) < 0.1f)
                    result = result < 0 ? -0.1f : 0.1f;
                
                FreeLook.m_XAxis.Value = result;
            }
        }
        else
            CloseBattleTurn();
    }
    private void CloseBattleTurn()
    {
        isInBattleTurning = false;
        turningTimer = 0;
        delayTimer = 0;
        chosen = -1;
        isFirstFrame = true;
        battleTurningDirection = Vector3.zero;
    }
    #endregion

    #region BattleChange

    private void UpdateByBattle(float deltaTime)
    {
        if (battlePercent < 0) return;
        battleTimer += deltaTime;
        battlePercent = Mathf.Clamp01(battleTimer/ ENTITY_CHANGE_TIME);
        if (battleTimer > ENTITY_CHANGE_TIME) battlePercent = -1;
    }

    private void SetParamByPercent(float[] from, float[] to, float percent)
    {
        int count = Mathf.Min(from.Length, to.Length);
        float[] result = new float[count];
        for (int i = 0; i < count; ++i)
            result[i] = Mathf.Lerp(from[i], to[i], percent);
        SetParamHardly(ref result);
    }
    #endregion

    #region damping
    protected override float Damping
    {
        set
        {
            CinemachineOrbitalTransposer cinemachineOrbitalTransposer;
            for (int i = 0; i < 3; i++)
            {
                cinemachineOrbitalTransposer = FreeLook.GetRig(i).GetCinemachineComponent<CinemachineOrbitalTransposer>();
                cinemachineOrbitalTransposer.m_XDamping = value;
                cinemachineOrbitalTransposer.m_YDamping = value;
                cinemachineOrbitalTransposer.m_ZDamping = value;
            }
        }
    }
    #endregion

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
    #region rotation
    private bool startRotating = false;
    private Transform selectTarget;//镜头旋转的参照点
    private float[] targetAngles;
    private float rotTime;
    private float rotAcceleration;
    private float rotatingTime = 0;
    public void StartRotate(Transform target, float[] targetAngles, float rotTime, float rotAcceleration)
    {
        this.selectTarget = target;
        this.targetAngles = targetAngles;
        this.rotTime = rotTime;
        this.rotAcceleration = rotAcceleration;
        if (target != null && targetAngles != null && targetAngles.Length > 0)
        {
            startRotating = true;
            rotatingTime = 0;
        }
    }

    private void UpdateRotation(float deltaTime)
    {
        if (rotTime > rotatingTime)
        {
            rotatingTime += deltaTime;
            Vector3 p2tV3 = selectTarget.position - FreeLook.LookAt.position;
            Vector2 p2tV2 = new Vector2(p2tV3.x, p2tV3.z).normalized;//player to target
            Vector3 c2pV3 = FreeLook.LookAt.position - transform.position;
            Vector2 c2pV2 = new Vector2(c2pV3.x, c2pV3.z).normalized;//camera to player
            float angle = -Vector2.SignedAngle(p2tV2, c2pV2);
            int targetAnglesIndex = 0;
            for (int i = 1; i < targetAngles.Length; ++i)
            {
                if (Mathf.Abs(targetAngles[i] - angle) < Mathf.Abs(targetAngles[targetAnglesIndex] - angle))
                {
                    targetAnglesIndex = i;
                }
            }
            float xResult = targetAngles[targetAnglesIndex] - angle;
            if (Mathf.Abs(xResult) < 1f)
            {
                startRotating = false;
                FreeLook.m_XAxis.Value = xResult;
                return;
            }
            float result = xResult * rotAcceleration;
            FreeLook.m_XAxis.Value = result;
        }
        else
        {
            startRotating = false;
        }
    }

    #endregion
    #region TableParamData    
    public override void SetTableParamData(float[] param)
    {
        if (freeLook == null) return;
        if (IgnoreTableData || param == null || param.Length == 0)
        {
            ResetScale();
        }
        else
        {
            SetScaleByParam(param);
        }
    }
    private void ResetScale()
    {
        for (int i = 0; i < 3; ++i)
            FreeLook.m_Orbits[i].Scale = 1;
    }
    private void SetScaleByParam(float[] param)
    {
        for (int i = 0; i < 3; ++i)
            FreeLook.m_Orbits[i].Scale = param[0];
    }
    public void SetScreenY(float screenY)
    {

    }

    /// <summary>
    /// 设置相机的相关的参数
    /// </summary>
    /// <param name="param"></param>
    private void SetParamHardly(ref float[] param)
    {
        for (int i = 0; i < 3; ++i)
        {
            FreeLook.m_Orbits[i].Scale = 1;
            FreeLook.m_Orbits[i].m_Height = param[i] * cameraHeight;
            FreeLook.m_Orbits[i].m_Radius = param[i + 3] * CameraDistance;
            FreeLook.GetRig(i).GetCinemachineComponent<CinemachineComposer>().ScreenY = param[i + 6] + screenYOffset;
        }
        if (param.Length > 9) FieldOfView = param[9];
    }
    /// <summary>
    /// 镜头参数 和 参数系数计算后才是真实的参数 
    /// 利用真实的参数计算出原本的参数
    /// </summary>
    /// <param name="param"></param>
    /// <returns></returns>
    public float[] DecodeParam(float[] param)
    {
        for (int i = 0; i < 3; ++i)
        {
            param[i] /=  cameraHeight;
            param[i + 3] /= CameraDistance;
            param[i + 6] -= screenYOffset;
        }
        return param;
    }
    private float entityChangeTimer = 0;
    private float[] before;
    private float[] after;
    [Header("ParamChangeTime")]
    public float ENTITY_CHANGE_TIME = 0.75f;
    [Header("PVE_Function")]
    public bool EnableParamChange = true;
    /// <summary>
    /// 变更相机的参数 瞬切
    /// </summary>
    public void ChangeParam2Taregt()
    {
        float[] target = TargetParam();
        if (!EnableParamChange || target == null) 
            return;
        SetParamHardly(ref target);
    }

    /// <summary>
    /// 平滑切换相机的参数
    /// </summary>
    public void ChangeParam2TargetSmoothly()
    {
        float[] target = TargetParam();
        if (!EnableParamChange || target == null)
            return;
        entityChangeTimer = 0;
        before = DecodeParam(GetCurrentParam());
        after = target;
    }
    public float[] GetCurrentParam()
    {
        float[] result = new float[9];
        for (int i = 0; i < 3; ++i)
        {
            result[i] = FreeLook.m_Orbits[i].m_Height;
            result[i + 3] = FreeLook.m_Orbits[i].m_Radius;
            result[i + 6] = FreeLook.GetRig(i).GetCinemachineComponent<CinemachineComposer>().ScreenY;
        }
        return result;
    }

    /// <summary>
    /// freelook相机在战斗状态切换是高度变化系数
    /// </summary>
    private float battleHeightOffParam = 0f;   // Temporarily set to 0, origin is 0.5

    /// <summary>
    /// freelook相机在战斗状态切换是半径变化系数
    /// </summary>
    private float battleRadiusOffParam = 0f;    // Temporarily set to 0, origin is 1
    override protected float[] TargetParam()
    {
        if (targetParam != null)
        {
            float battleHeightOffset = IsFighting ? battleHeightOffParam : 0;
            float battleRadiusOffset = IsFighting ? battleRadiusOffParam : 0;
            float[] temp = new float[targetParam.Length];
            for (int i = 0; i < temp.Length; i++)
                temp[i] = targetParam[i];
            for (int i = 0; i < 3; i++)
                temp[i] += battleHeightOffset;
            for (int i = 3; i < 6; i++)
                temp[i] += battleRadiusOffset;
            return temp;
        }
        return null;
    }

    private void UpdateParamForEntity(float deltaTime)
    {
        if (before == null || after == null) return;        
        entityChangeTimer += deltaTime;
        float percent = Mathf.Clamp01(entityChangeTimer / ENTITY_CHANGE_TIME);
        SetParamByPercent(before, after, percent);
        if (percent >= 1)
        {
            entityChangeTimer = 0;
            before = null;
            after = null;
        }
    }
    #endregion
    void OnGUI()
    {
        if (guion)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.white;
            style.fontSize = 30;

            GUI.color = Color.red;
            GUI.Label(new Rect(Screen.width - 800, 50, 80, 100), new GUIContent("X_Speed:" + FreeLook.m_XAxis.m_MaxSpeed), style);
            FreeLook.m_XAxis.m_MaxSpeed = GUI.HorizontalSlider(new Rect(Screen.width - 600, 60, 550, 100), FreeLook.m_XAxis.m_MaxSpeed, 0, 0.3f);

            GUI.Label(new Rect(Screen.width - 800, 100, 80, 100), new GUIContent("Y_Speed:" + FreeLook.m_YAxis.m_MaxSpeed ), style);
            FreeLook.m_YAxis.m_MaxSpeed = GUI.HorizontalSlider(new Rect(Screen.width - 600, 110, 550, 100), FreeLook.m_YAxis.m_MaxSpeed, 0, 0.01f);

            GUI.Label(new Rect(Screen.width - 800, 150, 80, 100), new GUIContent("X_Trigger:" + ((int)(TriggerAxisX * 10)) / 10.0f), style);
            TriggerAxisX = GUI.HorizontalSlider(new Rect(Screen.width - 600, 160, 550, 100), TriggerAxisX, 0, 10);

            GUI.Label(new Rect(Screen.width - 800, 200, 80, 100), new GUIContent("Y_Trigger:" + ((int)(TriggerAxisY * 10)) / 10.0f), style);
            TriggerAxisY = GUI.HorizontalSlider(new Rect(Screen.width - 600, 210, 550, 100), TriggerAxisY, 0, 10);

            GUI.Label(new Rect(Screen.width - 800, 250, 80, 100), new GUIContent("X_Accel:" + freeLook.m_XAxis.m_AccelTime), style);
            freeLook.m_XAxis.m_AccelTime = GUI.HorizontalSlider(new Rect(Screen.width - 600, 260, 550, 100), freeLook.m_XAxis.m_AccelTime, 0, 3);

            GUI.Label(new Rect(Screen.width - 800, 300, 80, 100), new GUIContent("X_Decel:" + freeLook.m_XAxis.m_DecelTime), style);
            freeLook.m_XAxis.m_DecelTime = GUI.HorizontalSlider(new Rect(Screen.width - 600, 310, 550, 100), freeLook.m_XAxis.m_DecelTime, 0, 3);

            GUI.Label(new Rect(Screen.width - 800, 350, 80, 100), new GUIContent("Y_Accel:" + freeLook.m_YAxis.m_AccelTime), style);
            freeLook.m_YAxis.m_AccelTime = GUI.HorizontalSlider(new Rect(Screen.width - 600, 360, 550, 100), freeLook.m_YAxis.m_AccelTime, 0, 3);

            GUI.Label(new Rect(Screen.width - 800, 400, 80, 100), new GUIContent("Y_Decel:" + freeLook.m_YAxis.m_DecelTime), style);
            freeLook.m_YAxis.m_DecelTime = GUI.HorizontalSlider(new Rect(Screen.width - 600, 410, 550, 100), freeLook.m_YAxis.m_DecelTime, 0, 3);

            //GUI.Label(new Rect(Screen.width - 800, 450, 80, 100), new GUIContent("BattleTime:" + BATTLE_TIME), style);
            //BATTLE_TIME = GUI.HorizontalSlider(new Rect(Screen.width - 600, 460, 550, 100), BATTLE_TIME, 0, 5);
            //GUI.Label(new Rect(Screen.width - 800, 500, 80, 100), new GUIContent("RotAcceleration:" + rotAcceleration), style);
            //rotAcceleration = GUI.HorizontalSlider(new Rect(Screen.width - 600, 510, 550, 100), rotAcceleration, 0.01f, 1);
        }
    }
    private void OnDrawGizmos()
    {
        if (FreeLook == null || FreeLook.LookAt == null)
            return;

        var playerPos = freeLook.LookAt.position;
        float minRange = 0;
        for (int i = 0; i < TurningDatas.Length; i++)
        {
            float maxRange = TurningDatas[i].maxRange;
            for (int j = 0; j < TurningDatas[i].targetAngles.Length; j++)
            {
                var degree = (TurningDatas[i].targetAngles[j] + angle2forward) * Mathf.Deg2Rad;
                var dir = new Vector3(Mathf.Sin(degree), 0, Mathf.Cos(degree));
                Gizmos.DrawRay(playerPos + dir * minRange, dir * maxRange);
            }

            if (TurningDatas[i].exclusiveAngles.Length > 0 && TurningDatas[i].exclusiveAngles.Length % 2 == 0)
            {
                float length = maxRange - minRange;
                for (int j = 0; j <= TurningDatas[i].exclusiveAngles.Length / 2; j += 2)
                {
                    Gizmos.color = Color.yellow;
                    DrawArea(playerPos, minRange, length, TurningDatas[i].exclusiveAngles[j], TurningDatas[i].exclusiveAngles[j + 1]);
                }
            }
            minRange = maxRange;
        }

        //if (battleTurningDirection == Vector3.zero)
        //{
        //    Gizmos.color = Color.blue;
        //    Gizmos.DrawRay(playerPos + Vector3.up, FreeLook.LookAt.forward);
        //}
        //else
        //{
        //    Gizmos.color = Color.red;
        //    Gizmos.DrawRay(playerPos + Vector3.up, battleTurningDirection * 3);
        //}
    }
    private void DrawArea(Vector3 pos,float minRange,float length,float angleStart,float angleEnd)
    {        
        for (float i = angleStart; i <= angleEnd; i+=1f)
        {
            var degree = (i + angle2forward )* Mathf.Deg2Rad ;
            var dir = new Vector3(Mathf.Sin(degree), 0, Mathf.Cos(degree));
            Gizmos.DrawRay(pos + dir * minRange,dir*length);
        }
    }
}
