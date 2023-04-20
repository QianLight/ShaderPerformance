/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.IO;

namespace Zeus.Framework.Http.UnityHttpDownloader.Platform
{
    abstract class PlatformDownloader : IDisposable
    {
        public abstract void EnableSpeedLimit(bool isLimit);
        public abstract int Error { get; }
        public abstract int TopPriorityError { get; }
        public abstract long AvgDownloadSpeed { get; }
        public abstract long DownloadTime { get; }
        public abstract long TotalReceived { get; }
        public abstract bool IsAbort { get; }
        public abstract bool IsAborting { get; }
        public abstract long RealNeedDownloadSize { get; }
        public abstract void StartDownLoad();
        public abstract void Abort();
        public abstract void SetOverrideRetryCountLimit(int times);
        public abstract void SetIsMultiThread(bool isMultiThread);
        public abstract void SetThreadLimit(int threadlimit);
        public abstract void SetAllowDownloadInBackground(bool allow);
        public abstract void SetAllowCarrierDataNetworkDownload(bool allow);
        public abstract void SetSucNotificationStr(string suc);
        public abstract void SetFailNotificationStr(string fail);
        public abstract void Dispose();

        protected DownloadArg Arg { get; private set; }
        public PlatformDownloader(DownloadArg arg)
        {
            Arg = arg;
        }
        public virtual void Refresh(DownloadArg arg)
        {
            Arg = arg;
        }


        /// <summary>
        /// 将路径的分隔符替换为系统默认分隔符
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string FormatPathSeparator(string path)
        {
            if (Path.DirectorySeparatorChar.Equals('/'))
            {
                path = path.Replace('\\', Path.DirectorySeparatorChar);
            }
            else
            {
                path = path.Replace('/', Path.DirectorySeparatorChar);
            }
            return path;
        }
    }
}