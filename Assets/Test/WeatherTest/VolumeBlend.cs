using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
[ExecuteInEditMode]
public class VolumeBlend : MonoBehaviour
{
    [SerializeField]
    VolumeProfile[] m_profiles;
    [SerializeField]
    Volume m_firstVolume,m_secondVolume;
    static bool _change;
    static float _timer;
    static float _changeTime;
    static int _currentIndex;
    static int _profilesLengh;
    static bool _firstOnce;

    private void Awake()
    {
        _profilesLengh = m_profiles.Length;
        m_firstVolume.weight = 0.01f;
        m_secondVolume.weight = 1;
    }
   
    void Update()
    {
        if (_change)
        {
            if(_firstOnce)
            {
                m_secondVolume.profile = m_profiles[_currentIndex];
                m_firstVolume.weight = 1;
                _firstOnce = false;
            }
            _timer += Time.deltaTime; 
            m_firstVolume.weight = Mathf.Lerp(1f, 0f, _timer / _changeTime);
            
            //这里低于0.01会出现闪烁，故设置成0.02
            if (m_firstVolume.weight <= 0.02f)
            {               
                _change = false;
                m_firstVolume.weight = 0.01f;
                m_firstVolume.profile = m_secondVolume.profile;
            }
        }        
    }

    /// <summary>
    /// 调用
    /// </summary>
    /// <param name="index">profile的索引</param>
    /// <param name="needSecond">变化时间</param>
   public static void SetProfile(int index,float needSecond)
    {
        if (_currentIndex == index || index >= _profilesLengh)
        {
            return;
        }
        _firstOnce = true;
        _changeTime = needSecond;   
        _currentIndex = index;
        _change = true;
        _timer = 0;        
    }  
}
