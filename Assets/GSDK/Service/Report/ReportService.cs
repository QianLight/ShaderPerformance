namespace GSDK
{
    public partial class ReportService : IReportService
    {
        private readonly ETService _etService = new ETService();
        private readonly MonitorService _monitorService = new MonitorService();

        public IETService ET
        {
            get
            {
                return _etService;
            }
        }

        public IMonitorService Monitor
        {
            get
            {
                return _monitorService;
            }
        }
    }
}