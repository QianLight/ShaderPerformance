/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.IO;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Zeus.Framework.Hotfix
{
    public class HotFixCheckAndFinishLastHotfix : HotFixUnzipBase
    {
        public HotFixCheckAndFinishLastHotfix(HotfixService executer) : base(executer, HotfixStep.CheckAndFinishLastHotfix)
        { }


        protected override ConcurrentQueue<Task> GenerateQueue(out long targetSize)
        {
            targetSize = 0;
            if (null != _hotFixExecuter.ResVersion && _hotFixExecuter.ResVersion.UnfinishedUnzipTasklist != null && _hotFixExecuter.ResVersion.UnfinishedUnzipTasklist.Count > 0)
            {
                ConcurrentQueue<Task> tempQueue = new ConcurrentQueue<Task>();
                List<ResVersionData.UnzipTask> removeList = new List<ResVersionData.UnzipTask>();
                for (int i = 0; i < _hotFixExecuter.ResVersion.UnfinishedUnzipTasklist.Count; i++)
                {
                    ResVersionData.UnzipTask task = _hotFixExecuter.ResVersion.UnfinishedUnzipTasklist[i];

                    if (File.Exists(task.PatchPath))
                    {
                        UnzipUtil unzip = new UnzipUtil(task.PatchPath);
                        targetSize += unzip.TargetSize;
                        tempQueue.Enqueue(new Task(task.ResVersion, task.PatchPath, task.UnzipPath, unzip));
                    }
                    else
                    {
                        removeList.Add(task);
                    }
                }
                if (removeList.Count > 0)
                {
                    for (int i = 0; i < removeList.Count; i++)
                    {
                        _hotFixExecuter.ResVersion.UnfinishedUnzipTasklist.Remove(removeList[i]);
                    }
                    _hotFixExecuter.ResVersion.Save();
                }
                return tempQueue;
            }
            return null;
        }

        protected override void OnStartUnzip(Task unzipTask)
        {
            _hotFixExecuter.InvokeOnCheckAndFinishLastHotfixStart(unzipTask.ResVersion);
        }

        protected override string GetErrorTag()
        {
            return "HotFixCheckAndFinishLastHotfix";
        }

        protected override void OnUnzipFinish(Task unzipTask)
        {
            if (unzipTask.Unzip.TopPriorityError == UnzipUtil.UnzipError.Null)
            {
                _hotFixExecuter.InvokeOnCheckAndFinishLastHotfixSuc(unzipTask.ResVersion, (int)unzipTask.Unzip.CostTime);
            }
            else
            {
                _hotFixExecuter.InvokeOnCheckAndFinishLastHotfixFail(unzipTask.ResVersion, unzipTask.Unzip.TopPriorityError.ToString(), (int)unzipTask.Unzip.CostTime);
            }
        }
    }
}