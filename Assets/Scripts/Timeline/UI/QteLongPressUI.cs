//using UnityEngine;
//using UnityEngine.CFEventSystems;
//using UnityEngine.CFUI;

//public class QteLongPressUI : QteBaseUI
//{

//    enum State
//    {
//        PREPARE,
//        PRESS,
//        REVERT,
//        REUSE,
//        FINISH,
//        TIMEOUT,
//        FAILED
//    };

//    private QteLongPressSignal SIG;
//    private CFButton btn;
//    private float dTime;
//    private State dState;

//    private CFImage wtSlider;
//    private CFImage wtSliderHander;
//    private CFImage xlSlider;
//    private CFImage xlSliderHander;
//    private Transform xlFull;
//    private CFImage xlFailed, bg;
//    private Transform FX1, PerfectFX;
//    private bool locked, lockpress;

//    protected override bool isUpdate => true;
    
//    protected override void OnInitial()
//    {
//        base.OnInitial();
//        if (btn == null)
//        {
//            bg = CFImage.Get<CFImage>(m_tf, "bg");
//            btn = CFButton.Get<CFButton>(m_tf);
//            btn.RegisterPointerDownEvent(OnBtnDown);
//            btn.RegisterPointerUpEvent(OnBtnUp);
//        }
//        if (wtSlider == null)
//        {
//            wtSlider = CFImage.Get<CFImage>(m_tf, "CD/CD");
//            wtSliderHander = CFImage.Get<CFImage>(m_tf, "CD/CD/fx");
//        }
//        if (xlSlider == null)
//        {
//            xlSlider = CFImage.Get<CFImage>(m_tf, "Xuli/P");
//            xlSliderHander = CFImage.Get<CFImage>(m_tf, "Xuli/P/fx");
//            xlFull = m_tf.Find("Xuli/Full");
//            xlFailed = CFImage.Get<CFImage>(m_tf, "Miss");
//            xlFailed.gameObject.SetActive(false);
//        }
//        if (FX1 == null)
//        {
//            FX1 = m_tf.Find("FX1");
//            PerfectFX = m_tf.Find("PerfectFx");
//            FX1.gameObject.SetActive(false);
//            PerfectFX.gameObject.SetActive(false);
//        }
//        ShowSelf(true);
//        locked = false;
//        lockpress = false;
//        SIG = m_signal as QteLongPressSignal;
//        SetState(State.PREPARE);
//    }


//    protected void OnBtnDown(UIBehaviour btn)
//    {
//        if (locked || lockpress) return;
//        if (dState == State.REVERT)
//        {
//            OnReuse();
//        }
//        else if (SIG.waitTo > 1e-5)
//        {
//            director.time = SIG.waitTo;
//        }
//        SetState(State.PRESS);
//        PlayAudio(SIG.Audio);
//    }


//    protected void OnBtnUp(UIBehaviour btn)
//    {
//        if (locked) return;
//        float dt = SIG.waitTo > 1e-5 ? SIG.waitTo : (float)SIG.time;
//        if (director.time - dt < SIG.PressTime)
//        {
//            SetState(State.REVERT);
//        }
//        else if (director.time - dTime < SIG.PressTimeout)
//        {
//            PerfectFX.gameObject.SetActive(true);
//            SetState(State.FINISH);
//        }
//        else
//        {
//            SetState(State.TIMEOUT);
//        }
//        RecycleAudio();
//    }

//    private void ShowSelf(bool show)
//    {
//        if (wtSlider != null)
//        {
//            wtSlider.transform.parent.gameObject.SetActive(show);
//            bg.transform.gameObject.SetActive(show);
//            xlSlider.transform.parent.gameObject.SetActive(show);
//        }
//        if (m_graphicRaycaster != null) m_graphicRaycaster.enabled = show;
//    }


//    public override void Update(float time)
//    {
//        base.Update(time);
//        switch (dState)
//        {
//            case State.PREPARE:
//                {
//                    float prog = (currTime - dTime) / SIG.waitTime;
//                    if (prog <= 1) UpdatePrepare(1 - prog);
//                    else SetState(State.FAILED);
//                }
//                break;
//            case State.PRESS:
//                {
//                    float start = SIG.waitTo > 1e-5 ? SIG.waitTo : (float)SIG.time;
//                    float prog = (float)(currTime - start) / SIG.PressTime;
//                    if (prog < 1) UpdatePress(prog);
//                    else if (currTime - start > SIG.PressTimeout)
//                    {
//                        SetState(State.TIMEOUT);
//                    }
//                    else
//                    {
//                        xlFull.gameObject.SetActive(true);
//                        lockpress = true;// 此时可以松开 不能按下
//                    }
//                }
//                break;
//            case State.REVERT:
//                {
//                    director.time -= time * SIG.backwardRate;
//                    float dt = SIG.waitTo > 1e-5 ? SIG.waitTo : (float)SIG.time;
//                    float prog = (float)(currTime - dt) / SIG.PressTime;
//                    director.Evaluate();
//                    if (prog < 1 && prog > 0) UpdatePress(prog);
//                    else SetState(State.FAILED);
//                }
//                break;
//            default:
//                break;
//        }
//    }

//    private void SetState(State state)
//    {
//        dState = state;
//        dTime = (float)director.time;
//        RTimeline.singleton.Evaluting = dState == State.REVERT;
//        HandleState();
//    }

//    private void HandleState()
//    {
//        switch (dState)
//        {
//            case State.PREPARE:
//                SetUI(dState);
//                break;
//            case State.FINISH:
//                OnFinish();
//                break;
//            case State.REUSE:
//                OnReuse();
//                break;
//            case State.TIMEOUT:
//                OnTimeout();
//                break;
//            case State.REVERT:
//                OnRevert();
//                break;
//            case State.FAILED:
//                OnFailed();
//                break;
//            case State.PRESS:
//                SetUI(dState);
//                break;
//        }
//    }

//    private void OnFailed()
//    {
//        ShowSelf(false);
//        OnReuse();
//        locked = true;
//        xlFailed.gameObject.SetActive(true);
//        if (SIG.failedTo > 1e-5)
//        {
//            director.time = SIG.failedTo;
//        }
//    }
    
//    private void OnRevert()
//    {
//        SetUI(dState);
//        director.Pause();
//    }

//    private void OnReuse()
//    {
//        director.Play();
//    }

//    private void OnFinish()
//    {
//        locked = true;
//        ShowSelf(false);
//        RecycleAudio();
//    }

//    private void OnTimeout()
//    {
//        locked = true;
//        FX1.gameObject.SetActive(true);
//        ShowSelf(false);
//    }

//    private void Return()
//    {
//        if (returnCb != null)
//        {
//            returnCb();
//        }
//    }

//    private void UpdatePrepare(float vale)
//    {
//        if (wtSlider != null)
//        {
//            wtSlider.fillAmount = vale;
//            wtSliderHander.transform.localEulerAngles =
//                new UnityEngine.Vector3(0, 0, 360 * vale);
//        }
//    }


//    private void UpdatePress(float vale)
//    {
//        float value = 1 - vale;
//        if (xlSlider)
//        {
//            xlSlider.fillAmount = 1 - value;
//            xlSliderHander.rectTransform.localEulerAngles =
//                new UnityEngine.Vector3(0, 0, 90 * value);
//        }
//    }
    

//    private void SetUI(State state)
//    {
//        if (wtSlider)
//        {
//            wtSlider.gameObject.SetActive(State.PREPARE == state);
//        }
//        if (xlSlider)
//        {
//            bool st = State.PRESS == state
//                || State.REVERT == state;
//            xlSlider.gameObject.SetActive(st);
//            xlSlider.transform.parent.gameObject.SetActive(st);
//            xlFull.gameObject.SetActive(false);
//        }
//        if (xlFailed) xlFailed.gameObject.SetActive(false);
//    }

//}