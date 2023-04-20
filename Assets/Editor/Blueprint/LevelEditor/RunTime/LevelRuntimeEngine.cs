using System;
using System.Collections.Generic;
using BluePrint;
using UnityEditor;
using System.Reflection;

namespace LevelEditor
{
    public enum SimulatorState
    {
        Run,
        Pause,
        Stop,
    }

    public class LevelRuntimeEngine : IBlueprintRuntimeEngine
    {
        public LevelEditorData levelFullData;
        public SimulatorState state { get; set; }

        private double _lastRecordTime = float.MinValue;

        private List<BlueprintRuntimeGraph> Graphs = new List<BlueprintRuntimeGraph>();

        public void Reset()
        {
            foreach (var g in Graphs) g.Reset();
            Graphs.Clear();

            state = SimulatorState.Stop;
        }

        private void BuildNode(LevelGraphData data, string dataname, string nodename, string listName, BlueprintRuntimeGraph g)
        {
            Type n1 = Type.GetType(nodename);
            Type t1 = Type.GetType(dataname);
            object[] list = new object[] { data.GetType().GetField(listName).GetValue(data), g };
            MethodInfo mi = this.GetType().GetMethod("BuildNodeByDataT").MakeGenericMethod(new Type[] { n1, t1 });
            mi.Invoke(this, list);
        }

        public void BuildNodeByDataT<N, T>(List<T> list, BlueprintRuntimeGraph g)
            where T : BluePrintNodeBaseData, new()
            where N : BlueprintRuntimeDataNode<T>, new()
        {
            for (int i = 0; i < list.Count; ++i)
            {
                N node = new N();
                node.SetGraph(g);
                node.Init(list[i]);
                g.AddNode(node);
            }
        }


        public void Build(string configPath)
        {
            Reset();
            
            levelFullData = DataIO.DeserializeData<LevelEditorData>(configPath);

            for(int k = 0; k < levelFullData.GraphDataList.Count; ++k)
            {
                LevelGraphData graphData = levelFullData.GraphDataList[k];
                BlueprintRuntimeGraph g = new BlueprintRuntimeGraph(this);
                g.Init(graphData.graphID);

                for (int i = 0; i < graphData.VariantDefine.Count; ++i)
                {
                    g.VarManager.AddVariant(graphData.VariantDefine[i].VariableName, graphData.VariantDefine[i].VarType);
                }

                BuildNode(graphData, "LevelEditor.LevelWaveData", "LevelEditor.LevelRTWaveNode", "WaveData", g);
                BuildNode(graphData, "LevelEditor.LevelMonitorData", "LevelEditor.LevelRTMonitorNode", "MonitorData", g);

                for (int i = 0; i < graphData.ControllerData.Count; ++i)
                {
                    CommonRTControllerNode node = CommonRTControllerNode.CreateControllerNode(graphData.ControllerData[i].TypeID, g);
                    node.SetGraph(g);
                    node.Init(graphData.ControllerData[i]);
                    g.AddNode(node);
                }

                BuildNode(graphData, "LevelEditor.LevelExstringData", "LevelEditor.LevelRTExstringNode", "ExstringData", g);
                BuildNode(graphData, "LevelEditor.LevelScriptData", "LevelEditor.LevelRTScriptNode", "ScriptData", g);
                BuildNode(graphData, "BluePrint.BluePrintTimerData", "BluePrint.CommonRTTimerNode", "TimerData", g);
                BuildNode(graphData, "BluePrint.BluePrintVariantData", "BluePrint.CommonRTVariableGetNode", "VarGetData", g);
                BuildNode(graphData, "BluePrint.BluePrintVariantData", "BluePrint.CommonRTVariableSetNode", "VarSetData", g);
                BuildNode(graphData, "BluePrint.ExtGetPlayerPositionData", "BluePrint.ExtRTNodeGetPlayerPosition", "GetPlayerPositionData", g);
                BuildNode(graphData, "BluePrint.BluePrintNodeBaseData", "BluePrint.ExtRTNodeGetPlayerHP", "GetPlayerHpData", g);
                BuildNode(graphData, "BluePrint.ExtGetPartnerAttrData", "BluePrint.ExtRTNodeGetPartnerAttr", "GetPartnerAttrData", g);
                BuildNode(graphData, "BluePrint.ExtInRangeData", "BluePrint.ExtRTNodeInRange", "InRangeData", g);
                BuildNode(graphData, "LevelEditor.LevelVictoryData", "LevelEditor.LevelRTVictoryNode", "VictoryData", g);
                BuildNode(graphData, "LevelEditor.LevelFailData", "LevelEditor.LevelRTFailNode", "FailData", g);
                BuildNode(graphData, "BluePrint.BluePrintSubGraphData", "BluePrint.BlueprintRuntimeSubGraphNode", "SubGraphData", g);
                BuildNode(graphData, "LevelEditor.LevelRandomNodeData", "LevelEditor.LevelRTRandomNode", "RandomData", g);
                BuildNode(graphData, "LevelEditor.LevelRecordData", "LevelEditor.LevelRTRecordNode", "RecordData", g);
                BuildNode(graphData, "LevelEditor.LevelVarData", "LevelEditor.LevelRTGetExternalVarNode", "GetGlobalData", g);
                BuildNode(graphData, "LevelEditor.LevelVarData", "LevelEditor.LevelRTSetExternalVarNode", "SetGlobalData", g);
                BuildNode(graphData, "LevelEditor.LevelSwitchPartnerData", "LevelEditor.LevelRTSwitchPartnerNode", "SwitchPartnerData", g);
                BuildNode(graphData, "LevelEditor.LevelTemproryPartnerData", "LevelEditor.LevelTemproryPartnerNode", "TemproryPartnerData", g);

                for (int i = 0; i < graphData.ConnectionData.Count; ++i)
                {
                    BlueprintRuntimeBaseNode sNode = g.FindNode(graphData.ConnectionData[i].StartNode);
                    BlueprintRuntimeBaseNode eNode = g.FindNode(graphData.ConnectionData[i].EndNode);

                    if (sNode != null && eNode != null)
                    {
                        BlueprintRuntimePin sPin = sNode.GetPin(graphData.ConnectionData[i].StartPin);
                        BlueprintRuntimePin ePin = eNode.GetPin(graphData.ConnectionData[i].EndPin);

                        if (sPin != null && ePin != null)
                            sNode.ConnectPin(sPin, ePin);
                    }
                }

                for (int i = 0; i < graphData.pinData.Count; ++i)
                {
                    BlueprintRuntimeBaseNode node = g.FindNode(graphData.pinData[i].nodeID);

                    if (node != null)
                    {
                        BlueprintRuntimePin pin = node.GetPin(graphData.pinData[i].pinID);
                        if (pin != null && pin is BlueprintRuntimeValuedPin)
                        {
                            BPVariant bpv = new BPVariant();
                            bpv.type = VariantType.Var_Float;
                            bpv.val._float = graphData.pinData[i].defaultValue;
                            (pin as BlueprintRuntimeValuedPin).SetDefaultValue(bpv);
                        }
                    }
                }

                g.AfterBuild();
                Graphs.Add(g);
            }
        }

        public BlueprintRuntimeGraph GetGraph(int GraphID)
        {
            for(int i = 0; i < Graphs.Count; ++i)
            {
                if (Graphs[i].GraphID == GraphID) return Graphs[i];
            }

            return null;
        }

        public BlueprintRuntimeGraph GetMainGraph()
        {
            return GetGraph(1);
        }

        public bool IsRunning()
        { return state == SimulatorState.Run; }

        public bool IsPausing()
        { return state == SimulatorState.Pause; }

        public bool IsStopping()
        { return state == SimulatorState.Stop; }


        public void StartSimulation()
        {
            if (state == SimulatorState.Stop)
            {
                state = SimulatorState.Run;
                _lastRecordTime = EditorApplication.timeSinceStartup;

                BlueprintRuntimeGraph mainGraph = GetGraph(1);
                mainGraph.Run(null);
            }
        }

        public void EndSimulation()
        {
            for (int i = 0; i < Graphs.Count; ++i)
                Graphs[i].Stop();

            state = SimulatorState.Stop;

            Reset();
        }

        public void PauseSimulation()
        {
            if (state == SimulatorState.Pause)
            {
                _lastRecordTime = EditorApplication.timeSinceStartup;
                state = SimulatorState.Run;
            }
            else if (state == SimulatorState.Run)
                state = SimulatorState.Pause;
        }

        public void Update()
        {
            if (state != SimulatorState.Run) return;
            //if (runtimeGraph != null)
            {
                double deltaT = EditorApplication.timeSinceStartup - _lastRecordTime;

                for (int i = 0; i < Graphs.Count; ++i)
                    Graphs[i].Update((float)deltaT);

                _lastRecordTime = EditorApplication.timeSinceStartup;
            }

        }

        
    }
}
