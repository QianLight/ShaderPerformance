using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class GMLocationNativeCode
{
    public static readonly string IOS_SRC_HEADER = @"#include ""PluginBase/AppDelegateListener.h""";

    public static readonly string IOS_SRC_FINISH =
"- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions\n{";

    public static readonly string IOS_SRC_OPENURL =
"- (BOOL)application:(UIApplication*)application openURL:(NSURL*)url sourceApplication:(NSString*)sourceApplication annotation:(id)annotation";

	public static readonly string IOS_SRC_OPENURL_IOS9 = "- (BOOL)application:(UIApplication*)app openURL:(NSURL*)url options:(NSDictionary<NSString*, id>*)options";

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


}
