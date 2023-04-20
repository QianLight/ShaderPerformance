using System;
using System.IO;
using CFEngine;
using UnityEngine;
using UnityEngine.Timeline;

#if UNITY_EDITOR
[Serializable]
[CustomStyle("QteEmitter")]
[MarkerAttribute (TrackType.MARKER)]
[CSDiscriptor("速度线")]
#endif
public class SpeedLineSignal : DirectorSignalEmmiter
{
    [SerializeField] public int m_Type;
    [SerializeField] public float m_EndTime;


    public override PropertyName id
    {
        get { return new PropertyName("SpeedLineSignal"); }
    }

    public override void Load (CFBinaryReader reader)
    {
        base.Load (reader);
        m_Type = reader.ReadInt32();
        m_EndTime = reader.ReadSingle();
    }
#if UNITY_EDITOR
    public override void Save (BinaryWriter bw, ref SignalSaveContext context)
    {
        base.Save (bw, ref context);
        if (!context.presave)
        {
            bw.Write(m_Type);
            bw.Write(m_EndTime);
        }
    }

    public override byte GetSignalType ()
    {
        return RSignal.SignalType_SpeedLine;
    }
#endif
}