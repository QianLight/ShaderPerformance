using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Animations;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Timeline;
#endif


public class CameraTransitionBehaviour : DirectBaseBehaviour
{
    public CameraTransitionAsset.TweenType m_tweenType;
    private float m_start = 1;
    private float m_end = 0;

    public CameraTransitionAsset TransitionAsset
    {
        get
        {
            return asset as CameraTransitionAsset;
        }
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        base.PrepareFrame(playable, info);

        CameraTransitionAsset transitionAsset = asset as CameraTransitionAsset;
        m_tweenType = transitionAsset.m_tweenType;

        double curTime = playable.GetTime();
        float normalisedTime = (float)(curTime / playable.GetDuration());
        float tweenProgress = EvaluateCurrentCurve(normalisedTime);
        float tweenProcess = Mathf.Lerp(m_start, m_end, tweenProgress);

        if (RTimeline.singleton.ScreenEffectAnimation != null)
        {
            RTimeline.singleton.ScreenEffectAnimation.transition_duration = tweenProcess;
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        base.OnBehaviourPause(playable, info);
        if (RTimeline.singleton.ScreenEffectAnimation != null)
        {
           RTimeline.singleton.ScreenEffectAnimation.Release();
        }
    }

    public float EvaluateCurrentCurve(float time)
    {
        if (m_tweenType == CameraTransitionAsset.TweenType.Custom && !IsCustomCurveNormalised())
        {
            Debug.LogError("Custom Curve is not normalised.  Curve must start at 0,0 and end at 1,1.");
            return 0f;
        }

        switch (m_tweenType)
        {
            case CameraTransitionAsset.TweenType.Linear:
                return TransitionAsset.m_LinearCurve.Evaluate(time);
            case CameraTransitionAsset.TweenType.Deceleration:
                return TransitionAsset.m_DecelerationCurve.Evaluate(time);
            case CameraTransitionAsset.TweenType.Harmonic:
                return TransitionAsset.m_HarmonicCurve.Evaluate(time);
            case CameraTransitionAsset.TweenType.Stay:
                return TransitionAsset.m_StayCurve.Evaluate(time);
            default:
                return TransitionAsset.customCurve.Evaluate(time);
        }
    }

    bool IsCustomCurveNormalised()
    {
        if (!Mathf.Approximately(TransitionAsset.customCurve[0].time, 0f))
            return false;

        if (!Mathf.Approximately(TransitionAsset.customCurve[0].value, 0f))
            return false;

        if (!Mathf.Approximately(TransitionAsset.customCurve[TransitionAsset.customCurve.length - 1].time, 1f))
            return false;

        return Mathf.Approximately(TransitionAsset.customCurve[TransitionAsset.customCurve.length - 1].value, 1f);
    }
}

