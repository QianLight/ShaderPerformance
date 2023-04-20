using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AssetCheck
{
    public class ProfilerData
    {
        public int drawcall = 0;
        public float frameTime = 0;
        public float averageOverDraw = 0;
        public float screenOverDraw = 0;

        public float fillRate;

    }

    public class TextureData
    {
        public string md5;
        public string path;
        public string name;
    }

    public class MeshData
    {
        public int VertexNum;
        public string Name;
        public int TriangleNum;
        public bool HasVertexColor;

        public bool ReadWriteEnabled;
        public string Compression;

        public bool OptimizeMesh;
        public bool ImportBlendShapes;
        public bool ImportCameras;
        public bool ImportLights;
        public string ImportNormals;
        public string MaterialImportMode;
        public bool ImportMaterials;
        public bool ImportAnimation;
        public bool ImportConstraints;


    }

    public class Defines
    {
        // 软件名
        public const string AssetCheckName = "Trident Devops AssetCheck";
        // 版本号
        public const string VersionId = "0.1";
        // CheckPathConfig配置路径
        public const string CheckPathConfigPath = "Packages/com.pwrd.devops/Devops-assetCheck/Editor/Configs";
        // CheckPathConfig名字
        public const string CheckPathConfigName = "checkPathConfig.asset";
        // CheckPathConfigTemp名字
        public const string CheckPathConfigTempName = "checkPathConfigTemp.asset";
        // tags名字
        public const string CheckTagsConfigName = "Tags.asset";
        // RuntimeConfig配置路径
        public const string CheckPathRuntimeConfigPath = "Packages/com.pwrd.devops/Devops-assetCheck/Runtime/Configs";
        // RuntimeRenderConfig名字
        public const string CheckPathRuntimeRenderConfig = "AssetCheckRenderInfoConfig.json";
        // RuntimeRenderResult??
        public const string CheckPathRuntimeRenderResult = "Packages/com.pwrd.devops/Devops-assetCheck/Editor/Configs/AssetCheckRenderResultConfig.json";
        // Runtime用的资源临时文件夹 需要在AssetsResources文件夹下
        public const string ResourceTempPath = "Assets/Resources/AssetCheckTempRes";
        // log输出目录
        public const string OutputDir = "AssetCheckResults";
        // scenename
        public const string SceneName = "AssetCheck";
        // scenePath
        public const string ScenePath = "Packages/com.pwrd.devops/Devops-assetCheck/Editor/Resources/Scene/Scene.unity";
        // sample
        public const string SamplesFolder = "Packages/com.pwrd.devops/Devops-assetCheck/Editor/Samples";
        // shellPath
        public const string ShellPath = "Packages/com.pwrd.devops/Devops-assetCheck/Editor/Sh/";
    }

}
