namespace GMSDK
{
    public class CallbackResult
    {
        // 一级错误码
        public int code;

        // 一级错误信息
        public string message;

        // 二级错误码
        public readonly int extraErrorCode;

        // 二级错误信息
        public readonly string extraErrorMessage;

        // 额外错误信息
        public readonly string additionalInfo;

        public CallbackResult()
        {
        }

        public CallbackResult(int code, string message, int extraErrorCode, string extraErrorMessage)
        {
            this.code = code;
            this.message = message;
            this.extraErrorCode = extraErrorCode;
            this.extraErrorMessage = extraErrorMessage;
            additionalInfo = "";
        }

        public CallbackResult(int code, string message, int extraErrorCode, string extraErrorMessage,
            string additionalInfo)
        {
            this.code = code;
            this.message = message;
            this.extraErrorCode = extraErrorCode;
            this.extraErrorMessage = extraErrorMessage;
            this.additionalInfo = additionalInfo;
        }

        public bool IsSuccess()
        {
            return code == 0;
        }

        public override string ToString()
        {
            return string.Format(
                "GSDKError[ code = {0}, message = {1}, extraErrorCode = {2}, extraErrorMessage = {3}, addtionalInfo = {4}]",
                code, message, extraErrorCode, extraErrorMessage, additionalInfo);
        }
    }
}