using System;
using System.Collections.Generic;
using UnityEngine;

namespace com.pwrd.hlod.editor
{
    public enum HlodMethod
    {
        AthenaSimplify,
        Simplygon,
    }

    public enum State
    {
        None,
        Cluster,
        ProxyMesh,
    }

    public enum GenerateClusterMethod
    {
        UE4_Triple,
        DeepFirst,
        ChunkArea,
        QuadTree,
        DP,
        KMeans,
        BVHTree,
        Octree,
    }
    
    public enum ProxyMapType
    {
        LODGroup,
        PrefabRoot,
    }
    
    public enum DPBestValue
    {
        MinBoundsSize,
        MaxCount,
    }

    /// <summary>
    /// 场景设置（场景的设置）
    /// </summary>
    [Serializable]
    public class SceneSetting //sceneSetting
    {
        public GenerateClusterMethod clusterMethod = GenerateClusterMethod.KMeans;
        public bool useBakeMaterialInfo = true;
        public bool useMeshReduction = true;
        public int useLODIndex = 0;
        public bool useHighRendererLightmap = false;
        public bool slpitAlphaTest;
        
        // global layer setting
        public List<LayerSetting> layerSettings = new List<LayerSetting>();

        public SceneSetting Clone()
        {
            var setting = new SceneSetting()
            {
                clusterMethod = this.clusterMethod,
                useMeshReduction = this.useMeshReduction,
                useBakeMaterialInfo = this.useBakeMaterialInfo,
                useLODIndex = this.useLODIndex,
                useHighRendererLightmap = this.useHighRendererLightmap,
                slpitAlphaTest = this.slpitAlphaTest,
            };
            setting.layerSettings = new List<LayerSetting>();
            foreach (var layerSetting in layerSettings)
            {
                setting.layerSettings.Add(layerSetting.Clone());
            }
            return setting;
        }
    }

    /// <summary>
    /// 层设置（层的设置）
    /// </summary>
    [Serializable]
    public class LayerSetting
    {
        public ClusterSetting clusterSetting = new ClusterSetting();
        public MeshReductionSetting meshReductionSetting = new MeshReductionSetting();

        public LayerSetting Clone()
        {
            return new LayerSetting()
            {
                clusterSetting = this.clusterSetting.Clone(),
                meshReductionSetting = this.meshReductionSetting.Clone(),
            };
        }

        public static List<LayerSetting> GetDefaultSetting()
        {
            var list = new List<LayerSetting>()
            {
                new LayerSetting(),
                new LayerSetting(),
                new LayerSetting(),
            };
            list[0].clusterSetting.clusterMinDiameter = 0;
            list[0].clusterSetting.clusterMaxDiameter = 20;
            list[0].clusterSetting.overlayPercentage = 0;
            list[0].meshReductionSetting.albedoSize = new Vector2Int(256, 256);
            list[1].clusterSetting.clusterMinDiameter = 20;
            list[1].clusterSetting.clusterMaxDiameter = 50;
            list[1].clusterSetting.overlayPercentage = 0;
            list[1].meshReductionSetting.albedoSize = new Vector2Int(512, 512);
            list[2].clusterSetting.clusterMinDiameter = 50;
            list[2].clusterSetting.clusterMaxDiameter = 125;
            list[2].clusterSetting.overlayPercentage = 0;
            list[2].meshReductionSetting.albedoSize = new Vector2Int(2048, 2048);
            return list;
        }
    }

    /// <summary>
    /// 分簇设置（层的设置）
    /// </summary>
    [Serializable]
    public class ClusterSetting
    {
        public float clusterMinDiameter = 0;
        public float clusterMaxDiameter = 20;
        public int overlayPercentage;
        public int mergeRendererMinCount;
        public bool splitByAlphaTest;

        //chunkarea
        public Vector3 startPos = Vector3.zero;
        public Vector3 endPos = new Vector3(400, 0, 400);
        public int horizonalChunckCount = 8;
        public int verticalChunckCount = 8;

        //quadtree/octree
        public bool tree_UseSelectBounds = true;
        public Vector3 tree_Center;
        public Vector3 tree_Size;
        public int tree_Depth = 2;

        //dp
        public DPBestValue dp_BestValue;

        //bvhtree
        public int bvh_SplitCount = 64;
        public int bvh_Depth = 5;

        //k-means
        public int clusterCount = 5;
        public int maxIterations = 10;

        public ClusterSetting Clone()
        {
            return new ClusterSetting()
            {
                clusterMinDiameter = clusterMinDiameter,
                clusterMaxDiameter = clusterMaxDiameter,
                overlayPercentage = overlayPercentage,
                mergeRendererMinCount = mergeRendererMinCount,
                splitByAlphaTest = splitByAlphaTest,
                startPos = startPos,
                endPos = endPos,
                horizonalChunckCount = horizonalChunckCount,
                verticalChunckCount = verticalChunckCount,
                tree_UseSelectBounds = tree_UseSelectBounds,
                tree_Center = tree_Center,
                tree_Size = tree_Size,
                tree_Depth = tree_Depth,
                dp_BestValue = dp_BestValue,
                bvh_SplitCount = bvh_SplitCount,
                bvh_Depth = bvh_Depth,
                clusterCount = clusterCount,
                maxIterations = maxIterations,
            };
        }
    }

    /// <summary>
    /// 减面设置（簇的设置）
    /// </summary>
    [Serializable]
    public class MeshReductionSetting
    {
        public float quality = 0.1f;
        public bool lockGeometricBorder = false;
        public float geometryImportance = 5.0f;
        public bool smoothNormals = false;
        public bool enableMeshOptimation = true;
        public bool fixMaterialQueue = false;
        public int materialQueue = 2000;
        public bool calcQuality = false;
        public bool fillTilling = false;
        public bool enableMaxEdgeLength = false;
        public float maxEdgeLength = 10;
        public bool enableScreenSize = true;
        public int screenSize = 50;
        public bool enableMaxDeviation = false;
        public float maxDeviation = 10000.0f;
        public bool useOcclusion;
        public float pitch, yaw, coverage = 180;
        //AthenaSimplify可见性检测自定义的观测点
        public List<Vector3> cameraPos = new List<Vector3>();
        public Vector2Int albedoSize = new Vector2Int(256, 256);
        public bool useCulling = false;
        public float culling = 0.01f;
        public float useProxyScreenPercent = 0.6f;
        public Shader targetShader;

        public float weldingThreshold = 0f;

        public MeshReductionSetting Clone()
        {
            return new MeshReductionSetting()
            {
                quality = this.quality,
                lockGeometricBorder = this.lockGeometricBorder,
                geometryImportance = this.geometryImportance,
                smoothNormals = this.smoothNormals,
                enableMeshOptimation = this.enableMeshOptimation,
                fixMaterialQueue = this.fixMaterialQueue,
                materialQueue = this.materialQueue,
                calcQuality = this.calcQuality,
                fillTilling = this.fillTilling,
                enableMaxEdgeLength = this.enableMaxEdgeLength,
                maxEdgeLength = this.maxEdgeLength,
                enableScreenSize = this.enableScreenSize,
                screenSize = this.screenSize,
                useOcclusion = this.useOcclusion,
                cameraPos = this.cameraPos,
                pitch = this.pitch,
                yaw = this.yaw,
                coverage = this.coverage,
                albedoSize = this.albedoSize,
                useCulling = this.useCulling,
                culling = this.culling,
                useProxyScreenPercent = this.useProxyScreenPercent,
                targetShader = this.targetShader,
                weldingThreshold = this.weldingThreshold
            };
        }
    }
}