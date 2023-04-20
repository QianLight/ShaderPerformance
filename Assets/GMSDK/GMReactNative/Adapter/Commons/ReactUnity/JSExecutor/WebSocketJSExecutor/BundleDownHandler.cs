/*
 * 加载bundlejs 任务，其实际上是一个mulitpart的post请求。 格式如下：
 * --boundary
 * Content-Type: application/json
 *
 * {done: xx, total: yy}
 * --boundary
 * Content-Type: application/json
 * {done: xx, total: yy}
 *
 * ...
 *
 * --boundary
 * X-Metro-Files-Changed-Count: xx
 * Content-Type: application/js
 *
 * jscode
 * --boundary--
 */

using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace GSDK.RNU
{
    public class BundleDownHandler: DownloadHandlerScript
    {
        private static string CRLF = "\r\n";
        private string boundary = "";
        private string delimiter;
        private string dataStrLeft = "";
        private long hasDownloaded = 0;
        private long contentLength = 0;
        private bool isBundleChunk = false;
        

        private Action<BundleChunkInfo, Dictionary<string, string>> progressAction = null;
        private Action<long, long> completeAction = null;
        private Action<BundleErrorChunkInfo> errorAction = null;

        public BundleDownHandler(Action<BundleChunkInfo, Dictionary<string, string>> progressAction, Action<long, long> completeAction, Action<BundleErrorChunkInfo> errorAction)
        {
            this.progressAction = progressAction;
            this.completeAction = completeAction;
            this.errorAction = errorAction;
        }
        
        protected override bool ReceiveData(byte[] byteFromServer, int dataLength)
        {
            if (byteFromServer == null || byteFromServer.Length < 1 || dataLength == 0)
            {
                return true;
            }

            var chunks = Encoding.UTF8.GetString(byteFromServer, 0, dataLength);
            // 如果是最后一个BundleChunk，开始记录下载进度
            if (isBundleChunk)
            {
                var endIndex = chunks.IndexOf("--" + boundary + "--", StringComparison.Ordinal);
                if (endIndex == -1)
                {
                    // not end yet
                    hasDownloaded += chunks.Length;
                    OnComplete(hasDownloaded, contentLength);
                    return true;
                }
                else
                {
                    OnComplete(contentLength, contentLength);
                    return false;
                }
            }
            
            if (boundary == "")
            {
                var index1 = chunks.IndexOf("--", StringComparison.Ordinal) + 2;
                if (index1 == 1)
                {
                    return true;
                }
                var index2 = chunks.IndexOf(CRLF, index1, StringComparison.Ordinal);
                boundary = chunks.Substring(index1, index2 - index1);
                delimiter = "--" + boundary + CRLF;
            }

            chunks = dataStrLeft + chunks;
            dataStrLeft = "";
            
            var startIndex = 0;
            while (true)
            {
                var startIndexOfChunk = chunks.IndexOf(delimiter, startIndex, StringComparison.Ordinal);
                if (startIndexOfChunk == -1)
                {
                    dataStrLeft += chunks;
                    break;
                }
                
                var endIndexOfChunk = chunks.IndexOf(delimiter, startIndexOfChunk + delimiter.Length, StringComparison.Ordinal);
                if (endIndexOfChunk == -1)
                {
                    
                    var endIndex = chunks.IndexOf("--" + boundary + "--", StringComparison.Ordinal);
                    if (endIndex != -1)
                    {
                        // 请求结束
                        var chunk = chunks.Substring(startIndexOfChunk + delimiter.Length, endIndex - startIndexOfChunk - delimiter.Length);
                        OnChunk(chunk);
                        return false;
                    }
                    
                    var thisDataLeft = chunks.Substring(startIndexOfChunk);
                    // X-Metro-Files-Changed-Count表明chunk 是bundle， bundlechunk需要记录下载进度
                    if (thisDataLeft.Contains("X-Metro-Files-Changed-Count"))
                    {
                        isBundleChunk = true;
                        var chunkSplit = new string[] {CRLF + CRLF};
                        var chunkInfo = thisDataLeft.Split(chunkSplit, StringSplitOptions.None);

                        var headers = GetHeaders(chunkInfo[0]);
                        if (headers.ContainsKey("Content-Length"))
                        {
                            contentLength = long.Parse(headers["Content-Length"]);
                            hasDownloaded += chunkInfo[1].Length;
                            completeAction(hasDownloaded, contentLength);
                        }
                    }
                    else
                    {
                        dataStrLeft += thisDataLeft;
                    }
                    break;
                }
                else
                {
                    var chunk = chunks.Substring(startIndexOfChunk + delimiter.Length, endIndexOfChunk - startIndexOfChunk - delimiter.Length);

                    OnChunk(chunk);
                    startIndex = endIndexOfChunk;
                }
            }
            
            return true;
        }

        private void OnChunk(string chunk)
        {
            var chunkSplit = new string[] {CRLF + CRLF};
            var chunkInfo = chunk.Split(chunkSplit, StringSplitOptions.None);

            var header = chunkInfo[0];
            var body = chunkInfo[1];
            
            var headers = GetHeaders(header);
            

            if (headers.ContainsKey("X-Http-Status") && headers["X-Http-Status"] == "500")
            {
                var errorInfo = JsonUtility.FromJson<BundleErrorChunkInfo>(body);
                errorAction(errorInfo);
                return;
            }
            

            if (headers.ContainsKey("Content-Type") && headers["Content-Type"] == "application/json")
            {
                try
                {
                    var ci = JsonUtility.FromJson<BundleChunkInfo>(body);
                    progressAction(ci, headers);
                }
                catch (Exception e)
                {
                    progressAction(new BundleChunkInfo(), headers);
                }
            }
        }

        private void OnComplete(long hasDown, long total)
        {
            completeAction(hasDown, total);
        }
        
        private static Dictionary<string, string> GetHeaders(string headersStr)
        {
            var headerSplit = new string[] {CRLF};
            var headers = headersStr.Split(headerSplit, StringSplitOptions.None);

            var r = new Dictionary<string, string>();
            foreach (var header in headers)
            {
                var headerKv = header.Split(':');
                if (headerKv.Length == 1)
                {
                    r.Add(headerKv[0].Trim(), "");
                }
                else
                {
                    r.Add(headerKv[0].Trim(), headerKv[1].Trim());
                }
            }

            return r;
        }
    }
}

