public class CombinPushParam
{
    public static void ParamCombin(SDKConfigModule gdevSdkConfigModule, SDKConfigModule originSdkConfigModule)
    {
        if (originSdkConfigModule.pushConfigModule.push_enable && !gdevSdkConfigModule.pushConfigModule.push_enable)
        {
            return;
        }

        originSdkConfigModule.pushConfigModule.push_enable = gdevSdkConfigModule.pushConfigModule.push_enable;
        if (gdevSdkConfigModule.pushConfigModule.push_enable)
        {
            originSdkConfigModule.pushConfigModule.android_push_app_name = CombinParamManager.GetCombineValue(
                originSdkConfigModule.pushConfigModule.android_push_app_name,
                gdevSdkConfigModule.pushConfigModule.android_push_app_name);
            originSdkConfigModule.pushConfigModule.android_push_huawei_appid =
                gdevSdkConfigModule.pushConfigModule.android_push_huawei_appid;
            originSdkConfigModule.pushConfigModule.android_push_mi_app_id =
                gdevSdkConfigModule.pushConfigModule.android_push_mi_app_id;
            originSdkConfigModule.pushConfigModule.android_push_mi_app_key =
                gdevSdkConfigModule.pushConfigModule.android_push_mi_app_key;
            originSdkConfigModule.pushConfigModule.android_push_meizu_app_id =
                gdevSdkConfigModule.pushConfigModule.android_push_meizu_app_id;
            originSdkConfigModule.pushConfigModule.android_push_meizu_app_key =
                gdevSdkConfigModule.pushConfigModule.android_push_meizu_app_key;
            originSdkConfigModule.pushConfigModule.android_push_oppo_app_key =
                gdevSdkConfigModule.pushConfigModule.android_push_oppo_app_key;
            originSdkConfigModule.pushConfigModule.android_push_oppo_app_secret =
                gdevSdkConfigModule.pushConfigModule.android_push_oppo_app_secret;
            originSdkConfigModule.pushConfigModule.android_push_umeng_app_key =
                gdevSdkConfigModule.pushConfigModule.android_push_umeng_app_key;
            originSdkConfigModule.pushConfigModule.android_push_umeng_app_secret =
                gdevSdkConfigModule.pushConfigModule.android_push_umeng_app_secret;
        }
    }
}