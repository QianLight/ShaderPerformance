/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;

namespace Zeus.Framework.Http.UnityHttpDownloader
{
    [Serializable]
    public class DownloadArg
    {
        private const int URL_COUNT_FACTOR = 2;
        [UnityEngine.SerializeField]
        private DownloadType downloadType;
        /// <summary>
        /// 后台状态下，所有任务下载成功时，如果有值，会将此值作为内容显示一个通知
        /// </summary>
        public string sucNotificationStr;
        /// <summary>
        /// 后台状态下，下载失败时，如果有值，会将此值作为内容显示一个通知
        /// </summary>
        public string failNotificationStr;

        /// <summary>
        /// True：多文件下载时使用，会将很多文件统筹管理；False：单文件下载时使用。
        /// </summary>
        public bool isGroupTask = false;

        public string[] sourceUrls;//目标文件链接
        private int _urlIndex = 0;
        public string destPath;//下载后保存目标文件的位置
        public bool isMultiThread = false;//是否启用多线程
        public int threadLimit = -1;//多线程下载的线程数限制
        public bool enableSpeedLimit = false;//是否限制下载速度
        public long targetSize = -1;//目标文件的尺寸
        public long fromIndex = -1;//下载任务从哪里开始
        public long toIndex = -1;//下载任务从哪里结束
        public CheckAlgorithm checkAlgorithm;//校验算法
        public Action<bool> callback;//回调

        /// <summary>
        /// false:下载整个文件   true:下载文件的某一部分（用于从多个小文件合成的大文件内拆分下载小文件）
        /// </summary>
        public bool IsPartialFile { get { return downloadType == DownloadType.PartialFile; } }
        
        /// <summary>
        /// 当index小于传入的url数组长度时，直接按index返回数组中的url，当index越界时，返回越界部分对应的url加刷新参数
        /// </summary>
        public string CurUrl
        {
            get
            {
                if(_urlIndex >= sourceUrls.Length)
                {
                    return sourceUrls[_urlIndex - sourceUrls.Length] + $"?Time={DateTime.UtcNow.Year}{DateTime.UtcNow.Month}{DateTime.UtcNow.Day}{DateTime.UtcNow.Hour}{DateTime.UtcNow.Minute}";
                }
                return sourceUrls[_urlIndex];
            }
        }

        public int MaxUrlCount
        {
            get
            {
                return sourceUrls.Length * URL_COUNT_FACTOR;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="_finishNotificationStr">后台状态下，下载完成所有任务时，如果有值，会将此值作为内容显示一个通知</param>
        public DownloadArg(DownloadType type, string sucNotificationStr = null, string failNotificationStr = null)
        {
            this.sucNotificationStr = sucNotificationStr;
            this.failNotificationStr = failNotificationStr;
            downloadType = type;
        }

        public void ResetIndex()
        {
            _urlIndex = 0;
        }

        public void SwitchNextUrl()
        {
            _urlIndex = (_urlIndex + 1) % (sourceUrls.Length * URL_COUNT_FACTOR);
        }

        public void OnFinish(bool result)
        {
            if(!result)
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder();
                for (int i = 0; i < sourceUrls.Length; i++)
                {
                    builder.Append("URL");
                    builder.Append(i);
                    builder.Append(':');
                    builder.Append(sourceUrls[i]);
                }
                UnityEngine.Debug.LogError(string.Format("[HttpDwonloader] Dwonload Fail.{0}",builder.ToString()));
            }
            if (callback != null)
            {
                callback(result);
            }
        }
    }
}
