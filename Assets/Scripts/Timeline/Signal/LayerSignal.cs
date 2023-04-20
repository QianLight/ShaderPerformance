using CFEngine;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

#if UNITY_EDITOR
[MarkerAttribute(TrackType.MARKER)]
[CSDiscriptor("摄像机层级")]
#endif
public class LayerSignal : DirectorSignalEmmiter
{

    [SerializeField]
    private int m_layerMask;
    
    [SerializeField]
    private bool m_bIgnorelayerStateStack;
    
    [SerializeField]
    private bool m_bForceEnableRender;
    public int layerMask
    {
        get { return m_layerMask; }
        set { m_layerMask = value; }
    }
    
    public bool BIgnorelayerStateStack
    {
        get { return m_bIgnorelayerStateStack; }
        set { m_bIgnorelayerStateStack = value; }
    }
    
    public bool BForceEnableRender
    {
        get { return m_bForceEnableRender; }
        set { m_bForceEnableRender = value; }
    }

    public override PropertyName id
    {
        get { return new PropertyName("LayerSignal"); }
    }

    public override void Load(CFBinaryReader reader)
    {
        base.Load(reader);
        m_layerMask = reader.ReadInt32();
        m_bIgnorelayerStateStack = reader.ReadBoolean();
        m_bForceEnableRender = reader.ReadBoolean();
    }

#if UNITY_EDITOR
    public override void Save(BinaryWriter bw, ref SignalSaveContext context)
    {
        base.Save(bw, ref context);
        if (!context.presave)
        {
            bw.Write(m_layerMask);
            bw.Write(m_bIgnorelayerStateStack);
            bw.Write(m_bForceEnableRender);
        }
    }

    public override byte GetSignalType()
    {
        return RSignal.SignalType_Layer;
    }

#endif

}
