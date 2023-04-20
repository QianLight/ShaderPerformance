using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CameraTransitionAsset : DirectBasePlayable<CameraTransitionBehaviour>, ITimelineClipAsset
{
    public enum TweenType
    {
        Linear,
        Deceleration,
        Harmonic,
        Stay,
        Custom,
    }
    public TweenType m_tweenType;

    [HideInInspector]
    public AnimationCurve customCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

    [HideInInspector]
    public AnimationCurve m_LinearCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [HideInInspector]
    public AnimationCurve m_DecelerationCurve = new AnimationCurve
    (
        new Keyframe(0f, 0f, -k_RightAngleInRads, k_RightAngleInRads),
        new Keyframe(1f, 1f, 0f, 0f)
    );
    [HideInInspector]
    public AnimationCurve m_HarmonicCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [HideInInspector]
    public AnimationCurve m_StayCurve = new AnimationCurve
    (
        new Keyframe(0f, 0f,0, 0),
        new Keyframe(0.99f, 0f,0, 0),
        new Keyframe(1f, 1f, 0f, 0f)
    );
    const float k_RightAngleInRads = Mathf.PI * 0.5f;

    [System.NonSerialized]
    public TrackAsset m_trackAsset;

    public ClipCaps clipCaps { get { return ClipCaps.None; } }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var behaviour = GetBehavior();
        behaviour.asset = this;
        return ScriptPlayable<CameraTransitionBehaviour>.Create(graph, behaviour);
    }
}

