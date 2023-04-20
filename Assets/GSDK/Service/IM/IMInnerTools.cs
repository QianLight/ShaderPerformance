using System.Collections.Generic;
using GMSDK;
using UNBridgeLib.LitJson;

namespace GSDK
{
    internal class IMConvertTool
    {
        private static void injectLogId(Result result, string logId)
        {
            if (result == null || string.IsNullOrEmpty(logId))
            {
                return;
            }

            string origin = result.AddtionalInfo;
            if (origin == null)
            {
                origin = "";
            }

            result.AddtionalInfo = JsonMapper.ToJson(new {logId, origin});
        }

        public static Result Convert(IMCallbackResult res)
        {
            int err = res.code;

            if (err != ErrorCode.Success)
            {
                GLog.LogError("[IM] Error occurred when convert IMCallbackResult! code:" + err + ", message:" +
                              res.message);
            }

            Result result = new Result(err, res.message, res.extraErrorCode, res.extraErrorMessage, res.additionalInfo);
            injectLogId(result, res.logId);
            return result;
        }

        public static Result Convert(ConversationOperationResult res)
        {
            Result result;
            int err = res.code;
            if (res.ret == null)
            {
                result = new Result(err, res.message, res.extraErrorCode, res.extraErrorMessage, res.additionalInfo);
                injectLogId(result, res.logId);
                return result;
            }

            if (err != ErrorCode.Success)
            {
                GLog.LogError("[IM] Error occurred when convert ConversationOperationResult! code:" + err
                    + ", message:" + res.message
                    + ", status:" + res.ret.status
                    + ", checkCode:" + res.ret.checkCode
                    + ", checkMessage:" + res.ret.checkMessage
                    + ", extraInfo:" + res.ret.extraInfo);
            }

            string additionalInfo = "";
            if (res.ret.status > 0)
            {
                additionalInfo = "status=" + res.ret.status;
            }

            result = new Result(err, res.message, (int) res.ret.checkCode, res.ret.checkMessage, additionalInfo);
            injectLogId(result, res.logId);
            return result;
        }

        public static Result Convert(GMIMSendMessageRet ret)
        {
            int err = ret.resultCode;
            if (err != ErrorCode.Success)
            {
                var errorLog = "[IM] Error occurred when convert GMIMSendMessageRet! code:" + err
                    + ", checkCode:" + ret.checkCode
                    + ", checkMessage:" + ret.checkMessage
                    + ", extraInfo:" + ret.extraInfo;
                if (ret.checkCode == (int) GMIMCheckCode.REJECT
                    || ret.checkCode == (int) GMIMCheckCode.SELF_VISIBLE
                    || ret.checkCode == (int) GMIMCheckCode.BANNED
                    || ret.checkCode == (int) GMIMCheckCode.BANNED_BY_IM)
                {
                    GLog.LogWarning(errorLog);
                }
                else
                {
                    GLog.LogError(errorLog);
                }
            }

            string additionalInfo = "";
            if (ret.status > 0)
            {
                additionalInfo = "status=" + ret.status;
            }

            Result result = new Result(err, ret.extraInfo, (int) ret.checkCode, ret.checkMessage, additionalInfo);
            injectLogId(result, ret.logId);
            return result;
        }

        public static GMIMConfig Convert(IMConfig cfg)
        {
            return new GMIMConfig()
            {
                initialMessageCount = cfg.InitialMessageCount,
                isNeedShark = cfg.NeedShark,
                messageCountPerPage = cfg.MessageCountPerPage,
                roleID = cfg.RoleID,
                serverID = cfg.ServerID,
                filterDissolvedGroup = cfg.FilterDissolvedGroup,
                filterEmptyConversation = cfg.FilterEmptyConversation,
                filterSelfRemovedGroup = cfg.FilterSelfRemovedGroup,
                broadCastThrottleDelay = cfg.BroadCastThrottleDelay,
                broadCastPollingInterval = cfg.BroadCastPollingInterval,
                broadCastPollingIntervalWhenWSDisconnected = cfg.BroadCastPollingIntervalWhenWSDisconnected,
                broadCastPollingLimit = cfg.BroadCastPollingLimit
            };
        }

        public static GMIMSendMessage Convert(IMSendMessageInfo msg)
        {

            return new GMIMSendMessage()
            {
                content = msg.Content,
                messageType = (int) msg.MessageType,
                mentionedUsers = msg.MentionedUsers,
                localExt = msg.LocalExt,
                receiverRoleID = msg.ReceiverRoleID,
                receiverSDKOpenID = msg.ReceiverSDKOpenID,
                ext = msg.ExtraInfo,
				bSkipRealSend = msg.SkipRealSend,
				oImageInfo = msg.ImageInfo,
                oVoiceInfo = msg.VoiceInfo
            };
        }

        public static IMSendMessageInfo Convert(GMIMSendMessage msg)
        {
            return (msg == null) ? null: new IMSendMessageInfo()
            {
                Content = msg.content,
                MessageType = (IMMessageType)msg.messageType,
                MentionedUsers = msg.mentionedUsers,
                LocalExt = msg.localExt,
                ReceiverRoleID = msg.receiverRoleID,
                ReceiverSDKOpenID = msg.receiverSDKOpenID,
                ExtraInfo = msg.ext,
				SkipRealSend = msg.bSkipRealSend,
				ImageInfo = msg.oImageInfo,
                VoiceInfo = msg.oVoiceInfo
            };
        }

        public static IMSenderProfile Convert(GMIMSenderProfile profile)
        {
            if (profile == null)
            {
                return null;
            }
            return new IMSenderProfile() { Uid = profile.uid, BasicExtInfo = profile.basicExtInfo, SenderNickName = profile.senderNickName, SenderPortrait = profile.senderPortrait };
        }

        public static GMIMSenderProfile Convert(IMSenderProfile profile)
        {
            if (profile == null)
            {
                return null;
            }
            return new GMIMSenderProfile() { uid = profile.Uid, basicExtInfo = profile.BasicExtInfo, senderNickName = profile.SenderNickName, senderPortrait = profile.SenderPortrait };
        }

        public static IMMessage Convert(GMIMMessage msg)
        {
            if (msg == null)
            {
                return null;
            }
            return new IMMessage()
            {
                MessageID = msg.messageID,
                BelongingConversationIdentifier = msg.belongingConversationIdentifier,
                Sender = msg.sender,
                Content = msg.content,
                MessageType = (IMMessageType)msg.messageType,
                CreatedAt = msg.createdAt,
                Status = (IMMessageStatus)msg.status,
                ServerMessageID = msg.serverMessageID,
                LocalExt = msg.localExt,
                ExtraInfo = msg.ext,
                MentionedUsers = msg.mentionedUsers,
                SenderProfile = Convert(msg.senderProfile),
                HasRecalled = msg.recalled,
                RecalledContent = msg.recalledContent,
                RecallerRole = msg.recallerRole,
                RecallerUserID = msg.recallerUserID,
                BlockListStatus = msg.blockListStatus,
                BroadCastIndex = msg.broadCastIndex,
            };
        }

        public static GMIMMessage Convert(IMMessage msg)
        {
            if (msg == null)
            {
                return null;
            }
            return new GMIMMessage()
            {
                messageID = msg.MessageID,
                belongingConversationIdentifier = msg.BelongingConversationIdentifier,
                sender = msg.Sender,
                content = msg.Content,
                messageType = (int)msg.MessageType,
                createdAt = msg.CreatedAt,
                status = (GMIMMessageStatus)msg.Status,
                serverMessageID = msg.ServerMessageID,
                localExt = msg.LocalExt,
                ext = msg.ExtraInfo,
                mentionedUsers = msg.MentionedUsers,
                senderProfile = Convert(msg.SenderProfile),
                recalled = msg.HasRecalled,
                recalledContent = msg.RecalledContent,
                recallerRole = msg.RecallerRole,
                recallerUserID = msg.RecallerUserID,
                blockListStatus = msg.BlockListStatus,
                broadCastIndex = msg.BroadCastIndex,// Todo: enum Type
            };
        }

        public static List<IMMessage> Convert(List<GMIMMessage> msgList)
        {
            if (msgList == null)
            {
                return null;
            }

            List<IMMessage> ret = new List<IMMessage>();
            for (int i = 0; i < msgList.Count; i++)
            {
                ret.Add(Convert(msgList[i]));
            }

            return ret;
        }

        public static IMImage Convert(GMIMImage img)
        {

            if (img == null)
            {
                return null;
            }
            return new IMImage()
            {
				ImagePath = img.oImagePath
			,
                Height = img.imageHeight
            ,
                ImageFormat = img.imageFormat
            ,
                MimeType = img.mimeType
            ,
                OriginImageURL = img.originImageURL
            ,
                OriginMD5 = img.originMD5
            ,
                PreviewImageHeight = img.previewImageHeight
            ,
                PreviewImageURL = img.previewImageURL
            ,
                PreviewImageWidth = img.previewImageWidth
            ,
                ThumbImageHeight = img.thumbImageHeight
            ,
                ThumbImageURL = img.thumbImageURL
            ,
                ThumbImageWidth = img.thumbImageWidth
            ,
                Width = img.imageWidth
            };
        }

        public static IMParticipant Convert(GMIMParticipant part)
        {
            if (part == null)
            {
                return null;
            }
            return new IMParticipant()
            {
                Alias = part.alias,
                BelongingConversationID = part.belongingConversationIdentifier,
                Role = (IMConversationParticipantRole) part.role,
                SortOrder = part.sortOrder,
                UserID = part.userID
            };
        }

        public static IMConversation Convert(GMIMConversation con)
        {
            if (con == null)
            {
                return null;
            }
            return new IMConversation()
            {
                ConversationID = con.conversationID, BelongingInbox = con.belongingInbox, ConversationType = (IMConversationType) con.conversationType
				, Description = con.desc, DraftAt = con.draftAt, DraftText = con.draftText, HasUnreadMention = con.hasUnreadMention
				, Icon = con.icon, IsCurrentUserAParticipant = con.isCurrentUserAParticipant, IsDissolved = con.isDissolved
				, IsMuted = con.mute, LastMessageIdentifier = con.lastMessageIdentifier, Name = con.name, Notice = con.notice
				, OldestUnreadMentionMessageIdentifier = con.oldestUnreadMentionMessageIdentifier, OwnerID = con.ownerID
				, ParticipantsCount = con.participantsCount, SelfUserInfo = Convert(con.selfUserInfo), ShortID = con.shortID
                , UnreadCount = con.unreadCount, UpdatedAt = con.updatedAt, HasOlderMessages = con.hasOlderMessages, LocalExt = con.localExt
                , CoreExt = con.coreExt, SettingExt = con.settingExt
            };
        }

        public static IMUserProfile Convert(GMIMUserProfile profile)
        {
            if (profile == null)
            {
                return null;
            }
            return new IMUserProfile() { Uid = profile.uid, BasicExtInfo = profile.basicExtInfo, DetailExtInfo = profile.detailExtInfo, UserNickName = profile.userNickName, UserPortrait = profile.userPortrait };
        }

        public static IMFriendInfo Convert(GMIMFriendInfo friend)
        {
            if (friend == null)
            {
                return null;
            }

            return new IMFriendInfo()
            {
                Uid = friend.uid, ApplyTimeSecond = friend.applyTimeSecond, Ext = friend.ext, UserInfo = friend.userInfo
            };
        }

        public static IMFriendApplyInfo Convert(GMIMFriendApplyInfo friendApply)
        {
            if (friendApply == null)
            {
                return null;
            }

            return new IMFriendApplyInfo()
            {
                Uid = friendApply.uid, ApplyTimeSecond = friendApply.applyTimeSecond, Ext = friendApply.ext,
                UserInfo = friendApply.userInfo, ApplyStatus = friendApply.applyStatus
            };
        }

        public static IMBlackListUser Convert(GMIMBlackListUser usr)
        {
            if (usr == null)
            {
                return null;
            }
            return new IMBlackListUser() { CreateTime = usr.createdAt, UserId = usr.userID };
        }

        public static GMIMBroadCastRet Convert(IMBroadcastRet ret)
        {
            if (ret == null)
            {
                return null;
            }
            return new GMIMBroadCastRet() { conversationShortId = ret.ConversationShortId, conversationType = (GMIMConversationType) ret.ConversationType, counter = ret.Counter };
        }

        public static IMBroadcastRet Convert(GMIMBroadCastRet ret)
        {
            if (ret == null)
            {
                return null;
            }
            return new IMBroadcastRet() { ConversationShortId = ret.conversationShortId, ConversationType = (IMConversationType)ret.conversationType, Counter = ret.counter };
        }
        
        public static ReceiveBroadcastMessageInfo Convert(ReceiveBroadCastMessageResult ret)
        {
            if (ret == null)
            {
                return null;
            }

            return new ReceiveBroadcastMessageInfo() { ConversationId = ret.conversationId, NextCursor = ret.nextCursor, MessagesList = new List<IMMessage>(Convert(ret.messagesArray)) };
        }
        
        public static DeleteBroadcastMessageInfo Convert(DeleteBroadCastMessageResult ret)
        {
            if (ret == null)
            {
                return null;
            }

            return new DeleteBroadcastMessageInfo() { ConversationId = ret.conversationId, MsgServerId = ret.msgServerId };
        }
        
        public static FriendEventInfo Convert(FriendEventResult ret)
        {
            if (ret == null)
            {
                return null;
            }

            return new FriendEventInfo() { Inbox = ret.inbox, Uid = ret.uid, Ext = ret.ext };
        }
    }
}
