using CFEngine;
using System.IO;
using UnityEngine;
using UnityEngine.Timeline;

#if UNITY_EDITOR
[MarkerAttribute(TrackType.MARKER)]
[CSDiscriptor("React")]
#endif
public class ActionSignal : DirectorSignalEmmiter
{
    [SerializeField]
    private int m_statisticID;

    [SerializeField]
    private string m_anim;

    public int StatisticID
    {
        get { return m_statisticID; }
        set { m_statisticID = value; }
    }

    public string Anim
    {
        get { return m_anim; }
        set { m_anim = value; }
    }

    public override PropertyName id
    {
        get { return new PropertyName("ActionSignal"); }
    }

    public override void Load(CFBinaryReader reader)
    {
        base.Load(reader);
        m_statisticID = reader.ReadInt32();
        m_anim = reader.ReadString();
    }

#if UNITY_EDITOR
    public override void Save(BinaryWriter bw, ref SignalSaveContext context)
    {
        base.Save(bw, ref context);
        if (!context.presave)
        {
            bw.Write(m_statisticID);
            bw.Write(m_anim);
        }
    }

    public override byte GetSignalType()
    {
        return RSignal.ActionType_Layer;
    }

#endif
}
