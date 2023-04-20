using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using System.Collections.Generic;
using CFEngine;
#endif

[TrackClipType(typeof(UIPlayerAsset))]
[TrackClipType(typeof(UIBossShowAsset))]
[TrackClipType(typeof(UIEmotionAsset))]
[TrackClipType(typeof(UIFadeAsset))]
[TrackClipType(typeof(MaskFadeAsset))]
[TrackClipType(typeof(TBCFadeAsset))]
[TrackClipType(typeof(UIDramaMapAsset))]
#if UNITY_EDITOR
[TrackColor(0.66f, 0.24f, 0.44f)]
[CSDiscriptor("界面")]
#endif
public class UIPlayableTrack : TrackAsset
#if UNITY_EDITOR
    , ITimelineAsset
#endif
{

    private GameObject canvas;
    private Camera uiCamera;

#if UNITY_EDITOR

    protected override bool CheckCondition()
    {
        canvas = GameObject.Find("Canvas");
        if (canvas)
        {
            uiCamera = canvas.GetComponentInChildren<Camera>();
            return canvas != null;
        }
        return false;
    }

    public override byte GetTrackType()
    {
        return RTimeline.TrackType_UI;
    }

    public PlayableAssetType assetType
    {
        get { return PlayableAssetType.ANIM; }
    }

    public List<string> ReferenceAssets(PlayableBinding pb)
    {
        List<string> list = new List<string>();
        var clips = GetClips();
        foreach (var it in clips)
        {
            if (it.asset is UIPlayerAsset)
            {
                UIPlayerAsset asset = it.asset as UIPlayerAsset;
                if (asset.clip != null)
                {
                    string path = DirectorHelper.GetAssetPath(asset.clip, ".anim");
                    list.Add(path);
                }
            }
            else if (it.asset is UIBossShowAsset)
            {
                UIBossShowAsset asset = it.asset as UIBossShowAsset;
                if (asset.clip != null)
                {
                    string path = DirectorHelper.GetAssetPath(asset.clip, ".anim");
                    list.Add(path);
                }
            }
        }
        return list;
    }

    protected override void OnCreateClip(TimelineClip clip)
    {
        base.OnCreateClip(clip);
        var asset = clip.asset;
        if (asset is UIPlayerAsset)
        {
            clip.displayName = "出场";
        }
        else if (asset is UIBossShowAsset)
        {
            clip.displayName = "精英小BOSS";
        }
        else if (asset is UIEmotionAsset)
        {
            clip.displayName = "情绪对话";
        }
        else if (asset is UIFadeAsset)
        {
            clip.displayName = "黑屏打字";
        }
        else if (asset is MaskFadeAsset)
        {
            clip.displayName = "黑屏遮罩";
        }
        else if (asset is MaskFadeAsset)
        {
            clip.displayName = "To be Continue";
        }
        else if (asset is UIDramaMapAsset)
        {
            clip.displayName = "剧情地图";
        }
    }

#endif

}