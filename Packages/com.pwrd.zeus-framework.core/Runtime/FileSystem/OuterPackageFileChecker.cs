/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using Zeus.Framework;
using System.Threading;

namespace Zeus.Core.FileSystem
{
    public enum OuterPackageFileCheckType
    {
        /// <summary>
        /// 校验全部文件
        /// </summary>
        All = 0,
        /// <summary>
        /// 只校验首包文件
        /// </summary>
        FirstPackage = 1,
        /// <summary>
        /// 只校验分包文件
        /// </summary>
        SubPackage = 2,
    }

    public class OuterPackageFileCheckResult
    {
        /// <summary>
        /// 首包校验失败的文件
        /// </summary>
        public List<string> FirstPackageCheckFailFile = new List<string>();
        /// <summary>
        /// 首包丢失的文件
        /// </summary>
        public List<string> FirstPackageMissingFile = new List<string>();
        /// <summary>
        /// 分包校验失败的文件
        /// </summary>
        public List<string> SubPackageCheckFailFile = new List<string>();
        /// <summary>
        /// 分包丢失的文件
        /// </summary>
        public List<string> SubPackageMissingFile = new List<string>();
    }

    internal static class OuterPackageFileChecker
    {

        #region 测试代码
        //[UnityEditor.MenuItem("Zeus/FileSystem/CheckOuterPackageFile")]
        //public static void Test()
        //{
        //    OuterPackage.Init();
        //    CheckFiles((checkFailFiles) =>
        //    {
        //        UnityEngine.Debug.Log("streamingAssetsPath:" + Application.streamingAssetsPath);
        //        foreach (var item in checkFailFiles)
        //        {
        //            UnityEngine.Debug.Log("checkFailFiles:" + item);
        //        }

        //    });
        //}
        #endregion

        /// <summary>
        /// 校验包外文件(不能在Zeus框架的文件系统初始化前调用)
        /// </summary>
        /// <param name="onFinish">校验结束的回调。回调参数：校验失败文件的完整路径</param>
        /// <param name="checkType">All：校验全部文件；FirstPackage：只校验首包文件；SubPackage：只校验分包文件</param>
        public static void CheckFiles(Action<OuterPackageFileCheckResult> onFinish, OuterPackageFileCheckType checkType = OuterPackageFileCheckType.All)
        {
            if (onFinish == null)
            {
                UnityEngine.Debug.LogError("[OuterPackageFileChecker] onFinish can not be null");
                return;
            }

            ThreadPool.QueueUserWorkItem(Check,new object[] { onFinish, checkType });
        }

        private static void Check(object obj)
        {
            object[] paramArray = (object[])obj;

            Action< OuterPackageFileCheckResult> onFinish = (Action<OuterPackageFileCheckResult>)paramArray[0];
            OuterPackageFileCheckType checkType = (OuterPackageFileCheckType)paramArray[1];

            OuterPackageFileCheckResult result = new OuterPackageFileCheckResult();
            try
            {
                RedundantFileCheckSumInfo outer = RedundantFileCheckSumInfo.LoadOuterInfosFromFB();
                RedundantFileCheckSumInfo inner = RedundantFileCheckSumInfo.LoadInnerInfosFromFB();
                if (outer == null || inner == null)
                {
                    Debug.LogError($"try check outerpackage files but outer == null({outer == null})  inner == null({inner == null})");
                    if (ZeusCore.IsPlaying)
                    {
                        ZeusCore.Instance.AddMainThreadTask(() =>
                        {
                            onFinish(result);
                        });
                    }
                    else
                    {
                        onFinish(result);
                    }
                }
                else
                {
                    string outerPackage = PathUtil.FormatPathSeparator(OuterPackage.GetRealPath(string.Empty));
                    foreach (var item in outer.InfoDic)
                    {
                        if(outer.SubPackageFiles.Contains(item.Key))
                        {
                            if (checkType == OuterPackageFileCheckType.FirstPackage)
                            {
                                continue;
                            }
                            string fullPath = outerPackage + item.Key;
                            if(File.Exists(fullPath))
                            {
                                string recordMD5;
                                if (outer.TryGetCheckSum(item.Key, out recordMD5))
                                {
                                    string calcMD5 = MD5Util.GetMD5FromFile(fullPath);
                                    if (!recordMD5.Equals(calcMD5))
                                    {
                                        result.SubPackageCheckFailFile.Add(fullPath);
                                    }
                                }
                            }
                            else
                            {
                                result.SubPackageMissingFile.Add(item.Key);
                            }
                        }
                        else
                        {
                            if (checkType == OuterPackageFileCheckType.SubPackage)
                            {
                                continue;
                            }
                            string fullPath = outerPackage + item.Key;
                            if (File.Exists(fullPath))
                            {
                                string recordMD5;
                                if (outer.TryGetCheckSum(item.Key, out recordMD5))
                                {
                                    string calcMD5 = MD5Util.GetMD5FromFile(fullPath);
                                    if (!recordMD5.Equals(calcMD5))
                                    {
                                        result.FirstPackageCheckFailFile.Add(fullPath);
                                    }
                                }
                            }
                            else
                            {
                                string outerMD5;
                                if (outer.TryGetCheckSum(item.Key, out outerMD5))
                                {
                                    string innerMD5;
                                    if (inner.TryGetCheckSum(item.Key, out innerMD5))
                                    {
                                        if (!outerMD5.Equals(innerMD5))
                                        {
                                            result.FirstPackageMissingFile.Add(fullPath);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError(e);
            }
            if (ZeusCore.IsPlaying)
            {
                ZeusCore.Instance.AddMainThreadTask(() =>
                {
                    onFinish(result);
                });
            }
            else
            {
                onFinish(result);
            }
        }
    }
}
