/*
 * @Author: hexiaonuo
 * @Date: 2021-10-15
 * @Description: Game init, for ReactUnity action
 * @FilePath: ReactUnity/UnityModule/Core/RNUMainInit.cs
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using GSDK.RNU;
using UnityEngine;

using UnityEngine.UI;
using Object = UnityEngine.Object;


namespace GSDK.RNU
{
    public partial class RNUMainCore
    {
        private static bool hasInit = false;
        // 系统级别的默认参数
        private static Dictionary<string, object> _initParams = new Dictionary<string, object>();


        private ModuleManager moduleManager;

        private GameObject pageParentGo;
        private GameObject gumihoRootGo;

        private bool isGameParentGo;
        // This scaling affects everything under the Canvas, including font sizes and image borders
        private float canvasScaleFactor = 1f;
        private Action closeCallBack;

        private static RNUMainCore mainCoreInstance;

        // 是否关闭面板的标识
        private bool closeFlag = false;

        //TODO 改为main 或者其他
        private string mainUIName = "YkApp";

        private RNUMainCore()
        {

        }

        public static void Init()
        {
            if (hasInit)
            {
                Util.LogAndReport("RNU has inited!");
                return;
            }
            InitInner();
        }


        public static void Init(MonoBehaviour context)
        {
            if (hasInit)
            {
                Util.LogAndReport("RNU has inited!");
                return;
            }

            InitInner();
        }

        public static void Init(Dictionary<string, object> initParams)
        {
            // 即使已经初始化，仍然可以设置initParams
            if (initParams != null)
            {
                _initParams = initParams;
            }

            if (hasInit)
            {
                Util.LogAndReport("RNU has inited!");
                return;
            }

            InitInner();
        }


        private static void InitInner()
        {
            try
            {
                // 相关C 方法注入
                RNUNative.Init();

                // 预热相关方法, 先阶段只预热yoga
                YogaHelper.PreHotCode();

                hasInit = true;
            }
            catch (Exception e)
            {
                InfoAndErrorReporter.RuLog("Init Error: " + e.StackTrace);
            }
        }


        public static void OpenPage(string url, GameObject parentGo)
        {
            OpenPage(url, parentGo, null);
        }

        public static void OpenPage(string url, GameObject parentGo, Action closeCallBack)
        {
            //TODO 暂时只支持同时打开一个RU实例
            if (mainCoreInstance != null)
            {
                Util.LogAndReport("one page has opened!, could not open page {0}", url);
                return;
            }
            try
            {
                InfoAndErrorReporter.StartReport();
                long start = DateTime.Now.Ticks;
                BundleInfo bi = Util.ParseUrlToBundleInfo(url);

                mainCoreInstance = new RNUMainCore();
                if (null != closeCallBack)
                {
                    mainCoreInstance.closeCallBack = closeCallBack;
                }
                mainCoreInstance.OpenPageByBundle(bi, parentGo);

                long end = DateTime.Now.Ticks;
                InfoAndErrorReporter.ReportOpenURL(true, null, url);
                InfoAndErrorReporter.ReportTTI((end - start) / 10000, url);
                Util.LogAndReport("RNU: OpenPage duration  {0}", end - start);
            }
            catch (AssetBundleLoadException e)
            {
                Util.LogAndReport("AssetBundleLoadException!");

                // 如果load Ab异常，重新触发一次gecko下载
                GMReactUnityMgr.instance.SDK.SyncGecko((result) => { });

                //InfoAndErrorReporter.ReportException("AssetBundleLoadException", e);
                InfoAndErrorReporter.ReportAssetBundleFileLoadInfo(false, url);
                InfoAndErrorReporter.ReportOpenURL(false, InfoAndErrorReporter.OpenURLABFileError, url);
                InfoAndErrorReporter.ReportMessageToUnity(InfoAndErrorReporter.OpenURLABFileError, "-1", "0");
                Close();
            }
            catch (CodeFileLoadException e)
            {
                Util.LogAndReport("CodeFileLoadException!");
                InfoAndErrorReporter.ReportException("CodeFileLoadException", e);
                InfoAndErrorReporter.ReportSourceFileExist(false, url);
                InfoAndErrorReporter.ReportSourceFileLoadFailure(InfoAndErrorReporter.OpenURLNOSourceFile, url);
                InfoAndErrorReporter.ReportOpenURL(false, InfoAndErrorReporter.OpenURLNOSourceFile, url);
                InfoAndErrorReporter.ReportMessageToUnity(InfoAndErrorReporter.OpenURLNOSourceFile, "-1", "0");
                Close();
            }
            catch (CodeFileFormatException e)
            {
                Util.LogAndReport("CodeFileFormatException!");
                InfoAndErrorReporter.ReportException("CodeFileFormatException", e);
                InfoAndErrorReporter.ReportSourceFileLoadFailure(InfoAndErrorReporter.OpenURLSourceFileFormatError, url);
                InfoAndErrorReporter.ReportOpenURL(false, InfoAndErrorReporter.OpenURLSourceFileFormatError, url);
                InfoAndErrorReporter.ReportMessageToUnity(InfoAndErrorReporter.OpenURLSourceFileFormatError, "-1", "0");
                Close();
            }
            catch (CodeFileEvalException e)
            {
                Util.LogAndReport("CodeFileEvalException!");
                InfoAndErrorReporter.ReportException("CodeFileEvalException", e);
                InfoAndErrorReporter.ReportSourceFileLoadFailure(InfoAndErrorReporter.OpenURLSourceFileLoadError, url);
                InfoAndErrorReporter.ReportOpenURL(false, InfoAndErrorReporter.OpenURLSourceFileLoadError, url);
                InfoAndErrorReporter.ReportMessageToUnity(InfoAndErrorReporter.OpenURLSourceFileLoadError, "-1", "0");
                Close();
            }
            catch (Exception e)
            {
                Util.LogAndReport("open page error: " + e.StackTrace);
                InfoAndErrorReporter.ReportException(e);
                InfoAndErrorReporter.ReportOpenURL(false, InfoAndErrorReporter.OpenURLUnkown, url);
                InfoAndErrorReporter.ReportMessageToUnity(InfoAndErrorReporter.OpenURLUnkown, "-1", "0");
                Close();
            }
            finally
            {
                InfoAndErrorReporter.FlushReport();
            }
        }

        private void OpenPageByBundle(BundleInfo bi, GameObject parentGo = null)
        {
            if (!hasInit)
            {
                InfoAndErrorReporter.RuLog("OpenPage without init!");
                InitInner();
            }

            InitPageParentGo(parentGo);

            Util.Log("OpenPageByBundle {0} {1} {2}", bi.ModuleName, bi.FilePathPrefix, bi.Paramz);
            long start = DateTime.Now.Ticks;
            //启动 JS引擎
            jsExecutor = new QuickJSExecutor();
            Util.LogAndReport("QuickJSExecutor init duration: {0}", (DateTime.Now.Ticks - start) / 10000);

            start = DateTime.Now.Ticks;
            //初始化RU模块等
            moduleManager = new ModuleManager(this);
            Util.LogAndReport("ModuleManager init duration: {0}", (DateTime.Now.Ticks - start) / 10000);

            start = DateTime.Now.Ticks;
            //TODO 用C lazy的方式注入模块
            Hashtable injectGlobalBridgeConfig = Help.GetInjectObj(moduleManager.GetAllModules());
            jsExecutor.SetGlobalVariable("__fbBatchedBridgeConfig", injectGlobalBridgeConfig);
            Util.LogAndReport("__fbBatchedBridgeConfig init duration: {0}", (DateTime.Now.Ticks - start) / 10000);

            // load AssetBundle
            string pathPrefix;
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                pathPrefix = "./Assets/RNUAssetBundles/osx";
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                pathPrefix = "./Assets/RNUAssetBundles/win";
            }
            else
            {
                pathPrefix = bi.FilePathPrefix;
            }
            var bundle = AssetBundle.LoadFromFile(Path.Combine(pathPrefix, "rnubundles"));
            if (bundle == null)
            {
                throw new AssetBundleLoadException();
            }
            assetBundle = bundle;
            InfoAndErrorReporter.ReportAssetBundleFileLoadInfo(true, bi.Url);

            start = DateTime.Now.Ticks;
            InfoAndErrorReporter.ReportSourceFileStartLoad(0, bi.Url);
            jsExecutor.LoadApplicationScript(Path.Combine(pathPrefix, "code"));
            long jsLoadDuration = (DateTime.Now.Ticks - start) / 10000;
            InfoAndErrorReporter.ReportSourceFileExist(true, bi.Url);
            InfoAndErrorReporter.ReportSourceFileLoadFinish(jsLoadDuration, bi.Url);

            Util.LogAndReport("LoadApplicationScript duration  {0} ", jsLoadDuration);

            start = DateTime.Now.Ticks;
            RunApplication(mainUIName, bi.Paramz);
            Util.LogAndReport("AppRegistry.runApplication duration  {0}", (DateTime.Now.Ticks - start) / 10000);
        }

        private void InitPageParentGo(GameObject go = null)
        {
            isGameParentGo = true;
            pageParentGo = go;
            if (pageParentGo == null)
            {
                Util.Log("openPage with default Game parentGo: ");
                pageParentGo = GameInteraction.GetGameGoParent();
            }

            if (pageParentGo == null)
            {
                Util.Log("openPage with default RU Canvas parentGo");
                isGameParentGo = false;

                // 初始化Unity Canvas
                pageParentGo = new GameObject("Canvas");
                var canvas = pageParentGo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                pageParentGo.AddComponent<GraphicRaycaster>();
            }

            // 挂载通用脚本
            StaticCommonScript scc = pageParentGo.AddComponent<StaticCommonScript>();
            StaticCommonScript.Init(scc);

            // 挂载监听脚本
            pageParentGo.AddComponent<WatchParentGo>();

            // 记录父Canvas
            var parentCanvas = pageParentGo.GetComponentInParent<Canvas>();
            if (parentCanvas == null)
            {
                Util.LogAndReport("parent gameObject must under some canvas!");
            }
            else
            {
                canvasScaleFactor = parentCanvas.scaleFactor;
            }
        }

        public void RunApplication(string rootName, string pageParams)
        {
            var uiManagerModule = moduleManager.GetUiManagerModule();
            int rootTag = uiManagerModule.GetRootPanelTag(rootName);
            var appParams = new ArrayList
            {
                rootName,
                new Hashtable
                {
                    {
                        "initialProps", new Hashtable(_initParams)
                        {
                            {"props", pageParams},
                        }
                    },
                    {"rootTag", rootTag}
                }
            };
            //TODO 减少字符串字面量的使用
            jsExecutor.CallFunction("AppRegistry", "runApplication", appParams);
        }

        // 获取面板父节点
        public static GameObject GetPageParentGo()
        {
            if (mainCoreInstance == null)
            {
                Util.LogAndReport("mainCoreInstance is null, GetPageParentGo failed, return null");
                return null;
            }
            return mainCoreInstance.pageParentGo;
        }


        public static void SetGumihoRootGo(GameObject go)
        {
            if (mainCoreInstance == null)
            {
                Util.LogAndReport("mainCoreInstance is null, return");
                return;
            }

            mainCoreInstance.gumihoRootGo = go;
        }

        public static bool IsGameparentGo()
        {
            if (mainCoreInstance == null)
            {
                Util.LogAndReport("mainCoreInstance is null, return false");
                return false;
            }

            return mainCoreInstance.isGameParentGo;
        }

        public static void SetRUCanvasSortOrder(int value)
        {
            if (!IsGameparentGo())
            {
                Canvas canvas = mainCoreInstance.pageParentGo.GetComponent<Canvas>();
                canvas.sortingOrder = value;
            }
            else
            {
                Util.Log("is gameParentGo, can not setCanvas sortOrder");
            }
        }

        public static void SetGumihoPanelActive(bool isActive)
        {
            mainCoreInstance.gumihoRootGo.SetActive(isActive);
        }


        public static void SetRUCanvasActive(bool isActive)
        {
            if (!IsGameparentGo())
            {
                mainCoreInstance.pageParentGo.SetActive(isActive);
            }
        }

        // 适配游戏节点的不同渲染模式伸缩大小
        // Yoga 需要获取游戏父节点当前大小来绘制九尾活动面板大小
        public static Rect GetGameGoParentRect()
        {
            Rect rectParam = Rect.zero;
            if (mainCoreInstance == null)
            {
                Util.LogAndReport("mainCoreInstance is null, GetGameGoParentRect failed, return zero");
                return rectParam;
            }
            if (mainCoreInstance.pageParentGo != null && mainCoreInstance.pageParentGo.transform.GetComponent<RectTransform>() != null)
            {
                rectParam = mainCoreInstance.pageParentGo.transform.GetComponent<RectTransform>().rect;
                Util.LogAndReport("RNUParentGo: {0}, width:{1}, height:{2}", mainCoreInstance.pageParentGo.name, rectParam.width, rectParam.height);
            }

            return rectParam;
        }

        public static float GetCanvasScaleFactor()
        {
            return mainCoreInstance.canvasScaleFactor;
        }

        // 设置游戏节点为全屏，接口提供给 js 活动侧使用
        public static void SetGameGoParentFullScreen()
        {
            if (mainCoreInstance == null)
            {
                Util.LogAndReport("mainCoreInstance is null, SetGameGoParentFullScreen failed, return");
                return;
            }
            if (!mainCoreInstance.isGameParentGo)
            {
                Util.Log("overLay canvas go, created by RU, no need to set full, return");
                return;
            }
            if (mainCoreInstance.pageParentGo == null)
            {
                Util.Log("RUNParentGo is null, can not set to full screen, return");
                return;
            }

            RectTransform rectInfo = mainCoreInstance.pageParentGo.GetComponent<RectTransform>();
            if (rectInfo == null)
            {
                Util.Log("RNUParentGo's RectTransform is null, add RectTransform Component...");
                mainCoreInstance.pageParentGo.AddComponent<RectTransform>();
                rectInfo = mainCoreInstance.pageParentGo.GetComponent<RectTransform>();
            }

            if (rectInfo == null)
            {
                Util.Log("rectTransform is null, and add failed, return");
                return;
            }

            /*
             * 设置游戏父节点的 RectTransform
             * 该节点锚点设置为横竖拉伸：anchorMin(0,0), anchorMax(1,1)
             * 该节点上下左右距离父节点距离为0，即完全重合父节点大小：anchoredPosition3D(0,0,0),sizeDelta(0,0)
             */
            rectInfo.anchorMin = new Vector2(0, 0);
            rectInfo.anchorMax = new Vector2(1, 1);
            rectInfo.pivot = new Vector2(0.5f, 0.5f);
            rectInfo.anchoredPosition3D = new Vector3(0, 0, 0);
            rectInfo.sizeDelta = new Vector2(0, 0);
        }

        public static void CloseIfNecessary()
        {
            if (mainCoreInstance != null && mainCoreInstance.closeFlag)
            {
                Close();
            }
        }

        public static void SetCloseFlag()
        {
            if (mainCoreInstance != null)
            {
                mainCoreInstance.closeFlag = true;
            }
        }


        public static void Close()
        {
            Util.LogAndReport("RNUMain close!");
            try
            {
                if (mainCoreInstance == null)
                {
                    Util.Log("mainCoreInstance is null, openPage first, return");
                    return;
                }

                mainCoreInstance.closeFlag = true; //解决拍脸跳转到其他界面，然后一键回主页的log上报问题

                // 清理所有Unity模块
                if (mainCoreInstance.moduleManager != null)
                {
                    mainCoreInstance.moduleManager.Destroy();
                }

                // 所有静态变量的清理等
                StaticCommonScript.Destroy();
                var scc = mainCoreInstance.pageParentGo.GetComponent<StaticCommonScript>();
                if (scc != null)
                {
                    Object.Destroy(scc);
                }

                var wpg = mainCoreInstance.pageParentGo.GetComponent<WatchParentGo>();
                if (wpg != null)
                {
                    Object.Destroy(wpg);
                }



                // 卸载 assetBundle
                if (mainCoreInstance.assetBundle != null)
                {
                    mainCoreInstance.assetBundle.Unload(true);
                }

                // JSExecutor
                if (mainCoreInstance.jsExecutor != null)
                {
                    mainCoreInstance.jsExecutor.Destroy();
                }

                // 销毁游戏父节点
                if (!mainCoreInstance.isGameParentGo)
                {
                    GameObject.Destroy(mainCoreInstance.pageParentGo);
                }

                if (mainCoreInstance.debugPanel != null)
                {
                    mainCoreInstance.debugPanel.Destory();
                }
            }
            catch (Exception e)
            {
                Util.LogAndReport("RNUMain close error!" + e.Message);
            }
            finally
            {
                if (null != mainCoreInstance && null != mainCoreInstance.closeCallBack)
                {
                    var cb = mainCoreInstance.closeCallBack;
                    mainCoreInstance = null;
                    cb();
                }
                mainCoreInstance = null;
            }
        }
    }
}
