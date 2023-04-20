/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.IO;
using Zeus.Core;
using System.Collections;
using Zeus.Core.FileSystem;
using System.Collections.Concurrent;
using ICSharpCode.ZeusSharpZipLib.Zip;

namespace Zeus.Framework.Hotfix
{
    public class HotFixUnzipBase : BaseHotFixStep
    {
        protected class Task
        {
            public string ResVersion;
            /// <summary>
            /// 绝对路径
            /// </summary>
            public string PatchPath;
            /// <summary>
            /// 绝对路径
            /// </summary>
            public string UnzipPath;

            public UnzipUtil Unzip;
            public Task(string ResVersion, string PatchPath, string UnzipPath, UnzipUtil unzip)
            {
                this.ResVersion = ResVersion;
                this.PatchPath = PatchPath;
                this.UnzipPath = UnzipPath;
                Unzip = unzip;
            }
        }


        private long _unzipSize;
        private long _targetSize;
        private Task curUnzipTask;
        private ConcurrentQueue<Task> queue;
        private string TEMP_UNIZP_PATH;
        public HotFixUnzipBase(HotfixService executer, HotfixStep step) : base(executer, step, 1)
        {
            TEMP_UNIZP_PATH = OuterPackage.GetRealPath(HotfixService.HotFixTempUnzipFolder);
            if (Directory.Exists(TEMP_UNIZP_PATH))
            {
                Directory.Delete(TEMP_UNIZP_PATH, true);
            }
        }

        public override void Run()
        {
            OnProcess(0, 1);
            _unzipSize = 0;
            _targetSize = 0;
            curUnzipTask = null;

            queue = GenerateQueue(out _targetSize);
            if (queue != null && queue.Count > 0)
            {
                StartUnzip();
            }
            else
            {
                OnProcess(1, 1);
                NextStep();
            }
        }

        protected virtual ConcurrentQueue<Task> GenerateQueue(out long targetSize)
        {
            targetSize = 0;
            return null;
        }

        private void StartUnzip()
        {
            if (queue.TryDequeue(out curUnzipTask))
            {
                OnStartUnzip(curUnzipTask);
                //如果空间足够，将文件先解压到临时文件夹再统一Move到目标位置
                string tempUnzipPath = DiskUtils.CheckAvailableSpaceBytes() > curUnzipTask.Unzip.TargetSize ? TEMP_UNIZP_PATH : null;
                curUnzipTask.Unzip.SetUnzipParams(curUnzipTask.UnzipPath, AddMainThreadMethod, tempUnzipPath,
                    TryHandleException, RedundantFileCheckSumInfo.RedundantFileCheckSumInfoFBName, ModifyFileUnzipPath);
                curUnzipTask.Unzip.Start();
            }
            else
            {
                NextStep();
            }
        }

        protected virtual void OnStartUnzip(Task unzipTask) { }

        private string ModifyFileUnzipPath(string path)
        {
            if (path.Contains(RedundantFileCheckSumInfo.RedundantFileCheckSumInfoFBName))
            {
                return RedundantFileCheckSumInfo.TempRedundantFileCheckSumInfoFB_OuterPackageFullPath;
            }
            return path;
        }

        private void AddMainThreadMethod()
        {
            try
            {
                if (File.Exists(RedundantFileCheckSumInfo.TempRedundantFileCheckSumInfoFB_OuterPackageFullPath))
                {
                    string dir = Path.GetDirectoryName(RedundantFileCheckSumInfo.RedundantFileCheckSumInfoFB_OuterPackageFullPath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }
                    else
                    {
                        if (File.Exists(RedundantFileCheckSumInfo.RedundantFileCheckSumInfoFB_OuterPackageFullPath))
                        {
                            File.Delete(RedundantFileCheckSumInfo.RedundantFileCheckSumInfoFB_OuterPackageFullPath);
                        }
                    }
                    File.Move(RedundantFileCheckSumInfo.TempRedundantFileCheckSumInfoFB_OuterPackageFullPath, RedundantFileCheckSumInfo.RedundantFileCheckSumInfoFB_OuterPackageFullPath);
                }
            }
            catch (Exception ex)
            {
                HotfixLogger.LogError(ex.ToString());
            }
            ZeusCore.Instance.AddMainThreadTask(OnUnzipComplete);
        }

        private void OnUnzipComplete()
        {
            OnUnzipFinish(curUnzipTask);
            UnzipUtil.UnzipError error = curUnzipTask.Unzip.TopPriorityError;
            if (error == UnzipUtil.UnzipError.Null)
            {
                if(null == _hotFixExecuter.ResVersion)
                {
                    OnError(HotfixError.NoResVersionData, "no ResVersionData found, and create failed");
                    return;
                }
                //清理记录的解压任务
                _hotFixExecuter.ResVersion.FinishHotfix(curUnzipTask.PatchPath);

                //删除patch文件
                string directoryPath = Path.GetDirectoryName(curUnzipTask.PatchPath);
                if (Directory.Exists(directoryPath))
                {
                    Directory.Delete(directoryPath, true);
                }

                _unzipSize += curUnzipTask.Unzip.TargetSize;
                curUnzipTask = null;
                if (queue.Count > 0)
                {
                    StartUnzip();
                }
                else
                {
                    OnProcess(_targetSize, _targetSize);
                    NextStep();
                }
            }
            else
            {
                HotfixError hfError = HotfixError.UnzipError;
                switch (error)
                {
                    case UnzipUtil.UnzipError.Exception:
                        break;
                    case UnzipUtil.UnzipError.HardDiskFull:
                        hfError = HotfixError.HardDiskFull;
                        break;
                    case UnzipUtil.UnzipError.WrongZipFile:
                        break;
                    case UnzipUtil.UnzipError.MissingZipFile:
                        break;
                }
                if (hfError == HotfixError.HardDiskFull)
                {
                    ZeusCore.Instance.StartCoroutine(OnHardiskFull());
                }
                else
                {
                    if (string.IsNullOrEmpty(curUnzipTask.Unzip.TempOutputPath) &&
                        Directory.Exists(curUnzipTask.Unzip.TempOutputPath))
                    {
                        Directory.Delete(curUnzipTask.Unzip.TempOutputPath, true);
                    }
                    OnError(hfError,
                        $"[HotfixService] [" + GetErrorTag() + "] TopPriorityError:{error.ToString()}, " +
                        $"Error:{curUnzipTask.Unzip.Error.ToString()}, " +
                        $"UnHandleException：{(curUnzipTask.Unzip.UnHandleException == null ? "null" : curUnzipTask.Unzip.UnHandleException.ToString())}");
                }
                curUnzipTask = null;
            }
        }

        protected virtual string GetErrorTag()
        {
            return string.Empty;
        }

        protected virtual void OnUnzipFinish(Task unzipTask) { }

        private IEnumerator OnHardiskFull()
        {
            WaitForTrigger trigger = new WaitForTrigger();
            long needHardiskSpace = _targetSize - _unzipSize - curUnzipTask.Unzip.UnzipSize;
            OnConfirm(HotfixTips.HardDiskFull, trigger, needHardiskSpace);
            yield return trigger;
            while (needHardiskSpace >= DiskUtils.CheckAvailableSpaceBytes())
            {
                trigger = new WaitForTrigger();
                OnConfirm(HotfixTips.HardDiskFull, trigger, needHardiskSpace);
                yield return trigger;
            }
            if (string.IsNullOrEmpty(curUnzipTask.Unzip.TempOutputPath) &&
                Directory.Exists(curUnzipTask.Unzip.TempOutputPath))
            {
                Directory.Delete(curUnzipTask.Unzip.TempOutputPath, true);
            }
            Run();
        }

        private bool TryHandleException(ZipFile zipFile, ZipEntry entry, Exception e)
        {
            byte[] buffer = new byte[1024];
            bool result = false;
            if (e is IOException)
            {
                //先做个缓存，下次登录替换目标文件
                try
                {
                    result = true;
                    string fullPath = curUnzipTask.Unzip.OutputPath + "/" + entry.Name;
                    string failPath = fullPath + DateTime.Now.Ticks;
                    VFileSystem.MoveFileOnNextLanch(failPath, fullPath);
                    UnzipUtil.StreamExtractToFile(zipFile, entry, failPath, buffer);
                }
                catch (Exception ex)
                {
                    HotfixLogger.LogError("[HotfixService][" + GetErrorTag() + "]", "Can not unzip file,ori exception:" + e.ToString() + ",sub exception:" + ex.ToString());
                    result = false;
                }
            }
            return result;
        }

        public override void Update()
        {
            if (curUnzipTask != null)
            {
                OnProcess(_unzipSize + curUnzipTask.Unzip.UnzipSize, _targetSize);
            }
        }
    }
}