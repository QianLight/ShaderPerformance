/*
 * @author yankang.nj
 * 提供给JS测的API， 通常不知道位置的API，都放在这里
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Random = System.Random;

namespace GSDK.RNU
{
    public partial class Common : SimpleUnityModule
    {
        public static string NAME = "Common";


        public override string GetName()
        {
            return NAME;
        }
        

        public override Hashtable GetConstants()
        {
            var dic = GameInteraction.GetGameDataDic();
            Hashtable constants = new Hashtable(dic) 
            {
                {"RUVersion", "v0.9.0"}
            };
            return constants;
        }


        [ReactMethod]
        public void close() 
        {
            Util.Log("js call csharp to close");
            RNUMainCore.SetCloseFlag();
        }


        // Unity get请求
        [ReactMethod(true)]
        public void getRequestByUnity(string url, Promise promise)
        {
            StaticCommonScript.StaticStartCoroutine(getRequestInner(url, promise));
        }


        private IEnumerator getRequestInner(string url, Promise promise)
        {
            Util.Log("getRequestInner {0}", url);
            UnityWebRequest versionRequest = UnityWebRequest.Get(url);
            yield return versionRequest.SendWebRequest();

            string version = versionRequest.downloadHandler.text;
            promise.Resolve(version);
        }


        // 等待n秒
        [ReactMethod(true)]
        public void resolveAfterSeconds(int secs, Promise promise)
        {
            StaticCommonScript.StaticStartCoroutine(Example1Coroutine(secs, promise));
        }

        private IEnumerator Example1Coroutine(int secs, Promise promise)
        {
            Debug.Log("Started Coroutine at timestamp : " + Time.time);

            //yield on a new YieldInstruction that waits for 5 seconds.
            yield return new WaitForSeconds(secs);
            
            Debug.Log("Finished Coroutine at timestamp : " + Time.time);
            
            promise.Resolve("WaitForSeconds 2 SUC");
        }


        [ReactMethod(true)]
        public void trackEvent(string eventName, Dictionary<string, object> paramz, Promise promise)
        {
            RawReporter.TrackEvent(eventName, paramz);
            promise.Resolve(true);
        }


        [ReactMethod(true)]
        public void monitorEventRaw(string eventName, Dictionary<string, object> category, Dictionary<string, object> metric, Dictionary<string, object> extra, Promise promise)
        {
            RawReporter.MonitorEvent(eventName, category, metric, extra);
            promise.Resolve(true);
        }


        [ReactMethod]
        public void setGameParentFullScreen()
        {
            Util.Log("js call csharp to setGameParentFullScreen");
            RNUMainCore.SetGameGoParentFullScreen();
        }


        // js 获取 gameData
        [ReactMethod(true)]
        public void getGameDataDic(Promise promise)
        {
            Util.Log("js call csharp to getGameDataDic");
            Dictionary<string, object> dic = GameInteraction.GetGameDataDic();
            promise.Resolve(dic);
        }


        // js 获取所有字体
        [ReactMethod(true)]
        public void getFontDataDic(Promise promise)
        {
            Util.Log("js call csharp to getFontDataDic");
            ArrayList list = GameInteraction.GetGameFontDic();
            promise.Resolve(list);
        }


        private string ScreenNameGenerate(string type = "jpeg")
        {

            long cur = DateTime.Now.Ticks / 1000;
            Random random = new Random((int)cur);
            int num = random.Next(100000, 999999);

            return cur.ToString() + "_" + num.ToString() + "." + type;
        }


        // js 截屏
        [ReactMethod(true)]
        public void captureScreenshot(bool isSaveToAlbum, Promise promise)
        {
            string filePath = ScreenNameGenerate();
            Util.Log("js call csharp to captureScreenshot : " + filePath);

#if(UNITY_2017_1_OR_NEWER)
            ScreenCapture.CaptureScreenshot(filePath);
#else
            Application.CaptureScreenshot(filePath);
#endif
            string resPath = Application.persistentDataPath + "/" + filePath;
            if (isSaveToAlbum)
            {
                Util.Log("save to Album is not support yet!");
            }

            promise.Resolve(resPath);
        }
        
        // js 截屏 指定区域
        [ReactMethod(true)]
        public void captureScreenshotInner(int x, int y, int width, int height, bool mipmap, Promise promise)
        {
            StaticCommonScript.StaticStartCoroutine(CaptureFromScreenInner(x, y, width, height, mipmap, promise));
        }
        
        private IEnumerator CaptureFromScreenInner(int x, int y, int width, int height, bool mipmap, Promise promise)
        {
            WaitForEndOfFrame frameEnd = new WaitForEndOfFrame();
            yield return frameEnd;
            var rect = new Rect(x, y, width, height);
            Texture2D screenShot = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, mipmap);  
            screenShot.ReadPixels(rect, 0, 0);  
            screenShot.Apply();  
            byte[] bytes = screenShot.EncodeToPNG();

            string filePath = "/" + ScreenNameGenerate("png");
            string filename = Application.persistentDataPath + filePath;
            File.WriteAllBytes(filename, bytes);
            promise.Resolve(filename);
        }


        // js 获取路径
        [ReactMethod(true)]
        public void getPersistentDatapath(int pathType, Promise promise)
        {
            Util.Log("js call csharp to getPersistentDatapath pathType is: " + pathType);
            string path = string.Empty;
            if (pathType == 1)
            {
                path = Application.dataPath;
            }
            else if (pathType == 2)
            {
                path = Application.streamingAssetsPath;
            }
            else if (pathType == 3)
            {
                path = Application.persistentDataPath;
            }
            else if (pathType == 4)
            {
                path = Application.temporaryCachePath;
            }
            promise.Resolve(path);
        }


        // js 获取路径下所有文件
        [ReactMethod(true)]
        public void getDirectoryFileList(String path, Promise promise)
        {
            Util.Log("js call csharp to getDirectoryFileList");
            DirectoryInfo directoryInfo = new DirectoryInfo(path);
            FileInfo[] files = directoryInfo.GetFiles("*", SearchOption.TopDirectoryOnly);
            ArrayList list = new ArrayList();
            for (int i = 0; i < files.Length; ++i)
            {
                list.Add(files[i].FullName);
            }
            promise.Resolve(list);
        }


        // js 删除给定文件
        [ReactMethod(true)]
        public void deleteFile(String filePath, Promise promise)
        {
            Util.Log("js call csharp to deleteFile");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                promise.Resolve(true);
            }
            else
            {
                promise.Resolve(false);
            }
        }


        // 模拟一个Unity Event 方便测试链路
        [ReactMethod]
        public void mockUnityEvent()
        {
            RNUMainCore.SendUnityEventToJs("mockUnityEvent", "time: " + Time.time);
        }


        [ReactMethod]
        public void copyToClipboard(string txt)
        {
            GUIUtility.systemCopyBuffer = txt;
        }


        [ReactMethod]
        public void openLog(bool open)
        {
            Util.logFlag = open;
            Util.Log("log " + (open ? " open !" : " close !"));
        }

        [ReactMethod]
        public void playGameAudio(string id)
        {
            RNUMainCore.PlayGameAudio(id);
        }
        
        [ReactMethod]
        public void pauseGameAudio(string id)
        {
            RNUMainCore.PauseGameAudio(id);
        }

        [ReactMethod]
        public void openGameUI(string id, Dictionary<string, object> p)
        {
            RNUMainCore.OpenGameUI(id, p);
        }


        
        [ReactMethod]
        public void setRUCanvasSortOrder(int value)
        {
            RNUMainCore.SetRUCanvasSortOrder(value);
        }
        
        
        [ReactMethod]
        public void setGumihoPanelActive(bool isActive)
        {
            RNUMainCore.SetGumihoPanelActive(isActive);
        }
        
        [ReactMethod]
        public void setRUCanvasActive(bool isActive)
        {
            RNUMainCore.SetRUCanvasActive(isActive);
        }
        
        public static string HashtableToJson(Hashtable hr, int readcount = 0)
        {
            string json = "{";
            foreach (DictionaryEntry row in hr)
            {
                try
                {
                    string key = "\"" + row.Key + "\":";
                    if (row.Value is Hashtable)
                    {
                        Hashtable t = (Hashtable)row.Value;
                        if (t.Count > 0)
                        {
                            json += key + HashtableToJson(t, readcount++) + ",";
                        }
                        else { json += key + "{},"; }
                    }
                    else
                    {
                        string value = "\"" + row.Value.ToString() + "\",";
                        json += key + value;
                    }
                }
                catch { }
            }

            json = json + "}";
            return json;
        }



    }
}