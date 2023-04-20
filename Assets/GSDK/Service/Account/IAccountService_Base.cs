using System;
using System.Collections.Generic;

namespace GSDK
{
    #region delegates
    
    /// <summary>
    /// 登录回调
    /// </summary>
    /// /// <param name="result">
    /// 包含错误码的结果
    /// 有UI登录无需关心错误情况
    /// <para>
    /// 【字节账号体系】可能返回的错误码：
    /// 		AccountActivationCodeUserCancel: 激活码页面点击关闭
    /// 		AccountUserCancel: 用户主动取消授权
    /// 		AccountAuthFail: 三方授权失败
    /// 		AccountFetchUserFail: 获取用户数据失败，检查配置或者三方平台配置
    /// 		AccountRiskControlForPhone: 需要手机二次验证，列举出来，内部已经闭环
    /// 		AccountUserBanError:  // 该用户帐号已被封禁
    ///  		AccountFrequentVisitsError: // 风控访问太频繁 
    /// 		AccountPassportError: // passport 其他错误
    ///         GSDK Server // 侧错误，具体查看 GSDK Server 通用错误
    /// 			额外关注
    /// 				AccountActiveCodeRegisterLimitError: // 无UI登录接口遇到此错误，可以调用激活码注册登录接口
    ///
    /// 【自定义账号体系】可能返回的错误码:
    ///         AccountCustomNetworkError: 网络错误
    ///         AccountCustomUnknownError: 未知错误
    /// </para>
    /// </param>
    /// <param name="info">
    /// 登录信息
    /// <see cref="AccountInfo"/>
    /// </param>
    public delegate void AccountLoginEventHandler(Result result, AccountInfo info);
    
    #endregion
    
    public interface IAccountService : IService
    {
        /// <summary>
        /// 登出 
        /// </summary>
        /// <remarks>
        /// 登出账号，只会让当前账号失效，但没有将账号从历史账号中删除。
        /// 下次登录不需要重新授权
        /// </remarks>
        void Logout();
        
        /// <summary>
        /// 是否已登录
        /// </summary>
        /// <returns>true为已登录，false为未登录</returns>
        bool HasLogined { get; }
        
        /// <summary>
        /// 获得登录信息
        /// </summary>
        /// <returns>用户的登录信息</returns>
        AccountInfo GetLoginRecord();
    }
    
    #region public defines
    
    public class AccountInfo
    {
        public AccountType AccountType;

        /// <summary>
        /// OpenID，是GSDK层的唯一账号，一个OpenID对应GSDK的一个用户
        /// </summary>
        public string OpenID;
        /// <summary>
        /// AccessToken，用于向服务端验证登录合法性
        /// </summary>
        public string Token;

        /// <summary>
        /// for GSDK inner using
        /// </summary>
        public string UserID;
        /// <summary>
        /// 是否为游客账号
        /// </summary>
        public bool IsGuest;
        
        /// <summary>
        /// 登录时间
        /// </summary>
        public string LoginTime;
        
        /// <summary>
        /// 该用户所绑定的其他（第三方）账户
        /// </summary>
        public List<UserDetailInfo> BindedUserInfoCollection;
		
        /// <summary>
        ///  330 国内登录新增字段。处于账号注销中状态的用户首次登录会返回 true。接入方可根据 cancelLogoff == true 的时候，提醒用户账号注销已经被终止的提示
        /// </summary>
        public bool CancelLogoff;
    }

    public enum AccountType
    {
        None = 0,
        /// <summary>
        /// 用于自动登录
        /// </summary>
        Auto,
        /// <summary>
        /// 游客
        /// </summary>
        Guest,
        /// <summary>
        /// 手机号
        /// </summary>
        Phone,
        /// <summary>
        /// 今日头条
        /// </summary>
        TouTiao,
        /// <summary>
        /// 抖音
        /// </summary>
        Awe,
        /// <summary>
        /// Google
        /// </summary>
        Google,
        /// <summary>
        /// Facebook
        /// </summary>
        Facebook,
        /// <summary>
        /// Twitter
        /// </summary>
        Twitter,
        /// <summary>
        /// Line
        /// </summary>
        Line,
        /// <summary>
        /// Kakao
        /// </summary>
        KakaoTalk,
        /// <summary>
        /// VK
        /// </summary>
        VK,
        /// <summary>
        /// Apple
        /// </summary>
        Apple,
        /// <summary>
        /// 云游戏登录
        /// </summary>
        CloudGame,
        /// <summary>
        /// 云游戏登录V2，仅在国内使用
        /// </summary>
        CloudGameV2,
        /// <summary>
        ///  使用账密体系类型账号
        /// </summary>
        Password,
        /// <summary>
        ///  抖音火山版
        /// </summary>
        HuoShan,
        /// <summary>
        ///  西瓜视频
        /// </summary>
        XiGua,
        /// <summary>
        ///  TapTap
        /// </summary>
        TapTap,
        /// <summary>
        /// 邮箱
        /// </summary>
        Email,
        /// <summary>
        /// Tik Tok
        /// </summary>
        TikTok,
        /// <summary>
        /// Google Play Games
        /// </summary>
        GooglePlay,
        /// <summary>
        ///  游戏自有账号，使用非字节账号体系
        /// </summary>
        Custom,
    }
    
    public class UserDetailInfo
    {
        /// <summary>
        /// PUID
        /// </summary>
        public Int64 PassportUserID;	
        public AccountType AccountType;
        public string NickName;
        public string AvatarUrl;
    }
    
    public partial class ErrorCode
    {
        /// <summary>
        /// 帐号内部请求Device Id失败(仅在非阻塞能力开启的情况下，登录前会请求Device Id)
        /// </summary>
        public const int AccountInitDidFailed = -105026;
        // unity 定义的错误？
		public const int AccountAccessTokenInvalid = -105901;
    }
    
    #endregion
}