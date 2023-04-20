
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using EcsData;
using CFUtilPoolLib;

namespace TDTools
{
    class SkillGraphChangeTemp
    {
        [MenuItem("Tools/TDTools/“刷”相关工具/刷技能脚本")]
        public static void DoExporter()
        {
            string skillPath = $"{Application.dataPath}/BundleRes/SkillPackage";
            var files = Directory.GetFiles(skillPath, "*.bytes", SearchOption.AllDirectories);
            var skillEditor = EditorWindow.GetWindow<SkillEditor>();
            var skillGraph = skillEditor.CurrentGraph as SkillGraph;
            foreach (var file in files)
            {
                bool isChange = false;
                try
                {
                    skillGraph.OpenData(file);
                    string name = skillGraph.configData.Name;
                    DoChange(skillGraph, ref isChange, name);
                    if (isChange)
                        skillGraph.SaveData(file, true);
                }
                catch
                {
                    Debug.Log(skillGraph.configData.Name);
                }
            }
        }

        //private static void DoChange(SkillGraph skillGraph, ref bool isChange, string name)
        //{
        //    foreach (var node in skillGraph.configData.SpecialActionData)
        //    {
        //        if (node.Type == 6 && (node.IntParameter1 == 28 || node.IntParameter1 == 30) && name.StartsWith("Role_"))
        //        {
        //            node.IntParameter1 = 62;
        //            isChange = true;
        //        }
        //    }
        //}

        private static void DoChange(SkillGraph skillGraph, ref bool isChange, string name)
        {
            foreach (var node in skillGraph.configData.SpecialActionData)
            {
                if (node.Type == 12 && node.SubType == 5 && !node.PlayerTrigger && name.StartsWith("Role_"))
                {
                    node.PlayerTrigger = true;
                    isChange = true;
                }
            }

            foreach (var node in skillGraph.configData.CameraShakeData)
            {
                if (!node.PlayerTrigger && name.StartsWith("Role_"))
                {
                    node.PlayerTrigger = true;
                    isChange = true;
                }
            }
        }

        [MenuItem("Tools/TDTools/关卡相关工具/导出主角技能特效")]
        public static void DoExporter_Fx()
        {
            string skillPath = $"{Application.dataPath}/BundleRes/SkillPackage";
            var partnerInfo = PartnerInfoReader.PartnerInfo.Table;
            var skillEditor = EditorWindow.GetWindow<SkillEditor>();
            var skillGraph = skillEditor.CurrentGraph as SkillGraph;
            var roleSkill = XSkillReader.RoleSkill.Table;
            List<string> roleSkillList =
                roleSkill.Where(o => o.SkillType != 0).Select(o => o.SkillScript).Distinct().ToList();
            StringBuilder output = new StringBuilder();
            output.AppendLine($"角色id\t角色名\t技能名\t特效路径\t类型\t持续时间");
            StringBuilder useSkill = new StringBuilder();
            useSkill.AppendLine($"角色id\t角色名\t技能名");
            foreach (var line in partnerInfo)
            {
                if (line.Open)
                {
                    string SkillLocation = XEntityPresentationReader.GetSkillLocByPresentId(line.PresentId);
                    string roleSkillPath = $"{skillPath}/{SkillLocation}";
                    var files = Directory.GetFiles(roleSkillPath, "*.bytes", SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        var name = Path.GetFileNameWithoutExtension(file);
                        if (!roleSkillList.Contains(name))
                        {
                            continue;
                        }

                        try
                        {
                            skillGraph.OpenData(file);
                            useSkill.AppendLine($"{line.ID}\t{line.Name}\t{name}");
                            foreach (var node in skillGraph.configData.FxData)
                            {
                                output.AppendLine(
                                    $"{line.ID}\t{line.Name}\t{name}\t{node.FxPath}\tFx\t{node.LifeTime}");
                            }

                            foreach (var node in skillGraph.configData.BulletData)
                            {
                                output.AppendLine(
                                    $"{line.ID}\t{line.Name}\t{name}\t{node.BulletPath}\tBullet\t{node.LifeTime}");
                            }

                            foreach (var node in skillGraph.configData.WarningData)
                            {
                                output.AppendLine(
                                    $"{line.ID}\t{line.Name}\t{name}\t{node.FxPath}\tWarning\t{node.LifeTime}");
                            }

                            foreach (var node in skillGraph.configData.CameraPostEffectData)
                            {
                                output.AppendLine(
                                    $"{line.ID}\t{line.Name}\t{name}\t{node.FxPath}\tCameraPostEffect\t{node.LifeTime}");
                            }
                        }
                        catch
                        {
                            Debug.Log(skillGraph.configData.Name);
                        }
                    }
                }
            }

            string SkillSavePath = $"{Application.dataPath}/Editor/TDTools/SkillEditorExtension/当前版本主角实际使用技能.txt";
            File.WriteAllText(SkillSavePath, useSkill.ToString(), Encoding.GetEncoding("gb2312"));

            string SavePath = $"{Application.dataPath}/Editor/TDTools/SkillEditorExtension/主角使用特效.txt";
            File.WriteAllText(SavePath, output.ToString(), Encoding.GetEncoding("gb2312"));
        }

        [MenuItem("Tools/TDTools/关卡相关工具/废弃改击退")]
        public static void DoExporter_abondon()
        {
            string skillPath = $"{Application.dataPath}/BundleRes/SkillPackage/Test";
            var files = Directory.GetFiles(skillPath, "*.bytes", SearchOption.AllDirectories);
            var skillEditor = EditorWindow.GetWindow<SkillEditor>();
            var skillGraph = skillEditor.CurrentGraph as SkillGraph;
            foreach (var file in files)
            {
                bool isChange = false;
                try
                {
                    skillGraph.OpenData(file);
                    string name = skillGraph.configData.Name;
                    DoChange_abondon(skillGraph, ref isChange, name);
                    if (isChange)
                        skillGraph.SaveData(file, true);
                }
                catch
                {
                    Debug.Log(skillGraph.configData.Name);
                }
            }
        }

        private static void DoChange_abondon(SkillGraph skillGraph, ref bool isChange, string name)
        {
            foreach (var node in skillGraph.configData.ResultData)
            {
                if (node.EffectID == 0)
                {
                    node.EffectID = 1;
                    isChange = true;
                }
            }

            foreach (var node in skillGraph.configData.BulletData)
            {
                if (node.WithCollision)
                {
                    if (node.EffectID == 0)
                    {
                        node.EffectID = 1;
                        isChange = true;
                    }
                }
            }
        }

        [MenuItem("Tools/TDTools/“刷”相关工具/刷音效脚本")]
        public static void DoExporter_music()
        {
            string skillPath = $"{Application.dataPath}/BundleRes/SkillPackage/Monster_Ninjin";
            var files = Directory.GetFiles(skillPath, "*.bytes", SearchOption.AllDirectories);
            var skillEditor = EditorWindow.GetWindow<SkillEditor>();
            var skillGraph = skillEditor.CurrentGraph as SkillGraph;
            foreach (var file in files)
            {
                bool isChange = false;
                try
                {
                    skillGraph.OpenData(file);
                    string name = skillGraph.configData.Name;
                    DoChange_music(skillGraph, ref isChange, name);
                    if (isChange)
                        skillGraph.SaveData(file, true);
                }
                catch
                {
                    Debug.Log(skillGraph.configData.Name);
                }
            }
        }

        private static void DoChange_music(SkillGraph skillGraph, ref bool isChange, string name)
        {
            foreach (var node in skillGraph.configData.AudioData)
            {
                if (!node.StopAtSkillEnd)
                {
                    node.StopAtSkillEnd = true;
                    isChange = true;
                }
            }
        }

        [MenuItem("Tools/TDTools/“刷”相关工具/刷Animation")]
        public static void DoExporter_ani()
        {
            string skillPath = $"{Application.dataPath}/BundleRes/SkillPackage";
            var files = Directory.GetFiles(skillPath, "*.bytes", SearchOption.AllDirectories);
            var skillEditor = EditorWindow.GetWindow<SkillEditor>();
            var skillGraph = skillEditor.CurrentGraph as SkillGraph;
            List<string> motionName = new List<string>();
            foreach (var file in files)
            {
                try
                {
                    string filename = file.Split('/').Last();
                    filename = filename.Split('\\').Last();
                    filename = filename.Split('.')[0];
                    skillGraph.OpenData(file);
                    DoChange_ani(skillGraph, filename, ref motionName);
                }
                catch
                {
                    Debug.Log(skillGraph.configData.Name);
                }
            }

            //motionName.Distinct();

            string savePath = $"{Application.dataPath}/BundleRes/SkillPackage/Save";
            if (!System.IO.Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            TextWriter writer = new StreamWriter(@savePath+"/save_motion.txt");
            foreach (string name in motionName)
            {
                writer.WriteLine(name + "\n");
            }
            writer.Close();
        }

        public static void DoChange_ani(SkillGraph skillGraph, string filename, ref List<string> motionName)
        {
            foreach (var node in skillGraph.configData.AnimationData)
            {
                string newName = filename;
                newName += $"/{node.ClipPath.Split('/').Last()}";
                newName = newName.Replace('/',':');
                motionName.Add(newName);
            }
        }

        [MenuItem("Tools/TDTools/“刷”相关工具/新受击刷参数")]
        public static void DoExporter_newhit()
        {
            string skillPath = $"{Application.dataPath}/BundleRes/SkillPackage";
            var files = Directory.GetFiles(skillPath, "*.bytes", SearchOption.AllDirectories);
            var skillEditor = EditorWindow.GetWindow<SkillEditor>();
            var skillGraph = skillEditor.CurrentGraph as SkillGraph;
            foreach (var file in files)
            {
                bool isChange = false;
                try
                {
                    skillGraph.OpenData(file);
                    string name = skillGraph.configData.Name;
                    DoChange_newhit(skillGraph, ref isChange, name);
                    if (isChange)
                        skillGraph.SaveData(file, true);
                }
                catch
                {
                    Debug.Log(skillGraph.configData.Name);
                }
            }
        }

        private static void DoChange_newhit(SkillGraph skillGraph, ref bool isChange, string name)
        {
            foreach (var node in skillGraph.configData.ResultData)
            {
                if (node.EffectID != 7)
                {
                    if (node.ParamVelocityH == 1 && node.ParamForwardCurveScale != 1){
                        node.ParamVelocityH = (Single)Math.Sqrt(Math.Abs(node.ParamForwardCurveScale))*Math.Sign(node.ParamForwardCurveScale);
                        isChange = true;
                    }
                    if (node.ParamVelocityV == 1 && node.ParamUpCurveScale != 1 && node.EffectID != 4 && node.EffectID != 5){
                        node.ParamVelocityV = (Single)Math.Sqrt(Math.Abs(node.ParamUpCurveScale))*Math.Sign(node.ParamUpCurveScale);
                        isChange = true;
                    }
                }
            }

            foreach (var node in skillGraph.configData.BulletData)
            {
                if (node.EffectID != 7)
                {
                    if (node.ParamVelocityH == 1 && node.ParamForwardCurveScale != 1){
                        node.ParamVelocityH = (Single)Math.Sqrt(Math.Abs(node.ParamForwardCurveScale))*Math.Sign(node.ParamForwardCurveScale);
                        isChange = true;
                    }
                    if (node.ParamVelocityV == 1 && node.ParamUpCurveScale != 1 && node.EffectID != 4 && node.EffectID != 5){
                        node.ParamVelocityV = (Single)Math.Sqrt(Math.Abs(node.ParamUpCurveScale))*Math.Sign(node.ParamUpCurveScale);
                        isChange = true;
                    }
                }
            }
        }
    }
}
