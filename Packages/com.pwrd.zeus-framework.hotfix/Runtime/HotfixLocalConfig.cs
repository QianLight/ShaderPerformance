/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus.Framework.Hotfix
{
    static class HotfixConfigVersion
    {
        public static string version = "";
    }
    public class HotfixLocalConfig
    {
        /// <summary>
        /// 这个需要是静态的，因为难以约束数据是否从磁盘上读取，只是修改实例的话，如果有操作是从新从磁盘上获取就会出错
        /// </summary>
        private static string m_overrideChannelId;
        public enum HotfixTestMode
        {
            Default = 0,//默认模式，根据标记文件（安卓及Editor）、后台配置的IDFA（ios）来确认是否下载测试数据
            Testing = 1,//测试优先模式
            OnlyNormal = 2,//仅正常模式
        }

        public List<string> serverUrls = new List<string>();

        /// <summary>
        /// 是否支持独立的热更控制文件的地址
        /// </summary>
        public bool independentControlDataUrl = false;

        /// <summary>
        /// 独立的热更控制文件地址
        /// </summary>
        public List<string> controlDataUrls = new List<string>();

        [SerializeField]
        private string channelId;
        /// <summary>
        /// Editor下这个字段的行为和直接操作channelId一致
        /// 非Editor下，不能写，只能读，且读的时候优先使用覆盖的值
        /// </summary>
        public string ChannelId
        {
            get
            {
#if UNITY_EDITOR
                return channelId;
#else
                if(string.IsNullOrEmpty(m_overrideChannelId))
                {
                    return channelId;
                }
                return m_overrideChannelId;
#endif
            }
#if UNITY_EDITOR
            set
            {
                channelId = value;
            }
#else
#endif
        }
        public string ver;
        public string Version
        {
            get
            {
                return ver;
            }
            set
            {
                HotfixConfigVersion.version = value;
                ver = value;
            }
        }
        public bool openHotfix;
        /// <summary>
        /// 是否自动预下载热更资源
        /// </summary>
        public bool autoPreDownload = false;
        /// <summary>
        /// 移动网络环境下是否允许预下载
        /// </summary>
        public bool allowPreDownloadOnCarrierDataNetwork = false;
        /// <summary>
        /// 预下载限速（kb/s）
        /// </summary>
        public int preDownloadSpeedLimit = 1024;
        /// <summary>
        /// url刷新参数
        /// </summary>
        public int urlRefreshParam = 2;

        public HotfixTestMode testMode = HotfixTestMode.Default;

        /// <summary>
        /// 如果向服务器请求远程版本信息时，得到404错误，即没有向后台上传此版本的安装包，没有生成相关版本的更新文件，此时是否忽略错误直接跳过热更
        /// </summary>
        public bool ignoreInit404Error = false;

        /// <summary>
        /// 当商店更新地址为空时是否跳过商店更新。
        /// true：跳过热更 false：依然执行商店更新的逻辑
        /// </summary>
        public bool ignoreEmptyAppStoreUrl = false;

        /// <summary>
        /// 使用实体文件存储包外版本
        /// </summary>
        public bool saveOuterPackageVersionInFile = true;

        /// <summary>
        /// 加密包外版本信息
        /// </summary>
        public bool encodeOuterPackageVersion = true;

        public static void OverrideChannelId(string channelId)
        {
            m_overrideChannelId = channelId;
        }
    }
}

