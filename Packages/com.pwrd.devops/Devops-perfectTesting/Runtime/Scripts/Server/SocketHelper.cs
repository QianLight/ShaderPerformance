using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using Devops.Test;
using Devops.Core;
/*
* 
*Socket客户端通信类
* 
*/
public class SocketHelper
{

    public  enum SocketStatus
    { 
        TestStart,
        TestEnd,
        ClientSuccess,
        Status,
        TestFinish,
        TestWait,
    }

    private static SocketHelper socketHelper = new SocketHelper();

    private Socket _serverSocket;

    private Socket _clientSocket;


    private Action<string> callback;

    public bool Connect = false;
    //单例模式
    public static SocketHelper GetInstance()
    {
        return socketHelper;
    }

    private SocketHelper()
    {

        //采用TCP方式连接


        //异步连接,连接成功调用connectCallback方法
        //IAsyncResult result = _serverSocket.BeginConnect(endpoint, new AsyncCallback(ConnectCallback), _serverSocket);

        //这里做一个超时的监测，当连接超过5秒还没成功表示超时
        //bool success = result.AsyncWaitHandle.WaitOne(5000, true);
        //if (!success)
        //{
        //    //超时
        //    Closed();
        //    Debug.Log("SocketHelper connect Time Out");
        //}
        //else
        //{
        //    //与socket建立连接成功，开启线程接受服务端数据。
        //    Thread thread = new Thread(new ThreadStart(ReceiveSorket));
        //    thread.IsBackground = true;
        //    thread.Start();
        //}
        //Thread thread = new Thread(new ThreadStart(CreateSocketServer));
        //thread.IsBackground = true;
        //thread.Start();
    }

    public void StartServer()
    {
        CreateSocketServer();

    }

    public void SetCallBack(Action<string> callback)
    {
        this.callback = callback;

    }


    private void CreateSocketServer()
    {
        _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        //服务器IP地址
        IPAddress address = IPAddress.Parse("127.0.0.1");

        //服务器端口
        IPEndPoint endpoint = new IPEndPoint(address, PTestConfig.Port);

        _serverSocket.Bind(endpoint);

        Debug.Log("SocketHelper 开始监听");
        _serverSocket.Listen(100);


        _serverSocket.BeginAccept(new AsyncCallback(ClientConnectCB), _serverSocket);
        //_clientSocket = _clientSocket.Accept();


    }

    void ClientConnectCB(IAsyncResult ar)
    {
        try
        {
            Socket clientSkt = _serverSocket.EndAccept(ar);
            _clientSocket = clientSkt;
            ConnectCallback(ar);
        }
        catch (Exception e)
        {
            Debug.Log("SocketHelper ClientConnectCB" + e);
        }
        _serverSocket.BeginAccept(new AsyncCallback(ClientConnectCB), _serverSocket);


       

    }

    private void ConnectCallback(IAsyncResult asyncConnect)
    {
        Debug.Log("SocketHelper 有客户端连入");

        Debug.Log("SocketHelper connect" + asyncConnect.AsyncState.ToString());
        //与socket建立连接成功，开启线程接受服务端数据。
        SendMessage(SocketHelper.SocketStatus.ClientSuccess.ToString());
        SendMessage(SocketHelper.SocketStatus.Status.ToString());
        Connect = true;
        //PEPkg pack = new PEPkg();
        //_clientSocket.BeginReceive(
        //            pack.bodyBuff,
        //            0,
        //            0,
        //            SocketFlags.None,
        //            new AsyncCallback(RcvHeadData),
        //            _clientSocket);

        Thread thread = new Thread(new ThreadStart(ReceiveSorket));
        thread.IsBackground = true;
        thread.Start();
        Invoker.InvokeInMainThread(() => {
            DevopsScreenshot.Instance();
        });
    }

    private void RcvHeadData(IAsyncResult ar)
    {

    }

    private void ReceiveSorket()
    {
        //在这个线程中接受服务器返回的数据
        while (true)
        {

            if (!_clientSocket.Connected)
            {
                //与服务器断开连接跳出循环
                Debug.Log("SocketHelper Failed to clientSocket server.");
                _clientSocket.Close();
                break;
                //continue;
            }
            try
            {
                //接受数据保存至bytes当中
                byte[] bytes = new byte[4];
                //Receive方法中会一直等待服务端回发消息
                //如果没有回发会一直在这里等着。

                int i = _clientSocket.Receive(bytes);
                if (i <= 0)
                {
                    _clientSocket.Close();
                    break;
                }

                int temp = BitConverter.ToInt32(bytes, 0);
                //Debug.Log("SocketHelper Len" + temp);

                byte[] bytesbody = new byte[temp];

                i = _clientSocket.Receive(bytesbody);
                if (i <= 0)
                {
                    _clientSocket.Close();
                    break;
                }
                //Debug.Log("SocketHelper bytesbody" + System.Text.Encoding.Default.GetString(bytesbody));

                Invoker.InvokeInMainThread(()=> {
                    callback?.Invoke(System.Text.Encoding.Default.GetString(bytesbody));
                });
                //Debug.Log("SocketHelper " + System.Text.Encoding.Default.GetString(bytesbody));
            }
            catch (Exception e)
            {
                Debug.Log("SocketHelper Failed to clientSocket error." + e);
                _clientSocket.Close();

                _serverSocket.BeginAccept(new AsyncCallback(ClientConnectCB), _serverSocket);
                break;
            }
        }
    }

    //关闭Socket
    public void Closed()
    {
        if (_clientSocket != null && _clientSocket.Connected)
        {
            _clientSocket.Shutdown(SocketShutdown.Both);
            _clientSocket.Close();
        }
        _clientSocket = null;
    }
    //向服务端发送一条字符串
    //一般不会发送字符串 应该是发送数据包
    public void SendMessage(string str)
    {
        if (_clientSocket == null)
            return;

        //byte[] msg = Encoding.UTF8.GetBytes(str);

        byte[] bodyBytes = System.Text.Encoding.Default.GetBytes(str);
        Int32 len = (Int32)bodyBytes.Length;
        byte[] lenBytes = BitConverter.GetBytes(len);
        byte[] sendbytes = lenBytes.Concat(bodyBytes).ToArray();


        if (!_clientSocket.Connected)
        {
            _clientSocket.Close();
            return;
        }
        try
        {
            //SendMsg(msg);

            //Debug.Log("SocketHelper send length" + sendbytes.Length + "len" + len);

            //_clientSocket.BeginSend(lenBytes, 0, lenBytes.Length, 0, sendcallback, _clientSocket);
            //_clientSocket.BeginSend(bodyBytes, 0, bodyBytes.Length, 0, sendcallback, _clientSocket);
            _clientSocket.BeginSend(sendbytes, 0, sendbytes.Length, 0, sendcallback, _clientSocket);

            //IAsyncResult asyncSend = _clientSocket.BeginSend(msg, 0, msg.Length, SocketFlags.None, new AsyncCallback(SendCallback), _clientSocket);
            //bool success = asyncSend.AsyncWaitHandle.WaitOne(5000, true);
            //if (!success)
            //{
            //    _clientSocket.Close();
            //    Debug.Log("SocketHelper Failed to SendMessage server.");
            //}
        }
        catch
        {
            Debug.Log("SocketHelper send message error");
        }
    }

    private void sendcallback(IAsyncResult ar)
    {
        try
        {
            Socket socket = (Socket)ar.AsyncState;
            int count = socket.EndSend(ar);
            Debug.Log("Socket send succ ,number is:" + count);
        }
        catch (SocketException e)
        {
            Debug.Log("socket send erro:" + e.ToString());
        }
    }


    public void SendMsg(byte[] data)
    {
        NetworkStream ns = null;
        try
        {
            ns = new NetworkStream(_clientSocket);
            if (ns.CanWrite)
            {
                //Int16 len = (Int16)data.Length;
                byte[] lenBytes = BitConverter.GetBytes(data.Length);

                //byte[] b = BitConverter.GetBytes(data.Length);

                List<byte> sendByteList = new List<byte>();
                sendByteList.AddRange(lenBytes);
                sendByteList.AddRange(data);
                data = sendByteList.ToArray();
                ns.BeginWrite(
                    data,
                    0,
                    data.Length,
                    new AsyncCallback(SendCB),
                    ns);
            }
        }
        catch (Exception e)
        {
            Debug.Log("SndMsgError." + e);
        }
    }

    private void SendCB(IAsyncResult ar)
    {
        NetworkStream ns = (NetworkStream)ar.AsyncState;
        try
        {
            ns.EndWrite(ar);
            ns.Flush();
            ns.Close();
        }
        catch (Exception e)
        {
            Debug.Log("SndMsgError." + e);
        }
    }

    private void SendCallback(IAsyncResult asyncConnect)
    {
        Debug.Log("SocketHelper send success");
        _clientSocket.EndSend(asyncConnect);

    }


    private void ReStartSocket()
    {

    }

}