using System;
using System.Collections;
using System.Collections.Generic;
using BluePrint;
using EcsData;
using UnityEditor;
using UnityEngine;

namespace EditorNode
{
    public class WarningNode : TimeTriggerNode<XWarningData>
    {
        private GameObject FxObject;
        private BaseSkillNode bulletNode = null;

        private BaseSkillNode getPositionNode = null;
        private BaseSkillNode getDirectionNode = null;
        private int commonWarningIndex = 0;
        private string[] commonWarningPrefab = new string[]
        {
            "",
            "fx_warning_scale",
            "fx_warning_circle_static",
            "fx_warning_rectangle",
            "fx_warning_rectangle_static",
            "fx_warning_annular",
            "fx_warning_circle_purple_loop",
            "fx_warning_circle_purple_once",
            "fx_warning_rectangle_purple_once",
        };

        private string[] commonWarningPrefabName = new string[]
        {
            "无",
            "红色圆形预警",
            "红色圆形不扩散",
            "红色矩形预警",
            "红色矩形不扩散",
            "红色环形预警",
            "紫色循环动态预警",
            "紫色单次动态预警",
            "紫色矩形预警",
        };
        public override void Init (BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init (Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header1";

            if (GetRoot.NeedInitRes)
            {
                if (nodeEditorData != null && !string.IsNullOrEmpty(nodeEditorData.CustomData1))
                {
                    FxObject = AssetDatabase.LoadAssetAtPath(nodeEditorData.CustomData1, typeof(GameObject)) as GameObject;
                }
                else
                    FxObject = AssetDatabase.LoadAssetAtPath(ResourecePath + HosterData.FxPath + ".prefab", typeof(GameObject)) as GameObject;
            }
        }

        public override void DrawDataInspector ()
        {
            base.DrawDataInspector ();

            FxObject = EditorGUITool.ObjectField ("Fx", FxObject, typeof (GameObject), false) as GameObject;
            if (FxObject != null)
            {
                HosterData.FxPath = FxObject.name.ToLower ();
                if (nodeEditorData != null)
                {
                    nodeEditorData.CustomData1 = AssetDatabase.GetAssetPath (FxObject);
                }
                // else
                // {
                //     HosterData.FxPath = AssetDatabase.GetAssetPath (FxObject).Replace (ResourecePath, "");
                //     HosterData.FxPath = HosterData.FxPath.Remove (HosterData.FxPath.LastIndexOf ('.'));

                // }
            }
            else HosterData.FxPath = "";

            HosterData.EndWithSkill = EditorGUITool.Toggle("EndWithSkill", HosterData.EndWithSkill);

            HosterData.LifeTime = TimeFrameField ("LifeTime", HosterData.LifeTime);

            HosterData.FollowTime = TimeFrameField("FollowTime", HosterData.FollowTime);
            
            HosterData.HighLightTime = TimeFrameField("HighLightTime(需要预制体支持高亮,建议不超过2s)", HosterData.HighLightTime);

            HosterData.NotRealWarning = EditorGUITool.Toggle("不是预警圈(需要使用预警逻辑的技能特效请勾选)", HosterData.NotRealWarning);
            HosterData.Angle = EditorGUITool.FloatField("Angle", HosterData.Angle);
            
            EditorGUITool.Vector3Field ("Scale", ref HosterData.ScaleX, ref HosterData.ScaleY, ref HosterData.ScaleZ);
            EditorGUITool.Vector3Field ("Offset", ref HosterData.OffsetX, ref HosterData.OffsetY, ref HosterData.OffsetZ);
            HosterData.PrefabType =
                EditorGUITool.Popup("Prefab类型", HosterData.PrefabType, new[] {"不可控", "圆形或弧形", "矩形"});
            if (HosterData.PrefabType == 1)
            {
                HosterData.Arc = EditorGUITool.Slider("弧度", HosterData.Arc, 0, 360);
            }
            HosterData.VanishDistance = EditorGUITool.FloatField("VanishDistance(8-1000)", HosterData.VanishDistance);

            // if (HosterData.PrefabType == 3)
            // {
            //     HosterData.minLoop = EditorGUITool.FloatField("小环缩放", HosterData.minLoop);
            //     HosterData.maxLoop = EditorGUITool.FloatField("大环缩放", HosterData.maxLoop);
            // }
            EditorGUILayout.BeginHorizontal();
            commonWarningIndex = EditorGUITool.Popup("通用预警", commonWarningIndex, commonWarningPrefabName);
            if (GUILayout.Button("set") && commonWarningIndex > 0)
            {
                HosterData.FxPath = commonWarningPrefab[commonWarningIndex];
                FxObject = AssetDatabase.LoadAssetAtPath(ResourecePath + "Effects/Prefabs/Generic/" + HosterData.FxPath + ".prefab", typeof(GameObject)) as GameObject;
                nodeEditorData.CustomData1 = AssetDatabase.GetAssetPath(FxObject);
            }
            EditorGUILayout.EndHorizontal();
            HosterData.WarningAllLow = EditorGUITool.Toggle ("忽略画质分档(默认低配) ", HosterData.WarningAllLow);
            HosterData.noRot = EditorGUITool.Toggle("OnRot(默认不开启,有消耗)", HosterData.noRot);
            HosterData.NeedTarget = EditorGUITool.Toggle ("NeedTarget: ", HosterData.NeedTarget);
            CheckAndDo(HosterData.FollowTime == 0 && HosterData.NeedTarget,
                () => HosterData.MultiTargetIndex = Mathf.Max(0, EditorGUITool.IntField("MultiTargetIndex", HosterData.MultiTargetIndex + 1) - 1),
                () => HosterData.MultiTargetIndex = 0);
            CheckAndDo(HosterData.FollowTime == 0,
                () => HosterData.FirerDirBased = EditorGUITool.Toggle("FirerDirBased", HosterData.FirerDirBased),
                () => HosterData.FirerDirBased = false);
            CheckAndDo(HosterData.FollowTime == 0,
                () => HosterData.Random = EditorGUITool.Toggle("Random", HosterData.Random),
                () => HosterData.Random = false);

            if (HosterData.Random)
            {
                if (HosterData.RandomSeed == 0)
                    HosterData.RandomSeed = new System.Random ((int) System.DateTime.Now.Ticks).Next ();
                HosterData.RandomRange = EditorGUITool.FloatField ("RandomRange", HosterData.RandomRange);
            }
            else
            {
                HosterData.RandomSeed = 0;
            }
            HosterData.StickOnGround = EditorGUITool.Toggle ("StickOnGround", HosterData.StickOnGround);

            CheckAndDo(HosterData.FollowTime == 0,
                () => HosterData.NeedBullet = EditorGUITool.Toggle("NeedBullet", HosterData.NeedBullet),
                () => HosterData.NeedBullet = false);
            
            if (HosterData.NeedBullet)
            {
                GetNodeByIndex<BulletNode> (ref bulletNode, ref HosterData.BulletIndex, true);
                if (bulletNode != null) bulletNode.GetHosterData<XBulletData>().Velocity = 0;

                HosterData.BulletDelay = TimeFrameField ("BulletDelay", HosterData.BulletDelay);
                HosterData.BulletDelay = HosterData.BulletDelay < 0.1f ? 0.1f : HosterData.BulletDelay;
                HosterData.VisibleBullet = EditorGUITool.Toggle("VisibleBullet", HosterData.VisibleBullet);
            }

            if (!HosterData.NeedTarget && HosterData.FollowTime == 0)
            {
                DrawLine();
                GetNodeByIndex<GetPositionNode>(ref getPositionNode, ref HosterData.GetPositionIndex, true, "GetPositionIndex");
                GetNodeByIndex<GetDirectionNode>(ref getDirectionNode, ref HosterData.GetDirectionIndex, true, "GetDirectionIndex");
            }
            else
            {
                HosterData.GetPositionIndex = -1;
                HosterData.GetDirectionIndex = -1;
            }
        }

        public override void BuildDataFinish ()
        {
            base.BuildDataFinish ();

            if (HosterData.NeedBullet)
            {
                GetNodeByIndex<BulletNode> (ref bulletNode, ref HosterData.BulletIndex);
            }

            GetNodeByIndex<GetPositionNode>(ref getPositionNode, ref HosterData.GetPositionIndex);
            GetNodeByIndex<GetDirectionNode>(ref getDirectionNode, ref HosterData.GetDirectionIndex);
        }

        public override bool CompileCheck ()
        {
            if (!base.CompileCheck ()) return false;

            if (HosterData.NeedBullet && HosterData.BulletIndex == -1)
            {
                LogError ("WarningNode_" + HosterData.Index + "BulletIndex不能为空");
                return false;
            }

            if(HosterData.NeedBullet && HosterData.BulletIndex != -1 && HosterData.VisibleBullet)
            {
                GetNodeByIndex<BulletNode>(ref bulletNode, ref HosterData.BulletIndex);
                if (bulletNode != null && (XBulletType)bulletNode.GetHosterData<XBulletData>().Type != XBulletType.Warning)
                {
                    LogError("WarningNode_" + HosterData.Index + ",勾选VisibleBullet对应子弹Type必须为warning");
                    return false;
                }
            }

            if (HosterData.Random)
            {
                HosterData.RandomSeed = new System.Random ((int) System.DateTime.Now.Ticks + HosterData.Index).Next ();
            }

            return true;
        }

        public override void ScanPolicy (CFEngine.OrderResList result, CFEngine.ResItem item)
        {
            if (!string.IsNullOrEmpty (HosterData.FxPath))
            {
                if (HosterData.FxPath.Contains(" "))
                {
                    result.sb.AppendLine(string.Format("name contains space skill {0} {1}", item.nameWithExt, HosterData.FxPath));
                    result.Add(item, System.IO.Path.GetFileName(HosterData.FxPath) + ".prefab", ResourecePath + HosterData.FxPath + ".prefab");
                }
            }
               
        }

        public override void CalcTriggerTime()
        {
            base.CalcTriggerTime();

            DFSTriggerTime(this, HosterData.BulletIndex);
        }
    }
}