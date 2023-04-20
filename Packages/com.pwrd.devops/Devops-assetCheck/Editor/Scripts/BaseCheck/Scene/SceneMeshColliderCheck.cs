using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetCheck
{
    [CheckRuleDescription("Scene", "场景中MeshCollider过多", "t:scene", "")]

    public class SceneMeshColliderCheck : RuleBase
    {
        [PublicParam("MeshCollider数量", eGUIType.Input)]
        public int meshColliderCount = 100;

        [PublicMethod]
        public bool Check(string path, out string output)
        {
            AssetHelper.OpenScene(path);
            MeshCollider[] meshColliders = (MeshCollider[])Resources.FindObjectsOfTypeAll(typeof(MeshCollider));
            int count = meshColliders.Length;
            output = count.ToString();
            AssetHelper.BackLastScene();
            return count < meshColliderCount;
        }
    }
}