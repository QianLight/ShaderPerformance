using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using CFEngine;
using System.Collections.Generic;

public class UIPlayDialogBehaviour : DirectBaseBehaviour
{
    private UIPlayDialogAsset UIAsset
    {
        get { return asset as UIPlayDialogAsset; }
    }

    private TimelineClip SelfClip
    {
        get { return UIAsset.m_clip; }
    }

    public override void OnGraphStart(Playable playable)
    {
        if (asset != null)
        {
            if (CFEngine.EngineContext.IsRunning)
            {
                TimelinePlayDialog.singleton.OnInit(UIAsset);
            }
        }
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        TimelinePlayDialog.singleton.Show(UIAsset);
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        TimelinePlayDialog.singleton.ShowDialogArea(false);
    }
}
