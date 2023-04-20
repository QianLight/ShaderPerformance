using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityEngine.AssetGraph
{
    public static class DependencyUtility
    {
        public static string[] GetDependencies(List<AssetReference> assetList)
        {
            string[] atlasList = new string[assetList.Count];
            for(int i = 0; i < assetList.Count; i++)
            {
                atlasList[i] = assetList[i].importFrom;
            }
            
            var dependencies = AssetDatabase.GetDependencies(atlasList);
            return dependencies;
        }
    }
}
