using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;
using VirtualSkill;

namespace EditorNode
{
    public class ResultNode : TimeTriggerNode<XResultData>
    {
        private BaseSkillNode fxNode = null;
        private BaseSkillNode audioNode = null;
        private BaseSkillNode multiConditionNode = null;

        private BaseSkillNode getPositionNode = null;
        private BaseSkillNode getDirectionNode = null;

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header4";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            HosterData.Tag = Mathf.Min(31, Mathf.Max(0, EditorGUITool.IntField("Tag", HosterData.Tag)));

            HosterData.JudgeTargetsDirectly = EditorGUITool.Toggle("JudgeTargetsDirectly", HosterData.JudgeTargetsDirectly);
            if (!HosterData.JudgeTargetsDirectly)
            {
                HosterData.SectorType = EditorGUITool.Toggle("SectorType", HosterData.SectorType);
                if (HosterData.SectorType)
                {
                    ParamTemplate("LowRange", HosterData,
                    () => { HosterData.LowRange = EditorGUITool.FloatField("Range (¡ý)", HosterData.LowRange); });
                    ParamTemplate("Range", HosterData,
                    () => { HosterData.Range = EditorGUITool.FloatField("Range (¡ü) ", HosterData.Range); });
                    HosterData.Scope = EditorGUITool.Slider("Scope (0~360): ", HosterData.Scope, 0, 360);
                }
                else
                {
                    HosterData.DynamicDepth = EditorGUITool.Toggle("DynamicDepth", HosterData.DynamicDepth);
                    if (!HosterData.DynamicDepth)
                    {
                        ParamTemplate("Range", HosterData,
                        () => { HosterData.Range = EditorGUITool.FloatField("Depth", HosterData.Range); });
                    }
                    HosterData.Scope = EditorGUITool.FloatField("Width", HosterData.Scope);
                }

                EditorGUITool.Vector3Field("Offset", ref HosterData.OffsetX, ref HosterData.OffsetY, ref HosterData.OffsetZ);
                HosterData.AngleShift = (int)EditorGUITool.Slider("AngleShift (-180~180)", HosterData.AngleShift, -180, 180);

                GetNodeByIndex<MultiConditionNode>(ref multiConditionNode, ref HosterData.FilterMultiConditionIndex, true, "FilterMultiConditionIndex");
                DrawLine();
            }

            HosterData.EffectID = EditorGUITool.Popup("EffectID: ", HosterData.EffectID, EditorGUITool.Translate<EditorEcs.XHitSlot>());
            
            ParamTemplate("TableIndex", HosterData,
                () => { HosterData.TableIndex = EditorGUITool.IntField("TableIndex", HosterData.TableIndex); });

            DrawLine();
            GetNodeByIndex<FxNode>(ref fxNode, ref HosterData.FxIndex, true, "HitFxIndex");
            GetNodeByIndex<AudioNode>(ref audioNode, ref HosterData.AudioIndex, true, "HitAudioIndex");
            DrawLine();
            GetNodeByIndex<GetPositionNode>(ref getPositionNode, ref HosterData.GetPositionIndex, true, "GetPositionIndex");
            GetNodeByIndex<GetDirectionNode>(ref getDirectionNode, ref HosterData.GetDirectionIndex, true, "GetDirectionIndex");
            DrawLine();

            DrawHitParam<XResultData>(HosterData);

            DrawLine();
        }

        public override void BuildDataFinish()
        {
            base.BuildDataFinish();

            GetNodeByIndex<MultiConditionNode>(ref multiConditionNode, ref HosterData.FilterMultiConditionIndex);
            GetNodeByIndex<FxNode>(ref fxNode, ref HosterData.FxIndex);
            GetNodeByIndex<AudioNode>(ref audioNode, ref HosterData.AudioIndex);

            GetNodeByIndex<GetPositionNode>(ref getPositionNode, ref HosterData.GetPositionIndex);
            GetNodeByIndex<GetDirectionNode>(ref getDirectionNode, ref HosterData.GetDirectionIndex);
        }

        public override void OnSelected()
        {
            base.OnSelected();

            SkillResultEditor.resultData = HosterData;
            SkillResultEditor.resultCalPos = true;
        }

        public override void SetDebug(bool flag = true)
        {
            base.SetDebug(flag);

            if (flag)
            {
                SkillResultEditor.resultData = HosterData;
                SkillResultEditor.resultCalPos = true;
            }
        }

        public override void CalcTriggerTime()
        {
            base.CalcTriggerTime();

            DFSTriggerTime(this, HosterData.AudioIndex);
            DFSTriggerTime(this, HosterData.FxIndex);
            DFSTriggerTime(this, HosterData.FilterMultiConditionIndex);
        }
    }
}