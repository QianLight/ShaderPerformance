using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AssetCheck
{
    public class RuntimeRenderCheckResult
    {
        static Dictionary<string, ProfilerRenderData[]> profilerRenderDatas;
        static string assetCheckPathConfig;
        static bool exportCSV;
        static bool CheckEndCloseApp;
        public static void Init()
        {
            if (File.Exists(Defines.CheckPathRuntimeRenderResult))
            {
                profilerRenderDatas = new Dictionary<string, ProfilerRenderData[]>();
                AssetCheckRenderResultConfig config = JsonUtility.FromJson<AssetCheckRenderResultConfig>(File.ReadAllText(Defines.CheckPathRuntimeRenderResult));
                File.Delete(Defines.CheckPathRuntimeRenderResult);
                assetCheckPathConfig = config.assetCheckPathConfig;
                exportCSV = config.exportCSV;
                CheckEndCloseApp = config.checkEndCloseApp;
                foreach (var singleFileResultDatas in config.filesProfilerRenderDatas)
                {
                    profilerRenderDatas.Add(singleFileResultDatas.fileName, singleFileResultDatas.profilerRenderDatas);
                }
            }
        }

        public static void Release()
        {
            profilerRenderDatas = null;
            assetCheckPathConfig = null;
        }

        public static bool HasGetResult()
        {
            return profilerRenderDatas != null;
        }

        public static ProfilerRenderData[] GetProfilerRenderData(string filePath)
        {
            if (!profilerRenderDatas.ContainsKey(filePath))
                return null;
            return profilerRenderDatas[filePath];
        }

        public static string GetAssetCheckPathConfigJson()
        {
            return assetCheckPathConfig;
        }

        public static bool IsExportCSV()
        {
            return exportCSV;
        }

        public static bool IsCheckEndClose()
        {
            return CheckEndCloseApp;
        }
    }

}