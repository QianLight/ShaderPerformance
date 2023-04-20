#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace TDTools.ExplorationEventEditor {
    [CustomEditor(typeof(ExplorationEventEditorManager))]
    public class ExplorationEventManagerCustomEditor : Editor {

        private ExplorationEventEditorManager _owner;
        private SerializedProperty _events;

        private List<Dictionary<string, string>> _table;
        private Dictionary<int, Dictionary<string, string>> _tableDic;

        private List<Dictionary<string, string>> _seaAreaTable;
        private int[] _seaAreaIDList;
        private string[] _displayedOptions;

        private int _maxRow;

        private readonly HashSet<string> SUPPORTED_HEADER = new HashSet<string>{"ID","Comment", "SeaAreaID", "TriggerPos", "XEntityBinded", "IslandBinded", "Script", "ActiveRule", "Effect"};

        [MenuItem("GameObject/Streamer/事件编辑工具", false, 0)]
        static void CreateManager() {
            var g = new GameObject("ExplorationEventEditor");
            g.AddComponent<ExplorationEventEditorManager>();
            g.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
        }

        [MenuItem("GameObject/Streamer/事件", false, 0)]
        static void CreateEvent() {
            var g = new GameObject("New Event");
            g.AddComponent<ExplorationEventHelper>().IsNew = true;
            g.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
        }

        void NewEvent() {
            var g = new GameObject("New Event");
            g.transform.parent = _owner.transform;
            ExplorationEventHelper newEvent = g.AddComponent<ExplorationEventHelper>();
            newEvent.IsNew = true;
            newEvent.AreaID = _owner.AreaID;
            _owner.Events.Add(newEvent);
            g.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
        }

        void OnEnable() {
            try {
                _owner = (ExplorationEventEditorManager)target;
                _events = serializedObject.FindProperty("Events");

                _seaAreaTable = TableReader.ReadTable("SeaExplore/SeaArea", true);
                _seaAreaIDList = new int[_seaAreaTable.Count];
                _displayedOptions = new string[_seaAreaTable.Count];

                bool areaAlreadySet = false;
                for (int i = 0; i < _seaAreaTable.Count; i++) {
                    _displayedOptions[i] = $"{_seaAreaTable[i]["ID"]} {_seaAreaTable[i]["Name"]}";
                    _seaAreaIDList[i] = int.Parse(_seaAreaTable[i]["ID"]);
                    if (_owner.AreaID == _seaAreaIDList[i])
                        areaAlreadySet = true;
                }

                if (!areaAlreadySet)
                    _owner.AreaID = _seaAreaIDList[0];

            } catch{ 
            }
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            if (GUILayout.Button("从表格中读取当前海域的所有事件")) {
                if (_owner.Events.Count == 0 || EditorUtility.DisplayDialog("读表", "确认读取表格中的事件? 所有管理器下未保存的子事件会丢失。请确保SeaEvent表没有被其他程序占用", "读取", "取消"))
                    LoadAllEvents();
            }

            EditorGUILayout.Separator();

            _owner.AreaID = EditorGUILayout.IntPopup("海域ID:", _owner.AreaID, _displayedOptions, _seaAreaIDList);

            if (GUILayout.Button("新建事件"))
                NewEvent();

            EditorGUILayout.PropertyField(_events, new GUIContent("事件列表"));

            EditorGUILayout.Separator();

            if (GUILayout.Button("将所有事件保存到表格中")) {
                if(EditorUtility.DisplayDialog("保存", "确认保存到SeaEvent表格? 这可能会覆盖表内数据。请确保SeaEvent表没有被其他程序占用", "保存", "取消"))
                    SaveToTable();
            }

            serializedObject.ApplyModifiedProperties();
        }

        void SaveToTable() {
            _table = TableReader.ReadTable("SeaExplore/SeaEvent", true);

            _tableDic = new Dictionary<int, Dictionary<string, string>>();
            for (int i = 0; i < _table.Count; i++)
                _tableDic[i] = _table[i];
            _maxRow = _table.Count;

            HashSet<string> set = new HashSet<string>();
            for (int i = 0; i < _owner.Events.Count; i++) {

                if (_owner.Events[i] == null) {
                    Debug.Log("事件列表中有null项");
                    continue;
                }

                if (!set.Contains($"{_owner.Events[i].ID}")) {
                    set.Add($"{_owner.Events[i].ID}");
                } else {
                    Debug.Log($"检测到重复的事件ID！ {_owner.Events[i].ID}");
                    EditorUtility.DisplayDialog("重复ID", $"检测到重复的事件ID！ {_owner.Events[i].ID}", "确定");
                }

                if (_owner.Events[i].AreaID != _owner.AreaID)
                    Debug.Log($"检测到{_owner.Events[i].ID}事件的海域ID和当前管理器海域ID不一致，请检查是否有误");

                if (_owner.Events[i].IsNew) {
                    _tableDic[_maxRow] = _owner.Events[i].ToDic();
                    _owner.Events[i].IsNew = false;
                    _owner.Events[i].Row = _maxRow;
                    _maxRow++;
                } else
                    _tableDic[_owner.Events[i].Row] = _owner.Events[i].ToDic();
            }

            StreamReader sr = new StreamReader($"{Application.dataPath}/Table/SeaExplore/SeaEvent.txt");
            string header = sr.ReadLine();
            string comment = sr.ReadLine();
            sr.Close();

            //Overwrite output dic to 
            StreamWriter sw = new StreamWriter($"{Application.dataPath}/Table/SeaExplore/SeaEvent.txt", false, System.Text.Encoding.Unicode) ;

            string[] s = header.Split('\t');
            sw.WriteLine(header);
            sw.WriteLine(comment);
            foreach (var pair in _tableDic) { 
                for (int j = 0; j < s.Length; j++) {
                    if (j > 0)
                        sw.Write('\t');

                    if (!pair.Value.ContainsKey(s[j])) {
                        Debug.Log($"检测到不支持的表头 {s[j]}");
                        continue;
                    }

                    sw.Write(pair.Value[s[j]]);
                }
                sw.WriteLine();
            }
            sw.Close();
        }

        void LoadAllEvents() {
            //ShowExample();
            _table = TableReader.ReadTable("SeaExplore/SeaEvent", true);

            _tableDic = new Dictionary<int, Dictionary<string, string>>();
            for (int i = 0; i < _table.Count; i++)
                _tableDic[i] = _table[i];
            CreateEventGameObject();
        }

        void CreateEventGameObject() {

            while (_owner.transform.childCount > 0)
                DestroyImmediate(_owner.transform.GetChild(0).gameObject);
            _owner.Events.Clear();

            int count = 0;

            HashSet<string> set = new HashSet<string>();

            for (int i = 0; i < _table.Count; i++) {

                if (_table[i]["SeaAreaID"].CompareTo($"{_owner.AreaID}") != 0)
                    continue;

                GameObject g = new GameObject($"Event ID {_table[i]["ID"]}");
                g.transform.parent = _owner.transform;
                string[] s = _table[i]["TriggerPos"].Split('=');

                g.transform.position = new Vector3(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]));
                _owner.Events.Add(g.AddComponent<ExplorationEventHelper>());

                _owner.Events[count].Comment = _table[i]["Comment"];
                _owner.Events[count].AreaID = _owner.AreaID;
                _owner.Events[count].Radius = float.Parse(s[3]);
                _owner.Events[count].EntityBinded = _table[i]["XEntityBinded"];
                _owner.Events[count].Island = _table[i]["IslandBinded"];
                _owner.Events[count].Script = _table[i]["Script"];
                _owner.Events[count].Effect = _table[i]["Effect"];
                if (_table[i]["ActiveRule"] != "")
                    _owner.Events[count].Trigger = (ExplorationEventHelper.TriggerType)(int.Parse(_table[i]["ActiveRule"]) - 1);
                _owner.Events[count].ID = int.Parse(_table[i]["ID"]);
                _owner.Events[count].Row = i;

                foreach (var pair in _table[i]) {
                    if (!SUPPORTED_HEADER.Contains(pair.Key)) {
                        if (!set.Contains(pair.Key)) {
                            set.Add(pair.Key);
                            Debug.Log($"检测到不支持的表头 {pair.Key}, 可能会造成错误");
                        }
                        _owner.Events[count].UnsupportedColumn[pair.Key] = pair.Value;
                    }
                }

                count++;
            }
        }

    }
}

#endif