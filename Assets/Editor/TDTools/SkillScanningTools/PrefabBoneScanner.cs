using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace TDTools {
    public class PrefabBoneScanner : EditorWindow {

        struct Bone {
            public string Key;
            public string Value;
        }

        List<string> results;
        ListView listView;

        [MenuItem("Tools/TDTools/��������/Prefab����ɨ�蹤��")]
        public static void ShowWindow() {
            GetWindow<PrefabBoneScanner>();
        }

        public void CreateGUI() {
            results = new List<string>();
            titleContent = new GUIContent("Prefab����ɨ�蹤��");
            var root = rootVisualElement;
            Toolbar bar = new Toolbar();
            bar.style.flexShrink = 0;
            root.Add(bar);

            ToolbarButton buttonStart = new ToolbarButton();
            buttonStart.text = "��ʼɨ��";
            buttonStart.clicked += Scan;
            bar.Add(buttonStart);

            VisualElement MakeItem() => new Label();

            void BindItem(VisualElement ve, int index) {
                Label label = ve.Q<Label>();
                label.text = results[index];
            }

            listView = new ListView(results, 16, MakeItem, BindItem);
            listView.style.flexGrow = 100;
            root.Add(listView);

            ToolbarButton buttonSave = new ToolbarButton();
            buttonSave.text = "������";
            buttonSave.clicked += () => {
                string path = EditorUtility.SaveFilePanel("������", "", "����ɨ����", "txt");
                using StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.UTF8);
                for (int i = 0; i < results.Count; i++)
                    sw.WriteLine(results[i]);
            };
            bar.Add(buttonSave);

        }

        void Scan() {
            results.Clear();
            var table = TableChecker.TableChecker.ReadTable("XEntityPresentation");
            var boneTable = TableChecker.TableChecker.ReadTable(EditorUtility.OpenFilePanel("�򿪱���", "", "txt"), true);

            Dictionary<string, List<Bone>> dic = new Dictionary<string, List<Bone>>();

            for (int i = 0; i < boneTable.Count; i++) { 
                var bone = boneTable[i];
                if (!dic.ContainsKey(bone["Prefab"]))
                    dic[bone["Prefab"]] = new List<Bone>();

                dic[bone["Prefab"]].Add(new Bone { Value = bone["Value"], Key = bone["Key"]});
            }
            HashSet<string> set = new HashSet<string>();

            for (int i = 0; i < table.Count; i++) {
                var row = table[i];
                if (set.Contains(row["Prefab"].ToLower()))
                    continue;
                set.Add(row["Prefab"].ToLower());
                string path = $"Assets/BundleRes/Prefabs/{row["Prefab"]}.prefab";
                GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (gameObject == null)
                    continue;
                var rootTransform = gameObject.transform.Find("root");
                if (rootTransform == null || rootTransform.parent == null)
                    continue;
                rootTransform = rootTransform.parent;

                HashSet<string> keys = new HashSet<string>();
                HashSet<string> values = new HashSet<string>();


                if (dic.ContainsKey(row["Prefab"])) {
                    var list2 = dic[row["Prefab"]];
                    for (int j = 0; j < list2.Count; j++) {
                        if (keys.Contains(list2[j].Key) || values.Contains(list2[j].Value))
                            continue;
                        var child = rootTransform.Find(list2[j].Value);
                        values.Add(list2[j].Value);
                        if (child == null) {
                            results.Add($"�����Ҳ���\t{row["Prefab"]}\t{list2[j].Value}");
                        } else {
                            keys.Add(list2[j].Key);
                            results.Add($"����\t{row["Prefab"]}\t{list2[j].Value}");
                        }
                    }
                }
                List<string> result1 = new List<string>();
                List<string> result2 = new List<string>();

                List<string> found1 = new List<string>();
                List<string> found2 = new List<string>();

                var list = dic["Default1"];
                for (int j = 0; j < list.Count; j++) {
                    if (keys.Contains(list[j].Key) || values.Contains(list[j].Value))
                        continue;
                    var child = rootTransform.Find(list[j].Value);
                    values.Add(list[j].Value);
                    if (child == null) {
                        result1.Add($"Ĭ��1�Ҳ���\t{row["Prefab"]}\t{list[j].Value}");
                    } else {
                        found1.Add($"Ĭ��1\t{row["Prefab"]}\t{list[j].Value}");
                    }
                }

                list = dic["Default2"];
                for (int j = 0; j < list.Count; j++) {
                    if (keys.Contains(list[j].Key) || values.Contains(list[j].Value))
                        continue;
                    var child = rootTransform.Find(list[j].Value);
                    values.Add(list[j].Value);
                    if (child == null) {
                        result2.Add($"Ĭ��2�Ҳ���\t{row["Prefab"]}\t{list[j].Value}");
                    } else {
                        found2.Add($"Ĭ��2\t{row["Prefab"]}\t{list[j].Value}");
                    }
                }

                if (result1.Count <= result2.Count) {
                    results.AddRange(found1);
                    results.AddRange(result1);
                } else {
                    results.AddRange(found2);
                    results.AddRange(result2);
                }
            }
            listView.itemsSource = results;
            listView.Rebuild();
        }
    }
}