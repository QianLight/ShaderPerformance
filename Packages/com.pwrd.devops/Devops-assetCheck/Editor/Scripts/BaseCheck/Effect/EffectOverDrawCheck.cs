using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace AssetCheck
{

    [CheckRuleDescription("ParticleSystem", "峰值OverDraw", "t:Prefab", "统计的数值来自于50帧(10秒)给出的数据取最高值，可能跟实际数据会有偏差", 80, true)]
    public class EffectOverDrawCheck : RuleBase
    {
        [PublicParam("最大OverDraw", eGUIType.Input)]
        public float overDraw = 0.8f;

        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            ProfilerRenderData[] renderDatas = RuntimeRenderCheckResult.GetProfilerRenderData(path);
            if (renderDatas == null)
                return true;
            float max = 0f;
            foreach (var renderData in renderDatas)
            {
                max = Math.Max(max, renderData.OverDraw);
            }
            output = $"{max}--标准值--{overDraw}--超出占比{(float)(max - overDraw) / overDraw}%";
            return max < overDraw;
        }
    }

}
