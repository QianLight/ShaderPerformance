using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;
using EditorEcs;
using VirtualSkill;

namespace EditorNode
{
    public class TargetSelectNode : TimeTriggerNode<XTargetSelectData>
    {

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header1";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            
            HosterData.SelectHoster = EditorGUITool.Toggle("SelectHoster", HosterData.SelectHoster);
            if (!HosterData.SelectHoster)
            {
                DrawLine();
                HosterData.SectorType = EditorGUITool.Toggle("SectorType", HosterData.SectorType);
                if (HosterData.SectorType)
                {
                    HosterData.RangeLower = EditorGUITool.FloatField("Range(¡ý)", HosterData.RangeLower);
                    HosterData.RangeUpper = EditorGUITool.FloatField("Range(¡ü)", HosterData.RangeUpper);
                    HosterData.Scope = EditorGUITool.Slider("Scope (0~360): ", HosterData.Scope, 0, 360);
                }
                else
                {
                    HosterData.RangeUpper = EditorGUITool.FloatField("Depth", HosterData.RangeUpper);
                    HosterData.Scope = EditorGUITool.FloatField("Width", HosterData.Scope);
                }
                DrawLine();

                HosterData.OffsetX = EditorGUITool.FloatField("OffsetX", HosterData.OffsetX);
                HosterData.OffsetZ = EditorGUITool.FloatField("OffsetZ", HosterData.OffsetZ);
            }
            HosterData.LookAt = EditorGUITool.Toggle("LookAtTarget", HosterData.LookAt);

            HosterData.DiscardLastTarget = EditorGUITool.Toggle("DiscardLastTarget", HosterData.DiscardLastTarget);

            HosterData.Sync = EditorGUITool.Toggle("Sync", HosterData.Sync);

            DrawLine();
            HosterData.SelectFilter = EditorGUITool.Popup("SelectFilter", HosterData.SelectFilter, EditorGUITool.Translate<XTargetSelectFilter>());
            if ((XTargetSelectFilter)HosterData.SelectFilter == XTargetSelectFilter.Random)
            {
                HosterData.RandomNum = Mathf.Min(Xuthus.TARGETS_MAX, EditorGUITool.IntField("RandomNum", HosterData.RandomNum));
                HosterData.DiscardHighestHate = EditorGUITool.Toggle("DiscardHighestHate", HosterData.DiscardHighestHate);
            }
        }

        public override bool IgnoreMultiIn => false;

        public override void OnSelected()
        {
            base.OnSelected();

            SkillTargetSelectEditor.targetSelectData = HosterData;
            SkillTargetSelectEditor.targetSelectCalPos = true;
        }

        public override void SetDebug(bool flag = true)
        {
            base.SetDebug(flag);

            if (flag)
            {
                SkillTargetSelectEditor.targetSelectData = HosterData;
                SkillTargetSelectEditor.targetSelectCalPos = true;
            }
        }
    }
}