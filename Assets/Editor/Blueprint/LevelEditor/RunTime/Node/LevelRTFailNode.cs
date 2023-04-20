using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using BluePrint;
using CFUtilPoolLib;
using XEditor;

namespace LevelEditor
{
    class LevelRTFailNode : BlueprintRuntimeDataNode<LevelFailData>
    {
        //public LevelRTFailNode(BlueprintRuntimeGraph e) : base(e)
        //{ }

        public override void Init(LevelFailData data, bool AutoStreamPin = true)
        {
            base.Init(data);

            BlueprintRuntimePin pinOut = new BlueprintRuntimePin(this, 1, PinType.Main, PinStream.In);
            AddPin(pinOut);
        }

        public override void Update(float deltaT)
        {
            base.Update(deltaT);
        }
    }
}
