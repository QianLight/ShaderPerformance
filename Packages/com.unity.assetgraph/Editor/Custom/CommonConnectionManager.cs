using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonConnectionManager<TNode> where TNode:CommonNode, new ()
{
    private static CommonConnectionManager<TNode> m_instance = new CommonConnectionManager<TNode>();
    public static CommonConnectionManager<TNode> Instance
    {
        get
        {
            return m_instance;
        }
    }

    private HashSet<CommonConnection> m_connections = new HashSet<CommonConnection>();
    public void Reset()
    {
        m_connections.Clear();
    }

    public CommonConnection Add(TNode from, TNode to)
    {
        for(var i = 0; i <  from.Outputs.Count; i++)
        {
            if(from.Outputs[i].m_to == to.Id)
            {
                Debug.Log($"connection from {from.Name} to {to.Name} already exist");
                return null;
            }
        }
        var connection = new CommonConnection(from, to);
        m_connections.Add(connection);
        return connection;
    }

    public CommonConnection Connect(TNode start, TNode end)
    {
        return Add(start, end);
    }

    public void Add(CommonConnection connection)
    {
        m_connections.Add(connection);
    }

    public void GetAllConnection(List<CommonConnection> result)
    {
        foreach (var connection in m_connections)
        {
            result.Add(connection);
        }
    }

    public void GetConnectionsByFrom(string from, List<CommonConnection> result)
    {
        foreach (var connection in m_connections)
        {
            if (connection.m_from.Equals(from))
            {
                result.Add(connection);
            }
        }
    }

    public void GetConnectionsByTo(string to, List<CommonConnection> result)
    {
        foreach (var connection in m_connections)
        {
            if (connection.m_to.Equals(to))
            {
                result.Add(connection);
            }
        }
    }

    public void Remove(CommonConnection connection)
    {
        m_connections.Remove(connection);
    }
}
