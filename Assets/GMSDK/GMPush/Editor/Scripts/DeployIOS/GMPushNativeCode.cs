public class GMPushNativeCode
{
    public static readonly string IOS_SRC_HEADER = @"#include ""PluginBase/AppDelegateListener.h""";

    public static readonly string IOS_SRC_FINISH =
        "- (BOOL)application:(UIApplication*)application didFinishLaunchingWithOptions:(NSDictionary*)launchOptions\n{";
    
    public static readonly string IOS_HEADER = "#import <BD_GameSDK_iOS/GMPushManager.h>\n";

    public static readonly string IOS_FINISH = "    // did finish launch\n    " +
                                               "    if (UNUserNotificationCenter.class) {\n" +
                                               "        [[UNUserNotificationCenter currentNotificationCenter] requestAuthorizationWithOptions:UNAuthorizationOptionBadge completionHandler:^(BOOL granted, NSError * _Nullable error) {\n" +
                                               "            if (error) {\n" +
                                               "                NSLog(@\"requestAuthorizationWithOptionsError:%@\", error);\n" +
                                               "            }\n" +
                                               "        }];\n" +
                                               "    } else {\n" +
                                               "        UIUserNotificationSettings *settings = [UIUserNotificationSettings settingsForTypes:UIUserNotificationTypeBadge categories:nil];\n" +
                                               "        [application registerUserNotificationSettings:settings];\n" +
                                               "    }\n" +
                                               "    [application registerForRemoteNotifications];\n" +
                                               "    [GMPushManager clearBadgeNumber];\n";
}