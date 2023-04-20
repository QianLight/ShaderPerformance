using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CFUtilPoolLib;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using CFEngine;
#endif

[TrackColor(0.86f, 0.84f, 0.44f)]
[TrackClipType(typeof(CharacterShadingSettingsAsset))]
[TrackBindingType(typeof(Animator))]
[ExcludeFromPreset]
#if UNITY_EDITOR
[CSDiscriptor("角色着色设置")]
#endif
public class CharacterShadingSettingsTrack : TrackAsset
#if UNITY_EDITOR
    , ITimelineAsset, IDisplayTrack
#endif
{
    
#if UNITY_EDITOR
    public override byte GetTrackType()
    {
        return RTimeline.TrackType_CharacterShadingSettings;
    }
    PlayableAssetType ITimelineAsset.assetType
    {
        get { return PlayableAssetType.ANIM; }
    }
    public List<string> ReferenceAssets(PlayableBinding pb)
    {
        List<string> list = CFAllocator.AllocateList<string>();
        var tack = pb.sourceObject as CharacterShadingSettingsTrack;
        var clips = tack.GetClips();
        return list;
    }

    public void GUIDisplayName()
    {
        
    }
#endif   
    protected override void OnCreateClip(TimelineClip clip)
    {
        if (clip.asset != null)
            clip.displayName = clip.asset.GetType().Name;
    }

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        foreach (var c in GetClips())
        {
            CharacterShadingSettingsAsset asset = (CharacterShadingSettingsAsset)(c.asset);
            asset.m_trackAsset = this;
        }
        return base.CreateTrackMixer(graph, go, inputCount);
    }
}