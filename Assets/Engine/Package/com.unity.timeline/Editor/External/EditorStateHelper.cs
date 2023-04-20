using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEditor.Timeline;

namespace UnityEditor.Timeline
{
    public class EditorStateHelper
    {

        public static void AddPlayListener(Action<bool> cb)
        {
            var win = TimelineWindow.instance;
            if (win != null && win.state != null)
            {
                win.state.OnPlayStateChange -= cb;
                win.state.OnPlayStateChange += cb;
            }
        }

        public static void SetupEdit(PlayableDirector director)
        {
            if (director != null && director.playableAsset != null)
            {
                SetupEdit((TimelineAsset)director.playableAsset);
            }
        }


        public static void SetupEdit(TimelineAsset asset)
        {
            if (TimelineWindow.instance?.state.editSequence.asset == null)
            {
                TimelineWindow.instance?.SetCurrentTimeline(asset);
            }
        }

        public static void ApplySetting(int frameRate, bool showMark, bool showPost, bool showAsFrame)
        {
            var win = TimelineWindow.instance;
            if (win != null && win.state != null)
            {
                var state = win.state;
                state.timeInFrames = showAsFrame;

                if (state.referenceSequence != null)
                {
                    state.referenceSequence.frameRate = frameRate;
                }


                state.showMarkerHeader = showMark;
                TimelineExternGUI.showEnv = showPost;
                win.treeView?.Reload();
            }
        }


        public static void PreviewEndCameraBlend(bool preview, float dur)
        {
            TimelineExternGUI.SetBlendWrap(preview, dur);
        }

        public static bool IsKFrame()
        {
            return TimelineWindow.instance == null ? false : TimelineWindow.instance.state.recording;
        }

    }

}
