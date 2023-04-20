using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if UNITY_EDITOR

#endif

namespace BluePrint
{
    public class CommonRTControllerNode : BlueprintRuntimeDataNode<BluePrintControllerData>
    {
        public BluePrintControllerData controllerData;
        public ControllerNodeType type;

        //public CommonRTControllerNode(BlueprintRuntimeGraph e) : base(e)
        //{ }

        public static CommonRTControllerNode CreateControllerNode(int ControllerTypeID, BlueprintRuntimeGraph e)
        {
            ControllerNodeType t = (ControllerNodeType)ControllerTypeID;

            switch (t)
            {
                case ControllerNodeType.ControllerNode_Start:
                    return new CommonRTControllerStartNode();
                case ControllerNodeType.ControllerNode_Branch:
                    return new CommonRTControllerBranchNode();
                case ControllerNodeType.ControllerNode_And:
                    return new CommonRTControllerOpAnd();
                case ControllerNodeType.ControllerNode_Or:
                    return new CommonRTControllerOpOr();
                case ControllerNodeType.ControllerNode_Equal:
                    return new CommonRTControllerOpEqual();
                case ControllerNodeType.ControllerNode_Great:
                    return new CommonRTControllerOpGreat();
                case ControllerNodeType.ControllerNode_Less:
                    return new CommonRTControllerOpLess();
                case ControllerNodeType.ControllerNode_GreatEqual:
                    return new CommonRTControllerOpGreatEqual();
                case ControllerNodeType.ControllerNode_LessEqual:
                    return new CommonRTControllerOpLessEqual();
                case ControllerNodeType.ControllerNode_Arithmetic:
                    return new CommonRTArithmeticNode();
                case ControllerNodeType.ControllerNode_For:
                case ControllerNodeType.ControllerNode_While:
                    return new CommonRTControllerWhileNode();
                case ControllerNodeType.FlowNode_And:
                    return new CommonRTFlowAnd();
                case ControllerNodeType.FlowNode_Or:
                    return new CommonRTFlowOr();
                case ControllerNodeType.ControllerNode_SubStart:
                    return new CommonRTControllerSubGraphStartNode();
                case ControllerNodeType.ControllerNode_SubEnd:
                    return new CommonRTControllerSubGraphEndNode();
                case ControllerNodeType.ControllerNode_Fly:
                    return new CommonRTControllerFlyNode();
            }

            return null;
        }
    }

    public class CommonRTControllerStartNode : CommonRTControllerNode
    {
        BlueprintRuntimePin startPin;

        //public CommonRTControllerStartNode(BlueprintRuntimeGraph e) : base(e)
        //{ }

        public override void Init(BluePrintControllerData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);
            type = ControllerNodeType.ControllerNode_Start;
            AddPin(startPin = new BlueprintRuntimePin(this, 1, PinType.Main, PinStream.Out));
        }

        public override void Execute(BlueprintRuntimePin activePin)
        {
            base.Execute(activePin);
            startPin.Active();
        }
    }

    public class CommonRTControllerFlyNode : CommonRTControllerNode
    {
        BlueprintRuntimePin startPin;

        //public CommonRTControllerStartNode(BlueprintRuntimeGraph e) : base(e)
        //{ }

        public override void Init(BluePrintControllerData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);
            type = ControllerNodeType.ControllerNode_Fly;
            AddPin(startPin = new BlueprintRuntimePin(this, 1, PinType.Main, PinStream.Out));
        }

        public override void Execute(BlueprintRuntimePin activePin)
        {
            base.Execute(activePin);
            startPin.Active();
        }
    }

    public class CommonRTControllerBranchNode : CommonRTControllerNode
    {
        BlueprintRuntimeValuedPin boolPin;
        BlueprintRuntimePin truePin;
        BlueprintRuntimePin falsePin;

        //public CommonRTControllerBranchNode(BlueprintRuntimeGraph e) : base(e)
        //{ }

        public override void Init(BluePrintControllerData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);
            type = ControllerNodeType.ControllerNode_Branch;
            AddPin(new BlueprintRuntimePin(this, 1, PinType.Main, PinStream.In));
            AddPin(boolPin = new BlueprintRuntimeValuedPin(this, 2, PinType.Data, PinStream.In, VariantType.Var_Bool));
            AddPin(truePin = new BlueprintRuntimePin(this, 3, PinType.Main, PinStream.Out));
            AddPin(falsePin = new BlueprintRuntimePin(this, 4, PinType.Main, PinStream.Out));
        }

        public override void Execute(BlueprintRuntimePin activePin)
        {
            base.Execute(activePin);

            if(boolPin != null)
            {
                BPVariant bpv = new BPVariant();
                bool hasCondition = boolPin.GetPinValue(ref bpv);
                if (hasCondition && bpv.IsTrue())
                {
                    truePin.Active();
                }
                else falsePin.Active();
            }
        }
    }

    public class CommonRTControllerOpAnd : CommonRTControllerNode
    {
        BlueprintRuntimeValuedPin data1Pin;
        BlueprintRuntimeValuedPin data2Pin;

        //public CommonRTControllerOpAnd(BlueprintRuntimeGraph e) : base(e)
        //{ }
        public override void Init(BluePrintControllerData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);
            type = ControllerNodeType.ControllerNode_And;
            AddPin(data1Pin = new BlueprintRuntimeValuedPin(this, 1, PinType.Data, PinStream.In, VariantType.Var_Bool));
            AddPin(data2Pin = new BlueprintRuntimeValuedPin(this, 2, PinType.Data, PinStream.In, VariantType.Var_Bool));

            BlueprintRuntimeValuedPin pinOut = new BlueprintRuntimeValuedPin(this, 3, PinType.Data, PinStream.Out, VariantType.Var_Bool);
            pinOut.SetValueSource(GetValue);
            AddPin(pinOut);
        }

        public BPVariant GetValue()
        {
            BPVariant ret = default;
            ret.type = VariantType.Var_Bool;

            if (data1Pin != null && data2Pin != null)
            {
                BPVariant data1 = new BPVariant();
                BPVariant data2 = new BPVariant();

                if(data1Pin.GetPinValue(ref data1))
                {
                    if (!data1.IsTrue())
                    {
                        ret.val._bool = false;
                        return ret;
                    }
                }
                else
                {
                    ret.val._bool = false;
                    return ret;
                }

                if(data2Pin.GetPinValue(ref data2))
                {
                    if (!data2.IsTrue())
                    {
                        ret.val._bool = false;
                        return ret;
                    }
                }
            }
            return ret;
        }
    }

    public class CommonRTControllerOpOr : CommonRTControllerNode
    {
        BlueprintRuntimeValuedPin data1Pin;
        BlueprintRuntimeValuedPin data2Pin;

        //public CommonRTControllerOpOr(BlueprintRuntimeGraph e) : base(e)
        //{ }

        public override void Init(BluePrintControllerData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);
            type = ControllerNodeType.ControllerNode_Or;
            AddPin(data1Pin = new BlueprintRuntimeValuedPin(this, 1, PinType.Data, PinStream.In, VariantType.Var_Bool));
            AddPin(data2Pin = new BlueprintRuntimeValuedPin(this, 2, PinType.Data, PinStream.In, VariantType.Var_Bool));

            BlueprintRuntimeValuedPin pinOut = new BlueprintRuntimeValuedPin(this, 3, PinType.Data, PinStream.Out, VariantType.Var_Bool);
            pinOut.SetValueSource(GetValue);
            AddPin(pinOut);
        }

        public BPVariant GetValue()
        {
            BPVariant ret = default;
            ret.type = VariantType.Var_Bool;

            if (data1Pin != null && data2Pin != null)
            {
                BPVariant data1 = new BPVariant();
                BPVariant data2 = new BPVariant();

                if (data1Pin.GetPinValue(ref data1))
                {
                    if(data1.IsTrue())
                    {
                        ret.val._bool = true;
                        return ret;
                    }
                }

                if (data2Pin.GetPinValue(ref data2))
                {
                    if(data2.IsTrue())
                    {
                        ret.val._bool = true;
                        return ret;
                    }
                }
            }
            return ret;
        }
    }

    public class CommonRTControllerOpEqual : CommonRTControllerNode
    {
        BlueprintRuntimeValuedPin data1Pin;
        BlueprintRuntimeValuedPin data2Pin;

        //public CommonRTControllerOpEqual(BlueprintRuntimeGraph e) : base(e)
        //{ }

        public override void Init(BluePrintControllerData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);
            type = ControllerNodeType.ControllerNode_Equal;
            AddPin(data1Pin = new BlueprintRuntimeValuedPin(this, 1, PinType.Data, PinStream.In, VariantType.Var_Float, true, true));
            AddPin(data2Pin = new BlueprintRuntimeValuedPin(this, 2, PinType.Data, PinStream.In, VariantType.Var_Float, true, true));

            BlueprintRuntimeValuedPin pinOut = new BlueprintRuntimeValuedPin(this, 3, PinType.Data, PinStream.Out, VariantType.Var_Bool);
            pinOut.SetValueSource(GetValue);
            AddPin(pinOut);
        }

        public BPVariant GetValue()
        {
            BPVariant ret = default;
            ret.type = VariantType.Var_Bool;

            if (data1Pin != null && data2Pin != null)
            {
                BPVariant data1 = new BPVariant();
                BPVariant data2 = new BPVariant();

                if (data1Pin.GetPinValue(ref data1) && data2Pin.GetPinValue(ref data2))
                {
                    if(data1.type == VariantType.Var_Float && data2.type == VariantType.Var_Float)
                        ret.val._bool = ( data1.val._float == data2.val._float);
                }
            }
            return ret;
        }
    }

    public class CommonRTControllerOpGreat : CommonRTControllerNode
    {
        BlueprintRuntimeValuedPin data1Pin;
        BlueprintRuntimeValuedPin data2Pin;

        //public CommonRTControllerOpGreat(BlueprintRuntimeGraph e) : base(e)
        //{ }

        public override void Init(BluePrintControllerData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);
            type = ControllerNodeType.ControllerNode_Great;
            AddPin(data1Pin = new BlueprintRuntimeValuedPin(this, 1, PinType.Data, PinStream.In, VariantType.Var_Float, true, true));
            AddPin(data2Pin = new BlueprintRuntimeValuedPin(this, 2, PinType.Data, PinStream.In, VariantType.Var_Float, true, true));

            BlueprintRuntimeValuedPin pinOut = new BlueprintRuntimeValuedPin(this, 3, PinType.Data, PinStream.Out, VariantType.Var_Bool);
            pinOut.SetValueSource(GetValue);
            AddPin(pinOut);
        }

        public BPVariant GetValue()
        {
            BPVariant ret = default;
            ret.type = VariantType.Var_Bool;

            if (data1Pin != null && data2Pin != null)
            {
                BPVariant data1 = new BPVariant();
                BPVariant data2 = new BPVariant();

                bool data1Valid = data1Pin.GetPinValue(ref data1);
                bool data2Valid = data2Pin.GetPinValue(ref data2);

                if (data1.type == VariantType.Var_Float && data2.type == VariantType.Var_Float)
                {
                    if (data1Valid && data2Valid)
                    {
                        ret.val._bool = data1.val._float > data2.val._float;
                    }
                    //else if (data1Valid && !data2Valid)
                    //{
                    //    ret.val._bool = true;
                    //}
                    //else
                    //    ret.val._bool = false;
                }
            }
            return ret;
        }
    }

    public class CommonRTControllerOpLess : CommonRTControllerNode
    {
        BlueprintRuntimeValuedPin data1Pin;
        BlueprintRuntimeValuedPin data2Pin;

        //public CommonRTControllerOpLess(BlueprintRuntimeGraph e) : base(e)
        //{ }

        public override void Init(BluePrintControllerData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);
            type = ControllerNodeType.ControllerNode_Less;
            AddPin(data1Pin = new BlueprintRuntimeValuedPin(this, 1, PinType.Data, PinStream.In, VariantType.Var_Float, true, true));
            AddPin(data2Pin = new BlueprintRuntimeValuedPin(this, 2, PinType.Data, PinStream.In, VariantType.Var_Float, true, true));

            BlueprintRuntimeValuedPin pinOut = new BlueprintRuntimeValuedPin(this, 3, PinType.Data, PinStream.Out, VariantType.Var_Bool);
            pinOut.SetValueSource(GetValue);
            AddPin(pinOut);
        }

        public BPVariant GetValue()
        {
            BPVariant ret = default;
            ret.type = VariantType.Var_Bool;

            if (data1Pin != null && data2Pin != null)
            {
                BPVariant data1 = new BPVariant();
                BPVariant data2 = new BPVariant();

                bool data1Valid = data1Pin.GetPinValue(ref data1);
                bool data2Valid = data2Pin.GetPinValue(ref data2);

                if (data1.type == VariantType.Var_Float && data2.type == VariantType.Var_Float)
                {
                    if (data1Valid && data2Valid)
                    {
                        ret.val._bool = data1.val._float < data2.val._float;
                    }
                    //else if (data2Valid && !data1Valid)
                    //{
                    //    ret.val._bool = true;
                    //}
                    //else
                    //    ret.val._bool = false;
                }
            }
            return ret;
        }
    }

    public class CommonRTControllerOpGreatEqual : CommonRTControllerNode
    {
        BlueprintRuntimeValuedPin data1Pin;
        BlueprintRuntimeValuedPin data2Pin;

        //public CommonRTControllerOpGreatEqual(BlueprintRuntimeGraph e) : base(e)
        //{ }

        public override void Init(BluePrintControllerData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);
            type = ControllerNodeType.ControllerNode_GreatEqual;
            AddPin(data1Pin = new BlueprintRuntimeValuedPin(this, 1, PinType.Data, PinStream.In, VariantType.Var_Float, true, true));
            AddPin(data2Pin = new BlueprintRuntimeValuedPin(this, 2, PinType.Data, PinStream.In, VariantType.Var_Float, true, true));

            BlueprintRuntimeValuedPin pinOut = new BlueprintRuntimeValuedPin(this, 3, PinType.Data, PinStream.Out, VariantType.Var_Bool);
            pinOut.SetValueSource(GetValue);
            AddPin(pinOut);
        }

        public BPVariant GetValue()
        {
            BPVariant ret = default;
            ret.type = VariantType.Var_Bool;

            if (data1Pin != null && data2Pin != null)
            {
                BPVariant data1 = new BPVariant();
                BPVariant data2 = new BPVariant();

                bool data1Valid = data1Pin.GetPinValue(ref data1);
                bool data2Valid = data2Pin.GetPinValue(ref data2);

                if (data1.type == VariantType.Var_Float && data2.type == VariantType.Var_Float)
                {
                    if (data1Valid && data2Valid)
                    {
                        ret.val._bool = data1.val._float >= data2.val._float;
                    }
                    //else if (data2Valid && !data1Valid)
                    //{
                    //    ret.val._bool = false;
                    //}
                    //else
                    //    ret.val._bool = true;
                }
                    
            }
            return ret;
        }
    }

    public class CommonRTControllerOpLessEqual : CommonRTControllerNode
    {
        BlueprintRuntimeValuedPin data1Pin;
        BlueprintRuntimeValuedPin data2Pin;

        //public CommonRTControllerOpLessEqual(BlueprintRuntimeGraph e) : base(e)
        //{ }

        public override void Init(BluePrintControllerData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);
            type = ControllerNodeType.ControllerNode_LessEqual;
            AddPin(data1Pin = new BlueprintRuntimeValuedPin(this, 1, PinType.Data, PinStream.In, VariantType.Var_Float, true, true));
            AddPin(data2Pin = new BlueprintRuntimeValuedPin(this, 2, PinType.Data, PinStream.In, VariantType.Var_Float, true, true));

            BlueprintRuntimeValuedPin pinOut = new BlueprintRuntimeValuedPin(this, 3, PinType.Data, PinStream.Out, VariantType.Var_Bool);
            pinOut.SetValueSource(GetValue);
            AddPin(pinOut);
        }

        public BPVariant GetValue()
        {
            BPVariant ret = default;
            ret.type = VariantType.Var_Bool;

            if (data1Pin != null && data2Pin != null)
            {
                BPVariant data1 = new BPVariant();
                BPVariant data2 = new BPVariant();

                bool data1Valid = data1Pin.GetPinValue(ref data1);
                bool data2Valid = data2Pin.GetPinValue(ref data2);

                if (data1Valid && data2Valid)
                {
                    ret.val._bool = data1.val._float <= data2.val._float;
                }
                //else if (data1Valid && !data2Valid)
                //{
                //    ret.val._bool = false;
                //}
                //else
                //    ret.val._bool = true;
            }
            return ret;
        }
    }

    public class CommonRTControllerForNode : CommonRTControllerNode
    {
        //public CommonRTControllerForNode(BlueprintRuntimeGraph e) : base(e)
        //{ }
    }

    public class CommonRTControllerWhileNode : CommonRTControllerNode
    {
        BlueprintRuntimeValuedPin condition;
        BlueprintRuntimePin pinLoop;
        BlueprintRuntimePin pinOver;

        //public CommonRTControllerWhileNode(BlueprintRuntimeGraph e) : base(e)
        //{ }

        public override void Init(BluePrintControllerData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);
            type = ControllerNodeType.ControllerNode_While;
            AddPin(new BlueprintRuntimePin(this, 1, PinType.Main, PinStream.In));
            AddPin(condition = new BlueprintRuntimeValuedPin(this, 2, PinType.Data, PinStream.In, VariantType.Var_Bool));
            AddPin(pinLoop = new BlueprintRuntimePin(this, 3, PinType.Main, PinStream.Out));
            AddPin(pinOver = new BlueprintRuntimePin(this, 4, PinType.Main, PinStream.Out));
        }

        public override void Execute(BlueprintRuntimePin activePin)
        {
            base.Execute(activePin);

            BPVariant b = default;
            bool hasCondition = false;
            hasCondition = condition.GetPinValue(ref b);
            if (hasCondition && b.IsTrue())
            {
                CommonRTControllerNode stackTopNode = parentGraph.PeekJumpNode();
                if(stackTopNode != this)
                    parentGraph.PushJumpNode(this);
                pinLoop.Active();
            }
            else
            {
                parentGraph.PopJumpNode();
                pinOver.Active();
            }
        }
    }

    public class CommonRTArithmeticNode : CommonRTControllerNode
    {
        BlueprintRuntimeValuedPin data1Pin;
        BlueprintRuntimeValuedPin data2Pin;

        //public CommonRTArithmeticNode(BlueprintRuntimeGraph e) : base(e)
        //{ }

        public override void Init(BluePrintControllerData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);
            type = ControllerNodeType.ControllerNode_Arithmetic;
            AddPin(data1Pin = new BlueprintRuntimeValuedPin(this, 1, PinType.Data, PinStream.In, VariantType.Var_Float, true, true));
            AddPin(data2Pin = new BlueprintRuntimeValuedPin(this, 2, PinType.Data, PinStream.In, VariantType.Var_Float, true, true));

            BlueprintRuntimeValuedPin pinOut = new BlueprintRuntimeValuedPin(this, 3, PinType.Data, PinStream.Out, VariantType.Var_Float);
            pinOut.SetValueSource(GetValue);
            AddPin(pinOut);
        }

        public BPVariant GetValue()
        {
            BPVariant ret = default;
            ret.type = VariantType.Var_Float;

            if (data1Pin != null && data2Pin != null)
            {
                BPVariant data1 = new BPVariant();
                BPVariant data2 = new BPVariant();

                bool data1Valid = data1Pin.GetPinValue(ref data1);
                bool data2Valid = data2Pin.GetPinValue(ref data2);

                if (data1.type == VariantType.Var_Float && data2.type == VariantType.Var_Float)
                {
                    if (data1Valid && data2Valid)
                    {
                        ArithmeticType t = (ArithmeticType)HostData.ArithmeticID;
                        switch (t)
                        {
                            case ArithmeticType.Add:
                                ret.val._float = data1.val._float + data2.val._float;
                                break;
                            case ArithmeticType.Sub:
                                ret.val._float = data1.val._float - data2.val._float;
                                break;
                            case ArithmeticType.Mul:
                                ret.val._float = data1.val._float * data2.val._float;
                                break;
                            case ArithmeticType.Div:
                                if (data2.val._float != 0)
                                    ret.val._float = data1.val._float / data2.val._float;
                                else
                                    throw new Exception("Divid Zero!! NodeID = " + HostData.NodeID);
                                break;
                        }
                    }

                }
            }
            return ret;
        }
    }

    public class CommonRTFlowAnd : CommonRTControllerNode
    {
        BlueprintRuntimePin pinIn1;
        BlueprintRuntimePin pinIn2;
        BlueprintRuntimePin pinIn3;
        BlueprintRuntimePin pinIn4;
        BlueprintRuntimePin pinOutput;

        bool pin1Active = false;
        bool pin2Active = false;
        bool pin3Active = false;
        bool pin4Active = false;
        bool bActived = false;

        //public CommonRTFlowAnd(BlueprintRuntimeGraph e) : base(e) { }

        public override void Init(BluePrintControllerData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);
            type = ControllerNodeType.FlowNode_And;
            //AddPin(new BlueprintRuntimePin(this, 1, PinType.Main, PinStream.In));
            AddPin(pinIn1 = new BlueprintRuntimePin(this, 1, PinType.Main, PinStream.In));
            AddPin(pinIn2 = new BlueprintRuntimePin(this, 2, PinType.Main, PinStream.In));
            AddPin(pinOutput = new BlueprintRuntimePin(this, 3, PinType.Main, PinStream.Out));
            AddPin(pinIn3 = new BlueprintRuntimePin(this, 4, PinType.Main, PinStream.In));
            AddPin(pinIn4 = new BlueprintRuntimePin(this, 5, PinType.Main, PinStream.In));
        }

        public override void AfterBuild()
        {
            base.AfterBuild();

            if (pinIn1.ReverseConnectionList.Count == 0) pin1Active = true;
            if (pinIn2.ReverseConnectionList.Count == 0) pin2Active = true;
            if (pinIn3.ReverseConnectionList.Count == 0) pin3Active = true;
            if (pinIn4.ReverseConnectionList.Count == 0) pin4Active = true;
        }

        public override void Execute(BlueprintRuntimePin activePin)
        {
            base.Execute( activePin);

           if(activePin == pinIn1)   pin1Active = true;

           if(activePin == pinIn2)   pin2Active = true;

           if(activePin == pinIn3)   pin3Active = true;

           if (activePin == pinIn4)  pin4Active = true;

            TryExecuteOutput();
        }

        private void TryExecuteOutput()
        {
            if (pin1Active && pin2Active && pin3Active && pin4Active)
                ExecuteOutput();
        }

        private void ExecuteOutput()
        {
            if(!bActived)
            {
                bActived = true;
                pin1Active = pin2Active = false;
                pinOutput.Active();
            }
        }
    }

    public class CommonRTFlowOr : CommonRTControllerNode
    {
        BlueprintRuntimePin pinIn1;
        BlueprintRuntimePin pinIn2;
        BlueprintRuntimePin pinOutput;

        bool bActived = false;

        public override void Init(BluePrintControllerData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);
            type = ControllerNodeType.FlowNode_Or;
            //AddPin(new BlueprintRuntimePin(this, 1, PinType.Main, PinStream.In));
            AddPin(pinIn1 = new BlueprintRuntimePin(this, 1, PinType.Main, PinStream.In));
            AddPin(pinIn2 = new BlueprintRuntimePin(this, 2, PinType.Main, PinStream.In));
            AddPin(pinOutput = new BlueprintRuntimePin(this, 3, PinType.Main, PinStream.Out));
        }

        public override void Execute(BlueprintRuntimePin activePin)
        {
            base.Execute(activePin);

            if(!bActived)
            {
                bActived = true;
                pinOutput.Active();
            }
        }
    }

    public class CommonRTControllerSubGraphStartNode : CommonRTControllerNode
    {
        BlueprintRuntimePin startPin;

        public override void Init(BluePrintControllerData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);
            type = ControllerNodeType.ControllerNode_SubStart;
            AddPin(startPin = new BlueprintRuntimePin(this, 1, PinType.Main, PinStream.Out));
        }

        public override void Execute(BlueprintRuntimePin activePin)
        {
            base.Execute(activePin);
#if UNITY_EDITOR
            BlueprintEditor.Editor.OpenGraph(parentGraph.GraphID);
#endif
            startPin.Active();
        }
    }

    public class CommonRTControllerSubGraphEndNode : CommonRTControllerNode
    {
        //BlueprintRuntimePin startPin;

        public override void Init(BluePrintControllerData data, bool AutoStreamPin = true)
        {
            base.Init(data, false);
            type = ControllerNodeType.ControllerNode_SubEnd;
            AddPin(new BlueprintRuntimePin(this, 1, PinType.Main, PinStream.In));
        }

        public override void Execute(BlueprintRuntimePin activePin)
        {
            base.Execute(activePin);
            
            if(parentGraph.CallStack.Count > 0)
            {
                BlueprintRuntimeSubGraphNode callNode = parentGraph.CallStack.Pop();
#if UNITY_EDITOR
                BlueprintEditor.Editor.OpenGraph(callNode.parentGraph.GraphID);
#endif
                callNode.ExecuteNext();
            }
        }
    }
}
