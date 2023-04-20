using System.Collections.Generic;
using CFUtilPoolLib;

/// <summary>
/// GSDK接口——支付接口
/// </summary>
public partial class GSDKSystem
{
    private CFUtilPoolLib.GSDK.PayPaymentDelegate m_payCallback;
    private CFUtilPoolLib.GSDK.PayRequestProductDelegate m_payRequestProductCallback;


    #region 支付接口

    public void Pay(string productID, CFUtilPoolLib.GSDK.GoodsType goodsType, string serverID, CFUtilPoolLib.GSDK.RoleInfoForPay roleInfo, string extraInfo, CFUtilPoolLib.GSDK.PayPaymentDelegate payCallback)
    {
        m_payCallback = payCallback;
        GSDK.RoleInfoForPay gRoleInfo = GetRoleInfoForPay(roleInfo);
        GSDK.Pay.Service.Pay(productID, (GSDK.GoodsType)goodsType, serverID, gRoleInfo, extraInfo, PayPaymentCallback);
        XDebug.singleton.AddGreenLog("GSDKSystem Pay Call productID=" + productID + ",goodsType=" + goodsType + ",serverID= " + serverID + ",roleID=" + roleInfo.RoleID);
    }

    public void RequestProduct(string serverID, string roleID, bool doLocalize, CFUtilPoolLib.GSDK.PayRequestProductDelegate payRequestProductCallback)
    {
        m_payRequestProductCallback = payRequestProductCallback;
        GSDK.Pay.Service.RequestProduct(serverID, roleID, doLocalize, PayRequestProductCallback);
        XDebug.singleton.AddGreenLog("GSDKSystem RequestProduct Call serverID=" + serverID + ",roleID=" + roleID + ",doLocalize= " + doLocalize);
    }

    public GSDK.RoleInfoForPay GetRoleInfoForPay(CFUtilPoolLib.GSDK.RoleInfoForPay roleInfo)
    {
        GSDK.RoleInfoForPay gRoleInfo = new GSDK.RoleInfoForPay();
        if (roleInfo != null)
        {
            gRoleInfo.RoleID = roleInfo.RoleID;
            gRoleInfo.RoleName = roleInfo.RoleName;
            gRoleInfo.RoleLevel = roleInfo.RoleLevel;
            gRoleInfo.RoleVipLevel = roleInfo.RoleVipLevel;
        }
        return gRoleInfo;
    }

    public void PayPaymentCallback(GSDK.Result result, string orderID, string productID)
    {
        if (m_payCallback != null)
        {
            CFUtilPoolLib.GSDK.Result gResult = GetGSDKResult(result);
            m_payCallback(gResult, orderID, productID);
            XDebug.singleton.AddGreenLog("PayPaymentCallback orderID=" + orderID + ",productID=" + productID + ",result=" + result.ToDetailedString());
        }
    }

    private CFUtilPoolLib.GSDK.ProductActivity GetGSDKProductActivity(GSDK.ProductActivity productActivity)
    {
        CFUtilPoolLib.GSDK.ProductActivity gProductActivity = new CFUtilPoolLib.GSDK.ProductActivity();
        if (productActivity != null)
        {
            gProductActivity.ActivityID = productActivity.ActivityID;
            gProductActivity.ActivityType = new CFUtilPoolLib.GSDK.ActivityType(productActivity.ActivityType.GetValue());
            gProductActivity.GiftType = new CFUtilPoolLib.GSDK.GiftType(productActivity.GiftType.GetValue());
            gProductActivity.StartTime = productActivity.StartTime;
            gProductActivity.EndTime = productActivity.EndTime;
            gProductActivity.Version = productActivity.Version;
            gProductActivity.Active = productActivity.Active;
            gProductActivity.CoinNumber = productActivity.CoinNumber;
            gProductActivity.ItemID = productActivity.ItemID;
            gProductActivity.ItemNumber = productActivity.ItemNumber;
            gProductActivity.Desc = productActivity.Desc;
        }
        return gProductActivity;
    }

    private List<CFUtilPoolLib.GSDK.Product> GetGSDKProducts(List<GSDK.Product> products)
    {
        List<CFUtilPoolLib.GSDK.Product> gProducts = new List<CFUtilPoolLib.GSDK.Product>();
        if (products != null)
        {
            for (int i = 0; i < products.Count; ++i)
            {
                GSDK.Product p = products[i];
                if (p == null) continue;
                CFUtilPoolLib.GSDK.Product gProduct = new CFUtilPoolLib.GSDK.Product();
                gProduct.AppID = p.AppID;
                gProduct.ProductID = p.ProductID;
                gProduct.ProductName = p.ProductName;
                gProduct.Price = p.Price;
                gProduct.PriceDesc = p.PriceDesc;
                gProduct.CurrencyType = p.CurrencyType;
                gProduct.Status = (CFUtilPoolLib.GSDK.ProductStatus)p.Status;
                gProduct.CreateTime = p.CreateTime;
                gProduct.UpdateTime = p.UpdateTime;
                gProduct.GoodsType = (CFUtilPoolLib.GSDK.GoodsType)p.GoodsType;
                gProduct.ExtraDiscountInfo = p.ExtraDiscountInfo;
                gProduct.Activities = new List<CFUtilPoolLib.GSDK.ProductActivity>();
                if (p.Activities != null)
                {
                    for (int j = 0; j < p.Activities.Count; ++j)
                    {
                        if (p.Activities[j] == null) continue;
                        CFUtilPoolLib.GSDK.ProductActivity gProductActivity = GetGSDKProductActivity(p.Activities[j]);
                        gProduct.Activities.Add(gProductActivity);
                    }
                }
                gProduct.ActivityJSONStringForCreateOrder = p.ActivityJSONStringForCreateOrder;
                gProducts.Add(gProduct);
            }
        }
        return gProducts;
    }

    private CFUtilPoolLib.GSDK.ProductAccumulationDetail GetGSDKProductAccumulationDetail(GSDK.ProductAccumulationDetail detail)
    {
        CFUtilPoolLib.GSDK.ProductAccumulationDetail gDetail = new CFUtilPoolLib.GSDK.ProductAccumulationDetail();
        gDetail.CoinNumber = detail.CoinNumber;
        gDetail.ItemID = detail.ItemID;
        gDetail.ItemNumber = detail.ItemNumber;
        gDetail.LevelNumber = detail.LevelNumber;
        return gDetail;
    }

    private List<CFUtilPoolLib.GSDK.ProductAccumulation> GetGSDKProductAccumulations(List<GSDK.ProductAccumulation> accumulations)
    {
        List<CFUtilPoolLib.GSDK.ProductAccumulation> gProductAccumulations = new List<CFUtilPoolLib.GSDK.ProductAccumulation>();
        if (accumulations != null)
        {
            for (int i = 0; i < accumulations.Count; ++i)
            {
                GSDK.ProductAccumulation p = accumulations[i];
                if (p == null) continue;
                CFUtilPoolLib.GSDK.ProductAccumulation gProductAccumulation = new CFUtilPoolLib.GSDK.ProductAccumulation();
                gProductAccumulation.CurrentSum = p.CurrentSum;
                gProductAccumulation.Version = p.Version;
                gProductAccumulation.GiftType = new CFUtilPoolLib.GSDK.GiftType(p.GiftType.GetValue());
                gProductAccumulation.StartTime = p.StartTime;
                gProductAccumulation.EndTime = p.EndTime;
                gProductAccumulation.AccumulationID = p.AccumulationID;
                gProductAccumulation.Details = new List<CFUtilPoolLib.GSDK.ProductAccumulationDetail>();
                if (p.Details != null)
                {
                    for (int j = 0; j < p.Details.Count; ++j)
                    {
                        if (p.Details[j] == null) continue;
                        CFUtilPoolLib.GSDK.ProductAccumulationDetail gProductAccumulationDetail = GetGSDKProductAccumulationDetail(p.Details[j]);
                        gProductAccumulation.Details.Add(gProductAccumulationDetail);
                    }
                }
                gProductAccumulation.Desc = p.Desc;
                gProductAccumulations.Add(gProductAccumulation);
            }
        }
        return gProductAccumulations;
    }

    public void PayRequestProductCallback(GSDK.Result result, List<GSDK.Product> products, List<GSDK.ProductAccumulation> accumulations)
    {
        if (m_payRequestProductCallback != null)
        {
            CFUtilPoolLib.GSDK.Result gResult = GetGSDKResult(result);
            List<CFUtilPoolLib.GSDK.Product> gPoducts = GetGSDKProducts(products);
            List<CFUtilPoolLib.GSDK.ProductAccumulation> gProductAccumulations = GetGSDKProductAccumulations(accumulations);
            m_payRequestProductCallback(gResult, gPoducts, gProductAccumulations);
            XDebug.singleton.AddGreenLog("PayRequestProductCallback result=" + result.ToDetailedString());
        }
    }
    #endregion
}
