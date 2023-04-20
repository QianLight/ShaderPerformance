using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TimelineQteRotateFollow : MonoBehaviour
{
    public bool m_update = false;
    public bool m_clockwise = true;
    public int m_rotateTimes = 1;
    public float m_currentAngle = 0;
    public float m_startAngle = 0;
    public float m_endAngle = 0;
    public float m_speed = 0;
    public float m_radius = 0;
    public RectTransform m_center;
    public RectTransform m_target;
    public float m_count = 0;

    public void Init(float startAngle, float endAngle, float speed)
    {
        m_currentAngle = startAngle;
        m_startAngle = startAngle;
        m_endAngle = endAngle;
        m_speed = speed;
        m_count = 0;
    }

    public void StartRotate()
    {
        m_update = true;
    }

    public void StopRotate()
    {
        m_update = false;
    }

    public void Update()
    {
        if (!m_update || m_center == null || m_target == null) return;

        if (m_clockwise)
        {
            m_currentAngle -= Time.deltaTime * m_speed;
            if (m_currentAngle < m_endAngle)
            {
                m_currentAngle = m_startAngle;
                m_count++;
            }
        }
        else
        {
            m_currentAngle += Time.deltaTime * m_speed;
            if (m_currentAngle > m_endAngle)
            {
                m_currentAngle = m_startAngle;
                m_count++;
            }
        }

        float rad = Mathf.Deg2Rad * (m_currentAngle);
        float x = m_radius * Mathf.Cos(rad);
        float y = m_radius * Mathf.Sin(rad);
        m_target.anchoredPosition = m_center.anchoredPosition + new Vector2(x, y);

        if(m_count == m_rotateTimes)
        {
            StopRotate();
        }
    }

    public void SetAngle(float angle)
    {
        float rad = Mathf.Deg2Rad * (angle);
        float x = m_radius * Mathf.Cos(rad);
        float y = m_radius * Mathf.Sin(rad);
        m_target.anchoredPosition = m_center.anchoredPosition + new Vector2(x, y);
    }
}
