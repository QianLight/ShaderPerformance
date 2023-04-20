using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class CommonNode
{
    public Color m_color { get; set; }
    private Rect m_tmpRect;
    public Rect m_rect
    {
        get 
        {
            return m_tmpRect;
        }
        set
        {
            m_tmpRect = value;
        }
    }

    public bool IsVisible { get; set; }
    public int tmpDepth { get; set; }

    public Vector3 FromPos
    {
        get
        {
            return new Vector3(m_rect.xMax, m_rect.center.y);
        }
    }

    public Vector3 ToPos
    {
        get
        {
            return new Vector3(m_rect.xMin, m_rect.center.y);
        }
    }

    public string Id;
    public string Name;
    public string Desc;
    [NonSerialized]
    public List<CommonConnection> Inputs = new List<CommonConnection>();
    [NonSerialized]
    public List<CommonConnection> Outputs = new List<CommonConnection>();

    private Vector2 m_descScrollViewPos;
    public virtual void OnGUI()
    {
        GUILayout.BeginHorizontal();
        Name = GUILayout.TextField(Name);
        m_descScrollViewPos = GUILayout.BeginScrollView(m_descScrollViewPos);
        Desc = GUILayout.TextField(Desc);
        GUILayout.EndScrollView();
        GUILayout.EndHorizontal();
    }

    public CommonNode()
    {
        Id = Guid.NewGuid().ToString();
    }
}
