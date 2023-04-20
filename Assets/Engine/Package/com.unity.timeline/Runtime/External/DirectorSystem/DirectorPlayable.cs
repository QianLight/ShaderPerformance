using System;
using System.Collections.Generic;
using CFEngine;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Audio;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    public interface IDirectorEvaluate
    {
        void InitOutput(ref PlayableOutput playableOutput);
        void Evaluate();
    }

    public class DirectorPlayable : PlayableBehaviour
    {
        private IntervalTree<RuntimeElement> m_IntervalTree = new IntervalTree<RuntimeElement>();
        private List<RuntimeElement> m_LastActiveClips;
        private List<RuntimeElement> m_CurrentActiveClips;
        private int m_ActiveBit = 0;
        private ScriptPlayable<DirectorTimeNotificationBehaviour> notificationBehaviour;

        public static ScriptPlayable<DirectorPlayable> Create(PlayableGraph graph)
        {
            var playable = ScriptPlayable<DirectorPlayable>.Create(graph);
            playable.SetTraversalMode(PlayableTraversalMode.Passthrough);
            playable.SetPropagateSetTime(true);
            return playable;
        }

        public void Clear()
        {
            for (int i = 0; i < m_IntervalTree.m_Entries.Count; ++i)
            {
                var entry = m_IntervalTree.m_Entries[i];
                var clip = entry.item as DirectorRuntimeClip;
                if (clip != null)
                {
                    DirectorRuntimeClip.ReleaseRuntimeClip(clip);
                }
            }

            m_IntervalTree.Clear();
            m_LastActiveClips?.Clear();
            m_CurrentActiveClips?.Clear();
            if (!notificationBehaviour.IsNull())
            {
                notificationBehaviour.GetBehaviour().Reset();
            }
        }

        public void Compile(ref PlayableGraph graph, Playable timelinePlayable, GameObject go,
            bool createOutputs)
        {
            var tracks = DirectorHelper.singleton.tracks;
            int trackCount = DirectorHelper.singleton.trackCount;
            if (tracks == null || trackCount == 0)
            {
                DebugLog.AddWarningLog("null track or gameObject");
                return;
            }

            int maximumNumberOfIntersections =
                tracks.Length * 2 + tracks.Length; // worse case: 2 overlapping clips per track + each track
            if (maximumNumberOfIntersections < 10)
                maximumNumberOfIntersections = 10;
            if (m_LastActiveClips == null)
            {
                m_LastActiveClips = new List<RuntimeElement>(maximumNumberOfIntersections);
            }
            else if (m_LastActiveClips.Capacity < maximumNumberOfIntersections)
            {
                m_LastActiveClips.Capacity = maximumNumberOfIntersections;
            }

            if (m_CurrentActiveClips == null)
            {
                m_CurrentActiveClips = new List<RuntimeElement>(maximumNumberOfIntersections);
            }
            else if (m_CurrentActiveClips.Capacity < maximumNumberOfIntersections)
            {
                m_CurrentActiveClips.Capacity = maximumNumberOfIntersections;
            }

            for (int i = 0; i < trackCount; ++i)
            {
                var track = tracks[i];
                if (track != null)
                {
                    CreateTrackPlayable(graph, timelinePlayable, track, tracks, go, createOutputs, true);
                }
            }
        }

        private void CreateTrackPlayable(
            PlayableGraph graph,
            Playable timelinePlayable,
            DirectorTrackAsset track,
            DirectorTrackAsset[] tracks,
            GameObject go,
            bool createOutputs,
            bool force)
        {
            if (track.HasFlag(DirectorTrackAsset.Flag_Created) && !force)
            {
                return;
            }

            if (track.name == "root")
                return;

            // DebugLog.AddLog2 ("create track:{0}", track.trackIndex.ToString ());
            DirectorTrackAsset parentActor = null;
            if (track.parentTrackIndex >= 0 &&
                track.parentTrackIndex < tracks.Length)
            {
                parentActor = tracks[track.parentTrackIndex];
            }

            bool connected = false;
            var parentPlayable = timelinePlayable;
            if (track.trackType == DirectorHelper.TrackType_Mark)
            {
                DirectorTimeNotificationBehaviour.CreateNotificationsPlayable(ref notificationBehaviour, graph, 0, 0);
                track.playable = notificationBehaviour;
            }
            else
            {
                if (parentActor != null)
                {
                    CreateTrackPlayable(graph, timelinePlayable, parentActor, tracks, go, createOutputs, false);
                    parentPlayable = parentActor.playable;
                }

                track.CreatePlayableGraph(ref graph, go, m_IntervalTree);
            }

            if (!track.playable.IsValid())
            {
                DebugLog.AddErrorLog("invalid playable");
                return;
            }

            // Special case for animation tracks
            if (parentPlayable.IsValid() && track.playable.IsValid())
            {
                int port = parentPlayable.GetInputCount();
                parentPlayable.SetInputCount(port + 1);
                connected = graph.Connect(track.playable, 0, parentPlayable, port);
                parentPlayable.SetInputWeight(port, 1.0f);
            }

            if (createOutputs && connected)
            {
                CreateTrackOutput(graph, track, go, parentPlayable, parentPlayable.GetInputCount() - 1);
            }

            track.SetFlag(DirectorTrackAsset.Flag_Created, true);
        }

        void CreateTrackOutput(PlayableGraph graph, DirectorTrackAsset track, GameObject go, Playable playable,
            int port)
        {
            if (track.parentTrackIndex >= 0)
                return;

            var binding = track.GetPlayableBinding();
            var playableOutput = binding.CreateOutput(graph);
            playableOutput.SetReferenceObject(binding.sourceObject);
            playableOutput.SetSourcePlayable(playable, port);
            playableOutput.SetWeight(1.0f);

            // // only apply this on our animation track
            if (track is DirectorAnimationTrack)
            {
                (track as DirectorAnimationTrack).InitOutput(ref playableOutput);
            }
            else if (track.trackType == DirectorHelper.TrackType_Mark)
            {
                // playableOutput.SetUserData (DirectorHelper.GetDirector ());
                // INotificationReceiver receiver = DirectorHelper.singleton.globalReceiver;
                // if (receiver != null) // runtime
                // {
                //     playableOutput.AddNotificationReceiver (receiver);
                // }
            }
        }

        public override void PrepareFrame(Playable playable, FrameData frameData)
        {
            if (m_IntervalTree == null || m_LastActiveClips == null || m_CurrentActiveClips == null)
                return;

            double localTime = playable.GetTime();
            m_ActiveBit = m_ActiveBit == 0 ? 1 : 0;

            m_CurrentActiveClips.Clear();
            m_IntervalTree.IntersectsWith(DiscreteTime.GetNearestTick(localTime), m_CurrentActiveClips);

            for (int i = 0; i < m_CurrentActiveClips.Count; i++)
            {
                m_CurrentActiveClips[i].intervalBit = m_ActiveBit;
            }

            // all previously active clips having a different intervalBit flag are not
            // in the current intersection, therefore are considered becoming disabled at this frame
            for (int i = 0; i < m_LastActiveClips.Count; i++)
            {
                var c = m_LastActiveClips[i];
                if (c.intervalBit != m_ActiveBit)
                {
                    // Set time to the latest timeline time before disabling the clip.
                    c.EvaluateAt(localTime, frameData);
                    c.enable = false;
                }
            }

            m_LastActiveClips.Clear();
            // case 998642 - don't use m_ActiveClips.AddRange, as in 4.6 .Net scripting it causes GC allocs
            for (int i = 0; i < m_CurrentActiveClips.Count; i++)
            {
                m_CurrentActiveClips[i].EvaluateAt(localTime, frameData);
                m_LastActiveClips.Add(m_CurrentActiveClips[i]);
            }

            var tracks = DirectorHelper.singleton.tracks;
            if (tracks != null)
            {
                for (int i = 0; i < DirectorHelper.singleton.trackCount; i++)
                {
                    var track = tracks[i];
                    if (track is IDirectorEvaluate)
                    {
                        (track as IDirectorEvaluate).Evaluate();
                    }
                }
            }
        }
    }
}