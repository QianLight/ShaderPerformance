
namespace GSDK
{
    public class QRCodeMethodName
    {
        public const string Init = "registerQRCode";
        public const string ScanQRCodeMethodName = "requestScanQRCode";
    }

    public class QRCodeEventName
    {
        public const string ScanResult = "scanQRCodeResult";
    }
    
    public class QRCodeService : IQRCodeService
    {

        public QRCodeService()
        {
#if UNITY_ANDROID
            UNBridge.Call(QRCodeMethodName.Init, null);
#endif
        }
        public void Scan(QRCodeType type, ScanQRCodeDelegate callback)
        {
            var qrCodeCallbackHandler = new QRCodeCallbackHandler(type)
            {
                ScanQRCodeCallback = callback
            };
            qrCodeCallbackHandler.OnSuccess = qrCodeCallbackHandler.OnScanQRCodeCallback;
            UNBridge.Listen(QRCodeEventName.ScanResult, qrCodeCallbackHandler);
            UNBridge.Call(QRCodeMethodName.ScanQRCodeMethodName);
        }
    }
}