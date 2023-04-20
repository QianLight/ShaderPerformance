namespace GSDK
{
    public class WebviewService : IWebviewService
    {
        public void Open(WebviewParameter parameter, WebviewShowDelegate webviewShowDelegate,
            WebviewExitDelegate webviewExitDelegate = null,
            WebviewCustomParamDelegate webviewCustomParamDelegate = null)
        {
            if (parameter == null)
            {
                GLog.LogError("parameter is null.");
                webviewShowDelegate.Invoke(new Result(ErrorCode.WebviewParameterError, "WebviewParameter is null."));
                return;
            }

            //传入退出回调
            GSDKProfilerTools.BeginSample("@qirui WebView Open");
            GMSDKMgr.instance.SDK.SdkShowWebViewController(parameter,
                callback =>
                {
                    InnerTools.SafeInvoke(() =>
                    {
                        if (webviewExitDelegate == null)
                        {
                            GLog.LogInfo("webviewExitDelegate is null.");
                            return;
                        }

                        webviewExitDelegate.Invoke();
                    });
                }, showCallback =>
                {
                    GSDKProfilerTools.EndSample();
                    InnerTools.SafeInvoke(() =>
                    {
                        if (webviewShowDelegate != null)
                        {
                            var result = InnerTools.ConvertToResult(showCallback);
                            webviewShowDelegate.Invoke(result);
                        }
                    });
                },paramCallback =>
                {
                    if (webviewCustomParamDelegate == null)
                    {
                        GLog.LogInfo("webviewCustomParamDelegate is null.");
                        return;
                    }
                    webviewCustomParamDelegate.Invoke(paramCallback.data);
                });
        }

        public void Close()
        {
            GSDKProfilerTools.BeginSample("@qirui WebView close");
            GMSDKMgr.instance.SDK.SdkCloseWebView();
            GSDKProfilerTools.EndSample();
        }
        
    }
}