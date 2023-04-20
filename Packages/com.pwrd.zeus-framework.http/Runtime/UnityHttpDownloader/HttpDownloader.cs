/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.IO;
using System;
using Zeus.Framework.Http.UnityHttpDownloader.Platform;

namespace Zeus.Framework.Http.UnityHttpDownloader
{
    public class HttpDownloader : IDisposable
    {
        private bool alreadyDisposed = false;
        private PlatformDownloader _downloader;
        private bool _allowDownloadInBackground = true;
        private bool _allowCarrierDataNetworkDownload = true;

        public const string TempExtension = ".temp";
        public const string TempMarkStr = "_temp_";
        public const string TempExtensionFilter = "*.temp";


        /// <summary>
        /// 位枚举
        /// </summary>
        public int Error
        {
            get
            {
                return _downloader.Error;
            }
        }

        public ErrorType TopPriorityError
        {
            get
            {
                return (ErrorType)_downloader.TopPriorityError;
            }
        }

        /// <summary>
        /// 每秒下载字节数（平均速度）
        /// </summary>
        public long AvgDownloadSpeed
        {
            get
            {
                return _downloader.AvgDownloadSpeed;
            }
        }

        /// <summary>
        /// 下载耗时
        /// </summary>
        public long DownloadTime
        {
            get
            {
                return _downloader.DownloadTime;
            }
        }

        public long TotalReceived
        {
            get
            {
                return _downloader.TotalReceived;
            }
        }

        public bool IsAbort
        {
            get
            {
                return _downloader.IsAbort;
            }
        }

        public bool IsAborting
        {
            get
            {
                return _downloader.IsAborting;
            }
        }

        public long RealNeedDownloadSize
        {
            get
            {
                return _downloader.RealNeedDownloadSize;
            }
        }

        public HttpDownloader(DownloadArg arg)
        {
#if UNITY_EDITOR
            _downloader = new UnityDownloader(arg);
#elif UNITY_ANDROID
            _downloader = new AndroidDownloader(arg);
#elif UNITY_IOS
            _downloader = iOSDownloaderManager.CreateDownloader(arg);
#else
            _downloader = new UnityDownloader(arg);
#endif
            _downloader.SetAllowDownloadInBackground(_allowDownloadInBackground);
            _downloader.SetAllowCarrierDataNetworkDownload(_allowCarrierDataNetworkDownload);
        }

        ~HttpDownloader()
        {
            Dispose(true);
        }

        public void Refresh(DownloadArg arg)
        {
            _downloader.Refresh(arg);
        }

        public void StartDownLoad()
        {
            _downloader.StartDownLoad();
        }


        /// <summary>
        /// 修改是否限速
        /// </summary>
        /// <param name="speed"></param>
        public void EnableSpeedLimit(bool isLimit)
        {
            _downloader.EnableSpeedLimit(isLimit);
        }

        /// <summary>
        /// 修改最大重试次数
        /// </summary>
        /// <param name="times"></param>
        public void SetOverrideRetryCountLimit(int times)
        {
            _downloader.SetOverrideRetryCountLimit(times);
        }

        /// <summary>
        /// 修改是否多线程下载，如果当前downloader正在下载也会立即生效
        /// </summary>
        /// <param name="speed"></param>
        public void SetIsMultiThread(bool isMultiThread)
        {
            _downloader.SetIsMultiThread(isMultiThread);
        }
        /// <summary>
        /// 修改是否线程数限制，如果当前downloader为多线程模式会生效
        /// </summary>
        /// <param name="speed"></param>
        public void SetThreadLimit(int limit)
        {
            if (limit > 0)
            {
                _downloader.SetThreadLimit(limit);
            }
        }

        public void Abort()
        {
            _downloader.Abort();
        }

        public void SetAllowDownloadInBackground(bool allow)
        {
            if (_allowDownloadInBackground != allow)
            {
                _allowDownloadInBackground = allow;
                _downloader.SetAllowDownloadInBackground(allow);
            }
        }

        /// <summary>
        /// 是否允许移动网络下载，编辑器状态不生效
        /// </summary>
        /// <param name="allow"></param>
        public void SetAllowCarrierDataNetworkDownload(bool allow)
        {
            if (_allowCarrierDataNetworkDownload != allow)
            {
                _allowCarrierDataNetworkDownload = allow;
                _downloader.SetAllowCarrierDataNetworkDownload(allow);
            }
        }

        public void SetSucNotificationStr(string suc)
        {
            _downloader.SetSucNotificationStr(suc);
        }

        public void SetFailNotificationStr(string fail)
        {
            _downloader.SetFailNotificationStr(fail);
        }

        /// <summary>
        /// 修改限速值
        /// </summary>
        /// <param name="speed"></param>
        public static void SetLimitSpeed(long speed)
        {
#if UNITY_EDITOR
            UnityDownloader.SetLimitSpeed(speed);
#elif UNITY_ANDROID
            AndroidDownloader.SetLimitSpeed(speed);
#elif UNITY_IOS
            iOSDownloader.SetLimitSpeed(speed);
#else
            UnityDownloader.SetLimitSpeed(speed);
#endif
        }

        /// <summary>
        /// 根据存储位置以及目标文件大小，计算剩余下载大小
        /// </summary>
        /// <param name="destPath"></param>
        /// <param name="targetSize"></param>
        /// <returns></returns>
        public static long CalcRealDownloadSize(string destPath, long targetSize)
        {
#if UNITY_EDITOR
            return UnityDownloader.CalcRealDownloadSize(destPath, targetSize);
#elif UNITY_ANDROID
            return AndroidDownloader.CalcRealDownloadSize(destPath, targetSize);
#elif UNITY_IOS
            return iOSDownloader.CalcRealDownloadSize(destPath, targetSize);
#else
            return UnityDownloader.CalcRealDownloadSize(destPath, targetSize);
#endif
        }

        /// <summary>
        /// 根据存储位置以及目标文件大小，获取文件已经下载的大小
        /// </summary>
        /// <param name="destPath"></param>
        /// <param name="targetSize"></param>
        /// <returns></returns>
        public static long GetAlreadyDownloadSize(string destPath, long targetSize)
        {
#if UNITY_EDITOR
            return targetSize - UnityDownloader.CalcRealDownloadSize(destPath, targetSize);
#elif UNITY_ANDROID
            return targetSize - AndroidDownloader.CalcRealDownloadSize(destPath, targetSize);
#elif UNITY_IOS
            return targetSize - iOSDownloader.CalcRealDownloadSize(destPath, targetSize);
#else
            return targetSize - UnityDownloader.CalcRealDownloadSize(destPath, targetSize);
#endif
        }

        public static bool TryGetFileNameWithoutExtensionFromTempFile(string tempFilePath, out string fileName)
        {
            if (tempFilePath.Contains(TempMarkStr))
            {
                string tempFileName = Path.GetFileName(tempFilePath);
                fileName = tempFileName.Substring(0, tempFileName.IndexOf(TempMarkStr));
                return true;
            }
            fileName = string.Empty;
            return false;
        }

        public static bool IsCarrierDataNetwork()
        {
#if UNITY_EDITOR
            return UnityDownloader.IsCarrierDataNetwork();
#elif UNITY_ANDROID
            return AndroidDownloader.IsCarrierDataNetwork();
#elif UNITY_IOS
            return iOSDownloader.IsCarrierDataNetwork();
#else
            return UnityDownloader.IsCarrierDataNetwork();
#endif
        }

        /// <summary>
        /// 设置用于检测网络状态的url，支持多个url，以分号(;)分割,仅用于iOS平台
        /// </summary>
        /// <param name="urls"></param>
        public static void SetNetworkStatusObseverUrls(string urls)
        {
#if !UNITY_EDITOR && UNITY_IOS
            iOSDownloaderManager.SetNetworkStatusObseverUrls(urls);
#endif
        }

        /// <summary>
        /// 检查是否获取通知权限
        /// </summary>
        /// <param name="callback">回调</param>
        public static void TryCheckNotificationPermission(System.Action<bool> callback)
        {
#if !UNITY_EDITOR
#if UNITY_IOS
            iOSDownloaderManager.TryCheckNotificationPermission(callback);
#elif UNITY_ANDROID
            AndroidDownloader.TryCheckNotificationPermission(callback);
#endif
#endif
        }

        /// <summary>
        /// 跳转到系统的通知权限设置界面
        /// </summary>
        public static void JumpNotificationPermissionSetting()
        {
#if !UNITY_EDITOR
#if UNITY_IOS
            iOSDownloaderManager.JumpNotificationPermissionSetting();
#elif UNITY_ANDROID
            AndroidDownloader.JumpNotificationPermissionSetting();
#endif
#endif
        }

        public static int ShowNotification(string str)
        {
#if !UNITY_EDITOR
#if UNITY_IOS
            iOSDownloader.ShowNotification(str);
#elif UNITY_ANDROID
            return AndroidDownloader.ShowNotification(str);
#endif
#endif
            return 0;
        }

        public static int ShowNotification(int id,string str)
        {
#if !UNITY_EDITOR
#if UNITY_IOS
#elif UNITY_ANDROID
            return AndroidDownloader.ShowNotification(id,str);
#endif
#endif
            return 0;
        }

        public static int ShowProgressNotification(int id, string title, string desc, int max, int progress, bool indeterminate)
        {
#if !UNITY_EDITOR
#if UNITY_IOS
#elif UNITY_ANDROID
            return AndroidDownloader.ShowProgressNotification(id, title, desc, max, progress, indeterminate);
#endif
#endif
            return 0;
        }

        public static void CancelNotification(int id)
        {
#if !UNITY_EDITOR
#if UNITY_IOS
#elif UNITY_ANDROID
            AndroidDownloader.CancelNotification(id);
#endif
#endif
        }

        /// <summary>
        /// 设置推送的图标及其颜色。注：仅安卓平台生效
        /// 原因：在国外某些手机上，app的推送的图标会出现纯白色的异常，原因可见以下网址，故增加smallicon和smallicon的rgb设置接口。 https://blog.csdn.net/SImple_a/article/details/103594842?utm_medium=distribute.wap_relevant.none-task-blog-2%7Edefault%7EBlogCommendFromMachineLearnPai2%7Edefault-1.wap_blog_relevant_pic&depth_1-utm_source=distribute.wap_relevant.none-task-blog-2%7Edefault%7EBlogCommendFromMachineLearnPai2%7Edefault-1.wap_blog_relevant_pic
        /// </summary>
        /// <param name="name">图标名</param>
        /// <param name="type">文件夹</param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static void SetNotificationSmallIcon(string name, string type, int r, int g, int b)
        {
#if !UNITY_EDITOR
#if UNITY_IOS
#elif UNITY_ANDROID
            AndroidDownloader.SetNotificationSmallIcon(name, type, r, g, b);
#endif
#endif
        }

        #region 某些版本的Unity在子线程创建或者获取Java的类对象会报找不到类型的错误，因此采用先在主线程获取类对象，再在子线程调用其函数的方式
#if !UNITY_EDITOR && UNITY_ANDROID
        private static long _refrence = 0;
        private static object _refrenceLock = new object();
        /// <summary>
        /// 某些版本的Unity在子线程创建或者获取Java的类对象会报找不到类型的错误，
        /// 因此采用先在主线程获取类对象，再在子线程调用其函数的方式
        /// </summary>
        public static void GenAndroidJavaClass()
        {
            lock (_refrenceLock)
            {
                if (_refrence == 0)
                {
                    AndroidDownloader.GenAndroidJavaClass();
                }
                _refrence++;
            }
        }
        public static void DisposeAndroidJavaClass()
        {
            lock (_refrenceLock)
            {
                _refrence--;
                if (_refrence < 0)
                {
                    _refrence = 0;
                }
                if (_refrence == 0)
                {
                    AndroidDownloader.DisposeAndroidJavaClass();
                }
            }
        }
#endif

        #endregion
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            //Don't dispose more than once
            if (alreadyDisposed)
                return;
            if (isDisposing)
            {
#if UNITY_EDITOR
#elif UNITY_ANDROID
#elif UNITY_IOS
                //iOS 平台比较特殊，需要移除引用以确保不会内存泄漏
                iOSDownloaderManager.RemoveDownloader((_downloader as iOSDownloader).Identifier);
#else
#endif  
                _downloader.Dispose();
                _downloader = null;
            }
            //elied:free unmanaged resource here
            //set disposed flag:
            alreadyDisposed = true;
        }
    }
}
