using System;
using System.Reflection;
using UNBridgeLib.LitJson;
using UNBridgeLib;
using GSDK;

namespace GSDK
{
    public class QRCodeCallbackHandler : BridgeCallBack
    {
        public ScanQRCodeDelegate ScanQRCodeCallback;
        private QRCodeType Type;
        
        public QRCodeCallbackHandler(QRCodeType codeType)
        {
            Type = codeType;
            this.OnFailed = new OnFailedDelegate(OnFailCallBack);
            this.OnTimeout = new OnTimeoutDelegate(OnTimeoutCallBack);
        }

        public void OnScanQRCodeCallback(JsonData jsonData)
        {
            LogUtils.D("OnScanQRCodeResult");
            ScanResult scanResult = QRCodeInnerTools.ConvertToScanResult(jsonData);
            var result = new Result(scanResult.code, scanResult.message);
            // 扫码授权PC登录
            if (Type == QRCodeType.AuthLogin && result.IsSuccess)
            {
                IService accountService = ServiceProvider.Instance.GetService(ServiceType.Account,"Bytedance");
                if (accountService != null)
                {
                    MethodInfo qrCodeAuthLoginMethod = accountService.GetType().GetMethod("QRCodeAuthLogin");
                    if (qrCodeAuthLoginMethod != null)
                    {
                        string token = scanResult.result;
                        qrCodeAuthLoginMethod.Invoke(accountService,
                            new object[]
                            {
                                token,
                                Delegate.CreateDelegate(qrCodeAuthLoginMethod.GetParameters()[1].ParameterType, this,
                                    "AuthLoginCallback")
                            });
                    }
                }
                return;
            }
            // 默认返回识别结果
            ScanQRCodeCallback.Invoke(result, scanResult.result);
        }

        private void AuthLoginCallback(Result result, int status)
        {
            ScanQRCodeCallback.Invoke(result, status.ToString());
        }
        
        /// <summary>
        /// 失败统一回调，用于调试接口
        /// </summary>
        public void OnFailCallBack(int code, string failMsg)
        {
            LogUtils.D("接口访问失败 " + code.ToString() + " " + failMsg);
        }
        /// <summary>
        /// 超时统一回调
        /// </summary>
        public void OnTimeoutCallBack()
        {
            JsonData jd = new JsonData();
            jd["code"] = -321;
            jd["message"] = "QRCode - request time out";
            if (this.OnSuccess != null)
            {
                this.OnSuccess(jd);
            }
        }
    }
}