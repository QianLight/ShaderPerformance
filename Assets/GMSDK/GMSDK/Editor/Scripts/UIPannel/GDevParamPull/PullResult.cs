public class PullResult
{
    public static int PullError = -1;
    public int errorCode; //错误码
    public string errorMsg; //错误信息
    public string paramsString; //Gdev拉取内容

    public PullResult(int errorCode, string errorMsg, string paramsString)
    {
        this.errorCode = errorCode;
        this.errorMsg = errorMsg;
        this.paramsString = paramsString;
    }
}