using AssetCheck;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("ParticleSystem", "检查ParticleSystem的FillRate", "t:Prefab", "统计的数值来自于50帧给出的数据取最高值，可能跟实际数据会有偏差", 80, true)]
    public class EffectFillRateCheck : RuleBase
    {
        [PublicParam("最大FillRate", eGUIType.Input)]
        public float maxFillRate = 0.3f;

        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = String.Empty;
            ProfilerRenderData[] renderDatas = RuntimeRenderCheckResult.GetProfilerRenderData(path);
            if (renderDatas == null)
                return true;
            float max = 0f;
            foreach (var renderData in renderDatas)
            {
                max = Math.Max(max, renderData.fillRate);
            }
            output = max.ToString();
            return max < maxFillRate;
        }
    }
}