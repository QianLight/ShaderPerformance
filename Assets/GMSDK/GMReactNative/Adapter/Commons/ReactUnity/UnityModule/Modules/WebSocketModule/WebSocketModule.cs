/*
 * @author yankang.nj
 * 实现性的 勿用于生产环境， 暂时只用来实现开发模式的 reload 和hot reload
 */

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GSDK.RNU
{
    public class WebSocketModule: SimpleUnityModule
    {

        private static string NAME = "WebSocketModule";

        private Dictionary<int, SimpleWebSocket> webSocketConnections = new Dictionary<int, SimpleWebSocket>();


        public override string GetName()
        {
            return NAME;
        }

        [ReactMethod]
        public void sendBinary(string base64String, double forSocketID)
        {
            // TODO
        }

        [ReactMethod]
        public void removeListeners(double count)
        {
            // no-op now
        }

        [ReactMethod]
        public void ping(int socketID)
        {
            var client = webSocketConnections[socketID];
            client.Send("");
        }

        [ReactMethod]
        public void close(int code, string reason, int socketID)
        {
            var client = webSocketConnections[socketID];
            webSocketConnections.Remove(socketID);
            client.Close();
        }

        [ReactMethod]
        public void send(string message, int socketID)
        {

            var client = webSocketConnections[socketID];
            client.Send(message);
        }

        [ReactMethod]
        public void connect(string url, ArrayList protocols, Dictionary<string, object> options,
            int socketID)
        {
            if (webSocketConnections.ContainsKey(socketID))
            {
                return;
            }


            var client = new SimpleWebSocket();

            client.OnOpen(() =>
            {
                webSocketConnections.Add(socketID, client);
                var p = new Dictionary<string, object>
                {
                    {"id", socketID},
                    {"protocol", ""}
                };
                SendEvent("websocketOpen", p);
            });

            client.OnClose(() =>
            {
                var p = new Dictionary<string, object>()
                    {
                        {"id", socketID},
                        {"code", -1},
                        {"reason", "TODO"}
                    };

                SendEvent("websocketClosed", p);
            });
            client.OnMessage((text) =>
            {
                var p = new Dictionary<string, object>
                {
                    {"id", socketID},
                    {"type", "text"},
                    {"data", text}
                };

                SendEvent("websocketMessage", p);
            });

            client.Connect(url);
        }

        [ReactMethod]
        public void addListener(string eventName)
        {
            // no-op
        }


        private void SendEvent(string eventName, Dictionary<string, object> paramz) {
            RNUMainCore.CallJSFunction("RCTDeviceEventEmitter", "emit", new ArrayList()
            {
                eventName,
                paramz,
            });
        }

        public override void Destroy()
        {
            foreach (var conn in webSocketConnections)
            {
                conn.Value.Close();
            }
            webSocketConnections.Clear();
        }
    }
}
