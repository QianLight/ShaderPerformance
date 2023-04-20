using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using LevelEditor;
using System.IO;
using UnityEditor.UIElements;
using System.Threading.Tasks;
using System;
using EcsData;
using CFEngine;
using System.Text;
using UnityEngine.Timeline;
using System.Collections.Concurrent;
using System.Xml.Serialization;
using System.Xml;


namespace TDTools.ResourceScanner {
    [System.Serializable]
    public struct ReferenceRecord{
        public NodeType Type;
        public string ID;

        public ReferenceRecord(NodeType type, string id) {
            Type = type;
            ID = id;
        }

        public override bool Equals(object obj) {
            if (obj is ReferenceRecord) {
                ReferenceRecord other = (ReferenceRecord)obj;
                return Type.Equals(other.Type) && ID.Equals(other.ID);
            }
            return false;
        }

        public override int GetHashCode() {
            return $"{Type}_{ID}".GetHashCode();
        }

        public override string ToString() {
            return $"{Type}_{ID}";
        }
    }

    public partial class ResourceReferenceScanner : EditorWindow {
        #region Variables

        static string DATAPATH;
        static string BTPATH;

        TableNode _dropObjectsTable;
        TableNode _skillListForEnemyTable;
        TableNode _presentationTable;
        TableNode _monsterTable;
        TableNode _buffTable;
        TableNode _unitAITable;
        TableNode _roleTable;
        TableNode _enemyModeStateTable;
        TableNode _enemyStageTable;
        TableNode _skillListForPet;
        TableNode _partnerShowTable;

        bool _isRunnning = false;

        Toggle[] _mapToggleArray;
        Toggle[] _roleToggleArray;

        Dictionary<int, List<int>> _skillListForRoleIndexer;
        HashSet<string> _skillListForRoleSet;
        List<Dictionary<string, string>> _skillListForRole;
        Dictionary<string, List<int>> _skillListForRoleDic;

        List<Dictionary<string, string>> _mapList;

        HashSet<string> _skillGraphSet;

        Action _onResultChange;

        //name fullname
        Dictionary<string, string> _effectDictionary;

        List<string> _timelineToScan;
        bool _scanTimeline;

        public Dictionary<ReferenceRecord, Dictionary<ReferenceRecord, int>> Reference;

        #endregion

        #region UI

        [MenuItem("Tools/TDTools/配置检查工具/资源扫描工具")]
        public static void ShowWindow() {
            var wnd = GetWindow<ResourceReferenceScanner>();
        }

        void OnEnable() {
            TableChecker.TableChecker.DATAPATH = Application.dataPath;
            DATAPATH = Application.dataPath.Replace('/', '\\');
            BTPATH = DATAPATH.Substring(0, DATAPATH.LastIndexOf('\\'));
            BTPATH = $@"{BTPATH.Substring(0, BTPATH.LastIndexOf('\\'))}\tools\AIEditor\trees\";

            //if (Reference == null)
            //    Reference = new Dictionary<ReferenceRecord, List<ReferenceRecord>>();

            var database = TableDatabase.Instance;

            _skillGraphSet = new HashSet<string>();
            _dropObjectsTable = database.GetTable("DropObject", "ID");
            _skillListForEnemyTable = database.GetTable("SkillListForEnemy", "SkillScript");
            _monsterTable = database.GetTable("XEntityStatistics", "ID");
            _buffTable = database.GetTable("BuffList", "BuffID|BuffLevel", "{BuffID}={BuffLevel}\t{BuffName}");
            _presentationTable = database.GetTable("XEntityPresentation", "PresentID");
            _unitAITable = database.GetTable("UnitAITable", "ID");
            _roleTable = database.GetTable("PartnerInfo", "ID", "{ID} {Name}");
            _enemyModeStateTable = database.GetTable("EnemyModeState", "EnemyID");
            _enemyStageTable = database.GetTable("EnemyStage", "StaticsID");
            _skillListForPet = database.GetTable("SkillListForPet", "SkillScript");
            _partnerShowTable = database.GetTable("PartnerShow", "ID");
            _timelineToScan = new List<string>();
            _skillListForRoleDic = new Dictionary<string, List<int>>();

            _skillListForRole = TableChecker.TableChecker.ReadTable("SkillListForRole");
            _skillListForRoleSet = new HashSet<string>();
            _skillListForRoleIndexer = new Dictionary<int, List<int>>();
            for (int i = 0; i < _skillListForRole.Count; i++) {
                int pID = int.Parse(_skillListForRole[i]["SkillPartnerID"]);
                if (!_skillListForRoleIndexer.ContainsKey(pID))
                    _skillListForRoleIndexer[pID] = new List<int>();

                _skillListForRoleIndexer[pID].Add(i);

                string skillScript = _skillListForRole[i]["SkillScript"];
                if (!_skillListForRoleDic.ContainsKey(skillScript)) { 
                    _skillListForRoleDic[skillScript] = new List<int>();
                }
                _skillListForRoleDic[skillScript].Add(i);
            }


            _effectDictionary = new Dictionary<string, string>();
            string fxPath = $@"{DATAPATH}\BundleRes\Effects\";
            var allfiles = GetAllFiles(new DirectoryInfo(fxPath), "*.prefab");
            //Debug.LogWarning(allfiles.Count);
            for (int i = 0; i < allfiles.Count; i++) {
                string fileName = allfiles[i].Name.ToLower();
                _effectDictionary[fileName] = allfiles[i].FullName;
                //Debug.LogWarning(fileName);
            }


            _scanTimeline = false;
            _isRunnning = false;
        }

        void CreateGUI() {
            titleContent = new GUIContent("资源扫描工具");
            var root = rootVisualElement;
            root.Clear();

            Toolbar bar = new Toolbar();
            root.Add(bar);

            #region List View

            VisualElement container = new VisualElement();
            root.Add(container);
            container.style.flexDirection = FlexDirection.Row;
            container.style.flexGrow = 100;

            VisualElement levelSelection = CreateLevelSelection();
            container.Add(levelSelection);

            VisualElement partnerSelection = CreateRoleSelection();
            container.Add(partnerSelection);

            List<string> missingFiles = new List<string>();
            List<string> requiredFiles = new List<string>();
            List<string> notRequiredFiles = new List<string>();

            ListView missingFilesListView = CreateResultUI(container, missingFiles, "缺失引用", false);
            ListView requiredFileListView = CreateResultUI(container, requiredFiles, "引用资源", true);
            ListView notRequiredFileListView = CreateResultUI(container, notRequiredFiles, "未引用资源", true);
            #endregion

            #region Show Result
            async Task ShowResult(ScannerResult result) {
                requiredFiles.Clear();
                requiredFiles.AddRange(result.AllFiles);
                requiredFileListView.Refresh();
                _onResultChange?.Invoke();

                ScannerResult allFiles = new ScannerResult();
                string skillPath = $@"{DATAPATH}\BundleRes\SkillPackage\";
                string animationPath = $@"{DATAPATH}\BundleRes\Animation\";
                string fxPath = $@"{DATAPATH}\BundleRes\Effects";
                string timelinePath = $@"{DATAPATH}\BundleRes\Timeline";
                string behitPath = $@"{DATAPATH}\BundleRes\HitPackage\";

                var allfiles = GetAllFiles(new DirectoryInfo(skillPath), new string[] { ".ecfg", ".bytes"});
                for (int i = 0; i < allfiles.Count; i++) {
                    allFiles.skillFiles.Add(allfiles[i].FullName.ToLower());
                }
                allfiles = GetAllFiles(new DirectoryInfo(animationPath), "*.anim");
                for (int i = 0; i < allfiles.Count; i++) {
                    allFiles.animationFiles.Add(allfiles[i].FullName.ToLower());
                }
                allfiles = GetAllFiles(new DirectoryInfo(fxPath), "*.prefab");
                for (int i = 0; i < allfiles.Count; i++) {
                    allFiles.FxFiles.Add(allfiles[i].FullName.ToLower());
                }
                allfiles = GetAllFiles(new DirectoryInfo(timelinePath), new string[] { ".prefab", ".playable", "_env.asset" });
                for (int i = 0; i < allfiles.Count; i++) {
                    allFiles.TimelineFiles.Add(allfiles[i].FullName.ToLower());
                }

                allfiles = GetAllFiles(new DirectoryInfo(behitPath), new string[] { ".ecfg", ".bytes"});
                for (int i = 0; i < allfiles.Count; i++) {
                    allFiles.BehitFiles.Add(allfiles[i].FullName.ToLower());
                }

                notRequiredFiles.Clear();
                allFiles.Difference(result);
                notRequiredFiles.AddRange(allFiles.AllFiles);
                notRequiredFileListView.Refresh();

                missingFiles.Clear();
                missingFiles.AddRange(result.MissingFiles);
                missingFilesListView.Refresh();

                _onResultChange?.Invoke();
                await Task.Yield();
            }

            #endregion

            #region Toolbar button

            #region Open Graph Button

            ToolbarButton button = new ToolbarButton();
            button.text = "打开关卡";
            bar.Add(button);
            button.clicked += async () => {
                TableDatabase.Instance.ClearAllSet();
                string path = EditorUtility.OpenFilePanel("打开关卡", $@"{Application.dataPath}\BundleRes\Table\Level\", "cfg"); ;
                if (File.Exists(path)) {
                    var task = ScanLevel(path, - 1);
                    await task;
                }
            };

            #endregion

            #region Scan All Button

            //ToolbarButton buttonScanAll = new ToolbarButton();
            //buttonScanAll.text = "扫描全部";
            //bar.Add(buttonScanAll);

            //buttonScanAll.clicked += async () => {
            //    if (_isRunnning)
            //        return;
            //    _isRunnning = true;

            //    ClearAllSet();
            //    requiredFiles.Clear();
            //    notRequiredFiles.Clear();
            //    requiredFileListView.Refresh();
            //    notRequiredFileListView.Refresh();
            //    await Task.Yield();

            //    int progressID = Progress.Start("扫描文件...");
            //    string levelpath = $@"{DATAPATH}\BundleRes\Table\Level\";
            //    var task = Task.Run(()=>GetAllFiles(new DirectoryInfo(levelpath), "*.cfg"));
            //    await task;
            //    var files = task.Result;
            //    ScannerResult result = new ScannerResult();
            //    List<Task<ScannerResult>> tasks = new List<Task<ScannerResult>>();
            //    await Task.Run(()=> {
            //        for (int i = 0; i < files.Count; i++) {
            //            tasks.Add(ScanLevel(files[i].FullName, progressID));
            //            Progress.Report(progressID, i / (float)files.Count);
            //        }

            //        for (int i = 0; i < _roleTable.Table.Count; i++) {
            //            Progress.Report(progressID, (files.Count + i) / (float)(files.Count + _roleTable.Table.Count));
            //            tasks.Add(ScanPartnerInfo(i));
            //        }
            //    });

            //    for (int i = 0; i < tasks.Count; i++) {
            //        await tasks[i];
            //        result.AddRange(tasks[i].Result);
            //    }

            //    Progress.Remove(progressID);
            //    await ShowResult(result);

            //    Debug.LogWarning($"总文件数:{requiredFiles.Count + notRequiredFiles.Count} 需要的文件数:{requiredFiles.Count}  不需要的文件数:{notRequiredFiles.Count}");
            //    _isRunnning = false;
            //};

            #endregion

            #region Scan Selected

            ToolbarButton scanSelectedButton = new ToolbarButton();
            bar.Add(scanSelectedButton);
            scanSelectedButton.text = "扫描所选的关卡和角色";
            scanSelectedButton.clicked += async () => {
                if (_isRunnning)
                    return;
                _isRunnning = true;
                string levelpath = $@"{DATAPATH}\BundleRes\Table\";

                TableDatabase.Instance.ClearAllSet();
                requiredFiles.Clear();
                notRequiredFiles.Clear();
                _timelineToScan.Clear();
                requiredFileListView.Refresh();
                notRequiredFileListView.Refresh();
                await Task.Yield();

                int progressID = Progress.Start("扫描文件...");
                List<Task<ScannerResult>> tasks = new List<Task<ScannerResult>>();
                ScannerResult results = new ScannerResult();
                Reference = new Dictionary<ReferenceRecord, Dictionary<ReferenceRecord, int>>();

                await Task.Run(() => {
                    for (int i = 0; i < _mapToggleArray.Length; i++) {
                        if (_mapToggleArray[i].value && _mapList[i]["LevelConfigFile"] != "") {
                            if (_mapList[i]["Buff"] != "")
                                results.AddRange(ScanBuff(_mapList[i]["Buff"], new ReferenceRecord(NodeType.Map, _mapList[i]["MapID"])));
                            tasks.Add(ScanLevel($"{levelpath}{_mapList[i]["LevelConfigFile"]}.cfg", progressID));

                        }
                        Progress.Report(progressID, i / (float)(_mapToggleArray.Length + _roleToggleArray.Length));
                    }

                    for (int i = 0; i < _roleToggleArray.Length; i++) {
                        Progress.Report(progressID, (i + _mapToggleArray.Length) / (float)(_mapToggleArray.Length + _roleToggleArray.Length));
                        if (_roleToggleArray[i].value) {
                            tasks.Add(ScanPartnerInfo(i));
                        }
                    }
                });
                for (int i = 0; i < tasks.Count; i++) {
                    await tasks[i];
                    results.AddRange(tasks[i].Result);
                    _timelineToScan.AddRange(tasks[i].Result.TimelineToScan);
                }

                if (_scanTimeline) {
                    for (int i = 0; i < _timelineToScan.Count; i++) {
                        results.AddRange(ScanCutScene(_timelineToScan[i]));
                    }
                }
                await ShowResult(results);

                Progress.Remove(progressID);
                _isRunnning = false;
            };
            #endregion

            Toggle toggleScanTL = new Toggle();
            toggleScanTL.text = "扫描Timeline资源";
            toggleScanTL.value = _scanTimeline;
            toggleScanTL.RegisterValueChangedCallback(obj => {
                _scanTimeline = obj.newValue;
            });
            bar.Add(toggleScanTL);

            #endregion
        }

        ListView CreateResultUI(VisualElement root, List<string> results, string title, bool shouldCorp) {
            string searchString = "";
            List<string> filteredList = new List<string>();

            VisualElement MakeItem() {
                VisualElement item = new VisualElement();
                Label label = new Label();
                item.Add(label);
                return label;
            }

            void BindItem(VisualElement ve, int index) {
                Label label = ve.Q<Label>();
                if (searchString == "") {
                    if (shouldCorp && results[index].Contains(@"\bundleres\"))
                        label.text = results[index].Substring(results[index].IndexOf(@"\bundleres\") + 11);
                    else
                        label.text = results[index];
                } else {
                    if (shouldCorp && filteredList[index].Contains(@"\bundleres\"))
                        label.text = filteredList[index].Substring(filteredList[index].IndexOf(@"\bundleres\") + 11);
                    else
                        label.text = filteredList[index];
                }
            }

            VisualElement container = new VisualElement();
            container.style.flexGrow = 100;
            Toolbar bar = new Toolbar();
            bar.style.flexShrink = 0;
            container.Add(bar);
            ToolbarPopupSearchField searchfield = new ToolbarPopupSearchField();
            ToolbarButton buttonSaveToFile = new ToolbarButton();
            buttonSaveToFile.style.marginLeft = 16;
            buttonSaveToFile.text = "保存到文件";
            buttonSaveToFile.clicked += () => {
                string path = EditorUtility.SaveFilePanel("保存", "", "扫描结果", "txt");
                if (path == "")
                    return;
                using var sr = new StreamWriter(path);
                for (int i = 0; i < results.Count; i++) {
                    sr.WriteLine(results[i]);
                }
            };
            bar.Add(buttonSaveToFile);
            Label totalCount = new Label();
            totalCount.text = $"总文件数:0";
            bar.Add(totalCount);
            bar.Add(searchfield);

            ListView listView = new ListView(results, 16, MakeItem, BindItem);
            listView.style.flexGrow = 100;
            listView.horizontalScrollingEnabled = true;
            container.Add(listView);
            Label matchCount = new Label();
            matchCount.text = $"符合条件的项:0";
            matchCount.style.display = DisplayStyle.None;
            bar.Add(matchCount);
            ToolbarButton saveSearchedOnly = new ToolbarButton();
            saveSearchedOnly.text = "仅保存搜索结果";
            saveSearchedOnly.style.display = DisplayStyle.None;
            saveSearchedOnly.clicked += () => {
                string path = EditorUtility.SaveFilePanel("保存", "", "扫描结果", "txt");
                if (path == "")
                    return;
                using var sr = new StreamWriter(path);
                for (int i = 0; i < filteredList.Count; i++) {
                    sr.WriteLine(filteredList[i]);
                }
            };
            bar.Add(saveSearchedOnly);
            searchfield.RegisterValueChangedCallback(obj => {
                searchString = obj.newValue.ToLower();
                if (searchString == "") {
                    listView.itemsSource = results;
                    matchCount.style.display = DisplayStyle.None;
                    saveSearchedOnly.style.display = DisplayStyle.None;
                } else {
                    filteredList.Clear();
                    string[] searchs = searchString.Split(' ');
                    int length = searchs.Length;
                    int count = results.Count;
                    for (int i = 0; i < count; i++) {
                        bool match = true;
                        for (int j = 0; j < length; j++) {
                            if (!results[i].ToLower().Contains(searchs[j])) {
                                match = false;
                                break;
                            }
                        }

                        if(match)
                            filteredList.Add(results[i]);
                    }
                    listView.itemsSource = filteredList;
                    matchCount.text = $"符合条件的项:{filteredList.Count}";
                    matchCount.style.display = DisplayStyle.Flex;
                    saveSearchedOnly.style.display = DisplayStyle.Flex;
                }
                listView.Refresh();
            });

            _onResultChange += () => {
                totalCount.text = $"总文件数:{results.Count}";
            };

            root.Add(container);
            return listView;
        }

        VisualElement CreateRoleSelection() {
            VisualElement root = new VisualElement();
            root.style.width = 128;
            ScrollView scrollView = new ScrollView();

            Toolbar toolbar = new Toolbar();
            toolbar.style.flexShrink = 0;
            ToolbarButton loadConfigButton = new ToolbarButton();
            loadConfigButton.text = "读取";
            loadConfigButton.clicked += () => {
                string path = EditorUtility.OpenFilePanel("读取配置", "", "txt");
                if (path != null && File.Exists(path)) {
                    int count = _roleToggleArray.Length;
                    for (int i = 0; i < count; i++)
                        _roleToggleArray[i].SetValueWithoutNotify(false);
                    try {
                        using StreamReader stream = new StreamReader(path);
                        while (!stream.EndOfStream) {
                            string line = stream.ReadLine();
                            for (int i = 0; i < count; i++) {
                                if (_roleTable.Table[i]["ID"].Equals(line)) {
                                    _roleToggleArray[i].value = true;
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
            saveConfigButton.text = "保存";
            saveConfigButton.clicked += () => {
                string path = EditorUtility.SaveFilePanel("保存配置", "", "新角色选取配置", "txt");
                if (path != null) {
                    using StreamWriter stream = new StreamWriter(path, false);
                    int count = _roleToggleArray.Length;
                    for (int i = 0; i < count; i++) {
                        if (_roleToggleArray[i].value)
                            stream.WriteLine(_roleTable.Table[i]["ID"]);
                    }
                }
            };
            toolbar.Add(saveConfigButton);

            Toggle selectAllToggle = new Toggle();
            selectAllToggle.text = "全选";
            selectAllToggle.RegisterValueChangedCallback(obj => {
                for (int i = 0; i < _roleToggleArray.Length; i++) {
                    _roleToggleArray[i].value = obj.newValue;
                }
            });
            toolbar.Add(selectAllToggle);

            root.Add(toolbar);

            Toolbar searchbar = new Toolbar();
            root.Add(searchbar);
            ToolbarPopupSearchField searchFiled = new ToolbarPopupSearchField();
            searchbar.style.flexShrink = 0;
            searchbar.Add(searchFiled);
            searchFiled.style.width = 128;
            searchFiled.RegisterValueChangedCallback(obj => {
                for (int i = 0; i < _roleTable.Table.Count; i++) {
                    if (_roleToggleArray[i].text.Contains(obj.newValue))
                        _roleToggleArray[i].style.display = DisplayStyle.Flex;
                    else
                        _roleToggleArray[i].style.display = DisplayStyle.None;
                }
            });

            _roleToggleArray = new Toggle[_roleTable.Table.Count];

            for (int i = 0; i < _roleTable.Table.Count; i++) {
                VisualElement element = new VisualElement();
                Toggle toggle = new Toggle();
                _roleToggleArray[i] = toggle;
                toggle.text = _roleTable.Table[i]["Name"];
                if (_roleTable.Table[i]["Open"].Equals("TRUE"))
                    toggle.SetValueWithoutNotify(true);
                else
                    toggle.SetValueWithoutNotify(false);
                element.style.flexDirection = FlexDirection.Row;
                element.Add(toggle);
                scrollView.Add(element);
            }
            root.Add(scrollView);
            return root;
        }

        List<FileInfo> GetAllFiles(DirectoryInfo rootDir, string[] extentions) {
            List<FileInfo> result = new List<FileInfo>();
            var files = rootDir.GetFiles();
            for (int i = 0; i < files.Length; i++) {
                for (int j = 0; j < extentions.Length; j++) {
                    if (files[i].Extension.Equals(extentions[j])) {
                        result.Add(files[i]);
                        break;
                    }
                }
            }
            var dirs = rootDir.GetDirectories();
            for (int i = 0; i < dirs.Length; i++) {
                result.AddRange(GetAllFiles(dirs[i], extentions));
            }
            return result;
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

        #endregion

        #region Scan Specific

        ScannerResult ScanMonster(string id, ReferenceRecord from) {
            ScannerResult results = new ScannerResult();

            if (!_monsterTable.Dic.ContainsKey(id)) {
                results.MissingFiles.Add($"怪物ID: {id} 不存在");
                //Debug.Log($"找不到怪物ID: {id}");
                return results;
            }

            var recordKey = new ReferenceRecord(NodeType.Enemy, id);
            AddReference(recordKey, from);

            if (_monsterTable.Set.Contains(id))
                return results;
            _monsterTable.Set.Add(id);

            Dictionary<string, string> row = _monsterTable.Table[_monsterTable.Dic[id]];
            //Debug.Log($"扫描怪物: {id} {row["Note"]}");

            if (row["AIID"] != "")
                results.AddRange(ScanUnitAI(row["AIID"], row["PresentID"]));

            if (row["PresentID"] != "")
                results.AddRange(ScanPresentID(row["PresentID"]));

            try {
                if (row["DropObjectIDs"] != "") {
                    string[] ids = row["DropObjectIDs"].Split('|');
                    for (int i = 0; i < ids.Length; i++) {
                        results.AddRange(ScanDropObject(ids[i]));
                    }
                }
            } catch {
                Debug.Log($"掉落物错误: {row["DropObjectIDs"]}");
            }

            if (row["InBornBuff"] != "") {
                results.AddRange(ScanBuff(row["InBornBuff"], new ReferenceRecord(NodeType.Enemy, id), row["PresentID"]));
            }

            if (row["EnterFightBuff"] != "") {
                results.AddRange(ScanBuff(row["EnterFightBuff"], new ReferenceRecord(NodeType.Enemy, id), row["PresentID"]));
            }

            if (_enemyModeStateTable.Dic.ContainsKey(row["ID"]) && !_enemyModeStateTable.Set.Contains(row["ID"]))
                results.AddRange(ScanBK(row["ID"], row["PresentID"]));

            if (_enemyStageTable.Dic.ContainsKey(id)) {
                var stage = _enemyStageTable.Table[_enemyStageTable.Dic[id]];

                if (stage["SkillID"] != "") {
                    results.AddRange(ScanSkill(stage["SkillID"], row["PresentID"]));
                    //Debug.LogWarning(stage["SkillID"]);
                }

                if (stage["BuffIDList"] != "") {
                    string[] buffs = stage["BuffIDList"].Split('|');
                    for (int i = 0; i < buffs.Length; i++) {
                        string[] buff = buffs[i].Split('=');
                        results.AddRange(ScanBuff($"{buff[1]}={buff[2]}", new ReferenceRecord(NodeType.EmemyStage, id), row["PresentID"]));
                        //Debug.LogWarning($"{buff[1]}={buff[2]}");
                    }
                }

                if (stage["CutScene"] != "") {
                    Debug.LogWarning(stage["CutScene"]);
                }
            }

            return results;
        }

        ScannerResult ScanBK(string enemyID, string presentID) {
            ScannerResult results = new ScannerResult();
            _enemyModeStateTable.Set.Add(enemyID);
            var row = _enemyModeStateTable.Table[_enemyModeStateTable.Dic[enemyID]];

            if (row["BKEnterSkillID"] != "")
                results.AddRange(ScanSkill(row["BKEnterSkillID"], presentID));

            if (row["NormalEnterSkillID"] != "")
                results.AddRange(ScanSkill(row["NormalEnterSkillID"], presentID));

            if (row["BindSkillList"] != "")
                results.AddRange(ScanSkill(row["BindSkillList"], presentID));

            if (row["BKEnterBuffID"] != "") {
                string[] buffes = row["BKEnterBuffID"].Split('=');
                results.AddRange(ScanBuff($"{buffes[0]}={buffes[1]}", new ReferenceRecord(NodeType.EmemyModeState, enemyID), presentID));
            }

            return results;
        }

        ScannerResult ScanDropObject(string id) {
            ScannerResult results = new ScannerResult();
            if (_dropObjectsTable.Set.Contains(id) || !_dropObjectsTable.Dic.ContainsKey(id)) {
                if (!_dropObjectsTable.Dic.ContainsKey(id)) {
                    results.MissingFiles.Add($"掉落物ID: {id} 不存在");
                    //Debug.Log($"找不到掉落物ID: {id}");
                }
                return results;
            }
            _dropObjectsTable.Set.Add(id);

            Dictionary<string, string> row = _dropObjectsTable.Table[_dropObjectsTable.Dic[id]];
            //Debug.Log($"扫描掉落物: {id}");
            if (row["EnemyID"] != "")
                results.AddRange(ScanMonster(row["EnemyID"], new ReferenceRecord(NodeType.DropObject, id)));

            void ScanEffect(string effect) {
                if (effect != "") {
                    string[] effects = effect.Split('|');
                    for (int i = 0; i < effects.Length; i++) {
                        string fxFile = $@"{DATAPATH}\BundleRes\{effects[i]}.prefab".ToLower().Replace('/', '\\');
                        //if (_effectDictionary.ContainsKey(fxFile))
                        //Debug.LogWarning(effects[i]);
                        results.FxFiles.Add(fxFile);
                    }
                }
            }

            ScanEffect(row["IdleEffectFriends"]);
            ScanEffect(row["IdleEffectEnemy"]);
            ScanEffect(row["DropFx"]);
            ScanEffect(row["DeathFx"]);
            ScanEffect(row["AttachFx"]);
            ScanEffect(row["Draweffect"]);

            if (row["PickBuff"] != "")
                results.AddRange(ScanBuff(row["PickBuff"], new ReferenceRecord(NodeType.DropObject, id)));

            if (row["IdleBuff"] != "")
                results.AddRange(ScanBuff(row["IdleBuff"], new ReferenceRecord(NodeType.DropObject, id)));

            return results;
        }

        ScannerResult ScanPetSkill(string skillName, string presentID) {
            ScannerResult results = new ScannerResult();
            if (skillName == "" || _skillListForPet.Set.Contains(skillName)) {
                return results;
            }

            if (skillName.Contains("=") || skillName.Contains("|")) {
                string[] s = skillName.Split('=', '|');
                for (int i = 0; i < s.Length; i++)
                    results.AddRange(ScanPetSkill(s[i], presentID));
                return results;
            }

            if (!_skillListForPet.Dic.ContainsKey(skillName)) {
                results.MissingFiles.Add($"宠物技能 {skillName} 不存在");
                //Debug.Log($"找不到宠物技能 {skillName}");
                return results;
            }

            Dictionary<string, string> pRow = _presentationTable.Table[_presentationTable.Dic[presentID]];
            _skillListForPet.Set.Add(skillName);



            string path = $@"{DATAPATH}\BundleRes\SkillPackage\{pRow["SkillLocation"]}{skillName}".Replace('/', '\\');
            if (!File.Exists($@"{path}.bytes")) {
                results.MissingFiles.Add($@"技能文件:{skillName}.bytes 不存在");
                //Debug.Log($@"技能文件:{path}.bytes 不存在");
            } else {
                results.skillFiles.Add($"{path}.bytes".ToLower());
                results.AddRange(ScanSkillGraph($@"{path}.bytes", presentID));
            }

            if (!File.Exists($@"{path}.ecfg")) {
                results.MissingFiles.Add($@"技能文件:{skillName}.ecfg 不存在");
                //Debug.Log($@"技能文件:{path}.ecfg 不存在");
            } else {
                results.skillFiles.Add($"{path}.ecfg".ToLower());
            }

            return results;
        }



        ScannerResult ScanBehitGraph(string behitPath, string presentID) {
            ScannerResult results = new ScannerResult();
            XHitData configData = DeserializeEcsData<XHitData>(behitPath);

            for (int i = 0; i < configData?.AnimationData.Count; i++) {
                var anim = configData.AnimationData[i];
                //Debug.LogWarning($@"扫描动画 {DATAPATH}\BundleRes\{anim.ClipPath}.anim".Replace('/','\\'));
                results.animationFiles.Add($@"{DATAPATH}\BundleRes\{anim.ClipPath}.anim".Replace('/', '\\').ToLower());
            }

            for (int i = 0; i < configData.BuffData.Count; i++) {
                var buff = configData.BuffData[i];
                results.AddRange(ScanBuff($"{buff.BuffID}={buff.BuffLevel}", new ReferenceRecord(NodeType.BeHit, behitPath), presentID));
                //Debug.LogWarning($"{buff.BuffID}={buff.BuffLevel}");
            }

            for (int i = 0; i < configData.FxData.Count; i++) {
                var fx = configData.FxData[i];
                if (fx.FxPath == "")
                    continue;
                string fileName = $"{fx.FxPath}.prefab".ToLower();
                if (_effectDictionary.ContainsKey(fileName)) {
                    results.FxFiles.Add(_effectDictionary[fileName].ToLower());
                } else {
                    results.MissingFiles.Add($"特效文件: {fileName} 不存在");
                    //Debug.Log($"fx: {fileName} not found!");
                }
            }

            ///扫描Trans
            for (int i = 0; i < configData.ScriptTransData.Count; i++) {
                var trans = configData.ScriptTransData[i];
                //results.AddRange(ScanSkill(trans.Name, presentID));
                string path = behitPath.Substring(0, behitPath.LastIndexOf('\\'));
                results.AddRange(ScanBehitGraph($@"{path}\{trans.Name}.bytes", presentID));
                results.BehitFiles.Add($@"{path}\{trans.Name}.bytes".ToLower());
                results.AddRange(ScanSkill(trans.Name, presentID));
                //Debug.LogWarning($@"{path}\{trans.Name}.ecfg");
            }

            return results;
        }

        ScannerResult ScanUnitAI(string id, string presentID) {
            ScannerResult results = new ScannerResult();
            if (_unitAITable.Set.Contains(id) || !_unitAITable.Dic.ContainsKey(id)) {
                if (!_unitAITable.Dic.ContainsKey(id))
                    Debug.Log($"找不到UnitAI: {id}");
                return results;
            }
            _unitAITable.Set.Add(id);
            
            Dictionary<string, string> row = _unitAITable.Table[_unitAITable.Dic[id]];
            //Debug.Log($"扫描AI: {id}{row["Comment"]}");
            results.AddRange(ScanSkill(row["MainSkillName"], presentID));
            results.AddRange(ScanSkill(row["LeftSkillName"], presentID));
            results.AddRange(ScanSkill(row["RightSkillName"], presentID));
            results.AddRange(ScanSkill(row["BackSkillName"], presentID));
            results.AddRange(ScanSkill(row["CheckingSkillName"], presentID));
            results.AddRange(ScanSkill(row["CheckingSkillAndStopName"], presentID));
            results.AddRange(ScanSkill(row["DashSkillName"], presentID));
            results.AddRange(ScanSkill(row["FarSkillName"], presentID));
            results.AddRange(ScanSkill(row["SelectSkillName"], presentID));
            results.AddRange(ScanSkill(row["UnusedSkillName"], presentID));
            results.AddRange(ScanSkill(row["TurnSkillName"], presentID));
            results.AddRange(ScanSkill(row["MoveSkillName"], presentID));

            if (row["CustomVariables"] == "")
                return results;
            try {
                string[] s = row["CustomVariables"].Split('|');
                for (int i = 0; i < s.Length; i++) {
                    string[] ss = s[i].Split('=');
                    if (ss[0].Contains("SkillName")) {
                        string[] skills = ss[1].Split('^');
                        for (int j = 0; j < skills.Length; j++) {
                            //Debug.LogWarning($"自定义技能 {skills[j]}");
                            results.AddRange(ScanSkill(skills[j], presentID));
                        }
                    } else if (ss[0].Contains("BuffId")) {
                        //Debug.LogWarning($"自定义buff {ss[1]}");
                        results.AddRange(ScanBuff(ss[1], new ReferenceRecord(NodeType.AI, id), presentID));
                    } else if (ss[0].Contains("Monster")) {
                        //Debug.LogWarning($"自定义怪 {ss[1]}");
                        results.AddRange(ScanMonster(ss[1], new ReferenceRecord(NodeType.AI, id)));
                    }
                }
            } catch{
                Debug.Log($"Error on Custom Variables {row["CustomVariables"]}");
            }

            if (row["Tree"] != "")
                results.AddRange(ScanBT(row["Tree"], presentID));

            if (row["Combat_SubTree"] != "")
                results.AddRange(ScanBT(row["Combat_SubTree"], presentID));

            if (row["Combat_PreCombatSubTree"] != "")
                results.AddRange(ScanBT(row["Combat_PreCombatSubTree"], presentID));

            return results;
        }

        ScannerResult ScanBT(string treeName, string presentID) {
            ScannerResult results = new ScannerResult();
            string path = $@"{BTPATH}{treeName}.xml";
            FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            XmlTextReader xr = new XmlTextReader(fs);
            xr.WhitespaceHandling = WhitespaceHandling.None;

            while (xr.Read()) {
                switch (xr.NodeType) {
                    case XmlNodeType.Element:
                        string nodeClass = xr.GetAttribute("Class");
                        if (nodeClass != null && nodeClass.Equals("AICastSkillByName")) {
                            string[] names = xr.GetAttribute("Name").Split(' ');
                            results.AddRange(ScanSkill(names[1], presentID));
                        }
                        break;
                }
            }
            return results;
        }

        ScannerResult ScanCutScene(string name) {
            var results = new ScannerResult();
            if (name == "")
                return results;

            string path = $@"{DATAPATH}\BundleRes\Timeline\{name}";

            if (File.Exists($"{path}.prefab")) {
                results.TimelineFiles.Add($"{path}.prefab".ToLower());
            } else {
                results.MissingFiles.Add($@"过场动画 Timeline\{name}.prefab 不存在");
                //Debug.Log($"过场动画 {path}.prefab 不存在");
            }

            if (File.Exists($"{path}.playable")) {
                results.TimelineFiles.Add($"{path}.playable".ToLower());

                TimelineAsset timelineAsset = AssetDatabase.LoadAssetAtPath<TimelineAsset>($@"Assets\BundleRes\Timeline\{name}.playable");
                if (timelineAsset != null) {
                    List<TrackAsset> tracks = new List<TrackAsset>();
                    tracks.AddRange(timelineAsset.GetOutputTracks());
                    for (int i = 0; i < tracks.Count; i++) {
                        var clips = tracks[i].GetClipList();
                        for (int j = 0; j < clips.Count; j++) {
                            var clip = clips[j];
                            var asset = clip.animationClip;
                            if (asset != null) {
                                string assetPath = AssetDatabase.GetAssetPath(asset);
                                string animationPath = $@"{DATAPATH}{assetPath.Substring(assetPath.IndexOf('/'))}";
                                results.animationFiles.Add(animationPath.Replace('/', '\\').ToLower());
                                //Debug.LogWarning(animationPath);
                            }
                        }
                    }
                } else {
                    results.MissingFiles.Add($@"无法读取timeline: Timeline\{name}.playable");
                    //Debug.LogWarning($@"无法读取timeline文件: Assets\BundleRes\Timeline\{name}.playable");
                }
            }

            if (File.Exists($"{path}_env.asset")) {
                results.TimelineFiles.Add($"{path}_env.asset".ToLower());
            }

            return results;
        }

        #endregion

        public void AddReference(ReferenceRecord key, ReferenceRecord value) {
            if (!Reference.ContainsKey(key)) {
                Reference[key] = new Dictionary<ReferenceRecord, int>();
            }
            if (!Reference[key].ContainsKey(value)) {
                Reference[key][value] = 1;
            } else {
                Reference[key][value]++;
            }
        }

        public async Task ScanAll() {
            if (_isRunnning)
                return;
            _isRunnning = true;
            string levelpath = $@"{DATAPATH}\BundleRes\Table\";

            TableDatabase.Instance.ClearAllSet();
            //requiredFiles.Clear();
            //notRequiredFiles.Clear();
            _timelineToScan.Clear();
            await Task.Yield();

            int progressID = Progress.Start("扫描文件...");
            List<Task<ScannerResult>> tasks = new List<Task<ScannerResult>>();
            ScannerResult results = new ScannerResult();
            Reference = new Dictionary<ReferenceRecord, Dictionary<ReferenceRecord, int>>();

            _mapList = TableChecker.TableChecker.ReadTable("MapList");
            _roleTable = TableDatabase.Instance.GetTable("PartnerInfo", "ID", "{ID} {Name}");

            await Task.Run(() => {

                for (int i = 0; i < _mapList.Count; i++) {
                    if (_mapList[i]["LevelConfigFile"] != "") {
                        if (_mapList[i]["Buff"] != "") {
                            results.AddRange(ScanBuff(_mapList[i]["Buff"], new ReferenceRecord(NodeType.Map, _mapList[i]["MapID"])));
                        }
                        tasks.Add(ScanLevel($"{levelpath}{_mapList[i]["LevelConfigFile"]}.cfg", progressID));

                    }
                    Progress.Report(progressID, i / (float)(_mapList.Count + _roleTable.Table.Count));
                }

                for (int i = 0; i < _roleTable.Table.Count; i++) {
                    Progress.Report(progressID, (i + _roleTable.Table.Count) / (float)(_mapList.Count + _roleTable.Table.Count));
                        tasks.Add(ScanPartnerInfo(i));
                }
            });
            for (int i = 0; i < tasks.Count; i++) {
                await tasks[i];
                results.AddRange(tasks[i].Result);
                _timelineToScan.AddRange(tasks[i].Result.TimelineToScan);
            }

            if (_scanTimeline) {
                for (int i = 0; i < _timelineToScan.Count; i++) {
                    results.AddRange(ScanCutScene(_timelineToScan[i]));
                }
            }

            Progress.Remove(progressID);
            _isRunnning = false;
        }

    }
}
