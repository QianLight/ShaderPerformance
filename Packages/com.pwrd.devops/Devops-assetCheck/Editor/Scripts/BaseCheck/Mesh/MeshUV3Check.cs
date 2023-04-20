using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Mesh", "包含UV3或UV4属性的网格", "t:model", "")]
    public class MeshUV3Check : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
                return true;
            //ModelImporter importer = (ModelImporter)ModelImporter.GetAtPath(path);
            if(mesh.uv3.Length > 0)
            {
                output += "包含UV3";
            }
            if (mesh.uv4.Length > 0)
            {
                output += "包含UV4";
            }
            return mesh.uv3.Length == 0 && mesh.uv4.Length == 0;
        }
    }
}