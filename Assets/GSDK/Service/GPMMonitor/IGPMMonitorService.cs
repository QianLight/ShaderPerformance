namespace GSDK
{
    /// <summary>
    /// 用于获取实例进行接口调用
    /// e.g.GPMMonitor.Service.MethodName();
    /// </summary>
    public static class GPMMonitor
    {
        public static IGPMMonitorService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.GPMMonitor) as IGPMMonitorService; }
        }
    }

    /// <summary>
    /// GPM性能监控提供两大能力:1.基于场景的性能监控。2.云端画质分档。
    /// 接入后可以采集玩家真实的性能数据，实时监控游戏线上出现的卡顿、内存过高等性能问题，
    /// 并提供场景、版本、机型等多维度分析，帮助项目快速发现和定位问题。
    ///
    /// 1.基于场景的性能监控
    /// 接入前需要联系GSDK同学在后台配置开启GPM性能监控，之后接入方可以在GPM平台-性能检测下看见相关数据。
    /// GPM平台url:https://gpm.bytedance.com/
    ///
    /// 2.云端画质分档
    /// 将画质分档信息配置在云端，根据设备硬件信息返回对应的画质分档。接入GPM以后，可以在机型天梯页面查看分档的详细信息。
    /// </summary>
    public interface IGPMMonitorService : IService
    {
        #region Methods

        /// <summary>
        /// 场景开始，调用该接口时会开始性能数据的采集。
        /// 如果调用该接口时，上一次场景未结束，则会自动结束上一次场景。
        /// </summary>
        /// <param name="sceneName">场景名称(支持中文，UTF-8)</param>
        void OnSceneStart(string sceneName);

        /// <summary>
        /// 场景加载完成。
        /// 场景开始到场景加载完成的时间，为场景加载耗时。
        /// 若未调用该接口，场景加载耗时为0;若多次调用该接口，则以最后一次调用为准。
        /// </summary>
        void OnSceneLoadFinish();

        /// <summary>
        /// 场景结束并将数据保存。
        /// 场景结束时会结束性能数据的采集，并在下次定时上报数据时上报采集到的数据。
        /// 如果游戏退出时没有结束场景，采集的数据将在下一次使用GPMMonitor.Service获取GPMMonitorService时保存，等到下一次定时上报时再上报。
        /// </summary>
        /// <param name="isUpload">场景信息是否上报</param>
        void OnSceneEnd(bool isUpload = true);

        /// <summary>
        /// 增加场景信息，只能在场景开始后设置，只针对该场景生效
        /// </summary>
        /// <param name="key">信息的key</param>
        /// <param name="value">信息的value</param>
        void AddSceneInfo(string key, string value);

        /// <summary>
        /// 增加场景信息，只能在场景开始后设置，只针对该场景生效
        /// </summary>
        /// <param name="key">信息的key</param>
        /// <param name="value">信息的value</param>
        void AddSceneInfo(string key, int value);

        /// <summary>
        /// 增加全局信息，对所有场景生效
        /// </summary>
        /// <param name="key">信息的key</param>
        /// <param name="value">信息的value</param>
        void AddGlobalInfo(string key, string value);

        /// <summary>
        /// 增加全局信息，对所有场景生效
        /// </summary>
        /// <param name="key">信息的key</param>
        /// <param name="value">信息的value</param>
        void AddGlobalInfo(string key, int value);
        
        /// <summary>
        /// 获取用户设备对应的云端画质分档。
        /// </summary>
        /// <returns>云端画质分档</returns>
        int GetGraphicLevel();

        #endregion
    }
}