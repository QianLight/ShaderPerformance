using System;
using System.Collections.Generic;
using GMSDK;

namespace GSDK
{
	/// <summary>
	/// Login回调信息
	/// </summary>
	/// <param name="result">
	/// 返回的错误码信息
	/// <para>
	/// 当前接口可能返回的错误码：
	///      IMInvalidArguments: 请求参数错误，请检查uid、token是否非法（0，或者空数据）
	/// </para>
	/// </param>
    public delegate void IMLoginEventHandler(Result result);

	/// <summary>
	/// 登录后所有inbox拉取消息完成会触发该事件
	/// </summary>
	/// <param name="info">inbox信息</param>
    public delegate void IMInitMessageEndEventHandler(InitMessageEndInfo info);

	/// <summary>
	/// 登录后inbox拉取消息完成会触发该事件回调
	/// </summary>
	/// <param name="info">inbox信息</param>
    public delegate void IMInboxInitMessageEndEventHandler(InboxInitMessageEndInfo info);

	/// <summary>
	/// 会话名称等消息属性更新，成员变更，新消息过来，会触发会话更新事件回调
	/// </summary>
	/// <param name="conversionInfo"> 会话信息</param>
    public delegate void IMConversationUpdatedEventHandler(ConversationIdInfo conversionInfo);

	/// <summary>
	/// 当Token失效的时候会触发该事件回调
	/// </summary>
    public delegate void IMTokenExpiredEventHandler();

	/// <summary>
	/// 会话成员更新，会触发会话更新事件回调
	/// </summary>
	/// <param name="conversionInfo"> 消息内容</param>
    public delegate void IMConversationParticipantsUpdatedEventHandler(ConversationIdInfo conversationID);

	/// <summary>
	/// 消息更新事件回调
	/// </summary>
	/// <param name="messageInfo"> 消息更新信息</param>
    public delegate void IMMessageUpdatedEventHandler(MessageUpdatedInfo messageInfo);

	/// <summary>
	/// 消息列表更新事件回调
	/// </summary>
	/// <param name="messageList"> 消息更新列表</param>
    public delegate void IMMessageListUpdatedEventHandler(MessageListUpdatedInfo messageList);

	/// <summary>
	/// 聊天列表数据源更新事件回调，只有数组顺序发生变化或增删才会触发
	/// </summary>
	/// <param name="conversationDataSource"> 会议数据源信息</param>
	public delegate void IMConversationDataSourceDidUpdateEventHandler(ConversationDataSourceUpdateInfo conversationDataSource);
	
	/// <summary>
	/// 获取超大群信息回调，只有主动拉取超大群消息才会触发，调用RegisterBroadCastListener后生效
	/// </summary>
	/// <param name="messageInfo">超大群消息信息</param>
	public delegate void IMReceiveBroadcastMessageEventHandler(ReceiveBroadcastMessageInfo messageInfo);
	
	/// <summary>
	/// 删除超大群消息回调，业务方收到此回调后，需要根据messageInfo信息定位到被删除的消息然后处理UI删除操作
	/// </summary>
	/// <param name="messageInfo">被删除的超大群消息信息</param>
	public delegate void IMDeleteBroadcastMessageEventHandler(DeleteBroadcastMessageInfo messageInfo);
	
	/// <summary>
	/// 收到好友申请事件
	/// </summary>
	/// <param name="friendEventInfo">好友申请参数</param>
	public delegate void IMReceiveFriendApplyEventHandler(FriendEventInfo friendEventInfo);
	
	/// <summary>
	/// 删除好友事件
	/// </summary>
	/// <param name="friendEventInfo">删除好友参数</param>
	public delegate void IMDeleteFriendEventHandler(FriendEventInfo friendEventInfo);
	
	/// <summary>
	/// 成为好友事件
	/// </summary>
	/// <param name="friendEventInfo">好友参数</param>
	public delegate void IMAddFriendEventHandler(FriendEventInfo friendEventInfo);

    /// <summary>
    /// 发送消息的回调信息
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      IMInvalidArguments: 请求参数错误，请检查uid、token是否非法（0，或者空数据）
    ///      IMConversationNotExist: 会话不存在
    ///      IMMessageSendErrorFileUplaodFailed: 发消息失败（文件上传失败）
    ///      IMOtherError: 其它错误，可能是登录前没有调用IM初始化方法、或者已经登录过了
    /// </para>
    /// </param>
    /// <param name="messageId">发送消息Id</param>
    public delegate void SendMessageEventHandler(Result result, string messageId);

    /// <summary>
    /// 会话相关操作结果回调
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      Success: 成功
    ///      IMConversationNotExist: 会话不存在
    ///      IMOtherError: 其它错误
    /// </para>
    /// </param>
    public delegate void IMOperationDelegate(Result result);

    /// <summary>
    /// 创建会话回调
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      Success: 成功
    ///      IMConversationNotExist: 会话不存在
    ///      IMOtherError: 其它错误
    /// </para>
    /// </param>
    /// <param name="conversationID">会话ID</param>
    public delegate void IMCreateConversationDelegate(Result result, string conversationID);

    /// <summary>
    /// 异步获取会话信息回调
    /// </summary>
    /// <param name="conversionInfo">会话信息</param>
    public delegate void IMGetConversationAsyncDelegate(IMConversation conversionInfo);

    /// <summary>
    /// 获取所有成员信息回调
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      IMInvalidArguments: 请求参数错误，请检查uid、token是否非法（0，或者空数据）
    /// </para>
    /// </param>
    /// <param name="participants"></param>
    public delegate void FetchAllParticipantsDelegate(Result result, List<IMParticipant> participants);
    
    /// <summary>
    /// 获取被拉黑的用户列表回调
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      Success: 成功
    ///      IMOtherError: 其它错误，
    /// </para>
    /// </param>
    /// <param name="blockedUserList">拉黑用户信息</param>
    public delegate void FetchblockListUsersDelegate(Result result, FetchblockListUsersInfo blockedUserList);

    /// <summary>
    /// 获取用户拉黑状态
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      Success: 成功
    ///      IMOtherError: 其它错误，
    /// </para>
    /// </param>
    /// <param name="isInBlocklist">是否被拉黑</param>
    public delegate void FetchUserBlockStatusDelegate(Result result, bool isInBlocklist);

    /// <summary>
    /// 获取用户信息回调
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      Success: 成功
    ///      IMOtherError: 其它错误，
    /// </para>
    /// </param>
    /// <param name="profile">用户信息</param>
    public delegate void FetchUserInfoDelegate(Result result, IMUserProfile profile);

    /// <summary>
    /// 获取超大群发消息回调
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      Success: 成功
    ///      IMInvalidArguments: 请求参数错误，请检查请求参数是否非法（0，或者空数据）
    ///      IMConversationNotExist: 会话不存在
    ///      IMOtherError: 其它错误，
    /// </para>
    /// </param>
    /// <param name="info">消息信息</param>
    public delegate void LoadNewBroadcastMessageDelegate(Result result, LoadNewBroadcastMessageInfo info);

    /// <summary>
    /// 批量拉黑、解除拉黑其它用户回调
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      Success: 成功
    ///      IMOtherError: 其它错误，
    /// </para>
    /// </param>
    /// <param name="ModifiedUsers">操作成功的用户ID</param>
    public delegate void ModifyUsersBlockListDelegate(Result result, List<long> ModifiedUsers);

    /// <summary>
    /// 获取超大群用户数目回调
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      IMInvalidArguments: 请求参数错误，请检查请求参数是否非法（0，或者空数据）
    ///      IMConversationNotExist: 会话不存在
    ///      IMOtherError: 其它错误，
    /// </para>
    /// </param>
    /// <param name="infolist">获取的结果信息</param>
    public delegate void BroadcastUserCounterDelegate(Result result, List<IMBroadcastRet> infolist);

    /// <summary>
    /// 添加会话成员的回调
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      Success: 成功.
    ///      IMConversationNotExist: 会话不存在
    ///      IMOtherError: 其它错误
    /// </para>
    /// </param>
    /// <param name="addedParticipants">成功添加的会话成员列表</param>
    public delegate void AddParticipantsDelegate(Result result, List<long> addedParticipants);

    /// <summary>
    /// 移除会话成员的回调
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      Success: 成功.
    ///      IMConversationNotExist: 会话不存在
    ///      IMOtherError: 其它错误
    /// </para>
    /// </param>
    /// <param name="removedParticipants">成功删除的成员列表</param>
    public delegate void RemovedParticipantsDelegate(Result result, List<long> removedParticipants);

    /// <summary>
    /// 异步发送消息的回调
    /// </summary>
    /// <param name="messageId">发送的消息ID</param>
    public delegate void SendMessageAsyncDelegate(string messageId);

    /// <summary>
    /// 重新发送消息的回调
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      Success: 成功
    ///      IMInvalidArguments: 请求参数错误，请检查请求参数是否非法（0，或者空数据）
    ///      IMConversationNotExist: 会话不存在
    ///      IMMessageIsAlreadySending: 消息正在发送中
    ///      IMOtherError: 其它错误
    /// </para>
    /// </param>
    public delegate void ResendMessageDelegate(Result result);

    /// <summary>
    /// 发送超大群消息回调
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      Success: 成功
    ///      IMInvalidArguments: 请求参数错误，请检查请求参数是否非法（0，或者空数据）
    ///      IMConversationNotExist: 会话不存在
    ///      IMMessageNotExist: 消息不存在
    ///      IMConversationCheckFailed: 会话检验不通过
    ///      IMMessageCheckFailed: 消息检验不通过
    ///      IMMessageCheckFailedButVisibleToSender: 消息检验不通过，但对自己可见
    ///      IMOtherError: 其它错误
    /// </para>
    /// </param>
    public delegate void SendBroadcastMessageDelegate(Result result);

    /// <summary>
    /// 查询用户信息回调
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      Success: 成功
    ///      IMOtherError: 其它错误，
    /// </para>
    /// </param>
    /// <param name="userInfo">用户信息</param>
    public delegate void QueryUserInfoDelegate(Result result, IMUserProfile userInfo);
    
    /// <summary>
    /// 批量查询用户信息回调
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      Success: 成功
    ///      IMOtherError: 其它错误，
    /// </para>
    /// </param>
    /// <param name="userInfoList">用户信息列表</param>
    public delegate void BatchQueryUserInfoDelegate(Result result, List<IMUserProfile> userInfoList);
    
    /// <summary>
    /// 搜索用户回调
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      Success: 成功
    ///      IMOtherError: 其它错误，
    /// </para>
    /// </param>
    /// <param name="userInfoList">用户信息列表</param>
    public delegate void SearchUserDelegate(Result result, List<IMUserProfile> userInfoList);
    
    /// <summary>
    /// 删除好友回调
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      Success: 成功
    ///      IMOtherError: 其它错误，
    /// </para>
    /// </param>
    /// <param name="uidList">被删除的用户id列表</param>
    public delegate void DeleteFriendsDelegate(Result result, List<long> uidList);
    
    /// <summary>
    /// 获取好友列表
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      Success: 成功
    ///      IMOtherError: 其它错误，
    /// </para>
    /// </param>
    /// <param name="friendInfoList">好友列表信息</param>
    public delegate void GetFriendListDelegate(Result result, IMFriendInfoList friendInfoList);
    
    /// <summary>
    /// 获取自己发出的好友申请列表
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      Success: 成功
    ///      IMOtherError: 其它错误，
    /// </para>
    /// </param>
    /// <param name="applyInfoList">好友申请列表信息</param>
    public delegate void GetSentApplyListDelegate(Result result, IMFriendApplyInfoList applyInfoList);
    
    /// <summary>
    /// 获取收到的好友申请列表
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      Success: 成功
    ///      IMOtherError: 其它错误，
    /// </para>
    /// </param>
    /// <param name="applyInfoList">好友申请列表信息</param>
    public delegate void GetReceivedApplyListDelegate(Result result, IMFriendApplyInfoList applyInfoList);
    
    /// <summary>
    /// 发起好友申请
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      Success: 成功
    ///      IMOtherError: 其它错误，
    /// </para>
    /// </param>
    public delegate void SendFriendApplyDelegate(Result result);
    
    /// <summary>
    /// 回复好友申请
    /// </summary>
    /// <param name="result">
    /// 返回的错误码信息
    /// <para>
    /// 当前接口可能返回的错误码：
    ///      Success: 成功
    ///      IMOtherError: 其它错误，
    /// </para>
    /// </param>
    public delegate void ReplyFriendApplyDelegate(Result result);
    
    /// <summary>
	/// 用于获取实例进行接口调用
	/// e.g.IM.Service.MethodName();
	/// </summary>
	public static class IM
	{
		public static IIMService Service
		{
			get
			{
				return ServiceProvider.Instance.GetService(ServiceType.IM) as IIMService;
			}
		}
	}

    /// <summary> IM 服务接口 </summary>
    /// <ConversationID>
    /// 会话ID，可以由以下场景获取：
    ///      * 会话更新事件 ConversationDataSourceDidUpdateEvent
    ///      * 通过CreateConversationWithOtherParticipants 获取
    ///      * 其它方式：Todo
    /// </ConversationID>

    public interface IIMService : IService
    {
	    #region Events
        /// <summary>
        /// Login回调事件
        /// </summary>
        /// <param name="result">
        /// ErrorCode 取值范围:
        ///      IMInvalidArguments: 请求参数错误，请检查uid、token是否非法（0，或者空数据）
        ///      IMOtherError: 其它错误，可能是登录前没有调用IM初始化方法、或者已经登录过了
        /// </param>
        event IMLoginEventHandler LoginEvent;

        /// <summary> 登录后所有inbox拉取消息完成会触发该事件 </summary>
        event IMInitMessageEndEventHandler InitMessageEndEvent;

        /// <summary> 登录后inbox拉取消息完成会触发该事件 </summary>
        event IMInboxInitMessageEndEventHandler InboxInitMessageEndEvent;

        /// <summary> 当Token失效的时候会触发该事件 </summary>
        event IMTokenExpiredEventHandler TokenExpiredEvent;

        /// <summary> 会话名称等消息属性更新，成员变更，新消息过来，会触发会话更新事件 </summary>
        /// <param name="conversionInfo"> 会话信息</param>
        event IMConversationUpdatedEventHandler ConversationUpdatedEvent;

        /// <summary> 会话成员更新，会触发会话更新事件 </summary>
        /// <param name="conversionInfo"> 消息内容</param>
        event IMConversationParticipantsUpdatedEventHandler ConversationParticipantsUpdatedEvent;

        /// <summary> 消息更新事件 </summary>
        /// <param name="messageInfo"> 消息更新信息</param>
        event IMMessageUpdatedEventHandler MessageUpdatedEvent;

        /// <summary> 消息列表更新事件 </summary>
        /// <param name="messageList"> 消息更新列表</param>
        event IMMessageListUpdatedEventHandler MessageListUpdatedEvent;

        /// <summary> 聊天列表数据源更新事件，只有数组顺序发生变化或增删才会触发 </summary>
        /// <param name="conversationDataSource"> 会议数据源信息</param>
        event IMConversationDataSourceDidUpdateEventHandler ConversationDataSourceDidUpdateEvent;

        /// <summary> 发送消息回调事件 </summary>
        /// <param name="result">
        /// ErrorCode 取值范围:
        ///      IMInvalidArguments: 请求参数错误，请检查uid、token是否非法（0，或者空数据）
        ///      IMConversationNotExist: 会话不存在
        ///      IMMessageSendErrorFileUplaodFailed: 发消息失败（文件上传失败）
        ///      IMOtherError: 其它错误，可能是登录前没有调用IM初始化方法、或者已经登录过了
        /// messageId: 发送成功的消息ID
        /// </param>
        event SendMessageEventHandler SendMessageEvent;
        
        /// <summary>
        /// 获取超大群信息回调，只有主动拉取超大群消息才会触发，调用RegisterBroadCastListener后生效
        /// </summary>
        /// <param name="messageInfo">超大群消息</param>
        event IMReceiveBroadcastMessageEventHandler ReceiveBroadcastMessageEvent;
        
        /// <summary>
        /// 删除超大群消息回调，业务方收到此回调后，需要根据messageInfo信息定位到被删除的消息然后处理UI删除操作
        /// </summary>
        /// <param name="messageInfo">被删除的超大群消息信息</param>
        event IMDeleteBroadcastMessageEventHandler DeleteBroadcastMessageEvent;

        /// <summary>
        /// 收到好友申请事件
        /// </summary>
        /// <param name="friendEventInfo">好友申请参数</param>
        event IMReceiveFriendApplyEventHandler ReceiveFriendApplyEvent;
        
        /// <summary>
        /// 删除好友事件
        /// </summary>
        /// <param name="friendEventInfo">删除好友参数</param>
        event IMDeleteFriendEventHandler DeleteFriendEvent;
        
        /// <summary>
        /// 成为好友事件
        /// </summary>
        /// <param name="friendEventInfo">好友参数</param>
        event IMAddFriendEventHandler AddFriendEvent;
        
        #endregion
        
        #region Account and Configuratin
        
        /// <summary>
        /// IM功能配置接口
        /// </summary>
        /// <param name="config">配置参数</param>
        void Config(IMConfig config);
        
#if UNITY_ANDROID
	    /// <summary>
	    /// 设置native日志开关（只针对安卓），注意只在开发阶段使用，切忌线上打开。
	    /// </summary>
	    /// <param name="open">true：开启; false：关闭</param>
	    /// <returns> 无</returns>
	    void SetAndroidLogOpen(bool open);
#endif

	    /// <summary>
	    /// 设置长连接异常断开时轮询拉混链消息的时间间隔
	    /// </summary>
	    /// <param name="seconds">轮询拉取时间间隔，单位：秒</param>
	    /// <returns> 无</returns>
	    void SetAutoPullMessageIntervalSeconds(double seconds);

	    /// <summary>
	    /// 主动拉取对应inbox的新消息。如果拉到新消息会触发 onMessageListUpdated 和 onConversationUpdated 
	    /// </summary>
	    /// <param name="inbox">收件箱</param>
	    void PullNewMessage(int inbox);

#if UNITY_ANDROID
	    /// <summary>
	    /// （仅安卓可用）主动触发激活长连接
	    /// 此接口仅在长链接失效，无法收取消息且无法定位原因时，进行内部调用
	    /// 此接口为长链接兜底机制，正常情况无需关注
	    /// </summary>
	    /// <returns> 无</returns>
        void ActivateLongConnection();
	    
        /// <summary>
        /// （仅安卓可用）主动触发断开长连接
        /// 此接口为长链接兜底机制，正常情况无需关注
        /// </summary>
        /// <returns> 无</returns>
        void DeactivateLongConnection();
        
        /// <summary>
        /// （仅安卓可用）主动触发获取长链接的状态
        /// 此接口为长链接兜底机制，正常情况无需关注
        /// </summary>
        /// <returns>true:长链接已链接；false：长链接已断开</returns>
        bool IsLongConnectionActivated();
#endif
	    
        /// <summary> 登录IM接口V1版本，需要CP维护Token </summary>
        /// <param name="userID">用户ID，必须为非0值</param>
        /// <param name="token">用户登录票据，不能为空</param>
        /// <returns> 无</returns>
        /// <seealso> 登录结果通过 LoginEvent 返回</seealso>
        void Login(long userID, string token);
        
        /// <summary> 登录IM接口V2版本，不需要CP维护Token，推荐使用 </summary>
        /// <param name="userID">用户ID，必须为非0值</param>
        /// <returns> 无</returns>
        /// <seealso> 登录结果通过 LoginEvent 返回</seealso>
        void Login(long userID);

        /// <summary> 登出 </summary>
        /// <returns> 无</returns>
        void Logout();

        /// <summary> 刷新票据 当触发 TokenExpiredEvent 事件的时候，需要调用些接口进行票据刷新</summary>
        /// <param name="token">用户登录票据，不能为空。</param>
        /// <returns> 无</returns>
        void RefreshToken(string token);
        
        /// <summary>
        /// 获取当前已经登录的UserID
        /// </summary>
        /// <returns>当前登录的UserID，若未登录返回“0”</returns>
        string GetCurrentUserId();

        #endregion

        #region Conversation
        /// <summary> 初始化会话列表 </summary>
        /// <param name="inboxes"> inboxes </param>
        /// <returns> 会话数据源ID </returns>
        string InitConversationDataSourceWithInboxes(List<int> inboxes);

        /// <summary> 进入会话 </summary>
        /// <param name="conversationId">会话ID </param>
        /// <returns> 无</returns>
        void EnterConversation(string conversationId);

        /// <summary> 进入会话，并以指定方式加载消息列表 </summary>
        /// <param name="conversationId">会话ID </param>
        /// <param name="mode">消息列表加载方式，0-加载最新一页消息，1-从第一条未读消息开始加载更新的消息。 </param>
        /// <param name="offset">偏移量，在mode=GMIMInitMessageListModeFirstUnread的情况下生效，表示从第一条未读消息开始，之前消息的偏移量，例如offset=1，表示第一条未读消息之前的一条，即最后一条已读消息，如果一页数量大于未读消息数量，则取更旧消息补足。offset不能传负数。 </param>
        /// <returns> 无</returns>
        void EnterConversation(string conversationId, int mode, int offset);

        /// <summary> 离开会话 </summary>
        /// <param name="conversationId">会话ID， </param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMConversationNotExist: 会话不存在
        ///      IMOtherError: 其它错误
        ///  </result>
        /// </param>
        /// <returns> 无</returns>
        void ExitConversation(string conversationId);

        /// <summary> 退出会话（退群） </summary>
        /// <param name="conversationId">会话ID， </param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMConversationNotExist: 会话不存在
        ///      IMOtherError: 其它错误
        ///  </result>
        /// </param>
        /// <returns> 无</returns>
        void QuitConversation(string conversationId, IMOperationDelegate callback);

        /// <summary> 解散会话 </summary>
        /// <param name="conversationId">会话ID， </param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMConversationNotExist: 会话不存在
        ///      IMOtherError: 其它错误
        ///  </result>
        /// </param>
        /// <returns> 无</returns>
        void DismissConversation(string conversationId, IMOperationDelegate callback);


        /// <summary> 更新会话信息 </summary>
        /// <param name="conversationId">会话ID， </param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMConversationNotExist: 会话不存在
        ///      IMOtherError: 其它错误
        /// </result>
        /// </param>
        /// <returns> 无</returns>
        void UpdateConversation(string conversationId, IMOperationDelegate callback);

        /// <summary> 设置会话名称 </summary>
        /// <param name="conversationId">会话ID， </param>
        /// <param name="name"> 会话名称 </param>
        /// <param name="ext"> 风控业务扩展字段，除定制业务外，默认传null </param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMConversationNotExist: 会话不存在
        ///      IMOtherError: 其它错误
        ///  </result>
        /// </param>
        /// <returns> 无</returns>
        void SetConversationName(string conversationId, string name, Dictionary<string, string> ext, IMOperationDelegate callback);

		/// <summary> 设置会话简介 </summary>
		/// <param name="conversationId">会话ID， </param>
		/// <param name="desc"> 会话简介 </param>
		/// <param name="ext"> 业务扩展字段，暂未使用，默认传null </param>
		/// <param name="callback">
		///  <result>
		///  ErrorCode 取值范围:
		///      Success: 成功
		///      IMConversationNotExist: 会话不存在
		///      IMOtherError: 其它错误
		///  </result>
		/// </param>
		/// <returns> 无</returns>
		void SetConversationDesc(string conversationId, string desc, Dictionary<string, string> ext, IMOperationDelegate callback);

		/// <summary> 设置会话图标 </summary>
		/// <param name="conversationId">会话ID， </param>
		/// <param name="icon"> 会话图标 </param>
		/// <param name="ext"> 业务扩展字段，暂未使用，默认传null </param>
		/// <param name="callback">
		///  <result>
		///  ErrorCode 取值范围:
		///      Success: 成功
		///      IMConversationNotExist: 会话不存在
		///      IMOtherError: 其它错误
		///  </result>
		/// </param>
		/// <returns> 无</returns>
		void SetConversationIcon(string conversationId, string icon, Dictionary<string, string> ext, IMOperationDelegate callback);

		/// <summary> 设置会话公告 </summary>
		/// <param name="conversationId">会话ID， </param>
		/// <param name="notice"> 会话公告 </param>
		/// <param name="ext"> 业务扩展字段，暂未使用，默认传null </param>
		/// <param name="callback">
		///  <result>
		///  ErrorCode 取值范围:
		///      Success: 成功
		///      IMConversationNotExist: 会话不存在
		///      IMOtherError: 其它错误
		///  </result>
		///  <value> conversationID: 成功创建的会话ID </value>
		/// </param>
		/// <returns> 无</returns>
		void SetConversationNotice(string conversationId, string notice, Dictionary<string, string> ext, IMOperationDelegate callback);

        /// <summary> 设置会话的共享拓展信息 </summary>
        /// <param name="conversationId">会话ID </param>
        /// <param name="ext"> 拓展信息键值对，该信息会被同步到会话中所有成员的设备 </param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMConversationNotExist: 会话不存在
        ///      IMOtherError: 其它错误
        ///  </result>
        /// </param>
        /// <returns> 无</returns>
        void SetConversationCoreExt(string conversationId, Dictionary<string, string> ext, IMOperationDelegate callback);

        /// <summary> 设置会话的个人拓展信息 </summary>
        /// <param name="conversationId">会话ID </param>
        /// <param name="ext"> 拓展信息键值对，该信息会被同步到该用户的其他设备 </param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMConversationNotExist: 会话不存在
        ///      IMOtherError: 其它错误
        ///  </result>
        /// </param>
        /// <returns> 无</returns>
        void SetConversationSettingExt(string conversationId, Dictionary<string, string> ext, IMOperationDelegate callback);

        /// <summary> 设置会话的本地拓展信息 </summary>
        /// <param name="conversationId">会话ID </param>
        /// <param name="ext"> 拓展信息键值对，该信息存储在本地数据库，不会同步到其他设备 </param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMConversationNotExist: 会话不存在
        ///      IMOtherError: 其它错误
        ///  </result>
        /// </param>
        /// <returns> 无</returns>
        void SetConversationLocalExt(string conversationId, Dictionary<string, string> ext, IMOperationDelegate callback);

        /// <summary> 创建会话 </summary>
        /// <param name="otherParticipants"> 其它成员</param>
        /// <param name="type"> 会话类型：单聊、群聊、轻直播 </param>
        /// <param name="inbox"> inbox </param>
        /// <param name="idempotentID"> 幂等ID </param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMConversationNotExist: 会话不存在
        ///      IMOtherError: 其它错误
        ///  </result>
        ///  conversationID: 成功创建的会话ID
        /// </param>
        /// <returns> 无</returns>
        void CreateConversationWithOtherParticipants(List<long> otherParticipants, IMConversationType type, int inbox, string idempotentID, IMCreateConversationDelegate callback);


        /// <summary> 根据id获取会话信息 </summary>
        /// <param name="conversationDataSourceID"> 会话数据源ID，创建会话数据源时会返回 </param>
        /// <returns> 会话数量</returns>
        int GetNumberOfConversations(string conversationDataSourceID);

        /// <summary> 根据索引获取会话信息 </summary>
        /// <param name="conversationDataSourceID">数据源ID</param>
        /// <param name="index"> 会话索引 </param>
        /// <returns> 会话信息</returns>
        IMConversation GetConversationAtIndex(string conversationDataSourceID, int index);

        /// <summary> 根据id获取会话信息 </summary>
        /// <param name="conversationId">会话ID</param>
        /// <returns> 会话信息</returns>
        IMConversation GetConversationWithID(string conversationId);

        /// <summary> 异步调用接口，获取会话信息 </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="conversationType">会话类型</param>
        /// <param name="inbox">会话所属inbox</param>
        /// <param name="callback">会话信息回调处理</param>
        void GetConversationAsync(string conversationId, IMConversationType conversationType, int inbox, IMGetConversationAsyncDelegate callback);

        /// <summary> 删除会话 </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="mode"> LocalDevice：只删除当前设备的消息，AllMyDevice：删除所有设备的消息 </param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMConversationNotExist: 会话不存在
        ///      IMOtherError: 其它错误
        ///  </result>
        /// </param>
        /// <returns> 无</returns>
        void DeleteConversation(string conversationId, IMConversationDeleteMode mode, IMOperationDelegate callback);


        /// <summary> 删除会话中所有消息 </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功， 当前只会返回成功
        ///  </result>
        /// </param>
        /// <returns> 无</returns>
        void DeleteAllConversationMessages(string conversationId, IMOperationDelegate callback);


		/// <summary> 获取会话中是否有拉取的历史消息 </summary>
		/// <param name="conversationId">会话ID</param>
		/// <returns> 判断结果</returns>
		bool HasOlderMessages(string conversationId);

		/// <summary> 主动加入群 </summary>
		/// <param name="inbox"> inbox </param>
		/// <param name="conversationId">会话ID</param>
		/// <param name="bizExtension"> 业务额外扩展字段 </param>
		/// <param name="callback">
		///  <result/>
		///  ErrorCode 取值范围:
		///      Success: 成功.
		///      IMConversationNotExist: 会话不存在
		///      IMOtherError: 其它错误
		/// </param>
		/// <returns> 无</returns>
		void JoinConversation(int inbox, string conversationId, Dictionary<string, string> bizExtension, IMOperationDelegate callback);

        #region Participants


        /// <summary> 获取所有群聊用户 </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 目前只会返回成功. 是否真正获取到成员列表，取决于 **participants** 参数是否为空。
        ///  </result>
        /// participants: 会话中用户列表。
        /// </param>
        /// <returns> 无</returns>
        void FetchAllParticipants(string conversationId, FetchAllParticipantsDelegate callback);

        /// <summary> 添加群聊用户 </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="participants"> 成员ID列表 </param>
        /// <param name="bizExtension"> 业务额外扩展字段 </param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功.
        ///      IMConversationNotExist: 会话不存在
        ///      IMOtherError: 其它错误
        ///  </result>
        /// addedParticipants: 成功添加的用户列表
        /// </param>
        /// <returns> 无</returns>
        void AddParticipants(string conversationId, List<long> participants, Dictionary<string, string> bizExtension, AddParticipantsDelegate callback);

        /// <summary> 删除群聊用户 </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="participants"> 成员ID列表 </param>
        /// <param name="bizExtension"> 业务额外扩展字段 </param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功.
        ///      IMConversationNotExist: 会话不存在
        ///      IMOtherError: 其它错误
        ///  </result>
        /// removedParticipants: 成功被删除的用户列表
        /// </param>
        /// <returns> 无</returns>
        void RemoveParticipants(string conversationId, List<long> participants, Dictionary<string, string> bizExtension, RemovedParticipantsDelegate callback);

        /// <summary> 设置成员角色信息 </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="participant"> 成员ID </param>
        /// <param name="roleInfo"> 角色信息 </param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMConversationNotExist: 会话不存在
        ///      IMOtherError: 其它错误
        ///  </result>
        /// </param>
        /// <returns> 无</returns>
        void SetRoleForParticipant(string conversationId, long participant, IMConversationParticipantRole roleInfo, IMOperationDelegate callback);

        /// <summary> 设置成员昵称 </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="participant"> 成员ID </param>
        /// <param name="alias"> 成员昵称 </param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMConversationNotExist: 会话不存在
        ///      IMOtherError: 其它错误
        ///  </result>
        /// </param>
        /// <returns> 无</returns>
        void SetAliasForParticipant(string conversationId, long participant, string alias, IMOperationDelegate callback);
        #endregion

        /// <summary> 设置草稿内容 </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="draft">草稿内容</param>
        /// <returns> 无</returns>
        void SetDraft(string conversationId, string draft);

        /// <summary> 设置某一个会话的免打扰状态 </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="shouldMute">true: 开启静音；false：取消静音</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMConversationNotExist: 会话不存在
        ///  </result>
        /// </param>
        /// <returns> 无</returns>
        void SetMute(string conversationId, bool shouldMute, IMOperationDelegate callback);

        #endregion
        

        #region Message
        /// <summary>
        /// 获取会话中消息数目
        /// </summary>
        /// <param name="conversationId">会话ID</param>
        /// <returns>会话中消息数</returns>
        int GetNumberOfMessages(string conversationId);

        /// <summary> 获取某一条消息的对象 </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="index">inbox</param>
        /// <returns> 消息数据</returns>
        IMMessage GetMessage(string conversationId, int index);

        /// <summary> 获取某一条消息的对象 </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="messageId">消息ID</param>
        /// <returns> 消息数据</returns>
        IMMessage GetMessage(string conversationId, string messageId);


        /// async notify through MessageListUpdatedEvent, if there're messages
        void LoadOlderMessages(string conversationId);//, IMOperationDelegate callback);

        /// async notify through MessageListUpdatedEvent, if there're messages
        void LoadNewerMessages(string conversationId);//, IMOperationDelegate callback);


        #region Single Message information
        /// <summary> 获取某一条消息的文本内容 </summary>
        /// <param name="message">消息内容</param>
        /// <returns> 消息的文本内容</returns>
        string GetTextContent(IMMessage message);

        /// <summary> 获取某一条消息的图片内容 </summary>
        /// <param name="message">消息内容</param>
        /// <returns> 消息的图片内容</returns>
        IMImage GetImage(IMMessage message);

        /// <summary> 获取某一条消息的语音内容 </summary>
        /// <param name="message">消息内容</param>
        /// <returns> 语音的VoiceID</returns>
        string GetVoiceId(IMMessage message);
        
        /// <summary> 获取某一条消息的语音时长 </summary>
        /// <param name="message">消息内容</param>
        /// <returns> 语音的时长，单位ms</returns>
        long GetVoiceDuration(IMMessage message);
        #endregion

        #region  Construct Message
        /// <summary>
        /// 构建文本信息
        /// </summary>
        /// <param name="text">文本内容</param>
        /// <returns>发送消息内容</returns>
        IMSendMessageInfo ConstructTextMessage(string text);

        /// <summary>
        /// 构建图片信息
        /// </summary>
        /// <param name="imagePath">图片本地路径</param>
        /// <param name="imageWidth">图片原始宽度</param>
        /// <param name="imageHeight">图片原始高度</param>
        /// <param name="mime">图片格式</param>
        /// <param name="format">后缀名</param>
        /// <param name="thumbWidth">缩略图片宽度</param>
        /// <param name="thumbHeight">缩略图片高度</param>
        /// <param name="previewWidth">图片预览高度</param>
        /// <param name="previewHeight">图片预览高度</param>
        /// <returns>发送消息内容</returns>
        IMSendMessageInfo ConstructImageMessage(string imagePath, int imageWidth, int imageHeight, string mime,
            string format, int thumbWidth, int thumbHeight, int previewWidth, int previewHeight);

        /// <summary>
        /// 构建语音消息
        /// 使用IMVoice录制完成后拿到的语音本地路径（安卓是本地voiceId，一般是以voice_开头的）和时长，构造语音消息发送参数。在发送消息时SDK会自动处理语音上传。
        /// </summary>
        /// <param name="localPath">使用IMVoice录制完成后拿到的本地路径（安卓是本地voiceId，一般是以voice_开头的）</param>
        /// <param name="duration">使用IMVoice录制完成后拿到的语音时长，单位ms </param>
        /// <returns>发送消息内容</returns>
        IMSendMessageInfo constructVoiceMessageWithLocalPath(string localPath, long duration);
        
        /// <summary>
        /// 构建语音消息
        /// </summary>
        /// <param name="voiceId">语音消息ID，通过语音模块获得</param>
        /// <returns>发送消息内容</returns>
        IMSendMessageInfo ConstructVoiceMessage(string voiceId);

        /// <summary>
        /// 构建表情信息
        /// </summary>
        /// <param name="content">表情内容</param>
        /// <returns>发送消息内容</returns>
        IMSendMessageInfo ConstructEmoticonMessage(Dictionary<string, object> content);
        #endregion



        #region Message Operating
        /// <summary> 发送消息 </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="message">消息ID， 通过 ConstructXXXMessage 返回</param>
        /// <returns> 消息ID（MessageID </returns>
        /// <seealso> 发送结果通过 SendMessageEvent 返回</seealso>
        string SendMessage(string conversationId, IMSendMessageInfo message);

        /// <summary> 发送本地消息，不走服务器 </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="message">消息ID， 通过 ConstructXXXMessage 返回</param>
        /// <returns> 消息ID（MessageID </returns>
        /// <seealso> 无</seealso>
        string SendLocalMessage(string conversationId, IMSendMessageInfo message);

        /// <summary> 发送消息，支持本地会话删除的情况下也可以正常发消息 </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="conversationType">会话类型</param>
        /// <param name="inbox">会话所属inbox</param>
        /// <param name="message">消息ID， 通过 ConstructXXXMessage 返回</param>
        /// <param name="callback"> 处理回调的消息ID </param>
        void SendMessageAsync(string conversationId, IMConversationType conversationType, int inbox, IMSendMessageInfo message, SendMessageAsyncDelegate callback);

        /// <summary> 重发消息 </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="messageId">消息ID， 通过 ConstructXXXMessage 返回值获取</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMInvalidArguments: 请求参数错误，请检查请求参数是否非法（0，或者空数据）
        ///      IMConversationNotExist: 会话不存在
        ///      IMMessageIsAlreadySending: 消息正在发送中
        ///      IMOtherError: 其它错误
        ///  </result>
        /// </param>
        /// <returns> 无 </returns>
        void ResendMessage(string conversationId, string messageId, ResendMessageDelegate callback);

        /// <summary> 撤回消息 </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="messageId">消息ID， 通过 ConstructXXXMessage 返回值获取</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMConversationNotExist: 会话不存在
        ///      IMOtherError: 其它错误，
        ///  </result>
        /// </param>
        /// <returns> 无 </returns>
        void RecallMessage(string conversationId, string messageId, IMOperationDelegate callback);

        /// <summary> 删除消息（同步删除服务端消息） </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="messageId">消息ID， 通过 ConstructXXXMessage 返回值获取</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMInvalidArguments: 请求参数错误，请检查请求参数是否非法（0，或者空数据）
        ///      IMConversationNotExist: 会话不存在
        ///      IMMessageNotExist: 消息不存在
        ///      IMOtherError: 其它错误，
        ///  </result>
        /// </param>
        /// <returns> 无 </returns>
        void DeleteMessage(string conversationId, string messageId, IMOperationDelegate callback);

        /// <summary> 发送超大群发消息 </summary>
        /// <param name="message">消息内容，通过 ConstructXXXMessage 返回</param>
        /// <param name="inbox"> inbox </param>
        /// <param name="conversationId">会话ID</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMInvalidArguments: 请求参数错误，请检查请求参数是否非法（0，或者空数据）
        ///      IMConversationNotExist: 会话不存在
        ///      IMMessageNotExist: 消息不存在
        ///      IMConversationCheckFailed: 会话检验不通过
        ///      IMMessageCheckFailed: 消息检验不通过
        ///      IMMessageCheckFailedButVisibleToSender: 消息检验不通过，但对自己可见
        ///      IMOtherError: 其它错误，
        /// </result>
        /// </param>
        /// <returns> 无 </returns>
        void SendBroadcastMessage(IMSendMessageInfo message, int inbox, string conversationId, SendBroadcastMessageDelegate callback);

        /// <summary> 获取超大群发消息 </summary>
        /// <param name="inbox"> inbox </param>
        /// <param name="conversationId">会话ID</param>
        /// <param name="cursor">传入0表示拉取最近的若干条信息；如果希望接着某一个cursor连续拉取，则传入callback中返回的cursor数值</param>
        /// <param name="limit">每次拉取的消息条数</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMInvalidArguments: 请求参数错误，请检查请求参数是否非法（0，或者空数据）
        ///      IMConversationNotExist: 会话不存在
        ///      IMOtherError: 其它错误，
        ///  </result>
        /// </param>
        /// <returns> 无 </returns>
        void LoadNewBroadcastMessage(string conversationId, int inbox, long cursor, long limit, LoadNewBroadcastMessageDelegate callback);

        /// <summary> 获取超大群旧消息 </summary>
        /// <param name="inbox"> inbox </param>
        /// <param name="conversationId">会话ID</param>
        /// <param name="cursor"> 从哪条消息开始往前拉，注意结果中包含此cursor的消息，可以取当前最早一条消息的cursor-1。（消息体中的broadCastIndex即为cursor）</param>
        /// <param name="limit">拉取的消息条数</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMInvalidArguments: 请求参数错误，请检查请求参数是否非法（0，或者空数据）
        ///      IMConversationNotExist: 会话不存在
        ///      IMOtherError: 其它错误，
        ///  </result>
        /// </param>
        /// <returns> 无 </returns>
        void LoadOldBroadcastMessage(string conversationId, int inbox, long cursor, long limit, LoadNewBroadcastMessageDelegate callback);
        
        /// <summary>
        /// 获取超大群用户数目
        /// </summary>
        /// <param name="conversationsIDArray">会话ID列表</param>
        /// <param name="inbox">inbox信息</param>
        /// <param name="callback">回调信息</param>
        void BroadcastUserCounter(List<string> conversationsIDArray, int inbox, BroadcastUserCounterDelegate callback);

        /// <summary>
        /// 监听超大群拉取消息事件
        /// </summary>
        /// <param name="conversationId">超大群会话ID</param>
        void RegisterBroadCastListener(string conversationId);
        
        /// <summary>
        /// 取消大群拉取消息事件
        /// </summary>
        /// <param name="conversationId">超大群会话ID</param>
        void UnregisterBroadCastListener(string conversationId);
        
        /// <summary>
        /// 设置超大群消息拉取时间间隔
        /// </summary>
        /// <param name="delay">时间间隔，单位：s</param>
        void SetBroadCastThrottleDelay(long delay);
        
        /// <summary>
        /// 进入超大群。进入之后只需要监听ReceiveBroadcastMessageEvent感知消息变化即可
        /// </summary>
        /// <param name="inbox">超大群会话对应的收件箱</param>
        /// <param name="conversationId">超大群会话ID</param>
        void EnterBroadCastConversation(int inbox, string conversationId);

        /// <summary>
        /// 退出超大群。退出之后不会再收到此超大群的消息。如果需要再次接收消息，需执行EnterBroadCastConversation
        /// </summary>
        /// <param name="conversationId">超大群会话ID</param>
        void ExitBroadCastConversation(string conversationId);
        
        /// <summary>
        /// 恢复接收超大群消息
        /// </summary>
        /// <param name="conversationId">超大群会话ID</param>
        /// <param name="fromLast">
        /// 是否从上次暂停时的位置开始接收消息。
        /// true：会收到从上次pause开始到现在的所有新消息，可能量比较大
        /// false：resume后会拉取最新的N（默认50）条消息，并接收之后的新消息
        /// </param>
        void ResumeBroadCastConversation(string conversationId, bool fromLast);
        
        /// <summary>
        /// 暂停接收超大群消息。暂停后不会再收到超大群消息，如果想恢复需调用ResumeBroadCastConversation
        /// </summary>
        /// <param name="conversationId">超大群会话ID</param>
        void PauseBroadCastConversation(string conversationId);
        
        #endregion

        /// <summary> 标记会话的所有消息为已读 </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///  </result>
        /// </param>
        /// <returns> 无 </returns>
        void MarkAllMessagesAsRead(string conversationId, IMOperationDelegate callback);


        /// <summary> 标记会话中指定消息之前的所有消息为已读 </summary>
        /// <param name="conversationId">会话ID</param>
        /// <param name="messageId">指定消息ID</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///  </result>
        /// </param>
        /// <returns> 无 </returns>
        void MarkAllMessagesAsReadBeforeMessage(string conversationId, string messageId, IMOperationDelegate callback);
        
        #endregion

        #region User Management

        /// <summary> 获取用户信息 </summary>
        /// <param name="inbox"> inbox </param>
        /// <param name="userID"> 用户ID</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMOtherError: 其它错误，
        ///  </result>
        /// </param>
        /// <returns> 无 </returns>
        void FetchUserInfo(int inbox, long userID, FetchUserInfoDelegate callback);
        
        /// <summary> 批量拉黑、解除拉黑其它用户 </summary>
        /// <param name="inbox"> inbox </param>
        /// <param name="arrUserID"> 其它用户列表</param>
        /// <param name="toBlockList"> 是否拉黑，true: 拉黑，false：解除拉黑</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMOtherError: 其它错误，
        ///  </result>
        /// </param>
        /// <returns> 无 </returns>
        void ModifyUsersBlockList(int inbox, List<long> arrUserID, bool toBlockList, ModifyUsersBlockListDelegate callback);

        /// <summary> 批量拉黑、解除拉黑其它用户 </summary>
        /// <param name="inbox"> inbox </param>
        /// <param name="arrUserID"> 其它用户列表</param>
        /// <param name="toBlockList"> 是否拉黑，true: 拉黑，false：解除拉黑 </param>
        /// <param name="blockType"> 拉黑类型，true: 按会话id拉黑，false：按会话类型拉黑 </param>
        /// <param name="convId"> 会话id </param>
        /// <param name="shortId"> 会话shortId </param>
        /// <param name="convType"> 会话类型 </param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMOtherError: 其它错误，
        ///  </result>
        /// </param>
        /// <returns> 无 </returns>
        void ModifyUsersBlockList(int inbox, List<long> arrUserID, bool toBlockList, bool blockType, string convId, long shortId, IMConversationType convType, ModifyUsersBlockListDelegate callback);

        /// <summary> 获取用户拉黑状态 </summary>
        /// <param name="inbox"> inbox </param>
        /// <param name="userID"> 用户ID</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMOtherError: 其它错误，
        ///  </result>
        /// isInBlocklist: 是否在黑名单。只有result为Success, 该值才有效
        /// </param>
        /// <returns> 无 </returns>
        void FetchUserBlockStatus(int inbox, long userID, FetchUserBlockStatusDelegate callback);

        /// <summary> 获取用户拉黑状态 </summary>
        /// <param name="inbox"> inbox </param>
        /// <param name="userID"> 用户ID</param>
        /// <param name="blockType"> 拉黑类型，true: 按会话id拉黑，false：按会话类型拉黑 </param>
        /// <param name="convId"> 会话id </param>
        /// <param name="shortId"> 会话shortId </param>
        /// <param name="convType"> 会话类型 </param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMOtherError: 其它错误，
        ///  </result>
        /// isInBlocklist: 是否在黑名单。只有result为Success, 该值才有效
        /// </param>
        /// <returns> 无 </returns>
        void FetchUserBlockStatus(int inbox, long userID, bool blockType, string convId, long shortId, IMConversationType convType, FetchUserBlockStatusDelegate callback);

        /// <summary> 获取被拉黑的用户列表 </summary>
        /// <param name="inbox"> inbox </param>
        /// <param name="cursor"> 传入0表示从头开始拉取；如果希望接着某一个cursor连续拉取，则传入callback中返回的cursor数值 </param>
        /// <param name="limit"> 每次拉取的黑名单用户数量 </param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMOtherError: 其它错误，
        ///  </result>
        /// blockedUserList: 是否在黑名单。只有result为Success, 该值才有效
        /// </param>
        /// <returns> 无 </returns>
        void FetchBlockListUsers(int inbox, long cursor, int limit, FetchblockListUsersDelegate callback);

        /// <summary> 获取被拉黑的用户列表 </summary>
        /// <param name="inbox"> inbox </param>
        /// <param name="cursor"> 传入0表示从头开始拉取；如果希望接着某一个cursor连续拉取，则传入callback中返回的cursor数值 </param>
        /// <param name="limit"> 每次拉取的黑名单用户数量 </param>
        /// <param name="blockType"> 拉黑类型，true: 按会话id拉黑，false：按会话类型拉黑 </param>
        /// <param name="convId"> 会话id </param>
        /// <param name="shortId"> 会话shortId </param>
        /// <param name="convType"> 会话类型 </param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMOtherError: 其它错误，
        ///  </result>
        /// blockedUserList: 是否在黑名单。只有result为Success, 该值才有效
        /// </param>
        /// <returns> 无 </returns>
        void FetchBlockListUsers(int inbox, long cursor, int limit, bool blockType, string convId, long shortId, IMConversationType convType, FetchblockListUsersDelegate callback);
        #endregion
        
	    #region Friend

        /// <summary> 获取用户信息 </summary>
        /// <param name="inbox"> inbox </param>
        /// <param name="uid"> 用户ID</param>
        /// <param name="fromSource"> true：从游戏数据源获取；false：从imcloud获取 </param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMOtherError: 其它错误，
        ///  </result>
        /// </param>
        /// <returns> 无 </returns>
        void QueryUserInfo(int inbox, long uid, bool fromSource, QueryUserInfoDelegate callback);
        
        /// <summary> 批量获取用户信息 </summary>
        /// <param name="inbox"> inbox </param>
        /// <param name="uidList"> 用户ID列表</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMOtherError: 其它错误，
        ///  </result>
        /// </param>
        /// <returns> 无 </returns>
        void BatchQueryUserInfo(int inbox, List<long> uidList, BatchQueryUserInfoDelegate callback);
        
        /// <summary> 搜索用户 </summary>
        /// <param name="inbox"> inbox </param>
        /// <param name="key"> 搜索key</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMOtherError: 其它错误，
        ///  </result>
        /// </param>
        /// <returns> 无 </returns>
        void SearchUser(int inbox, string key, SearchUserDelegate callback);

        /// <summary> 删除好友 </summary>
        /// <param name="inbox"> inbox </param>
        /// <param name="arrUserID"> 要删除的用户id列表</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMOtherError: 其它错误，
        ///  </result>
        /// </param>
        /// <returns> 无 </returns>
        void DeleteFriends(int inbox, List<long> arrUserID, DeleteFriendsDelegate callback);

        /// <summary> 获取好友列表 </summary>
        /// <param name="inbox"> inbox </param>
        /// <param name="cursor"> 分页起始游标</param>
        /// <param name="limit"> 拉取数量</param>
        /// <param name="getTotalCount"> 是否获取总数</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMOtherError: 其它错误，
        ///  </result>
        /// </param>
        /// <returns> 无 </returns>
        void GetFriendList(int inbox, long cursor, long limit, bool getTotalCount, GetFriendListDelegate callback);
        
        /// <summary> 获取自己发出的好友申请列表 </summary>
        /// <param name="inbox"> inbox </param>
        /// <param name="cursor"> 分页起始游标</param>
        /// <param name="limit"> 拉取数量</param>
        /// <param name="status"> 申请状态。 -1-全部，0-申请中，1-同意，2-拒绝，3-过期</param>
        /// <param name="getTotalCount"> 是否获取总数</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMOtherError: 其它错误，
        ///  </result>
        /// </param>
        /// <returns> 无 </returns>
        void GetSentApplyList(int inbox, long cursor, long limit, bool getTotalCount, int status, GetSentApplyListDelegate callback);

        /// <summary> 获取收到的的好友申请列表 </summary>
        /// <param name="inbox"> inbox </param>
        /// <param name="cursor"> 分页起始游标</param>
        /// <param name="limit"> 拉取数量</param>
        /// <param name="status"> 申请状态。 -1-全部，0-申请中，1-同意，2-拒绝，3-过期</param>
        /// <param name="getTotalCount"> 是否获取总数</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMOtherError: 其它错误，
        ///  </result>
        /// </param>
        /// <returns> 无 </returns>
        void GetReceivedApplyList(int inbox, long cursor, long limit, bool getTotalCount, int status, GetReceivedApplyListDelegate callback);

        /// <summary> 发起好友申请 </summary>
        /// <param name="inbox"> inbox </param>
        /// <param name="uid"> 用户id</param>
        /// <param name="ext"> 扩展参数</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMOtherError: 其它错误，
        ///  </result>
        /// </param>
        /// <returns> 无 </returns>
        void SendFriendApply(int inbox, long uid, Dictionary<string, string> ext, SendFriendApplyDelegate callback);
        
        /// <summary> 回复好友申请 </summary>
        /// <param name="inbox"> inbox </param>
        /// <param name="arrUserID"> 用户id列表</param>
        /// <param name="attitude"> 回复态度。0-同意，1-拒绝</param>
        /// <param name="ext">扩展参数</param>
        /// <param name="callback">
        ///  <result>
        ///  ErrorCode 取值范围:
        ///      Success: 成功
        ///      IMOtherError: 其它错误，
        ///  </result>
        /// </param>
        /// <returns> 无 </returns>
        void ReplyFriendApply(int inbox, List<long> arrUserID, int attitude, Dictionary<string, string> ext, ReplyFriendApplyDelegate callback);
        #endregion
    }
    
    //错误码处理
    public static partial class ErrorCode
    {
	    /// <summary>
	    /// 本地会话找不到
	    /// 处理建议：保证调用时本地会话存在。或者使用getConversationAsync方法拿到会话之后再执行操作。
	    /// </summary>
	    public const int IMConversationNotFound = -170001;
	    
	    /// <summary>
	    /// 本地消息找不到
	    /// 处理建议：检查操作的消息是否存在。如无法解决，联系IMG oncall。
	    /// </summary>
	    public const int IMMessageNotFound = -170002;
	    
	    /// <summary>
	    /// 参数错误
	    /// 处理建议：检查方法传入参数是否有问题。
	    /// </summary>
	    public const int IMInvalidParam = -170003;
	    
	    /// <summary>
	    /// GSDK的AccessToken无效
	    /// 处理建议：检查GSDK的登录态，确认在GSDK登录成功之后调用。
	    /// </summary>
	    public const int IMAccessTokenInvalid = -170004;  
	    
	    /// <summary>
	    /// GSDK的AccessToken过期
	    /// 处理建议：检查GSDK的登录态，确认在GSDK登录成功之后调用。
	    /// </summary>
	    public const int IMAccessTokenExpired = -170005; 
	    
	    /// <summary>
	    /// GSDK的SdkOpenId无效
	    /// 处理建议：检查GSDK的登录态，确认在GSDK登录成功之后调用。
	    /// </summary>
	    public const int IMSdkOpenIdInvalid = -170006;
	    
	    /// <summary>
	    /// 登录时获取token服务器内部错误
	    /// 处理建议：检查网络是否正常，如无法解决，联系IMG oncall。
	    /// </summary>
	    public const int IMTokenServerError = -170007; 
	    
	    /// <summary>
	    /// 重复登录
	    /// 处理建议：根据具体需求场景，决定放弃操作或者先退出登录再重新登录。
	    /// </summary>
	    public const int IMAlreadyLogin = -170008; 
	    
	    /// <summary>
	    /// 在登录过程中
	    /// 处理建议：登录流程已经在执行中，可以放弃操作。
	    /// </summary>
	    public const int IMLoginInProgress = -170009;
	    
	    /// <summary>
	    /// 当前接口已经在请求中
	    /// 处理建议：根据自己需要处理。
	    /// </summary>
	    public const int IMAlreadyRequesting = -170010;
	    
	    /// <summary>
	    /// SDK内部DB操作异常
	    /// 处理建议：带上对应操作的sdk日志，联系IMG oncall。
	    /// </summary>
	    public const int IMDBError = -171001;
	    
	    /// <summary>
	    /// 消息在发送中
	    /// 处理建议：联系IMG oncall。
	    /// </summary>
	    public const int IMMsgIsSending = -171002;
	    
	    /// <summary>
	    /// 用户不在会话中
	    /// 处理建议：检查对应的用户是否在会话中。
	    /// </summary>
	    public const int IMUserNotInConversation = -171003;
	    
	    /// <summary>
	    /// 会话校验不通过
	    /// 处理建议：检查会话信息是否有异常（比如ticket为空、是否已解散等），如无法解决，联系IMG oncall。
	    /// </summary>
	    public const int IMCheckConversationNotPass = -171004;
	    
	    /// <summary>
	    /// 消息校验不通过
	    /// 处理建议：检查消息格式或内容是否有异常，如无法解决，联系IMG oncall。
	    /// </summary>
	    public const int IMCheckMsgNotPass = -171005;
	    
	    /// <summary>
	    /// 消息校验不通过但自己可见
	    /// 处理建议：属于风控自见情况，当做发送成功处理。
	    /// </summary>
	    public const int IMCheckMsgNotPassButSelfVisible = -171006;
	    
	    /// <summary>
	    /// 用户被禁言
	    /// 处理建议：当做消息发送失败处理。
	    /// </summary>
	    public const int IMUserHasBeenBlocked = -171007;
	    
	    /// <summary>
	    /// 语音或图片消息上传失败
	    /// 处理建议：带上详细错误信息（含二级错误码）联系IMG oncall。
	    /// </summary>
	    public const int IMUploadFail = -171008;
	    
	    /// <summary>
	    /// 好友重复申请
	    /// 处理建议：检查是否已经有相同的申请。
	    /// </summary>
	    public const int IMFriendDuplicateApply = -171009;
	    
	    /// <summary>
	    /// 触达申请数量上限
	    /// 处理建议：修改上限配置或给用户提示。
	    /// </summary>
	    public const int IMFriendSendApplyLimit = -171010;
	    
	    /// <summary>
	    /// 触达对方接收申请数量上限
	    /// 处理建议：修改上限配置或给用户提示。
	    /// </summary>
	    public const int IMFriendReceiveApplyLimit = -171011;
	    
	    /// <summary>
	    /// 已经是好友了
	    /// 处理建议：检查被申请人是否已经是好友。
	    /// </summary>
	    public const int IMFriendAlreadyFriend = -171012;
	    
	    /// <summary>
	    /// 其他错误
	    /// 处理建议：联系IMG oncall。
	    /// </summary>
	    public const int IMOtherError = -179999;
    }
}