namespace GSDK
{
    /// <summary>
    /// 用于获取实例进行接口调用
    ///
    /// 注意：协议加密模块提供的接口为耗时操作，建议在子线程调用
    /// e.g.Agreement.Service.MethodName();
    /// </summary>
    public static class Agreement
    {
        public static IAgreementService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.Agreement) as IAgreementService; }
        }
    }

    /// <summary>
    /// 协议加密模块提供的接口为耗时操作，建议在子线程调用
    /// </summary>
    public interface IAgreementService : IService
    {
        /// <summary>
        /// 初始化
        ///
        /// 注意：
        /// 1 初始化成功过，若再次调用会忽略，要再次初始化需要先释放SDK资源；失败则可尝试调整参数重新初始化
        /// 2 加密算法类型选择，只对加密过程有效；解密时能能自动获取到加密时使用的算法；客户端和服务端算法可以不一样
        /// </summary>
        /// <param name="configurationData">sdk配置内容缓冲区地址</param>
        /// <param name="configurationLength">sdk配置内容缓冲区内容长度</param>
        /// <param name="agreementCryptoMethod">选择支持的加密算法</param>
        /// <returns>
        /// 返回调用的结果
        /// <para>
        /// 可能返回的错误码：
        /// Success：成功
        /// AgreementErrorInitializeCore：初始化内核接口失败(动态库未正确加载)
        /// AgreementErrorInitializeCfgSign：初始化配置信息失败(验签)
        /// AgreementErrorInitializeCfgType：初始化配置信息与sdk类型不匹配(客户端及服务端配置要一一对应)
        /// </para>
        /// </returns>
        int Initialize(byte[] configurationData, uint configurationLength, AgreementCryptoType agreementCryptoMethod);

        /// <summary>
        /// 释放SDK资源(非线程安全)
        /// </summary>
        void Release();

        /// <summary>
        /// 加密数据(线程安全)
        ///
        /// 注意：当输出输出缓冲区相同时，完成加密后输出数据长度较输出增加约64个字节，会覆盖输入数据内存区域，若后续还有数据需要继续加密则会被污染
        /// </summary>
        /// <param name="inputData">待加密数据缓冲区地址</param>
        /// <param name="inputLength">待加密数据长度</param>
        /// <param name="outputBuffer">加密后数据输出缓冲区地址，当输入缓冲区为空，outBufMaxSizes会返回输出缓冲区需要的最大长度</param>
        /// <param name="outputBufferMaxSizes">输入时是指向加密输出缓冲区最大长度值的地址，输出时返回加密后的输出缓冲区中的数据长度</param>
        /// <returns>
        /// 返回调用的结果
        /// <para>
        /// 可能返回的错误码：
        /// Success：成功
        /// AgreementErrorEncryptFailure：加密数据失败
        /// AgreementErrorOutputBufferNotEnough：输出缓冲区长度不够
        /// AgreementErrorCryptoType：数据包不合法
        /// AgreementErrorAllocTempleBuffer：无法分配临时缓冲区
        /// AgreementErrorNotInitialize：请调用初始化接口
        /// </para>
        /// </returns>
        int Encrypt(byte[] inputData, uint inputLength, byte[] outputBuffer, ref uint outputBufferMaxSizes);

        /// <summary>
        /// 解密数据(线程安全)
        ///
        /// 当输出输出缓冲区相同时，解密后数据比输入数据长度短，不会覆盖输入数据内存区域，是安全的
        /// </summary>
        /// <param name="inputData">待解密数据缓冲区地址</param>
        /// <param name="inputLength">待解密数据长度</param>
        /// <param name="outputBuffer">解密后数据输出缓冲区地址</param>
        /// <param name="outputBufferMaxSizes">输入时是指向解密输出缓冲区最大长度值的地址，输出时返回解密后的输出缓冲区中的数据长度</param>
        /// <returns>
        /// 返回调用的结果
        /// <para>
        /// 可能返回的错误码：
        /// Success：成功
        /// AgreementErrorDecryptSdkType：解密的sdk类型不匹配(客户端加密数据需要服务端来解密，反之亦然)
        /// AgreementErrorDecryptIncorrectVersion：不正确的协议版本(不支持或非法版本)
        /// AgreementErrorDecryptUnrecognized：无法识别数据内容
        /// AgreementErrorDecryptChecksum：解密数据后校验失败
        /// AgreementErrorDecryptFailure：解密数据失败
        /// AgreementErrorDecryptFailFix：解密数据后修复过程出错
        /// AgreementErrorDecryptFailLength：解密数据后数据长度不正确
        /// AgreementErrorOutputBufferNotEnough：输出缓冲区长度不够
        /// AgreementErrorCryptoType：数据包不合法
        /// AgreementErrorAllocTempleBuffer：无法分配临时缓冲区
        /// AgreementErrorNotInitialize：请调用初始化接口
        /// </para>
        /// </returns>
        int Decrypt(byte[] inputData, uint inputLength, byte[] outputBuffer, ref uint outputBufferMaxSizes);

        /// <summary>
        /// 获取当前SDK版本号(线程安全)
        /// </summary>
        /// <returns>返回Agreement SDK版本</returns>
        string GetAgreementVersion();
    }

    /// <summary>
    /// 加密算法
    /// </summary>
    public enum AgreementCryptoType
    {
        /// <summary>
        /// Rivest Cipher 4
        /// </summary>
        RC4 = 1,
        
        /// <summary>
        /// AES-CBC-128
        /// </summary>
        AesCbc128 = 2,
        
        /// <summary>
        /// AES-CBC-192
        /// </summary>
        AesCbc192 = 3,
        
        /// <summary>
        /// AES-CBC-256
        /// </summary>
        AesCbc256 = 4,
    }

    public static partial class ErrorCode
    {
        ///<summary>
        ///发生未知错误
        ///</summary>
        public const int AgreementErrorUnknown = -270001;

        ///<summary>
        ///传入参数不正确
        ///</summary>
        public const int AgreementErrorParameter = -270100;

        ///<summary>
        ///初始化内核接口失败(动态库未正确加载)
        ///</summary>
        public const int AgreementErrorInitializeCore = -270101;

        ///<summary>
        ///初始化配置信息失败(验签)
        ///</summary>
        public const int AgreementErrorInitializeCfgSign = -270102;

        ///<summary>
        ///初始化配置信息与sdk类型不匹配(客户端及服务端配置要一一对应)
        ///</summary>
        public const int AgreementErrorInitializeCfgType = -270103;

        ///<summary>
        ///Sdk没有完成初始化
        ///</summary>
        public const int AgreementErrorNotInitialize = -270104;

        ///<summary>
        ///不正确的加密算法类型(不支持或非法类型)
        ///</summary>
        public const int AgreementErrorIncorrectMethod = -270105;

        ///<summary>
        ///解密的sdk类型不匹配(客户端加密数据需要服务端来解密，反之亦然)
        ///</summary>
        public const int AgreementErrorDecryptSdkType = -270106;

        ///<summary>
        ///不正确的协议版本(不支持或非法版本)
        ///</summary>
        public const int AgreementErrorDecryptIncorrectVersion = -270107;

        ///<summary>
        ///无法识别数据内容
        ///</summary>
        public const int AgreementErrorDecryptUnrecognized = -270108;

        ///<summary>
        ///解密数据后校验失败
        ///</summary>
        public const int AgreementErrorDecryptChecksum = -270109;

        ///<summary>
        ///解密数据失败
        ///</summary>
        public const int AgreementErrorDecryptFailure = -270110;

        ///<summary>
        ///解密数据后修复过程出错
        ///</summary>
        public const int AgreementErrorDecryptFailFix = -270111;

        ///<summary>
        ///解密数据后数据长度不正确
        ///</summary>
        public const int AgreementErrorDecryptFailLength = -270112;

        ///<summary>
        ///输出缓冲区长度不够
        ///</summary>
        public const int AgreementErrorOutputBufferNotEnough = -270113;

        ///<summary>
        ///加密数据失败
        ///</summary>
        public const int AgreementErrorEncryptFailure = -270114;

        ///<summary>
        ///初始化配置信息失败(交换密钥)
        ///</summary>
        public const int AgreementErrorInitializeCfgDh = -270115;

        ///<summary>
        ///数据包不合法
        ///</summary>
        public const int AgreementErrorCryptoType = -270116;

        ///<summary>
        ///无法分配临时缓冲区
        ///</summary>
        public const int AgreementErrorAllocTempleBuffer = -270117;
    }
}