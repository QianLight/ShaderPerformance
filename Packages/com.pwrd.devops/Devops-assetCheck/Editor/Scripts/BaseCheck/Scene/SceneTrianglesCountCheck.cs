using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AssetCheck
{
    [CheckRuleDescription("Scene", "场景面数检查", "t:scene", "")]
    public class SceneTrianglesCountCheck : RuleBase
    {
        [PublicParam("场景面数", eGUIType.Input)]
        public uint trianglesCount = 200000;
        [PublicMethod]
        public bool Check(string path, out string output)
        {
            uint CurrentSceneTrianglesCount = 0;
            AssetHelper.OpenScene(path);
            MeshFilter[] meshFilters = (MeshFilter[])Resources.FindObjectsOfTypeAll(typeof(MeshFilter));
            foreach (var meshFilter in meshFilters)
            {
                if (meshFilter.sharedMesh != null)
                {
                    CurrentSceneTrianglesCount += (uint)meshFilter.sharedMesh.triangles.Length;
                }
            }
            output = CurrentSceneTrianglesCount.ToString();
            AssetHelper.BackLastScene();
            return CurrentSceneTrianglesCount < trianglesCount;
        }
    }
}