using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace AssetCheck
{
    [CheckRuleDescription("ParticleSystem", "检查粒子系统运行时内存占用量", "t:Prefab", "这里主要统计的是图片资源的内存占用量 + mesh资源的内存占用量")]
    public class ParticleMemoryCheck : RuleBase
    {
        [PublicParam("内存大小（k）", eGUIType.Input)]
        public long memoryUse = 500;

        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (gObject == null)
                return true;
            long currentMemoryUse = GetTextureMemorySize(gObject) / 1024;
            output = currentMemoryUse.ToString();
            return currentMemoryUse < memoryUse;
        }


        public long GetTextureMemorySize(GameObject effectObj)
        {
            if (effectObj == null)
            {
                return 0;
            }

            long sumSize = 0;

            List<ParticleSystemRenderer> meshRendererlist =
                new List<ParticleSystemRenderer>(effectObj.GetComponentsInChildren<ParticleSystemRenderer>());
            List<Texture> texList = new List<Texture>();
            List<Mesh> meshList = new List<Mesh>();
            foreach (ParticleSystemRenderer item in meshRendererlist)
            {
                Material[] mats = item.sharedMaterials;
                foreach (Material mat in mats)
                {
                    if (mat != null)
                    {
                        int[] textureNames = mat.GetTexturePropertyNameIDs();
                        foreach(var textureName in textureNames)
                        {
                            Texture tex = mat.GetTexture(textureName);
                            if (tex != null && !texList.Contains(tex))
                                texList.Add(tex);
                        }
                    }
                }
                if(item.renderMode == ParticleSystemRenderMode.Mesh)
                {
                    if(item.mesh != null)
                    {
                        if (!meshList.Contains(item.mesh))
                            meshList.Add(item.mesh);
                    }
                }
            }

            foreach (var tex in texList)
            {
                sumSize += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(tex);
            }
            foreach(var mesh in meshList)
            {
                sumSize += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(mesh);
            }
            return sumSize;
        }
    }
}