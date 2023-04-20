using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace AssetCheck
{
    [CheckRuleDescription("Animation", "动画的导入设置未关闭ResampleCurve", "t:model", "转成四元数以后对数据表现优化并不太明显，但是显著增加了内存使用")]
    public class AnimationResampleCurveCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            ModelImporter importer = ModelImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
                return true;

            output = importer.resampleCurves.ToString();
            return !importer.resampleCurves;
        }
    }
}
