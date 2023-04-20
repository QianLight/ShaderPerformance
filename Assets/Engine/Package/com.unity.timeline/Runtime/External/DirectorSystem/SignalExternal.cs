#if UNITY_EDITOR
using System.IO;

namespace UnityEngine.Timeline
{
    public struct SignalSaveContext
    {
        public bool presave;
        public uint flag;
    }

    partial class Marker
    {
        [System.NonSerialized]
        public byte trackIndex;
        public virtual void Save (BinaryWriter bw, ref SignalSaveContext context)
        {
            if (!context.presave)
            {
                bw.Write (trackIndex);
                bw.Write (time);
                bw.Write (context.flag);
            }

        }

        public virtual byte GetSignalType ()
        {
            return 255;
        }
    }

#if UNITY_EDITOR
    [CSDiscriptor("显隐")]
#endif
    partial class ActiveSignalEmmiter
    {
        public override void Save (BinaryWriter bw, ref SignalSaveContext context)
        {
            context.flag |= retroactive?DirectorSignalEmmiter.Flag_Retroactive : 0;
            context.flag |= emitOnce?DirectorSignalEmmiter.Flag_EmitOnce : 0;
            context.flag |= Active?DirectorSignalEmmiter.Flag_Active : 0;
            base.Save (bw, ref context);
        }
        public override byte GetSignalType ()
        {
            return DirectorSignalEmmiter.SignalType_Active;
        }
    }

#if UNITY_EDITOR
    [CSDiscriptor("跳转")]
#endif
    partial class JumpSignalEmmiter
    {
        public override void Save (BinaryWriter bw, ref SignalSaveContext context)
        {
            context.flag |= emitOnce?DirectorSignalEmmiter.Flag_EmitOnce : 0;
            base.Save (bw, ref context);
            if (!context.presave)
                bw.Write (jumpTime);
        }
        public override byte GetSignalType ()
        {
            return DirectorSignalEmmiter.SignalType_Jump;
        }
    }

#if UNITY_EDITOR
    [CSDiscriptor("速度")]
#endif
    partial class SlowSignalEmitter
    {
        public override void Save (BinaryWriter bw, ref SignalSaveContext context)
        {
            context.flag |= retroactive?DirectorSignalEmmiter.Flag_Retroactive : 0;
            context.flag |= emitOnce?DirectorSignalEmmiter.Flag_EmitOnce : 0;
            base.Save (bw, ref context);
            if (!context.presave)
                bw.Write (slowRate);
        }
        public override byte GetSignalType ()
        {
            return DirectorSignalEmmiter.SignalType_Slow;
        }
    }
}
#endif