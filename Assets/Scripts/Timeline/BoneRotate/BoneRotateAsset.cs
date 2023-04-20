using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class BoneRotateAsset : DirectBasePlayable<BoneRotateBehaviour>, ITimelineClipAsset
{
    public enum RotateType
    {
        HEAD,
        HEAD_OFF,
    }

    public enum TweenType
    {
        Linear,
        Deceleration,
        Harmonic,
        Custom,
    }

    public RotateType m_bone;
    public bool m_startRotEnable;
    //public bool m_endRotEnable;
    public Vector3 m_startRot;
    public Vector3 m_endRot;
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
    const float k_RightAngleInRads = Mathf.PI * 0.5f;

    [System.NonSerialized]
    public TrackAsset m_trackAsset;

    public ClipCaps clipCaps { get { return ClipCaps.None; } }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var behaviour = GetBehavior();
        behaviour.asset = this;
        return ScriptPlayable<BoneRotateBehaviour>.Create(graph, behaviour);
    }
}

