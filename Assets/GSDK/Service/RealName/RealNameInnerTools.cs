namespace GSDK
{
    public class RealNameInnerTools
    {
        public static RealNameState Convert(GMSDK.VerifiedResult ret)
        {
            return new RealNameState()
            {
                IsVerified = ret.isVerified,
                NeedParentVerify = ret.needParentVerify
            };
        }

        public static RealNameInfo Convert(GMSDK.RealNameResultRet ret)
        {
            return new RealNameInfo()
            {
                IsVerified = ret.hasRealName,
                Age = ret.age,
                IdentityNumber = ret.identityNumber,
                RealName = ret.realName
            };
        }
    }
}