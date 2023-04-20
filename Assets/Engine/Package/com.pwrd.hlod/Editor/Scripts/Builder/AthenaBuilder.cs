using System;
using System.Collections.Generic;
using System.IO;
using Athena.MeshSimplify;
using UnityEditor;
using UnityEditor.FBX;
using UnityEngine;

namespace com.pwrd.hlod.editor
{
    public class AthenaBuilder : IHLODBuilder
    {
        public List<HLODResultData> RunAggregate(List<(List<Renderer>, AggregateParam)> aggregateList)
        {
            var resultList = new List<HLODResultData>();
            AggregateBakedUtils.UpdateProgressBar("", "", 0);
            for (int i = 0; i < aggregateList.Count; i++)
            {
                DoAggregate(aggregateList, i, resultList);

                GC.Collect();
                Resources.UnloadUnusedAssets();
            }

            EditorUtility.ClearProgressBar();
            return resultList;
        }

        private void DoAggregate(List<(List<Renderer>, AggregateParam)> aggregateList, int i,
            List<HLODResultData> resultList)
        {
            var progressCount = aggregateList.Count;
            var tuple = aggregateList[i];
            var list = tuple.Item1;
            if (list.Count == 0)
            {
                return;
            }

            var param = tuple.Item2;
            try
            {
                var prefix = "";
                var title = "Athena Simplify Running[" + param.outputName + "]";
                AggregateBakedUtils.UpdateProgressBar(title, param.outputName + " 马上开始", (i) / progressCount);

                var outputPath = AggregateBakedUtils.GetTempOutputFolder();
                //计算包围盒中心.
                var center = AggregateBakedUtils.GetBoundsCenter(list);

                AggregateBakedUtils.UpdateProgressBar(title, prefix + "正在收集数据", (i + 0.2f) / progressCount);

                GlobalFunc.BeginSample("TransportData");
                var builder = TransportData(list, center, outputPath, param);
                GlobalFunc.EndSample("TransportData");

                AggregateBakedUtils.UpdateProgressBar(title, prefix + "正在减面并合并网格", (i + 0.7f) / progressCount);
                ReduceAndCombine(builder, outputPath, param);
                
                GlobalFunc.BeginSample("Import");

                //1.把输出结果拷贝进来,并创建prefab
                AggregateBakedUtils.UpdateProgressBar(title, prefix + "正在引入Assets", (i + 0.9f) / progressCount);
                var result = AggregateBakedUtils.ImportAssetIntoProject(outputPath, param, center);
                resultList.Add(result);

                GlobalFunc.EndSample("Import");
            }
            catch (Exception e)
            {
                HLODDebug.LogError(param.outputName + " " + e + e.StackTrace + "");
                SimplygonBridge.DeInitSDK();
            }

            EditorUtility.ClearProgressBar();
        }

        private FbxBuilder TransportData(List<Renderer> renderers, Vector3 center, string ouputPath,
            AggregateParam param, bool flipX = false)
        {

            FbxBuilder builder = new FbxBuilder();
            
            if (!param.useBakeMaterialInfo)
            {
                using (var fbxExporter = new ModelExporter ()) {
                    string filename = ouputPath + param.outputName + ".fbx";
            
                    List<GameObject> gos = new List<GameObject>();
                    for (int i = 0; i < renderers.Count; i++)
                    {
                        gos.Add(renderers[i].gameObject);
                    }
                    if (fbxExporter.ExportRendereres(gos, filename, center) > 0) {
                        string message = string.Format ("Successfully exported: {0}", filename);
                        UnityEngine.Debug.Log (message);
                    }
                }

                return null;
            }
            
            GlobalFunc.BeginSample("exportRenderer");

            foreach (var cur in renderers)
            {
                //拿mesh
                var mesh = AggregateBakedUtils.GetMeshReverseVert(cur); //似乎不需要额外对反面模型处理，暂时关闭
                if (mesh == null)
                {
                    continue;
                }
                
                var meshFilter = cur.transform.GetComponent<MeshFilter>();
                Vector3Int reverse = Vector3Int.one;
                reverse.x = meshFilter.transform.lossyScale.x > 0 ? 1 : -1;
                reverse.y = meshFilter.transform.lossyScale.y > 0 ? 1 : -1;
                reverse.z = meshFilter.transform.lossyScale.z > 0 ? 1 : -1;
                
                GlobalFunc.BeginSample("bakeTexture");

                //烘焙贴图
                var (albedoTexFilePath, normalTexFilePath, metallicTexFilePath, lightmapTexFilePath) =
                    AggregateBakedUtils.BakeTextures(cur, ouputPath, param);

                GlobalFunc.EndSample("bakeTexture");

                //收集贴图路径
                string albedoTexResult = null,
                    normalTexResult = null,
                    metallicTexResult = null,
                    lightmapTexResult = null;
                albedoTexResult = Path.GetFileNameWithoutExtension(albedoTexFilePath);
                if (!string.IsNullOrEmpty(normalTexFilePath) && (param.textureChannel & TextureChannel.Normal) != 0)
                {
                    normalTexResult = Path.GetFileNameWithoutExtension(normalTexFilePath);
                }

                if (!string.IsNullOrEmpty(metallicTexFilePath) &&
                    (param.textureChannel & TextureChannel.Metallic) != 0)
                {
                    metallicTexResult = Path.GetFileNameWithoutExtension(metallicTexFilePath);
                }

                if (!string.IsNullOrEmpty(lightmapTexFilePath) &&
                    (param.textureChannel & TextureChannel.LightMap) != 0)
                {
                    lightmapTexResult = Path.GetFileNameWithoutExtension(lightmapTexFilePath);
                }

                //收集mesh数据
                var orign = AggregateBakedUtils.GetAssetReadable(mesh);
                AggregateBakedUtils.SetAssetReadable(mesh, true);

                FbxEntity fet = new FbxEntity(mesh);
                // fet.unityTranslate = Matrix4x4.Translate(-center).MultiplyPoint(cur.transform.position);
                // fet.fbxRotate = new FbxDouble3(cur.transform.eulerAngles.x, cur.transform.eulerAngles.y, cur.transform.eulerAngles.z);
                // Debug.Log(cur.transform.name + " : " + cur.transform.eulerAngles);
                // fet.unityScale = cur.transform.lossyScale;
                Matrix4x4 flipx = Matrix4x4.identity;
                flipx.m00 = reverse.x;
                flipx.m11 = reverse.y;
                flipx.m22 = reverse.z;
                fet.matrix = Matrix4x4.Translate(-center) * cur.localToWorldMatrix * flipx;

                if (!string.IsNullOrEmpty(albedoTexResult))
                {
                    albedoTexFilePath = albedoTexFilePath.Replace("\\", "/");
                    fet.AddTex(TextureChannel.Albedo, albedoTexFilePath);
                }

                if (!string.IsNullOrEmpty(normalTexResult))
                {
                    normalTexFilePath = normalTexFilePath.Replace("\\", "/");
                    fet.AddTex(TextureChannel.Normal, normalTexFilePath);
                }

                if (!string.IsNullOrEmpty(metallicTexResult))
                {
                    metallicTexFilePath = metallicTexFilePath.Replace("\\", "/");
                    fet.AddTex(TextureChannel.Metallic, metallicTexFilePath);
                }

                if (!string.IsNullOrEmpty(lightmapTexResult))
                {
                    lightmapTexFilePath = lightmapTexFilePath.Replace("\\", "/");
                    fet.AddTex(TextureChannel.LightMap, lightmapTexFilePath);
                }

                builder.Add(fet);

                AggregateBakedUtils.SetAssetReadable(mesh, orign);

                GlobalFunc.EndSample("exportRenderer");
            }

            return builder;
        }

        private void ReduceAndCombine(FbxBuilder builder, string ouputPath, AggregateParam param)
        {
            GlobalFunc.BeginSample("doHlod");

            string filename = ouputPath + param.outputName + ".fbx";
            if (param.useBakeMaterialInfo)
            {
                builder.Export(filename);
            }

            CombineSetting setting = new CombineSetting();
            setting.inPath = filename;
            var targetFolder = Path.Combine(param.outputPath, param.outputName);

            if (Directory.Exists(targetFolder))
                Directory.Delete(targetFolder, true);
            Directory.CreateDirectory(targetFolder);

            setting.useVoxel = param.useVoxel;
            setting.outFbxPath = targetFolder + "/" + param.outputName + ".fbx";
            setting.outTexPath = targetFolder + "/" + param.outputName;
            setting.percent = param.reductionSetting.triangleRatio;
            setting.screenSize = (int) param.reductionSetting.screenSize;
            setting.uvQuality = 1;
            setting.excuteOrder = 0;
            setting.texWidth = Mathf.Max(2048, param.textureWidth); //把结果导进来的时候再压缩
            setting.texHeight = Mathf.Max(2048, param.textureHeight);
            setting.padding = 2;
            setting.lockBound = 0;
            setting.minAngle = 10;
            setting.textureChannel = (int) param.textureChannel;
            setting.weldingThreshold = param.reductionSetting.weldingThreshold;
            setting.useOcclusion = param.useOcclusion;
            setting.cameraPos = param.cameraPos;
            HlodTool.Combine(setting);

            GlobalFunc.EndSample("doHlod");
        }
    }
}