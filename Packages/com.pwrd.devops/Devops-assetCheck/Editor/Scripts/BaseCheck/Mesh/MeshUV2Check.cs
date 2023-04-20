using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Mesh", "包含UV2属性的网格", "t:model", "")]
    public class MeshUV2Check : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
                return true;
            //ModelImporter importer = (ModelImporter)ModelImporter.GetAtPath(path);
            if(mesh.uv2.Length > 0)
            {
                output = "包含UV2";
            }
            return mesh.uv2.Length == 0;
        }
    }
}

