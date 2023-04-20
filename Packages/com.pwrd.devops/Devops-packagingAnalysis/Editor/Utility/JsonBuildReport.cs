using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using UnityEngine;
using System.Reflection;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build.Reporting;
#endif

namespace Trident.JsonReport
{
    public static class BuildReportSettings
    {
        public const string LastBuildPath = "Library/LastBuild.buildreport";
        public static string GameProductName = "Game";
        public static string ResProductName = "Res";

        public static bool GenJsonReport = true;
        public static bool DetialAssetType = true;
        public static bool ProgressBar = false;
        public static bool AutoDelete = false;

        static string JsonReportName = "JsonReport";
        static string TimeFormat = "yyyyMMdd_HHmmss";

        static string BuildReportFormat = "Library/Build_{0}/BuildReport.buildreport";
        static string JsonFileFormat = "Library/Build_{0}/{1}.json";

        static string LastReportPath = "Assets/LastBuild.buildreport";
        static string LastJsonFileFormat = "Library/{0}.json";

        public static void InitializeFromCommandLine()
        {
            //  字段不多，就直接遍历好了
            var fields = typeof(BuildReportSettings).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic);
            var args = System.Environment.GetCommandLineArgs();
            foreach (var arg in args)
            {
                if (!arg.Contains("="))
                    continue;

                string[] strs = arg.Split('=');
                if (strs.Length != 2)
                    continue;

                foreach (var field in fields)
                {
                    if (field.IsInitOnly)
                        continue;

                    if (strs[0].Equals(field.Name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (field.FieldType == typeof(string))
                        {
                            field.SetValue(null, strs[1]);
                        }
                        else if (field.FieldType == typeof(bool))
                        {
                            if (bool.TryParse(strs[1], out var value))
                                field.SetValue(null, value);
                        }
                    }
                }
            }
        }

        public static bool IsGame()
        {
            return Environment.CurrentDirectory.EndsWith(GameProductName, System.StringComparison.CurrentCultureIgnoreCase);
        }
        public static bool IsRes()
        {
            return Environment.CurrentDirectory.EndsWith(ResProductName, System.StringComparison.CurrentCultureIgnoreCase);
        }

        public static string GetGameDirectory()
        {
            return GetDirectory(GameProductName);
        }

        public static string GetResDirectory()
        {
            return GetDirectory(ResProductName);
        }

        private static string GetDirectory(string name)
        {
            var dir = System.Environment.CurrentDirectory;
            if (dir.EndsWith(name, StringComparison.CurrentCultureIgnoreCase))
                return dir;

            var parentDir = Path.GetDirectoryName(dir);
            return Path.Combine(parentDir, name);
        }

        public static string GetBuildReportPath() { return LastReportPath; }
        public static string GetBuildReportPath(DateTime dateTime)
        {
            var timeString = dateTime.ToString(TimeFormat);
            return string.Format(BuildReportFormat, timeString);
        }
        public static string GetJsonReportPath()
        {
            return string.Format(LastJsonFileFormat, JsonReportName);
        }
        public static string GetJsonReportPath(DateTime dateTime)
        {
            var timeString = dateTime.ToString(TimeFormat);
            return string.Format(JsonFileFormat, timeString, JsonReportName);
        }

        public static string GetJsonFilePath(string fileName)
        {
            return string.Format(LastJsonFileFormat, fileName);
        }
        public static string GetJsonFilePath(string fileName, DateTime dateTime)
        {
            var timeString = dateTime.ToString(TimeFormat);
            return string.Format(JsonFileFormat, timeString, fileName);
        }

        public static string GetDateTime(DateTime dateTime) { return dateTime.ToString(TimeFormat); }
    }

    [System.Serializable]
    public class JsonBuildReport
    {
        private JsonBuildReport() { }
#if UNITY_EDITOR
        public JsonBuildReport(BuildReport report, bool detialAssetType, bool displayProgressBar)
        {
            projectName = Application.productName;
            platform = report.summary.platform.ToString();
            buildStartTime = report.summary.buildEndedAt.Ticks;
            long resBuildStartTime = buildStartTime;
            totalSize = report.summary.totalSize.ToString();
            buildResult = report.summary.result.ToString();
            totalErrors = report.summary.totalErrors;
            buildTimestamp = ReadAllText("Assets/Resources/BuildTimestamp");
            var dateTime = report.summary.buildEndedAt.AddHours(8);

            //  源文件，如 Assets/Resources/test.mat
            JsonSourceFileMap sourceFileMap = new JsonSourceFileMap(report, detialAssetType, displayProgressBar);
            //  输出文件，如 Bundle，Assembly，LuaFile之类的
            JsonOutputFileMap outputFileMap = new JsonOutputFileMap(report.files, sourceFileMap, displayProgressBar);
            //  打包步骤，如 Build Bundle, Build Player 之类的
            JsonBuildStepMap buildStepMap = new JsonBuildStepMap(report.steps, displayProgressBar);
            //  Save Json 文件
            WriteAllText(BuildReportSettings.GetJsonFilePath(nameof(JsonSourceFileMap), dateTime), JsonUtility.ToJson(sourceFileMap), dateTime);
            WriteAllText(BuildReportSettings.GetJsonFilePath(nameof(JsonOutputFileMap), dateTime), JsonUtility.ToJson(outputFileMap), dateTime);
            WriteAllText(BuildReportSettings.GetJsonFilePath(nameof(JsonBuildStepMap), dateTime), JsonUtility.ToJson(buildStepMap), dateTime);

            var resRoot = BuildReportSettings.GetResDirectory();
            bool hasResProject = Directory.Exists(resRoot);
            if (hasResProject)
            {
                var assetPath = AssetDatabase.GetAssetPath(report);
                var lastReportPath = BuildReportSettings.GetBuildReportPath();
                if (lastReportPath.Equals(assetPath, StringComparison.CurrentCultureIgnoreCase))
                {
                    var mergeSourcePath = BuildReportSettings.GetJsonFilePath(nameof(JsonSourceFileMap));
                    var mergeOutputPath = BuildReportSettings.GetJsonFilePath(nameof(JsonOutputFileMap));
                    var mergeStepPath = string.Empty;
                    bool checkDiscarded = true;
                    if (BuildReportSettings.IsGame())
                    {
                        //  如果存在则是双工程
                        if (hasResProject)
                        {
                            checkDiscarded = false;
                            //  如果有上一次的 Res JsonBuildReport.json，则应该从Res开始算时间。
                            var resReportPath = $"{resRoot}/{BuildReportSettings.GetJsonFilePath(nameof(JsonBuildReport))}";
                            if (File.Exists(resReportPath))
                            {
                                var json = File.ReadAllText(resReportPath);
                                var resReport = JsonUtility.FromJson<JsonBuildReport>(json);
                                resBuildStartTime = resReport.buildStartTime;
                            }

                            //  如果有res工程下对应的上一次的 step，则合起来。
                            mergeStepPath = $"{resRoot}/{BuildReportSettings.GetJsonFilePath(nameof(JsonBuildStepMap))}";
                            //  如果有res工程下对应的上一次的 source，则合起来。
                            mergeSourcePath = $"{resRoot}/{BuildReportSettings.GetJsonFilePath(nameof(JsonSourceFileMap))}";
                            //  如果有res工程下对应的上一次的 output，则合起来。
                            mergeOutputPath = $"{resRoot}/{BuildReportSettings.GetJsonFilePath(nameof(JsonOutputFileMap))}";
                        }
                    }

                    //  合并之前的SourceFile结果
                    if (File.Exists(mergeSourcePath))
                    {
                        var last = JsonUtility.FromJson<JsonSourceFileMap>(File.ReadAllText(mergeSourcePath));
                        last.Merge(sourceFileMap.GetEnumerator(), checkDiscarded);
                        sourceFileMap = last;
                        WriteAllText(BuildReportSettings.GetJsonFilePath(nameof(JsonSourceFileMap)), JsonUtility.ToJson(last), dateTime);
                    }
                    else
                    {
                        WriteAllText(BuildReportSettings.GetJsonFilePath(nameof(JsonSourceFileMap)), JsonUtility.ToJson(sourceFileMap), dateTime);
                    }
                    //  合并之前的OutputFile结果
                    if (File.Exists(mergeOutputPath))
                    {
                        var last = JsonUtility.FromJson<JsonOutputFileMap>(File.ReadAllText(mergeOutputPath));
                        last.Merge(outputFileMap.GetEnumerator(), checkDiscarded);
                        outputFileMap = last;
                        WriteAllText(BuildReportSettings.GetJsonFilePath(nameof(JsonOutputFileMap)), JsonUtility.ToJson(last), dateTime);
                    }
                    else
                    {
                        WriteAllText(BuildReportSettings.GetJsonFilePath(nameof(JsonOutputFileMap)), JsonUtility.ToJson(outputFileMap), dateTime);
                    }
                    //  合并之前的Step结果
                    if (File.Exists(mergeStepPath))
                    {
                        var last = JsonUtility.FromJson<JsonBuildStepMap>(File.ReadAllText(mergeStepPath));
                        buildStepMap.AddRange(last.GetEnumerator());
                        WriteAllText(BuildReportSettings.GetJsonFilePath(nameof(JsonBuildStepMap)), JsonUtility.ToJson(last), dateTime);
                    }
                    else
                    {
                        WriteAllText(BuildReportSettings.GetJsonFilePath(nameof(JsonBuildStepMap)), JsonUtility.ToJson(buildStepMap), dateTime);
                    }
                }
            }
            else
            {
                //  单工程先不考虑会在单工程中增量打包bundle的情况
                WriteAllText(BuildReportSettings.GetJsonFilePath(nameof(JsonSourceFileMap)), JsonUtility.ToJson(sourceFileMap), dateTime);
                WriteAllText(BuildReportSettings.GetJsonFilePath(nameof(JsonOutputFileMap)), JsonUtility.ToJson(outputFileMap), dateTime);
                WriteAllText(BuildReportSettings.GetJsonFilePath(nameof(JsonBuildStepMap)), JsonUtility.ToJson(buildStepMap), dateTime);
            }

            //  划分步骤
            build_step_info = new Build_step_info[buildStepMap.rootStepCount];
            int index = 0;
            foreach (var jstep in buildStepMap.GetEnumerator())
            {
                build_step_info[index++] = new Build_step_info(jstep);
            }

            //  划分资源类型
            Dictionary<string, (ulong size, HashSet<Build_asset_info> files)> assortedDict = new Dictionary<string, (ulong, HashSet<Build_asset_info>)>();
            foreach (var file in outputFileMap.GetEnumerator())
            {
                if (file.type == "PACKAGED FILE" || file.type == "AssetBundle")
                    continue;
                assortedDict.TryGetValue(file.type, out var tuple);
                tuple.size += file.size + file.manifestSize;
                if (tuple.files == null)
                    tuple.files = new HashSet<Build_asset_info>();
                bool find = false;
                JsonSourceFile source;
                //  Find by Path
                (find, source) = sourceFileMap.FindSourceFileByPath(file.filePath);
                if (find)
                {
                    tuple.files.Add(new Build_asset_info() { name = AssetNameNotNull(source.path), size = Math.Round(source.size / (double)1024, 1).ToString() });
                }
                else
                {
                    HashSet<JsonSourceFile> files;
                    //  Find by Name
                    (find, files) = sourceFileMap.FindSourceFileByName(file.fileName);
                    if (find)
                    {
                        foreach (var src in files)
                        {
                            tuple.files.Add(new Build_asset_info() { name = AssetNameNotNull(src.path), size = Math.Round(src.size / (double)1024, 1).ToString() });
                        }
                    }
                    else
                    {
                        //  Find by ShortPath
                        for (int i = 0; i < file.shortPathList.Count; i++)
                        {
                            var shortPath = file.shortPathList[i];
                            (find, files) = sourceFileMap.FindSourceFilesByShortPath(shortPath);
                            if (find)
                            {
                                foreach (var src in files)
                                {
                                    tuple.files.Add(new Build_asset_info() { name = AssetNameNotNull(src.path), size = Math.Round(src.size / (double)1024, 1).ToString() });
                                }
                            }
                        }
                    }
                }
                if (!find)
                {
                    tuple.files.Add(new Build_asset_info() { name = "[NonSrc] " + file.absPath, size = Math.Round(file.size / (double)1024, 1).ToString() });
                }
                assortedDict[file.type] = tuple;
            }

            build_assettype_info = new Build_assettype_info[assortedDict.Count];
            index = 0;
            foreach (var item in assortedDict)
            {
                build_assettype_info[index++] = new Build_assettype_info()
                {
                    build_assettype_name = item.Key,
                    build_assettype_size = Math.Round(item.Value.size / (double)1024, 1).ToString(),
                    build_list = item.Value.files.ToArray()
                };
            }

            //  保存该JsonReport
            WriteAllText(BuildReportSettings.GetJsonReportPath(dateTime), JsonUtility.ToJson(this), dateTime);
            if (resBuildStartTime < buildStartTime)
            {
                //  如果有res的起始时间，则加上这段时间
                totalTime += (ulong)report.summary.totalTime.Add(TimeSpan.FromTicks(buildStartTime - resBuildStartTime)).TotalSeconds;
                buildStartTime = resBuildStartTime;
            }
            else
            {
                totalTime = report.summary.totalTime.TotalSeconds.ToString();
            }
            WriteAllText(BuildReportSettings.GetJsonReportPath(), JsonUtility.ToJson(this), dateTime);
            ProgressBar.Clear();
            if (BuildReportSettings.AutoDelete)
            {
                File.Delete(BuildReportSettings.GetBuildReportPath());
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void CheckCopyBuildReport(BuildReport report)
        {
            var dateTime = report.summary.buildEndedAt.AddHours(8);
            var reportPath = AssetDatabase.GetAssetPath(report);
            //  检查拷贝report
            var reportTargetPath = BuildReportSettings.GetBuildReportPath(dateTime);
            if (!File.Exists(reportTargetPath))
            {
                Copy(reportPath, reportTargetPath);
                SetFileDateTime(reportTargetPath, dateTime);
            }
        }

        [MenuItem("AssetCollect/UnitTest/TestBuildFromCommandLine")]
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void BuildFromCommandLine()
        {
            BuildReportSettings.InitializeFromCommandLine();
            var assetPath = BuildReportSettings.GetBuildReportPath();
            Copy(BuildReportSettings.LastBuildPath, assetPath);
            var report = AssetDatabase.LoadAssetAtPath<BuildReport>(assetPath);
            CheckCopyBuildReport(report);
            if (BuildReportSettings.GenJsonReport)
                new JsonBuildReport(report, BuildReportSettings.DetialAssetType, BuildReportSettings.ProgressBar);
            Selection.activeObject = report;
        }

        public static void OpenLastBuildReport()
        {
            var assetPath = BuildReportSettings.GetBuildReportPath();
            Copy(BuildReportSettings.LastBuildPath, assetPath);
            var report = AssetDatabase.LoadAssetAtPath<BuildReport>(assetPath);
            CheckCopyBuildReport(report);
            new JsonBuildReport(report, true, true);
            Selection.activeObject = report;
        }

        static void WriteAllText(string path, string content, DateTime dateTime)
        {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(path, content);
            var fileInfo = new FileInfo(path);
            fileInfo.CreationTime = fileInfo.LastWriteTime = fileInfo.LastAccessTime = dateTime;
        }

        static string ReadAllText(string path)
        {
            if (!File.Exists(path))
                return "";
            return File.ReadAllText(path);
        }

        static void Copy(string src, string dst)
        {
            var dir = Path.GetDirectoryName(dst);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.Copy(src, dst, true);
            AssetDatabase.Refresh();
        }

        static void SetFileDateTime(string path, DateTime dateTime)
        {
            var fileInfo = new FileInfo(path);
            fileInfo.CreationTime = fileInfo.LastWriteTime = fileInfo.LastAccessTime = dateTime;
        }

        static string STRNULL = "Null";
        static string AssetNameNotNull(string assetName)
        {
            if(assetName != null && assetName.Length > 0)
            {
                return assetName;
            }
            return STRNULL;
        }
#endif
        public string projectName;      //  工程名
        public string platform;         //  目标平台
        public long buildStartTime;     //  开始构建时间(Ticks)
        public string totalTime;         //  总耗时（s）
        public string totalSize;         //  总大小 (byte)
        public string buildResult;      //  构建结果
        public bool isSucceed;          //  是否构建成功
        public int totalErrors;         //  错误数
        public string buildTimestamp;   // 打包的时间戳 是Profiler工程收集的时间，为了他们有共同的ID
        public Build_step_info[] build_step_info;
        public Build_assettype_info[] build_assettype_info;


        [System.Serializable]
        public class Build_step_info
        {
            public string build_step_name;      //  步骤名
            public string build_step_duration;    //  持续时间(s)
            public Build_step_info[] build_sub_steps_list;  //  子步骤列表

            public Build_step_info(JsonBuildStep step)
            {
                build_step_name = step.stepName;
                build_step_duration = new TimeSpan(step.duration).TotalSeconds.ToString();
                var temp_list = new List<Build_step_info>();
                foreach (var child in step.children)
                {
                    temp_list.Add(new Build_step_info(child));
                }
                build_sub_steps_list = temp_list.ToArray();
            }
        }

        [System.Serializable]
        public class Build_assettype_info
        {
            public string build_assettype_name;     //  资源类型
            public string build_assettype_size;       //  资源大小(kb)
            public Build_asset_info[] build_list;   //  包含的源文件列表
        }

        [System.Serializable]
        public class Build_asset_info
        {
            public string name;
            public string size;
        }

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }

    [System.Serializable]
    public class JsonSourceFile
    {
        public string type;     //  源文件类型
        public string path;     //  源文件路径
        public ulong size;      //  源文件大小(byte)

        public List<string> shortPathList = new List<string>();   //  如：CAB-xxxxxxxx, archive:/BuildPlayer-xxxxx 等
    }

    /// <summary>
    /// 源文件映射，方便查询
    /// </summary>
    [System.Serializable]
    public class JsonSourceFileMap
    {
        public JsonSourceFileMap(JsonSourceFileMap map)
        {
            if (map != null)
            {
                sourceFiles.AddRange(map.sourceFiles);
            }
        }
#if UNITY_EDITOR
        public JsonSourceFileMap(BuildReport report, bool detialAssetType, bool displayProgressBar)
        {
            pathMap = GetSourceAssetsMap(report, detialAssetType, displayProgressBar);
            sourceFiles.AddRange(pathMap.Values);
            (nameMap, shortPathMap) = BuildShortPathMap(sourceFiles);
        }
#endif

        [SerializeField] private List<JsonSourceFile> sourceFiles = new List<JsonSourceFile>();

        private Dictionary<string, JsonSourceFile> pathMap;
        private Dictionary<string, HashSet<JsonSourceFile>> nameMap;
        private Dictionary<string, HashSet<JsonSourceFile>> shortPathMap;

        public IEnumerable<JsonSourceFile> GetEnumerator()
        {
            foreach (var file in sourceFiles)
            {
                yield return file;
            }
        }

        private void RemoveDiscardedFiles()
        {
            List<int> remove = new List<int>();
            for (int i = sourceFiles.Count - 1; i >= 0; i--)
            {
                var file = sourceFiles[i];
                if (file.path.StartsWith("Assets/", System.StringComparison.CurrentCultureIgnoreCase) && !File.Exists(file.path))
                {
                    remove.Add(i);
                }
            }

            foreach (var index in remove)
            {
                sourceFiles.RemoveAt(index);
            }
        }

        internal void Merge(IEnumerable<JsonSourceFile> files, bool checkDiscarded)
        {
            if (checkDiscarded)
            {
                RemoveDiscardedFiles();
            }
            pathMap = BuildSourceAssetsMap(sourceFiles);

            foreach (var file in files)
            {
                pathMap[file.path] = file;
            }

            sourceFiles.Clear();
            sourceFiles.AddRange(pathMap.Values);
            (nameMap, shortPathMap) = BuildShortPathMap(sourceFiles);
        }

        /// <summary>
        /// 按源文件路径来查找
        /// <summary>
        public (bool succeed, JsonSourceFile file) FindSourceFileByPath(string path)
        {
            if (pathMap == null)
                pathMap = BuildSourceAssetsMap(sourceFiles);
            if (string.IsNullOrEmpty(path) || !pathMap.TryGetValue(path, out var file))
                return (false, null);

            return (true, file);
        }

        /// <summary>
        /// 按源文件名来查找
        /// <summary>
        public (bool succeed, HashSet<JsonSourceFile> files) FindSourceFileByName(string name)
        {
            if (nameMap == null)
                (nameMap, shortPathMap) = BuildShortPathMap(sourceFiles);
            if (string.IsNullOrEmpty(name) || (!nameMap.TryGetValue(name, out var files) && !shortPathMap.TryGetValue(name, out files)))
                return (false, null);

            return (true, files);
        }

        /// <summary>
        /// 按源文件生成的输出文件的ShortPath来查找，可以用来查找bundle中包含哪些源文件
        /// </summary>
        public (bool succeed, HashSet<JsonSourceFile> files) FindSourceFilesByShortPath(string shortPath)
        {
            if (shortPathMap == null)
                (nameMap, shortPathMap) = BuildShortPathMap(sourceFiles);
            if (string.IsNullOrEmpty(shortPath) || !shortPathMap.TryGetValue(shortPath, out var files))
                return (false, null);

            return (true, files);
        }

        static Dictionary<string, JsonSourceFile> BuildSourceAssetsMap(IEnumerable<JsonSourceFile> sourceFiles)
        {
            Dictionary<string, JsonSourceFile> map = new Dictionary<string, JsonSourceFile>();
            foreach (var file in sourceFiles)
            {
                map.Add(file.path, file);
            }
            return map;
        }

        static (Dictionary<string, HashSet<JsonSourceFile>> nameMap, Dictionary<string, HashSet<JsonSourceFile>> shortMap) BuildShortPathMap(IEnumerable<JsonSourceFile> sourceFiles)
        {
            var nameMap = new Dictionary<string, HashSet<JsonSourceFile>>();
            var shortMap = new Dictionary<string, HashSet<JsonSourceFile>>();
            foreach (var src in sourceFiles)
            {
                if (src.path.Equals("AssetBundle Object", System.StringComparison.CurrentCultureIgnoreCase))
                    continue;
                var name = Path.GetFileName(src.path);
                if (!nameMap.TryGetValue(name, out var nameSet))
                {
                    nameMap[name] = nameSet = new HashSet<JsonSourceFile>();
                }
                nameSet.Add(src);
                foreach (var shortPath in src.shortPathList)
                {
                    if (!shortMap.TryGetValue(shortPath, out var srcSet))
                    {
                        shortMap[shortPath] = srcSet = new HashSet<JsonSourceFile>();
                    }
                    srcSet.Add(src);
                }
            }
            return (nameMap, shortMap);
        }

#if UNITY_EDITOR
        static Dictionary<string, JsonSourceFile> GetSourceAssetsMap(BuildReport report, bool detialAssetType, bool displayProgressBar)
        {
            var map = new Dictionary<string, (JsonSourceFile sourcrFile, HashSet<string> shortPathSet)>();
#if UNITY_2019_3_OR_NEWER
            for (int packedIndex = 0; packedIndex < report.packedAssets.Length; packedIndex++)
            {
                var packedAsset = report.packedAssets[packedIndex];

                if (displayProgressBar) ProgressBar.Display("PackedAsset", packedAsset.shortPath, packedIndex, report.packedAssets.Length);

                foreach (var entry in packedAsset.contents)
                {
                    var entryPath = entry.sourceAssetPath;
                    if (!map.TryGetValue(entryPath, out var item))
                    {
                        var type = GetAssetType(entryPath, detialAssetType);
                        map[entryPath] = item = (new JsonSourceFile() { path = entryPath, type = type }, new HashSet<string>());
                    }
                    item.shortPathSet.Add(packedAsset.shortPath);
                    item.sourcrFile.size += entry.packedSize;
                }
            }
#else
            var serializedObject = new SerializedObject(report);
            var appendices = serializedObject.FindProperty("m_Appendices");
            if (appendices != null)
            {
                for (var appendixIndex = 0; appendixIndex < appendices.arraySize; appendixIndex++)
                {
                    var appendix = appendices.GetArrayElementAtIndex(appendixIndex);
                    if (appendix.objectReferenceValue.GetType() != typeof(Object))
                        continue;
                    var appendixSO = new SerializedObject(appendix.objectReferenceValue);
                    if (appendixSO.FindProperty("m_ShortPath") == null)
                        continue;
                    var pathProperty = appendixSO.FindProperty("m_ShortPath");
                    if (pathProperty == null)
                        continue;
                    var shortPath = pathProperty.stringValue;
                    var contents = appendixSO.FindProperty("m_Contents");
                    if (contents == null)
                        continue;
                    for (var entryIndex = 0; entryIndex < contents.arraySize; entryIndex++)
                    {
                        var entry = contents.GetArrayElementAtIndex(entryIndex);
                        var entryPathProp = entry.FindPropertyRelative("buildTimeAssetPath");
                        if (entryPathProp == null)
                            continue;
                        var entryPath = entryPathProp.stringValue;
                        if (string.IsNullOrEmpty(entryPath))
                            continue;

                        if (displayProgressBar) ProgressBar.Display(shortPath, $"[{entryIndex}/{contents.arraySize}]{entryPath}", entryIndex / (float)contents.arraySize);

                        if (!map.TryGetValue(entryPath, out var item))
                        {
                            var type = GetAssetType(entryPath, detialAssetType);
                            map[entryPath] = item = (new JsonSourceFile() { path = entryPath, type = type }, new HashSet<string>());
                        }
                        item.shortPathSet.Add(shortPath);
                        var sizeProp = entry.FindPropertyRelative("packedSize");
                        var size = sizeProp != null ? sizeProp.intValue : 0;
                        item.sourcrFile.size += (ulong)size;
                    }
                }
            }
#endif
            var result = new Dictionary<string, JsonSourceFile>();
            foreach (var item in map)
            {
                item.Value.sourcrFile.shortPathList = item.Value.shortPathSet.ToList();
                result.Add(item.Key, item.Value.sourcrFile);
            }
            return result;
        }

        static Dictionary<string, string> assetTypeMap = new Dictionary<string, string>();
        static string GetAssetType(string assetPath, bool detialAssetType)
        {
            if (assetTypeMap.TryGetValue(assetPath, out var type))
            {
                return type;
            }
            var importer = UnityEditor.AssetImporter.GetAtPath(assetPath);
            type = importer != null ? importer.GetType().Name : "Unknown";
            if (type.EndsWith("Importer"))
                type = type.Substring(0, type.Length - 8);
            //  细分资源类型
            if (detialAssetType && (
                type.Equals("Unknown", System.StringComparison.CurrentCultureIgnoreCase) ||
                type.Equals("Asset", System.StringComparison.CurrentCultureIgnoreCase)
                ))
            {
                var asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
                if (asset != null)
                {
                    type = asset.GetType().Name;
                }
                else
                {
                    if (assetPath == "Resources/unity_builtin_extra")
                        type = "Built-in Asset";
                    else if (assetPath.StartsWith("Built-in Texture", System.StringComparison.CurrentCultureIgnoreCase))
                        type = "Built-in Texture";
                    else if (assetPath.StartsWith("Assets/", System.StringComparison.CurrentCultureIgnoreCase))
                        type = "Deleted Assets";
                    else
                        type = "Unknown";
                }
            }
            assetTypeMap[assetPath] = type;
            return type;
        }
#endif
    }

    [System.Serializable]
    public class JsonOutputFile
    {
        public string type;         //  输出文件类型
        public string fileName;     //  输出文件名
        public string filePath;     //  输出文件路径
        public string absPath;      //  输出文件绝对路径
        public ulong size;          //  输出文件大小(byte)
        public ulong manifestSize;  //  manifest大小(byte)，如果有的话

        public List<string> shortPathList = new List<string>();     //  如：CAB-xxxxxxxx, archive:/BuildPlayer-xxxxx 等
        public List<ulong> shortSizeList = new List<ulong>();       //  与上对应的大小
    }

    /// <summary>
    /// 输出文件映射，方便查询
    /// </summary>
    [Serializable]
    public class JsonOutputFileMap
    {
        public JsonOutputFileMap(JsonOutputFileMap map)
        {
            if (map != null)
            {
                outputFiles.AddRange(map.outputFiles);
            }
        }
#if UNITY_EDITOR
        public JsonOutputFileMap(BuildFile[] files, JsonSourceFileMap sourceFileMap, bool displayProgressBar)
        {
            fileNameMap = GetOutputFilesMap(files, sourceFileMap, displayProgressBar);
            outputFiles.AddRange(fileNameMap.Values);
            shortPathMap = BuildShortPathMap(fileNameMap.Values);
        }
#endif
        [UnityEngine.SerializeField] List<JsonOutputFile> outputFiles = new List<JsonOutputFile>();

        private Dictionary<string, JsonOutputFile> fileNameMap = new Dictionary<string, JsonOutputFile>();
        private Dictionary<string, (JsonOutputFile file, int index)> shortPathMap = new Dictionary<string, (JsonOutputFile, int)>();

        public IEnumerable<JsonOutputFile> GetEnumerator()
        {
            foreach (var file in outputFiles)
            {
                yield return file;
            }
        }

        private void RemoveDiscardedFiles()
        {
            //List<int> remove = new List<int>();
            //for (int i = outputFiles.Count - 1; i >= 0; i--)
            //{
            //    var file = outputFiles[i];
            //    if (!File.Exists(file.filePath))
            //    {
            //        remove.Add(i);
            //    }
            //}

            //foreach (var index in remove)
            //{
            //    outputFiles.RemoveAt(index);
            //}
        }

        internal void Merge(IEnumerable<JsonOutputFile> files, bool checkDiscarded)
        {
            if (checkDiscarded)
            {
                RemoveDiscardedFiles();
            }
            fileNameMap = BuildOutputFilesMap(outputFiles);
            foreach (var file in files)
            {
                fileNameMap[file.fileName] = file;
            }
            outputFiles.Clear();
            outputFiles.AddRange(fileNameMap.Values);
            shortPathMap = BuildShortPathMap(outputFiles);
        }

        /// <summary>
        /// 按输出文件名来查找
        /// </summary>
        public (bool succeed, JsonOutputFile file) FindOutputFileByFileName(string fileName)
        {
            if (fileNameMap != null)
                fileNameMap = BuildOutputFilesMap(outputFiles);
            if (string.IsNullOrEmpty(fileName) || !fileNameMap.TryGetValue(fileName, out var file))
                return (false, null);

            return (true, file);
        }

        /// <summary>
        /// 按输出文件的ShortPath来查找，可以用来查找shortPath对应的是文件
        /// </summary>
        public (bool succeed, JsonOutputFile file, int index) FindOutputFileByShortPath(string shortPath)
        {
            if (shortPathMap != null)
                BuildShortPathMap(outputFiles);
            if (string.IsNullOrEmpty(shortPath) || !shortPathMap.TryGetValue(shortPath, out var file))
                return (false, null, -1);

            return (true, file.file, file.index);
        }

        static Dictionary<string, JsonOutputFile> BuildOutputFilesMap(IEnumerable<JsonOutputFile> outputFiles)
        {
            var map = new Dictionary<string, JsonOutputFile>();
            foreach (var file in outputFiles)
            {
                map.Add(file.fileName, file);
            }
            return map;
        }

        static Dictionary<string, (JsonOutputFile, int)> BuildShortPathMap(IEnumerable<JsonOutputFile> outputFiles)
        {
            Dictionary<string, (JsonOutputFile, int)> map = new Dictionary<string, (JsonOutputFile, int)>();

            foreach (var file in outputFiles)
            {
                for (int index = 0; index < file.shortPathList.Count; index++)
                {
                    map.Add(file.shortPathList[index], (file, index));
                }
            }

            return map;
        }

#if UNITY_EDITOR
        static (string fileName, string shortPath, bool manifest, bool meta) DecodeOutputPath(string outputPath)
        {
            //  如果是 一般 Bundle 的 packed 文件，如 ……/Res/b_navmesh_b_cj_04_02.unity3d/CAB-18fc1f19e1d3792b24de3269299354a5
            int shortIndex = outputPath.LastIndexOf("/CAB-");
            //  如果是 Scene Bundle 的 packed 文件，如 ……/Res/b_scene_-267268513.unity3d/BuildPlayer-B_CJ_xuezhandaodi
            if (shortIndex < 0)
                shortIndex = outputPath.LastIndexOf("/BuildPlayer-");
            //  如果是 上面两种之一的文件，截取 shortPath
            var shortPath = shortIndex < 0 ? null : outputPath.Substring(shortIndex + 1);
            //  如果是 上面两种之一的文件，截取真正的 outputPath
            if (shortIndex >= 0) outputPath = outputPath.Substring(0, shortIndex);

            var fileName = Path.GetFileName(outputPath);

            if (shortIndex < 0)
            {
                //  检查是否是meta
                int index = fileName.IndexOf(".meta");
                bool meta = index >= 0;
                if (meta)
                {
                    fileName = fileName.Substring(0, index);
                }
                //  检查是否是manifest
                index = fileName.IndexOf(".manifest");
                if (index >= 0)
                {
                    return (fileName.Substring(0, index), null, !meta, meta);
                }
                return (fileName, null, false, meta);
            }
            return (fileName, shortPath, false, false);
        }

        static Dictionary<string, JsonOutputFile> GetOutputFilesMap(BuildFile[] files, JsonSourceFileMap sourceFileMap, bool displayProgressBar)
        {
            Dictionary<string, JsonOutputFile> outputMap = new Dictionary<string, JsonOutputFile>();
            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];

                if (displayProgressBar) ProgressBar.Display("BuildFile", file.path, 1 / (float)files.Length);

                (string fileName, string shortPath, bool manifest, bool meta) = DecodeOutputPath(file.path);
                if (!outputMap.TryGetValue(fileName, out var outputFile))
                {
                    outputMap[fileName] = outputFile = new JsonOutputFile() { fileName = fileName };
                }

                if (string.IsNullOrEmpty(shortPath))
                {
                    outputFile.absPath = file.path;
                    if(file.path != null && file.path.Length > System.Environment.CurrentDirectory.Length + 1)
                    {
                        outputFile.filePath = file.path.Substring(System.Environment.CurrentDirectory.Length + 1);
                    }
                    else
                    {
                        outputFile.filePath = file.path;
                    }
                    if (manifest)
                        outputFile.manifestSize = file.size;
                    else
                        outputFile.size = file.size;
                }
                else
                {
                    outputFile.shortPathList.Add(shortPath);
                    outputFile.shortSizeList.Add(file.size);
                }
                if (string.IsNullOrEmpty(outputFile.type))
                {
                    outputFile.type = GetOutputFileType(outputFile, sourceFileMap);
                }
            }

            return outputMap;
        }

        static Dictionary<string, float> temp = new Dictionary<string, float>();
        static string GetOutputFileType(JsonOutputFile outputFile, JsonSourceFileMap map)
        {
            bool succeed;
            JsonSourceFile source;
            (succeed, source) = map.FindSourceFileByPath(outputFile.filePath);
            if (succeed)
            {
                return source.type;
            }
            else
            {
                HashSet<JsonSourceFile> files;
                //  按所包含的源文件大小分
                foreach (var shortPath in outputFile.shortPathList)
                {
                    if (shortPath.StartsWith("BuildPlayer-", System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        return "Scene";
                    }
                    (succeed, files) = map.FindSourceFilesByShortPath(shortPath);
                    if (succeed)
                    {
                        foreach (var file in files)
                        {
                            if (file.type.Equals("SpriteAtlas", System.StringComparison.CurrentCultureIgnoreCase))
                                return file.type;
                            temp.TryGetValue(file.type, out var size);
                            temp[file.type] = size + (file.size * TypeSizeWeight(file.type));
                        }
                    }
                }
                if (!succeed)
                {
                    (succeed, files) = map.FindSourceFileByName(outputFile.fileName);
                    if (succeed)
                    {
                        foreach (var file in files)
                        {
                            if (file.type.Equals("SpriteAtlas", System.StringComparison.CurrentCultureIgnoreCase))
                                return file.type;
                            temp.TryGetValue(file.type, out var size);
                            temp[file.type] = size + (file.size * TypeSizeWeight(file.type));
                        }
                    }
                }
            }
            //  取最大的类型
            string type = "Unknown";
            if (temp.Count > 0)
            {
                float typeSize = 0;
                foreach (var item in temp)
                {
                    if (typeSize > item.Value)
                        continue;
                    type = item.Key;
                    typeSize = item.Value;
                }
                temp.Clear();
            }
            else
            {
                if (outputFile.fileName.EndsWith(".unity3d", System.StringComparison.CurrentCultureIgnoreCase)
                    || outputFile.filePath.EndsWith(".manifest", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    type = "AssetBundle";
                }
                else if (outputFile.filePath.EndsWith(".meta", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    type = "Meta";
                }
                else if (outputFile.fileName.EndsWith(".dll", System.StringComparison.CurrentCultureIgnoreCase)
                    || outputFile.fileName.EndsWith(".pdb", System.StringComparison.CurrentCultureIgnoreCase)
                    || outputFile.fileName.EndsWith(".mdb", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    type = "Assembly";
                }
                else if (outputFile.fileName.EndsWith(".64bytes", System.StringComparison.CurrentCultureIgnoreCase)
                    || outputFile.fileName.EndsWith(".32bytes", System.StringComparison.CurrentCultureIgnoreCase)
                    || outputFile.fileName.EndsWith(".lua", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    type = "LuaScripts";
                }
                else if (outputFile.fileName.EndsWith(".xml", System.StringComparison.CurrentCultureIgnoreCase)
                    || outputFile.fileName.EndsWith(".txt", System.StringComparison.CurrentCultureIgnoreCase)
                    || outputFile.fileName.EndsWith(".json", System.StringComparison.CurrentCultureIgnoreCase)
                    || outputFile.fileName.EndsWith(".bytes", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    type = "TextAsset";
                }
                else if (outputFile.fileName.EndsWith(".png", System.StringComparison.CurrentCultureIgnoreCase)
                    || outputFile.fileName.EndsWith(".tga", System.StringComparison.CurrentCultureIgnoreCase)
                    || outputFile.fileName.EndsWith(".jpg", System.StringComparison.CurrentCultureIgnoreCase)
                    || outputFile.fileName.EndsWith(".hdr", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    type = "Texture";
                }
                else if (outputFile.fileName.EndsWith(".acb", System.StringComparison.CurrentCultureIgnoreCase)
                    || outputFile.fileName.EndsWith(".awb", System.StringComparison.CurrentCultureIgnoreCase)
                    || outputFile.fileName.EndsWith(".usm", System.StringComparison.CurrentCultureIgnoreCase)
                    || outputFile.fileName.EndsWith(".acf", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    type = "Criware";
                }
                else if (outputFile.fileName.EndsWith(".a", System.StringComparison.CurrentCultureIgnoreCase)
                    || outputFile.fileName.EndsWith(".h", System.StringComparison.CurrentCultureIgnoreCase)
                    || outputFile.fileName.EndsWith(".mm", System.StringComparison.CurrentCultureIgnoreCase)
                    || outputFile.fileName.EndsWith(".java", System.StringComparison.CurrentCultureIgnoreCase)
                    || outputFile.fileName.EndsWith(".jar", System.StringComparison.CurrentCultureIgnoreCase)
                    || outputFile.fileName.EndsWith(".aar", System.StringComparison.CurrentCultureIgnoreCase)
                    /*|| outputFile.filePath.Contains("/Plugins/")*/)
                {
                    type = "Plugin";
                }
                else if (outputFile.fileName.EndsWith(".apk", System.StringComparison.CurrentCultureIgnoreCase)
                    || outputFile.fileName.EndsWith(".ipa", System.StringComparison.CurrentCultureIgnoreCase))
                {
                    type = "PACKAGED FILE";
                    //UnityEngine.Debug.LogError(outputFile.filePath);
                }
            }
            if(type == "Plugins")
                type = "Plugin";
            return type;
        }

        static float TypeSizeWeight(string type)
        {
            switch (type)
            {
                case "Texture":
                case "Texture2D":
                    return 1.2f;
                case "Unknown":
                    return 0.1f;
                case "MonoScript":
                    return 0.001f;
                case "Deleted Assets":
                    return 0.0000001f;
            }

            return 1f;
        }
#endif
    }
    [System.Serializable]
    public class JsonBuildStep
    {
        [NonSerialized] public int offset;      //  不序列化: 嵌套深度偏移
        [NonSerialized] public int depth;       //  不序列化: 嵌套深度   

        public string stepName;         //  步骤名
        public long duration;           //  持续时长(Ticks)

        public JsonMessage[] messages;            //  当前step的消息记录
        public List<JsonBuildStep> children = new List<JsonBuildStep>();   //  该 step 的细节

        public IEnumerable<JsonMessage> GetTotalErrors()
        {
            foreach (var message in messages)
            {
                if (message.isError)
                    yield return message;
            }

            foreach (var child in children)
            {
                foreach (var error in child.GetTotalErrors())
                {
                    yield return error;
                }
            }
        }
    }

    [System.Serializable]
    public class JsonMessage
    {
        public bool isError;
        public string message;
    }

    [Serializable]
    public class JsonBuildStepMap
    {
        public JsonBuildStepMap(JsonBuildStepMap map)
        {
            buildSteps = new List<JsonBuildStep>();
            errors = new List<JsonMessage>();
            if (map != null)
            {
                buildSteps.AddRange(map.buildSteps);
                errors.AddRange(map.errors);
            }
        }

#if UNITY_EDITOR
        public JsonBuildStepMap(BuildStep[] steps, bool displayProgressBar)
        {
            buildSteps = GetBuildSteps(steps, displayProgressBar);
            errors = new List<JsonMessage>();
            foreach (var build in buildSteps)
            {
                errors.AddRange(build.GetTotalErrors());
            }
        }
#endif
        [SerializeField] private List<JsonBuildStep> buildSteps;
        [SerializeField] private List<JsonMessage> errors;

        [NonSerialized] private HashSet<string> stepNames;

        public int rootStepCount { get { return buildSteps.Count; } }
        public int errorsCount { get { return errors.Count; } }

        public IEnumerable<JsonBuildStep> GetEnumerator()
        {
            foreach (var file in buildSteps)
            {
                yield return file;
            }
        }

        public IEnumerable<JsonMessage> GetErrorEnumerator()
        {
            if (errors == null)
                CalcTotalError();
            foreach (var error in errors)
            {
                yield return error;
            }
        }

        public bool ContainsStep(string stepName)
        {
            if (string.IsNullOrEmpty(stepName) || buildSteps == null || buildSteps.Count == 0)
                return false;
            if (stepNames == null)
                stepNames = new HashSet<string>();
            if (stepNames.Count == 0)
            {
                foreach (var step in buildSteps)
                {
                    stepNames.Add(step.stepName);
                }
            }
            return stepNames.Contains(stepName);
        }

        public void AddRange(IEnumerable<JsonBuildStep> steps)
        {
            var map = new Dictionary<string, JsonBuildStep>();
            AddRange(map, buildSteps);
            AddRange(map, steps);
            buildSteps.Clear();
            buildSteps.AddRange(map.Values);
            CalcTotalError();
        }

        private void AddRange(Dictionary<string, JsonBuildStep> map, IEnumerable<JsonBuildStep> steps)
        {
            foreach (var step in steps)
            {
                if (map.TryGetValue(step.stepName, out var src))
                {
                    src.duration += step.duration;
                    if (step.messages != null && step.messages.Length > 0)
                    {
                        var array = new JsonMessage[src.messages.Length + step.messages.Length];
                        Array.Copy(src.messages, 0, array, 0, src.messages.Length);
                        Array.Copy(step.messages, 0, array, src.messages.Length, step.messages.Length);
                        src.messages = array;
                    }
                    src.children.AddRange(step.children);
                }
                else
                {
                    map[step.stepName] = step;
                }
            }
        }

        public string[] GetErrors()
        {
            if (errors == null)
                CalcTotalError();
            return errors.Select((msg) => msg.message).ToArray();
        }

        void CalcTotalError()
        {
            if (errors == null)
                errors = new List<JsonMessage>();
            else
                errors.Clear();
            foreach (var build in buildSteps)
            {
                errors.AddRange(build.GetTotalErrors());
            }
        }

#if UNITY_EDITOR
        static Dictionary<string, bool> forceBreaks = new Dictionary<string, bool>
        {
            {"Build Asset Bundles", false},
            {"Build player", false},
            {"IL2CPP", true},
            {"Preparing streaming assets", true},
            {"Copying streaming assets", true},
            {"Cross-compiling assemblies", true},
            {"Building Gradle project", true},
            {"Creating Xcode project", true}
        };

        static List<JsonBuildStep> GetBuildSteps(BuildStep[] steps, bool displayProgressBar)
        {
            Dictionary<int, JsonBuildStep> map = new Dictionary<int, JsonBuildStep>();
            var root = new JsonBuildStep() { depth = -1, offset = -1 };
            var branch = new Stack<JsonBuildStep>();
            branch.Push(root);
            int offset = 0;
            JsonBuildStep buildPlayer = null;
            List<JsonBuildStep> buildSceneBundles = new List<JsonBuildStep>();
            for (int index = 0; index < steps.Length; index++)
            {
                var step = steps[index];

                if (displayProgressBar) ProgressBar.Display("BuildStep", step.name, 1 / (float)steps.Length);
                //  这是Build Scene Bundles的第一步，这里强制设置一个步骤把这些步骤包起来
                if (step.name == "Prepare assets for target platform")
                {
                    //  Build player 在这之前，所以不会为空
                    var node = new JsonBuildStep() { stepName = "Build Scene Bundles", depth = step.depth - offset - 1, offset = offset, messages = new JsonMessage[0] };
                    buildPlayer.children.Add(node);
                    branch.Push(node);
                    buildSceneBundles.Add(node);
                }
                if (forceBreaks.TryGetValue(step.name, out var forcePop))
                {
                    //  如果是强制打断的节点，则需要在父级上的把时间减去
                    int depth = step.depth;
                    for (int i = index - 1; i >= 0; i--)
                    {
                        var node = map[i];
                        if (node.offset != offset)
                            break;
                        var ticks = step.duration.Ticks;
                        if (node.depth + node.offset < depth)
                        {
                            depth--;
                            node.duration -= ticks;
                        }
                    }
                    offset = step.depth;
                }
                else if (step.depth <= offset)
                {
                    offset = 0;
                }

                var current = new JsonBuildStep()
                {
                    offset = offset,
                    depth = step.depth - offset,
                    stepName = step.name,
                    duration = step.duration.Ticks,
                    messages = ToJsonMessages(step.messages, step.name),
                };
                if (current.stepName == "Build player")
                {
                    buildPlayer = current;
                }
                map[index] = current;

                JsonBuildStep last = null;
                //  如果上个节点的深度小于等于当前节点则取出
                while (branch.Count > 1 && current.depth <= branch.Peek().depth)
                {
                    last = branch.Pop();
                }
                //  如果深度大于上一个节点，则表示为上一个节点的子节点
                if (branch.Count > 0 && current.depth > branch.Peek().depth)
                {
                    branch.Peek().children.Add(current);
                }
                //  如果该节点为强制取出节点，把上一个节点再推回去
                branch.Push(forcePop && last != null ? last : current);
            }

            foreach (var buildScene in buildSceneBundles)
            {
                foreach (var child in buildScene.children)
                {
                    buildScene.duration += child.duration;
                }
            }

            return root.children;
        }

        static JsonMessage[] empty = new JsonMessage[0];
        static JsonMessage[] ToJsonMessages(BuildStepMessage[] messages, string stepName)
        {
            if (messages == null || messages.Length == 0) return empty;

            JsonMessage[] result = new JsonMessage[messages.Length];
            for (int i = 0; i < messages.Length; i++)
            {
                var msg = messages[i];
                result[i] = new JsonMessage()
                {
                    isError = !(msg.type == LogType.Warning || msg.type == LogType.Log),
                    message = $"Step:{stepName}  {msg.type}:{msg.content};    "
                };
            }
            return result;
        }
#endif
    }

#if UNITY_EDITOR
    public class ProgressBar
    {
        private static float lastTime;
        public static float interval = 1f;

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Display(string title, string message, int index, float total)
        {
            Display(title, message, index / total);
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Display(string title, string message, float progress)
        {
            if (interval < 0.017f)
            {
                if (EditorUtility.DisplayCancelableProgressBar(title, message, progress))
                {
                    EditorUtility.ClearProgressBar();
                    throw new System.Exception("Process Cancel");
                }
            }
            else
            {
                var delta = UnityEngine.Time.realtimeSinceStartup - lastTime;
                if (delta > interval)
                {
                    lastTime += delta;
                    if (EditorUtility.DisplayCancelableProgressBar(title, message, progress))
                    {
                        EditorUtility.ClearProgressBar();
                        throw new System.Exception("Process Cancel");
                    }
                }
            }
        }

        [System.Diagnostics.Conditional("UNITY_EDITOR"), UnityEditor.Callbacks.DidReloadScripts]
        public static void Clear()
        {
            EditorUtility.ClearProgressBar();
        }
    }
#endif
}