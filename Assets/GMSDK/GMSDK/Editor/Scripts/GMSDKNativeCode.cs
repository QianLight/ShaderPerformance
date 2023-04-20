using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class GMSDKNativeCode
{
    public static readonly string IOS_SRC_HEADER = @"#include ""PluginBase/AppDelegateListener.h""";

    public static readonly string IOS_SRC_FINISH =
"- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions\n{";

    public static readonly string IOS_SRC_OPENURL =
"- (BOOL)application:(UIApplication*)application openURL:(NSURL*)url sourceApplication:(NSString*)sourceApplication annotation:(id)annotation\n{";

	public static readonly string IOS_SRC_OPENURL_IOS9 = "- (BOOL)application:(UIApplication*)app openURL:(NSURL*)url options:(NSDictionary<NSString*, id>*)options\n{";

    public static readonly string IOS_SRC_REGISTER =
"- (void)application:(UIApplication*)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData*)deviceToken\n{";

    public static readonly string IOS_SRC_REGISTER_FAIL =
        "- (void)application:(UIApplication*)application didFailToRegisterForRemoteNotificationsWithError:(NSError*)error\n{";

    public static readonly string IOS_SRC_RECEIVE =
"- (void)application:(UIApplication*)application didReceiveRemoteNotification:(NSDictionary*)userInfo\n{";

	public static readonly string IOS_SRC_RECEIVE_NOTIFICATION = 
		"- (void)application:(UIApplication *)application didReceiveRemoteNotification:(NSDictionary *)userInfo fetchCompletionHandler:(void (^)(UIBackgroundFetchResult result))handler\n" +
		"{";
	
    public static readonly string IOS_SRC_ACTIVE = "- (void)applicationDidBecomeActive:(UIApplication*)application\n{";

	public static readonly string IOS_CONTINUEACTIVITY_HEAD_EXIST_CHECK = "- (BOOL)application:(UIApplication *)application continueUserActivity:(NSUserActivity *)userActivity";
	public static readonly string IOS_CONTINUEACTIVITY_HEAD = "- (BOOL)application:(UIApplication *)application continueUserActivity:(NSUserActivity *)userActivity restorationHandler:(void (^)(NSArray * _Nullable))restorationHandler\n{";

	public static readonly string IOS_CONTINUEACTIVITY_CONTENT = "\n    [GMRouter forwardURL:nil withUserActivity:userActivity];\n";
	
	public static readonly string IOS_CONTINUEACTIVITY_END = "\n	return NO;\n" + 
	                                                         "}";

    //头引用
	public static readonly string IOS_HEADER = "\n#import <BD_GameSDK_iOS/BDGTracker.h>\n" + 
	    "#if __has_include(<BD_GameSDK_iOS/GMAccountManager+Base.h>) || __has_include(<GMAccountSDK/GMAccountManager+Base.h>)\n" + 
	    "#import <BD_GameSDK_iOS/GMAccountManager+Base.h>\n" +
	    "#endif\n" +
		"#import <BD_GameSDK_iOS/NSArray+GMFunction.h>\n" +
		"#import <BD_GameSDK_iOS/GMSDKInitManager.h>\n" +
		"#import <UserNotifications/UserNotifications.h>\n" + 
		"#import <BD_GameSDK_iOS/GMRouter.h>\n" + 
		"#define UNITY_USES_REMOTE_NOTIFICATIONS 1 \n";

    //did finish launch
    public static readonly string IOS_GMZ_FINISH =
	    "#if __has_include(<BD_GameSDK_iOS/GMAccountManager+Base.h>) || __has_include(<GMAccountSDK/GMAccountManager+Base.h>)\n" +
	    "[[GMAccountManager sharedManager] application:application didFinishLaunchingWithOptions:launchOptions];\n" +
	    "#endif\n";

    //8.0三方授权
	public static readonly string IOS_HANDLE_URL = "\n" +
	                                               "#if __has_include(<BD_GameSDK_iOS/GMAccountManager+Base.h>) || __has_include(<GMAccountSDK/GMAccountManager+Base.h>)\n" +
	                                               "    if ([[GMAccountManager sharedManager] application:application openURL:url sourceApplication:sourceApplication annotation:annotation]) {\n" +
	                                               "        return YES;\n" +
	                                               "    }\n" +
												   "    [GMRouter handleURLStr:url.absoluteString withLocalParams:nil];\n" +
												   "    return YES;\n" + 
	                                               "    #endif\n";

	//这一条要和IOS_PUSH_TOKEN一起替换

	public static readonly string IOS_GMA_OPTIONS_CONTENT =
		"#if __has_include(<BD_GameSDK_iOS/GMAccountManager+Base.h>) || __has_include(<GMAccountSDK/GMAccountManager+Base.h>)\n" +
		"    if ([[GMAccountManager sharedManager] application:app openURL:url options:options]) {\n" +
		"        return YES;\n" +
		"    }\n" +
		"    [GMRouter handleURLStr:url.absoluteString withLocalParams:nil];\n" +
		"	 [GMRouter forwardURL:url withUserActivity:nil];\n" +
		"    return NO;\n" +
		"    #endif\n";

	public static readonly string IOS_GMA_OPTIONS = "\n" +
	                                                "- (BOOL)application:(UIApplication*)app openURL:(NSURL*)url options:(NSDictionary<NSString*, id>*)options\n" +
	                                                "{\n" +
	                                                IOS_GMA_OPTIONS_CONTENT +
	                                                "}\n";

	//public static readonly string IOS_RECEIVE_NOTIFICATION = "\n" +
	//	"    [GMPushManager application:application didReceiveRemoteNotification:userInfo];\n";

	// public static readonly string IOS_REPORT_TOKEN = "\n" +     
		// (ConfigSettings.Instance.configModel.pushAvailable ? "    [GMPushManager reportToken:deviceToken];" : "");

    //UnityViewControllerBaseiOS.mm
    public static readonly string IOS_SRC_PREFERRED_SCREEN_EDGES_DEFERRING_SYSTEM_GESTURES = "- (UIRectEdge)preferredScreenEdgesDeferringSystemGestures\n{";

    public static readonly string IOS_SRC_PREFERS_HOME_INDICATOR_AUTO_HIDDEN = "- (BOOL)prefersHomeIndicatorAutoHidden\n{";

    public static readonly string IOS_UIRECTEDGEALL = "    return UIRectEdgeAll;";

    public static readonly string IOS_AUTO_HIDE = "    return false;";

	public static readonly string IOS_UnitySetAbsoluteURL =
        "    NSURL* url = userActivity.webpageURL;\n" +
		"    if (url)\n" +
		"        UnitySetAbsoluteURL(url.absoluteString.UTF8String);";
}
