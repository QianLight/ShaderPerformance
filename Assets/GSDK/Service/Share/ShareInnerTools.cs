using System.Collections.Generic;
using GMSDK;

namespace GSDK
{
    public static class ShareInnerTools
    {
        public static ShareSilentlyStatus ConvertShareSilentlyStatus(GMShareBehindStatus status)
        {
            switch (status)
            {
                case GMShareBehindStatus.BeforeDouyinAuthRenew:
                    return ShareSilentlyStatus.BeforeDouyinAuthRenew;
                case GMShareBehindStatus.BeforeFormalShare:
                    return ShareSilentlyStatus.BeforeFormalShare;
                default:
                    return ShareSilentlyStatus.BeforeFormalShare; 
            }
        }
        
        public static Result ConvertShareError(GMSDK.CallbackResult result)
        {
            return new Result(result.code, result.extraErrorCode, 0, result.message, result.extraErrorMessage,
                result.additionalInfo);
        }
    }
}