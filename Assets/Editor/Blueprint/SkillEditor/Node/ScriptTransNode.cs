using EcsData;
using UnityEngine;
using BluePrint;
using UnityEditor;
using System.IO;

namespace EditorNode
{
    public class ScriptTransNode : TimeTriggerNode<XScriptTransData>
    {
        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, false);

            HeaderImage = "BluePrint/Header1";

            if (AutoDefaultMainPin)
            {
                BluePrintPin pinIn = new BluePrintPin(this, -1, "In", PinType.Main, PinStream.In);
                AddPin(pinIn);
            }
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();
            HosterData.Name = EditorGUITool.TextField("ScriptName", HosterData.Name);
            HosterData.Hash = (int)CFUtilPoolLib.XCommon.singleton.XHash(HosterData.Name);
            EditorGUITool.TextField("ScriptHash", HosterData.Hash.ToString());
            if (GetRoot is SkillGraph)
            {
                if (!string.IsNullOrEmpty(HosterData.Name))
                {
                    string file = Application.dataPath + "/BundleRes/SkillPackage/" + (GetRoot as SkillGraph).LoadCacheDirectory() + "/" + HosterData.Name + ".bytes";
                    if (File.Exists(file) && GUILayout.Button("JumpTo"))
                    {
                        SkillEditor.Instance.OpenFile(file);
                    }
                }
            }
            HosterData.Type = EditorGUITool.Popup("Type", HosterData.Type, EditorGUITool.Translate<EditorEcs.XScriptType>());
            HosterData.Force = EditorGUITool.Toggle("Force", HosterData.Force);
            if(GetRoot is SkillGraph)HosterData.InheritTarget = EditorGUITool.Toggle("InheritTarget", HosterData.InheritTarget);
        }

        public override void SetDebug(bool flag = true)
        {
            base.SetDebug(flag);

            SkillGraph graph = GetRoot as SkillGraph;
            CFUtilPoolLib.XEntityPresentation.RowData data = XEntityPresentationReader.GetData((uint)graph.configData.PresentID);
            
            if (VirtualSkill.SkillHoster.GetHoster != null) 
                VirtualSkill.SkillHoster.GetHoster.LoadSkillData("/BundleRes/SkillPackage/" + data.SkillLocation + HosterData.Name + ".bytes");
        }

        public override bool CompileCheck()
        {
            if (!base.CompileCheck()) return false;

            if (GetRoot.GetConfigData<XConfigData>().Name == HosterData.Name)
            {
                LogError("ScriptTransNode_" + HosterData.Index + " ,不能跳转到本身！！！\n" + GetRoot.DataPath);
                return false;
            }

            return true;
        }
    }
}
