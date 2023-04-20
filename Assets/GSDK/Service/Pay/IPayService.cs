using System;
using System.Collections.Generic;

namespace GSDK
{
    #region delegates
    /// <summary>
    /// 请求商品信息的回调
    /// </summary>
    /// <param name="result">包含有错误码的请求结果，可通过 result.IsSuccess 来判断请求是否成功</param>
    /// <para>
    ///     当前接口可能返回的错误码:
    ///         Success:成功
    ///         PayUnknownError:服务端返回空，请联系技术BP oncall定位问题
    ///         PayServerFailed:服务端请求失败，可提示用户重试，并联系中台服务端同学定位解决
    ///         PayNetworkError:网络请求失败，请检查设备网络状况，并建议重试
    /// </para>
    /// <param name="products">商品列表</param>
    /// <param name="accumulations">累积活动信息列表</param>
    public delegate void PayRequestProductDelegate(Result result, List<Product> products, List<ProductAccumulation> accumulations);
    
    /// <summary>
    /// 支付回调
    /// </summary>
    /// <param name="result">包含有错误码的请求结果，可通过 result.IsSuccess 来判断请求是否成功</param>
    /// <para>
    ///     当前接口可能返回的错误码:
    ///         Success:成功
    ///         PayNotLoginError:用户未登录，提示用户先登录
    ///         PayParameterError：接口参数客户端校验没通过，检查调用接口是必填传入参数是否都填写了
    ///         PayConfigError：配置异常检查，config.json中app维度参数是否配置
    ///         PayParseServerOrderError：解析后端下放订单失败
    ///         PayHitLocalSecurityPolicy：风控问题，联系中台排查，检查服务器下发的订单数据 did & sdkOpenId 是否与本地的一致（并可以Toast提示用户"支付失败，请重试"）
    ///         PayGuestCannotPay：游客不支持支付，提示支付失败，请重试
    ///         PayUserIsNotAuthenticated：用户未实名，不能支付
    ///         PayMinorPayLimited: 未成年人支付限制，提供防沉迷相关提示
    ///         PayPayingError：订单支付中，建议可toast提示用户
    ///         PayAppleOrGoogleError：苹果/google侧错误，可提示用户稍后重试
    ///         PayUnknownError:服务端返回空，请联系技术BP oncall定位问题
    ///         PayServerFailed:服务端请求失败，可提示用户重试，并联系中台服务端同学定位解决
    ///         PayNetworkError:网络请求失败，请检查设备网络状况，并建议重试
    /// </para>
    /// <param name="orderID">该次支付的订单ID</param>
    /// <param name="productID">该次支付的商品ID</param>
    public delegate void PayPaymentDelegate(Result result, string orderID, string productID);
    
    /// <summary>
    /// 查询商品价格信息的回调
    /// </summary>
    /// <param name="result">包含有错误码的请求结果，可通过 result.IsSuccess 来判断请求是否成功</param>
    /// <para>
    ///     当前接口可能返回的错误码:
    ///         Success:成功
    ///         PayUnknownError:服务端返回空，请联系技术BP oncall定位问题
    ///         PayServerFailed:服务端请求失败，可提示用户重试，并联系中台服务端同学定位解决
    ///         PayNetworkError:网络请求失败，请检查设备网络状况，并建议重试
    /// </para>
    /// <param name="priceInfo">商品的价格信息</param>
    public delegate void PayQueryProductsPriceDelegate(Result result, List<ProductPriceInfo> priceInfo);
    
    /// <summary>
    /// 设置支付限制结果的回调
    /// </summary>
    /// <param name="result">包含有错误码的请求结果，可通过 result.IsSuccess 来判断请求是否成功</param>
    /// <para>
    ///     当前接口可能返回的错误码:
    ///         Success:成功
    ///         PayServerFailed:服务端请求失败，可提示用户重试，并联系中台服务端同学定位解决
    ///         PayNetworkError:网络请求失败，请检查设备网络状况，并建议重试
    /// </para>
    public delegate void PaySetPayLimitDelegate(Result result);
    
    /// <summary>
    /// 获取支付限制结果的回调
    /// </summary>
    /// <param name="result">包含有错误码的请求结果，可通过 result.IsSuccess 来判断请求是否成功</param>
    /// <para>
    ///     当前接口可能返回的错误码:
    ///         Success:成功
    ///         PayServerFailed:服务端请求失败，可提示用户重试，并联系中台服务端同学定位解决
    ///         PayNetworkError:网络请求失败，请检查设备网络状况，并建议重试
    /// </para>
    /// <param name="payLimitResult">
    /// 支付限制信息，是一个 JSON 格式的字符串
    /// 例如：{ "age_enum": 1, "limit_amount": 5000, "used_amount": 2000 }
    /// </param>
    public delegate void PayFetchPayLimitDelegate(Result result, string payLimitResult);
    
    /// <summary>
    /// 获取奖励回调
    /// </summary>
    /// <param name="result">包含有错误码的请求结果，可通过 result.IsSuccess 来判断请求是否成功</param>
    /// <para>
    ///    获取奖励可能返回的错误码:
    ///             Success:成功
    ///             PayNoRewards:没有预注册奖励
    ///             PayAppleOrGoogleError:pipo 三方回调失败
    /// </para>
    /// <param name="orderID">该次的订单ID</param>
    /// <param name="productID">该次的商品ID</param>
    public delegate void PayReceiveRewardsDelegate(Result result, string orderID, string productID);
    #endregion

    # region static service class
    public static class Pay
    {
        public static IPayService Service
        {
            get
            {
                return ServiceProvider.Instance.GetService(ServiceType.Pay) as IPayService;
            }
        }
    }
    #endregion
    
    #region IPayService
    public interface IPayService : IService
    {
        #region Methods

        /// <summary>
        /// 从服务器拉取已经配置的商品列表，价格默认会做本地化处理
        /// </summary>
        /// <param name="serverID">角色所在的服务器ID</param>
        /// <param name="roleID">角色ID</param>
        /// <param name="doLocalize">是否对商品价格进行本地化，对海外有作用，若传入false，则默认货币为美元</param>
        /// <param name="payRequestProductCallback">获取信息回调</param>
        void RequestProduct(string serverID, string roleID, bool doLocalize, PayRequestProductDelegate payRequestProductCallback);
        

        /// <summary>
        /// 调用本接口后，会调出第三方支付平台的界面，购买商品，并返回支付结果。
        /// 回调返回的结果也只是「支付结果」，返回的成功 / 失败不作为服务端发货的依据。
        /// 一般来说，支付成功后，GSDK 服务端会主动返回支付回调至 GameServer，发货逻辑交给服务端处理。
        /// </summary>
        /// <param name="productID">由服务器分配的唯一的商品ID</param>
        /// <param name="goodsType">物品类型，可从 [RequestProduct] 接口的回调中获得</param>
        /// <param name="serverID">角色所在的服务器ID，可从角色模块中获得</param>
        /// <param name="roleInfo">角色相关信息，可从角色模块中获得</param>
        /// <param name="extraInfo">自定义信息，会在服务端支付回调中原样返回</param>
        /// <param name="payCallback">支付回调</param>
        void Pay(string productID, GoodsType goodsType, string serverID, RoleInfoForPay roleInfo, string extraInfo, PayPaymentDelegate payCallback);

#if UNITY_IOS

        /// <summary>
        /// 支付模块是否启用sdk自带错误弹窗提示
        /// </summary>
        /// <param name="flag">true:启用sdk自带错误弹窗提示  false:关闭sdk自带错误弹窗提示，由游戏方自己处理。</param>
        void EnableShowErrorToast(bool flag);
#endif
        
        #region 海外专有接口


        #endregion

        #endregion
    }
    #endregion

    #region public defines

    public class RoleInfoForPay
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public string RoleID;
        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName;
        /// <summary>
        /// 角色等级
        /// </summary>
        public string RoleLevel;
        /// <summary>
        /// 角色Vip等级，若不知道该传多少可默认传0
        /// </summary>
        public int RoleVipLevel;
    }

    /// <summary>
    /// 一个商品的信息
    /// </summary>
    public class Product
    {
        /// <summary>
        /// sdk服务器分配的id
        /// </summary>
        public string AppID;
        /// <summary>
        /// 游戏服务器分配的商品id
        /// </summary>
        public string ProductID;
        /// <summary>
        /// 游戏定义的商品名称
        /// </summary>
        public string ProductName;
        /// <summary>
        /// 商品价格
        /// </summary>
        public long Price;
        /// <summary>
        /// 商品价格描述
        /// </summary>
        public string PriceDesc;
        /// <summary>
        /// 币种
        /// </summary>
        public string CurrencyType;
        /// <summary>
        /// 商品状态（上架，下架）
        /// </summary>
        public ProductStatus Status;
        /// <summary>
        /// 商品在SDK服务器的创建时间
        /// </summary>
        public long CreateTime;
        /// <summary>
        /// 商品在SDK服务器的更新时间
        /// </summary>
        public long UpdateTime;
        /// <summary>
        /// 商品类型
        /// </summary>
        public GoodsType GoodsType;
        /// <summary>
        /// 折扣额外信息，该字段仅作展示用，不会对商品实际价格有影响
        /// </summary>
        public string ExtraDiscountInfo;

        /// <summary>
        /// 当前活动
        /// </summary>
        public List<ProductActivity> Activities;
        /// <summary>
        /// 下单的activity字段
        /// </summary>
        public string ActivityJSONStringForCreateOrder;
    }

    public enum ProductStatus
    {
        /// <summary>
        /// 已下架
        /// </summary>
        NotAvailable = 0,
        /// <summary>
        /// 可购买
        /// </summary>
        Available = 1
    }

    public enum GoodsType
    {
        /// <summary>
        /// 未知类型
        /// </summary>
        UnknownGoodsType = 0,
        /// <summary>
        /// 现实货币直购商品
        /// </summary>
        RealCurrencyGoods = 1,
        /// <summary>
        /// 虚拟币商品
        /// </summary>
        VirtualCurrencyGoods = 2,
        /// <summary>
        /// 订阅商品
        /// </summary>
        SubscriptionGoods = 3,
        /// <summary>
        /// 奖励商品（预注册/兑换码）
        /// </summary>
        RewardGoods = 4,
    }

    /// <summary>
    /// 虚拟币活动
    /// </summary>
    public class ProductActivity
    {
        /// <summary>
        /// 唯一标志
        /// </summary>
        public long ActivityID;
        /// <summary>
        /// 活动描述类型
        /// First: 首冲
        /// Limit: 道具
        /// Combo: 组合
        /// </summary>
        public ActivityType ActivityType;
        /// <summary>
        /// 礼物描述类型
        /// Coin: 游戏币
        /// Item: 道具
        /// Combo: 组合
        /// </summary>
        public GiftType GiftType;
        /// <summary>
        /// 开始时间
        /// </summary>
        public long StartTime;
        /// <summary>
        /// 结束时间
        /// </summary>
        public long EndTime;
        /// <summary>
        /// 当前版本，服务端验证
        /// </summary>
        public int Version;
        /// <summary>
        /// 是否生效
        /// </summary>
        public bool Active;
        /// <summary>
        /// 赠送游戏币数量
        /// </summary>
        public int CoinNumber;
        /// <summary>
        /// 赠送道具id
        /// </summary>
        public string ItemID;
        /// <summary>
        /// 赠送道具数量
        /// </summary>
        public int ItemNumber;
        /// <summary>
        /// 描述
        /// </summary>
        public string Desc;         
    }

    /// <summary>
    /// 累计赠送虚拟币活动
    /// </summary>
    public class ProductAccumulation
    {
        /// <summary>
        /// 累计计算中玩家当前有效值
        /// </summary>
        public long CurrentSum;
        /// <summary>
        /// 当前版本，服务端验证
        /// </summary>
        public int Version;
        /// <summary>
        /// 礼物描述类型
        /// Coin: 游戏币
        /// Item: 道具
        /// Combo: 组合
        /// </summary>   
        public GiftType GiftType;
        /// <summary>
        /// 开始时间,以秒为单位
        /// </summary>
        public long StartTime;
        /// <summary>
        /// 结束时间 以秒为单位
        /// </summary>
        public long EndTime;
        /// <summary>
        /// 该累积的唯一ID
        /// </summary>
        public long AccumulationID;
        /// <summary>
        /// 累计赠送细节
        /// </summary>
        public List<ProductAccumulationDetail> Details;
        /// <summary>
        /// 描述
        /// </summary>
        public string Desc;        
    }
    
    /// <summary>
    /// 累计赠送的每一个档位描述信息
    /// </summary>
    public class ProductAccumulationDetail
    {
        /// <summary>
        /// 赠送游戏币数量
        /// </summary>
        public int CoinNumber;
        /// <summary>
        /// 赠送道具id
        /// </summary>
        public string ItemID;
        /// <summary>
        /// 赠送道具数量
        /// </summary>
        public int ItemNumber;
        /// <summary>
        /// 满足赠送条件的数量
        /// </summary>
        public int LevelNumber;    
    }
    
    public class GiftType
    {
        public static GiftType Coin = new GiftType("coin");
        public static GiftType Item = new GiftType("item");
        public static GiftType Combo = new GiftType("combo");
        private string _value;
        public GiftType(string value)
        {
            _value = value;
        }

        public string GetValue()
        {
            return _value;
        }

        public override string ToString()
        {
            return _value;
        }
    }

    public class ActivityType
    {
        public static ActivityType First = new ActivityType("First");
        public static ActivityType Limit = new ActivityType("Limit");
        public static ActivityType Combo = new ActivityType("combo");
        private string _value;

        public ActivityType(string value)
        {
            _value = value;
        }

        public string GetValue()
        {
            return _value;
        }
        
        public override string ToString()
        {
            return _value;
        }
    }
    
    public class ProductPriceInfo
    {
        /// <summary>
        /// 商品ID
        /// </summary>
        public string ProductID;
        /// <summary>
        /// 完整的价格表示，例如「HK$15.00」
        /// </summary>
        public string PriceDesc;
        /// <summary>
        /// 货币类型，通常是 ISO 4217 中的三个字母缩写
        /// </summary>
        public string CurrencyType;
        /// <summary>
        /// 价格，单位为分。例如 1500 代表实际价格 15.00
        /// </summary>
        public long Price;
    }
    
    public static partial class ErrorCode
    {
        /// <summary>
        /// 接口参数客户端校验没通过，检查调用接口是必填传入参数是否都填写了
        /// </summary>
        public const int PayParameterError = -390000;
        
        /// <summary>
        /// 未知问题，请联系oncall 解决
        /// </summary>
        public const int PayUnknownError = -390001;
        
        /// <summary>
        /// 配置异常检查，config.json中app维度参数是否配置
        /// </summary>
        public const int PayConfigError = -390002;
        
        /// <summary>
        /// 订单支付中，建议可toast提示用户
        /// </summary>
        public const int PayPayingError = -390003;
        
        /// <summary>
        /// 解析后端下放订单失败
        /// </summary>
        public const int PayParseServerOrderError = -390004;
        
        /// <summary>
        /// 风控问题，联系中台排查，检查服务器下发的订单数据 did & sdkOpenId 是否与本地的一致（并可以Toast提示用户"支付失败，请重试"）
        /// </summary>
        public const int PayHitLocalSecurityPolicy = -390005;
        
        /// <summary>
        /// 游客不支持支付，提示支付失败，请重试 
        /// </summary>
        public const int PayGuestCannotPay = -390006;
        
        /// <summary>
        /// 用户未实名，不能支付
        /// </summary>
        public const int PayUserIsNotAuthenticated = -390007;
        
        /// <summary>
        /// 未成年人支付限制
        /// </summary>
        public const int PayMinorPayLimited = -390008;

        /// <summary>
        /// 用户取消支付，可以提示用户已「取消支付」
        /// </summary>
        public const int PayCounterPayCanceled = -390009;
        
        /// <summary>
        /// 收银台支付进行中 / 未完成支付订单
        /// 提示用户当前有正在处理的支付，请稍后重试	
        /// </summary>
        public const int PayCounterPayProceeding = -390010;
        
        /// <summary>
        /// 没有奖励
        /// </summary>
        public const int PayNoRewards = -390011;

        /// <summary>
        /// 网络错误，请检查网络状况
        /// </summary>
        public const int PayNetworkError = -393001;
        
        /// <summary>
        /// 用户未登录，提示用户先登录
        /// </summary>
        public const int PayNotLoginError = -399800;

        /// <summary>
        /// GSDK 服务端问题，请联系技术BP oncall支持，同时可以提示"支付失败，请重试"
        /// </summary>
        public const int PayServerFailed = -397000;
        
        /// <summary>
        /// 依赖的三方（财经、国际）错误，请拉起/或联系技术BP拉起三方oncall（国内 - 财经 / 海外 - 国际）
        /// </summary>
        public const int PayTripartiteError = -398000;
        
        /// <summary>
        /// 苹果/google侧错误，可提示用户稍后重试
        /// </summary>
        public const int PayAppleOrGoogleError = -399000;
    }
    #endregion
}