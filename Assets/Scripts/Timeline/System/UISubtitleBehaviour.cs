using UnityEngine.Playables;
using UnityEngine.Timeline;

public class UISubtitleBehaviour : DirectBaseBehaviour
{
    private UISubtitleAsset UIAsset
    {
        get { return asset as UISubtitleAsset; }
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        if (asset == null) return;
        TimelineSubtitle.singleton.Show(UIAsset);
        TimelineSubtitle.singleton.CurrentPlayingAsset = UIAsset;
    }


    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (asset == null) return;
        if(TimelineSubtitle.singleton.CurrentPlayingAsset == UIAsset)
        {
            TimelineSubtitle.singleton.Clear();
            TimelineSubtitle.singleton.CurrentPlayingAsset = null;
        }
    }
}
