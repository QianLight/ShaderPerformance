using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GSDK
{
    public class DownloadSettings : ScriptableObject
    {
        private static DownloadSettings _instance;
        public DownloadModel Model = new DownloadModel();


        public static DownloadSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = CreateInstance<DownloadSettings>();
                }

                return _instance;
            }
        }
    }

    public class DownloadModel
    {
        public string GSDKVersion = "";

        public readonly Dictionary<string, bool> PluginMap = new Dictionary<string, bool>();

        public void RefreshRegionModel()
        {
            var plugins = DownloadConstants.GetPlugins();
            PluginMap.Clear();

            foreach (var plugin in plugins)
            {
                PluginMap.Add(plugin, false);
            }

            // 初始化gradle对应插件的map
        }
    }

    public static class DownloadConstants
    {
        public static string Region = "domestic";

        public static readonly Dictionary<string, string> OverseasGradlePluginMap = new Dictionary<string, string>
        {
            {"GMReplay", "screenrecord"}, {"GMVoice", "voice"}, {"GMShare", "share"},
            {"GMMagicBox", "debug_sdk"}, {"GMPointsSystem", "marketing"}, {"GMPoster", "webeditor"},
            {"GMDownload", "download"}, {"GMUpgrade", "upgrade"}, {"GMPush", "push"}, {"GPMMonitor", "gpm_monitor"},
            {"GMTranslate", "translate"}, {"GMAd", "ad"}, {"GMDiagnosis", "udp"}, {"GMAppStoreStar", "rating"},
            {"GMReactNative", "rn"}, {"GMMediaUpload", "mediaupload"}, {"GMLocation", "location"}, {"GMIM", "im"},
            {"GMThanos", "thanos"}, {"GMEmoticon", "emoticon"}, {"GMAdjust", "adjust"},
        };

        public static readonly Dictionary<string, string> DomesticGradlePluginMap = new Dictionary<string, string>
        {
            {"GMReplay", "screenrecord"}, {"GMVoice", "voice"}, {"GMShare", "share"}, {"GMMagicBox", "debug_sdk"},
            {"GMPointsSystem", "marketing"}, {"GMPoster", "webeditor"}, {"GMDownload", "download"},
            {"GMUpgrade", "upgrade"}, {"GPMMonitor", "gpm_monitor"}, {"GMDiagnosis", "udp"}, {"GMLive", "live"},
            {"GMPush", "push"}, {"GMAppStoreStar", "rating"}, {"GMReactNative", "rn"}, {"GMMediaUpload", "mediaupload"},
            {"GMLocation", "location"}, {"GMIM", "im"}, {"GMThanos", "thanos"}, {"GMEmoticon", "emoticon"},
        };


        public static readonly string[] DomesticPlugins =
        {
            "GMShare", "GMDownload", "GMPointsSystem", "GMReplay", "GMPoster", "GMUpgrade", "GMVoice",
            "GMPush", "GPMMonitor", "GMAppStoreStar", "GMReactNative", "GMMediaUpload", "GMLocation", "GMIM",
            "GMThanos",
            "GMMagicbox", "GMEmoticon", "GMLive", "GMDiagnosis"
        };

        public static readonly string[] OverseasPlugins =
        {
            "GMShare", "GMDownload", "GMAdjust", "GMReplay", "GMAd", "GMUpgrade", "GMVoice",
            "GMPush", "GPMMonitor", "GMAppStoreStar", "GMReactNative", "GMMediaUpload", "GMLocation", "GMIM",
            "GMThanos",
            "GMMagicbox", "GMEmoticon", "GMTranslate", "GMAIHelp", "GMDiagnosis"
        };

        public const string UrlPath = "https://lf1-snkcdn-cn.dailygn.com/obj/lf-game-lf/";
        public const string FileType = ".unitypackage";

        // 下载文件的目录
        public static readonly string CurrentDownloadPath =
            Directory.GetCurrentDirectory() + "/" + "DownloadPlugins" + "/";

        public static string[] GetPlugins()
        {
            return IsDomestic() ? DomesticPlugins : OverseasPlugins;
        }

        public static Dictionary<string, string> GetGradleMap()
        {
            return IsDomestic() ? DomesticGradlePluginMap : OverseasGradlePluginMap;
        }

        private static bool IsDomestic()
        {
            return Region.Equals("domestic");
        }
    }
}