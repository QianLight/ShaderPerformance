using System;
using System.Runtime.InteropServices;
using AOT;
#if UNITY_EDITOR
using Ender.LitJson;
using GMSDK;
using UnityEngine;

namespace Ender
{
    public class EnderRemoteManager : ServiceSingleton<EnderRemoteManager>
    {
        private static string uid;
        private static string channelId;
        private static int connectCount;
        private static IEnderRemoteCallback enderRemoteCallback;

        private delegate void enderRecvMsgCallback(string message);

        private delegate void enderRecvAlohaMsgCallback(string message);

        private delegate void enderConnectionsCallback(int count);

        private delegate void enderSelfStateCallback(int state);

        private delegate void enderOnErrorCallback(int error);

        [DllImport("GMEnderRemote")]
        private static extern void InitChannel(string roomid, string uid, string token);

        [DllImport("GMEnderRemote")]
        private static extern void ReleaseChannel();

        [DllImport("GMEnderRemote")]
        private static extern void ReNewToken(string token);

        [DllImport("GMEnderRemote")]
        private static extern void SendMsg(int msgType, string msg);

        [DllImport("GMEnderRemote")]
        private static extern void SetEnderRemoteMessageCallback(enderRecvMsgCallback callback);

        [DllImport("GMEnderRemote")]
        private static extern void SetEnderRemoteAlohaMessageCallback(enderRecvAlohaMsgCallback callback);

        [DllImport("GMEnderRemote")]
        private static extern void SetEnderRemoteConnectionsCallback(enderConnectionsCallback callback);

        [DllImport("GMEnderRemote")]
        private static extern void SetEnderRemoteSelfStateCallback(enderSelfStateCallback callback);

        [DllImport("GMEnderRemote")]
        private static extern void SetEnderRemoteOnErrorCallback(enderOnErrorCallback callback);

        [MonoPInvokeCallback(typeof(enderRecvMsgCallback))]
        public static void HandleMsgFromNative(string message)
        {
            Debug.Log("Ender Remote HandleMsgFromNative: " + message);
            if (enderRemoteCallback != null)
            {
                enderRemoteCallback.HandleEnderRemoteMsgFromNative(message);
            }
        }

        [MonoPInvokeCallback(typeof(enderRecvAlohaMsgCallback))]
        public static void HandleAlohaMsgFromNative(string message)
        {
            Debug.Log("Ender Remote HandleAlohaMsgFromNative: " + message);
            if (enderRemoteCallback != null)
            {
                enderRemoteCallback.HandleEnderRemoteAlohaMsgFromNative(message);
            }
        }

        [MonoPInvokeCallback(typeof(enderConnectionsCallback))]
        public static void HandleConnectionChange(int count)
        {
            Debug.Log("Ender Remote HandleConnectionChange: " + count);
            connectCount = count;
            if (enderRemoteCallback != null)
            {
                enderRemoteCallback.HandleConnectionChange(count);
            }
        }

        [MonoPInvokeCallback(typeof(enderSelfStateCallback))]
        public static void HandleSelfStateChange(int state)
        {
            Debug.Log("Ender Remote HandleSelfStateChange: " + state);
            if (state == EnderRemoteConstants.SelfState.Retry)
            {
                Debug.LogError("当前连接已断开，请检查网络状态，重试中...");
            }
        }

        [MonoPInvokeCallback(typeof(enderOnErrorCallback))]
        public static void HandleError(int error)
        {
            Debug.Log("Ender Remote HandleError: " + error);
            if (error == -1000) // token invalid
            {
                EnderHttpRequestUtils.GetRtcToken(channelId, uid, (token) =>
                {
                    if (String.IsNullOrEmpty(token))
                    {
                        Debug.LogError("renew token, get token failed");
                        return;
                    }

                    ReNewToken(token);
                });
            }
            else
            {
                //todo handle other error
            }
        }

        private EnderRemoteManager()
        {
        }

        ~EnderRemoteManager()
        {
            Release();
        }

        public void OnDestroy() {
            Release();
        }

        public void Init(string localUid, string localChannelId)
        {
            uid = localUid;
            channelId = localChannelId;
            InitRemoteNetTool();
        }

        private void InitRemoteNetTool()
        {
            EnderHttpRequestUtils.GetRtcToken(channelId, uid, (token) =>
            {
                if (String.IsNullOrEmpty(token))
                {
                    Debug.LogError("ender remote: get token failed");
                    return;
                }

                Debug.Log("remote net tool token: " + token);
                InitChannel(channelId, uid, token);
                SetEnderRemoteMessageCallback(HandleMsgFromNative);
                SetEnderRemoteAlohaMessageCallback(HandleAlohaMsgFromNative);
                SetEnderRemoteConnectionsCallback(HandleConnectionChange);
                SetEnderRemoteSelfStateCallback(HandleSelfStateChange);
                SetEnderRemoteOnErrorCallback(HandleError);
            });
        }


        public void Release()
        {
            ReleaseChannel();
            enderRemoteCallback = null;
            connectCount = 0;
            SetEnderRemoteMessageCallback(null);
            SetEnderRemoteAlohaMessageCallback(null);
            SetEnderRemoteConnectionsCallback(null);
            SetEnderRemoteSelfStateCallback(null);
            SetEnderRemoteOnErrorCallback(null);
        }

        public void SetEnderRemoteCallback(IEnderRemoteCallback remoteCallback)
        {
            enderRemoteCallback = remoteCallback;
        }
        
        public bool IsConnected()
        {
            return connectCount > 0;
        }
        
        public void SendMessage(int msgType, string message)
        {
             SendMsg(msgType, message);
        }

        public void CheckCloudDeviceIsAvailable(string serial, long deadLine, Action<bool> onResult, Action<long> addTimeResult)
        {
            if (GetTimeStamp() < deadLine)
            {
                onResult(true);
                if (deadLine - GetTimeStamp() < 600000) //如果使用时距离deadline少于10min，则自动续期
                {
                    Debug.Log("automatic device renewal");
                    EnderHttpRequestUtils.AddDeviceTime(serial, success =>
                    {
                        if (success)
                        {
                            EnderHttpRequestUtils.QueryEnderRemoteDeviceInfo(serial, response =>
                            {
                                JsonData data = JsonMapper.ToObject(response);
                                long newDeadLine = long.Parse(data["occupation_deadline"].ToString()) * 1000;
                                Debug.Log("device new deadline: " + newDeadLine);
                                addTimeResult(newDeadLine);
                            });
                        }
                    });
                }
            }
            else
            {
                onResult(false);
            }

        }
        
        public static long GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            var ret = Convert.ToInt64(ts.TotalSeconds * 1000);
            return ret;
        }
    }
}
#endif