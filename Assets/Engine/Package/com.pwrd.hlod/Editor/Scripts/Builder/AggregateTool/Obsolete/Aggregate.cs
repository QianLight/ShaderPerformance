using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;

namespace com.pwrd.hlod.editor
{
    public class Aggregate
    {
        public static HLODResultData RunAggregate(List<Renderer> renderers, AggregateParam param)
        {
            var list = RunAggregate(new List<(List<Renderer>, AggregateParam)>() {(renderers, param)});
            return list.First();
        }

        public static List<HLODResultData> RunAggregate(List<(List<Renderer>, AggregateParam)> aggregateList)
        {
            var resultList = new List<HLODResultData>();
            for (int i = 0; i < aggregateList.Count; i++)
            {
                try
                {
                    var tuple = aggregateList[i];
                    var list = tuple.Item1;
                    var param = tuple.Item2;
                    var progressTitle = param.outputName + "代理网格生成";

                    EditorUtility.DisplayProgressBar(progressTitle, "马上开始", (float) i / aggregateList.Count);
                    var center = AggregateUtils.GetBoundsCenter(list);
                    var mergeDatas = CollectMergeDatas(list, center);

                    EditorUtility.DisplayProgressBar(progressTitle, "正在打包图集", ((float) i + 0.1f) / aggregateList.Count);
                    var atlasTuple = PackedAtlas(mergeDatas, param.textureWidth, param.lightMapWidth);

                    EditorUtility.DisplayProgressBar(progressTitle, "正在合并网格", ((float) i + 0.3f) / aggregateList.Count);
                    var mesh = CombineMesh(mergeDatas);

                    EditorUtility.DisplayProgressBar(progressTitle, "正在简化网格", ((float) i + 0.7f) / aggregateList.Count);
                    mesh = SimplifyMesh(mesh, param.reductionSetting.triangleRatio);

                    EditorUtility.DisplayProgressBar(progressTitle, "正在创建Assets",
                        ((float) i + 0.9f) / aggregateList.Count);
                    var resultData = CreateAssets(atlasTuple.Item1, atlasTuple.Item2, mesh, param, center);

                    EditorUtility.DisplayProgressBar(progressTitle, "成功了!!!", ((float) i + 1f) / aggregateList.Count);
                    resultList.Add(resultData);
                }
                catch (Exception e)
                {
                    HLODDebug.LogError(e);
                    continue;
                }
            }

            EditorUtility.ClearProgressBar();
            return resultList;
        }

        private static Mesh SimplifyMesh(Mesh mesh, float quality)
        {
            // var meshSimplifier = new MeshSimplifier();
            // meshSimplifier.Initialize(mesh);
            // meshSimplifier.SimplifyMesh(quality);
            // var destMesh = meshSimplifier.ToMesh();
            // return destMesh;


            return mesh;
        }

        private static HLODResultData CreateAssets(TextureAtlas atlas, TextureAtlas atlas2, Mesh mesh,
            AggregateParam param, Vector3 center)
        {
            var path = HLODTool.GetRelativePath(Path.Combine(param.outputPath, param.outputName));
            var fullPath = Path.GetFullPath(path);

            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);

            var texPath = Path.Combine(fullPath, param.outputName + "_abledo.png");
            var texPath2 = Path.Combine(fullPath, param.outputName + "_lightmap.png");
            if (File.Exists(texPath))
                File.Delete(texPath);
            HLODDebug.Log("[HLOD][CutLightMap] generate: " + path);

            using (var stream = File.Create(texPath))
            {
                var bytes = atlas.atlas.EncodeToPNG();
                stream.Write(bytes, 0, bytes.Length);
            }

            AssetDatabase.ImportAsset(HLODTool.GetRelativePath(texPath));

            if (File.Exists(texPath2))
                File.Delete(texPath2);

            using (var stream = File.Create(texPath2))
            {
                var bytes = atlas2.atlas.EncodeToPNG();
                stream.Write(bytes, 0, bytes.Length);
            }

            AssetDatabase.ImportAsset(HLODTool.GetRelativePath(texPath2));

            var albedo = AssetDatabase.LoadAssetAtPath<Texture>(HLODTool.GetRelativePath(texPath));
            var lightMap = AssetDatabase.LoadAssetAtPath<Texture>(HLODTool.GetRelativePath(texPath2));

            AssetDatabase.CreateAsset(mesh, Path.Combine(path, param.outputName + ".asset"));

            var list = new List<string>()
            {
                "Athena/G_Game/HLOD Simple Lit self",
            };
            Shader shader = null;
            foreach (var shaderName in list)
            {
                shader = Shader.Find(shaderName);
                if (shader != null)
                    break;
            }

            var newMaterial = new Material(shader);
            newMaterial.SetTexture("_BaseMap", albedo);
            newMaterial.SetTexture("_HLOD_LightMap", lightMap);
            AssetDatabase.CreateAsset(newMaterial, Path.Combine(path, param.outputName + ".mat"));

            //2.创建Prefab和材质球
            var prefab = AggregateUtils.CreatePrefab(fullPath, param.outputName, center, mesh, newMaterial);
            var resultData = new HLODResultData()
            {
                param = param,
                prefab = prefab,
            };
            return resultData;
        }

        private static Mesh CombineMesh(List<MergeData> mergeDatas)
        {
            var combinedMesh = new Mesh();
            // int i = 0;
            foreach (var data in mergeDatas)
            {
                //AssetDatabase.CreateAsset(data.mesh, "assets/hlod/mesh_" + (i++) + ".asset");
                var matrix = Matrix4x4.Translate(-data.center) * data.renderer.transform.localToWorldMatrix;
                combinedMesh = AggregateUtils.CombineMesh(combinedMesh, data.mesh, matrix, data.albedoArea,
                    data.lightMapArea);
            }

            return combinedMesh;
        }

        private static (TextureAtlas, TextureAtlas) PackedAtlas(List<MergeData> mergeDatas, int abledoSize,
            int lightMapSize)
        {
            TextureAtlas albedoAtlas = new TextureAtlas();
            TextureAtlas lightMapAtlas = new TextureAtlas();
            var albedoReMap = new Dictionary<Texture, List<MergeData>>();
            var lightMapReMap = new Dictionary<Texture, List<MergeData>>();
            foreach (var data in mergeDatas)
            {
                albedoAtlas.AddTexture(data.albedo);
                lightMapAtlas.AddTexture(data.lightMap);
                if (!albedoReMap.ContainsKey(data.albedo))
                {
                    albedoReMap[data.albedo] = new List<MergeData>();
                }

                if (!lightMapReMap.ContainsKey(data.lightMap))
                {
                    lightMapReMap[data.lightMap] = new List<MergeData>();
                }

                albedoReMap[data.albedo].Add(data);
                lightMapReMap[data.lightMap].Add(data);
            }

            //打图集 
            var abledoResult = albedoAtlas.PackedAtlas(abledoSize);
            var lightMapResult = lightMapAtlas.PackedAtlas(lightMapSize);
            foreach (var pair in abledoResult)
            {
                var tex = pair.Key;
                var rect = pair.Value;
                foreach (var data in albedoReMap[tex])
                {
                    data.albedoArea = rect;
                    data.albedoAtlas = albedoAtlas;
                }
            }

            foreach (var pair in lightMapResult)
            {
                var tex = pair.Key;
                var rect = pair.Value;
                foreach (var data in lightMapReMap[tex])
                {
                    data.lightMapArea = rect;
                    data.lightMapAtlas = lightMapAtlas;
                }
            }

            //对于每一个mesh,合并一下00
            return (albedoAtlas, lightMapAtlas);
        }

        private static List<MergeData> CollectMergeDatas(List<Renderer> renderers, Vector3 center)
        {
            var list = new List<MergeData>();
            AggregateUtils.GenerateLightMapTex();
            foreach (var renderer in renderers)
            {
                var meshFilter = renderer.GetComponent<MeshFilter>();
                if (meshFilter == null)
                    continue;
                var mainMesh = meshFilter.sharedMesh;
                if (mainMesh == null)
                    continue;

                Rect uv2Area = AggregateUtils.GetUV2Area(mainMesh);
                Rect lightMapArea = AggregateUtils.GetLightMapArea(uv2Area, renderer.lightmapScaleOffset);
                var lightMap = AggregateUtils.CutLightMap(lightMapArea, renderer.lightmapIndex);
                for (int i = 0; i < mainMesh.subMeshCount; i++)
                {
                    var mesh = AggregateUtils.ConvertSubToSingleMesh(mainMesh, i);
                    var albedo = AggregateUtils.GetMainTexture(renderer.sharedMaterials[i]);
                    if (albedo == null)
                        continue;

                    list.Add(new MergeData()
                    {
                        mesh = mesh,
                        renderer = renderer,
                        albedo = albedo,
                        lightMap = lightMap,
                        center = center,
                    });
                }
            }

            return list;
        }

        private class MergeData
        {
            public Mesh mesh;
            public Renderer renderer;
            public Vector3 center;
            public Rect albedoArea;
            public Rect lightMapArea;
            public Texture2D albedo;
            public Texture2D lightMap;
            public TextureAtlas albedoAtlas;
            public TextureAtlas lightMapAtlas;
        }

        private class TextureAtlas
        {
            public List<Texture2D> texList = new List<Texture2D>();
            public Texture2D atlas;

            public void AddTexture(Texture2D tex, Vector2 size = new Vector2())
            {
                if (!texList.Contains(tex))
                {
                    texList.Add(tex);
                }
            }

            public Dictionary<Texture2D, Rect> PackedAtlas(int maxSize)
            {
                var result = new Dictionary<Texture2D, Rect>();
                atlas = new Texture2D(2, 2);
                var orignReadables = new bool[texList.Count];
                for (int i = 0; i < texList.Count; i++)
                {
                    orignReadables[i] = AggregateUtils.GetReadable(texList[i]);
                    AggregateUtils.SetReadable(texList[i], true);
                }

                var rects = atlas.PackTextures(texList.ToArray(), 0, maxSize);

                for (int i = 0; i < texList.Count; i++)
                {
                    AggregateUtils.SetReadable(texList[i], orignReadables[i]);
                    result[texList[i]] = rects[i];
                }

                return result;
            }
        }
    }
}