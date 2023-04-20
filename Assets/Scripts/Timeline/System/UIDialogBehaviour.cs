using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class UIDialogBehaviour : DirectBaseBehaviour
{
    public double bPlayTime = 0;
    private bool haveJump = false;

    private UIDialogAsset UIAsset
    {
        get { return asset as UIDialogAsset; }
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
                TimelineDialog.singleton.OnInit(UIAsset);
            }
        }
    }


    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        PlayableDirector director = (playable.GetGraph().GetResolver() as PlayableDirector);
        if (director == null || asset == null || SelfClip == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            if (SelfClip.previousClip == null || SelfClip.start > SelfClip.previousClip.end || TimelineDialog.singleton.IsAuto)
            {
                TimelineDialog.singleton.Show(UIAsset);
                TimelineDialog.singleton.HideNext();
                TimelineDialog.singleton.SetCurrentPlayingClip(SelfClip);
                TimelineDialog.singleton.AlreadyHaveJump = false;
                bPlayTime = director.time;
                if (SelfClip.previousClip == null && director.time == 0)
                {
                    bPlayTime = 0.01f; //如果第一句话放在第0帧，将导致播放结束不会暂停，所以这里强制设置为0.01f
                }
            }
        }
        else
        {
            TimelineDialog.singleton.Show(UIAsset);
            TimelineDialog.singleton.HideNext();
            TimelineDialog.singleton.SetCurrentPlayingClip(SelfClip);
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        if (UIAsset.m_isEmpty) return;

        if (Application.isPlaying)
        {
            RuntimeTimePause(playable);
        }
        else
        {
            NotRuntimePause();
        }
    }

    private void NotRuntimePause()
    {
        TimelineClip currentPlayingClip = TimelineDialog.singleton.GetCurrentPlayingClip();
        if (SelfClip == currentPlayingClip)
        {
            if (currentPlayingClip.nextClip != null && currentPlayingClip.nextClip.asset != null)
            {
                UIDialogAsset nextAsset = currentPlayingClip.nextClip.asset as UIDialogAsset;
                if (nextAsset.m_isEmpty)
                    TimelineDialog.singleton.ShowDialogArea(false);
                else
                    TimelineDialog.singleton.HideDialogButBg();
            }

            TimelineDialog.singleton.SetCurrentPlayingClip(null);
        }
    }

    private void RuntimeTimePause(Playable playable)
    {
        PlayableDirector director = (playable.GetGraph().GetResolver() as PlayableDirector);
        if (asset != null && director != null)
        {
            if (bPlayTime > 0)
            {
                TimelineDialog.singleton.ShowNext();

                if (!TimelineDialog.singleton.IsAuto && UIAsset.m_isPause) //非自动播放，暂停功能
                {
                    if (TimelineDialog.singleton.AlreadyHaveJump)
                    {
                        TimelineDialog.singleton.AlreadyHaveJump = false;
                        RTimeline.singleton.SetSpeed(1);
                        RTimeline.singleton.DialogIsPaused = false;
                    }
                    else
                    {
                        RTimeline.singleton.SetSpeed(0);
                        RTimeline.singleton.DialogIsPaused = true;
                    }
                    TimelineDialog.singleton.SetState(TextState.Text_Clip_End);
                }

                if (!TimelineDialog.singleton.IsAuto && !UIAsset.m_isPause) //非自动&最后一个不暂停，播放完毕就隐藏对话框
                {
                    if (SelfClip.nextClip == null || SelfClip.nextClip.start > SelfClip.end)
                    {
                        TimelineDialog.singleton.ShowDialogArea(false);
                        TimelineDialog.singleton.SetCurrentPlayingClip(null);
                    }
                }

                bool isPlaying = RTimeline.singleton.IsEventPlaying(director, CFUtilPoolLib.AudioChannel.Action); //自动播放状态，等待语音播放完毕再继续
                if (TimelineDialog.singleton.IsAuto && isPlaying)
                {
                    RTimeline.singleton.SetSpeed(0);
                    RTimeline.singleton.DialogIsPaused = true;
                }

                bPlayTime = 0;
            }

            if (TimelineDialog.singleton.IsAuto && SelfClip != null)
            {
                if (SelfClip.nextClip == null || SelfClip.nextClip.start > SelfClip.end)
                {
                    TimelineDialog.singleton.SetCurrentPlayingClip(null);
                }
            }
        }
    }

    /// <summary>
    /// 继续下一句对话
    /// </summary>
    public void GoOn()
    {
        PlayableDirector director = RTimeline.singleton.Director;
        if (!director.playableGraph.IsValid()) return;

        double speed = director.playableGraph.GetRootPlayable(0).GetSpeed();
        if (speed == 1.0 && TimelineDialog.singleton.IsAuto)
        {
            return;
        }
        if (speed == 0 && TimelineChapterStartUI.singleton.isShow)
        {
            return;
        }

        if (TimelineDialog.singleton.CanJumpToNext)
        {
            XRuntimeFmod fmod = RTimeline.singleton.GetFmod(director);
            fmod.Stop(CFUtilPoolLib.AudioChannel.Action); //点击下一句的时候，如果下一句没有配置对应的语音，也要手动停止掉上一句正在播放的语言

            TimelineClip clip = TimelineDialog.singleton.GetCurrentPlayingClip();

            if(RTimeline.singleton.DialogIsPaused)
            {
                RTimeline.singleton.DialogIsPaused = false;
            }

            if (clip == null)
            {
                RTimeline.singleton.SetSpeed(1);
                TimelineDialog.singleton.ShowDialogArea(false);
                TimelineDialog.singleton.SetCurrentPlayingClip(null);
                return;
            }
            RTimeline.singleton.SetSpeed(1.0f);

            if (clip.nextClip != null && clip.nextClip.asset != null)
            {
                UIDialogAsset nextAsset = clip.nextClip.asset as UIDialogAsset;
                nextAsset.GetNativeBehaviour().bPlayTime = director.time;

                TimelineDialog.singleton.Show(nextAsset);
                TimelineDialog.singleton.SetCurrentPlayingClip(clip.nextClip);
                director.time = clip.nextClip.start;
            }
            else
            {
                TimelineDialog.singleton.ShowDialogArea(false);
                TimelineDialog.singleton.SetCurrentPlayingClip(null);
                director.time = clip.end;
            }
            TimelineDialog.singleton.CanJumpToNext = false;
            TimelineDialog.singleton.AlreadyHaveJump = true;
        }
    }
}
