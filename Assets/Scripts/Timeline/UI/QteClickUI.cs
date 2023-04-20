//using UnityEngine.CFEventSystems;
//using UnityEngine.CFUI;
//using UnityEngine;


////使用TimelineQteClickUI，此类已废除
//public class QteClickUI : QteBaseUI
//{
//    private QteClickSignal SIG;
//    private CFButton btn;
//    private CFImage cd, bg;
//    private CFImage cdHandler;
//    private Transform fx, miss;
//    private float initTime;
//    private float prog;
//    private bool m_clicked = false;
//    private float m_startTime;

//    protected override bool isUpdate => true;

//    protected override void OnInitial()
//    {
//        base.OnInitial();
//        if (btn == null)
//        {
//            btn = CFButton.Get<CFButton>(m_tf);
//            bg = CFImage.Get<CFImage>(m_tf, "bg");
//            cd = CFImage.Get<CFImage>(m_tf, "CD/CD");
//            cdHandler = CFImage.Get<CFImage>(m_tf, "CD/CD/fx");
//            btn.RegisterPointerClickEvent(OnBtnClick);
//            fx = m_tf.Find("fx");
//            miss = m_tf.Find("Miss");
//            Show(true);
//        }
//        SIG = m_signal as QteClickSignal;
//        m_clicked = false;
//        m_startTime = Time.realtimeSinceStartup;
//        RTimeline.singleton.SetSpeed(m_signal.m_speed); //设置速度
//    }


//    private void Show(bool show)
//    {
//        m_tf.gameObject.SetActive(show);
//        if (m_graphicRaycaster != null) m_graphicRaycaster.enabled = show;
//    }

//    protected void OnBtnClick(UIBehaviour btn)
//    {
//        m_clicked = true;
//        Show(false);
//        RTimeline.singleton.SetSpeed(1);
//    }

//    private bool CheckRight()
//    {
//        return true;
//    }

//    public override void Update(float time)
//    {
//        //base.Update(time);

//        //if (!m_clicked)
//        //{
//        //    float gap = Time.realtimeSinceStartup - m_startTime;
//        //    if (gap >= m_signal.m_signalDuration)
//        //    {
//        //        m_clicked = true;
//        //        Show(false);
//        //        RTimeline.singleton.SetSpeed(1);

//        //        if (m_signal.m_failJump)   //未点击，并且有失败跳转则跳转到指定时间
//        //        {
//        //            RTimeline.singleton.JumpToCertainTime(m_signal.m_jumpTime);
//        //        }
//        //        else
//        //        {
//        //            float[] times = new float[3];
//        //            times[2] = 0;                                   //黑屏持续0秒
//        //            times[1] = 0.01f;                               //白到黑0.01f
//        //            times[0] = m_signal.m_blackDuration - times[1]; //剩余时间是黑到白
//        //            RTimeline.singleton.ControlBlack(times);
//        //        }
//        //    }
//        //}
//    }
//}