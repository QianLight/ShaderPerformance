/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/

using System.IO;
using System.Text;
using UnityEngine;

namespace Zeus.Core.ReportSystem
{
    class ReportMarkCenter
    {
        private string _reportedIdPath;
        private int _reportedId = -1;

        private string _growIdPath;
        private volatile int _growId = -1;

        internal ReportMarkCenter(string reportRoot)
        {
            _reportedIdPath = Path.Combine(reportRoot, ReportConsts.REPORTEDID_FILE_NAME);
            _growIdPath = Path.Combine(reportRoot, ReportConsts.GROWID_FILE_NAME);
        }

        internal int ReadReportedId()
        {
            if (_reportedId == -1)
                _reportedId = ReadId(_reportedIdPath);

            return _reportedId;
        }

        internal int ReadGrowId()
        {
            if (_growId == -1)
                _growId = ReadId(_growIdPath);

            return _growId;
        }

        internal void WriteReportedId(int id)
        {
            _reportedId = id;
            WriteId(_reportedIdPath, id);
        }

        internal void WriteGrowId(int id)
        {
            if (id <= _growId)
            {
                Debug.LogError("新传输的值:" + id + "比磁盘存储的值:" + _growId + "小");
            }

            _growId = id;
            WriteId(_growIdPath, id);
        }

        private int ReadId(string path)
        {
            if (!File.Exists(path))
            {
                return 0;
            }

            string reportStr = File.ReadAllText(path);

            int reportId;
            if (!int.TryParse(reportStr, out reportId))
            {
                reportId = Repair(path);
            }

            return reportId;
        }

        private void WriteId(string path, int reportId)
        {
            File.WriteAllText(path, reportId.ToString(), Encoding.UTF8);
        }

        //如果读取失败，修复文件
        private int Repair(string path)
        {
            if (path.Contains(ReportConsts.REPORTEDID_FILE_NAME))
            {
                _reportedId = 0;
                WriteId(path, _reportedId);
                return _reportedId;
            }
            else
            {
                _growId = ReadReportedId();
                WriteId(path, _growId);
                return _growId;
            }
        }
    }

}
