//using System;
//using System.IO;
//using CFEngine;
//using UnityEngine;
//using UnityEngine.Timeline;


////#if UNITY_EDITOR
////[Serializable]
////[CSDiscriptor("QTE")]
////[MarkerAttribute(TrackType.MARKER)]
////#endif

//public class QteSignal : DirectorSignalEmmiter
//{
//    public bool m_enable = true;                //是否开启
//    public Vector2 m_position;                  //UI的位置
//    public QTEType m_qteType;                   //QTE的类型
//    public float m_speed;                       //播放速度
//    public float m_signalDuration;              //慢/快放时长
//    public float m_whiteToBlackDuration;        //白到黑时间
//    public float m_keepBlackDuration;           //黑屏保持时间
//    public float m_BlackToWhiteDuration;        //黑到白时间
//    public bool m_missJump;                     //未点击，跳转到指定时间
//    public float m_jumpTime;                    //跳转到指定时间
//    public float m_missDuration;                //失败持续时间
//    public float m_succeedDuration;             //成功持续时间
//    public float m_fullStateDuration;           //达到100%时的持续时间
//    public float m_addPercent;                  //单次点击增加百分比
//    public float m_decayPercent;                //每秒衰减百分比
//    public string m_clickEvent;                 //点击音效
//    public string m_succeedEvent;               //成功的音效
//    public string m_iconName;                   //按钮的背景图

//    public float BlackTotalDuration
//    {
//        get
//        {
//            return (m_whiteToBlackDuration + m_keepBlackDuration + m_whiteToBlackDuration);
//        }
//    }
//}

//public enum QTEType
//{
//    None,
//    Click,
//    ContinueClick,
//}