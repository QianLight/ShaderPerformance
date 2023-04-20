using GMSDK;

namespace GSDK
{
    public class SystemInnerTools
    {
        public static Result ConvertSystemError(CallbackResult callbackResult)
        {
            var result = InnerTools.ConvertToResult(callbackResult);

            if (callbackResult.IsSuccess())
            {
                return result;
            }

            result.Error = ErrorCode.SystemUnknownError;
            switch (callbackResult.code)
            {
                case -1001:
                    result.Error = ErrorCode.SystemServerParameterError;
                    break;
                case -5000:
                    result.Error = ErrorCode.SystemServerExceptionError;
                    break;
                case -1010:
                    result.Error = ErrorCode.SystemFetchError;
                    break;
                case -3000:
                    result.Error = ErrorCode.SystemNetworkAnomaliesError;
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}