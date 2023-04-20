using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.Collections.Generic;
using System.IO;
using CFEngine;
using EcsData;
using BluePrint;
using System;

namespace TDTools {
    public class BehitBatchEdiTools : EditorWindow {

        #region Variables
        #endregion

        [MenuItem("Tools/TDTools/废弃工具/受击刷曲线大小写")]
        public static void ShowWindow() {
            BehitBatchEdiTools wnd = GetWindow<BehitBatchEdiTools>();
            wnd.titleContent = new GUIContent("受击刷");
        }

        public static T DeserializeEcsData<T>(string path) {
            string json = "";
            byte[] bytes;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                bytes = new byte[fs.Length + 1];
                fs.Read(bytes, 0, bytes.Length);
                //bytes[fs.Length] = 0;
            }
            SimpleTools.Unlock(ref bytes, 0, bytes.Length - 1);
            json = System.Text.Encoding.UTF8.GetString(bytes);
            return JsonUtility.FromJson<T>(json);
        }

        public static void SerializeEcsData<T>(T data, string path) {
            string json = JsonUtility.ToJson(data);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
            SimpleTools.Lock(ref bytes);
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            fs.Write(bytes, 0, bytes.Length);
        }

        List<FileInfo> GetAllFiles(DirectoryInfo rootDir, string searchPattern) {
            List<FileInfo> result = new List<FileInfo>();
            result.AddRange(rootDir.GetFiles(searchPattern));
            var dirs = rootDir.GetDirectories();
            for (int i = 0; i < dirs.Length; i++) {
                result.AddRange(GetAllFiles(dirs[i], searchPattern));
            }

            return result;
        }

        public void CreateGUI() {
            VisualElement root = rootVisualElement;

            Toolbar bar = new Toolbar();
            bar.style.flexShrink = 0;
            root.Add(bar);

            ToolbarButton buttonScan = new ToolbarButton();
            bar.Add(buttonScan);
            buttonScan.text = "扫描受击";
            buttonScan.clicked += ScanAll;

            ToolbarButton btnSkill = new ToolbarButton();
            bar.Add(btnSkill);
            btnSkill.text = "扫描技能";
            btnSkill.clicked += ScanSkill;
        }

        void ScanSkill() {
            var list = GetAllFiles(new DirectoryInfo($@"{Application.dataPath}\BundleRes\SkillPackage\"), "*.bytes");
            var wnd = GetWindow<SkillEditor>();
            string curvePath = $@"{Application.dataPath}\Editor\EditorResources\Server\";

            var curveFiles = GetAllFiles(new DirectoryInfo($@"{Application.dataPath}\Editor\EditorResources\Server\"), "*.txt");

            HashSet<string> set = new HashSet<string>();
            for (int i = 0; i < curveFiles.Count; i++)
                set.Add(curveFiles[i].Name);

            using FileStream fs = new FileStream(EditorUtility.SaveFilePanel("保存到文件", "", "技能大小写检查结果", "txt"), FileMode.Create);
            using StreamWriter sw = new StreamWriter(fs);

            for (int i = 0; i < list.Count; i++) {
                try {
                    //(wnd.CurrentGraph as SkillGraph).OpenData(list[i].FullName);
                    //DataIO.SerializeEcsData<XSkillData>(list[i].FullName, (wnd.CurrentGraph as SkillGraph).configData);
                    //DataIO.SerializeEcsData<GraphConfigData>(list[i].FullName.Replace(".bytes", ".ecfg"), (wnd.CurrentGraph as SkillGraph).graphConfigData);

                    var data = DataIO.DeserializeEcsData<XSkillData>(list[i].FullName);
                    for (int j = 0; j < data.ChargeData.Count; j++) {
                        string forward = data.ChargeData[j].CurveForward;
                        string side = data.ChargeData[j].CurveSide;

                        if (forward != "" && File.Exists($"{curvePath}{forward}")) {
                            if (!set.Contains(forward.Substring(forward.LastIndexOf('/') + 1))) {
                                Debug.Log($"{list[i].FullName}\t{forward}\t大小写错误");
                                sw.WriteLine($"{list[i].FullName}\t{forward}\t大小写错误");
                            }
                        } else {
                        }

                        if (side != "" && File.Exists($"{curvePath}{side}")) {
                            if (!set.Contains(side.Substring(side.LastIndexOf('/') + 1))) {
                                Debug.Log($"{list[i].FullName}\t{side}\t大小写错误");
                                sw.WriteLine($"{list[i].FullName}\t{side}\t大小写错误");
                            }
                        } else {
                        }


                        if (data.ChargeData[j].UsingUp) {
                            string up = data.ChargeData[j].CurveUp;

                            if (up == "") {
                                Debug.Log($"usingup但是 曲线 {up} 不存在");
                                sw.WriteLine($"usingup但是 曲线 {up} 不存在");
                            }

                            if (File.Exists($"{curvePath}{up}")) {
                                if (!set.Contains(up.Substring(up.LastIndexOf('/') + 1))) {
                                    Debug.Log($"{up} 大小写错误");
                                    sw.WriteLine($"{up} 大小写错误");
                                }
                            } else {
                                Debug.Log($"{list[i].FullName}\tusingup但是 曲线 {up} 不存在");
                                sw.WriteLine($"{list[i].FullName}\tusingup但是 曲线 {up} 不存在");
                            }
                        }
                    }
                } catch {
                    Debug.Log(list[i]);
                }
            }
        }


        void ScanAll() {
            var list = GetAllFiles(new DirectoryInfo($@"{Application.dataPath}\BundleRes\HitPackage\"), "*.bytes");
            var wnd = GetWindow<BehitEditor>();
            string curvePath = $@"{Application.dataPath}\Editor\EditorResources\Server\";

            var curveFiles = GetAllFiles(new DirectoryInfo($@"{Application.dataPath}\Editor\EditorResources\Server\"), "*.txt");

            HashSet<string> set = new HashSet<string>();
            for (int i = 0; i < curveFiles.Count; i++)
                set.Add(curveFiles[i].Name);

            using FileStream fs = new FileStream(EditorUtility.SaveFilePanel("保存到文件", "", "受击大小写检查结果", "txt"), FileMode.Create);
            using StreamWriter sw = new StreamWriter(fs);

            for (int i = 0; i < list.Count; i++) {
                try {
                    //(wnd.CurrentGraph as BehitGraph).OpenData(list[i].FullName);
                    //DataIO.SerializeEcsData<XHitData>(list[i].FullName, (wnd.CurrentGraph as BehitGraph).configData);
                    //DataIO.SerializeEcsData<GraphConfigData>(list[i].FullName.Replace(".bytes", ".ecfg"), (wnd.CurrentGraph as BehitGraph).graphConfigData);

                    var data = DataIO.DeserializeEcsData<XHitData>(list[i].FullName);
                    bool changed = false;

                    for (int j = 0; j < data.KnockedBackData.Count; j++) {
                        string forward = data.KnockedBackData[j].CurveForward;
                        string side = data.KnockedBackData[j].CurveSide;

                        if (forward != "" && File.Exists($"{curvePath}{forward}")) {
                            if (!set.Contains(forward.Substring(forward.LastIndexOf('/') + 1))) {
                                //Debug.Log($"{list[i].FullName}\t{forward}\t大小写错误");
                                sw.WriteLine($"{list[i].FullName}\t{forward}\t大小写错误");


                                for (int k = 0; k < curveFiles.Count; k++) {
                                    if (curveFiles[k].Name.ToLower().Equals(forward.Substring(forward.LastIndexOf('/') + 1).ToLower())) {
                                        data.KnockedBackData[j].CurveForward = $"{forward.Substring(0, forward.LastIndexOf('/') + 1)}{curveFiles[k].Name}";
                                        Debug.Log(data.KnockedBackData[j].CurveForward);
                                        changed = true;
                                        break;
                                    }
                                }
                            }
                        } else {
                            if (forward != "") {
                                //Debug.Log($"{list[i].FullName}\t{forward}\t不存在");
                                sw.WriteLine($"{list[i].FullName}\t{forward}\t不存在");
                            }
                        }

                        if (side != "" && File.Exists($"{curvePath}{side}")) {
                            if (!set.Contains(side.Substring(side.LastIndexOf('/') + 1))) {
                                //Debug.Log($"{list[i].FullName}\t{side}\t大小写错误");
                                sw.WriteLine($"{list[i].FullName}\t{side}\t大小写错误");

                                for (int k = 0; k < curveFiles.Count; k++) {
                                    if (curveFiles[k].Name.ToLower().Equals(side.Substring(side.LastIndexOf('/') + 1).ToLower())) {
                                        data.KnockedBackData[j].CurveSide = $"{side.Substring(0, side.LastIndexOf('/') + 1)}{curveFiles[k].Name}";
                                        Debug.Log(data.KnockedBackData[j].CurveSide);
                                        changed = true;
                                        break;
                                    }
                                }
                            }
                        } else {
                            if (side != "") {
                                //Debug.Log($"{list[i].FullName}\t{side}\t不存在");
                                sw.WriteLine($"{list[i].FullName}\t{side}\t不存在");
                            }
                        }


                        if (data.KnockedBackData[j].UsingUp) {
                            string up = data.KnockedBackData[j].CurveUp;

                            if (up == "") {
                                Debug.Log($"usingup但是 曲线 {up} 不存在");
                                sw.WriteLine($"usingup但是 曲线 {up} 不存在");
                            }

                            if (File.Exists($"{curvePath}{up}")) {
                                if (!set.Contains(up.Substring(up.LastIndexOf('/') + 1))) {
                                    Debug.Log($"{up} 大小写错误");
                                    sw.WriteLine($"{up} 大小写错误");
                                }
                            } else {
                                Debug.Log($"usingup但是 曲线 {up} 不存在");
                                sw.WriteLine($"usingup但是 曲线 {up} 不存在");
                            }
                        }
                    }
                    if (changed)
                        DataIO.SerializeEcsData(list[i].FullName, data);
                } catch (Exception e){
                    Debug.Log(e.Message + "\n" + e.StackTrace);
                }
            }
        }
    }
}