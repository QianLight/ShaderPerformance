using UnityEditor;

using System;
using System.IO;
using System.Collections.Generic;

using V1 = AssetBundleGraph;
using Model = UnityEngine.AssetGraph.DataModel.Version2;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace UnityEngine.AssetGraph
{

    [CustomNode("Export/Export To Directory", 100)]
    public class Exporter : Node, Model.NodeDataImporter
    {

        public enum ExportOption : int
        {
            ErrorIfNoExportDirectoryFound,
            AutomaticallyCreateIfNoExportDirectoryFound,
            DeleteAndRecreateExportDirectory
        }

        [SerializeField] private SerializableMultiTargetString m_exportPath;
        [SerializeField] private SerializableMultiTargetInt m_exportOption;
        [SerializeField] private SerializableMultiTargetInt m_flattenDir;
        [SerializeField] private bool m_creatMD5XML = true;

        public override string ActiveStyle
        {
            get
            {
                return "node 0 on";
            }
        }

        public override string InactiveStyle
        {
            get
            {
                return "node 0";
            }
        }

        public override string Category
        {
            get
            {
                return "Export";
            }
        }

        public override Model.NodeOutputSemantics NodeInputType
        {
            get
            {
                return
                    (Model.NodeOutputSemantics)
                    ((uint)Model.NodeOutputSemantics.Assets |
                     (uint)Model.NodeOutputSemantics.AssetBundles);
            }
        }

        public override Model.NodeOutputSemantics NodeOutputType
        {
            get
            {
                return Model.NodeOutputSemantics.None;
            }
        }

        public override void Initialize(Model.NodeData data)
        {
            //Take care of this with Initialize(NodeData)
            m_exportPath = new SerializableMultiTargetString();
            m_exportOption = new SerializableMultiTargetInt();
            m_flattenDir = new SerializableMultiTargetInt();

            data.AddDefaultInputPoint();
        }

        public void Import(V1.NodeData v1, Model.NodeData v2)
        {
            m_exportPath = new SerializableMultiTargetString(v1.ExporterExportPath);
            m_exportOption = new SerializableMultiTargetInt(v1.ExporterExportOption);
            m_flattenDir = new SerializableMultiTargetInt();
        }

        public override Node Clone(Model.NodeData newData)
        {
            var newNode = new Exporter();
            newNode.m_exportPath = new SerializableMultiTargetString(m_exportPath);
            newNode.m_exportOption = new SerializableMultiTargetInt(m_exportOption);
            newNode.m_flattenDir = new SerializableMultiTargetInt(m_flattenDir);

            newData.AddDefaultInputPoint();

            return newNode;
        }

        public override void OnInspectorGUI(NodeGUI node, AssetReferenceStreamManager streamManager, NodeGUIEditor editor, Action onValueChanged)
        {

            if (m_exportPath == null)
            {
                return;
            }

            var currentEditingGroup = editor.CurrentEditingGroup;

            EditorGUILayout.HelpBox("Export To Directory: Export given files to output directory.", MessageType.Info);
            editor.UpdateNodeName(node);

            GUILayout.Space(10f);

            //Show target configuration tab
            editor.DrawPlatformSelector(node);
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                var disabledScope = editor.DrawOverrideTargetToggle(node, m_exportPath.ContainsValueOf(currentEditingGroup), (bool enabled) =>
                {
                    using (new RecordUndoScope("Remove Target Export Settings", node, true))
                    {
                        if (enabled)
                        {
                            m_exportPath[currentEditingGroup] = m_exportPath.DefaultValue;
                            m_exportOption[currentEditingGroup] = m_exportOption.DefaultValue;
                            m_flattenDir[currentEditingGroup] = m_flattenDir.DefaultValue;
                        }
                        else
                        {
                            m_exportPath.Remove(currentEditingGroup);
                            m_exportOption.Remove(currentEditingGroup);
                            m_flattenDir.Remove(currentEditingGroup);
                        }
                        onValueChanged();
                    }
                });

                using (disabledScope)
                {
                    ExportOption opt = (ExportOption)m_exportOption[currentEditingGroup];
                    var newOption = (ExportOption)EditorGUILayout.EnumPopup("Export Option", opt);
                    if (newOption != opt)
                    {
                        using (new RecordUndoScope("Change Export Option", node, true))
                        {
                            m_exportOption[currentEditingGroup] = (int)newOption;
                            onValueChanged();
                        }
                    }

                    EditorGUILayout.LabelField("Export Path:");

                    string newExportPath = null;

                    newExportPath = editor.DrawFolderSelector("", "Select Export Folder",
                        m_exportPath[currentEditingGroup],
                        GetExportPath(m_exportPath[currentEditingGroup]),
                        (string folderSelected) =>
                        {
                            var projectPath = Directory.GetParent(Application.dataPath).ToString();

                            if (projectPath == folderSelected)
                            {
                                folderSelected = string.Empty;
                            }
                            else
                            {
                                var index = folderSelected.IndexOf(projectPath);
                                if (index >= 0)
                                {
                                    folderSelected = folderSelected.Substring(projectPath.Length + index);
                                    if (folderSelected.IndexOf('/') == 0)
                                    {
                                        folderSelected = folderSelected.Substring(1);
                                    }
                                }
                            }
                            return folderSelected;
                        }
                    );
                    if (newExportPath != m_exportPath[currentEditingGroup])
                    {
                        using (new RecordUndoScope("Change Export Path", node, true))
                        {
                            m_exportPath[currentEditingGroup] = newExportPath;
                            onValueChanged();
                        }
                    }

                    int flat = m_flattenDir[currentEditingGroup];
                    var newFlat = EditorGUILayout.ToggleLeft("Flatten Directory", flat == 1) ? 1 : 0;
                    if (newFlat != flat)
                    {
                        using (new RecordUndoScope("Change Flatten Directory", node, true))
                        {
                            m_flattenDir[currentEditingGroup] = newFlat;
                            onValueChanged();
                        }
                    }

                    var exporterNodePath = GetExportPath(newExportPath);
                    if (ValidateExportPath(
                        newExportPath,
                        exporterNodePath,
                        () =>
                        {
                        },
                        () =>
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUILayout.LabelField(exporterNodePath + " does not exist.");
                                if (GUILayout.Button("Create directory"))
                                {
                                    Directory.CreateDirectory(exporterNodePath);
                                }
                                onValueChanged();
                            }
                            EditorGUILayout.Space();

                            string parentDir = Path.GetDirectoryName(exporterNodePath);
                            if (Directory.Exists(parentDir))
                            {
                                EditorGUILayout.LabelField("Available Directories:");
                                string[] dirs = Directory.GetDirectories(parentDir);
                                foreach (string s in dirs)
                                {
                                    EditorGUILayout.LabelField(s);
                                }
                            }
                        },
                        () => {
                            throw new NodeException(
                                "Directory path contains '\\': " + newExportPath,
                                "It is not supported on Mac.");
                        }
                    ))
                    {
                        GUILayout.Space(10f);

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button(GUIHelper.RevealInFinderLabel))
                            {
                                EditorUtility.RevealInFinder(exporterNodePath);
                            }
                        }
                    }
                }
            }
            m_creatMD5XML = EditorGUILayout.ToggleLeft("生成与上次打包差异对比", m_creatMD5XML);
        }

        public override void Prepare(BuildTarget target,
            Model.NodeData node,
            IEnumerable<PerformGraph.AssetGroups> incoming,
            IEnumerable<Model.ConnectionData> connectionsToOutput,
            PerformGraph.Output Output)
        {
            ValidateExportPath(
                m_exportPath[target],
                GetExportPath(m_exportPath[target]),
                () =>
                {
                    throw new NodeException("Export Path is empty.", "Set valid export path from inspector.", node);
                },
                () =>
                {
                    if (m_exportOption[target] == (int)ExportOption.ErrorIfNoExportDirectoryFound)
                    {
                        throw new NodeException("Directory set to Export Path does not exist. Path:" + m_exportPath[target],
                            "Create exporting directory or set valid export path from inspector.", node);
                    }
                },
                () => {
                    throw new NodeException(
                        "Directory path contains '\\': " + m_exportPath[target],
                        "This is not supported on Mac.", node);
                }
            );
        }

        public override void Build(BuildTarget target,
            Model.NodeData node,
            IEnumerable<PerformGraph.AssetGroups> incoming,
            IEnumerable<Model.ConnectionData> connectionsToOutput,
            PerformGraph.Output Output,
            Action<Model.NodeData, string, float> progressFunc)
        {
            System.DateTime time0 = System.DateTime.Now;
            Export(target, node, incoming, connectionsToOutput, progressFunc);
            var span = System.DateTime.Now - time0;
            Debug.Log("Export file spent : " + span.ToString());
        }

        private void Export(BuildTarget target,
            Model.NodeData node,
            IEnumerable<PerformGraph.AssetGroups> incoming,
            IEnumerable<Model.ConnectionData> connectionsToOutput,
            Action<Model.NodeData, string, float> progressFunc)
        {
            if (incoming == null)
            {
                return;
            }

            var exportPath = GetExportPath(m_exportPath[target]);

            if (m_exportOption[target] == (int)ExportOption.DeleteAndRecreateExportDirectory)
            {
                if (Directory.Exists(exportPath))
                {
                    FileUtility.DeleteDirectory(exportPath, true);
                }
            }

            if (m_exportOption[target] != (int)ExportOption.ErrorIfNoExportDirectoryFound)
            {
                if (!Directory.Exists(exportPath))
                {
                    Directory.CreateDirectory(exportPath);
                }
            }
            else
            {
                string folder = BuildTargetUtility.TargetToAssetBundlePlatformName(target);
                string targetDir = Path.Combine(exportPath, folder);
                if (Directory.Exists(targetDir))
                {
                    BackupCopyBundle(targetDir);
                }
            }

            var report = new ExportReport(node);

            var _tempCachePath = Model.Settings.Path.BundleBuilderCachePath.Replace(@"\", @"/");
            var cacheFolderDepth = _tempCachePath.Split(Model.Settings.UNITY_FOLDER_SEPARATOR).Length + 1;
            int fileIndex = 0;
            int fileCount = incoming.Count();
            foreach (var ag in incoming)
            {
                foreach (var groupKey in ag.assetGroups.Keys)
                {
                    var inputSources = ag.assetGroups[groupKey];

                    foreach (var source in inputSources)
                    {
                        var destinationSourcePath = source.importFrom;

                        string destination = null;
                        if (m_flattenDir[target] == 0)
                        {
                            // in bundleBulider, use platform-package folder for export destination.
                            var tempCachePath = Model.Settings.Path.BundleBuilderCachePath.Replace(@"\", @"/");
                            if (destinationSourcePath.StartsWith(tempCachePath))
                            {

                                var splitted = destinationSourcePath.Split(Model.Settings.UNITY_FOLDER_SEPARATOR);
                                var reducedArray = new string[splitted.Length - cacheFolderDepth];

                                Array.Copy(splitted, cacheFolderDepth, reducedArray, 0, reducedArray.Length);
                                var fromDepthToEnd = string.Join(Model.Settings.UNITY_FOLDER_SEPARATOR.ToString(), reducedArray);

                                destinationSourcePath = fromDepthToEnd;
                            }
                            destination = FileUtility.PathCombine(exportPath, destinationSourcePath);
                        }
                        else
                        {
                            destination = FileUtility.PathCombine(exportPath, source.fileNameAndExtension);
                        }

                        var parentDir = Directory.GetParent(destination).ToString();

                        if (!Directory.Exists(parentDir))
                        {
                            Directory.CreateDirectory(parentDir);
                        }
                        if (File.Exists(destination))
                        {
                            File.Delete(destination);
                        }
                        if (string.IsNullOrEmpty(source.importFrom))
                        {
                            report.AddErrorEntry(source.absolutePath, destination, "Source Asset import path is empty; given asset is not imported by Unity.");
                            continue;
                        }
                        try
                        {
                            if (progressFunc != null) progressFunc(node, $"Copying {fileIndex}/{fileCount}:{source.fileNameAndExtension}", 0.5f);
                            FileTool.TryHardLinkCopy(source.importFrom, destination);
                            report.AddExportedEntry(source.importFrom, destination);
                            fileIndex++;
                        }
                        catch (Exception e)
                        {
                            report.AddErrorEntry(source.importFrom, destination, e.Message);
                        }

                        source.exportTo = destination;
                    }
                }
            }
            var export = GetExportPath(m_exportPath[target]);
            string fold = BuildTargetUtility.TargetToAssetBundlePlatformName(target);
            string targetPath = Path.Combine(export, fold).Replace(@"\", @"/");
            if (m_creatMD5XML)
            {
                OutputMD5File(targetPath);
                BackupDiffer(targetPath);
            }
            AssetBundleBuildReport.AddExportReport(report);
        }

	private const int BundleBackupGeneration = 1;
        private void BackupDiffer(string newPath)
        {
            StringBuilder differenceInfo = new StringBuilder();
            var oldPath = GetLastBackupPath(newPath);
            if (!Directory.Exists(oldPath))
                return;
            //获取assetMapName文件
            var assetMap = new Dictionary<string, string>();
            assetMap = GetAssetMap(oldPath, assetMap);
            assetMap = GetAssetMap(newPath,assetMap);
            //新旧文件都有md5文件则比较md5,否则比较文件
            var oldXmlPath = oldPath + "/MD5Version.xml";
            var newXmlPath = newPath + "/MD5Version.xml";
            if (File.Exists(oldXmlPath) && File.Exists(newXmlPath))
            {
                var oldXmlDictionary = DecodeMD5Version(oldXmlPath);
                var newXmlDictionary = DecodeMD5Version(newXmlPath);
                foreach (var oldXmlNode in oldXmlDictionary)
                {
                    MD5FileInfo newXmlInfo;
                    if (newXmlDictionary.TryGetValue(oldXmlNode.Key, out newXmlInfo))
                    {
                        if (oldXmlNode.Value.MD5 != newXmlInfo.MD5)
                        {
                            string asset;
                            assetMap.TryGetValue(newXmlInfo.Name, out asset);
                            differenceInfo.Append($"修改了 {newXmlInfo.Name}   {asset}\n");
                        }
                        newXmlDictionary.Remove(newXmlInfo.Name);
                    }
                    else
                    {
                        string asset;
                        assetMap.TryGetValue(oldXmlNode.Value.Name, out asset);
                        differenceInfo.Append($"删除了 {oldXmlNode.Value.Name}   {asset}\n");
                    }
                }
                foreach (var newXmlNode in newXmlDictionary)
                {
                    string asset;
                    assetMap.TryGetValue(newXmlNode.Key, out asset);
                    differenceInfo.Append($"添加了 {newXmlNode.Key}   {asset}\n");
                }
            }
            else
            {
                FileInfo[] filesOld = new DirectoryInfo(oldPath).GetFiles("*");
                FileInfo[] filesNew = new DirectoryInfo(newPath).GetFiles("*");
                Array.Sort(filesOld, (x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
                Array.Sort(filesNew, (x, y) => string.Compare(x.Name, y.Name, StringComparison.Ordinal));
                
                for (int i = 0, j = 0; i < filesOld.Length && j < filesNew.Length;)
                {
                    var oldName = filesOld[i].Name;
                    var newName = filesNew[j].Name;
                    var oldLength = filesOld[i].Length;
                    var newLength = filesNew[j].Length;

                    if (oldName == newName)
                    {
                        if (oldLength != newLength)
                            differenceInfo.Append($"修改了 {newName}\n");
                        i++;
                        j++;
                    }
                    else
                    {
                        if (string.CompareOrdinal(oldName, newName) < 0)
                        {
                            differenceInfo.Append($"删除了 {oldName}\n");
                            i++;
                        }
                        else
                        {
                            differenceInfo.Append($"添加了 {newName}\n");
                            j++;
                        }
                    }
                }
            }
            var outputPath = newPath.Substring(0,newPath.LastIndexOf("/"));
            var stream = File.Open(outputPath + "/difference.txt",FileMode.Create,FileAccess.Write);
            var streamWriter = new StreamWriter(stream);
            streamWriter.Write(differenceInfo.ToString());
            streamWriter.Flush();
            stream.Close();
        }
        private void BackupCopyBundle(string sourcePath)
        {
            try
            {
                string suffix = "_Backup";
                string backupPath = sourcePath + suffix;
                if (Directory.Exists(backupPath))
                {
                    Directory.Delete(backupPath, true);
                }
                if (Directory.Exists(sourcePath))
                {
                    Directory.Move(sourcePath, backupPath);
                }
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        private string GetExportPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return Directory.GetParent(Application.dataPath).ToString();
            }
            else if (path[0] == '/')
            {
                return path;
            }
            else
            {
                return FileUtility.GetPathWithProjectPath(path);
            }
        }

        public static bool ValidateExportPath(string currentExportFilePath, string combinedPath, Action NullOrEmpty, Action DoesNotExist, Action Invalid)
        {
            if (string.IsNullOrEmpty(currentExportFilePath))
            {
                NullOrEmpty();
                return false;
            }
#if UNITY_EDITOR_OSX
            if (currentExportFilePath.Contains("\\"))
            {
                Invalid();
                return false;
            }
#endif
            if (!Directory.Exists(combinedPath))
            {
                DoesNotExist();
                return false;
            }
            return true;
        }

        private Dictionary<string, string> GetAssetMap(string path, Dictionary<string, string> dic)
        {
            var realPath = path + "/assetMapName.xml";
            if (!File.Exists(realPath))
                return dic;
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(realPath);
            XmlNode rootNode = xmlDoc.SelectSingleNode("Assets");
            if (rootNode == null)
                return dic;
            XmlNodeList nodeList = rootNode.ChildNodes;
            foreach (XmlNode assetNode in nodeList)
            {
                dic[assetNode.FirstChild.InnerText] = (assetNode as XmlElement)?.GetAttribute("path");
            }

            return dic;
        }

        private string GetLastBackupPath(string newPath)
        {
            var generation = 1;
            string oldPath = newPath + "_Backup_" + generation;
            while (Directory.Exists(oldPath))
            {
                oldPath = newPath + "_Backup_" + ++generation;
            }

            oldPath = newPath + "_Backup_" + --generation;
            if (generation == 0)
                return null;
            return oldPath;
        }


        #region Copy from Zeus.MD5Util
        public static Dictionary<string, MD5FileInfo> DecodeMD5Version(string versionFile)
        {
            var stream = File.Open(versionFile, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            try
            {
                Dictionary<string, MD5FileInfo> md5FileInfos = new Dictionary<string, MD5FileInfo>();

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(stream);

                XmlNode rootNode = xmlDoc.SelectSingleNode("MD5Files");

                foreach (XmlElement fileElement in rootNode.ChildNodes)
                {
                    MD5FileInfo fileInfo = new MD5FileInfo();
                    fileInfo.Name = fileElement.GetAttribute("name");
                    fileInfo.MD5 = fileElement.GetAttribute("md5").ToLower();
                    fileInfo.Size = long.Parse(fileElement.GetAttribute("size"));
                    md5FileInfos.Add(fileInfo.Name, fileInfo);
                }

                xmlDoc = null;
                return md5FileInfos;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                stream.Close();
            }
        }
        public class MD5FileInfo
        {
            public string Name { set; get; }
            public string MD5 { set; get; }
            public long Size { set; get; }
        }
        #endregion

        private void OutputMD5File(string path)
        {
            Dictionary<string, List<string>> dicFileMD5 = new Dictionary<string, List<string>>();// <fileName, [md5,size]>
            MD5CryptoServiceProvider md5GenGenerator = new MD5CryptoServiceProvider();
            DirectoryInfo info = Directory.CreateDirectory(path);
            foreach(FileInfo fileInfo in info.GetFiles())
            {
                FileStream file = fileInfo.OpenRead();
                byte[] hash = md5GenGenerator.ComputeHash(file);
                List<string> tempList = new List<string>();
                string strMD5 = System.BitConverter.ToString(hash).Replace("-","");
                string size = file.Length.ToString();
                tempList.Add(strMD5);
                tempList.Add(size);
                file.Close();
                if (!dicFileMD5.ContainsKey(fileInfo.Name))
                    dicFileMD5.Add(fileInfo.Name, tempList);
                // else
                //     Debug.LogWarning("<Two File has the same name> name = " + fileInfo.Name);
            }
            XmlDocument xmlDoc = new XmlDocument();
            XmlElement xmlRoot = xmlDoc.CreateElement("MD5Files");
            xmlDoc.AppendChild(xmlRoot);
            foreach (KeyValuePair<string, List<string>> pair in dicFileMD5)
            {
                XmlElement xmlElem = xmlDoc.CreateElement("File");
                xmlRoot.AppendChild(xmlElem);

                xmlElem.SetAttribute("name", pair.Key);
                xmlElem.SetAttribute("md5", pair.Value[0]);
                xmlElem.SetAttribute("size", pair.Value[1]);
            }
            xmlDoc.Save(path + "/"+"MD5Version.xml");
            AssetDatabase.Refresh();
        }
    }
}