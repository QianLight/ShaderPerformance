using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetCheck
{

    [CheckRuleDescription("ParticleSystem", "峰值顶点数", "t:Prefab", "统计的数值来自于50帧(10秒)给出的数据取最高值，可能跟实际数据会有偏差", 30, true)]
    public class ParticleVerticesCheck : RuleBase
    {
        [PublicParam("最大顶点数", eGUIType.Input)]
        public int verticesCount = 500;

        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            ProfilerRenderData[] renderDatas = RuntimeRenderCheckResult.GetProfilerRenderData(path);
            if (renderDatas == null)
                return true;
            int max = 0;
            foreach (var renderData in renderDatas)
            {
                max = Math.Max(max, (int)renderData.Vertices);
            }
            output = $"{max}--标准值--{verticesCount}--超出占比{(float)(max - verticesCount) / verticesCount}%";
            return max < verticesCount;
        }
    }
}