using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using CFEngine;
#endif

[TrackColor (0.86f, 0.84f, 0.44f)]
[TrackClipType (typeof (CameraPlayableAsset))]
[TrackBindingType (typeof (GameObject))]
#if UNITY_EDITOR
[CSDiscriptor("镜头")]
#endif
public class CameraPlayableTrack : TrackAsset
#if UNITY_EDITOR
    , ITimelineAsset
#endif
{
    protected override bool CheckCondition ()
    {
        var clips = GetClips ();
        int cnt = clips.Count ();
#if UNITY_EDITOR
        if (cnt > 0)
        {
            EditorUtility.DisplayDialog (" Warning!", "Only support add one clip on camera track", "OK");
        }
#endif
        return cnt < 1;
    }

    protected override void OnCreateClip (TimelineClip clip)
    {
        base.OnCreateClip (clip);
        clip.displayName = "Main Camera";
    }

    public override Playable CreateTrackMixer (PlayableGraph graph, GameObject go, int inputCount)
    {
        var director = go.GetComponent<PlayableDirector> ();
        Camera mc = Camera.main;
        if (director != null && mc != null)
        {
            director.SetGenericBinding (this, mc.gameObject);
        }

        return base.CreateTrackMixer (graph, go, inputCount);
    }

#if UNITY_EDITOR

    PlayableAssetType ITimelineAsset.assetType
    {
        get { return PlayableAssetType.ANIM; }
    }

    List<string> ITimelineAsset.ReferenceAssets (PlayableBinding pb)
    {
        List<string> list = new List<string> ();
        var tack = pb.sourceObject as CameraPlayableTrack;
        var clips = tack.GetClips ();
        foreach (var item in clips)
        {
            CameraPlayableAsset asset = item.asset as CameraPlayableAsset;
            string path = asset.path;
            if (!string.IsNullOrEmpty (path))
            {
                path = "Animation/Main_Camera/" + asset.path;
                list.Add (path);
            }

            if (!string.IsNullOrEmpty (asset.shake))
            {
                path = "Animation/Main_Camera/" + asset.shake;
                list.Add (path);
            }
        }
        return list;
    }
    public override byte GetTrackType ()
    {
        return RTimeline.TrackType_Camera;
    }
#endif

}