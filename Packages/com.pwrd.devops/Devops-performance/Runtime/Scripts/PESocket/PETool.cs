/****************************************************
	文件：PETool.cs
	作者：Plane
	邮箱: 1785275942@qq.com
	日期：2018/10/30 11:21   	
	功能：工具类
*****************************************************/

using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using UnityEngine;

namespace PENet
{
    public class NetMsgSender : PEMsg
    {
        public const int SendBufferMaxSize = 1024 * 1024;
        public byte[] buffer = new byte[SendBufferMaxSize];
        private int cursor = 0;
        public NetMsgSender(int msgId)
        {
            cursor = 4;
            WriteInt(msgId);
        }

        public void Reset()
        {
            cursor = 8;
        }

        public void TryExpandBuffer(int msgId, int length)
        {
            if(length > buffer.Length)
            {
                buffer = new byte[length];
            }

            cursor = 4;
            WriteInt(msgId);
        }

        public int GetBufferSize()
        {
            return cursor;
        }

        public void WriteString(string str)
        {
            byte[] strBuffer = Encoding.GetEncoding("UTF-8").GetBytes(str);
            WriteInt(strBuffer.Length);
            Buffer.BlockCopy(strBuffer, 0, buffer, cursor, strBuffer.Length);
            cursor += strBuffer.Length;
        }

        public void WriteStringArray(string[] strArray)
        {
            if(strArray == null)
            {
                WriteInt(0);
            }
            else
            {
                WriteInt(strArray.Length);
                for(int i = 0; i < strArray.Length; i++)
                {
                    WriteString(strArray[i]);
                }
            }
        }

        public void WriteBool(bool b)
        {
            byte[] bs = BitConverter.GetBytes(b);
            Buffer.BlockCopy(bs, 0, buffer, cursor, bs.Length);
            cursor += bs.Length;
        }

        public void WriteInt(int n)
        {
            byte[] bs = BitConverter.GetBytes(n);
            Buffer.BlockCopy(bs, 0, buffer, cursor, bs.Length);
            cursor += bs.Length;
        }

        public void WriteLong(long l)
        {
            byte[] bs = BitConverter.GetBytes(l);
            Buffer.BlockCopy(bs, 0, buffer, cursor, bs.Length);
            cursor += bs.Length;
        }

        public void WriteByteArray(byte[] bytes)
        {
            if(bytes == null)
            {
                WriteInt(0);
            }
            else
            {
                WriteInt(bytes.Length);
                Buffer.BlockCopy(bytes, 0, buffer, cursor, bytes.Length);
                cursor += bytes.Length;
            }
        }

        public void WriteByte(byte b)
        {
            buffer[cursor] = b;
            cursor += 1;
        }

        public void WriteEnd()
        {
            if (cursor == 8)
            {
                WriteByte(0);
            }
            byte[] bs = BitConverter.GetBytes(cursor - 8);
            Buffer.BlockCopy(bs, 0, buffer, 0, bs.Length);
        }
    }

    public class NetMsgReceiver : PEMsg
    {
        byte[] buffer;
        int cursor = 0;
        public int msgId;
        public NetMsgReceiver(byte[] buffer)
        {
            this.buffer = buffer;
            msgId = ReadInt();
        }

        public int ReadInt()
        {
            int n = BitConverter.ToInt32(buffer, cursor);
            cursor += sizeof(int);
            return n;
        }

        public long ReadLong()
        {
            long l = BitConverter.ToInt64(buffer, cursor);
            cursor += sizeof(long);
            return l;
        }

        public bool ReadBool()
        {
            bool b = BitConverter.ToBoolean(buffer, cursor);
            cursor += sizeof(bool);
            return b;
        }

        public string ReadString()
        {
            int length = ReadInt();
            string str = Encoding.GetEncoding("UTF-8").GetString(buffer, cursor, length);
            cursor += length;
            return str;
        }

        public string[] ReadStringArray()
        {
            int length = ReadInt();
            if (length == 0)
                return null;
            string[] strArray = new string[length];
            for(int i = 0; i < length; i++)
            {
                strArray[i] = ReadString();
            }
            return strArray;
        }


        public byte[] ReadByteArray()
        {
            int length = ReadInt();
            if (length == 0)
                return null;
            byte[] bytes = new byte[length];
            Buffer.BlockCopy(buffer, cursor, bytes, 0, bytes.Length);
            cursor += length;
            return bytes;
        }
    }

    public class PETool
    {

        public static byte[] PackNetMsg<T>(T msg) where T : PEMsg
        {
            return PackLenInfo(Serialize(msg));
        }

        /// <summary>
        /// Add length info to package
        /// </summary>
        public static byte[] PackLenInfo(byte[] data)
        {
            int len = data.Length;
            byte[] pkg = new byte[len + 4];
            byte[] head = BitConverter.GetBytes(len);
            head.CopyTo(pkg, 0);
            data.CopyTo(pkg, 4);
            return pkg;
        }

        public static byte[] Serialize<T>(T msg) where T : PEMsg
        {
            string json = UnityEngine.JsonUtility.ToJson(msg);
            return Compress(System.Text.Encoding.Default.GetBytes(json));
            //DataContractJsonSerializer serializer = new DataContractJsonSerializer(msg.GetType());
            //using (MemoryStream ms = new MemoryStream())
            //{
            //    serializer.WriteObject(ms, msg);
            //    return Compress(ms.ToArray());
            //}
        }

        public static T DeSerialize<T>(byte[] bytes) where T : PEMsg
        {
            string str = System.Text.Encoding.Default.GetString(DeCompress(bytes));
            return UnityEngine.JsonUtility.FromJson<T>(str);
            //using (MemoryStream ms = new MemoryStream(DeCompress(bytes)))
            //{
            //    T obj = Activator.CreateInstance<T>();
            //    DataContractJsonSerializer serializer = new DataContractJsonSerializer(obj.GetType());
            //    obj = (T)serializer.ReadObject(ms);
            //    ms.Close();
            //    return obj;
            //}
        }
        public static byte[] Compress(byte[] input)
        {
            using (MemoryStream outMS = new MemoryStream())
            {
                using (GZipStream gzs = new GZipStream(outMS, CompressionMode.Compress, true))
                {
                    gzs.Write(input, 0, input.Length);
                    gzs.Close();
                    return outMS.ToArray();
                }
            }
        }

        public static byte[] DeCompress(byte[] input)
        {
            using (MemoryStream inputMS = new MemoryStream(input))
            {
                using (MemoryStream outMS = new MemoryStream())
                {
                    using (GZipStream gzs = new GZipStream(inputMS, CompressionMode.Decompress))
                    {
                        byte[] bytes = new byte[1024];
                        int len = 0;
                        while ((len = gzs.Read(bytes, 0, bytes.Length)) > 0)
                        {
                            outMS.Write(bytes, 0, len);
                        }
                        //高版本可用
                        //gzs.CopyTo(outMS);
                        gzs.Close();
                        return outMS.ToArray();
                    }
                }
            }
        }

        #region Log
        public static bool log = true;
        public static Action<string, int> logCB = null;
        public static void LogMsg(string msg, LogLevel lv = LogLevel.None)
        {
            if (log != true)
            {
                return;
            }
            //Add Time Stamp
            msg = DateTime.Now.ToLongTimeString() + " >> " + msg;
            if (logCB != null)
            {
                logCB(msg, (int)lv);
            }
            else
            {
                if (lv == LogLevel.None)
                {
                    Debug.Log(msg);
                }
                else if (lv == LogLevel.Warn)
                {
                    //Console.ForegroundColor = ConsoleColor.Yellow;
                    Debug.LogWarning(msg);
                    //Console.ForegroundColor = ConsoleColor.Gray;
                }
                else if (lv == LogLevel.Error)
                {
                    //Console.ForegroundColor = ConsoleColor.Red;
                    Debug.LogError(msg);
                    //Console.ForegroundColor = ConsoleColor.Gray;
                }
                else if (lv == LogLevel.Info)
                {
                    //Console.ForegroundColor = ConsoleColor.Green;
                    Debug.Log(msg);
                    //Console.ForegroundColor = ConsoleColor.Gray;
                }
                else
                {
                    //Console.ForegroundColor = ConsoleColor.Red;
                    Debug.LogError(msg + " >> Unknow Log Type\n");
                    //Console.ForegroundColor = ConsoleColor.Gray;
                }
            }
        }
        #endregion
    }

    /// <summary>
    /// Log Level
    /// </summary>
    public enum LogLevel
    {
        None = 0,// None
        Warn = 1,//Yellow
        Error = 2,//Red
        Info = 3//Green
    }
}