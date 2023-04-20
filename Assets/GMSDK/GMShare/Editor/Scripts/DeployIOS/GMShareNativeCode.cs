using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class GMShareNativeCode
{
    public static readonly string IOS_SRC_HEADER = @"#include ""PluginBase/AppDelegateListener.h""";

    public static readonly string IOS_SRC_FINISH =
"- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions\n{";

    public static readonly string IOS_SRC_OPENURL =
		"\n" +
		"    if ([[GMAccountManager sharedManager] application:application openURL:url sourceApplication:sourceApplication annotation:annotation]) {\n" +
		"        return YES;\n" +
		"    }\n";

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

	public static readonly string IOS_SRC_OPENURL_OPTION = "- (BOOL)application:(UIApplication*)app openURL:(NSURL*)url options:(NSDictionary<NSString*, id>*)options\n{";
	
    //头引用
	public static readonly string IOS_HEADER = "\n" +
		"#import <BD_GameSDK_iOS/GMShareManager.h>\n";
    //did finish launch
//	public static readonly string IOS_GMZ_FINISH = "";

	public static readonly string IOS_HANDLE_URL =  "\n" +
													"    if ([[GMShareManager sharedInstance] handleOpenURL:url]) {\n" +
													"        return YES;\n" +
													"    }\n";

    //这一条要和IOS_PUSH_TOKEN一起替换

	public static readonly string IOS_GMA_OPTIONS = "    if ([[GMShareManager sharedInstance] handleOpenURL:url]) {\n" +
													"        return YES;\n" +
													"    }\n";

	public static readonly string IOS_CONTINUEACTIVITY_SHARE =
		"\tif ([[GMShareManager sharedInstance] application:application handleContinueUserActivity:userActivity]) {\n" +
		"\t\treturn YES;\n" +
		"\t}\n";
}
