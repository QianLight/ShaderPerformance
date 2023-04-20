using System;
using System.Collections.Generic;
using UnityEngine;

namespace Athena.MeshSimplify
{
    [Flags]
    public enum TextureChannel
    {
	    Albedo = 1 << 0,
	    Normal = 1 << 1,
	    Metallic = 1 << 2,
	    LightMap = 1 << 3,
    }
    
    public class SimplygonData
    {
        public GameObject target;
        public string outputFolder;
        public int lodCount;
        public string nameExt = "";
        public bool isTerrain;
        public List<ReductionSetting> lodSimplygonItemDatas;

        public SimplygonData(GameObject target, string outputFolder) : this(target, outputFolder, new ReductionSetting(){ triangleRatio = 0.5f })
        {
            
        }
        
        public SimplygonData(GameObject target, string outputFolder, params ReductionSetting[] simplygonItemDatas)
        {
            this.target = target;
            this.outputFolder = outputFolder;
            this.lodCount = simplygonItemDatas.Length;
            lodSimplygonItemDatas = new List<ReductionSetting>(simplygonItemDatas);
        }
    }

    /// <summary>
    /// 减面单位
    /// </summary>
    public enum ReductionUnit
    {
        Default, //fbx or gameobject
        Mesh,    //mesh, 可以解决部分模型多套uv不一致时减面后uv映射错误的问题（减面次数高、耗时高）
    }

    public class ReductionSetting : ICloneable
    {
        /// <summary> 减面单位 </summary>
        public ReductionUnit reductionUnit = ReductionUnit.Default;
        /// <summary> 三角面缩减比例 0-1 </summary>
        public float triangleRatio = 0.5f;
        /// <summary> 使用数据偏好 </summary>
        public SimplygonUseDataPreferences useDataPreferences = SimplygonUseDataPreferences.OnlyUseOriginalData;
        /// <summary> 锁定边界 </summary>
        public bool lockGeometricBorder = true;
        /// <summary> 启用 屏幕尺寸 </summary>
        public bool enableScreenSize = false;
        /// <summary> 屏幕尺寸 20-100000 </summary>
        public uint screenSize = 600;
        /// <summary> 启用缩减后限制三角面最大边的长度 </summary>
        public bool enableMaxEdgeLength = false;
        /// <summary> 缩减后限制三角面最大边的长度 </summary>
        public float maxEdgeLength = 7.0f;
        /// <summary> 启用最大表面偏差 </summary>
        public bool enableMaxDeviation = false;
        /// <summary> 最大表面偏差 0-float max </summary>
        public float maxDeviation = 10000.0f;
        /// <summary> 启用期望三角面数量 </summary>
        public bool enableTriangleCount = false;
        /// <summary> 期望三角面数量 0-uint max </summary>
        public uint triangleCount = 0;
        public SimplygonReductionHeuristics reductionHeuristics = SimplygonReductionHeuristics.Fast;
        /// <summary>  几何的顶点法线的重要性值，尖锐的边缘 0-10 </summary>
        public float shadingImportance = 1.0f;
        /// <summary> 几何体的顶点和三角形的位置的重要性值，或几何体的轮廓 0-10 </summary>
        public float geometryImportance = 1.0f;
        /// <summary> UV坐标的重要值 0-10 </summary>
        public float textureImportance = 1.0f;
        /// <summary> 三角形角度范围10-170 </summary>
        public float angleRange = 10;
        /// <summary> 融合阈值 </summary>
        public float weldingThreshold = 0;

        public float materialImportance = 1.0f;
        public float groupImportance = 1.0f;
        public float vertexColorImportance = 1.0f;
        public float edgeSetImportance = 1.0f;
        public float skinningImportance = 1.0f;
        // public float curvatureImportance = 1.0f;    //官方已移除
        
        /// <summary> abledo尺寸 </summary>
        public Vector2Int albedoSize = new Vector2Int(1024, 1024);

        public object Clone()
        {
            var newData = new ReductionSetting();
            newData.triangleRatio = triangleRatio * 0.5f;
            newData.useDataPreferences = useDataPreferences;
            newData.lockGeometricBorder = lockGeometricBorder;
            newData.enableScreenSize = enableScreenSize;
            newData.screenSize = screenSize;
            newData.enableMaxEdgeLength = enableMaxEdgeLength;
            newData.maxEdgeLength = maxEdgeLength;
            newData.enableMaxDeviation = enableMaxDeviation;
            newData.maxDeviation = maxDeviation;
            newData.enableTriangleCount = enableTriangleCount;
            newData.triangleCount = triangleCount;
            newData.weldingThreshold = weldingThreshold;
            return newData;
        }
    }

    public enum SimplygonUseDataPreferences
    {
        OnlyUseOriginalData,
        PreferOriginalData,
        PreferOptimizedResult,
    }
    
    public enum SimplygonReductionHeuristics
    {
        Fast,
        Consistent,
    }
}