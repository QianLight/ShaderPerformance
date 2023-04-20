using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BluePrint;
using CFUtilPoolLib;
using XEditor;

namespace LevelEditor
{
    class LevelRTStartTimingNode : BlueprintRuntimeDataNode<LevelStartTimeData>
    {
        BlueprintRuntimePin timeOutPin;

        public override void Init(LevelStartTimeData data, bool AutoStreamPin = true)
        {
            base.Init(data);

            AddPin(timeOutPin = new BlueprintRuntimePin(this, 1, PinType.Main, PinStream.Out));
        }

        public override void Execute(BlueprintRuntimePin activePin)
        {
            
        }
    }

    class LevelRTEndTimingNode : BlueprintRuntimeDataNode<LevelEndTimeData>
    {
        BlueprintRuntimeValuedPin timePin;
        float ElapseTime = 0;

        public override void Init(LevelEndTimeData data, bool AutoStreamPin = true)
        {
            base.Init(data);

            timePin = new BlueprintRuntimeValuedPin(this, 1, PinType.Data, PinStream.Out, VariantType.Var_Float, false);
            timePin.SetValueSource(GetTime);
            AddPin(timePin);
        }

        public BPVariant GetTime()
        {
            BPVariant ret = new BPVariant();
            ret.type = VariantType.Var_Float;
            ret.val._float = ElapseTime;
            return ret;
        }

        public override void Execute(BlueprintRuntimePin activePin)
        {
            
        }
    }
}