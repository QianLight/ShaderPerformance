/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using Zeus.Build;

namespace Zeus.Framework.Hotfix
{
    class HotfixBuildProcessor : IModifyPlayerSettings
    {
        public void OnModifyPlayerSettings(BuildTarget target)
        {
            HotfixLocalConfig config = HotfixLocalConfigHelper.LoadLocalConfig();

            bool openHotfix = false;
            if (CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.HOTFIX_OPEN, ref openHotfix))
            {
                config.openHotfix = openHotfix;
            }
            string url = string.Empty, channel = string.Empty, version = string.Empty, testMode = string.Empty;
            int index = 0;
            List<string> urlList = new List<string>();
            while (CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.HOTFIX_SERVER_URL + index.ToString(), ref url))
            {
                urlList.Add(url);
                ++index;
            }
            urlList.RemoveAll(t => string.IsNullOrEmpty(t));
            if (urlList.Count > 0)
            {
                config.serverUrls = urlList;
            }
            bool independentControlDataUrl = config.independentControlDataUrl;
            if (CommandLineArgs.TryGetBool(GlobalBuild.CmdArgsKey.HOTFIX_INDEPENDENT_CONTROL_DATA_URL, ref independentControlDataUrl))
            {
                config.independentControlDataUrl = independentControlDataUrl;
            }
            if (config.independentControlDataUrl)
            {
                index = 0;
                List<string> controlDataUrlList = new List<string>();
                while (CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.HOTFIX_CONTROL_DATA_URL + index.ToString(), ref url))
                {
                    controlDataUrlList.Add(url);
                    ++index;
                }
                controlDataUrlList.RemoveAll(t => string.IsNullOrEmpty(t));
                if (controlDataUrlList.Count > 0)
                {
                    config.controlDataUrls = controlDataUrlList;
                }
            }
            if (CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.HOTFIX_CHANNEl, ref channel))
            {
                config.ChannelId = channel;
            }
            if (CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.HOTFIX_VERSION, ref version))
            {
                config.Version = version;
            }
            if (CommandLineArgs.TryGetString(GlobalBuild.CmdArgsKey.HOTFIX_TEST_MODE, ref testMode))
            {
                if (System.Enum.IsDefined(typeof(HotfixLocalConfig.HotfixTestMode), testMode))
                {
                    config.testMode = (HotfixLocalConfig.HotfixTestMode)System.Enum.Parse(typeof(HotfixLocalConfig.HotfixTestMode), testMode, true);
                }
                else
                {
                    UnityEngine.Debug.LogError("[CommandLineArgs] HotfixLocalConfig.HotfixTestMode Wrong:" + testMode + ",Will Use Default Mode.");
                }
            }

            Zeus.Framework.Hotfix.HotfixLocalConfigHelper.SaveLocalConfig(config);
        }
    }
}
#endif