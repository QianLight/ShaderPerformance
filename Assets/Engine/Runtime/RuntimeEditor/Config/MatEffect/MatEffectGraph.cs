#if UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CFEngine
{
    public class NodesProperty
    {
        public IList nodes;
        public SerializedProperty sp;
    }

    [Serializable]
    public class NodeIndex
    {
        public string t;
        public int index;
        public short uniqueID;

        [NonSerialized]
        public MatEffectNode node;
    }

    [Serializable]
    public class MatEffectGraphNodes
    {
        public int effectIndex = -1;
        public List<NodeIndex> nodeList = new List<NodeIndex> ();
    }

    public class MatEffectGraph : Graph
    {
        [SerializeReference]
        public List<EffectTemplate> effectTemplate = new List<EffectTemplate> ();
        //public List<ShaderParam> shaderParam = new List<ShaderParam>();
        //public List<ShaderColor> shaderColor = new List<ShaderColor>();
        //public List<ShaderTexture> shaderTexture = new List<ShaderTexture>();
        //public List<RenderActive> renderActive = new List<RenderActive>();
        //public List<MatSwitch> matSwitch = new List<MatSwitch>();
        //public List<MatLoad> matLoad = new List<MatLoad>();
        //public List<ScaleParam> scaleParam = new List<ScaleParam>();
        //public List<ShaderRT> shaderRT = new List<ShaderRT>();

        public List<MatEffectGraphNodes> templates = new List<MatEffectGraphNodes> ();
        

        [NonSerialized]
        public Dictionary<string, NodesProperty> nodeList = new Dictionary<string, NodesProperty> ();

        [NonSerialized]
        public SerializedProperty nodesSp;

        public override void Init ()
        {
            base.Init ();
            nodeList.Clear ();
            //nodes.Clear();
            //AddNode(effectTemplate);
            //AddNode(shaderParam);
            //AddNode(shaderColor);
            //AddNode(shaderTexture);
            //AddNode(renderActive);
            //AddNode(matLoad);
            //AddNode(matSwitch);
            //AddNode(scaleParam);
            //AddNode(shaderRT);
            //EditorCommon.SaveAsset(this);
            nodeList.Add(typeof(EffectTemplate).Name,
                new NodesProperty() { nodes = effectTemplate, sp = serializedGraph.FindProperty("effectTemplate") });
            nodesSp = serializedGraph.FindProperty("nodes");
            var types = EngineUtility.GetAssemblyType(typeof(AbstractNode));

            foreach (var t in types)
            {
                if (t != typeof(EffectTemplate))
                {
                    nodeList.Add(t.Name,
                        new NodesProperty() {sp = nodesSp });
                }
            }
        }

        private AbstractNode GetNextNode (AbstractNode pre)
        {
            var port = pre.GetPort ("Next");
            if (port != null && port.TotalConnections > 0)
            {
                port.HydratePorts ();
                var next = port.GetConnection (0);
                return next != null?next.node : null;
            }
            return null;
        }
        public void Save ()
        {
            templates.Clear ();
            effectTemplate.Clear();
            for(int i = 0;i<nodes.Count;++i)
            {
                if(nodes[i] is EffectTemplate)
                {
                    effectTemplate.Add(nodes[i] as EffectTemplate);
                }
            }
            for (int i = 0; i < effectTemplate.Count; ++i)
            {
                var et = effectTemplate[i];
                var next = GetNextNode(et);
                if (next != null)
                {
                    var node = new MatEffectGraphNodes()
                    {
                        effectIndex = i,
                    };
                    short nodeID = (short)et.groupID;
                    while (next != null)
                    {
                        var nodeType = next.GetType().Name;
                        int index = nodes.IndexOf(next);
                        if (index >= 0)
                        {
                            node.nodeList.Add(new NodeIndex()
                            {
                                t = nodeType,
                                index = index,
                                uniqueID = nodeID,
                            });
                            nodeID++;
                            next = GetNextNode(next);
                        }
                        else
                        {
                            break;
                        }
                    }
                    templates.Add(node);
                }                
            }
            EditorCommon.SaveAsset (this);
        }
    }
}

#endif