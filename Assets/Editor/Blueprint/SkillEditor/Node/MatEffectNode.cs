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
    public class MatEffectNode : EventTriggerNode<XMatEffectData>
    {
        private BaseSkillNode effectNode = null;
        private MatEffectData med;
        private MatEffectNodeInfo matEffectNode;
        public override void Init (BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init (Root, pos, false);
            HeaderImage = "BluePrint/Header8";
            // BuildData ();
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
            BuildData ();
            if (matEffectNode != null)
            {
                EditorGUILayout.BeginHorizontal ();
                EditorGUITool.LabelField ("EffectType", GUILayout.MaxWidth (100));
                EditorGUITool.LabelField (matEffectNode.et.effectName, ShaderEffectNode.GetRedStyle (), GUILayout.MaxWidth (100));
                EditorGUILayout.EndHorizontal ();
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
                }
            }
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
                if (!string.IsNullOrEmpty(med.path) &&
                    !string.IsNullOrEmpty(HosterData.path))
                {

                    string ext = ResObject.GetExt((byte)med.x);
                    GetRoot.resMap[HosterData.path + ext] = med.path;
                }
            }
        }
        // protected override bool OnMouseRightDown (Event e)
        // {
        //     return true;
        // }

        protected override bool OnMouseDrag (Event e)
        {
            return true;
        }

        public bool OnRootDrag (Event e)
        {
            Bounds = Bounds.Move (e.delta / Scale);
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

        // public void BeginEffect (EffectPreviewContext context, List<XMatEffectData> MatEffectData)
        // {
        //     if (matEffectNode != null && context.effectInst != null)
        //     {
        //         EffectObject eo = SharedObjectPool<EffectObject>.Get ();
        //         eo.template = matEffectNode.node.GetEffectTemplate ();

        //         if (!string.IsNullOrEmpty (med.path))
        //         {
        //             eo.asset.path = med.path;
        //             LoadMgr.GetResHandler (ref eo.asset, ResObject.GetExt ((byte) med.x));
        //         }
        //         eo.partMask = med.partMask;

        //         eo.value.x = med.x;
        //         eo.value.y = med.y;
        //         eo.value.z = med.z;
        //         eo.value.w = med.w;
        //         eo.param = eo.value;
        //         RenderEffectSystem.Setup (eo);
        //         context.effectInst.effectData.Push (eo);
        //     }
        // }

        public override void CalcTriggerTime()
        {
            base.CalcTriggerTime();

            DFSTriggerTime(this, HosterData.NextNode);
        }
    }
}