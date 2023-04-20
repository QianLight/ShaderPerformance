using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Animation", "精度过高的动画片段", "t:model", "如果非特殊必要，可以保存小数点后两位，此规则检查小数点后是否超过两位数字，如果优化以后导致动画有可见的轻度变形，可以设置精度更高一些")]
    public class AnimationPrecisionCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
			output = string.Empty;
			ModelImporter importer = ModelImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
                return true;
			ModelImporterClipAnimation[] animationClipList = importer.clipAnimations;

			foreach (ModelImporterClipAnimation theAnimation in animationClipList)
			{
				ClipAnimationInfoCurve[] curves = theAnimation.curves;
				Keyframe key;
				Keyframe[] keyFrames;
				for (int ii = 0; ii < curves.Length; ++ii)
				{
					ClipAnimationInfoCurve curveDate = curves[ii];
					if (curveDate.curve == null || curveDate.curve.keys == null)
					{
						//Debug.LogWarning(string.Format("AnimationClipCurveData {0} don't have curve; Animation name {1} ", curveDate, animationPath));
						continue;
					}
					keyFrames = curveDate.curve.keys;
					for (int i = 0; i < keyFrames.Length; i++)
					{
						key = keyFrames[i];
						if (FloatAccuracy(key.value) > 3 || FloatAccuracy(key.inTangent) > 3 || FloatAccuracy(key.outTangent) > 3)
                        {
							output = "精度超过小数点后2位";
                            return false;

                        }

					}
					curveDate.curve.keys = keyFrames;
				}
			}
			return false;
        }

		int FloatAccuracy(float f)
        {
			string str = f.ToString();
			int nIndex = str.LastIndexOf('.');
			if (nIndex < 0)
				return 0;
			return str.Length - nIndex - 1;
        }

	}
}

