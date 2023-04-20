using System.Collections.Generic;

public class CommonNodeManager<TNode> where TNode:CommonNode, new ()
{
    private static CommonNodeManager<TNode> m_instance = new CommonNodeManager<TNode>();
    public static CommonNodeManager<TNode> Instance
    {
        get
        {
            return m_instance;
        }
    }

    public void Reset()
    {
        m_configNodes.Clear();
    }

    Dictionary<string, TNode> m_configNodes = new Dictionary<string, TNode>();

    public void Add(TNode node)
    {
        m_configNodes[node.Id] = node;
    }

    public void GetAllNodes(List<TNode> result)
    {
        foreach (var pair in m_configNodes)
        {
            result.Add(pair.Value);
        }
    }

    public TNode GetNode(string nodeId)
    {
        if (!m_configNodes.TryGetValue(nodeId, out var configNode))
        {
            return null;
        }
        return configNode;
    }

    public TNode CreateNode(string name)
    {
        var node = new TNode
        {
            Name = name,
        };
        m_configNodes.Add(node.Id, node);
        return node;
    }
}

