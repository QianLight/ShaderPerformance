using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BluePrint
{
    public enum BlueprintErrorCode
    {
        Invalid,
        None,
        Warning,
        NodeDataError,
        DataTypeError,
        NoStart,
        NoReturn,
    }

    public struct BlueprintErrorData
    {
        public BlueprintErrorCode ErrorCode;
        public string Desc;
        public BlueprintConnection ErrorConnection;

        public BlueprintErrorData(BlueprintErrorCode c, string message, BlueprintConnection conn)
        {
            ErrorCode = c;
            Desc = message;
            ErrorConnection = conn;
        }
    }

    public class BlueprintGraphErrorInfo
    {
        public int GraphID;
        public string GraphName;
        public List<BlueprintErrorData> ErrorDataList = new List<BlueprintErrorData>();

        public void Reset()
        {
            GraphID = 0;
            ErrorDataList.Clear();
        }

        public bool HasError
        {
            get { return ErrorDataList.Count != 0; }
        }
    }

    public class BlueprintNodeErrorInfo
    {
        public string graphName;
        public int nodeID;
        public List<BlueprintErrorData> ErrorDataList = new List<BlueprintErrorData>();

        public void Reset()
        {
            nodeID = 0;
            ErrorDataList.Clear();
        }

        public bool HasError
        {
            get { return ErrorDataList.Count != 0; }
        }
    }

    public class BlueprintErrorInfoList
    {
        public List<BlueprintGraphErrorInfo> _graph_error_list = new List<BlueprintGraphErrorInfo>();

        public List<BlueprintNodeErrorInfo> _node_error_list = new List<BlueprintNodeErrorInfo>();

        public void Clear()
        {
            _graph_error_list.Clear();
            _node_error_list.Clear();
        }

        public void AddGraphError(BlueprintGraphErrorInfo info)
        {
            _graph_error_list.Add(info);
        }

        public void AddNodeError(BlueprintNodeErrorInfo info)
        {
            _node_error_list.Add(info);
        }
    }

    class ErrorCheckManager
    {
        private static BlueprintErrorInfoList ErrorInfos = new BlueprintErrorInfoList();

        public static BlueprintErrorInfoList CheckError(BluePrintGraph graph)
        {
            ErrorInfos.Clear();

            int startNode = 0;
            int endNode = 0;
            if (graph.GraphID > 1)
            {
                foreach (BluePrintNode tmp in graph.widgetList)
                {
                    if (tmp is ControllerSubGraphStartNode)
                        startNode += 1;

                    if (tmp is ControllerSubGraphEndNode)
                        endNode += 1;
                }

                BlueprintGraphErrorInfo ge = new BlueprintGraphErrorInfo();
                ge.GraphID = graph.GraphID;
                ge.GraphName = graph.GraphName;

                if(startNode != 1)
                {
                    BlueprintErrorData d = new BlueprintErrorData();
                    d.ErrorCode = BlueprintErrorCode.NoStart;
                    d.Desc = "开始节点不存在或者多于一个";
                    ge.ErrorDataList.Add(d);
                }

                if(endNode <= 0)
                {
                    BlueprintErrorData d = new BlueprintErrorData();
                    d.ErrorCode = BlueprintErrorCode.NoReturn;
                    d.Desc = "没有结束节点";
                    ge.ErrorDataList.Add(d);
                }

                if (ge.HasError) ErrorInfos.AddGraphError(ge);
            }
            else
            {
                 foreach (BluePrintNode tmp in graph.widgetList)
                {
                    if (tmp is ControllerStartNode)
                        startNode += 1;
                }

                if (startNode != 1)
                {
                    BlueprintGraphErrorInfo ge = new BlueprintGraphErrorInfo();
                    ge.GraphID = graph.GraphID;
                    ge.GraphName = graph.GraphName;
                    BlueprintErrorData d = new BlueprintErrorData();
                    d.ErrorCode = BlueprintErrorCode.NoStart;
                    d.Desc = "开始节点不存在或者多于一个";
                    ge.ErrorDataList.Add(d);
                    ErrorInfos.AddGraphError(ge);
                }
            }

            foreach (BluePrintNode node in graph.widgetList)
            {
                // single node check
                node.CheckError();
                if (node.HasCompileError())
                {
                    ErrorInfos.AddNodeError(node.nodeErrorInfo);
                    
                }

                // data type check
                for (int j = 0; j < node.pinList.Count; ++j)
                {
                    BluePrintValuedPin startPin = node.pinList[j] as BluePrintValuedPin;
                    if (startPin != null)
                    {
                        foreach (BlueprintConnection conn in node.pinList[j].connections)
                        {
                            BluePrintNode eNode = conn.connectEnd.GetNode<BluePrintNode>();

                            if(eNode != null)
                            {
                                BluePrintPin ePin = eNode.GetPin(conn.connectEnd.pinID);
                                BluePrintValuedPin endPin = ePin as BluePrintValuedPin;

                                if(endPin == null)
                                {
                                    BlueprintErrorData error = new BlueprintErrorData();
                                    error.ErrorCode = BlueprintErrorCode.DataTypeError;
                                    AddError(node.Root.GraphName, node.nodeEditorData.NodeID, error);
                                }
                                else
                                {
                                    if(!PinTypeSame(startPin, endPin))
                                    {
                                        BlueprintErrorData error = new BlueprintErrorData();
                                        error.ErrorCode = BlueprintErrorCode.DataTypeError;
                                        error.ErrorConnection = conn;
                                        AddError(node.Root.GraphName, node.nodeEditorData.NodeID, error);
                                    }
                                }
                            }
                        }
                    }
                }

            }

            return ErrorInfos;
        }

        private static bool PinTypeSame(BluePrintValuedPin sPin, BluePrintValuedPin ePin)
        {
            VariantType startType = sPin.dataType;
            VariantType endType = ePin.dataType;

            if(startType == VariantType.Var_Custom)
                startType = sPin.GetRealType();
 
            if (endType == VariantType.Var_Custom)
                endType = ePin.GetRealType();

            return startType == endType;
        }



        private static void AddError(string graphName, int nodeID, BlueprintErrorData errorData)
        {
            for(int i = 0; i < ErrorInfos._node_error_list.Count; ++i)
            {
                if(ErrorInfos._node_error_list[i].nodeID == nodeID)
                {
                    ErrorInfos._node_error_list[i].ErrorDataList.Add(errorData);
                    return;
                }
            }

            BlueprintNodeErrorInfo errorInfo = new BlueprintNodeErrorInfo();
            errorInfo.graphName = graphName;
            errorInfo.nodeID = nodeID;
            errorInfo.ErrorDataList.Add(errorData);
            ErrorInfos._node_error_list.Add(errorInfo);

        }
    }
}
