using System;
using UNBridgeLib;
using UNBridgeLib.LitJson;
using UnityEngine;

namespace GMSDK {
	public class IMCallbackHandler : BridgeCallBack
	{
		public Action<IMCallbackResult> LoginIMCallback;
		public Action<IMTokenExpiredResult> IMTokenExpiredCallback;
		public Action<InitMessageEndResult> InitMessageEndCallback;
		public Action<InboxInitMessageEndResult> InboxInitMessageEndCallback;
		public Action<ConversationIdResult> ConversationUpdatedCallback;
		public Action<ConversationIdResult> ConversationParticipantsUpdatedCallback;
		public Action<MessageUpdatedResult> MessageUpdatedCallback;
		public Action<MessageListUpdatedResult> MessageListUpdatedCallback;
		public Action<ConversationDataSourceUpdateResult> ConversationDataSourceUpdateCallback;
		public Action<ReceiveBroadCastMessageResult> ReceiveBroadCastMessageCallback;
		public Action<DeleteBroadCastMessageResult> DeleteBroadCastMessageCallback;
		public Action<FriendEventResult> ReceiveFriendApplyCallback;
		public Action<FriendEventResult> DeleteFriendCallback;
		public Action<FriendEventResult> AddFriendCallback;
		//发送消息通知
		public Action<SendMessageResult> SendMessageCallback;

		// 初始化会话列表回调
		public Action<InitConversationDataSourceWithInboxesResult> InitConversationDataSourceWithInboxesCallback;
		// 初始化会话列表回调
		public Action<NumberOfConversationsResult> NumberOfConversationsCallback;
		//根据index获取会话列表中每个对象回调
		public Action<ConversationResult> ConversationAtIndexCallback;
		//根据会话ID生成/获取会话Model回调
		public Action<ConversationResult> GetConversationWithIDCallback;
		//根据接口异步获取会话Model
		public Action<ConversationResult> GetConversationAsyncCallback;
		//设置会话coreInfoExt拓展信息
		public Action<ConversationOperationResult> SetConversationCoreExtCallback;
		//设置会话settingInfoExt拓展信息
		public Action<ConversationOperationResult> SetConversationSettingExtCallback;
		//设置会话localInfoExt拓展信息
		public Action<ConversationOperationResult> SetConversationLocalExtCallback;
		// 更新会话信息回调
		public Action<IMCallbackResult> UpdateCurrentIfNeededCallback;
		//设置聊天name回调
		public Action<ConversationOperationResult> SetNameCallback;
		//设置聊天简介回调
		public Action<ConversationOperationResult> SetDescCallback;
		//设置聊天图标回调
		public Action<ConversationOperationResult> SetIconCallback;
		//设置聊天公告回调
		public Action<ConversationOperationResult> SetNoticeCallback;
		//创建会话回调
		public Action<CreateConversationWithOtherParticipantsResult> CreateConversationWithOtherParticipantsCallback;
		//删除会话回调
		public Action<IMCallbackResult> DeleteWithModeCallback;
		//删除会话中所有消息回调
		public Action<IMCallbackResult> DeleteAllMessagesCallback;
		//主动加群回调
		public Action<ConversationOperationResult> joinGroupCallback;
		//添加用户进群回调
		public Action<AddParticipantsResult> AddParticipantsCallback;
		//删除群聊用户回调
		public Action<RemovedParticipantsResult> RemovedParticipantsCallback;
		//离开会话回调
		public Action<ConversationOperationResult> LeaveConversationCallback;
		//解散会话回调
		public Action<ConversationOperationResult> DismissConversationCallback;
		//设置成员身份回调
		public Action<ConversationOperationResult> SetRoleForParticipantCallback;
		//设置成员群昵称回调
		public Action<ConversationOperationResult> SetAliasForParticipantCallback;
		//设置静音回调
		public Action<ConversationOperationResult> SetMuteCallback;
		//获取会话成员列表回调
		public Action<fetchConversationAllParticipantsResult> FetchConversationAllParticipantsCallback;
		//设置会话全部已读回调
		public Action<IMCallbackResult> MarkAllMessagesAsReadCallback;
		//设置会话指定消息之前全部消息已读回调
		public Action<IMCallbackResult> MarkAllMessagesAsReadBeforeMessageCallback;
		//发送消息回调
		public Action<SendMessageIDResult> SendMessageIDCallback;
		//异步发送消息回调
		public Action<SendMessageAsyncIDResult> SendMessageAsyncIDCallback;
		//重发消息回调
		public Action<ResendMessageResult> ResendMessageCallback;
		//根据会话ID与消息ID生成/获取Model回调
		public Action<GetMessageWithIDResult> GetMessageWithIDCallback;
		//删除消息回调
		public Action<IMCallbackResult> DeleteMessageIDCallback;
		//删除消息回调
		public Action<NumberOfConversationsResult> NumberOfMessagesCallback;
		//根据index获取消息列表中每个对象回调
		public Action<GetMessageWithIDResult> MessageAtIndexCallback;
		//加载历史消息回调
		public Action<IMCallbackResult> LoadOlderMessagesCallback;
		//加载新消息回调
		public Action<IMCallbackResult> LoadNewerMessagesCallback;
		//构建文本消息回调
		public Action<ConstructMessageResult> ConstructTextMessageCallback;
		//构建图片消息回调
		public Action<ConstructMessageResult> ConstructImageMessageCallback;
		//构建语音消息回调
		public Action<ConstructMessageResult> ConstructVoiceMessageCallback;
		//获取文本消息内容回调
		public Action<GetTextContentResult> GetTextContentCallback;
		//获取图片消息内容回调
		public Action<GetImageResult> GetImageCallback;
		//获取语音消息内容回调
		public Action<GetVoiceIdResult> GetVoiceIdCallback;
		//标记为本地消息回调
		public Action<SkipRealSendAndMarkAsSentResult> SkipRealSendAndMarkAsSentCallback;
		//撤回消息回调
		public Action<IMCallbackResult> RecallMessageCallback;
		//超大群发消息回调
		public Action<BroadCastSendMessageResult> BroadCastSendMessageCallback;
		//超大群获取新消息回调
		public Action<LoadNewBroadCastMessageResult> LoadNewBroadCastMessageCallback;
		//超大群获取旧消息回调
		public Action<LoadNewBroadCastMessageResult> LoadOldBroadCastMessageCallback;
		//获取群人数接口回调
		public Action<BroadCastUserCounterResult> BroadCastUserCounterCallback;
		//根据uid获取用户信息回调
		public Action<FetchUserInfoInInboxResult> FetchUserInfoInInboxCallback;
		//批量拉黑/解除拉黑其他用户回调
		public Action<ModifyUsersblockListResult> ModifyUsersBlockListCallback;
		//查询是否被自己拉黑回调
		public Action<FetchUserblockStatusInInboxResult> FetchUserblockStatusInInboxCallback;
		//获取拉黑的用户数组回调
		public Action<FetchblockListUsersInInboxResult> FetchblockListUsersInInboxCallback;
		//查询用户信息回调
		public Action<QueryUserInfoResult> QueryUserInfoCallback;
		//批量查询用户信息回调
		public Action<BatchQueryUserInfoResult> BatchQueryUserInfoCallback;
		//搜索用户回调
		public Action<BatchQueryUserInfoResult> SearchUserCallback;
		//删除好友回调
		public Action<DeleteFriendsResult> DeleteFriendsCallback;
		//获取好友列表回调
		public Action<FriendListResult> GetFriendListCallback;
		//获取发起的好友申请列表回调
		public Action<FriendApplyListResult> GetSentApplyListCallback;
		//获取收到的好友申请列表回调
		public Action<FriendApplyListResult> GetReceivedApplyListCallback;
		//发起好友申请回调
		public Action<IMCallbackResult> SendFriendApplyCallback;
		//回复好友申请回调
		public Action<IMCallbackResult> ReplyFriendApplyCallback;

		public IMCallbackHandler()
		{
			this.OnFailed = new OnFailedDelegate(OnFailCallBack);
			this.OnTimeout = new OnTimeoutDelegate(OnTimeoutCallBack);
		}

		/// <summary>
		/// 失败统一回调，用于调试接口
		/// </summary>
		public void OnFailCallBack(int code, string failMsg)
		{
			LogUtils.D("IM - OnFailCallBack");
			LogUtils.D("接口访问失败 " + code.ToString() + " " + failMsg);
		}
		/// <summary>
		/// 超时统一回调
		/// </summary>
		public void OnTimeoutCallBack()
		{
			JsonData jd = new JsonData();
			jd["code"] = -321;
			jd["message"] = "IM - request time out";
			if (this.OnSuccess != null)
			{
				this.OnSuccess(jd);
			}
		}

		public void onLoginIMCallback(JsonData jd)
		{
			LogUtils.D("onLoginIMCallback");
			IMCallbackResult result = SdkUtil.ToObject<IMCallbackResult>(jd.ToJson());
			SdkUtil.InvokeAction<IMCallbackResult>(LoginIMCallback, result);
		}

		public void onIMTokenExpiredCallback(JsonData jd)
		{
			LogUtils.D("onIMTokenExpiredCallback");
			IMTokenExpiredResult result = SdkUtil.ToObject<IMTokenExpiredResult>(jd.ToJson());
			SdkUtil.InvokeAction<IMTokenExpiredResult>(IMTokenExpiredCallback, result);
		}

		public void onInitMessageEndCallback(JsonData jd)
		{
			LogUtils.D("onInitMessageEndCallback");
			InitMessageEndResult result = SdkUtil.ToObject<InitMessageEndResult>(jd.ToJson());
			SdkUtil.InvokeAction<InitMessageEndResult>(InitMessageEndCallback, result);
		}

		public void onInboxInitMessageEndCallback(JsonData jd)
		{
			LogUtils.D("onInboxInitMessageEndCallback");
			InboxInitMessageEndResult result = SdkUtil.ToObject<InboxInitMessageEndResult>(jd.ToJson());
			SdkUtil.InvokeAction<InboxInitMessageEndResult>(InboxInitMessageEndCallback, result);
		}

		public void onConversationUpdatedCallback(JsonData jd)
		{
			LogUtils.D("onConversationUpdatedCallback");
			ConversationIdResult result = SdkUtil.ToObject<ConversationIdResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConversationIdResult>(ConversationUpdatedCallback, result);
		}

		public void onConversationParticipantsUpdatedCallback(JsonData jd)
		{
			LogUtils.D("onConversationParticipantsUpdatedCallback");
			ConversationIdResult result = SdkUtil.ToObject<ConversationIdResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConversationIdResult>(ConversationParticipantsUpdatedCallback, result);
		}

		public void onMessageUpdatedCallback(JsonData jd)
		{
			LogUtils.D("onMessageUpdatedCallback");
			MessageUpdatedResult result = SdkUtil.ToObject<MessageUpdatedResult>(jd.ToJson());
			SdkUtil.InvokeAction<MessageUpdatedResult>(MessageUpdatedCallback, result);
		}

		public void onMessageListUpdatedCallback(JsonData jd)
		{
			LogUtils.D("onMessageListUpdatedCallback");
			MessageListUpdatedResult result = SdkUtil.ToObject<MessageListUpdatedResult>(jd.ToJson());
			SdkUtil.InvokeAction<MessageListUpdatedResult>(MessageListUpdatedCallback, result);
		}

		public void onConversationDataSourceUpdateCallback(JsonData jd)
		{
			LogUtils.D("onConversationDataSourceUpdateCallback");
			ConversationDataSourceUpdateResult result = SdkUtil.ToObject<ConversationDataSourceUpdateResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConversationDataSourceUpdateResult>(ConversationDataSourceUpdateCallback, result);
		}

		public void onSendMessageCallback(JsonData jd)
		{
			LogUtils.D("IM - onSendMessageCallback");
			SendMessageResult result = SdkUtil.ToObject<SendMessageResult>(jd.ToJson());
			if (SendMessageCallback != null)
			{
				SdkUtil.InvokeAction<SendMessageResult>(SendMessageCallback, result);
			}
		}
		
		public void onReceiveBroadCastMessageCallback(JsonData jd)
		{
			LogUtils.D("IM - onReceiveBroadCastMessageCallback");
			ReceiveBroadCastMessageResult result = SdkUtil.ToObject<ReceiveBroadCastMessageResult>(jd.ToJson());
			if (ReceiveBroadCastMessageCallback != null)
			{
				SdkUtil.InvokeAction<ReceiveBroadCastMessageResult>(ReceiveBroadCastMessageCallback, result);
			}
		}
		
		public void onDeleteBroadCastMessageCallback(JsonData jd)
		{
			LogUtils.D("IM - onDeleteBroadCastMessageCallback");
			DeleteBroadCastMessageResult result = SdkUtil.ToObject<DeleteBroadCastMessageResult>(jd.ToJson());
			if (DeleteBroadCastMessageCallback != null)
			{
				SdkUtil.InvokeAction<DeleteBroadCastMessageResult>(DeleteBroadCastMessageCallback, result);
			}
		}

		public void onReceiveFriendApplyCallback(JsonData jd)
		{
			LogUtils.D("IM - onReceiveFriendApplyCallback");
			FriendEventResult result = SdkUtil.ToObject<FriendEventResult>(jd.ToJson());
			if (ReceiveFriendApplyCallback != null)
			{
				SdkUtil.InvokeAction<FriendEventResult>(ReceiveFriendApplyCallback, result);
			}
		}
		
		public void onDeleteFriendCallback(JsonData jd)
		{
			LogUtils.D("IM - onDeleteFriendCallback");
			FriendEventResult result = SdkUtil.ToObject<FriendEventResult>(jd.ToJson());
			if (DeleteFriendCallback != null)
			{
				SdkUtil.InvokeAction<FriendEventResult>(DeleteFriendCallback, result);
			}
		}
		
		public void onAddFriendCallback(JsonData jd)
		{
			LogUtils.D("IM - onAddFriendCallback");
			FriendEventResult result = SdkUtil.ToObject<FriendEventResult>(jd.ToJson());
			if (AddFriendCallback != null)
			{
				SdkUtil.InvokeAction<FriendEventResult>(AddFriendCallback, result);
			}
		}

		public void OnInitConversationDataSourceWithInboxesCallback(JsonData jd)
		{
			LogUtils.D("IM - OnInitConversationDataSourceWithInboxesCallback");
			InitConversationDataSourceWithInboxesResult result = SdkUtil.ToObject<InitConversationDataSourceWithInboxesResult>(jd.ToJson());
			SdkUtil.InvokeAction<InitConversationDataSourceWithInboxesResult> (InitConversationDataSourceWithInboxesCallback, result);
		}

		public void OnNumberOfConversationsCallback(JsonData jd)
		{
			LogUtils.D("IM - OnNumberOfConversationsCallback");
			NumberOfConversationsResult result = SdkUtil.ToObject<NumberOfConversationsResult>(jd.ToJson());
			SdkUtil.InvokeAction<NumberOfConversationsResult> (NumberOfConversationsCallback, result);
		}

		public void OnConversationAtIndexCallback(JsonData jd)
		{
			LogUtils.D("IM - OnConversationAtIndexCallback");
			ConversationResult result = SdkUtil.ToObject<ConversationResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConversationResult> (ConversationAtIndexCallback, result);
		}

		public void OnGetConversationWithIDCallback(JsonData jd)
		{
			LogUtils.D("IM - OnGetConversationWithIDCallback");
			ConversationResult result = SdkUtil.ToObject<ConversationResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConversationResult> (GetConversationWithIDCallback, result);
		}

		public void OnGetConversationAsyncCallback(JsonData jd)
		{
			LogUtils.D("IM - OnGetConversationAsyncCallback"+jd.ToString());
			ConversationResult result = SdkUtil.ToObject<ConversationResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConversationResult>(GetConversationAsyncCallback, result);
		}

		public void OnUpdateCurrentIfNeededCallback(JsonData jd)
		{
			LogUtils.D("IM - OnUpdateCurrentIfNeededCallback");
			IMCallbackResult result = SdkUtil.ToObject<IMCallbackResult>(jd.ToJson());
			SdkUtil.InvokeAction<IMCallbackResult> (UpdateCurrentIfNeededCallback, result);
		}

		public void OnSetConversationCoreExtCallback(JsonData jd)
		{
			LogUtils.D("IM - OnSetConversationCoreExtCallback");
			ConversationOperationResult result = SdkUtil.ToObject<ConversationOperationResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConversationOperationResult>(SetConversationCoreExtCallback, result);
		}

		public void OnSetConversationSettingExtCallback(JsonData jd)
		{
			LogUtils.D("IM - OnSetConversationSettingExtCallback");
			ConversationOperationResult result = SdkUtil.ToObject<ConversationOperationResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConversationOperationResult>(SetConversationSettingExtCallback, result);
		}

		public void OnSetConversationLocalExtCallback(JsonData jd)
		{
			LogUtils.D("IM - OnSetConversationLocalExtCallback");
			ConversationOperationResult result = SdkUtil.ToObject<ConversationOperationResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConversationOperationResult>(SetConversationLocalExtCallback, result);
		}

		public void OnSetNameCallback(JsonData jd)
		{
			LogUtils.D("IM - OnSetNameCallback");
			ConversationOperationResult result = SdkUtil.ToObject<ConversationOperationResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConversationOperationResult> (SetNameCallback, result);
		}

		public void OnSetDescCallback(JsonData jd)
		{
			LogUtils.D("IM - OnSetDescCallback");
			ConversationOperationResult result = SdkUtil.ToObject<ConversationOperationResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConversationOperationResult> (SetDescCallback, result);
		}

		public void OnSetIconCallback(JsonData jd)
		{
			LogUtils.D("IM - OnSetIconCallback");
			ConversationOperationResult result = SdkUtil.ToObject<ConversationOperationResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConversationOperationResult> (SetIconCallback, result);
		}

		public void OnSetNoticeCallback(JsonData jd)
		{
			LogUtils.D("IM - OnSetNoticeCallback");
			ConversationOperationResult result = SdkUtil.ToObject<ConversationOperationResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConversationOperationResult> (SetNoticeCallback, result);
		}

		public void OnCreateConversationWithOtherParticipantsCallback(JsonData jd)
		{
			LogUtils.D("IM - OnCreateConversationWithOtherParticipantsCallback");
			CreateConversationWithOtherParticipantsResult result = SdkUtil.ToObject<CreateConversationWithOtherParticipantsResult>(jd.ToJson());
			SdkUtil.InvokeAction<CreateConversationWithOtherParticipantsResult> (CreateConversationWithOtherParticipantsCallback, result);
		}

		public void OnDeleteWithModeCallback(JsonData jd)
		{
			LogUtils.D("IM - OnDeleteWithModeCallback");
			IMCallbackResult result = SdkUtil.ToObject<IMCallbackResult>(jd.ToJson());
			SdkUtil.InvokeAction<IMCallbackResult> (DeleteWithModeCallback, result);
		}

		public void OnDeleteAllMessagesCallback(JsonData jd)
		{
			LogUtils.D("IM - OnDeleteAllMessagesCallback");
			IMCallbackResult result = SdkUtil.ToObject<IMCallbackResult>(jd.ToJson());
			SdkUtil.InvokeAction<IMCallbackResult> (DeleteAllMessagesCallback, result);
		}

		public void OnJoinGroupCallback(JsonData jd)
		{
			LogUtils.D("IM - OnJoinGroupCallback");
			ConversationOperationResult result = SdkUtil.ToObject<ConversationOperationResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConversationOperationResult> (joinGroupCallback, result);
		}
		
		public void OnAddParticipantsCallback(JsonData jd)
		{
			LogUtils.D("IM - OnAddParticipantsCallback");
			AddParticipantsResult result = SdkUtil.ToObject<AddParticipantsResult>(jd.ToJson());
			SdkUtil.InvokeAction<AddParticipantsResult> (AddParticipantsCallback, result);
		}

		public void OnRemoveParticipantsCallback(JsonData jd)
		{
			LogUtils.D("IM - OnRemoveParticipantsCallback");
			RemovedParticipantsResult result = SdkUtil.ToObject<RemovedParticipantsResult>(jd.ToJson());
			SdkUtil.InvokeAction<RemovedParticipantsResult> (RemovedParticipantsCallback, result);
		}

		public void OnLeaveConversationCallback(JsonData jd)
		{
			LogUtils.D("IM - OnLeaveConversationCallback");
			ConversationOperationResult result = SdkUtil.ToObject<ConversationOperationResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConversationOperationResult> (LeaveConversationCallback, result);
		}

		public void OnDismissConversationCallback(JsonData jd)
		{
			LogUtils.D("IM - OnDismissConversationCallback");
			ConversationOperationResult result = SdkUtil.ToObject<ConversationOperationResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConversationOperationResult> (DismissConversationCallback, result);
		}

		public void OnSetRoleForParticipantCallback(JsonData jd)
		{
			LogUtils.D("IM - OnSetRoleForParticipantCallback");
			ConversationOperationResult result = SdkUtil.ToObject<ConversationOperationResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConversationOperationResult> (SetRoleForParticipantCallback, result);
		}

		public void OnSetAliasForParticipantCallback(JsonData jd)
		{
			LogUtils.D("IM - OnSetAliasForParticipantCallback");
			ConversationOperationResult result = SdkUtil.ToObject<ConversationOperationResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConversationOperationResult> (SetAliasForParticipantCallback, result);
		}

		public void OnSetMuteCallback(JsonData jd)
		{
			LogUtils.D("IM - OnSetMuteCallback");
			ConversationOperationResult result = SdkUtil.ToObject<ConversationOperationResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConversationOperationResult> (SetMuteCallback, result);
		}

		public void OnFetchConversationAllParticipantsCallback(JsonData jd)
		{
			LogUtils.D("IM - OnFetchConversationAllParticipantsCallback");
			fetchConversationAllParticipantsResult result = SdkUtil.ToObject<fetchConversationAllParticipantsResult>(jd.ToJson());
			SdkUtil.InvokeAction<fetchConversationAllParticipantsResult> (FetchConversationAllParticipantsCallback, result);
		}

		public void OnMarkAllMessagesAsReadCallback(JsonData jd)
		{
			LogUtils.D("IM - OnMarkAllMessagesAsReadCallback");
			IMCallbackResult result = SdkUtil.ToObject<IMCallbackResult>(jd.ToJson());
			SdkUtil.InvokeAction<IMCallbackResult> (MarkAllMessagesAsReadCallback, result);
		}

		public void OnMarkAllMessagesAsReadBeforeMessageCallback(JsonData jd)
		{
			LogUtils.D("IM - OnMarkAllMessagesAsReadBeforeMessageCallback");
			IMCallbackResult result = SdkUtil.ToObject<IMCallbackResult>(jd.ToJson());
			SdkUtil.InvokeAction<IMCallbackResult>(MarkAllMessagesAsReadBeforeMessageCallback, result);
		}

		public void OnSendMessageIDCallback(JsonData jd)
		{
			LogUtils.D("IM - OnSendMessageIDCallback");
			SendMessageIDResult result = SdkUtil.ToObject<SendMessageIDResult>(jd.ToJson());
			SdkUtil.InvokeAction<SendMessageIDResult> (SendMessageIDCallback, result);
		}

		public void OnSendMessageAsyncIDCallback(JsonData jd)
		{
			LogUtils.D("IM - OnSendMessageAsyncIDCallback");
			SendMessageAsyncIDResult result = SdkUtil.ToObject<SendMessageAsyncIDResult>(jd.ToJson());
			SdkUtil.InvokeAction<SendMessageAsyncIDResult>(SendMessageAsyncIDCallback, result);
		}

		public void OnResendMessageCallback(JsonData jd)
		{
			LogUtils.D("IM - OnResendMessageCallback");
			ResendMessageResult result = SdkUtil.ToObject<ResendMessageResult>(jd.ToJson());
			SdkUtil.InvokeAction<ResendMessageResult> (ResendMessageCallback, result);
		}

		public void OnGetMessageWithIDCallback(JsonData jd)
		{
			LogUtils.D("IM - OnGetMessageWithIDCallback");
			GetMessageWithIDResult result = SdkUtil.ToObject<GetMessageWithIDResult>(jd.ToJson());
			SdkUtil.InvokeAction<GetMessageWithIDResult> (GetMessageWithIDCallback, result);
		}

		public void OnDeleteMessageIDCallback(JsonData jd)
		{
			LogUtils.D("IM - OnDeleteMessageIDCallback");
			IMCallbackResult result = SdkUtil.ToObject<IMCallbackResult>(jd.ToJson());
			SdkUtil.InvokeAction<IMCallbackResult> (DeleteMessageIDCallback, result);
		}

		public void OnNumberOfMessagesCallback(JsonData jd)
		{
			LogUtils.D("IM - OnNumberOfMessagesCallback");
			NumberOfConversationsResult result = SdkUtil.ToObject<NumberOfConversationsResult>(jd.ToJson());
			SdkUtil.InvokeAction<NumberOfConversationsResult> (NumberOfMessagesCallback, result);
		}

		public void OnMessageAtIndexCallback(JsonData jd)
		{
			LogUtils.D("IM - OnMessageAtIndexCallback");
			GetMessageWithIDResult result = SdkUtil.ToObject<GetMessageWithIDResult>(jd.ToJson());
			SdkUtil.InvokeAction<GetMessageWithIDResult> (MessageAtIndexCallback, result);
		}

		public void OnLoadOlderMessagesCallback(JsonData jd)
		{
			LogUtils.D("IM - OnLoadOlderMessagesCallback");
			IMCallbackResult result = SdkUtil.ToObject<IMCallbackResult>(jd.ToJson());
			SdkUtil.InvokeAction<IMCallbackResult> (LoadOlderMessagesCallback, result);
		}

		public void OnLoadNewerMessagesCallback(JsonData jd)
		{
			LogUtils.D("IM - OnLoadNewerMessagesCallback");
			IMCallbackResult result = SdkUtil.ToObject<IMCallbackResult>(jd.ToJson());
			SdkUtil.InvokeAction<IMCallbackResult> (LoadNewerMessagesCallback, result);
		}

		public void OnConstructTextMessageCallback(JsonData jd)
		{
			LogUtils.D("IM - OnConstructTextMessageCallback");
			ConstructMessageResult result = SdkUtil.ToObject<ConstructMessageResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConstructMessageResult> (ConstructTextMessageCallback, result);
		}

		public void OnConstructImageMessageCallback(JsonData jd)
		{
			LogUtils.D("IM - OnConstructImageMessageCallback");
			ConstructMessageResult result = SdkUtil.ToObject<ConstructMessageResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConstructMessageResult> (ConstructImageMessageCallback, result);
		}

		public void OnConstructVoiceMessageCallback(JsonData jd)
		{
			LogUtils.D("IM - OnConstructVoiceMessageCallback");
			ConstructMessageResult result = SdkUtil.ToObject<ConstructMessageResult>(jd.ToJson());
			SdkUtil.InvokeAction<ConstructMessageResult> (ConstructVoiceMessageCallback, result);
		}

		public void OnGetTextContentCallback(JsonData jd)
		{
			LogUtils.D("IM - OnGetTextContentCallback");
			GetTextContentResult result = SdkUtil.ToObject<GetTextContentResult>(jd.ToJson());
			SdkUtil.InvokeAction<GetTextContentResult> (GetTextContentCallback, result);
		}

		public void OnGetImageCallback(JsonData jd)
		{
			LogUtils.D("IM - OnGetImageCallback");
			GetImageResult result = SdkUtil.ToObject<GetImageResult>(jd.ToJson());
			SdkUtil.InvokeAction<GetImageResult> (GetImageCallback, result);
		}

		public void OnGetVoiceIdCallback(JsonData jd)
		{
			LogUtils.D("IM - OnGetVoiceIdCallback");
			GetVoiceIdResult result = SdkUtil.ToObject<GetVoiceIdResult>(jd.ToJson());
			SdkUtil.InvokeAction<GetVoiceIdResult> (GetVoiceIdCallback, result);
		}

		public void OnSkipRealSendAndMarkAsSentCallback(JsonData jd)
		{
			LogUtils.D("IM - OnSkipRealSendAndMarkAsSentCallback");
			SkipRealSendAndMarkAsSentResult result = SdkUtil.ToObject<SkipRealSendAndMarkAsSentResult>(jd.ToJson());
			SdkUtil.InvokeAction<SkipRealSendAndMarkAsSentResult> (SkipRealSendAndMarkAsSentCallback, result);
		}

		public void OnRecallMessageCallback(JsonData jd)
		{
			LogUtils.D("IM - OnRecallMessageCallback");
			IMCallbackResult result = SdkUtil.ToObject<IMCallbackResult>(jd.ToJson());
			SdkUtil.InvokeAction<IMCallbackResult> (RecallMessageCallback, result);
		}

		public void OnBroadCastSendMessageCallback(JsonData jd)
		{
			LogUtils.D("IM - OnBroadCastSendMessageCallback");
			BroadCastSendMessageResult result = SdkUtil.ToObject<BroadCastSendMessageResult>(jd.ToJson());
			SdkUtil.InvokeAction<BroadCastSendMessageResult> (BroadCastSendMessageCallback, result);
		}

		public void OnLoadNewBroadCastMessageCallback(JsonData jd)
		{
			LogUtils.D("IM - OnLoadNewBroadCastMessageCallback");
			LoadNewBroadCastMessageResult result = SdkUtil.ToObject<LoadNewBroadCastMessageResult>(jd.ToJson());
			SdkUtil.InvokeAction<LoadNewBroadCastMessageResult> (LoadNewBroadCastMessageCallback, result);
		}
		
		public void OnLoadOldBroadCastMessageCallback(JsonData jd)
		{
			LogUtils.D("IM - OnLoadOldBroadCastMessageCallback");
			LoadNewBroadCastMessageResult result = SdkUtil.ToObject<LoadNewBroadCastMessageResult>(jd.ToJson());
			SdkUtil.InvokeAction<LoadNewBroadCastMessageResult> (LoadOldBroadCastMessageCallback, result);
		}

		public void OnBroadCastUserCounterCallback(JsonData jd)
		{
			LogUtils.D("IM - OnBroadCastUserCounterCallback");
			BroadCastUserCounterResult result = SdkUtil.ToObject<BroadCastUserCounterResult>(jd.ToJson());
			SdkUtil.InvokeAction<BroadCastUserCounterResult> (BroadCastUserCounterCallback, result);
		}

		public void OnFetchUserInfoInInboxCallback(JsonData jd)
		{
			LogUtils.D("IM - OnFetchUserInfoInInboxCallback");
			FetchUserInfoInInboxResult result = SdkUtil.ToObject<FetchUserInfoInInboxResult>(jd.ToJson());
			SdkUtil.InvokeAction<FetchUserInfoInInboxResult> (FetchUserInfoInInboxCallback, result);
		}

		public void OnModifyUsersBlockListCallback(JsonData jd)
		{
			LogUtils.D("IM - OnModifyUsersBlockListCallback");
			LogUtils.D("IM-ModifyUsersBlockListCallback" + jd.ToJson());
			ModifyUsersblockListResult result = SdkUtil.ToObject<ModifyUsersblockListResult>(jd.ToJson());
			SdkUtil.InvokeAction<ModifyUsersblockListResult> (ModifyUsersBlockListCallback, result);
		}

		public void OnFetchUserBlackStatusInInboxCallback(JsonData jd)
		{
			LogUtils.D("IM - OnFetchUserBlackStatusInInboxCallback");
			FetchUserblockStatusInInboxResult result = SdkUtil.ToObject<FetchUserblockStatusInInboxResult>(jd.ToJson());
			SdkUtil.InvokeAction<FetchUserblockStatusInInboxResult> (FetchUserblockStatusInInboxCallback, result);
		}

		public void OnFetchblockListUsersInInboxCallback(JsonData jd)
		{
			LogUtils.D("IM - OnFetchblockListUsersInInboxCallback");
			FetchblockListUsersInInboxResult result = SdkUtil.ToObject<FetchblockListUsersInInboxResult>(jd.ToJson());
			SdkUtil.InvokeAction<FetchblockListUsersInInboxResult> (FetchblockListUsersInInboxCallback, result);
		}
		
		public void OnQueryUserInfoCallback(JsonData jd)
		{
			LogUtils.D("IM - OnQueryUserInfoCallback");
			QueryUserInfoResult result = SdkUtil.ToObject<QueryUserInfoResult>(jd.ToJson());
			SdkUtil.InvokeAction<QueryUserInfoResult> (QueryUserInfoCallback, result);
		}
		
		public void OnBatchQueryUserInfoCallback(JsonData jd)
		{
			LogUtils.D("IM - OnBatchQueryUserInfoCallback");
			BatchQueryUserInfoResult result = SdkUtil.ToObject<BatchQueryUserInfoResult>(jd.ToJson());
			SdkUtil.InvokeAction<BatchQueryUserInfoResult> (BatchQueryUserInfoCallback, result);
		}
		
		public void OnSearchUserCallback(JsonData jd)
		{
			LogUtils.D("IM - OnSearchUserCallback");
			BatchQueryUserInfoResult result = SdkUtil.ToObject<BatchQueryUserInfoResult>(jd.ToJson());
			SdkUtil.InvokeAction<BatchQueryUserInfoResult> (SearchUserCallback, result);
		}
		
		public void OnDeleteFriendsCallback(JsonData jd)
		{
			LogUtils.D("IM - OnDeleteFriendsCallback");
			DeleteFriendsResult result = SdkUtil.ToObject<DeleteFriendsResult>(jd.ToJson());
			SdkUtil.InvokeAction<DeleteFriendsResult> (DeleteFriendsCallback, result);
		}
		
		public void OnGetFriendListCallback(JsonData jd)
		{
			LogUtils.D("IM - OnGetFriendListCallback");
			FriendListResult result = SdkUtil.ToObject<FriendListResult>(jd.ToJson());
			SdkUtil.InvokeAction<FriendListResult> (GetFriendListCallback, result);
		}
		
		public void OnGetSentApplyListCallback(JsonData jd)
		{
			LogUtils.D("IM - OnGetSentApplyListCallback");
			FriendApplyListResult result = SdkUtil.ToObject<FriendApplyListResult>(jd.ToJson());
			SdkUtil.InvokeAction<FriendApplyListResult> (GetSentApplyListCallback, result);
		}
		
		public void OnGetReceivedApplyListCallback(JsonData jd)
		{
			LogUtils.D("IM - OnGetReceivedApplyListCallback");
			FriendApplyListResult result = SdkUtil.ToObject<FriendApplyListResult>(jd.ToJson());
			SdkUtil.InvokeAction<FriendApplyListResult> (GetReceivedApplyListCallback, result);
		}
		
		public void OnSendFriendApplyCallback(JsonData jd)
		{
			LogUtils.D("IM - OnSendFriendApplyCallback");
			IMCallbackResult result = SdkUtil.ToObject<IMCallbackResult>(jd.ToJson());
			SdkUtil.InvokeAction<IMCallbackResult> (SendFriendApplyCallback, result);
		}
		
		public void OnReplyFriendApplyCallback(JsonData jd)
		{
			LogUtils.D("IM - OnReplyFriendApplyCallback");
			IMCallbackResult result = SdkUtil.ToObject<IMCallbackResult>(jd.ToJson());
			SdkUtil.InvokeAction<IMCallbackResult> (ReplyFriendApplyCallback, result);
		}
	}
}
