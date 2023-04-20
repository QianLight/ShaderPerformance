using System;

namespace PENet
{
    /// <summary>
    ///  该枚举和蓝图内的MessageType对应，保证通信消息类型相同
    /// </summary>
    public enum MessageType
    {
        Connected = 1,
        UISync = 2,
        CreateUnityLogicPackage = 3,
        CreateActor = 4,
        UpdateActorParam = 5,
        UnityConsoleDoubleClick = 6,
        UnityIsPlay = 7,
        BlueprintResourcePath = 8,
        CreateActorPrefab = 9,
        CreateChildActor = 10,
        CreateChildActorPrefab = 11,
        RefreshAssets = 12,
        ReNameActor = 13,
        ActorClassName = 14,
        CopyActor = 15,
        QueryAcotrParent = 16,
        OpenActor = 17,
        DeleteActor = 18,
        AllNotExportClasses = 19,
        UnityProcessId = 20,
        StartBPDebugMode = 21,
        ExitBPDebugMode = 22,
        ExitUnityPlayMode = 23,
        RestartServer = 100,
    }

    [Serializable]
    public class MessageData
    {
        public MessageType messageType;

        public string data = string.Empty;
    }

}