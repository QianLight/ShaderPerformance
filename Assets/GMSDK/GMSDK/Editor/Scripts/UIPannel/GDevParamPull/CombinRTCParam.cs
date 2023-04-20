public class CombinRTCParam
{
    public static void ParamCombin(SDKConfigModule gdevSdkConfigModule,
        SDKConfigModule originSdkConfigModule)
    {
        if (originSdkConfigModule.rtcConfigModule.rtc_available && !gdevSdkConfigModule.rtcConfigModule.rtc_available)
        {
            return;
        }

        originSdkConfigModule.rtcConfigModule.rtc_available = gdevSdkConfigModule.rtcConfigModule.rtc_available;
        if (gdevSdkConfigModule.rtcConfigModule.rtc_available)
        {
            originSdkConfigModule.rtcConfigModule.rtc_app_id = CombinParamManager.GetCombineValue(
                originSdkConfigModule.rtcConfigModule.rtc_app_id, gdevSdkConfigModule.rtcConfigModule.rtc_app_id);
        }
    }
}