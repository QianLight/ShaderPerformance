//using System;
//using System.Collections.Generic;
//using System.IO;
//using CFEngine;
//using UnityEngine;
//using UnityEngine.Playables;
//using UnityEngine.Timeline;

////暂时屏蔽，未用到
////[Serializable]
////#if UNITY_EDITOR
////[CustomStyle ("QteEmitter")]
////[CSDiscriptor("连点","Add Qte")]
////[MarkerAttribute (TrackType.MARKER)]
////#endif
//public class QteContinuousSignal : QteSignal
//#if UNITY_EDITOR
//    , ITimelineAsset
//#endif
//{
//    public static int maxCount = 16;
//    [SerializeField] int[] m_fxbyCnt;
//    private byte m_fxByCount;
//    // #if UNITY_EDITOR

//    [SerializeField] string[] m_fxPath;
//    // #endif
//    private short fxPathStart = 0;
//    [SerializeField] float m_timeout;

//    [SerializeField] float m_duration;

//    public int[] FxByCnt
//    {
//        get { return m_fxbyCnt; }
//        set { m_fxbyCnt = value; }
//    }
//    public int FxByCntLength
//    {
//        get
//        {
//            // #if UNITY_EDITOR
//            //if (!EngineContext.IsRunning)
//            {
//                return m_fxbyCnt != null ? m_fxbyCnt.Length : 0;
//            }
//            // #endif
//            // return m_fxByCount;
//        }
//    }
//    // #if UNITY_EDITOR
//    public string[] FxPath
//    {
//        get { return m_fxPath; }
//        set { m_fxPath = value; }
//    }
//    // #endif
//    public float TimeOut
//    {
//        get { return m_timeout; }
//        set { m_timeout = value; }
//    }

//    public float Duration
//    {
//        get { return m_duration; }
//        set { m_duration = value; }
//    }

//    public override PropertyName id
//    {
//        get { return new PropertyName ("QteContinuousSignal"); }
//    }

//    public string GetFxPath (int i)
//    {
//        if (i >= 0 && m_fxPath != null && i < m_fxPath.Length)
//        {
//            return m_fxPath[i];
//        }

//        // if (fxPathStart >= 0)
//        // {
//        //     return DirectorHelper.singleton.GetStr(fxPathStart + i);
//        // }
//        return null;
//    }
//    public override void Load (CFBinaryReader reader)
//    {
//        m_timeout = reader.ReadSingle ();
//        m_duration = reader.ReadSingle ();
//        if (m_fxbyCnt == null)
//        {
//            m_fxbyCnt = new int[maxCount];
//        }
//        byte fxByCount = reader.ReadByte ();
//        for (int i = 0; i < fxByCount && i < maxCount; ++i)
//        {
//            m_fxbyCnt[i] = reader.ReadByte ();;
//        }
//    }

//#if UNITY_EDITOR
//    public override void Save (BinaryWriter bw, ref SignalSaveContext context)
//    {
//        base.Save (bw, ref context);
//        if (context.presave)
//        {
//            byte fxCount = (byte) (m_fxPath != null ? m_fxPath.Length : 0);
//            fxPathStart = -1;
//            for (int i = 0; i < fxCount; ++i)
//            {
//                short index = DirectorHelper.SaveStringIndex (bw, m_fxPath[i], true);
//                if (i == 0)
//                {
//                    fxPathStart = index;
//                }
//            }
//        }
//        else
//        {
//            bw.Write (m_timeout);
//            bw.Write (m_duration);
//            byte fxByCount = (byte) (m_fxbyCnt != null ? m_fxbyCnt.Length : 0);
//            bw.Write (fxByCount);
//            for (int i = 0; i < fxByCount; ++i)
//            {
//                byte c = (byte) m_fxbyCnt[i];
//                bw.Write (c);
//            }
//            bw.Write (fxPathStart);
//        }

//    }

//    public override byte GetSignalType ()
//    {
//        return RSignal.SignalType_QteContinuous;
//    }

//    PlayableAssetType ITimelineAsset.assetType
//    {
//        get { return PlayableAssetType.PREFAB; }
//    }

//    List<string> ITimelineAsset.ReferenceAssets (PlayableBinding pb)
//    {
//        List<string> list = new List<string> ();
//        string path = "";
//        for (int i = 0; i < FxPath.Length; i++)
//        {
//            path = GetFxPath (i);
//            if (!string.IsNullOrEmpty (path))
//            {
//                list.Add (path);
//            }
//        }
//        return list;
//    }

//#endif

//}