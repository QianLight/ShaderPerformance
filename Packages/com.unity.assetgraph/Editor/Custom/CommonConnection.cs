using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CommonConnection
{
    private CommonNode FromNode;
    private CommonNode ToNode;
    public string m_from;
    public string m_to;
    public HashSet<string> Datas = new HashSet<string>();
    public Rect m_rect { get; set; }

    private CommonConnection() { }
    public CommonConnection(CommonNode from, CommonNode to)
    {
        FromNode = from;
        ToNode = to;
        m_from = from.Id;
        m_to = to.Id;
    }

    public void InitNodes(CommonNode from, CommonNode to)
    {
        for (var i = 0; i < from.Outputs.Count; i++)
        {
            if (from.Outputs[i].m_to == to.Id)
            {
                Debug.LogError($"connection from {from.Name} to {to.Name} already exist");
            }
        }
        for (var i = 0; i < to.Inputs.Count; i++)
        {
            if (to.Inputs[i].m_from == from.Id)
            {
                Debug.LogError($"connection from {from.Name} to {to.Name} already exist");
            }
        }
        from.Outputs.Add(this);
        to.Inputs.Add(this);
        FromNode = from;
        ToNode = to;
    }

    public override string ToString()
    {
        if (null != FromNode && null != ToNode)
        {
            return $"connection {FromNode.Name}->{ToNode.Name}";
        }
        else
        {
            return $"connection {m_from}->{m_to}";
        }
    }
}
