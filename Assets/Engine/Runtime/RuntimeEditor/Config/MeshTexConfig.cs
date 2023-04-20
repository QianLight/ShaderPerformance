#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace CFEngine
{
    public enum TexFilterType
    {
        Or,     // 等同于 ||
        And,    // 等同于 &&
        Nor,    // 等同于 !a && !b && ... && !n
    }
    public enum TexFlag
    {
        None,
        IgnoreImport = 0x00000001
    }

    [System.Serializable]
    public class TexCompressFilter
    {
        // public bool isNor = false;
        public string str;
    }

    [System.Serializable]
    public class TexImportSetting
    {
        public SpriteSize maxTextureSize = SpriteSize.E1024x1024;
        public TextureImporterFormat format = TextureImporterFormat.RGB24;
        public TextureImporterFormat alphaFormat = TextureImporterFormat.RGBA32;
        public TextureCompressionQuality texCompressorQuality = TextureCompressionQuality.Best;
    }

    [System.Serializable]
    public class TexCompressConfig : BaseFolderHash
    {
        public bool vaild = true;
        public string name = "";
        public int priority = 0;
        public TexFilterType type = TexFilterType.Or;
        public List<TexCompressFilter> compressFilters = new List<TexCompressFilter> ();
        public TextureImporterType importType = TextureImporterType.Default;
        public TextureImporterShape importShape = TextureImporterShape.Texture2D;
        public bool sRGB = true;
        public bool attrOverride = true;
        public bool mipMap = false;
        public float minBias = 0;
        public bool isReadable = false;
        public FilterMode filterMode = FilterMode.Bilinear;
        public bool isOverride = true;
        public TextureWrapMode wrapMode = TextureWrapMode.Repeat;
        public int anisoLevel = -1;
        public TexImportSetting iosSetting = new TexImportSetting ()
        {
            format = TextureImporterFormat.ASTC_6x6,
            alphaFormat = TextureImporterFormat.ASTC_6x6
        };
        public TexImportSetting androidSetting = new TexImportSetting ()
        {
            format = TextureImporterFormat.ASTC_6x6,
            alphaFormat = TextureImporterFormat.ASTC_6x6,
            texCompressorQuality = TextureCompressionQuality.Best
        };
        public TexImportSetting standaloneSetting = new TexImportSetting ()
        {
            format = TextureImporterFormat.DXT1,
            alphaFormat = TextureImporterFormat.DXT5
        };

        public override string ToString ()
        {
            return name;
        }
    }

    [System.Serializable]
    public class TexConfigData : BaseAssetConfig
    {
        public List<TexCompressConfig> texConfigs = new List<TexCompressConfig> ();
        public List<string> unResizePath = new List<string>();

        public override IList GetList () { return texConfigs; }

        public override Type GetListType () { return typeof (List<TexCompressConfig>); }

        public override void OnAdd () { texConfigs.Add (new TexCompressConfig ()); }
    }

    [System.Serializable]
    public class MeshOptimizeConfig : BaseFolderHash
    {
        public string name = "";
        public string exportDir = "";
        public bool isExport = false;
        public bool isReadable = false;
        public bool exportReadable = false;
        public bool removeUV2 = false;
        public bool removeColor = true;
        public bool rotate90 = true;
        public bool resample = false;
        public bool importCamera = false;
        public bool overrideImportNormals = false;
        public bool overrideImportTangents = false;
        public bool overrideImportBlendShapeNormals = false;
        public static int maxTriangular = 66666;
        public TexFilterType type = TexFilterType.Or;
        public ModelImporterNormals importNormal = ModelImporterNormals.Import;
        public ModelImporterTangents importTangent = ModelImporterTangents.Import;
        public ModelImporterNormals importBlendShapeNormals = ModelImporterNormals.Import;
        [FormerlySerializedAs("path")] public List<string> filters = new List<string> ();
    }

    [System.Serializable]
    public class MeshConfigData : BaseAssetConfig
    {
        public List<MeshOptimizeConfig> meshConfigs = new List<MeshOptimizeConfig> ();

        public override IList GetList () { return meshConfigs; }

        public override Type GetListType () { return typeof (List<MeshOptimizeConfig>); }

        public override void OnAdd () { meshConfigs.Add (new MeshOptimizeConfig ()); }
    }
}
#endif