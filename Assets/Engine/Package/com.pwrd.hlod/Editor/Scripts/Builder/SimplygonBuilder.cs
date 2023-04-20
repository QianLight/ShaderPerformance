using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Athena.MeshSimplify;
using UnityEditor;
using UnityEditor.FBX;
using UnityEngine;

namespace com.pwrd.hlod.editor
{
    public class SimplygonBuilder : IHLODBuilder
    {
        public List<HLODResultData> RunAggregate(List<(List<Renderer>, AggregateParam)> aggregateList)
        {
            var resultList = new List<HLODResultData>();
            AggregateBakedUtils.UpdateProgressBar("", "", 0);
            for (int i = 0; i < aggregateList.Count; i++)
            {
                AggregateBakedUtils.Init();
                if (!AggregateBakedUtils.IsInit)
                {
                    return resultList;
                }
                DoAggregate(aggregateList, i, resultList);
                SimplygonBridge.DeInitSDK();
                GC.Collect();
                Resources.UnloadUnusedAssets();
            }

            EditorUtility.ClearProgressBar();
            return resultList;
        }

        private void DoAggregate(List<(List<Renderer>, AggregateParam)> aggregateList, int i, List<HLODResultData> resultList)
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
                var title = "Simplygon Running[" + param.outputName + "]";
                AggregateBakedUtils.UpdateProgressBar(title, param.outputName + " 马上开始", (i) / progressCount);

                var outputPath = AggregateBakedUtils.GetTempOutputFolder();
                //计算包围盒中心.
                var center = AggregateBakedUtils.GetBoundsCenter(list);

                SimplygonBridge.SetTempFolderPath(outputPath);
                SimplygonBridge.CreateNewScene();

                AggregateBakedUtils.UpdateProgressBar(title, prefix + "正在传输数据到Simplygon", (i + 0.2f) / progressCount);
                
                GlobalFunc.BeginSample("TransportData");
                TransportData(list, center, outputPath, param);
                GlobalFunc.EndSample("TransportData");

                if (param.useMeshReduction)
                {
                    AggregateBakedUtils.UpdateProgressBar(title, prefix + "正在执行减面", (i + 0.7f) / progressCount);
                    if (param.useOcclusion)
                    {
                        SimplygonBridge.RunReductionProcessingUseOcclusion(param.reductionSetting.triangleRatio,
                                param.camPitchAngle,
                                param.camYawAngle, param.camCoverage);
                    }
                    else
                    {
                        SimplygonBridge.RunReductionProcessing(param.reductionSetting);
                    }
                }

                AggregateBakedUtils.UpdateProgressBar(title, prefix + "正在合并网格", (i + 0.8f) / progressCount);
                SimplygonBridge.RunAggregate(Mathf.Max(2048, param.textureWidth), Mathf.Max(2048, param.textureHeight), param.textureChannel);
                
                AggregateBakedUtils.UpdateProgressBar(title, prefix + "正在导出网格", (i + 0.85f) / progressCount);
                SimplygonBridge.ExportScene(outputPath + "Aggregate.obj");
                //Debug.Log("[HLOD] outPath:" + outputPath);
                
                GlobalFunc.BeginSample("Import");

                //1.把输出结果拷贝进来,并创建prefab
                AggregateBakedUtils.UpdateProgressBar(title, prefix + "正在引入Assets", (i + 0.9f) / progressCount);
                var result = AggregateBakedUtils.CopyAssetIntoProject(outputPath, param, center);
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

        private void TransportData(List<Renderer> renderers, Vector3 center, string ouputPath, AggregateParam param,
            bool flipX = false)
        {
            foreach (var cur in renderers)
            {
                //拿mesh
                var mesh = AggregateBakedUtils.GetMesh(cur);
                if (mesh == null)
                {
                    continue;
                }

                GlobalFunc.BeginSample("bakeTexture");

                //烘焙贴图
                var (albedoTexFilePath, normalTexFilePath, metallicTexFilePath, lightmapTexFilePath) =
                    AggregateBakedUtils.BakeTextures(cur, ouputPath, param);

                GlobalFunc.EndSample("bakeTexture");

                GlobalFunc.BeginSample("bindData");

                //绑定材质球贴图
                string matName = Path.GetFileNameWithoutExtension(albedoTexFilePath);
                AggregateBakedUtils.AddTextureMaterialByChannel(matName, albedoTexFilePath, TextureChannel.Albedo);
                if (!string.IsNullOrEmpty(normalTexFilePath) && (param.textureChannel & TextureChannel.Normal) != 0)
                {
                    AggregateBakedUtils.AddTextureMaterialByChannel(matName, normalTexFilePath, TextureChannel.Normal);
                }

                if (!string.IsNullOrEmpty(metallicTexFilePath) && (param.textureChannel & TextureChannel.Metallic) != 0)
                {
                    AggregateBakedUtils.AddTextureMaterialByChannel(matName, metallicTexFilePath, TextureChannel.Metallic);
                }

                if (!string.IsNullOrEmpty(lightmapTexFilePath) && (param.textureChannel & TextureChannel.LightMap) != 0)
                {
                    AggregateBakedUtils.AddTextureMaterialByChannel(matName, lightmapTexFilePath, TextureChannel.LightMap);
                }

                //向simplygon传递数据
                var orign = AggregateBakedUtils.GetAssetReadable(mesh);
                AggregateBakedUtils.SetAssetReadable(mesh, true);
                for (int i = 0; i < mesh.subMeshCount; i++)
                {
                    //创建网格
                    var lastFix = "_" + i + "_" + DateTime.Now.ToFileTime();
                    var meshName = cur.transform.name + lastFix;
                    var matrix = Matrix4x4.Translate(-center) * cur.transform.localToWorldMatrix;
                    AggregateBakedUtils.AddMeshNode(meshName, matrix, mesh, i, flipX);

                    // AggregateBakedUtils.AddTextureMaterail(texName, texPath, lightmapPath);

                    //绑定
                    SimplygonBridge.BindMeshAndMaterial(meshName, matName);
                }

                AggregateBakedUtils.SetAssetReadable(mesh, orign);

                GlobalFunc.EndSample("bindData");
            }
        }
    }
}