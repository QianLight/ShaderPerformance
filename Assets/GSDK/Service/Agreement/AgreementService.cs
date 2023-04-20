using System;
using System.Text;

namespace GSDK
{
    public class AgreementService : IAgreementService
    {
        public int Initialize(byte[] configurationData, uint configurationLength,
            AgreementCryptoType agreementCryptoMethod)
        {
            var code = Client.TpsClientSdkInit(configurationData, configurationLength,
                (Client.CRYPTOTYPE) agreementCryptoMethod);
            return AgreementInnerTools.ConvertAgreementError(code);
        }

        public void Release()
        {
            Client.TpsClientSdkClose();
        }

        public int Encrypt(byte[] inputData, uint inputLength, byte[] outputBuffer, ref uint outputBufferMaxSizes)
        {
            var code = Client.TpsClientSdkEncrypt(inputData, inputLength, outputBuffer, ref outputBufferMaxSizes);
            return AgreementInnerTools.ConvertAgreementError(code);
        }

        public int Decrypt(byte[] inputData, uint inputLength, byte[] outputBuffer, ref uint outputBufferMaxSizes)
        {
            var code = Client.TpsClientSdkDecrypt(inputData, inputLength, outputBuffer, ref outputBufferMaxSizes);
            return AgreementInnerTools.ConvertAgreementError(code);
        }

        public string GetAgreementVersion()
        {
            var buffer = new byte[512];
            Array.Clear(buffer, 0, buffer.Length);
            var bufferSizes = (uint) buffer.Length;
            Client.TpsClientSdkVersion(buffer, bufferSizes);
            var version = Encoding.UTF8.GetString(buffer);
            return version;
        }
    }
}