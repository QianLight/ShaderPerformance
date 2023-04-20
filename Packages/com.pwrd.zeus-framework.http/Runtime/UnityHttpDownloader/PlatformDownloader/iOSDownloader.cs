/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_IOS
using System;
using UnityEngine;
using System.Runtime.InteropServices;//DllImport在此命名空间下

namespace Zeus.Framework.Http.UnityHttpDownloader.Platform
{
    class iOSDownloader : PlatformDownloader
    {
        [DllImport("__Internal")]
        private static extern void GenerateiOSDownloader(ulong identifier, string arg);

        [DllImport("__Internal")]
        private static extern void DeleteiOSDownloader(ulong identifier);

        [DllImport("__Internal")]
        private static extern void StartDownload(ulong identifier);

        [DllImport("__Internal")]
        private static extern void Refresh(ulong oriIdentifier, string newArg);

        [DllImport("__Internal")]
        private static extern long GetAvgDownloadSpeed(ulong identifier);
        [DllImport("__Internal")]
        private static extern long GetDownloadTime(ulong identifier);
        [DllImport("__Internal")]
        private static extern long GetTotalReceive(ulong identifier);
        [DllImport("__Internal")]
        private static extern long GetRealNeedDownloadSize(ulong identifier);
        [DllImport("__Internal")]
        private static extern bool GetIsAbort(ulong identifier);
        [DllImport("__Internal")]
        private static extern bool GetIsAborting(ulong identifier);

        [DllImport("__Internal")]
        private static extern void SetDownloadSpeedLimit(long limitSpeed);
        [DllImport("__Internal")]
        private static extern void EnableLimitDownloadSpeed(ulong identifier, bool enable);
        [DllImport("__Internal")]
        private static extern void SetOverrideRetryCountLimit(ulong identifier, int times);
        [DllImport("__Internal")]
        private static extern void SetIsMultiThread(ulong identifier, bool isMultiThread);
        [DllImport("__Internal")]
        private static extern void SetThreadLimit(ulong identifier, int threadLimit);
        [DllImport("__Internal")]
        private static extern void Abort(ulong identifier);
        [DllImport("__Internal")]
        private static extern long NativeCalcRealDownloadSize(string destPath, long targetSize);
        [DllImport("__Internal")]
        private static extern void SetAllowDownloadInBackground(ulong identifier, bool allow);
        [DllImport("__Internal")]
        private static extern void SetAllowCarrierDataNetworkDownload(ulong identifier, bool allow);
        [DllImport("__Internal")]
        private static extern void SetSucNotificationStr(ulong identifier, string suc);
        [DllImport("__Internal")]
        private static extern void SetFailNotificationStr(ulong identifier, string fail);
        [DllImport("__Internal")]
        private static extern void NativeShowNotification(string str);
        [DllImport("__Internal")]
        private static extern bool NativeIsCarrierDataNetwork();

        private int _error;
        private ulong _identifier;
        private bool alreadyDisposed = false;
        /// <summary>
        /// 位枚举
        /// </summary>
        public override int Error
        {
            get
            {
                return _error;
            }
        }

        public override int TopPriorityError
        {
            get
            {
                int bit = 0;
                int error = Error;
                if (error == 0)
                {
                    return 0;
                }
                while ((error /= 2) != 0)
                {
                    bit++;
                }
                return 1 << bit;
            }
        }

        /// <summary>
        /// 每秒下载字节数（平均速度）
        /// </summary>
        public override long AvgDownloadSpeed
        {
            get
            {
                return GetAvgDownloadSpeed(Identifier);
            }
        }

        /// <summary>
        /// 下载耗时(毫秒级)
        /// </summary>
        public override long DownloadTime
        {
            get
            {
                return GetDownloadTime(Identifier);
            }
        }

        public override long TotalReceived
        {
            get
            {
                return GetTotalReceive(Identifier);
            }
        }

        public override bool IsAbort
        {
            get
            {
                return GetIsAbort(Identifier);
            }
        }

        public override bool IsAborting
        {
            get
            {
                return GetIsAborting(Identifier);
            }
        }

        public override long RealNeedDownloadSize
        {
            get
            {
                return GetRealNeedDownloadSize(Identifier);
            }
        }

        private string JsonArg
        {
            get
            {
                string arg = JsonUtility.ToJson(Arg);
                return arg;
            }
        }

        public ulong Identifier
        {
            get
            {
                return _identifier;
            }
        }

        static iOSDownloader()
        {
            iOSDownloaderManager.SetTempStr(HttpDownloader.TempExtension, HttpDownloader.TempMarkStr);
        }

        public iOSDownloader(ulong identifier, DownloadArg arg) : base(arg)
        {
            _identifier = identifier;
            GenerateiOSDownloader(Identifier, JsonArg);
        }

        ~iOSDownloader()
        {
            Dispose();
        }

        public override void Dispose()
        {
            if (alreadyDisposed)
                return;
            alreadyDisposed = true;
            DeleteiOSDownloader(Identifier);
            GC.SuppressFinalize(this);
        }

        public override void Refresh(DownloadArg arg)
        {
            base.Refresh(arg);
            Refresh(Identifier, JsonArg);
        }

        public override void StartDownLoad()
        {
            StartDownload(Identifier);
        }

        /// <summary>
        /// 修改是否限速
        /// </summary>
        /// <param name="isLimit"></param>
        public override void EnableSpeedLimit(bool isLimit)
        {
            EnableLimitDownloadSpeed(Identifier, isLimit);
        }

        public override void SetOverrideRetryCountLimit(int times)
        {
            SetOverrideRetryCountLimit(Identifier, times);
        }

        public override void SetIsMultiThread(bool isMultiThread)
        {
            SetIsMultiThread(Identifier, isMultiThread);
        }

        public override void SetThreadLimit(int threadlimit)
        {
            SetThreadLimit(Identifier, threadlimit);
        }

        public override void Abort()
        {
            Abort(Identifier);
        }

        public void OnFinish(bool suc, int error)
        {
            _error = error;
            Arg.OnFinish(suc);
        }

        public override void SetAllowDownloadInBackground(bool allow)
        {
            SetAllowDownloadInBackground(Identifier, allow);
        }

        /// <summary>
        /// 是否允许移动网络下载
        /// </summary>
        /// <param name="allow"></param>
        public override void SetAllowCarrierDataNetworkDownload(bool allow)
        {
            SetAllowCarrierDataNetworkDownload(Identifier, allow);
        }

        public override void SetSucNotificationStr(string suc)
        {
            if(suc == null)
            {
                //此处为适应iOS的特殊处理
                suc = string.Empty;
            }
            if (Arg != null)
            {
                Arg.sucNotificationStr = suc;
            }
            SetSucNotificationStr(Identifier, suc);
        }

        public override void SetFailNotificationStr(string fail)
        { 
            if (fail == null)
            {
                //此处为适应iOS的特殊处理
                fail = string.Empty;
            }
            if (Arg != null)
            {
                Arg.failNotificationStr = fail;
            }
            SetFailNotificationStr(Identifier, fail);
        }

        public static void ShowNotification(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                NativeShowNotification(str);
            }
        }

        /// <summary>
        /// 修改限速值
        /// </summary>
        /// <param name="limitSpeed"></param>
        public static void SetLimitSpeed(long limitSpeed)
        {
            //iOS不支持限速
            SetDownloadSpeedLimit(limitSpeed);
        }

        /// <summary>
        /// 根据存储位置以及目标文件大小，计算剩余下载大小
        /// </summary>
        /// <param name="destPath"></param>
        /// <param name="targetSize"></param>
        /// <returns></returns>
        public static long CalcRealDownloadSize(string destPath, long targetSize)
        {
            destPath = FormatPathSeparator(destPath);
            return NativeCalcRealDownloadSize(destPath, targetSize);
        }

        public static bool IsCarrierDataNetwork()
        {
            return NativeIsCarrierDataNetwork();
        }
    }
}
#endif