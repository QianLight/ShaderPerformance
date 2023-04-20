using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BluePrint;

namespace BluePrint
{
    public class BlueprintRuntimeBaseNode
    {
        public int NodeID = 0;
        public List<BlueprintRuntimePin> PinList = new List<BlueprintRuntimePin>();

        public BlueprintRuntimePin pinIn;
        public BlueprintRuntimePin pinOut;

        public BlueprintRuntimeGraph parentGraph;

        public bool Executed { get; set; }

        public BlueprintRuntimeBaseNode() { }

        public BlueprintRuntimeBaseNode(BlueprintRuntimeGraph e)
        {
            parentGraph = e;
        }


        public void SetGraph(BlueprintRuntimeGraph e)
        {
            parentGraph = e;
        }

        public virtual void Init(bool AutoStreamPin = true)
        {
            if (AutoStreamPin)
            {
                pinIn = new BlueprintRuntimePin(this, -1, PinType.Main, PinStream.In);
                pinOut = new BlueprintRuntimePin(this, -2, PinType.Main, PinStream.Out);
                AddPin(pinIn);
                AddPin(pinOut);
            }
        }
        public virtual void Execute(BlueprintRuntimePin activePin)
        {
            SetExecuted(true);
        }

        public virtual void Update(float deltaT) { }

        public void AddPin(BlueprintRuntimePin pin)
        {
            PinList.Add(pin);
        }

        public BlueprintRuntimePin GetPin(int pinID)
        {
            for (int i = 0; i < PinList.Count; ++i)
            {
                if (PinList[i].pinID == pinID)
                    return PinList[i];
            }

            return null;
        }

        public void SetExecuted(bool bEnabled)
        {
            Executed = bEnabled;
        }

        public BlueprintRuntimeConnection ConnectPin(BlueprintRuntimePin start, BlueprintRuntimePin end)
        {
            BlueprintRuntimeConnection conn = new BlueprintRuntimeConnection(start, end);
            start.AddConnection(conn);

            BlueprintRuntimeConnection reverseConnection = new BlueprintRuntimeConnection(end, start);
            end.AddReverseConnection(reverseConnection);

            return conn;
        }

        public virtual void OnEndSimulation()  { }
        public virtual void AfterBuild() { }
    }


    public class BlueprintRuntimeDataNode<T> : BlueprintRuntimeBaseNode where T : BluePrintNodeBaseData, new()
    {
        public T HostData;

        //public BlueprintRuntimeDataNode() { }
        //public BlueprintRuntimeDataNode(BlueprintRuntimeGraph e) : base(e)
        //{
        //}

        public virtual void Init(T data, bool AutoStreamPin = true)
        {
            base.Init(AutoStreamPin);

            HostData = data;
            NodeID = data.NodeID;
        }

        public virtual void UnInit() { }

        public T GetData() { return HostData; }

        
    }
}
