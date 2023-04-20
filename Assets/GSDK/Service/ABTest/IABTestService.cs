namespace GSDK
{
    /// <summary>
    /// 用于获取实例进行接口调用
    /// e.g.ABTest.Service.MethodName();
    /// </summary>
    public static class ABTest
    {
        public static IABTestService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.ABTest) as IABTestService; }
        }
    }

    public interface IABTestService : IService
    {
        /// <summary>
        /// 注册实验
        /// </summary>
        /// <param name="key">实验key，必须与libra平台的配置名字保持一致</param>
        /// <param name="defaultValue">从服务端取值失败时的本地默认值</param>
        /// <param name="owner">实验的负责人</param>
        /// <param name="description">实验的说明</param>
        /// <param name="isBindUser">
        /// 是否对账号信息敏感，以uid维度的实验建议填true，在账号频繁切换的情况，曝光数据更加准确；否则为false
        /// 账号频繁切换：针对某些对账号切换敏感的曝光数据（比如游戏账号相关）可能会有数据残缺
        /// </param>
        /// <returns>true表示解析成功，false表示defaultValue解析失败</returns>
        bool RegisterExperiment(string key, string defaultValue, string owner, string description, bool isBindUser = false);

        /// <summary>
        /// 实验取值，ios可选择是否曝光
        /// </summary>
        /// <returns>The get experiment value.</returns>
        /// <param name="key">实验key，必须与libra平台的"配置参数"名字保持一致</param>
        /// <param name="withExposure">取值的同时是否触发曝光，仅对iOS设置有效，安卓永远触发曝光</param>
        /// <return>获取到的实验结果json字符串</return>
        string GetExperimentValue(string key, bool withExposure);
    }
}