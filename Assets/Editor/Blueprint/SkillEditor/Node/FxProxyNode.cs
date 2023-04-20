using EcsData;
using UnityEngine;
using BluePrint;
using System.Collections.Generic;

namespace EditorNode
{
    public class FxProxyNode : TimeTriggerNode<XFxProxyData>
    {
        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, AutoDefaultMainPin);

            HeaderImage = "BluePrint/Header3";
        }

    }
}
