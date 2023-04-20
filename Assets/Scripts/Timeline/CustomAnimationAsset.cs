using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CustomAnimationAsset : DirectBasePlayable<CustomAnimationBehaviour>, ITimelineClipAsset
{
    public AnimationClip clip;
    public bool m_facialIdle = false;

    [System.NonSerialized]
    public TrackAsset trackAsset;

    public ClipCaps clipCaps { get { return ClipCaps.None; } }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var behaviour = GetBehavior();
        behaviour.asset = this;
        behaviour.clip = clip;
        behaviour.m_facialIdle = m_facialIdle;

        if(clip == null)
        {
            Debug.LogError("CustomAnimationAsset clip is null " + trackAsset.name);
        }

        //if(ator != null)
        //    behaviour.SetAnimator(ator);

        return ScriptPlayable<CustomAnimationBehaviour>.Create(graph, behaviour);
    }

}

