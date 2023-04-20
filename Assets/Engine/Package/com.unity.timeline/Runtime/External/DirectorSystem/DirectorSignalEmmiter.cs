using CFEngine;
using UnityEngine.Playables;

namespace UnityEngine.Timeline
{
    public class DirectorSignalEmmiter:
// #if UNITY_EDITOR
        Marker,
// #endif
        INotificationOptionProvider, INotification, ISharedObject
        {
// #if UNITY_EDITOR
            [System.NonSerialized]
// #endif
            public DirectorTrackAsset parentTrack;
            private double t;
// #if UNITY_EDITOR
            [System.NonSerialized]
// #endif
            public float jumpTime;
// #if UNITY_EDITOR
            [System.NonSerialized]
// #endif
            public float slowRate;
// #if UNITY_EDITOR
            [System.NonSerialized]
// #endif
            public byte signalType = 255;
            private uint flag = 0;
            public static uint Flag_Retroactive = 0x00000001;
            public static uint Flag_EmitOnce = 0x00000002;
            public static uint Flag_Active = 0x00000004;
            public static byte SignalType_Active = 0;
            public static byte SignalType_Jump = (byte) (SignalType_Active + 1);
            public static byte SignalType_Slow = (byte) (SignalType_Jump + 1);
            public static byte EngineSignalCount = (byte)(100);

            public DirectorSignalEmmiter ()
            {

            }
            public double T
            {
                get { return t; }
            }

            public void SetFlag (uint f, bool add)
            {
                if (add)
                {
                    flag |= f;
                }
                else
                {
                    flag &= ~(f);
                }
            }

            public bool HasFlag (uint f)
            {
                return (flag & f) != 0;
            }

            public virtual void Reset ()
            {
                flag = 0;
            }

            public virtual PropertyName id
            {
                get { return new PropertyName ("DummySignal"); }
            }

            NotificationFlags INotificationOptionProvider.flags
            {
                get
                {
                    return (HasFlag (Flag_Retroactive) ? NotificationFlags.Retroactive : default (NotificationFlags)) |
                        (HasFlag (Flag_EmitOnce) ? NotificationFlags.TriggerOnce : default (NotificationFlags)) |
                        NotificationFlags.TriggerInEditMode;
                }
            }

            public virtual void Load (CFBinaryReader reader)
            {                
                byte trackIndex = reader.ReadByte ();
                var tracks = DirectorHelper.singleton.tracks;
                if (trackIndex < tracks.Length)
                    parentTrack = tracks[trackIndex];
                t = reader.ReadDouble ();
                flag = reader.ReadUInt32 ();
                if (signalType == SignalType_Jump)
                {
                    jumpTime = reader.ReadSingle ();
                }
                else if (signalType == SignalType_Slow)
                {
                    slowRate = reader.ReadSingle ();
                }
            }
        }

}