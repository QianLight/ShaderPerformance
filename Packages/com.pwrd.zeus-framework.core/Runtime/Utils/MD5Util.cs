/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Security.Cryptography;
using System;
using System.Xml;
using System.Text;

namespace Zeus
{
    public class MD5FileInfo
    {
        public string Name { set; get; }
        public string MD5 { set; get; }
        public long Size { set; get; }
    }

    public static class MD5Util
    {
        public static bool CompareMD5(string sourceMD5, MD5FileInfo targetMD5Info)
        {
            return CompareMD5(sourceMD5, targetMD5Info.MD5);
        }

        public static bool CompareMD5(string sourceMD5, string targetMD5)
        {
            return sourceMD5 == targetMD5;
        }

        public static MD5FileInfo GetMD5FileInfoFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError(string.Format("Can't find path \"{0}\"", filePath));
            }
            MD5FileInfo info = new MD5FileInfo();
            info.Name = Path.GetFileName(filePath);
            DateTime start = DateTime.Now;
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                MD5CryptoServiceProvider md5GenGenerator = new MD5CryptoServiceProvider();
                byte[] md5Hash = md5GenGenerator.ComputeHash(fs);
                info.MD5 = BitConverter.ToString(md5Hash);
                info.Size = fs.Length;
            }
            TimeSpan cost = DateTime.Now - start;
            Debug.Log(string.Format("Generate md5 from {0} cost {1} s", filePath, cost.TotalSeconds));
            return info;
        }

        public static Dictionary<string, MD5FileInfo> DecodeMD5Version(string versionFile)
        {
            var stream = File.Open(versionFile, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            try
            {
                Dictionary<string, MD5FileInfo> md5FileInfos = new Dictionary<string, MD5FileInfo>();

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(stream);

                XmlNode rootNode = xmlDoc.SelectSingleNode("MD5Files");

                foreach (XmlElement fileElement in rootNode.ChildNodes)
                {
                    MD5FileInfo fileInfo = new MD5FileInfo();
                    fileInfo.Name = fileElement.GetAttribute("name");
                    fileInfo.MD5 = fileElement.GetAttribute("md5").ToLower();
                    fileInfo.Size = long.Parse(fileElement.GetAttribute("size"));
                    md5FileInfos.Add(fileInfo.Name, fileInfo);
                }

                xmlDoc = null;
                return md5FileInfos;
            }
            catch (Exception e)
            {
                throw e;
            }
            finally
            {
                stream.Close();
            }
        }



        public static string GetMD5FromFile(string filePath)
        {
            try
            {
                using (var file = new FileStream(filePath, FileMode.Open))
                {
                    System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    byte[] retVal = md5.ComputeHash(file);
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < retVal.Length; i++)
                    {
                        sb.Append(retVal[i].ToString("x2"));
                    }
                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail, error:" + ex.Message);
            }
        }
    }

}