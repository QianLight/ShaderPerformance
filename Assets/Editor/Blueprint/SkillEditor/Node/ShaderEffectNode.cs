using System;
using System.Collections;
using System.Collections.Generic;
using BluePrint;
using CFEngine;
using CFEngine.Editor;
using EcsData;
using UnityEditor;
using UnityEngine;
using VirtualSkill;
namespace EditorNode
{
    public class ShaderEffectNode : TimeTriggerNode<XShaderEffectData>
    {
        private static EffectPreviewContext previewContext;

        private BaseSkillNode effectNode = null;
        private MatEffectData med;
        private MatEffectNodeInfo matEffectNode;
        private static GUIStyle redText;

        public static GUIStyle GetRedStyle ()
        {
            if (redText == null)
            {
                redText = new GUIStyle (EditorStyles.boldLabel);
                redText.normal.textColor = new Color (1, 0, 0);
                redText.fixedWidth = 100;
            }
            return redText;
        }

        public override void Init (BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init (Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header8";
            //BuildData ();
        }

        public void BuildData ()
        {
            if (matEffectNode == null)
            {
                matEffectNode = EffectConfig.instance.FindNode (HosterData.uniqueID);
            }
            if (med == null)
            {
                med = new MatEffectData ();
                med.x = HosterData.x;
                med.y = HosterData.y;
                med.z = HosterData.z;
                med.w = HosterData.w;
                med.param = HosterData.keywordMask;
                med.partMask = HosterData.partMask;
                string ext = ResObject.GetExt((byte)med.x);
                med.path = GetRoot.GetPath(HosterData.path, ext);
                EffectConfig.InitResData(med, HosterData.path, ext);
            }
        }
        public override void DrawDataInspector ()
        {
            base.DrawDataInspector ();

            HosterData.LifeTime = TimeFrameField ("LifeTime", HosterData.LifeTime);
            HosterData.EndWithSkill = EditorGUITool.Toggle ("EndWithSkill", HosterData.EndWithSkill);
            BuildData ();
            if (matEffectNode != null)
            {
                EditorGUILayout.BeginHorizontal ();
                EditorGUITool.LabelField ("EffectType", GUILayout.MaxWidth (100));
                EditorGUITool.LabelField (matEffectNode.et.effectName, GetRedStyle (), GUILayout.MaxWidth (100));
                EditorGUILayout.EndHorizontal ();
            }


             //if (matEffectNode.et.effectName == "Fade")
            {
                EditorGUILayout.BeginHorizontal();
                HosterData.HiddenShadow  = EditorGUITool.Toggle("HiddenShadow", HosterData.HiddenShadow );
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal ();
            GetNodeByIndex<MatEffectNode> (ref effectNode, ref HosterData.NextNode, false, "Next");
            EditorGUITool.LabelField ("Next", GUILayout.MaxWidth (100));
            EditorGUITool.LabelField (HosterData.NextNode.ToString (), GUILayout.MaxWidth (100));
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            EditorGUITool.LabelField ("UniqueID", GUILayout.MaxWidth (100));
            EditorGUITool.LabelField (HosterData.uniqueID.ToString (), GUILayout.MaxWidth (100));
            EditorGUILayout.EndHorizontal ();

            EditorGUILayout.BeginHorizontal ();
            EditorGUITool.LabelField ("Priority", GUILayout.MaxWidth (100));
            HosterData.Priority = EditorGUILayout.IntField (HosterData.Priority, GUILayout.MaxWidth (200));
            EditorGUILayout.EndHorizontal ();

            if (matEffectNode.et.effectName == "Hide")
            {
                HosterData.FadeIn = 0;
                HosterData.FadeOut = 0;
            }
            else
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUITool.LabelField("FadeIn", GUILayout.MaxWidth (100));
                HosterData.FadeIn = EditorGUILayout.Slider (HosterData.FadeIn, 0.0f, 2, GUILayout.MaxWidth (200));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUITool.LabelField ("FadeOut", GUILayout.MaxWidth (100));
                HosterData.FadeOut = EditorGUILayout.Slider (HosterData.FadeOut, 0.0f, 2, GUILayout.MaxWidth (200));
                EditorGUILayout.EndHorizontal();
            }
            
            
            

            EditorGUILayout.BeginHorizontal ();
            EditorGUITool.LabelField (string.Format ("TotalTime(LifeTime+FadeOut):{0}", HosterData.FadeOut + HosterData.LifeTime), GUILayout.MaxWidth (300));
            EditorGUILayout.EndHorizontal ();

            EditorGUI.BeginChangeCheck ();
            PartConfig.instance.OnPartGUI (Root.graphConfigData.tag, ref med.partMask);
            if (EditorGUI.EndChangeCheck ())
            {
                HosterData.partMask = med.partMask;
            }
            if (matEffectNode != null)
            {
                EditorGUI.BeginChangeCheck ();
                matEffectNode.node.OnGUI (med);
                if (EditorGUI.EndChangeCheck ())
                {
                    HosterData.path = med.asset != null?med.asset.name.ToLower (): "";
                    HosterData.x = med.x;
                    HosterData.y = med.y;
                    HosterData.z = med.z;
                    HosterData.w = med.w;
                    HosterData.keywordMask = med.param;
                }
            }

            HosterData.EndSame = EditorGUITool.Toggle("EndSame", HosterData.EndSame);

            EditorGUILayout.BeginHorizontal ();
            if (GUILayout.Button ("Fix", GUILayout.MaxWidth (80)))
            {
                var megNode = EffectConfig.instance.FindEffectGraph (HosterData.uniqueID);
                if (megNode != null)
                {
                    if (megNode.nodeList.Count > 0)
                    {
                        var n = megNode.nodeList[0];
                        if (n.node != null)
                        {
                            var graph = GetRoot as SkillGraph;
                            float xoffset = 160;
                            EditorNode.MatEffectNode lasteffectNode = null;
                            int nextNodeIndex = HosterData.NextNode;
                            for (int i = 1; i < megNode.nodeList.Count; ++i)
                            {
                                n = megNode.nodeList[i];
                                var current = graph.GetNodeByIndex (nextNodeIndex) as MatEffectNode;
                                if (current == null)
                                {
                                    current = CreateMatEffectNode (graph, this, xoffset, n, true);
                                    if (lasteffectNode == null)
                                    {
                                        HosterData.NextNode = current.HosterData.Index;
                                        lasteffectNode = current;
                                    }
                                    else
                                    {
                                        lasteffectNode.HosterData.NextNode = current.HosterData.Index;
                                    }
                                    nextNodeIndex = -1;
                                }
                                else
                                {
                                    lasteffectNode = current;
                                    nextNodeIndex = current.HosterData.NextNode;
                                }
                                xoffset += 140;
                            }
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal ();
        }

        public override void BuildDataFinish ()
        {
            base.BuildDataFinish ();
            GetNodeByIndex<MatEffectNode> (ref effectNode, ref HosterData.NextNode);
        }
        public override void PreBuild() 
        {
            base.PreBuild();
            if (med != null)
            {
                if(!string.IsNullOrEmpty(med.path)&&
                    !string.IsNullOrEmpty(HosterData.path))
                {

                    string ext = ResObject.GetExt((byte)med.x);
                    GetRoot.resMap[HosterData.path + ext] = med.path;
                }
            }
        }

        protected override bool OnMouseDrag (Event e)
        {
            base.OnMouseDrag (e);
            if (effectNode != null)
            {
                (effectNode as MatEffectNode).OnRootDrag (e);
            }
            return true;
        }
        public override void OnDeleteClicked ()
        {
            base.OnDeleteClicked ();
            if (effectNode != null)
            {
                (effectNode as MatEffectNode).OnDeleteClicked ();
            }
        }



        public static void AddMenu(BaseSkillGraph graph, GenericMenu menu, Event e, string nodeName)
        {
            menu.AddItem(
                new GUIContent("AddAction/ShaderEffect/" + nodeName),
                false,
                (object o) => { AddDynamicNodeInGraphTemplate(graph, nodeName, o); }, e);
        }
        private static EditorNode.MatEffectNode CreateMatEffectNode (
            BaseSkillGraph graph,
            BaseSkillNode parentNode,
            float xoffset, NodeIndex n, bool absPos = false)
        {
            var configData = graph.GetConfigData<EcsData.XConfigData>();
            var matEffectData = (configData is XSkillData) ? (configData as XSkillData).MatEffectData : (configData as XHitData).MatEffectData;
            var effectNode = graph.AddNodeInGraphByScript<XMatEffectData, EditorNode.MatEffectNode>
                (parentNode.Bounds.position + new Vector2 (xoffset, 0),
                    ref matEffectData, true);
            effectNode.HosterData.uniqueID = n.uniqueID;
            n.node.InitData (ref effectNode.HosterData.x,
                ref effectNode.HosterData.y,
                ref effectNode.HosterData.z,
                ref effectNode.HosterData.w,
                ref effectNode.HosterData.path,
                ref effectNode.HosterData.keywordMask);
            return effectNode;
        }
        private static void AddDynamicNodeInGraphTemplate (BaseSkillGraph graph, string effectName, object o)
        {
            var e = o as Event;

            var megNode = EffectConfig.instance.FindEffectGraph (effectName);
            if (megNode != null)
            {
                if (megNode.nodeList.Count > 0)
                {
                    var n = megNode.nodeList[0];
                    if (n.node != null)
                    {
                        var configData = graph.GetConfigData<EcsData.XConfigData>();
                        var shaderEffectData = (configData is XSkillData) ? (configData as XSkillData).ShaderEffectData : (configData as XHitData).ShaderEffectData;
                        var skillNode = graph.AddNodeInGraphByScript<XShaderEffectData, EditorNode.ShaderEffectNode> (
                                e.mousePosition, ref shaderEffectData);
                        skillNode.HosterData.uniqueID = n.uniqueID;
                        skillNode.HosterData.partMask = 0xffffffff;
                        n.node.InitData (ref skillNode.HosterData.x,
                            ref skillNode.HosterData.y,
                            ref skillNode.HosterData.z,
                            ref skillNode.HosterData.w,
                            ref skillNode.HosterData.path,
                            ref skillNode.HosterData.keywordMask);
                        EditorNode.MatEffectNode lasteffectNode = null;
                        float xoffset = 160;
                        for (int i = 1; i < megNode.nodeList.Count; ++i)
                        {
                            n = megNode.nodeList[i];
                            var effectNode = CreateMatEffectNode (graph, skillNode, xoffset, n);
                            if (lasteffectNode == null)
                            {
                                skillNode.HosterData.NextNode = effectNode.HosterData.Index;
                            }
                            else
                            {
                                lasteffectNode.HosterData.NextNode = effectNode.HosterData.Index;
                            }
                            lasteffectNode = effectNode;
                            xoffset += 140;
                        }
                    }

                }

            }

        }

        public override void CalcTriggerTime()
        {
            base.CalcTriggerTime();

            DFSTriggerTime(this, HosterData.NextNode);
        }
    }
}