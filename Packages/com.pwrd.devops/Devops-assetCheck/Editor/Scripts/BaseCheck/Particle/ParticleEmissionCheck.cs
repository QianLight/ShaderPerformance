using UnityEditor;
using UnityEngine;
namespace AssetCheck
{
    [CheckRuleDescription("ParticleSystem", "检查prefab粒子发射网格数", "t:Prefab", "")]
    public class ParticleEmissionCheck : RuleBase
    {
        [PublicParam("最大数量", eGUIType.Input)]
        public int maxcount = 500;

        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (gObject == null)
                return true;
            int nParticleCount = ParticleCount(gObject); ;
            output = nParticleCount.ToString();
            return nParticleCount < maxcount;
        }

        int ParticleCount(GameObject gObject)
        {
            int nCount = 0;
            ParticleSystem[] pss = gObject.GetComponentsInChildren<ParticleSystem>();

            var particleRenders = gObject.GetComponentsInChildren<ParticleSystemRenderer>();

            foreach (var psRender in particleRenders)
            {

                var meshList = new Mesh[psRender.meshCount];
                psRender.GetMeshes(meshList);

                foreach (var ms in meshList)
                {
                    if (ms == null) continue;

                    if (psRender.renderMode == ParticleSystemRenderMode.Mesh)
                        nCount += ms.triangles.Length / 3;
                }
            }
            return nCount;
        }
    }
}

