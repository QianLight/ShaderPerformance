using System;
using CFEngine;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    // The RuntimeClip wraps a single clip in an instanciated sequence.
    // It supports the IInterval interface so that it can be stored in the interval tree
    // It is this class that is returned by an interval tree query.
    class DirectorRuntimeClip : RuntimeElement, ISharedObject
    {
        delegate Int64 GetKeyframeTime (DirectorRuntimeClip clip);
        delegate void SetEnable (DirectorRuntimeClip clip, bool enable);
        delegate void EvaluateAtClip (DirectorRuntimeClip clip, double localTime, ref FrameData frameData);

        class ClipFun
        {
            public GetKeyframeTime starFun;
            public GetKeyframeTime endFun;
            public SetEnable enableFun;
            public EvaluateAtClip evaluateFun;
        }
        public static int CommonClip = 0;
        public static int Schedule = 1;
        public static int Infinite = 2;

        private Playable m_Playable;
        private Playable m_ParentMixer;
        private DirectorClip m_Clip;
        private double m_StartDelay;
        private double m_FinishTail;
        private bool m_Started = false;
        private int m_ClipType = 0;
        private static ClipFun[] m_ClipFun = new ClipFun[]
        {
            new ClipFun () { starFun = GetStartCommon, endFun = GetEndCommon, enableFun = SetEnableCommon, evaluateFun = EvaluateAtCommon },
            new ClipFun () { starFun = GetStartSchedule, endFun = GetEndSchedule, enableFun = SetEnableSchedule, evaluateFun = EvaluateAtSchedul },
            new ClipFun () { starFun = GetStartInfinite, endFun = GetEndInfinite, enableFun = SetEnableInfinite, evaluateFun = EvaluateAtInfinite },
        };

        public static DirectorRuntimeClip GetRuntimeClip (
            int clipType,
            DirectorClip clip,
            Playable clipPlayable,
            Playable parentMixer,
            double startDelay = 0.2, double finishTail = 0.1)
        {
            DirectorRuntimeClip drc = SharedObjectPool<DirectorRuntimeClip>.Get ();
            drc.m_Clip = clip;
            drc.m_Playable = clipPlayable;
            drc.m_ParentMixer = parentMixer;
            drc.m_StartDelay = startDelay;
            drc.m_FinishTail = finishTail;
            drc.m_ClipType = clipType;
            clipPlayable.Pause ();
            return drc;
        }

        public static void ReleaseRuntimeClip (DirectorRuntimeClip clip)
        {
            SharedObjectPool<DirectorRuntimeClip>.Release (clip);
        }

        public override void Reset ()
        {
            m_Playable = Playable.Null;
            m_ParentMixer = Playable.Null;
            m_Clip = null;
            m_StartDelay = 0;
            m_FinishTail = 0;
            m_Started = false;
            m_ClipType = 0;
        }

        #region clipFun
        //Common
        static Int64 GetStartCommon (DirectorRuntimeClip clip)
        {
            return DiscreteTime.GetNearestTick (clip.m_Clip.extrapolatedStart);
        }

        static Int64 GetEndCommon (DirectorRuntimeClip clip)
        {
            return DiscreteTime.GetNearestTick (clip.m_Clip.extrapolatedStart + clip.m_Clip.extrapolatedDuration);
        }

        static void SetEnableCommon (DirectorRuntimeClip clip, bool enable)
        {
            if (enable && clip.m_Playable.GetPlayState () != PlayState.Playing)
            {
                clip.m_Playable.Play ();
            }
            else if (!enable && clip.m_Playable.GetPlayState () != PlayState.Paused)
            {
                clip.m_Playable.Pause ();
                if (clip.m_ParentMixer.IsValid ())
                    clip.m_ParentMixer.SetInputWeight (clip.m_Playable, 0.0f);
            }
        }

        static void EvaluateAtCommon (DirectorRuntimeClip drc, double localTime, ref FrameData frameData)
        {
            drc.enable = true;
            var clip = drc.m_Clip;

            float weight = 1.0f;
            if (clip.IsPreExtrapolatedTime (localTime))
                weight = clip.EvaluateMixIn ((float) clip.start);
            else if (clip.IsPostExtrapolatedTime (localTime))
                weight = clip.EvaluateMixOut ((float) clip.end);
            else
                weight = clip.EvaluateMixIn (localTime) * clip.EvaluateMixOut (localTime);

            if (drc.m_ParentMixer.IsValid ())
                drc.m_ParentMixer.SetInputWeight (drc.m_Playable, weight);

            // localTime of the sequence to localtime of the clip
            double clipTime = clip.ToLocalTime (localTime);
            if (clipTime > 0)
            {
                drc.m_Playable.SetTime (clipTime);
            }
            drc.m_Playable.SetDuration (clip.extrapolatedDuration);
        }

        //Schedule
        static double GetScheduleStart (DirectorRuntimeClip clip)
        {
            return Math.Max (0, clip.m_Clip.start - clip.m_StartDelay);
        }
        static Int64 GetStartSchedule (DirectorRuntimeClip clip)
        {
            return DiscreteTime.GetNearestTick (GetScheduleStart (clip));
        }
        static double GetScheduleDuration (DirectorRuntimeClip clip)
        {
            return clip.m_Clip.duration + clip.m_FinishTail + clip.m_Clip.start;
        }
        static Int64 GetEndSchedule (DirectorRuntimeClip clip)
        {
            return DiscreteTime.GetNearestTick (GetScheduleStart (clip) + GetScheduleDuration (clip));
        }
        static void SetEnableSchedule (DirectorRuntimeClip clip, bool enable)
        {
            if (enable && clip.m_Playable.GetPlayState () != PlayState.Playing)
            {
                clip.m_Playable.Play ();
            }
            else if (!enable && clip.m_Playable.GetPlayState () != PlayState.Paused)
            {
                clip.m_Playable.Pause ();
                if (clip.m_ParentMixer.IsValid ())
                    clip.m_ParentMixer.SetInputWeight (clip.m_Playable, 0.0f);
            }

            clip.m_Started &= enable;
        }
        static void EvaluateAtSchedul (DirectorRuntimeClip drc, double localTime, ref FrameData frameData)
        {
            if (frameData.timeHeld)
            {
                drc.enable = false;
                return;
            }

            bool forceSeek = frameData.seekOccurred ||
                frameData.timeLooped ||
                frameData.evaluationType == FrameData.EvaluationType.Evaluate;

            // If we are in the tail region of the clip, then dont do anything
            if (localTime > GetScheduleStart (drc) + GetScheduleDuration (drc) - drc.m_FinishTail)
                return;
            var clip = drc.m_Clip;
            // this may set the weight to 1 in a delay, but it will avoid missing the start
            float weight = clip.EvaluateMixIn (localTime) * clip.EvaluateMixOut (localTime);
            if (drc.m_ParentMixer.IsValid ())
                drc.m_ParentMixer.SetInputWeight (drc.m_Playable, weight);

            // localTime of the sequence to localtime of the clip
            if (!drc.m_Started || forceSeek)
            {
                // accounts for clip in and speed
                // double clipTime = clip.ToLocalTime (Math.Max (localTime, clip.start));
                // multiply by the time scale so the delay is local to the clip
                //  Audio will rescale based on it's effective time scale (which includes the parent)
                // double startDelay = Math.Max (clip.start - localTime, 0) * clip.timeScale;
                // double durationLocal = clip.duration * clip.timeScale;
                // if (drc.m_Playable.IsPlayableOfType<AudioClipPlayable> ())
                //     ((AudioClipPlayable) drc.m_Playable).Seek (clipTime, startDelay, durationLocal);

                drc.m_Started = true;
            }
        }
        //Infinite
        static Int64 GetStartInfinite (DirectorRuntimeClip clip)
        {
            return 0;
        }

        static Int64 GetEndInfinite (DirectorRuntimeClip clip)
        {
            return DiscreteTime.GetNearestTick (TimelineClip.kMaxTimeValue);
        }
        static void SetEnableInfinite (DirectorRuntimeClip clip, bool enable)
        {
            if (enable)
                clip.m_Playable.Play ();
            else
                clip.m_Playable.Pause ();
        }
        static void EvaluateAtInfinite (DirectorRuntimeClip drc, double localTime, ref FrameData frameData)
        {
            drc.m_Playable.SetTime (localTime);
        }
        #endregion

        public override Int64 intervalStart
        {
            get
            {
                return m_ClipFun[m_ClipType].starFun (this);
            }
        }

        public override Int64 intervalEnd
        {
            get
            {
                return m_ClipFun[m_ClipType].endFun (this);
            }
        }

        public override bool enable
        {
            set
            {
                m_ClipFun[m_ClipType].enableFun (this, value);
            }
        }

        public override void EvaluateAt (double localTime, FrameData frameData)
        {
            m_ClipFun[m_ClipType].evaluateFun (this, localTime, ref frameData);
        }
    }
}