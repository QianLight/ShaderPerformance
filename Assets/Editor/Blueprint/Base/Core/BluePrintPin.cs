using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;

namespace BluePrint
{
    public enum PinType
    {
        Main,
        Data,
    }

    public enum PinStream
    {
        In,
        Out,
    }

    public struct BPVariant
    {
        [StructLayout(LayoutKind.Explicit)]
        public struct __Value
        {
            [FieldOffset(0)]
            internal bool _bool;

            [FieldOffset(0)]
            internal float _float;

            [FieldOffset(0)]
            internal Vector3 _vec3;

            [FieldOffset(0)]
            internal UInt64 _uint64;
        }

        public VariantType type;
        public __Value val;

        public bool IsTrue()
        {
            return type == VariantType.Var_Bool && val._bool == true;
        }
    }

    public class BluePrintPin : BluePrintWidget
    {
        protected BluePrintNode node;

        public int pinID;
        public PinType pinType;
        public PinStream pinStream;
        public string Desc;
        public int pinHeight = 22;
        public bool NoRightDownEvent = false;

        public float connectDeltaX;
        public float connectDeltaY;

        public List<BlueprintConnection> connections = new List<BlueprintConnection>();
        public List<BlueprintReverseConnection> reverseConnections = new List<BlueprintReverseConnection>();

        public T GetNode<T>() where T : BluePrintNode
        {
            return node as T;
        }

        public BluePrintPin(BluePrintNode parent, int id, string desc, PinType type, PinStream stream, float deltaX = 15, float deltaY = 70,int height=22)
        {
            node = parent;
            Root = parent.Root;
            pinID = id;
            pinType = type;
            pinStream = stream;
            Desc = desc;
            connectDeltaX = deltaX;
            connectDeltaY = deltaY;
            pinHeight = height;
            NoRightDownEvent = false;

            //SetDefaultValue(float.MinValue);
        }

        
        public override void Draw()
        {
            adjustedBounds = Bounds.Scale(Scale);

            if(pinStream == PinStream.In)
            {
                DrawPinIcon(adjustedBounds, pinType, reverseConnections.Count > 0, IsPinExecuted());

                var width = DrawTool.CalculateTextSize(Desc, BlueprintStyles.PinTextStyle).x;

                if (!string.IsNullOrEmpty(Desc))
                {
                     var labelRect = new Rect(adjustedBounds.x + 18f * Scale, adjustedBounds.y, width , 16f * Scale);
                    DrawTool.DrawLabel(labelRect, Desc, BlueprintStyles.PinTextStyle, TextAnchor.MiddleCenter);
                }
            }
            else if(pinStream == PinStream.Out)
            {
                if (!string.IsNullOrEmpty(Desc))
                {
                    var width = DrawTool.CalculateTextSize(Desc, BlueprintStyles.PinTextStyle).x;
                    var labelRect = new Rect(adjustedBounds.x - width, adjustedBounds.y, width , 16f * Scale);
                    DrawTool.DrawLabel(labelRect, Desc, BlueprintStyles.PinTextStyle, TextAnchor.MiddleCenter);
                }

                DrawPinIcon(adjustedBounds, pinType, connections.Count > 0, pinData == null ? false : pinData.executed);
            }
 
            for(int i = 0; i < connections.Count; ++i)
            {
                connections[i].Draw();
            }
        }

        public void AddConnection(BlueprintConnection connection)
        {
            connections.Add(connection);
        }

        public void RemoveConnection(BlueprintConnection connection)
        {
            connections.Remove(connection);
        }

        public void RemoveConnection(BluePrintPin endPin)
        {
            for (int i = connections.Count -1; i >= 0; --i)
            {
                if (connections[i].connectEnd == endPin)
                {
                    connections.RemoveAt(i);
                    break;
                }
            }
        }

        public void AddReversceConnection(BlueprintReverseConnection connection)
        {
            reverseConnections.Add(connection);
        }

        public void RemoveReverseConnection(BluePrintPin startPin)
        {
            for(int i = 0; i < reverseConnections.Count; ++i)
            {
                if(reverseConnections[i].reverseConnectEnd == startPin)
                {
                    reverseConnections.RemoveAt(i);
                    break;
                }
            }
        }

        public bool HasConnection()
        {
            return (connections.Count > 0 || reverseConnections.Count > 0 || pinID == -1 || pinID == -2);
        }

        public bool HasConnectionOrDefaultValue()
        {
            bool HasDefaultValue = false;

            if(this is BluePrintValuedPin)
            {
                BluePrintValuedPin vp = this as BluePrintValuedPin;
                BPVariant variant = new BPVariant();
                if(vp.GetDefaultValue(ref variant))
                {
                    HasDefaultValue = true;
                }
            }
            return HasConnection() || HasDefaultValue;
        }

        public void OnDeleted()
        {
            for(int i = 0; i < connections.Count; ++i)
            {
                BluePrintPin endPin = connections[i].connectEnd;

                endPin.RemoveReverseConnection(this);
            }

            connections.Clear();

            for (int i = 0; i < reverseConnections.Count; ++i)
            {
                BluePrintPin startPin = reverseConnections[i].reverseConnectEnd;

                startPin.RemoveConnection(this);
            }

            reverseConnections.Clear();
        }

        protected override bool OnMouseLeftDown(Event e)
        {
            if(pinStream == PinStream.Out)
            {
                node.Root.ConnectStartPin = this;
                return true;
            }

            return false;
            
        }

        protected override bool OnMouseRightDown(Event e)
        {
            if (NoRightDownEvent) return false;

            if(pinStream == PinStream.Out)
            {
                if(connections.Count > 0)
                {
                    var genericMenu = new GenericMenu();
                    for(int i = 0; i < connections.Count; ++i)
                    {
                        BluePrintPin endPin = connections[i].connectEnd;
                        string text = string.Format("Break -->{0}({1})[{2}]",
                            endPin.node.NodeName, endPin.node.nodeEditorData.NodeID, endPin.node.nodeEditorData.Tag);
                        //"Break -->" + endPin.node.NodeName + "(" + endPin.node.nodeEditorData.NodeID + ")";
                        genericMenu.AddItem(new GUIContent(text), false, BreakPinConnection, connections[i]);
                    }
                    genericMenu.ShowAsContext();
                    return true;
                }
            }
            return false;
        }

        protected override bool OnMouseUp(Event e)
        {
            if(node.Root.ConnectStartPin != null)
            {
                BluePrintPin cachedPin = node.Root.ConnectStartPin;

                if(cachedPin.node != node &&
                    cachedPin.pinType == pinType &&
                    pinStream == PinStream.In)
                {
                    if (node.CanConnect(cachedPin, this) && cachedPin.node.CanConnect(cachedPin, this))
                    {
                        BlueprintConnection connection = node.ConnectPin(cachedPin, this);

                        node.OnConnectionSucc(cachedPin, this, connection);
                    }
                }
            }

            base.OnMouseUp(e);
            return true;
        }

        protected void DrawPinIcon(Rect rect, PinType type, bool IsConnected, bool IsActived = false)
        {
            switch(type)
            {
                case PinType.Main:
                    {
                        if(IsActived)
                        {
                            DrawTool.DrawIcon(rect, BluePrintHelper.strPinMainActived);
                        }
                        else if (IsConnected)
                        {
                            DrawTool.DrawIcon(rect, BluePrintHelper.strPinMainConnected);
                        }
                        else
                        {
                            DrawTool.DrawIcon(rect, BluePrintHelper.strPinMainNoConnect);
                        }
                    }
                     break;
                case PinType.Data:
                    {
                        if (IsConnected)
                        {
                            DrawTool.DrawIcon(rect, BluePrintHelper.strPinDataConnected);
                        }
                        else
                        {
                            DrawTool.DrawIcon(rect, BluePrintHelper.strPinDataNoConnect);
                        }
                    }
                    break;
            }
        }

        

        protected void BreakPinConnection(object o)
        {
            BlueprintConnection conn = (BlueprintConnection)o;

            if (conn.connectStart != this)
            {
                Debug.Log("break error!");
                return;
            }

            node.OnConnectionBreak(this, conn.connectEnd);
            RemoveConnection(conn);
            conn.connectEnd.RemoveReverseConnection(this);
        }

        public Color SelectColor = Color.green;

        #region simulation

        private BlueprintRuntimePin pinData;

        public void OnEnterSimulation(BlueprintRuntimeBaseNode nodeData)
        {
            pinData = nodeData.GetPin(pinID);
        }

        public void OnEndSimulation()
        {
            pinData = null;
        }

        public bool IsPinExecuted()
        {
            return pinData == null ? false : pinData.executed;
        }

        #endregion
    }

    public class BluePrintValuedPin : BluePrintPin
    {
        public VariantType dataType;
        public bool HasDefaultVale = false;

        public BPVariant DefaultValue { get; set;}

        public BluePrintValuedPin(BluePrintNode parent, int id, string desc, PinType type, PinStream stream, VariantType dType, float v = float.MinValue) 
            : base(parent, id, desc, type, stream)
        {
            dataType = dType;
            HasDefaultVale = false;

            if (v > float.MinValue && dType == VariantType.Var_Float)
            {
                HasDefaultVale = true;
                SetDefaultValue(v);
            }
        }

        public void SetDefaultValue(float v)
        {
            BPVariant bpv = new BPVariant();
            bpv.type = VariantType.Var_Float;
            bpv.val._float = v;
            HasDefaultVale = true;
            SetDefaultValue(bpv);
        }

        public void SetDefaultValue(BPVariant v)
        {
            if (HasDefaultVale)
                DefaultValue = v;
        }

        public bool GetDefaultValue(ref BPVariant v)
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

        public VariantType GetRealType()
        {
            if(dataType == VariantType.Var_Custom)
            {
                CommonVariableSetNode s = node as CommonVariableSetNode;
                if (s != null) return s.GetRealVariableType();

                CommonVariableGetNode g = node as CommonVariableGetNode;
                if (g != null) return g.GetRealVariableType();

                BluePrintNode b = node as BluePrintNode;
                if (b != null) return b.GetDataType(pinID);

                return VariantType.Var_Unknow;
            }

            return dataType;
        }

        public override void Draw()
        {
            base.Draw();

            var width = DrawTool.CalculateTextSize(Desc, BlueprintStyles.PinTextStyle).x;
            BPVariant v = default(BPVariant);
            var textRect = new Rect(adjustedBounds.x + 24f * Scale + width, adjustedBounds.y, 50 * Scale, 16f * Scale);
            if (GetDefaultValue(ref v))
            {
                var newName = EditorGUI.TextField(textRect, v.val._float.ToString(), BlueprintStyles.PinDefaultValueStyle);
                float result;
                if (float.TryParse(newName, out result))
                {
                    SetDefaultValue(result);
                }
            }
        }


    }
}
