using System.Collections.Generic;

namespace GSDK
{
    #region Delegate

    /// <summary>
    /// 加入房间的回调。
    /// </summary>
    /// <param name="result">事件的结果信息</param>
    /// <para>
    ///    当前接口可能返回的错误码:
    ///             Success:成功
    ///             RTCIDNil:AppID或RoomID或UID为空
    ///             RTCJoinRoomError:加入房间失败
    ///             RTCSdkError:SDK错误
    ///             RTCUnknownError:未知错误
    /// </para>
    /// <param name="joinRoomInfo">当前用户加入房间的信息</param>
    public delegate void JoinRoomDelegate(Result result, JoinRoomInfo joinRoomInfo);

    #endregion

    #region EventHandler

    /// <summary>
    /// 离开房间的事件。
    /// </summary>
    public delegate void LeaveRoomEventHandler();

    /// <summary>
    /// 错误事件。
    /// </summary>
    /// <param name="result">事件的结果信息</param>
    /// <para>
    ///    当前接口可能返回的错误码:
    ///             RTCTokenInvalidOrExpired:Token不合法或者过期，如果出现此错误码，可以使用UpdateToken更新token，恢复与房间的连接
    ///             RTCNoPublishPermission:没有发布流的权限
    ///             RTCNoSubscribePermission:没有订阅流的权限
    ///             RTCLoginDuplicate:用户在其它设备上重复登录	
    ///             RTCJoinRoomError:加入房间失败
    ///             RTCAudioDeviceNoUsePermission:没有音频权限(单次拒绝)
    ///             RTCAudioDeviceNoUsePermissionWithoutPrompt:没有音频权限（永久拒绝）
    ///             RTCUserOrRoomIDInvalid:用户ID 或者RoomID 存在非法字符
    ///             RTCInternetNoPermission:没有网络权限
    ///             RTCUnknownError:未知错误
    /// </para>
    public delegate void ErrorEventHandler(Result result);

    /// <summary>
    /// 其它用户加入当前房间的事件。
    /// </summary>
    /// <param name="otherUserJoinInfo">其它用户加入房间的信息</param>
    public delegate void OtherUserJoinEventHandler(OtherUserJoinInfo otherUserJoinInfo);

    /// <summary>
    /// 其它用户离开当前房间的事件。
    /// </summary>
    /// <param name="otherUserLeaveInfo">其它用户离开房间的信息</param>
    public delegate void OtherUserLeaveEventHandler(OtherUserLeaveInfo otherUserLeaveInfo);

    /// <summary>
    /// 当前房间内，其它用户开启/关闭发送音频（麦克风）的事件。
    /// </summary>
    /// <param name="otherUserMuteAudioInfo">其它用户开启/关闭发送音频的信息</param>
    public delegate void OtherUserMuteSendAudioEventHandler(OtherUserMuteAudioInfo otherUserMuteAudioInfo);

    /// <summary>
    /// 当前房间内，其它用户开启/关闭接收音频（扬声器）的事件。
    /// </summary>
    /// <param name="otherUserMuteAudioInfo">其它用户开启/关闭接收音频的信息</param>
    public delegate void OtherUserMuteRecvAudioEventHandler(OtherUserMuteAudioInfo otherUserMuteAudioInfo);

    /// <summary>
    /// 当前房间内，正在说话的用户（包括自己）的事件。
    /// </summary>
    /// <param name="audioVolumeIndicationInfo">正在说话的用户信息</param>
    public delegate void AudioVolumeIndicationEventHandler(AudioVolumeIndicationInfo audioVolumeIndicationInfo);

    /// <summary>
    /// 语音连接中断的事件。
    /// </summary>
    public delegate void ConnectionLostEventHandler();

    #endregion
    
    /// <summary>
    /// 用于获取实例进行接口调用
    /// e.g.RTC.Service.MethodName();
    /// </summary>
    public static class RTC
    {
        public static IRTCService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.RTC) as IRTCService; }
        }
    }

    /// <summary>
    /// 提供即时通讯的能力（参考某5v5moba手游的队内语音交流）；
    /// </summary>
    public interface IRTCService : IService
    {
        #region Events

        /// <summary>
        /// 本人离开房间后触发该事件
        /// </summary>
        event LeaveRoomEventHandler LeaveRoomEvent; 
        /// <summary>
        /// 出现任何错误时，都会触发该事件
        /// </summary>
        event ErrorEventHandler ErrorEvent; 
        /// <summary>
        /// 其它用户加入房间后触发
        /// </summary>
        event OtherUserJoinEventHandler OtherUserJoinEvent; 
        /// <summary>
        /// 其它用户离开房间后触发
        /// </summary>
        event OtherUserLeaveEventHandler OtherUserLeaveEvent; 
        /// <summary>
        /// 其它用户静音/取消静音发送音频（麦克风）触发
        /// </summary>
        event OtherUserMuteSendAudioEventHandler OtherUserMuteSendAudioEvent; 
        /// <summary>
        /// 其它用户静音/取消静音接收音频（扬声器）触发
        /// </summary>
        event OtherUserMuteRecvAudioEventHandler OtherUserMuteRecvAudioEvent; 
        /// <summary>
        /// 有人说话时（包括自己）会触发
        /// </summary>
        event AudioVolumeIndicationEventHandler AudioVolumeIndicationEvent; 
        /// <summary>
        /// 本人连接丢失会触发
        /// </summary>
        event ConnectionLostEventHandler ConnectionLostEvent;

        #endregion

        #region Methods

        /// <summary>
        /// 初始化RTC（使用RTC必调，推荐在Start中调用）。
        /// </summary>
        /// <param name="clientRole">设置角色，主播模式或观众模式（局部语音模式下该配置无效）</param>
        void Initialize(ClientRole clientRole = ClientRole.Broadcaster);

        /// <summary>
        /// 加入房间。
        /// </summary>
        /// <param name="roomID">房间ID，不支持中文</param>
        /// <param name="userID">用户ID，不支持中文</param>
        /// <param name="token">鉴权使用的token，生成步骤可以移步文档站（游戏服务-游戏语音-RTC实时通信token）</param>
        /// <param name="joinRoomCallback">加入房间的回调</param>
        void JoinRoom(string roomID, string userID, string token, JoinRoomDelegate joinRoomCallback);

        /// <summary>
        /// 离开房间。(再次加入房间需要重新设置用户机型等级、小队ID、是否开启区域语音)
        /// </summary>
        void LeaveRoom();
        
        /// <summary>
        /// 更新token(建议在返回token过期错误码后调用该接口，该接口目前仅支持iOS，Android还未实现)。
        /// </summary>
        /// <param name="token">鉴权使用的token，生成步骤可以移步文档站（游戏服务-游戏语音-RTC实时通信token）</param>
        void UpdateToken(string token);

        /// <summary>
        /// 关闭打开音频采集，目前仅P5需要使用（不等于静音，需要重启音频，会消耗时间，一般业务使用muteLocalAudioStream本地静音）。（需要在加入房间后调用）
        /// </summary>
        /// <param name="mute">静音状态，true：静音，false：不静音</param>
        void EnableLocalAudio(bool enabled);

        /// <summary>
        /// 加入房间前调用，选择默认情况下关闭/打开音频采集，目前仅P5需要使用（不等于静音，需要重启音频，会消耗时间，一般业务使用muteLocalAudioStream本地静音）
        /// </summary>
        /// <param name="mute">静音状态，true：静音，false：不静音</param>
        void SetDefaultEnableLocalAudio(bool enabled);

        /// <summary>
        /// 调整发送音量。（需要在加入房间后调用）
        /// </summary>
        /// <param name="volume">
        ///     发送音量可以在0~200范围内进行调节
        ///     0:静音
        ///     100:原始音量
        ///     200:最大为原始音量的2倍
        /// </param>
        
        void AdjustSendAudioVolume(int volume);

        /// <summary>
        /// 设置发送音频流的静音状态。（需要在加入房间后调用）
        /// </summary>
        /// <param name="mute">静音状态，true：静音，false：不静音</param>
        void MuteSendAudioStream(bool mute);

        /// <summary>
        /// 调整接收音量的大小（可以指定用户）。（需要在加入房间后调用）
        /// </summary>
        /// <param name="volume">
        ///     接收音量可以在0~200范围内进行调节:
        ///     0:静音;
        ///     100:原始音量;
        ///     200:最大为原始音量的2倍;
        /// </param>
        /// <param name="userID">用户ID，不支持中文</param>
        void AdjustRecvAudioVolume(string userID, int volume);

        /// <summary>
        /// 调整接收音量的大小（对总音量的大小进行调整）。（需要在加入房间后调用）
        /// </summary>
        /// <param name="volume">
        ///     接收音量可以在0~200范围内进行调节:
        ///     0:静音;
        ///     100:原始音量;
        ///     200:最大为原始音量的2倍;
        /// </param>
        void AdjustAllRecvAudioVolume(int volume);

        /// <summary>
        /// 设置特定接收音频流的静音状态（可以指定用户）。（需要在加入房间后调用）
        /// </summary>
        /// <param name="userID">用户ID，不支持中文</param>
        /// <param name="mute">静音状态，true：静音，false：不静音</param>
        void MuteRecvAudioStream(string userID, bool mute);

        /// <summary>
        /// 设置所有接收音频流的静音状态（静音全部，或解除静音全部）。（需要在加入房间后调用）
        /// </summary>
        /// <param name="mute">静音状态，true：静音，false：不静音</param>
        void MuteAllRecvAudioStreams(bool mute);

        /// <summary>
        /// 设置用户角色（局部语音模式下，该接口失效）。（需要在加入房间后调用）
        /// </summary>
        /// <param name="clientRole">主播模式或观众模式（局部语音模式下该配置无效）</param>
        void SetClientRole(ClientRole clientRole);

        /// <summary>
        /// 设置用户机型等级。（该接口只能在JoinRoom前调用）
        /// </summary>
        /// <param name="performanceProfileLevel">机型等级</param>
        void SetPerformanceProfileLevel(PerformanceProfileLevel performanceProfileLevel);

        // 以下接口仅提供给局部语音模式使用（开启局部语音模式的方法是在JoinRoom前调用SetTeamID）
        // 局部语音模式是指在世界频道的基础上增加了小队的概念
        // 使用场景：
        //     1.手游Moba（三排时）的全队语音（五人之间的实时语音，RoomID相同）和开黑语音（三人之间的实时语音，TeamID相同）
        //     2.大逃杀游戏（吃鸡）的世界语音（一百个人，RoomID相同）和四人小队语音（四个人，TeamID相同）
        // 小队频道是世界频道的子集，一个世界频道可以有无数个小队频道
        // 可以通过以下接口设置发送/接收模式为小队或世界

        #region 局部语音模式接口

        /// <summary>
        /// 设置小队ID（该接口只能在JoinRoom前调用；JoinRoom后，该接口失效，需要先调LeaveRoom才能再次使用该接口）。
        /// 如果设置TeamID，则是局部语音模式；如果不设置TeamID，则默认是普通语音模式。
        /// </summary>
        /// <param name="teamID">小队ID，不支持中文</param>
        /// <returns>true:调用成功;false:调用失败(未初始化||已经加入房间不支持更改TeamID)</returns>
        bool SetTeamID(string teamID);

        /// <summary>
        /// 设置音频发送模式。（需要在加入房间后调用）
        /// </summary>
        /// <param name="mode">局部语音模式，有静音、小队、世界三种值可选</param>
        /// <returns>true:调用成功;false:调用失败(未初始化||非局部语音模式)</returns>
        bool SetAudioSendMode(RTCRangeAudioMode mode);

        /// <summary>
        /// 设置音频接收模式。（需要在加入房间后调用）
        /// </summary>
        /// <param name="mode">局部语音模式，有静音、小队、世界三种值可选</param>
        /// <returns>true:调用成功;false:调用失败(未初始化||非局部语音模式)</returns>
        bool SetAudioRecvMode(RTCRangeAudioMode mode);

        // 区域语音:语音音量大小随距离而变化。（该功能仅在局部语音模式下有效，且默认开启，需要更改可以使用EnableRangeAudio）
        //     开启该功能后，收听非同一小队的音量大小会随距离而变化，可以使用UpdateAudioRecvRange更改接收范围、UpdateSelfPosition更改位置。
        //     关闭该功能后，收听非同一小队的音量大小与距离无关。

        #region 区域语音接口

        /// <summary>
        /// 设置是否开启区域语音，默认开启（该接口只能在JoinRoom前调用；JoinRoom后，该接口失效，需要先调LeaveRoom才能再次使用该接口）。
        /// </summary>
        /// <param name="enable">是否开启区域语音</param>
        /// <returns>true:调用成功;false:调用失败(未初始化||已经加入了房间，不支持修改)</returns>
        bool EnableRangeAudio(bool enable);

        /// <summary>
        /// 更新音频接收范围。（需要在加入房间后调用）
        /// 衰减公式：( x代表距离 ,g(x)代表音量值，范围: 0~1.0 )
        /// x在[0,minRange)时, g(x) = 1.0;
        /// x在[minRange,maxRange]时, g(x) = minRange/x;
        /// x在(maxRange,+∞)时, g(x) = 0.
        /// </summary>
        /// <param name="minRange">最小收听范围</param>
        /// <param name="maxRange">最大收听范围</param>
        /// <returns>true:调用成功;false:调用失败(未初始化||非局部语音模式||关闭了区域语音功能||参数不合理)</returns>
        bool UpdateAudioRecvRange(int minRange, int maxRange);

        /// <summary>
        /// 更新自己的位置。（需要在加入房间后调用）
        /// </summary>
        /// <param name="x">x坐标</param>
        /// <param name="y">y坐标</param>
        /// <param name="z">z坐标</param>
        /// <returns>true:调用成功;false:调用失败(未初始化||非局部语音模式||关闭了区域语音功能)</returns>
        bool UpdateSelfPosition(int x, int y, int z);

        #endregion

        #endregion

        #endregion
    }
    
    
    #region PublicDefinitions RTC使用的所有定义

    public enum ClientRole
    {
        /// <summary>
        /// 主播
        /// </summary>
        Broadcaster = 1,

        /// <summary>
        /// 观众（只能听不能说）
        /// </summary>
        Audience = 2
    }

    public enum PerformanceProfileLevel
    {
        /// <summary>
        /// 自动检测机型
        /// </summary>
        Auto = 0,

        /// <summary>
        /// 低端机型
        /// </summary>
        Low = 1,

        /// <summary>
        /// 中端机型
        /// </summary>
        Medium = 2,

        /// <summary>
        /// 高端机型
        /// </summary>
        High = 3,
    }

    public enum LeaveRoomReason
    {
        /// <summary>
        /// 用户主动离开
        /// </summary>
        Initiative = 0,

        /// <summary>
        /// 因过长时间收不到对方数据包，超时掉线
        /// </summary>
        NetworkTimeout = 1,
    }

    public enum RTCRangeAudioMode
    {
        /// <summary>
        /// 静音
        /// </summary>
        Mute = 0,

        /// <summary>
        /// 小队范围
        /// </summary>
        Team = 1,

        /// <summary>
        /// 世界范围
        /// </summary>
        World = 2,
    }

    public struct JoinRoomInfo
    {
        /// <summary>
        /// 房间ID，不支持中文
        /// </summary>
        public string RoomID;

        /// <summary>
        /// 用户ID，不支持中文
        /// </summary>
        public string UserID;

        /// <summary>
        /// 从Native加入频道开始到该回调触发的延迟，单位：毫秒
        /// </summary>
        public int Elapsed;
    }

    public struct OtherUserJoinInfo
    {
        /// <summary>
        /// 用户ID，不支持中文
        /// </summary>
        public string UserID;

        /// <summary>
        /// 从Native加入频道开始到该回调触发的延迟，单位：毫秒
        /// </summary>
        public int Elapsed;
    }

    public struct OtherUserLeaveInfo
    {
        /// <summary>
        /// 用户ID，不支持中文
        /// </summary>
        public string UserID;

        /// <summary>
        /// 用户离开房间的原因
        /// </summary>
        public LeaveRoomReason Reason;
    }

    public struct OtherUserMuteAudioInfo
    {
        /// <summary>
        /// 用户ID，不支持中文
        /// </summary>
        public string UserID;

        /// <summary>
        /// 是否静音，true：静音，false：不静音
        /// </summary>
        public bool IsMuted;
    }

    public struct AudioVolumeIndicationInfo
    {
        /// <summary>
        /// 房间内当前说话者的信息
        /// </summary>
        public List<SpeakerInfo> Speakers;

        /// <summary>
        /// 房间内所有用户的总音量，范围：[0,255]
        /// </summary>
        public int TotalVolume;
    }

    public struct SpeakerInfo
    {
        /// <summary>
        /// 用户ID，不支持中文
        /// </summary>
        public string UserID;

        /// <summary>
        /// 用户的音量，范围：[0,255]
        /// </summary>
        public int Volume;
    }
    
    #endregion
    
    /// <summary>
    /// RTC可能出现的错误码
    /// </summary>
    public static partial class ErrorCode
    {
        /// <summary>
        /// SDK错误
        /// </summary>
        public const int RTCSdkError = -361100;

        /// <summary>
        /// 参数错误
        /// </summary>
        public const int RTCIDNil = -360100;

        /// <summary>
        /// Token不合法或者过期，如果出现此错误码，可以使用UpdateToken更新token，恢复与房间的连接
        /// </summary>
        public const int RTCTokenInvalidOrExpired = -360000;

        /// <summary>
        /// 加入房间失败
        /// </summary>
        public const int RTCJoinRoomError = -360001;

        /// <summary>
        /// 没有发布流的权限
        /// </summary>
        public const int RTCNoPublishPermission = 360002;

        /// <summary>
        /// 没有订阅流的权限
        /// </summary>
        public const int RTCNoSubscribePermission = -360003;

        /// <summary>
        /// 用户在其它设备上重复登录	
        /// </summary>
        public const int RTCLoginDuplicate = -360004;

        /// <summary>
        /// 没有音频权限（永久拒绝）
        /// </summary>
        public const int RTCAudioDeviceNoUsePermissionWithoutPrompt = -360005;

        /// <summary>
        /// 没有音频权限(单次拒绝)
        /// </summary>
        public const int RTCAudioDeviceNoUsePermission = -362001;

        /// <summary>
        /// 未知错误
        /// </summary>
        public const int RTCUnknownError = -369999;
    }
}