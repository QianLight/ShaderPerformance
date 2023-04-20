//using CFEngine;
//using UnityEngine.CFEventSystems;
//using UnityEngine.CFUI;

////TimelineQteContinueClickUI，此类已废除
//public class QteContinuousUI : QteBaseUI
//{

//    private CFButton btn;
//    private float lastTime, initTime;
//    private int clickCount;
//    private QteContinuousSignal SIG;

//    protected override bool isUpdate => true;

//    protected override void OnInitial ()
//    {
//        base.OnInitial ();
//        if (btn == null)
//        {
//            btn = CFButton.Get<CFButton> (m_tf);
//            btn.RegisterPointerClickEvent (OnBtnClick);
//        }
//        clickCount = 0;
//        SIG = m_signal as QteContinuousSignal;
//        initTime = currTime;
//    }

//    public override void Update (float time)
//    {
//        base.Update(time);
//        if (currTime - initTime >= SIG.Duration)
//        {
//            Return ();
//        }
//    }

//    protected void OnBtnClick (UIBehaviour btn)
//    {
//        clickCount++;
//        ShakeCamera ();
//        if (currTime - lastTime < SIG.TimeOut)
//        {
//            int idx = MatchTap ();
//            if (idx >= 0)
//            {
//                PlayFx (idx);
//            }
//        }
//        else
//        {
//            clickCount = 1;
//        }
//        lastTime = currTime;
//    }

//    private int MatchTap ()
//    {
//        for (int i = 0; i < SIG.FxByCntLength; i++)
//        {
//            if (clickCount == SIG.FxByCnt[i])
//            {
//                return i;
//            }
//        }
//        return -1;
//    }

//    private void PlayFx (int i)
//    {
//        string path = SIG.GetFxPath (i);
//        var sfx = SFXMgr.singleton.Create (path);
//        if (sfx != null)
//        {
//            sfx.SetParent (m_tf);
//            sfx.flag.SetFlag (SFX.Flag_Follow, true);
//            sfx.Duration = 3;
//        }
//        // XFxMgr.singleton.CreateAndPlay(path, tf, Vector3.zero, Vector3.one, 1, true, 3.0f);
//    }

//    private void ShakeCamera ()
//    {
//        CameraPlayableBehaviour.TrigerShaking ();
//    }

//    private void Return ()
//    {
//        if (returnCb != null)
//        {
//            returnCb ();
//        }
//    }

//}