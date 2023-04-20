/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
#if UNITY_EDITOR
using System.IO;
using UnityEngine;

namespace Zeus.Build
{
    /// <summary>
    /// 用来处理XCode工程中需要修改的class
    /// </summary>
    public partial class XCodeClassHelper : System.IDisposable
    {

        private string filePath;

        public XCodeClassHelper(string path)
        {
            filePath = path;
            if (!System.IO.File.Exists(filePath))
            {
                Debug.LogError("路径" + filePath + "处文件不存在");
                return;
            }
        }

        /// <summary>
        /// 在指定标识后插入代码
        /// </summary>
        /// <param name="sign">标识</param>
        /// <param name="code">要插入的代码</param>
        public void WriteBelow(string sign, string code)
        {
            StreamReader streamReader = new StreamReader(filePath);
            string result = streamReader.ReadToEnd();
            streamReader.Close();

            int beginIndex = result.IndexOf(sign);
            if (beginIndex == -1)
            {
                Debug.LogError(filePath + "中没有找到标识" + sign);
                return;
            }

            int endIndex = result.LastIndexOf("\n", beginIndex + sign.Length);

            result = result.Substring(0, endIndex) + "\n" + code + "\n" + result.Substring(endIndex);

            StreamWriter streamWriter = new StreamWriter(filePath);
            streamWriter.Write(result);
            streamWriter.Close();
        }
        /// <summary>
        /// 在指定标识前插入代码
        /// </summary>
        /// <param name="sign">标识</param>
        /// <param name="code">要插入的代码</param>
        public void WriteAbove(string sign, string code)
        {
            StreamReader streamReader = new StreamReader(filePath);
            string result = streamReader.ReadToEnd();
            streamReader.Close();

            int index = result.IndexOf(sign);
            if (index == -1)
            {
                Debug.LogError(filePath + "中没有找到标识" + sign);
                return;
            }

            result = result.Substring(0, index) + "\n" + code + "\n" + result.Substring(index);

            StreamWriter streamWriter = new StreamWriter(filePath);
            streamWriter.Write(result);
            streamWriter.Close();
        }
        /// <summary>
        /// 在最后添加代码
        /// </summary>
        /// <param name="code">要添加的代码</param>
        public void WriteEnd(string code)
        {
            StreamReader streamReader = new StreamReader(filePath);
            string result = streamReader.ReadToEnd();
            streamReader.Close();
            result = result + "\n" + code;
            StreamWriter streamWriter = new StreamWriter(filePath);
            streamWriter.Write(result);
            streamWriter.Close();
        }
        /// <summary>
        /// 替换
        /// </summary>
        public void Replace(string sign, string code)
        {
            StreamReader streamReader = new StreamReader(filePath);
            string result = streamReader.ReadToEnd();
            streamReader.Close();

            int beginIndex = result.IndexOf(sign);
            if (beginIndex == -1)
            {
                Debug.LogError(filePath + "中没有找到标识" + sign);
                return;
            }

            result = result.Replace(sign, code);
            StreamWriter streamWriter = new StreamWriter(filePath);
            streamWriter.Write(result);
            streamWriter.Close();
        }

        public void Dispose()
        {
        }
    }
}
#endif