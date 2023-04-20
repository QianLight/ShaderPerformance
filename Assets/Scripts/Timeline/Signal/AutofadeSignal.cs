using CFEngine;
using System.IO;
using UnityEngine;
using UnityEngine.Timeline;

#if UNITY_EDITOR
[MarkerAttribute(TrackType.MARKER)]
[CSDiscriptor("渐隐")]
#endif
public class AutofadeSignal : DirectorSignalEmmiter
{
    [SerializeField]
    private float m_out;

    [SerializeField]
    private float m_free;

    [SerializeField]
    private float m_in;

    public float  Free
    {
        get {return m_free;}
        set { m_free = value;}
    }

    public float Out
    {
        get { return m_out; }
        set { m_out = value; }
    }

    public float In
    {
        get { return m_in; }
        set { m_in = value; }
    }

    public override PropertyName id
    {
        get { return new PropertyName("AutofadeSignal"); }
    }

    public override void Load(CFBinaryReader reader)
    {
        base.Load(reader);
        m_out = reader.ReadSingle();
        m_in = reader.ReadSingle();
        m_free = reader.ReadSingle();
    }

#if UNITY_EDITOR
    public override void Save(BinaryWriter bw, ref SignalSaveContext context)
    {
        base.Save(bw, ref context);
        if (!context.presave)
        {
            bw.Write(m_out);
            bw.Write(m_in);
            bw.Write(m_free);
        }
    }

    public override byte GetSignalType()
    {
        return RSignal.AutoFade_Layer;
    }

#endif
}
