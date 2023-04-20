namespace GSDK
{
    public class PushInnerTools
    {
        public static Result ConvertPushError(GMSDK.LocalPushResult result)
        {
            if (result.code == 0)
            {
                return new Result(0, "Success", 0, "", "");
            }
            
            return new Result(result.code, result.message, result.extraErrorCode, result.extraErrorMessage, result.additionalInfo);
        }
    }
}