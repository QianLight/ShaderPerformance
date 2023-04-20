using CFEngine;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.Timeline;


[Serializable]
#if UNITY_EDITOR
[MarkerAttribute(TrackType.MARKER)]
[CSDiscriptor("视频")]
#endif
public class Mp4Signal : DirectorSignalEmmiter
{

    [SerializeField]
    string m_movie = "";
    
    public string movie
    {
        get { return m_movie; }
        set { m_movie = value; }
    }
    

    public override PropertyName id
    {
        get { return new PropertyName("Mp4Signal"); }
    }

    public override void Load(CFBinaryReader reader)
    {
        base.Load(reader);
        m_movie = reader.ReadString();
    }


#if UNITY_EDITOR
    public override void Save(BinaryWriter bw, ref SignalSaveContext context)
    {
        base.Save(bw, ref context);
        if (!context.presave)
        {
            bw.Write(movie);
        }
    }

    public override byte GetSignalType()
    {
        return RSignal.Movie_Layer;
    }

#endif
}