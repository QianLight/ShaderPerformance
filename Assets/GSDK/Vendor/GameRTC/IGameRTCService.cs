using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameRTC
{
    #region EventHandler
    /** {zh}
     * @type callback
     * @region 房间管理
     * @author chuzhongtao
     * @brief 用户首次成功加入房间/重连进房后，收到此回调。
     * @param roomID 房间 ID
     * @param userID 用户 ID
     * @param errorCode 用户加入房间回调的状态码:  <br>
     *        + 0: 成功  <br>
     *        + !0: 失败  <br>
     *        + 具体原因参看错误码 JoinRoomErrorCode{@link #JoinRoomErrorCode} 以及警告码 RoomWarnCode{@link #RoomWarnCode}。
     * @param isRejoined 是否为重连进房: <br>
     *        + true: 断网重连后 SDK 内部自动进房；  <br>
     *        + false: 首次进房。
     * @param elapsed 进房耗时。用户从调用 JoinRoom{@link #JoinRoom} 方法到加入房间成功所经历的时间间隔，单位为 ms。
     */
    public delegate void OnJoinRoomResultEventHandler(string roomID,
        string userID, JoinRoomErrorCode errorCode, bool isRejoined, int elapsed);

    /** {zh}
     * @type callback
     * @region 房间管理
     * @author chuzhongtao
     * @brief 离开房间成功回调 <br>
     *        用户调用 LeaveRoom{@link #LeaveRoom} 方法后，SDK 会停止所有的发布订阅流，并释放所有通话相关的音视频资源。SDK 完成所有的资源释放后，用户收到此回调。
     * @param roomID 房间 ID
     * @notes  <br>
     *       + 离开房间结束通话后，如果 App 需要使用系统音视频设备，建议在收到此回调后再初始化音视频设备，否则可能由于被 SDK 占用而导致 App 初始化音视频设备失败。  <br>
     *       + 用户调用 LeaveRoom{@link #LeaveRoom} 方法离开房间后，如果立即调用 Release{@link #Release} 方法销毁 GameRTC 引擎，则将无法收到此回调事件。
     */
    public delegate void OnLeaveRoomEventHandler(string roomID);

    /** {zh}
     * @type callback
     * @region 房间管理
     * @author chuzhongtao
     * @brief 远端用户加入房间回调。<br>
     *        以下情形下，本地用户会收到此回调：<br>
     *        + 远端用户首次加入房间/重连加入房间；<br>
     *        + 本地用户加入房间时，会收到回调，提示已在房间中的远端用户。
     * @param roomID 房间 ID
     * @param userID 用户 ID
     */
    public delegate void OnUserJoinedEventHandler(string roomID, string userID);

    /** {zh}
     * @type callback
     * @region 房间管理
     * @author chuzhongtao
     * @brief 远端用户离开房间回调。<br>
     *        房间中有用户主动调用 LeaveRoom{@link #LeaveRoom} 方法离开房间或断线超时离开房间时，房间中其他用户都会收到此回调。
     * @param roomID 房间 ID
     * @param userID 用户 ID
     * @param reason 用户离开的原因。参看 UserLeaveReasonType{@link #UserLeaveReasonType}。
     */
    public delegate void OnUserLeaveEventHandler(string roomID, string userID, UserLeaveReasonType reason);

    /** {zh}
     * @type callback
     * @region 音频管理
     * @author chuzhongtao
     * @brief 远端用户开关麦克风回调。  <br>
     *        当用户调用 EnableMicrophone{@link #EnableMicrophone} 更改本地音频采集和发布状态时，房间内其他用户会收到此回调。
     * @param roomID 房间 ID
     * @param userID 更改本地音频采集和发布状态的用户的 ID
     * @param enable 是否开启麦克风  <br>
     *        + true: 打开麦克风 <br>
     *        + false: 关闭麦克风
     */
    public delegate void OnMicrophoneEnabledEventHandler(string roomID, string userID, bool enable);

    /** {zh}
     * @type callback
     * @region 音频管理
     * @author chuzhongtao
     * @brief 远端用户启停音频数据发送的回调。  <br>
     *        当用户调用 EnableAudioSend{@link #EnableAudioSend} 更改本地音频发送状态时，房间内其他用户会收到此回调。
     * @param roomID 房间 ID
     * @param userID 更改本地音频发送状态的用户的 ID
     * @param enable 是否开启音频发送  <br>
     *        + true: 开启音频发送 <br>
     *        + false: 停止音频发送 <br>
     */
    public delegate void OnAudioSendEnabledEventHandler(string roomID, string userID, bool enable);

    /** {zh}
     * @type callback
     * @region 音频管理
     * @author chuzhongtao
     * @brief 远端用户启停音频数据接收的回调。  <br>
     *        当用户调用 EnableSpeakerphone{@link #EnableSpeakerphone} 更改远端音频接收状态时，房间内其他用户会收到此回调。
     * @param roomID 房间 ID
     * @param userID 更改远端音频接收状态的用户的 ID
     * @param enable 是否接收远端音频  <br>
     *        + true: 开启接收远端音频 <br>
     *        + false: 停止接收远端音频 <br>
     */
    public delegate void OnSpeakerphoneEnabledEventHandler(string roomID, string userID, bool enable);

    /** {zh}
     * @type callback
     * @region 音频管理
     * @author chuzhongtao
     * @brief 提示本地采集的音量信息和在房间内订阅的远端用户的音量信息。<br>
     *        本回调默认不开启。你可以通过 AudioVolumeIndicationInterval{@link #AudioVolumeIndicationInterval} 开启。
     * @param roomID 房间 ID
     * @param speakers 本地用户和订阅的远端用户的 ID 和音量。参看 GameRTCAudioVolumeInfo{@link #GameRTCAudioVolumeInfo}。
     * @param totalVolume speakers 中包含的所有音频音量之和，取值范围是: [0,255]。
     * @notes  <br>
     *         + 对于本地用户：只要进行本地音频采集，回调内就会包含本地音频流的音量信息。<br>
     *         + 对于远端用户：本地必须订阅某远端用户的音频流，回调内才会包含其发送的音频流的音量信息。
     */
    public delegate void OnAudioVolumeIndicationEventHandler(string roomID,
        List<GameRTCAudioVolumeInfo> speakers, int totalVolume);

    /** {zh}
     * @type callback
     * @region 引擎管理
     * @author chuzhongtao
     * @brief 通话中用户的媒体流网络上下行质量报告回调。加入房间成功后，每隔 2 秒会收到此回调，通知房间内用户与媒体服务器之间数据交互的网络质量。
     * @param roomID 房间 ID
     * @param userID 回调的网络质量报告所属用户的用户 ID
     * @param txQuality 用户的媒体流上行网络质量，参看 NetworkQuality{@link #NetworkQuality}。
     * @param rxQuality 用户的媒体流下行网络质量，参看 NetworkQuality{@link #NetworkQuality}。
     * @notes <br>
     *        + 当 userId 为本地用户 ID 时，txQuality 为该用户的上行网络质量，rxQuality 为该用户的下行网络质量。<br>
     *        + 当 userId 为远端用户 ID 时，目前仅支持获取该用户的上行网络质量 txQuality，下行网络质量 rxQuality 为 0 。
     */
    public delegate void OnNetworkQualityEventHandler(string roomID, string userID,
        NetworkQuality txQuality, NetworkQuality rxQuality);

    /** {zh}
     * @type callback
     * @region 引擎管理
     * @author chuzhongtao
     * @brief 用户与 RTC 服务器连接状态发生改变时，收到此回调。
     * @param state 用户当前与 RTC 服务器的连接状态，参看 ConnectionState{@link #ConnectionState}。
     */
    public delegate void OnConnectionStateChangedEventHandler(ConnectionState state);

    /** {zh}
     * @type callback
     * @region 房间管理
     * @author chuzhongtao
     * @brief SDK 运行时，出现房间相关警告，收到此回调。
     * @param roomID 房间 ID
     * @param code 警告代码，详情定义参看 RoomWarnCode{@link #RoomWarnCode}。
     * @notes 通常情况下，SDK 上报的警告信息可以忽略，SDK 能够自动恢复。
     */
    public delegate void OnRoomWarningEventHandler(string roomID, RoomWarnCode code);

    /** {zh}
     * @type callback
     * @region 房间管理
     * @author chuzhongtao
     * @brief 房间相关错误回调。
     * @param roomID 房间 ID
     * @param code 错误码，参看 RoomErrorCode{@link #RoomErrorCode}。
     */
    public delegate void OnRoomErrorEventHandler(string roomID, RoomErrorCode code);

    /** {zh}
     * @type callback
     * @region 引擎管理
     * @author chuzhongtao
     * @brief SDK 运行时出现了引擎相关的警告时，收到此回调。
     * @param code 警告码，详情定义参看 EngineWarnCode{@link #EngineWarnCode}。
     * @notes 通常情况下，SDK 上报的警告信息可以忽略，SDK 能够自动恢复。
     */
    public delegate void OnEngineWarningEventHandler(EngineWarnCode code);

    #endregion


    /** {zh}
     * @type api
     * @brief 提供即时通讯的能力（参考某5v5moba手游的队内语音交流）；
     */
    public interface IGameRTCService
    {
        #region Events
        /** {zh}
         * @brief 进房成功回调
         */
        event OnJoinRoomResultEventHandler OnJoinRoomResultEvent;
        /** {zh}
         * @brief 退房成功回调
         */
        event OnLeaveRoomEventHandler OnLeaveRoomEvent;
        /** {zh}
         * @brief 远端用户加入房间回调
         */
        event OnUserJoinedEventHandler OnUserJoinedEvent;
        /** {zh}
         * @brief 远端用户离开房间回调
         */
        event OnUserLeaveEventHandler OnUserLeaveEvent;
        /** {zh}
         * @brief 远端用户开关麦克风回调
         */
        event OnMicrophoneEnabledEventHandler OnMicrophoneEnabledEvent;
        /** {zh}
         * @brief 远端用户启停音频数据发送回调
         */
        event OnAudioSendEnabledEventHandler OnAudioSendEnabledEvent;
        /** {zh}
         * @brief 远端用户启停音频数据接收回调
         */
        event OnSpeakerphoneEnabledEventHandler OnSpeakerphoneEnabledEvent;
        /** {zh}
         * @brief 音量统计信息回调
         */
        event OnAudioVolumeIndicationEventHandler OnAudioVolumeIndicationEvent;
        /** {zh}
         * @brief 网络状态改变的回调
         */
        event OnConnectionStateChangedEventHandler OnConnectionStateChangedEvent;
        /** {zh}
         * @brief 网络质量回调
         */
        event OnNetworkQualityEventHandler OnNetworkQualityEvent;
        /** {zh}
         * @brief 引擎警告回调
         */
        event OnEngineWarningEventHandler OnEngineWarningEvent;
        /** {zh}
         * @brief 房间警告回调
         */
        event OnRoomWarningEventHandler OnRoomWarningEvent;
        /** {zh}
         * @brief 房间错误回调
         */
        event OnRoomErrorEventHandler OnRoomErrorEvent;

        #endregion

        /** {zh}
         * @type api
         * @region 引擎管理
         * @author chuzhongtao
         * @brief 创建 GameRTCEngine 实例。  <br>
         *        GameRTCEngine 实例成功创建后，你才可以使用 SDK 提供的其他能力。
         * @param initParams 引擎初始化参数，参看 GameRTCEngineParams{@link #GameRTCEngineParams}。
         */
        void Initialize(GameRTCEngineParams initParams);

        /** {zh}
         * @type api
         * @region 引擎管理
         * @author chuzhongtao
         * @brief 销毁由 Initialize{@link #Initialize} 所创建引擎实例，并释放所有相关资源。  <br>
         * @notes  <br>
         *        + 你必须在所有业务场景的最后阶段调用该方法。该方法在调用之后，会销毁所有 SDK 相关的内存，并且停止与媒体服务器的任何交互。  <br>
         *        + 本方法为阻塞调用，会阻塞当前线程直到 SDK 彻底完成退出逻辑。因此，不可在回调线程中直接调用本方法；也不可在回调方法中等待主线程的执行而同时在主线程调用本方法，否则会造成死锁。
         */
        void Release();

        /** {zh}
         * @type api
         * @region 引擎管理
         * @author chuzhongtao
         * @brief 获取 SDK 当前的版本号。
         * @return SDK 当前的版本号。
         */
        string GetSdkVersion();

        /** {zh}
         * @type api
         * @region 房间管理
         * @author chuzhongtao
         * @brief 加入实时语音房间。 <br>
         *        在使用实时语音的通话功能时，必须先加入实时语音房间。  <br>
         *        调用该方法后，会收到 OnJoinRoomResultEventHandler{@link #OnJoinRoomResultEventHandler} 回调通知。
         * @param roomID 房间 ID，用户调用此接口加入的房间的房间 ID。<br>
         *        房间 ID 为长度在 128 字节内的非空字符串，支持以下字符集范围: <br>
         *            1. 26 个大写字母 A ~ Z 。<br>
         *            2. 26 个小写字母 a ~ z 。<br>
         *            3. 10 个数字 0 ~ 9 。<br>
         *            4. 下划线 "_"，at 符 "@"，减号 "-"。<br>
         * @param userID 用户 ID，用户调用此接口加入的房间时使用的用户 ID 。<br>
         *        用户 ID 为长度在 128 字节内的非空字符串，支持以下字符集范围:<br>
         *            1. 26 个大写字母 A ~ Z 。 <br>
         *            2. 26 个小写字母 a ~ z 。 <br>
         *            3. 10 个数字 0 ~ 9 。 <br>
         *            4. 下划线 "_"，at 符 "@"，减号 "-"。
         * @param token 动态密钥，用于对登录用户进行鉴权验证。<br>
         *        进入房间需要携带 Token。测试时可使用控制台生成临时 Token，正式上线需要使用密钥 SDK 在你的服务端生成并下发 Token。
         * @param roomConfig 房间配置参数，参看 GameRTCRoomConfig{@link #GameRTCRoomConfig}。
         * @return 方法调用结果 <br>
         *         + 0: 成功；  <br>
         *         + <0: 失败。
         * @notes  <br>
         *        + 使用不同 App ID 的 App 是不能互通的。  <br>
         *        + 请务必保证生成 Token 使用的 App ID 和创建引擎时使用的 App ID 相同，否则会导致加入房间失败。  <br>
         *        + 用户加入房间成功后，在本地网络状况不佳的情况下，SDK 可能会与服务器失去连接，此时 SDK 将会自动重连。重连成功后，本端会收到 OnJoinRoomResultEventHandler{@link #OnJoinRoomResultEventHandler} 回调通知，其 isRejoined 参数为 true。  <br>
         *        + 同一个 App ID 的同一个房间内，每个用户的用户 ID 必须是唯一的。如果两个用户的用户 ID 相同，则后加入房间的用户会将先加入房间的用户踢出房间，并且先加入房间的用户会收到 OnRoomErrorEventHandler{@link #OnRoomErrorEventHandler} 回调通知，错误类型为重复登录 DuplicateLogin。
         */
        int JoinRoom(string roomID, string userID, string token, GameRTCRoomConfig roomConfig);

        /** {zh}
         * @type api
         * @region 房间管理
         * @author chuzhongtao
         * @brief 离开房间。  <br>
         *        用户调用此方法离开房间，结束通话过程，释放所有通话相关的资源。<br>
         *        调用 JoinRoom{@link #JoinRoom} 方法加入房间后，在通话结束时必须调用此方法，并收到 OnLeaveRoomEventHandler{@link #OnLeaveRoomEventHandler} 回调确认结束通话，否则无法开始下一次通话。<br>
         *        无论当前是否在房间中，都可以调用此方法。重复调用此方法不会有负面影响。
         * @param roomID 房间 ID，用户调用此接口离开的房间的房间 ID。<br>
         *               房间 ID 为长度在 128 字节内的非空字符串，支持以下字符集范围: <br>
         *               1. 26 个大写字母 A ~ Z 。<br>
         *               2. 26 个小写字母 a ~ z 。<br>
         *               3. 10 个数字 0 ~ 9 。<br>
         *               4. 下划线 "_"，at 符 "@"，减号 "-"。
         * @return 方法调用结果  <br>
         *         + 0: 成功；  <br>
         *         + -1: 失败，引擎已经被销毁或者未创建，或者是房间 ID 为空。
         */
        int LeaveRoom(string roomID);

        /** {zh}
         * @type api
         * @region 房间管理
         * @author chuzhongtao
         * @brief 更新进房 token。  <br>
         *        在进房时如果收到错误码为 kJoinRoomInvalidToken 的 OnJoinRoomResultEventHandler{@link #OnJoinRoomResultEventHandler} 回调，表示 token 过期导致进房失败。  <br>
         *        此时，你应调用此接口，以新 token 进房。
         * @param roomID 房间 ID，用户调用此接口加入的房间的房间 ID。<br>
         *        房间 ID 为长度在 128 字节内的非空字符串，支持以下字符集范围: <br>
         *             1. 26 个大写字母 A ~ Z 。<br>
         *             2. 26 个小写字母 a ~ z 。<br>
         *             3. 10 个数字 0 ~ 9 。<br>
         *             4. 下划线 "_"，at 符 "@"，减号 "-"。
         * @param token 动态密钥，用于对登录用户进行鉴权验证。<br>
         *        进入房间需要携带 Token。测试时可使用控制台生成临时 Token，正式上线需要使用密钥 SDK 在你的服务端生成并下发 Token。
         * @return 方法调用结果  <br>
         *         + 0: 成功；  <br>
         *         + -1: 失败，引擎已经被销毁或者未创建，或者是房间 ID 为空。
         */
        int UpdateToken(string roomID, string token);

        /** {zh}
         * @type api
         * @region 范围语音
         * @author chuzhongtao
         * @brief 在世界房间中，使用范围语音功能时，设定语音的接收范围和衰减信息。
         * @param roomID 房间 ID
         * @param range 语音的收听范围，参看 GameRTCReceiveRange{@link #GameRTCReceiveRange}。
         * @return 方法调用结果  <br>
         *         + 0: 成功；  <br>
         *         + -1: 失败，引擎已经被销毁或者未创建，或者是房间 ID 为空。
         * @notes 你应在加入房间时，通过参数设置，加入世界房间，并开启范围语音: <br>
         *        - 关于世界房间，参看 RoomType{@link #RoomType} 中的 World。 <br>
         *        - 关于范围语音，参看 GameRTCRoomConfig{@link #GameRTCRoomConfig}。
         */
        int UpdateReceiveRange(string roomID, GameRTCReceiveRange range);

        /** {zh}
         * @type api
         * @region 范围语音
         * @author chuzhongtao
         * @brief 使用范围语音功能时，设置玩家的坐标。
         * @param roomID 房间 ID
         * @param pos 玩家坐标值，参看 GameRTCPositionInfo{@link #GameRTCPositionInfo}。
         * @return 方法调用结果  <br>
         *         + 0: 成功；  <br>
         *         + -1: 失败，引擎已经被销毁或者未创建，或者是房间 ID 为空。
         */
        int UpdatePosition(string roomID, GameRTCPositionInfo pos);

        /** {zh}
         * @type api
         * @region 空间语音
         * @brief 在使用空间语音功能时，必须通过此接口传入玩家的方位，SDK 内部会根据传入的方位值实现空间音频效果
         * @param roomID 加入的房间 ID <br>
         *        房间 ID 为长度在 128 字节内的非空字符串，支持以下字符集范围: <br>
         *            1. 26 个大写字母 A ~ Z 。<br>
         *            2. 26 个小写字母 a ~ z 。<br>
         *            3. 10 个数字 0 ~ 9 。<br>
         *            4. 下划线 "_"，at 符 "@"，减号 "-"。<br>
         * @param info 玩家方位值，具体参看 GameRTCOrientationInfo{@link #GameRTCOrientationInfo}
         * @return 方法调用结果  <br>
         *         +  0：成功  <br>
         *         + -1：引擎已经被销毁或者未创建，或者是房间 ID 为空
         * @notes 该方法需在进房设置 GameRTCRoomConfig{@link #GameRTCRoomConfig} 参数时，将 EnableSpatialAudio{@link #EnableSpatialAudio} 设为 true 开启空间音效功能后方可调用。
         */
        int UpdateOrientation(string roomID, GameRTCOrientationInfo info);

        /**
         * @type api
         * @region 音频管理
         * @author chuzhongtao
         * @brief 控制本地音频采集和发布状态。<br>
         *        改变该状态后，房间内其他用户会收到 OnMicrophoneEnabledEventHandler{@link #OnMicrophoneEnabledEventHandler} 回调。
         * @param roomID 房间 ID
         * @param enable 本地音频采集和发布状态：<br>
         *        + true：开始采集本地音频，并向房间内发布。<br>
         *        + false：停止采集本地音频，并停止发布。
         * @return 方法调用结果  <br>
         *         + 0: 成功；  <br>
         *         + -1: 失败，引擎已经被销毁或者未创建，或者是房间 ID 为空。
         */
        int EnableMicrophone(string roomID, bool enable);

        /** {zh}
         * @type api
         * @region 音频管理
         * @author chuzhongtao
         * @brief 控制本地语音的发送状态：发送/不发送。  <br>
         *        使用此方法后，房间中的其他用户会收到 OnAudioSendEnabledEventHandler{@link #OnAudioSendEnabledEventHandler} 回调。
         * @param roomID 房间 ID
         * @param enable 发送状态，标识是否发送语音：<br>
         *        + true: 发布语音 <br>
         *        + false: 不发布语音
         * @return 方法调用结果  <br>
         *         + 0: 成功  <br>
         *         + <0: 失败
         * @notes  <br>
         *      + 初次调用本方法发送本地语音时，会开启本地音频采集功能。<br>
         *      + 之后再次调用本方法时，仅控制本地音频流的发送状态，不会影响本地音频采集状态。
         */
        int EnableAudioSend(string roomID, bool enable);

        /** {zh}
         * @type api
         * @region 音频管理
         * @author chuzhongtao
         * @brief 设置对来自远端的语音的接收状态。<br>
         *        使用此方法后，房间中的其他用户会收到 OnSpeakerphoneEnabledEventHandler{@link #OnSpeakerphoneEnabledEventHandler} 回调。
         * @param roomID 房间 ID
         * @param enable 接收状态：<br>
         *        + true: 接收 <br>
         *        + false: 不接收
         * @return 方法调用结果  <br>
         *         + 0: 成功；  <br>
         *         + -1: 失败，引擎已经被销毁或者未创建，或者是房间 ID 为空。
         * @notes 本方法只控制本地是否接收远端音频流，并不影响本地音频播放设备的工作状态。
         */
        int EnableSpeakerphone(string roomID, bool enable);

        /** {zh}
         * @type api
         * @region 音频管理
         * @author chuzhongtao
         * @brief 设置对来自指定远端用户的语音的接收状态。
         * @param roomID 房间 ID
         * @param userID 指定远端用户的 ID
         * @param enable 接收状态：<br>
         *        + true: 接收 <br>
         *        + false: 不接收
         * @return 方法调用结果  <br>
         *         + 0: 成功；  <br>
         *         + -1: 失败，引擎已经被销毁或者未创建，或者是房间 ID 为空。
         * @notes 本方法只控制本地是否接收远端音频流，并不影响本地音频播放设备的工作状态。
         */
        int EnableAudioReceive(string roomID, string userID, bool enable);

        /** {zh}
         * @type api
         * @region 音频管理
         * @author chuzhongtao
         * @brief 调节音频采集音量。
         * @param volume 音频采集音量，调节范围: [0,400]，单位: %  <br>
         *        + 0: 静音   <br>
         *        + 100: 原始音量  <br>
         *        + 400: 最大可为原始音量的 4 倍(自带溢出保护，防止音量放大可能导致的爆音等问题)。
         * @return 方法调用结果  <br>
         *         + 0: 成功；  <br>
         *         + -1: 失败，引擎已经被销毁或者未创建，或者是房间 ID 为空。
         * @notes 为保证更好的通话质量，建议将 volume 值设为 [0,100]。
         */
        int SetRecordingVolume(int volume);

        /** {zh}
         * @type api
         * @region 音频管理
         * @author chuzhongtao
         * @brief 调节本地播放的所有远端用户混音后的音量。
         * @param volume 音频播放音量，调节范围: [0,400]，单位: %  <br>
         *        + 0: 静音   <br>
         *        + 100: 原始音量  <br>
         *        + 400: 最大可为原始音量的 4 倍(自带溢出保护，防止音量放大可能导致的爆音等问题)。
         * @return 方法调用结果  <br>
         *         + 0: 成功；  <br>
         *         + -1: 失败，引擎已经被销毁或者未创建，或者是房间 ID 为空。
         * @notes 为保证更好的通话质量，建议将 volume 值设为 [0,100]。
         */
        int SetPlaybackVolume(int volume);

        /** {zh}
         * @type api
         * @region 音频管理
         * @author chuzhongtao
         * @brief 调节来自指定远端用户的音频播放音量
         * @param roomID 房间 ID
         * @param userID 指定远端用户的 ID
         * @param volume 播放音量，调节范围: [0,400]，单位: %  <br>
         *               + 0: 静音   <br>
         *               + 100: 原始音量  <br>
         *               + 400: 最大可为原始音量的 4 倍(自带溢出保护，防止音量放大可能导致的爆音等问题)。
         * @return 方法调用结果  <br>
         *         + 0: 成功；  <br>
         *         + -1: 失败，引擎已经被销毁或者未创建，或者是房间 ID 为空。
         * @notes 开启范围语音后，此接口不生效。
         */
        int SetRemoteAudioPlaybackVolume(string roomID, string userID, int volume);

        /** {zh}
         * @type api
         * @region 音频管理
         * @brief 设置音频场景类型。  <br>
         *        你可以根据你的应用所在场景，选择合适的音频场景类型。
         *        选择音频场景后，RTC 会自动根据客户端音频路由和发布订阅状态，适用通话音量/媒体音量。  <br>
         *        在进房前和进房后设置均可生效。
         * @param [in] scenario 音频场景类型，
         *        参见 AudioScenarioType{@link #AudioScenarioType}
         * @notes  <br>
         *        + 通话音量更适合通话，会议等对信息准确度要求更高的场景。通话音量会激活系统硬件信号处理，使通话声音会更清晰。此时，音量无法降低到 0。<br>
         *        + 媒体音量更适合娱乐场景，因其声音的表现力会更强。媒体音量下，音量最低可以降低到 0。
         */
        int SetAudioScenario(AudioScenarioType scenario);
        
        /**
         * @type api
         * @region 音频管理
         * @author dixing
         * @brief 设置音质档位。你应根据业务场景需要选择适合的音质档位。  <br>
         * @param audioProfile 音质档位，参看 AudioProfileType{@link #AudioProfileType}
         * @notes  <br>
         *        + 该方法在进房前后均可调用；  <br>
         *        + 支持通话过程中动态切换音质档位。
         */
        int SetAudioProfile(AudioProfileType audioProfile);

        /**
        * @type api
        * @region 美声特效管理
        * @author luomingkang
        * @brief 设置变声特效类型。  <br>
        *        你可以根据你的需要，选择合适的变声特效。  <br>
        *        本方法只在单声道情况下生效，且与 setVoiceReverbType 接口互斥，后设置的特效会覆盖先设置的特效。  <br>
        *        本方法在进房前和进房后设置均可生效。  <br>
        * @param [in] voice_changer 变声特效类型，
        *        参见 VoiceChangerType{@link #VoiceChangerType}
        * @notes  <br>
        *        + 设置巨人特效后会进行基频检测，在确认男女之后选择对应的参数进行设置。  <br>
        */
        int SetVoiceChangerType(VoiceChangerType voice_changer);

        /**
        * @type api
        * @region 美声特效管理
        * @author luomingkang
        * @brief 设置混响特效类型。  <br>
        *        你可以根据你的需要，选择合适的混响特效。  <br>
        *        本方法只在单声道情况下生效，且与 setVoiceChangerType 接口互斥，后设置的特效会覆盖先设置的特效  <br>
        *        本方法在进房前和进房后设置均可生效。  <br>
        * @param [in] voice_reverb 混响特效类型，
        *        参见 VoiceReverbType{@link #VoiceReverbType}
        */
        int SetVoiceReverbType(VoiceReverbType voice_reverb);

#if UNITY_STANDALONE_WIN
        /** {zh}
         * @type api
         * @region 音频设备管理
         * @author chuzhongtao
         * @brief 获取音频采集设备的数量
         * @return  <br>
         *        + 获取成功则返回实际的音频采集设备数量；  <br>
         *        + 获取失败返回 -1，失败的原因可能是引擎未初始化。
         * @notes 该接口仅适用于 PC 平台，不适用于移动平台。
         */
        int GetRecordingDeviceCount();

        /** {zh}
         * @type api
         * @region 音频设备管理
         * @author chuzhongtao
         * @brief 指定音频采集设备
         * @param deviceID 设备 ID
         * @return 方法调用结果  <br>
         *        + 0: 成功；  <br>
         *        + <0: 失败，需要检查 deviceID 是否为空，以及引擎是否存在。
         * @notes 该接口仅适用于 PC 平台，不适用于移动平台。
         */
        int SetRecordingDevice(string deviceID);

        /** {zh}
         * @type api
         * @region 音频设备管理
         * @author chuzhongtao
         * @brief 获取当前音频采集设备
         * @param deviceID 设备 ID
         * @return 方法调用结果  <br>
         *        + 0: 成功；  <br>
         *        + <0: 失败，需要检查 deviceID 是否为空，以及引擎是否存在。
         * @notes 该接口仅适用于 PC 平台，不适用于移动平台。
         */
        int GetCurrentRecordingDevice(ref string deviceID);

        /** {zh}
         * @type api
         * @region 音频设备管理
         * @author chuzhongtao
         * @brief 获取全部音频采集设备信息
         * @param audioDeviceList 音频设备信息列表
         * @return  <br>
         *        + 0: 成功；  <br>
         *        + <0: 失败，你需要检查 audioDeviceInfo 是否为空，以及引擎是否存在。
         * @notes 该接口仅适用于 PC 平台，不适用于移动平台。
         */
        int GetAllRecordingDevices(ref List<AudioDeviceInfo> audioDeviceList);

        /** {zh}
         * @type api
         * @region 音频设备管理
         * @author chuzhongtao
         * @brief 获取音频播放设备的数量
         * @return  <br>
         *        + 获取成功则返回实际的播放设备数量；  <br>
         *        + 获取失败返回 -1，失败的原因可能是引擎未初始化。
         * @notes 该接口仅适用于 PC 平台，不适用于移动平台。
         */
        int GetPlaybackDeviceCount();

        /** {zh}
         * @type api
         * @region 音频设备管理
         * @author chuzhongtao
         * @brief 指定音频播放设备
         * @param deviceID 设备ID
         * @return 方法调用结果  <br>
         *        + 0: 成功；  <br>
         *        + <0: 失败，需要检查 deviceID 是否为空，以及引擎是否存在。
         * @notes 该接口仅适用于 PC 平台，不适用于移动平台。
         */
        int SetPlaybackDevice(string deviceID);

        /** {zh}
         * @type api
         * @region 音频设备管理
         * @author chuzhongtao
         * @brief 获取当前音频播放设备
         * @param deviceID 设备ID
         * @return 方法调用结果  <br>
         *        + 0: 成功；  <br>
         *        + <0: 失败，需要检查 deviceID 是否为空，以及引擎是否存在。
         * @notes 该接口仅适用于 PC 平台，不适用于移动平台。
         */
        int GetCurrentPlaybackDevice(ref string deviceID);

        /** {zh}
         * @type api
         * @region 音频设备管理
         * @author chuzhongtao
         * @brief 获取全部音频播放设备信息
         * @param audioDeviceList 音频设备信息列表
         * @return  <br>
         *        + 0: 成功；  <br>
         *        + <0: 失败，你需要检查 audioDeviceInfo 是否为空，以及引擎是否存在。
         * @notes 该接口仅适用于 PC 平台，不适用于移动平台。
         */
        int GetAllPlaybackDevices(ref List<AudioDeviceInfo> audioDeviceList);

#endif
    }

    /** {zh}
     * @type keytype
     * @author chuzhongtao
     * @brief 引擎相关配置信息
     */
    public struct GameRTCEngineConfig {
        /** {zh}
         * @brief 应用 ID。只有使用相同 appId 创建的 Engine 实例之间才能进行实时音视频通话。
         */
        public string AppID;
    }

    /** {zh}
     * @type keytype
     * @author chuzhongtao
     * @brief GameRTC 引擎初始化参数
     */
    public struct GameRTCEngineParams {
        /** {zh}
         * @brief 区域码，对于有海外合规要求的游戏，需要设置为 1；对于没有合规要求的游戏，设置为 0。
         */
        public int AreaCode;
        /** {zh}
         * @hidden
         * @brief 废弃
         */
        public bool IsLeaveRoomReleaseEngine;
        /** {zh}
         * @brief 应用 ID。只有使用相同 appId 创建的 Engine 实例之间才能进行实时音视频通话。
         */
        public string AppID;
        /**
         * @brief 向C++层透传的引擎参数，包含环境设置等。
         */
        public Dictionary<string,object> Params;
    }

    /** {zh}
     * @type errorcode
     * @author chuzhongtao
     * @brief 加入房间错误码
     */
    public enum JoinRoomErrorCode {
        /** {zh}
         * @brief 无效值
         */
        kJoinRoomInvalid = -1,
        /** {zh}
         * @brief 进房成功
         */
        kJoinRoomSuccess = 0,
        /** {zh}
         * @brief 用户调用 JoinRoom{@link #JoinRoom} 加入房间时使用的 Token 无效或过期失效。  <br>
         *        需要重新获取 Token，并调用 UpdateToken{@link #UpdateToken} 方法更新 Token。
         */
        kJoinRoomInvalidToken = -1000,
        /** {zh}
         * @brief 加入房间错误。  <br>
         *        用户调用 JoinRoom{@link #JoinRoom} 方法时发生未知错误导致加入房间失败，需要用户重新加入房间。
         */
        kJoinRoomError = -1001,
        /** {zh}
         * @brief 加入房间失败。
         *        用户调用 JoinRoom{@link #JoinRoom} 加入房间或由于网络状况不佳断网重连时，由于服务器错误导致用户加入房间失败，SDK 会自动重试加入房间。
         */
        kJoinRoomFailed = -2001
    }

    /** {zh}
     * @type keytype
     * @author chuzhongtao
     * @brief 用户离开房间的原因
     */
    public enum UserLeaveReasonType {
        /** {zh}
         * @brief 用户主动离开，即调用 LeaveRoom{@link #LeaveRoom} 方法退出房间。
         */
        Quit = 0,
        /** {zh}
         * @brief 用户掉线。远端用户因为网络等原因掉线。
         */
        Dropped = 1,
    }

    /** {zh}
     * @type keytype
     * @author chuzhongtao
     * @brief 房间类型
     */
    public enum RoomType {
        /** {zh}
         * @brief 小队房间。  <br>
         *        进入同一小队房间的成员是队友关系。  <br>
         *        进入小队房间后，打开麦克风可以向房间内所有队友讲话；打开扬声器可以收听房间内所有队友讲话。
         */
        Team = 0,
        /** {zh}
         * @brief 世界房间。  <br>
         *        你需要判断进入同一房间的成员是否为队友关系，进而决定收听逻辑。
         */
        World = 1,
    };

    /** {zh}
     * @type keytype
     * @author chuzhongtao
     * @brief 用户与 RTC 服务器的连接状态
     */
    public enum ConnectionState {
        /** {zh}
         * @brief 连接断开
         */
        Disconnected = 0,
        /** {zh}
         * @brief 首次连接，正在连接中
         */
        Connecting = 1,
        /** {zh}
         * @brief 首次连接成功
         */
        Connected = 2,
        /** {zh}
         * @brief 连接断开后，重新连接中
         */
        Reconnecting = 3,
        /** {zh}
         * @brief 连接断开后，重连成功
         */
        Reconnected = 4,
        /** {zh}
         * @brief 网络连接断开超过 10 秒，仍然会继续重连。
         */
        Lost = 5,
    }

    /** {zh}
     * @type keytype
     * @author chuzhongtao
     * @brief 房间配置信息
     */
    public struct GameRTCRoomConfig {
        /** {zh}
         * @brief 是否开启范围语音  <br>
         *        + true: 开启  <br>
         *        + false: 关闭  <br>
         * @notes 开启范围语音时，非同一小队玩家的声音会根据距离衰减，对于不需要这种衰减效果的应用，设置为 false 即可。
         */
        public bool EnableRangeAudio;
        /**
         * @type api
         * @brief 是否开启空间音效  <br>
         *        + true: 开启  <br>
         *        + false: 关闭  <br>
         * @notes 开启空间音效时，声音会根据玩家朝向不同，有不同的方位感，对于不需要空间音效效果的应用，设置为 false 即可。
         */
        public bool EnableSpatialAudio;
        /** {zh}
         * @type api
         * @brief 开启后会收到 OnAudioVolumeIndicationEventHandler{@link #OnAudioVolumeIndicationEventHandler} 音量提示回调的时间间隔：  <br>
         *        + ≤ 0: 禁用音量提示功能。  <br>
         *        + >0: 启用音量提示功能，并设置收到音量提示回调的时间间隔，单位为毫秒。 <br>
         * @notes 建议设置为大于等于 2000 毫秒；小于 10 毫秒时，行为未定义。
         */
        public int AudioVolumeIndicationInterval;
        /** {zh}
         * @brief 房间类型，参看 RoomType{@link #RoomType}。对于普通组队语音设置为 Team 即可。
         */
        public RoomType RoomType;
    };

    /** {zh}
     * @type keytype
     * @author chuzhongtao
     * @brief 音频流来源的用户 ID, 及对应的音量。
     */
    public struct GameRTCAudioVolumeInfo {
        /** {zh}
         * @brief 说话者的音量，范围为 0（最低）- 255（最高）
         */
        public int Volume;
        /** {zh}
         * @brief 说话者的用户 ID
         */
        public string UserID;
    }

    /** {zh}
     * @type keytype
     * @author chuzhongtao
     * @brief 使用范围语音功能时，语音的接收范围
     */
    public struct GameRTCReceiveRange {
        /** {zh}
         * @brief 收听声音无衰减的范围最小值。 <br>
         *        当收听者和声源距离小于 min 的时候，收听到的声音完全无衰减。
         */
        public int Min;
        /** {zh}
         * @brief 能够收听到声音的范围最大值。 <br>
         *        当收听者和声源距离大于 max 的时候，无法收听到声音。<br>
         *        当收听者和声源距离介于 [min, max) 之间时，收听到的音量根据距离有衰减。
         */
        public int Max;
    }

    /** {zh}
     * @type keytype
     * @author chuzhongtao
     * @brief 玩家的位置信息
     */
    public struct GameRTCPositionInfo {
        /** {zh}
         * @brief x 坐标
         */
        public int X;
        /** {zh}
         * @brief y 坐标
         */
        public int Y;
        /** {zh}
         * @brief z 坐标
         */
        public int Z;
    }

    /**
    * @type keytype
    * @brief 描述空间语音中玩家位置
    */
    public struct GameRTCOrientationInfo {
        /**
        * @brief 前向方位，在 x 轴的投影
        */
        public float x_axis_0;
        /**
        * @brief 右手方位，在 x 轴的投影
        */    
        public float x_axis_1;
        /**
        * @brief 头顶方位，在 x 轴的投影
        */    
        public float x_axis_2;
        /**
        * @brief 前向方位，在 y 轴的投影
        */    
        public float y_axis_0;
        /**
        * @brief 右手方位，在 y 轴的投影
        */    
        public float y_axis_1;
        /**
        * @brief 头顶方位，在 y 轴的投影
        */    
        public float y_axis_2;
        /**
        * @brief 前向方位，在 z 轴的投影
        */    
        public float z_axis_0;
        /**
        * @brief 右手方位，在 z 轴的投影
        */    
        public float z_axis_1;
        /**
        * @brief 头顶方位，在 z 轴的投影
        */    
        public float z_axis_2;
    }

    /** {zh}
     * @type errorcode
     * @author chuzhongtao
     * @brief 房间相关错误码
     */
    public enum RoomErrorCode {
        /** {zh}
         * @brief 无效错误码
         */
        InvalidErrorCode = -1,
        /** {zh}
         * @brief 因没有发布音频流权限，导致在房间中发布音视频流失败。  <br>
         *        需要检查 appId 是否正确。
         */
        NoPublishPermission = -1002,
        /** {zh}
         * @brief 因没有订阅音频流权限，导致在房间中订阅音视频流失败。  <br>
         *        需要检查 appId 是否正确。
         */
        NoSubscribePermission = -1003,
        DuplicateLogin = -1004,
    }

    /** {zh}
     * @type errorcode
     * @author chuzhongtao
     * @brief 房间警告码
     */
    public enum RoomWarnCode {
        /** {zh}
         * @brief 无效警告码
         */
        InvalidWarnCode = -1,
        /** {zh}
         * @brief 发布音频流失败。  <br>
         *        用户在所在房间中发布音视频流时，由于服务器错误导致发布失败。此时，会自动重试加入房间。
         */
        PublishStreamFailed = -2002,
        /** {zh}
         * @brief 订阅音频流失败。  <br>
         *        当前房间中找不到订阅的音视频流导致订阅失败。  <br>
         *        建议退出房间，重新进房。
         */
        SubscribeStreamFailed_404 = -2003,
        /** {zh}
         * @brief 订阅音频流失败。  <br>
         *        用户订阅所在房间中的音视频流时，由于服务器错误导致订阅失败。此时，SDK 会自动重试订阅。
         */
        SubscribeStreamFailed_5xx = -2004,
        /** {zh}
         * @brief 调度异常，服务器返回的媒体服务器地址不可用。
         */
        InvalidExpectMsAddr = -2007,
    }

    /** {zh}
     * @type errorcode
     * @author chuzhongtao
     * @brief 引擎相关警告码
     */
    public enum EngineWarnCode {
        /** {zh}
         * @brief 无效警告码
         */
        InvalidWarnCode = -1,
        /** {zh}
         * @brief 麦克风权限异常，当前应用没有获取麦克风权限
         */
        NoMicrophonePermission = -5002,
        /** {zh}
         * @brief 音频采集设备启动失败。当前音频采集设备可能被其他应用占用
         */
        ADMRecordingStartFail = -5003,
        /** {zh}
         * @brief 音频播放设备启动失败。原因可能为系统资源不足，或参数错误
         */
        ADMPlayoutStartFail = -5004,
        /** {zh}
         * @brief 无可用音频采集设备，请插入可用采集设备
         */
        ADMNoRecordingDevice = -5005,
        /** {zh}
         * @brief 无可用音频播放设备，请插入可用播放设备
         */
        ADMNoPlayoutDevice = -5006,
    }

    /** {zh}
     * @type keytype
     * @author chuzhongtao
     * @brief 用户的媒体流网络上下行质量
     */
    public enum NetworkQuality {
        /** {zh}
         * @brief 质量未知
         */
        Unknown = 0,
        /** {zh}
         * @brief 质量极好
         */
        Excellent = 1,
        /** {zh}
         * @brief 用户主观感觉近似“极好”，但码率可能略低于“极好”
         */
        Good = 2,
        /** {zh}
         * @brief 用户主观感受有瑕疵但不影响沟通
         */
        Poor = 3,
        /** {zh}
         * @brief 勉强能够沟通但不顺畅
         */
        Bad = 4,
        /** {zh}
         * @brief 网络质量非常差，基本不能沟通
         */
        Vbad = 5,
    };

    /** {zh}
     * @hidden
     * @type keytype
     * @brief 日志级别
     */
    public enum RtcLogLevel {
        /** {zh}
         * @hidden
         */
        RTC_LOG_LEVEL_TRACE,
        /** {zh}
         * @hidden
         * @brief 打印 debug 级别及以上级别信息。
         */
        RTC_LOG_LEVEL_DEBUG,
        /** {zh}
         * @hidden
         * @brief 打印 info 级别及以上级别信息。
         */
        RTC_LOG_LEVEL_INFO,
        /** {zh}
         * @hidden
         * @brief 打印 warning 级别及以上级别信息。
         */
        RTC_LOG_LEVEL_WARNING,
        /** {zh}
         * @hidden
         * @brief 打印 error 级别信息。
         */
        RTC_LOG_LEVEL_ERROR,
    }

    /** {zh}
     * @type keytype
     * @author chuzhongtao
     * @brief 音频设备信息
     */
    public struct AudioDeviceInfo {
        /** {zh}
         * @brief 音频设备 ID
         */
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=512)]
        public string DeviceID;
        /** {zh}
         * @brief 音频设备名字
         */
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=512)]
        public string DeviceName;
    }

    /** {zh}
     * @type keytype
     * @author chuzhongtao
     * @brief 音频场景类型
     */
    public enum AudioScenarioType {
        /** {zh}
        * @brief 音乐场景。默认为此场景。
        *        此场景适用于对音乐表现力有要求的场景。如音乐直播等。
        *        音频路由和发布订阅状态，到音量类型的映射如下：
        *        <table>
        *           <tr><th></th><th>仅发布音视频流</th><th>仅订阅音视频流</th><th>发布并订阅音视频流</th><th>备注</th></tr>
        *           <tr><td>设备自带麦克风和扬声器/听筒</td><td>媒体音量</td><td>媒体音量</td><td>通话音量</td><td>/</td></tr>
        *           <tr><td>有线耳机</td><td>媒体音量</td><td>媒体音量</td><td>媒体音量</td><td>/</td></tr>
        *           <tr><td>蓝牙耳机</td><td>媒体音量</td><td>媒体音量</td><td>媒体音量</td><td>即使蓝牙耳机有麦克风，也只能使用设备自带麦克风进行本地音频采集。</td></tr>
        *        </table>
        */
        kAudioScenarioTypeMusic = 0,
        /** {zh}
        * @brief 高质量通话场景。<br>
        *        此场景适用于对音乐表现力有要求的场景。但又希望能够使用蓝牙耳机上自带的麦克风进行音频采集的场景。
        *        此场景可以兼顾外放/使用蓝牙耳机时的音频体验；并尽可能避免使用蓝牙耳机时音量类型切换导致的听感突变。<br>
        *        音频路由和发布订阅状态，到音量类型的映射如下：<br>
        *        <table>
        *           <tr><th></th><th>仅发布音视频流</th><th>仅订阅音视频流</th><th>发布并订阅音视频流</th> <th>备注</th> </tr>
        *           <tr><td>设备自带麦克风和扬声器/听筒</td><td>媒体音量</td><td>媒体音量</td><td>通话音量</td><td>/</td></tr>
        *           <tr><td>有线耳机</td><td>媒体音量</td><td>媒体音量</td><td>媒体音量</td><td>/</td></tr>
        *           <tr><td>蓝牙耳机</td><td>通话音量</td><td>通话音量</td><td>通话音量</td><td>能够使用蓝牙耳机上自带的麦克风进行音频采集。</td></tr>
        *        </table>
        */
        kAudioScenarioTypeHighQualityCommunication = 1,
        /** {zh}
        * @brief 纯通话音量场景。<br>
        *        此场景下，无论客户端音频路由情况和发布订阅状态，全程使用通话音量。
        *        适用于需要频繁上下麦的通话或会议场景。<br>
        *        此场景可以保持统一的音频模式，不会有音量突变的听感；
        *        最大程度上的消除回声，使通话清晰度达到最优；
        *        使用蓝牙耳机时，能够使用蓝牙耳机上自带的麦克风进行音频采集。<br>
        *        但是，使用媒体音量进行播放的其他音频的音量会被压低，且音质会变差。
        */
        kAudioScenarioTypeCommunication = 2,
        /** {zh}
        * @brief 纯媒体场景。一般不建议使用。<br>
        *        此场景下，无论客户端音频路由情况和发布订阅状态，全程使用媒体音量。
        */
        kAudioScenarioTypeMedia = 3,
        /** {zh}
        * @brief 游戏媒体场景。需配合游戏音效消除的优化一起使用。  <br>
        *        此场景下，蓝牙耳机时使用通话音量，其它设备使用媒体音量。
        *        外放通话且无游戏音效消除优化时，极易出现回声和啸叫。
        */
        kAudioScenarioTypeGameStreaming = 4,
    }

    /**
     * @type keytype
     * @author dixing
     * @brief 音质档位
     */
    public enum AudioProfileType {
        /**
         * @brief 默认音质
         */
        kAudioProfileTypeDefault = 0,
        /**
         * @brief 流畅音质。  <br>
         *        流畅优先、低延迟、低功耗、低流量消耗，适用于大部分游戏场景，如 MMORPG、MOBA、FPS 等游戏中的小队语音、组队语音、国战语音等。
         */
        kAudioProfileTypeFluent = 1,
        /**
         * @brief 标准音质。  <br>
         *        适用于对音质有一定要求的场景，同时延时、功耗和流量消耗相对适中，适合 Sirius 等狼人杀类游戏。
         */
        kAudioProfileTypeStandard = 2,
        /**
         * @brief 高清音质  <br>
         *        超高音质，同时延时、功耗和流量消耗相对较大，适用于连麦 PK、在线教育等场景。 <br>
         *        游戏场景不建议使用。
         */
        kAudioProfileTypeHD = 3,
    }

    /**
    * @type keytype
    * @brief 变声特效类型
    */
    public enum VoiceChangerType {
        /**
        * @brief 原声
        */
        kVoiceChangerTypeOriginal = 0,
        /**
        * @brief 巨人
        */
        kVoiceChangerTypeGiant = 1,
        /**
        * @brief 花栗鼠
        */
        kVoiceChangerTypeChipmunk = 2,
        /**
        * @brief 小黄人
        */
        kVoiceChangerTypeMinionst = 3,
        /**
        * @brief 颤音
        */
        kVoiceChangerTypeVibrato = 4,
        /**
        * @brief 机器人
        */
        kVoiceChangerTypeRobot = 5,
    }

    /**
    * @type keytype
    * @brief 混响特效类型
    */
    public enum VoiceReverbType {
        /**
        * @brief 原声
        */
        kVoiceReverbTypeOriginal = 0,
        /**
        * @brief 回声
        */
        kVoiceReverbTypeEcho = 1,
        /**
        * @brief 演唱会
        */
        kVoiceReverbTypeConcert = 2,
        /**
        * @brief 空灵
        */
        kVoiceReverbTypeEthereal = 3,
        /**
        * @brief KTV
        */
        kVoiceReverbTypeKTV = 4,
        /**
        * @brief 录音棚
        */
        kVoiceReverbTypeStudio = 5,
    }
}
