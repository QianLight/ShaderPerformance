/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Text;

namespace Zeus.Core.FileSystem
{
    public class Inner2OuterCopier
    {
        [System.Serializable]
        public class Recorder
        {
            public List<string> list = new List<string>();
            public Recorder() { }
            public Recorder(List<string> list)
            {
                this.list = list;
            }
        }
        /// <summary>
        /// 需要复制到包外的文件/文件夹(需要相对于StreamingAssets文件夹的相对路径)
        /// </summary>
        private List<string> dirList = new List<string>()
        {
            "Cri",
        };

        public string SaveFilePath
        {
            get
            {
                return VFileSystem.GetBuildinSettingPath("Inner2OuterCopyRecord.json");
            }
        }

        private Action _callBack;

        public Inner2OuterCopier(Action action)
        {
            _callBack = action;
        }

        #region 生成列表记录文件
        public void GenRecordFile()
        {
            SaveList(GenCopyList());
        }

        private List<string> GenCopyList()
        {
            List<string> list = new List<string>();
            for (int i = 0; i < dirList.Count; i++)
            {
                string path = Zeus.Framework.PathUtil.CombinePath(Application.streamingAssetsPath, dirList[i]);
                if (File.Exists(path))
                {
                    list.Add(dirList[i].Replace("\\", "/"));
                }
                else
                {
                    if (Directory.Exists(path))
                    {
                        string[] array = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                        for (int j = 0; j < array.Length; j++)
                        {
                            if (array[j].EndsWith(".meta"))
                            {
                                continue;
                            }
                            list.Add(array[j].Substring(Application.streamingAssetsPath.Length + 1).Replace("\\", "/"));
                        }
                    }
                }
            }
            return list;
        }

        private void SaveList(List<string> list)
        {
            Recorder config = new Recorder(list);
            string content = JsonUtility.ToJson(config);
            FileUtil.CreateFileWithText(InnerPackage.GetFullPath(SaveFilePath), content);
            //UnityEditor.AssetDatabase.Refresh();
        }
        #endregion

        #region 执行复制
        public void Execute()
        {
            System.Threading.ThreadPool.QueueUserWorkItem(StartThread);
        }

        private void StartThread(object obj)
        {
            List<string> list = LoadList();
            byte[] buffer = new byte[128 * ZeusConstant.KB];
            for (int i = 0; i < list.Count; i++)
            {
                if (File.Exists(OuterPackage.GetRealPath(list[i])))
                {
                    continue;
                }
                OuterPackage.CopyFromInternal(list[i], buffer);
            }
            if (_callBack != null)
            {
                _callBack();
            }
        }

        private List<string> LoadList()
        {
            Recorder config;
            if (InnerPackage.ExistsFile(SaveFilePath))
            {
                using (var stream = InnerPackage.OpenReadStream(SaveFilePath))
                {
                    var streamReader = new StreamReader(stream, Encoding.UTF8);
                    string content = streamReader.ReadToEnd();
                    config = JsonUtility.FromJson<Recorder>(content);
                }
            }
            else
            {
                config = new Recorder();
            }
            return config.list;
        }
        #endregion
    }
}