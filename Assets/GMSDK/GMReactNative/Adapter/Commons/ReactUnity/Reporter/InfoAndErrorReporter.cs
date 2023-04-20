/*
 * @author yankang.nj
 * 引擎内部埋点上报， 参考https://bytedance.feishu.cn/sheets/shtcn2dz1kisF4AUPWFv3vm3bVa?sheet=0170df  先上报1.0 已经存在的埋点
 *
 * 异常埋点上报，包括JS测异常和Csharp 异常
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace GSDK.RNU
{
    public class InfoAndErrorReporter
    {
        public static string OpenURLUnkown = "9999";
        public static string OpenURLNOSourceFile = "1001";
        public static string OpenURLABFileError = "1010";
        public static string JSEvalError = "2001";
        public static string OpenURLSourceFileFormatError = "1011";
        public static string OpenURLSourceFileLoadError = "1002";
        public static string RNUInnerError = "1005";

        private static string OpenURLEvent = "mgame_jw_ru_open_url";
        private static string SourceFileExistEvent = "mgame_jw_ru_local_exist";
        private static string StartLoadEvent = "mgame_jw_ru_start_loading";
        private static string LoadFailedEvent = "mgame_jw_ru_load_failed";
        private static string LoadFinishEvent = "mgame_jw_ru_loaded_finish";
        private static string TTIEvent = "mgame_jw_ru_time_to_interact";
        private static string ABFileLoadEvent = "mgame_jw_ru_asset_bundle_info";
        private static string TrackBatchEvent = "mgame_jw_ru_batch_info";

        private static string ExceptionName = "ReactUnityError";
        private static string AppLogName = "rn_react_unity_log";


        private static Dictionary<string, object> monitorTmpMap = new Dictionary<string, object>();
        private static Dictionary<string, object> trackTmpMap = new Dictionary<string, object>();
        
        private static bool isBatchReport = false;

        // 合并上报的开始
        public static void StartReport()
        {
            trackTmpMap = new Dictionary<string, object>();
            monitorTmpMap = new Dictionary<string, object>();
            isBatchReport = true;
        }

        // 统一上报的开始
        public static void FlushReport()
        {
            isBatchReport = false;
            RawReporter.TrackEvent(TrackBatchEvent, trackTmpMap);
            trackTmpMap = null;
            
            RawReporter.MonitorEvent(AppLogName, monitorTmpMap, new Dictionary<string, object>(),
                new Dictionary<string, object>());
            monitorTmpMap = null;
        } 
        
        // OpenURL（引擎调用业务包）
        public static void ReportOpenURL(bool status, string error_code, string url)
        {
            Dictionary<string, object> reportData =
                new Dictionary<string, object>()
                {
                    {"status", status ? "1" : "0"},
                    {"error_code", error_code},
                    {"error_msg", ""},
                    {"scene", "0"},
                    {"url", url},
                    {"isReactUnity", "1"},
                };
            
            if (isBatchReport)
            {
                trackTmpMap.Add(OpenURLEvent, reportData);
            }
            else
            {
                RawReporter.TrackEvent(OpenURLEvent, reportData);
            }
        }

        // 业务加载时做出的文件判断
        public static void ReportSourceFileExist(bool status, string url)
        {
            Dictionary<string, object> reportData =
                new Dictionary<string, object>()
                {
                    {"status", status ? "1" : "0"},
                    {"scene", "0"},
                    {"url", url},
                    {"isReactUnity", "1"},
                };

            if (isBatchReport)
            {
                trackTmpMap.Add(SourceFileExistEvent, reportData);
            }
            else
            {
                RawReporter.TrackEvent(SourceFileExistEvent, reportData);
            }
        }

        // 业务加载AB文件判断
        public static void ReportAssetBundleFileLoadInfo(bool status, string url)
        {
            
            Dictionary<string, object> reportData =
                new Dictionary<string, object>()
                {
                    {"status", status ? "1" : "0"},
                    {"scene", "0"},
                    {"url", url},
                    {"isReactUnity", "1"},
                };

            if (isBatchReport)
            {
                trackTmpMap.Add(ABFileLoadEvent, reportData);
            }
            else
            {
                RawReporter.TrackEvent(ABFileLoadEvent, reportData);
            }
        }

        public static void ReportSourceFileStartLoad(long duration, string url)
        {
            Dictionary<string, object> reportData =
                new Dictionary<string, object>()
                {
                    {"duration", duration},
                    {"scene", "0"},
                    {"url", url},
                    {"window_id", "0"},
                    {"isReactUnity", "1"},
                };

            if (isBatchReport)
            {
                trackTmpMap.Add(StartLoadEvent, reportData);
            }
            else
            {
                RawReporter.TrackEvent(StartLoadEvent, reportData);
            }
        }

        public static void ReportSourceFileLoadFinish(long duration, string url)
        {
            Dictionary<string, object> reportData =
                new Dictionary<string, object>()
                {
                    {"duration", duration},
                    {"scene", "0"},
                    {"url", url},
                    {"window_id", "0"},
                    {"isReactUnity", "1"},
                };

            if (isBatchReport)
            {
                trackTmpMap.Add(LoadFinishEvent, reportData);
            }
            else
            {
                RawReporter.TrackEvent(LoadFinishEvent, reportData);
            }
        }

        public static void ReportSourceFileLoadFailure(string error_code, string url)
        {
            Dictionary<string, object> reportData =
                new Dictionary<string, object>()
                {
                    {"error_code", error_code},
                    {"error_msg", ""},
                    {"scene", "0"},
                    {"url", url},
                    {"window_id", "0"},
                    {"isReactUnity", "1"},
                };

            if (isBatchReport)
            {
                trackTmpMap.Add(LoadFailedEvent, reportData);
            }
            else
            {
                RawReporter.TrackEvent(LoadFailedEvent, reportData);
            }
        }


        // TTI 时间
        public static void ReportTTI(long duration, string url)
        {
            Dictionary<string, object> reportData =
                new Dictionary<string, object>()
                {
                    {"duration", duration},
                    {"scene", "0"},
                    {"url", url},
                    {"window_id", "0"},
                    {"isReactUnity", "1"},
                };

            if (isBatchReport)
            {
                trackTmpMap.Add(TTIEvent, reportData);
            }
            else
            {
                RawReporter.TrackEvent(TTIEvent, reportData);
            }
        }

        public static void ReportNativeModuleInvokeException(int moduleId, int methodId, Exception e)
        {
            RawReporter.ReportException(ExceptionName,
                "NativeModuleCallError moduleId:" + moduleId + " methodId:" + methodId, e.StackTrace);
        }

        public static void ReportJSException(string title, string stack)
        {
            RawReporter.ReportException(ExceptionName, "JSCallError: " + title, stack);
        }

        public static void ReportException(Exception e)
        {
            RawReporter.ReportException(ExceptionName, e.Message, e.StackTrace);
        }

        public static void ReportException(string memssage, Exception e)
        {
            RawReporter.ReportException(ExceptionName, memssage, e.StackTrace);
        }
        public static void RuLog(string message)
        {
            if (isBatchReport)
            {
                monitorTmpMap.Add("key_" + monitorTmpMap.Count , message);
            }
            else
            {
                RawReporter.MonitorEvent(AppLogName, new Dictionary<string, object>()
                {
                    {"message", message}
                }, new Dictionary<string, object>(), new Dictionary<string, object>());
            }
        }

        public static void ReportMessageToUnity(string errorCode, string gameId, string sceneType)
        {
            Dictionary<string, object> p = new Dictionary<string, object>()
            {
                {"action", "error_message"},
                {
                    "params", new Dictionary<string, object>()
                    {
                        {"error_code", "-" + errorCode}, // 添加- 为了遵循1.0标准
                        {"ingame_id", gameId},
                        {"error_msg", ""},
                        {"scene_type", sceneType},
                        { "type", 2} // 0 rn 1 web 2 ru
                    }
                },
                {"message", ""}
            };

            RawReporter.ReportMessageToUnity(p);
        }
    }
}