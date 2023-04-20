using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BluePrint
{
    class CommonRTTimerNode : BlueprintRuntimeDataNode<BluePrintTimerData>
    {
        private bool startUpdate = false;
        private float elapsedTime = 0.0f;

        //public CommonRTTimerNode(BlueprintRuntimeGraph e) : base(e)
        //{ }

        public override void Init(BluePrintTimerData data, bool AutoStreamPin = true)
        {
            base.Init(data, AutoStreamPin);
        }

        public override void Execute(BlueprintRuntimePin activePin)
        {
            base.Execute(activePin);
            startUpdate = true;
            elapsedTime = 0;
        }

        public override void Update(float deltaT)
        {
            if (!startUpdate) return;

            //Debug.Log("deltaT = " + deltaT.ToString());

            elapsedTime += deltaT;

            if (elapsedTime >= HostData.Interval)
            {
                startUpdate = false;
                elapsedTime = 0;
                if (pinOut != null) pinOut.Active();
            }
        }
    }
}
