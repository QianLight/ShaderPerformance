#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CFEngine
{
    [Serializable]
    public class Connection
    {
        public string nodeId;
        public string portName;

        [NonSerialized] public Port port;
    }

    [Serializable]
    public class Port
    {
        [NonSerialized]
        public AbstractNode node;

        public Type Type
        {
            get
            {
                if (m_Type == null)
                {
                    m_Type = Type.GetType (m_SerializedType);
                }

                return m_Type;
            }
            set
            {
                m_Type = value;
                m_SerializedType = value?.AssemblyQualifiedName;
            }
        }

        public string name;
        public string fieldName;
        public bool acceptsMultipleConnections;
        public bool isInput;

        /// <summary>
        /// Enumerate all ports connected by edges to this port
        /// </summary>
        public IEnumerable<Port> Connections
        {
            get
            {
                for (var i = 0; i < m_Connections.Count; i++)
                {
                    yield return m_Connections[i].port;
                }
            }
        }

        public int TotalConnections
        {
            get
            {
                return m_Connections.Count;
            }
        }
        public Port GetConnection (int index)
        {
            return m_Connections[index].port;
        }

        [SerializeField] public List<Connection> m_Connections;
        [SerializeField] string m_SerializedType;

        Type m_Type;

        public Port ()
        {
            m_Connections = new List<Connection> ();
        }

        /// <summary>
        /// Resolve the value on this port.
        /// 
        /// If this is an input port that accepts multiple connections,
        /// only the first connection's output value will be returned.
        /// 
        /// If this is an output port, then the node's `OnRequestValue()`
        /// will be executed and best effort will be made to convert
        /// to the requested type. 
        /// </summary>
        public virtual T GetValue<T> (T defaultValue = default)
        {
            // If this is an input port, consume the  
            // value from connected port. 
            if (isInput)
            {
                HydratePorts ();
                if (m_Connections.Count > 0)
                {
                    return m_Connections[0].port.GetValue<T> ();
                }

                return defaultValue;
            }

            // Otherwise, attempt resolution from the parent node.
            object value = node.OnRequestValue (this);

            // Make sure we don't try to cast to a value type from null
            if (value == null && typeof (T).IsValueType)
            {
                throw new InvalidCastException (
                    $"Cannot cast null to value type `{typeof(T).FullName}`"
                );
            }

            // Short circuit Convert.ChangeType if we can cast quicker
            if (value == null || typeof (T).IsAssignableFrom (value.GetType ()))
            {
                return (T) value;
            }

            // Try for IConvertible support
            try
            {
                return (T) Convert.ChangeType (value, typeof (T));
            }
            catch (Exception e)
            {
                throw new InvalidCastException (
                    $"Cannot cast `{value.GetType()}` to `{typeof(T)}`. Error: {e}."
                );
            }
        }

        /// <summary>
        /// Return an enumerable list of values for this port.
        /// 
        /// If this is an input port, the output value of each connected
        /// port is aggregated into a new list of values.
        /// 
        /// If this is an output port, then the node's `OnRequestValue()`
        /// will be executed with the expectation of returning IEnumerable<T>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual IEnumerable<T> GetValues<T> ()
        {
            if (isInput)
            {
                HydratePorts ();

                var values = new T[m_Connections.Count];
                if (m_Connections.Count > 0)
                {
                    for (var i = 0; i < m_Connections.Count; i++)
                    {
                        values[i] = m_Connections[i].port.GetValue<T> ();
                    }
                }

                return values;
            }

            // Otherwise, resolve from the node.
            return (IEnumerable<T>) node.OnRequestValue (this);
        }

        internal void DisconnectAll ()
        {
            // Remove ourselves from all other connected ports
            for (var i = 0; i < m_Connections.Count; i++)
            {
                var port = m_Connections[i].port;
                port?.m_Connections.RemoveAll ((edge) => edge.port == this);
            }

            m_Connections.Clear ();
        }

        internal void Connect (Port port)
        {
            m_Connections.Add (new Connection ()
            {
                port = port,
                    nodeId = port.node.id,
                    portName = port.name
            });

            port.m_Connections.Add (new Connection ()
            {
                port = this,
                    nodeId = node.id,
                    portName = name
            });
        }

        internal void Disconnect (Port port)
        {
            m_Connections.RemoveAll ((edge) => edge.port == port);
            port.m_Connections.RemoveAll ((edge) => edge.port == this);
        }

        /// <summary>
        /// Load Port class instances from the Graph for each connection.
        /// </summary>
        /// <remarks>
        /// This is implemented as an on-demand post-deserialize
        /// operation in order to avoid serializing cyclic references
        /// </remarks>
        static HashSet<string> connects = new HashSet<string> ();
        static List<Connection> removes = new List<Connection> ();
        public void HydratePorts ()
        {
            var graph = node.graph;

            if (m_Connections != null)
            {
                connects.Clear ();
                removes.Clear ();
                for (var i = 0; i < m_Connections.Count; i++)
                {
                    var edge = m_Connections[i];
                    if (connects.Contains (edge.nodeId))
                    {
                        removes.Add (edge);
                    }
                    else
                    {
                        var connected = graph.FindNodeById (edge.nodeId);
                        if (connected != null)
                        {
                            edge.port = connected.GetPort (edge.portName);
                        }
                        else
                        {
                            removes.Add(edge);
                            DebugLog.AddErrorLog2("node connect not find:{0} {1} node:{2}", node.GetType().Name, node.name, edge.nodeId);
                        }
                    }
                }
                for (int i = 0; i < removes.Count; ++i)
                {
                    m_Connections.Remove (removes[i]);
                }
            }

        }
    }
}

#endif