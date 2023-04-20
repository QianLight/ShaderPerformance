using LevelEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools.ResourceScanner {
    public partial class ResourceReferenceScanner{

        /// <summary>
        /// 创建一个关卡选择框
        /// </summary>
        VisualElement CreateLevelSelection() {
            VisualElement root = new VisualElement();
            root.style.width = 200;

            Toolbar toolbar = new Toolbar();
            toolbar.style.flexShrink = 0;
            ToolbarButton loadConfigButton = new ToolbarButton();
            loadConfigButton.text = "读取配置";
            loadConfigButton.clicked += () => {
                string path = EditorUtility.OpenFilePanel("读取配置", "", "txt");
                if (path != null && File.Exists(path)) {
                    int count = _mapToggleArray.Length;
                    for (int i = 0; i < count; i++)
                        _mapToggleArray[i].SetValueWithoutNotify(false);
                    try {
                        using StreamReader stream = new StreamReader(path);
                        while (!stream.EndOfStream) {
                            string line = stream.ReadLine();
                            for (int i = 0; i < count; i++) {
                                if (_mapList[i]["MapID"].Equals(line)) {
                                    _mapToggleArray[i].value = true;
                                    break;
                                }
                            }
                        }
                    } catch {
                        Debug.Log("读取失败");
                    }
                }
            };
            toolbar.Add(loadConfigButton);

            ToolbarButton saveConfigButton = new ToolbarButton();
            saveConfigButton.text = "保存配置";
            saveConfigButton.clicked += () => {
                string path = EditorUtility.SaveFilePanel("保存配置", "", "新关卡选取配置", "txt");
                if (path != null) {
                    using StreamWriter stream = new StreamWriter(path, false);
                    int count = _mapToggleArray.Length;
                    for (int i = 0; i < count; i++) {
                        if (_mapToggleArray[i].value)
                            stream.WriteLine(_mapList[i]["MapID"]);
                    }
                }
            };
            toolbar.Add(saveConfigButton);

            Toggle selectAllToggle = new Toggle();
            selectAllToggle.text = "全选";
            selectAllToggle.RegisterValueChangedCallback(obj => {
                for (int i = 0; i < _mapToggleArray.Length; i++) {
                    _mapToggleArray[i].value = obj.newValue;
                }
            });
            toolbar.Add(selectAllToggle);

            root.Add(toolbar);

            Toolbar searchbar = new Toolbar();
            root.Add(searchbar);
            ToolbarPopupSearchField searchFiled = new ToolbarPopupSearchField();
            searchbar.style.flexShrink = 0;
            searchbar.Add(searchFiled);
            searchFiled.style.width = 200;
            searchFiled.RegisterValueChangedCallback(obj => {
                for (int i = 0; i < _mapList.Count; i++) {
                    if (_mapToggleArray[i].text.Contains(obj.newValue))
                        _mapToggleArray[i].style.display = DisplayStyle.Flex;
                    else
                        _mapToggleArray[i].style.display = DisplayStyle.None;
                }
            });

            ScrollView scrollView = new ScrollView();
            _mapList = TableChecker.TableChecker.ReadTable("MapList");
            _mapToggleArray = new Toggle[_mapList.Count];

            for (int i = 0; i < _mapList.Count; i++) {
                VisualElement element = new VisualElement();
                Toggle toggle = new Toggle();
                _mapToggleArray[i] = toggle;
                toggle.text = _mapList[i]["Comment"];
                element.style.flexDirection = FlexDirection.Row;
                element.Add(toggle);
                scrollView.Add(element);
            }
            root.Add(scrollView);
            return root;
        }

        async Task<ScannerResult> ScanLevel(string path, int parentId) {
            ScannerResult results = new ScannerResult();
            if (!File.Exists(path))
                return results;

            int progressID = Progress.Start("扫描资源", path, Progress.Options.None, parentId);
            var data = DataIO.DeserializeData<LevelEditorData>(path);
            if (data == null)
                return results;
            for (int j = 0; j < data.GraphDataList.Count; j++) {

                for (int i = 0; i < data.GraphDataList[j].WaveData.Count; i++) {
                    LevelWaveData waveData = data.GraphDataList[j].WaveData[i];
                    switch (waveData.SpawnType) {
                        case (int)LevelSpawnType.Spawn_Source_Monster:
                            results.AddRange(ScanMonster(waveData.SpawnID.ToString(), new ReferenceRecord(NodeType.LevelScript, path)));
                            break;
                    }
                }

                for (int i = 0; i < data.GraphDataList[j].ScriptData.Count; i++) {
                    var scriptData = data.GraphDataList[j].ScriptData[i];
                    if (scriptData.Cmd == LevelScriptCmd.Level_Cmd_Addbuff) {
                        int targetID = (int)scriptData.valueParam[0];
                        if (_monsterTable.Dic.ContainsKey(targetID.ToString())) {
                            var monster = _monsterTable.Table[_monsterTable.Dic[targetID.ToString()]];
                            results.AddRange(ScanBuff($"{scriptData.valueParam[1]}={scriptData.valueParam[2]}", new ReferenceRecord(NodeType.LevelScript, path), monster["PresentID"]));
                        } else {
                            results.AddRange(ScanBuff($"{scriptData.valueParam[1]}={scriptData.valueParam[2]}", new ReferenceRecord(NodeType.LevelScript, path)));
                        }
                    } else if (scriptData.Cmd == LevelScriptCmd.Level_Cmd_Cutscene) {
                        results.TimelineToScan.Add(scriptData.stringParam[0]);
                    }
                }

                Progress.Report(progressID, j / (float)data.GraphDataList.Count);
                await Task.Yield();
            }

            Progress.Remove(progressID);
            return results;
        }
    }
}