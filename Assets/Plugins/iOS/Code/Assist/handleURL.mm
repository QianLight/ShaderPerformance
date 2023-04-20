//
//  handleURL.m
//  Unity-iPhone
//
//  Created by 彭怀亮 on 17/06/14.
//

#include "../SDKDef.h"

#if defined (SYS_IOS)

#import "handleURL.h"


@implementation SDKProcess

+ (BOOL)application:(UIApplication *)application handleOpenURL:(NSURL *)url
{
	BOOL retValue = NO;

#ifdef SYS_IOS

#ifdef PLATFORM_ID_TENCENT
	retValue = [WGInterface HandleOpenURL:url];
#endif

#endif

	return retValue;
}

+ (BOOL)application:(UIApplication *)application openURL:(NSURL *)url sourceApplication:(NSString *)sourceApplication annotation:(id)annotation;
{
    BOOL retValue = NO;

#ifdef SYS_IOS

#endif
    return retValue;
}

+ (void)applicationDidBecomeActive:(UIApplication *)application
{
#ifdef SYS_IOS

#endif
}

+ (BOOL)application:(UIApplication *)application didFinishLaunchingWithOptions:(NSDictionary *)launchOptions
{
    BOOL retValue = YES;

#ifdef SYS_IOS

#ifdef PLUGIN_ID_BUGLY
    [BuglyLog initLogger:BLYLogLevelDebug consolePrint:YES];

    [[CrashReporter sharedInstance] enableLog:YES];
    [[CrashReporter sharedInstance] installWithAppId:@"900001055" applicationGroupIdentifier:@"XXXXX"];

    exp_call_back_func = &exception_callback_handler;
#endif
    
#endif

    return retValue;
}


+ (void)applicationDidEnterBackground:(UIApplication *)application
{
#ifdef SYS_IOS

#endif
}

+ (void)applicationWillEnterForeground:(UIApplication *)application
{
#ifdef SYS_IOS
    
#endif
}

+ (void)application:(UIApplication *)application didRegisterForRemoteNotificationsWithDeviceToken:(NSData *)deviceToken
{
#ifdef SYS_IOS
    
#endif
}

+ (void)application:(UIApplication *)application didFailToRegisterForRemoteNotificationsWithError:(NSError *)error
{
#ifdef SYS_IOS

#if defined PLATFORM_ID_TENCENT
	NSLog(@"CFSDK ::: Register remote notifications failed with error: %@", [error description]);
	[MSDKXG WGFailedRegisteredAPNS];
#endif

#endif
}

+ (NSUInteger)application:(UIApplication *)application supportedInterfaceOrientationsForWindow:(UIWindow *)window
{
#ifdef SYS_IOS
    return 0;
#endif
}

+ (void)application:(UIApplication *)application didReceiveLocalNotification:(UILocalNotification *)notification
{
#ifdef SYS_IOS

#endif
}

+ (void)application:(UIApplication *)application didReceiveRemoteNotification:(NSDictionary *)userInfo
{
#ifdef SYS_IOS

#endif
}

+ (void)dealloc
{
#ifdef SYS_IOS

#endif
}


+ (void)touchesMoved:(NSSet *)touches withEvent:(UIEvent *)event
{
#ifdef SYS_IOS

#endif
}

@end

#endif
