using System.Collections.Generic;
using GMSDK;

namespace GSDK
{
    public static class CdKeyInnerTools
    {
        public static Result ConvertCdKeyError(CallbackResult result)
        {
            return new Result(result.code, result.message, result.extraErrorCode, result.extraErrorMessage,result.additionalInfo);
        }
    }
}