using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICommand
{
    bool Do();
    bool Undo();
}

public abstract class BaseCommand<TNode> : ICommand where TNode: CommonNode, new()
{
    protected CommonNodeGraph<TNode> m_graph;

    public BaseCommand (CommonNodeGraph<TNode> graph)
    {
        m_graph = graph; 
    }
    public abstract bool Do();
    public abstract bool Undo();
}

public class AddNodeCommand<TNode> : BaseCommand<TNode> where TNode: CommonNode, new()
{
    private TNode m_node;
    public AddNodeCommand(CommonNodeGraph<TNode> graph) : base(graph) { }

    public override bool Do()
    {
        m_node = CommonNodeManager<TNode>.Instance.CreateNode("New");
        m_graph.AddNewNode(m_node);
        return true;
    }

    public override bool Undo()
    {
        m_graph.DeleteNodeInGraph(m_node);
        return true;
    }
}

public class DeleteNodeCommand<TNode> : BaseCommand<TNode> where TNode: CommonNode, new()
{
    private TNode m_node;
    private List<CommonConnection> m_connections = new List<CommonConnection>();
    public DeleteNodeCommand(CommonNodeGraph<TNode> graph, TNode node) : base(graph) 
    {
        m_node = node;
    }

    public override bool Do()
    {
        m_connections.Clear();
        m_graph.DeleteNodeInGraph(m_node);
        foreach(var input in m_node.Inputs)
        {
            m_graph.RemoveConnection(input);
            m_connections.Add(input);
        }
        foreach(var output in m_node.Outputs)
        {
            m_graph.RemoveConnection(output);
            m_connections.Add(output);
        }
        return true;
    }

    public override bool Undo()
    {
        m_graph.AddNode(m_node);
        foreach(var connection in m_connections)
        {
            m_graph.AddInitedConnection(connection);
        }
        return true;
    }
}

public class CopyNodeCommand<TNode> : BaseCommand<TNode> where TNode: CommonNode, new()
{

    public CopyNodeCommand(CommonNodeGraph<TNode> graph) : base(graph) { }

    public override bool Do()
    {
        return true;
    }

    public override bool Undo()
    {
        return true;
    }
}

public class PasteNodeCommand<TNode> : BaseCommand<TNode> where TNode: CommonNode, new()
{

    public PasteNodeCommand(CommonNodeGraph<TNode> graph) : base(graph) { }

    public override bool Do()
    {
        return true;
    }

    public override bool Undo()
    {
        return true;
    }
}

public class AddConnectionCommand<TNode> : BaseCommand<TNode> where TNode:CommonNode, new()
{
    private CommonConnection m_connection;
    public AddConnectionCommand(CommonNodeGraph<TNode> graph, TNode from, TNode to) : base(graph)
    {
        var exist = false;
        foreach (var output in from.Outputs)
        {
            if (output.m_to == to.Id)
            {
                exist = true;
            }
        }

        if (!exist)
        {
            m_connection = CommonConnectionManager<TNode>.Instance.Add(from, to);
            m_connection.InitNodes(from, to);
        }
    }
    public override bool Do()
    {
        if(null == m_connection)
        {
            return false;
        }
        m_graph.AddInitedConnection(m_connection);
        return true;
    }

    public override bool Undo()
    {
        m_graph.RemoveConnection(m_connection);
        return true;
    }
}


public class DeleteConnectionCommand<TNode> : BaseCommand<TNode> where TNode:CommonNode, new()
{
    private CommonConnection m_connection;
    public DeleteConnectionCommand(CommonNodeGraph<TNode> graph, CommonConnection connection) : base(graph) 
    {
        m_connection = connection;  
    }

    public override bool Do()
    {
        m_graph.RemoveConnection(m_connection);
        return true;
    }

    public override bool Undo()
    {
        m_graph.AddInitedConnection(m_connection);
        return true;
    } 
}
