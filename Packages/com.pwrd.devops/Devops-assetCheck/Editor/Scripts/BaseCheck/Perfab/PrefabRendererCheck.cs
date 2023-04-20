using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



namespace AssetCheck
{
    [CheckRuleDescription("Prefab", "缺失Renderer的LODGroup组件", "t:prefab", "")]
    public class PrefabRendererCheck : RuleBase
    {
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gameObject = AssetDatabase.LoadAssetAtPath<GameObject> (path);
            if (gameObject == null)
                return true;
            LODGroup group = gameObject.GetComponent<LODGroup>();
            if(group != null)
            {
                for (int i = 0; i < group.GetLODs().Length; i++)
                {
                    LOD lod = group.GetLODs()[i];
                    if (lod.renderers == null)
                    {
                        output = gameObject.name;
                        return false;
                    }
                }
            }
            

            return true;
        }
    }
}
