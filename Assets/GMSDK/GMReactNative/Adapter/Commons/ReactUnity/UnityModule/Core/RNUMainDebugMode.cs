using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace GSDK.RNU
{
    internal class DebugUrlInfo
    {
        public string debugIp;
        public int debugPort;
        public Dictionary<string, string> paramsDic;
    }
    
    public partial class RNUMainCore
    {
        // debug 模式下的调试面板
        public DebugPanel debugPanel;
        // debug 地址。 默认127.0.0.1:8081
        private string debugIp;
        private int debugPort;
        private string pageParams;
        
        
        public static void DebugPage(string ip, string port, GameObject parentGo = null)
        {
            //TODO 暂时只支持同时打开一个RU实例
            if (mainCoreInstance != null)
            {
                Util.LogAndReport("one page has opened!, could not open page");
                return;
            }

            mainCoreInstance = new RNUMainCore();
            if (string.IsNullOrEmpty(ip))
            {
                ip = "127.0.0.1";
            }

            int portInt;
            if (string.IsNullOrEmpty(port))
            {
                portInt = 8081;
            }
            else
            {
                portInt = Convert.ToInt32(port);
            }
            mainCoreInstance.DebugPageInner(ip, portInt, "{}", parentGo);
        }

        // 形如 http://127.0.0.1:8081?rctModule=rudemo&rctModuleName=rudemo&rctModuleParams=%7B%22page%22%3A%22TeamLiebian%22%7D";
        public static void DebugPage(string debugUrl, GameObject parentGo = null)
        {
            //TODO 暂时只支持同时打开一个RU实例
            if (mainCoreInstance != null)
            {
                Util.LogAndReport("one page has opened!, could not open page");
                return;
            }
            
            DebugUrlInfo info;
            try
            {
                info = ParseUrl(debugUrl);
            }
            catch (Exception e)
            {
                Util.LogError("Parse DebugUrl error");
                return;
            }
            
            mainCoreInstance = new RNUMainCore();
            
            mainCoreInstance.DebugPageInner(info.debugIp, info.debugPort, info.paramsDic["rctModuleParams"], parentGo);
        }

        private static DebugUrlInfo ParseUrl(string debugUrl)
        {
            Debug.Log("debugUrl:" + debugUrl);
            if (!debugUrl.StartsWith("http://"))
            {
                throw new Exception("debugUrl format error");
            }
            
            var info = new DebugUrlInfo();
            
            var quesIndex = debugUrl.IndexOf("?", StringComparison.Ordinal);
            Debug.Log("quesIndex:" + quesIndex);
            if (quesIndex == -1)
            {
                throw new Exception("debugUrl format error");
            }
            
            var debugHost = debugUrl.Substring(7, quesIndex - 7);
            Debug.Log("debugHost:" + debugHost);
            var ipAndPort = debugHost.Split(':');
            
            if (ipAndPort.Length != 2)
            {
                throw new Exception("debugUrl format error");
            }

            info.debugIp = ipAndPort[0];
            info.debugPort = Convert.ToInt32(ipAndPort[1]);
            
            var paramsStr = debugUrl.Substring(quesIndex + 1);
            var debugParams = paramsStr.Split('&');
            
            var debugParamsDictionary = new Dictionary<string, string>();
            foreach (var param in debugParams)
            {
                var kv = param.Split('=');
                if (kv.Length != 2)
                {
                    continue;
                }
                
                debugParamsDictionary.Add(kv[0], kv[1]);
            }
            info.paramsDic = debugParamsDictionary;
            return info;
        }
        
        private void DebugPageInner(string ip, int port, string pageParams, GameObject parentGo)
        {
            if (!hasInit)
            {
                Util.Log("OpenPage without init!");
                InitInner();
            }

            debugIp = ip;
            debugPort = port;
            this.pageParams = pageParams;
            InitPageParentGo(parentGo);
            
            // 初始化unity测调试面板
            debugPanel = new DebugPanel(this);
            
            // 加载baseAb， 包括border等能力
            StaticCommonScript.StaticStartCoroutine(LoadBaseAbFromServer());

            //启动 JS引擎 "ws://" + ip + ":" + port + "/debugger-proxy?role=client"
            jsExecutor = new WebSocketJSExecutor(ip, port, this);
            //初始化RU模块
            moduleManager = new ModuleManager(this);
            
            Hashtable injectGlobalBridgeConfig = Help.GetInjectObj(moduleManager.GetAllModules());
            jsExecutor.SetGlobalVariable("__fbBatchedBridgeConfig", injectGlobalBridgeConfig);
            jsExecutor.LoadApplicationScript("http://" + ip + ":" + port + "/index.bundle?platform=android&dev=true&minify=false&app=com.YkApp&modulesOnly=false&runModule=true");

            // 默认开启HotReload
            HMRClient.setup(ip, port, true);
            
            RunApplication(mainUIName, pageParams);
        }
        
        // 调试模式下， reload 触发
        public void RestartJs(bool enableHot)
        {
            // 清理上一次运行环境
            StaticCommonScript.Destroy();
            moduleManager.Destroy();
            jsExecutor.Destroy();
            
            // 重新设置运行环境
            var scc = pageParentGo.GetComponent<StaticCommonScript>();
            StaticCommonScript.Init(scc);

            if (assetBundle == null)
            {
                // 加载baseAb， 包括border等能力
                StaticCommonScript.StaticStartCoroutine(LoadBaseAbFromServer());
            }
            
            moduleManager = new ModuleManager(this);
            
            jsExecutor = new WebSocketJSExecutor(debugIp, debugPort, this);

            // 启动
            Hashtable injectGlobalBridgeConfig = Help.GetInjectObj(moduleManager.GetAllModules());
            jsExecutor.SetGlobalVariable("__fbBatchedBridgeConfig", injectGlobalBridgeConfig);
            jsExecutor.LoadApplicationScript("http://" + debugIp  + ":" + debugPort + "/index.bundle?platform=android&dev=true&minify=false&app=com.YkApp&modulesOnly=false&runModule=true");
            HMRClient.setup(debugIp, debugPort, enableHot);
            // pageParams TODO
            RunApplication(mainUIName, pageParams);
        }

        public bool IsDebugMode()
        {
            return debugPanel != null;
        }

        public string GetDebugIp()
        {
            return debugIp;
        }
        
        public int GetDebugPort()
        {
            return debugPort;
        }

        private IEnumerator LoadBaseAbFromServer()
        {
            var uv = Application.unityVersion;
            var platform = Application.platform;
            
            var baseAbUrl = "http://" + debugIp + ":" + debugPort + "/getRUBaseAb?platform=" + platform + "&" + "unityVersion=" + uv;
            var baseAbReq = UnityWebRequest.Get(baseAbUrl);
            yield return baseAbReq.SendWebRequest();

            if (baseAbReq.isHttpError || baseAbReq.error != null)
            {
                Util.Log("LoadBaseAb Error");
            }
            else
            {
                assetBundle = AssetBundle.LoadFromMemory(baseAbReq.downloadHandler.data);
            }
        }
        
    }
}