namespace GSDK
{
    /// <summary>
    /// 打开内置网页是否成功的回调
    /// </summary>
    /// <param name="result">
    /// 判断调用是否成功
    /// <para>
    /// Success: 打开成功
    /// WebviewParameterError：参数问题，可能是传入必选参数
    /// WebviewURLEmptyError：URL为空
    /// WebviewURLError：输入URL存在格式错误
    /// </para>
    /// </param>
    public delegate void WebviewShowDelegate(Result result);
    
    /// <summary>
    /// 返回打开内置网页是否成功，并可以判断是否已退出内置网页
    /// </summary>
    public delegate void WebviewExitDelegate();
    
    /// <summary>
    /// JSbridge自定义参数，customData为json字符串
    /// </summary>
    public delegate void WebviewCustomParamDelegate(string customData);


    /// <summary>
    /// 用于获取实例进行接口调用
    /// e.g.Webview.Service.MethodName();
    /// </summary>
    public static class Webview
    {
        public static IWebviewService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.Webview) as IWebviewService; }
        }
    }

    /// <summary>
    /// 内置网页介绍
    /// 1 拉起内置网页
    /// 2 支持jsbridge能力（需找中台开发同事范泽鑫配置白名单），jsbridge能力可参考：https://bytedance.feishu.cn/docs/doccnN9aCIHG1EExiJFkwAPqYyf
    /// 3 「Android」使用富文本发布器页面（需要发行同学配置id并获取发布器URL，并引入Poster子包），直接调用OpenWebview接口跳转发布器url即可
    /// </summary>
    public interface IWebviewService : IService
    {
        /// <summary>
        /// 打开内置webview
        /// </summary>
        /// <param name="parameter">webview参数，title和url必传，其余可选</param>
        /// <param name="webviewShowDelegate">打开内置网页是否成功的回调</param>
        /// <param name="webviewExitDelegate">是否退出内置网页的回调</param>
        /// <param name="webviewCustomParamDelegate">JSbridge自定义参数回调，参数由业务方业自己定义，根据需要将Webview前端自定义的数据传到unity侧进行处理</param>
        void Open(WebviewParameter parameter, WebviewShowDelegate webviewShowDelegate,
            WebviewExitDelegate webviewExitDelegate = null,
            WebviewCustomParamDelegate webviewCustomParamDelegate = null);

        void Close();
    }

    /// <summary>
    /// 打开网页参数
    /// </summary>
    public class WebviewParameter
    {
        /// <summary>
        /// 当前端没有解析到url的标题时，会显示的默认标题
        /// 例如打开百度会显示「百度一下，你就知道」，若没有网页设置标题则会显示Title
        /// </summary>
        public string Title;

        /// <summary>
        /// 跳转的url，错误格式会打开失败，并返回错误码
        /// </summary>
        public string URL;

        /// <summary>
        /// 「可选」打开页面的来源，根据数据需求设置
        /// </summary>
        public string Source;

        /// <summary>
        /// 「可选」设置屏幕方向，包括（默认）跟随系统、固定横屏和固定竖屏，通过WebConstants设置
        /// </summary>
        public WebOrientation Orientation;

        /// <summary>
        /// 「可选」是否自定义尺寸大小WebView界面（左上角为（0，0））
        /// 0:全屏不可自定义，相关参数全部失效 1：可自定义
        /// </summary>
        public int CustomsizeScreen;
        
        /// <summary>
        /// 「可选」自定义尺寸大小WebView界面的水平长度
        /// </summary>
        public int Width;
        
        /// <summary>
        /// 「可选」自定义尺寸大小WebView界面的垂直长度
        /// </summary>
        public int Height;
        
        /// <summary>
        /// 「可选」自定义尺寸大小WebView界面的水平坐标点位置
        /// </summary>
        public int LocationX;
        
        /// <summary>
        /// 「可选」自定义尺寸大小WebView界面的垂直坐标点位置
        /// </summary>
        public int LocationY;
        
        /// <summary>
        /// 「可选」自定义尺寸大小WebView界面的水平滚动条显隐 0: 显示 1：隐藏
        /// </summary>
        public int HorizontalScrollbarHidden;
        
        /// <summary>
        /// 「可选」自定义尺寸大小WebView界面的垂直滚动条显隐 0: 显示 1：隐藏
        /// </summary>
        public int VerticalScrollbarHidden;
        public WebviewParameter(string title, string url)
        {
            Title = title;
            URL = url;
            Source = "";
            Orientation = WebOrientation.Autorotate;
            CustomsizeScreen = 0;
            Width = -1;
            Height = -1;
            LocationX = 0;
            LocationY = 0;
            HorizontalScrollbarHidden = 0;
            VerticalScrollbarHidden = 0;
        }
    }

    /// <summary>
    /// 屏幕方向
    /// </summary>
    public enum WebOrientation
    {
        /// <summary>
        /// 跟随系统
        /// </summary>
        Autorotate = 0,

        /// <summary>
        /// 固定横屏
        /// </summary>
        Landscape,

        /// <summary>
        /// 固定竖屏
        /// </summary>
        Portrait
    }

    public static partial class ErrorCode
    {
        /// <summary>
        /// 参数问题
        /// </summary>
        public const int WebviewParameterError = -149902;
        
        /// <summary>
        /// URL为空
        /// </summary>
        public const int WebviewURLEmptyError = -140001;
        
        /// <summary>
        /// 输入URL存在格式错误
        /// </summary>
        public const int WebviewURLError = -140002;

        /// <summary>
        /// 未知问题
        /// </summary>
        public const int WebviewUnknownError = -149999;
    }
}