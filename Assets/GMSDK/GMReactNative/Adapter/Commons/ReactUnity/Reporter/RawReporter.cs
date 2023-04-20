using System;
using System.Collections.Generic;
using UNBridgeLib.LitJson;

namespace GSDK.RNU
{
    public static class RawReporter
    {

        public static void TrackEvent(string eventName, Dictionary<string, object> paramz)
        {

            long start = DateTime.Now.Ticks;
            Report.Service.ET.ReportTrackEvent(eventName,  JsonMapper.ToJson(paramz));
            long end = DateTime.Now.Ticks;
            Util.Log("TrackEvent duration  {0}", end - start);
        }

        public static void MonitorEvent(string eventName, Dictionary<string, object> category, Dictionary<string, object> metric, Dictionary<string, object> extra)
        {
            long start = DateTime.Now.Ticks;
            GMReactUnityMgr.instance.SDK.RUMonitorEvent(eventName, category, metric, extra, result =>
            {

            });
            long end = DateTime.Now.Ticks;
            Util.Log("MonitorEvent duration  {0}", end - start);
        }

        public static void ReportException(string name, string reason, string stack)
        {
            long start = DateTime.Now.Ticks;
            var info = new ExceptionInfo(name, reason, stack);
            Report.Service.Monitor.ReportException(info);
            long end = DateTime.Now.Ticks;
            Util.Log("MonitorEvent duration  {0}", end - start);
        }

        public static void ReportMessageToUnity(Dictionary<string, object> paramz)
        {
            GMReactUnityMgr.instance.SDK.RUSendMessageToUnity(JsonMapper.ToJson(paramz));
        }
    }
}
