﻿using System;
using System.Collections.Generic;
using UnityEngine;
using GMSDK;
using UNBridgeLib;

namespace GSDK
{
    internal class IMService : IIMService
    {
        public event IMLoginEventHandler LoginEvent;
        public event IMTokenExpiredEventHandler TokenExpiredEvent;
        public event IMConversationUpdatedEventHandler ConversationUpdatedEvent;
        public event IMConversationParticipantsUpdatedEventHandler ConversationParticipantsUpdatedEvent;
        public event IMMessageUpdatedEventHandler MessageUpdatedEvent;
        public event IMMessageListUpdatedEventHandler MessageListUpdatedEvent;
        public event IMConversationDataSourceDidUpdateEventHandler ConversationDataSourceDidUpdateEvent;
        public event SendMessageEventHandler SendMessageEvent;
        public event IMInitMessageEndEventHandler InitMessageEndEvent;
        public event IMInboxInitMessageEndEventHandler InboxInitMessageEndEvent;
        public event IMReceiveBroadcastMessageEventHandler ReceiveBroadcastMessageEvent;
        public event IMDeleteBroadcastMessageEventHandler DeleteBroadcastMessageEvent;
        public event IMReceiveFriendApplyEventHandler ReceiveFriendApplyEvent;
        public event IMDeleteFriendEventHandler DeleteFriendEvent;
        public event IMAddFriendEventHandler AddFriendEvent;

        BaseIMSDK imsdk = null;
        public IMService()
        {
            BindListeners();
        }

        private void BindListeners()
        {
            if(imsdk == null)
            {
                imsdk = new BaseIMSDK();
            }
            imsdk.ListenLoginIMEvent((IMCallbackResult result) =>
            {
                GLog.LogInfo("[IM]On login callback:" + result.code + ", "+ result.message + " and LoginEvent is null?" +(LoginEvent == null));
                safeInvoke(() =>
                {
                    if(LoginEvent != null)
                    { 
                        LoginEvent.Invoke(IMConvertTool.Convert(result));
                    }
                });
            });

            imsdk.ListenInitMessageEndEvent((GMSDK.InitMessageEndResult result) =>
            {
                safeInvoke(() =>
                {
                    InitMessageEndInfo info = null;
                    if (result != null) {
                        info = new InitMessageEndInfo();
                    }
                    if (InitMessageEndEvent != null)
                    {
                        InitMessageEndEvent.Invoke(info);
                    }
                });
            });

            imsdk.ListenInboxInitMessageEndEvent((GMSDK.InboxInitMessageEndResult result) =>
            {
                safeInvoke(() =>
                {
                    InboxInitMessageEndInfo info = null;
                    if (result != null) {
                        info = new InboxInitMessageEndInfo();
                        info.Inbox = result.inbox;
                    }
                    if (InboxInitMessageEndEvent != null)
                    {
                        InboxInitMessageEndEvent.Invoke(info);
                    }
                });
            });

            imsdk.ListenIMTokenExpiredEvent((GMSDK.IMTokenExpiredResult result) =>
            {
                safeInvoke(() =>
                {
                    if (TokenExpiredEvent != null)
                    {
                        TokenExpiredEvent.Invoke();
                    }
                });
            });


            imsdk.ListenConversationParticipantsUpdatedEvent((ConversationIdResult result) =>
            {
                safeInvoke(() =>
                {
                    ConversationIdInfo info = null;
                    if (result != null)
                    {
                        info = new ConversationIdInfo()
                        {
                            ConversationId = result.conversationId
                        };
                    }
                    if(ConversationParticipantsUpdatedEvent != null)
                    { 
                        ConversationParticipantsUpdatedEvent.Invoke(info);
                    }
                });
            });

            imsdk.ListenConversationUpdatedEvent((ConversationIdResult result) =>
            {
                safeInvoke(() =>
                {
                    ConversationIdInfo info = null;
                    if (result != null)
                    {
                        info = new ConversationIdInfo()
                        {
                            ConversationId = result.conversationId
                        };
                    }
                    if(ConversationUpdatedEvent !=null)
                    { 
                        ConversationUpdatedEvent.Invoke(info);
                    }
                });
            });

            imsdk.ListenConversationDataSourceDidUpdateEvent((GMSDK.ConversationDataSourceUpdateResult obj) => {
                safeInvoke(() =>
                {
                    safeInvoke(() =>
                    {
                        ConversationDataSourceUpdateInfo info = null;
                        if (obj != null)
                        {
                            info = new ConversationDataSourceUpdateInfo()
                            {
                                AfterUpdateConversationIds = obj.afterUpdateConversationIds,
                                BeforeUpdateConversationIds = obj.beforeUpdateConversationIds,
                                ConversationDataSourceID = obj.conversationDataSourceID
                            };

                        }
                        if(ConversationDataSourceDidUpdateEvent != null)
                        { 
                            ConversationDataSourceDidUpdateEvent.Invoke(info);
                        }
                    });
                });
            });


            imsdk.ListenMessageListUpdatedEvent((MessageListUpdatedResult result) =>
            {
                safeInvoke(() =>
                {
                    if (result != null)
                    {
                        MessageListUpdatedInfo info = new MessageListUpdatedInfo()
                        {
                            ConversationId = result.conversationId,
                            Reason = result.reason,
                            InsertMessageIdList = result.insertMessageIds,
                            DeleteMessageIdList = result.deleteMessageIds
                        };
                        if (MessageListUpdatedEvent != null)
                        {
                            MessageListUpdatedEvent.Invoke(info);
                        }
                    }
                });
            });

            imsdk.ListenMessageUpdatedEvent((MessageUpdatedResult result) =>
            {
                safeInvoke(() =>
                {
                    if (result != null)
                    {
                        MessageUpdatedInfo info = new MessageUpdatedInfo()
                        {
                            ConversationId = result.conversationId,
                            MessageId = result.messageId
                        };
                        if(MessageUpdatedEvent != null)
                        MessageUpdatedEvent.Invoke(info);
                    }
                });
            });

            imsdk.ListenSendMessageEvent((SendMessageResult result) =>
            {
                safeInvoke(() =>
                {
                    if (result != null)
                    {
                        if(SendMessageEvent != null)
                        SendMessageEvent.Invoke(IMConvertTool.Convert(result.ret), result.messageId);
                    }
                });
            });
            
            imsdk.ListenReceiveBroadCastMessageEvent((ReceiveBroadCastMessageResult result) =>
            {
                safeInvoke(() =>
                {
                    if (result != null)
                    {
                        if (ReceiveBroadcastMessageEvent != null)
                        ReceiveBroadcastMessageEvent.Invoke(IMConvertTool.Convert(result));    
                    }
                });
            });
            
            imsdk.ListenDeleteBroadCastMessageEvent((DeleteBroadCastMessageResult result) =>
            {
                safeInvoke(() =>
                {
                    if (result != null)
                    {
                        if (DeleteBroadcastMessageEvent != null)
                        {
                            DeleteBroadcastMessageEvent.Invoke(IMConvertTool.Convert(result)); 
                        }
                    }
                });
            });
            imsdk.ListenReceiveFriendApplyEvent((FriendEventResult result) =>
            {
                safeInvoke(() =>
                {
                    if (result != null)
                    {
                        if (ReceiveFriendApplyEvent != null)
                        {
                            ReceiveFriendApplyEvent.Invoke(IMConvertTool.Convert(result));
                        }
                    }
                });
            });
            imsdk.ListenDeleteFriendEvent((FriendEventResult result) =>
            {
                safeInvoke(() =>
                {
                    if (result != null)
                    {
                        if (DeleteFriendEvent != null)
                        {
                            DeleteFriendEvent.Invoke(IMConvertTool.Convert(result));
                        }
                    }
                });
            });
            imsdk.ListenAddFriendEvent((FriendEventResult result) =>
            {
                safeInvoke(() =>
                {
                    if (result != null)
                    {
                        if (AddFriendEvent != null)
                        {
                            AddFriendEvent.Invoke(IMConvertTool.Convert(result));
                        }
                    }
                });
            });
        }

        void OnCallback(IMCallbackResult result, IMOperationDelegate callback)
        {
            try
            {
                if(callback != null)
                { 
                    callback.Invoke(IMConvertTool.Convert(result));
                }
            }
            catch (Exception e)
            {
                GMExeption ex = new GMExeption
                {
                    exception = e
                };
                if (GMSDKMgr.instance.exceptionCallback != null)
                {
                    GMSDKMgr.instance.exceptionCallback.onGMExceptionCallback(ex);
                }
            }
        }

        void OnOperationCallback(ConversationOperationResult result, IMOperationDelegate callback)
        {
            try
            {
                if (callback != null)
                {
                    callback.Invoke(IMConvertTool.Convert(result));
                }
            }
            catch (Exception e)
            {
                GMExeption ex = new GMExeption
                {
                    exception = e
                };
                if (GMSDKMgr.instance.exceptionCallback != null)
                {
                    GMSDKMgr.instance.exceptionCallback.onGMExceptionCallback(ex);
                }
            }
        }

        public void JoinConversation(int inbox, string conversationId, Dictionary<string, string> bizExtension, IMOperationDelegate callback)
        {
            imsdk.joinGroup(inbox, conversationId, bizExtension, (ConversationOperationResult result) => { OnOperationCallback(result, callback); });
        }
        
        public void AddParticipants(string conversationId, List<long> participants, Dictionary<string, string> bizExtension, AddParticipantsDelegate callback)
        {
            imsdk.addParticipants(conversationId, participants, bizExtension, (GMSDK.AddParticipantsResult result) =>
            {
                safeInvoke(() =>
                {
                    if(callback != null)
                    { 
                        callback.Invoke(IMConvertTool.Convert(result), result.addedParticipants);
                    }
                });
            }
            );
        }

        public void BroadcastUserCounter(List<string> conversationsIDArray, int inbox, BroadcastUserCounterDelegate callback)
        {
            if (conversationsIDArray == null)
            {
                conversationsIDArray = new List<string>();
            }

            imsdk.broadCastUserCounter(conversationsIDArray, inbox, (BroadCastUserCounterResult result)=>
                {
                    safeInvoke(() =>
                    {
                        List<IMBroadcastRet> list = new List<IMBroadcastRet>();
                        if (result.infosArray != null)
                        {
                            for (int i = 0; i < result.infosArray.Count; i++)
                            {
                                list.Add(IMConvertTool.Convert(result.infosArray[i]));
                            }
                        }
                        if (callback != null)
                        {
                            callback.Invoke(IMConvertTool.Convert(result), list);
                        }
                    });
                }
            );
        }

        public void Config(IMConfig config)
        {
            GLog.LogInfo("IMService Config");
            imsdk.configIM(IMConvertTool.Convert(config));
        }

        public IMSendMessageInfo ConstructImageMessage(string imagePath, int imageWidth, int imageHeight, string mime, string format, int thumbWidth, int thumbHeight, int previewWidth, int previewHeight)
        {
            return IMConvertTool.Convert(imsdk.constructImageMessage(imagePath, imageWidth, imageHeight, mime, format, thumbWidth, thumbHeight, previewWidth, previewHeight));
        }

        public IMSendMessageInfo ConstructTextMessage(string text)
        {
            return IMConvertTool.Convert(imsdk.constructTextMessage(text));
        }

        public IMSendMessageInfo constructVoiceMessageWithLocalPath(string localPath, long duration)
        {
            return IMConvertTool.Convert(imsdk.constructVoiceMessageWithLocalPath(localPath, duration));
        }

        public IMSendMessageInfo ConstructVoiceMessage(string voiceId)
        {
            return IMConvertTool.Convert(imsdk.constructVoiceMessage(voiceId));
        }

        public IMSendMessageInfo ConstructEmoticonMessage(Dictionary<string, object> content)
        {
            return IMConvertTool.Convert(imsdk.constructEmoticonMessage(content));
        }

        public void CreateConversationWithOtherParticipants(List<long> otherParticipants, IMConversationType type, int inbox, string idempotentID, IMCreateConversationDelegate callback)
        {
            imsdk.createConversationWithOtherParticipants(otherParticipants, (int)type, inbox, idempotentID, (GMSDK.CreateConversationWithOtherParticipantsResult result)=>
            {
                try
                {
                    if (callback != null)
                    {
                        callback.Invoke(IMConvertTool.Convert(result), result != null ? result.conversationIdentifier:null);
                    }
                }
                catch(Exception e)
                {
                    GMExeption ex = new GMExeption
                    {
                        exception = e
                    };
                    if (GMSDKMgr.instance.exceptionCallback != null)
                    {
                        GMSDKMgr.instance.exceptionCallback.onGMExceptionCallback(ex);
                    }
                }
            });
        }

        public void DeleteAllConversationMessages(string conversationId, IMOperationDelegate callback)
        {
            imsdk.deleteAllMessages(conversationId, (IMCallbackResult result) =>
            {
                OnCallback(result, callback);
            });
        }

        public void DeleteConversation(string conversationId, IMConversationDeleteMode mode, IMOperationDelegate callback)
        {
            imsdk.deleteWithMode(conversationId, (GMIMConversationDeleteMode)mode, (IMCallbackResult result) =>
            {
                OnCallback(result, callback);
            });
        }

        public void DeleteMessage(string conversationId, string messageId, IMOperationDelegate callback)
        {
            imsdk.deleteMessageID(conversationId, messageId, (IMCallbackResult result) =>
            {
                OnCallback(result, callback);
            });
        }

        public void DismissConversation(string conversationId, IMOperationDelegate callback)
        {
            imsdk.dismissConversation(conversationId, (ConversationOperationResult result)=>
            {
                OnOperationCallback(result, callback);
            });
        }

        public void EnterConversation(string conversationId)
        {
            imsdk.userWillEnterCurrentConversation(conversationId);
        }

        public void EnterConversation(string conversationId, int mode, int offset)
        {
            imsdk.userWillEnterCurrentConversation(conversationId, mode, offset);
        }

        public void ExitConversation(string conversationId)
        {
            imsdk.userWillExitCurrentConversation(conversationId);
        }

        public void FetchAllParticipants(string conversationId, FetchAllParticipantsDelegate callback)
        {
            imsdk.fetchConversationAllParticipants(conversationId, (fetchConversationAllParticipantsResult result)=>
            {
                safeInvoke(() =>
                {
                    List<IMParticipant> parts = new List<IMParticipant>();
                    if(result.participants != null)
                    {
                        for (int i = 0; i < result.participants.Count; i++)
                        {
                            parts.Add(IMConvertTool.Convert(result.participants[i]));
                        }
                    }
                    if (callback != null)
                    {
                        callback.Invoke(IMConvertTool.Convert(result), parts);
                    }
                });
            });
        }


        public void FetchBlockListUsers(int inbox, long cursor, int limit, FetchblockListUsersDelegate callback)
        {
            imsdk.fetchBlockListUsersInInbox(inbox, cursor, limit, false, "", 0, 1, (FetchblockListUsersInInboxResult result)=>
            {
                try
                {
                    FetchblockListUsersInfo info = new FetchblockListUsersInfo();
                    info.HasMore = result.hasMore;
                    info.NextCursor = result.nextCursor;
                    if(result != null && result.blockListUsers != null)
                    {
                        info.BlockListUsers = new List<IMBlackListUser>();
                        for (int i = 0; i < result.blockListUsers.Count; i++)
                        {
                            info.BlockListUsers.Add(IMConvertTool.Convert(result.blockListUsers[i]));
                        }

                    }

                    if (callback != null)
                    {
                        callback.Invoke(IMConvertTool.Convert(result), info);
                    }
                }
                catch (Exception e)
                {
                    GMExeption ex = new GMExeption
                    {
                        exception = e
                    };
                    if (GMSDKMgr.instance.exceptionCallback != null)
                    {
                        GMSDKMgr.instance.exceptionCallback.onGMExceptionCallback(ex);
                    }
                }
            });

        }

        public void FetchBlockListUsers(int inbox, long cursor, int limit, bool blockType, string convId, long shortId, IMConversationType convType, FetchblockListUsersDelegate callback)
        {
            imsdk.fetchBlockListUsersInInbox(inbox, cursor, limit, blockType, convId, shortId, (int) convType, (FetchblockListUsersInInboxResult result)=>
            {
                try
                {
                    FetchblockListUsersInfo info = new FetchblockListUsersInfo();
                    info.HasMore = result.hasMore;
                    info.NextCursor = result.nextCursor;
                    if(result != null && result.blockListUsers != null)
                    {
                        info.BlockListUsers = new List<IMBlackListUser>();
                        for (int i = 0; i < result.blockListUsers.Count; i++)
                        {
                            info.BlockListUsers.Add(IMConvertTool.Convert(result.blockListUsers[i]));
                        }

                    }

                    if (callback != null)
                    {
                        callback.Invoke(IMConvertTool.Convert(result), info);
                    }
                }
                catch (Exception e)
                {
                    GMExeption ex = new GMExeption
                    {
                        exception = e
                    };
                    if (GMSDKMgr.instance.exceptionCallback != null)
                    {
                        GMSDKMgr.instance.exceptionCallback.onGMExceptionCallback(ex);
                    }
                }
            });

        }
        
        public void FetchUserBlockStatus(int inbox, long userID, FetchUserBlockStatusDelegate callback)
        {
            imsdk.fetchUserBlockStatusInInbox(inbox, userID, false, "", 0, 1, (FetchUserblockStatusInInboxResult result) =>
            {
                try
                {

                    if (callback != null)
                    {
                        callback.Invoke(IMConvertTool.Convert(result), result.isInBlockList);
                    }
                }
                catch (Exception e)
                {
                    GMExeption ex = new GMExeption
                    {
                        exception = e
                    };
                    if (GMSDKMgr.instance.exceptionCallback != null)
                    {
                        GMSDKMgr.instance.exceptionCallback.onGMExceptionCallback(ex);
                    }
                }
            });
        }

        public void FetchUserBlockStatus(int inbox, long userID, bool blockType, string convId, long shortId, IMConversationType convType, FetchUserBlockStatusDelegate callback)
        {
            imsdk.fetchUserBlockStatusInInbox(inbox, userID, blockType, convId, shortId, (int) convType, (FetchUserblockStatusInInboxResult result) =>
            {
                try
                {

                    if (callback != null)
                    {
                        callback.Invoke(IMConvertTool.Convert(result), result.isInBlockList);
                    }
                }
                catch (Exception e)
                {
                    GMExeption ex = new GMExeption
                    {
                        exception = e
                    };
                    if (GMSDKMgr.instance.exceptionCallback != null)
                    {
                        GMSDKMgr.instance.exceptionCallback.onGMExceptionCallback(ex);
                    }
                }
            });
        }

        public void FetchUserInfo(int inbox, long userID, FetchUserInfoDelegate callback)
        {
            imsdk.fetchUserInfoInInbox(inbox, userID, (FetchUserInfoInInboxResult result) =>
            {
                try
                {
                    if (callback != null)
                    {
                        callback.Invoke(IMConvertTool.Convert(result), IMConvertTool.Convert(result.profile));
                    }
                }
                catch (Exception e)
                {
                    GMExeption ex = new GMExeption
                    {
                        exception = e
                    };
                    if (GMSDKMgr.instance.exceptionCallback != null)
                    {
                        GMSDKMgr.instance.exceptionCallback.onGMExceptionCallback(ex);
                    }
                }
            });
        }

        public IMConversation GetConversationAtIndex(string conversationDataSourceID, int index)
        {
            return IMConvertTool.Convert(imsdk.conversationAtIndex(conversationDataSourceID, index));
        }

        public IMConversation GetConversationWithID(string conversationId)
        {
            return IMConvertTool.Convert(imsdk.getConversationWithID(conversationId));
        }

        public void GetConversationAsync(string conversationId, IMConversationType conversationType, int inbox, IMGetConversationAsyncDelegate callback)
        {
            imsdk.getConversationAsync(conversationId, (int)conversationType, inbox, (ConversationResult result) =>
            {
                safeInvoke(() =>
                {
                    if (callback != null)
                    {
                        if (result.conversation.conversationID != null)
                        {
                            callback.Invoke(IMConvertTool.Convert(result.conversation));
                        }
                        else {
                            callback.Invoke(null);
                        }
                    }
                });
            });
        }

        public IMImage GetImage(IMMessage message)
        {
            return IMConvertTool.Convert(imsdk.getImage(IMConvertTool.Convert(message)));
        }

        public IMMessage GetMessage(string conversationId, int index)
        {
            return IMConvertTool.Convert(imsdk.messageAtIndex(conversationId, index));
        }

        public IMMessage GetMessage(string conversationId, string messageId)
        {
            return IMConvertTool.Convert( imsdk.getMessageWithID(conversationId, messageId));
        }

        public int GetNumberOfConversations(string conversationDataSourceID)
        {
            return imsdk.numberOfConversations(conversationDataSourceID);
        }

        public int GetNumberOfMessages(string conversationId)
        {
            return imsdk.numberOfMessages(conversationId);
        }

        public string GetTextContent(IMMessage message)
        {
            return imsdk.getTextContent(IMConvertTool.Convert(message));
        }

        public string GetVoiceId(IMMessage message)
        {
            return imsdk.getVoiceId(IMConvertTool.Convert(message));
        }
        
        public long GetVoiceDuration(IMMessage message)
        {
            return imsdk.getVoiceDuration(IMConvertTool.Convert(message));
        }

        public string InitConversationDataSourceWithInboxes(List<int> inboxes)
        {
            return imsdk.initConversationDataSourceWithInboxes(inboxes);
        }

        void safeInvoke(Action action)
        {
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                GMExeption ex = new GMExeption
                {
                    exception = e
                };
                if (GMSDKMgr.instance.exceptionCallback != null)
                {
                    GMSDKMgr.instance.exceptionCallback.onGMExceptionCallback(ex);
                }
            }
        }

        public void LoadNewBroadcastMessage(string conversationId, int inbox, long cursor, long limit, LoadNewBroadcastMessageDelegate callback)
        {
            imsdk.loadNewBroadCastMessage(conversationId, inbox, cursor, limit, (LoadNewBroadCastMessageResult result) =>
            {
                safeInvoke(() =>
                {
                    LoadNewBroadcastMessageInfo info = new LoadNewBroadcastMessageInfo
                    {
                        NextCursor = (long)(result != null?result.nextCursor : 0)
                    };

                    if (result != null && result.messagesArray != null)
                    {
                        info.MessagesList = new List<IMMessage>();
                        for (int i = 0; i < result.messagesArray.Count; i++)
                        {
                            info.MessagesList.Add(IMConvertTool.Convert(result.messagesArray[i]));
                        }
                    }
                    if (callback != null)
                    {
                        callback.Invoke(IMConvertTool.Convert(result), info);
                    }
                });
            });
        }
        
        public void LoadOldBroadcastMessage(string conversationId, int inbox, long cursor, long limit, LoadNewBroadcastMessageDelegate callback)
        {
            imsdk.loadOldBroadCastMessage(conversationId, inbox, cursor, limit, (LoadNewBroadCastMessageResult result) =>
            {
                safeInvoke(() =>
                {
                    LoadNewBroadcastMessageInfo info = new LoadNewBroadcastMessageInfo
                    {
                        NextCursor = (long)(result != null?result.nextCursor : 0)
                    };

                    if (result != null && result.messagesArray != null)
                    {
                        info.MessagesList = new List<IMMessage>();
                        for (int i = 0; i < result.messagesArray.Count; i++)
                        {
                            info.MessagesList.Add(IMConvertTool.Convert(result.messagesArray[i]));
                        }
                    }
                    if (callback != null)
                    {
                        callback.Invoke(IMConvertTool.Convert(result), info);
                    }
                });
            });
        }

        public void LoadNewerMessages(string conversationId)
        {
            // Todo
            imsdk.loadNewerMessages(conversationId, null);
        }

        public void LoadOlderMessages(string conversationId)
        {
            // Todo
            imsdk.loadOlderMessages(conversationId, null);
        }
        
#if UNITY_ANDROID
        public void SetAndroidLogOpen(bool open)
        {
            imsdk.setLogOpen(open);
        }
#endif

        public void SetAutoPullMessageIntervalSeconds(double seconds)
        {
            imsdk.setAutoPullMessageIntervalSeconds(seconds);
        }
		
        public void PullNewMessage(int inbox)
        {
            imsdk.pullNewMessage(inbox);
        }

#if UNITY_ANDROID
        public void ActivateLongConnection()
        {
            imsdk.activateLongConnection();
        }
		
        public void DeactivateLongConnection()
        {
            imsdk.breakLongConnection();
        }

        public bool IsLongConnectionActivated()
        {
            return imsdk.isLongConnectionKeepLive();
        }
#endif
        
        public void Login(long userID, string token)
        {

            GLog.LogInfo("IMService Login userID:" + userID + " token is empty?" + (string.IsNullOrEmpty(token)));
            imsdk.loginIM(userID, token);
        }

        public void Login(long userID)
        {

            GLog.LogInfo("IMService Login userID:" + userID);
            imsdk.loginIM(userID);
        }
        
        public void Logout()
        {
            imsdk.logoutIM();
        }

        public string GetCurrentUserId()
        {
            return imsdk.getCurrentUserId();
        }
        
        public void MarkAllMessagesAsRead(string conversationId, IMOperationDelegate callback)
        {
            imsdk.markAllMessagesAsRead(conversationId, (IMCallbackResult result) =>
            {
                OnCallback(result, callback);
            });
        }

        public void MarkAllMessagesAsReadBeforeMessage(string conversationId, string messageId, IMOperationDelegate callback)
        {
            imsdk.markAllMessagesAsReadBeforeMessage(conversationId, messageId, (IMCallbackResult result) =>
            {
                OnCallback(result, callback);
            });
        }

        public void ModifyUsersBlockList(int inbox, List<long> arrUserID, bool toBlockList, ModifyUsersBlockListDelegate callback)
        {
            imsdk.modifyUsersBlockList(inbox, arrUserID, toBlockList, false, "", 0, 1, (ModifyUsersblockListResult result)=>
            {
                safeInvoke(() =>
                {
                    if (callback != null)
                    {
                        callback.Invoke(IMConvertTool.Convert(result), result.modifiedUsers);
                    }
                });
            });
        }

        public void ModifyUsersBlockList(int inbox, List<long> arrUserID, bool toBlockList, bool blockType, string convId, long shortId, IMConversationType convType, ModifyUsersBlockListDelegate callback)
        {
            imsdk.modifyUsersBlockList(inbox, arrUserID, toBlockList, blockType, convId, shortId, (int) convType, (ModifyUsersblockListResult result)=>
            {
                safeInvoke(() =>
                {
                    if (callback != null)
                    {
                        callback.Invoke(IMConvertTool.Convert(result), result.modifiedUsers);
                    }
                });
            });
        }


        public void QuitConversation(string conversationId, IMOperationDelegate callback)
        {
            imsdk.leaveConversation(conversationId, (ConversationOperationResult result) =>
            {
                OnOperationCallback(result, callback);
            });
        }

        public void RecallMessage(string conversationId, string messageId, IMOperationDelegate callback)
        {
            imsdk.recallMessage(conversationId, messageId, (IMCallbackResult result) =>
            {
                OnCallback(result, callback);
            });
        }

        public void RefreshToken(string token)
        {
            imsdk.refreshIMToken(token);
        }

        public void RemoveParticipants(string conversationId, List<long> participants, Dictionary<string, string> bizExtension, RemovedParticipantsDelegate callback)
        {
            imsdk.removeParticipants(conversationId, participants, bizExtension, (GMSDK.RemovedParticipantsResult result) =>
            {
                safeInvoke(() =>
                {
                    if (callback != null)
                    {
                        callback.Invoke(IMConvertTool.Convert(result), result.removedParticipants);
                    }
                });
            }
            );
        }

        public void ResendMessage(string conversationId, string messageId, ResendMessageDelegate callback)
        {
            imsdk.resendMessage(conversationId, messageId, (GMSDK.ResendMessageResult result) =>
            {
                safeInvoke(() =>
                {
                    if (callback != null)
                    {
                        callback.Invoke(IMConvertTool.Convert(result != null ? result.ret : null));
                    }
                });
            }
            );
        }

        public void SendBroadcastMessage(IMSendMessageInfo message, int inbox, string conversationId, SendBroadcastMessageDelegate callback)
        {
            imsdk.broadCastSendMessage(IMConvertTool.Convert(message), inbox, conversationId, (BroadCastSendMessageResult result) =>
            {
                safeInvoke(() =>
                {
                    if (callback != null)
                    {
                        callback.Invoke(IMConvertTool.Convert(result.ret));
                    }
                });
            }
            );
        }

        public string SendLocalMessage(string conversationId, IMSendMessageInfo message)
        {
            GLog.LogInfo("[IM] SendLocalMessage conversationId is " + conversationId);
            GMIMSendMessage sendMsg = imsdk.skipRealSendAndMarkAsSent(IMConvertTool.Convert(message));

            return imsdk.sendMessage(conversationId, sendMsg);
        }

        public string SendMessage(string conversationId, IMSendMessageInfo message)
        {
            GLog.LogInfo("[IM] SendMessage conversationId is " + conversationId);
            return imsdk.sendMessage(conversationId, IMConvertTool.Convert(message));
        }

        public void SendMessageAsync(string conversationId, IMConversationType conversationType, int inbox, IMSendMessageInfo message, SendMessageAsyncDelegate callback)
        {
            GLog.LogInfo("[IM] SendMessageAsync conversationId is " + conversationId);
            imsdk.sendMessageAsync(conversationId, (int) conversationType, inbox, IMConvertTool.Convert(message), (SendMessageAsyncIDResult result) => {
                safeInvoke(() =>
                {
                    if (result.messageId != null && callback != null)
                    {
                       callback.Invoke(result.messageId);
                    }
                });
            });
        }

        public void SetAliasForParticipant(string conversationId, long participant, string alias, IMOperationDelegate callback)
        {
            imsdk.setAliasForParticipant(conversationId, participant, alias, (ConversationOperationResult result) =>
            {
                OnOperationCallback(result, callback);
            });
        }

        public void SetConversationName(string conversationId, string name, Dictionary<string, string> ext, IMOperationDelegate callback)
        {
            imsdk.setName(conversationId, name, ext, (ConversationOperationResult result) =>
            {
                OnOperationCallback(result, callback);
            });
        }

		public void SetConversationDesc(string conversationId, string desc, Dictionary<string, string> ext, IMOperationDelegate callback)
		{
			imsdk.setDesc(conversationId, desc, ext, (ConversationOperationResult result) =>
				{
					OnOperationCallback(result, callback);
				});
		}

		public void SetConversationIcon(string conversationId, string icon, Dictionary<string, string> ext, IMOperationDelegate callback)
		{
			imsdk.setIcon(conversationId, icon, ext, (ConversationOperationResult result) =>
				{
					OnOperationCallback(result, callback);
				});
		}

		public void SetConversationNotice(string conversationId, string notice, Dictionary<string, string> ext, IMOperationDelegate callback)
		{
			imsdk.setNotice(conversationId, notice, ext, (ConversationOperationResult result) =>
				{
					OnOperationCallback(result, callback);
				});
		}

        public void SetConversationCoreExt(string conversationId, Dictionary<string, string> ext, IMOperationDelegate callback)
        {
            imsdk.setConversationCoreExt(conversationId, ext, (ConversationOperationResult result) =>
            {
                OnOperationCallback(result, callback);
            });
        }

        public void SetConversationSettingExt(string conversationId, Dictionary<string, string> ext, IMOperationDelegate callback)
        {
            imsdk.setConversationSettingExt(conversationId, ext, (ConversationOperationResult result) =>
            {
                OnOperationCallback(result, callback);
            });
        }

        public void SetConversationLocalExt(string conversationId, Dictionary<string, string> ext, IMOperationDelegate callback)
        {
            imsdk.setConversationLocalExt(conversationId, ext, (ConversationOperationResult result) =>
            {
                OnOperationCallback(result, callback);
            });
        }

        public void SetDraft(string conversationId, string draft)
        {
            imsdk.setDraft(conversationId, draft);
        }

        public void SetMute(string conversationId, bool shouldMute, IMOperationDelegate callback)
        {
            imsdk.setMute(conversationId, shouldMute, (ConversationOperationResult result) =>
            {
                OnOperationCallback(result, callback);
            });
        }

        public void SetRoleForParticipant(string conversationId, long participant, IMConversationParticipantRole role, IMOperationDelegate callback)
        {
            imsdk.setRoleForParticipant(conversationId, participant, (GMIMConversationParticipantRole)role, (ConversationOperationResult result) =>
            {
                OnOperationCallback(result, callback);
            });
        }

        public void UpdateConversation(string conversationId, IMOperationDelegate callback)
        {
            imsdk.updateCurrentIfNeeded(conversationId, (IMCallbackResult result) =>
            {
                OnCallback(result, callback);
            });
        }

		public bool HasOlderMessages(string conversationId)
		{
			return imsdk.hasOlderMessages(conversationId);
		}

        public void RegisterBroadCastListener(string conversationId)
        {
            imsdk.registerBroadCastListener(conversationId);
        }

        public void UnregisterBroadCastListener(string conversationId)
        {
            imsdk.unregisterBroadCastListener(conversationId);
        }

        public void SetBroadCastThrottleDelay(long delay)
        {
            imsdk.setBroadCastThrottleDelay(delay);
        }

        public void EnterBroadCastConversation(int inbox, string conversationId)
        {
            imsdk.enterBroadCastConversation(inbox, conversationId);
        }

        public void ExitBroadCastConversation(string conversationId)
        {
            imsdk.exitBroadCastConversation(conversationId);
        }

        public void ResumeBroadCastConversation(string conversationId, bool fromLast)
        {
            imsdk.resumeBroadCastConversation(conversationId, fromLast);
        }

        public void PauseBroadCastConversation(string conversationId)
        {
            imsdk.pauseBroadCastConversation(conversationId);
        }

        public void QueryUserInfo(int inbox, long uid, bool fromSource, QueryUserInfoDelegate callback)
        {
            imsdk.queryUserInfo(inbox, uid, fromSource, (QueryUserInfoResult result) =>
            {
                try
                {
                    if (callback != null)
                    {
                        callback.Invoke(IMConvertTool.Convert(result), IMConvertTool.Convert(result.userInfo));
                    }
                }
                catch (Exception e)
                {
                    GMExeption ex = new GMExeption
                    {
                        exception = e
                    };
                    if (GMSDKMgr.instance.exceptionCallback != null)
                    {
                        GMSDKMgr.instance.exceptionCallback.onGMExceptionCallback(ex);
                    }
                }
            });
        }

        public void BatchQueryUserInfo(int inbox, List<long> uidList, BatchQueryUserInfoDelegate callback)
        {
            imsdk.batchQueryUserInfo(inbox, uidList, (BatchQueryUserInfoResult result) =>
            {
                try
                {
                    if (callback != null)
                    {
                        List<IMUserProfile> userProfiles = new List<IMUserProfile>();
                        if (result.userInfoList != null)
                        {
                            for (int i = 0; i < result.userInfoList.Count; i++)
                            {
                                userProfiles.Add(IMConvertTool.Convert(result.userInfoList[i]));
                            }
                        }

                        callback.Invoke(IMConvertTool.Convert(result), userProfiles);
                    }
                }
                catch (Exception e)
                {
                    GMExeption ex = new GMExeption
                    {
                        exception = e
                    };
                    if (GMSDKMgr.instance.exceptionCallback != null)
                    {
                        GMSDKMgr.instance.exceptionCallback.onGMExceptionCallback(ex);
                    }
                }
            });
        }

        public void SearchUser(int inbox, string key, SearchUserDelegate callback)
        {
            imsdk.searchUser(inbox, key, (BatchQueryUserInfoResult result) =>
            {
                try
                {
                    if (callback != null)
                    {
                        List<IMUserProfile> userProfiles = new List<IMUserProfile>();
                        if (result.userInfoList != null)
                        {
                            for (int i = 0; i < result.userInfoList.Count; i++)
                            {
                                userProfiles.Add(IMConvertTool.Convert(result.userInfoList[i]));
                            }
                        }

                        callback.Invoke(IMConvertTool.Convert(result), userProfiles);
                    }
                }
                catch (Exception e)
                {
                    GMExeption ex = new GMExeption
                    {
                        exception = e
                    };
                    if (GMSDKMgr.instance.exceptionCallback != null)
                    {
                        GMSDKMgr.instance.exceptionCallback.onGMExceptionCallback(ex);
                    }
                }
            });
        }

        public void DeleteFriends(int inbox, List<long> arrUserID, DeleteFriendsDelegate callback)
        {
            imsdk.deleteFriends(inbox, arrUserID, (DeleteFriendsResult result) =>
            {
                try
                {
                    if (callback != null)
                    {
                        callback.Invoke(IMConvertTool.Convert(result), result.uidList);
                    }
                }
                catch (Exception e)
                {
                    GMExeption ex = new GMExeption
                    {
                        exception = e
                    };
                    if (GMSDKMgr.instance.exceptionCallback != null)
                    {
                        GMSDKMgr.instance.exceptionCallback.onGMExceptionCallback(ex);
                    }
                }
            });
        }

        public void GetFriendList(int inbox, long cursor, long limit, bool getTotalCount, GetFriendListDelegate callback)
        {
            imsdk.getFriendList(inbox, cursor, limit, getTotalCount, (FriendListResult result) =>
            {
                try
                {
                    if (callback != null)
                    {
                        IMFriendInfoList friendInfoList = new IMFriendInfoList();
                        friendInfoList.NextCursor = result.nextCursor;
                        friendInfoList.HasMore = result.hasMore;
                        friendInfoList.TotalCount = result.totalCount;
                        List<IMFriendInfo> friendInfos = new List<IMFriendInfo>();
                        if (result.friendList != null)
                        {
                            for (int i = 0; i < result.friendList.Count; i++)
                            {
                                friendInfos.Add(IMConvertTool.Convert(result.friendList[i]));
                            }
                        }
                        friendInfoList.FriendList = friendInfos;
                        callback.Invoke(IMConvertTool.Convert(result), friendInfoList);
                    }
                }
                catch (Exception e)
                {
                    GMExeption ex = new GMExeption
                    {
                        exception = e
                    };
                    if (GMSDKMgr.instance.exceptionCallback != null)
                    {
                        GMSDKMgr.instance.exceptionCallback.onGMExceptionCallback(ex);
                    }
                }
            });
        }

        public void GetSentApplyList(int inbox, long cursor, long limit, bool getTotalCount, int status, GetSentApplyListDelegate callback)
        {
            imsdk.getSentApplyList(inbox, cursor, limit, getTotalCount, status, (FriendApplyListResult result) =>
            {
                try
                {
                    if (callback != null)
                    {
                        IMFriendApplyInfoList applyInfoList = new IMFriendApplyInfoList();
                        applyInfoList.NextCursor = result.nextCursor;
                        applyInfoList.HasMore = result.hasMore;
                        applyInfoList.TotalCount = result.totalCount;
                        List<IMFriendApplyInfo> applyInfos = new List<IMFriendApplyInfo>();
                        if (result.applyList != null)
                        {
                            for (int i = 0; i < result.applyList.Count; i++)
                            {
                                applyInfos.Add(IMConvertTool.Convert(result.applyList[i]));
                            }
                        }
                        applyInfoList.ApplyList = applyInfos;
                        callback.Invoke(IMConvertTool.Convert(result), applyInfoList);
                    }
                }
                catch (Exception e)
                {
                    GMExeption ex = new GMExeption
                    {
                        exception = e
                    };
                    if (GMSDKMgr.instance.exceptionCallback != null)
                    {
                        GMSDKMgr.instance.exceptionCallback.onGMExceptionCallback(ex);
                    }
                }
            });
        }

        public void GetReceivedApplyList(int inbox, long cursor, long limit, bool getTotalCount, int status, GetReceivedApplyListDelegate callback)
        {
            imsdk.getReceivedApplyList(inbox, cursor, limit, getTotalCount, status, (FriendApplyListResult result) =>
            {
                try
                {
                    if (callback != null)
                    {
                        IMFriendApplyInfoList applyInfoList = new IMFriendApplyInfoList();
                        applyInfoList.NextCursor = result.nextCursor;
                        applyInfoList.HasMore = result.hasMore;
                        applyInfoList.TotalCount = result.totalCount;
                        List<IMFriendApplyInfo> applyInfos = new List<IMFriendApplyInfo>();
                        if (result.applyList != null)
                        {
                            for (int i = 0; i < result.applyList.Count; i++)
                            {
                                applyInfos.Add(IMConvertTool.Convert(result.applyList[i]));
                            }
                        }
                        applyInfoList.ApplyList = applyInfos;
                        callback.Invoke(IMConvertTool.Convert(result), applyInfoList);
                    }
                }
                catch (Exception e)
                {
                    GMExeption ex = new GMExeption
                    {
                        exception = e
                    };
                    if (GMSDKMgr.instance.exceptionCallback != null)
                    {
                        GMSDKMgr.instance.exceptionCallback.onGMExceptionCallback(ex);
                    }
                }
            });
        }

        public void SendFriendApply(int inbox, long uid, Dictionary<string, string> ext, SendFriendApplyDelegate callback)
        {
            imsdk.sendFriendApply(inbox, uid, ext, (IMCallbackResult result) =>
            {
                try
                {
                    if (callback != null)
                    {
                        callback.Invoke(IMConvertTool.Convert(result));
                    }
                }
                catch (Exception e)
                {
                    GMExeption ex = new GMExeption
                    {
                        exception = e
                    };
                    if (GMSDKMgr.instance.exceptionCallback != null)
                    {
                        GMSDKMgr.instance.exceptionCallback.onGMExceptionCallback(ex);
                    }
                }
            });
        }

        public void ReplyFriendApply(int inbox, List<long> arrUserID, int attitude, Dictionary<string, string> ext, ReplyFriendApplyDelegate callback)
        {
            imsdk.replyFriendApply(inbox, arrUserID, attitude, ext, (IMCallbackResult result) =>
            {
                try
                {
                    if (callback != null)
                    {
                        callback.Invoke(IMConvertTool.Convert(result));
                    }
                }
                catch (Exception e)
                {
                    GMExeption ex = new GMExeption
                    {
                        exception = e
                    };
                    if (GMSDKMgr.instance.exceptionCallback != null)
                    {
                        GMSDKMgr.instance.exceptionCallback.onGMExceptionCallback(ex);
                    }
                }
            });
        }
    }

}
