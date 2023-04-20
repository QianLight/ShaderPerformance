using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CFEngine.Editor
{
    public class SkillScanPolicy : ScanPolicy
    {
        public override string ScanType
        {
            get { return "SkillPackage"; }
        }
        public override string ResExt
        {
            get { return "*.bytes"; }
        }

        public override ResItem Scan(string name, string path, OrderResList result, ResScanConfig config)
        {
            var skill = result.Add(null, name, path);
            try
            {
                SkillGraph graph = new SkillGraph();
                graph.OpenData(path);
                for (int i = 0; i < graph.widgetList.Count; ++i)
                {
                    (graph.widgetList[i] as EditorNode.BaseSkillNode).ScanPolicy(result, skill);
                }
            }
            catch (Exception e)
            {
                DebugLog.AddErrorLog(name + "_skill_" + e.StackTrace);
            }
            return skill;
        }
    }

    public class HitScanPolicy : ScanPolicy
    {
        public override string ScanType
        {
            get { return "HitPackage"; }
        }
        public override string ResExt
        {
            get { return "*.bytes"; }
        }

        public override ResItem Scan(string name, string path, OrderResList result, ResScanConfig config)
        {
            var hit = result.Add(null, name, path);
            try
            {
                BehitGraph graph = new BehitGraph();
                graph.OpenData(path);
                for (int i = 0; i < graph.widgetList.Count; ++i)
                {
                    (graph.widgetList[i] as EditorNode.BaseSkillNode).ScanPolicy(result, hit);
                }
            }
            catch (Exception e)
            {
                DebugLog.AddErrorLog(name + "_hit_" + e.StackTrace);
            }
            return hit;
        }
    }
}