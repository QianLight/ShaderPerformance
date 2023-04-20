using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetCheck
{
    [Serializable]
    public class AssetCheckRenderInfoConfig
    {
        public bool exportCSV;
        public bool checkEndCloseApp;
        public string assetCheckPathConfig;
        public List<string> assetList = new List<string>();
        public List<string> copyAssetList = new List<string>();
    }

    [Serializable]
    public class ProfilerRenderData
    {
        public long SetPassCall = 0;
        public long DrawCalls = 0;
        public long Vertices = 0;
        public float OverDraw = 0f;
        public float fillRate = 0f;

        public static ProfilerRenderData operator -(ProfilerRenderData p1, ProfilerRenderData p2)
        {
            return new ProfilerRenderData()
            {
                SetPassCall = p1.SetPassCall - p2.SetPassCall,
                DrawCalls = p1.DrawCalls - p2.DrawCalls,
                Vertices = p1.Vertices - p2.Vertices,
            };
        }
    }

    [Serializable]
    public class SingleFileProfilerRenderDatas
    {
        public string fileName;
        public ProfilerRenderData[] profilerRenderDatas;
    }

    [Serializable]
    public class AssetCheckRenderResultConfig
    {
        public bool checkEndCloseApp;
        public bool exportCSV;
        public string assetCheckPathConfig;
        public List<SingleFileProfilerRenderDatas> filesProfilerRenderDatas = new List<SingleFileProfilerRenderDatas>();
    }
}