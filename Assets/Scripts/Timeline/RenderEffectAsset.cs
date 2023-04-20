using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

class RenderEffectAsset : DirectBasePlayable<RenderEffectBehaviour>, ITimelineClipAsset
{
    public uint m_effectID;

    [System.NonSerialized]
    public TrackAsset trackAsset;

    public ClipCaps clipCaps { get { return ClipCaps.None; } }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var behaviour = GetBehavior();
        behaviour.asset = this;
        return ScriptPlayable<RenderEffectBehaviour>.Create(graph, behaviour);
    }

}

