#if  _HLOD_USE_ATHENA_ATLASING_
using Athena;
#endif

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using static UnityEditor.EditorApplication;
using Object = UnityEngine.Object;

namespace com.pwrd.hlod.editor
{
    public static class HLODHelper
    {
        private static List<Texture2D> m_lightMapTex;

        public static HLODResultData RunAggregate(List<Renderer> renderers, AggregateParam param)
        {
            var list = RunAggregate(new List<(List<Renderer>, AggregateParam)>() {(renderers, param)});
            return list.First();
        }

        // static int fakeProgress = 0, fakeProgressMax = 50;
        // private static int listCount;
        // private static int listIndex;

        public static List<HLODResultData> RunAggregate(List<(List<Renderer>, AggregateParam)> aggregateList)
        {
            var resultList = new List<HLODResultData>();
            if (!AggregateBakedUtils.IsInit)
                return resultList;

            GenerateLightMapTex();
            foreach (var tuple in aggregateList)
            {
                var list = tuple.Item1;
                var param = tuple.Item2;
                
                // fakeProgress = 0;
                // listIndex++;
                
                var outputPath = GetTempOutputFolder();
                SimplygonBridge.SetTempFolderPath(outputPath);
                SimplygonBridge.CreateNewScene();
                //计算包围盒中心.
                var center = GetBoundsCenter(list);
                TransportData(list, center);
                if (param.useOcclusion)
                {
                    SimplygonBridge.RunReductionByScreenSizeUseOcclusion((int)param.reductionSetting.screenSize, param.camPitchAngle,
                        param.camYawAngle, param.camCoverage);
                }
                else
                {
                    SimplygonBridge.RunReductionByScreenSize((int)param.reductionSetting.screenSize);
                }

                SimplygonBridge.RunAggregate(param.textureWidth, param.textureHeight, param.textureChannel);
                SimplygonBridge.ExportScene(outputPath + "Aggregate.obj");
                HLODDebug.Log("[HLOD] outPath:" + outputPath);


                //1.把输出结果拷贝进来,并创建prefab
                var result = CopyAssetIntoProject(outputPath, param, center);
                resultList.Add(result);
            }

            EditorUtility.DisplayProgressBar("Simplygon Running...", "Done", 1);
            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
            return resultList;
        }

        private static void GenerateLightMapTex()
        {
            //通过shader解码  lightmap
            m_lightMapTex = new List<Texture2D>();
            var outPath = GetTempOutputFolderRoot();
            for (int i = 0; i < LightmapSettings.lightmaps.Length; i++)
            {
                var lightmap = LightmapSettings.lightmaps[i];
                var tex = lightmap.lightmapColor;
                var shadowMask = lightmap.shadowMask;
                var dst = RenderTexture.GetTemporary(tex.width, tex.height);
                var mat = new Material(Shader.Find("Athena/G_Game/HLODLightMap"));
                mat.SetTexture("LightMap", tex);
                mat.SetTexture("ShadowMask", shadowMask);
                Graphics.Blit(dst, dst, mat);
                var dstTex = new Texture2D(tex.height, tex.height);
                RenderTexture.active = dst;
                dstTex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0, false);
                dstTex.Apply();
                RenderTexture.ReleaseTemporary(dst);

                var bytes = dstTex.EncodeToPNG();
                var path = Path.Combine(outPath, "lightmap_" + i + ".png");
                if (File.Exists(path))
                    File.Delete(path);

                using (var stream = File.Create(path))
                {
                    stream.Write(bytes, 0, bytes.Length);
                }

                m_lightMapTex.Add(dstTex);
            }
        }

        private static HLODResultData CopyAssetIntoProject(string path, AggregateParam param, Vector3 center)
        {
            var targetFolder = Path.Combine(param.outputPath, param.outputName);
            if (Directory.Exists(targetFolder))
                Directory.Delete(targetFolder, true);
            Directory.CreateDirectory(targetFolder);


            var objName = "aggregate.obj";
            var albedoName = "albedo.png";
            var lightMapName = "lightmap.png";
            var copyList = new List<string>()
            {
                objName,
                albedoName,
                lightMapName,
            };
            var assetNameMap = new Dictionary<string, string>()
            {
                {objName, param.outputName + ".obj"},
                {albedoName, param.outputName + "_albedo.png"},
                {lightMapName, param.outputName + "_lightmap.png"},
            };
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file).ToLower();
                if (copyList.Contains(fileName))
                {
                    var targetPath = Path.Combine(targetFolder, assetNameMap[fileName]);
                    var relativePath = HLODTool.GetRelativePath(targetPath);
                    File.Copy(file, targetPath);
                    AssetDatabase.ImportAsset(relativePath);
                }
            }

            //Load Mesh
            var objPath = HLODTool.GetRelativePath(Path.Combine(targetFolder, assetNameMap[objName]));
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

            //创建材质球
            var list = new List<string>()
            {
                "Athena/G_Game/HLOD Simple Lit",
            };
            Shader shader = null;
            foreach (var shaderName in list)
            {
                shader = Shader.Find(shaderName);
                if (shader != null)
                    break;
            }

            var albedo =
                AssetDatabase.LoadAssetAtPath<Texture>(HLODTool.GetRelativePath(Path.Combine(targetFolder, assetNameMap[albedoName])));
            var lightMap =
                AssetDatabase.LoadAssetAtPath<Texture>(HLODTool.GetRelativePath(Path.Combine(targetFolder, assetNameMap[lightMapName])));
 			var albedoImporter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(albedo)) as TextureImporter;
            if (albedoImporter != null)
            {
                albedoImporter.maxTextureSize = Mathf.Max(param.textureHeight, param.textureWidth);
#if  _HLOD_USE_ATHENA_ATLASING_
                TextureUtility.SetupTextureSettings(albedoImporter, TextureUtility.TexturePlatformType.TPT_ALBEDO);
#endif
                albedoImporter.SaveAndReimport();
            }
            var lightMapImopter = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(lightMap)) as TextureImporter;
            if (lightMapImopter != null)
            {
                lightMapImopter.sRGBTexture = true;
                lightMapImopter.maxTextureSize = Mathf.Max(param.lightMapHeight, param.lightMapWidth);
#if  _HLOD_USE_ATHENA_ATLASING_
                TextureUtility.SetupTextureSettings(lightMapImopter, TextureUtility.TexturePlatformType.TPT_LIGHTMAP);
#endif
                lightMapImopter.SaveAndReimport();
            }

            var newMaterial = new Material(shader);
            newMaterial.SetTexture("_BaseMap", albedo);
            newMaterial.SetTexture("_HLOD_LightMap", lightMap);
            AssetDatabase.CreateAsset(newMaterial,
                HLODTool.GetRelativePath(Path.Combine(targetFolder, param.outputName + ".mat")));

            //2.创建Prefab和材质球
            var prefab = CreatePrefab(targetFolder, param.outputName, center, mesh, newMaterial);
            var resultData = new HLODResultData()
            {
                param = param,
                prefab = prefab,
            };
            return resultData;
        }

        private static GameObject CreatePrefab(string path, string name, Vector3 center, Mesh mesh, Material mat)
        {
            var targetPath = HLODTool.GetRelativePath(Path.Combine(path, name));
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            var go = new GameObject();
            var secondGo = new GameObject();
            secondGo.transform.parent = go.transform;
            secondGo.transform.position = center;
            //secondGo.transform.position = Vector3.zero;
            secondGo.transform.localScale = new Vector3(-1, 1, 1);
            secondGo.name = "Pivot";
            var meshGo = new GameObject();
            meshGo.name = "AggregateMesh";
            meshGo.transform.parent = secondGo.transform;
            meshGo.transform.localPosition = Vector3.zero;
            meshGo.transform.localScale = Vector3.one;

            var filter = meshGo.AddComponent<MeshFilter>();
            var renderer = meshGo.AddComponent<MeshRenderer>();
            filter.sharedMesh = mesh;
            renderer.sharedMaterial = mat;

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, targetPath + ".prefab");
            GameObject.DestroyImmediate(go);
            return prefab;
        }

        private static Vector3 GetBoundsCenter(List<Renderer> renderers)
        {
            var bounds = new Bounds();

            foreach (var renderer in renderers)
            {
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds.center;
        }

        private static Mesh ConvertSubTOSingleMesh(Mesh mesh, int subIndex)
        {
            HLODDebug.Log("[HLOD][ConvertSubTOSingleMesh]");

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

            List<Vector3> vectices = new List<Vector3>();
            List<Vector3> normal = new List<Vector3>();
            List<Vector2> uv = new List<Vector2>();
            List<Vector2> uv2 = new List<Vector2>();
            List<Vector4> tangents = new List<Vector4>();
            int[] indices = null;
            var oldTriangles = new List<int>(mesh.triangles).GetRange(indiceStart, indiceCount);
            indices = new int[indiceCount];
            Dictionary<int, int> map = new Dictionary<int, int>();
            for (int i = 0; i < oldTriangles.Count; i++)
            {
                var oldIndex = oldTriangles[i];
                if (!map.ContainsKey(oldIndex))
                {
                    map[oldIndex] = vectices.Count;
                    vectices.Add(mesh.vertices[oldIndex]);
                    normal.Add(mesh.normals[oldIndex]);
                    tangents.Add(mesh.tangents[oldIndex]);
                    uv.Add(mesh.uv[oldIndex]);
                    if (oldIndex < mesh.uv2.Length)
                        uv2.Add(mesh.uv2[oldIndex]);
                }

                indices[i] = map[oldIndex];
            }

            var newMesh = new Mesh();
            newMesh.vertices = vectices.ToArray();
            newMesh.normals = normal.ToArray();
            newMesh.uv = uv.ToArray();
            newMesh.uv2 = uv2.ToArray();
            newMesh.tangents = tangents.ToArray();
            newMesh.triangles = indices;
            return newMesh;
        }


        private static void AddMeshNode(string name, Matrix4x4 matrix, Mesh mesh, int subIndex, Rect uv2Area)
        {
            HLODDebug.Log("[HLOD]" + "AddMeshNode");

            var sx = 1.0f / (uv2Area.max.x - uv2Area.min.x);
            var sy = 1.0f / (uv2Area.max.y - uv2Area.min.y);
            var ox = -uv2Area.min.x * sx;
            var oy = -uv2Area.min.y * sy;
            Rect uv2OffsetScale = new Rect(sx, sy, ox, oy);

            var subMesh = ConvertSubTOSingleMesh(mesh, subIndex);

            int vertexCount = subMesh.vertices.Length;
            int triangleCount = subMesh.triangles.Length / 3;
            int[] triangleArr = subMesh.triangles;
            float[] vertexArr = Convert(subMesh.vertices, matrix);
            var matrixArr = GetRelativeMatrix(null, Vector3.zero);
            var uvList = new List<Vector2>();
            var uvList2 = new List<Vector2>();
            var normalList = new List<Vector3>();
            var subMeshUV2 = subMesh.uv2.Length == 0 ? subMesh.uv : subMesh.uv2;
            for (int i = 0; i < subMeshUV2.Length; i++)
            {
                var uv2 = subMeshUV2[i];
                subMeshUV2[i] = new Vector2(uv2.x * uv2OffsetScale.x + uv2OffsetScale.width,
                    uv2.y * uv2OffsetScale.y + uv2OffsetScale.height);
            }

            foreach (var index in triangleArr)
            {
                uvList.Add(subMesh.uv[index]);
                uvList2.Add(subMeshUV2[index]);

                //计算法线在世界坐标系的方向.
                var point = subMesh.vertices[index];
                Vector3 worldPoint = matrix * new Vector4(point.x, point.y, point.z, 1);
                var point2 = point + subMesh.normals[index];
                Vector3 worldPoint2 = matrix * new Vector4(point2.x, point2.y, point2.z, 1);
                Vector3 worldNormal = (worldPoint2 - worldPoint).normalized;
                normalList.Add(worldNormal);
            }

            float[] uvArr = Convert(uvList);
            float[] uvArr2 = Convert(uvList2);
            float[] normalArr = Convert(normalList);

            SimplygonBridge.AddMeshNode(name, matrixArr, vertexCount, vertexArr, triangleCount, triangleArr, uvArr, uvArr2,
                normalArr);
        }

        private static void TransportData(List<Renderer> renderers, Vector3 center)
        {
            HLODDebug.Log("[HLOD][TransportData] start");
            Dictionary<string, string> textureMap = new Dictionary<string, string>();

            foreach (var cur in renderers)
            {
                Mesh mesh = null;
                Material material = null;
                Material[] materials = null;
                if (cur is MeshRenderer)
                {
                    var meshFilter = cur.transform.GetComponent<MeshFilter>();
                    mesh = meshFilter.sharedMesh;
                }

                if (cur is SkinnedMeshRenderer)
                {
                    var skinRender = cur as SkinnedMeshRenderer;
                    mesh = skinRender.sharedMesh;
                }

                material = cur.sharedMaterial;
                materials = cur.sharedMaterials;
                if (mesh == null || material == null || materials == null)
                {
                    continue;
                }

                Rect uv2Area = GetUV2Area(mesh);
                Rect lightMapArea = GetLightMapArea(uv2Area, cur.lightmapScaleOffset);
                HLODDebug.Log("[HLOD][CutLightMap] name:" + cur.name, cur);

                var lightMapTex = CutLightMap(lightMapArea, cur.lightmapIndex);
                int count = mesh.subMeshCount;
                bool origin;
                SetMeshReadable(mesh, out origin);
                for (int i = 0; i < count; i++)
                {
                    HLODDebug.Log("[HLOD][TransportData] subMesh count:" + count);
                    var desc = mesh.GetSubMesh(i);
                    var lastFix = "_" + i + "_" + DateTime.Now.ToFileTime();
                    var meshName = cur.transform.name + lastFix;
                    var tex = GetMainTexture(materials, i);

                    var texPath = Path.GetFullPath(AssetDatabase.GetAssetPath(tex));
                    var matrix = Matrix4x4.Translate(-center) * cur.transform.localToWorldMatrix;
                    AddMeshNode(meshName, matrix, mesh, i, uv2Area);
                    var texName = Path.GetFileNameWithoutExtension(texPath) + lastFix;
                    if (textureMap.ContainsKey(texPath))
                    {
                        texName = textureMap[texPath];
                    }

                    textureMap[texPath] = texName;
                    AddTextureMaterail(texName, texPath, lightMapTex);
                    SimplygonBridge.BindMeshAndMaterial(meshName, texName);
                }

                if (!origin)
                {
                    SetReadable(mesh, false);
                }
            }

            HLODDebug.Log("[HLOD][TransportData] end");
        }

        static int couter = 0;

        public static string GetLightMap(Mesh mesh, Renderer renderer)
        {
            Rect uv2Area = GetUV2Area(mesh);
            Rect lightMapArea = GetLightMapArea(uv2Area, renderer.lightmapScaleOffset);
            var lightMapTex = CutLightMap(lightMapArea, renderer.lightmapIndex);
            return lightMapTex;
        }

        private static string CutLightMap(Rect area, int index)
        {
            if (index >= m_lightMapTex.Count || index < 0)
                return "";
            var tex = m_lightMapTex[index];
            int startX = (int) (area.x * tex.width);
            int startY = (int) (area.y * tex.height);
            int width = (int) ((area.width) * tex.width) - startX;
            int height = (int) (area.height * tex.height) - startY;
            couter++;
            try
            {
                var colors = tex.GetPixels(startX, startY, width, height);
                var newTex = new Texture2D(width, height);
                newTex.SetPixels(colors);
                newTex.Apply();
                var bytes = newTex.EncodeToPNG();

                var path = GetTempOutputFolderRoot() + "newTex_" + couter + ".png";
                if (File.Exists(path))
                    File.Delete(path);
                HLODDebug.Log("[HLOD][CutLightMap] generate: " + path);

                using (var stream = File.Create(path))
                {
                    stream.Write(bytes, 0, bytes.Length);
                }

                if (newTex)
                {
                    Object.DestroyImmediate(newTex);
                    newTex = null;
                }

                return path;
            }
            catch (Exception)
            {
                return "";
            }
        }

        private static Rect GetUV2Area(Mesh mesh)
        {
            var area = new Rect();
            area.min = new Vector2(float.MaxValue, float.MaxValue);
            area.max = new Vector2(float.MinValue, float.MinValue);
            var uv2s = mesh.uv2;
            if (uv2s.Length == 0)
            {
                uv2s = mesh.uv;
            }

            foreach (var uv2 in uv2s)
            {
                if (uv2.x < area.xMin)
                {
                    area.xMin = uv2.x;
                }

                if (uv2.y < area.yMin)
                {
                    area.yMin = uv2.y;
                }

                if (uv2.x > area.xMax)
                {
                    area.xMax = uv2.x;
                }

                if (uv2.y > area.yMax)
                {
                    area.yMax = uv2.y;
                }
            }

            return area;
        }

        private static Rect GetLightMapArea(Rect uvArea, UnityEngine.Vector4 so)
        {
            return new Rect(uvArea.xMin * so.x + so.z,
                uvArea.yMin * so.y + so.w,
                uvArea.xMax * so.x + so.z,
                uvArea.yMax * so.y + so.w);
        }

        private static void SetMaterial()
        {
        }

        private static void SetMeshReadable(Object mesh, out bool originValue)
        {
            var so = new SerializedObject(mesh);
            var property = so.FindProperty("m_IsReadable");
            originValue = property.boolValue;
            if (!originValue)
            {
                property.boolValue = true;
                so.ApplyModifiedProperties();
            }
        }

        private static void SetReadable(Object mesh, bool value)
        {
            var so = new SerializedObject(mesh);
            var property = so.FindProperty("m_IsReadable");
            property.boolValue = value;
            so.ApplyModifiedProperties();
        }

        private static string GetTempOutputFolder()
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

        private static string GetTempOutputFolderRoot()
        {
            var path = Path.Combine(Application.dataPath, "../Temp/HLOD/");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return path;
        }

        private static void AddTextureMaterail(string name, string path, string lightMapTex)
        {
            HLODDebug.Log("[SimplygonTooDll][AddTextureMaterail] ligthmap:" + lightMapTex);
            SimplygonBridge.AddTextureMaterial(name, path, lightMapTex);
        }

        private static Texture GetMainTexture(Material[] materials, int index)
        {
            var mat = index < materials.Length ? materials[index] : null;
            if (mat == null)
                return null;

            var names = mat.GetTexturePropertyNames();
            foreach (var name in names)
            {
                var tex = mat.GetTexture(name);
                if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(tex)))
                    continue;
                return tex;
            }

            return null;
        }

        private static float[] GetRelativeMatrix(Transform cur, Vector3 parentPos)
        {
            return new float[]
            {
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1,
            };
        }

        private static void LodMatrixArr(float[] arr)
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

            HLODDebug.Log("[LodMatrixArr]" + sb);
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

        public static Task EditorMainThread(Action action)
        {
            return Task.Run(() =>
            {
                bool hasDone = false;
                CallbackFunction delayCall = () =>
                {
                    if (!hasDone && action != null)
                        action();
                    hasDone = true;
                };
                EditorApplication.update += delayCall;

                while (!hasDone)
                {
                    Thread.Sleep(1);
                }

                EditorApplication.update -= delayCall;
            });
        }
    }
}