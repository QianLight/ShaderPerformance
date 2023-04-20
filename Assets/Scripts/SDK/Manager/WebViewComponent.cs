using LiteWebView;
using System;

public class WebViewComponent : SDKComponent
{
    public override bool on { get { return true; } }

    public override string version { get { return "1.0.0"; } }

    public override int type { get { return SDKEnum.WebView; } }

    private WebView webView;

    public override void Init()
    {
        webView = transform.gameObject.AddComponent<WebView>();
        webView.Init();
    }

    public void LoadUrl(string url, int top = 0, int btm = 0, int left = 0, int right = 0)
    {
        webView.Show(top, btm, left, right);
        webView.LoadUrl(url);
    }

    public void LoadUrl(string url, string arg, Action<string> cb, int top = 0, int btm = 0, int left = 0, int right = 0)
    {
        webView.RegistJsInterfaceAction(arg, cb);
        LoadUrl(url, top, btm, left, right);
    }

    public void LoadHtml(string url, int top = 0, int btm = 0, int left = 0, int right = 0)
    {
        webView.Show(top, btm, left, right);
        webView.LoadLocal(url);
    }

    public void LoadHtml(string url, string arg, Action<string> cb, int top = 0, int btm = 0, int left = 0, int right = 0)
    {
        webView.RegistJsInterfaceAction(arg, cb);
        LoadHtml(url, top, btm, left, right);
    }

    public void CallJS(string funName, string msg)
    {
        webView.CallJS(funName, msg);
    }

    public void Close()
    {
        webView.Close();
    }

    public void RegisterJSCallback(string name,Action<string> action)
    {
        webView.RegistJsInterfaceAction(name, action);
    }
}