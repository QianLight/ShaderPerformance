using CFClient.Utility;
using CFUtilPoolLib;
using UnityEngine;
using UnityEngine.CFEventSystems;
using UnityEngine.CFUI;

public class TimelineQteContinueClickUI : TimelineBaseUI<TimelineQteContinueClickUI, QteClickSignal>
{
    private enum QTEState
    {
        StartCountdown,
        MissCountdown,
        SucceedCountdown,
        BlackCountdown,
        Finish,
    }

    private Transform m_hint;
    private CFButton m_btn;
    private CFImage m_btnIcon;
    private Transform m_miss;
    private Transform m_timeBar;
    private CFImage m_bar;
    private Transform m_timeFx;
    private Transform m_succeedFx;

    protected QteClickSignal m_signal;
    private float m_countdownStartTime;
    private bool m_jump = false;

    private QTEState m_state;
    private XRuntimeFmod m_fmod;

    private int m_clickCount = 0;
    private float m_percent = 0;
    private Transform m_press;
    private CFImage m_pressBar;
    private RectTransform m_fx;
    private RectTransform m_fx2;
    private TimelineQteRotateFollow m_rotate2;
    private Transform m_qteClickFx;
    private Canvas m_canvas;

    //protected override string prefab { get { return "InteractivePressBtn_1_new"; } }

    protected override string Dir
    {
        get
        {
            return "UI/OPsystemprefab/cutscene/";
        }
    }

    public override void Show(QteClickSignal arg)
    {
        if (arg != null)
        {
            if (!string.IsNullOrEmpty(arg.m_prefabName)) prefab = arg.m_prefabName; //如果信号的m_prefabName不为空，则使用策划配置的prefab，否则使用默认的prefab
            else prefab = "InteractivePressBtn_1_new";
        }
        base.Show(arg);
        m_canvas = go.GetComponent<Canvas>();
        ControlLayer();
        m_signal = arg;

        m_hint = go.transform.Find("T");
        m_btn = CFButton.Get<CFButton>(go, "Btn");
        m_btnIcon = CFImage.Get<CFImage>(go, "Btn");
        m_miss = go.transform.Find("Miss");
        m_timeBar = go.transform.Find("TimeBar");
        m_bar = CFImage.Get<CFImage>(go, "TimeBar/Bar");

        m_press = go.transform.Find("Press");
        m_pressBar = CFImage.Get<CFImage>(go, "Press/Bar");
        m_fx = go.transform.Find("Press/fx") as RectTransform;
        m_fx2 = go.transform.Find("Press/fx2") as RectTransform;
        m_timeFx = go.transform.Find("TimeBar/fx");
        m_succeedFx = go.transform.Find("FX");
        m_qteClickFx = go.transform.Find("FX01");
        m_qteClickFx.gameObject.SetActive(false);

        string iconName = m_signal.m_iconName;
        if (string.IsNullOrEmpty(iconName)) iconName = "ui_cutscene_qte_btn_0";
        m_btnIcon.Bind(iconName, "ui_cutscene");

        m_timeFx.localEulerAngles = new Vector3(0, 0, 360);

        m_rotate2 = m_fx.GetComponent<TimelineQteRotateFollow>();
        m_rotate2.StopRotate();
        m_rotate2.SetAngle(m_rotate2.m_startAngle); //180->90

        m_timeBar.gameObject.SetActive(true);
        m_bar.fillAmount = 1;
        m_pressBar.fillAmount = 0;
        m_clickCount = 0;
        m_percent = 0;
        m_btn.RegisterPointerClickEvent(OnBtnClick);
        m_btn.RegisterPointerDownEvent(OnBtnPressDown);
        m_btn.RegisterPointerUpEvent(OnBtnPressUp);

        var rect = go.transform.GetComponent<RectTransform>();
        rect.anchoredPosition = arg.m_position;

        m_countdownStartTime = Time.realtimeSinceStartup;
        RTimeline.singleton.SetSpeed(m_signal.m_speed); //设置速度

        m_jump = false;
        if (m_fmod == null) m_fmod = XRuntimeFmod.GetFMOD();
        m_fmod.StartEvent("event:/UI/QTE/Appear", CFUtilPoolLib.AudioChannel.Action, true);

        ShowNormal();
        SwitchState(QTEState.StartCountdown);

        //自动化测试
#if AUTOTESTING
        int count = 1;
        if(m_signal.m_addPercent > 0)
        {
            count = Mathf.CeilToInt(1.0f / m_signal.m_addPercent);
        }
        AutoTestingInterface.CallFunction(AutoTestingInterface.QteContinueClick, count.ToString());
#endif
    }

    protected override void ControlLayer()
    {
        base.ControlLayer();
        if (m_canvas != null)
        {
            int layer = int.Parse(RTimeline.singleton.GetTimelineSettingByID(TimelineSettingID.QTE_Layer).Param);
            m_canvas.sortingOrder = layer;
        }
    }
    private void OnBtnPressDown(UIBehaviour btn)
    {
        m_btn.transform.localScale = Vector3.one * 0.9f;
        m_qteClickFx.gameObject.SetActive(true);
    }

    private void OnBtnPressUp(UIBehaviour btn)
    {
        m_btn.transform.localScale = Vector3.one;
        m_qteClickFx.gameObject.SetActive(false);
    }

    /// <summary>
    /// 每次点击，累加1，达到配置的总数时，表示成功。
    /// 成功之后，进入成功倒计时，以显示成功特效和暂留一部分UI(一定时长)
    /// </summary>
    /// <param name="btn"></param>
    private void OnBtnClick(UIBehaviour btn)
    {
        m_clickCount++;
        //if (m_signal.m_clickCount == 0) m_clickCount = 1;
        //float percent = m_clickCount * 1.0f / m_signal.m_clickCount;
        m_percent += m_signal.m_addPercent;
        if (m_percent > 1) m_percent = 1;
        SetPercent();

        if (m_clickCount == 1)
        {
            m_hint.gameObject.SetActive(false); //第一次点击完，连点提示文字隐藏
            m_press.gameObject.SetActive(true); //第一次点击完，蓄能条显示
        }

        if (m_percent >= 1.0f /*m_clickCount == m_signal.m_clickCount*/) //成功了，则最后一次播放成功音效
        {
            m_fmod.StartEvent(m_signal.m_clickEvent, CFUtilPoolLib.AudioChannel.Action, true);
            m_fmod.StartEvent(m_signal.m_succeedEvent, CFUtilPoolLib.AudioChannel.Motion, true);
            m_countdownStartTime = Time.realtimeSinceStartup;
            RTimeline.singleton.SetSpeed(1);
            ShowSucced();
            SwitchState(QTEState.SucceedCountdown);
        }
        else
        {
            m_fmod.StartEvent(m_signal.m_clickEvent, CFUtilPoolLib.AudioChannel.Action, true); //播放单击音效
        }
    }

    private void SetPercent()
    {
        m_pressBar.fillAmount = m_percent;
        float angle = m_rotate2.m_startAngle - (m_rotate2.m_startAngle - m_rotate2.m_endAngle) * m_percent; // 180->90 
        m_rotate2.SetAngle(angle);
    }

    public void StopEvent()
    {
        if (m_fmod != null)
        {
            m_fmod.Stop(AudioChannel.Action);
            m_fmod.Stop(AudioChannel.Motion);
        }
    }

    public override void Update(float time)
    {
        base.Update(time);
        if (go == null) return;

        if (m_state == QTEState.StartCountdown)
        {
            StartCountdown();
        }
        else if (m_state == QTEState.SucceedCountdown)
        {
            SucceedCountdown();
        }
        else if (m_state == QTEState.MissCountdown)
        {
            MissCountdown();
        }
        else if (m_state == QTEState.BlackCountdown)
        {
            BlackCountdown();
        }
    }

    /// <summary>
    /// 出现QTE之时开始倒计时，倒计时结束，分两种状态：
    /// 勾选了失败情况，开始miss状态倒计时，此时间段将显示miss图标，
    /// 未勾选失败，则表明直接进入完成状态，此时timeline的速度恢复为正常速度1。
    /// </summary>
    private void StartCountdown()
    {
        float gap = Time.realtimeSinceStartup - m_countdownStartTime;
        float percent = gap * 1.0f / m_signal.m_signalDuration;
        if (percent > 1) percent = 1;
        float angle = percent * 360;
        m_timeFx.localEulerAngles = new Vector3(0, 0, 360 - angle);
        percent = 1 - percent;
        m_bar.fillAmount = percent;
        if (gap >= m_signal.m_signalDuration)
        {
            if (m_signal.m_missJump) //勾选了失败，先暂停，然后播放黑幕
            {
                ShowMiss();
                SwitchState(QTEState.MissCountdown);
                RTimeline.singleton.QTEMissCallback(); //miss++
            }
            else
            {
                RTimeline.singleton.SetSpeed(1);
                HideAll();
                SwitchState(QTEState.Finish);
            }
        }

        //做下衰减的倒计时
        m_percent -= m_signal.m_decayPercent * Time.deltaTime;
        if (m_percent < 0) m_percent = 0;
        SetPercent();
    }

    /// <summary>
    /// 在达到成功之后，要留一点时间m_fullStateDuration，用了显示UI，大于这个时间之后，要隐藏UI。
    /// 在时间达到m_succeedDuration之后，则进入完成状态，此时恢复速度为1，隐藏所有UI。
    /// </summary>
    private void SucceedCountdown()
    {
        float gap = Time.realtimeSinceStartup - m_countdownStartTime;
        if (gap >= m_signal.m_fullStateDuration)
        {
            HideUIExcludeSuccedFx();
        }
        if (gap >= m_signal.m_succeedDuration)
        {
            HideAll();
            SwitchState(QTEState.Finish);
        }
    }

    /// <summary>
    /// miss之后，开始黑屏状态
    /// </summary>
    private void MissCountdown()
    {
        float gap = Time.realtimeSinceStartup - m_countdownStartTime;
        if (gap >= m_signal.m_missDuration)
        {
            float[] times = new float[3];
            times[2] = m_signal.m_keepBlackDuration;        //黑屏持续时间
            times[1] = m_signal.m_whiteToBlackDuration;     //白到黑时间
            times[0] = m_signal.m_BlackToWhiteDuration;     //黑到白时间
            RTimeline.singleton.ControlBlack(times);
            m_countdownStartTime = Time.realtimeSinceStartup;
            SwitchState(QTEState.BlackCountdown);
        }
    }

    /// <summary>
    /// 屏幕从白-》全黑的时候，再去跳转到指定的时间，以防穿帮
    /// 屏幕可能会持续黑指定时间，在m_keepBlackDuration配置
    /// 屏幕从全黑到全白的之后，在恢复正常的速度1，进而达到完成状态。
    /// </summary>
    private void BlackCountdown()
    {
        float gap = Time.realtimeSinceStartup - m_countdownStartTime;
        if (gap >= m_signal.m_whiteToBlackDuration && !m_jump)
        {
            RTimeline.singleton.JumpToCertainTime(m_signal.m_jumpTime); //全黑的时候跳转到指定时间
            m_jump = true;
        }
        if (gap >= m_signal.BlackTotalDuration)
        {
            RTimeline.singleton.SetSpeed(1);
            SwitchState(QTEState.Finish);
            if (m_signal.m_missJump && !string.IsNullOrEmpty(m_signal.m_failEvent)) //失败了，并且勾选了失败跳转，
            {
                m_fmod.StartEvent(m_signal.m_failEvent, CFUtilPoolLib.AudioChannel.Motion, true);
            }
        }
    }

    private void SwitchState(QTEState state)
    {
        m_state = state;
    }

    private void ShowNormal()
    {
        this.go.SetActive(true);
        m_hint.gameObject.SetActive(true);
        m_btn.gameObject.SetActive(true);
        m_timeBar.gameObject.SetActive(true);
        m_miss.gameObject.SetActive(false);
        m_press.gameObject.SetActive(false);
        m_fx2.gameObject.SetActive(false);
        m_succeedFx.gameObject.SetActive(false);
    }

    private void ShowMiss()
    {
        m_hint.gameObject.SetActive(false);
        m_btn.gameObject.SetActive(false);
        m_timeBar.gameObject.SetActive(false);
        m_miss.gameObject.SetActive(true);
        m_press.gameObject.SetActive(false);
        m_fx2.gameObject.SetActive(false);

        m_countdownStartTime = Time.realtimeSinceStartup;
        RTimeline.singleton.SetSpeed(0);
        m_fmod.StartEvent("event:/UI/QTE/Fail", CFUtilPoolLib.AudioChannel.Action, true);
        if (m_signal.m_missJump && !string.IsNullOrEmpty(m_signal.m_failEvent)) //失败了，并且勾选了失败跳转，则再播放此音效
        {
            m_fmod.StartEvent(m_signal.m_failEvent, CFUtilPoolLib.AudioChannel.Motion, true);
        }
    }

    private void ShowSucced()
    {
        m_fx2.gameObject.SetActive(true);
        m_succeedFx.gameObject.SetActive(true);
    }

    private void HideUIExcludeSuccedFx()
    {
        m_hint.gameObject.SetActive(false);
        m_btn.gameObject.SetActive(false);
        m_timeBar.gameObject.SetActive(false);
        m_miss.gameObject.SetActive(false);
        m_press.gameObject.SetActive(false);
    }

    private void HideAll()
    {
        this.go.SetActive(false);
    }
}