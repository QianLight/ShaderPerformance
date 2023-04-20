namespace GSDK
{
    public class AgreementInnerTools
    {
        public static int ConvertAgreementError(int nativeCode)
        {
            if (nativeCode == 0)
            {
                return ErrorCode.Success;
            }

            int code = ErrorCode.AgreementErrorUnknown;
            switch (nativeCode)
            {
                case -1:
                    code = ErrorCode.AgreementErrorUnknown;
                    break;
                case 100:
                    code = ErrorCode.AgreementErrorParameter;
                    break;
                case 101:
                    code = ErrorCode.AgreementErrorInitializeCore;
                    break;
                case 102:
                    code = ErrorCode.AgreementErrorInitializeCfgSign;
                    break;
                case 103:
                    code = ErrorCode.AgreementErrorInitializeCfgType;
                    break;
                case 104:
                    code = ErrorCode.AgreementErrorNotInitialize;
                    break;
                case 105:
                    code = ErrorCode.AgreementErrorIncorrectMethod;
                    break;
                case 106:
                    code = ErrorCode.AgreementErrorDecryptSdkType;
                    break;
                case 107:
                    code = ErrorCode.AgreementErrorDecryptIncorrectVersion;
                    break;
                case 108:
                    code = ErrorCode.AgreementErrorDecryptUnrecognized;
                    break;
                case 109:
                    code = ErrorCode.AgreementErrorDecryptChecksum;
                    break;
                case 110:
                    code = ErrorCode.AgreementErrorDecryptFailure;
                    break;
                case 111:
                    code = ErrorCode.AgreementErrorDecryptFailFix;
                    break;
                case 112:
                    code = ErrorCode.AgreementErrorDecryptFailLength;
                    break;
                case 113:
                    code = ErrorCode.AgreementErrorOutputBufferNotEnough;
                    break;
                case 114:
                    code = ErrorCode.AgreementErrorEncryptFailure;
                    break;
                case 115:
                    code = ErrorCode.AgreementErrorInitializeCfgDh;
                    break;
                case 116:
                    code = ErrorCode.AgreementErrorCryptoType;
                    break;
                case 117:
                    code = ErrorCode.AgreementErrorAllocTempleBuffer;
                    break;
            }

            return code;
        }
    }
}