/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using Zeus.Core;

namespace Zeus.Framework.Hotfix
{
    public enum HotfixStep
    {
        None,
        CheckAndFinishLastHotfix,
        RequestPatchData,
        Check,
        Download,
        Unzip,
        Report,
        Finish,
    }

    public enum HotfixError
    {
        Null,
        /// <summary>
        /// 网络连接异常，无法连接服务器
        /// </summary>
        NetError,
        /// <summary>
        /// 初始化热更版本数据失败
        /// </summary>
        InitError,
        /// <summary>
        /// 服务器异常，返回的channel、version与本地不一致
        /// </summary>
        ServerError,
        /// <summary>
        /// 存储空间不足
        /// </summary>
        HardDiskFull,
        /// <summary>
        /// 下载热更文件失败
        /// </summary>
        DownloadError,
        /// <summary>
        /// 下载的热更文件校验失败
        /// </summary>
        CheckFail,
        /// <summary>
        /// 解压失败
        /// </summary>
        UnzipError,

        Exception,
        /// <summary>
        /// 404错误
        /// </summary>
        HttpStatusCode404Error,
        /// <summary>
        /// 没有找到Resversion，本地没有，并且创建失败
        /// </summary>
        NoResVersionData,
    }

    public enum HotfixTips
    {
        /// <summary>
        /// 跳转商店下载
        /// </summary>
        AppStoreDownload,
        /// <summary>
        /// 强制下载，用户确认后下载
        /// </summary>
        ForceDownload,
        /// <summary>
        /// 推荐下载，可跳过,用户确认后下载
        /// </summary>
        RecommendDownload,
        /// <summary>
        /// 空间不足
        /// </summary>
        HardDiskFull,
        /// <summary>
        /// 还未到更新时间，是否提前预下载，当不允许自动开启预下载时会发出此提示
        /// </summary>
        PreDownload,
        /// <summary>
        /// 跳转商店推荐下载
        /// </summary>
        AppStoreRecommandDownload,
    }

    public abstract class BaseHotFixStep
    {
        protected HotfixService _hotFixExecuter;
        HotfixStep _step;
        public HotfixStep Step { get { return _step; } }
        int RetryTimes;

        public BaseHotFixStep(HotfixService executer, HotfixStep step, int retry = 3)
        {
            _hotFixExecuter = executer;
            _step = step;
            RetryTimes = retry;
        }

        public abstract void Run();

        public void NextStep()
        {
            _hotFixExecuter.NextStep();
        }

        public void Finish()
        {
            _hotFixExecuter.Finish();
        }

        private bool Retry()
        {
            if (RetryTimes > 0)
            {
                RetryTimes--;
                Run();
                return true;
            }
            return false;
        }

        protected void OnProcess(long current, long target)
        {
            _hotFixExecuter.OnProcess(Step, current, target);
        }

        protected void OnChoice(HotfixTips tips, WaitForYesOrNo yesOrNo, object param = null)
        {
            if (param == null)
            {
                param = 0L;
            }
            _hotFixExecuter.OnChoice(tips, yesOrNo, param);
        }

        protected void OnConfirm(HotfixTips tips, WaitForTrigger trigger, object param = null)
        {
            if (param == null)
            {
                param = 0L;
            }
            _hotFixExecuter.OnConfirm(tips, trigger, param);
        }

        protected void OnError(HotfixError error, string msg, object param = null)
        {
            if (!Retry())
            {
                _hotFixExecuter.Error(new HotfixException(msg, error, param));
            }
        }

        public virtual void Update() { }
    }
}
