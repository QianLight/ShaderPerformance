using System.Collections.Generic;
using System.IO;
using CFEngine;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GPUInstancingRoot))]
public class GPUInstancingRootEditor : Editor
{
    private struct MeshMaterialPair
    {
        public readonly Mesh mesh;
        public readonly Material material;

        public override int GetHashCode()
        {
            int rendererHash = mesh ? mesh.GetHashCode() : 0;
            int materialHash = material ? material.GetHashCode() : 0;
            return rendererHash + materialHash;
        }

        public override bool Equals(object obj)
        {
            return obj is MeshMaterialPair other && other.mesh == mesh && other.material == material;
        }

        public MeshMaterialPair(Mesh mesh, Material material)
        {
            this.mesh = mesh;
            this.material = material;
        }
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (target is GPUInstancingRoot root)
        {
            if (root.defaultDistance.Count < 1)
            {
                EditorGUILayout.HelpBox("还没设置lod距离！", MessageType.Error);
            }
            else if (GUILayout.Button("收集"))
            {
                CollectInstancingData(root);
            }
        }
    }

    public static void CollectInstancingData(GPUInstancingRoot root)
    {
        MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>();
        List<Material> mats = ListPool<Material>.Get();
        Dictionary<MeshMaterialPair, List<MeshRenderer>> pairs = new Dictionary<MeshMaterialPair, List<MeshRenderer>>();
        HashSet<Renderer> errorRenderers = new HashSet<Renderer>();
            
        foreach (MeshRenderer renderer in renderers)
        {
            MeshFilter meshFilter = renderer.GetComponent<MeshFilter>();
            if (!meshFilter || !meshFilter.sharedMesh)
                continue;

            renderer.GetSharedMaterials(mats);
            foreach (Material mat in mats)
            {
                if (!mat || !mat.shader || mat.shader != Shader.Find("URP/Scene/UberGrass"))
                {
                    errorRenderers.Add(renderer);
                }
                else
                {
                    MeshMaterialPair rmh = new MeshMaterialPair(meshFilter.sharedMesh, mat);
                    List<MeshRenderer> list = pairs.ForceGetValue(rmh);
                    list.Add(renderer);       
                }
            }
        }

        root.groups.Clear();
        foreach (KeyValuePair<MeshMaterialPair, List<MeshRenderer>> pair in pairs)
        {
            GPUInstancingGroup group = new GPUInstancingGroup();

            // lod 0
            GPUInstancingLod lod = new GPUInstancingLod();
            lod.distance = GetLodDistance(root, pair.Key.mesh, 0);
            lod.mesh = pair.Key.mesh;
            group.lods = new List<GPUInstancingLod> {lod};

            // Try find other lod meshes.
            string meshPath = AssetDatabase.GetAssetPath(lod.mesh);
            int extIndex = meshPath.LastIndexOf('.');
            int lodIndex = 1;
            while (true)
            {
                string lodMeshPath = meshPath.Insert(extIndex, "_lod" + lodIndex);
                if (!File.Exists(lodMeshPath))
                    break;

                string lowerPath = lodMeshPath.ToLower();

                Mesh lodMesh = null;
                if (lowerPath.EndsWith(".fbx"))
                {
                    Object[] assets = AssetDatabase.LoadAllAssetsAtPath(lowerPath);
                    foreach (Object asset in assets)
                    {
                        if (asset is Mesh mesh)
                        {
                            lodMesh = mesh;
                        }
                    }
                }
                else if (lowerPath.EndsWith(".asset"))
                {
                    lodMesh = AssetDatabase.LoadAssetAtPath<Mesh>(lowerPath);
                }

                if (lodMesh)
                {
                    lod = new GPUInstancingLod();
                    lod.distance = GetLodDistance(root, pair.Key.mesh, lodIndex);
                    lod.mesh = lodMesh;
                    group.lods.Add(lod);
                    lodIndex++;
                }
                else
                {
                    break;
                }
            }

            if (!pair.Key.material.enableInstancing)
            {
                pair.Key.material.enableInstancing = true;
                EditorUtility.SetDirty(pair.Key.material);
            }

            group.cullingRadius = 1f;
            group.material = pair.Key.material;
            group.renderers = pair.Value;
            root.groups.Add(group);
        }

        if (errorRenderers.Count > 0)
        {
            EditorUtility.DisplayDialog("收集Instance列表失败！", "存在材质不正确的Renderer。\n已经帮你选中这批物体。\n也可以在Console里查看报错里的列表。", "好的");
            List<GameObject> newSelections = new List<GameObject>();
            int index = 0;
            foreach (Renderer renderer in errorRenderers)
            {
                newSelections.Add(renderer.gameObject);
                Debug.LogError($"收集Instance失败Renderer[{index++}]：{renderer}", renderer);
            }
            Selection.objects = newSelections.ToArray();
        }
    }

    private static float GetLodDistance(GPUInstancingRoot root, Mesh mesh, int lodIndex)
    {
        foreach (GPUInstancingRoot.CustomLodDistance lodDistance in root.customDistance)
        {
            if (lodDistance.mesh == mesh)
            {
                return lodDistance.distances[lodIndex];
            }
        }

        return root.defaultDistance[lodIndex];
    }
}