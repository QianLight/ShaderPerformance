using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace CFEngine.Editor
{
    public class ScaneInstance
    {
        public string name;
        public string ext;
        public string path;
        public ScanPolicy policy;
    }

    public partial class ResScanTool : BaseConfigTool<ResScanConfig>
    {
        enum OpScanType
        {
            None,
            Scan,
            Save,
            Load
        }
        private Vector2 jobScroll = Vector2.zero;

        private List<ScanPolicy> scanTypes = new List<ScanPolicy>();
        private string[] resTypeNames;

        public OrderResList result = new OrderResList();
        private OpScanType opScanType = OpScanType.None;
        private int scanIndex = -1;
        private Queue<ScaneInstance> scaneInstances = new Queue<ScaneInstance> ();
        private int scanTotalCount = 0;
        private int scanCount = 0;
        private int calcSizeIndex0 = -1;
        private int calcSizeIndex1 = -1;
        private Vector2 resultScroll = Vector2.zero;
        private List<ResItem> scanRoot = new List<ResItem>();
        private string search = "";
        private string searchLow = "";
        private ESortType sortType = ESortType.ResType;
        private List<string> scanResultPath = new List<string>();
        private Vector2 scanResScroll = Vector2.zero;
        private string scaneResultPath = "";
        public override void OnInit ()
        {
            base.OnInit ();
            config = ResScanConfig.instance;
            var resNames = new List<string>();
            scanTypes.Clear();
            var types = EngineUtility.GetAssemblyType (typeof (ScanPolicy));
            foreach (var t in types)
            {
                var process = Activator.CreateInstance (t) as ScanPolicy;
                if (process != null)
                {
                    scanTypes.Add(process);
                    resNames.Add(process.ScanType);
                }
            }
            resTypeNames = resNames.ToArray();

            LoadResultList();
        }
        public override void OnUninit()
        {
            base.OnUninit();
            EditorUtility.ClearProgressBar();
        }
        protected override void OnSave () { }

        private void LoadResultList()
        {
            DirectoryInfo di = new DirectoryInfo("Assets/../Dump");
            if (di.Exists)
            {
                scaneResultPath = di.FullName;
                var files = di.GetFiles("*.xml", SearchOption.TopDirectoryOnly);
                scanResultPath.Clear();
                for (int i = 0; i < files.Length; ++i)
                {
                    scanResultPath.Add(files[i].Name);
                }
            }
        }
        protected override void OnConfigGui (ref Rect rect)
        {
            //if (config.folder.FolderGroup ("ScanTypes", "ScanTypes", rect.width))
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("ScanAll", GUILayout.MaxWidth(80)))
                {
                    opScanType = OpScanType.Scan;
                    scanIndex = -1;
                }
                if (GUILayout.Button("Save", GUILayout.MaxWidth(80)))
                {
                    opScanType = OpScanType.Save;
                }
                //if (GUILayout.Button("Load", GUILayout.MaxWidth(80)))
                //{
                //    opScanType = OpScanType.Load;
                //}
                EditorGUILayout.EndHorizontal();


                for (int i = 0; i < scanTypes.Count; ++i)
                {
                    var st = scanTypes[i];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(st.ScanType);
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.Space();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Add", GUILayout.MaxWidth(80)))
                {
                    config.jobs.Add(new ScanJob());
                }
                EditorGUILayout.EndHorizontal();

                DeleteInfo deletejob = new DeleteInfo();
                deletejob.BeginDelete();
                EditorCommon.BeginScroll(ref jobScroll, config.jobs.Count, 10, -1, rect.width - 20);
                for (int i = 0; i < config.jobs.Count; ++i)
                {
                    var job = config.jobs[i];
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(string.Format("{0}.", i.ToString()), GUILayout.MaxWidth(40));
                    ToolsUtility.FolderSelect(ref job.folder);
                    job.scanType = EditorGUILayout.Popup(job.scanType, resTypeNames, GUILayout.MaxWidth(160));
                    if (GUILayout.Button("Scan", GUILayout.MaxWidth(80)))
                    {
                        opScanType = OpScanType.Scan;
                        scanIndex = i;
                    }
                    deletejob.RemveButton(i);
                    EditorGUILayout.EndHorizontal();
                }
                EditorCommon.EndScroll ();
                deletejob.EndDelete(config.jobs);
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Clear", GUILayout.MaxWidth(80)))
                {
                    for (int i = 0; i < scanResultPath.Count; ++i)
                    {
                        string path = scanResultPath[i];
                        path = string.Format("{0}/{1}", scaneResultPath, path);
                        File.Delete(path);
                    }
                    scanResultPath.Clear();
                }
                EditorGUILayout.EndHorizontal();
                deletejob.BeginDelete();
                EditorCommon.BeginScroll(ref scanResScroll, scanResultPath.Count, 10, -1, rect.width - 20);
                for (int i = 0; i < scanResultPath.Count; ++i)
                {
                    var resultPath = scanResultPath[i];
                    EditorGUILayout.BeginHorizontal();

                    EditorGUILayout.LabelField(resultPath, GUILayout.MaxWidth(300));
                    if (GUILayout.Button("Load", GUILayout.MaxWidth(80)))
                    {
                        opScanType = OpScanType.Load;
                        scanIndex = i;
                    }
                    deletejob.RemveButton(i);
                    EditorGUILayout.EndHorizontal();
                }
                EditorCommon.EndScroll();
                if (deletejob.removeIndex >= 0)
                {
                    string path = scanResultPath[deletejob.removeIndex];
                    path = string.Format("{0}/{1}", scaneResultPath, path);
                    File.Delete(path);
                }
                deletejob.EndDelete(scanResultPath);
       
                EditorGUILayout.Space();
                if(scanRoot.Count>0)
                {
                    GUILayout.BeginHorizontal("HelpBox");
                    GUILayout.Space(30);
                    EditorGUI.BeginChangeCheck();
                    search = EditorGUILayout.TextField("", search, "SearchTextField", GUILayout.MaxWidth(300));
                    if (EditorGUI.EndChangeCheck())
                    {
                        searchLow = search;
                    }
                    GUILayout.Label("", "SearchCancelButtonEmpty");
                    GUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(
                        string.Format("Total Size:{0} ({1} Bytes)", EditorUtility.FormatBytes(result.size), result.size.ToString()), 
                        GUILayout.MaxWidth(400));
                    EditorGUI.BeginChangeCheck();
                    sortType = (ESortType)EditorGUILayout.EnumPopup("SortType", sortType, GUILayout.MaxWidth(300));
                    if (EditorGUI.EndChangeCheck())
                    {
                        ResItem.Sort(scanRoot, sortType);
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorCommon.BeginScroll(ref resultScroll, scanRoot.Count, 30, -1, rect.width - 20);
                for (int i = 0; i < scanRoot.Count; ++i)
                {
                    var root = scanRoot[i];
                    if (root.nameWithExtLow.Contains(searchLow))
                    {
                        EditorGUILayout.BeginHorizontal();

                        EditorGUILayout.LabelField(string.Format("{0}.{1} - {2}", i.ToString(), root.nameWithExt, root.stateStr), GUILayout.MaxWidth(rect.width - 120));
                        if (GUILayout.Button("View", GUILayout.MaxWidth(80)))
                        {
                            ResWindow.Open(root);
                        }
                        EditorGUILayout.EndHorizontal();
                    }
    
                }
                EditorCommon.EndScroll();
            }

        }

        protected override void OnConfigUpdate ()
        {
            switch (opScanType)
            {
                case OpScanType.Scan:
                    {
                        ScanJobCollect (scanIndex);
                        scanRoot.Clear();
                        scanIndex = -1;
                    }
                    break;
                case OpScanType.Save:
                    {
                        result.Save();
                        LoadResultList();
                    }
                    break;
                case OpScanType.Load:
                    {
                        string path = scanResultPath[scanIndex];
                        path = string.Format("{0}/{1}", scaneResultPath, path);
                        result.Load(path, scanRoot);
                        calcSizeIndex0 = 0;
                        calcSizeIndex1 = 0;
                        result.BeginCalcSize();
                    }
                    break;
            }
            opScanType = OpScanType.None;
            if (scaneInstances.Count > 0)
            {
                int processCount = 20;
                do
                {
                    var si = scaneInstances.Dequeue();
                    scanCount++;
                    EditorUtility.DisplayProgressBar(string.Format("Scan Count:{0} Total:{1}", scanCount, scanTotalCount), si.name, (float)scanCount / scanTotalCount);
                    var item = si.policy.Scan(si.name, si.path, result, config);
                    if (item != null)
                        scanRoot.Add(item);
                    processCount--;
                }
                while (scaneInstances.Count > 0 && processCount > 0);
                if (scaneInstances.Count == 0)
                {
                    calcSizeIndex0 = 0;
                    calcSizeIndex1 = 0;
                    result.BeginCalcSize();
                    EditorUtility.ClearProgressBar ();
                }
            }
            int resCount = result.res.Count;
            if (calcSizeIndex0 >= 0 && calcSizeIndex0 < resCount)
            {
                var ri = result.CalcSize0(calcSizeIndex0, config);
                EditorUtility.DisplayProgressBar(string.Format("Calc0 Index:{0} Total:{1}", calcSizeIndex0, resCount),
                    ri.nameWithExt, (float)calcSizeIndex0 / resCount);

                calcSizeIndex0++;
                if(calcSizeIndex0 == result.res.Count)
                {
                    calcSizeIndex0 = -1;
                    EditorUtility.ClearProgressBar();
                }
            }
            else if (calcSizeIndex1 >= 0 && calcSizeIndex1 < result.res.Count)
            {
                var ri = result.CalcSize1(calcSizeIndex1);
                EditorUtility.DisplayProgressBar(string.Format("Calc1 Index:{0} Total:{1}", calcSizeIndex1, resCount),
                    ri.nameWithExt, (float)calcSizeIndex1 / resCount);
                calcSizeIndex1++;
                if (calcSizeIndex1 == result.res.Count)
                {
                    result.OutputLog();
                    calcSizeIndex1 = -1;
                    EditorUtility.ClearProgressBar();
                }
            }
        }
        private void PrepareScan (ScanJob job)
        {
            int scanType = (int)job.scanType;
            if (scanType >= 0 && scanType < scanTypes.Count)
            {
                var policy = scanTypes[(int)job.scanType];
                if (policy != null)
                {
                    DirectoryInfo di = new DirectoryInfo(job.folder);
                    if (di.Exists)
                    {

                        var files = di.GetFiles(policy.ResExt, SearchOption.AllDirectories);
                        for (int i = 0; i < files.Length; ++i)
                        {
                            var file = files[i];
                            string fullName = file.FullName;
                            fullName = fullName.Replace('\\', '/');
                            int index = fullName.IndexOf("Assets/");
                            fullName = fullName.Substring(index);
                            ScaneInstance si = new ScaneInstance()
                            {
                                name = file.Name,
                                path = fullName,
                                policy = policy,
                            };
                            scaneInstances.Enqueue(si);
                            scanTotalCount++;
                        }
                    }
                }

            }

        }

        private void ScanJobCollect (int index)
        {
            result.Clear ();
            scanTotalCount = 0;
            scanCount = 0;
            if (index >= 0 && index < config.jobs.Count)
            {
                var job = config.jobs[index];
                PrepareScan (job);
            }
            else
            {
                for (int i = 0; i < config.jobs.Count; ++i)
                {
                    var job = config.jobs[i];
                    PrepareScan (job);
                }
            }
        }
    }
}