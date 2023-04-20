#if _HLOD_USE_ATHENA_ATLASING_
using Athena;
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Athena.MeshSimplify;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace com.pwrd.hlod.editor
{
    public static class AggregateBakedUtils
    {
        private static string m_CurInfo;
        private static float m_curProcess;
        private static string m_curTitle;
        
        public static bool IsInit { get; private set; }
        

        public static void UpdateProgressBar(string title, string info, float process)
        {
            m_curTitle = title;
            m_CurInfo = info;
            m_curProcess = process;
            var isCancel = EditorUtility.DisplayCancelableProgressBar(title, info, process);
            if (isCancel)
            {
                EditorUtility.ClearProgressBar();
                throw new Exception("取消hlod生成");
            }
        }

        public static void Init()
        {
            IsInit = false;
            
            SimplygonBridge.SetLogCallback((info) =>
            {
                UpdateProgressBar(m_curTitle, m_CurInfo + " " + info, m_curProcess);

                LogInfo("[SimplygonTooDll]" + info);
            });

            IsInit = SimplygonBridge.InitSDK();

            if (!IsInit)
            {
                //EditorUtility.DisplayDialog("", "Simplygon 初始化失败", "确定");
                EditorUtility.ClearProgressBar();
                throw new Exception("Simplygon Init Failed.");
            }

            LogInfo("DllBridge InitSDK");
        }

        public static void DeInit()
        {
            IsInit = false;
            SimplygonBridge.DeInitSDK();
            LogInfo("DllBridge DeInitSDK");
        }

        public static (string, string, string, string) BakeTextures(Renderer renderer, string ouputPath, AggregateParam param)
        {
            //烘焙贴图
            var baker = new RendererBaker();
            baker.SetUseHighRendererLightmap(param.useHighRendererLightmap);
            baker.SetUseLODIndex(param.useLODIndex);
            baker.SetShaderBindConfigData(param.shaderBindConfig);
            baker.SetRendererBakerSetting(param.rendererBakerSetting);
            baker.SetDecalData(param.decalTagList);
            baker.Init(renderer);
            var albedoTex = baker.Bake(renderer, TextureChannel.Albedo, param.useAlphaTest);
            Texture2D normalTex = null, metallicTex = null, lightmapTex = null;
            if ((param.textureChannel & TextureChannel.Normal) != 0)
                normalTex = baker.Bake(renderer, TextureChannel.Normal);
            if ((param.textureChannel & TextureChannel.Metallic) != 0)
                metallicTex = baker.Bake(renderer, TextureChannel.Metallic);
            if ((param.textureChannel & TextureChannel.LightMap) != 0)
                lightmapTex = baker.Bake(renderer, TextureChannel.LightMap);
            baker.Release();

            var name = renderer.name;

            //创建贴图文件
            var albedoTexFilePath = WriteTexture(albedoTex, ouputPath, name, TextureChannel.Albedo);
            var normalTexFilePath = WriteTexture(normalTex, ouputPath, name, TextureChannel.Normal);
            var metallicTexFilePath = WriteTexture(metallicTex, ouputPath, name, TextureChannel.Metallic);
            var lightmapTexFilePath = WriteTexture(lightmapTex, ouputPath, name, TextureChannel.LightMap);
            
            return (albedoTexFilePath, normalTexFilePath, metallicTexFilePath, lightmapTexFilePath);
        }

        public static string WriteTexture(Texture2D tex, string ouputPath, string name, TextureChannel channel)
        {
            string path = "";
            if (tex)
            {
                var lightmapTexBytes = tex.EncodeToPNG();
                path = Path.Combine(ouputPath, name + "_" + channel.ToString() +"_" + DateTime.Now.ToFileTime() + ".png");
                if (File.Exists(path)) File.Delete(path);
                using (var stream = File.Create(path))
                {
                    stream.Write(lightmapTexBytes, 0, lightmapTexBytes.Length);
                }
                GameObject.DestroyImmediate(tex);
            }
            return path;
        }

        public static HLODResultData CopyAssetIntoProject(string path, AggregateParam param, Vector3 center, bool flipX = false)
        {
            var targetFolder = Path.Combine(param.outputPath, param.outputName);
            if (Directory.Exists(targetFolder))
                Directory.Delete(targetFolder, true);
            Directory.CreateDirectory(targetFolder);

            var objName = "aggregate.obj";
            var albedoName = "albedo.png";
            var normalName = "normal.png";
            var metallicName = "metallic.png";
            var occlusionName = "occlusion.png";
            var lightmapName = "lightmap.png";
            var copyList = new List<string>()
            {
                objName,
                albedoName,
                normalName,
                metallicName,
                occlusionName,
                lightmapName,
            };
            var assetNameMap = new Dictionary<string, string>()
            {
                {objName, param.outputName + ".obj"},
                {albedoName, param.outputName + "_albedo.png"},
                {normalName, param.outputName + "_normal.png"},
                {metallicName, param.outputName + "_metallic.png"},
                {occlusionName, param.outputName + "_occlusion.png"},
                {lightmapName, param.outputName + "_lightmap.png"},
            };
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file).ToLower();
                if (copyList.Contains(fileName))
                {
                    var targetPath = Path.Combine(targetFolder, assetNameMap[fileName]);
                    var relativePath = GetRelativePath(targetPath);
                    File.Copy(file, targetPath);
                    try
                    {
                        AssetDatabase.ImportAsset(relativePath);
                    }
                    catch
                    {
                    }
                }
            }

            //Load Mesh
            var objPath = GetRelativePath(Path.Combine(targetFolder, assetNameMap[objName]));
            var assets = AssetDatabase.LoadAllAssetsAtPath(objPath);
            Mesh mesh = null;
            foreach (var asset in assets)
            {
                if (asset is Mesh)
                {
                    mesh = asset as Mesh;
                    break;
                }
            }
            
            //平滑法线
            mesh = GameObject.Instantiate(mesh);
            if (param.smoothNormals) ShadowBiasBaker.SmoothNormal(mesh);
            
            //压缩mesh（只保留顶点和法线）
            bool isVertexCompression = false;
#if HLOD_USE_ATHENA_PIPELINE
            if (param.useMeshReduction && param.enableMeshOptimation)
            {
                mesh = Athena.MeshOptimizeTool.GetOptimizeMesh(mesh, Athena.MeshOptimizeTool.OptimizeType.CompressPosNormalTo6Channel);
                isVertexCompression = true;
            }
#endif
            var meshPath = objPath.Replace(".obj", ".asset");
            AssetDatabase.CreateAsset(mesh, meshPath);
            File.Delete((objPath));

            //创建材质球
            var list = new List<string>()
            {
#if HLOD_USE_URP
                "Athena/G_Game/HLOD Unlit",
                "Universal Render Pipeline/Unlit",
#else
                "Unlit/Texture",
#endif
            };
            Shader shader = null;
            foreach (var shaderName in list)
            {
                shader = Shader.Find(shaderName);
                if (shader != null)
                    break;
            }

            if (param.targetShader)
            {
                shader = param.targetShader;
            }

            var textureHeight = param.useMeshReduction ? param.textureHeight : 1024;
            var textureWidth = param.useMeshReduction ? param.textureWidth : 1024;
            var albedo = ImportTexture(targetFolder, assetNameMap, albedoName, textureWidth, textureHeight);
            var normal = ImportTexture(targetFolder, assetNameMap, normalName, textureWidth, textureHeight);
            var metallic = ImportTexture(targetFolder, assetNameMap, metallicName, textureWidth, textureHeight);
            var occlusion = ImportTexture(targetFolder, assetNameMap, occlusionName, textureWidth, textureHeight);
            var lightmap = ImportTexture(targetFolder, assetNameMap, lightmapName, textureWidth, textureHeight);
            
            var newMaterial = new Material(shader);
            if (param.useMeshReduction && param.enableMeshOptimation && isVertexCompression)
            {
                newMaterial.SetInt("_VertexCompression", 2);
            }
            newMaterial.SetTexture("_BaseMap", albedo);
            newMaterial.SetColor("_BaseColor", Color.white);
            if (newMaterial.HasProperty("_BumpMap")) newMaterial.SetTexture("_BumpMap", normal);
            if (newMaterial.HasProperty("_NormalMap")) newMaterial.SetTexture("_NormalMap", normal);
            newMaterial.SetTexture("_MetallicGlossMap", metallic);
            newMaterial.SetFloat("_Metallic", 0);
            newMaterial.SetFloat("_Smoothness", 0);
            newMaterial.SetTexture("_OcclusionMap", occlusion);
            newMaterial.SetTexture("_LightMap", lightmap);
            
            if (param.fixMaterialQueue)
            {
                newMaterial.renderQueue = param.materialQueue;
            }
            if (param.targetMaterialPropertiesAction != null)
            {
                param.targetMaterialPropertiesAction?.Invoke(newMaterial, param);
            }
            else
            {
                newMaterial.SetFloat("_SelfNormalBias", 0.02f);
                newMaterial.SetFloat("_SelfDepthBias", 0.1f);
                if (param.useAlphaTest)
                {
                    newMaterial.EnableKeyword("_ALPHATEST_ON");
                    newMaterial.SetFloat("_AlphaClip", 1);
                }
            }

            AssetDatabase.CreateAsset(newMaterial, GetRelativePath(Path.Combine(targetFolder, param.outputName + ".mat")));

            //2.创建Prefab和材质球
            var prefab = CreatePrefab(targetFolder, param.outputName, center, mesh, newMaterial, flipX);
            var resultData = new HLODResultData()
            {
                param = param,
                prefab = prefab, 
            };
            return resultData;
        }

        public static HLODResultData ImportAssetIntoProject(string path, AggregateParam param, Vector3 center)
        {
            var targetFolder = Path.Combine(param.outputPath, param.outputName);

            var objName = param.outputName + ".fbx";
            var albedoName = param.outputName + "_diffuse.tga";
            var normalName = param.outputName + "_normal.tga";
            var metallicName = param.outputName + "_metallic.tga";
            var occlusionName = param.outputName + "_occlusion.tga";
            var lightmapName = param.outputName + "_lightmap.tga";
            var copyList = new List<string>()
            {
                objName,
                albedoName,
                normalName,
                metallicName,
                occlusionName,
                lightmapName,
            };


            //Load Mesh
            var objPath = GetRelativePath(Path.Combine(targetFolder, objName));
            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(objPath);
            mesh = GameObject.Instantiate(mesh);
            if (param.smoothNormals)
            {
                ShadowBiasBaker.SmoothNormal(mesh);
            }
            else
            {
                mesh.RecalculateNormals(60);
            }
            
            //压缩mesh（只保留顶点和法线）
            bool isVertexCompression = false;
#if HLOD_USE_ATHENA_PIPELINE
            if (param.useMeshReduction && param.enableMeshOptimation)
            {
                mesh = Athena.MeshOptimizeTool.GetOptimizeMesh(mesh, Athena.MeshOptimizeTool.OptimizeType.CompressPosNormalTo6Channel);
                isVertexCompression = true;
            }
#endif
            var meshPath = objPath.Replace(".fbx", ".asset");
            File.Delete((objPath));
            AssetDatabase.CreateAsset(mesh, meshPath);

            //创建材质球
            var list = new List<string>()
            {
#if HLOD_USE_URP
                "Athena/G_Game/HLOD Unlit",
                "Universal Render Pipeline/Unlit",
#else
                "Unlit/Texture",
#endif            
            };
            Shader shader = null;
            foreach (var shaderName in list)
            {
                shader = Shader.Find(shaderName);
                if (shader != null)
                    break;
            }

            if (param.targetShader)
            {
                shader = param.targetShader;
            }

            var textureHeight = param.useMeshReduction ? param.textureHeight : 1024;
            var textureWidth = param.useMeshReduction ? param.textureWidth : 1024;
            var albedo = ImportTexture(targetFolder, albedoName, textureWidth, textureHeight);
            var normal = ImportTexture(targetFolder, normalName, textureWidth, textureHeight);
            var metallic = ImportTexture(targetFolder, metallicName, textureWidth, textureHeight);
            var occlusion = ImportTexture(targetFolder, occlusionName, textureWidth, textureHeight);
            var lightmap = ImportTexture(targetFolder, lightmapName, textureWidth, textureHeight);
            
            var newMaterial = new Material(shader);
            if (param.useMeshReduction && param.enableMeshOptimation && isVertexCompression)
            {
                newMaterial.SetInt("_VertexCompression", 2);
            }
            newMaterial.SetTexture("_BaseMap", albedo);
            if(newMaterial.HasProperty("_MainTex"))
                newMaterial.SetTexture("_MainTex", albedo);
            newMaterial.SetColor("_BaseColor", Color.white);
            if (newMaterial.HasProperty("_BumpMap")) newMaterial.SetTexture("_BumpMap", normal);
            if (newMaterial.HasProperty("_NormalMap")) newMaterial.SetTexture("_NormalMap", normal);
            newMaterial.SetTexture("_MetallicGlossMap", metallic);
            newMaterial.SetFloat("_Metallic", 0);
            newMaterial.SetFloat("_Smoothness", 0);
            newMaterial.SetTexture("_OcclusionMap", occlusion);
            newMaterial.SetTexture("_LightMap", lightmap);
            
            if (param.fixMaterialQueue)
            {
                newMaterial.renderQueue = param.materialQueue;
            }
            if (param.targetMaterialPropertiesAction != null)
            {
                param.targetMaterialPropertiesAction?.Invoke(newMaterial, param);
            }
            else
            {
                newMaterial.SetFloat("_SelfNormalBias", 0.02f);
                newMaterial.SetFloat("_SelfDepthBias", 0.1f);
                if (param.useAlphaTest)
                {
                    newMaterial.EnableKeyword("_ALPHATEST_ON");
                    newMaterial.SetFloat("_AlphaClip", 1);
                }
            }

            AssetDatabase.CreateAsset(newMaterial, GetRelativePath(Path.Combine(targetFolder, param.outputName + ".mat")));

            //2.创建Prefab和材质球
            var prefab = CreatePrefab(targetFolder, param.outputName, center, mesh, newMaterial, false);
            var resultData = new HLODResultData()
            {
                param = param,
                prefab = prefab, 
            };
            return resultData;
        }

        private static Texture ImportTexture(string targetFolder, string mapName, int textureWidth, int textureHeight)
        {
            var relativePath = GetRelativePath(Path.Combine(targetFolder, mapName));
            AssetDatabase.ImportAsset(relativePath);
            var map = AssetDatabase.LoadAssetAtPath<Texture>(relativePath);
            if (map)
            {
                var mapPath = AssetDatabase.GetAssetPath(map);
#if _HLOD_USE_ATHENA_ATLASING_
            ImportTexture(mapPath, Mathf.Max(textureWidth, textureHeight), TextureUtility.TexturePlatformType.TPT_ALBEDO);
#else
                AssetDatabase.ImportAsset(mapPath);
                ImportTexture(mapPath, Mathf.Max(textureWidth, textureHeight));
            }
            return map;
#endif
        }
        
        private static Texture ImportTexture(string targetFolder, Dictionary<string, string> assetNameMap, string mapName, int textureWidth, int textureHeight)
        {
            var map = AssetDatabase.LoadAssetAtPath<Texture>(GetRelativePath(Path.Combine(targetFolder, assetNameMap[mapName])));
            if (map)
            {
                var mapPath = AssetDatabase.GetAssetPath(map);
#if _HLOD_USE_ATHENA_ATLASING_
            ImportTexture(mapPath, Mathf.Max(textureWidth, textureHeight), TextureUtility.TexturePlatformType.TPT_ALBEDO);
#else
                ImportTexture(mapPath, Mathf.Max(textureWidth, textureHeight));
            }
            return map;
#endif
        }
        
#if _HLOD_USE_ATHENA_ATLASING_
        public static void ImportTexture(string path, int maxSize, TextureUtility.TexturePlatformType texType = TextureUtility.TexturePlatformType.TPT_ALBEDO)
        {
            var albedoImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (albedoImporter != null)
            {
                albedoImporter.maxTextureSize = maxSize;
                TextureUtility.SetupTextureSettings(albedoImporter, texType);
                albedoImporter.SaveAndReimport();
            }
        }
#else
        public static void ImportTexture(string path, int maxSize)
        {
            var albedoImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            if (albedoImporter != null)
            {
                albedoImporter.maxTextureSize = maxSize;

                try
                {
                    var androidSettings = albedoImporter.GetPlatformTextureSettings("Android");
                    androidSettings.overridden = true;
#if UNITY_2020_1_OR_NEWER
                    androidSettings.format = TextureImporterFormat.ASTC_6x6;
#else
                    androidSettings.format = TextureImporterFormat.ASTC_RGBA_6x6;
#endif
                    albedoImporter.SetPlatformTextureSettings(androidSettings);
                }
                catch { }

                try
                {
                    var iOSSettings = albedoImporter.GetPlatformTextureSettings("iPhone");
                    iOSSettings.overridden = true;
#if UNITY_2020_1_OR_NEWER
                    iOSSettings.format = TextureImporterFormat.ASTC_6x6;
#else
                    iOSSettings.format = TextureImporterFormat.ASTC_RGBA_6x6;
#endif
                    albedoImporter.SetPlatformTextureSettings(iOSSettings);
                }
                catch { }

                albedoImporter.SaveAndReimport();
            }
        }
#endif

        private static void LogInfo(string info)
        {
            HLODDebug.Log(info);
        }

        public static string GetRelativePath(string fullPath)
        {
            return fullPath.Substring(Application.dataPath.Length - 6);
        }

        public static GameObject CreatePrefab(string path, string name, Vector3 center, Mesh mesh, Material mat, bool flipX = false)
        {
            var targetPath = GetRelativePath(Path.Combine(path, name));
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var go = new GameObject();
            var meshGo = new GameObject();
            meshGo.name = "AggregateMesh";

            var filter = meshGo.AddComponent<MeshFilter>();
            var renderer = meshGo.AddComponent<MeshRenderer>();
            filter.sharedMesh = mesh;
            renderer.sharedMaterial = mat;
            
            go.transform.position = Vector3.zero;
            meshGo.transform.parent = go.transform;
            meshGo.transform.position = center;

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, targetPath + ".prefab");
            GameObject.DestroyImmediate(go);
            return prefab;

        }

        public static Vector3 GetBoundsCenter(List<Renderer> renderers)
        {
            var bounds = new Bounds();
            
            if (renderers.Count > 0)
            {
                bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Count; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
            }

            return bounds.center;
        }

        public static Mesh ConvertSubTOSingleMesh(Mesh mesh, int subIndex)
        {
            LogInfo("[HLOD][ConvertSubTOSingleMesh]");

            if (mesh.subMeshCount == 1)
                return mesh;

            int preCount = 0;
            for (int i = 0; i < subIndex; i++)
            {
                var curDec = mesh.GetSubMesh(i);
                if (curDec.topology == MeshTopology.Triangles)
                {
                    preCount += curDec.indexCount;
                }

                if (curDec.topology == MeshTopology.Quads)
                {
                    preCount += curDec.indexCount / 4 * 6;
                }
            }

            int indiceStart = preCount;
            var subDec = mesh.GetSubMesh(subIndex);
            int indiceCount = (subDec.topology == MeshTopology.Quads) ? (subDec.indexCount / 4 * 6) : subDec.indexCount;

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normal = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<Vector2> uv2 = new List<Vector2>();
            List<Vector4> tangents = new List<Vector4>();
            
            var meshVertices = mesh.vertices;
            var meshTriangles = mesh.triangles;
            var meshNormals = mesh.normals;
            var meshUV = mesh.uv;
            var meshUV2 = mesh.uv2;
            var meshTangents = mesh.tangents;
            
            int[] indices = null;
            var oldTriangles = new List<int>(meshTriangles).GetRange(indiceStart, indiceCount);
            indices = new int[indiceCount];
            Dictionary<int, int> map = new Dictionary<int, int>();
            for (int i = 0; i < oldTriangles.Count; i++)
            {
                var oldIndex = oldTriangles[i];
                if (!map.ContainsKey(oldIndex))
                {
                    map[oldIndex] = vertices.Count;
                    vertices.Add(meshVertices[oldIndex]);
                    normal.Add(meshNormals[oldIndex]);
                    tangents.Add(meshTangents[oldIndex]);
                    uv.Add(meshUV[oldIndex]);
                    if (oldIndex < meshUV2.Length)
                        uv2.Add(meshUV2[oldIndex]);
                }

                indices[i] = map[oldIndex];
            }

            var newMesh = new Mesh();
            newMesh.vertices = vertices.ToArray();
            newMesh.normals = normal.ToArray();
            newMesh.uv = uv.ToArray();
            newMesh.uv2 = uv2.ToArray();
            newMesh.tangents = tangents.ToArray();
            newMesh.triangles = indices;
            return newMesh;
        }


        public static void AddMeshNode(string name, Matrix4x4 matrix, Mesh mesh, int subIndex, bool flipX = false)
        {
            var subMesh = ConvertSubTOSingleMesh(mesh, subIndex);
            

            var meshVertices = subMesh.vertices;
            var meshNormals = subMesh.normals;
            var meshUV = subMesh.uv;
            var meshUV2 = subMesh.uv2;
            var meshTriangles = subMesh.triangles;
            
            int vertexCount = meshVertices.Length;
            int triangleCount = meshTriangles.Length / 3;
            int[] triangleArr = null;
            if (flipX)
            {
                triangleArr = meshTriangles;
            }
            else
            {
                triangleArr = new int[meshTriangles.Length];
                for (int i = 0; i < meshTriangles.Length / 3; i++)//估计是坐标系的问题，所以反转一下三角形顺序
                {
                    triangleArr[i * 3 + 0] = meshTriangles[i * 3 + 0];
                    triangleArr[i * 3 + 1] = meshTriangles[i * 3 + 2];
                    triangleArr[i * 3 + 2] = meshTriangles[i * 3 + 1];
                }
            }
            
            float[] vertexArr = Convert(meshVertices, matrix);
            var matrixArr = GetRelativeMatrix(null, Vector3.zero, flipX); //估计是坐标系的问题，所以反转一下三角形顺序，这里面反转了x
            var uvList = new List<Vector2>();
            var uvList2 = new List<Vector2>();
            var normalList = new List<Vector3>();
            var subMeshUV2 =meshUV2.Length == 0 ?meshUV : meshUV2;
            
            foreach (var index in triangleArr)
            {
                uvList.Add(meshUV[index]);
                uvList2.Add(subMeshUV2[index]);

                //计算法线在世界坐标系的方向.
                var point = meshVertices[index];
                Vector3 worldPoint = matrix * new Vector4(point.x, point.y, point.z, 1);
                var point2 = point + meshNormals[index];
                Vector3 worldPoint2 = matrix * new Vector4(point2.x, point2.y, point2.z, 1);
                Vector3 worldNormal = (worldPoint2 - worldPoint).normalized;
                normalList.Add(worldNormal);
            }

            if (subMesh != null && mesh != subMesh)
                GameObject.DestroyImmediate(subMesh);
            
            float[] uvArr = Convert(uvList);
            float[] uvArr2 = Convert(uvList2);
            float[] normalArr = Convert(normalList);

            SimplygonBridge.AddMeshNode(name, matrixArr, vertexCount, vertexArr, triangleCount, triangleArr, uvArr, uvArr2,
                normalArr);
        }

        public static void SetAssetReadable(Object obj, bool value)
        {
            var so = new SerializedObject(obj);
            var property = so.FindProperty("m_IsReadable");
            property.boolValue = value;
            so.ApplyModifiedProperties();
        }

        public static bool GetAssetReadable(Object obj)
        {
            var so = new SerializedObject(obj);
            var property = so.FindProperty("m_IsReadable");
            return property.boolValue;
        }
        
        public static Mesh GetMesh(Renderer cur, bool handleReverse = true)
        {
            Mesh mesh = null;
            if (cur is MeshRenderer)
            {
                var meshFilter = cur.transform.GetComponent<MeshFilter>();
                //缩放为-1时，先反面操作，再由合成工具通过矩阵反面回来
                if (handleReverse && meshFilter.transform.lossyScale.x * meshFilter.transform.lossyScale.y * meshFilter.transform.lossyScale.z < 0)
                    mesh = MeshReverse(meshFilter.sharedMesh);
                else
                    mesh = meshFilter.sharedMesh;
            }

            if (cur is SkinnedMeshRenderer)
            {
                var skinRender = cur as SkinnedMeshRenderer;
                mesh = skinRender.sharedMesh;
            }

            return mesh;
        }
        
        public static Mesh GetMeshReverseVert(Renderer cur)
        {
            Mesh mesh = null;
            if (cur is MeshRenderer)
            {
                var meshFilter = cur.transform.GetComponent<MeshFilter>();
                //缩放为-1时，先反面操作，再由合成工具通过矩阵反面回来
                if (meshFilter.transform.lossyScale.x < 0 || meshFilter.transform.lossyScale.y < 0 || meshFilter.transform.lossyScale.z < 0)
                {
                    Vector3Int rev = Vector3Int.one;
                    rev.x = meshFilter.transform.lossyScale.x > 0 ? 1 : -1;
                    rev.y = meshFilter.transform.lossyScale.y > 0 ? 1 : -1;
                    rev.z = meshFilter.transform.lossyScale.z > 0 ? 1 : -1;

                    Mesh newMesh = new Mesh();
                    Mesh omesh = meshFilter.sharedMesh;
                    Vector3[] verts = omesh.vertices;
                    for (int i = 0; i < verts.Length; i++)
                    {
                        verts[i] = new Vector3(verts[i].x * rev.x, verts[i].y * rev.y, verts[i].z * rev.z);
                    }

                    newMesh.vertices = verts;
                    newMesh.uv = omesh.uv;
                    newMesh.uv2 = omesh.uv2;
                    newMesh.colors = omesh.colors;
                    newMesh.bounds = omesh.bounds;
                    if (rev.x * rev.y * rev.z < 0)
                    {
                        var triangles = omesh.triangles.ToList();
                        triangles.Reverse();
                        newMesh.triangles = triangles.ToArray();
                    }
                    else
                    {
                        newMesh.triangles = omesh.triangles;
                    }
            
                    newMesh.RecalculateNormals();
                    newMesh.RecalculateTangents();
                    mesh = newMesh;
                }
                else
                    mesh = meshFilter.sharedMesh;
            }

            if (cur is SkinnedMeshRenderer)
            {
                var skinRender = cur as SkinnedMeshRenderer;
                mesh = skinRender.sharedMesh;
            }

            return mesh;
        }
        
        /// <summary>
        /// 反面操作
        /// </summary>
        private static Mesh MeshReverse(Mesh mesh)
        {
            Mesh newMesh = new Mesh();
            newMesh.vertices = mesh.vertices;
            newMesh.uv = mesh.uv;
            newMesh.uv2 = mesh.uv2;
            newMesh.colors = mesh.colors;
            newMesh.bounds = mesh.bounds;
            var triangles = mesh.triangles.ToList();
            triangles.Reverse();
            newMesh.triangles = triangles.ToArray();
            
            newMesh.RecalculateNormals();
            newMesh.RecalculateTangents();
            return newMesh;
        }

        public static string GetTempOutputFolder()
        {
            var time = DateTime.Now.ToString("MMdd_HHmmss_fff");
            var path = Path.Combine(Application.dataPath, "../Temp/HLOD/" + time);
            path += "\\";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        public static string GetTempOutputFolderRoot()
        {
            var path = Path.Combine(Application.dataPath, "../Temp/HLOD/");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }
        
        public static void AddTextureMaterail(string name, string path, string lightmapPath)
        {
            if (string.IsNullOrEmpty(lightmapPath))
            {
                SimplygonBridge.AddTextureMaterial(name, path);
            }
            else
            {
                SimplygonBridge.AddTextureMaterial(name, path, lightmapPath);
            }
        }
        
        public static void AddTextureMaterialByChannel(string name, string path, TextureChannel channel)
        {
            SimplygonBridge.AddTextureMaterialByChannel(name, path, channel);
        }

        public static float[] GetRelativeMatrix(Transform cur, Vector3 parentPos, bool flipX = false)
        {
            return new float[]
            {
                (flipX ? 1 : -1), 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1,
            };
        }

        public static void LodMatrixArr(float[] arr)
        {
            var sb = new StringBuilder();
            sb.AppendLine("meshMatirx:");

            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    sb.Append(arr[x + y * 4]).Append(" ");
                }

                sb.AppendLine();
            }

            LogInfo("[LodMatrixArr]" + sb);
        }
        //return matrix;

        static float[] Convert(Vector3[] values)
        {
            var list = new List<float>();
            foreach (var v in values)
            {
                list.Add(v.x);
                list.Add(v.y);
                list.Add(v.z);
            }

            return list.ToArray();
        }

        static float[] Convert(Vector3[] values, Matrix4x4 matrix)
        {
            var list = new List<float>();
            foreach (var v in values)
            {
                var pos = matrix * new Vector4(v.x, v.y, v.z, 1);
                list.Add(pos.x);
                list.Add(pos.y);
                list.Add(pos.z);
            }

            return list.ToArray();
        }

        static float[] Convert(List<Vector4> values)
        {
            var list = new List<float>();
            foreach (var v in values)
            {
                list.Add(v.x);
                list.Add(v.y);
                list.Add(v.z);
                list.Add(v.w);
            }

            return list.ToArray();
        }

        static float[] Convert(List<Vector3> values)
        {
            var list = new List<float>();
            foreach (var v in values)
            {
                list.Add(v.x);
                list.Add(v.y);
                list.Add(v.z);
            }

            return list.ToArray();
        }

        static float[] Convert(List<Vector2> values)
        {
            var list = new List<float>();
            foreach (var v in values)
            {
                list.Add(v.x);
                list.Add(v.y);
            }

            return list.ToArray();
        }

        static float[] Convert(List<Color> values)
        {
            var list = new List<float>();
            foreach (var v in values)
            {
                list.Add(v.r);
                list.Add(v.g);
                list.Add(v.b);
                list.Add(v.a);
            }

            return list.ToArray();
        }
    }
}