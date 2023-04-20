using UnityEditor;
using UnityEngine;
using BluePrint;
using EcsData;
using EditorEcs;

namespace EditorNode
{
    public class BuffNode : TimeTriggerNode<XBuffData>
    {
        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header9";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            ParamTemplate("BuffID", HosterData,
                () => { HosterData.BuffID = EditorGUITool.IntField("BuffID", HosterData.BuffID); });
            bool isReduceLevel = HosterData.BuffLevel >= 10000;
            isReduceLevel = EditorGUITool.Toggle("BuffReduceLevel", isReduceLevel);
            if(isReduceLevel)
            {
                ParamTemplate("BuffLevel", HosterData,
                    () => { HosterData.BuffLevel = EditorGUITool.IntField("BuffLevel", HosterData.BuffLevel % 10000) + 10000; });
            }
            else
            {
                ParamTemplate("BuffLevel", HosterData,
                    () => { HosterData.BuffLevel = EditorGUITool.IntField("BuffLevel", HosterData.BuffLevel % 10000); });
            }
            HosterData.BuffLayer = EditorGUITool.IntField("BuffLayer", HosterData.BuffLayer);
            HosterData.TargetType = EditorGUITool.Popup("TargetType", HosterData.TargetType, EditorGUITool.Translate<XBuffTargetType>());

            CheckAndDo((XBuffTargetType)HosterData.TargetType == XBuffTargetType.Target,
                () => HosterData.MultiTargetIndex = Mathf.Max(0, EditorGUITool.IntField("MultiTargetIndex", HosterData.MultiTargetIndex + 1) - 1),
                () => HosterData.MultiTargetIndex = 0);

            DrawLine();

            HosterData.TransformSkinID = EditorGUITool.IntField("TransformSkinID", HosterData.TransformSkinID);
            HosterData.TransformSkinLifeTime = GetRoot.TimeFrameField("TransformSkinLifeTime", HosterData.TransformSkinLifeTime);
        }

        public override bool CompileCheck()
        {
            if (HosterData.BuffID == 0 && (HosterData.BuffLevel % 10000) == 0 && HosterData.BuffIDParamIndex == -1 &&
                HosterData.BuffLevelParamIndex == -1)
            {
                hasError = true;
                LogError("Node_" + HosterData.Index + "，Buff配置异常！！！");
                return false;
            }
            if (HosterData.BuffLayer == 0)
            {
                hasError = true;
                LogError("Node_" + HosterData.Index + "，Buff配置异常，BuffLayer不能为0！！！");
                return false;
            }

            return base.CompileCheck();
        }

        private static uint TransformSkinToken = 0; 
        public override void SetDebug(bool flag = true)
        {
            base.SetDebug(flag);

            if (flag && HosterData.TransformSkinID != 0)
            {
                if (VirtualSkill.SkillHoster.GetHoster != null)
                {
                    CFUtilPoolLib.XTimerMgr.singleton.KillTimer(TransformSkinToken);
                    VirtualSkill.SkillHoster.GetHoster.EntityDic[1].TransformSkin(HosterData.TransformSkinID);
                    TransformSkinToken = CFUtilPoolLib.XTimerMgr.singleton.SetTimer(HosterData.TransformSkinLifeTime,
                        (object o) => { VirtualSkill.SkillHoster.GetHoster.EntityDic[1].TransformSkin(GetRoot.GetConfigData<XConfigData>().PresentID); },
                        null);
                }
            }
        }
    }
}