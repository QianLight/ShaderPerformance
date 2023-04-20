using CFEngine;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Timeline;

[Serializable]
#if UNITY_EDITOR
[CustomStyle("AudioEmmiter")]
[MarkerAttribute(TrackType.MARKER)]
[CSDiscriptor("声音已废弃")]
#endif
public class FModSignal : DirectorSignalEmmiter
{

    [SerializeField] string m_clip = "";

    [SerializeField] string m_key = "";

    [SerializeField] float m_param = 0;

    public string clip
    {
        get { return m_clip; }
        set { m_clip = value; }
    }

    public string key
    {
        get { return m_key; }
        set { m_key = value; }
    }

    public float param
    {
        get { return m_param; }
        set { m_param = value; }
    }

    public override PropertyName id
    {
        get { return new PropertyName("FModSignal"); }
    }

    public override void Load(CFBinaryReader reader)
    {
        base.Load(reader);
        m_clip = reader.ReadString();
        m_key = reader.ReadString();
        m_param = reader.ReadSingle();
    }

#if UNITY_EDITOR
    public override void Save(BinaryWriter bw, ref SignalSaveContext context)
    {
        base.Save(bw, ref context);
        if (!context.presave)
        {
            bw.Write(m_clip);
            bw.Write(m_key);
            bw.Write(m_param);
        }
    }

    public override byte GetSignalType()
    {
        return RSignal.FMod_Layer;
    }

#endif

}