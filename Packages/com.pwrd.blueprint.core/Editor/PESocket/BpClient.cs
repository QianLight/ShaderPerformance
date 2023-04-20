using Protocol;
using UnityEditor;
using UnityEngine;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;
using System.Net.NetworkInformation;
using UnityEditor.Experimental.GraphView;
using System.IO;
using System.Linq;

namespace PENet
{
    [Serializable]
    public class LinkData
    {
        public string BPProjectName;
        public string BPProjectPath;
        public string BPEditorVersion;
    }

    [InitializeOnLoad]
    public class BpClient
    {
        public static PESocket<ClientSession, NetMsg> server;

        public static bool IsConnected = false;

        public static List<System.Diagnostics.Process> BpProcesses = new List<System.Diagnostics.Process>();

        public static int currentProcessIndex = 0;

        public static int currentPostId;

        public static HashSet<int> WrongPostIds = new HashSet<int>();

        public static int HasConnectedCount = 0;

        public static readonly ConcurrentQueue<string> dataQueue = new ConcurrentQueue<string>();

        public static Action<MessageData> OnReceiveMessage;

        public static string UnityAssetPath;

        public static string BindBpPjtPath;

        public static string LinkPath = "\\Packages\\com.pwrd.blueprint.core\\link.json";

        public static string BpPath = "";

        public static bool connected = false;

        public static long startTime = 0;

        public static long connectStartTime = 0;

        public static long timeSpan = 2;

        static BpClient()
        {
            EditorApplication.update += Update;
            startTime = DateTimeOffset.Now.ToUnixTimeSeconds();
            AssemblyReloadEvents.beforeAssemblyReload += CloseSocket;
        }
             
        public static void OnImportPackageStarted(string packagename)
        {
            Debug.Log(packagename);
        }

        public static void CloseSocket()
        {
            IsConnected = false;
            if (server != null)
            {
                try
                {
                    server.Close();
                }
                finally
                {
                    server = null;
                }
                EditorApplication.playModeStateChanged -= Bluepirnt.Debug.BlueprintDebugController.ChangedPlaymodeState;
                Debug.Log("Close BlueprintServer");
            }
        }

        [MenuItem("Blueprint/Reconnect Blueprint")]
        public static void ReConnection()
        {
            HasConnectedCount = 0;
            BpProcesses.Clear();
            WrongPostIds.Clear();
            CloseSocket();
        }

        public static void SendMessage(string str)
        {
            server.session.SendMessage(str);
        }
         
        public static void SendMessage(MessageType messageType, string str)
        {
            MessageData data = new MessageData()
            {
                messageType = messageType,
                data = str,
            };
            server.session.SendMsg(new NetMsg
            {
                text = JsonUtility.ToJson(data),
            });
        }

        public static void Update()
        {
            if (!IsConnected && HasConnectedCount >= BpProcesses.Count() && DateTimeOffset.Now.ToUnixTimeSeconds() - startTime > timeSpan)
            {
                HasConnectedCount = 0;
                GetAllBpProcesses();
                startTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                connectStartTime = DateTimeOffset.Now.ToUnixTimeSeconds();
                ConnectNextBpProcess();
            }

            while (dataQueue.Count > 0)
            {
                string data;
                if (dataQueue.TryDequeue(out data))
                {
                    MessageData message = JsonUtility.FromJson<MessageData>(data);
                    OnReceiveMessage?.Invoke(message);
                }
            }
        }
        public static bool CheckPort(int port)
        {
            bool isAvailable = true;

            // Evaluate current system tcp connections. This is the same information provided
            // by the netstat command line application, just in .Net strongly-typed object
            // form.  We will look through the list, and if our port we would like to use
            // in our TcpClient is occupied, we will set isAvailable to false.
            IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
            {
                if (tcpi.LocalEndPoint.Port == port)
                {
                    isAvailable = false;
                    break;
                }
            }

            return isAvailable;
        }

        private static void SetBindBpPjtPath()
        {
            var path = Directory.GetCurrentDirectory() + LinkPath;
            if (File.Exists(path))
            {
                string jsonStr = File.ReadAllText(path);
                var linkData = JsonUtility.FromJson<LinkData>(jsonStr);
                var bpPath = linkData.BPProjectPath;
                BindBpPjtPath = !Path.IsPathRooted(bpPath) ? Path.GetFullPath(Path.Combine(Directory.GetParent(Application.dataPath).ToString(), bpPath)) : bpPath;
            }
        }

        public static void GetAllBpProcesses()
        {
            currentProcessIndex = 0;
            BpProcesses.Clear();
            SetBindBpPjtPath();
            if (string.IsNullOrEmpty(BindBpPjtPath))
                return;
            try
            {
                BpProcesses.AddRange(System.Diagnostics.Process.GetProcessesByName("BlueprintEditor"));
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        public static void ConnectNextBpProcess()
        {
            CloseSocket();
            if (BpProcesses.Count > currentProcessIndex)//先在所有蓝图进程中连接第一个，根据返回的路径判断是否是正确的，不正确再连接下一个
            {
                if (WrongPostIds.Contains(currentPostId))
                {
                    currentProcessIndex++;
                    ConnectNextBpProcess();
                    return;
                }
                currentPostId = ((int)BpProcesses[currentProcessIndex].Id + 256) % 65536;
                server = new PENet.PESocket<ClientSession, NetMsg>();
                server.StartAsClient(IPCfg.srvIP, currentPostId);
                currentProcessIndex++;
            }
        }

        private static IEnumerable<string> SplitCommandLine(string commandLine)
        {
            bool inQuotes = false;

            return Split(commandLine, c =>
            {
                if (c == '\"')
                    inQuotes = !inQuotes;

                return !inQuotes && c == ' ';
            })
                              .Select(arg => TrimMatchingQuotes(arg.Trim(), '\"'))
                              .Where(arg => !string.IsNullOrEmpty(arg));
        }

        public static IEnumerable<string> Split(string str,
                                            Func<char, bool> controller)
        {
            int nextPiece = 0;

            for (int c = 0; c < str.Length; c++)
            {
                if (controller(str[c]))
                {
                    yield return str.Substring(nextPiece, c - nextPiece);
                    nextPiece = c + 1;
                }
            }

            yield return str.Substring(nextPiece);
        }

        public static string TrimMatchingQuotes(string input, char quote)
        {
            if ((input.Length >= 2) &&
                (input[0] == quote) && (input[input.Length - 1] == quote))
                return input.Substring(1, input.Length - 2);

            return input;
        }
    }
}
