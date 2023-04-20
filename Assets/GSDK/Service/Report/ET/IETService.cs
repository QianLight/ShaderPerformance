namespace GSDK
{
    /// <summary>
    /// ET平台接口，客户端上报埋点
    /// </summary>
    public interface IETService : IService
    {
        /// <summary>
        /// 在GSDK初始化成功之后，上报游戏启动行为，用于数据分析。例如主动启动、外链拉起等。
        /// </summary>
        void ReportLaunchEvent();

        /// <summary>
        /// 埋点上报
        ///
        /// 若对ReportLaunchEvent初始化上报埋点有补充，需传入事件名为"mgame_launch_log"
        /// </summary>
        /// <param name="eventName">事件名</param>
        /// <param name="jsonParams">上报参数，类型为json字符串</param>
        void ReportTrackEvent(string eventName, string jsonParams);
    }
}