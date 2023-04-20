using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace AssetCheck
{
    [CheckRuleDescription("Mesh", "顶点数量和面数检查", "t:Mesh", "")]
    public class MeshVertexNumCheck : RuleBase
    {
        [PublicParam("顶点数量", eGUIType.Input)]
        public int VertexNum = 1000;

        [PublicParam("面数", eGUIType.Input)]
        public int TriangleNum = 1000;
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if(mesh == null)
                return true;
            output = $"顶点数量:{mesh.vertexCount} 面数:{mesh.triangles.Length}";
            return mesh.vertexCount < VertexNum && mesh.triangles.Length < TriangleNum;
        }
    }
}

