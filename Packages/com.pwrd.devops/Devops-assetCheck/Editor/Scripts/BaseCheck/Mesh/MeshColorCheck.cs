using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Mesh", "包含Color属性的网格", "t:Mesh", "")]
    public class MeshColorCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
                return true;
            //TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            if(mesh.colors.Length > 0)
            {
                output = "包含color属性";
            }
            return mesh.colors.Length == 0;
        }
    }
}

