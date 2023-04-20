using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("ParticleSystem", "检查使用的Texture的数量超过了阈值", "t:Prefab", "采样次数过多会影响性能，建议数量尽量低")]
    public class ParticleTextureCountCheck : RuleBase
    {
        [PublicParam("单材质最多图片使用个数", eGUIType.Input)]
        public int textureCount = 5;

        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (gObject == null)
                return true;
            List<string> materialNames = new List<string>();
            ParticleSystemRenderer[] psrs = gObject.GetComponentsInChildren<ParticleSystemRenderer>();
            bool result = true;
            foreach (var psr in psrs)
            {
                foreach (var material in psr.sharedMaterials)
                {
                    if (material == null)
                        continue;
                    string[] texturePropertyNames = material.GetTexturePropertyNames();
                    int tCount = 0;
                    foreach (var texturePropertyName in texturePropertyNames)
                    {
                        Texture texture = material.GetTexture(texturePropertyName);
                        if (texture != null)
                        {
                            tCount++;
                        }
                    }
                    if(tCount >= textureCount)
                    {
                        result = false;
                        materialNames.Add(material.name);
                    }
                }
            }
            output = string.Join(",", materialNames);
            return result;
        }
    }
}