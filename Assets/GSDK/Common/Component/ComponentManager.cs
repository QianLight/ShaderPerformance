using System.Collections.Generic;
using System.IO;
using UNBridgeLib.LitJson;
using UnityEngine;
using NotImplementedException = System.NotImplementedException;

namespace GSDK
{
    public enum ComponentName
    {
        Main,
        RN,
        GameLiveSDK,
        RTC,
        Replay,
        GameRoom,
        Account,
        Voice,
        Share,
        IM,
        Emoticon,
        VideoShare,
        AppScore,
        ForceUpdate,
        Live,
        Thanos,
        Adjust,
        Translate,
        Boost,
        Download,
        Ad,
        UDP,
        SSL,
        MediaUpload,
        Push,
        Poster,
        Location,
        DeepLink,
        QRCode,
        AppsFlyer,
        Firebase,
        AdPangle,
        ASR,
        Webview,
        CloudGame,
        GNA,
        GEAS,
        Compliance,
        AdAttribution,
        Pay
    }

    public class ComponentInfo
    {
        public ComponentInfo(ComponentName name, string androidName, string unityName)
        {
            this.name = name;
            this.androidName = androidName;
            this.unityName = unityName;
        }

        /// <summary>
        /// 统一组件名称
        /// </summary>
        public readonly ComponentName name;

        /// <summary>
        /// android平台的组件名称
        /// </summary>
        public readonly string androidName;

        /// <summary>
        /// unity平台的组件名称
        /// </summary>
        public readonly string unityName;

        /// <summary>
        /// 版本号
        /// </summary>
        public string version = null;
    }

    public class ComponentManager
    {
        /// <summary>
        /// Android组件名称和三端统一对外的映射关系
        /// </summary>
        public static readonly Dictionary<string, ComponentInfo> ComponentInfos = new Dictionary<string, ComponentInfo>
        {
            // GSDK 主包
            {ComponentName.Main.ToString(), new ComponentInfo(ComponentName.Main, "", "")},
            // 九尾组件
            {ComponentName.RN.ToString(), new ComponentInfo(ComponentName.RN, "rn", "GMReactNative")},
            // 游戏直播组件
            {ComponentName.GameLiveSDK.ToString(), new ComponentInfo(ComponentName.GameLiveSDK, "gamelivesdk", "")},
            // RTC组件
            {ComponentName.RTC.ToString(), new ComponentInfo(ComponentName.RTC, "rtc", "GMRTC")},
            // 视频录制
            {ComponentName.Replay.ToString(), new ComponentInfo(ComponentName.Replay, "screenrecord", "GMReplay")},
            // 游戏房
            {ComponentName.GameRoom.ToString(), new ComponentInfo(ComponentName.GameRoom, "gameroom", "GMGameRoom")},
            // 账号模块
            {ComponentName.Account.ToString(), new ComponentInfo(ComponentName.Account, "account", "GMAccount")},
            // 语音消息
            {ComponentName.Voice.ToString(), new ComponentInfo(ComponentName.Voice, "voice", "GMVoice")},
            // 分享
            {ComponentName.Share.ToString(), new ComponentInfo(ComponentName.Share, "share", "GMShare")},
            // 即时通信
            {ComponentName.IM.ToString(), new ComponentInfo(ComponentName.IM, "im", "GMIM")},
            // 表情模块
            {ComponentName.Emoticon.ToString(), new ComponentInfo(ComponentName.Emoticon, "emoticon", "GMEmoticon")},
            // 积分系统
            {ComponentName.VideoShare.ToString(), new ComponentInfo(ComponentName.VideoShare, "marketing", "GMPointsSystem")},
            // 应用评价
            {ComponentName.AppScore.ToString(), new ComponentInfo(ComponentName.AppScore, "rating", "GMAppStoreStar")},
            // 更新下载
            {ComponentName.ForceUpdate.ToString(), new ComponentInfo(ComponentName.ForceUpdate, "upgrade", "GMUpgrade")},
            // 直播互动玩法
            {ComponentName.Live.ToString(), new ComponentInfo(ComponentName.Live, "live", "GMLive")},
            // 省流量更新
            {ComponentName.Thanos.ToString(), new ComponentInfo(ComponentName.Thanos, "thanos", "GMThanos")},
            // Adjust
            {ComponentName.Adjust.ToString(), new ComponentInfo(ComponentName.Adjust, "adjust", "GMAdjust")},
            // 文本翻译
            {ComponentName.Translate.ToString(), new ComponentInfo(ComponentName.Translate, "translate", "GMTranslate")},
            // 性能加速
            {ComponentName.Boost.ToString(), new ComponentInfo(ComponentName.Boost, "boost", "")},
            // 下载能力
            {ComponentName.Download.ToString(), new ComponentInfo(ComponentName.Download, "download", "GMDownload")},
            // Ad
            {ComponentName.Ad.ToString(), new ComponentInfo(ComponentName.Ad, "ad", "GMAd")},
            // 网络诊断工具
            {ComponentName.UDP.ToString(), new ComponentInfo(ComponentName.UDP, "udp", "GMDiagnosis")},
            // 安全协议加密
            {ComponentName.SSL.ToString(), new ComponentInfo(ComponentName.SSL, "agreement", "GMAgreement")},
            // 图片上传与审核
            {ComponentName.MediaUpload.ToString(), new ComponentInfo(ComponentName.MediaUpload, "mediaupload", "GMMediaUpload")},
            // 消息推送
            {ComponentName.Push.ToString(), new ComponentInfo(ComponentName.Push, "push", "GMPush")},
            // 社区发布组件
            {ComponentName.Poster.ToString(), new ComponentInfo(ComponentName.Poster, "webeditor", "GMPoster")},
            // 位置服务
            {ComponentName.Location.ToString(), new ComponentInfo(ComponentName.Location, "location", "GMLocation")},
            // DeepLink
            {ComponentName.DeepLink.ToString(), new ComponentInfo(ComponentName.DeepLink, "deeplink", "GMDeepLink")},
            // 扫码组件
            {ComponentName.QRCode.ToString(), new ComponentInfo(ComponentName.QRCode, "qrcode", "GMQRCode")},
            // AppsFlyer数据归因
            {ComponentName.AppsFlyer.ToString(), new ComponentInfo(ComponentName.AppsFlyer, "appsflyer", "GMAppsFlyer")},
            // Firebase
            {ComponentName.Firebase.ToString(), new ComponentInfo(ComponentName.Firebase, "firebase", "GMFirebase")},
            // 穿山甲聚合广告
            {ComponentName.AdPangle.ToString(), new ComponentInfo(ComponentName.AdPangle, "adpangle", "GMAdPangle")},
            // 语音转文字
            {ComponentName.ASR.ToString(), new ComponentInfo(ComponentName.ASR, "asr", "GMVoice")},
            // Webview
            {ComponentName.Webview.ToString(), new ComponentInfo(ComponentName.Webview, "webview", "")},
            // 云游戏
            {ComponentName.CloudGame.ToString(), new ComponentInfo(ComponentName.CloudGame, "cloudgame", "CloudGame")},
            // 游戏网络加速
            {ComponentName.GNA.ToString(), new ComponentInfo(ComponentName.GNA, "gna", "GNA")},
            // 游戏体验分析服务
            {ComponentName.GEAS.ToString(), new ComponentInfo(ComponentName.GEAS, "geas", "GEAS")},
            // 合规组件
            {ComponentName.Compliance.ToString(), new ComponentInfo(ComponentName.Compliance, "compliance", "Compliance")}
        };

//         private string GetComponentVersion(ComponentName component)
//         {
//             // 1. 检查是否缓存
//             ComponentInfo targetComponentInfo = ComponentInfos[component.ToString()];
//             if (targetComponentInfo == null)
//             {
//                 GLog.LogDebug("尚未配置该组件信息：" + component);
//                 return "";
//             }
//             else if (targetComponentInfo.version != null)
//             {
//                 return targetComponentInfo.version;
//             }
//
//
//             // 2. 设置默认值，保证只解析一次json文件内容
//             foreach (var componentInfo in ComponentInfos.Values)
//             {
//                 componentInfo.version = "";
//             }
//
//             // 3. 解析gsdk.json文件，获取组件的版本号
// #if UNITY_ANDROID
//             string versionFilePath = "jar:file://" + Application.dataPath + "!/assets/" + "gsdk.json";
// #else
//             string versionFilePath = Path.Combine(Application.dataPath, @"GSDK/gsdk.json");
// #endif
//             if (!File.Exists(versionFilePath))
//             {
//                 // gsdk.json文件不存在
//                 GLog.LogDebug("gsdk.json文件不存在，路径为：" + versionFilePath, "wzs");
//             }
//             else
//             {
//                 string content = File.ReadAllText(versionFilePath);
//                 JsonData json = JsonMapper.ToObject(content);
//                 if (json["dsl"] != null && json["dsl"]["components"] != null)
//                 {
//                     GLog.LogDebug("gsdk.json文件内容配置dsl和components元素不为空。");
//                     JsonData jsonData = json["dsl"]["components"];
//                     foreach (JsonData data in jsonData)
//                     {
//                         string name = data.ContainsKey("name") ? (string) data["name"] : "";
//                         ComponentInfo target = ComponentInfos[name];
//                         if (target != null)
//                         {
//                             string version = data.ContainsKey("version") ? (string) data["version"] : "";
//                             target.version = version;
//                         }
//                     }
//
//                     GLog.LogDebug("gsdk.json文件内容解析成功。");
//                     return ComponentInfos[component.ToString()].version;
//                 }
//             }
//
//             GLog.LogDebug("gsdk.json文件内容配置不正确，使用默认值。");
//             return "";
//         }
    }
}