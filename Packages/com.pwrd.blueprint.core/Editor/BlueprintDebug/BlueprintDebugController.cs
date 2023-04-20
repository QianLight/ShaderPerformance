using System.Linq;
using UnityEditor;

namespace Bluepirnt.Debug
{
    using PENet;
    using System;
    using System.Collections.Generic;

    [InitializeOnLoad]
    class BlueprintDebugController
    {
        static BlueprintDebugController()
        {
            BpClient.OnReceiveMessage += HandleBlueprintMsg;
        }

        public static List<string> SaveNotExportClasses = new List<string>();

        public static void ChangedPlaymodeState(PlayModeStateChange changeState)
        {
            switch (changeState)
            {
                case PlayModeStateChange.ExitingEditMode:
                    BpClient.SendMessage(MessageType.StartBPDebugMode, String.Empty);
                    HasNotExportWarning();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    BpClient.SendMessage(MessageType.ExitBPDebugMode, String.Empty);
                    break;
            }
        }

        private static void HasNotExportWarning()
        {
            if (SaveNotExportClasses.Count > 0)
            {
                var warningText = string.Empty;
                foreach (var cls in SaveNotExportClasses)
                    warningText += cls + "\n";
                if (!EditorUtility.DisplayDialog("检测到以下类未导出,可能影响调试功能。是否继续运行？", warningText, "是", "否"))
                    EditorApplication.isPlaying = false;
            }
        }

        private static void HandleBlueprintMsg(MessageData messageData)
        {
            switch (messageData.messageType)
            {
                case MessageType.AllNotExportClasses:
                    if (!string.IsNullOrEmpty(messageData.data))
                        BlueprintDebugController.SaveNotExportClasses = messageData.data.Split(' ').ToList();
                    break;
                case MessageType.ExitUnityPlayMode:
                    EditorApplication.isPlaying = false;
                    break;
            }
        }
    }
}
