/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zeus.Core;
using Zeus.Framework;
using System.IO;

namespace Zeus.Core.FileSystem
{
    public sealed class FileSystemLauncher : ILauncher
    {
        private volatile bool _IsDone;
        private volatile Exception _Exception;

        public ZeusLaunchErrorFlag Execute()
        {
            ZeusLaunchErrorFlag flag = ZeusLaunchErrorFlag.None;
            InnerPackage.Init();
            OuterPackage.Init();

            VFileSystem.MoveFile();

            flag = flag | OuterPackage.ProcessOutPackageContent();

            ZeusCore.Instance.StartCoroutine(OuterPackage.CopyFileInner2Outer());

#if ZEUS_FIRSTCOPY
            ZeusCore.Instance.RegisterUpdate(_Update);
            ThreadPool.QueueUserWorkItem(_CopyThread, null);
#endif
            return flag;
        }

        private void _CopyThread(object state)
        {
            try
            {
                OuterPackage.Clear();

                List<string> otherFiles = OtherFileList.Deserialize().m_ListFileName;
                byte[] buffer = new byte[ZeusConstant.MB];

                int totalCount = otherFiles.Count;
                for (int i = 0; i < totalCount; ++i)
                {
                    var fileName = otherFiles[i];
                    VFileSystem.CreateParentDirectory(fileName);
                    OuterPackage.CopyFromInternal(fileName);
                }

                _IsDone = true;
            }
            catch (Exception e)
            {
                _Exception = e;
            }
        }

        private void _Update()
        {
            if (_Exception != null)
            {
                Exception e = _Exception;
                _Exception = null;
                ZeusCore.Instance.UnRegisterUpdate(_Update);
                UnityEngine.Debug.LogException(e);
            }

            if (_IsDone)
            {
                ZeusCore.Instance.UnRegisterUpdate(_Update);
            }
        }
    }
}
