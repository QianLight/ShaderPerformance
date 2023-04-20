/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using ZeusFlatBuffers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Collections.Concurrent;
using Zeus.Core.FileSystem;

namespace Zeus.Framework.Asset
{
    public enum BundleCompressMethod
    {
        None,
        Zip
    }

    public class SubPackageBundleInfoContainer
    {
        private class SubpackageInfo
        {
            public List<string> TagSequence;
            public Dictionary<string, List<string>> Tag2ChunkNames;
            public Dictionary<string, SubPackageChunkInfo> ChunkInfoDic;
            //bundle to bundleInfo
            public Dictionary<string, SubPackageBundleInfo> BundleInfoDic;
            //chunk to other info
            public Dictionary<string, SubPackageOtherInfo> OtherInfoDic;
            public string ChunkListName;

            public SubpackageInfo()
            {
                Tag2ChunkNames = new Dictionary<string, List<string>>();
                BundleInfoDic = new Dictionary<string, SubPackageBundleInfo>();
                ChunkInfoDic = new Dictionary<string, SubPackageChunkInfo>();
                OtherInfoDic = new Dictionary<string, SubPackageOtherInfo>();
            }

#if UNITY_EDITOR
            public void Init(List<SubPackageBundleInfo> bundleInfos,
                List<SubPackageOtherInfo> otherAssetInfos,
                Dictionary<string, List<SubPackageChunkInfo>> tag2Chunks,
                List<string> tagSequence,
                string chunkListName)
            {
                Tag2ChunkNames = new Dictionary<string, List<string>>();
                BundleInfoDic = new Dictionary<string, SubPackageBundleInfo>();
                ChunkInfoDic = new Dictionary<string, SubPackageChunkInfo>();
                OtherInfoDic = new Dictionary<string, SubPackageOtherInfo>();
                
                ChunkListName = chunkListName;
                TagSequence = tagSequence;
                for (int i = 0; i < bundleInfos.Count; i++)
                {
                    if (!BundleInfoDic.ContainsKey(bundleInfos[i].BundleName))
                    {
                        BundleInfoDic.Add(bundleInfos[i].BundleName, bundleInfos[i]);
                    }
                }
                for (int i = 0; otherAssetInfos != null && i < otherAssetInfos.Count; i++)
                {
                    if (!OtherInfoDic.ContainsKey(otherAssetInfos[i].Path))
                    {
                        OtherInfoDic.Add(otherAssetInfos[i].ChunkFile, otherAssetInfos[i]);
                    }
                }
                foreach (var pair in tag2Chunks)
                {
                    List<string> nameList = new List<string>();
                    Tag2ChunkNames.Add(pair.Key, nameList);
                    foreach (var chunk in pair.Value)
                    {
                        nameList.Add(chunk.FileName);
                        if (!ChunkInfoDic.ContainsKey(chunk.FileName))
                        {
                            ChunkInfoDic.Add(chunk.FileName, chunk);
                        }
                    }
                }
            }
#endif
            public void Init(SubpackageInfoFB infoFB)
            {
                ChunkListName = infoFB.ChunkListName;
                BundleInfoDic = new Dictionary<string, SubPackageBundleInfo>();
                TagSequence = new List<string>();
                Tag2ChunkNames = new Dictionary<string, List<string>>();
                ChunkInfoDic = new Dictionary<string, SubPackageChunkInfo>();
                for (int i = 0; i < infoFB.PartsLength; ++i)
                {
                    SubpackagePartInfoFB part = infoFB.Parts(i).Value;
                    TagSequence.Add(part.Tag);
                    List<string> chunkNames = new List<string>();
                    Tag2ChunkNames.Add(part.Tag, chunkNames);
                    for (int j = 0; j < part.ChunksLength; ++j)
                    {
                        SubpackageChunkInfoFB chunkInfoFB = part.Chunks(j).Value;
                        chunkNames.Add(chunkInfoFB.Name);
                        if (!ChunkInfoDic.ContainsKey(chunkInfoFB.Name))
                        {
                            //新版暂时不支持压缩
                            //chunk名字由“MD5值”加“_”加“16进制的CRC32值”组成，如：ab6e58711005e983cd672fe364438224_1b91a0b3.bundleChunk
                            //故通过名字取得CRC32值需要去掉后缀名，截取索引33之后的子字符串再转成十进制的int值
                            ChunkInfoDic.Add(chunkInfoFB.Name, new SubPackageChunkInfo(chunkInfoFB.Name,
                                int.Parse(Path.GetFileNameWithoutExtension(chunkInfoFB.Name).Substring(33), System.Globalization.NumberStyles.HexNumber), chunkInfoFB.Size, BundleCompressMethod.None));
                        }
                        for (int k = 0; k < chunkInfoFB.BundlesLength; k++)
                        {
                            SubpackageBundleInfoFB bundleInfoFB = chunkInfoFB.Bundles(k).Value;
                            string bundleInfoFBName = string.Intern(bundleInfoFB.Name);
                            if (!BundleInfoDic.ContainsKey(bundleInfoFBName))
                            {
                                BundleInfoDic.Add(bundleInfoFBName, new SubPackageBundleInfo(bundleInfoFBName, bundleInfoFB.Crc32, bundleInfoFB.Size, chunkInfoFB.Name, bundleInfoFB.Offset));
                            }
                        }
                        for (int k = 0; k < chunkInfoFB.OthersLength; k++)
                        {
                            SubpackageOtherInfoFB otherInfoFb = chunkInfoFB.Others(k).Value;
                            if (!OtherInfoDic.ContainsKey(chunkInfoFB.Name))
                            {
                                OtherInfoDic.Add(chunkInfoFB.Name, new SubPackageOtherInfo(otherInfoFb.Path, chunkInfoFB.Name));
                            }
                        }
                    }
                }
            }
        }

        public static string SubpackageBundleInfoPath = VFileSystem.GetZeusSettingPath("SubpackageBundleInfo.fb");

        private SubpackageInfo _info;
        private Dictionary<string, List<string>> _chunk2Bundles;
        private Dictionary<string, string> _chunk2Tag;

        public List<string> TagSequence
        {
            get
            {
                return _info.TagSequence;
            }
        }

        public string ChunkListName { get { return _info.ChunkListName; } }

        public Dictionary<string,List<string>> GetChunk2Bundles()
        {
            return _chunk2Bundles;
        }
        public Dictionary<string,SubPackageBundleInfo> GetBundleInfoDic()
        {
            return _info.BundleInfoDic;
        }
        
        public List<string> GetChunkSequence(string[] tags = null)
        {
            if(tags == null || tags.Length == 0)
            {
                tags = _info.TagSequence.ToArray();
            }
            List<string> chunkSequence = new List<string>();
            List<string> chunks;
            HashSet<string> chunkSet = new HashSet<string>();
            foreach (string tag in tags)
            {
                if (_info.Tag2ChunkNames.TryGetValue(tag, out chunks))
                {
                    foreach (string chunk in chunks)
                    {
                        if (chunkSet.Add(chunk))
                        {
                            chunkSequence.Add(chunk);
                        }
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError($"There is no tag named \"{tag}\".");
                }
            }
            return chunkSequence;
        }

        public ConcurrentDictionary<string, HashSet<string>> GetTagChunkSet(string[] tags, List<string> chunkWhiteList)
        {
            if (tags == null || tags.Length == 0)
            {
                tags = _info.TagSequence.ToArray();
            }
            var tag2chunkSet = new ConcurrentDictionary<string, HashSet<string>>();
            var chunkWhiteSet = new HashSet<string>(chunkWhiteList);
            foreach (string tag in tags)
            {
                HashSet<string> chunkSet = new HashSet<string>();
                tag2chunkSet.TryAdd(tag, chunkSet);
                List<string> chunks;
                if (_info.Tag2ChunkNames.TryGetValue(tag, out chunks))
                {
                    foreach (string chunk in chunks)
                    {
                        if (chunkWhiteSet.Contains(chunk))
                        {
                            chunkSet.Add(chunk);
                        }
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError($"There is no tag named \"{tag}\".");
                }
            }
            return tag2chunkSet;
        }

        public int GetChunkCount()
        {
            return _info.ChunkInfoDic.Count;
        }

#if UNITY_EDITOR
        public string[] ChunkNames
        {
            get
            {
                return _info.ChunkInfoDic.Keys.ToArray();
            }
        }
#endif

        /// <summary>
        /// 获取tag资源大小
        /// </summary>
        /// <param name="includeChunk">如果不为null，则只计算该参数传入的chunk大小</param>
        /// <returns></returns>
        public Dictionary<string, long> GetTag2Size(string[] tags, HashSet<string> includeChunk = null)
        {
            Dictionary<string, long> tag2Size = new Dictionary<string, long>();
            if (tags == null)
            {
                return tag2Size;
            }
            foreach (string tag in tags)
            {
                if (includeChunk == null)
                {
                    tag2Size.Add(tag, GetChunksSize(_info.Tag2ChunkNames[tag]));
                }
                else
                {
                    foreach (string chunk in _info.Tag2ChunkNames[tag])
                    {
                        long size = 0;
                        if (includeChunk.Contains(chunk))
                        {
                            size += _info.ChunkInfoDic[chunk].FileSize;
                        }
                        tag2Size.Add(tag, size);
                    }
                }
            }
            return tag2Size;
        }

        /// <summary>
        /// 获取所有tag资源大小
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, double> GetTag2Size()
        {
            Dictionary<string, double> tag2Size = new Dictionary<string, double>();
            foreach (var pair in _info.Tag2ChunkNames)
            {
                double size = 0;
                foreach (string chunk in pair.Value)
                {
                    size += _info.ChunkInfoDic[chunk].FileSize;
                }
                tag2Size.Add(pair.Key, size);
            }
            return tag2Size;
        }


        public long GetChunkTotalSize()
        {
            long size = 0;
            foreach (var infoPair in _info.ChunkInfoDic)
            {
                size += infoPair.Value.FileSize;
            }
            return size;
        }

        public long GetChunksSize(List<string> chunks)
        {
            long size = 0;
            if (chunks != null)
            {
                SubPackageChunkInfo info;
                foreach (string chunk in chunks)
                {
                    if (TryGetChunkInfo(chunk, out info))
                    {
                        size += info.FileSize;
                    }
                    else
                    {
                        UnityEngine.Debug.LogError($"Can't get chunk \"{chunk}\" from subpackageinfo");
                    }
                }
            }
            return size;
        }

        public long GetChunksSize(string chunk)
        {
            if (!string.IsNullOrEmpty(chunk))
            {
                SubPackageChunkInfo info;
                if (TryGetChunkInfo(chunk, out info))
                {
                    return info.FileSize;
                }
                else
                {
                    UnityEngine.Debug.LogError($"Can't get chunk \"{chunk}\" from subpackageinfo");
                }
            }
            return 0;
        }

        public Dictionary<string, List<string>> Chunk2Bundles
        {
            get
            {
                if (_chunk2Bundles == null)
                {
                    _chunk2Bundles = GetChunk2BundlesDic();
                }
                return _chunk2Bundles;
            }
        }

        public bool TryGetOtherInfo(string chunkFile, out SubPackageOtherInfo otherInfo)
        {
            return _info.OtherInfoDic.TryGetValue(chunkFile, out otherInfo);
        }

        public bool TryGetOtherFilePath(string chunkFile, ref string path)
        {
            SubPackageOtherInfo otherInfo;
            if (_info.OtherInfoDic.TryGetValue(chunkFile, out otherInfo))
            {
                path = OuterPackage.GetRealPath(otherInfo.Path);
                return true;
            }
            else
            {
                return false;
            }
        }

        private Dictionary<string, List<string>> GetChunk2BundlesDic()
        {
            Dictionary<string, List<string>> chunk2Bundles = new Dictionary<string, List<string>>();
            foreach (var bundle in _info.BundleInfoDic)
            {
                string chunkName = bundle.Value.ChunkFile;
                List<string> bundleList;
                if (!chunk2Bundles.TryGetValue(chunkName, out bundleList))
                {
                    bundleList = new List<string>();
                    chunk2Bundles.Add(chunkName, bundleList);
                }
                bundleList.Add(bundle.Key);
            }
            return chunk2Bundles;
        }

        public SubPackageBundleInfoContainer()
        {
            _info = new SubpackageInfo();
        }

        /// <summary>
        /// 可通过Bundle名获取Bundle信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public bool TryGetBundleInfo(string name, out SubPackageBundleInfo info)
        {
            return _info.BundleInfoDic.TryGetValue(name, out info);
        }

        public bool TryGetChunkInfo(string name, out SubPackageChunkInfo info)
        {
            return _info.ChunkInfoDic.TryGetValue(name, out info);
        }

        public bool TryGetTag2ChunkNames(string tagName,out List<string> result)
        {
            return _info.Tag2ChunkNames.TryGetValue(tagName, out result);
        }

        /// <summary>
        /// 判断是否包含该tag，不包含说明传入的tag是非法数据
        /// </summary>
        /// <param name="tagName"></param>
        /// <returns></returns>
        public bool ContainsTag(string tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return false;

            return _info.Tag2ChunkNames.ContainsKey(tagName);
        }

        public void Init(SubpackageInfoFB info)
        {
            _info.Init(info);
        }

#if UNITY_EDITOR
        public void Init(List<SubPackageBundleInfo> bundleInfos,
            List<SubPackageOtherInfo> otherAssetInfos,
            Dictionary<string, List<SubPackageChunkInfo>> tag2Chunks,
            List<string> tagSequence,
            string chunkListName)
        {
            _info.Init(bundleInfos, otherAssetInfos, tag2Chunks, tagSequence, chunkListName);
        }

        public void Save(string level = null)
        {
            string infoPath = Path.Combine(UnityEngine.Application.dataPath, GetContainerFilePath(level));
            FlatBufferBuilder builder = new FlatBufferBuilder(1);
            Dictionary<string, List<Offset<SubpackageBundleInfoFB>>> chunk2BundleOffsets = new Dictionary<string, List<Offset<SubpackageBundleInfoFB>>>();
            foreach (var bundleInfo in _info.BundleInfoDic.Values)
            {
                List<Offset<SubpackageBundleInfoFB>> list;
                if(!chunk2BundleOffsets.TryGetValue(bundleInfo.ChunkFile,out list))
                {
                    list = new List<Offset<SubpackageBundleInfoFB>>();
                    chunk2BundleOffsets.Add(bundleInfo.ChunkFile, list);
                }
                list.Add(SubpackageBundleInfoFB.CreateSubpackageBundleInfoFB(builder, builder.CreateString(bundleInfo.BundleName),
                    bundleInfo.BundleCrc32, bundleInfo.BundleSize, bundleInfo.ChunkFrom));
            }

            Dictionary<string, List<Offset<SubpackageOtherInfoFB>>> chunk2OtherOffsets = new Dictionary<string, List<Offset<SubpackageOtherInfoFB>>>();
            foreach (var otherInfo in _info.OtherInfoDic.Values)
            {
                List<Offset<SubpackageOtherInfoFB>> list;
                if (!chunk2OtherOffsets.TryGetValue(otherInfo.ChunkFile, out list))
                {
                    list = new List<Offset<SubpackageOtherInfoFB>>();
                    chunk2OtherOffsets.Add(otherInfo.ChunkFile, list);
                }
                list.Add(SubpackageOtherInfoFB.CreateSubpackageOtherInfoFB(builder, builder.CreateString(otherInfo.Path)));
            }

            Offset<SubpackagePartInfoFB>[] partOffsets = new Offset<SubpackagePartInfoFB>[_info.TagSequence.Count];
            int i = 0;
            foreach (var pair in _info.Tag2ChunkNames)
            {
                Offset<SubpackageChunkInfoFB>[] chunkOffsets = new Offset<SubpackageChunkInfoFB>[pair.Value.Count];
                int j = 0;
                foreach (string chunkName in pair.Value)
                {
                    SubPackageChunkInfo chunkInfo = _info.ChunkInfoDic[chunkName];
                    Offset<SubpackageBundleInfoFB>[] bundleOffsets;
                    List<Offset<SubpackageBundleInfoFB>> bundleList;
                    if (chunk2BundleOffsets.TryGetValue(chunkName, out bundleList))
                    {
                        bundleOffsets = bundleList.ToArray();
                    }
                    else
                    {
                        bundleOffsets = new Offset<SubpackageBundleInfoFB>[0];
                    }

                    Offset<SubpackageOtherInfoFB>[] otherOffsets;
                    List<Offset<SubpackageOtherInfoFB>> otherList;
                    if (chunk2OtherOffsets.TryGetValue(chunkName, out otherList))
                    {
                        otherOffsets = otherList.ToArray();
                    }
                    else
                    {
                        otherOffsets = new Offset<SubpackageOtherInfoFB>[0];
                    }

                    chunkOffsets[j++] = SubpackageChunkInfoFB.CreateSubpackageChunkInfoFB(builder, 
                        builder.CreateString(chunkInfo.FileName),chunkInfo.FileSize, 
                        SubpackageChunkInfoFB.CreateBundlesVector(builder, bundleOffsets), 
                        SubpackageChunkInfoFB.CreateOthersVector(builder, otherOffsets));
                }
                VectorOffset chunkVec = SubpackagePartInfoFB.CreateChunksVector(builder, chunkOffsets);
                partOffsets[i++] = SubpackagePartInfoFB.CreateSubpackagePartInfoFB(builder, builder.CreateString(pair.Key), chunkVec);
            }
            VectorOffset partVec = SubpackageInfoFB.CreatePartsVector(builder, partOffsets);
            StringOffset partStr = builder.CreateString(_info.ChunkListName);
            var endOffset = SubpackageInfoFB.CreateSubpackageInfoFB(builder, partVec, partStr);
            SubpackageInfoFB.FinishSubpackageInfoFBBuffer(builder, endOffset);

            File.WriteAllBytes(infoPath, builder.SizedByteArray());
        }

#endif

        public void DumpContent(string filePath)
        {
            System.Text.StringBuilder strBuilder = new System.Text.StringBuilder();
            foreach(var chunkinfo in _info.ChunkInfoDic)
            {
                strBuilder.AppendLine(chunkinfo.Value.FileName);
            }
           System.IO.File.WriteAllText(filePath, strBuilder.ToString());
        }
        /// <summary>
        /// 获取二包配置信息
        /// </summary>
        /// <param name="assetLevel">资产等级，不传入为默认等级</param>
        /// <returns></returns>
        public static SubPackageBundleInfoContainer LoadSubPackageInfo(string assetLevel = null)
        {
            SubPackageBundleInfoContainer container = null;
            try
            {
                var containerPath = GetContainerFilePath(assetLevel);
                if (VFileSystem.ExistsFile(containerPath))
                {
                    byte[] buffer = VFileSystem.ReadAllBytes(containerPath);
                    container = new SubPackageBundleInfoContainer();
                    container.Init(SubpackageInfoFB.GetRootAsSubpackageInfoFB(new ZeusFlatBuffers.ByteBuffer(buffer)));
                }
                else
                {
                    UnityEngine.Debug.LogError("Can't find file \"SubpackageBundleInfo.fb\"");
                    container = new SubPackageBundleInfoContainer();
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("LoadSubPackageInfo failed!");
                UnityEngine.Debug.LogException(ex);
            }
            return container;
        }

        public static bool IsExistSubpackageInfo(string level)
        {
            return VFileSystem.ExistsFile(GetContainerFilePath(level));
        }
        
        public static string GetContainerFilePath(string level = null)
        {
            if (string.IsNullOrEmpty(level))
            {
                return VFileSystem.GetZeusSettingPath("SubpackageBundleInfo.fb");
            }
            else
            {
                return VFileSystem.GetZeusSettingPath( level + "_SubpackageBundleInfo.fb");
            }
        }
    }
}