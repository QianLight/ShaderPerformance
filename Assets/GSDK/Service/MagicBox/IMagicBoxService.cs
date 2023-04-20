namespace GSDK
{
    /// <summary>
    /// 调试辅助模块。
    /// 用法：MagicBox.Service.XXX();
    /// </summary>
    public static class MagicBox
    {
        public static IMagicBoxService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.MagicBox) as IMagicBoxService; }
        }
    }

    /// <summary>
    /// 调试辅助模块。
    /// </summary>
    public interface IMagicBoxService : IService
    {
        /// <summary>
        /// 显示调试辅助悬浮窗。
        /// </summary>
        void Show();

        /// <summary>
        /// 隐藏调试辅助悬浮窗。
        /// </summary>
        void Hide();
    }
}