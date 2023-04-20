using AssetCheck;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("ParticleSystem", "峰值drawcall", "t:Prefab", "统计的数值来自于50帧(10秒)给出的数据取最高值，可能跟实际数据会有偏差", 30, true)]
    public class EffectDrawCallCheck : RuleBase
    {
        [PublicParam("最大Drawcall", eGUIType.Input)]
        public int maxdrawcall = 10;

        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = String.Empty;
            ProfilerRenderData[] renderDatas = RuntimeRenderCheckResult.GetProfilerRenderData(path);
            if (renderDatas == null)
                return true;
            long max = 0;
            foreach (var renderData in renderDatas)
            {
                max = Math.Max(max, renderData.DrawCalls);
            }
            output = $"{max}--标准值--{maxdrawcall}--超出占比{(float)(max - maxdrawcall)/maxdrawcall}%";
            return max < maxdrawcall;
        }
    }
}