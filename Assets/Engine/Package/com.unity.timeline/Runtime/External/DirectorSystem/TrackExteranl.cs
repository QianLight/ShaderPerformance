using System;

#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using CFEngine;
using UnityEditor;
using UnityEngine.Playables;
#endif

namespace UnityEngine.Timeline
{
#if UNITY_EDITOR
    public struct TrackSaveContext
    {
        public uint flag;
        public bool presave;
    }
    partial class TrackAsset
    {
        [NonSerialized]
        public DirectorTrackAsset runtimeTrack;
        [NonSerialized]
        public short clipStart;
        [NonSerialized]
        public short clipEnd;
        // [NonSerialized]
        // public short signalStart;
        // [NonSerialized]
        // public short signalEnd;
        public virtual void BindTrack ()
        {
            var director = DirectorHelper.GetDirector ();
            if (director != null && runtimeTrack != null)
            {
                runtimeTrack.BindTransform = ExternalHelp.FetchAttachOfTrack (director, this);
            }
        }

        public virtual byte GetTrackType ()
        {
            return 255;
        }

        public string GetStreamName ()
        {
            var o = outputs;
            var it = o.GetEnumerator ();
            while (it.MoveNext ())
            {
                return it.Current.streamName;
            }
            return "";
        }
        public virtual void Save (BinaryWriter bw, ref TrackSaveContext context)
        {
            if (context.presave)
            {
                string sn = GetStreamName ();
                DirectorHelper.SaveStringIndex (bw, sn, true);
            }
            else
            {
                context.flag |= IsCompilable () ? DirectorTrackAsset.Flag_HasClips : 0;
                context.flag |= muted?DirectorTrackAsset.Flag_Mute : 0;
                //context.flag |= CanCompileNotifications () ? DirectorTrackAsset.Flag_HasNotifications : 0;
                bw.Write (context.flag);
                float s = (float) start;
                bw.Write (s);
                float e = (float) end;
                bw.Write (e);
                //clips
                bw.Write (clipStart);
                bw.Write (clipEnd);
                // bw.Write (signalStart);
                // bw.Write (signalEnd);

                var o = outputs;
                var it = o.GetEnumerator ();
                string sn = null;
                while (it.MoveNext ())
                {
                    sn = it.Current.streamName;
                    break;
                }
                DirectorHelper.SaveStringIndex (bw, sn, false);
            }

        }

    }

    partial class AnimationTrack
    {
        public override byte GetTrackType ()
        {
            return DirectorHelper.TrackType_Animation;
        }

        public override void Save (BinaryWriter bw, ref TrackSaveContext context)
        {
            if (context.presave)
            {
                base.Save (bw, ref context);
                DirectorHelper.SaveAsset (bw, avatarMask, ".mask", true);
            }
            else
            {
                context.flag |= AnimatesRootTransform () ? DirectorAnimationTrack.Flag_RootTransform : 0;
                context.flag |= DirectorAnimationTrack.Flag_BindObject;
                base.Save (bw, ref context);

                DirectorHelper.SaveVector (bw, ref m_Position);
                DirectorHelper.SaveVector (bw, ref m_EulerAngles);

                bw.Write ((byte) m_TrackOffset);

                DirectorHelper.SaveAsset (bw, avatarMask, ".mask", false);
            }

        }

    }

    partial class ActivationTrack
    {
        public override byte GetTrackType ()
        {
            return DirectorHelper.TrackType_Active;
        }

        public override void Save (BinaryWriter bw, ref TrackSaveContext context)
        {
            base.Save (bw, ref context);
            bw.Write ((byte) m_PostPlaybackState);
        }
    }

    partial class ControlTrack
    {
#if UNITY_EDITOR
        public List<Transform> fxPool = new List<Transform>();
#endif

        public override byte GetTrackType ()
        {
            return DirectorHelper.TrackType_Control;
        }
        public override void Save (BinaryWriter bw, ref TrackSaveContext context)
        {
            context.flag |= DirectorTrackAsset.Flag_NeedBind;
            base.Save (bw, ref context);
        }
    }

    partial class PlayableTrack
    {
        public override byte GetTrackType ()
        {
            return DirectorHelper.TrackType_Playable;
        }
    }
    
    partial class MarkerTrack
    {
        public override byte GetTrackType ()
        {
            return DirectorHelper.TrackType_Mark;
        }

        public override void Save (BinaryWriter bw, ref TrackSaveContext context)
        {
            context.flag |= DirectorTrackAsset.Flag_HasNotifications;
            base.Save (bw, ref context);
        }
    }
    partial class AnimClipTrack : TrackAsset
    {
        public override byte GetTrackType ()
        {
            return DirectorHelper.TrackType_AnimClip;
        }
        public override void Save (BinaryWriter bw, ref TrackSaveContext context)
        {
            context.flag |= DirectorTrackAsset.Flag_HasClips;
            context.flag |= DirectorTrackAsset.Flag_NeedBind;            
            base.Save (bw, ref context);
        }
    }
#endif
}