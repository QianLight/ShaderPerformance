using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

namespace UnityEditor.Timeline
{
    public delegate void AddTrackContextMenu (GenericMenu menu, TrackAsset track);
    public class CommonTrackDrawer
    {
        public static AddTrackContextMenu cb;

        public static void AddSubTrack (GenericMenu menu, TrackAsset track)
        {
            if (cb != null)
            {
                if (!track.isSubTrack)
                {
                    menu.AddSeparator (string.Empty);
                    if (track.lockedInHierarchy || TimelineWindow.instance.state.editSequence.isReadOnly) { }
                    else
                    {
                        cb (menu, track);
                    }
                }
            }

        }
        public static void AddSubTrack (Type trackOfType, string trackName, TrackAsset track)
        {
            TimelineHelpers.CreateTrack (trackOfType, track, trackName);
        }
    }
}