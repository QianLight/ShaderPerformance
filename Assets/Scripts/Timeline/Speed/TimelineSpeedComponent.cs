using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[ExecuteInEditMode]
public class TimelineSpeedComponent : MonoBehaviour
{
    public float m_speed = 1.0f;

    public static TimelineSpeedComponent m_instance;

    public void Awake()
    {
        m_instance = this; 
    }

    public void OnDestroy()
    {
        m_instance = null;
    }
}
