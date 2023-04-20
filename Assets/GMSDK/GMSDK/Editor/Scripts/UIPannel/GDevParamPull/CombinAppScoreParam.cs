public class CombinAppScoreParam
{
    public static void ParamCombin(SDKConfigModule gdevSdkConfigModule,
        SDKConfigModule originSdkConfigModule)
    {
        originSdkConfigModule.ratingConfigModule.iOS_rating_app_store_id = CombinParamManager.GetCombineValue(
            originSdkConfigModule.ratingConfigModule.iOS_rating_app_store_id,
            gdevSdkConfigModule.ratingConfigModule.iOS_rating_app_store_id);
    }
}