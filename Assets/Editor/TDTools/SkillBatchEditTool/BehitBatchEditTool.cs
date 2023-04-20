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

        [MenuItem("Tools/TDTools/��������/�ܻ�ˢ���ߴ�Сд")]
        public static void ShowWindow() {
            BehitBatchEdiTools wnd = GetWindow<BehitBatchEdiTools>();
            wnd.titleContent = new GUIContent("�ܻ�ˢ");
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
            buttonScan.text = "ɨ���ܻ�";
            buttonScan.clicked += ScanAll;

            ToolbarButton btnSkill = new ToolbarButton();
            bar.Add(btnSkill);
            btnSkill.text = "ɨ�輼��";
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

            using FileStream fs = new FileStream(EditorUtility.SaveFilePanel("���浽�ļ�", "", "���ܴ�Сд�����", "txt"), FileMode.Create);
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
                                Debug.Log($"{list[i].FullName}\t{forward}\t��Сд����");
                                sw.WriteLine($"{list[i].FullName}\t{forward}\t��Сд����");
                            }
                        } else {
                        }

                        if (side != "" && File.Exists($"{curvePath}{side}")) {
                            if (!set.Contains(side.Substring(side.LastIndexOf('/') + 1))) {
                                Debug.Log($"{list[i].FullName}\t{side}\t��Сд����");
                                sw.WriteLine($"{list[i].FullName}\t{side}\t��Сд����");
                            }
                        } else {
                        }


                        if (data.ChargeData[j].UsingUp) {
                            string up = data.ChargeData[j].CurveUp;

                            if (up == "") {
                                Debug.Log($"usingup���� ���� {up} ������");
                                sw.WriteLine($"usingup���� ���� {up} ������");
                            }

                            if (File.Exists($"{curvePath}{up}")) {
                                if (!set.Contains(up.Substring(up.LastIndexOf('/') + 1))) {
                                    Debug.Log($"{up} ��Сд����");
                                    sw.WriteLine($"{up} ��Сд����");
                                }
                            } else {
                                Debug.Log($"{list[i].FullName}\tusingup���� ���� {up} ������");
                                sw.WriteLine($"{list[i].FullName}\tusingup���� ���� {up} ������");
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

            using FileStream fs = new FileStream(EditorUtility.SaveFilePanel("���浽�ļ�", "", "�ܻ���Сд�����", "txt"), FileMode.Create);
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
                                //Debug.Log($"{list[i].FullName}\t{forward}\t��Сд����");
                                sw.WriteLine($"{list[i].FullName}\t{forward}\t��Сд����");


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
                                //Debug.Log($"{list[i].FullName}\t{forward}\t������");
                                sw.WriteLine($"{list[i].FullName}\t{forward}\t������");
                            }
                        }

                        if (side != "" && File.Exists($"{curvePath}{side}")) {
                            if (!set.Contains(side.Substring(side.LastIndexOf('/') + 1))) {
                                //Debug.Log($"{list[i].FullName}\t{side}\t��Сд����");
                                sw.WriteLine($"{list[i].FullName}\t{side}\t��Сд����");

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
                                //Debug.Log($"{list[i].FullName}\t{side}\t������");
                                sw.WriteLine($"{list[i].FullName}\t{side}\t������");
                            }
                        }


                        if (data.KnockedBackData[j].UsingUp) {
                            string up = data.KnockedBackData[j].CurveUp;

                            if (up == "") {
                                Debug.Log($"usingup���� ���� {up} ������");
                                sw.WriteLine($"usingup���� ���� {up} ������");
                            }

                            if (File.Exists($"{curvePath}{up}")) {
                                if (!set.Contains(up.Substring(up.LastIndexOf('/') + 1))) {
                                    Debug.Log($"{up} ��Сд����");
                                    sw.WriteLine($"{up} ��Сд����");
                                }
                            } else {
                                Debug.Log($"usingup���� ���� {up} ������");
                                sw.WriteLine($"usingup���� ���� {up} ������");
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