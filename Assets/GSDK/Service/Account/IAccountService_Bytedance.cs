using System;
using System.Collections.Generic;

namespace GSDK
{
    #region delegates

    #region 通用回调

    /// <summary>
    /// 重置游客
    /// </summary>
    /// 包含错误码的结果
    /// <para>
    /// 	可能返回的错误码：
    ///        	AccountNetError: 网络错误 
    ///         GSDK Server 侧错误，具体查看 GSDK Server 通用错误
    /// </para>
    public delegate void AccountResetGuestDelegate(Result result, bool needLogout);


    /// <summary>
    /// 登出回调
    /// </summary>
    /// /// <param name="result">
    /// 包含错误码的结果
    /// <para>
    /// 	可能返回的错误码：
    /// 		AccountNotLoggedIn: 未登录错误
    ///        	AccountNetError: 网络错误 
    ///         GSDK Server 侧错误，具体查看 GSDK Server 通用错误
    /// </para>
    /// </param>
    public delegate void AccountLogoutEventHandler(Result result);


    /// <summary>
    /// 绑定事件回调
    /// </summary>
    /// <param name="result">
    /// 包含错误码的结果
    /// <para>
    /// 	可能返回的错误码：
    /// 		国内
    ///			AccountNotLoggedIn:   // 用户未登录
    /// 		AccountNetError:      // 网络错误
    /// 		AccountUserBanError:  // 该用户帐号已被封禁
    ///  		AccountFrequentVisitsError: // 风控访问太频繁 
    /// 		AccountPassportError: // passport 其他错误
    /// 		海外
    /// 
    /// </para>
    /// </param>
    /// <param name="bindType">绑定的账户类型</param>
    public delegate void AccountBindAccountEventHandler(Result result, AccountType bindType);


    /// <summary>
    /// 解绑事件回调
    /// </summary>
    /// <param name="result">
    /// 包含错误码的结果
    /// <para>
    /// 	可能返回的错误码：
    /// 		ErrorCode.AccountError: 		// 通用错误，一般发生未知错误时返回，建议弹窗提示稍后再试
    /// 		ErrorCode.AccountNotLoggedIn: 	// 用户未登录
    /// 		ErrorCode.AccountUnbindThirdPartyFailed: // 解绑三方账号失败
    /// </para>
    /// </param>
    /// <param name="unbindType">解绑的账户类型</param>
    public delegate void AccountUnbindAccountEventHandler(Result result, AccountType unbindType);

    /// <summary>
    /// 面板关闭的回调
    /// </summary>
    public delegate void AccountPanelClosedDelegate();

    #endregion

    #region 国内特有回调

    /// <summary>
    /// 显示用户中心回调
    /// </summary>
    /// /// <param name="result">
    /// 包含错误码的结果，
    /// <para>
    /// 	可能返回的错误码：
    ///			AccountNotLoggedIn: // 用户未登录
    /// </para>
    /// </param>
    public delegate void AccountShowUserCenterDelegate(Result result);


    /// <summary>
    /// 实名认证回调
    /// </summary>
    /// <param name="result">
    /// 包含错误码的结果
    /// <para>
    /// 	可能返回的错误码：
    ///			AccountNotLoggedIn: // 未登录错误
    /// 		AccountRealNameAuthLevelLow: // 实名认证等级低，无需实名认证即可进入游戏
    /// 		AccountRealNameAuthCancel: // 用户取消实名认证
    /// 		AccountLocalAlreadyRealName: // 该帐号已被实名认证，无需再次实名认证
    ///			AccountUserCancel: // 抖音一键实名，用户跳转三方授权取消
    /// 		AccountPassportError: // passport 其他错误	
    ///        	AccountNetError: // 网络错误 
    ///         GSDK Server // 侧错误，具体查看 GSDK Server 通用错误
    /// </para>
    /// </param>
    /// <param name="realNameAuthResult">
    /// 实名信息
    /// <see cref="RealNameAuthResult"/>
    /// </param>
    public delegate void AccountRealNameAuthEventHandler(Result result, RealNameAuthResult realNameAuthResult);
    
    /// <summary>
    /// 关联信息的回调
    /// </summary>
    /// <param name="result">关联结果</param>
    /// <param name="linkInfo">关联信息</param>
    public delegate void AccountLinkInfoDelegate(Result result, List<LinkInfo> linkInfo);



    #endregion

    #region 海外特有回调


    #endregion

    #endregion

    #region static service class

    public static class Account
    {
        public static IAccountBytedanceService Service
        {
            get
            {
                return ServiceProvider.Instance.GetService(ServiceType.Account,
                    "Bytedance") as IAccountBytedanceService;
            }
        }
    }

    #endregion


    #region IAccountService

    public interface IAccountBytedanceService : IAccountService
    {
        #region 回调Event

        event AccountLoginEventHandler LoginEvent;

        event AccountLogoutEventHandler LogoutEvent;

        event AccountRealNameAuthEventHandler RealNameAuthEvent;

        event AccountBindAccountEventHandler BindAccountEvent;

        event AccountUnbindAccountEventHandler UnbindAccountEvent;


        #endregion


        #region 通用接口

        /// <summary>
        /// 登录（带UI）
        /// </summary>
        /// <param name="panelClosedCallback">面板被用户关闭的回调</param>
        /// <remarks>登录结果通过LoginEvent返回</remarks>
        /// <seealso cref="LoginEvent"/>
        void LoginWithUI(AccountPanelClosedDelegate panelClosedCallback = null);


        /// <summary>
        /// （无UI）登录
        /// </summary>
        /// <param name="accountType">
        /// 登录的账号类型：
        /// 1. Auto为自动登录，即用上一次登录的账号进行登录。
        /// 2. 其他的为使用对应的第三方账号授权登录。
        /// 3. 传入CloudGame为字节云游戏登录，仅在安卓有效
        /// 4. 传入CloudGameV2为字节云游戏登录，仅在安卓国内有效
        /// </param>
        /// <remarks>登录结果通过LoginEvent返回</remarks>
        void LoginWithoutUI(AccountType accountType);


        /// <summary>
        /// （无UI）重置游客
        /// </summary>
        /// <remarks>要注意只有游客账号本身以及绑定了游客账号的三方账号才可以调用该接口</remarks>
        void ResetGuest(AccountResetGuestDelegate resetGuestCallback);


        /// <summary>
        /// 检测隐私协议是否更新
        /// </summary>
        /// <returns>true为协议已更新 / false为协议未更新</returns>
        bool IsProtocolUpdated();


        /// <summary>
        /// 【安卓特有接口】判断是否为云游戏环境
        /// </summary>
        /// <returns>true为正处于云游戏环境，false为不为云游戏环境</returns>
        bool IsCloudRuntime();

        #endregion


        #region 国内独有接口


        /// <summary>
        /// 显示用户中心
        /// </summary>
        /// <param name="showUserCenterDelegate">打开用户中心面板的回调</param>
        /// <param name="panelClosedCallback">面板被用户关闭的回调</param>
        void ShowUserCenter(AccountShowUserCenterDelegate showUserCenterDelegate,
            AccountPanelClosedDelegate panelClosedCallback = null);
        
        /// <summary>
        /// 实名认证（带UI），外渠使用
        /// </summary>
        /// <param name="authType">
        /// 	<seealso cref="RealNameAuthType">
        /// 		DeterminedByServer = 0, 		// 0：认证等级走网络配置,由服务器决定采用2还是3
        ///			CanSkipAuth = 2, 				// 2：认证等级不走网络配置,可以关闭实名认证页面，跳过认证
        ///			EnforceAuth = 3					// 3：认证等级不走网络配置,强制实名认证(没有关闭按钮)
        /// 	</seealso>
        /// </param>
        /// <remarks>
        /// 结果在RealNameAuthEvent中返回
        /// </remarks>
        /// <seealso cref="RealNameAuthEvent"/>
        void AuthRealNameWithUI(RealNameAuthType authType = RealNameAuthType.DeterminedByServer);

        /// <summary>
        /// 显示绑定手机界面
        /// </summary>
        /// <param name="panelClosedCallback">面板被用户关闭的回调</param>
        /// <remarks>
        /// 结果在BindAccountEvent中返回
        /// </remarks>
        /// <seealso cref="BindAccountEvent"/>
        void ShowPhoneBindingView(AccountPanelClosedDelegate panelClosedCallback = null);

        /// <summary>
        /// 检查用户是否已绑定特定第三方账号
        /// </summary>
        /// <param name="type">
        /// 	目前支持国内Phone、Awe（抖音）、TouTiao（头条）、Apple（iOS特有）、HuoShan（抖音火山版）、XiGua（西瓜视频） 六种
        /// </param>
        /// <returns>true为已绑定，false为未绑定</returns>
        bool HasBinded(AccountType type);

        /// <summary>
        /// 某些游戏，登录场景内有独立的【协议】按钮，点击后需要调起协议条款界面 
        /// </summary>
        /// <param name="panelClosedCallback">面板被用户关闭的回调</param>
        void ShowLicense(AccountPanelClosedDelegate panelClosedCallback = null);

        /// <summary>
        /// //登出。token过期。表现对齐用户中心退出登录效果。
        /// </summary>
        void LogoutAndSetTokenExpired();

        #region 关联能力

        /// <summary>
        /// 获取关联信息
        /// </summary>
        /// <param name="callback">请求结果</param>
		/// <para>
        /// 	可能返回的错误码：
        /// 		ErrorCode.AccountParameterError
        /// 		ErrorCode.AccountTokenError
        /// 		ErrorCode.AccountServerError
        /// 		ErrorCode.AccountNetError
        /// 		ErrorCode.AccountGServerError
        /// 		ErrorCode.AccountGClientError
        /// </para>
        void FetchLinkInfo(AccountLinkInfoDelegate callback);

        /// <summary>
        /// 进行关联
        /// </summary>
        /// <param name="callback">请求结果</param>
        /// <param name="scopeInfo">关联的Scope权限信息；如果不传，会使用登录时的默认配置，该配置根据配置面板中的勾选情况生成</param>
        /// <para>
        /// 	可能返回的错误码：
        /// 		ErrorCode.AccountUserCancel
        /// 		ErrorCode.AccountAuthFail
        /// 		ErrorCode.AccountParameterError
        /// 		ErrorCode.AccountTokenError
        /// 		ErrorCode.AccountLinkAuthCommonError
        /// 		ErrorCode.AccountLinkAuthCodeExpiredError
        /// 		ErrorCode.AccountLinkAuthNotMatchError
        /// 		ErrorCode.AccountNetError
        /// 		ErrorCode.AccountServerError
        /// 		ErrorCode.AccountNotInstallPluginError
        /// 		ErrorCode.AccountGServerError
        /// 		ErrorCode.AccountGClientError
        /// </para>
        void LinkAuth(AccountLinkInfoDelegate callback, ScopeInfo scopeInfo = null);

        /// <summary>
        /// 解除关联 
        /// </summary>
        /// <param name="callback">请求结果</param>
        /// <para>
        /// 	可能返回的错误码：
        /// 		ErrorCode.AccountParameterError
        /// 		ErrorCode.AccountTokenError
        /// 		ErrorCode.AccountServerError
        /// 		ErrorCode.AccountLinkReleaseNotHaveInfoError
        /// 		ErrorCode.AccountNetError
        /// 		ErrorCode.AccountGServerError
        /// 		ErrorCode.AccountGClientError
        /// </para>
        void LinkRelease(AccountLinkInfoDelegate callback);

        /// <summary>
        /// 获取对应平台的默认Scope信息
        /// </summary>
        /// <returns>SDK登录时的默认配置，根据配置面板中的勾选情况生成</returns>
        ScopeInfo GetDefaultScopeInfo();

        #endregion


        #endregion


        #region 海外独有接口


        #endregion
    }

    #endregion

    #region public defines

    /// <summary>
    /// 实名认证（SDK自有界面）认证等级根据type来定
    /// </summary>
    public enum RealNameAuthType
    {
        /// <summary>
        /// 0：认证等级走网络配置,由服务器决定采用2还是3
        /// </summary>
        DeterminedByServer = 0,

        /// <summary>
        /// 2：认证等级不走网络配置,可以关闭实名认证页面，跳过认证
        /// </summary>
        CanSkipAuth = 2,

        /// <summary>
        /// 3：认证等级不走网络配置,强制实名认证(没有关闭按钮)
        /// </summary>
        EnforceAuth = 3,
    }


    public enum RealNameAuthLevel
    {
        Default = 0,
        Low = 1,
        Medium = 2,
        High = 3,
    }

    public class RealNameAuthResult
    {
        /// <summary>
        /// 实名等级
        /// </summary>
        public RealNameAuthLevel AuthLevel;

        /// <summary>
        /// 年龄
        /// </summary>
        public int Age;

        /// <summary>
        /// 防沉迷的提示文本
        /// </summary>
        public string AntiAdditionTips;
    }

    public enum BindOrUnbindOperation
    {
        Bind,
        Unbind
    }

    /// <summary>
    /// AgeGate弹窗类型
    /// </summary>
    public enum AgeGateWindowType
    {
        Undefined = 0,

        /// <summary>
        /// 选择出生年月日窗口
        /// </summary>
        Date = 1,

        /// <summary>
        /// 选择「我已达到xx岁」或「我未达到xx岁」
        /// </summary>
        Age = 2,

        /// <summary>
        /// 选择出生年份
        /// </summary>
        BirthYear = 3,

        /// <summary>
        /// 在登录界面中增加「我已达到xx岁」check box
        /// 默认已勾选
        /// </summary>
        CheckedPassAgeGate = 4,

        /// <summary>
        /// 在登录界面中增加「我已达到xx岁」check box
        /// 默认未勾选
        /// </summary>
        NotCheckedPassAgeGate = 5,
    }


    public enum AccountActionType
    {
        EnableFacebook = 1,
    }

    public abstract class AccountAction
    {
        public AccountActionType ActionType;
    }

    public class EnableFacebookAction : AccountAction
    {
        public readonly bool Enable;

        [Obsolete("FaceBook初始化于3.1.0.0版本以后废弃，不需要进行调用")]
        public EnableFacebookAction(bool enable)
        {
            ActionType = AccountActionType.EnableFacebook;
            Enable = enable;
        }
    }

    public enum EmailOptType
    {
        None = 0,

        /// <summary>
        /// 注册
        /// </summary>
        REGISTER,

        /// <summary>
        /// 忘记密码
        /// </summary>
        FORGET_PASSWORD
    }

    public class ScopeInfo
    {
        /// <summary>
        /// 必选权限列表
        /// </summary>
        public List<string> Permission;

        /// <summary>
        /// 可选权限列表（默认勾选）
        /// </summary>
        public List<string> AdditionalPermissionSelected;
        
        /// <summary>
        /// 可选权限列表（默认不勾选）
        /// </summary>
        public List<string> AdditionalPermissionUnSelected;

        /// <summary>
        /// 额外参数，预留字段
        /// </summary>
        public Dictionary<object, object> Extra;
    }

    public class LinkInfo
    {
        /// <summary>
        /// 关联的平台类型
        /// </summary>
        public AccountType Platform;

        /// <summary>
        /// 昵称
        /// </summary>
        public string Nickname;

        /// <summary>
        /// 头像Url
        /// </summary>
        public string AvatarUrl;

        /// <summary>
        /// 关联的平台openId
        /// </summary>
        public string OpenId;

        /// <summary>
        /// 关联的平台accessToken，过期返回空（非空也可能过期）
        /// </summary>
        public string AccessToken;

        /// <summary>
        /// 关联的平台已授权权限列表，过期返回空
        /// </summary>
        public string ScopeList;
    }

    #endregion
}

namespace GSDK
{
    public partial class ErrorCode
    {
        /* passport */
        // p 普通授权失败（apple无返回）
        public const int AccountAuthFailInApple = -100000;

        // p 用户主动取消
        public const int AccountUserCancel = -100001;

        // p 三方授权失败
        public const int AccountAuthFail = -100002;

        // p 第三方授权：获取三方用户信息异常
        public const int AccountFetchUserFail = -100003;

        // p 触发风控：需要手机二次验证
        public const int AccountRiskControlForPhone = -100004;

        // p 该用户帐号已被封禁
        public const int AccountUserBanError = -100008;

        // p 当前不满足快捷登录条件，无法进行快捷登陆
        public const int AccountGuestLoginFailForNoCondition = -100009;

        // p 游客模拟器登录失败 Android
        public const int AccountGuestInSimulationError = -100010;

        // p passport 风控，访问频繁
        public const int AccountFrequentVisitsError = -100011;

        //邮箱发码相关错误码
        //用户不存在
        public const int AccountEmailUserNotExist = -100012;

        //邮箱已存在
        public const int AccountEmailAlreadyExists = -100016;

        //邮箱格式错误
        public const int AccountEmailFormatError = -100017;

        //邮箱验证码发送频繁
        public const int AccountEmailCodesSentFrequently = -100018;

        //邮箱验证码过期
        public const int AccountEmailCodeExpired = -100019;

        //邮箱验证码错误                                            
        public const int AccountEmailCodeError = -100020;

        //邮箱密码格式错误
        public const int AccountEmailPwdFormatError = -100021;


        /* GSDK Server 通用错误 */
        // g 检查吊起登录参数，检查是否初始化完成
        public const int AccountParameterError = -101000;

        // g token异常、过期、不匹配
        public const int AccountTokenError = -101001;

        // g 无UI单绑定不支持三方账号进行绑定操作, 错误信息会返回
        public const int AccountWithUINoSupportBindError = -101002;

        // g 游客绑定三方后，仍用游客方式登录
        public const int AccountGuestBindThirdInGuestLoginError = -101003;

        // 账号/设备被封禁
        public const int AccountDeviceOrAccountBanError = -101004;

        // g 当天创建游客达上限
        public const int AccountCreateGuestLimitError = -101005;

        // g 当前三方账户已绑定该游戏，可以解绑
        public const int AccountThirdAleardyBindError = -101006;

        // 账号注册限制
        public const int AccountRegisterLimitError = -101007;

        // 当前三方账户已绑定该游戏，不可解绑
        public const int AccountThirdAlreadyBindNoUnBindError = -101008;

        // 已达到账号绑定数量上限
        public const int AccountBindCountLimitError = -101009;

        // is_can_unbind为NO，不支持解除绑定
        public const int AccountNotSupportUnBindError = -101010;

        // 该游戏没配置单设备多游客模式，但是客户端传参is_create=1，错误调用
        public const int AccountNotConfigParamError = -101011;

        // 没有绑定对应的平台账号
        public const int AccountNotFoundThirdPlatfromError = -101012;

        // Android 渠道错误 iOS 不管
        // 激活码相关错误
        public const int AccountActiveAboutError = -101013;

        // 命中风控
        public const int AccountRiskControlError = -101014;

        // 校验手机号验证身份
        public const int AccountRishControlForPhoneError = -101015;

        // 服务器异常
        public const int AccountServerError = -101016;

        // 注册限制尚未开启注册
        public const int AccountNotStartRegisterError = -101017;

        // 查询sdkopenid，没有该userid
        public const int AccountQuerySdkOpenIdNotUseUserIDError = -101018;

        // 用户已实名认证过
        public const int AccountRealNameHasVerifiedError = -101019;

        // 账号注销
        public const int AccountIsBeingCancelError = -101022;

        // 当前帐号没有可重置的游客类型
        public const int AccountUserNoHaveGuestForResetError = -101023;

        // 激活码注册限制
        public const int AccountActiveCodeRegisterLimitError = -101024;
        
        // Passport登录态出现错误
        public const int AccountPassportTokenError = -101025;
        
        // 本地存在已经登录过的游客帐号
        public const int AccountVisitorExistError = -101026;
        
        // 多设备登录达到上限
        public const int AccountMultipleDeviceLoginReachedTheLimit = -101027;
        
        // 当前三方帐号已经被绑定，无法进行绑定
        public const int AccountHasBound = -101028;
        
        // 进行关联时，抖音返回的通用错误
        public const int AccountLinkAuthCommonError = -101029;
        
        // 进行关联时，抖音的授权码已经过期
        public const int AccountLinkAuthCodeExpiredError = -101030;
        
        // 进行关联时，更新关联关系的抖音帐号与当前关联抖音帐号不一致
        public const int AccountLinkAuthNotMatchError = -101031;
        
        // 解除关联时，当前帐号不存在关联信息
        public const int AccountLinkReleaseNotHaveInfoError = -101032;
        
        

        /* client */
        /// 实名认证等级低
        public const int AccountRealNameAuthLevelLow = -105000;

        /// 用户取消实名认证
        public const int AccountRealNameAuthCancel = -105001;

        /// 本地判断用户已经实名
        public const int AccountLocalAlreadyRealName = -105002;

        /// 抖音未实名
        public const int AccountNotRealNameWithDouyin = -105012;
        /// 相关参数未配置
        public const int AccountLocalConfigError = -105020;

        /// 需要设备安装对应的app，但是却没有。例如抖音火山登录方式
        public const int AccountNotInstallAppError = -105021;
	
        // 未引入三方SDK库文件
        public const int AccountNotInstallPluginError = -105022;
        
        // 暂不支持西瓜视频iPad版授权登录
        public const int AccountIPadXiguaNotSupportedError = -105023;
        
        // 非游客帐号不能绑定手机号
        public const int AccountOnlyVisitorCanBeBound = -105025;

        /// 本地没有历史账号
        public const int AccountNotHistoryUser = -105030;

        ///  抖音火山绑定但是本地没有安装
        public const int AccountHuoShanNotInstallOrNotSupportError = -105040;

        /// 有UI登录激活码页面取消
        public const int AccountActivationCodeUserCancel = -105050;

        /// 账号密码长度不够
        public const int AccountCodeCodeLengthError = -105060;

        /// 帐密时密码类型格式错误
        public const int AccountCodeCodeFormatError = -105061;

        /// 帐密时风控被关闭
        public const int AccountCodeCloseGuardError = -105062;

        //参数为空
        public const int AccountEmailParamsNull = -105063;

        /// 绑定达到上限
        public const int AccountLocalBindLimit = -105070;

        /// 扫码登录接口不支持游客
        public const int AccountQrNotSupportGuestError = -105080;

        /// 二维码token无效
        public const int AccountQrTokenInvaildError = -105081;

        // unity 定义的错误？
        public const int AccountUserInfoNullError = -105900;

        // other 错误
        // 网络错误
        public const int AccountNetError = -103001;

        // passport 其他错误
        public const int AccountPassportError = -100999;

        /// 未登录错误
        public const int AccountNotLoggedIn = -109800;

        // Android 独有渠道
        public const int AccountChannelError = -104998;

        // gsdk server 其他错误
        public const int AccountGServerError = -104999;

        // 客户端其他错误
        public const int AccountGClientError = -105999;
    }
}