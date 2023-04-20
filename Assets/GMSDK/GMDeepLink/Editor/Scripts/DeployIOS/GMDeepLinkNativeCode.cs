public class GMDeepLinkNativeCode
{
    public static readonly string IOS_SRC_HEADER = @"#include ""PluginBase/AppDelegateListener.h""";

    public static readonly string IOS_SRC_CONTINUEACTIVITY = 
        "- (BOOL)application:(UIApplication *)application continueUserActivity:(NSUserActivity *)userActivity restorationHandler:(void (^)(NSArray * _Nullable))restorationHandler\n{";

    public static readonly string IOS_SRC_OPENURL = 
        "- (BOOL)application:(UIApplication*)application openURL:(NSURL*)url sourceApplication:(NSString*)sourceApplication annotation:(id)annotation\n{";

    public static readonly string IOS_SRC_OPENURL_IOS9 = 
        "- (BOOL)application:(UIApplication*)app openURL:(NSURL*)url options:(NSDictionary<NSString*, id>*)options\n{";
    
    public static readonly string IOS_HEADER = "#import <BD_GameSDK_iOS/GMDeepLinkManager.h>\n";

    public static readonly string IOS_CONTINUEACTIVITY =
        "\n\tif ([[userActivity activityType] isEqualToString:NSUserActivityTypeBrowsingWeb]) {" +
        "\n\t\tNSURL *url = [userActivity webpageURL];" +
        "\n\t\t[[GMDeepLinkManager shareInstance] deepLinkWithType:GMDeepLinkTypeUniversalLink uri:[url absoluteString]];" +
        "\n\t}\n";

    public static readonly string IOS_OPENURL = 
        "\n\t[[GMDeepLinkManager shareInstance] deepLinkWithType:GMDeepLinkTypeScheme uri:[url absoluteString]];\n";

    public static readonly string IOS_OPENURL_IOS9 = 
        "\n\t[[GMDeepLinkManager shareInstance] deepLinkWithType:GMDeepLinkTypeScheme uri:[url absoluteString]];\n";
}