using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GMSDK
{
    public class GPMMonitor : MonoBehaviour
    {
        static private readonly string gameType = "game_scene";
        private Coroutine coroutine = null;
        private string currentScene = "";

        private IEnumerator FrameEndCoroutine()
        {
            WaitForEndOfFrame frameEndEnum = new WaitForEndOfFrame();
            while (true)
            {
                yield return frameEndEnum;
                GPMCXXBridge.LogFrameEnd();
            }
        }

        public void LogSceneStart(string sceneName)
        {
            if (coroutine != null)
            {
                LogSceneEnd(true);
            }
            GPMCXXBridge.LogSceneStart(gameType, sceneName);
            currentScene = sceneName;
            coroutine = StartCoroutine(FrameEndCoroutine());
            LogSceneInfo("rom_size_avail");
        }

        public void LogSceneLoaded()
        {
            if (coroutine != null)
            {
                GPMCXXBridge.LogSceneLoaded(gameType, currentScene);
            }
        }

        public void LogSceneEnd(bool isUpload)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
                GPMCXXBridge.LogSceneEnd(gameType, currentScene, isUpload);
                currentScene = "";
                coroutine = null;
            }
        }

        static public void LogGlobalInfo(string key, string value)
        {
            GPMCXXBridge.LogGlobalInfo(gameType, key, value);
        }

        static public void LogGlobalInfo(string key, int value)
        {
            GPMCXXBridge.LogGlobalInfo(gameType, key, value);
        }

        static public void LogGlobalInfo(string key)
        {
            GPMCXXBridge.LogGlobalInfo(gameType, key);
        }

        public void LogSceneInfo(string key, string value)
        {
            if (coroutine != null)
            {
                GPMCXXBridge.LogSceneInfo(gameType, currentScene, key, value);
            }
        }

        public void LogSceneInfo(string key, int value)
        {
            if (coroutine != null)
            {
                GPMCXXBridge.LogSceneInfo(gameType, currentScene, key, value);
            }
        }

        public void LogSceneInfo(string key)
        {
            if (coroutine != null)
            {
                GPMCXXBridge.LogSceneInfo(gameType, currentScene, key);
            }
        }
    }
}
