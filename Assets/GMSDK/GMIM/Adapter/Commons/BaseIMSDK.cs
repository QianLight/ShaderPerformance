using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UNBridgeLib;
using UNBridgeLib.LitJson;
using GMSDK;

namespace GMSDK
{
	public class BaseIMSDK 
	{
		public BaseIMSDK()
		{
#if UNITY_ANDROID
			UNBridge.Call(IMMethodName.Init, null);
#endif
		}

#if UNITY_ANDROID
		public void setLogOpen(bool open)
		{
			LogUtils.D("IM - setLogOpen:" + open);
			JsonData param = new JsonData();
			param["open"] = open;
			UNBridge.Call(IMMethodName.setLogOpen, param);
		}
#endif

		public void loginIM(long userID, string token)
		{
			LogUtils.D("IM - loginIM");
			JsonData param = new JsonData();
			param["userID"] = userID;
			param["token"] = token;
			UNBridge.Call(IMMethodName.loginIM, param);
		}
		
		public void loginIM(long userID)
		{
			LogUtils.D("IM - loginIMV2");
			JsonData param = new JsonData();
			param["userID"] = userID;
			UNBridge.Call(IMMethodName.loginIMV2, param);
		}

		public void logoutIM()
		{
			LogUtils.D("IM - logoutIM");
			JsonData param = new JsonData();
			UNBridge.Call(IMMethodName.logoutIM, param);
		}

		public void refreshIMToken(string token)
		{
			LogUtils.D("IM - refreshIMToken");
			JsonData param = new JsonData();
			param["token"] = token;
			UNBridge.Call(IMMethodName.refreshIMToken, param);
		}

		public void configIM(GMIMConfig config)
		{
			LogUtils.D("IM - configIM");
			
			if (config == null)
			{
				config = new GMIMConfig();
			}
			
			JsonData param = new JsonData();
			param["config"] = JsonMapper.ToObject(JsonMapper.ToJson(config));
			UNBridge.Call(IMMethodName.configIM, param);
		}

		public string getCurrentUserId()
		{
			LogUtils.D("IM - getCurrentUserId");
			object res = UNBridge.CallSync(IMMethodName.getCurrentUserId, null);
			if (res != null) {
				return (string)res;
			}

			return "0";
		}
		
		public void setAutoPullMessageIntervalSeconds(double seconds)
		{
			LogUtils.D("IM - setAutoPullMessageIntervalSeconds:"+seconds);
			JsonData param = new JsonData();
			param["seconds"] = seconds;
			UNBridge.Call(IMMethodName.setAutoPullMessageIntervalSeconds, param);
		}
		
		public void pullNewMessage(int inbox)
		{
			LogUtils.D("IM - pullNewMessage, inbox:"+inbox);
			JsonData param = new JsonData();
			param["inbox"] = inbox;
			UNBridge.Call(IMMethodName.pullNewMessage, param);
		}
		
#if UNITY_ANDROID
		public void activateLongConnection()
		{
			LogUtils.D("IM - activateLongConnection");
			UNBridge.Call(IMMethodName.connectWS, null);
		}
		
		public void breakLongConnection()
		{
			LogUtils.D("IM - breakLongConnection");
			UNBridge.Call(IMMethodName.disconnectWS, null);
		}

		public bool isLongConnectionKeepLive()
		{
			bool result = false;
			object ret = UNBridge.CallSync(IMMethodName.isWSConnected, null);
			if (ret != null) {
				result = (bool)ret;
			}

			LogUtils.D("IM - isLongConnectionKeepLive:" + result);
			return result;
		}
#endif
		
		public string initConversationDataSourceWithInboxes(List<int> inboxes)
		{
			LogUtils.D("IM - initConversationDataSourceWithInboxes:" + JsonMapper.ToJson(inboxes));

			if (inboxes == null)
			{
				inboxes = new List<int>();
			}
			
			JsonData param = new JsonData();
			param["inboxes"] = JsonMapper.ToObject(JsonMapper.ToJson(inboxes));
			object res = UNBridge.CallSync(IMMethodName.initConversationDataSourceWithInboxes, param);
			if (res != null) {
				return (string)res;
			}
			return "";
		}

		public int numberOfConversations(string conversationDataSourceID)
		{
			LogUtils.D("IM - numberOfConversations");

			JsonData param = new JsonData();
			param["conversationDataSourceID"] = conversationDataSourceID;
			object res = UNBridge.CallSync(IMMethodName.numberOfConversations, param);
			if (res != null) {
				return (int)res;
			}
			return -1;
		}

		public GMIMConversation conversationAtIndex(string conversationDataSourceID, int index)
		{
			LogUtils.D("IM - conversationAtIndex");

			JsonData param = new JsonData();
			param["conversationDataSourceID"] = conversationDataSourceID;
			param["index"] = index;
			object res = UNBridge.CallSync(IMMethodName.conversationAtIndex, param);
			if (res != null) {
				return JsonMapper.ToObject<GMIMConversation>((string)res);
			}
			return null;
		}

		public GMIMConversation getConversationWithID(string conversationId)
		{
			LogUtils.D("IM - getConversationWithID");

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			object res = UNBridge.CallSync(IMMethodName.getConversationWithID, param);
			if (res != null) {
				return JsonMapper.ToObject<GMIMConversation>((string)res);
			}
			return null;
		}

		public void getConversationAsync(string conversationId, int conversationType, int inbox, Action<ConversationResult> callback) {
			LogUtils.D("IM - getConversationAsync");

			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.GetConversationAsyncCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnGetConversationAsyncCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["conversationType"] = conversationType;
			param["inbox"] = inbox;
			UNBridge.Call(IMMethodName.getConversationAsync, param, unCallBack);
		}

		public void updateCurrentIfNeeded(string conversationId, Action<IMCallbackResult>callback)
		{
			LogUtils.D("IM - updateCurrentIfNeeded");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.UpdateCurrentIfNeededCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnUpdateCurrentIfNeededCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			UNBridge.Call(IMMethodName.updateCurrentIfNeeded, param, unCallBack);
		}

		public void setName(string conversationId, string name, Dictionary<string, string> ext, Action<ConversationOperationResult>callback)
		{
			LogUtils.D("IM - setName");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.SetNameCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnSetNameCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["name"] = name;
			
			if (ext == null)
			{
				ext = new Dictionary<string, string>();
			}

			param["ext"] = JsonMapper.ToObject(JsonMapper.ToJson(ext));	
			UNBridge.Call(IMMethodName.setName, param, unCallBack);
		}

		public void setDesc(string conversationId, string desc, Dictionary<string, string> ext, Action<ConversationOperationResult>callback)
		{
			LogUtils.D("IM - setDesc");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.SetDescCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnSetDescCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["desc"] = desc;
			
			if (ext == null)
			{
				ext = new Dictionary<string, string>();
			}

			param["ext"] = JsonMapper.ToObject(JsonMapper.ToJson(ext));
			UNBridge.Call(IMMethodName.setDesc, param, unCallBack);
		}

		public void setIcon(string conversationId, string icon, Dictionary<string, string> ext, Action<ConversationOperationResult>callback)
		{
			LogUtils.D("IM - setIcon");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.SetIconCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnSetIconCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["icon"] = icon;
			
			if (ext == null)
			{
				ext = new Dictionary<string, string>();
			}

			param["ext"] = JsonMapper.ToObject(JsonMapper.ToJson(ext));	
			UNBridge.Call(IMMethodName.setIcon, param, unCallBack);
		}

		public void setNotice(string conversationId, string notice, Dictionary<string, string> ext, Action<ConversationOperationResult>callback)
		{
			LogUtils.D("IM - setNotice");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.SetNoticeCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnSetNoticeCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["notice"] = notice;
			
			if (ext == null)
			{
				ext = new Dictionary<string, string>();
			}

			param["ext"] = JsonMapper.ToObject(JsonMapper.ToJson(ext));	
			UNBridge.Call(IMMethodName.setNotice, param, unCallBack);
		}

		public void setConversationCoreExt(string conversationId, Dictionary<string, string> ext, Action<ConversationOperationResult> callback)
		{
			LogUtils.D("IM - setConversationCoreExt");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.SetConversationCoreExtCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnSetConversationCoreExtCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			if (ext == null)
			{
				ext = new Dictionary<string, string>();
			}

			param["ext"] = JsonMapper.ToObject(JsonMapper.ToJson(ext));	
			UNBridge.Call(IMMethodName.setConversationCoreExt, param, unCallBack);
		}

		public void setConversationSettingExt(string conversationId, Dictionary<string, string> ext, Action<ConversationOperationResult> callback)
		{
			LogUtils.D("IM - setConversationSettingExt");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.SetConversationSettingExtCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnSetConversationSettingExtCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			if (ext == null)
			{
				ext = new Dictionary<string, string>();
			}

			param["ext"] = JsonMapper.ToObject(JsonMapper.ToJson(ext));	
			UNBridge.Call(IMMethodName.setConversationSettingExt, param, unCallBack);
		}

		public void setConversationLocalExt(string conversationId, Dictionary<string, string> ext, Action<ConversationOperationResult> callback)
		{
			LogUtils.D("IM - setConversationLocalExt");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.SetConversationLocalExtCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnSetConversationLocalExtCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			if (ext == null)
			{
				ext = new Dictionary<string, string>();
			}

			param["ext"] = JsonMapper.ToObject(JsonMapper.ToJson(ext));	
			UNBridge.Call(IMMethodName.setConversationLocalExt, param, unCallBack);
		}

		public void createConversationWithOtherParticipants(List<long>otherParticipants, int type, int inbox, string idempotentID, Action<CreateConversationWithOtherParticipantsResult>callback)
		{
			LogUtils.D("IM - createConversationWithOtherParticipants");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.CreateConversationWithOtherParticipantsCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnCreateConversationWithOtherParticipantsCallback);
			if (otherParticipants == null)
			{
				otherParticipants = new List<long>();
			}
			
			JsonData param = new JsonData();
			param["otherParticipants"] = JsonMapper.ToObject(JsonMapper.ToJson(otherParticipants));
			param["type"] = type;
			param["inbox"] = inbox;
			param["idempotentID"] = idempotentID;
			UNBridge.Call(IMMethodName.createConversationWithOtherParticipants, param, unCallBack);
		}

		public void deleteWithMode(string conversationId, GMIMConversationDeleteMode mode, Action<IMCallbackResult>callback)
		{
			LogUtils.D("IM - deleteWithMode");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.DeleteWithModeCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnDeleteWithModeCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["mode"] = (int)mode;
			UNBridge.Call(IMMethodName.deleteWithMode, param, unCallBack);
		}

		public void deleteAllMessages(string conversationId, Action<IMCallbackResult>callback)
		{
			LogUtils.D("IM - deleteAllMessages");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.DeleteAllMessagesCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnDeleteAllMessagesCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			UNBridge.Call(IMMethodName.deleteAllMessages, param, unCallBack);
		}

		public void joinGroup(int inbox, string conversationId, Dictionary<string, string> bizExtension, Action<ConversationOperationResult> callback)
		{
			LogUtils.D("IM - joinGroup, cid:" + conversationId);
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.joinGroupCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnJoinGroupCallback);
			if (bizExtension == null)
			{
				bizExtension = new Dictionary<string, string>();
			}

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["inbox"] = inbox;
			param["bizExtension"] = JsonMapper.ToObject(JsonMapper.ToJson(bizExtension));
			UNBridge.Call(IMMethodName.joinGroup, param, unCallBack);
		}

		public void addParticipants(string conversationId, List<long> participants, Dictionary<string, string> bizExtension, Action<AddParticipantsResult>callback)
		{
			LogUtils.D("IM - addParticipants");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.AddParticipantsCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnAddParticipantsCallback);
			if (participants == null)
			{
				participants = new List<long>();
			}
			if (bizExtension == null)
			{
				bizExtension = new Dictionary<string, string>();
			}
			
			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["participants"] = JsonMapper.ToObject(JsonMapper.ToJson(participants));
			param["bizExtension"] = JsonMapper.ToObject(JsonMapper.ToJson(bizExtension));
			UNBridge.Call(IMMethodName.addParticipants, param, unCallBack);
		}

		public void removeParticipants(string conversationId, List<long> participants, Dictionary<string, string> bizExtension, Action<RemovedParticipantsResult>callback)
		{
			LogUtils.D("IM - removeParticipants");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.RemovedParticipantsCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnRemoveParticipantsCallback);
			if (participants == null)
			{
				participants = new List<long>();
			}
			if (bizExtension == null)
			{
				bizExtension = new Dictionary<string, string>();
			}
			
			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["participants"] = JsonMapper.ToObject(JsonMapper.ToJson(participants));
			param["bizExtension"] = JsonMapper.ToObject(JsonMapper.ToJson(bizExtension));
			UNBridge.Call(IMMethodName.removeParticipants, param, unCallBack);
		}

		public void leaveConversation(string conversationId, Action<ConversationOperationResult>callback)
		{
			LogUtils.D("IM - leaveConversation");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.LeaveConversationCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnLeaveConversationCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			UNBridge.Call(IMMethodName.leaveConversation, param, unCallBack);
		}

		public void dismissConversation(string conversationId, Action<ConversationOperationResult>callback)
		{
			LogUtils.D("IM - dismissConversation");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.DismissConversationCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnDismissConversationCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			UNBridge.Call(IMMethodName.dismissConversation, param, unCallBack);
		}

		public void setRoleForParticipant(string conversationId, long participant, GMIMConversationParticipantRole role, Action<ConversationOperationResult>callback)
		{
			LogUtils.D("IM - setRoleForParticipant");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.SetRoleForParticipantCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnSetRoleForParticipantCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["participant"] = participant;
			param["role"] = (int)role;
			UNBridge.Call(IMMethodName.setRoleForParticipant, param, unCallBack);
		}

		public void setAliasForParticipant(string conversationId, long participant, string alias, Action<ConversationOperationResult>callback)
		{
			LogUtils.D("IM - setAliasForParticipant");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.SetAliasForParticipantCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnSetAliasForParticipantCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["participant"] = participant;
			param["alias"] = alias;
			UNBridge.Call(IMMethodName.setAliasForParticipant, param, unCallBack);
		}

		public void setDraft(string conversationId, string draft)
		{
			LogUtils.D("IM - setAliasForParticipant");
			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["draft"] = draft;
			UNBridge.Call(IMMethodName.setDraft, param);
		}

		public void setMute(string conversationId, bool shouldMute, Action<ConversationOperationResult>callback)
		{
			LogUtils.D("IM - setMute");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.SetMuteCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnSetMuteCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["shouldMute"] = shouldMute;
			UNBridge.Call(IMMethodName.setMute, param, unCallBack);
		}

		public void userWillEnterCurrentConversation(string conversationId)
		{
			LogUtils.D("IM - userWillEnterCurrentConversation");
			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["mode"] = 0;
			param["offset"] = 0;
			UNBridge.Call(IMMethodName.userWillEnterCurrentConversation, param);
		}

		public void userWillEnterCurrentConversation(string conversationId, int mode, int offset)
		{
			LogUtils.D("IM - userWillExitCurrentConversationWithMode");
			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["mode"] = mode;
			param["offset"] = offset;
			UNBridge.Call(IMMethodName.userWillEnterCurrentConversation, param);
		}

		public void userWillExitCurrentConversation(string conversationId)
		{
			LogUtils.D("IM - userWillExitCurrentConversation");
			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			UNBridge.Call(IMMethodName.userWillExitCurrentConversation, param);
		}

		public void fetchConversationAllParticipants(string conversationId, Action<fetchConversationAllParticipantsResult>callback)
		{
			LogUtils.D("IM - fetchConversationAllParticipants");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.FetchConversationAllParticipantsCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnFetchConversationAllParticipantsCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			UNBridge.Call(IMMethodName.fetchConversationAllParticipants, param, unCallBack);
		}

		public void markAllMessagesAsRead(string conversationId, Action<IMCallbackResult>callback)
		{
			LogUtils.D("IM - markAllMessagesAsRead");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.MarkAllMessagesAsReadCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnMarkAllMessagesAsReadCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			UNBridge.Call(IMMethodName.markAllMessagesAsRead, param, unCallBack);
		}

		public void markAllMessagesAsReadBeforeMessage(string conversationId, string messageId, Action<IMCallbackResult> callback)
		{
			LogUtils.D("IM - markAllMessagesAsReadBeforeMessage");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.MarkAllMessagesAsReadBeforeMessageCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnMarkAllMessagesAsReadBeforeMessageCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["messageId"] = messageId;
			UNBridge.Call(IMMethodName.markAllMessagesAsReadBeforeMessage, param, unCallBack);
		}

		public string sendMessage(string conversationId, GMIMSendMessage message)
		{
			LogUtils.D("IM - sendMessage");
			if (message == null)
			{
				message = new GMIMSendMessage();
			}
			
			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["message"] = JsonMapper.ToObject(JsonMapper.ToJson(message));
			object res = UNBridge.CallSync(IMMethodName.sendMessage, param);
			if (res != null) {
				return (string)res;
			}
			return "";
		}

		public void sendMessageAsync(string conversationId, int conversationType, int inbox, GMIMSendMessage message, Action<SendMessageAsyncIDResult> callback)
		{
			LogUtils.D("IM - sendMessageAsync");
			if (message == null)
			{
				message = new GMIMSendMessage();
			}
			
			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["conversationType"] = conversationType;
			param["inbox"] = inbox;
  			param["message"] = JsonMapper.ToObject(JsonMapper.ToJson(message));
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.SendMessageAsyncIDCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnSendMessageAsyncIDCallback);
			UNBridge.Call(IMMethodName.sendMessageAsync, param, unCallBack);
		}

		public void resendMessage(string conversationId, string messageId, Action<ResendMessageResult>callback)
		{
			LogUtils.D("IM - resendMessage");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.ResendMessageCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnResendMessageCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["messageId"] = messageId;
			UNBridge.Call(IMMethodName.resendMessage, param, unCallBack);
		}

		public GMIMMessage getMessageWithID(string conversationId, string messageId)
		{
			LogUtils.D("IM - getMessageWithID");

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["messageId"] = messageId;
			object res = UNBridge.CallSync(IMMethodName.getMessageWithID, param);
			if (res != null) {
				try
				{
					return JsonMapper.ToObject<GMIMMessage>((string)res);
				}
				catch (Exception e)
				{
					LogUtils.E("IM - getMessageWithID error:" + e);
				}
			}
			return null;
		}

		public void deleteMessageID(string conversationId, string messageId, Action<IMCallbackResult>callback)
		{
			LogUtils.D("IM - deleteMessageID");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.DeleteMessageIDCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnDeleteMessageIDCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["messageId"] = messageId;
			UNBridge.Call(IMMethodName.deleteMessageID, param, unCallBack);
		}

		public int numberOfMessages(string conversationId)
		{
			LogUtils.D("IM - numberOfMessages");

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			object res = UNBridge.CallSync(IMMethodName.numberOfMessages, param);
			if (res != null) {
				return (int)res;
			}
			return -1;
		}

		public GMIMMessage messageAtIndex(string conversationId, int index)
		{
			LogUtils.D("IM - messageAtIndex");

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["index"] = index;
			object res = UNBridge.CallSync(IMMethodName.messageAtIndex, param);
			if (res != null) {
				try
				{
					return JsonMapper.ToObject<GMIMMessage>((string)res);
				}
				catch (Exception e)
				{
					LogUtils.E("IM - messageAtIndex error:" + e);
				}
			}
			return null;
		}

		public void loadOlderMessages(string conversationId, Action<IMCallbackResult>callback)
		{
			LogUtils.D("IM - loadOlderMessages");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.LoadOlderMessagesCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnLoadOlderMessagesCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			UNBridge.Call(IMMethodName.loadOlderMessages, param, unCallBack);
		}

		public void loadNewerMessages(string conversationId, Action<IMCallbackResult>callback)
		{
			LogUtils.D("IM - loadNewerMessages");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.LoadNewerMessagesCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnLoadNewerMessagesCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			UNBridge.Call(IMMethodName.loadNewerMessages, param, unCallBack);
		}

		public GMIMSendMessage constructTextMessage(string text)
		{
			LogUtils.D("IM - constructTextMessage");

			JsonData param = new JsonData();
			param["text"] = text;
			object res = UNBridge.CallSync(IMMethodName.constructTextMessage, param);
			if (res != null) {
				return JsonMapper.ToObject<GMIMSendMessage>((string)res);
			}
			return null;
		}

		public GMIMSendMessage constructImageMessage(string imagePath, int imageWidth, int imageHeight, string mime, 
			string format, int thumbWidth, int thumbHeight, int previewWidth, int previewHeight)
		{
			LogUtils.D("IM - constructImageMessage");

			JsonData param = new JsonData();
			param["imagePath"] = imagePath;
			param["imageWidth"] = imageWidth;
			param["imageHeight"] = imageHeight;
			param["mime"] = mime;
			param["format"] = format;
			param["thumbWidth"] = thumbWidth;
			param["thumbHeight"] = thumbHeight;
			param["previewWidth"] = previewWidth;
			param["previewHeight"] = previewHeight;
			object res = UNBridge.CallSync(IMMethodName.constructImageMessage, param);
			if (res != null) {
				return JsonMapper.ToObject<GMIMSendMessage>((string)res);
			}
			return null;
		}

		public GMIMSendMessage constructVoiceMessage(string voiceId)
		{
			LogUtils.D("IM - constructVoiceMessage");

			JsonData param = new JsonData();
			param["voiceId"] = voiceId;
			object res = UNBridge.CallSync(IMMethodName.constructVoiceMessage, param);
			if (res != null) {
				return JsonMapper.ToObject<GMIMSendMessage>((string)res);
			}
			return null;
		}
		
		public GMIMSendMessage constructVoiceMessageWithLocalPath(string localPath, long duration)
		{
			LogUtils.D("IM - constructVoiceMessageWithLocalPath");
			JsonData param = new JsonData();
			param["localPath"] = localPath;
			param["duration"] = duration;
			object res = UNBridge.CallSync(IMMethodName.constructVoiceMessageWithLocalPath, param);
			if (res != null) {
				return JsonMapper.ToObject<GMIMSendMessage>((string)res);
			}
			return null;
		}

		public GMIMSendMessage constructEmoticonMessage(Dictionary<string, object> emotionMessage)
		{
			LogUtils.D("IM - constructEmoticonMessage");
			GMIMSendMessage sendMessage = new GMIMSendMessage();
			sendMessage.content = JsonMapper.ToJson(emotionMessage);
			sendMessage.messageType = (int)GMIMMessageType.GMIMMessageTypeEmoticon;
			return sendMessage;
		}

		public string getTextContent(GMIMMessage message)
		{
			LogUtils.D("IM - getTextContent");
			string text = "";
			try
			{
				text = DataUtils.GetString(JsonMapper.ToObject(message.content), "text");
			}
			catch (Exception e)
			{
				LogUtils.E("IM - getTextContent error:" + e);
			}

			return text;
		}

		public GMIMImage getImage(GMIMMessage message)
		{
			LogUtils.D("IM - getImage");

			JsonData param = new JsonData();
			param["conversationId"] = message.belongingConversationIdentifier;
			param["messageId"] = message.messageID;
			object res = UNBridge.CallSync(IMMethodName.getImage, param);
			if (res != null)
			{
				GMIMImage image = JsonMapper.ToObject<GMIMImage>((string) res);
				try
				{
					JsonData jsonData = JsonMapper.ToObject(message.content);
					image.imageWidth = DataUtils.GetInt(jsonData, "GMImageWidth");
					image.imageHeight = DataUtils.GetInt(jsonData, "GMImageHeight");
				}
				catch (Exception e)
				{
					LogUtils.E("IM - getImage error:" + e);
				}

				return image;
			}

			return null;
		}

		public string getVoiceId(GMIMMessage message)
		{
			LogUtils.D("IM - getVoiceId");
			try
			{
				JsonData jsonData = JsonMapper.ToObject(message.content);
				return DataUtils.GetString(jsonData, "GMVoiceID");
			}
			catch (Exception e)
			{
				LogUtils.E("IM - getVoiceId error:" + e);
				return "";
			}
		}

		public long getVoiceDuration(GMIMMessage message)
		{
			LogUtils.D("IM - getVoiceDuration");
			long result = 0;
			try
			{
				return DataUtils.GetLong(JsonMapper.ToObject(message.content), "GMVoiceDuration");
			}
			catch (Exception e)
			{
				LogUtils.E("IM - getVoiceDuration error:" + e);
				result = 0;
			}

			return result;
		}

		public GMIMSendMessage skipRealSendAndMarkAsSent(GMIMSendMessage sendMessage)
		{
			LogUtils.D("IM - skipRealSendAndMarkAsSent");
			if (sendMessage == null)
			{
				sendMessage = new GMIMSendMessage();
			}
			
			JsonData param = new JsonData();
			param["sendMessage"] = JsonMapper.ToObject(JsonMapper.ToJson(sendMessage));
			object res = UNBridge.CallSync(IMMethodName.skipRealSendAndMarkAsSent, param);
			if (res != null) {
				return JsonMapper.ToObject<GMIMSendMessage>((string)res);
			}
			return null;
		}

		public void recallMessage(string conversationId, string messageId, Action<IMCallbackResult>callback)
		{
			LogUtils.D("IM - recallMessage");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.RecallMessageCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnRecallMessageCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["messageId"] = messageId;
			UNBridge.Call(IMMethodName.recallMessage, param, unCallBack);
		}

		public void broadCastSendMessage(GMIMSendMessage message, int inbox, string conversationId, Action<BroadCastSendMessageResult>callback)
		{
			LogUtils.D("IM - broadCastSendMessage");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.BroadCastSendMessageCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnBroadCastSendMessageCallback);
			if (message == null)
			{
				message = new GMIMSendMessage();
			}
			
			JsonData param = new JsonData();
			param["message"] = JsonMapper.ToObject(JsonMapper.ToJson(message));
			param["inbox"] = inbox;
			param["conversationId"] = conversationId;
			UNBridge.Call(IMMethodName.broadCastSendMessage, param, unCallBack);
		}

		public void loadNewBroadCastMessage(string conversationId, int inbox, long cursor, long limit, Action<LoadNewBroadCastMessageResult>callback)
		{
			LogUtils.D("IM - loadNewBroadCastMessage");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.LoadNewBroadCastMessageCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnLoadNewBroadCastMessageCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["inbox"] = inbox;
			param["cursor"] = cursor;
			param["limit"] = limit;
			UNBridge.Call(IMMethodName.loadNewBroadCastMessage, param, unCallBack);
		}
		
		public void loadOldBroadCastMessage(string conversationId, int inbox, long cursor, long limit, Action<LoadNewBroadCastMessageResult>callback)
		{
			LogUtils.D("IM - loadOldBroadCastMessage");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.LoadOldBroadCastMessageCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnLoadOldBroadCastMessageCallback);

			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["inbox"] = inbox;
			param["cursor"] = cursor;
			param["limit"] = limit;
			UNBridge.Call(IMMethodName.loadOldBroadCastMessage, param, unCallBack);
		}

		public void broadCastUserCounter(List<string> conversationsIDArray, int inbox, Action<BroadCastUserCounterResult>callback)
		{
			LogUtils.D("IM - broadCastUserCounter");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.BroadCastUserCounterCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnBroadCastUserCounterCallback);

			List<GMIMBroadCastRet> conversationsArray = new List<GMIMBroadCastRet>();
			foreach (string conversationID in conversationsIDArray)
            {
				GMIMBroadCastRet ret = new GMIMBroadCastRet();
				ret.conversationShortId = long.Parse(conversationID);
				conversationsArray.Add(ret);
			}

			JsonData param = new JsonData();
			param["conversationsArray"] = JsonMapper.ToObject(JsonMapper.ToJson(conversationsArray));
			param["inbox"] = inbox;
			UNBridge.Call(IMMethodName.broadCastUserCounter, param, unCallBack);
		}

		public void registerBroadCastListener(string conversationId)
		{
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			LogUtils.D("IM - registerBroadCastListener");
			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			UNBridge.Call(IMMethodName.registerBroadCastListener, param, unCallBack);
		}
		
		public void unregisterBroadCastListener(string conversationId)
		{
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			LogUtils.D("IM - unregisterBroadCastListener");
			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			UNBridge.Call(IMMethodName.unregisterBroadCastListener, param, unCallBack);
		}
		
		public void enterBroadCastConversation(int inbox, string conversationId)
		{
			LogUtils.D("IM - enterBroadCastConversation");
			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["inbox"] = inbox;
			UNBridge.Call(IMMethodName.enterBroadCastConversation, param, new IMCallbackHandler());
		}
		
		public void exitBroadCastConversation(string conversationId)
		{
			LogUtils.D("IM - exitBroadCastConversation");
			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			UNBridge.Call(IMMethodName.exitBroadCastConversation, param, new IMCallbackHandler());
		}
		
		public void resumeBroadCastConversation(string conversationId, bool fromLast)
		{
			LogUtils.D("IM - resumeBroadCastConversation");
			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			param["fromLast"] = fromLast;
			UNBridge.Call(IMMethodName.resumeBroadCastConversation, param, new IMCallbackHandler());
		}
		
		public void pauseBroadCastConversation(string conversationId)
		{
			LogUtils.D("IM - pauseBroadCastConversation");
			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			UNBridge.Call(IMMethodName.pauseBroadCastConversation, param, new IMCallbackHandler());
		}

		public void setBroadCastThrottleDelay(long delay)
		{
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			LogUtils.D("IM - setBroadCastThrottleDelay");
			JsonData param = new JsonData();
			param["delay"] = delay;
			UNBridge.Call(IMMethodName.setBroadCastThrottleDelay, param, unCallBack);
		}
		
		public void fetchUserInfoInInbox(int inbox, long userID, Action<FetchUserInfoInInboxResult>callback)
		{
			LogUtils.D("IM - fetchUserInfoInInbox");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.FetchUserInfoInInboxCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnFetchUserInfoInInboxCallback);

			JsonData param = new JsonData();
			param["inbox"] = inbox;
			param["userID"] = userID;
			UNBridge.Call(IMMethodName.fetchUserInfoInInbox, param, unCallBack);
		}

		public void modifyUsersBlockList(int inbox, List<long> arrUserID, bool toBlockList, bool blockType, string convId, long shortId, int convType, Action<ModifyUsersblockListResult>callback)
		{
			LogUtils.D("IM - modifyUsersBlockList");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.ModifyUsersBlockListCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnModifyUsersBlockListCallback);
			if (arrUserID == null)
			{
				arrUserID = new List<long>();
			}
			
			JsonData param = new JsonData();
			param["inbox"] = inbox;
			param["arrUserID"] = JsonMapper.ToObject(JsonMapper.ToJson(arrUserID));
			param["toBlockList"] = toBlockList;
			param["blockType"] = blockType;
			param["conversationId"] = convId;
			param["shortId"] = shortId;
			param["conversationType"] = convType;
			UNBridge.Call(IMMethodName.modifyUsersBlockList, param, unCallBack);
		}

		public void fetchUserBlockStatusInInbox(int inbox, long userID, bool blockType, string convId, long shortId, int convType, Action<FetchUserblockStatusInInboxResult>callback)
		{
			LogUtils.D("IM - fetchUserBlockStatusInInbox");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.FetchUserblockStatusInInboxCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnFetchUserBlackStatusInInboxCallback);

			JsonData param = new JsonData();
			param["inbox"] = inbox;
			param["userID"] = userID;
			param["blockType"] = blockType;
			param["conversationId"] = convId;
			param["shortId"] = shortId;
			param["conversationType"] = convType;
			UNBridge.Call(IMMethodName.fetchUserBlockStatusInInbox, param, unCallBack);
		}

		public void fetchBlockListUsersInInbox(int inbox, long cursor, int limit, bool blockType, string convId, long shortId, int convType, Action<FetchblockListUsersInInboxResult>callback)
		{
			LogUtils.D("IM - fetchBlockListUsersInInbox");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.FetchblockListUsersInInboxCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnFetchblockListUsersInInboxCallback);

			JsonData param = new JsonData();
			param["inbox"] = inbox;
			param["cursor"] = cursor;
			param["limit"] = limit;
			param["blockType"] = blockType;
			param["conversationId"] = convId;
			param["shortId"] = shortId;
			param["conversationType"] = convType;
			UNBridge.Call(IMMethodName.fetchBlockListUsersInInbox, param, unCallBack);
		}

		public void queryUserInfo(int inbox, long uid, bool fromSource, Action<QueryUserInfoResult> callback)
		{
			LogUtils.D("IM - queryUserInfo");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.QueryUserInfoCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnQueryUserInfoCallback);

			JsonData param = new JsonData();
			param["inbox"] = inbox;
			param["uid"] = uid;
			param["fromSource"] = fromSource;
			UNBridge.Call(IMMethodName.queryUserInfo, param, unCallBack);
		}
		
		public void batchQueryUserInfo(int inbox, List<long> arrUserID, Action<BatchQueryUserInfoResult> callback)
		{
			LogUtils.D("IM - batchQueryUserInfo");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.BatchQueryUserInfoCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnBatchQueryUserInfoCallback);
			if (arrUserID == null)
			{
				arrUserID = new List<long>();
			}
			JsonData param = new JsonData();
			param["inbox"] = inbox;
			param["uidList"] = JsonMapper.ToObject(JsonMapper.ToJson(arrUserID));
			UNBridge.Call(IMMethodName.batchQueryUserInfo, param, unCallBack);
		}
		
		public void searchUser(int inbox, string key, Action<BatchQueryUserInfoResult> callback)
		{
			LogUtils.D("IM - searchUser");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.SearchUserCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnSearchUserCallback);

			JsonData param = new JsonData();
			param["inbox"] = inbox;
			param["key"] = key;
			UNBridge.Call(IMMethodName.searchUser, param, unCallBack);
		}
		
		public void deleteFriends(int inbox, List<long> arrUserID, Action<DeleteFriendsResult> callback)
		{
			LogUtils.D("IM - deleteFriends");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.DeleteFriendsCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnDeleteFriendsCallback);
			if (arrUserID == null)
			{
				arrUserID = new List<long>();
			}
			JsonData param = new JsonData();
			param["inbox"] = inbox;
			param["uidList"] = JsonMapper.ToObject(JsonMapper.ToJson(arrUserID));
			UNBridge.Call(IMMethodName.deleteFriends, param, unCallBack);
		}

		public void getFriendList(int inbox, long cursor, long limit, bool getTotalCount, Action<FriendListResult> callback)
		{
			LogUtils.D("IM - getFriendList");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.GetFriendListCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnGetFriendListCallback);

			JsonData param = new JsonData();
			param["inbox"] = inbox;
			param["cursor"] = cursor;
			param["limit"] = limit;
			param["getTotalCount"] = getTotalCount;
			UNBridge.Call(IMMethodName.getFriendList, param, unCallBack);
		}
		
		public void getSentApplyList(int inbox, long cursor, long limit, bool getTotalCount, int status, Action<FriendApplyListResult> callback)
		{
			LogUtils.D("IM - getSentApplyList");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.GetSentApplyListCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnGetSentApplyListCallback);

			JsonData param = new JsonData();
			param["inbox"] = inbox;
			param["cursor"] = cursor;
			param["limit"] = limit;
			param["getTotalCount"] = getTotalCount;
			param["status"] = status;
			UNBridge.Call(IMMethodName.getSentApplyList, param, unCallBack);
		}
		
		public void getReceivedApplyList(int inbox, long cursor, long limit, bool getTotalCount, int status, Action<FriendApplyListResult> callback)
		{
			LogUtils.D("IM - getReceivedApplyList");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.GetReceivedApplyListCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnGetReceivedApplyListCallback);

			JsonData param = new JsonData();
			param["inbox"] = inbox;
			param["cursor"] = cursor;
			param["limit"] = limit;
			param["getTotalCount"] = getTotalCount;
			param["status"] = status;
			UNBridge.Call(IMMethodName.getReceivedApplyList, param, unCallBack);
		}
		
		public void sendFriendApply(int inbox, long uid, Dictionary<string, string> ext, Action<IMCallbackResult> callback)
		{
			LogUtils.D("IM - sendFriendApply");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.SendFriendApplyCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnSendFriendApplyCallback);
			if (ext == null)
			{
				ext = new Dictionary<string, string>();
			}
			JsonData param = new JsonData();
			param["inbox"] = inbox;
			param["uid"] = uid;
			param["ext"] = JsonMapper.ToObject(JsonMapper.ToJson(ext));
			UNBridge.Call(IMMethodName.sendFriendApply, param, unCallBack);
		}

		public void replyFriendApply(int inbox, List<long> arrUserID, int attitude, Dictionary<string, string> ext, Action<IMCallbackResult> callback)
		{
			LogUtils.D("IM - replyFriendApply");
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.ReplyFriendApplyCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnReplyFriendApplyCallback);
			if (ext == null)
			{
				ext = new Dictionary<string, string>();
			}
			if (arrUserID == null)
			{
				arrUserID = new List<long>();
			}
			JsonData param = new JsonData();
			param["inbox"] = inbox;
			param["uidList"] = JsonMapper.ToObject(JsonMapper.ToJson(arrUserID));
			param["attitude"] = attitude;
			param["ext"] = JsonMapper.ToObject(JsonMapper.ToJson(ext));
			UNBridge.Call(IMMethodName.replyFriendApply, param, unCallBack);
		}
		
		public bool hasOlderMessages(string conversationId)
		{
			LogUtils.D("IM - hasOlderMessages");
			JsonData param = new JsonData();
			param["conversationId"] = conversationId;
			object res = UNBridge.CallSync(IMMethodName.hasOlderMessages, param);
			if (res != null)
			{
				return (bool)res;
			}
			return false;
		}

		public void ListenLoginIMEvent(Action<IMCallbackResult> callback)
		{
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.LoginIMCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.onLoginIMCallback);
			UNBridge.Listen(IMResultName.onloginIM, unCallBack);
		}

		public void ListenInitMessageEndEvent(Action<InitMessageEndResult> callback)
		{
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.InitMessageEndCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.onInitMessageEndCallback);
			UNBridge.Listen(IMResultName.onInitMessageEnd, unCallBack);
		}

		public void ListenInboxInitMessageEndEvent(Action<InboxInitMessageEndResult> callback)
		{
			IMCallbackHandler unCallBack = new IMCallbackHandler();
            unCallBack.InboxInitMessageEndCallback = callback;
            unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.onInboxInitMessageEndCallback);
            UNBridge.Listen(IMResultName.onInboxInitMessageEnd, unCallBack);
        }

		public void ListenIMTokenExpiredEvent(Action<IMTokenExpiredResult> callback)
		{
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.IMTokenExpiredCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.onIMTokenExpiredCallback);
			UNBridge.Listen(IMResultName.onIMTokenExpired, unCallBack);
		}

		public void ListenConversationUpdatedEvent(Action<ConversationIdResult> callback)
		{
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.ConversationUpdatedCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.onConversationUpdatedCallback);
			UNBridge.Listen(IMResultName.onConversationUpdated, unCallBack);
		}

		public void ListenConversationParticipantsUpdatedEvent(Action<ConversationIdResult> callback)
		{
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.ConversationParticipantsUpdatedCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.onConversationParticipantsUpdatedCallback);
			UNBridge.Listen(IMResultName.onConversationParticipantsUpdated, unCallBack);
		}

		public void ListenMessageUpdatedEvent(Action<MessageUpdatedResult> callback)
		{
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.MessageUpdatedCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.onMessageUpdatedCallback);
			UNBridge.Listen(IMResultName.onMessageUpdated, unCallBack);
		}

		public void ListenMessageListUpdatedEvent(Action<MessageListUpdatedResult> callback)
		{
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.MessageListUpdatedCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.onMessageListUpdatedCallback);
			UNBridge.Listen(IMResultName.onMessageListUpdated, unCallBack);
		}

		public void ListenConversationDataSourceDidUpdateEvent(Action<ConversationDataSourceUpdateResult> callback)
		{
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.ConversationDataSourceUpdateCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.onConversationDataSourceUpdateCallback);
			UNBridge.Listen(IMResultName.onConversationDataSourceDidUpdate, unCallBack);
		}

		public void ListenSendMessageEvent(Action<SendMessageResult> callback)
		{
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.SendMessageCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.onSendMessageCallback);
			UNBridge.Listen(IMResultName.onSendMessage, unCallBack);
		}
		
		public void ListenReceiveBroadCastMessageEvent(Action<ReceiveBroadCastMessageResult> callback)
		{
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.ReceiveBroadCastMessageCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.onReceiveBroadCastMessageCallback);
			UNBridge.Listen(IMResultName.onReceiveBroadCastMessage, unCallBack);
		}
		
		public void ListenDeleteBroadCastMessageEvent(Action<DeleteBroadCastMessageResult> callback)
		{
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.DeleteBroadCastMessageCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.onDeleteBroadCastMessageCallback);
			UNBridge.Listen(IMResultName.onDeleteBroadCastMessage, unCallBack);
		}
		
		public void ListenReceiveFriendApplyEvent(Action<FriendEventResult> callback)
		{
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.ReceiveFriendApplyCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.onReceiveFriendApplyCallback);
			UNBridge.Listen(IMResultName.onReceiveFriendApply, unCallBack);
		}
		
		public void ListenDeleteFriendEvent(Action<FriendEventResult> callback)
		{
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.DeleteFriendCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.onDeleteFriendCallback);
			UNBridge.Listen(IMResultName.onDeleteFriend, unCallBack);
		}
		
		public void ListenAddFriendEvent(Action<FriendEventResult> callback)
		{
			IMCallbackHandler unCallBack = new IMCallbackHandler();
			unCallBack.AddFriendCallback = callback;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.onAddFriendCallback);
			UNBridge.Listen(IMResultName.onAddFriend, unCallBack);
		}
	}
}
