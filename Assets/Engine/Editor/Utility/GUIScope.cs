using System;
using UnityEngine;

public class GUIColorScope : IDisposable
{
    private bool m_Enabled;
    private Color m_Color;
    
    public GUIColorScope(Color color, bool enabled = true)
    {
        m_Enabled = enabled;
        if (enabled)
        {
            m_Color = GUI.color;
            GUI.color = color;
        }
    }

    public void Dispose()
    {
        if (m_Enabled)
        {
            GUI.color = m_Color;
        }
    }
}