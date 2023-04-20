//using System;
//using System.IO;
//using CFEngine;
//using UnityEngine;
//using UnityEngine.Timeline;

////暂时屏蔽，未用到
////[Serializable]
////#if UNITY_EDITOR
////[CustomStyle("QteEmitter")]
////[CSDiscriptor("长按","Add Qte")]
////[MarkerAttribute(TrackType.MARKER)]
////#endif
//public class QteLongPressSignal : QteSignal
//{
//    [SerializeField] float m_waitTime;
//    [SerializeField] float m_pressTime;
//    [SerializeField] float m_pressTimeout;
//    [SerializeField] float m_waitTo;
//    [SerializeField] float m_failedTo;
//    [SerializeField] float m_backwardRate;
//    [SerializeField] string m_audio;

//    public float waitTime
//    {
//        get { return m_waitTime; }
//        set { m_waitTime = value; }
//    }

//    public float PressTime
//    {
//        get { return m_pressTime; }
//        set { m_pressTime = value; }
//    }

//    public float PressTimeout
//    {
//        get { return m_pressTimeout; }
//        set { m_pressTimeout = value; }
//    }

//    public float waitTo
//    {
//        get { return m_waitTo; }
//        set { m_waitTo = value; }
//    }

//    public float failedTo
//    {
//        get { return m_failedTo; }
//        set { m_failedTo = value; }
//    }

//    public float backwardRate
//    {
//        get { return m_backwardRate; }
//        set { m_backwardRate = value; }
//    }

//    public string Audio
//    {
//        get { return m_audio; }
//        set { m_audio = value; }
//    }

//    public override PropertyName id
//    {
//        get { return new PropertyName("QteLongPressSignal"); }
//    }

//    public override void Load(CFBinaryReader reader)
//    {
//        base.Load(reader);
//        m_waitTime = reader.ReadSingle();
//        m_pressTime = reader.ReadSingle();
//        m_pressTimeout = reader.ReadSingle();
//        m_failedTo = reader.ReadSingle();
//        m_waitTo = reader.ReadSingle();
//        m_backwardRate = reader.ReadSingle();
//        m_audio = reader.ReadString();
//    }

//#if UNITY_EDITOR
//    public override void Save(BinaryWriter bw, ref SignalSaveContext context)
//    {
//        base.Save(bw, ref context);
//        if (!context.presave)
//        {
//            bw.Write(m_waitTime);
//            bw.Write(m_pressTime);
//            bw.Write(m_pressTimeout);
//            bw.Write(m_failedTo);
//            bw.Write(m_waitTo);
//            bw.Write(m_backwardRate);
//            bw.Write(m_audio);
//        }
//    }

//    public override byte GetSignalType()
//    {
//        return RSignal.SignalType_LongPress;
//    }
//#endif
//}