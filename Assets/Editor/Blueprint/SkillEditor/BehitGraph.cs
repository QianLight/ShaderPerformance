using EcsData;
using EditorNode;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;
using System.Reflection;
using System.IO;
using VirtualSkill;

public class BehitGraph : ConfigGraph<XHitData>
{
    private static GUIContent
        SaveButtonContent = new GUIContent("Save", "Save to file");
    private static GUIContent
        LoadButtonContent = new GUIContent("Load", "Load from file");

    public override string[] NodeNameArray
    {
        get { return nodeNameArray; }
    }

    private string[] nodeNameArray ={
        "Animation",
        "Fx",
        "FxProxy",
        "AudioProxy",
        "KnockedBack",
        "Audio",
        "ActionStatus",
        "HitStatus",
        "Freeze",
        "QTE",
        "QTEDisplay",
        "Buff",
        "Param",
        "AdjustHitDirection",
        "SpecialAction",
        "Message",

        "Condition",
        "MultiCondition",
        "Timer",
        "Empty",
        "End",
        "Loop",
        "Continue",
        "Break",
        "Switch",
        "Until",
        "ScriptTrans",
        "GetPosition",
        "GetDirection",
        "Curve",
    };

    public override string BackupFileName { get { return "BehitBackup.bytes"; } }

    protected override string RuntimeCacheName { get { return "Behit"; } }

    protected override string TmpKey => "_Hit";

    public BehitGraph()
    {

    }

    public override void DrawDataInspector()
    {
        if (selectNode is BaseSkillNode) return;

        base.DrawDataInspector();
    }
}
