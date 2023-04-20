/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using System.IO;
using ZeusFlatBuffers;

namespace Zeus.Core.FileSystem
{
    public class RedundantFileCheckSumInfo
    {
        //重要：此处的文件名以及文件结构不允许修改，如有修改，需要与热更后台进行沟通以便同步处理！！！
        public static string RedundantFileCheckSumInfoFBName = "RedundantFileCheckSumInfoFB";

        public static string RedundantFileCheckSumInfoFB_RelativePath = VFileSystem.GetZeusSettingPath(RedundantFileCheckSumInfoFBName);
        public static string RedundantFileCheckSumInfoFB_OuterPackageFullPath = OuterPackage.GetRealPath(RedundantFileCheckSumInfoFB_RelativePath);

        public static string TempRedundantFileCheckSumInfoFB_RelativePath = RedundantFileCheckSumInfoFB_RelativePath + "_Temp";
        public static string TempRedundantFileCheckSumInfoFB_OuterPackageFullPath = OuterPackage.GetRealPath(TempRedundantFileCheckSumInfoFB_RelativePath);

        private Dictionary<string, string> infoDic = new Dictionary<string, string>();
        private HashSet<string> subPackageFiles = new HashSet<string>();

        public Dictionary<string, string> InfoDic
        {
            get
            {
                return infoDic;
            }
        }

        public HashSet<string> SubPackageFiles
        {
            get
            {
                return subPackageFiles;
            }
        }

        /// <summary>
        /// 可通过相对路径取到对应文件校验值
        /// </summary>
        /// <param name="relativePath"></param>
        /// <param name="checkSum"></param>
        /// <returns></returns>
        public bool TryGetCheckSum(string relativePath, out string checkSum)
        {
            relativePath = relativePath.Replace('\\', '/');
            return infoDic.TryGetValue(relativePath, out checkSum);
        }

        private void Add(string relativePath, string checkSum, bool isSubPackageFile)
        {
            relativePath = relativePath.Replace('\\','/');
            infoDic[relativePath] = checkSum;
            if (isSubPackageFile)
            {
                subPackageFiles.Add(relativePath);
            }
        }

        private static RedundantFileCheckSumInfo CreateFrom(RedundantFileCheckSumInfoFB infoFB)
        {
            RedundantFileCheckSumInfo info = new RedundantFileCheckSumInfo();
            for (int i = 0; i < infoFB.InfosLength; i++)
            {
                RelativePath2CheckSumInfoFB temp = infoFB.Infos(i).Value;
                info.Add(temp.RelativePath, temp.CheckSum, temp.IsSubPackage);
            }
            return info;
        }

#if UNITY_EDITOR
        public static void DeleteOldDataFile()
        {
            string infoPath = Path.Combine(UnityEngine.Application.streamingAssetsPath, RedundantFileCheckSumInfoFB_RelativePath);
            if (File.Exists(infoPath))
            {
                File.Delete(infoPath);
            }
            string oldInfoPath = Path.Combine(UnityEngine.Application.dataPath, RedundantFileCheckSumInfoFB_RelativePath);
            if (File.Exists(oldInfoPath))
            {
                File.Delete(oldInfoPath);
            }
        }


        public static RedundantFileCheckSumInfo CreateNewOrLoadInfos()
        {
            string infoPath = Path.Combine(UnityEngine.Application.streamingAssetsPath, RedundantFileCheckSumInfoFB_RelativePath);
            if (File.Exists(infoPath))
            {
                RedundantFileCheckSumInfoFB infoFB = RedundantFileCheckSumInfoFB.GetRootAsRedundantFileCheckSumInfoFB(new ZeusFlatBuffers.ByteBuffer(File.ReadAllBytes(infoPath)));
                return CreateFrom(infoFB);
            }
            else
            {
                return new RedundantFileCheckSumInfo();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="relative">运行时相对于OuterPackage文件夹得相对路径</param>
        /// <param name="fullPath">与key中的文件对应的需要计算并记录校验值的文件当前的路径</param>
        /// <param name="isSubPackageFile">是否为将要打入分包的文件</param>
        public void Update(string relative, string fullPath, bool isSubPackageFile)
        {
            string md5 = MD5Util.GetMD5FromFile(fullPath);
            Add(relative, md5, isSubPackageFile);
        }

        public void Remove(string relativePath)
        {
            relativePath = relativePath.Replace('\\', '/');
            infoDic.Remove(relativePath);
            subPackageFiles.Remove(relativePath);
        }

        public void SaveFB()
        {
            string infoPath = Path.Combine(UnityEngine.Application.streamingAssetsPath, RedundantFileCheckSumInfoFB_RelativePath);
            FlatBufferBuilder builder = new FlatBufferBuilder(1);
            Offset<RelativePath2CheckSumInfoFB>[] infosOffsets = new Offset<RelativePath2CheckSumInfoFB>[infoDic.Count];
            int i = 0;
            foreach (var bundleInfo in infoDic)
            {
                infosOffsets[i++] = RelativePath2CheckSumInfoFB.CreateRelativePath2CheckSumInfoFB(builder, builder.CreateString(bundleInfo.Key),
                    builder.CreateString(bundleInfo.Value), subPackageFiles.Contains(bundleInfo.Key));
            }
            VectorOffset infosVec = RedundantFileCheckSumInfoFB.CreateInfosVector(builder, infosOffsets);
            RedundantFileCheckSumInfoFB.StartRedundantFileCheckSumInfoFB(builder);
            RedundantFileCheckSumInfoFB.AddInfos(builder, infosVec);
            var endOffset = RedundantFileCheckSumInfoFB.EndRedundantFileCheckSumInfoFB(builder);
            RedundantFileCheckSumInfoFB.FinishRedundantFileCheckSumInfoFBBuffer(builder, endOffset);
            if (!Directory.Exists(Path.GetDirectoryName(infoPath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(infoPath));
            }
            if (File.Exists(infoPath))
            {
                File.Delete(infoPath);
            }
            File.WriteAllBytes(infoPath, builder.SizedByteArray());
        }

        /// <summary>
        /// 本函数仅在编辑器下有效
        /// </summary>
        public static RedundantFileCheckSumInfo EditorLoadFromFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    RedundantFileCheckSumInfoFB infoFB = RedundantFileCheckSumInfoFB.GetRootAsRedundantFileCheckSumInfoFB(new ZeusFlatBuffers.ByteBuffer(File.ReadAllBytes(path)));
                    RedundantFileCheckSumInfo info = CreateFrom(infoFB);
                    return info;
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
            return null;
        }
#endif
        public static RedundantFileCheckSumInfo LoadInnerInfosFromFB()
        {
            try
            {
                if (InnerPackage.ExistsFile(RedundantFileCheckSumInfoFB_RelativePath))
                {
                    RedundantFileCheckSumInfoFB infoFB = RedundantFileCheckSumInfoFB.GetRootAsRedundantFileCheckSumInfoFB(new ZeusFlatBuffers.ByteBuffer(InnerPackage.ReadAllBytes(RedundantFileCheckSumInfoFB_RelativePath)));
                    return CreateFrom(infoFB);
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            return null;
        }

        public static RedundantFileCheckSumInfo LoadOuterInfosFromFB()
        {
            try
            {
                if (File.Exists(RedundantFileCheckSumInfoFB_OuterPackageFullPath))
                {
                    RedundantFileCheckSumInfoFB infoFB = RedundantFileCheckSumInfoFB.GetRootAsRedundantFileCheckSumInfoFB(new ZeusFlatBuffers.ByteBuffer(File.ReadAllBytes(RedundantFileCheckSumInfoFB_OuterPackageFullPath)));
                    return CreateFrom(infoFB);
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            return null;
        }

        public static RedundantFileCheckSumInfo LoadTempInfosFromFB()
        {
            try
            {
                if (File.Exists(TempRedundantFileCheckSumInfoFB_OuterPackageFullPath))
                {
                    RedundantFileCheckSumInfoFB infoFB = RedundantFileCheckSumInfoFB.GetRootAsRedundantFileCheckSumInfoFB(new ZeusFlatBuffers.ByteBuffer(File.ReadAllBytes(TempRedundantFileCheckSumInfoFB_OuterPackageFullPath)));
                    return CreateFrom(infoFB);
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
            }
            return null;
        }
    }
}