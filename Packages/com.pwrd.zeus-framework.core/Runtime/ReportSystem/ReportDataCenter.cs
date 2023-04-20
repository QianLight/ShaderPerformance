/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Zeus.Core.ReportSystem
{
    public class ReportDataCenter
    {
        private string _reportRoot;
        private ConcurrentQueue<string> _files;

        private bool _encryptData;
        private string _power;
        private string _os;

        //写
        private volatile ReportLogWithTopicGroup _writeGroup;
        private int _writeIndex = 0;

        //读
        private ReportLogWithTopicGroup _readGroup;
        private ReportData _reportData = new ReportData();

        //删除
        private ConcurrentQueue<string> _deleteFiles = new ConcurrentQueue<string>();
        private int _deleteId = -1;
        private DateTime _startTime;

        internal ReportDataCenter(string reportRoot, bool encryptData)
        {
            _startTime = DateTime.UtcNow;

            _encryptData = encryptData;
            _power = ReportConsts.POWER_1 + ReportConsts.POWER_2 + ReportConsts.POWER_3 + ReportConsts.POWER_4;
            _os = ReportConsts.OS_1 + ReportConsts.OS_2 + ReportConsts.OS_3;

            _reportRoot = reportRoot;

            InitFiles();
            DeleteRedundantFiles();
        }

        internal void Release()
        {
            string temp;
            while (!_files.IsEmpty)
            {
                _files.TryDequeue(out temp);
            }
            while (!_deleteFiles.IsEmpty)
            {
                _deleteFiles.TryDequeue(out temp);
            }
        }

        internal void SetProject(string project)
        {
            _reportData.SetProject(project);
        }

        internal void SetChannel(string channel)
        {
            _reportData.SetChannel(channel);
        }

        private void InitFiles()
        {
            var filesFromDisk = Directory.GetFiles(_reportRoot, "zeus_report*");
            _files = new ConcurrentQueue<string>(filesFromDisk.OrderBy(file => new FileInfo(file).CreationTime));
        }

        #region 写

        internal void WriteToDisk(ConcurrentQueue<ReportLogWithTopic> queue, ref int lastId)
        {
            if (null == _writeGroup)
            {
                _writeGroup = CreateNewGroup();
            }

            while (true)
            {
                ReportLogWithTopic log;
                if (!queue.TryDequeue(out log))
                {
                    break;
                }

                if (_writeGroup.Size >= ReportConsts.FILE_MAX_SIZE)
                {
                    Save(_writeGroup);
                    _writeGroup = CreateNewGroup();
                }

                _writeGroup.AddLog(log);
                lastId = log.ReportId;
            }

            if (!_writeGroup.IsEmpty())
            {
                Save(_writeGroup);
            }
        }

        private ReportLogWithTopicGroup CreateNewGroup()
        {
            var group = new ReportLogWithTopicGroup();
            group.path = Path.Combine(_reportRoot, string.Format(
                ReportConsts.REPORT_FILE_NAME,
                DateTime.UtcNow.ToString(ReportConsts.REPORT_FILE_TIME_PATTERN),
                ++_writeIndex,
                _encryptData ? ReportConsts.REPORT_ENCRYPT_MARK : string.Empty));
            _files.Enqueue(group.path);

            return group;
        }

        private void Save(ReportLogWithTopicGroup group)
        {
            try
            {
                var json = JsonUtility.ToJson(group);
                if (_encryptData)
                {
                    json = EncryptUtil.AesEncrypt(json, _power, _os);
                }
                File.WriteAllText(_writeGroup.path, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.LogErrorFormat("[ReportDataCenter][Save] Exception:{0}\nFile path:{1}", ex, _writeGroup.path);
            }
        }

        #endregion

        #region 读

        internal ReportData GetData(int uploadCount, int curReportedId)
        {
            CheckFileCountLimit();

            if (!TryLoadReadGroup())
            {
                return null;
            }

            _reportData.Reset();
            while (uploadCount > 0)
            {
				ReportLogWithTopic tempLog;
                if (!_readGroup.TryGetLog(out tempLog))
                {
                    if (_readGroup != _writeGroup)
                    {
                        RemoveCurFile();
                        _readGroup = GetNextGroup();
                        if (null == _readGroup)
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (null == tempLog)
                {
                    break;
                }

                //过滤已发送过的log
                if (tempLog.ReportId <= curReportedId)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(_reportData._store) && string.IsNullOrEmpty(_reportData.__topic__))
                {
                    _reportData._store = tempLog.Store;
                    _reportData.__topic__ = tempLog.topic;
                }
                else
                {
                    //不同Topic或Store无法合批发送
                    if (tempLog.Store != _reportData._store || tempLog.topic != _reportData.__topic__)
                    {
                        break;
                    }
                }

                _reportData.AddLog(tempLog.log);

                uploadCount--;
            }

            return _reportData;
        }

        internal ReportData GetResendData()
        {
            return _reportData;
        }

        private bool TryLoadReadGroup()
        {
            if (null == _readGroup)
            {
                _readGroup = GetNextGroup();
            }

            return null != _readGroup;
        }

        private void RemoveCurFile()
        {
            string file;
            if (_files.TryDequeue(out file))
            {
                AddToDeleteFile(file);
            }
        }

        private ReportLogWithTopicGroup GetNextGroup()
        {
			string readFile;
            _files.TryPeek(out readFile);

            if (null == readFile)
            {
                return null;
            }

            if (null != _writeGroup && readFile == _writeGroup.path)
            {
                return _writeGroup;
            }

            ReportLogWithTopicGroup result = null;
            try
            {
                string content = File.ReadAllText(readFile, Encoding.UTF8);
                if (string.IsNullOrEmpty(content))
                {
                    return null;
                }

                if (readFile.Contains(ReportConsts.REPORT_ENCRYPT_MARK))
                {
                    content = EncryptUtil.AesDecrypt(content, _power, _os);
                }
                
                result = JsonUtility.FromJson<ReportLogWithTopicGroup>(content);
            }
            catch (Exception ex)
            {
                if (_files.TryDequeue(out readFile))
                {
                    File.Delete(readFile);
                }
                Debug.LogErrorFormat("[ReportDataCenter][GetNextGroup] Exception:{0}\n delete file: {1}", ex, readFile);
            }

            return result;
        }

        #endregion

        #region 删

        internal void SetDeleteId(int id)
        {
            _deleteId = id;
        }

        internal void HandleDeleteQueue(int reportedId)
        {
            if (reportedId < _deleteId)
            {
                return;
            }

            string deleteFile;
            while (_deleteFiles.TryDequeue(out deleteFile))
            {
                File.Delete(deleteFile);
            }
        }

        private void AddToDeleteFile(string file)
        { 
            _deleteFiles.Enqueue(file);
        }

        private void CheckFileCountLimit()
        {
            if (DateTime.UtcNow > _startTime.AddHours(ReportConsts.CHECK_REDUNDANT_FILE_INTERVAL))
            {
                _startTime = DateTime.UtcNow;
                DeleteRedundantFiles();
            }
        }

        //控制本地数据的大小,只留最新的几个文件
        private void DeleteRedundantFiles()
        {
            int deleteCount = _files.Count - ReportConsts.FILE_COUNT_LIMIT;

            string path;
            for (int i = 0; i < deleteCount; i++)
            {
                if (_files.TryDequeue(out path) && File.Exists(path))
                {
                    File.Delete(path);
                }
            }

            _readGroup = null;
        }

        #endregion
    }

}
