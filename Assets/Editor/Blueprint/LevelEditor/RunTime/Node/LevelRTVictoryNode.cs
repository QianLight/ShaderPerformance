using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BluePrint;
using CFUtilPoolLib;
using XEditor;

namespace LevelEditor
{
    class LevelRTVictoryNode : BlueprintRuntimeDataNode<LevelVictoryData>
    {
        //public LevelRTVictoryNode(BlueprintRuntimeGraph e) : base(e)
        //{ }

        public override void Init(LevelVictoryData data, bool AutoStreamPin = true)
        {
            base.Init(data);

            BlueprintRuntimePin pinOut = new BlueprintRuntimePin(this, 1, PinType.Main, PinStream.In);
            AddPin(pinOut);
        }

        public override void Execute(BlueprintRuntimePin activePin)
        {
            base.Execute(activePin);

            Debug.Log("Victory");
        }
    }
}
