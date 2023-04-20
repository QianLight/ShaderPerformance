using System;
using System.Text;

/**
 * 合并登录模块参数
 */
public class CombinLoginParame
{
    public static void ParamCombin(SDKConfigModule gdevSdkConfigModule,
        SDKConfigModule originSdkConfigModule)
    {
        ToutiaoLoginCombin(gdevSdkConfigModule, originSdkConfigModule);
        DouyinLoginCombin(gdevSdkConfigModule, originSdkConfigModule);
        MobileLoginCombin(gdevSdkConfigModule, originSdkConfigModule);
        XiguaLoginCombin(gdevSdkConfigModule, originSdkConfigModule);
        TapTapLoginCombin(gdevSdkConfigModule, originSdkConfigModule);
        //todo Gdev暂不支持抖音火山版
        AppleLoginCombin(gdevSdkConfigModule, originSdkConfigModule);
    }

    private static void ToutiaoLoginCombin(SDKConfigModule gdevSdkConfigModule,
        SDKConfigModule originSdkConfigModule)
    {
        if (originSdkConfigModule.loginConfigModule.toutiao_login_enable &&
            !gdevSdkConfigModule.loginConfigModule.toutiao_login_enable)
        {
            return;
        }

        originSdkConfigModule.loginConfigModule.toutiao_login_enable =
            gdevSdkConfigModule.loginConfigModule.toutiao_login_enable;
        if (gdevSdkConfigModule.loginConfigModule.toutiao_login_enable)
        {
            originSdkConfigModule.loginConfigModule.toutiao_platform_id = CombinParamManager.GetCombineValue(
                originSdkConfigModule.loginConfigModule.toutiao_platform_id,
                gdevSdkConfigModule.loginConfigModule.toutiao_platform_id);

            originSdkConfigModule.loginConfigModule.toutiao_platform_key = CombinParamManager.GetCombineValue(
                originSdkConfigModule.loginConfigModule.toutiao_platform_key,
                gdevSdkConfigModule.loginConfigModule.toutiao_platform_key);

            originSdkConfigModule.loginConfigModule.toutiao_friend_permission =
                gdevSdkConfigModule.loginConfigModule.toutiao_friend_permission;
        }
    }

    private static void DouyinLoginCombin(SDKConfigModule gdevSDKConfigModule,
        SDKConfigModule originSDKConfigModule)
    {
        if (originSDKConfigModule.loginConfigModule.aweme_login_enable &&
            !gdevSDKConfigModule.loginConfigModule.aweme_login_enable)
        {
            return;
        }

        //todo GDev暂未支持
        bool aweme_dy_real_name_permission = false;
        originSDKConfigModule.loginConfigModule.aweme_login_enable =
            gdevSDKConfigModule.loginConfigModule.aweme_login_enable;
        if (gdevSDKConfigModule.loginConfigModule.aweme_login_enable)
        {
            originSDKConfigModule.loginConfigModule.aweme_platform_id = CombinParamManager.GetCombineValue(
                originSDKConfigModule.loginConfigModule.aweme_platform_id,
                gdevSDKConfigModule.loginConfigModule.aweme_platform_id);
            originSDKConfigModule.loginConfigModule.aweme_platform_key = CombinParamManager.GetCombineValue(
                originSDKConfigModule.loginConfigModule.aweme_platform_key,
                gdevSDKConfigModule.loginConfigModule.aweme_platform_key);
            originSDKConfigModule.loginConfigModule.aweme_friend_permission =
                gdevSDKConfigModule.loginConfigModule.aweme_friend_permission;
            originSDKConfigModule.loginConfigModule.aweme_relation_follow =
                gdevSDKConfigModule.loginConfigModule.aweme_relation_follow;
            originSDKConfigModule.loginConfigModule.aweme_relation_follow_default_check =
                gdevSDKConfigModule.loginConfigModule.aweme_relation_follow_default_check;
            originSDKConfigModule.loginConfigModule.aweme_mobile_permission =
                gdevSDKConfigModule.loginConfigModule.aweme_mobile_permission;
            originSDKConfigModule.loginConfigModule.aweme_video_create =
                gdevSDKConfigModule.loginConfigModule.aweme_video_create;
            originSDKConfigModule.loginConfigModule.aweme_friend_list =
                gdevSDKConfigModule.loginConfigModule.aweme_friend_list;
            originSDKConfigModule.loginConfigModule.aweme_video_list_data_permission =
                gdevSDKConfigModule.loginConfigModule.aweme_video_list_data_permission;
        }
    }

    private static void MobileLoginCombin(SDKConfigModule gdevSDKConfigModule,
        SDKConfigModule originSDKConfigModule)
    {
        originSDKConfigModule.loginConfigModule.android_cm_app_id =
            CombinParamManager.GetCombineValue(originSDKConfigModule.loginConfigModule.android_cm_app_id,
                gdevSDKConfigModule.loginConfigModule.android_cm_app_id);

        originSDKConfigModule.loginConfigModule.android_cm_app_key = CombinParamManager.GetCombineValue(
            originSDKConfigModule.loginConfigModule.android_cm_app_key,
            gdevSDKConfigModule.loginConfigModule.android_cm_app_key);

        originSDKConfigModule.loginConfigModule.iOS_cm_app_id = CombinParamManager.GetCombineValue(
            originSDKConfigModule.loginConfigModule.iOS_cm_app_id, gdevSDKConfigModule.loginConfigModule.iOS_cm_app_id);

        originSDKConfigModule.loginConfigModule.iOS_cm_app_key = CombinParamManager.GetCombineValue(
            originSDKConfigModule.loginConfigModule.iOS_cm_app_key,
            gdevSDKConfigModule.loginConfigModule.iOS_cm_app_key);

        originSDKConfigModule.loginConfigModule.android_ct_app_key = CombinParamManager.GetCombineValue(
            originSDKConfigModule.loginConfigModule.android_ct_app_key,
            gdevSDKConfigModule.loginConfigModule.android_ct_app_key);

        originSDKConfigModule.loginConfigModule.android_ct_app_secret = CombinParamManager.GetCombineValue(
            originSDKConfigModule.loginConfigModule.android_ct_app_secret,
            gdevSDKConfigModule.loginConfigModule.android_ct_app_secret);

        originSDKConfigModule.loginConfigModule.iOS_ct_app_key = CombinParamManager.GetCombineValue(
            originSDKConfigModule.loginConfigModule.iOS_ct_app_key,
            gdevSDKConfigModule.loginConfigModule.iOS_ct_app_key);

        originSDKConfigModule.loginConfigModule.iOS_ct_app_secret = CombinParamManager.GetCombineValue(
            originSDKConfigModule.loginConfigModule.iOS_ct_app_secret,
            gdevSDKConfigModule.loginConfigModule.iOS_ct_app_secret);

        originSDKConfigModule.loginConfigModule.android_cu_app_key = CombinParamManager.GetCombineValue(
            originSDKConfigModule.loginConfigModule.android_cu_app_key,
            gdevSDKConfigModule.loginConfigModule.android_cu_app_key);

        originSDKConfigModule.loginConfigModule.android_cu_app_secret = CombinParamManager.GetCombineValue(
            originSDKConfigModule.loginConfigModule.android_cu_app_secret,
            gdevSDKConfigModule.loginConfigModule.android_cu_app_secret);

        originSDKConfigModule.loginConfigModule.iOS_cu_app_key = CombinParamManager.GetCombineValue(
            originSDKConfigModule.loginConfigModule.iOS_cu_app_key,
            gdevSDKConfigModule.loginConfigModule.iOS_cu_app_key);

        originSDKConfigModule.loginConfigModule.iOS_cu_app_secret = CombinParamManager.GetCombineValue(
            originSDKConfigModule.loginConfigModule.iOS_cu_app_secret,
            gdevSDKConfigModule.loginConfigModule.iOS_cu_app_secret);
    }

    private static void XiguaLoginCombin(SDKConfigModule gdevSDKConfigModule,
        SDKConfigModule originSDKConfigModule)
    {
        if (originSDKConfigModule.loginConfigModule.xigua_login_enable &&
            !gdevSDKConfigModule.loginConfigModule.xigua_login_enable)
        {
            return;
        }

        originSDKConfigModule.loginConfigModule.xigua_login_enable =
            gdevSDKConfigModule.loginConfigModule.xigua_login_enable;
        if (gdevSDKConfigModule.loginConfigModule.xigua_login_enable)
        {
            originSDKConfigModule.loginConfigModule.xigua_platform_id = CombinParamManager.GetCombineValue(
                originSDKConfigModule.loginConfigModule.xigua_platform_id,
                gdevSDKConfigModule.loginConfigModule.xigua_platform_id);
            originSDKConfigModule.loginConfigModule.xigua_platform_key = CombinParamManager.GetCombineValue(
                originSDKConfigModule.loginConfigModule.xigua_platform_key,
                gdevSDKConfigModule.loginConfigModule.xigua_platform_key);
        }
    }

    private static void TapTapLoginCombin(SDKConfigModule gdevSDKConfigModule,
        SDKConfigModule originSDKConfigModule)
    {
        if (originSDKConfigModule.loginConfigModule.android_taptap_login_enable &&
            !gdevSDKConfigModule.loginConfigModule.android_taptap_login_enable)
        {
            return;
        }

        originSDKConfigModule.loginConfigModule.android_taptap_login_enable =
            gdevSDKConfigModule.loginConfigModule.android_taptap_login_enable;
        if (gdevSDKConfigModule.loginConfigModule.android_taptap_login_enable)
        {
            originSDKConfigModule.loginConfigModule.android_taptap_platform_id = CombinParamManager.GetCombineValue(
                originSDKConfigModule.loginConfigModule.android_taptap_platform_id,
                gdevSDKConfigModule.loginConfigModule.android_taptap_platform_id);

            originSDKConfigModule.loginConfigModule.android_taptap_platform_key = CombinParamManager.GetCombineValue(
                originSDKConfigModule.loginConfigModule.android_taptap_platform_key,
                gdevSDKConfigModule.loginConfigModule.android_taptap_platform_key);

            originSDKConfigModule.loginConfigModule.android_taptap_platform_secret =
                CombinParamManager.GetCombineValue(
                    originSDKConfigModule.loginConfigModule.android_taptap_platform_secret,
                    gdevSDKConfigModule.loginConfigModule.android_taptap_platform_secret);
        }
    }




    private static void AppleLoginCombin(SDKConfigModule gdevSdkConfigModule, SDKConfigModule originSdkConfigModule)
    {
        if (originSdkConfigModule.loginConfigModule.iOS_apple_login_enable &&
            !gdevSdkConfigModule.loginConfigModule.iOS_apple_login_enable)
        {
            return;
        }

        originSdkConfigModule.loginConfigModule.iOS_apple_login_enable =
            gdevSdkConfigModule.loginConfigModule.iOS_apple_login_enable;
        if (gdevSdkConfigModule.loginConfigModule.iOS_apple_login_enable)
        {
            originSdkConfigModule.loginConfigModule.iOS_apple_platform_id = CombinParamManager.GetCombineValue(
                originSdkConfigModule.loginConfigModule.iOS_apple_platform_id,
                gdevSdkConfigModule.loginConfigModule.iOS_apple_platform_id);
        }
    }
}