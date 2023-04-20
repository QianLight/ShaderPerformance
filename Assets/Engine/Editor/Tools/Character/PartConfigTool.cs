using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using PartEditor = CFEngine.Editor.CommonListEditor<CFEngine.PartList>;
using PartContext = CFEngine.Editor.AssetListContext<CFEngine.PartList>;
using PrefabPartEditor = CFEngine.Editor.CommonListEditor<CFEngine.PrefabPartList>;
using PrefabPartContext = CFEngine.Editor.AssetListContext<CFEngine.PrefabPartList>;

namespace CFEngine.Editor
{
    public partial class PartConfigTool : BaseConfigTool<PartConfig>
    {
        public enum OpPartType
        {
            None,
            Update
        }
        private PartContext partContext;
        private PartEditor partEditor;

        private PrefabPartContext prefabPartContext;
        private PrefabPartEditor prefabpartEditor;
        private OpPartType opPartType = OpPartType.None;
        private string prefabName = "";
        public override void OnInit ()
        {
            base.OnInit ();
            config = PartConfig.instance;

            config.parts.name = "GlobalParts";

            partContext.name = "Parts";
            partContext.headGUI = PartHeadGUI;
            partContext.elementGUI = PartConfigGUI;
            partContext.needDelete = true;
            partContext.needAdd = true;

            partEditor = new PartEditor (config.parts, ref partContext);

            config.prefabParts.name = "PrefabParts";
            prefabPartContext.name = "Parts";
            prefabPartContext.elementGUI = PrefabPartConfigGUI;
            prefabPartContext.needDelete = true;
            prefabPartContext.needAdd = true;

            prefabpartEditor = new PrefabPartEditor (config.prefabParts, ref prefabPartContext);

        }
        private void PartHeadGUI (ref Rect rect, ref PartContext context, PartList data)
        {
            rect.width = 80;
            if (GUI.Button (rect, "Update"))
            {
                opPartType = OpPartType.Update;
                prefabName = "";
            }
        }

        private void PartConfigGUI (ref ListElementContext lec, ref PartContext context, PartList data, int i)
        {
            var part = data.partSuffix[i];
            ToolsUtility.InitListContext (ref lec, context.defaultHeight);
            if (ToolsUtility.TextField (ref lec, string.Format ("Suffix.{0}", i.ToString ()), 80, ref part, 160))
            {
                data.partSuffix[i] = part.ToLower ();
            }
            int partMask = 1 << i;
            ToolsUtility.Label (ref lec, "mask:" + partMask.ToString (), 160);

        }
        private void PrefabPartConfigGUI (ref ListElementContext lec, ref PrefabPartContext context, PrefabPartList data, int i)
        {
            var pp = data.prefabParts[i];
            ToolsUtility.InitListContext (ref lec, context.defaultHeight);
            ToolsUtility.Label (ref lec, pp.prefabName, 160, true);
            string folderPath = pp.GetHash ();
            bool ppFolder = ToolsUtility.SHButton (ref lec, config.folder, folderPath);
            if (ToolsUtility.Button (ref lec, "Add", 80))
            {
                pp.partSuffix.Add ("");
            }
            if (ToolsUtility.Button (ref lec, "Update", 80))
            {
                if (!string.IsNullOrEmpty (pp.prefabName))
                {
                    opPartType = OpPartType.Update;
                    prefabName = pp.prefabName;
                }
            }
            if (ppFolder)
            {
                ToolsUtility.NewLine (ref lec, 25);
                if (ToolsUtility.TextField (ref lec, string.Format ("Prefab.{0}", i.ToString ()), 80, ref pp.prefabName, 160, true))
                {
                    pp.prefabName = pp.prefabName.ToLower ();
                }
                int deleteIndex = ToolsUtility.BeginDelete ();
                for (int j = 0; j < pp.partSuffix.Count; ++j)
                {
                    var part = pp.partSuffix[j];
                    ToolsUtility.NewLine (ref lec, 25);
                    if (ToolsUtility.TextField (ref lec, string.Format ("Suffix.{0}", j.ToString ()), 80, ref part, 160, true))
                    {
                        pp.partSuffix[j] = part.ToLower ();
                    }
                    int partMask = 1 << (j + PartConfig.prefabPartOffset);
                    ToolsUtility.Label (ref lec, "mask:" + partMask.ToString (), 160);
                    ToolsUtility.DeleteButton (ref deleteIndex, j, ref lec);
                }
                ToolsUtility.EndDelete (deleteIndex, pp.partSuffix);
            }
        }

        protected override void OnConfigGui (ref Rect rect)
        {
            partEditor.Draw (config.folder, ref rect);
            prefabpartEditor.Draw (config.folder, ref rect);
        }

        protected override void OnConfigUpdate ()
        {
            switch (opPartType)
            {
                case OpPartType.Update:
                    {
                        config.GetPartInfo (prefabName, true);
                        prefabName = "";
                    }
                    break;
            }
            opPartType = OpPartType.None;
        }
    }
}