/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_ANDROID
using UnityEngine;
using System;

namespace Zeus.Framework.Http.UnityHttpDownloader.Platform
{
    class AndroidDownloader : PlatformDownloader
    {
        static AndroidJavaClass _iDownloadArgClass;
        static AndroidJavaClass _downloadHelperClass;
        static AndroidJavaClass _internetUtilClass;
        static AndroidJavaClass _notificationUtilClass;

        public static void GenAndroidJavaClass()
        {
            if (_iDownloadArgClass == null)
            {
                _iDownloadArgClass = new AndroidJavaClass("com.zeus.zeusnativedownloader.IDownloadArg");
            }
            if (_downloadHelperClass == null)
            {
                _downloadHelperClass = new AndroidJavaClass("com.zeus.zeusnativedownloader.DownloadHelper");
            }
            if (_internetUtilClass == null)
            {
                _internetUtilClass = new AndroidJavaClass("com.zeus.zeusnativedownloader.InternetUtil");
            }
            if (_notificationUtilClass == null)
            {
                _notificationUtilClass = new AndroidJavaClass("com.zeus.zeusnativedownloader.NotificationUtil");
            }
            try
            {
                using (var backgroundUtil = new AndroidJavaClass("com.zeus.zeusnativedownloader.BackgroundUtil"))
                {
                    backgroundUtil.CallStatic<bool>("IsBackGround");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public static void DisposeAndroidJavaClass()
        {
            if (_iDownloadArgClass != null)
            {
                _iDownloadArgClass.Dispose();
                _iDownloadArgClass = null;
            }
            if (_downloadHelperClass != null)
            {
                _downloadHelperClass.Dispose();
                _downloadHelperClass = null;
            }
            if (_internetUtilClass != null)
            {
                _internetUtilClass.Dispose();
                _internetUtilClass = null;
            }
            if (_notificationUtilClass != null)
            {
                _notificationUtilClass.Dispose();
                _notificationUtilClass = null;
            }
            try
            {
                using (var backgroundUtil = new AndroidJavaClass("com.zeus.zeusnativedownloader.BackgroundUtil"))
                {
                    backgroundUtil.CallStatic("UnRegistActivityLifecycleCallbacks");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public class DownloadArgProxy : AndroidJavaProxy
        {
            static DownloadArgProxy()
            {
                if (_iDownloadArgClass == null)
                {
                    _iDownloadArgClass = new AndroidJavaClass("com.zeus.zeusnativedownloader.IDownloadArg");
                }
            }
            private DownloadArg _arg;
            public DownloadArgProxy(DownloadArg arg) : base(_iDownloadArgClass)
            {
                _arg = arg;
            }

            public string[] GetSourceUrls()
            {
                return _arg.sourceUrls;
            }
            public string GetDestPath()
            {
                return _arg.destPath;
            }
            public string GetSucNotificationStr()
            {
                return _arg.sucNotificationStr;
            }
            public string GetFailNotificationStr()
            {
                return _arg.failNotificationStr;
            }
            public bool GetIsGroupTask()
            {
                return _arg.isGroupTask;
            }
            public bool GetIsMultiThread()
            {
                return _arg.isMultiThread;
            }
            public int GetThreadLimit()
            {
                return _arg.threadLimit;
            }
            public bool IsLimitSpeed()
            {
                return _arg.enableSpeedLimit;
            }
            public long GetTargetSize()
            {
                return _arg.targetSize;
            }
            public bool GetSplitFile()
            {
                return _arg.IsPartialFile;
            }
            public long GetFromIndex()
            {
                return _arg.fromIndex;
            }
            public long GetToIndex()
            {
                return _arg.toIndex;
            }
            public int GetCheckAlgorithmType()
            {
                if (_arg.checkAlgorithm != null)
                {
                    return (int)_arg.checkAlgorithm.checkAlgorithmType;
                }
                return (int)CheckAlgorithmType.None;
            }
            public string GetMD5()
            {
                if (_arg.checkAlgorithm != null)
                {
                    return _arg.checkAlgorithm.md5;
                }
                return null;
            }
            public int GetCRC32()
            {
                if (_arg.checkAlgorithm != null)
                {
                    return _arg.checkAlgorithm.crc32;
                }
                return 0;
            }
            public void OnComplete(bool result)
            {
                _arg.OnFinish(result);
            }
        }

        AndroidJavaObject _javaObject;
        DownloadArgProxy _proxy;
        private bool alreadyDisposed = false;
        /// <summary>
        /// 位枚举
        /// </summary>
        public override int Error
        {
            get
            {
                if (_javaObject == null)
                {
                    return 0;
                }
                return _javaObject.Call<int>("Error");
            }
        }

        public override int TopPriorityError
        {
            get
            {
                if (_javaObject == null)
                {
                    return 0;
                }
                return _javaObject.Call<int>("TopPriorityError");
            }
        }

        /// <summary>
        /// 每秒下载字节数（平均速度）
        /// </summary>
        public override long AvgDownloadSpeed
        {
            get
            {
                if (_javaObject == null)
                {
                    return 0;
                }
                return _javaObject.Call<long>("AvgDownloadSpeed");
            }
        }

        /// <summary>
        /// 下载耗时
        /// </summary>
        public override long DownloadTime
        {
            get
            {
                if (_javaObject == null)
                {
                    return 0;
                }
                return _javaObject.Call<long>("DownloadTime");
            }
        }

        public override long TotalReceived
        {
            get
            {
                if (_javaObject == null)
                {
                    return 0;
                }
                return _javaObject.Call<long>("TotalReceived");
            }
        }

        public override bool IsAbort
        {
            get
            {
                if (_javaObject == null)
                {
                    return true;
                }
                return _javaObject.Call<bool>("IsAbort");
            }
        }

        public override bool IsAborting
        {
            get
            {
                if (_javaObject == null)
                {
                    return true;
                }
                return _javaObject.Call<bool>("IsAborting");
            }
        }

        public override long RealNeedDownloadSize
        {
            get
            {
                if (_javaObject == null)
                {
                    return 0L;
                }
                return _javaObject.Get<long>("RealNeedDownloadSize");
            }
        }

        static AndroidDownloader()
        {
            if (_downloadHelperClass == null)
            {
                _downloadHelperClass = new AndroidJavaClass("com.zeus.zeusnativedownloader.DownloadHelper");
            }
            _downloadHelperClass.SetStatic("TempMarkStr", HttpDownloader.TempMarkStr);
            _downloadHelperClass.SetStatic("TempExtension", HttpDownloader.TempExtension);
        }

        public AndroidDownloader(DownloadArg arg) : base(arg)
        {
            _proxy = new DownloadArgProxy(Arg);
            if (_downloadHelperClass == null)
            {
                _downloadHelperClass = new AndroidJavaClass("com.zeus.zeusnativedownloader.DownloadHelper");
            }
            _javaObject = _downloadHelperClass.CallStatic<AndroidJavaObject>("CreateDownloader", _proxy);
        }

        public override void Refresh(DownloadArg arg)
        {
            base.Refresh(arg);
            if (_javaObject != null)
            {
                _proxy = new DownloadArgProxy(Arg);
                _javaObject.Call("Refresh", _proxy);
            }
        }

        public override void StartDownLoad()
        {
            if (_javaObject != null)
            {
                _javaObject.Call("StartDownLoad");
            }
        }

        /// <summary>
        /// 修改是否限速
        /// </summary>
        /// <param name="speed"></param>
        public override void EnableSpeedLimit(bool isLimit)
        {
            if (_javaObject != null)
            {
                _javaObject.Call("EnableSpeedLimit", isLimit);
            }
        }

        public override void SetOverrideRetryCountLimit(int times)
        {
            if (_javaObject != null)
            {
                _javaObject.Call("SetOverrideRetryCountLimit", times);
            }
        }

        public override void SetIsMultiThread(bool isMultiThread)
        {
            if (_javaObject != null)
            {
                _javaObject.Call("SetIsMultiThread", isMultiThread);
            }
        }

        public override void SetThreadLimit(int threadlimit)
        {
            if (_javaObject != null)
            {
                _javaObject.Call("SetThreadLimit", threadlimit);
            }
        }

        public override void Abort()
        {
            if (_javaObject != null)
            {
                _javaObject.Call("Abort");
            }
        }

        public override void SetAllowDownloadInBackground(bool allow)
        {
            if (_javaObject != null)
            {
                _javaObject.Call("SetAllowDownloadInBackground", allow);
            }
        }

        /// <summary>
        /// 是否允许移动网络下载，编辑器状态不生效
        /// </summary>
        /// <param name="allow"></param>
        public override void SetAllowCarrierDataNetworkDownload(bool allow)
        {
            if (_javaObject != null)
            {
                _javaObject.Call("SetAllowCarrierDataNetworkDownload", allow);
            }
        }

        public override void SetSucNotificationStr(string suc)
        {
            if (Arg != null)
            {
                Arg.sucNotificationStr = suc;
            }
            if (_javaObject != null)
            {
                _javaObject.Call("SetSucNotificationStr", suc);
            }
        }

        public override void SetFailNotificationStr(string fail)
        {
            if (Arg != null)
            {
                Arg.failNotificationStr = fail;
            }
            if (_javaObject != null)
            {
                _javaObject.Call("SetFailNotificationStr", fail);
            }
        }

        /// <summary>
        /// 修改限速值
        /// </summary>
        /// <param name="speed"></param>
        public static void SetLimitSpeed(long limitSpeed)
        {
            if (_downloadHelperClass == null)
            {
                _downloadHelperClass = new AndroidJavaClass("com.zeus.zeusnativedownloader.DownloadHelper");
            }
            _downloadHelperClass.CallStatic("SetLimitSpeed", limitSpeed);
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
            if (_downloadHelperClass == null)
            {
                _downloadHelperClass = new AndroidJavaClass("com.zeus.zeusnativedownloader.DownloadHelper");
            }
            return _downloadHelperClass.CallStatic<long>("CalcRealDownloadSize", destPath, targetSize);
        }

        public static bool IsCarrierDataNetwork()
        {
            if (_internetUtilClass == null)
            {
                _internetUtilClass = new AndroidJavaClass("com.zeus.zeusnativedownloader.InternetUtil");
            }
            return _internetUtilClass.CallStatic<bool>("IsCarrierDataNetwork");
        }

        /// <summary>
        /// 检查是否获取通知权限
        /// </summary>
        /// <param name="callback">回调</param>
        public static void TryCheckNotificationPermission(System.Action<bool> callback)
        {
            if (_notificationUtilClass == null)
            {
                _notificationUtilClass = new AndroidJavaClass("com.zeus.zeusnativedownloader.NotificationUtil");
            }
            callback(_notificationUtilClass.CallStatic<bool>("IsNotificationAllow"));
        }

        /// <summary>
        /// 跳转到系统的通知权限设置界面
        /// </summary>
        public static void JumpNotificationPermissionSetting()
        {
            if (_notificationUtilClass == null)
            {
                _notificationUtilClass = new AndroidJavaClass("com.zeus.zeusnativedownloader.NotificationUtil");
            }
            _notificationUtilClass.CallStatic("GoToNotificationSetting");
        }

        public static int ShowNotification(string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                if (_notificationUtilClass == null)
                {
                    _notificationUtilClass = new AndroidJavaClass("com.zeus.zeusnativedownloader.NotificationUtil");
                }
                return _notificationUtilClass.CallStatic<int>("ShowNotification", str);
            }
            else
            {
                UnityEngine.Debug.LogError("Notification Str Can Not Be Empty Or Null.");
            }
            return 0;
        }

        public static int ShowNotification(int id, string str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                if (_notificationUtilClass == null)
                {
                    _notificationUtilClass = new AndroidJavaClass("com.zeus.zeusnativedownloader.NotificationUtil");
                }
                return _notificationUtilClass.CallStatic<int>("ShowNotification", id, str);
            }
            else
            {
                UnityEngine.Debug.LogError("Notification Str Can Not Be Empty Or Null.");
            }
            return id;
        }

        public static int ShowProgressNotification(int id, string title, string desc, int max, int progress, bool indeterminate)
        {
            if (_notificationUtilClass == null)
            {
                _notificationUtilClass = new AndroidJavaClass("com.zeus.zeusnativedownloader.NotificationUtil");
            }
            return _notificationUtilClass.CallStatic<int>("ShowProgressNotification", id, title, desc, max, progress, indeterminate);
        }

        public static void CancelNotification(int id)
        {
            if (_notificationUtilClass == null)
            {
                _notificationUtilClass = new AndroidJavaClass("com.zeus.zeusnativedownloader.NotificationUtil");
            }
            _notificationUtilClass.CallStatic("CancelNotification", id);
        }

        /// <summary>
        /// 设置推送的图标及其颜色。
        /// 原因：在国外某些手机上，app的推送的图标会出现纯白色的异常，原因可见以下网址，故增加smallicon和smallicon的rgb设置接口。 https://blog.csdn.net/SImple_a/article/details/103594842?utm_medium=distribute.wap_relevant.none-task-blog-2%7Edefault%7EBlogCommendFromMachineLearnPai2%7Edefault-1.wap_blog_relevant_pic&depth_1-utm_source=distribute.wap_relevant.none-task-blog-2%7Edefault%7EBlogCommendFromMachineLearnPai2%7Edefault-1.wap_blog_relevant_pic
        /// </summary>
        /// <param name="name">图标名</param>
        /// <param name="type">文件夹</param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        public static void SetNotificationSmallIcon(string name, string type, int r, int g, int b)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(type))
            {
                r = Mathf.Clamp(r, 0, 255);
                g = Mathf.Clamp(g, 0, 255);
                b = Mathf.Clamp(b, 0, 255);
                if (_notificationUtilClass == null)
                {
                    _notificationUtilClass = new AndroidJavaClass("com.zeus.zeusnativedownloader.NotificationUtil");
                }
                _notificationUtilClass.CallStatic("SetNotificationSmallIcon", name, type, r, g, b);
            }
        }

        public override void Dispose()
        {
            if (alreadyDisposed)
                return;

            alreadyDisposed = true;
            if (_javaObject != null)
            {
                _javaObject.Dispose();
                _javaObject = null;
            }
            GC.SuppressFinalize(this);
        }

        ~AndroidDownloader()
        {
            Dispose();
        }
    }
}
#endif
