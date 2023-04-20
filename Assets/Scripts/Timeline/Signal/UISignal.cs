using System;
using System.IO;
using CFEngine;
using UnityEngine;
using UnityEngine.Timeline;

public enum UISign
{
    TEAM_OPEN1,
    TEAM_OPEN2,
    CARD_WANTED,
    CARD_PROPERTY,
    CARD_SSR,
    DRAMA_MAP,
    CONTROL_UI,
    CHAPTER_START,
    BOUNTY_SHOW,
    BlackIn,
    BlackOut,
}

#if UNITY_EDITOR
[Serializable]
[CSDiscriptor("程序节点")]
[MarkerAttribute (TrackType.MARKER)]
#endif
public class UISignal : DirectorSignalEmmiter
{
    [SerializeField] public UISign m_sign;
    [SerializeField] public string m_arg;
    [SerializeField] public float m_duration = 3;
    public UISign uiSign
    {
        get { return m_sign; }
        set { m_sign = value; }
    }

    public override PropertyName id
    {
        get { return new PropertyName ("UISignal"); }
    }

    public override void Load (CFBinaryReader reader)
    {
        base.Load (reader);
        m_sign = (UISign) reader.ReadByte ();
    }
#if UNITY_EDITOR
    public override void Save (BinaryWriter bw, ref SignalSaveContext context)
    {
        base.Save (bw, ref context);
        if(!context.presave)
        bw.Write ((byte) m_sign);
    }

    public override byte GetSignalType ()
    {
        return RSignal.SignalType_UI;
    }
#endif

}