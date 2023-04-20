using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using GMSDK;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public partial class PayService : IPayService
    {
        private GMSDK.MainSDK gsdk = GMSDKMgr.instance.SDK;
        
        public void RequestProduct(string serverID, string roleID, bool doLocalize, PayRequestProductDelegate payRequestProductCallback)
        {
#if UNITY_STANDALONE_WIN && !GMEnderOn
            RequestProductCallback = payRequestProductCallback;
            RequestProduct(serverID, roleID, ReuqestProductFunc);
#else
            string openID = "";
            IAccountService accountService =
            ServiceProvider.Instance.GetService(ServiceType.Account,"Bytedance") as IAccountService;;
            if (accountService != null)
            {
                openID = accountService.GetLoginRecord().OpenID;
            }
            if (string.IsNullOrEmpty(openID))
            {
                OnRequestProductCallback(
                    new Result(ErrorCode.PayServerFailed, "open id is null or empty!"),
                    null, 
                    null, 
                    payRequestProductCallback
                );
            }
            else
            {
                gsdk.SdkRequestProduct(openID, serverID, roleID, (GMSDK.PayRequestProductResult payRequestProductResult) =>
                {
                    OnRequestProductCallback(payRequestProductResult, payRequestProductCallback);
                });
            }
#endif
        }

        private void OnRequestProductCallback(PayRequestProductResult payRequestProductResult, PayRequestProductDelegate payRequestProductCallback)
        {
            if (payRequestProductResult == null || payRequestProductResult.product == null)
            {
                OnRequestProductCallback(
                    InnerTools.ConvertToResult(payRequestProductResult),
                    new List<Product>(), 
                    new List<ProductAccumulation>(), 
                    payRequestProductCallback);                
            }
            else
            {
                OnRequestProductCallback(
                    InnerTools.ConvertToResult(payRequestProductResult),
                    PayInnerTools.Convert(payRequestProductResult.product.products),
                    PayInnerTools.Convert(payRequestProductResult.product.accumulations),
                    payRequestProductCallback);
            }
        }

        void OnRequestProductCallback(Result result, List<Product> products, List<ProductAccumulation> accumulations, PayRequestProductDelegate payRequestProductCallback)
        {
            if (payRequestProductCallback != null)
            {
                try
                {
                    payRequestProductCallback(result, products, accumulations);
                }
                catch (Exception ex)
                {
                    GLog.LogException(ex);
                }
            }
        }


        
        public void Pay(string productID, GoodsType goodsType, string serverID, RoleInfoForPay roleInfo, string extraInfo, PayPaymentDelegate payCallback)
        {
#if UNITY_STANDALONE_WIN && !GMEnderOn
            PayCallback = payCallback;
            Pay( productID,  (int)goodsType,  serverID,  roleInfo.RoleID, roleInfo.RoleName,  roleInfo.RoleLevel, roleInfo.RoleVipLevel.ToString() , extraInfo, PayFunc);
#else            
            string openID = "";
            IAccountService accountService = ServiceProvider.Instance.GetService(ServiceType.Account,"Bytedance") as IAccountService;
            if (accountService != null)
            {
                openID = accountService.GetLoginRecord().OpenID;
            }
            
            if (string.IsNullOrEmpty(openID))
            {
                OnPayCallback(new Result(ErrorCode.PayServerFailed, "open id is null or empty!"),
                    null, null, payCallback);
            }
            else
            {
                // 对齐：
                // Native客户端：goodsType客户端有作用，其他部分透传
                // 服务端：currency 和 activity 已废弃
                gsdk.SdkPay(openID, productID, serverID, "", roleInfo.RoleID, roleInfo.RoleName, roleInfo.RoleLevel, roleInfo.RoleVipLevel, 
                    extraInfo, "", (int)goodsType,(GMSDK.PayBuyProductResult payBuyProductResult) =>
                    {
                        OnPayCallback(InnerTools.ConvertToResult(payBuyProductResult),
                            payBuyProductResult.orderId, payBuyProductResult.productId, payCallback);
                    });
            }
#endif
        }

#if UNITY_IOS
        public void EnableShowErrorToast(bool showErrorTip)
        {
            gsdk.SdkConfigShowErrorTip(showErrorTip);
        }
#endif

        void OnPayCallback(Result result, string orderID, string productID, PayPaymentDelegate payCallback)
        {
            if (payCallback != null)
            {
                try
                {
                    payCallback(result, orderID, productID);
                }
                catch (Exception ex)
                {
                    GLog.LogException(ex);
                }
            }
        }
        
#if UNITY_STANDALONE_WIN && !GMEnderOn
        
        public PayRequestProductDelegate RequestProductCallback;
        
        [MonoPInvokeCallback(typeof(Callback2P))]
        private static void ReuqestProductFunc(string resultStr, string callbackStr)
        {
                
            GLog.LogDebug("resultStr:" + resultStr + ",callbackStr:" + callbackStr);
            Result result = JsonMapper.ToObject<Result>(resultStr);
            List<Product> products = new List<Product>();
            JsonData jsonData = JsonMapper.ToObject(callbackStr);
            for (int i = 0; i < jsonData.Count; i++)
            {
                string productStr = JsonMapper.ToJson(jsonData[i]);
                GLog.LogDebug("productStr " + productStr);
                try
                {
                    Product product = JsonMapper.ToObject<Product>(productStr);
                    products.Add(product);
                }
                catch (Exception e)
                {
                    GLog.LogError("parser product " + e.Message);
                }

                    
            }

            PayService payService = GSDK.Pay.Service as PayService;
            if (payService.RequestProductCallback != null)
            {
                payService.RequestProductCallback(result, products, null);
            }
        }
        
        public PayPaymentDelegate PayCallback;
        
        [MonoPInvokeCallback(typeof(Callback3P))]
        private static void PayFunc(string resultStr, string orderID, string productIDStr)
        {
            GLog.LogDebug("resultStr:" + resultStr + ",orderID:" + orderID + "productID:" + productIDStr);
            Result result = JsonMapper.ToObject<Result>(resultStr);
            PayService payService = GSDK.Pay.Service as PayService;
            if (payService.PayCallback != null)
            {
                payService.PayCallback(result, orderID, productIDStr);
            }
        }
        
        [DllImport(PluginName.GSDK)]
        private static extern void RequestProduct(string serverID, string roleID, Callback2P callback);
            
        [DllImport(PluginName.GSDK)]
        private static extern void Pay(string productID, int goodsType, string serverID, string roleID, string roleName, string roleLevel, string roleVipLevel, string extraInfo, Callback3P callback);
#endif        
        

    }
}