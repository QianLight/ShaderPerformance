using System.Collections.Generic;

namespace GSDK
{
    public delegate void UploadFileDelegate(Result uploadFileCallback);
    
    /// <summary>
    /// 捕获到系统日志，埋点上报，会触发事件回调
    /// </summary>
    /// <param name="info"> 事件信息</param>
    public delegate void SystemLogUploadEventHandler(Dictionary<string, string> info);

    /// <summary>
    /// 端监控上报接口，所有内容目前可以通过对外开放的GPM平台查看：https://gpm.bytedance.com/applist
    /// </summary>
    public interface IMonitorService : IService
    {
        #region Exception

        /// <summary>
        /// 主动上报 C# LUA等混合语言的崩溃堆栈 需要自行完成调用栈的解析，这里仅供上传展示
        /// <param name="exceptionInfo">崩溃信息</param>
        /// </summary>
        void ReportException(ExceptionInfo exceptionInfo);

#if UNITY_IOS
        /// <summary>
        /// 主动上报 C# LUA等混合语言的崩溃堆栈 需要自行完成调用栈的解析，这里仅供上传展示，同步上传
        ///
        /// 注意：这里主要应对开启[Script Call Optimization = Fast but no Exception]的情景下AppDomain.CurrentDomain.UnhandledException中使用，
        /// 其他情况，请勿使用
        /// 说明：在Fast but no Exception模式下发生异常即崩溃，因此异步无法上报异常，需要用同步接口ReportExceptionSync上报
        /// </summary>
        /// <param name="exceptionInfo">崩溃信息</param>
        void ReportExceptionByDelay(ExceptionInfo exceptionInfo);
#endif

        /// <summary>
        /// 用于异常列表（仅限ANR和崩溃）的自定义过滤项设置
        /// 
        /// 请尽量在SDK初始化之前完成设置，否则启动阶段的上报可能无法携带
        /// </summary>
        /// <param name="tagMap">全局异常标识，key为自定义tag，value用于过滤</param>
        void AddTags(Dictionary<string, string> tagMap);
        
        /// <summary>
        /// 用于异常列表（仅限ANR和崩溃）的自定义过滤项设置，作用和AddTags相同
        ///
        /// 与AddTags的区别在于传入的是json字符串格式，如果传入非json字符串格式，接口无效
        /// json字符串的格式固定，key和value均为string类型，且不支持嵌套，例如"{\"data\":\"test_123\"}"
        /// 请尽量在SDK初始化之前完成设置，否则启动阶段的上报可能无法携带
        /// </summary>
        /// <param name="tagMap">全局异常标识，key为自定义tag，value用于过滤</param>
        void AddTagsByJson(string tagMapJsonStr);
        
        /// <summary>
        /// 用于异常列表的自定义信息展示
        ///
        /// 与上面tag的区别在于，这部分信息只能用于展示，不能用于过滤筛选
        /// 请尽量在SDK初始化之前完成设置，否则启动阶段的上报可能无法携带
        /// </summary>
        /// <param name="customInfo"></param>
        void AddCustomInfo(Dictionary<string, string> customInfo);

        /// <summary>
        /// 用于异常列表的自定义信息展示，作用和AddCustomInfo相同
        /// 
        /// 与AddCustomInfo的区别在于传入的是json字符串格式，如果传入非json字符串格式，接口无效
        /// json字符串的格式固定，key和value均为string类型，且不支持嵌套，例如"{\"data\":\"test_123\"}"
        /// 请尽量在SDK初始化之前完成设置，否则启动阶段的上报可能无法携带
        /// </summary>
        /// <param name="customInfoJsonStr">自定义信息,json字符串格式</param>
        void AddCustomInfoByJson(string customInfoJsonStr);

        #endregion

        #region Log

        /// <summary>
        /// 开发日志记录，请在sdk 初始化之后使用
        ///
        /// slardar平台查看的地址为 相应应用界面下的 单点追查->日志文件检索 输入did查看
        /// </summary>
        /// <param name="level">在slardar平台查看时，对应的slardar平台的level统一为warning, MonitorLogLevel会映射成相应的tag字符串，值请参照MonitorLogLevel的定义</param>
        /// <param name="content">日志内容，不可为空</param>
        void Log(MonitorLogLevel level, string content);

        /// <summary>
        /// 上报开发者定义的事件，可用于监控
        /// </summary>
        /// <param name="eventName">类型的名称，需要在slardar的【上报配置/事件埋点配置/allow_service_name】下添加采样，注意采样率的设置</param>
        /// <param name="metric">字典必须是key-value形式，不可嵌套，是数值类型的信息，对应 Slardar 的 metric</param>
        /// <param name="category">字典必须是key-value形式，不可嵌套，是维度信息，对应 Slardar 的 category</param>
        /// <param name="extraValue">额外信息，方便追查问题使用，Slardar 平台不会进行展示，hive 中可以查询</param>
        void LogEvent(string eventName, string metric, string category, string extraValue);

        #endregion

        #region Upload

        /// <summary>
        /// 上传日志文件
        /// </summary>
        /// <param name="uploadFileInfo">文件信息，包含路径，类型和上报场景</param>
        /// <param name="extraInfo">通用参数，前端可视化时候展示</param>
        /// <param name="callback">上报完成的回调，判断是否上传成功</param>
        void UploadFile(UploadFileInfo uploadFileInfo, Dictionary<string, string> extraInfo,
            UploadFileDelegate callback);

        #endregion

        /// <summary>
        /// 上报游戏的热更版本号，用于在监控后台进行查看，精确问题发生版本等。
        ///
        /// 注：首次调用设置后，需要杀进程下次启动后才会在请求中抓包看到，抓包请求里面看header->custom->gm_patch_version
        /// </summary>
        /// <param name="version">热更版本号，建议使用数字</param>
        void ReportPatchVersion(string version);

#if UNITY_IOS
        /// <summary>
        /// 开启系统日志重定向上报功能（仅限iOS）
        /// 
        /// </summary>
        /// <param name="enable">是否开启，true-开启，false-关闭</param>
        void EnableSystemLogUpload(bool enable);

        /// <summary>
        /// 输出指定的系统日志内容（仅限iOS）
        /// 
        /// </summary>
        /// <param name="contents">上报内容数组</param>
        void OutputSystemLog(List<string> contents);
        
        /// <summary> 捕获到系统日志并埋点上报，同时会触发该事件 </summary>
        event SystemLogUploadEventHandler SystemLogUploadEvent;
#endif

#if UNITY_ANDROID

        #region Trace

        /// <summary>
        /// 启动耗时统计
        /// 
        /// 一次应用启动应该只有一次调用
        /// StartTrace() 必须保证 和 EndTrace() 在同一线程，且配对出现
        /// </summary>
        void StartTrace();

        /// <summary>
        /// 结束耗时统计
        ///
        /// 一次启动过程只应该调用一次
        /// 和cancelTrace互斥，在cancelTrace之后调用endTrace无效
        /// </summary>
        /// <param name="launchTag">启动监控的Tag，业务可以自定义扩展</param>
        /// <param name="endActivityName">启动结束时的Activity名字，默认为UnityPlayerActivity</param>
        /// <param name="maxTimeMs"> 最大时长约束阈值(推荐2000ms),超过时长则取消上报</param>
        void StopTrace(string launchTag, long maxTimeMs, string endActivityName = "UnityPlayerActivity");

        /// <summary>
        /// 取消耗时统计
        /// </summary>
        void CancelTrace();

        /// <summary>
        /// 启动子任务的耗时统计
        ///
        /// 必须保证已调用 StartTrace()
        /// 
        /// 同一个module可以在不同的线程，但module的taskName需要在同一个线程
        /// StartTimingSubTrace 和 EndTimingSubTrace 配对使用
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <param name="taskName">任务名称</param>
        void StartSpan(string moduleName, string taskName);

        /// <summary>
        /// 停止指定module和taskName的子任务耗时统计
        /// 
        /// StartSpanOfTrace,必须保证和 EndSpanOfTrace 在同一个线程
        /// </summary>
        /// <param name="moduleName">模块名称</param>
        /// <param name="taskName">任务名称</param>
        void StopSpan(string moduleName, string taskName);

        #endregion

        /// <summary>
        /// 获取线程数量并上报（不主动上报，也会在启动20分钟后开始周期性监控）
        /// </summary>
        /// <param name="scene">场景值</param>
        void ReportThreadCount(string scene);

        #region Current

        /// <summary>
        /// 开始采集指定场景的电流值（不主动上报，也会定时采集）
        /// </summary>
        /// <param name="scene">场景值</param>
        void StartReportCurrent(string scene);

        /// <summary>
        /// 结束该场景电流值的采集
        /// </summary>
        /// <param name="scene">场景值</param>
        void StopReportCurrent(string scene);

        #endregion

#endif
    }

    /// <summary>
    /// 上传异常信息
    /// </summary>
    public class ExceptionInfo
    {
        /// <summary>
        /// 崩溃名称
        /// </summary>
        public string Name;

        /// <summary>
        /// 崩溃原因
        /// </summary>
        public string Reason;

        /// <summary>
        /// 崩溃堆栈
        /// </summary>
        public string Stack;

        /// <summary>
        /// 自定义过滤项（不可嵌套，均为字符串类型）
        /// </summary>
        public string FilterJson;

        /// <summary>
        /// 额外参数（不可嵌套，均为字符串类型）
        /// </summary>
        public string ExtraParamsJson;

        public ExceptionInfo(string name, string reason, string stack, string filterJson = null, string extraParamsJson =null)
        {
            Name = name;
            Reason = reason;
            Stack = stack;
            FilterJson = filterJson;
            ExtraParamsJson = extraParamsJson;
        }
    }

    public enum MonitorLogLevel
    {
        /// <summary>
        /// 日志平台映射tag字符串：cp_warn
        /// </summary>
        Warn,
        /// <summary>
        /// 日志平台映射tag字符串：cp_error
        /// </summary>
        Error,
        /// <summary>
        /// 日志平台映射tag字符串：cp_exception
        /// </summary>
        Exception,
        /// <summary>
        /// 日志平台映射tag字符串：cp_assert
        /// </summary>
        Assert,
        /// <summary>
        /// 日志平台映射tag字符串：cp_processInfo
        /// </summary>
        ProcessInfo
    }

    public class UploadFileInfo
    {
        /// <summary>
        /// 文件路径
        /// </summary>
        public string Path;

        /// <summary>
        /// 文件类型，可选
        /// </summary>
        public string Type;

        /// <summary>
        /// 上报时场景，可选
        /// </summary>
        public string Scene;

        public UploadFileInfo(string path, string type = null, string scene = null)
        {
            Path = path;
            Type = type;
            Scene = scene;
        }
    }

    public static partial class ErrorCode
    {
        public const int UploadFileError = 1;
    }
}