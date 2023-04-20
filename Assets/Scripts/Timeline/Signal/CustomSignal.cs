using System;
using System.IO;
using CFEngine;
using UnityEngine;
using UnityEngine.Timeline;


/*
* 不带参数的signal都放在这儿
*/


/// <summary>
/// 不要随便改枚举值的顺序， 新的就往后加
/// </summary>
public enum SignalType
{
    Capture,
}


[Serializable]
#if UNITY_EDITOR
[CSDiscriptor("通用")]
#endif
public class CustomSignal : DirectorSignalEmmiter
{

    [SerializeField] protected SignalType m_type;

    public SignalType type
    {
        get { return m_type; }
        set { m_type = value; }
    }

    public override PropertyName id
    {
        get { return new PropertyName("CustomSignal"); }
    }

    public override void Load(CFBinaryReader reader)
    {
        base.Load(reader);
        m_type = (SignalType)reader.ReadByte();
    }

#if UNITY_EDITOR
    public override void Save(BinaryWriter bw, ref SignalSaveContext context)
    {
        base.Save(bw, ref context);
        if (!context.presave)
            bw.Write((byte)m_type);
    }

    public override byte GetSignalType()
    {
        return RSignal.SignalType_Custom;
    }
#endif
}
