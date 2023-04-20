/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/

using System.IO;
using System.Text;
using UnityEngine;
using Zeus.Core.FileSystem;
using System;
using System.Collections.Generic;

namespace Zeus.Core.ReportSystem
{
    //上报系统Json数据
    [Serializable]
    public struct ZeusReportConf
    {
        //是否开启上报系统
        public bool enable;
        //是否加密数据
        public bool encryptData;
        //上报间隔
        public int uploadInterval;
        //每次上报最大条数
        public int uploadSize;
        //终端
        public string endPoint;
        //项目名
        public string projectName;
        //渠道名
        public string channelName;
        //关闭上报的模块
        public List<string> bannedStores;
    }

    public static class ReportUtil
    {
        private static string _reportConfPath = VFileSystem.GetBuildinSettingPath("ZeusReportConfig.json");

        public static void SaveConf(ZeusReportConf conf)
        {
            string jsonPath = VFileSystem.GetRealPath(_reportConfPath);
            string directory = Path.GetDirectoryName(jsonPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string jsonData = JsonUtility.ToJson(conf, true);
            File.WriteAllText(jsonPath, jsonData, Encoding.UTF8);
        }

        public static ZeusReportConf LoadConf()
        {
            ZeusReportConf conf;
            if (VFileSystem.ExistsFile(_reportConfPath))
            {
                string content = VFileSystem.ReadAllText(_reportConfPath, Encoding.UTF8);
                conf = JsonUtility.FromJson<ZeusReportConf>(content);
            }
            else
            {
                conf = new ZeusReportConf();
                conf.bannedStores = new List<string>();
            }
            return conf;
        }

    }

}
