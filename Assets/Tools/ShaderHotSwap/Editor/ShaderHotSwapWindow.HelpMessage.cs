using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UsingTheirs.ShaderHotSwap
{
    public partial class ShaderHotSwapWindow
    {
        #region Help Message
        string helpMessage;
        MessageType helpMessageType;
        double lastMessageTime;
        int lastMessageIndex;
        const float minHelpMessageShowSec = 1f;

        protected void ShowHelpMessage()
        {
            if (!string.IsNullOrEmpty(helpMessage))
            {
                //EditorGUILayout.HelpBox(helpMessage, helpMessageType);
                var width = EditorGUIUtility.currentViewWidth - 20;
                var height  = EditorStyles.helpBox.CalcHeight(new GUIContent(helpMessage), width);
                height = Mathf.Max(40, height);
                var rc = new Rect(10, 20, width, height);
                EditorGUI.HelpBox(rc, helpMessage, helpMessageType);
                EditorGUI.HelpBox(rc, helpMessage, helpMessageType);
                EditorGUI.HelpBox(rc, helpMessage, helpMessageType);
            }
        }

        protected bool HasErrorMessage()
        {
            return !string.IsNullOrEmpty(helpMessage) && helpMessageType == MessageType.Error;
        }

        protected virtual void SetHelpMessage(string message, MessageType type, bool autoClear = true)
        {
            if (type == MessageType.Error)
                Logger.LogError(message);
                
            helpMessage = message;
            helpMessageType = type;
            lastMessageTime = EditorApplication.timeSinceStartup;
            lastMessageIndex++;
            Repaint();
            
            if (autoClear)
                ClearHelpMessage();
        }

        protected void ClearHelpMessage()
        {
            if (EditorApplication.timeSinceStartup - lastMessageTime >= minHelpMessageShowSec)
            {
                ClearHelpMessageImpl();
            }
            else
            {
                EditorCoroutine.Start(DelayedClearHelpMessage(lastMessageIndex));
            }

        }

        IEnumerator DelayedClearHelpMessage(int clearMessageIndex)
        {
            yield return new EditorCoroutine.CustomWaitForSeconds(minHelpMessageShowSec);

            if (clearMessageIndex == lastMessageIndex)
                ClearHelpMessageImpl();
        }

        void ClearHelpMessageImpl()
        {
            helpMessage = null;
            Repaint();
        }
        #endregion
        
    }
}