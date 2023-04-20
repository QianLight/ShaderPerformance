/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Zeus.Core.ReportSystem
{
    #region 上传数据结构
    internal class ReportData
    {
        [SerializeField]
        internal string __topic__ = null;
        [SerializeField]
        internal List<ReportLog> __logs__ = new List<ReportLog>();
        [SerializeField]
        internal ReportTags __tags__ = new ReportTags();

        [NonSerialized]
        internal string _store = null;

        //获取最后一条Log的ReportId
        internal int GetLastReportId()
        {
            if (__logs__.Count > 0)
            {
                return __logs__[__logs__.Count - 1].reportId;
            }

            return 0;
        }

        internal void Reset()
        {
            __topic__ = null;
            __logs__.Clear();
            _store = null;
        }

        internal void SetProject(string project)
        {
            __tags__.pro = project;
        }

        internal void SetChannel(string channel)
        {
            __tags__.chn = channel;
        }

        internal void AddLog(ReportLog log)
        {
            __logs__.Add(log);
        }

        internal bool IsEmpty()
        {
            return __logs__.Count <= 0;
        }
    }

    [Serializable]
    internal class ReportLog : ISerializationCallbackReceiver
    {
        /// <summary>
        /// report id 数据编号
        /// </summary>
        [SerializeField]
        internal string rptid;

        /// <summary>
        /// 时间戳
        /// </summary>
        [SerializeField]
        internal string tm;

        /// <summary>
        /// user id 用户id
        /// </summary>
        [SerializeField]
        internal string uid;

        /// <summary>
        /// user name 用户名
        /// </summary>
        [SerializeField]
        internal string un;

        /// <summary>
        /// player name 玩家名
        /// </summary>
        [SerializeField]
        internal string pn;

        /// <summary>
        /// 上报数据内容
        /// </summary>
        [SerializeField]
        internal string hint;

        /// <summary>
        /// 所在库
        /// </summary>
        [SerializeField]
        internal string st;

        [NonSerialized]
        internal int reportId;

        internal ReportLog(string store, string hint, int reportId, ulong userId, string userName, string playerName)
        {
            st = store;
            this.hint = hint;

            this.reportId = reportId;
            rptid = reportId.ToString();
            uid = userId.ToString();
            un = userName;
            pn = playerName;
            tm = DateTime.UtcNow.ToString();
        }

        public void OnAfterDeserialize()
        {
            int.TryParse(rptid, out reportId);
        }

        public void OnBeforeSerialize()
        {
        }

        internal void SetBaseInfo()
        {
            
        }

    }

    [Serializable]
    public class ReportTags
    {
        //项目名
        [SerializeField]
        internal string pro;
        //渠道号
        [SerializeField]
        internal string chn;
        //设备码
        [SerializeField]
        internal string dvid;
        //平台
        [SerializeField]
        internal string plm;

        internal ReportTags()
        {
            dvid = SystemInfo.deviceUniqueIdentifier;
            plm = Application.platform.ToString();
        }
    }

    #endregion

    #region 存储数据结构

    internal class ReportLogWithTopicGroup
    {
        public List<ReportLogWithTopic> logs = new List<ReportLogWithTopic>();
        //文件路径
        [NonSerialized]
        public string path;

        //读取下标
        private int readIndex;

        internal int Size
        {
            get
            {
                return logs.Count;
            }
        }

        internal bool TryGetLog(out ReportLogWithTopic log)
        {
            if (readIndex < logs.Count)
            {
                log = logs[readIndex++];
                return true;
            }
            else
            {
                log = null;
                return false;
            }
        }

        internal void AddLog(ReportLogWithTopic log)
        {
            logs.Add(log);
        }

        internal bool IsEmpty()
        {
            return logs.Count <= 0;
        }
    }

    [Serializable]
    internal class ReportLogWithTopic
    {
        public string topic;
        public ReportLog log;

        internal int ReportId
        {
            get
            {
                if (null != log)
                {
                    return log.reportId;
                }

                return -1;
            }
        }

        internal string Store
        {
            get
            {
                if (null != log)
                {
                    return log.st;
                }

                return string.Empty;
            }
        }

        public ReportLogWithTopic(string topic, ReportLog log)
        {
            this.topic = topic;
            this.log = log;
        }

    }

    #endregion
}
