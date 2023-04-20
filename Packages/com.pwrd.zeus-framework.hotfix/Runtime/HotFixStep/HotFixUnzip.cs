/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Zeus.Framework.Hotfix
{
    public class HotFixUnzip : HotFixUnzipBase
    {
        public HotFixUnzip(HotfixService executer) : base(executer, HotfixStep.Unzip)
        { }

        protected override ConcurrentQueue<Task> GenerateQueue(out long targetSize)
        {
            targetSize = 0;
            if (null != _hotFixExecuter.ResVersion && _hotFixExecuter.ReachOpenTimeHFPatchDataList != null && _hotFixExecuter.ReachOpenTimeHFPatchDataList.Count > 0)
            {
                ConcurrentQueue<Task> tempQueue = new ConcurrentQueue<Task>();
                List<HFPatchConfigData> removeList = new List<HFPatchConfigData>();
                for (int i = 0; i < _hotFixExecuter.ReachOpenTimeHFPatchDataList.Count; i++)
                {
                    HFPatchConfigData patchData = _hotFixExecuter.ReachOpenTimeHFPatchDataList[i];

                    string zipFile = patchData.GetPatchSavePath();
                    if (File.Exists(zipFile))
                    {
                        UnzipUtil unzip = new UnzipUtil(zipFile);
                        targetSize += unzip.TargetSize;
                        _hotFixExecuter.ResVersion.RecordUnfinishedUnzipTask(patchData.TargetVersion, zipFile, patchData.GetPatchUnzipPath());
                        tempQueue.Enqueue(new Task(patchData.TargetVersion, zipFile, patchData.GetPatchUnzipPath(), unzip));
                    }
                    else
                    {
                        removeList.Add(patchData);
                    }
                }
                if (removeList.Count > 0)
                {
                    for (int i = 0; i < removeList.Count; i++)
                    {
                        _hotFixExecuter.ReachOpenTimeHFPatchDataList.Remove(removeList[i]);
                    }
                }
                return tempQueue;
            }
            return null;
        }

        protected override void OnStartUnzip(Task unzipTask)
        {
            _hotFixExecuter.InvokeOnUnzipStart(unzipTask.ResVersion);
        }

        protected override string GetErrorTag()
        {
            return "HotfixUnzip";
        }

        protected override void OnUnzipFinish(Task unzipTask)
        {
            if (unzipTask.Unzip.TopPriorityError == UnzipUtil.UnzipError.Null)
            {
                _hotFixExecuter.InvokeOnUnzipSuc(unzipTask.ResVersion, (int)unzipTask.Unzip.CostTime);
            }
            else
            {
                _hotFixExecuter.InvokeOnUnzipFail(unzipTask.ResVersion,
                    Path.GetFileName(unzipTask.PatchPath), unzipTask.Unzip.TopPriorityError.ToString(), (int)unzipTask.Unzip.CostTime);
            }
        }
    }
}