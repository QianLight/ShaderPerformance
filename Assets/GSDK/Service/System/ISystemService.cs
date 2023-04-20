namespace GSDK
{
    #region Delegate

    /// <summary>
    /// 通过回调返回充电状态，true为正在充电
    /// </summary>
    /// <param name="charging">充电状态</param>
    public delegate void SystemFetchChargingStatusEventHandler(bool charging);

    /// <summary>
    /// 通过回调返回插入耳机，true为耳机已插入
    /// </summary>
    /// <param name="plugging">是否插入耳机</param>
    public delegate void SystemFetchHeadsetStatusEventHandler(bool plugging);

#if UNITY_ANDROID
    /// <summary>
    /// 通过回调返回是否是模拟器
    /// </summary>
    /// <para>
    /// 可能返回的错误码：
    /// Success：成功
    /// SystemServerParameterError：服务端参数错误
    /// SystemServerExceptionError：服务器异常
    /// SystemFetchError：获取失败
    /// SystemNetworkAnomaliesError：网络异常
    /// </para>
    /// <param name="result">判断调用是否成功，包含上述错误码</param>
    /// <param name="isEmulator">是否为模拟器，true为是，false为否</param>
    /// <param name="emulatorBrand">模拟器品牌</param>
    public delegate void SystemCheckEmulatorDelegate(Result result, bool isEmulator, string emulatorBrand);
#endif

    #endregion

    /// <summary>
    /// 用于获取实例进行接口调用
    /// e.g.System.Service.MethodName();
    /// </summary>
    public static class System
    {
        public static ISystemService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.System) as ISystemService; }
        }
    }

    /// <summary>
    /// 获取相关的系统信息
    /// </summary>
    public interface ISystemService : IService
    {
        event SystemFetchChargingStatusEventHandler ChargingEvent;

        event SystemFetchHeadsetStatusEventHandler HeadsetEvent;

        /// <summary>
        /// 获取当前设备网络状态
        /// </summary>
        NetworkState NetworkState { get; }

        /// <summary>
        /// 是否正在充电
        /// </summary>
        /// <returns>true 表示正在充电</returns>
        bool Charging { get; }

        /// <summary>
        /// 获取设备电量
        /// </summary>
        /// <returns>取值返回在(0...1)</returns>
        double Electricity { get; }

        /// <summary>
        /// 是否插入耳机
        /// </summary>
        /// <returns>true 表示耳机已插入，false 表示耳机未插入</returns>
        bool HeadsetPlugging { get; }

        /// <summary>
        /// 获取和设置屏幕亮度（0.0 - 1.0）
        /// </summary>
        /// <returns>调用该接口会返回设备当前屏幕的亮度，取值范围 0.0 ~ 1.0。</returns>
        double ScreenBrightness { get; set; }

        /// <summary>
        /// 获取和设置屏幕亮度（0.0 - 1.0）
        /// </summary>
        /// <returns>调用该接口会返回设备当前屏幕的亮度，取值范围 0.0 ~ 1.0。</returns>
        double CurrentWindowBrightness { get; set; }

        /// <summary>
        /// 判断设备屏幕类型
        /// </summary>
        /// <returns>返回屏幕类型，可判断是否为异形屏</returns>
        SystemScreenType ScreenType { get; }

#if UNITY_ANDROID
        /// <summary>
        /// 通过回调返回是否是模拟器
        ///
        /// 需要GSDK初始化后才能调用（依赖GP初始化流程）
        /// </summary>
        void CheckEmulator(SystemCheckEmulatorDelegate emulatorDelegate);
#endif
    }

    /// <summary>
    /// 当前设备网络状态 WIFI、2G、3G、4G、5G和未连接
    /// </summary>
    public enum NetworkState
    {
        Unknown = -1,
        NotReachable = 0,
        Wifi,
        _2G,
        _3G,
        _4G,
        _5G
    }

    /// <summary>
    /// 屏幕类型
    /// </summary>
    public enum SystemScreenType
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// 非异形屏
        /// </summary>
        NotAnomalous,

        /// <summary>
        /// 异形屏
        /// </summary>
        Anomalous
    }

    public static partial class ErrorCode
    {
        /// <summary>
        /// 服务端参数错误
        /// </summary>
        public const int SystemServerParameterError = -281001;

        /// <summary>
        /// 服务器异常
        /// </summary>
        public const int SystemServerExceptionError = -285000;

        /// <summary>
        /// 获取失败
        /// </summary>
        public const int SystemFetchError = -281010;

        /// <summary>
        /// 网络异常
        /// </summary>
        public const int SystemNetworkAnomaliesError = -283000;

        /// <summary>
        /// 未知异常
        /// </summary>
        public const int SystemUnknownError = -289999;
    }
}