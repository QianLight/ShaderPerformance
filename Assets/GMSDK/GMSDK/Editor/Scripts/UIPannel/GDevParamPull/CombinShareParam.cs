public class CombinShareParam
{
    public static void ParamCombin(SDKConfigModule gdevSdkConfigModule, SDKConfigModule originSdkConfigModule)
    {
        originSdkConfigModule.shareConfigModule.android_share_panel_id = CombinParamManager.GetCombineValue(
            originSdkConfigModule.shareConfigModule.android_share_panel_id,
            gdevSdkConfigModule.shareConfigModule.android_share_panel_id);

        originSdkConfigModule.shareConfigModule.iOS_share_panel_id = CombinParamManager.GetCombineValue(
            originSdkConfigModule.shareConfigModule.iOS_share_panel_id,
            gdevSdkConfigModule.shareConfigModule.iOS_share_panel_id);
        //android
        CombinAndroidQQ(gdevSdkConfigModule, originSdkConfigModule);
        CombinAndroidWX(gdevSdkConfigModule, originSdkConfigModule);
        CombinAndroidWB(gdevSdkConfigModule, originSdkConfigModule);
        CombinAndroidAwe(gdevSdkConfigModule, originSdkConfigModule);

        //iOS
        CombinIOSQQ(gdevSdkConfigModule, originSdkConfigModule);
        CombinIOSWX(gdevSdkConfigModule, originSdkConfigModule);
        CombinIOSWB(gdevSdkConfigModule, originSdkConfigModule);
        CombinIOSAwe(gdevSdkConfigModule, originSdkConfigModule);

    }

    private static void CombinAndroidQQ(SDKConfigModule gdevSdkConfigModule,
        SDKConfigModule originSdkConfigModule)
    {
        if (originSdkConfigModule.shareConfigModule.android_qq_available &&
            !gdevSdkConfigModule.shareConfigModule.android_qq_available)
        {
            return;
        }

        originSdkConfigModule.shareConfigModule.android_qq_available =
            gdevSdkConfigModule.shareConfigModule.android_qq_available;
        if (gdevSdkConfigModule.shareConfigModule.android_qq_available)
        {
            originSdkConfigModule.shareConfigModule.android_qq_key = CombinParamManager.GetCombineValue(
                originSdkConfigModule.shareConfigModule.android_qq_key,
                gdevSdkConfigModule.shareConfigModule.android_qq_key);
        }
    }

    private static void CombinAndroidWX(SDKConfigModule gdevSdkConfigModule,
        SDKConfigModule originSdkConfigModule)
    {
        if (originSdkConfigModule.shareConfigModule.android_wx_available &&
            !gdevSdkConfigModule.shareConfigModule.android_wx_available)
        {
            return;
        }

        originSdkConfigModule.shareConfigModule.android_wx_available =
            gdevSdkConfigModule.shareConfigModule.android_wx_available;
        if (gdevSdkConfigModule.shareConfigModule.android_wx_available)
        {
            originSdkConfigModule.shareConfigModule.android_wx_key = CombinParamManager.GetCombineValue(
                originSdkConfigModule.shareConfigModule.android_wx_key,
                gdevSdkConfigModule.shareConfigModule.android_wx_key);
        }
    }

    private static void CombinAndroidWB(SDKConfigModule gdevSdkConfigModule,
        SDKConfigModule originSdkConfigModule)
    {
        if (originSdkConfigModule.shareConfigModule.android_weibo_available &&
            !gdevSdkConfigModule.shareConfigModule.android_weibo_available)
        {
            return;
        }

        originSdkConfigModule.shareConfigModule.android_weibo_available =
            gdevSdkConfigModule.shareConfigModule.android_weibo_available;
        if (gdevSdkConfigModule.shareConfigModule.android_weibo_available)
        {
            originSdkConfigModule.shareConfigModule.android_weibo_key =
                CombinParamManager.GetCombineValue(originSdkConfigModule.shareConfigModule.android_weibo_key,
                    gdevSdkConfigModule.shareConfigModule.android_weibo_key);

            originSdkConfigModule.shareConfigModule.android_weibo_url = CombinParamManager.GetCombineValue(
                originSdkConfigModule.shareConfigModule.android_weibo_url,
                gdevSdkConfigModule.shareConfigModule.android_weibo_url);
        }
    }

    private static void CombinAndroidAwe(SDKConfigModule gdevSdkConfigModule,
        SDKConfigModule originSdkConfigModule)
    {
        if (originSdkConfigModule.shareConfigModule.android_awe_available &&
            !gdevSdkConfigModule.shareConfigModule.android_awe_available)
        {
            return;
        }

        originSdkConfigModule.shareConfigModule.android_awe_available =
            gdevSdkConfigModule.shareConfigModule.android_awe_available;
        if (gdevSdkConfigModule.shareConfigModule.android_awe_available)
        {
            originSdkConfigModule.shareConfigModule.android_awe_key = CombinParamManager.GetCombineValue(
                originSdkConfigModule.shareConfigModule.android_awe_key,
                gdevSdkConfigModule.shareConfigModule.android_awe_key);
        }
    }

    private static void CombinIOSQQ(SDKConfigModule gdevSdkConfigModule,
        SDKConfigModule originSdkConfigModule)
    {
        if (originSdkConfigModule.shareConfigModule.iOS_qq_available &&
            !gdevSdkConfigModule.shareConfigModule.iOS_qq_available)
        {
            return;
        }

        originSdkConfigModule.shareConfigModule.iOS_qq_available =
            gdevSdkConfigModule.shareConfigModule.iOS_qq_available;
        if (gdevSdkConfigModule.shareConfigModule.iOS_qq_available)
        {
            originSdkConfigModule.shareConfigModule.iOS_qq_key = CombinParamManager.GetCombineValue(
                originSdkConfigModule.shareConfigModule.iOS_qq_key, gdevSdkConfigModule.shareConfigModule.iOS_qq_key);
            originSdkConfigModule.shareConfigModule.iOS_qq_universal_link = CombinParamManager.GetCombineValue(
                originSdkConfigModule.shareConfigModule.iOS_qq_universal_link,
                gdevSdkConfigModule.shareConfigModule.iOS_qq_universal_link);
        }
    }

    private static void CombinIOSWX(SDKConfigModule gdevSdkConfigModule,
        SDKConfigModule originSdkConfigModule)
    {
        if (originSdkConfigModule.shareConfigModule.iOS_wx_available &&
            !gdevSdkConfigModule.shareConfigModule.iOS_wx_available)
        {
            return;
        }

        originSdkConfigModule.shareConfigModule.iOS_wx_available =
            gdevSdkConfigModule.shareConfigModule.iOS_wx_available;
        if (gdevSdkConfigModule.shareConfigModule.iOS_wx_available)
        {
            originSdkConfigModule.shareConfigModule.iOS_wx_key = CombinParamManager.GetCombineValue(
                originSdkConfigModule.shareConfigModule.iOS_wx_key, gdevSdkConfigModule.shareConfigModule.iOS_wx_key);
            originSdkConfigModule.shareConfigModule.iOS_wx_universal_link = CombinParamManager.GetCombineValue(
                originSdkConfigModule.shareConfigModule.iOS_wx_universal_link,
                gdevSdkConfigModule.shareConfigModule.iOS_wx_universal_link);
        }
    }

    private static void CombinIOSWB(SDKConfigModule gdevSdkConfigModule,
        SDKConfigModule originSdkConfigModule)
    {
        if (originSdkConfigModule.shareConfigModule.iOS_weibo_available &&
            !gdevSdkConfigModule.shareConfigModule.iOS_weibo_available)
        {
            return;
        }

        originSdkConfigModule.shareConfigModule.iOS_weibo_available =
            gdevSdkConfigModule.shareConfigModule.iOS_weibo_available;
        if (gdevSdkConfigModule.shareConfigModule.iOS_weibo_available)
        {
            originSdkConfigModule.shareConfigModule.iOS_weibo_key = CombinParamManager.GetCombineValue(
                originSdkConfigModule.shareConfigModule.iOS_weibo_key,
                gdevSdkConfigModule.shareConfigModule.iOS_weibo_key);
            originSdkConfigModule.shareConfigModule.iOS_weibo_universal_link = CombinParamManager.GetCombineValue(
                originSdkConfigModule.shareConfigModule.iOS_weibo_universal_link,
                gdevSdkConfigModule.shareConfigModule.iOS_weibo_universal_link);
        }
    }

    private static void CombinIOSAwe(SDKConfigModule gdevSdkConfigModule,
        SDKConfigModule originSdkConfigModule)
    {
        if (originSdkConfigModule.shareConfigModule.iOS_awe_available &&
            !gdevSdkConfigModule.shareConfigModule.iOS_awe_available)
        {
            return;
        }

        originSdkConfigModule.shareConfigModule.iOS_awe_available =
            gdevSdkConfigModule.shareConfigModule.iOS_awe_available;
        if (gdevSdkConfigModule.shareConfigModule.iOS_awe_available)
        {
            originSdkConfigModule.shareConfigModule.iOS_awe_key = CombinParamManager.GetCombineValue(
                originSdkConfigModule.shareConfigModule.iOS_awe_key, gdevSdkConfigModule.shareConfigModule.iOS_awe_key);
        }
    }
}