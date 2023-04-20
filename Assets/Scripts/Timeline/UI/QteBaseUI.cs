using System;
using UnityEngine;
using UnityEngine.Playables;
using CFUtilPoolLib;
using UnityEngine.CFUI;
using CFEngine;

public class QteBaseUI
{
    protected QteClickSignal m_signal;
    protected Transform m_tf;
    protected Action returnCb;
    private uint timer;
    const float delta = 0.06f;
    private bool stop;
    protected GraphicRaycaster m_graphicRaycaster;
    protected XRuntimeFmod _runtimeFmod;

#if UNITY_EDITOR
    protected FmodPlayableUtils _fmod;
#endif

    protected PlayableDirector director { get { return RTimeline.singleton.Director; } }

    protected virtual bool isUpdate { get { return false; } }

    protected float currTime { get { return (float)RTimeline.singleton.Director.time; } }

    public void Initial(QteClickSignal sig,
        Transform trans,
        Action action)
    {
        m_signal = sig;
        m_tf = trans;
        m_graphicRaycaster = m_tf.parent.GetComponent<GraphicRaycaster>();
        returnCb = action;

        OnInitial();

        if (isUpdate)
        {
            stop = false;
            //XTimerMgr.singleton.KillTimer(timer);
            //timer = XTimerMgr.singleton.SetTimer(delta, Update, null);
        }
    }


    protected virtual void OnInitial()
    {
    }


    public virtual void Update(float time)
    {

    }


    //private void Update(object obj)
    //{
    //    OnUpdate(delta);
    //    if (!stop)
    //    {
    //        timer = XTimerMgr.singleton.SetTimer(delta, Update, null);
    //    }
    //}



    //protected virtual void OnUpdate(float delta)
    //{
    //}

    public virtual void OnDestroy()
    {
        stop = true;
        XTimerMgr.singleton.KillTimer(timer);
        timer = 0;
    }


    protected void PlayAudio(string evt)
    {
        if (string.IsNullOrEmpty(evt)) return;
#if UNITY_EDITOR
        if (!EngineContext.IsRunning)
        {
            GameObject go = GameObject.Find("timeline_Audio");
            if (go != null)
            {
                _fmod = go.GetComponent<FmodPlayableUtils>();
            }
            if (_fmod != null)
            {
                _fmod.Play(evt, 0);
            }
        }
        else
#endif
        {
            if (_runtimeFmod == null) _runtimeFmod = XRuntimeFmod.GetFMOD();
            if (_runtimeFmod != null) _runtimeFmod.StartEvent(evt, AudioChannel.Motion, true);
        }
    }


    protected void RecycleAudio()
    {
        if (_runtimeFmod != null)
        {
            //_runtimeFmod.Stop(AudioChannel.Motion);
            XRuntimeFmod.ReturnFMOD(_runtimeFmod);
            _runtimeFmod = null;
        }

#if UNITY_EDITOR
        _fmod = null;
#endif
    }

}
