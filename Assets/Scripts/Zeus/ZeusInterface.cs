using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Zeus.Framework.Asset;
using Zeus.Core.FileSystem;
using Zeus.Framework;
using CFEngine;
using Zeus.Framework.Hotfix;
using Zeus.Core;
using System.IO;

public class AssetManagerInterface : IAssetManager
{
    public void Init()
    {
        AssetManager.Init();
        refDic.Clear();

        AssetManager.SetSucNotificationStr("下载成功");
        AssetManager.SetFailNotificationStr("下载失败");
        AssetManager.SetShowBackgroundDownloadProgress(true, "正在下载", "等待网络");
        AssetManager.SetIsAutoRetryDownloading(true);
        AssetManager.InitProcessedChunkDictAsync();
    }

    public Coroutine StartCoroutine(IEnumerator e)
    {
        return ZeusCore.Instance.StartCoroutine(e);
    }

    public void StopCoroutine(Coroutine c)
    {
        ZeusCore.Instance.StopCoroutine(c);
    }

    private string GetZeusPath(string path)
    {
        SimpleTools.StringToLower(ref path);
        path = path.Replace("assets/", "");
        return path;
    }

    public UnityEngine.Object GetAsset(string path, Type type)
    {
        string zeusPath = GetZeusPath(path);
        IAssetRef r = AssetManager.LoadAsset(zeusPath, type);
        if (r == null || r.AssetObject == null)
        {
            Debug.LogWarning("Zeus not find: " + path);
            return null;
        }
        refDic[path] = r;
        r.Retain();
        return r.AssetObject;
    }

    public void GetAssetAsync(string path, Type type, Action<UnityEngine.Object, object> callback, object param)
    {
        string zeusPath = GetZeusPath(path);
        AssetManager.LoadAssetAsync(path, type, (IAssetRef r, object o) =>
          {
              if (r == null)
              {
                  Debug.LogWarning("Zeus not find: " + path);
                  callback(null, o);
                  return;
              }
              refDic[path] = r;
              r.Retain();
              callback(r.AssetObject, o);
          },
        param);
    }

    Dictionary<string, IAssetRef> refDic = new Dictionary<string, IAssetRef>();
    public void ReleaseAsset(string path)
    {
        IAssetRef r = null;
        SimpleTools.StringToLower(ref path);
        if (refDic.TryGetValue(path, out r))
        {
            if (r != null)
            {
                r.Release();
                if (r.RefCount <= 0)
                    refDic.Remove(path);
            }
        }
    }

    public void LoadScene(string path,UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        AssetManager.LoadScene(path, mode);
    }

    public void LoadSceneAsync(string path, UnityEngine.SceneManagement.LoadSceneMode mode, Action<bool, float, object> callback, object param)
    {
        AssetManager.LoadSceneAsync(path, mode, callback, param);
    }

    public void UnloadSceneAsync(string path, Action<bool, float, object> callback, object param)
    {
        AssetManager.UnloadSceneAsync(path, callback, param);
    }

    #region Subpackage
    public bool IsSubpackageReady()
    {
        return AssetManager.IsSubpackageReady();
    }

    public bool IsSubpackageReady(string tag)
    {
        return AssetManager.IsSubpackageReady(tag);
    }

    public void DownloadSubpackageBundles()
    {
        AssetManager.DownloadSubpackageBundles(4, OnDownloading);
    }

    public void DownloadSubpackageBundles(string[] tags)
    {
        AssetManager.DownloadSubpackageBundles(4, OnDownloading, tags, false);
    }

    public void PauseDownloading()
    {
        AssetManager.PauseDownloading();
    }

    public void GetSubPackageSize(out double totalSize, out double completeSize)
    {
        AssetManager.GetSubPackageSize(out totalSize, out completeSize);
    }

    public Dictionary<string,double> GetTag2SizeDic()
    {
        return AssetManager.GetTag2SizeDic();
    }

    public double GetSizeToDownloadOfTag(string tag)
    {
        return AssetManager.GetSizeToDownloadOfTag(tag);
    }

    public double GetTagSize(string tag)
    {
        return AssetManager.GetTagSize(tag);
    }

    private double downloadprecent;
    public double DownloadPrecent { get { return downloadprecent; } }
    private int substatus;
    public int SubStatus { get { return substatus; } }
    private double _completeSize;
    public double CompleteSize { get { return _completeSize; } }
    private double _totalSize;
    public double TotalSize { get { return _totalSize; } }
    private double _avgDownloadSpeed;
    public double AvgDownloadSpeed { get { return _avgDownloadSpeed; } }
    private int subError;
    public int SubError { get { return subError; } }

    private void OnDownloading(double completedSize, double totalSize, double avgDownloadSpeed, SubpackageState state, SubpackageError error)
    {
        substatus = (int)state;
        downloadprecent = completedSize / totalSize;
        _completeSize = completedSize;
        _totalSize = totalSize;
        _avgDownloadSpeed = avgDownloadSpeed;
        subError = (int)error;
        //Debug.Log("Downloading: " + (completedSize / totalSize) + "(" + completedSize + ")");
    }
    #endregion

    public bool IsCarrierDataNetwork()
    {
        return Zeus.Framework.Http.UnityHttpDownloader.HttpDownloader.IsCarrierDataNetwork();
    }

    public void SaveRefCountLog()
    {
        AssetManager.Dump();
    }

    public void UnloadBundleAndReInit()
    {
        Zeus.Framework.Asset.AssetManager.UnloadBundleAndReInit();
    }
}

public class VFileSystemInterface : IVFileSystem
{
    public Stream OpenFile(string virtualPath, FileMode fileMode, FileAccess fileAccess)
    {
        try
        {
            return VFileSystem.OpenFile(virtualPath, fileMode, fileAccess);
        }
        catch
        {
            Debug.Log("VFileSystem Load Error :" + virtualPath);
        }
        return null;
    }

    public byte[] ReadAllBytes(string virtualPath)
    {
        try
        {
            return VFileSystem.ReadAllBytes(virtualPath);
        }
        catch
        {
            Debug.Log("VFileSystem Load Error :" + virtualPath);
        }
        return null;
    }

    public string GetRealPath(string virtualPath)
    {
        return VFileSystem.GetRealPath(virtualPath);
    }
}

public class OuterPackageInterface : IOuterPackage
{
    public bool Finish { get { return OuterPackage.Finish; } }

    public void RegistAfterClearOuterPackageFunc(Action action)
    {
        OuterPackage.RegistAfterClearOuterPackageFunc(action);
    }

    public void RegistCopyFileInner2OuterFunc(Func<List<string>> action)
    {
        OuterPackage.RegistCopyFileInner2OuterFunc(action);
    }
}

public class ZeusFrameworkInterface : IZeusFramework
{
    public void Start()
    {
        ZeusFramework.Start();
    }
}

public class HotfixServiceInterface : IHotfixService
{
    CFHotfixService hotfix;
    Action<int, long, long> callback;

    public string Version => hotfix?.CurVersion;

    public void SetupHotfixService()
    {
        hotfix = new CFHotfixService();
        hotfix.StartHotfix(callback);
    }

    public void RegistOnProcess(Action<int, long, long> action)
    {
        callback = action;
    }

    public long AvgDownloadSpeed { get { return hotfix != null ? hotfix.AvgDownloadSpeed : 0; } }
}

public class CFHotfixService : HotfixService
{
    // private long lastCurrent = 0;
    // private float timer = 0f;
    // private float timer2 = 0.1f;
    private bool needReload = false;
    Action<int, long, long> callback;

    public void StartHotfix(Action<int, long, long> action)
    {
        callback = action;
        needReload = false;
        RegistCallbacks();
        ZeusCore.Instance.StartCoroutine(this.Execute());
    }

    public override void OnProcess(HotfixStep step, long current, long target)
    {
        callback((int)step, current, target);
        string title = string.Empty;
        switch (step)
        {
//             case HotfixStep.Init:
//                 ui.Title = "正在初始化版本信息";
//                 ui.Speed = string.Empty;
//                 break;
//             case HotfixStep.Check:
//                 ui.Title = "版本对比中";
//                 ui.Speed = string.Empty;
//                 break;
//             case HotfixStep.Download:
//                 ui.Title = "下载中[" + +(current / 1024) + "kb/" + (target / 1024) + "kb]";
//                 timer += Time.deltaTime;
//                 timer2 += Time.deltaTime;
//                 if (timer > 0f && timer2 >= 0.5f)
//                 {
//                     timer2 -= 0.5f;
//                     ui.Speed = "CurSpeed:" + Mathf.Max(0, (current - lastCurrent) / timer / 1024).ToString("F2") + "kb/s,AvgSpeed:" +
//                         ((float)AvgDownloadSpeed / 1024).ToString("F2") + "kb/s,CostTime:" + DownloadTime + "ms";
//                 }
//                 if (timer >= 1f)
//                 {
//                     timer -= 1f;
//                     lastCurrent = current;
//                 }
//                 break;
//             case HotfixStep.Unzip:
//                 ui.Title = "解压中[" + +(current / 1024) + "kb/" + (target / 1024) + "kb]";
//                 ui.Speed = string.Empty;
//                 break;
        }
    }
    /// <summary>
    /// 只有推荐更新的情况下才会调用此函数,使得玩家拥有选择的余地
    /// </summary>
    /// <param name="tips"></param>
    /// <param name="yesOrNo"></param>
    /// <param name="param"></param>
    public override void OnChoice(HotfixTips tips, WaitForYesOrNo yesOrNo, object param)
    {
        string tipStr = string.Empty;
        switch (tips)
        {
            case HotfixTips.RecommendDownload:
                tipStr = "推荐更新，当前版本：" + CurVersion + ",目标版本：" + TargetVersion + ",文件大小：" + ((long)param / 1024).ToString() + "kb";
                //true 准备下载
                //false 跳过更新
                break;
            case HotfixTips.HardDiskFull:
                //true 重新检查磁盘空间
                //false 跳过更新
                tipStr = "存储空间不足，需要存储空间：" + ((long)param / 1024).ToString() + "kb";
                break;
            case HotfixTips.PreDownload:
                tipStr = "是否预下载热更版本，当前版本：" + CurVersion + ",目标版本：" + TargetVersion + ",文件大小：" + ((long)param / 1024).ToString() + "kb";
                //true 开始预下载
                //false 不进行预下载，跳过更新
                break;
        }
        yesOrNo.YesOrNo(true);
        //         ui.tip = tipStr;
        //         ui._yesOrNo = yesOrNo;
    }

    public string GetCapacityValue(long capacity)
    {
        float value;
        if (capacity < 1024 * 1024)
        {
            value = capacity / 1024f;
            return string.Format("{0}KB", value.ToString("F2"));
        }
        else
        {
            value = capacity / 1024f / 1024f;
            return string.Format("{0}MB", value.ToString("F2"));
        }
    }

    /// <summary>
    /// 强制玩家确认的情况会调用此函数
    /// </summary>
    /// <param name="tips"></param>
    /// <param name="trigger"></param>
    /// <param name="param"></param>
    public override void OnConfirm(HotfixTips tips, WaitForTrigger trigger, object param)
    {
        string tipStr = string.Empty;
        switch (tips)
        {
            case HotfixTips.AppStoreRecommandDownload:
            case HotfixTips.AppStoreDownload:
                tipStr = "游戏已更新，请前往应用商店更新到最新版本";
                break;
            case HotfixTips.PreDownload:
            case HotfixTips.RecommendDownload:
            case HotfixTips.ForceDownload:
                tipStr = string.Format("检查到有资源更新，是否下载更新资源!\n文件大小：{0}", GetCapacityValue((long)param));
                //tipStr = "强制更新，当前版本：" + CurVersion + ",目标版本：" + TargetVersion + ",文件大小：" + ((long)param / 1024).ToString() + "kb";
                break;
            case HotfixTips.HardDiskFull:
                tipStr = "存储空间不足，需要存储空间：" + GetCapacityValue((long)param);
                break;
        }

        XUpdater.XUpdater.singleton.HotfixConfirm(tipStr, () => { trigger.Trigger(); });
        //ui.tip = tipStr;
        //ui._trigger = trigger;
    }

    /// <summary>
    /// 提示玩家发生错误并点击确认重试
    /// </summary>
    /// <param name="error"></param>
    /// <param name="trigger"></param>
    public override void OnErrorConfirm(HotfixError error, WaitForTrigger trigger, object param)
    {
        string errorStr = string.Empty;
        //此处根据需要，对错误类型加以适当的提示文本
        switch (error)
        {
            case HotfixError.NetError:
                errorStr = "网络连接异常，无法连接服务器，请重试";
                break;
            case HotfixError.InitError:
                errorStr = "初始化热更版本数据失败，请重试";
                break;
            case HotfixError.HardDiskFull:
                errorStr = "存储空间不足，请重试，需要空间：" + GetCapacityValue((long)param);
                break;
            case HotfixError.DownloadError:
                errorStr = "下载热更文件失败，本次更新为强制更新,请确认重试";
                break;
            default:
                errorStr = "更新异常，请重试";
                break;
        }
        XUpdater.XUpdater.singleton.HotfixConfirm(errorStr, () => { trigger.Trigger(); });
        //ui.tip = errorStr;
        //ui._trigger = trigger;
    }


    /// <summary>
    /// 提示玩家发生错误，选择重试或者跳过，只有推荐更新时会调用此函数
    /// </summary>
    /// <param name="error"></param>
    /// <param name="yesOrNo"></param>
    public override void OnErrorChoice(HotfixError error, WaitForYesOrNo yesOrNo, object param)
    {
        //yes:重试   no:跳过


        string errorStr = string.Empty;
        //此处根据需要，对错误类型加以适当的提示文本
        switch (error)
        {
            case HotfixError.DownloadError:
                errorStr = "下载热更文件失败，请重试";
                break;
            case HotfixError.HttpStatusCode404Error:
                errorStr = "网络连接异常，无法获取服务器数据，请重试";
                break;
            default:
                errorStr = "更新异常，请重试";
                break;
        }
        XUpdater.XUpdater.singleton.HotfixConfirm(errorStr, () => { yesOrNo.YesOrNo(true); });
        //ui.tip = errorStr;
        //ui._yesOrNo = yesOrNo;
    }

    public override void OnFinish()
    {
        callback((int)HotfixStep.Finish, 1, 1);

        UnregistCallbacks();
//         ui.Title = "热更结束";
//         ui.tip = string.Empty;
//         ui.Speed = string.Empty;
        if (needReload)
        {
            //清理已经加载的bundle及其路径缓存，以便下次加载资源的时候使用最新的bundle
            //加载完资源调用UnloadAll(false) 确保之后加载最新资源
            Zeus.Framework.Asset.AssetManager.UnloadBundleAndReInit();
        }
    }

    #region 预下载相关接口：如果某些场景对性能及网络要求较高，可使用以下接口，用于在运行过程中对预下载进行状态查询以及开关控制
    /// <summary>
    /// 尝试开启预下载热更文件并返回结果
    /// </summary>
    /// <returns>true：开启了预下载  false：未开启预下载</returns>
    public bool TryStartPreDownload()
    {
        return base.StartPreDownload();
    }
    /// <summary>
    /// 是否正处于预下载热更文件的状态(暂停状态也会返回true)
    /// </summary>
    /// <returns></returns>
    public bool CheckIsPreDownloading()
    {
        return base.IsPreDownloading();
    }
    /// <summary>
    /// 预下载是否处于暂停状态
    /// </summary>
    /// <returns></returns>
    public bool CheckIsPreDownloadPause()
    {
        return base.IsPreDownloadPause();
    }
    /// <summary>
    /// 暂停预下载（可恢复）
    /// </summary>
    /// <returns></returns>
    public void TryPausePreDownload()
    {
        base.PausePreDownload();
    }
    /// <summary>
    /// 恢复预下载
    /// </summary>
    /// <returns></returns>
    public void TryResumePreDownload()
    {
        base.ResumePreDownload();
    }
    /// <summary>
    /// 停止预下载热更文件（停止后不可恢复）
    /// </summary>
    public void TryStopPreDownload()
    {
        base.StopPreDownLoad();
    }

    /// <summary>
    /// 查询热更信息，
    /// （1）正常的更新：根据返回的信息正确处理就可以了
    /// （2）预下载更新：如果热更的配置文件内开启了自动预下载，则会自动进行预下载并返回无更新；如果没有开启自动预下载，则返回更新内容，
    ///                  调用方检测到返回的信息为预下载时，可以查询是否在进行预下载，来选择是否重新走热更流程，因为设置中未开启自动预下载时，
    ///                  热更会做UI层提示给玩家，最终会由玩家决定本次是否启动预下载。注：如果当前预下载内容已经开始下载了，则返回无更新。
    /// </summary>
    /// <param name="success">成功回调,参数1：热更类型  参数2：热更包大小  参数3：是否为预下载</param>
    /// <param name="fail">失败回调</param>
    /// <param name="serverUrls">自定义的查询地址，会覆盖包内配置文件的url</param>
    /// <param name="customTargetVersion">自定义查询目标版本，如果有值且与本地版本一致则无更新</param>
    public static void CustomQueryHotfixInfo(System.Action<HotFixType, double, bool> success, System.Action<HotfixError> fail, string urls = null, string customTargetVersion = null)
    {
        QueryHotfixInfo(success, fail, urls, customTargetVersion);
    }
    #endregion

    #region callback
    private void RegistCallbacks()
    {
        RegistOnInit(OnInit);
        RegistOnDownloadStart(OnDownloadStart);
        RegistOnDownloadSuc(OnDownloadSuc);
        RegistOnDownloadFail(OnDownloadFail);
    }

    private void UnregistCallbacks()
    {
        UnregistOnInit(OnInit);
        UnregistOnDownloadStart(OnDownloadStart);
        UnregistOnDownloadSuc(OnDownloadSuc);
        UnregistOnDownloadFail(OnDownloadFail);
    }

    private void OnInit(HotFixType type, string version)
    {
        Debug.Log("Oninit:" + type + "," + version);
    }

    private void OnDownloadStart(string[] url)
    {
        Debug.Log("OnDownloadStart:" + url[0]);
    }

    private void OnDownloadSuc(string[] url, int time)
    {
        Debug.Log("OnDownloadSuc:" + url[0] + ",time:" + time);
    }

    private void OnDownloadFail(string[] url, double size, string error, int time)
    {
        Debug.Log("OnDownloadFail:" + url[0] + ",size:" + size + ",error:" + error + ",time:" + time);
    }
    #endregion
}
