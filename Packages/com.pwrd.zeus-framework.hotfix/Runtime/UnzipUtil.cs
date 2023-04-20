/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using ICSharpCode.ZeusSharpZipLib.Zip;
using System.Diagnostics;
using System.Collections.Concurrent;

namespace Zeus.Framework.Hotfix
{
    public class UnzipUtil
    {
        [Flags]
        public enum UnzipError
        {
            Null = 0,
            Exception = 1 << 0,//未能处理的异常
            HardDiskFull = 1 << 1,//磁盘空间已满
            WrongZipFile = 1 << 2,//要解压的文件格式不是zip压缩包
            MissingZipFile = 1 << 3,//要解压的文件不存在
        }
        private string _zipPath;
        private string _outputPath;
        private string _tempOutputPath;
        private UnzipError _error;
        private Stopwatch _stopwatch;
        private long _targetSize = 0;
        private long _unzipSize = 0;
        private Action _callback;//结束回调
        private Func<ZipFile, ZipEntry, Exception, bool> _handleExtractException;//解压异常回调，如果返回false，则说明回调也未能处理异常情况，记录错误，终止解压
        private string _firstUnzipFileName;
        private Func<string, string> _modifyFileUnzipPath;

        ZipFile _zipFile;

        /// <summary>
        /// 输出目录
        /// </summary>
        public string OutputPath { get { return _outputPath; } }
        /// <summary>
        /// 临时输出目录：用于将文件先整体解压到此目录，完全解压完成之后再将文件move到目标输出目录,可以为空，则会将文件直接解压到目标输出目录
        /// </summary>
        public string TempOutputPath { get { return _tempOutputPath; } }
        /// <summary>
        /// 压缩包路径
        /// </summary>
        public string ZipPath { get { return _zipPath; } }
        /// <summary>
        /// 错误类型
        /// </summary>
        public UnzipError Error { get { return _error; } }
        public UnzipError TopPriorityError
        {
            get
            {
                int bit = 0;
                int error = (int)Error;
                if (error == 0)
                {
                    return UnzipError.Null;
                }
                while ((error /= 2) != 0)
                {
                    bit++;
                }
                return (UnzipError)(1 << bit);
            }
        }
        /// <summary>
        /// 未能处理的异常
        /// </summary>
        public Exception UnHandleException { get; private set; }
        /// <summary>
        /// 全部解压的尺寸
        /// </summary>
        public long TargetSize { get { return _targetSize; } }
        /// <summary>
        /// 当前解压的尺寸
        /// </summary>
        public long UnzipSize { get { return Interlocked.Read(ref _unzipSize); } }
        /// <summary>
        /// 耗时(毫秒)
        /// </summary>
        public long CostTime { get { return _stopwatch == null ? 0 : _stopwatch.ElapsedMilliseconds; } }
        //默认使用处理器核心数
        private int _threadLimit = UnityEngine.SystemInfo.processorCount;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="zipPath">压缩包路径</param>
        public UnzipUtil(string zipPath, int threadLimit = -1)
        {
            if (threadLimit > 0)
            {
                _threadLimit = threadLimit;
            }
            _stopwatch = new Stopwatch();


            _zipPath = zipPath;
            _error = UnzipError.Null;
            if (!File.Exists(_zipPath))
            {
                _error = _error | UnzipError.MissingZipFile;
            }
            else
            {
                try
                {
                    _targetSize = 0;
                    _zipFile = new ZipFile(_zipPath);
                    for (int i = 0; i < _zipFile.Count; i++)
                    {
                        ZipEntry entry = _zipFile.GetEntryWithoutClone(i);
                        if (entry.IsFile)
                        {
                            _targetSize += entry.Size;
                        }
                    }
                }
                catch (InvalidDataException e)
                {
                    _error = _error | UnzipError.WrongZipFile;
                    UnHandleException = e;
                }
                catch (Exception e)
                {
                    _error = _error | UnzipError.Exception;
                    UnHandleException = e;
                }
                if (_targetSize == 0)
                {
                    _error = _error | UnzipError.WrongZipFile;
                }
            }
        }

        /// <summary>
        /// 设置解压参数
        /// </summary>
        /// <param name="outputPath">输出目录</param>
        /// <param name="callback">结束回调</param>
        /// <param name="tempOutputPath">临时输出目录：用于将文件先整体解压到此目录，完全解压完成之后再将文件move到目标输出目录,可以为空，则会将文件直接解压到目标输出目录</param>
        /// <param name="handleExtractException">解压异常回调，如果返回false，则说明回调也未能处理异常情况，记录错误，终止解压</param>
        /// <param name="firstUnzipFileName">将某个文件放在第一个解压</param>
        /// <param name="modifyFileUnzipPath">用于单独修改某些文件的输出路径</param>
        public void SetUnzipParams(string outputPath, Action callback,
            string tempOutputPath = null,
            Func<ZipFile, ZipEntry, Exception, bool> handleExtractException = null,
            string firstUnzipFileName = null,
            Func<string, string> modifyFileUnzipPath = null)
        {
            _outputPath = outputPath;
            _callback = callback;
            _tempOutputPath = tempOutputPath;
            _handleExtractException = handleExtractException;
            _firstUnzipFileName = firstUnzipFileName;
            _modifyFileUnzipPath = modifyFileUnzipPath;

            if (!string.IsNullOrEmpty(_outputPath) && !Directory.Exists(_outputPath))
            {
                Directory.CreateDirectory(_outputPath);
            }
            if (!string.IsNullOrEmpty(_tempOutputPath) && !Directory.Exists(_tempOutputPath))
            {
                Directory.CreateDirectory(_tempOutputPath);
            }
        }

        public void Start()
        {
            if (_error != UnzipError.Null)
            {
                if (_callback != null)
                {
                    _callback();
                }
            }
            else
            {
                ThreadPool.QueueUserWorkItem(Unzip);
            }
        }

        ConcurrentQueue<KeyValuePair<string, ZipEntry>> queue;
        private void Unzip(object o)
        {
            _stopwatch.Reset();
            _stopwatch.Start();
            if (_zipFile != null)
            {
                using (_zipFile)
                {
                    ConcurrentQueue<KeyValuePair<string, ZipEntry>> tempQueue = new ConcurrentQueue<KeyValuePair<string, ZipEntry>>();
                    CancellationTokenSource cancellation = new CancellationTokenSource();
                    List<System.Threading.Tasks.Task> tasks = new List<System.Threading.Tasks.Task>();
                    ZipEntry firstUnzipEntry = null;
                    string firstUnzipEntryPath = null;
                    for (int i = 0; i < _zipFile.Count; i++)
                    {
                        ZipEntry entry = _zipFile.GetEntryWithoutClone(i);
                        string fullPath;
                        if (string.IsNullOrEmpty(_tempOutputPath))
                        {
                            fullPath = PathUtil.CombinePath(_outputPath, entry.Name);
                            if (_modifyFileUnzipPath != null)
                            {
                                fullPath = _modifyFileUnzipPath(fullPath);
                            }
                        }
                        else
                        {
                            fullPath = PathUtil.CombinePath(_tempOutputPath, entry.Name);
                        }
                        if (entry.IsFile)
                        {
                            string dir = Path.GetDirectoryName(fullPath);
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }
                            string fileName = Path.GetFileName(fullPath);
                            bool firstUnzip = false;
                            if (!string.IsNullOrEmpty(_firstUnzipFileName))
                            {
                                if (!string.IsNullOrEmpty(fileName) && _firstUnzipFileName.Equals(fileName))
                                {
                                    firstUnzip = true;
                                }
                            }
                            if (firstUnzip)
                            {
                                firstUnzipEntry = entry;
                                firstUnzipEntryPath = fullPath;
                            }
                            else
                            {
                                tempQueue.Enqueue(new KeyValuePair<string, ZipEntry>(fullPath, entry));
                            }
                        }
                        else
                        {
                            if (!Directory.Exists(fullPath))
                            {
                                Directory.CreateDirectory(fullPath);
                            }
                        }
                    }
                    if (firstUnzipEntry != null && !string.IsNullOrEmpty(firstUnzipEntryPath))
                    {
                        queue = new ConcurrentQueue<KeyValuePair<string, ZipEntry>>();
                        queue.Enqueue(new KeyValuePair<string, ZipEntry>(firstUnzipEntryPath, firstUnzipEntry));
                        tasks.Add(System.Threading.Tasks.Task.Factory.StartNew(Extract, new object[] { _zipFile, cancellation }, cancellation.Token));
                        System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
                        tasks.Clear();
                    }
                    if (!cancellation.IsCancellationRequested && tempQueue.Count > 0)
                    {
                        queue = tempQueue;
                        for (int i = 0; i < _threadLimit; i++)
                        {
                            tasks.Add(System.Threading.Tasks.Task.Factory.StartNew(Extract, new object[] { _zipFile, cancellation }, cancellation.Token));
                        }
                        System.Threading.Tasks.Task.WaitAll(tasks.ToArray());
                    }
                    if (!cancellation.IsCancellationRequested && !string.IsNullOrEmpty(_tempOutputPath))
                    {
                        if (firstUnzipEntry != null && !string.IsNullOrEmpty(firstUnzipEntryPath) && File.Exists(firstUnzipEntryPath))
                        {
                            string path = PathUtil.CombinePath(_outputPath, firstUnzipEntry.Name);
                            if (_modifyFileUnzipPath != null)
                            {
                                path = _modifyFileUnzipPath(path);
                            }
                            string dir = Path.GetDirectoryName(path);
                            if(!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }
                            else
                            {
                                if (File.Exists(path))
                                {
                                    File.Delete(path);
                                }
                            }
                            File.Move(firstUnzipEntryPath, path);
                        }
                        string[] files = Directory.GetFiles(_tempOutputPath, "*", SearchOption.AllDirectories);
                        _tempOutputPath = PathUtil.FormatPathSeparator(_tempOutputPath);
                        for (int i = 0; i < files.Length; i++)
                        {
                            files[i] = PathUtil.FormatPathSeparator(files[i]);
                            string targetPath = PathUtil.CombinePath(_outputPath, files[i].Substring(_tempOutputPath.Length + 1));
                            if (_modifyFileUnzipPath != null)
                            {
                                targetPath = _modifyFileUnzipPath(targetPath);
                            }
                            string dir = Path.GetDirectoryName(targetPath);
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }
                            else
                            {
                                if (File.Exists(targetPath))
                                {
                                    File.Delete(targetPath);
                                }
                            }
                            File.Move(files[i], targetPath);
                        }
                    }
                    cancellation.Dispose();
                }
                _zipFile = null;
            }
            _stopwatch.Stop();
            if (_callback != null)
            {
                _callback();
            }
        }

        private void Extract(object obj)
        {
            object[] list = (object[])obj;
            ZipFile zipfile = (ZipFile)list[0];
            CancellationTokenSource cancellation = (CancellationTokenSource)list[1];
            if (cancellation.IsCancellationRequested)
            {
                return;
            }
            byte[] buffer = new byte[2048];
            KeyValuePair<string, ZipEntry> pair;
            while (queue.TryDequeue(out pair))
            {
                string fullPath = pair.Key;
                string tempPath = fullPath + ".unzipTemp";

                try
                {
                    using (Stream readStream = zipfile.GetInputStream(pair.Value))
                    {
                        string dir = Path.GetDirectoryName(tempPath);
                        if (!Directory.Exists(dir))
                        {
                            try
                            {
                                Directory.CreateDirectory(dir);
                            }
                            catch (Exception ex)
                            {
                                if (!Directory.Exists(dir))
                                {
                                    throw ex;
                                }
                            }
                        }
                        else
                        {
                            if (File.Exists(tempPath))
                                File.Delete(tempPath);
                        }
                        using (Stream writeStream = File.Create(tempPath))
                        {
                            while (true)
                            {
                                int readCount = readStream.Read(buffer, 0, buffer.Length);
                                if (cancellation.IsCancellationRequested || readCount <= 0)
                                {
                                    break;
                                }
                                writeStream.Write(buffer, 0, readCount);
                            }
                            writeStream.Flush();
                        }
                    }
                    if (cancellation.IsCancellationRequested)
                    {
                        if (File.Exists(tempPath)) File.Delete(tempPath);
                        break;
                    }
                    else
                    {
                        if (File.Exists(fullPath)) File.Delete(fullPath);
                        File.Move(tempPath, fullPath);
                        Interlocked.Add(ref _unzipSize, pair.Value.Size);
                    }
                }
                catch (Exception e)
                {
                    if (TryHandleException(zipfile, pair.Value, e))
                    {
                        Interlocked.Add(ref _unzipSize, pair.Value.Size);
                    }
                    else
                    {
                        //未能处理异常，结束解压
                        _error = _error | UnzipError.Exception;
                        cancellation.Cancel();
                        UnityEngine.Debug.LogError("TryHandleException Fail.\r\n" + e.ToString());
                        if (File.Exists(tempPath)) File.Delete(tempPath);
                        break;
                    }
                }
            }
        }

        public static void StreamExtractToFile(ZipFile zipfile, ZipEntry entry, string path, byte[] buffer)
        {
            string temp = path + ".unzipTemp";
            using (var readStream = zipfile.GetInputStream(entry))
            {
                if (File.Exists(temp)) File.Delete(temp);
                using (var writeStream = File.Create(temp))
                {
                    while (true)
                    {
                        int readCount = readStream.Read(buffer, 0, buffer.Length);
                        if (readCount <= 0)
                        {
                            break;
                        }
                        writeStream.Write(buffer, 0, readCount);
                    }
                    writeStream.Flush();
                }
                if (File.Exists(path)) File.Delete(path);
                File.Move(temp, path);
            }
        }

        private bool TryHandleException(ZipFile zipfile, ZipEntry entry, Exception e)
        {
            if (e is IOException)
            {
                if (DiskUtils.IsHardDiskFull(e as IOException))
                {
                    _error = _error | UnzipError.HardDiskFull;
                    UnHandleException = e;
                    //存储空间不足，结束解压
                    return false;
                }
            }

            bool result = false;

            if (_handleExtractException != null)
            {
                result = _handleExtractException(zipfile, entry, e);
            }
            if (!result)
            {
                _error = _error | UnzipError.Exception;
                UnHandleException = e;
            }
            return result;
        }
    }
}

