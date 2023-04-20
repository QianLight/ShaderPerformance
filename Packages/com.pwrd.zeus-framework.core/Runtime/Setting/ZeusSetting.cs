/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using System;
using System.Text;
using Zeus.Core.FileSystem;
using System.IO;


namespace Zeus.Core
{
    [Serializable]
	public class ZeusSetting
	{
        private static ZeusSetting m_vFileSettingInstance;
        private const string filePath = @"ZeusSetting/EditorSetting/ZeusSetting.json";

        public List<string> includeBuildSourcePaths;

        public List<string> includeBuildTargetPaths;

        public List<string> excludeBuildSourcePaths;

        public string FilePath 
        {
            get 
            {
                return filePath;
            }
        }

        public static ZeusSetting GetInstance()
        {
            if (m_vFileSettingInstance == null)
            {
                if (VFileSystem.Exists(filePath))
                {
                    string jsonData = VFileSystem.ReadAllText(filePath, Encoding.UTF8);
                    m_vFileSettingInstance = UnityEngine.JsonUtility.FromJson<ZeusSetting>(jsonData);
                }
                else
                {
                    m_vFileSettingInstance = new ZeusSetting();
                    VFileSystem.WriteAllText(filePath, UnityEngine.JsonUtility.ToJson(m_vFileSettingInstance, true));
                }
            }
            return m_vFileSettingInstance;
        }

        private ZeusSetting()
        {
            includeBuildSourcePaths = new List<string>();
            excludeBuildSourcePaths = new List<string>();
            includeBuildTargetPaths = new List<string>();
        }

        public void Save()
        {
            string jsonData = UnityEngine.JsonUtility.ToJson(this, true);
            string jsonPath = VFileSystem.GetRealPath(filePath);
            System.IO.File.WriteAllText(jsonPath, jsonData);
        }
    }    
}
