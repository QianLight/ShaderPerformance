using System;
using System.Collections.Generic;
using GMSDK;

namespace GSDK
{
    public class IMConfig
    {
        /// <summary>
        /// 用户的serverID
        /// </summary>
        public int ServerID;

        /// <summary>
        /// 用户的roleID
        /// </summary>
        public string RoleID;

        /// <summary>
        /// 是否需要进行风控
        /// </summary>
        public bool NeedShark;

        /// <summary>
        /// 首页消息加载条数，默认20
        /// </summary>
        public int InitialMessageCount = 20;

        /// <summary>
        /// 分页消息加载条数，默认20
        /// </summary>
        public int MessageCountPerPage = 20;

        /// <summary>
        /// 过滤过空会话
        /// </summary>
        public bool FilterEmptyConversation;

        /// <summary>
        /// 过滤已解散群会话
        /// </summary>
        public bool FilterDissolvedGroup = true;

        /// <summary>
        /// 过滤已退出群会话
        /// </summary>
        public bool FilterSelfRemovedGroup;

        /// <summary>
        /// 超大群推送缓冲时延，单位ms，默认100
        /// </summary>
        public long BroadCastThrottleDelay = 100;

        /// <summary>
        /// 超大群内部轮询时间，单位ms，默认10000
        /// </summary>
        public long BroadCastPollingInterval = 10000;

        /// <summary>
        /// 长链断线时超大群内部轮询时间，单位ms，默认2000
        /// </summary>
        public long BroadCastPollingIntervalWhenWSDisconnected = 2000;

        /// <summary>
        /// 超大群内部轮询拉取的分页个数，默认30
        /// </summary>
        public int BroadCastPollingLimit = 30;
    }

    public class IMConversation
    {
        public string ConversationID;
        public IMConversationType ConversationType;
        public bool IsDissolved;
        public long ShortID;
        public int BelongingInbox;
        public bool IsCurrentUserAParticipant;
        public int ParticipantsCount;
        public IMParticipant SelfUserInfo;
        public long UpdatedAt;
        public string LastMessageIdentifier;
        public int UnreadCount;
        public long DraftAt;
        public string DraftText;
        public bool HasUnreadMention;
        public string OldestUnreadMentionMessageIdentifier;
        public bool HasOlderMessages;
        public Dictionary<string, object> LocalExt;
        //CoreInfo
        public long OwnerID;
        public string Name;
        public string Description;
        public string Icon;
        public string Notice;
        public Dictionary<string, string> CoreExt;
        //SettingsInfo
        public bool IsMuted;
        public Dictionary<string, string> SettingExt;
    }

    public class IMParticipant
    {
        public long UserID;
        public string BelongingConversationID;
        public long SortOrder;
        public IMConversationParticipantRole Role;
        public string Alias;
    }

    public class IMMessage
    {
        public string MessageID;
        public string BelongingConversationIdentifier;
        public long Sender;
        public string Content;
        public IMMessageType MessageType;
        public long CreatedAt;
        public IMMessageStatus Status;
        public long ServerMessageID;
        public Dictionary<string, object> LocalExt;
        public Dictionary<string, object> ExtraInfo;
        public List<long> MentionedUsers;
        public IMSenderProfile SenderProfile;
        public bool HasRecalled;
        public string RecalledContent;
        public long RecallerRole;
        public long RecallerUserID;
		public GMIMMessageBlockListStatus BlockListStatus;
        /// <summary>
        /// 只有超大群消息有值，普通会话为0
        /// </summary>
        public int BroadCastIndex;
    }

    public class IMSenderProfile
    {
        public long Uid;
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string SenderNickName;
        /// <summary>
        /// 用户头像
        /// </summary>
		public string SenderPortrait;
		public string BasicExtInfo;
    }

    public class IMSendMessageInfo
    {
		public string Content;
		public IMMessageType MessageType;
        public List<long> MentionedUsers;
		public Dictionary<string, object> LocalExt;
        public string ReceiverRoleID;
        public string ReceiverSDKOpenID;
		public Dictionary<string, object> ExtraInfo;
		public bool SkipRealSend;
		public GMIMImage ImageInfo;
        public GMIMVoice VoiceInfo;
    }

    public class IMImage
    {
		public string ImagePath;
        public string OriginMD5;
        public string OriginImageURL;
        public int Width;
        public int Height;
        public string ImageFormat;
        public string MimeType;
        public string PreviewImageURL;
        public int PreviewImageWidth;
        public int PreviewImageHeight;
        public string ThumbImageURL;
        public int ThumbImageWidth;
        public int ThumbImageHeight;
    }

    public class IMConversationOperationRet
    {
        public int Status; // Todo:
        public long CheckCode;
        public string CheckMessage;
        public string ExtraInfo;
    }


    public class IMBroadcastRet
    {
        public long ConversationShortId;
        public IMConversationType ConversationType;
        public int Counter;
    }

    public class IMUserProfile
    {
        public long Uid;
        public string UserNickName;
        public string UserPortrait;
        public string BasicExtInfo;
        public string DetailExtInfo;
    }

    public class IMFriendInfo
    {
        public long Uid;
        public long ApplyTimeSecond;
        public Dictionary<string, object> Ext;
        public GMIMUserProfile UserInfo;
    }
    
    public class IMFriendInfoList
    {
        public List<IMFriendInfo> FriendList;
        public bool HasMore;
        public long NextCursor;
        public long TotalCount;
    }

    public class IMFriendApplyInfo
    {
        public long Uid;
        public long ApplyTimeSecond;
        public Dictionary<string, object> Ext;
        public GMIMUserProfile UserInfo;
        public int ApplyStatus;
    }
    
    public class IMFriendApplyInfoList
    {
        public List<IMFriendApplyInfo> ApplyList;
        public bool HasMore;
        public long NextCursor;
        public long TotalCount;
    }

    public class IMBlackListUser
    {
        public long UserId;
        public long CreateTime;
    }

	public class IMImageInfo
	{
		public string ImagePath;
		public int ImageWidth;
		public int ImageHeight;
		public string Mime;
		public string Format;
	}

    public enum IMMessageType
    {
        /// <summary>
        /// 文本消息
        /// </summary>
        Text = 10001,
        /// <summary>
        /// 图片消息
        /// </summary>
        Image = 10003,
        /// <summary>
        /// 语音消息
        /// </summary>
        Voice = 20000,
        /// <summary>
        /// 表情消息
        /// </summary>
        Emoticon = 20001, 
    };

    public enum IMMessageBlockListStatus
    {
        /// <summary>
        /// 无黑名单信息
        /// </summary>
        None = 0,
        /// <summary>
        /// 你拉黑了对方
        /// </summary>
        YouBlockOther = 1,
        /// <summary>
        /// 对方拉黑了你
        /// </summary>
        OtherBlockYou = 2,
        /// <summary>
        /// 互相拉黑
        /// </summary>
        BlockEachOther = 3,
    };

    public enum IMConversationType
    {
        /// <summary>
        /// 单聊。无论创建多少次，和同一个人的单聊永远会有同一个 ID
        /// </summary>
        C2C = 1,
        /// <summary>
        /// 群聊
        /// </summary>
        Group = 2,
        /// <summary>
        /// 轻直播
        /// </summary>
        Live = 3,
        /// <summary>
        /// 超大群
        /// </summary>
        BroadCast = 4,
    }

    public enum IMMessageStatus
    {
        /// <summary>
        /// 准备中
        /// </summary>
        Preparing = 0,
        /// <summary>
        /// 发送中
        /// </summary>
        Sending = 1,
        /// <summary>
        /// 发送成功
        /// </summary>
        SendSuccess = 2,
        /// <summary>
        /// 发送失败
        /// </summary>
        SendFailed = 3,
        /// <summary>
        /// 发送失败
        /// </summary>
        Received = 5,
    };

    public enum FriendReply
    {
        /// <summary>
        /// 同意
        /// </summary>
        ACCEPT = 0,

        /// <summary>
        /// 拒绝
        /// </summary>
        DENY = 1
    }

    public enum FriendApplyStatus
    {
        // -1-全部，0-申请中，1-同意，2-拒绝，3-过期
        /// <summary>
        /// 申请中
        /// </summary>
        APPLYING = 0,

        /// <summary>
        /// 已同意
        /// </summary>
        ACCEPTED = 1,

        /// <summary>
        /// 已拒绝
        /// </summary>
        DENIED = 2,

        /// <summary>
        /// 已过期
        /// </summary>
        EXPIRED = 3,

        /// <summary>
        /// 全部
        /// </summary>
        ALL = -1
    }

    public enum IMConversationDeleteMode
    {
        LocalDevice = 0,
        AllMyDevice = 1,
    };

    public enum IMConversationParticipantRole
    {
        /// <summary>
        /// 普通成员
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 拥有者。一个群只能有一个拥有者。
        /// </summary>
        Owner = 1,
        /// <summary>
        /// 管理员
        /// </summary>
        Admin = 2,
        /// <summary>
        /// 游客
        /// </summary>
        Visitor = 3,
        /// <summary>
        /// 系统，目前仅用于风控撤回角色
        /// </summary>
        System = 4,
    };


    public class FetchblockListUsersInfo
    {
        public List<IMBlackListUser> BlockListUsers;
        public bool HasMore;
        public long NextCursor;
    }


    public class ConversationIdInfo
    {
        public string ConversationId;
    }

    public class MessageUpdatedInfo
    {
        public string ConversationId;
        public string MessageId;
    }

    public class MessageListUpdatedInfo
    {
        public string ConversationId;
        public int Reason;
        public List<string> InsertMessageIdList;
        public List<string> DeleteMessageIdList;
    }

    public class ConversationDataSourceUpdateInfo
    {
        public string ConversationDataSourceID;
        public List<string> BeforeUpdateConversationIds;
        public List<string> AfterUpdateConversationIds;
    }

    public class IMTokenExpiredInfo
    {
    }

    public class LoadNewBroadcastMessageInfo
    {
        public List<IMMessage> MessagesList;
        public long NextCursor;
    }

    public class InitMessageEndInfo
    {
    }

    public class InboxInitMessageEndInfo
    {
        public int Inbox;
    }

    public class ReceiveBroadcastMessageInfo
    {
        public string ConversationId;
        public List<IMMessage> MessagesList;
        public long NextCursor;
    }
    
    public class DeleteBroadcastMessageInfo
    {
        public string ConversationId;
        public long MsgServerId;
    }

    public class FriendEventInfo
    {
        public int Inbox;
        public long Uid;
        public Dictionary<string, object> Ext;
    }
}
