/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace Zeus.Framework.Http.UnityHttpDownloader.Tool
{
    public static class CheckAlgorithmUtil
    {
        public static string GetMD5FromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    using (var fs = new FileStream(filePath, FileMode.Open))
                    {
                        System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                        byte[] retVal = md5.ComputeHash(fs);
                        System.Text.StringBuilder sb = new System.Text.StringBuilder();
                        for (int i = 0; i < retVal.Length; i++)
                        {
                            sb.Append(retVal[i].ToString("x2"));
                        }
                        return sb.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail, error:" + ex.Message);
            }
            return string.Empty;
        }




        public static int GetCrc32FromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    using (var fs = new FileStream(filePath, FileMode.Open))
                    {
                        uint result = 0;
                        byte[] buffer = new byte[2048];
                        while (true)
                        {
                            int count = fs.Read(buffer, 0, 2048);
                            if (count > 0)
                            {
                                result = Crc32Algorithm.Append(result, buffer, 0, count);
                            }
                            else
                            {
                                break;
                            }
                        }
                        return (int)result;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail, error:" + ex.Message);
            }
            return 0;
        }
    }
}
