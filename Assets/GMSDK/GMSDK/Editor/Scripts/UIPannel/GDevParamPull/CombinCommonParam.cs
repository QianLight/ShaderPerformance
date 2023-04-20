/**
 * 合并通用模块参数
 */
public class CombinCommonParam
{
    public static void ParamCombin(SDKConfigModule gdevSdkConfigModule, SDKConfigModule originSdkConfigModule)
    {
        originSdkConfigModule.commonModule.app_id =
            CombinParamManager.GetCombineValue(originSdkConfigModule.commonModule.app_id,
                gdevSdkConfigModule.commonModule.app_id);

        originSdkConfigModule.commonModule.package_name = CombinParamManager.GetCombineValue(
            originSdkConfigModule.commonModule.package_name,
            gdevSdkConfigModule.commonModule.package_name);

        originSdkConfigModule.commonModule.iOS_bundleId = CombinParamManager.GetCombineValue(
            originSdkConfigModule.commonModule.iOS_bundleId,
            gdevSdkConfigModule.commonModule.iOS_bundleId);

        originSdkConfigModule.commonModule.iOS_app_display_name = CombinParamManager.GetCombineValue(
            originSdkConfigModule.commonModule.iOS_app_display_name,
            gdevSdkConfigModule.commonModule.iOS_app_display_name);

        originSdkConfigModule.commonModule.app_name = CombinParamManager.GetCombineValue(
            originSdkConfigModule.commonModule.app_name,
            gdevSdkConfigModule.commonModule.app_name);
    }
}