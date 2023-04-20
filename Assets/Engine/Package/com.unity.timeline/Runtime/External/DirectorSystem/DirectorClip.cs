using System;
using CFEngine;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{

    public partial class DirectorClip
    {
        public double m_Start;
        public double m_ClipIn;

        public double m_Duration;
        public double m_TimeScale = 1.0;

        // for mixing out scripts - default is no mix out (i.e. flat)
        public double m_EaseInDuration;
        public double m_EaseOutDuration;

        // the blend durations override ease in / out durations
        public double m_BlendInDuration = -1.0f;
        public double m_BlendOutDuration = -1.0f;

        // doubles as ease in/out and blend in/out curves
        AnimationCurve m_MixInCurve;
        AnimationCurve m_MixOutCurve;
        // extrapolation
        public TimelineClip.ClipExtrapolation m_PostExtrapolationMode;
        public TimelineClip.ClipExtrapolation m_PreExtrapolationMode;
        public double m_PostExtrapolationTime;
        public double m_PreExtrapolationTime;

        PlayableAsset m_Asset;

        public short index = -1;

        /// <summary>
        /// A speed multiplier for the clip;
        /// </summary>
        public double timeScale
        {
            get { return clipCaps.HasAny (ClipCaps.SpeedMultiplier) ? Math.Max (TimelineClip.kTimeScaleMin, Math.Min (m_TimeScale, TimelineClip.kTimeScaleMax)) : 1.0; }
        }

        /// <summary>
        /// The start time, in seconds, of the clip
        /// </summary>
        public double start
        {
            get { return m_Start; }
        }

        /// <summary>
        /// The length, in seconds, of the clip
        /// </summary>
        public double duration
        {
            get { return m_Duration; }
        }

        /// <summary>
        /// The end time, in seconds of the clip
        /// </summary>
        public double end
        {
            get { return m_Start + m_Duration; }
        }

        /// <summary>
        /// Local offset time of the clip.
        /// </summary>
        public double clipIn
        {
            get { return clipCaps.HasAny (ClipCaps.ClipIn) ? m_ClipIn : 0; }
        }

        /// <summary>
        /// The PlayableAsset attached to the clip.
        /// </summary>
        public PlayableAsset asset
        {
            get { return m_Asset; }
            set { m_Asset = value; }
        }

        /// <summary>
        /// The ease in duration of the timeline clip in seconds. This only applies if the start of the clip is not overlapping.
        /// </summary>
        public double easeInDuration
        {
            get { return clipCaps.HasAny (ClipCaps.Blending) ? Math.Min (Math.Max (m_EaseInDuration, 0), duration * 0.49) : 0; }
        }

        /// <summary>
        /// The ease out duration of the timeline clip in seconds. This only applies if the end of the clip is not overlapping.
        /// </summary>
        public double easeOutDuration
        {
            get { return clipCaps.HasAny (ClipCaps.Blending) ? Math.Min (Math.Max (m_EaseOutDuration, 0), duration * 0.49) : 0; }
        }

        /// <summary>
        /// The amount of overlap in seconds on the start of a clip.
        /// </summary>
        public double blendInDuration
        {
            get { return clipCaps.HasAny (ClipCaps.Blending) ? m_BlendInDuration : 0; }
        }

        /// <summary>
        /// The amount of overlap in seconds at the end of a clip.
        /// </summary>
        public double blendOutDuration
        {
            get { return clipCaps.HasAny (ClipCaps.Blending) ? m_BlendOutDuration : 0; }
        }

        /// <summary>
        /// Returns whether the clip is blending in
        /// </summary>
        public bool hasBlendIn { get { return clipCaps.HasAny (ClipCaps.Blending) && m_BlendInDuration > 0; } }

        /// <summary>
        /// Returns whether the clip is blending out
        /// </summary>
        public bool hasBlendOut { get { return clipCaps.HasAny (ClipCaps.Blending) && m_BlendOutDuration > 0; } }

        /// <summary>
        /// The animation curve used for calculating weights during an ease in or a blend in.
        /// </summary>
        public AnimationCurve mixInCurve
        {
            get
            {
                // auto fix broken curves
                if (m_MixInCurve == null || m_MixInCurve.length < 2)
                    m_MixInCurve = GetDefaultMixInCurve ();

                return m_MixInCurve;
            }
        }

        /// <summary>
        /// The amount of the clip blending or easing in, in seconds
        /// </summary>
        public double mixInDuration
        {
            get { return hasBlendIn ? blendInDuration : easeInDuration; }
        }

        /// <summary>
        /// The animation curve used for calculating weights during an ease out or a blend out.
        /// </summary>
        public AnimationCurve mixOutCurve
        {
            get
            {
                if (m_MixOutCurve == null || m_MixOutCurve.length < 2)
                    m_MixOutCurve = GetDefaultMixOutCurve ();
                return m_MixOutCurve;
            }
        }

        /// <summary>
        /// The time in seconds that an ease out or blend out starts
        /// </summary>
        public double mixOutTime
        {
            get { return duration - mixOutDuration + m_Start; }
        }

        /// <summary>
        /// The amount of the clip blending or easing out, in seconds
        /// </summary>
        public double mixOutDuration
        {
            get { return hasBlendOut ? blendOutDuration : easeOutDuration; }
        }

        /// <summary>
        /// Returns the capabilities supported by this clip.
        /// </summary>
        public ClipCaps clipCaps
        {
            get
            {
                var clipAsset = asset as ITimelineClipAsset;
                return (clipAsset != null) ? clipAsset.clipCaps : TimelineClip.kDefaultClipCaps;
            }
        }

        /// <summary>
        /// Given a time, returns the weight from the mix out
        /// </summary>
        /// <param name="time">Time (relative to the timeline)</param>
        /// <returns></returns>
        public float EvaluateMixOut (double time)
        {
            if (!clipCaps.HasAny (ClipCaps.Blending))
                return 1.0f;

            if (mixOutDuration > Mathf.Epsilon)
            {
                var perc = (float) (time - mixOutTime) / (float) mixOutDuration;
                perc = Mathf.Clamp01 (mixOutCurve.Evaluate (perc));
                return perc;
            }
            return 1.0f;
        }

        /// <summary>
        /// Given a time, returns the weight from the mix in
        /// </summary>
        /// <param name="time">Time (relative to the timeline)</param>
        /// <returns></returns>
        public float EvaluateMixIn (double time)
        {
            if (!clipCaps.HasAny (ClipCaps.Blending))
                return 1.0f;

            if (mixInDuration > Mathf.Epsilon)
            {
                var perc = (float) (time - m_Start) / (float) mixInDuration;
                perc = Mathf.Clamp01 (mixInCurve.Evaluate (perc));
                return perc;
            }
            return 1.0f;
        }

        static AnimationCurve GetDefaultMixInCurve ()
        {
            return AnimationCurve.EaseInOut (0, 0, 1, 1);
        }

        static AnimationCurve GetDefaultMixOutCurve ()
        {
            return AnimationCurve.EaseInOut (0, 1, 1, 0);
        }

        /// <summary>
        /// Converts from global time to a clips local time.
        /// </summary>
        /// <param name="time">time relative to the timeline</param>
        /// <returns>
        /// The local time with extrapolation applied
        /// </returns>
        public double ToLocalTime (double time)
        {
            if (time < 0)
                return time;

            // handle Extrapolation
            if (IsPreExtrapolatedTime (time))
                time = GetExtrapolatedTime (time - m_Start, m_PreExtrapolationMode, m_Duration);
            else if (IsPostExtrapolatedTime (time))
                time = GetExtrapolatedTime (time - m_Start, m_PostExtrapolationMode, m_Duration);
            else
                time -= m_Start;

            // handle looping and time scale within the clip
            time *= timeScale;
            time += clipIn;

            return time;
        }

        /// <summary>
        /// Returns whether the clip is being extrapolated past the end time.
        /// </summary>
        public TimelineClip.ClipExtrapolation postExtrapolationMode
        {
            get { return clipCaps.HasAny (ClipCaps.Extrapolation) ? m_PostExtrapolationMode : TimelineClip.ClipExtrapolation.None; }
        }

        /// <summary>
        /// Returns whether the clip is being extrapolated before the start time.
        /// </summary>
        public TimelineClip.ClipExtrapolation preExtrapolationMode
        {
            get { return clipCaps.HasAny (ClipCaps.Extrapolation) ? m_PreExtrapolationMode : TimelineClip.ClipExtrapolation.None; }
        }

        /// <summary>
        /// Given a time, returns whether it falls within the clip pre-extrapolation
        /// </summary>
        /// <param name="sequenceTime">The time relative to the timeline</param>
        public bool IsPreExtrapolatedTime (double sequenceTime)
        {
            return preExtrapolationMode != TimelineClip.ClipExtrapolation.None &&
                sequenceTime < m_Start && sequenceTime >= m_Start - m_PreExtrapolationTime;
        }

        /// <summary>
        /// Given a time, returns whether it falls within the clip post-extrapolation
        /// </summary>
        /// <param name="sequenceTime">The time relative to the timeline</param>
        public bool IsPostExtrapolatedTime (double sequenceTime)
        {
            return postExtrapolationMode != TimelineClip.ClipExtrapolation.None &&
                (sequenceTime > end) && (sequenceTime - end < m_PostExtrapolationTime);
        }

        /// <summary>
        /// The start time of the clip, accounting for pre-extrapolation
        /// </summary>
        public double extrapolatedStart
        {
            get
            {
                if (m_PreExtrapolationMode != TimelineClip.ClipExtrapolation.None)
                    return m_Start - m_PreExtrapolationTime;

                return m_Start;
            }
        }

        /// <summary>
        /// The length of the clip in seconds, including extrapolation.
        /// </summary>
        public double extrapolatedDuration
        {
            get
            {
                double length = m_Duration;

                if (m_PostExtrapolationMode != TimelineClip.ClipExtrapolation.None)
                    length += Math.Min (m_PostExtrapolationTime, TimelineClip.kMaxTimeValue);

                if (m_PreExtrapolationMode != TimelineClip.ClipExtrapolation.None)
                    length += m_PreExtrapolationTime;

                return length;
            }
        }

        static double GetExtrapolatedTime (double time, TimelineClip.ClipExtrapolation mode, double duration)
        {
            if (duration == 0)
                return 0;

            switch (mode)
            {
                case TimelineClip.ClipExtrapolation.None:
                    break;

                case TimelineClip.ClipExtrapolation.Loop:
                    if (time < 0)
                        time = duration - (-time % duration);
                    else if (time > duration)
                        time %= duration;
                    break;

                case TimelineClip.ClipExtrapolation.Hold:
                    if (time < 0)
                        return 0;
                    if (time > duration)
                        return duration;
                    break;

                case TimelineClip.ClipExtrapolation.PingPong:
                    if (time < 0)
                    {
                        time = duration * 2 - (-time % (duration * 2));
                        time = duration - Math.Abs (time - duration);
                    }
                    else
                    {
                        time = time % (duration * 2.0);
                        time = duration - Math.Abs (time - duration);
                    }
                    break;

                case TimelineClip.ClipExtrapolation.Continue:
                    break;
            }
            return time;
        }
        public void Load (CFBinaryReader reader)
        {
            m_Start = reader.ReadDouble ();
            m_ClipIn = reader.ReadDouble ();
            m_Duration = reader.ReadDouble ();
            m_TimeScale = reader.ReadDouble ();
            m_EaseInDuration = reader.ReadDouble ();
            m_EaseOutDuration = reader.ReadDouble ();
            m_BlendInDuration = reader.ReadDouble ();
            m_BlendOutDuration = reader.ReadDouble ();
            m_PostExtrapolationMode = (TimelineClip.ClipExtrapolation) reader.ReadByte ();
            m_PreExtrapolationMode = (TimelineClip.ClipExtrapolation) reader.ReadByte ();
            m_PostExtrapolationTime = reader.ReadDouble ();
            m_PreExtrapolationTime = reader.ReadDouble ();
            byte assetType = reader.ReadByte ();
            DirectPlayableAsset a = DirectorHelper.CreateAsset (assetType, reader);
            if (a != null)
            {
                a.directorClip = this;
                m_Asset = a;
                a.Load (reader);
            }
        }

        public void Reset ()
        {
            var dpa = asset as DirectPlayableAsset;
            if (dpa != null)
            {
                dpa.Reset ();
                DirectorHelper.ReleaseAsset (dpa);
                asset = null;
            }
            index = -1;
        }
    }
}