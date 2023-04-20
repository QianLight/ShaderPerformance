using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Mesh", "未开启OptimizeMesh选项的网格", "t:model", "")]
    public class MeshOptimizeMeshCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            Mesh mesh = AssetDatabase.LoadAssetAtPath <Mesh> (path);
            if (mesh == null)
                return true;
            ModelImporter importer = ModelImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
                return true;
#if UNITY_2019_1_OR_NEWER
            if(importer.optimizeMeshVertices)
            {
                output = "未开启";
            }
            return importer.optimizeMeshVertices;
#else
            if(importer.optimizeMesh)
            {
                output = "未开启";
            }
            return importer.optimizeMesh;
#endif
        }
    }
}
