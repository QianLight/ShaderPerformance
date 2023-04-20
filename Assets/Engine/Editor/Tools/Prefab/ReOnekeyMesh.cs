#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;

using System.IO;

namespace CFEngine.Editor
{
    public class ReOnekeyMesh
    {
        // private List<Mesh> shadowMesh = new List<Mesh>();

        [MenuItem("Tools/角色/ReOnekeyShadowMesh")]
        public static void Excute()
        {
            GetDirs("Assets/Creatures", 0);
        }
        private static void GetDirs(string dirPath, int count)
        {
            
            foreach (string path in Directory.GetFiles(dirPath, "*.Asset", SearchOption.AllDirectories))
            {
                Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if (mesh && mesh.name.Contains("_sd_"))
                {
                    Debug.Log(mesh.name);
                    mesh.tangents = null;
                    mesh.colors = null;
                    mesh.normals = null;
                    for (int i = 0; i < 8; i++)
                    {
                        mesh.SetUVs(i, (Vector4[])null);
                    }
                    count++;
                    AssetDatabase.SaveAssetIfDirty(mesh);
                }
            }

            Debug.LogFormat("处理shadow模型共计:{0}个",count);
        }
    }
}
#endif