namespace GSDK
{
    /// <summary>
    /// 用于获取实例进行接口调用
    /// e.g.Report.Service.MethodName();
    /// </summary>
    public static class Report
    {
        public static IReportService Service
        {
            get
            {
                return ServiceProvider.Instance.GetService(ServiceType.Report) as IReportService;
            }
        }
    }
    
    /// <summary>
    /// 「数据相关」-上报事件统一入口
    /// </summary>
    public partial interface IReportService : IService
    {
        /// <summary>
        /// ET平台 - 上报埋点入口
        /// </summary>
        IETService ET
        {
            get;
        }

        /// <summary>
        /// GPM平台 - 上报端监控埋点入口
        /// </summary>
        IMonitorService Monitor
        {
            get;
        }
    }
}