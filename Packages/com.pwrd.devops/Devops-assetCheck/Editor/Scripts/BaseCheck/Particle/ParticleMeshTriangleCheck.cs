using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("ParticleSystem", "检查粒子系统使用的Mesh的面数超过阈值", "t:Prefab", "")]

    public class ParticleMeshTriangleCheck : RuleBase
    {
        [PublicParam("最大面数", eGUIType.Input)]
        public int maxCount = 500;


        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (gObject == null)
                return true;

            List<string> meshNames = new List<string>();
            ParticleSystemRenderer[] psrs = gObject.GetComponentsInChildren<ParticleSystemRenderer>();
            bool result = true;
            foreach (var psr in psrs)
            {
                if (psr.renderMode == ParticleSystemRenderMode.Mesh)
                {
                    if (psr.mesh != null && psr.mesh.triangles.Length > maxCount)
                    {
                        if(!meshNames.Contains(psr.mesh.name))
                            meshNames.Add(psr.mesh.name);
                        result = false;
                    }
                }
            }
            output = string.Join(",", meshNames);
            return result;
        }
    }
}