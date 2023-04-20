using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using ZeusFlatBuffers;

public static class AssetDatabaseEx
{
    const string DEP_CACHE_FILE = "assetDatabaseEx_dep.data";
    private static string depCacheFilePath
    {
        get
        {
            return Path.Combine(UnityEngine.AssetGraph.DataModel.Version2.Settings.Path.CachePath, DEP_CACHE_FILE);
        }
    }

    public class DependData
    {
        public int assetPathIndex;
        public Hash128 assetDependencyHash;
        public int[] dependsPathIndex;
        public string[] dependsPath; //用于返回查询结果，不保存

        public DependData(DependDataFB dataFB)
        {
            assetPathIndex = dataFB.Index;
            assetDependencyHash = Hash128.Parse(dataFB.Hash);
            dependsPathIndex = new int[dataFB.DepIndicesLength];
            for (int i = 0; i < dataFB.DepIndicesLength; i++)
            {
                dependsPathIndex[i] = dataFB.DepIndices(i);
            }
        }

        public DependData()
        {

        }
    }
    // save data
    static Dictionary<string, DependData> _asset2DepData;
    static List<string> _assetList;
    // temp data
    static Dictionary<string, int> m_strIndex;

    private static bool _isInit = false;
    private static bool _isDirty = false;

    public static void SaveDependData()
    {
        if (!_isDirty)
            return;

        FlatBufferBuilder builder = new FlatBufferBuilder(1);

        Offset<DependDataFB>[] dataOffsets = new Offset<DependDataFB>[_asset2DepData.Count];
        StringOffset[] assetPathOffsets = new StringOffset[_assetList.Count];

        int i = 0;
        foreach (var pair in _asset2DepData)
        {
            DependData depData = pair.Value;
            VectorOffset depIndices = DependDataFB.CreateDepIndicesVector(builder, depData.dependsPathIndex);
            dataOffsets[i++] = DependDataFB.CreateDependDataFB(builder, depData.assetPathIndex, builder.CreateString(depData.assetDependencyHash.ToString()), depIndices);
        }
        i = 0;
        foreach (string asset in _assetList)
        {
            assetPathOffsets[i++] = builder.CreateString(asset);
        }

        VectorOffset dataVec = AssetDependDataFB.CreateDataVector(builder, dataOffsets);
        VectorOffset assetPathVec = AssetDependDataFB.CreateAssetPathVector(builder, assetPathOffsets);
        AssetDependDataFB.StartAssetDependDataFB(builder);
        AssetDependDataFB.AddData(builder, dataVec);
        AssetDependDataFB.AddAssetPath(builder, assetPathVec);
        Offset<AssetDependDataFB> endOffset = AssetDependDataFB.EndAssetDependDataFB(builder);
        AssetDependDataFB.FinishAssetDependDataFBBuffer(builder, endOffset);

        File.WriteAllBytes(depCacheFilePath, builder.SizedByteArray());

        _isDirty = false;
    }

    public static void InitDepenData()
    {
        _isInit = true;
        _isDirty = false;
        _asset2DepData = new Dictionary<string, DependData>();
        _assetList = new List<string>();
        m_strIndex = new Dictionary<string, int>();
        if (File.Exists(depCacheFilePath))
        {
            byte[] bytes = File.ReadAllBytes(depCacheFilePath);
            ZeusFlatBuffers.ByteBuffer buffer = new ZeusFlatBuffers.ByteBuffer(bytes);
            AssetDependDataFB assetDepData = AssetDependDataFB.GetRootAsAssetDependDataFB(buffer);

            for (int i = 0; i < assetDepData.AssetPathLength; ++i)
            {
                string assetPath = assetDepData.AssetPath(i);
                _assetList.Add(assetPath);
                m_strIndex.Add(assetPath, i);
            }

            for (int i = 0; i < assetDepData.DataLength; ++i)
            {
                DependDataFB dataFB = assetDepData.Data(i).Value;
                DependData data = new DependData(dataFB);
                _asset2DepData.Add(_assetList[dataFB.Index], data);
            }

            foreach (var data in _asset2DepData.Values)
            {
                data.dependsPath = new string[data.dependsPathIndex.Length];
                for (int i = 0; i < data.dependsPath.Length; i++)
                {
                    data.dependsPath[i] = _assetList[data.dependsPathIndex[i]];
                }
            }
        }
    }

    public static void ClearCache(bool deleteDataFile = true)
    {
        _asset2DepData.Clear();
        _assetList.Clear();
        m_strIndex.Clear();
        if (deleteDataFile)
        {
            File.Delete(depCacheFilePath);
        }
        _isDirty = true;
    }


    public static string[] GetDependencies(string pathName, bool recursive = true, bool isSkipScript = true)
    {
        if (!_isInit)
        {
            InitDepenData();
        }

        string formatPathName = pathName.Replace("\\", "/");
        List<string> deps = new List<string>();
        Dictionary<string, Hash128> asset2Hash = new Dictionary<string, Hash128>();
        Hash128 hash = AssetDatabase.GetAssetDependencyHash(formatPathName);
        if (!hash.isValid)
        {
            Debug.LogError($"The asset path \"{formatPathName}\" is invalid.");
            return null;
        }
        asset2Hash.Add(formatPathName, hash);
        GetDeps(formatPathName, ref deps, ref asset2Hash, recursive, isSkipScript);
        return deps.ToArray();
    }

    public static string[] GetDirectDependencies(string pathName)
    {
        return GetDependencies(pathName, false);
    }

    private static void GetDeps(string pathName, ref List<string> deps, ref Dictionary<string, Hash128> asset2Hash, bool recursive = true, bool isSkipScript = true)
    {
        if (recursive)
        {
            if (isSkipScript && pathName.ToLower().EndsWith(".cs"))
            {
                return;
            }
            deps.Add(pathName);
        }
        string[] directDeps;
        DependData depData;
        if (_asset2DepData.TryGetValue(pathName, out depData))
        {
            if (asset2Hash[pathName] == depData.assetDependencyHash)
            {
                int i = 0;
                for (; i < depData.dependsPath.Length; ++i)
                {
                    string dep = depData.dependsPath[i];
                    if (asset2Hash.ContainsKey(dep))
                    {
                        continue;
                    }
                    Hash128 hash = AssetDatabase.GetAssetDependencyHash(dep);
                    if (!hash.isValid)
                    {
                        break;
                    }
                    asset2Hash.Add(dep, hash);
                    if (recursive)
                    {
                        GetDeps(dep, ref deps, ref asset2Hash);
                    }
                    else
                    {
                        if (isSkipScript && pathName.ToLower().EndsWith(".cs"))
                        {
                            return;
                        }
                        deps.Add(dep);
                    }
                }
                if (i == depData.dependsPath.Length)
                {
                    return;
                }
            }
        }
        directDeps = AssetDatabase.GetDependencies(pathName, false);
        depData = new DependData();
        _asset2DepData[pathName] = depData;
        depData.assetDependencyHash = AssetDatabase.GetAssetDependencyHash(pathName);
        depData.assetPathIndex = GetAssetIndices(new string[] { pathName })[0];
        depData.dependsPathIndex = GetAssetIndices(directDeps);
        depData.dependsPath = directDeps;
        _isDirty = true;
        foreach (string dep in directDeps)
        {
            if (asset2Hash.ContainsKey(dep))
            {
                continue;
            }
            Hash128 hash = AssetDatabase.GetAssetDependencyHash(dep);
            asset2Hash.Add(dep, hash);
            if (recursive)
            {
                GetDeps(dep, ref deps, ref asset2Hash);
            }
            else
            {
                if (isSkipScript && pathName.ToLower().EndsWith(".cs"))
                {
                    return;
                }
                deps.Add(dep);
            }
        }
    }

    public static bool SetAssetDirty(string pathName)
    {
        if (!_isInit)
        {
            InitDepenData();
        }
        string formatPathName = pathName.Replace("\\", "/");
        return _asset2DepData.Remove(formatPathName);
    }

    private static int[] GetAssetIndices(string[] pathNames)
    {
        int[] indices = new int[pathNames.Length];
        for (int i = 0; i < pathNames.Length; ++i)
        {
            if (!m_strIndex.TryGetValue(pathNames[i], out indices[i]))
            {
                indices[i] = _assetList.Count;
                _assetList.Add(pathNames[i]);
                m_strIndex.Add(pathNames[i], indices[i]);
            }
        }
        return indices;
    }
}