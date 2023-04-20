using CFEngine;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    public class ExternalHelp
    {

        public static Transform FetchAttachOfTrack (TrackAsset track)
        {
            return FetchAttachOfTrack (DirectorHelper.GetDirector (), track);
        }
        public static TrackAsset FetchTrack (PlayableDirector director, TrackAsset track)
        {
            if (track != null && director != null)
            {
                while (true)
                {
                    //some times this will be crash
                    var parent = track.parent as TrackAsset;
                    if (parent == null)
                    {
                        break;
                    }
                    else track = parent;
                }
                return track;
            }
            return null;
        }
        public static Transform FetchAttachOfTrack (PlayableDirector director, TrackAsset track)
        {
            track = FetchTrack (director, track);
            if (track != null)
            {
                var binding = director.GetGenericBinding (track);
                if (binding is Animator)
                {
                    return (binding as Animator).transform;
                }
                else if (binding is Animation)
                {
                    return (binding as Animation).transform;
                }
                else if (binding is GameObject)
                {
                    return (binding as GameObject).transform;
                }
            }
            return null;
        }

        public static TrackAsset GetRootTrack (Marker imaker)
        {
            if (imaker)
            {
                var director = DirectorHelper.GetDirector ();
                TrackAsset track = imaker.parent;
                while (track)
                {
                    if (track.parent == null ||
                        track.parent == director.playableAsset)
                    {
                        return track;
                    }
                    else
                    {
                        track = track.parent as TrackAsset;
                    }
                }
            }
            return null;
        }

        private static IInterface m_Interface;

        public static IInterface Interface
        {
            get { return m_Interface; }
            set { m_Interface = value; }
        }

        //public static GameObject CreateFromPrefab (string location, Vector3 position, Quaternion quaternion, bool usePool = true, bool dontDestroy = false)
        //{
        //    if (m_Interface != null)
        //        return m_Interface.CreateFromPrefab (location, position, quaternion, usePool, dontDestroy);
        //    else
        //    {
        //        DebugLog.AddWarningLog ("timeline interface is null");
        //        return null;
        //    }
        //}

//        public static void UnSafeDestroy (UnityEngine.GameObject o, bool returnPool = true, bool destroyImm = false)
//        {
//#if UNITY_EDITOR
//            if (!EngineContext.IsRunning)
//            {
//                if (o != null)
//                    GameObject.DestroyImmediate (o);
//            }
//            else
//#endif
//            {
//                if (m_Interface != null)
//                {
//                    m_Interface.UnSafeDestroy (o, returnPool, destroyImm);
//                }
//            }

//        }
        
    }

}