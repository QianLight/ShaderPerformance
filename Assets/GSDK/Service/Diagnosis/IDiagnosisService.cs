namespace GSDK
{
    /// <summary>
    /// 用于获取实例进行接口调用
    /// e.g.Diagnosis.Service.MethodName();
    /// </summary>
    public static class Diagnosis
    {
        public static IDiagnosisService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.Diagnosis) as IDiagnosisService; }
        }
    }

    // 可通过内置网页跳转网络诊断页面进行网络诊断
    /// 通过内置网页（具体使用可参考IWebviewService#OpenWebview）打开网络诊断页面，需传入网络诊断url地址。「页面只支持竖屏」
    ///
    /// 特别注意！使用此功能目前需要中台将appid添加至白名单，请联系游戏中台产品同学@曾祥霆
    ///
    /// 
    /// 不同区域的url地址：（最新url地址请参考「游戏服务」-「网络诊断工具」-「概述」：https://gdoc.bytedance.com/docs/1479）
    /// 
    /// 国内包：https://gsdk.snssdk.com/network_diagnosis
    /// 海外-新加坡包：http://gsdk-sg.bytegsdk.com/network_diagnosis
    /// 海外-美东包：https://gsdk-va.bytegsdk.com/network_diagnosis
    public interface IDiagnosisService : IService
    {
    }

}