#import <WebKit/WebKit.h>

@interface LiteWebView :
NSObject<WKNavigationDelegate>
{
    WKWebView* _webView;
    NSString* _gameObjectName;
}
@end

@implementation LiteWebView

- (void)init:(const char*)gameObjectName{
    _gameObjectName = [NSString stringWithUTF8String:gameObjectName];
}

- (void)createWebView{
    if(_webView == nil){
        UIView* view = UnityGetGLViewController().view;
        
        _webView = [[WKWebView alloc] initWithFrame:view.frame];
        _webView.navigationDelegate = self;

        _webView.hidden = YES;
        [view addSubview:_webView];
    }
}

- (void)disposeWebView{
    if(_webView != nil){
        _webView.navigationDelegate = nil;
        [_webView removeFromSuperview];
        _webView = nil;
    }
}

//显示webview
- (void)show:(int)top bottom:(int)bottom left:(int)left right:(int)right {
    [self createWebView];
    UIView *view = UnityGetGLViewController().view;
    _webView.hidden = NO;
    CGRect frame = view.frame;
    CGFloat scale = view.contentScaleFactor;
    frame.size.width -= (left + right) / scale;
    frame.size.height -= (top + bottom) / scale;
    frame.origin.x += left / scale;
    frame.origin.y += top / scale;
    _webView.frame = frame;
}

//加载页面
- (void)loadUrl:(const char*)url{
    [self createWebView];
    NSString *urlStr = [NSString stringWithUTF8String:url];
    NSURL *nsurl = [NSURL URLWithString:urlStr];
    NSURLRequest *request = [NSURLRequest requestWithURL:nsurl];
    [_webView loadRequest:request];
    [_webView reload];
}

//关闭webview窗口
- (void)close{
    if(_webView == nil){
        return;
    }
    _webView.hidden = YES;
    [self disposeWebView];
}

//调用JS
- (void)callJS:(const char*)funName msg:(const char*)msg{
    if(_webView == nil){
        return;
    }
    NSString *jsStr= [NSString stringWithFormat:@"%s(\"%s\")",funName,msg];

    [_webView evaluateJavaScript:jsStr completionHandler:nil];
}


- (void)webView:(WKWebView *)webView decidePolicyForNavigationAction:(WKNavigationAction *)navigationAction decisionHandler:(void (^)(WKNavigationActionPolicy))decisionHandler;
{    
    NSString *url =navigationAction.request.URL.absoluteString;
    NSLog(@"navigationResponse.response = %@", url);
    UnitySendMessage([_gameObjectName UTF8String], "OnLoadingUrl", [url UTF8String]);
    NSRange range = [url rangeOfString:@"LiteWebView://"];
    if(range.location != NSNotFound){
        NSString *msg = [url substringFromIndex:range.length];
        UnitySendMessage([_gameObjectName UTF8String], "OnJsCall", [msg UTF8String]);
    }
    decisionHandler(WKNavigationActionPolicyAllow);
}


@end


extern "C"
{
    void _registResponseGameObject(const char* gameObjectName);
    void _show(int top, int bottom, int left, int right);
    void _loadUrl(const char* url);
    void _close();
    void _callJS(const char* funName, const char* msg);


    static LiteWebView *ulite;
    const char* gameObjectName;
    
    void _registCallBackGameObjectName(const char* gameObjectName){
        if(ulite != nil){
            return;
        }
        ulite = [LiteWebView alloc];
        [ulite init:gameObjectName];
    }

    void _show(int top, int bottom, int left, int right){
        if(ulite == nil){
            return;
        }
        
        [ulite show:top bottom:bottom left:left right:right];
        
    }
    
    void _loadUrl(const char* url){
        if(ulite == nil){
            return;
        }
        [ulite loadUrl:url];
    }
    
    void _close(){
        if(ulite == nil){
            return;
        }
        
        [ulite close];
    }
    
    void _callJS(const char* funName, const char* msg){
        if(ulite == nil){
            return;
        }
        
        [ulite callJS:funName msg:msg];
    }
}

