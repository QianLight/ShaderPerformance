/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/

namespace Zeus.Core.ReportSystem
{
    public static class ReportConsts
    {
        public static class Store
        {
            public const string HOTFIX = "hotfix";
        }

        public static class Topic
        {
            public const string DEFAULT = "default";
        }

        internal const string REPORT_FILE_NAME = "zeus_report_{0}_{1}{2}.json";

        internal const string REPORT_FILE_TIME_PATTERN = "yyyyMMddHHmmss";

        internal const string REPORT_ENCRYPT_MARK = "_ECTA";

        internal const string REPORT_ROOT_RELATIVE_PATH = "zeus_report/";

        internal const string REPORTEDID_FILE_NAME = "zeus_rptid_reported.txt";

        internal const string GROWID_FILE_NAME = "zeus_rptid_grow.txt";

        internal const string POWER_1 = @"PnKdE2tGCA";

        internal const string POWER_2 = @"9oJ+S";

        internal const string POWER_3 = @"ESKRaBVa>Z{z1";

        internal const string POWER_4 = @"d1t8";

        internal const string OS_1 = @"KD9Wk";

        internal const string OS_2 = @"U7L5x8";

        internal const string OS_3 = @"avPbO";

        #region Http

        internal const int REPORT_TIMEOUT = 5000;

        internal const string REPORT_METHOD = "POST";
        //project name:{0} endpoint:{1} store name:{2}
        internal const string REPORT_URL = "http://{0}.{1}/logstores/{2}/track";

        internal const string HEADER_BODYRAWSIZE = "x-log-bodyrawsize";

        internal const string HEADER_APIVERSION = "x-log-apiversion";

        internal const string HEADER_APIVERSION_VALUE = "0.6.0";

        #endregion

        //失败重发的时间间隔
        internal const int RESEND_INTERVAL = 30;
        //单个数据文件的最大写入数
        internal const int FILE_MAX_SIZE = 5;
        //检查冗余文件时间间隔
        internal const double CHECK_REDUNDANT_FILE_INTERVAL = 2d;
        //磁盘中最大数据文件数量
        internal const int FILE_COUNT_LIMIT = 30;
    }
}
