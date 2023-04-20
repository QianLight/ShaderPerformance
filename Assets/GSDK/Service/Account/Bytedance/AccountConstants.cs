namespace GSDK
{
    public class AccountConstants
    {
        #region Method Name

        public const string MethodFetchLinkInfo = "queryGsdkAccountLinkAuth";
        public const string MethodLinkAuth = "gsdkAccountLinkAuth";
        public const string MethodLinkRelease = "gsdkAccountReleaseAuth";
        public const string MethodGetDefaultScopeInfo = "getDefaultAuthPermissionInfo";

        #endregion

        #region Event Name

        public const string EventLinkAuthResult = "gsdkAccountLinkAuthResult";

        #endregion
    }
}