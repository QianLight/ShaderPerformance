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
    public class RoleSkillBatchEditTool : EditorWindow {

        #region Variables
        private List<Dictionary<string, string>> _targetTable;
        private List<Dictionary<string, string>> _skillListForRoleTable;
        private Dictionary<string, List<Dictionary<string, string>>> _skillListForRoleTableIndex;

        Toggle[] _toggleArr;
        #endregion

        [MenuItem("Tools/TDTools/废弃工具/技能批量刷Pre-Condition工具")]
        public static void ShowWindow() {
            RoleSkillBatchEditTool wnd = GetWindow<RoleSkillBatchEditTool>();
            wnd.titleContent = new GUIContent("技能批量刷Pre-Condition工具");
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

        public void CreateGUI() {
            VisualElement root = rootVisualElement;

            Toolbar bar = new Toolbar();
            bar.style.flexShrink = 0;
            root.Add(bar);

            ToolbarButton buttonScan = new ToolbarButton();
            bar.Add(buttonScan);
            buttonScan.text = "批量操作";
            buttonScan.clicked += () => {
                for (int i = 0; i < _targetTable.Count; i++) {
                    try {
                        if (_toggleArr[i].value) {
                            string id = _targetTable[i]["ID"];
                            for (int j = 0; j < _skillListForRoleTableIndex[id].Count; j++) {
                                var row = _skillListForRoleTableIndex[id][j];
                                string path = $@"{Application.dataPath}\BundleRes\SkillPackage\{_targetTable[i]["Path"]}{row["SkillScript"]}".Replace('/', '\\');
                                if (File.Exists($"{path}.bytes")) {
                                    var data = DeserializeEcsData<XSkillData>($"{path}.bytes");
                                    if (data.PreConditionData.Count > 0) {
                                        Debug.Log($"<color=green>已有pre-condition</color>: {row["SkillScript"]}");
                                        continue;
                                    }

                                    var configData = DeserializeEcsData<GraphConfigData>($"{path}.ecfg");
                                    if (configData.NodeConfigList.Count >= 64) {
                                        Debug.Log($"<color=white>已有65个节点</color>: {row["SkillScript"]}");
                                        continue;
                                    }

                                    int maxIndex = configData.NodeConfigList[0].NodeID;
                                    var maxPos = configData.NodeConfigList[0].Position;
                                    for (int k = 1; k < configData.NodeConfigList.Count; k ++){
                                        if (configData.NodeConfigList[k].NodeID > maxIndex)
                                            maxIndex = configData.NodeConfigList[k].NodeID;

                                        if (configData.NodeConfigList[k].Position.y > maxPos.y)
                                            maxPos.y = configData.NodeConfigList[k].Position.y;
                                    }

                                    maxIndex += 1;

                                    NodeConfigData node = new NodeConfigData();
                                    node.NodeID = maxIndex;
                                    node.Position = maxPos;
                                    node.Position.y += 200;
                                    node.Position.x = 0;
                                    node.TitleName = "PreCondition 1";
                                    configData.NodeConfigList.Add(node);


                                    XPreConditionData preCon = new XPreConditionData();
                                    preCon.Index = configData.NodeConfigList.Count - 1;
                                    preCon.Or = false;
                                    preCon.Not = false;
                                    preCon.ErrorType.Add(0);
                                    XConditionData con = new XConditionData();
                                    con.Index = -1;
                                    con.Operation = 0;
                                    con.FunctionHash = 287165352;
                                    con.Rhs = 6;
                                    con.Parameter1 = 0;
                                    con.Parameter2 = 0;
                                    con.StringParameter = "";
                                    con.Not = false;

                                    data.Version = (int)System.DateTime.Now.Ticks;
                                    preCon.Cond.Add(con);
                                    data.PreConditionData.Add(preCon);
                                    data.PreCondition = configData.NodeConfigList.Count - 1;

                                    SerializeEcsData(data, $"{path}.bytes");
                                    SerializeEcsData(configData, $"{path}.ecfg");
                                }
                            }
                        }
                    } catch (Exception e) {
                        Debug.Log($"<color=red>添加失败:</color> {_targetTable[i]["ID"]} {_targetTable[i]["Name"]} {e.Message}");
                    }
                }
            };

            VisualElement roleContainer = new VisualElement();
            root.Add(roleContainer);

            ScrollView scroll = new ScrollView();
            roleContainer.Add(scroll);

            _toggleArr = new Toggle[_targetTable.Count];
            for (int i = 0; i < _targetTable.Count; i++) {
                _toggleArr[i] = new Toggle();
                _toggleArr[i].text = _targetTable[i]["Name"];
                _toggleArr[i].value = true;
                scroll.Add(_toggleArr[i]);
            }
        }

        private void OnEnable() {
            TableChecker.TableChecker.DATAPATH = Application.dataPath;
            _targetTable = TableChecker.TableChecker.ReadTable($"{Application.dataPath}/Editor/TDTools/SkillBatchEditTool/Precondition.txt", true);
            _skillListForRoleTable = TableChecker.TableChecker.ReadTable("SkillListForRole");
            _skillListForRoleTableIndex = new Dictionary<string, List<Dictionary<string, string>>>();

            for (int i = 0; i < _skillListForRoleTable.Count; i++) {
                var row = _skillListForRoleTable[i];
                if (!_skillListForRoleTableIndex.ContainsKey(row["SkillPartnerID"]))
                    _skillListForRoleTableIndex[row["SkillPartnerID"]] = new List<Dictionary<string, string>>();
                _skillListForRoleTableIndex[row["SkillPartnerID"]].Add(row);
            }
        }
    }
}