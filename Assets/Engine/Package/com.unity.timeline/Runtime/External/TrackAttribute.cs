using System;

namespace UnityEngine.Timeline
{

    [AttributeUsage(AttributeTargets.Class)]
    public class TrackAttribute : Attribute
    {
        public bool onlyInSub;


        public TrackAttribute(bool onlySub)
        {
            onlyInSub = onlySub;
        }

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class CSDiscriptor : Attribute
    {
        public string discriptor;

        public string classify;

        public CSDiscriptor(string disc, string clas = "")
        {
            discriptor = disc;
            classify = clas;
        }
    }


    public enum TrackType
    {
        NONE = 0,
        MARKER = 1,
        ANIMTION = 1 << 1,
        CONTROL = 1 << 2,
        ANCHOR = 1 << 3,
        PLAYABLE = 1 << 4,
        OTHER = 1 << 7 // put other at last
    }


    [AttributeUsage(AttributeTargets.Class)]

    public class MarkerAttribute : Attribute
    {
        public TrackType supportType = default(TrackType);

        public MarkerAttribute(bool onlyMarker)
        {
            if (onlyMarker)
                supportType |= TrackType.MARKER;
        }


        public MarkerAttribute(TrackType type)
        {
            supportType = type;
        }

        public void AddType(TrackType type)
        {
            supportType |= type;
        }

        public bool SupportTrackType(TrackAsset track, Type type)
        {
            bool rst = false;
            if (track is MarkerTrack)
            {
                rst = (supportType & TrackType.MARKER) > 0;
            }
            else if (track is AnimationTrack)
            {
                rst = (supportType & TrackType.ANIMTION) > 0;
            }
            else if (track is ControlTrack)
            {
                rst = (supportType & TrackType.CONTROL) > 0;
            }
            else if (track is PlayableTrack)
            {
                rst = (supportType & TrackType.PLAYABLE) > 0;
            }
            return rst;
        }

    }

}
