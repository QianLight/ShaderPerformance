/*******************************************************************
* Copyright © 2017—2020 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Zeus.Core.FileSystem
{
    /// <summary>
    /// 所有非AssetBundle的文件列表
    /// </summary>
    [Serializable]
    internal class OtherFileList
    {
        internal static OtherFileList Deserialize()
        {
            using (var stream = InnerPackage.OpenReadStream("ZeusSetting/OtherFileList"))
            {
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                OtherFileList ret = (OtherFileList)binaryFormatter.Deserialize(stream);
                return ret;
            }
        }

        internal List<string> m_ListFileName;


#if UNITY_EDITOR

        private const string EDITOR_PATH = "ZeusSetting/OtherFileList";

        internal void Init()
        {
            m_ListFileName = new List<string>();
        }

        internal void SerializeToDisk()
        {
            VFileSystem.CreateParentDirectory(EDITOR_PATH);
            FileStream fileStream = new FileStream(VFileSystem.GetAssetsFolderRealPath(EDITOR_PATH), FileMode.OpenOrCreate);
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            binaryFormatter.Serialize(fileStream, this);
            fileStream.Close();
        }

        internal void DeleteFromDisk()
        {
            File.Delete(VFileSystem.GetAssetsFolderRealPath(EDITOR_PATH));
        }

#endif
    }
}
