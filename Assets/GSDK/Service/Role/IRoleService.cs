using System.Collections.Generic;


namespace GSDK
{
    #region delegates
    /// <summary>
    /// 获取区列表的回调
    /// </summary>
    /// <param name="result">包含了错误码的结果</param>
    /// <param name="zones">
    /// 一个List，里面装着每个区的信息
    /// <see cref="ZoneInfo"/>
    /// </param>
    public delegate void FetchZonesListDelegate(Result result, List<ZoneInfo> zones);
    
    
    /// <summary>
    /// 获取角色列表的回调
    /// </summary>
    /// <param name="result">包含了错误码的结果</param>
    /// <param name="roles">
    /// 一个List，里面装着每个角色的信息
    /// <see cref="RoleInfo"/>
    /// </param>
    public delegate void FetchRolesListDelegate(Result result, List<RoleInfo> roles);
    
    
    /// <summary>
    /// 获取区和角色列表的回调
    /// </summary>
    /// <param name="result">包含了错误码的结果</param>
    /// <param name="zones">
    /// 区列表
    /// <see cref="ZoneInfo"/>
    /// </param>
    /// <param name="roles">
    /// 角色列表
    /// <see cref="RoleInfo"/>
    /// </param>
    public delegate void FetchZonesAndRolesListDelegate(Result result, List<ZoneInfo> zones, List<RoleInfo> roles);


    /// <summary>
    /// 上报角色信息回调
    /// </summary>
    /// <param name="result">包含有错误码的请求结果，可通过 result.IsSuccess 来判断请求是否成功
    /// <para>
    /// 可能发生错误码：
    /// OpenIDInvalidInRole: openID不存在，请重新登录
    /// </para>
    /// </param>
    public delegate void RoleSaveRoleInfoDelegate(Result result);
    
    
    /// <summary>
    /// ping服务器的回调
    /// </summary>
    /// <remarks>
    /// 由于调用接口时传入的是多个服务器，这个回调也会返回多次，一次返回一个服务器的结果
    /// </remarks>
    /// <param name="result">包含了错误码的结果</param>
    /// <param name="serverInfo">
    /// 服务器信息
    /// <see cref="ServerInfo"/>
    /// </param>
    /// <param name="isFinished">
    /// 是否已ping完
    /// <remarks>目前存在bug，永远只会返回false</remarks>
    /// </param>
    public delegate void PingServerListDelegate(Result result, ServerInfo serverInfo, bool isFinished);
    #endregion
    
    #region static service class
    public class Role
    {
        public static IRoleService Service
        {
            get
            {
                return ServiceProvider.Instance.GetService(ServiceType.Role) as IRoleService;
            }
        }
    }
    #endregion

    #region IRoleService
    public interface IRoleService : IService
    {
        /// <summary>
        /// 获取区列表，数据从回调中获得
        /// </summary>
        /// <param name="gameVersion">
        /// 游戏版本
        /// <remarks>
        /// 需要跟游戏在运维平台上配置的gameVersion一致，否则区列表会返回空
        /// </remarks>
        /// </param>
        /// <param name="fetchZonesListCallback">回调方法</param>
        /// <param name="timeout">超时时间，单位s（可选参数，默认15s）</param>
        /// <param name="extraInfo">附加参数，例如区服tag，需用JSON字符串</param>
        void FetchZonesList(string gameVersion, FetchZonesListDelegate fetchZonesListCallback, double timeout = double.NaN, string extraInfo = "");

        
        /// <summary>
        /// 获取角色列表，数据从回调中获得
        /// </summary>
        /// <param name="fetchRolesListCallback">回调方法</param>
        /// <param name="timeout">超时时间，单位s（可选参数，默认15s）</param>
        void FetchRolesList(FetchRolesListDelegate fetchRolesListCallback, double timeout = double.NaN);

        
        /// <summary>
        /// 获取区和角色列表，数据从回调中获得
        /// </summary>
        /// <param name="gameVersion">
        /// 游戏版本
        /// <remarks>
        /// 需要跟游戏在运维平台上配置的gameVersion一致，否则区列表会返回空
        /// </remarks>
        /// </param>
        /// <param name="fetchZonesAndRolesListCallback">回调方法</param>
        /// <param name="timeout">超时时间，单位s（可选参数，默认15s）</param>
        /// <param name="extraInfo">附加参数，例如区服tag，需用JSON字符串</param>
        void FetchZonesAndRolesList(string gameVersion, FetchZonesAndRolesListDelegate fetchZonesAndRolesListCallback, double timeout = double.NaN, string extraInfo = "");

        
        /// <summary>
        /// ping服务器，数据从回调中获得
        /// </summary>
        /// <param name="servers">需要ping的服务器列表</param>
        /// <param name="pingServerListCallback">
        /// 回调方法
        /// <remarks>这个回调会返回多次，一次返回一个服务器的结果</remarks>
        /// </param>
        /// <param name="timeout">超时时间，单位s（可选参数，默认15s）</param>
        void PingServerList(List<ServerInfo> servers, PingServerListDelegate pingServerListCallback, double timeout = double.NaN);
        
    }
    #endregion

    #region public defines
    public class ZoneInfo
    {
        /// <summary>
        /// 区名
        /// </summary>
        public string ZoneName;
        /// <summary>
        /// 区ID
        /// </summary>
        public int ZoneID;
        /// <summary>
        /// 渠道ID
        /// </summary>
        public string ChannelID;
        /// <summary>
        /// cp自定义数据  
        /// </summary>
        public string ExtraInfo;
        /// <summary>
        /// cp自定义数据，k-v都是字符串
        /// </summary>
        public Dictionary<string, string> ExtraKV;
        /// <summary>
        /// 服信息列表
        /// </summary>
        public List<ServerInfo> Servers;   
    }

    /// <summary>
    /// 服务器属性
    /// </summary>
    public class ServerInfo
    {
        /// <summary>
        /// 服务器ID
        /// </summary>
        public int ServerId;
        /// <summary>
        /// 服务器名称
        /// </summary>
        public string ServerName;
        /// <summary>
        /// 服务器类型
        /// </summary>
        public ServerType ServerType;
        /// <summary>
        /// 服务器入口地址
        /// </summary>
        public string ServerEntry;
        /// <summary>
        /// 实际服务器ID
        /// </summary>
        public int RealServerId;
        /// <summary>
        /// 是否已合服
        /// </summary>
        public bool IsMerged;
        /// <summary>
        /// 服务器运维状态
        /// </summary>
        public ServerStatus ServerStatus;
        /// <summary>
        /// 在线人数状态
        /// </summary>
        public ServerOnlineLoad OnlineLoad;
        /// <summary>
        /// 扩展字段
        /// </summary>
        public string ExtraInfo;
        /// <summary>
        /// 服自定义Tag数据
        /// </summary>
        public List<ServerTag> Tags;
        /// <summary>
        /// 开服时间unix时间戳UTC
        /// </summary>
        public int OpenTimestamp;
        /// <summary>
        /// 自定义参数
        /// </summary>
        public Dictionary<string, string> ExtraKV;

        // 与ping接口相关的字段

        /// <summary>
        /// 用于ping测试的地址
        /// </summary>
        public string PingAddr;
        /// <summary>
        /// ping测试时间，调用ping接口后会对其赋值ping服的时间，默认为0
        /// </summary>
        public int Time;              
    }

    public enum ServerType
    {
        None = 0,
        /// <summary>
        /// 正式服
        /// </summary>
        Formal = 1,
        /// <summary>
        /// 体验服
        /// </summary>
        Experience = 2,
        /// <summary>
        /// 审核服
        /// </summary>
        Audit = 3,
        /// <summary>
        /// 测试服
        /// </summary>
        Test = 4           
    }

    public enum ServerStatus
    {
        None = 0,
        /// <summary>
        /// 在线
        /// </summary>
        Online = 1,
        /// <summary>
        /// 维护
        /// </summary>
        InMaintenance = 2,
        /// <summary>
        /// 离线
        /// </summary>
        Offline = 3            
    }

    public enum ServerOnlineLoad
    {
        /// <summary>
        /// 空闲
        /// </summary>
        Idle = 1,
        /// <summary>
        /// 繁忙
        /// </summary>
        Busy = 2,
        /// <summary>
        /// 爆满
        /// </summary>
        Full = 3,
        /// <summary>
        /// 自动
        /// </summary>
        Auto = 9,    
    }
    
    public class ServerTag
    {
        public string TagName;
        public int TagValue;        
    }

    public class RoleInfo
    {
        /// <summary>
        /// 角色ID
        /// </summary>
        public string RoleId;
        /// <summary>
        /// 角色名称
        /// </summary>
        public string RoleName;
        /// <summary>
        /// 角色等级
        /// </summary>
        public string RoleLevel;
        /// <summary>
        /// 实际所在区服ID
        /// </summary>
        public int RealServerId;
        /// <summary>
        /// 游戏服名称
        /// </summary>
        public string ServerName;
        /// <summary>
        /// 登录时间
        /// </summary>
        public long LoginTime;
        /// <summary>
        /// 头像
        /// </summary>
        public string AvatarUrl;
        /// <summary>
        /// 职业
        /// </summary>
        public string Job;
        /// <summary>
        /// 埋点穿透字段
        /// </summary>
        public string ExtraInfo;         
    }

    public static partial class ErrorCode
    {        
        /// <summary>
        /// 网络错误
        /// </summary>
        public const int RoleNetWorkError = -133001;
        
        /// <summary>
        /// 参数错误
        /// </summary>
        public const int RoleInvalidArguments = -130001;
        
        /// <summary>
        /// AccessToken无效/过期
        /// </summary>
        public const int RoleAccessTokenError = -130002;

        /// <summary>
        /// 区服列表为空
        /// </summary>
        public const int RoleZonesListIsEmpty = -130003;
        
        /// <summary>
        /// sdk_open_id有误
        /// </summary>
        public const int RoleOpenIDInvalid = -130004;
        
        /// <summary>
        /// 命中风控策略
        /// </summary>
        public const int RoleHitServerSecurityPolicy = -130005;
        
        /// <summary>
        /// 服务端错误
        /// </summary>
        public const int RoleSystemError = -130006;
        
        /// <summary>
        /// 角色通用错误，一般在出现未知错误时返回
        /// 此时请通过Result类中Message类查看具体错误信息。
        /// 如果Message类无具体信息，则通过ExtraCode1查看具体的错误码，通过onCall咨询
        /// </summary>
        public const int RoleUnknownError = -139999;
    }
    #endregion

}