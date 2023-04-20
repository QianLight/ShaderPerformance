using System.Collections.Generic;
using System.Runtime.InteropServices;
using GMSDK;
using UNBridgeLib;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class MonitorService : IMonitorService
    {
        private readonly MainSDK _mainSDK;
        public event SystemLogUploadEventHandler SystemLogUploadEvent;

        public MonitorService()
        {
            _mainSDK = GMSDKMgr.instance.SDK;
        }

        public void ReportException(ExceptionInfo exceptionInfo)
        {
            JsonData filter = null;
            if (!string.IsNullOrEmpty(exceptionInfo.FilterJson))
            {
                filter = JsonMapper.ToObject(exceptionInfo.FilterJson);
            }

            JsonData extraParams = null;
            if (!string.IsNullOrEmpty(exceptionInfo.ExtraParamsJson))
            {
                extraParams = JsonMapper.ToObject(exceptionInfo.ExtraParamsJson);
            }
            Loom.QueueOnMainThread(() =>
            {
                GSDKProfilerTools.BeginSample("Monitor ReportException");
                _mainSDK.SdkLogException(exceptionInfo.Name, exceptionInfo.Reason,
                    exceptionInfo.Stack, filter, extraParams);
                GSDKProfilerTools.EndSample();
            });
        }

#if UNITY_IOS
        public void ReportExceptionByDelay(ExceptionInfo exceptionInfo)
        {
            GSDKProfilerTools.BeginSample("Monitor ReportExceptionByDelay");
            _mainSDK.SdkLogExceptionSyn(exceptionInfo.Name, exceptionInfo.Reason,
                exceptionInfo.Stack);
            GSDKProfilerTools.EndSample();
        }
#endif

        public void AddTags(Dictionary<string, string> tagMap)
        {
            GSDKProfilerTools.BeginSample("Monitor AddTags");
            _mainSDK.SdkAddTags(tagMap);
            GSDKProfilerTools.EndSample();
        }

        public void AddTagsByJson(string tagMapJsonStr)
        {
            GSDKProfilerTools.BeginSample("Monitor AddTagsByJson");
            _mainSDK.SdkAddTagsByJson(tagMapJsonStr);
            GSDKProfilerTools.EndSample();
        }

        public void AddCustomInfo(Dictionary<string, string> customInfo)
        {
            GSDKProfilerTools.BeginSample("Monitor AddCustomInfo");
            _mainSDK.SdkAddCustomInfo(customInfo);
            GSDKProfilerTools.EndSample();
        }
        
        public void AddCustomInfoByJson(string customInfoJsonStr)
        {
            GSDKProfilerTools.BeginSample("Monitor AddCustomInfoByJson");
            _mainSDK.SdkAddCustomInfoByJson(customInfoJsonStr);
            GSDKProfilerTools.EndSample();
        }

        public void Log(MonitorLogLevel level, string content)
        {
            GSDKProfilerTools.BeginSample("Monitor Log");
#if UNITY_STANDALONE_WIN && !GMEnderOn
            ReportLog((int)level,content);
#else            
            _mainSDK.SdkLog((GMDevLogLevel) level, content);
#endif
            GSDKProfilerTools.EndSample();
        }

        public void LogEvent(string eventName, string metric, string category, string extraValue)
        {
            GSDKProfilerTools.BeginSample("Monitor LogEvent");
#if UNITY_STANDALONE_WIN && !GMEnderOn
            ReportLogEvent(eventName, metric, category, extraValue);
#endif
            var jsonMetric = metric == null ? null : JsonMapper.ToObject(metric);
            var jsonCategory = category == null ? null : JsonMapper.ToObject(category);
            _mainSDK.SdkLogEvent(eventName, jsonMetric, jsonCategory, extraValue);
            GSDKProfilerTools.EndSample();
        }

        public void UploadFile(UploadFileInfo uploadFileInfo, Dictionary<string, string> extraInfo,
            UploadFileDelegate callback)
        {
            var json = new JsonData();
            foreach (var item in extraInfo)
            {
                json[item.Key] = item.Value;
            }

            _mainSDK.SdkUploadFile(uploadFileInfo.Path, uploadFileInfo.Type, uploadFileInfo.Scene, json,
                callbackResult =>
                {
                    var result = InnerTools.ConvertToResult(callbackResult);
                    callback.Invoke(result);
                });
        }

        public void ReportPatchVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
            {
                GLog.LogError("version is null at SetPatchVersion.");
                return;
            }

            _mainSDK.SetGMPatchVersion(version);
        }
        
#if UNITY_IOS
        public void EnableSystemLogUpload(bool enable)
        {
            _mainSDK.SdkEnableSystemLogUpload(enable);

            if (enable)
            {
                _mainSDK.ListenSystemLogEvent((Dictionary<string, string> info) =>
                {
                    InnerTools.SafeInvoke(() =>
                    {
                        if (SystemLogUploadEvent != null)
                        {
                            SystemLogUploadEvent.Invoke(info);
                        }
                    });
                });
            }
            else
            {
                _mainSDK.unListenSystemLogEvent();
            }
        }

        public void OutputSystemLog(List<string> contents)
        {
            _mainSDK.OutputSystemLogUpload(contents);
        }
#endif

#if UNITY_ANDROID
        public void StartTrace()
        {
            _mainSDK.SDKStartTrace();
        }

        public void StopTrace(string launchTag, long maxTimeMs, string endActivityName)
        {
            _mainSDK.SDKEndTrace(launchTag, endActivityName, maxTimeMs);
        }

        public void CancelTrace()
        {
            _mainSDK.SDKCancelTrace();
        }

        public void ReportThreadCount(string scene)
        {
            _mainSDK.SDKReportTrace(scene);
        }

        public void StartSpan(string moduleName, string taskName)
        {
            _mainSDK.SDKStartSpan(moduleName, taskName);
        }

        public void StopSpan(string moduleName, string taskName)
        {
            _mainSDK.SDKEndSpan(moduleName, taskName);
        }

        public void StartReportCurrent(string scene)
        {
            _mainSDK.SDKStartCollectCurrent(scene);
        }

        public void StopReportCurrent(string scene)
        {
            _mainSDK.SDKStopCollectCurrent(scene);
        }
#endif
#if UNITY_STANDALONE_WIN && !GMEnderOn        
        [DllImport(PluginName.GSDK)]
        private static extern void ReportLog(
            int level,
            string content
        );

        [DllImport(PluginName.GSDK)]
        private static extern void ReportLogEvent(
            string eventName, 
            string metric, 
            string category, 
            string extraValue
        );
#endif		
    }
}