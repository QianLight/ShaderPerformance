using System;

namespace GSDK
{
    public enum ServiceType
    {
        App,
        ABTest,
        Account,
        Bulletin,
        CdKey, //营销兑换
        Channel,
        Download,
        Emoticon,
        FriendChain,
        GPMMonitor,
        IM,
        Location,
        MediaUpload,
        Pay,
        GameNetDiagnosis,
        NetExperience,
        NetMna,
        NetMpa,
        GnaClient,
        AgeGate,
        AntiAddiction,
        Cookie,
        Protocol,
        Compliance,
        GameLive, //游戏直播
        PointsSystem,
        RecommendContact,
        Live, //游戏互动玩法
        DeepLink,
        // AntiAddiction,
        AdPangle,
        GameRoom, //游戏房

        RealName, 
        Privacy,
        Push,
        Rating,
        ReactNative,
        Replay,
        Report,
        Role,
        Share,
        System,
        Security,
        Voice,
        RTC,
        Webview,
        Diagnosis,
#if UNITY_ANDROID
        Thanos,
#endif
        Geas,
        MagicBox,
        Upgrade,
        Agreement,
        QRCode,
        InstallPopup,
#if UNITY_ANDROID
        CloudGame,
#endif
        VideoUpload,
    }


    public interface IService
    {
    }

    public interface IServiceProvider
    {
        IService GetService(ServiceType service, string moduleName = "");
    }
}