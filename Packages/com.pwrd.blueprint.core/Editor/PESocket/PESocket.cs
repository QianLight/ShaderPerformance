
using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using Protocol;

namespace PENet {
    public class PESocket<T, K>
        where T : PESession<K>, new()
        where K : PEMsg {
        private Socket skt = null;
        public T session = null;
        public int backlog = 10;
        List<T> sessionLst = new List<T>();
        public bool close = false;

        public PESocket() {
            skt = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        #region Server
        /// <summary>
        /// Launch Server
        /// </summary>
        public bool StartAsServer(string ip, int port) {
            try {
                skt.Bind(new IPEndPoint(IPAddress.Parse(ip), port));
                skt.Listen(backlog);
                skt.BeginAccept(new AsyncCallback(ClientConnectCB), skt);
                PETool.LogMsg("\nServer Start Success!\nWaiting for Connecting......", LogLevel.Info);
                return true;
            }
            catch (Exception e) {
                PETool.LogMsg(e.Message, LogLevel.Error);
            }
            return false;
        }

        void ClientConnectCB(IAsyncResult ar) {
            if (skt == null || close)
            {
                return;
            }
            try {
                Socket clientSkt = skt.EndAccept(ar);
                UnityEngine.Debug.Log("client ");
                T session = new T();
                sessionLst.Add(session);
                session.StartRcvData(clientSkt, () => {
                    if (sessionLst.Contains(session)) {
                        sessionLst.Remove(session);
                    }
                });
            }
            catch (Exception e) {
                PETool.LogMsg(e.Message, LogLevel.Error);
            }
            try
            {
                skt.BeginAccept(new AsyncCallback(ClientConnectCB), skt);
            } catch (Exception e)
            {
                PETool.LogMsg(e.Message, LogLevel.Error);
            }
        }
        #endregion

        #region Client
        /// <summary>
        /// Launch Client
        /// </summary>
        public void StartAsClient(string ip, int port) {
            try {
                skt.BeginConnect(new IPEndPoint(IPAddress.Parse(ip), port), new AsyncCallback(ServerConnectCB), skt);
                PETool.LogMsg("\nClient Start Success!\nConnecting To Server......", LogLevel.Info);
            }
            catch (Exception e) {
                PETool.LogMsg(e.Message, LogLevel.Error);
            }
        }

        void ServerConnectCB(IAsyncResult ar) {
            try {
                skt.EndConnect(ar);
                session = new T();
                session.StartRcvData(skt, null);
            }
            catch (Exception e) {
                PETool.LogMsg(e.Message, LogLevel.Error);
                if (BpClient.currentProcessIndex == BpClient.BpProcesses.Count)//保证所有蓝图进程全部尝试连接一遍
                    BpClient.BpProcesses.Clear();
                else
                    BpClient.ConnectNextBpProcess();
            }
        }
        #endregion

        public void Close() {
            if (skt != null) {
                close = true;
                skt.Close();
                
                skt = null;
                sessionLst.Clear();
            }
        }

        public List<T> GetSesstionLst() {
            return sessionLst;
        }

        /// <summary>
        /// Log
        /// </summary>
        /// <param name="log">log switch</param>
        /// <param name="logCB">log function</param>
        public void SetLog(bool log = true, Action<string, int> logCB = null) {
            if (log == false) {
                PETool.log = false;
            }
            if (logCB != null) {
                PETool.logCB = logCB;
            }
        }
    }
}