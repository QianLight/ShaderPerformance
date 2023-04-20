using System;
using System.Collections.Generic;
using BluePrint;
using CFEngine;
using EcsData;
using UnityEditor;
using UnityEngine;

namespace EditorNode
{
    public class FxNode : TimeTriggerNode<XFxData>
    {
        private GameObject FxObject;
        private GameObject originBone;
        private GameObject targetBone;
        private BaseSkillNode nextNode = null;

        private BaseSkillNode getPositionNode = null;
        private BaseSkillNode getDirectionNode = null;

        public override void Init (BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init (Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header9";

            if (GetRoot.NeedInitRes)
            {
                // var sfxName = EngineUtility.GetFileName (HosterData.FxPath);
                if (nodeEditorData != null && !string.IsNullOrEmpty(nodeEditorData.CustomData1))
                {
                    FxObject = AssetDatabase.LoadAssetAtPath(nodeEditorData.CustomData1, typeof(GameObject)) as GameObject;
                }
                else
                {
                    var sfxEditorAsset = SFXWrapper.GetSFXEditorAsset(HosterData.FxPath);
                    if (sfxEditorAsset != null)
                    {
                        FxObject = sfxEditorAsset.srcSFX;
                    }
                    else
                    {
                        DebugLog.AddErrorLog2("fx prefab not find:{0}", HosterData.FxPath);
                        FxObject = AssetDatabase.LoadAssetAtPath(ResourecePath + HosterData.FxPath + ".prefab", typeof(GameObject)) as GameObject;
                    }
                }
            }
        }

        private void DrawFlag(string name,uint flag)
        {
            bool enable = (HosterData.Flag & flag) != 0;
            enable = EditorGUITool.Toggle(name, enable);
            if (enable)
            {
                HosterData.Flag |= flag;
            }
            else
            {
                HosterData.Flag &= ~(flag);
            }
        }
        public override void DrawDataInspector ()
        {
            base.DrawDataInspector ();
            HosterData.PlayerTrigger = EditorGUITool.Toggle("PlayerTrigger", HosterData.PlayerTrigger);
            HosterData.Tag = EditorGUITool.IntField("Tag", HosterData.Tag);
            HosterData.LifeTime = TimeFrameField ("LifeTime: ", HosterData.LifeTime);
            FxObject = EditorGUITool.ObjectField ("Fx", FxObject, typeof (GameObject), false) as GameObject;
            if (FxObject != null)
            {
                HosterData.FxPath = FxObject.name.ToLower ();
                if (nodeEditorData != null)
                {
                    nodeEditorData.CustomData1 = AssetDatabase.GetAssetPath (FxObject);
                }
                // HosterData.FxPath = AssetDatabase.GetAssetPath (FxObject).Replace (ResourecePath, "");
                // HosterData.FxPath = HosterData.FxPath.Remove (HosterData.FxPath.LastIndexOf ('.'));
            }
            else HosterData.FxPath = "";
            EditorGUITool.LabelField (string.Format ("FxName:{0}", HosterData.FxPath));

            HosterData.AttachTarget = EditorGUITool.Toggle("AttackTarget", HosterData.AttachTarget);

            if (!HosterData.AttachTarget) BoneFieldByPath("Bone", ref originBone, ref HosterData.Bone);
            else HosterData.Bone = "";

            HosterData.isLineRenderer = EditorGUITool.Toggle ("isLineRenderer", HosterData.isLineRenderer);

            EditorGUITool.Vector3Field ("Scale", ref HosterData.ScaleX, ref HosterData.ScaleY, ref HosterData.ScaleZ);
            EditorGUITool.Vector3Field ("Offset", ref HosterData.OffsetX, ref HosterData.OffsetY, ref HosterData.OffsetZ);
            EditorGUITool.Vector3Field ("Rotate", ref HosterData.RotateX, ref HosterData.RotateY, ref HosterData.RotateZ);

            if (!HosterData.isLineRenderer)
            {
                HosterData.StickOnGround = EditorGUITool.Toggle ("StickOnGround", HosterData.StickOnGround);
                HosterData.Follow = EditorGUITool.Toggle ("Follow", HosterData.Follow);
            }
            else
            {
                BoneFieldByPath ("TargetBone", ref targetBone, ref HosterData.TargetBone);
            }

            HosterData.DestroyDelay = TimeFrameField ("Destroy Delay: ", HosterData.DestroyDelay);

            DrawFlag("NotKillWithEntity", EcsData.XFxData.Flag_NotKillWithEntity);
            DrawFlag("NeedTrans", EcsData.XFxData.Flag_NeedTrans);

            ClientEcsData.NextNodeType nextNodeType = (ClientEcsData.NextNodeType)HosterData.NextNodeType;

            nextNodeType = (ClientEcsData.NextNodeType)EditorGUILayout.EnumPopup("NextNodeType", nextNodeType);
            HosterData.NextNodeType = (int)nextNodeType;
            switch(nextNodeType)
            {
                case ClientEcsData.NextNodeType.MatEffect:
                    GetNodeByIndex<ShaderEffectNode>(ref nextNode, ref HosterData.NextNode, true, "Next");
                    break;
                case ClientEcsData.NextNodeType.SpecialAction:
                    GetNodeByIndex<SpecialActionNode>(ref nextNode, ref HosterData.NextNode, true, "Next");
                    break;
            }

            if (!HosterData.AttachTarget)
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

        private void DrawBone (ref GameObject b, ref string path)
        {

        }

        protected void BoneFieldByPath (string label, ref GameObject bone, ref string path)
        {
            if (bone == null && !String.IsNullOrEmpty (path))
            {
                bone = GameObject.Find ("Player/" + path);
            }
            bone = EditorGUITool.ObjectField (label, bone, typeof (GameObject), true) as GameObject;
            if (bone != null)
            {
                Transform tmp = bone.transform;
                string BonePath = "";
                while (tmp.parent != null)
                {
                    BonePath = tmp.name + (BonePath != "" ? "/" : "") + BonePath;
                    tmp = tmp.parent;
                }
                path = BonePath;
            }
            EditorGUILayout.BeginHorizontal ();
            EditorGUITool.LabelField (path);
            if (GUILayout.Button ("Clear"))
            {
                bone = null;
                path = "";
            }
            EditorGUILayout.EndHorizontal ();
        }

        public override void ScanPolicy (CFEngine.OrderResList result, CFEngine.ResItem item)
        {
            if (!string.IsNullOrEmpty (HosterData.FxPath))
            {
                if (HosterData.FxPath.Contains(" "))
                {
                    result.sb.AppendLine(string.Format("name contains space skill {0} {1}", item.nameWithExt, HosterData.FxPath));
                }
                result.Add(item, System.IO.Path.GetFileName(HosterData.FxPath) + ".prefab", ResourecePath + HosterData.FxPath + ".prefab");
            }
                
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
            

            GetNodeByIndex<GetPositionNode>(ref getPositionNode, ref HosterData.GetPositionIndex);
            GetNodeByIndex<GetDirectionNode>(ref getDirectionNode, ref HosterData.GetDirectionIndex);
            if (nodeEditorData != null)
            {
                nodeEditorData.CustomData1 = AssetDatabase.GetAssetPath (FxObject);
            }
        }

        public override bool CompileCheck()
        {
            if (!base.CompileCheck()) return false;

            if (string.IsNullOrEmpty(HosterData.FxPath))
            {
                LogError("FxNode_" + HosterData.Index + " FxPath²»ÄÜÎª¿Õ");
                return false;
            }

            return true;
        }

        public override void CalcTriggerTime()
        {
            base.CalcTriggerTime();

            DFSTriggerTime(this, HosterData.NextNode);
        }
    }
}