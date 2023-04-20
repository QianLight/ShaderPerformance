using System;
using System.Collections.Generic;
using System.Linq;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class PayInnerTools
    {
        public static List<Product> Convert(List<GMSDK.GMProductModel> oldList)
        {
            List<Product> products = new List<Product>();
            if (oldList != null)
            {
                foreach (var product in oldList)
                {
                    products.Add(Convert(product));
                }
            }
            return products;
        }

        public static List<ProductAccumulation> Convert(List<GMSDK.GMProductAccumulation> oldList)
        {
            List<ProductAccumulation> accumulations = new List<ProductAccumulation>();
            if (oldList != null)
            {
                foreach (var accumulation in oldList)
                {
                    accumulations.Add(Convert(accumulation));
                }
            }
            return accumulations;
        } 
        
        public static Product Convert(GMSDK.GMProductModel product)
        {
            if (product == null)
            {
                return new Product();
            }
            List<ProductActivity> activities = new List<ProductActivity>();
            if (product.activities != null)
            {
                foreach (var activity in product.activities)
                {
                    activities.Add(Convert(activity));
                }
            }

            return new Product()
            {
                AppID = product.appid,
                ProductID = product.productId,
                ProductName = product.productName,
                Price = product.price,
                PriceDesc = product.priceDesc,
                CurrencyType = product.currencyType,
                Status = ConvertProductStatus(product.status),
                CreateTime = product.createTime,
                UpdateTime = product.updateTime,
                GoodsType = ConvertGoodsType(product.goodsType),
                ExtraDiscountInfo = product.extra,
                Activities = activities,
                ActivityJSONStringForCreateOrder = product.activityJSONStringForCreateOrder
            };
        }

        public static ProductStatus ConvertProductStatus(int old)
        {
            switch (old)
            {
                case 0:
                    return ProductStatus.NotAvailable;
                case 1:
                    return ProductStatus.Available;
                default:
                    return ProductStatus.NotAvailable;
            }
        }

        public static GoodsType ConvertGoodsType(int old)
        {
            switch (old)
            {
                case 1:
                    return GoodsType.RealCurrencyGoods;
                case 2:
                    return GoodsType.VirtualCurrencyGoods;
                case 3:
                    return GoodsType.SubscriptionGoods;
                case 4:
                    return GoodsType.RewardGoods;
                default:
                    return GoodsType.UnknownGoodsType;
            }
        }

        public static ProductActivity Convert(GMSDK.GMProductActivity productActivity)
        {
            return new ProductActivity()
            {
                ActivityID = productActivity.activityId,
                ActivityType = new ActivityType(productActivity.activityType),
                GiftType =  new GiftType(productActivity.giftType),
                StartTime = productActivity.startTime,
                EndTime = productActivity.endTime,
                Version = productActivity.version,
                Active = productActivity.active,
                CoinNumber = productActivity.coinNumber,
                ItemID = productActivity.itemId,
                ItemNumber = productActivity.itemNumber,
                Desc = productActivity.desc
            };
        }

        public static ProductAccumulation Convert(GMSDK.GMProductAccumulation productAccumulation)
        {
            if (productAccumulation == null)
            {
                return new ProductAccumulation();
            }

            List<ProductAccumulationDetail> details = new List<ProductAccumulationDetail>();
            if (productAccumulation.desc != null)
            {
                foreach (var detail in productAccumulation.details)
                {
                    details.Add(Convert(detail));
                }
            }
            return new ProductAccumulation()
            {
                CurrentSum = productAccumulation.currentSum,
                Version = productAccumulation.version,
                GiftType = new GiftType(productAccumulation.giftType),
                StartTime = productAccumulation.startTime,
                EndTime = productAccumulation.endTime,
                AccumulationID = productAccumulation.accId,
                Details = details,
                Desc = productAccumulation.desc,
            };
        }

        public static ProductAccumulationDetail Convert(GMSDK.GMProductAccumulationDetail productAccumulationDetail)
        {
            if (productAccumulationDetail == null)
            {
                return new ProductAccumulationDetail();    
            }
            return new ProductAccumulationDetail()
            {
                CoinNumber = productAccumulationDetail.coinNumber,
                ItemID = productAccumulationDetail.itemId,
                ItemNumber = productAccumulationDetail.itemNumber,
                LevelNumber = productAccumulationDetail.levelNumber
            };
        }
    }
}