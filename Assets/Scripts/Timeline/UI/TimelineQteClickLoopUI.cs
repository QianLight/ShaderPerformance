using CFClient.Utility;
using CFUtilPoolLib;
using UnityEngine;
using UnityEngine.CFEventSystems;
using UnityEngine.CFUI;

public class TimelineQteClickLoopUI : TimelineBaseUI<TimelineQteClickLoopUI, QteClickSignal>
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
    private Canvas m_canvas;

    //protected override string prefab { get { return "InteractiveClickBtn_0_new"; } }

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
            else prefab = "InteractiveClickBtn_0_new";
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
        m_timeFx = go.transform.Find("TimeBar/fx");
        m_timeFx.gameObject.SetActive(false);
        m_succeedFx = go.transform.Find("FX");

        string iconName = m_signal.m_iconName;
        if (string.IsNullOrEmpty(iconName)) iconName = "ui_cutscene_qte_btn_0";
        m_btnIcon.Bind(iconName, "ui_cutscene");

        m_timeFx.localEulerAngles = new Vector3(0, 0, 360);
        m_timeBar.gameObject.SetActive(true);
        m_bar.fillAmount = 1;

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
        AutoTestingInterface.CallFunction(AutoTestingInterface.QteClickLoop);
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
    }

    private void OnBtnPressUp(UIBehaviour btn)
    {
        m_btn.transform.localScale = Vector3.one;
    }

    private void OnBtnClick(UIBehaviour btn)
    {
        m_fmod.StartEvent(m_signal.m_clickEvent, CFUtilPoolLib.AudioChannel.Action, true);
        m_fmod.StartEvent(m_signal.m_succeedEvent, CFUtilPoolLib.AudioChannel.Motion, true);
        if (m_state != QTEState.SucceedCountdown)
        {
            m_countdownStartTime = Time.realtimeSinceStartup;
            RTimeline.singleton.SetSpeed(1);
            ShowSucceed();
            SwitchState(QTEState.SucceedCountdown);
        }
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
    }

    private void StartCountdown()
    {
        float gap = Time.realtimeSinceStartup - m_countdownStartTime;
        if (gap >= m_signal.m_signalDuration)
        {
            RTimeline.singleton.JumpToCertainTime((float)m_signal.time); //跳转到信号开始的时间
            m_countdownStartTime = Time.realtimeSinceStartup; //重新开始倒计时
        }
    }

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
        m_succeedFx.gameObject.SetActive(false);
    }

    private void ShowSucceed()
    {
        m_hint.gameObject.SetActive(false);
        m_timeBar.gameObject.SetActive(false);
        m_succeedFx.gameObject.SetActive(true);
    }

    private void HideUIExcludeSuccedFx()
    {
        m_hint.gameObject.SetActive(false);
        m_btn.gameObject.SetActive(false);
        m_timeBar.gameObject.SetActive(false);
        m_miss.gameObject.SetActive(false);
    }

    private void HideAll()
    {
        this.go.SetActive(false);
    }
}