using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    public float m_colck,m_setpTime;
    public int m_volumeIndex;
    private void Start()
    {
        //进入游戏时起始时间
        SetTime(7, 0.1f);
        SetVolume(0, 0.1f);
    }
    [ContextMenu("StartRain")]
    public void StartRain()
    {
        RainController.StartRain();
    }
    [ContextMenu("StopRain")]
    public void StopRain()
    {
        RainController.StopRain();
    }
    [ContextMenu("TestSetTime")]
    public void TestSetTime()
    {
        SetTime(m_colck, m_setpTime);
    }
    [ContextMenu("TestSetVolume")]
    public void TestSetVolume()
    {
        SetVolume(m_volumeIndex, m_setpTime);
    }
    [ContextMenu("TestSetTimeAndVolume")]
    public void TestSetTimeAndVolume()
    {
        SetTime(m_colck, m_setpTime);
        SetVolume(m_volumeIndex, m_setpTime);
    }

    /// <summary>
    /// 太阳的位移
    /// </summary>
    /// <param name="clock">时间</param>
    /// <param name="stepTime">过渡时间</param>
    public void SetTime(float clock,float stepTime)
    {
        AlternationOfDayAndNight.SetTime(clock, stepTime);
    }

    /// <summary>
    /// Volume的改变
    /// </summary>
    /// <param name="volumeIndex">volume索引</param>
    /// <param name="stepTime">过渡时间</param>
    public void SetVolume(int volumeIndex,float stepTime)
    {
        VolumeBlend.SetProfile(volumeIndex, stepTime);
    }
}
