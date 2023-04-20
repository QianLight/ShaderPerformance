using System;
using System.Collections.Generic;
using System.Linq;

namespace BluePrint
{
    public static class StackExtensions
    {
        public static Stack<T> Clone<T>(this Stack<T> original)
        {
            var arr = new T[original.Count];
            original.CopyTo(arr, 0);
            Array.Reverse(arr);
            return new Stack<T>(arr);
        }
    }

    public class BlueprintRuntimeGraph
    {
        public int GraphID {get;set;}

        public IBlueprintRuntimeEngine Engine { get; set; }

        public Stack<CommonRTControllerNode> JumpNodeStack = new Stack<CommonRTControllerNode>();

        protected List<BlueprintRuntimeBaseNode> NodeList = new List<BlueprintRuntimeBaseNode>();

        public Stack<BlueprintRuntimeSubGraphNode> CallStack;

        public BlueprintRuntimeVariantManager VarManager;

        public BlueprintRuntimeGraph(IBlueprintRuntimeEngine e)
        {
            Engine = e;
        }

        public void Init(int GraphID)
        {
            this.GraphID = GraphID;
            VarManager = new BlueprintRuntimeVariantManager(this);
        }

        public void Reset()
        {
            JumpNodeStack.Clear();
            if(CallStack != null) CallStack.Clear();
            VarManager.Clear();
            NodeList = new List<BlueprintRuntimeBaseNode>();
        }

        public bool Run(BlueprintRuntimeSubGraphNode parentNode)
        {
            if (parentNode == null)
                CallStack = new Stack<BlueprintRuntimeSubGraphNode>();
            else
            {
                CallStack = parentNode.parentGraph.CallStack.Clone();
                CallStack.Push(parentNode);
            }

            BlueprintRuntimeBaseNode flyNode = FindFlyNode();
            if(flyNode != null)
            {
                flyNode.Execute(null);
            }

            BlueprintRuntimeBaseNode startNode = FindStartNode();
            if (startNode != null)
            {
                startNode.Execute(null);
                return true;
            }

            return false;
        }

        public void AfterBuild()
        {
            for (int i = 0; i < NodeList.Count; ++i)
            {
                NodeList[i].AfterBuild();
            }
        }

        public void Stop()
        {
            for (int i = 0; i < NodeList.Count; ++i)
            {
                NodeList[i].OnEndSimulation();
            }
        }


        public void Update(float deltaT)
        {
            for (int i = 0; i < NodeList.Count; ++i)
            {
                NodeList[i].Update(deltaT);
            }
        }

        public void PushJumpNode(CommonRTControllerNode node)
        {
            JumpNodeStack.Push(node);
        }

        public CommonRTControllerNode PeekJumpNode()
        {
            if (JumpNodeStack.Count > 0)
                return JumpNodeStack.Peek();
            return null;
        }

        public CommonRTControllerNode PopJumpNode()
        {
            return JumpNodeStack.Pop();
        }

        public void AddNode(BlueprintRuntimeBaseNode node)
        {
            NodeList.Add(node);
        }

        public BlueprintRuntimeBaseNode FindNode(int nodeID)
        {
            for (int i = 0; i < NodeList.Count; ++i)
            {
                if (NodeList[i].NodeID == nodeID) return NodeList[i];
            }

            return null;
        }

        public BlueprintRuntimePin FindPin(int nodeID, int pinID)
        {
            for (int i = 0; i < NodeList.Count; ++i)
            {
                if (NodeList[i].NodeID == nodeID)
                {
                    return NodeList[i].GetPin(pinID);
                }
            }

            return null;
        }

        public BlueprintRuntimeBaseNode FindStartNode()
        {
            for (int i = 0; i < NodeList.Count; ++i)
            {
                if (NodeList[i] is CommonRTControllerStartNode) return NodeList[i];
                if (NodeList[i] is CommonRTControllerSubGraphStartNode) return NodeList[i];
            }
            return null;
        }

        public BlueprintRuntimeBaseNode FindFlyNode()
        {
            for (int i = 0; i < NodeList.Count; ++i)
            {
                if (NodeList[i] is CommonRTControllerFlyNode) return NodeList[i];
            }
            return null;
        }
    }
}
