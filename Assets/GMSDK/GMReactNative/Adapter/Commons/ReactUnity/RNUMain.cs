/*
 * author yankang.nj
 * RNU 入口，仅包括一系列对外暴露的API，以静态方法提供，保持足够简单
 *
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GSDK.RNU
{
    public class RNUMain
    {
        // RNUMain 无实例类，仅提供静态接口、以及内部调用链路
        private RNUMain() {}

        private static string GameMessageEvent = "GameMessage";


        public static void Init() {
            RNUMainCore.Init();
        }

        public static void Init(MonoBehaviour context) {
            RNUMainCore.Init(context);
        }

        public static void Init(Dictionary<string, object> initParams) {
            RNUMainCore.Init(initParams);
        }

        public static void SetGameGoParent(string parentGoName)
        {
            GameInteraction.SetGameGoParent(parentGoName);
        }

        public static void SetRuGameAdvancedInjection(IRuGameAdvancedInjection gim)
        { 
            RNUMainCore.SetGameAdvancedInjection(gim);
        }
        
        public static void SetGameGoParent(GameObject parentGo)
        {
            GameInteraction.SetGameGoParent(parentGo);
        }

        public static void SetGameFont(string fontName, Font font)
        {
            GameInteraction.SetGameFont(fontName, font);
        }

        public static void SetGameData(Dictionary<string, object> gameData)
        {
            GameInteraction.SetGameData(gameData);
        }

        // 根据服务端配置的url 打开页面
        public static void OpenPage(string url, GameObject parentGo = null, Action closeCallback = null)
        {
            RNUMainCore.OpenPage(url, parentGo, closeCallback);
        }

        public static void DebugPage(string ip, string port, GameObject parentGo)
        {
            RNUMainCore.DebugPage(ip, port, parentGo);
        }
        
        public static void DebugPage(string debugURL, GameObject parentGo)
        {
            RNUMainCore.DebugPage(debugURL, parentGo);
        }

        // TODO 处理各种上面传递过来的Message
        public static void HandleEveryMessage()
        {
            // no-op now
        }

        
        // 处理九尾 2.0 UGUI 关闭点击事件系统
        public static void SetRNUTouchIgnore(GameObject gameObject, bool isIgnore)
        {
            GameInteraction.SetRNUTouchIgnore(gameObject, isIgnore);
        }
        
        
        public static void Close() {
            RNUMainCore.Close();
        }

        public static void SetGumihoPanelActive(bool isActive)
        {
            RNUMainCore.SetGumihoPanelActive(isActive);
        }

        public static void SendMessageToRu(string message)
        {
            RNUMainCore.SendUnityEventToJs(GameMessageEvent, message);
        }
    }
}
