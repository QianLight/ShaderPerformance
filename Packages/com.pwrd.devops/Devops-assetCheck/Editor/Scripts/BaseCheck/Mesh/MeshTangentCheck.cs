using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace AssetCheck
{
    [CheckRuleDescription("Mesh", "包含切线属性的网格", "t:model", "")]
    public class MeshTangentCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
                return true;
            if(mesh.tangents.Length > 0)
            {
                output = "包含切线";
            }
            return mesh.tangents.Length == 0;
        }
    }
}