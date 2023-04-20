using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("ParticleSystem", "检查Mesh发射数量超标的粒子系统", "t:Prefab", "尽量使用Billboard发射模式，如果用Mesh，建议发射数量尽量少")]
    public class ParticleMeshCheck : RuleBase
    {
        [PublicParam("最大数量", eGUIType.Input)]
        public int maxCount = 5;

        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (gObject == null)
                return true;

            int meshCount = ParticleMeshCount(gObject);
            output = meshCount.ToString();
            return meshCount < maxCount;
        }

        int ParticleMeshCount(GameObject gObject)
        {
            int nCount = 0;
            ParticleSystemRenderer[] psrs = gObject.GetComponentsInChildren<ParticleSystemRenderer>();
            foreach (var psr in psrs)
            {
                if(psr.renderMode == ParticleSystemRenderMode.Mesh)
                {
                    ParticleSystem ps = psr.gameObject.GetComponent<ParticleSystem>();
                    if(ps != null)
                    {
                        nCount += ps.main.maxParticles;
                    }
                }
            }
            return nCount;
        }
    }
}
