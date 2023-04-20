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

        [MenuItem("GameObject/Streamer/�¼��༭����", false, 0)]
        static void CreateManager() {
            var g = new GameObject("ExplorationEventEditor");
            g.AddComponent<ExplorationEventEditorManager>();
            g.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
        }

        [MenuItem("GameObject/Streamer/�¼�", false, 0)]
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

            if (GUILayout.Button("�ӱ���ж�ȡ��ǰ����������¼�")) {
                if (_owner.Events.Count == 0 || EditorUtility.DisplayDialog("����", "ȷ�϶�ȡ����е��¼�? ���й�������δ��������¼��ᶪʧ����ȷ��SeaEvent��û�б���������ռ��", "��ȡ", "ȡ��"))
                    LoadAllEvents();
            }

            EditorGUILayout.Separator();

            _owner.AreaID = EditorGUILayout.IntPopup("����ID:", _owner.AreaID, _displayedOptions, _seaAreaIDList);

            if (GUILayout.Button("�½��¼�"))
                NewEvent();

            EditorGUILayout.PropertyField(_events, new GUIContent("�¼��б�"));

            EditorGUILayout.Separator();

            if (GUILayout.Button("�������¼����浽�����")) {
                if(EditorUtility.DisplayDialog("����", "ȷ�ϱ��浽SeaEvent���? ����ܻḲ�Ǳ������ݡ���ȷ��SeaEvent��û�б���������ռ��", "����", "ȡ��"))
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
                    Debug.Log("�¼��б�����null��");
                    continue;
                }

                if (!set.Contains($"{_owner.Events[i].ID}")) {
                    set.Add($"{_owner.Events[i].ID}");
                } else {
                    Debug.Log($"��⵽�ظ����¼�ID�� {_owner.Events[i].ID}");
                    EditorUtility.DisplayDialog("�ظ�ID", $"��⵽�ظ����¼�ID�� {_owner.Events[i].ID}", "ȷ��");
                }

                if (_owner.Events[i].AreaID != _owner.AreaID)
                    Debug.Log($"��⵽{_owner.Events[i].ID}�¼��ĺ���ID�͵�ǰ����������ID��һ�£������Ƿ�����");

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
                        Debug.Log($"��⵽��֧�ֵı�ͷ {s[j]}");
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
                            Debug.Log($"��⵽��֧�ֵı�ͷ {pair.Key}, ���ܻ���ɴ���");
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