/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using Common.Tool;
using UnityEngine;

namespace Zeus.Framework
{
    public class FileReadRequest
    {
        public enum FileType
        {
            String,
            Byte,
        }
        public FileReadRequest()
        {

        }
        public FileReadRequest(string path, FileType type, object param = null)
        {
            this.m_strPath = path;
            this.m_ReadType = type;
            this.m_Param = param;
        }

        public string m_strPath;
        public FileType m_ReadType;
        public object m_Param;
    }

    public class FileReadResult
    {
        public FileReadRequest m_Request;
        public string m_strContent;
        public byte[] m_byteContent;
        public Exception m_Error;

        public bool WithError()
        {
            return m_Error == null;
        }
        public string GetStringContent()
        {
            return m_strContent;
        }
        public byte[] GetByteContent()
        {
            return m_byteContent;
        }
    }

    public class FileReaderTaskManager
    {
        public static void BeginLoadFile(FileReadRequest request, Action<FileReadResult> callback)
        {
            BeginLoadFile(new List<FileReadRequest>() { request }, (res) =>
            {
                callback(res[0]);
            });
        }
        public static void BeginLoadFile(List<FileReadRequest> request, Action<List<FileReadResult>> callback)
        {
            FileReaderAsyncTask elem = new FileReaderAsyncTask(request, callback);
            AsyncManager.Instance.ExecuteAsyncTask(elem);
        }
    }

    internal class FileReaderAsyncTask : IAsyncTask
    {
        private readonly List<FileReadRequest> _requestList;
        private readonly Action<List<FileReadResult>> _callback;
        private readonly List<FileReadResult> _resultList;

        public FileReaderAsyncTask(List<FileReadRequest> request, Action<List<FileReadResult>> callback)
        {
            if (null == callback)
            {
                Debug.LogError("callback can't be null when read file");
                return;
            }
            if (request == null || request.Count == 0)
            {
                Debug.LogError("request can't be null & empty when read file");
                return;
            }
            _requestList = request;
            _callback = callback;

            _resultList = new List<FileReadResult>(_requestList.Count);
            for (var requestIndex = 0; requestIndex < _requestList.Count; ++requestIndex)
            {
                FileReadResult elem = new FileReadResult();
                elem.m_Request = _requestList[requestIndex];
                _resultList.Add(elem);
            }
        }
        public AsyncState BeforeAsyncTask()
        {
            return AsyncState.Doing;
        }
        public AsyncState DoAsyncTask()
        {
            for (var requestIndex = 0; requestIndex < _requestList.Count; ++requestIndex)
            {
                var request = _requestList[requestIndex];
                var result = _resultList[requestIndex];
                result.m_Error = null;

                try
                {
                    if (request.m_ReadType == FileReadRequest.FileType.Byte)
                    {
                        result.m_byteContent = Zeus.Core.FileUtil.LoadFileBytes(request.m_strPath);
                    }
                    if (request.m_ReadType == FileReadRequest.FileType.String)
                    {
                        result.m_strContent = Zeus.Core.FileUtil.LoadFileText(request.m_strPath);
                    }
                }
                catch (Exception e)
                {
                    result.m_Error = e;
                    Debug.LogError("error on load file " + request.m_strPath);
                    Debug.LogException(e);
                }
            }
            return AsyncState.After;
        }
        public AsyncState AfterAsyncTask()
        {
            _callback(_resultList);
            return AsyncState.Done;
        }

    }
}