using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CharacterShadingSettingsAsset : DirectBasePlayable<CharacterShadingSettingsBehaviour>, ITimelineClipAsset
{
    public bool outlineZWrite = true;
    public bool darkEffect;
    public Color darkEffectColor = Color.gray;

    //控制脸部材质的参数
    public bool shadowColorEffect = false;
    public Color shadowColorMultiply = Color.gray;

    public bool customGradientEffect = false;
    public Texture2D customGradientTexture = null;

    [Serializable]
    public class PartInfo
    {
        
    }

    [System.NonSerialized]
    public TrackAsset m_trackAsset;

    [System.NonSerialized]
    public TimelineClip m_clip;

    public ClipCaps clipCaps { get { return ClipCaps.None; } }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var behaviour = GetBehavior();
        behaviour.asset = this;
        return ScriptPlayable<CharacterShadingSettingsBehaviour>.Create(graph, behaviour);
    }
}
