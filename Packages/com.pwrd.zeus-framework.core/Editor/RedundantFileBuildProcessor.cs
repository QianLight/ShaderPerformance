/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using UnityEditor;
using UnityEngine;
using Zeus.Build;
using System.IO;
using Zeus.Framework;

namespace Zeus.Core.FileSystem
{
    public class RedundantFileBuildProcessor : IModifyPlayerSettings, IInternalBeforeBuild
    {
        public void OnModifyPlayerSettings(BuildTarget target)
        {
            RedundantFileCheckSumInfo.DeleteOldDataFile();
        }

        public void OnInternalBeforeBuild(BuildTarget target, string outputPath)
        {
            RedundantFileCheckSumInfo ckeckSumInfo = RedundantFileCheckSumInfo.CreateNewOrLoadInfos();
            //记录所有StreammingAssets中的文件
            if (Directory.Exists(Application.streamingAssetsPath))
            {
                string[] files = Directory.GetFiles(Application.streamingAssetsPath, "*", SearchOption.AllDirectories);
                foreach (var fullPath in files)
                {
                    if (fullPath.EndsWith(".meta") || fullPath.Contains(".svn") || fullPath.Contains(".git"))
                    {
                        continue;
                    }
                    if(fullPath.Contains("ZeusSetting"))
                    {
                        continue;
                    }
                    string relativePath = fullPath.Substring(Application.streamingAssetsPath.Length + 1);//加1是为了去掉分隔符
                    string old;
                    if (!ckeckSumInfo.TryGetCheckSum(relativePath, out old))//防止重复进行计算，浪费时间
                    {
                        ckeckSumInfo.Update(relativePath, fullPath, false);
                    }
                }
            }
            ckeckSumInfo.SaveFB();
        }
    }
}