using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using Object = UnityEngine.Object;

namespace Trident.AssetCollect
{
    public static class BuildReportInspector
    {
        [System.Serializable]
        public class OutputData
        {
            public string totalTime;
            public string totalSize;
            public Build_step_info[] build_step_info;
            public Build_assettype_info[] build_assettype_info;

            public static string[] mergeStepName = { "Packaging assets" };

            private string GetErrorStr(BuildStepMessage[] msgs,string stepName)
            {
                string tempError = "";
                foreach (var message in msgs)
                {
                    if (message.type == LogType.Error)
                    {
                        string str = "Step:" + stepName + "  Error:" + message.content + ";    ";
                        tempError = tempError + str;
                    }
                    else if (message.type == LogType.Exception)
                    {
                        string str = "Step:" + stepName + "  Exception:" + message.content + ";    ";
                        tempError = tempError + str;
                    }
                    else if (message.type == LogType.Assert)
                    {
                        string str = "Assert:" + stepName + "  Exception:" + message.content + ";    ";
                        tempError = tempError + str;
                    }
                }
                return tempError;
            }

            public void ProcessFromReport(BuildReport report)
            {
                if (report == null)
                    return;
                //totalTime = report.summary.totalTime.TotalSeconds.ToString();
                //totalSize = report.summary.totalSize.ToString();
                double totalTimeDouble = 0;
                Dictionary<string, Build_step_info> tempStepInfos = new Dictionary<string, Build_step_info>();
                Dictionary<string, double> tempStepDurations = new Dictionary<string, double>();
                for (int i = 0; i < report.steps.Count(); i++)
                {
                    var stepFullName = report.steps[i].name;
                    bool foundMerge = false;
                    totalTimeDouble += report.steps[i].duration.TotalSeconds;
                    foreach (var mergeName in mergeStepName)
                    {
                        if (stepFullName.Contains(mergeName))
                        {
                            foundMerge = true;
                            if (!tempStepInfos.ContainsKey(mergeName))
                            {
                                tempStepInfos[mergeName] = new Build_step_info();
                                tempStepInfos[mergeName].build_step_name = mergeName;
                                tempStepInfos[mergeName].build_step_duration = report.steps[i].duration.TotalSeconds.ToString();
                                tempStepDurations[mergeName] = report.steps[i].duration.TotalSeconds;
                                tempStepInfos[mergeName].build_step_error = GetErrorStr(report.steps[i].messages, report.steps[i].name);
                            }
                            else
                            {
                                tempStepDurations[mergeName] = tempStepDurations[mergeName] + report.steps[i].duration.TotalSeconds;
                                tempStepInfos[mergeName].build_step_duration = tempStepDurations[mergeName].ToString();
                                tempStepInfos[mergeName].build_step_error += GetErrorStr(report.steps[i].messages, report.steps[i].name);
                            }
                        }
                    }
                    if (foundMerge == false) //不用合并的
                    {
                        tempStepInfos[stepFullName] = new Build_step_info();
                        tempStepInfos[stepFullName].build_step_name = stepFullName;
                        tempStepInfos[stepFullName].build_step_duration = report.steps[i].duration.TotalSeconds.ToString();
                        tempStepInfos[stepFullName].build_step_error = GetErrorStr(report.steps[i].messages, report.steps[i].name);
                    }
                }
                totalTimeDouble = Math.Round(totalTimeDouble, 1);
                totalTime = totalTimeDouble.ToString();
                totalSize = report.summary.totalSize.ToString();

                build_step_info = new Build_step_info[tempStepInfos.Count];
                int k = 0;
                foreach(var kvp in tempStepInfos)
                {
                    build_step_info[k] = kvp.Value;
                    k++;
                }

                var tempFileFormats = new Dictionary<string, double>();
#if UNITY_2019_3_OR_NEWER
                foreach (var packedAsset in report.packedAssets)
                {
                    var totalSizeProp = packedAsset.overhead;
                    foreach (var entry in packedAsset.contents)
                    {
                        var asset = UnityEditor.AssetImporter.GetAtPath(entry.sourceAssetPath);
                        var type = asset != null ? asset.GetType().Name : "Unknown";
                        if (type.EndsWith("Importer"))
                            type = type.Substring(0, type.Length - 8);
                        var sizeProp = entry.packedSize;
                        if (!tempFileFormats.ContainsKey(type))
                            tempFileFormats[type] = 0;
                        tempFileFormats[type] += (double)sizeProp;
                    }
                }
#else
                var serializedObject = new SerializedObject(report);
                var appendices = serializedObject.FindProperty("m_Appendices");
                if (appendices != null)
                {
                    for (var i = 0; i < appendices.arraySize; i++)
                    {
                        var appendix = appendices.GetArrayElementAtIndex(i);
                        if (appendix.objectReferenceValue.GetType() != typeof(Object))
                            continue;
                        var appendixSO = new SerializedObject(appendix.objectReferenceValue);
                        if (appendixSO.FindProperty("m_ShortPath") == null)
                            continue;
                        var pathProperty = appendixSO.FindProperty("m_ShortPath");
                        if (pathProperty == null)
                            continue;
                        var contents = appendixSO.FindProperty("m_Contents");
                        if (contents == null)
                            continue;
                        for (var j = 0; j < contents.arraySize; j++)
                        {
                            var entry = contents.GetArrayElementAtIndex(j);
                            var entryPathProp = entry.FindPropertyRelative("buildTimeAssetPath");
                            if (entryPathProp == null)
                                continue;
                            var entryPath = entryPathProp.stringValue;
                            if (string.IsNullOrEmpty(entryPath))
                                continue;
                            var asset = UnityEditor.AssetImporter.GetAtPath(entryPath);
                            var type = asset != null ? asset.GetType().Name : "Unknown";
                            if (type.EndsWith("Importer"))
                                type = type.Substring(0, type.Length - 8);
                            var sizeProp = entry.FindPropertyRelative("packedSize");

                            var size = sizeProp != null ? sizeProp.intValue : 0;
                            if (!tempFileFormats.ContainsKey(type))
                                tempFileFormats[type] = 0;
                            tempFileFormats[type] += (double)size;
                        }
                    }
                }
#endif

                build_assettype_info = new Build_assettype_info[tempFileFormats.Count()];
                int index = 0;
                foreach (var kvp in tempFileFormats)
                {
                    build_assettype_info[index] = new Build_assettype_info();
                    build_assettype_info[index].build_assettype_name = kvp.Key;
                    double size = Math.Round((kvp.Value / (double)1024), 1);
                    build_assettype_info[index].build_assettype_size = kvp.Value.ToString();
                    index++;
                }
            }
        }

        [System.Serializable]
        public class Build_step_info
        {
            public string build_step_error;
            public string build_step_name;
            public string build_step_duration;
        }

        [System.Serializable]
        public class Build_assettype_info
        {
            public string build_assettype_name;
            public string build_assettype_size;
        }

        public static void OpenLastBuild()
        {
            const string buildReportDir = "Assets/BuildReports";
            if (!Directory.Exists(buildReportDir))
                Directory.CreateDirectory(buildReportDir);
            var date = File.GetLastWriteTime("Library/LastBuild.buildreport");
            var assetPath = buildReportDir + "/Build_" + date.ToString("yyyy-dd-MM-HH-mm-ss") + ".buildreport";
            File.Copy("Library/LastBuild.buildreport", assetPath, true);
            AssetDatabase.ImportAsset(assetPath);

            var report = AssetDatabase.LoadAssetAtPath<BuildReport>(assetPath);
            OutputData output = new OutputData();
            output.ProcessFromReport(report);
            ConvertToJson(output);
        }

        public static string ConvertToJson(OutputData data)
        {
            var jsonstr = JsonUtility.ToJson(data);
            Debug.Log(jsonstr);
            var path = UnityEngine.Application.dataPath + "/Resources/lastbuildreport.json";
            File.WriteAllText(path, jsonstr);
            return jsonstr;
        }
    }
}