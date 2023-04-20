using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System.IO;
using System.Collections.Generic;
using EcsData;
using System.Threading.Tasks;

public class SkillEditorScannerSlotDown : EditorWindow
{
    [MenuItem("Tools/TDTools/废弃工具/技能slotdown扫描工具")]
    public static void ShowExample()
    {
        SkillEditorScannerSlotDown wnd = GetWindow<SkillEditorScannerSlotDown>();
        wnd.titleContent = new GUIContent("SkillEditorScannerSlotDown");
    }

    public void CreateGUI(){
        VisualElement root = rootVisualElement;

        Toolbar toolbar = new Toolbar();
        ToolbarButton scanButton = new ToolbarButton();
        scanButton.text = "扫描";
        toolbar.Add(scanButton);
        root.Add(toolbar);

        List<string> results = new List<string>();
        ListView resutsListView = new ListView();
        resutsListView.style.flexGrow = 100;
        resutsListView.makeItem = () => new Label();
        resutsListView.bindItem = (v, e) => (v as Label).text = results[e];
        resutsListView.itemsSource = results;
        resutsListView.itemHeight = 16;
        root.Add(resutsListView);

        scanButton.clicked += async () => {
            results.Clear();
            var task = Scan();
            await task;
            results = task.Result;
            resutsListView.itemsSource = results;
            resutsListView.Refresh();
        };

        ToolbarButton saveToFileButton = new ToolbarButton();
        toolbar.Add(saveToFileButton);
        saveToFileButton.text = "保存结果到txt";
        saveToFileButton.clicked += () => {
            if (results.Count == 0)
                return;
            string path = EditorUtility.SaveFilePanel("保存结果", "", "扫描结果", "txt");
            if (path == "")
                return;
            Task.Run(() => {
                StreamWriter sw = new StreamWriter(path);
                for (int i = 0; i < results.Count; i++) {
                    sw.WriteLine(results[i]);
                }
                sw.Close();
            });
        };
    }

    async Task<List<string>> Scan() {
        List<string> results = new List<string>();
        string path = $@"{Application.dataPath}\BundleRes\SkillPackage\";
        var skills = GetAllSkillFiles(new DirectoryInfo(path));
        int progressID = Progress.Start("扫描技能Slotdown");
        await Task.Run(()=>{
            for (int i = 0; i < skills.Count; i++) {
                Progress.Report(progressID, i / (float)skills.Count);
                if (ScanSkill(skills[i].FullName))
                    results.Add(skills[i].FullName);
            }
        });
        Progress.Remove(progressID);
        return results;
    }

    bool ScanSkill(string path) {
        int hashcode = (int)CFUtilPoolLib.XCommon.singleton.XHash("c_is_slot_down");

        if (!File.Exists(path))
            return false;

        var data = DataIO.DeserializeEcsData<XSkillData>(path);

        HashSet<int> _qteSet = new HashSet<int>();
        for (int i = 0; i < data.QTEData.Count; i++) {
            _qteSet.Add(data.QTEData[i].Index);
        }

        for (int i = 0; i < data.ConditionData.Count; i++) {
            if (data.ConditionData[i].FunctionHash == hashcode) {
                bool containBoth = true;
                for (int j = 0; j < data.ConditionData[i].TransferData.Count; j++) {
                    if (!_qteSet.Contains(data.ConditionData[i].TransferData[j].Index))
                        containBoth = false;
                }

                if (containBoth) {
                    return true;
                }
            }
            
        }

        return false;
    }

    List<FileInfo> GetAllSkillFiles(DirectoryInfo root) {
        List<FileInfo> results = new List<FileInfo>();
        results.AddRange(root.GetFiles("*.bytes"));
        var dirs = root.GetDirectories();
        for (int i = 0; i < dirs.Length; i++) {
            results.AddRange(GetAllSkillFiles(dirs[i]));
        }
        return results;
    }
}