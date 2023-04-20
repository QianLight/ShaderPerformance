using CFEngine;
using CFUtilPoolLib;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Timeline;

#if UNITY_EDITOR
[MarkerAttribute(TrackType.MARKER)]
[CSDiscriptor("控制背景音")]
#endif
public class TimelineBgmSignal : DirectorSignalEmmiter
{
    //背景音的控制
    public int m_pauseOrStop;
    public bool m_pause;
    public bool m_stop;
    public string m_paramStr;
    public float m_bgmVolume = 1;

    //环境音的控制
    public bool m_useBgmSettings = true; //默认都用bgm的设置

    public int m_pauseOrStopEnv;
    public bool m_pauseEnv;
    public bool m_stopEnv;
    public string m_paramStrEnv;
    public float m_environmentVolume = 1;

    public bool m_isLastSignal = false;

    private TimelineBgmData m_data;

    public TimelineBgmData GetTimelineBgmData()
    {
        if (m_data == null)
        {
            m_data = new TimelineBgmData();
            m_data.m_pauseOrStop = m_pauseOrStop;
            m_data.m_pause = m_pause;
            m_data.m_stop = m_stop;
            m_data.m_bgmVolume = m_bgmVolume;
            m_data.m_params = new Dictionary<string, float>();
            string[] strs = m_paramStr.Split('|');
            for (int i = 0; i < strs.Length; ++i)
            {
                string[] keyValue = strs[i].Split('=');
                float v = 0;
                if (keyValue.Length >= 2) float.TryParse(keyValue[1], out v);
                m_data.m_params.Add(keyValue[0], v);
            }

            m_data.m_useBgmSettings = m_useBgmSettings;
            if(!m_useBgmSettings)
            {
                m_data.m_pauseOrStopEnv = m_pauseOrStopEnv;
                m_data.m_pauseEnv = m_pauseEnv;
                m_data.m_stopEnv = m_stopEnv;
                m_data.m_environmentVolume = m_environmentVolume;
                m_data.m_paramsEnv = new Dictionary<string, float>();
                strs = m_paramStrEnv.Split('|');
                for (int i = 0; i < strs.Length; ++i)
                {
                    string[] keyValue = strs[i].Split('=');
                    float v = 0;
                    if (keyValue.Length >= 2) float.TryParse(keyValue[1], out v);
                    m_data.m_paramsEnv.Add(keyValue[0], v);
                }
            }
        }
        return m_data;
    }
}
