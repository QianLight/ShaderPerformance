using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BluePrint;

namespace BluePrint
{
    public class BlueprintRuntimePin
    {
        protected BlueprintRuntimeBaseNode Parent;

        public List<BlueprintRuntimeConnection> ConnectionList = new List<BlueprintRuntimeConnection>();
        public List<BlueprintRuntimeConnection> ReverseConnectionList = new List<BlueprintRuntimeConnection>();

        public int pinID;
        public PinType pinType;
        public PinStream pinStream;
        public bool dataOpen;

        public bool executed = false;
        
        public BlueprintRuntimePin(BlueprintRuntimeBaseNode parent, int id, PinType type, PinStream stream, bool bOpen = true)
        {
            Parent = parent;
            pinID = id;
            pinType = type;
            pinStream = stream;
            dataOpen = bOpen;
            executed = false;
        }

        public void AddConnection(BlueprintRuntimeConnection connection)
        {
            ConnectionList.Add(connection);
        }

        public void AddReverseConnection(BlueprintRuntimeConnection connection)
        {
            ReverseConnectionList.Add(connection);
        }

        public void SetExecuted(bool bEnabled)
        {
            executed = bEnabled;
        }

        public void Active()
        {
            if (pinType != PinType.Main) return;

            switch (pinStream)
            {
                case PinStream.In:
                    if (Parent != null)
                    {
                        Parent.Execute(this);
                        SetExecuted(true);
                    }
                    break;
                case PinStream.Out:
                    if (ConnectionList.Count == 0)
                    {
                        CommonRTControllerNode jumpNode = Parent.parentGraph.PeekJumpNode();
                        if (jumpNode != null)
                        {
                            jumpNode.Execute(this);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < ConnectionList.Count; ++i)
                        {
                            BlueprintRuntimePin ePin = ConnectionList[i].endPin;
                            ePin.Active();
                            SetExecuted(true);
                        }
                    }
                    break;
            }
        }  

        public bool HasNextStreamNode()
        {
            if (pinType == PinType.Main && pinStream == PinStream.Out)
                return ConnectionList.Count > 0;

            return false;
        }
    }

    public class BlueprintRuntimeValuedPin : BlueprintRuntimePin
    {
        public delegate BPVariant PinValueCb();

        public VariantType dataType;
        PinValueCb valueFunc;

        public bool HasDefaultVale = false;
        public BPVariant DefaultValue { get; set; }
        

        public BlueprintRuntimeValuedPin(BlueprintRuntimeBaseNode parent, int id, PinType type, PinStream stream, VariantType dType, bool bOpen = true, bool hasDefault = false)
            : base(parent, id, type, stream, bOpen)
        {
            dataType = dType;
            HasDefaultVale = hasDefault;
        }

        public void SetValueSource(PinValueCb callback)
        {
           valueFunc = callback;
        }

        public void SetDefaultValue(BPVariant v)
        {
            if (HasDefaultVale)
                DefaultValue = v;
        }

        public bool GetDefaultVale(ref BPVariant v)
        {
            if (!HasDefaultVale) return false;
            if (dataType != VariantType.Var_Float) return false;

            if (DefaultValue.val._float > float.MinValue)
            {
                v = DefaultValue;
                return true;
            }
            return false;
        }

        public bool GetPinValue(ref BPVariant v)
        {
            if (GetData(ref v))
                return true;

            if (GetDefaultVale(ref v))
                return true;

            return false;
        }

        protected bool GetData(ref BPVariant value)
        {
            if (pinType != PinType.Data) return false;

            switch (pinStream)
            {
                case PinStream.In:
                    bool HasData = false;
                    if( ReverseConnectionList.Count > 0)
                    {
                        BlueprintRuntimePin ePin = ReverseConnectionList[0].endPin;
                        BlueprintRuntimeValuedPin evPin = ePin as BlueprintRuntimeValuedPin;
                        
                        if (evPin != null && evPin.dataOpen)
                        {
                            if (evPin.GetData(ref value)) HasData = true;
                        }
                    }
                    return HasData;
                case PinStream.Out:
                    if (valueFunc != null)
                    {
                        value = valueFunc();
                        return true;
                    }
                    break;
            }

            return false;
        }
    }
}
