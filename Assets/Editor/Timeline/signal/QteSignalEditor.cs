//using UnityEditor;
//using UnityEditor.Timeline;
//using UnityEngine.Playables;
//using UnityEngine.Timeline;
//using System.Collections.Generic;



//[CustomEditor(typeof(QteSignal))]
//public class QteSignalEditor : Editor
//{
//    private QteSignal m_signal;
//    private SerializedObject m_so;

//    private SerializedProperty m_time;

//    public SerializedProperty m_enable;                     //是否开启
//    public SerializedProperty m_position;                   //UI的位置
//    public SerializedProperty m_qteType;                    //QTE的类型
//    public SerializedProperty m_speed;                      //播放速度
//    public SerializedProperty m_signalDuration;             //慢/快放时长
//    public SerializedProperty m_whiteToBlackDuration;       //白到黑时间
//    public SerializedProperty m_keepBlackDuration;          //黑屏保持时间
//    public SerializedProperty m_BlackToWhiteDuration;       //黑到白时间
//    public SerializedProperty m_missJump;                   //未点击，跳转到指定时间
//    public SerializedProperty m_jumpTime;                   //跳转到指定时间
//    public SerializedProperty m_missDuration;               //失败持续时间
//    public SerializedProperty m_succeedDuration;            //成功持续时间
//    public SerializedProperty m_fullStateDuration;          //达到100%时的持续时间
//    public SerializedProperty m_addPercent;                 //单次点击增加百分比
//    public SerializedProperty m_decayPercent;               //每秒衰减百分比
//    public SerializedProperty m_clickEvent;                 //点击音效
//    public SerializedProperty m_succeedEvent;               //成功的音效
//    public SerializedProperty m_iconName;                   //按钮的背景图

//    private void OnEnable()
//    {
//        m_signal = target as QteSignal;
//        m_so = new SerializedObject(m_signal);

//        m_time = m_so.FindProperty("m_Time");

//        m_enable = m_so.FindProperty("m_enable");
//        m_position = m_so.FindProperty("m_position");
//        m_qteType = m_so.FindProperty("m_qteType");
//        m_speed = m_so.FindProperty("m_speed");
//        m_signalDuration = m_so.FindProperty("m_signalDuration");
//        m_whiteToBlackDuration = m_so.FindProperty("m_whiteToBlackDuration");
//        m_keepBlackDuration = m_so.FindProperty("m_keepBlackDuration");
//        m_BlackToWhiteDuration = m_so.FindProperty("m_BlackToWhiteDuration");
//        m_missJump = m_so.FindProperty("m_missJump");
//        m_jumpTime = m_so.FindProperty("m_jumpTime");
//        m_missDuration = m_so.FindProperty("m_missDuration");
//        m_succeedDuration = m_so.FindProperty("m_succeedDuration");
//        m_fullStateDuration = m_so.FindProperty("m_fullStateDuration");
//        m_addPercent = m_so.FindProperty("m_addPercent");
//        m_decayPercent = m_so.FindProperty("m_decayPercent");
//        m_clickEvent = m_so.FindProperty("m_clickEvent");
//        m_succeedEvent = m_so.FindProperty("m_succeedEvent");
//        m_iconName = m_so.FindProperty("m_iconName");
//    }

//    public override void OnInspectorGUI()
//    {
//        EditorGUILayout.PropertyField(m_time, true);
//        EditorGUILayout.PropertyField(m_enable, true);
//        EditorGUILayout.PropertyField(m_position, true);
//        EditorGUILayout.PropertyField(m_qteType, true);
//        EditorGUILayout.PropertyField(m_speed, true);
//        EditorGUILayout.PropertyField(m_signalDuration, true);
//        EditorGUILayout.PropertyField(m_whiteToBlackDuration, true);
//        EditorGUILayout.PropertyField(m_keepBlackDuration, true);
//        EditorGUILayout.PropertyField(m_BlackToWhiteDuration, true);
//        EditorGUILayout.PropertyField(m_missJump, true);
//        EditorGUILayout.PropertyField(m_jumpTime, true);
//        EditorGUILayout.PropertyField(m_missDuration, true);
//        EditorGUILayout.PropertyField(m_succeedDuration, true);
//        EditorGUILayout.PropertyField(m_fullStateDuration, true);
//        if (m_signal.m_qteType == QTEType.ContinueClick)
//        {
//            EditorGUILayout.PropertyField(m_addPercent, true);
//            EditorGUILayout.PropertyField(m_decayPercent, true);
//        }
//        EditorGUILayout.PropertyField(m_clickEvent, true);
//        EditorGUILayout.PropertyField(m_succeedEvent, true);
//        EditorGUILayout.PropertyField(m_iconName, true);

//        m_so.ApplyModifiedProperties();
//        EditorUtility.SetDirty(m_signal);
//    }
//}