using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace AssetCheck
{
    [CheckRuleDescription("Animation", "Compression != Optimal的动画资源", "t:model", "")]
    public class AnimationOptimalCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            ModelImporter importer = ModelImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
                return true;
            output = importer.animationCompression.ToString();
            return importer.animationCompression == ModelImporterAnimationCompression.Optimal;
        }
    }
}
