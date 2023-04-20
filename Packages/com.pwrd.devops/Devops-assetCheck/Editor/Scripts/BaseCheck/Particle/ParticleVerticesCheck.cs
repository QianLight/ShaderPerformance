using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetCheck
{

    [CheckRuleDescription("ParticleSystem", "��ֵ������", "t:Prefab", "ͳ�Ƶ���ֵ������50֡(10��)����������ȡ���ֵ�����ܸ�ʵ�����ݻ���ƫ��", 30, true)]
    public class ParticleVerticesCheck : RuleBase
    {
        [PublicParam("��󶥵���", eGUIType.Input)]
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
            output = $"{max}--��׼ֵ--{verticesCount}--����ռ��{(float)(max - verticesCount) / verticesCount}%";
            return max < verticesCount;
        }
    }
}