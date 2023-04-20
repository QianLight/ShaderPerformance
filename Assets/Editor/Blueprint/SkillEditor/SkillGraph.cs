using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using BluePrint;
using CFEngine;
using CFEngine.Editor;
using EcsData;
using EditorEcs;
using EditorNode;
using UnityEditor;
using UnityEngine;
using VirtualSkill;

public class SkillGraph : ConfigGraph<XSkillData>
{
    private static GUIContent
    SaveButtonContent = new GUIContent ("Save", "Save to file");
    private static GUIContent
    LoadButtonContent = new GUIContent ("Load", "Load from file");

    public override string[] NodeNameArray
    {
        get { return nodeNameArray; }
    }
    private string[] nodeNameArray = new string[]
    {
        "Animation",
        "Result",
        "Fx",
        "Charge",
        "AimTarget",
        "QTE",
        "QTEDisplay",
        "Bullet",
        "Audio",
        "ActionStatus",
        "Warning",
        "TargetSelect",
        "Freeze",
        "CameraMotion",
        "CameraStretch",
        "CameraShake",
        "CameraLayerMask",
        "CameraPostEffect",
        // "ShaderEffect",
        "MobUnit",
        "Buff",
        "Rotate",
        "Param",
        "Message",
        "Random",
        "SpecialAction",
        "LookAt",
        "InterruptReturn",

        "Condition",
        "MultiCondition",
        "Timer",
        "Empty",
        "End",
        "Loop",
        "Continue",
        "Break",
        "Switch",
        "PreCondition",
        "Until",
        "ScriptTrans",
        "GetPosition",
        "GetDirection",
        "Curve",
        // "MatEffect",
    };
    public override string BackupFileName { get { return "SkillBackup.bytes"; } }

    protected override string RuntimeCacheName { get { return "Skill"; } }

    protected override string TmpKey => "_Skill";

    public SkillGraph ()
    {
        
    }

    public SkillEditorKeyboardData KeyboardData { get { return keyboardData; } }

    public int sfxType;

    private BaseSkillNode skillAttackField = null;
    private BaseSkillNode preConditionNode = null;

    protected override void InitGraphData ()
    {
        base.InitGraphData ();

        skillAttackField = null;
        preConditionNode = null;
    }

    public override void DrawDataInspector ()
    {
        if (selectNode is BaseSkillNode) return;

        base.DrawDataInspector ();

        if (configData != null)
        {
            configData.SkillType = (int) (XSkillType)EditorGUITool.Popup ("SkillType", configData.SkillType, EditorGUITool.Translate<XSkillType> ());

            //GetNodeByIndex<ResultNode> (ref skillAttackField, ref configData.SkillAttackField, true, "SkillAttackField");

            if (configData.PreConditionData.Count != 0)
                configData.PreCondition = configData.PreConditionData[0].Index;
            GetNodeByIndex<PreConditionNode> (ref preConditionNode, ref configData.PreCondition, true, "PreConditionNode");

            if (!configData.TriggerCDAtEnd) configData.PhaseCount = Mathf.Max (1, EditorGUITool.IntField ("PhaseCount", configData.PhaseCount));
            else configData.PhaseCount = 1;

            configData.TriggerCDAtEnd = EditorGUITool.Toggle ("TriggerCDAtEnd", configData.TriggerCDAtEnd);
            configData.PressEffect = EditorGUITool.Toggle("PressEffect", configData.PressEffect);
            configData.DisableGlobalRotate = EditorGUITool.Toggle("DisableGlobalRotate", configData.DisableGlobalRotate);
            configData.BlockSens = Mathf.Max(0, EditorGUITool.IntField("BlockSens", configData.BlockSens));

            EditorGUI.BeginDisabledGroup(true);
            EditorGUITool.Toggle("HasInterruptReturn", configData.HasInterruptReturn);
            EditorGUI.EndDisabledGroup();

            uint hash = CFUtilPoolLib.XCommon.singleton.XHash (configData.Name);

            CFUtilPoolLib.SeqListRef<int> qte = XSkillReader.GetSkillQTE (hash);
            if (qte.Count != 0)
            {
                EditorGUITool.LabelField ("Qte: ");

                for (int i = 0; i < qte.Count; ++i)
                {
                    EditorGUITool.LabelField ("QteID: " + qte[i, 0] + "\t\tSlotID: " + qte[i, 1]);
                }
            }

            configData.NeedCheckCameraEffectWhenCast = EditorGUITool.Toggle("NeedCheckCameraEffectWhenCast", configData.NeedCheckCameraEffectWhenCast);

            SoloCamera = EditorGUITool.Toggle("SoloCamera", SoloCamera);
            if (SoloCamera)
            {
                SoloCameraObj = EditorGUITool.ObjectField("SoloCameraObj", SoloCameraObj, typeof(GameObject), false) as GameObject;
                if (SkillHoster.GetHoster != null && SkillHoster.GetHoster.CSoloMix != null)
                {
                    if (GUILayout.Button("SwitchFollowAndLookAt"))
                    {
                        if (SkillHoster.GetHoster.CSoloMix != null)
                        {
                            SkillHoster.targetControl = true;
                            Transform tmp = SkillHoster.GetHoster.CSoloMix.LookAt;
                            SkillHoster.GetHoster.CSoloMix.LookAt = SkillHoster.GetHoster.CSoloMix.Follow;
                            SkillHoster.GetHoster.CSoloMix.Follow = tmp;

                            bool isPlayer = SkillHoster.PlayerIndex==0;
                            if (isPlayer)
                            {
                                SkillHoster.GetHoster.EntityDic[SkillHoster.GetHoster.Target].mHeightOffset = SkillHoster.GetHoster.EntityDic[SkillHoster.GetHoster.Target].presentData.BoundHeight * 0.72f;
                                SkillHoster.GetHoster.EntityDic[SkillHoster.PlayerIndex].mHeightOffset = 0;
                            }
                            else
                            {
                                SkillHoster.GetHoster.EntityDic[SkillHoster.GetHoster.Target].mHeightOffset = 0;
                                SkillHoster.GetHoster.EntityDic[SkillHoster.PlayerIndex].mHeightOffset = SkillHoster.GetHoster.EntityDic[SkillHoster.PlayerIndex].presentData.BoundHeight*0.72f;
                            }

                            SkillHoster.GetHoster.SoloFollowIndex = SkillHoster.GetHoster.SoloFollowIndex == SkillHoster.PlayerIndex ? SkillHoster.GetHoster.Target : SkillHoster.PlayerIndex;
                            //SkillHoster.GetHoster.CSoloMix.SetTableParamData(SkillHoster.GetHoster.EntityDic[SkillHoster.GetHoster.SoloFollowIndex].presentData.CameraScaleParam);
                        }
                    }
                }
            }

            if (SkillHoster.GetHoster != null)
            {
                EditorGUILayout.Space ();
                SkillHoster.GetHoster.LagType = EditorGUITool.Popup ("Lag", SkillHoster.GetHoster.LagType, SkillHoster.GetHoster.NetStr);
                switch (SkillHoster.GetHoster.LagType)
                {
                    case 1:
                        SkillHoster.GetHoster.Lag = 0.05f;
                        break;
                    case 2:
                        SkillHoster.GetHoster.Lag = 0.1f;
                        break;
                    case 3:
                        SkillHoster.GetHoster.Lag = 0.2f;
                        break;
                    default:
                        SkillHoster.GetHoster.Lag = 0f;
                        break;
                }
                SkillHoster.GetHoster.FluctuationsType = EditorGUITool.Popup ("Fluctuations", SkillHoster.GetHoster.FluctuationsType, SkillHoster.GetHoster.NetStr);
                switch (SkillHoster.GetHoster.FluctuationsType)
                {
                    case 1:
                        SkillHoster.GetHoster.Fluctuations = 0.05f;
                        break;
                    case 2:
                        SkillHoster.GetHoster.Fluctuations = 0.1f;
                        break;
                    case 3:
                        SkillHoster.GetHoster.Fluctuations = 0.2f;
                        break;
                    default:
                        SkillHoster.GetHoster.Fluctuations = 0f;
                        break;
                }
                EditorGUI.BeginChangeCheck();
                graphConfigData.tag = EditorGUITool.TextField ("PartTag", graphConfigData.tag);
                if(EditorGUI.EndChangeCheck())
                {
                    SkillHoster.GetHoster.partTag = graphConfigData.tag;
                }
            }

            string[] option = new string[EditorSFXData.instance.profileLevels];
            for (int i = 0; i < EditorSFXData.instance.profileLevels; i++)
            {
                option[i] = EditorSFXData.instance.settingType[i].exampleInfo;
            }
            sfxType = EditorGUITool.Popup("技能类型", sfxType, option);
        }
    }

    public override void BuildDataFinish()
    {
        base.BuildDataFinish();

        GetNodeByIndex<ResultNode>(ref skillAttackField, ref configData.SkillAttackField);

        GetNodeByIndex<PreConditionNode>(ref preConditionNode, ref configData.PreCondition);

        configData.HasInterruptReturn = configData.InterruptReturnData.Count != 0;
    }
}