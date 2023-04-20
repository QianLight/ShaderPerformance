/*
 * @author yankang.nj
 * 
 * .net framework 3.5 （gsdk的版本） 以下没有websocket实现，且.net framework 4.5 的websocket在unity2017下有bug。
 * 故通过socket重新实现websocket
 *
 *  websocket 是基于frame的协议，其frame格式如下：
 *   0                   1                   2                   3
 *   0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1 2 3 4 5 6 7 8 9 0 1
 *   +-+-+-+-+-------+-+-------------+-------------------------------+
 *   |F|R|R|R| opcode|M| Payload len |    Extended payload length    |
 *   |I|S|S|S|  (4)  |A|     (7)     |             (16/64)           |
 *   |N|V|V|V|       |S|             |   (if payload len==126/127)   |
 *   | |1|2|3|       |K|             |                               |
 *   +-+-+-+-+-------+-+-------------+ - - - - - - - - - - - - - - - +
 *   |     Extended payload length continued, if payload len == 127  |
 *   + - - - - - - - - - - - - - - - +-------------------------------+
 *   |                               |Masking-key, if MASK set to 1  |
 *   +-------------------------------+-------------------------------+
 *   | Masking-key (continued)       |          Payload Data         |
 *   +-------------------------------- - - - - - - - - - - - - - - - +
 *   :                     Payload Data continued ...                :
 *   + - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - +
 *   |                     Payload Data continued ...                |
 *   +---------------------------------------------------------------+
 *
 *   read：
 *   step1: 读 16个bit
 *   step2: 取第8bit 判断mask， 记录K = hasMask ？ 32 ： 0
 *   step3: 取9-15bit，转化为无符号整数L。 如果L < 126， 跳转到step6； 如果L = 126 跳转 step4； 如果L = 127 跳转 step5。
 *   step4: 继续读16个bit，转化为无符号整数L
 *   step5: 继续读64个bit，转化为无符号整数L
 *   step6: 取 M = L + K
 *   step7: 继续读取M个字节。
 *   step8: payload = [k, k+1, ....M]
 *
 *   send：(这里考虑到实现的简单，故只会发送小于126的frame)
 *   step1: message ---》 utf8 bytes。 并记录bytes的长度为L
 *   step2: 设 L = m * 125 + n; 
 *   step3: 构造m + n 个Frame。其中：
 *          第m +n个 FIN 位是1， 其他是0
 *          1，2，3位 都是0
 *          4-7位表示 opcode： 第1个 是0x1， 其他是0
 *          8位表示mask 是1 （客户端发服务端mask必须是1）
 *          9-15位表示 字节数。（前面是125，后面是n）
 *          接下来的32位是随机的 mask-key
 *          放实际的数据 125或者n
 *
 * TODO 这里并没有严谨的实现Sec-WebSocket-Key相关逻辑，没有严谨的实现mask-key的随机生成。
 *
 * 参考： https://datatracker.ietf.org/doc/html/rfc6455
 * 参考： https://developer.mozilla.org/zh-CN/docs/Web/API/WebSockets_API/Writing_WebSocket_servers
 */

using System;
using System.Collections;
using System.Text;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace GSDK.RNU
{
    internal struct WebSocketFrame
    {
        public bool isEndFrame;
        public byte[] payloads;
        public int opCode;
    }

    //TODO 需要SimpleWebSocket 本身保证onClose， onMessage等调用发生在主线程。使用者不必关心。
    public class SimpleWebSocket
    {
        private Socket socket;
        private Thread messageLoopThread;
        
        // 由于消息的生成在 messageLoopThread线程， 而listener的添加 可以在任意线程（一般在主线程） 这里需要保证线程安全性
        private readonly ArrayList messageListeners = ArrayList.Synchronized(new ArrayList());
        private readonly ArrayList closeListeners = ArrayList.Synchronized(new ArrayList());
        private readonly ArrayList openListeners = ArrayList.Synchronized(new ArrayList());

        ~SimpleWebSocket()
        {
            Close();
        }
        
        public void Connect(string url)
        {
            var one = url.Replace("ws://", "")
                .Replace("wss://", "");
            var two = one.Split(':');
            var address = two[0];

            var three = two[1].Split('/');
            var port = three[0];
            var path = one.Replace(address + ":" + port, "");
            Connect(address, Convert.ToInt32(port), path);
        }

        public void Connect(string address, int port, string path = "/")
        {
            var ipAddress = IPAddress.Parse(address);
            var ipe = new IPEndPoint(ipAddress, port);
            socket =
                new Socket(ipe.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipe);
            if (!socket.Connected)
            {
                throw new SystemException("socket connect error");
            }

            UpgradeReq(path, address + ":" + port);

            if (!CheckUpgradeRes())
            {
                throw new SystemException("invalid socket");
            }
            
            EmitOpen();

            messageLoopThread = new Thread(HandleSocketData);
            messageLoopThread.Start(this);
        }

        public void Send(string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            var byteLen = bytes.Length;
            var n = byteLen % 125;
            var m = (byteLen - n) / 125;

            for (var i = 0; i < 1 + m; i++)
            {
                byte opCode = 0x00;
                byte fin = 0x00;
                int len;
                if (i == 0) // first frame
                {
                    opCode = 0x01; // text
                }

                if (i == m) // last frame
                {
                    fin = 0x80; // fin = 1
                    len = n;
                }
                else
                {
                    len = 125;
                }

                var frame = GenerateFrame(bytes, (byte) (fin | opCode), i * 125, len);
                socket.Send(frame, 0);
            }
        }

        public void Close()
        {
            if (socket.Connected)
            {
                var frame = GenerateFrame(new byte[0], 0x80 | 0x08, 0, 0);
                socket.Send(frame);
                socket.Close();
                EmitClose();
            }
        }

        public void OnMessage(Action<string> messageHandler)
        {
            messageListeners.Add(messageHandler);
        }

        public void OnClose(Action closeHandler)
        {
            closeListeners.Add(closeHandler);
        }

        public void OnOpen(Action openHandler)
        {
            openListeners.Add(openHandler);
        }

        private void UpgradeReq(string path, string host)
        {
            var req = "GET " + path + " HTTP/1.1\r\n"
                      + "Host:" + host + "\r\n"
                      + "Upgrade: websocket\r\n"
                      + "Connection: Upgrade\r\n"
                      + "Sec-WebSocket-Key: " + GetWebSocketKey() + "\r\n"
                      + "Sec-WebSocket-Version:13\r\n\r\n";

            var reqBytes = Encoding.UTF8.GetBytes(req);
            socket.Send(reqBytes, reqBytes.Length, 0);
        }

        private bool CheckUpgradeRes()
        {
            var bytes = 0;

            var bytesReceived = new byte[1024];
            while (true)
            {
                bytes += socket.Receive(bytesReceived, bytes, 1, 0);

                if (bytes >= 4
                    && bytesReceived[bytes - 4] == '\r'
                    && bytesReceived[bytes - 3] == '\n'
                    && bytesReceived[bytes - 2] == '\r'
                    && bytesReceived[bytes - 1] == '\n'
                )
                {
                    var res = Encoding.UTF8.GetString(bytesReceived, 0, bytes);
                    var resHeaders = res.Split("\r\n".ToCharArray());

                    foreach (var header in resHeaders)
                    {
                        if (header.Contains("Sec-WebSocket-Accept"))
                        {
                            var acceptsKv = header.Split(':');
                            if (acceptsKv.Length == 2 && CheckServerKey())
                            {
                                return true;
                            }
                        }
                    }

                    return false;
                }
            }
        }

        private void EmitString(string message)
        {
            lock (messageListeners.SyncRoot)
            {
                foreach (Action<string> listener in messageListeners)
                {
                    listener(message);
                }
            }
        }

        private void EmitClose()
        {
            lock (closeListeners.SyncRoot)
            {
                foreach (Action listener in closeListeners)
                {
                    listener();
                }
            }
        }

        private void EmitOpen()
        {
            lock (openListeners.SyncRoot)
            {
                foreach (Action listener in openListeners)
                {
                    listener();
                }
            }
        }
        
        private void EmitBuffer(Byte[] message)
        {
            Util.Log("EmitBuffer len:" + message.Length);
        }

        private static string GetWebSocketKey()
        {
            //TODO 按照规范生成base64字符串
            return "dGhlIHNhbXBsZSBub25jZQ==";
        }

        private static bool CheckServerKey()
        {
            // TODO 按照规范校验服务端的值是否合法
            return true;
        }

        private static byte[] GetMaskKey()
        {
            return new byte[] {0x48, 0x84, 0xFF, 0xAA};
        }

        private static void HandleSocketData(object o)
        {
            var rus = (SimpleWebSocket) o;
            var socket = rus.socket;

            while (true)
            {
                var messageBytes = new byte[0];
                var messageCode = 0;

                WebSocketFrame wsf;
                do
                {
                    wsf = new WebSocketFrame();
                    ReadFrame(socket, ref wsf);
                    messageBytes = messageBytes.Concat(wsf.payloads).ToArray();
                    if (wsf.opCode != 0) // = 0 是连续帧 不需要赋值
                    {
                        messageCode = wsf.opCode;
                    }
                } while (!wsf.isEndFrame);

                if (messageCode == 1)
                {
                    var message = Encoding.UTF8.GetString(messageBytes, 0, messageBytes.Length);
                    rus.EmitString(message);
                }
                else if (messageCode == 2)
                {
                    rus.EmitBuffer(messageBytes);
                }
                else if (messageCode == 0x9) // ping 
                {
                    // res pong
                    rus.Pong(messageBytes);
                }
                else if (messageCode == 0x8)
                {
                    socket.Close();
                    rus.EmitClose();
                    return; // Thread close
                }
                else
                {
                    Util.Log("message format not support");
                }
            }
        }

        private void Pong(byte[] payloads)
        {
            var frame = GenerateFrame(payloads, 0x80 | 0x0A, 0, payloads.Length);
            socket.Send(frame, 0);
        }

        private static void ReadFrame(Socket socket, ref WebSocketFrame wsf)
        {
            var firstStep = ReadBytes(socket, 2);
            wsf.isEndFrame = (firstStep[0] & 0x80) != 0; //FIN = 1
            wsf.opCode = firstStep[0] & 0x0F;

            var l = firstStep[1]; // mask = 0 
            ulong m = 0;
            if (l < 126)
            {
                m = l;
            }
            else if (l == 126)
            {
                var secondStep = ReadBytes(socket, 2);

                if (BitConverter.IsLittleEndian)
                {
                    secondStep = secondStep.Reverse().ToArray();
                }

                m = BitConverter.ToUInt16(secondStep, 0);
            }
            else if (l == 127)
            {
                var secondStep = ReadBytes(socket, 8);

                if (BitConverter.IsLittleEndian)
                {
                    secondStep = secondStep.Reverse().ToArray();
                }

                m = BitConverter.ToUInt64(secondStep, 0);
            }
            
            var mByte = (long) (m);

            wsf.payloads = ReadBytes(socket, mByte);
        }

        private static byte[] GenerateFrame(byte[] bytes, byte header1, int offset, int len)
        {
            var frame = new byte[len + 6];
            frame[0] = header1;

            var smallLen = (byte) (len | 0x80);
            frame[1] = smallLen;

            var maskingKey = GetMaskKey();
            frame[2] = maskingKey[0];
            frame[3] = maskingKey[1];
            frame[4] = maskingKey[2];
            frame[5] = maskingKey[3];

            for (var i = 0; i < len; i++)
            {
                var item = bytes[offset + i];
                frame[6 + i] = (byte) (item ^ maskingKey[i & 0x3]);
            }

            return frame;
        }


        private static byte[] ReadBytes(Socket socket, long len)
        {
            var target = new byte[len];
            var currentLen = 0;
            while (currentLen < len)
            {
                currentLen += socket.Receive(target, currentLen, target.Length - currentLen, 0);
            }

            return target;
        }
    }
}