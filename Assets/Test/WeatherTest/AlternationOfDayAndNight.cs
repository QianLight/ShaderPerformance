using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlternationOfDayAndNight : MonoBehaviour
{
    //设置水平线日出日落的时间
    static int m_sunriseHour = 6;//6点日出
    static int m_sunsetHour = 20;//20点日落

    static float m_targetTime, m_needSecond;
    static float _currentTime;

    [SerializeField]
    Light _sunLight;
      

    static bool _timeRun;
    static float _oldTime;
    static float _timer;
    void Start()
    {
        
    }
    void Update()
    {
        if (_timeRun)
        {
            UpdateTimeOfDay();
            RotateSunLight();
        }
    }


    /// <summary>
    /// 调用
    /// </summary>
    /// <param name="targetTime">目标时间24小时制</param>
    /// <param name="needSecond">所需时间</param>    
    public static void SetTime(float targetTime, float needSecond)
    {
        if (_currentTime != targetTime && targetTime <= 24)
        {
            _timeRun = true;
            _timer = 0;
            if (targetTime < _currentTime)
            {
                targetTime -= _currentTime;
                if (targetTime < 0)
                {
                    m_targetTime = targetTime + 24 + _currentTime;
                }
            }
            else
            {
                m_targetTime = targetTime;
            }
            _oldTime = _currentTime;
            m_needSecond = needSecond;         
        }
    }

    private void UpdateTimeOfDay()
    {
        _timer += Time.deltaTime;
        _currentTime = Mathf.Lerp(_oldTime, m_targetTime, _timer / m_needSecond);
     
        if (_currentTime >= 24)
        {
            _currentTime = 0;
            m_targetTime -= 24;
            _oldTime = 0;
        }

        if (_timer - m_needSecond >= 0.01f)
        {
            _currentTime = m_targetTime;
            _timeRun = false;
            _timer = 0;
        }  
    }

    private void RotateSunLight()
    {
        float sunLightRotation;
        if (_currentTime > m_sunriseHour && _currentTime < m_sunsetHour)
        {
            float sunriseToSunsetDuration = CalculateTimeDifference(m_sunriseHour, m_sunsetHour);
            float timSinceSunrise = CalculateTimeDifference(m_sunriseHour, _currentTime);

            sunLightRotation = Mathf.Lerp(0, 180, timSinceSunrise / sunriseToSunsetDuration);
        }
        else
        {
            float sunsetToSunriseDuration = CalculateTimeDifference(m_sunsetHour, m_sunriseHour);
            float timeSinceSunset = CalculateTimeDifference(m_sunsetHour, _currentTime);
                     
            sunLightRotation = Mathf.Lerp(180, 360, timeSinceSunset / sunsetToSunriseDuration);
        }
        _sunLight.transform.rotation = Quaternion.AngleAxis(sunLightRotation, Vector3.right);
    }

    private float CalculateTimeDifference(float fromTime, float toTime)
    {
        float diff = toTime - fromTime;
        if (diff < 0)
        {
            diff += 24;
        }
        return diff;
    }
}
