#if ENABLE_UPO
using UPOLuaProfiler;
using UnityEngine;

namespace UPOHelper.Network
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using Utils;

    public class NetworkServer
    {
        private static Queue<UPOMessage> m_queue = new Queue<UPOMessage>(256);

        private static NetworkStream ns;
        private static BinaryWriter bw;
        private static BinaryReader br;
        private static bool isConnected = false;

        private static TcpClient m_client = null;
        private static TcpListener _tcpListener;

        private static string host = "0.0.0.0";
        private static int listenPort = 57000;

        private static Thread m_sendThread;
        private static Thread m_receiveThread;
        private static bool keepReceive = true;
        private static bool keepSend = true;
        private static bool keepListen = true;

        public static void ConnectTcpPort(int port)
        {
            // 检查可用端口并返回
            listenPort = GetAvailablePort(port, 100, "tcp");
#if !UNITY_EDITOR
            DumpPort(listenPort);
#endif
            Thread listenThead = new Thread(new ThreadStart(StartListening));
            listenThead.Start();
        }

        private static int GetAvailablePort(int beginPort, int maxIter, string type)
        {
            int availablePort = beginPort;
            for (int port = beginPort; port < beginPort + maxIter; port++)
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                try
                {
                    socket.Bind(ep);
                    socket.Close();
                    availablePort = port;
                    Debug.LogWarning("upo networkserver@GetAvailablePort success:" + availablePort);
                    break;
                }
                catch (SocketException)
                {
                    Debug.LogError("upo networkserver@GetAvailablePort, Port not available:" + port.ToString());
                }
            }

            return availablePort;
        }

        private static void DumpPort(int port)
        {
            try
            {
                string fileName = "/sdcard/upo/upo_port";
                Directory.CreateDirectory("/sdcard/upo/");
                if (!File.Exists(fileName))
                {
                    Debug.Log("upo networkserver@DumpPort, create file" + fileName);
                    File.Create(fileName).Dispose();
                }

                File.WriteAllText(fileName, port.ToString());
            }
            catch (Exception e)
            {
                Debug.LogError("upo networkserver@DumpPort, write port failed: " + e.Message);
            }
        }

        private static void StartListening()
        {
            if (m_client != null) return;

            keepListen = true;
            IPAddress myIP = IPAddress.Parse(host);
            _tcpListener = new TcpListener(myIP, listenPort);
            _tcpListener.Start();
            Debug.LogWarning("upo NetworkServer:Listening IP: " + host + " on port: " + listenPort);

            while (keepListen)
            {
                try
                {
                    m_client = _tcpListener.AcceptTcpClient();

                    if (_tcpListener == null)
                        return;

                    if (m_client != null)
                    {
                        Debug.LogWarning("upo NetworkServer:<color=#00ff00>Package Connect Success</color>");
                        m_client.Client.SendTimeout = 30000;

                        ns = m_client.GetStream();
                        bw = new BinaryWriter(ns);
                        br = new BinaryReader(ns);

                        if (m_sendThread == null)
                        {
                            m_sendThread = new Thread(DoSendMessage);
                            // m_sendThread.Priority = ThreadPriority.Highest;
                            m_sendThread.Start();
                        }

                        if (m_receiveThread == null)
                        {
                            m_receiveThread = new Thread(DoReceiveMessage);
                            // m_receiveThread.Priority = ThreadPriority.Lowest;
                            Debug.Log("start receive thread");
                            m_receiveThread.Start();
                        }

                        // break;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Listening error:" + e);
                    Close();
                }

                Thread.Sleep(1000);
            }
        }

        public static void Close()
        {
            try
            {
                isConnected = false;
                keepReceive = false;
                keepSend = false;

                if (m_client != null)
                {
                    if (m_client.Connected)
                    {
                        m_client.Close();
                    }

                    m_client = null;
                }

                lock (m_queue)
                {
                    m_queue.Clear();
                }

                m_receiveThread = null;
                m_sendThread = null;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private static void DoSendMessage()
        {
            keepSend = true;
            while (keepSend)
            {
                try
                {
                    if (m_sendThread == null)
                    {
                        Debug.Log("<color=#ff0000>Package m_sendThread null</color>");
                        return;
                    }

                    if (m_queue.Count > 0)
                    {
                        while (m_queue.Count > 0)
                        {
                            UPOMessage s = null;
                            lock (m_queue)
                            {
                                s = m_queue.Dequeue();
                            }

                            SendRaw(s.Serialize());
                            
                        }
                    }

                    Thread.Sleep(100);
                }
                catch (ThreadAbortException e)
                {
                    Debug.Log(e);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                    Close();
                }
            }
        }

        private static void DoReceiveMessage()
        {
            Debug.Log("receive message");
            // string resultMess;
            keepReceive = true;
            while (keepReceive)
            {
                try
                {
                    if (m_receiveThread == null)
                    {
                        Debug.Log("<color=#ff0000>Package m_receiveThread null</color>");
                        return;
                    }

                    if (ns.CanRead && ns.DataAvailable)
                    {
                        UPOMessage msg = new UPOMessage();
                        msg.Deserialize(br);
                        if (!DealReceivedMessage(msg))
                            break;
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }

                Thread.Sleep(1000);
            }
        }

        public static void SendMessage(UPOMessageType type, byte[] rawData)
        {
            if (m_client == null)
            {
                Debug.Log("m_client is null, return");
                return;
            }

            UPOMessage message = new UPOMessage(type, rawData);
            lock (m_queue)
            {
                Debug.Log("put message in queue, " + type);
                m_queue.Enqueue(message);
            }
        }

        public static bool IsConnected()
        {
            return m_client != null;
        }

        private static void SendRaw(byte[] bytes)
        {
            // Debug.Log("send data @NetworkServer:SendRaw");
            bw.Write(bytes);
        }

        private static bool DealReceivedMessage(UPOMessage message)
        {
            switch (message.GetMessageType())
            {
                case UPOMessageType.Lua:
#if ENABLE_UPO_LUA
                    LuaNetworkService.OnReceiveMessage(message);
#endif
                    break;

                case UPOMessageType.Mono:
                    break;
                case UPOMessageType.Il2cpp:
                    break;
                case UPOMessageType.CustomizedData:
                    break;
                case UPOMessageType.Overdraw:
                    break;
                case UPOMessageType.Mipmap:
                    break;
                default:
                    Debug.LogError("unknown message type @DealReceivedMessage: " + message.GetMessageType());
                    break;
            }

            return true;
        }
    }
}
#endif