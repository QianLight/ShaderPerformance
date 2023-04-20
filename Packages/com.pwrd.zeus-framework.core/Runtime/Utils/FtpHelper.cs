/*******************************************************************
* Copyright © 2017—2021 Perfect World.Co.Ltd. All rights reserved.
* author: 移动项目支持部 ZEUS FRAMEWORK TEAM
********************************************************************/
namespace Zeus.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.IO;
    using UnityEngine;

    /// <summary>
    /// FTP文件信息
    /// </summary>
    public class FtpFileInfo {
        public string Name = string.Empty;
        public bool IsDirectory = false;
        public List<FtpFileInfo> DirectoryList = null;
        public string CreateTime = string.Empty;
        public long Size = -1;
    }

    public partial class FtpHelper
    {
        private string _Uri = string.Empty;
        private string _UserName = "anonymous";
        private string _Password = "@anonymous";
        private IWebProxy _Proxy = null;

        private FtpWebRequest _Request = null;
        private FtpWebResponse _Response = null;

        #region 构造函数与析构函数
        /// <summary>
        /// 创建匿名FTP传输实例
        /// </summary>
        /// <param name="uri">FTP地址</param>
        public FtpHelper(string uri) { _Uri = uri; }

        /// <summary>
        /// 创建FTP传输实例
        /// </summary>
        /// <param name="uri">FTP地址</param>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        public FtpHelper(string uri, string userName, string password) : this(uri) { _UserName = userName; _Password = password; }

        /// <summary>
        /// 创建FTP传输实例
        /// </summary>
        /// <param name="uri">FTP地址</param>
        /// <param name="userName">用户名</param>
        /// <param name="password">密码</param>
        /// <param name="proxy">FTP通信代理</param>
        public FtpHelper(string uri, string userName, string password, IWebProxy proxy) : this(uri, userName, password) { _Proxy = proxy; }

        ~FtpHelper() {
            if (_Response != null) {
                _Response.Close();
            }
            if (_Request != null)
            {
                _Request.Abort();
            }
        }
        #endregion

        #region 创建指定FTP协议方法的连接对象
        /// <summary>
        /// 建立指定FTP协议方法的传输，并返回FTP服务器请求对象
        /// </summary>
        /// <param name="ftpMethod">FTP协议方法</param>
        /// <returns>返回FTP服务器请求对象</returns>
        private FtpWebRequest CreateRequest(string ftpMethod) {
            return CreateRequest(new Uri(_Uri), ftpMethod);
        }

        /// <summary>
        /// 建立指定FTP协议方法的传输，并返回FTP服务器请求对象
        /// </summary>
        /// <param name="ftpMethod">FTP协议方法</param>
        /// <param name="remotePath">远程目录</param>
        /// <returns>返回FTP服务器请求对象</returns>
        private FtpWebRequest CreateRequest(string ftpMethod, string remotePath) {
            return CreateRequest(new Uri(_Uri + "/" + remotePath), ftpMethod);
        }

        /// <summary>
        /// 建立指定FTP协议方法的传输，并返回FTP服务器请求对象
        /// </summary>
        /// <param name="uri">FTP地址</param>
        /// <param name="ftpMethod">FTP协议方法</param>
        /// <returns>返回FTP服务器请求对象</returns>
        private FtpWebRequest CreateRequest(Uri uri, string ftpMethod)
        {
            try
            {
                _Request = FtpWebRequest.Create(uri) as FtpWebRequest;
                _Request.Method = ftpMethod;
                _Request.Credentials = new NetworkCredential(_UserName, _Password);
                //_Request.KeepAlive = false;
                _Request.Timeout = 10000;
                _Request.ReadWriteTimeout = 10000;
                _Request.UseBinary = true;
                if (_Proxy != null) { _Request.Proxy = _Proxy; }
                return _Request;
            }
            catch (WebException ex)
            {
                Debug.LogError(ex.Message);
                //throw ex;
                return null;
            }
        }

        /// <summary>
        /// 建立指定FTP协议方法的传输，并返回FTP服务器响应对象
        /// </summary>
        /// <param name="ftpMethod">FTP协议方法</param>
        /// <returns>返回FTP服务器响应对象</returns>
        private FtpWebResponse CreateResponse(string ftpMethod) {
            return CreateResponse(ftpMethod, string.Empty);
        }

        /// <summary>
        /// 建立指定FTP协议方法的传输，并返回FTP服务器响应对象
        /// </summary>
        /// <param name="ftpMethod">FTP协议方法</param>
        /// <param name="remotePath">远程目录</param>
        /// <returns>返回FTP服务器响应对象</returns>
        private FtpWebResponse CreateResponse(string ftpMethod, string remotePath) {
           return  _Response = CreateRequest(ftpMethod, remotePath).GetResponse() as FtpWebResponse;
        }
        #endregion

        #region 操作远程目录文件信息
        /// <summary>
        /// 获取FTP服务器上的文件的简短列表
        /// </summary>
        /// <returns>返回文件的简短列表</returns>
        public List<string> GetAllFtpListDirectory()
        {
            return GetAllFtpListDirectory(string.Empty);
        }

        /// <summary>
        /// 获取FTP服务器上指定路径下的文件的简短列表
        /// </summary>
        /// <param name="remotePath">远端路径</param>
        /// <returns>返回文件的简短列表</returns>
        public List<string> GetAllFtpListDirectory(string remotePath)
        {
            List<string> list = new List<string>();
            CreateResponse(WebRequestMethods.Ftp.ListDirectory, remotePath);
            using (StreamReader reader = new StreamReader(_Response.GetResponseStream()))
            {
            Tag_StreamReader: string line = reader.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    list.Add(line);
                    goto Tag_StreamReader;
                }
                reader.Close();
            }
            return list;
        }


        /// <summary>
        /// 获取FTP服务器上的所有文件的详细列表
        /// </summary>
        /// <returns></returns>
        public List<FtpFileInfo> GetAllFtpListDirectoryDetails() { return GetAllFtpListDirectoryDetails(string.Empty); }

        /// <summary>
        /// 获取FTP服务器上指定路径下的所有文件的详细列表
        /// </summary>
        /// <param name="remotePath">远端路径</param>
        /// <param name="includeSub">是否获取子目录中的文件</param>
        /// <returns></returns>
        public List<FtpFileInfo> GetAllFtpListDirectoryDetails(string remotePath, bool includeSub = true) {
            List<FtpFileInfo> list = new List<FtpFileInfo>();
            try
            {
                CreateResponse(WebRequestMethods.Ftp.ListDirectoryDetails, remotePath);
                StreamReader reader = new StreamReader(_Response.GetResponseStream());
            Tag_StreamReader: string line = reader.ReadLine();
                if (!string.IsNullOrEmpty(line))
                {
                    Debug.Log(line);
                    var fileInfo = ConvertToFtpFileInfo(line);
                    if (fileInfo != null)
                    {
                        if (includeSub && fileInfo.IsDirectory)
                        {
                            fileInfo.DirectoryList = GetAllFtpListDirectoryDetails(remotePath + "/" + fileInfo.Name + "/");
                        }
                        list.Add(fileInfo);
                    }
                    goto Tag_StreamReader;
                }
                reader.Close();
            }
            catch (WebException ex)
            {
                Debug.LogError(ex.Message);
            }
            return list;
        }

        /// <summary>
        /// 【虚方法】将字符串转换成FtpFileInfo
        /// </summary>
        protected virtual FtpFileInfo ConvertToFtpFileInfo(string ftpFileInfo)
        {
            string[] strArr = ftpFileInfo.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (strArr.Length >= 4)
            {
                FtpFileInfo result = new FtpFileInfo();
                // Name
                string name = string.Empty;
                for (int index = 3; index < strArr.Length; index++)
                {
                    name += " " + strArr[index];
                }
                result.Name = name.Trim();
                // CreateTime
                result.CreateTime = string.Format("{0} {1}", strArr[0], strArr[1]);
                //  Directory
                if (ftpFileInfo.Contains("<DIR>"))
                {
                    result.IsDirectory = true;
                }
                // Size
                else
                {
                    result.Size = Int64.Parse(strArr[2]);
                }

                return result;
            }
            return null;
        }

        /// <summary>
        /// 指定的远程文件是否存在
        /// </summary>
        /// <param name="remotePath"></param>
        /// <returns></returns>
        public bool ExistsRemotePath(string remotePath) {
            try
            {
                if (string.IsNullOrEmpty(remotePath)) return true;

                string checkName = Path.GetFileName(remotePath);
                if (string.IsNullOrEmpty(checkName)) { checkName = Path.GetFileName(Path.GetDirectoryName(remotePath)); }
                List<FtpFileInfo> list = GetAllFtpListDirectoryDetails(remotePath, false);
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Name == checkName) return true;
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 在FTP服务器上创建目录
        /// </summary>
        /// <param name="remotePath">远端路径</param>
        public bool MakeDirectory(string remotePath)
        {
            try
            {
                CreateResponse(WebRequestMethods.Ftp.MakeDirectory, remotePath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 尝试在FTP服务器上创建嵌套文件目录
        /// </summary>
        /// <param name="remotePath">远端路径</param>
        /// <returns></returns>
        public void TryMakeDirectory(string remotePath)
        {
            try
            {
                if (!ExistsRemotePath(remotePath))
                {
                    string[] folder = Path.GetDirectoryName(remotePath).Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                    string dir = string.Empty;
                    for (int index = 0; index < folder.Length; index++)
                    {
                        dir = Path.Combine(dir, folder[index]);
                        MakeDirectory(dir);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        /// <summary>
        /// 尝试在FTP服务器上移除文件目录
        /// </summary>
        /// <param name="remotePath">远端路径</param>
        public bool TryRemoveDirectory(string remotePath)
        {
            try
            {
                if (string.IsNullOrEmpty(remotePath)|| ExistsRemotePath(remotePath)) return false;

                CreateResponse(WebRequestMethods.Ftp.RemoveDirectory, remotePath);
                return true;
            }
            catch (WebException ex)
            {
                Debug.LogError(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 尝试在FTP服务器上删除文件
        /// </summary>
        /// <param name="remotePath">远端路径</param>
        public bool TryDeleteFile(string remotePath)
        {
            try
            {
                if (string.IsNullOrEmpty(remotePath) || ExistsRemotePath(remotePath)) return false;

                CreateResponse(WebRequestMethods.Ftp.DeleteFile, remotePath);
                return true;
            }
            catch (WebException ex)
            {
                Debug.LogError(ex.Message);
                return false;
            }
        }
        #endregion

        #region 上传文件
        /// <summary>
        /// 上传文件到FTP服务器，默认覆盖远端文件
        /// </summary>
        /// <param name="localPath">本地带有完整路径的文件名</param>
        public void UploadFile(string localPath)
        {
             UploadFile(localPath, localPath, true);
        }

        /// <summary>
        /// 上传文件到FTP服务器
        /// </summary>
        /// <param name="localPath">本地带有完整路径的文件</param>
        /// <param name="isOverride">是否覆盖远程服务器上面同名的文件</param>
        public void UploadFile(string localPath, bool isOverride)
        {
             UploadFile(localPath, localPath, isOverride);
        }
        /// <summary>
        /// 上传文件到FTP服务器，默认覆盖远端文件
        /// </summary>
        /// <param name="localPath">本地带有完整路径的文件</param>
        /// <param name="remotePath">要在FTP服务器上面保存文件名</param>
        public void UploadFile(string localPath, string remotePath)
        {
             UploadFile(localPath, remotePath, true);
        }
        /// <summary>
        /// 上传文件到FTP服务器
        /// </summary>
        /// <param name="localPath">本地带有完整路径的文件名</param>
        /// <param name="remotePath">要在FTP服务器上面保存文件名</param>
        /// <param name="isOverride">是否覆盖远程服务器上面同名的文件</param>
        public void UploadFile(string localPath, string remotePath, bool isOverride)
        {
            try
            {
                if (File.Exists(localPath))
                {
                    FileStream Stream = new FileStream(localPath, FileMode.Open, FileAccess.Read);
                    byte[] bt = new byte[Stream.Length];
                    Stream.Read(bt, 0, (Int32)Stream.Length); 
                    Stream.Close();
                    UploadFile(bt, remotePath, isOverride);
                }
                else
                {
                    Debug.LogError(string.Format("本地不存在 {0} 文件", localPath));
                }
            }
            catch (WebException ex)
            {
                Debug.LogError(ex.Message);
            }
        }
        /// <summary>
        /// 上传文件到FTP服务器，默认覆盖远端文件
        /// </summary>
        /// <param name="bytes">上传的二进制数据</param>
        /// <param name="remotePath">要在FTP服务器上面保存文件名</param>
        public void UploadFile(byte[] bytes, string remotePath)
        {
            UploadFile(bytes, remotePath, true);
        }
        /// <summary>
        /// 上传文件到FTP服务器
        /// </summary>
        /// <param name="bytes">文件二进制内容</param>
        /// <param name="remotePath">要在FTP服务器上面保存文件名</param>
        /// <param name="isOverride">是否覆盖远程服务器上面同名的文件</param>
        public void UploadFile(byte[] bytes, string remotePath, bool isOverride)
        {
            try
            {
                TryMakeDirectory(remotePath);

                if (!isOverride && ExistsRemotePath(remotePath))
                {
                    Debug.LogWarning("FTP服务上面已经存在同名文件！");
                    return;
                }
                CreateRequest(WebRequestMethods.Ftp.UploadFile, remotePath);
                Stream requestStream = _Request.GetRequestStream();
                MemoryStream memoryStream = new MemoryStream(bytes);

                const int bufferSize = 1024 * 64;
                byte[] buffer = new byte[bufferSize];
                int bytesRead = 0;
                int TotalRead = 0;
                while (true)
                {
                    bytesRead = memoryStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                        break;
                    TotalRead += bytesRead;
                    requestStream.Write(buffer, 0, bytesRead);
                }
                requestStream.Close();
                _Response = (FtpWebResponse)_Request.GetResponse();
                memoryStream.Close();
                memoryStream.Dispose();
                bytes = null;
            }
            catch (WebException ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        /// <summary>
        /// 上传目录中的文件到FTP服务器
        /// </summary>
        /// <param name="localPath">要上传的目录</param>
        public void UploadFiles(string localPath)
        {
            UploadFiles(Directory.GetFiles(localPath, "*", SearchOption.TopDirectoryOnly));
        }

        /// <summary>
        /// 上传目录中的文件到FTP服务器
        /// </summary>
        /// <param name="localPath">要上传的目录</param>
        /// <param name="searchOption">指定上传操作应包括所有子目录还是仅包括当前目录</param>
        public void UploadFiles(string localPath, SearchOption searchOption)
        {
            UploadFiles(Directory.GetFiles(localPath, "*", searchOption));
        }

        /// <summary>
        /// 上传目录中的文件到FTP服务器
        /// </summary>
        /// <param name="localPath">要上传的目录</param>
        /// <param name="searchPattern">要与 path 中的文件名匹配的上传字符串。此参数不能以两个句点（“..”）结束，不能在 System.IO.Path.DirectorySeparatorChar或 System.IO.Path.AltDirectorySeparatorChar 的前面包含两个句点（“..”），也不能包含 System.IO.Path.InvalidPathChars中的任何字符。</param>
        public void UploadFiles(string localPath, string searchPattern)
        {
            UploadFiles(Directory.GetFiles(localPath, searchPattern, SearchOption.TopDirectoryOnly));
        }

        /// <summary>
        /// 上传目录中的文件到FTP服务器
        /// </summary>
        /// <param name="localPath">要上传的目录</param>
        /// <param name="searchPattern">要与 path 中的文件名匹配的上传字符串。此参数不能以两个句点（“..”）结束，不能在 System.IO.Path.DirectorySeparatorChar或 System.IO.Path.AltDirectorySeparatorChar 的前面包含两个句点（“..”），也不能包含 System.IO.Path.InvalidPathChars中的任何字符。</param>
        /// <param name="searchOption">指定上传操作应包括所有子目录还是仅包括当前目录</param>
        public void UploadFiles(string localPath, string searchPattern, SearchOption searchOption)
        {
            UploadFiles(Directory.GetFiles(localPath, searchPattern, searchOption));
        }

        /// <summary>
        /// 上传列表中的文件到FTP服务器
        /// </summary>
        /// <param name="localFileList">要上次的文件列表</param>
        public void UploadFiles(string[] localFileList)
        {
            UploadFiles(localFileList, string.Empty);
        }

        /// <summary>
        /// 上传列表中的文件到FTP服务器
        /// </summary>
        /// <param name="localFileList">要上次的文件列表</param>
        /// <param name="removePathRoot">要上传的远程根目录</param>
        public void UploadFiles(string[] localFileList, string removePathRoot) {
            for (int i = 0; i < localFileList.Length; i++)
            {
                UploadFile(localFileList[i], Path.Combine(removePathRoot, localFileList[i]));
                //ThreadPool.QueueUserWorkItem(h =>
                //{
                //    UploadFile(Path.Combine(removePath, localFileList[i]));
                //});
            }
        }

        #endregion

        #region 下载文件        
        /// <summary>
        /// 从FTP服务器下载文件，使用与远程文件同名的文件名来保存文件，默认覆盖文件
        /// </summary>
        /// <param name="remotePath">远端带有完整路径的文件名</param>
        public void DownloadFile(string remotePath)
        {
            DownloadFile(remotePath, remotePath, true);
        }

        /// <summary>
        /// 从FTP服务器下载文件，使用与远程文件同名的文件名来保存文件
        /// </summary>
        /// <param name="remotePath">远端带有完整路径的文件名</param>
        /// <param name="isOverride">是否覆盖本地同名的文件</param>
        public void DownloadFile(string remotePath, bool isOverride)
        {
            DownloadFile(remotePath, remotePath, isOverride);
        }

        /// <summary>
        /// 从FTP服务器下载文件，保存到指定路径，默认覆盖文件
        /// </summary>
        /// <param name="remotePath">远端带有完整路径的文件名</param>
        /// <param name="localPath">本地带有完整路径的文件名</param>
        public void DownloadFile(string remotePath, string localPath)
        {
            DownloadFile(remotePath, localPath, true);
        }

        /// <summary>
        /// 从FTP服务器下载文件，保存到指定路径
        /// </summary>
        /// <param name="remotePath">远端带有完整路径的文件名</param>
        /// <param name="localPath">本地带有完整路径的文件名</param>
        /// <param name="isOverride">是否覆盖本地同名的文件</param>
        public void DownloadFile(string remotePath, string localPath, bool isOverride)
        {
            try
            {
                if (!isOverride && Directory.Exists(localPath))
                {
                    Debug.LogWarning(string.Format("本地 {0} 文件已存在", localPath));
                    return;
                }
                byte[] buffer = DownloadFileBytes(remotePath);
                if (buffer != null)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(remotePath));
                    File.WriteAllBytes(localPath, buffer);
                }
            }
            catch (WebException ex)
            {
                Debug.LogError(ex.Message);
            }
        }

        /// <summary>
        /// 从FTP服务器下载文件，返回文件二进制数据
        /// </summary>
        /// <param name="remotePath">远程文件名</param>
        public byte[] DownloadFileBytes(string remotePath)
        {
            try
            {
                CreateResponse(WebRequestMethods.Ftp.DownloadFile, remotePath);
                Stream responseStream = _Response.GetResponseStream();
                MemoryStream memoryStream = new MemoryStream(1024 * 512);

                const int bufferSize = 1024 * 64;
                byte[] buffer = new byte[bufferSize];
            Tag_Read:
                int readCount = readCount = responseStream.Read(buffer, 0, buffer.Length);
                if (readCount > 0) { 
                    memoryStream.Write(buffer, 0, readCount);
                    goto Tag_Read;
                }
                if (memoryStream.Length > 0)
                {
                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }
            return null;
        }

        #endregion

    }
}
