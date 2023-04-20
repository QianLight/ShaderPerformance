/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_IOS
using System.Collections.Generic;
using System.Runtime.InteropServices;//DllImport在此命名空间下
using System.Reflection;

namespace Zeus.Framework.Http.UnityHttpDownloader.Platform
{
    class iOSDownloaderManager
    {
        private static ulong _identifierKey = 0;

        private delegate void FinishCallback(ulong identifier, bool suc, int error);

        private static Dictionary<ulong, iOSDownloader> downloaderDic = new Dictionary<ulong, iOSDownloader>();

        [DllImport("__Internal")]
        private static extern void SetNetworkStatusObseverHost(string host);

        [DllImport("__Internal")]
        public static extern void SetTempStr(string TempExtension, string TempMarkStr);

        [DllImport("__Internal")]
        private static extern void RegistFinishCallback(FinishCallback callback);

        static iOSDownloaderManager()
        {
            SetTempStr(HttpDownloader.TempExtension, HttpDownloader.TempMarkStr);
            RegistFinishCallback(OnFinish);
        }

        [AOT.MonoPInvokeCallback(typeof(FinishCallback))]
        private static void OnFinish(ulong identifier, bool suc, int error)
        {
            iOSDownloader downloader;
            if (downloaderDic.TryGetValue(identifier, out downloader))
            {
                downloader.OnFinish(suc, error);
            }
        }

        private static ulong genIdentifier()
        {
            ulong identifier = _identifierKey;
            if (_identifierKey == ulong.MaxValue)
            {
                _identifierKey = 0;
            }
            else
            {
                _identifierKey++;
            }
            return identifier;
        }

        /// <summary>
        /// 设置用于检测网络状态的url，支持多个url，以分号(;)分割
        /// </summary>
        /// <param name="urls"></param>
        public static void SetNetworkStatusObseverUrls(string urls)
        {
            SetNetworkStatusObseverHost(urls);
        }

        public static iOSDownloader CreateDownloader(DownloadArg arg)
        {
            iOSDownloader downloader = new iOSDownloader(genIdentifier(), arg);
            downloaderDic[downloader.Identifier] = downloader;
            return downloader;
        }

        public static void RemoveDownloader(ulong identifier)
        {
            downloaderDic.Remove(identifier);
        }

        #region 通知相关
        public delegate void CheckPermissionCallback(bool result);

        [DllImport("__Internal")]
        private static extern void CheckNotificationPermission(CheckPermissionCallback callback);
        [DllImport("__Internal")]
        private static extern void GoToNotificationPermissionSetting();

        private static System.Action<bool> _callback;
        private static bool _checking = false;

        [AOT.MonoPInvokeCallback(typeof(CheckPermissionCallback))]
        private static void OnCheckNotificationPermission(bool result)
        {
            //此处不能直接用_callback(result)的方式调用回调，否则IL2CPP运行到此会报不支持的错误。
            FieldInfo field = typeof(iOSDownloaderManager).GetField("_callback", BindingFlags.Static | BindingFlags.NonPublic);
            object o = field.GetValue(null);
            if (o != null)
            {
                o.GetType().GetMethod("Invoke").Invoke(o, new object[] { result });
                field.SetValue(null, null);//清空回调
            }
            _checking = false;
        }

        /// <summary>
        /// 检查是否获取通知权限
        /// </summary>
        /// <param name="callback">回调</param>
        public static void TryCheckNotificationPermission(System.Action<bool> callback)
        {
            if (_callback == null)
            {
                _callback = callback;
            }
            else
            {
                _callback += callback;
            }

            if (!_checking)
            {
                _checking = true;
                CheckNotificationPermission(OnCheckNotificationPermission);
            }
        }

        /// <summary>
        /// 跳转到系统的通知权限设置界面
        /// </summary>
        public static void JumpNotificationPermissionSetting()
        {
            GoToNotificationPermissionSetting();
        }
        #endregion
    }
}
#endif