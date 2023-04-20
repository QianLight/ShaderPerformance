using System.Collections.Generic;
using UNBridgeLib.LitJson;

#if UNITY_ANDROID
namespace GSDK
{

    #region delegates
    
    /// <summary>
    /// 退出游戏的回调
    /// </summary>
    /// <param name="result">判断返回是否成功</param>
    /// <param name="exitGameInfo">渠道是否有自己的退出dialog</param>
    public delegate void ChannelExitGameInfoDelegate(Result result, ExitGameInfo exitGameInfo);
    
    /// <summary>
    /// 渠道通用接口回调
    /// </summary>
    /// <param name="result">判断返回是否成功</param>
    /// <param name="generalResult">通用数据</param>
    public delegate void ChannelGeneralDelegate(Result result, GeneralResult generalResult);
    
    /// <summary>
    /// 发生登出时回调
    /// </summary>
    /// <param name="result">判断返回是否成功</param>
    public delegate void ChannelLogoutEventHandler(Result result);

    /// <summary>
    /// 发生帐号切换时回调
    /// </summary>
    /// <param name="loginResult">登录账号相关信息</param>
    public delegate void ChannelSwitchAccountEventHandler(LoginResult loginResult);

    /// <summary>
    /// 发生游戏退出时回调
    /// </summary>
    /// <param name="exitResult">是否退出游戏</param>
    public delegate void ChannelExitChannelEventHandler(ExitResult exitResult);

    #endregion

    # region static service class

    public static class Channel
    {
        public static IChannelService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.Channel) as IChannelService; }
        }
    }

    #endregion

    #region IChannelService

    public interface IChannelService : IService
    {
        #region 回调Event
        event ChannelLogoutEventHandler LogoutChannelEvent;
    
        event ChannelSwitchAccountEventHandler SwitchAccountChannelEvent;
    
        event ChannelExitChannelEventHandler ExitChannelEvent;
    
        #endregion
        
        #region Methods

        /// <summary>
        /// 玩家进入游戏区服时，需要调用此接口，上传游戏角色信息，如果游戏中没有相应信息则不填
        /// </summary>
        /// <param name="gameUploadInfo">需要上传的游戏角色信息</param>
        void EnterGameUpload(GameUploadInfo gameUploadInfo);

        /// <summary>
        /// 创建新角色时，需要调用此接口，上传游戏角色信息，如果游戏中没有相应信息就填入默认值：空字符串等
        /// </summary>
        /// <param name="gameUploadInfo">需要上传的游戏角色信息</param>
        void CreateNewRoleUpload(GameUploadInfo gameUploadInfo);

        /// <summary>
        /// 在角色等级发生变化时，需要调用此接口，上传游戏角色信息，
        /// 如果游戏中没有相应信息就填默认值，如果有相应信息，请务必填上，以免无法通过渠道的人工审核
        /// </summary>
        /// <param name="gameUploadInfo">需要上传的游戏角色信息</param>
        void RoleLevelUpload(GameUploadInfo gameUploadInfo);

        /// <summary>
        /// 角色退出游戏时调用该接口，需要调用此接口，上传游戏角色信息，如果游戏中没有相应信息则不填
        /// </summary>
        /// <param name="gameUploadInfo">需要上传的游戏角色信息</param>
        void RoleExitUpload(GameUploadInfo gameUploadInfo);

        /// <summary>
        /// 用户选择退出游戏时调用，比如返回键等
        /// </summary>
        /// <param name="exitGameCallback">退出游戏回调</param>
        void ExitGame(ChannelExitGameInfoDelegate exitGameCallback);

        /// <summary>
        /// 登陆成功后进行注册，监听SDK中主动抛出的帐号退出和帐号切换成功的回调信息，做游戏帐号的操作
        /// </summary>
        void RegisterAccountStatusChangedListener();

        /// <summary>
        /// 调用用户中心、切换帐号、登出、帐号设置密码接口只在字节或部分渠道可以使用，
        /// 所以在调用这些接口前，需要判断是否可用，从而方便游戏定制界面，
        /// 这样就可以游戏的设置界面调用此接口来判断是否要显示实名认证按钮
        /// </summary>
        /// <param name="apiName">接口名称</param>
        void IsApiAvailable(string apiName);

        /// <summary>
        /// 注册通用回调
        /// </summary>
        /// <param name="channelGeneralCallback">通用回调结果</param>
        void ChannelGeneralCallback(ChannelGeneralDelegate channelGeneralCallback);

        /// <summary>
        /// 发起通用行为
        /// </summary>
        /// <param name="jsonData">通用数据</param>
        void ChannelGeneral(JsonData jsonData);
        #endregion
    }

    #endregion

    #region public defines

    /// <summary>
    /// 游戏角色信息
    /// </summary>
    public class GameUploadInfo
    {
        /// <summary>
        ///余额货币种类id list
        /// </summary>
        public List<string> BalanceIds;

        /// <summary>
        ///余额货币种类name list
        /// </summary>
        public List<string> BalanceNames;

        /// <summary>
        /// 余额货币数额 list
        /// </summary>
        public List<string> BalanceNums;

        /// <summary>
        /// 关系角色id list
        /// </summary>
        public List<string> RoleIds;

        /// <summary>
        /// 亲密度 list
        /// </summary>
        public List<string> Intimacys;

        /// <summary>
        /// 关系id list
        /// </summary>
        public List<string> NexusIds;

        /// <summary>
        /// 关系name list
        /// </summary>
        public List<string> NexusNames;

        /// <summary>
        /// 榜单id list
        /// </summary>
        public List<string> ListIds;

        /// <summary>
        ///榜单name list
        /// </summary>
        public List<string> ListNames;

        /// <summary>
        ///榜单排名name list
        /// </summary>
        public List<string> Nums;

        /// <summary>
        ///排名指标name list
        /// </summary>
        public List<string> Coins;

        /// <summary>
        ///排名指标名称 list
        /// </summary>
        public List<string> Costs;

        /// <summary>
        ///（必填）角色状态（enterServer（登录），levelUp（升级），createRole（创建角色），exitServer（退出））
        /// </summary>
        public string Type;

        /// <summary>
        ///（必填）游戏区服ID
        /// </summary>
        public string ZoneId;

        /// <summary>
        ///（必填）游戏区服名称
        /// </summary>
        public string ZoneName;

        /// <summary>
        /// （必填）玩家角色ID
        /// </summary>
        public string RoleId;

        /// <summary>
        ///（必填）玩家角色名
        /// </summary>
        public string RoleName;

        /// <summary>
        ///（必填）职业ID
        /// </summary>
        public string ProfessionId;

        /// <summary>
        ///（必填）职业名称
        /// </summary>
        public string Profession;

        /// <summary>
        ///（必填）性别
        /// </summary>
        public string Gender;

        /// <summary>
        ///（选填）职业称号ID
        /// </summary>
        public string ProfessionRoleId;

        /// <summary>
        ///（选填）职业称号
        /// </summary>
        public string ProfessionRoleName;

        /// <summary>
        ///（必填）玩家角色等级
        /// </summary>
        public string RoleLevel;

        /// <summary>
        ///（必填）战力数值
        /// </summary>
        public string Power;

        /// <summary>
        ///（必填）当前用户VIP等级
        /// </summary>
        public string Vip;

        /// <summary>
        ///（必填）帐号余额
        /// </summary>
        public string Balance;

        /// <summary>
        ///（必填）所属帮派帮派ID
        /// </summary>
        public string PartyId;

        /// <summary>
        ///（必填）所属帮派名称
        /// </summary>
        public string PartyName;

        /// <summary>
        ///（必填）帮派称号ID
        /// </summary>
        public string PartyRoleId;

        /// <summary>
        ///（必填）帮派称号名称
        /// </summary>
        public string PartyRoleName;

        /// <summary>
        ///（必填）好友关系
        /// </summary>
        public string FriendList;

        /// <summary>
        ///（选填）排行榜列表
        /// </summary>
        public string Ranking;

        /// <summary>
        ///游戏关卡
        /// </summary>
        public string Chapter;

        /// <summary>
        ///服区Id
        /// </summary>
        public string ServerId;

        /// <summary>
        ///服区name
        /// </summary>
        public string ServerName;
    }

    public class ExitGameInfo
    {
        /// <summary>
        /// true: 表明渠道有自己的退出dialog，游戏侧自杀进程即可
        /// false：渠道方没有自己的退出dialog，游戏侧自行弹出退出确认框
        /// </summary>
        public bool HasDialog;
    }

    public class LoginResult
    {
        /// <summary>
        /// 用于向服务端验证登录合法性
        /// </summary>
        public string Token;
        
        /// <summary>
        /// 账号相关信息
        /// </summary>
        public AccountInfo AccountInfo;
    }

    public class ExitResult
    {
        /// <summary>
        /// true: 表明渠道通知退出游戏
        /// false：表明渠道没有通知退出游戏
        /// </summary>
        public bool IsExit;
    }

    public class GeneralResult
    {
        /// <summary>
        /// 通用回调数据
        /// </summary>
        public string ExtraData;
    }

    #endregion
}
#endif
