#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using Athena.MeshSimplify;
using UnityEngine;

namespace com.pwrd.hlod.editor
{
    [Serializable]
    public class AggregateParam
    {
        public Cluster cluster;
        public AggregateParam()
        {

        }
        public AggregateParam(AggregateParam param)
        {
            var type = typeof(AggregateParam);
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                field.SetValue(this, field.GetValue(param));
            }
        }

        public string outputPath;
        public string outputName;

        public int textureWidth;
        public int textureHeight;
        public int lightMapWidth;
        public int lightMapHeight;

        public bool smoothNormals;
        public bool enableMeshOptimation = true;
        public bool fixMaterialQueue = false;
        public int materialQueue = 2000;
        public Shader targetShader;
        public Action<Material, AggregateParam> targetMaterialPropertiesAction;

        public ReductionSetting reductionSetting = new ReductionSetting();
        
        public bool useOcclusion;
        public float useProxyScreenPercent;

        //相机参数传递给Simplgyon的Visibility
        //参数意义: https://documentation.simplygon.com/SimplygonSDK_8.3.35800.0/articles/simplygonui/Visibility.html?q=camera
        public float camPitchAngle;
        public float camYawAngle;
        public float camCoverage;
        public bool useAlphaTest;
        
        //AthenaSimplify可见性检测自定义的观测点
        public List<Vector3> cameraPos = new List<Vector3>();

        public bool useMeshReduction = true;
        public bool useBakeMaterialInfo = true;
        public bool useHighRendererLightmap;
        public int useLODIndex;
        public RendererBakerSetting rendererBakerSetting;
        public ShaderBindConfig shaderBindConfig;
        public List<HLODDecalTag> decalTagList = new List<HLODDecalTag>();

        public bool useVoxel = false;
        public TextureChannel textureChannel = TextureChannel.Albedo;
    }
}
#endif
