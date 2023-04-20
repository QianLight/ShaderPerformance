using System.Collections.Generic;
using System.Text;

namespace GMSDK
{
	public class IMMethodName
	{
		public const string Init = "registerIm";
		public const string setLogOpen = "setLogOpen";
		public const string loginIM = "loginIM";
		public const string loginIMV2 = "loginIMV2";
		public const string logoutIM = "logoutIM";
		public const string refreshIMToken = "refreshIMToken";
		public const string configIM = "configIM";
		public const string setAutoPullMessageIntervalSeconds = "setAutoPullMessageIntervalSeconds";
		public const string pullNewMessage = "pullNewMessage";
		public const string connectWS = "connectWS";
		public const string disconnectWS = "disconnectWS";
		public const string isWSConnected = "isWSConnected";
		public const string initConversationDataSourceWithInboxes = "initConversationDataSourceWithInboxes";
		public const string numberOfConversations = "numberOfConversations";
		public const string conversationAtIndex = "conversationAtIndex";
		public const string getConversationWithID = "getConversationWithID";
		public const string getConversationAsync = "getConversationAsync";
		public const string updateCurrentIfNeeded = "updateCurrentIfNeeded";
		public const string setConversationCoreExt = "setConversationCoreExt";
		public const string setConversationSettingExt = "setConversationSettingExt";
		public const string setConversationLocalExt = "setConversationLocalExt";
		public const string setName = "setName";
		public const string setDesc = "setDesc";
		public const string setIcon = "setIcon";
		public const string setNotice = "setNotice";
		public const string createConversationWithOtherParticipants = "createConversationWithOtherParticipants";
		public const string deleteWithMode = "deleteWithMode";
		public const string deleteAllMessages = "deleteAllMessages";
		public const string addParticipants = "addParticipants";
		public const string removeParticipants = "removeParticipants";
		public const string leaveConversation = "leaveConversation";
		public const string dismissConversation = "dismissConversation";
		public const string setRoleForParticipant = "setRoleForParticipant";
		public const string setAliasForParticipant = "setAliasForParticipant";
		public const string setDraft = "setDraft";
		public const string setMute = "setMute";
		public const string userWillEnterCurrentConversation = "userWillEnterCurrentConversation";
		public const string userWillExitCurrentConversation = "userWillExitCurrentConversation";
		public const string fetchConversationAllParticipants = "fetchConversationAllParticipants";
		public const string markAllMessagesAsRead = "markAllMessagesAsRead";
		public const string markAllMessagesAsReadBeforeMessage = "markAllMessagesAsReadBeforeMessage";
		public const string sendMessage = "sendMessage";
		public const string sendMessageAsync = "sendMessageAsync";
		public const string resendMessage = "resendMessage";
		public const string getMessageWithID = "getMessageWithID";
		public const string deleteMessageID = "deleteMessageID";
		public const string numberOfMessages = "numberOfMessages";
		public const string messageAtIndex = "messageAtIndex";
		public const string loadOlderMessages = "loadOlderMessages";
		public const string loadNewerMessages = "loadNewerMessages";
		public const string constructTextMessage = "constructTextMessage";
		public const string constructImageMessage = "constructImageMessage";
		public const string constructVoiceMessage = "constructVoiceMessage";
		public const string constructVoiceMessageWithLocalPath = "constructVoiceMessageWithLocalPath";
		public const string getTextContent = "getTextContent";
		public const string getImage = "getImage";
		public const string getVoiceId = "getVoiceId";
		public const string skipRealSendAndMarkAsSent = "skipRealSendAndMarkAsSent";
		public const string recallMessage = "recallMessage";
		public const string broadCastSendMessage = "broadCastSendMessage";
		public const string loadNewBroadCastMessage = "loadNewBroadCastMessage";
		public const string loadOldBroadCastMessage = "loadOldBroadCastMessage";
		public const string broadCastUserCounter = "broadCastUserCounter";
		public const string fetchUserInfoInInbox = "fetchUserInfoInInbox";
		public const string modifyUsersBlockList = "modifyUsersBlockList";
		public const string fetchUserBlockStatusInInbox = "fetchUserBlockStatusInInbox";
		public const string fetchBlockListUsersInInbox = "fetchBlockListUsersInInbox";
		public const string hasOlderMessages = "hasOlderMessages";
		public const string getCurrentUserId = "getCurrentUserId";
		public const string registerBroadCastListener = "registerBroadCastListener";
		public const string unregisterBroadCastListener = "unregisterBroadCastListener";
		public const string setBroadCastThrottleDelay = "setBroadCastThrottleDelay";
		public const string joinGroup = "joinGroup";
		public const string enterBroadCastConversation = "enterBroadCastConversation";
		public const string exitBroadCastConversation = "exitBroadCastConversation";
		public const string resumeBroadCastConversation = "resumeBroadCastConversation";
		public const string pauseBroadCastConversation = "pauseBroadCastConversation";
		public const string queryUserInfo = "queryUserInfo";
		public const string batchQueryUserInfo = "batchQueryUserInfo";
		public const string searchUser = "searchUser";
		public const string deleteFriends = "deleteFriends";
		public const string getFriendList = "getFriendList";
		public const string getSentApplyList = "getSentApplyList";
		public const string getReceivedApplyList = "getReceivedApplyList";
		public const string sendFriendApply = "sendFriendApply";
		public const string replyFriendApply = "replyFriendApply";
	}
	public class IMResultName
	{
		public const string onloginIM = "onloginIM";
		public const string onInitMessageEnd = "onInitMessageEnd";
		public const string onInboxInitMessageEnd = "onInboxInitMessageEnd";
		public const string onIMTokenExpired = "onIMTokenExpired";
		public const string onConversationUpdated = "onIMConversationUpdated";
		public const string onConversationParticipantsUpdated = "onIMParticipantsUpdated";
		public const string onMessageUpdated = "onIMMessageUpdated";
		public const string onMessageListUpdated = "onIMMessageListUpdated";
		public const string onConversationDataSourceDidUpdate = "onConversationDataSourceDidUpdate";
		public const string onSendMessage = "onSendMessage";
		public const string onReceiveBroadCastMessage = "onReceiveBroadCastMessage";
		public const string onDeleteBroadCastMessage = "onDeleteBroadCastMessage";
		public const string onReceiveFriendApply = "onReceiveFriendApply";
		public const string onDeleteFriend = "onDeleteFriend";
		public const string onAddFriend = "onAddFriend";
	}

	public class GMIMConfig
	{
		//用户的serverID
		public int serverID;

		//用户的roleID
		public string roleID;

		//是否需要进行风控
		public bool isNeedShark;

		//首页消息加载条数，默认20
		public int initialMessageCount = 20;

		//分页消息加载条数，默认20
		public int messageCountPerPage = 20;

		//过滤过空会话
		public bool filterEmptyConversation;

		//过滤已解散群会话
		public bool filterDissolvedGroup = true;

		//过滤已退出群会话
		public bool filterSelfRemovedGroup;

		//超大群推送缓冲时延，单位ms，默认100
		public long broadCastThrottleDelay = 100;

		//超大群内部轮询时间，单位ms，默认10000
		public long broadCastPollingInterval = 10000;

		//长链断线时超大群内部轮询时间，单位ms，默认2000
		public long broadCastPollingIntervalWhenWSDisconnected = 2000;

		//超大群内部轮询拉取的分页个数，默认30
		public int broadCastPollingLimit = 30;
	}

	public class GMIMConversation
	{
		public string conversationID;
		public GMIMConversationType conversationType;
		public bool isDissolved;
		public long shortID;
		public int belongingInbox;
		public bool isCurrentUserAParticipant;
		public int participantsCount;
		public GMIMParticipant selfUserInfo;
		public long updatedAt;
		public string lastMessageIdentifier;
		public int unreadCount;
		public long draftAt;
		public string draftText;
		public bool hasUnreadMention;
		public string oldestUnreadMentionMessageIdentifier;
		public bool hasOlderMessages;
		public Dictionary<string, object> localExt;
		//CoreInfo
		public long ownerID;
		public string name;
		public string desc;
		public string icon;
		public string notice;
		public Dictionary<string, string> coreExt;
		//SettingsInfo
		public bool mute;
		public Dictionary<string, string> settingExt;
	}

	public class GMIMParticipant
	{
		public long userID;
		public string belongingConversationIdentifier;
		public long sortOrder;
		public long role;
		public string alias;
	}

	public class GMIMMessage
	{
		public string messageID;
		public string belongingConversationIdentifier;
		public long sender;
		public string content;
		public int messageType;
		public long createdAt;
		public GMIMMessageStatus status;
		public long serverMessageID;
		public Dictionary<string, object> localExt;
		public Dictionary<string, object> ext;
		public List<long> mentionedUsers;
		public GMIMSenderProfile senderProfile;
		public bool recalled;
		public string recalledContent;
		public long recallerRole;
		public long recallerUserID;
		public GMIMMessageBlockListStatus blockListStatus;
		public int broadCastIndex;
	}

	public class GMIMSenderProfile
	{
		public long uid;
		public string senderNickName;     /// 用户昵称
		public string senderPortrait;     /// 用户头像
		public string basicExtInfo; 
	}

	public class GMIMSendMessage
	{
		public string content;
		public int messageType;
		public List<long> mentionedUsers;
		public Dictionary<string, object> localExt;
		public string receiverRoleID;
		public string receiverSDKOpenID;
		public Dictionary<string, object> ext;
		public bool bSkipRealSend;
		public GMIMImage oImageInfo;
		public GMIMVoice oVoiceInfo;
	}

	public class GMIMImage
	{
		public string oImagePath;
		public string originMD5;
		public string originImageURL;
		public int imageWidth;
		public int imageHeight;
		public string imageFormat;
		public string mimeType;
		public string previewImageURL;
		public int previewImageWidth;
		public int previewImageHeight;
		public string thumbImageURL;
		public int thumbImageWidth;
		public int thumbImageHeight;
	}
	
	public class GMIMVoice
	{
		public string localPath;
		public long duration;
	}

	public class GMIMConversationOperationRet
	{
		public int status;
		public long checkCode;
		public string checkMessage;
		public string extraInfo;
	}

	public class GMIMSendMessageRet
	{
		public int resultCode;
		public long checkCode;
		public string checkMessage;
		public int status;
		public string extraInfo;
		public int blockListStatus;
		public string logId;
	}

	public class GMIMBroadCastRet
	{
		public long conversationShortId;
		public GMIMConversationType conversationType;
		public int counter;
	}

	public class GMIMUserProfile
	{
		public long uid;
		public string userNickName;
		public string userPortrait;
		public string basicExtInfo;
		public string detailExtInfo;
	}
	
	public class GMIMFriendInfo
	{
		public long uid;
		public long applyTimeSecond;
		public Dictionary<string, object> ext;
		public GMIMUserProfile userInfo;
	}
	
	public class GMIMFriendApplyInfo
	{
		public long uid;
		public long applyTimeSecond;
		public Dictionary<string, object> ext;
		public GMIMUserProfile userInfo;
		public int applyStatus;
	}

	public class GMIMBlackListUser 
	{
		public long userID;
		public long createdAt; 
	}

	public enum GMIMMessageType
	{
		GMIMMessageTypeText = 10001, /**文本消息*/
		GMIMMessageTypeImage = 10003,  /**图片消息*/
		GMIMMessageTypeVoice = 20000, /**语音消息*/
		GMIMMessageTypeEmoticon = 20001, /**表情消息*/
	};

	public enum GMIMMessageBlockListStatus
	{
		GMIMMessageBlockListStatusNone                          = 0,//无
		GMIMMessageBlockListStatusYouBlockOther                 = 1,//你拉黑了对方
		GMIMMessageBlockListStatusOtherBlockYou                 = 2,//对方拉黑了你
		GMIMMessageBlockListStatusBlockEachOther                = 3,//互相拉黑
	};

	public enum GMIMConversationType
	{
		GMIMConversationType1to1Chat = 1,//单聊。无论创建多少次，和同一个人的单聊永远会有同一个 ID
		GMIMConversationTypeGroupChat = 2,//群聊
		GMIMConversationTypeLiveChat = 3,//轻直播
		GMIMConversationTypeBroadCastChat = 4,//超大群
	}

	public enum GMIMMessageStatus
	{
		GMIMMessageStatusPreparing        = 0,//准备中
		GMIMMessageStatusSending          = 1,//发送中
		GMIMMessageStatusSendSuccess      = 2,//发送成功
		GMIMMessageStatusSendFailed       = 3,//发送失败
		GMIMMessageStatusReceived         = 5,//发送失败
	};

	public enum GMIMConversationDeleteMode
	{
		GMIMConversationDeleteModeLocalDevice = 0,
		GMIMConversationDeleteModeAllMyDevice = 1,
	};

	public enum GMIMConversationParticipantRole
	{
		GMIMConversationParticipantRoleNormal   = 0,//普通成员
		GMIMConversationParticipantRoleOwner    = 1,//拥有者。一个群只能有一个拥有者。
		GMIMConversationParticipantRoleAdmin    = 2,//管理员
		GMIMConversationParticipantRoleVisitor  = 3,//游客
		GMIMConversationParticipantRoleSystem   = 4,//系统，目前仅用于风控撤回角色
	};

	public enum GMIMCheckCode
	{
		REJECT = -4005, //风控被拒
		SELF_VISIBLE = -4001, //风控自见
		BANNED = -4002, //业受禁言
		BANNED_BY_IM = -4, //IM禁言
		YOU_BLOCK_OTHER = -1, //你拉黑了别人
		OTHER_BLOCK_YOU = -2, //别人被你拉黑
		BLOCK_EACH_OTHER = -3 //互相拉黑
	}

	public class InitConversationDataSourceWithInboxesResult
	{
		public string conversationDataSourceID;
	}

	public class NumberOfConversationsResult
	{
		public int count;
	}

	public class ConversationResult
	{
		public GMIMConversation conversation;
	}

	public class ConversationOperationResult : IMCallbackResult
	{
		public GMIMConversationOperationRet ret;
	}

	public class CreateConversationWithOtherParticipantsResult : IMCallbackResult
	{
		public string conversationIdentifier;
		public GMIMConversationOperationRet ret;
	}

	public class AddParticipantsResult : IMCallbackResult
	{
		public List<long> addedParticipants;
		public GMIMConversationOperationRet ret;
	}

	public class RemovedParticipantsResult : IMCallbackResult
	{
		public List<long> removedParticipants;
		public GMIMConversationOperationRet ret;
	}

	public class fetchConversationAllParticipantsResult : IMCallbackResult
	{
		public List<GMIMParticipant> participants;
	}

	public class SendMessageResult
	{
		public string messageId;
		public GMIMSendMessageRet ret;
	}

	public class SendMessageIDResult
	{
		public string messageId;
	}

	public class SendMessageAsyncIDResult
	{
		public string messageId;
	}

	public class ResendMessageResult
	{
		public GMIMSendMessageRet ret;
	}

	public class GetMessageWithIDResult
	{
		public GMIMMessage message;
	}

	public class ConstructMessageResult
	{
		public GMIMSendMessage sendMessage;
		public bool success;
	}

	public class GetTextContentResult
	{
		public string text;
	}

	public class GetVoiceIdResult
	{
		public string voiceID;
	}

	public class GetImageResult
	{
		public GMIMImage image;
	}

	public class SkipRealSendAndMarkAsSentResult
	{
		public GMIMSendMessage ret;
	}

	public class IMCallbackResult : CallbackResult
	{
		public string logId;

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("{")
				.Append("code=").Append(code)
				.Append(", message='").Append(message).Append("'")
				.Append(", logId='").Append(logId).Append("'")
				.Append("}");
			return sb.ToString();
		}
	}

	public class BroadCastSendMessageResult : IMCallbackResult
	{
		public GMIMSendMessageRet ret;
	}

	public class LoadNewBroadCastMessageResult : IMCallbackResult
	{
		public List<GMIMMessage> messagesArray;
		public long nextCursor;
	}

	public class BroadCastUserCounterResult : IMCallbackResult
	{
		public List<GMIMBroadCastRet> infosArray;
	}

	public class FetchUserInfoInInboxResult : IMCallbackResult
	{
		public GMIMUserProfile profile;
	}

	public class ModifyUsersblockListResult : IMCallbackResult
	{
		public List<long> modifiedUsers;
	}

	public class FetchUserblockStatusInInboxResult : IMCallbackResult
	{
		public bool isInBlockList;
	}

	public class FetchblockListUsersInInboxResult : IMCallbackResult
	{
		public List<GMIMBlackListUser> blockListUsers;
		public bool hasMore;
		public long nextCursor;
	}
	
	public class QueryUserInfoResult : IMCallbackResult
	{
		public GMIMUserProfile userInfo;
	}
	
	public class BatchQueryUserInfoResult : IMCallbackResult
	{
		public List<GMIMUserProfile> userInfoList;
	}
	
	public class DeleteFriendsResult : IMCallbackResult
	{
		public List<long> uidList;
	}

	public class FriendListResult : IMCallbackResult
	{
		public bool hasMore;
		public long nextCursor;
		public List<GMIMFriendInfo> friendList;
		public long totalCount;
	}
	
	public class FriendApplyListResult : IMCallbackResult
	{
		public bool hasMore;
		public long nextCursor;
		public List<GMIMFriendApplyInfo> applyList;
		public long totalCount;
	}

	public class ConversationIdResult
	{
		public string conversationId;
	}

	public class MessageUpdatedResult
	{
		public string conversationId;
		public string messageId;
	}

	public class MessageListUpdatedResult
	{
		public string conversationId;
		public int reason;
		public List<string> insertMessageIds;
		public List<string> deleteMessageIds;
	}

	public class ConversationDataSourceUpdateResult
	{
		public string conversationDataSourceID;
		public List<string> beforeUpdateConversationIds;
		public List<string> afterUpdateConversationIds;
	}

	public class IMTokenExpiredResult
	{
	}

	public class InitMessageEndResult
	{
	}

	public class InboxInitMessageEndResult
	{
		public int inbox;
	}

	public class ReceiveBroadCastMessageResult
	{
		public string conversationId;
		public List<GMIMMessage> messagesArray;
		public long nextCursor;
	}
	
	public class DeleteBroadCastMessageResult
	{
		public string conversationId;
		public long msgServerId;
	}

	public class FriendEventResult
	{
		public int inbox;
		public long uid;
		public Dictionary<string, object> ext;
	}
}