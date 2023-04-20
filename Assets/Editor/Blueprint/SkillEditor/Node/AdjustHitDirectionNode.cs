using EcsData;
using UnityEngine;
using BluePrint;
using System.Collections.Generic;
using UnityEditor;

namespace EditorNode
{
    public class AdjustHitDirectionNode : EventTriggerNode<XAdjustHitDirectionData>
    {
        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos);

            HeaderImage = "BluePrint/Header3";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HosterData.HitDirectionType = EditorGUITool.Popup("Type", HosterData.HitDirectionType, EditorGUITool.Translate<XHitDirectionType>());
        }
    }
}
