using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class ControlPartAsset : DirectBasePlayable<ControlPartBehaviour>, ITimelineClipAsset
{
    [Serializable]
    public class PartInfo
    {
        public string m_path;
        public bool m_enable;
    }


    [System.NonSerialized]
    public TrackAsset m_trackAsset;

    [System.NonSerialized]
    public TimelineClip m_clip;

    public List<PartInfo> m_partInfos = new List<PartInfo>();
    public List<bool> m_cachedEnables = new List<bool>();

    public ClipCaps clipCaps { get { return ClipCaps.None; } }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var behaviour = GetBehavior();
        behaviour.asset = this;
        return ScriptPlayable<ControlPartBehaviour>.Create(graph, behaviour);
    }
}

