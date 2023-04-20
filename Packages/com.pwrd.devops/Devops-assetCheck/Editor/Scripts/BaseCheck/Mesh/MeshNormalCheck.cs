using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Mesh", "包含Normal属性的网格", "t:model", "")]
    public class MeshNormalCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
                return true;
            if(mesh.normals.Length > 0)
            {
                output = "包含Normal属性";
            }
            return mesh.normals.Length == 0;
        }
    }
}

