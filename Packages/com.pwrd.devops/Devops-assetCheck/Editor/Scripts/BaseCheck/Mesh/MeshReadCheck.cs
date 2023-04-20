using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace AssetCheck
{
    [CheckRuleDescription("Mesh", "开启Read/Write选项的网格", "t:model", "")]
    public class MeshReadCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh == null)
                return true;
            if(mesh.isReadable)
            {
                output = "开启了read/write";
            }
            return !mesh.isReadable;
        }
    }
}