using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System.Collections.Generic;
using EcsData;
using CFEngine;
using System.Threading.Tasks;
using System.Text;
using System;
using BluePrint;
using Unity.EditorCoroutines.Editor;

namespace TDTools {
    public class SkillFrametimeExportTool : EditorWindow {

        readonly List<int> comboID = new List<int> { 2, 3, 4, 5, 92, 106 };
        readonly List<int> chargeID = new List<int> {6, 26, 14};
        readonly List<int> skillID = new List<int> {7, 12 };
        readonly List<int> AI = new List<int> {1, 91};

        [MenuItem("Tools/TDTools/���ܹ���/���ܹؼ�֡�������")]
        public static void ShowExample() {
            SkillFrametimeExportTool wnd = GetWindow<SkillFrametimeExportTool>();
            wnd.titleContent = new GUIContent("SkillFrametimeExportTool");
        }

        public void CreateGUI() {
            VisualElement root = rootVisualElement;

            Toolbar bar = new Toolbar();
            root.Add(bar);

            ToolbarButton buttonScanAll = new ToolbarButton();
            buttonScanAll.text = "ɨ��ȫ��";
            buttonScanAll.clicked += () => {
                string path = $@"{Application.dataPath}\BundleRes\SkillPackage";
                var files = GetAllBytesFiles(path);
                TableChecker.TableChecker.DATAPATH = Application.dataPath;
                EditorCoroutineUtility.StartCoroutine(Scan(files),  this);
            };
            bar.Add(buttonScanAll);

            ToolbarButton buttonOpen = new ToolbarButton();
            buttonOpen.text = "���ļ�";
            buttonOpen.clicked += () => {
                string path = EditorUtility.OpenFolderPanel("��ɨ���ļ���", "", "");
                if (path != "") {
                    var files = GetAllBytesFiles(path);
                    TableChecker.TableChecker.DATAPATH = Application.dataPath;
                    EditorCoroutineUtility.StartCoroutine(Scan(files), this);
                }
            };
            bar.Add(buttonOpen);
        }

        public static int secondToFrame(float seconds) {
            if (seconds >= 0)
                return (int)(seconds * 30.0 + 0.5);
            else
                return -1;
        }

        void PrintResult(string path, List<Dictionary<string, string>> results) {
            string comment = "������\t" +
                "�ܳ�(֡)\t" +
                "Result(֡)\t" +
                "Bullet(֡)\t" +
                "����һ���չ�(֡)\t" +
                "������(֡)\t" +
                "�Ӽ���(֡)\t" +
                "��A1(֡)\t" +
                "���ƶ�(֡)\t" +
                "��ʼCD(s)\t" +
                "ƽʱCD(s)\t" +
                "PVP��ʼCD(s)\t" +
                "PVPƽʱCD(s)\t" +
                "ȫ·��";

            using StreamWriter sw = new StreamWriter(path, false, Encoding.Unicode);
            sw.WriteLine(comment);
            for (int i = 0; i < results.Count; i++) {
                var result = results[i];
                sw.Write($"{result["Name"]}\t");
                sw.Write(result["Length"] + "\t");
                sw.Write(result["BulletFrame"] + "\t");
                sw.Write(result["ResultFrame"] + "\t");

                sw.Write($"{result["QTECombo"]}\t");
                sw.Write(result["QTECharge"] + "\t");
                sw.Write(result["QTESkill"] + "\t");
                sw.Write(result["QTEAI"] + "\t");

                sw.Write(result["CanMove"] + "\t");

                if (result.ContainsKey("InitCD")) {
                    sw.Write(result["InitCD"] + "\t");
                    sw.Write(result["CDRatio"] + "\t");
                    sw.Write(result["PvPInitCD"] + "\t");
                    sw.Write(result["PvPCDRatio"] + "\t");
                }
                sw.Write(result["Fullpath"]);
                sw.WriteLine();
            }
        }

        List<FileInfo> GetAllBytesFiles(string path) {
            List<FileInfo> result = new List<FileInfo>();
            DirectoryInfo dir = new DirectoryInfo(path);
            result.AddRange(dir.GetFiles("*.bytes"));
            var dirs = dir.GetDirectories();
            for (int i = 0; i < dirs.Length; i++) {
                result.AddRange(GetAllBytesFiles(dirs[i].FullName));
            }
            return result;
        }

        public static T DeserializeEcsData<T>(string path) {
            string json = "";
            byte[] bytes;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                bytes = new byte[fs.Length + 1];
                fs.Read(bytes, 0, bytes.Length);
            }
            SimpleTools.Unlock(ref bytes, 0, bytes.Length - 1);
            json = System.Text.Encoding.UTF8.GetString(bytes);
            return JsonUtility.FromJson<T>(json);
        }

        System.Collections.IEnumerator Scan(List<FileInfo> files) {
            List<Dictionary<string, string>> tableSkillListForRole = TableChecker.TableChecker.ReadTable("SkillListForRole");
            List<Dictionary<string, string>> results = new List<Dictionary<string, string>>();
            Dictionary<string, int> _tableIndexDic = new Dictionary<string, int>();
            string savePath = EditorUtility.SaveFilePanel("������", "", "���ܽ��", "txt");

            for (int i = 0; i < tableSkillListForRole.Count; i++) {
                _tableIndexDic[tableSkillListForRole[i]["SkillScript"]] = i;
            } 

            int count = files.Count;
            int progressID = Progress.Start("ɨ���ļ�");
            for (int i = 0; i < count; i++) {
                string path = files[i].FullName;
                try {

                    SkillGraph graph = new SkillGraph();
                    graph.Init(GetWindow<SkillEditor>());
                    graph.NeedInitRes = false;
                    graph.OpenData(path);
                    //graph.configData = DeserializeEcsData<XSkillData>(path);
                    //graph.graphConfigData = DeserializeEcsData<GraphConfigData>(path.Replace(".bytes", ".ecfg"));
                    //graph.ReBuildNodeByData();
                    var data = graph.configData;
                    var graphData = graph.graphConfigData;

                    int nodeCount = graph.GetNodeCount();
                    for (int j = 0; j < nodeCount; j++)
                        graph.GetNodeByIndex(j).CalcTriggerTime();
                    

                    Dictionary<string, string> result = new Dictionary<string, string>();
                    result["Name"] = data.Name;
                    Progress.Report(progressID, i / (float)count, data.Name);
                    if (!_tableIndexDic.ContainsKey(data.Name))
                        continue;
                    result["Length"] = secondToFrame(data.Length).ToString();
                    string bulletFrame = "";
                    string resultFrame = "";
                    for (int j = 0; j < data.BulletData.Count; j++) {
                        int time = secondToFrame(graph.GetNodeByIndex(data.BulletData[j].Index).TriggerTime);
                        if (time == -1) continue;
                        if (bulletFrame != "")
                            bulletFrame += "|";
                        bulletFrame += time;
                    }

                    for (int j = 0; j < data.ResultData.Count; j++) {
                        int time = secondToFrame(graph.GetNodeByIndex(data.ResultData[j].Index).TriggerTime);
                        if (time == -1) continue;
                        if (resultFrame != "")
                            resultFrame += "|";
                        resultFrame += time;
                    }

                    string canMove = "";
                    for (int j = 0; j < data.ActionStatusData.Count; j++) {
                        int triggerTime = secondToFrame(graph.GetNodeByIndex(data.ActionStatusData[j].Index).TriggerTime);
                        if (triggerTime == -1) continue;
                        if (data.ActionStatusData[j].CanMove) {
                            if (canMove != "")
                                canMove += "|";
                            canMove += triggerTime;
                        }
                    }

                    result["CanMove"] = canMove;

                    string QTECombo = "";
                    string QTECharge = "";
                    string QTESkill = "";
                    string QTEA1 = "";

                    for (int j = 0; j < data.QTEData.Count; j++) {
                        int id = data.QTEData[j].QTEID;
                        int triggerTime = secondToFrame(graph.GetNodeByIndex(data.QTEData[j].Index).TriggerTime);
                        if (triggerTime == -1) continue;
                        if (comboID.Contains(id)) {
                            if (QTECombo != "")
                                QTECombo += "|";
                            QTECombo += triggerTime;
                        } else if (chargeID.Contains(id)) {
                            if (QTECharge != "")
                                QTECharge += "|";
                            QTECharge += triggerTime;
                        } else if (skillID.Contains(id)) {
                            if (triggerTime == 0) continue;
                            if (QTESkill != "")
                                QTESkill += "|";
                            QTESkill += triggerTime;
                        } else if (AI.Contains(id)) {
                            if (QTEA1 != "")
                                QTEA1 += "|";
                            QTEA1 += triggerTime;
                        }
                    }

                    result["QTECombo"] = QTECombo;
                    result["QTECharge"] = QTECharge;
                    result["QTESkill"] = QTESkill;
                    result["QTEAI"] = QTEA1;

                    result["BulletFrame"] = bulletFrame;
                    result["ResultFrame"] = resultFrame;

                    if (_tableIndexDic.ContainsKey(result["Name"])) {
                        int index = _tableIndexDic[result["Name"]];
                        var row = tableSkillListForRole[index];
                        result["InitCD"] = row["InitCD"];
                        result["CDRatio"] = row["CDRatio"];
                        result["PvPInitCD"] = row["PvPInitCD"];
                        result["PvPCDRatio"] = row["PvPCDRatio"];
                    }

                    result["Fullpath"] = files[i].FullName;

                    results.Add(result);
                } catch (Exception e){
                    Debug.Log(e.Message);
                }

                yield return null;
            }
            Progress.Remove(progressID);
            if (savePath != "")
                PrintResult(savePath, results);
        }
    }
}