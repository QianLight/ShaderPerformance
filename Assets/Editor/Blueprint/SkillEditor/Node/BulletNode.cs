using System;
using System.Collections;
using System.Collections.Generic;
using BluePrint;
using CFEngine;
using EcsData;
using UnityEditor;
using UnityEngine;

namespace EditorNode
{
    public class BulletNode : TimeTriggerNode<XBulletData>
    {
        private GameObject BulletObject;

        private BaseSkillNode fxNode = null;
        private BaseSkillNode bulletNode = null;
        private BaseSkillNode triggerBulletNode = null;

        private BaseSkillNode mobUnitNode = null;
        private BaseSkillNode triggerMobUnitNode = null;

        private BaseSkillNode audioNode = null;
        private BaseSkillNode selfAudioNode = null;
        private BaseSkillNode selfShakeNode = null;

        private BaseSkillNode getPositionNode = null;
        private BaseSkillNode getDirectionNode = null;

        private BaseSkillNode curveNode = null;
        private BaseSkillNode nextNode = null;

        public override void Init (BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init (Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header2";

            if (GetRoot.NeedInitRes)
            {
                if (nodeEditorData != null && !string.IsNullOrEmpty(nodeEditorData.CustomData1))
                {
                    BulletObject = AssetDatabase.LoadAssetAtPath(nodeEditorData.CustomData1, typeof(GameObject)) as GameObject;
                }
                else
                {
                    if (!string.IsNullOrEmpty(HosterData.BulletPath))
                    {
                        var sfxEditorAsset = SFXWrapper.GetSFXEditorAsset(HosterData.BulletPath);
                        if (sfxEditorAsset != null)
                        {
                            BulletObject = sfxEditorAsset.srcSFX;
                        }
                        else
                        {
                            DebugLog.AddErrorLog2("bullet prefab not find:{0}", HosterData.BulletPath);
                            BulletObject = AssetDatabase.LoadAssetAtPath(ResourecePath + HosterData.BulletPath + ".prefab", typeof(GameObject)) as GameObject;
                        }
                    }

                }
            }
            //BulletObject = AssetDatabase.LoadAssetAtPath(ResourecePath + HosterData.BulletPath + ".prefab", typeof(GameObject)) as GameObject;
        }

        private void ResetBulletData()
        {
            HosterData.StickGround = false;
            HosterData.Pitch = false;
            HosterData.Velocity = 0;
            HosterData.VelocityY = 0;
            HosterData.Acceleration = 0;
            HosterData.AccelerationAt = 0;
            HosterData.AccelerationY = 0;
            HosterData.Follow = false;
            HosterData.AimTarget = false;
            HosterData.TowardTarget = false;
        }

        private bool isWarning => (XBulletType)HosterData.Type == XBulletType.Warning;
        private bool isRing => (XBulletType)HosterData.Type == XBulletType.Ring;
        private bool isPingPong => (XBulletType)HosterData.Type == XBulletType.PingPong;
        private bool isTrap => (XBulletType)HosterData.Type == XBulletType.Trap;
        private bool isBindHoster => HosterData.BindHoster;
        private bool useCurve => HosterData.CurveDataIndex != -1;
        private bool isStickGround => HosterData.StickGround;
        private bool isAimTarget => HosterData.AimTarget;
        private bool isTowardTarget => HosterData.TowardTarget;
        private bool isFollow => HosterData.Follow;
        private bool isPitch => HosterData.Pitch;
        private bool withCollision => HosterData.WithCollision;
        private bool isGroup => HosterData.Group;
        private bool isOnce => HosterData.Once;

        private static Vector2 scrollview = Vector2.zero;

        private bool LifeWithHoster = true;
        const float LifeWithHosterTime = 7200;//server scene life: 2 hours
        public override void DrawDataInspector()
        {
            float height = GetRoot.editorWindow.position.height - 200;
            scrollview = EditorGUILayout.BeginScrollView(scrollview, false, false, GUILayout.Width(0), GUILayout.Height(height));

            base.DrawDataInspector();

            HosterData.Type = EditorGUITool.Popup("Type", HosterData.Type, EditorGUITool.Translate<XBulletType>());

            if (!LifeWithHoster)
            {
                CheckAndDo(!isWarning,
                    () => HosterData.LifeTime = TimeFrameField("LifeTime", HosterData.LifeTime, false, false, Xuthus.BULLET_LIFE_MAX),
                    () => { HosterData.LifeTime = 0; HosterData.LifeTimeParamIndex = -1; });
            }

            LifeWithHoster = HosterData.LifeTime == LifeWithHosterTime;
            LifeWithHoster = EditorGUITool.Toggle("Whole Life within Firer", LifeWithHoster);
            if (LifeWithHoster)
            {
                HosterData.LifeTime = 7200;
                HosterData.LifeTimeParamIndex = -1;
                HosterData.EndWithSkill = false;
            }
            else if (HosterData.LifeTime == 7200)
                HosterData.LifeTime = 0;

            HosterData.DestroyDelay = TimeFrameField("DestroyDelay", HosterData.DestroyDelay);

            EditorGUI.BeginDisabledGroup(LifeWithHoster);
            HosterData.EndWithSkill = EditorGUITool.Toggle("EndWithSkill", HosterData.EndWithSkill);
            EditorGUI.EndDisabledGroup();

            DrawLine();
            CheckAndDo(!isWarning,
                () => HosterData.BindHoster = EditorGUITool.Toggle("BindHoster", HosterData.BindHoster),
                () => HosterData.BindHoster = false);

            DrawLine();
            CheckAndDo(!isWarning && !isBindHoster && !useCurve && !isRing,
                () => HosterData.StickGround = EditorGUITool.Toggle("StickGround", HosterData.StickGround),
                () => HosterData.StickGround = false);

            CheckAndDo(!isWarning && !isBindHoster && !useCurve,
                () => ParamTemplate("Velocity", HosterData, () => HosterData.Velocity = Mathf.Min(200f, EditorGUITool.FloatField("Velocity", HosterData.Velocity))),
                () => { HosterData.Velocity = 0; HosterData.VelocityParamIndex = -1; });

            CheckAndDo(!isWarning && !isBindHoster && !useCurve,
                () => HosterData.Acceleration = EditorGUITool.FloatField("Acceleration", HosterData.Acceleration),
                () => HosterData.Acceleration = 0);

            CheckAndDo(!isWarning && !isBindHoster && !useCurve && HosterData.Acceleration != 0,
                () => HosterData.AccelerationAt = TimeFrameField("AccelerationAt", HosterData.AccelerationAt),
                () => HosterData.AccelerationAt = 0);

            CheckAndDo(!isWarning && !isBindHoster && !useCurve && !isStickGround && !isRing && !isAimTarget && !isTowardTarget && !isFollow,
                () => HosterData.Pitch = EditorGUITool.Toggle("Use Pitch", HosterData.Pitch),
                () => HosterData.Pitch = false);
            CheckAndDo(!isWarning && !isBindHoster && !useCurve && !isStickGround && !isRing && !isAimTarget && !isTowardTarget && !isFollow && !isPitch,
                () => HosterData.VelocityY = Mathf.Min(200f, EditorGUITool.FloatField("VelocityY", HosterData.VelocityY)),
                () => HosterData.VelocityY = 0);
            CheckAndDo(!isWarning && !isBindHoster && !useCurve && !isStickGround && !isRing && !isAimTarget && !isTowardTarget && !isFollow,
                () => HosterData.AccelerationY = EditorGUITool.FloatField("AccelerationY", HosterData.AccelerationY),
                () => HosterData.AccelerationY = 0);

            CheckAndDo(!isWarning && !isBindHoster && !useCurve && !isStickGround && !isRing,
                () => HosterData.AimTarget = EditorGUILayout.Toggle("AimTarget", HosterData.AimTarget),
                () => HosterData.AimTarget = false);
            CheckAndDo(!isWarning && !isBindHoster && !useCurve && !isStickGround && !isRing && HosterData.AimTarget,
                () => HosterData.TowardTarget = EditorGUITool.Toggle("TowardTarget", HosterData.TowardTarget),
                () => HosterData.TowardTarget = false);

            CheckAndDo(!isWarning && !isBindHoster && !useCurve && withCollision && !isGroup && isOnce,
                () => HosterData.Follow = EditorGUITool.Toggle("Follow", HosterData.Follow),
                () => HosterData.Follow = false);

            CheckAndDo(!isWarning && !isBindHoster && !useCurve && (isAimTarget || isTowardTarget || isFollow),
                () => HosterData.MultiTargetIndex = Mathf.Max(0, EditorGUITool.IntField("MultiTargetIndex", HosterData.MultiTargetIndex + 1) - 1),
                () => HosterData.MultiTargetIndex = 0);

            DrawLine();
            CheckAndDo(!isWarning && !isBindHoster && !isRing,
                () => GetNodeByIndex<CurveNode>(ref curveNode, ref HosterData.CurveDataIndex, true, "CurveDataIndex"),
                () => HosterData.CurveDataIndex = -1);

            DrawLine();

            CheckAndDo(isTrap,
                () => HosterData.TrapVelocity = EditorGUITool.FloatField("TrapVelocity", HosterData.TrapVelocity),
                () => HosterData.TrapVelocity = 0);
            CheckAndDo(isTrap,
                () => HosterData.TrapType = EditorGUITool.Popup("TrapType", HosterData.TrapType,EditorGUITool.Translate<EditorEcs.XTrapType>()),
                () => HosterData.TrapType = 0);
            CheckAndDo(isTrap && (EditorEcs.XTrapType)HosterData.TrapType == EditorEcs.XTrapType.SceneForward,
                () => HosterData.TrapWorldFace = Mathf.Max(0, Mathf.Min(360, EditorGUITool.FloatField("TrapWorldFace", HosterData.TrapWorldFace))),
                () => HosterData.TrapWorldFace = 0);
            DrawLine();

            HosterData.InitPitchAngle = -EditorGUITool.FloatField("InitPitchAngle", -HosterData.InitPitchAngle);
            EditorGUITool.Vector3Field ("Offset", ref HosterData.OffsetX, ref HosterData.OffsetY, ref HosterData.OffsetZ);
            HosterData.Angle = EditorGUITool.FloatField ("Angle(clockwise)", HosterData.Angle);
            BulletObject = EditorGUITool.ObjectField ("Bullet", BulletObject, typeof (GameObject), false) as GameObject;
            CheckAndDo(BulletObject != null,
                () =>
                {
                    HosterData.BulletPath = BulletObject.name.ToLower();
                    if (nodeEditorData != null)
                        nodeEditorData.CustomData1 = AssetDatabase.GetAssetPath(BulletObject);
                },
                () => HosterData.BulletPath = "");

            GetNodeByIndex<AudioNode> (ref selfAudioNode, ref HosterData.BulletAudioIndex, true, "BulletAudioIndex");
            GetNodeByIndex<CameraShakeNode> (ref selfShakeNode, ref HosterData.BulletShakeIndex, true, "BulletShakeIndex");
            DrawLine ();
            CheckAndDo(!isWarning,
                () =>
                {
                    GetNodeByIndex<GetPositionNode>(ref getPositionNode, ref HosterData.GetPositionIndex, true, "GetPositionIndex");
                    GetNodeByIndex<GetDirectionNode>(ref getDirectionNode, ref HosterData.GetDirectionIndex, true, "GetDirectionIndex");
                },
                () => { HosterData.GetPositionIndex = -1; HosterData.GetDirectionIndex = -1; }
                );
            DrawLine();

            CheckAndDo(isPingPong && !HosterData.IsPingPongLookTo,
                () => HosterData.IsPingPongWhirl = EditorGUITool.Toggle("IsPingPongWhirl", HosterData.IsPingPongWhirl),
                () => HosterData.IsPingPongWhirl = false);
            CheckAndDo(isPingPong && !HosterData.IsPingPongWhirl,
                () => HosterData.IsPingPongLookTo = EditorGUITool.Toggle("IsPingPongLookTo", HosterData.IsPingPongLookTo),
                () => HosterData.IsPingPongLookTo = false);
            CheckAndDo(isPingPong,
                () => HosterData.IsPingPongCounterCharge = EditorGUITool.Toggle("IsPingPongCounterCharge", HosterData.IsPingPongCounterCharge),
                () => HosterData.IsPingPongCounterCharge = false);
            if (isPingPong) HosterData.Once = false;

            DrawBulletInfo(!isPingPong);

            CheckAndDo(!isWarning,
                ()=> HosterData.Group = EditorGUITool.Toggle("Group", HosterData.Group),
                () => HosterData.Group = false);

            CheckAndDo(isGroup,
            () =>
            {
                HosterData.AngleStep = EditorGUITool.FloatField("AngleStep", HosterData.AngleStep);
                HosterData.TimeStep = TimeFrameField("TimeStep", HosterData.TimeStep);
                HosterData.StepCount = Mathf.Min(64, EditorGUITool.IntField("StepCount (max:64)", HosterData.StepCount));
            },
            () => { HosterData.AngleStep = 0; HosterData.TimeStep = 0; HosterData.StepCount = 0; });
            DrawLine();

            HosterData.StickyTime = TimeFrameField ("StickyTime", HosterData.StickyTime);
            DrawLine ();

            CheckAndDo(HosterData.WithCollision && !isTrap,
                () => DrawHitParam<XBulletData>(HosterData));

            DrawLine ();

            HosterData.FxStickOnGround = EditorGUITool.Toggle ("FxStickOnGround", HosterData.FxStickOnGround);
            HosterData.KnockBackWithHitterDir = EditorGUITool.Toggle ("KnockBackWithHitterDir", HosterData.KnockBackWithHitterDir);

            ClientEcsData.NextNodeType nextNodeType = (ClientEcsData.NextNodeType)HosterData.NextNodeType;

            nextNodeType = (ClientEcsData.NextNodeType)EditorGUILayout.EnumPopup("NextNodeType", nextNodeType);
            HosterData.NextNodeType = (int)nextNodeType;
            switch (nextNodeType)
            {
                case ClientEcsData.NextNodeType.MatEffect:
                    GetNodeByIndex<ShaderEffectNode>(ref nextNode, ref HosterData.NextNode, true, "Next");
                    break;
                case ClientEcsData.NextNodeType.SpecialAction:
                    GetNodeByIndex<SpecialActionNode>(ref nextNode, ref HosterData.NextNode, true, "Next");
                    break;
            }

            EditorGUILayout.EndScrollView ();
        }

        private void DrawBulletInfo (bool showOnce)
        {
            HosterData.TerrainCheck = EditorGUILayout.Toggle("TerrainCheck", HosterData.TerrainCheck);
            HosterData.WithCollision = EditorGUITool.Toggle ("WithCollision", HosterData.WithCollision);
            if (HosterData.WithCollision)
            {
                HosterData.CollisionAt = TimeFrameField ("CollisionAt", HosterData.CollisionAt);

                switch ((XBulletType) HosterData.Type)
                {
                    case XBulletType.Cuboid:
                        HosterData.HalfLength = EditorGUITool.FloatField ("HalfLength : ", HosterData.HalfLength);
                        HosterData.Radius = EditorGUITool.FloatField ("HalfWidth : ", HosterData.Radius);
                        break;
                    case XBulletType.Ring:
                        HosterData.Radius = EditorGUITool.FloatField("Radius : ", HosterData.Radius);
                        HosterData.HalfLength = HosterData.Radius;
                        HosterData.InitDis = EditorGUITool.FloatField("InitDis : ", HosterData.InitDis);
                        break;
                    default:
                        HosterData.Radius = EditorGUITool.FloatField ("Radius : ", HosterData.Radius);
                        HosterData.HalfLength = HosterData.Radius;
                        break;
                }
                HosterData.HalfHeight = EditorGUITool.FloatField ("HalfHeight : ", HosterData.HalfHeight);

                if (showOnce) HosterData.Once = EditorGUITool.Toggle ("Once", HosterData.Once);
                if (!HosterData.Once)
                {
                    HosterData.HurtCount = Mathf.Max (1, EditorGUITool.IntField ("HurtCount", HosterData.HurtCount));
                    HosterData.HurtTime = Mathf.Max((XBulletType)HosterData.Type == XBulletType.Trap ? 0f : 0.1f, TimeFrameField("HurtTime", HosterData.HurtTime));
                }
                DrawLine ();
                HosterData.EffectID = EditorGUITool.Popup ("EffectID", HosterData.EffectID, EditorGUITool.Translate<EditorEcs.XHitSlot> ());
                ParamTemplate("TableIndex", HosterData,
                () => { HosterData.TableIndex = EditorGUITool.IntField("TableIndex", HosterData.TableIndex); });
                DrawLine ();
                GetNodeByIndex<AudioNode> (ref audioNode, ref HosterData.AudioIndex, true, "HitAudioIndex");
                GetNodeByIndex<FxNode> (ref fxNode, ref HosterData.FxIndex, true, "HitFxIndex");
            }
            GetNodeByIndex<BulletNode> (ref bulletNode, ref HosterData.EndBulletIndex, true, "EndBulletIndex");
            GetNodeByIndex<BulletNode>(ref triggerBulletNode, ref HosterData.TriggerEndBulletIndex, true, "TriggerEndBulletIndex");
            DrawLine();
            GetNodeByIndex<MobUnitNode>(ref mobUnitNode, ref HosterData.EndMobUnitIndex, true, "EndMobUnitIndex");
            GetNodeByIndex<MobUnitNode>(ref triggerMobUnitNode, ref HosterData.TriggerEndMobUnitIndex, true, "TriggerEndMobUnitIndex");

            if (HosterData.Once)
            {
                HosterData.CollisionOnly = EditorGUITool.Toggle("CollisionOnly", HosterData.CollisionOnly);
            }
            else
            {
                HosterData.CollisionOnly = false;
            }

            DrawLine ();
        }

        public override void BuildDataFinish ()
        {
            base.BuildDataFinish ();

            ClientEcsData.NextNodeType nextNodeType = (ClientEcsData.NextNodeType)HosterData.NextNodeType;
            switch (nextNodeType)
            {
                case ClientEcsData.NextNodeType.MatEffect:
                    GetNodeByIndex<ShaderEffectNode>(ref nextNode, ref HosterData.NextNode);
                    break;
                case ClientEcsData.NextNodeType.SpecialAction:
                    GetNodeByIndex<SpecialActionNode>(ref nextNode, ref HosterData.NextNode);
                    break;
            }

            GetNodeByIndex<CurveNode>(ref curveNode, ref HosterData.CurveDataIndex);
            GetNodeByIndex<CameraShakeNode> (ref selfShakeNode, ref HosterData.BulletShakeIndex);
            if (HosterData.WithCollision)
            {
                GetNodeByIndex<AudioNode> (ref audioNode, ref HosterData.AudioIndex);
                GetNodeByIndex<FxNode> (ref fxNode, ref HosterData.FxIndex);
            }
            GetNodeByIndex<AudioNode> (ref selfAudioNode, ref HosterData.BulletAudioIndex);
            GetNodeByIndex<BulletNode> (ref bulletNode, ref HosterData.EndBulletIndex);
            GetNodeByIndex<BulletNode>(ref triggerBulletNode, ref HosterData.TriggerEndBulletIndex);

            GetNodeByIndex<MobUnitNode>(ref mobUnitNode, ref HosterData.EndMobUnitIndex);
            GetNodeByIndex<MobUnitNode>(ref triggerMobUnitNode, ref HosterData.TriggerEndMobUnitIndex);

            GetNodeByIndex<GetPositionNode>(ref getPositionNode, ref HosterData.GetPositionIndex);
            GetNodeByIndex<GetDirectionNode>(ref getDirectionNode, ref HosterData.GetDirectionIndex);
        }

        public override bool CompileCheck ()
        {
            if (!base.CompileCheck ()) return false;

            CheckDeep(this, 0);

            return true;
        }

        private bool CheckDeep(BaseSkillNode node, int deep)
        {
            if (deep >= Xuthus.BULLET_GEN_MAX)
            {
                LogError("BulletNode_" + HosterData.Index + ",生成子弹代数超过" + Xuthus.BULLET_GEN_MAX + "！！！");
                return false;
            }

            if (deep > 0)
            {
                if (node.GetHosterData<XBulletData>().LifeTimeParamIndex != -1 ||
                    node.GetHosterData<XBulletData>().VelocityParamIndex != -1)
                {
                    LogError("BulletNode_" + node.GetHosterData<XBulletData>().Index + ",生成子弹不能使用Param！！！");
                    return false;
                }
            }

            if (node.GetHosterData<XBulletData>().EndBulletIndex != -1)
            {
                BaseSkillNode childNode = null;
                GetNodeByIndex<BulletNode>(ref childNode, ref node.GetHosterData<XBulletData>().EndBulletIndex);
                if (!CheckDeep(childNode, deep + 1)) return false;
            }
            if (node.GetHosterData<XBulletData>().TriggerEndBulletIndex != -1)
            {
                BaseSkillNode childNode = null;
                GetNodeByIndex<BulletNode>(ref childNode, ref node.GetHosterData<XBulletData>().TriggerEndBulletIndex);
                if (!CheckDeep(childNode, deep + 1)) return false;
            }

            return true;
        }

        public override void ScanPolicy (CFEngine.OrderResList result, CFEngine.ResItem item)
        {
            if (!string.IsNullOrEmpty (HosterData.BulletPath))
            {
                if (HosterData.BulletPath.Contains(" "))
                {
                    result.sb.AppendLine(string.Format("name contains space skill {0} {1}", item.nameWithExt, HosterData.BulletPath));
                    result.Add(item, System.IO.Path.GetFileName(HosterData.BulletPath) + ".prefab", ResourecePath + HosterData.BulletPath + ".prefab");
                }
            }                
        }

        public override void CalcTriggerTime()
        {
            base.CalcTriggerTime();

            DFSTriggerTime(this, HosterData.AudioIndex);
            DFSTriggerTime(this, HosterData.BulletAudioIndex);
            DFSTriggerTime(this, HosterData.BulletShakeIndex);
            DFSTriggerTime(this, HosterData.CurveDataIndex);
            DFSTriggerTime(this, HosterData.EndBulletIndex);
            DFSTriggerTime(this, HosterData.FxIndex);
            DFSTriggerTime(this, HosterData.TriggerEndBulletIndex);
            DFSTriggerTime(this, HosterData.EndMobUnitIndex);
            DFSTriggerTime(this, HosterData.TriggerEndMobUnitIndex);
            DFSTriggerTime(this, HosterData.NextNode);
        }
    }
}