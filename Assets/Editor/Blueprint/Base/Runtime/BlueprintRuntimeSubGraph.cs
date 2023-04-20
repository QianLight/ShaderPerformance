using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BluePrint
{
    public class BlueprintRuntimeSubGraphNode : BlueprintRuntimeDataNode<BluePrintSubGraphData>
    {
        public override void Init(BluePrintSubGraphData data, bool AutoStreamPin = true)
        {
            base.Init(data, AutoStreamPin);
        }

        public override void Execute(BlueprintRuntimePin activePin)
        {
            base.Execute(activePin);

            int graphID = HostData.GraphID;

            BlueprintRuntimeGraph subGraph = parentGraph.Engine.GetGraph(graphID);
            subGraph.Run(this);
        }

        public void ExecuteNext()
        {
            if (pinOut != null) pinOut.Active();
        }
    }
}