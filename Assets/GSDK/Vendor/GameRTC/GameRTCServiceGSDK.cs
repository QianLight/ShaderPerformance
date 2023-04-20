using System;
using System.Collections.Generic;
using GMSDK;
using UNBridgeLib;
using UNBridgeLib.LitJson;

namespace GameRTC
{
    public class GameRTCService : IGameRTCService, IGameRTCCallback
    {
        #region Events

        public event OnJoinRoomResultEventHandler OnJoinRoomResultEvent;
        public event OnLeaveRoomEventHandler OnLeaveRoomEvent;
        public event OnUserJoinedEventHandler OnUserJoinedEvent;
        public event OnUserLeaveEventHandler OnUserLeaveEvent;

        public event OnMicrophoneEnabledEventHandler OnMicrophoneEnabledEvent;
        public event OnAudioSendEnabledEventHandler OnAudioSendEnabledEvent;
        public event OnSpeakerphoneEnabledEventHandler OnSpeakerphoneEnabledEvent;

        public event OnAudioVolumeIndicationEventHandler OnAudioVolumeIndicationEvent;
        public event OnNetworkQualityEventHandler OnNetworkQualityEvent;
        public event OnConnectionStateChangedEventHandler OnConnectionStateChangedEvent;

        public event OnRoomWarningEventHandler OnRoomWarningEvent;
        public event OnRoomErrorEventHandler OnRoomErrorEvent;
        public event OnEngineWarningEventHandler OnEngineWarningEvent;

        #endregion

        #region Variables
        
        private static GameRTCService _instance = null;
        private static int _roomCount = 0;
        private GameRTCEngineParams _initParams;

		#endregion
        
		#region Methods

		public GameRTCService()
		{
#if UNITY_ANDROID
			UNBridge.Call(GameRTCMethodName.registerGameRTC, null);
#endif
		}
		
        public void Initialize(GameRTCEngineParams initParams)
        {
            _instance = this;
            _initParams = initParams;
			_initParams.Params = initParams.Params;
            Loom.Initialize();
        }
        
        private void InitGameRTCEngine(GameRTCEngineConfig config, string parameter)
        {
            LogUtils.D("InitGameRTCEngine");
            JsonData param = new JsonData();
			param["config"] = JsonMapper.ToJson(config);
			param["parameter"] = parameter;
            UNBridge.Call(GameRTCMethodName.initGameRTCEngine, param);
			string logInfo = string.Format("AppID: {0},parameter: {1}", config.AppID, parameter);
			GameRTCLog.ReportApiCall("InitGameRTCEngine", logInfo);
        }

        public void Release()
        {
            LogUtils.D("Release");
            JsonData param = new JsonData();
            UNBridge.Call(GameRTCMethodName.release, param);
			GameRTCLog.ReportApiCall("Release", "");
        }
        
        public string GetSdkVersion()
        {
            LogUtils.D("GetSdkVersion");
            JsonData param = new JsonData();
            object res = UNBridge.CallSync(GameRTCMethodName.getSdkVersion, param);
            string version = "";
            if (res != null) 
            {
                version = (string)res;
            }
            GameRTCLog.ReportApiCall("GetSdkVersion", version);
            return version;
        }

        public int JoinRoom(string roomID, string userID, string token, GameRTCRoomConfig roomConfig)
        {
            if (_initParams.Params == null)
			{
				_initParams.Params = new Dictionary<string, object>();
			}
			//区域码取非0值均选择default海外网络
			if (_initParams.AreaCode != 0)
            {
                _initParams.Params["config_hosts"] = "[\"rtcg.bytevcloud.com\"]";
                _initParams.Params["access_hosts"] = "[\"rtcpcg-access-sg.bytevcloud.com\",\"rtcpcg-access-va.bytevcloud.com\",\"rtcg-access.bytevcloud.com\"]";
                _initParams.Params["rtc.log_sdk_websocket_url"] = "wss://rtc-logger.bytevcloud.com/report";
            }
            string parameter = JsonMapper.ToJson(_initParams.Params);
            GameRTCEngineConfig config = new GameRTCEngineConfig();
            config.AppID = _initParams.AppID;

			GameRTCListenJoinRoomResult();
			GameRTCListenLeaveRoom();
			GameRTCListenUserJoined();
			GameRTCListenUserLeave();
			GameRTCListenMicrophoneEnabled();
			GameRTCListenAudioSendEnabled();
			GameRTCListenSpeakerphoneEnabled();
			GameRTCListenAudioVolumeIndication();
			GameRTCListenConnectionState();
			GameRTCListenNetworkQuality();
			GameRTCListenRoomWarning();
			GameRTCListenRoomError();
			GameRTCListenEngineWarning();

			InitGameRTCEngine(config, parameter);

            JsonData param = new JsonData();
            param["roomID"] = roomID;
            param["userID"] = userID;
            param["token"] = token;
			param["roomConfig"] = JsonMapper.ToJson(roomConfig);
            LogUtils.D("JoinRoom");
            object res = UNBridge.CallSync(GameRTCMethodName.joinRoom, param);
            int code = -1;
			if (res != null)
            {
                code = (int)res;
            }
            string logInfo = string.Format("roomID: {0}, userID: {1}, token: {2}, enableRangeAudio: {3}, enableSpatialAudio: {4}, audioVolumeIndicationInterval: {5}, roomType: {6}, code: {7}",
                roomID, userID, token, roomConfig.EnableRangeAudio, roomConfig.EnableSpatialAudio, roomConfig.AudioVolumeIndicationInterval, roomConfig.RoomType, code);
            GameRTCLog.ReportApiCall("JoinRoom", logInfo);
            return code;
        }

        public int LeaveRoom(string roomID)
        {
            LogUtils.D("LeaveRoom");
            JsonData param = new JsonData();
            param["roomID"] = roomID;
            object res = UNBridge.CallSync(GameRTCMethodName.leaveRoom, param);
            int code = -1;
			if (res != null)
            {
                code = (int)res;
            }
            string logInfo = string.Format("roomID: {0}, code: {1}", roomID, code);
            GameRTCLog.ReportApiCall("LeaveRoom", logInfo);
			return code;
        }
		
        public int UpdateToken(string roomID, string token)
        {
            LogUtils.D("UpdateToken");
            JsonData param = new JsonData();
            param["roomID"] = roomID;
            param["token"] = token;
            object res = UNBridge.CallSync(GameRTCMethodName.updateToken, param);
            int code = -1;
			if (res != null)
            {
                code = (int)res;
            }
			string logInfo = string.Format("roomID: {0}, token: {1}, code: {2}", roomID, token, code);
            GameRTCLog.ReportApiCall("UpdateToken", logInfo);
			return code;
        }

        public int UpdateReceiveRange(string roomID, GameRTCReceiveRange range)
		{
			LogUtils.D("UpdateReceiveRange");
			JsonData param = new JsonData();
			param["roomID"] = roomID;
			param["range"] = JsonMapper.ToJson(range);
			object res = UNBridge.CallSync(GameRTCMethodName.updateReceiveRange, param);
			int code = -1;
			if (res != null)
			{
				code = (int)res;
			}
			string logInfo = string.Format("roomID: {0}, min: {1}, max: {2}, code: {3}", roomID, range.Min, range.Max, code);
            GameRTCLog.ReportApiCall("UpdateReceiveRange", logInfo);
			return code;
		}

		public int UpdatePosition(string roomID, GameRTCPositionInfo pos)
		{
			LogUtils.D("UpdatePostion");
			JsonData param = new JsonData();
			param["roomID"] = roomID;
			param["pos"] = JsonMapper.ToJson(pos);
			object res = UNBridge.CallSync(GameRTCMethodName.updatePosition, param);
			int code = -1;
			if (res != null)
			{
				code = (int)res;
			}
			string logInfo = string.Format("roomID: {0}, x: {1}, y: {2}, z: {3} ,code: {4}", roomID, pos.X, pos.Y, pos.Z, code);
            GameRTCLog.ReportApiCall("UpdatePosition", logInfo);
			return code;
		}

		public int UpdateOrientation(string roomID, GameRTCOrientationInfo info)
		{
			LogUtils.D("UpdatePosition");
			JsonData param = new JsonData();
			param["roomID"] = roomID;
			param["info"] = JsonMapper.ToJson(info);
			object res = UNBridge.CallSync(GameRTCMethodName.updateOrientation, param);
			int code = -1;
			if (res != null)
			{
				code = (int)res;
			}
			string logInfo = string.Format("roomID: {0}, forward: ({1},{2},{3}), right: ({4},{5},{6}), up: ({7},{8},{9}), code: {10}", 
                roomID, info.x_axis_0, info.y_axis_0, info.z_axis_0, info.x_axis_1, info.y_axis_1, info.z_axis_1, info.x_axis_2, info.y_axis_2, info.z_axis_2, code);
			GameRTCLog.ReportApiCall("UpdateOrientation", logInfo);
			return code;
		}

		public int EnableMicrophone(string roomID, bool enable)
		{
			LogUtils.D("EnableMicrophone");
			JsonData param = new JsonData();
			param["roomID"] = roomID;
			param["enable"] = enable;
			object res = UNBridge.CallSync(GameRTCMethodName.enableMicrophone, param);
			int code = -1;
			if (res != null)
			{
				code = (int)res;
			}
			string logInfo = string.Format("roomID: {0}, enable: {1}, code: {2}", roomID, enable, code);
            GameRTCLog.ReportApiCall("EnableMicrophone", logInfo);
			return code;
		}

		public int EnableAudioSend(string roomID, bool enable)
		{
			LogUtils.D("EnableAudioSend");
			JsonData param = new JsonData();
			param["roomID"] = roomID;
			param["enable"] = enable;
			object res = UNBridge.CallSync(GameRTCMethodName.enableAudioSend, param);
			int code = -1;
			if (res != null)
			{
				code = (int)res;
			}
			string logInfo = string.Format("roomID: {0}, enable: {1}, code: {2}", roomID, enable, code);
            GameRTCLog.ReportApiCall("EnableAudioSend", logInfo);
			return code;
		}

		public int EnableSpeakerphone(string roomID, bool enable)
		{
			LogUtils.D("EnableSpeakerphone");
			JsonData param = new JsonData();
			param["roomID"] = roomID;
			param["enable"] = enable;
			object res = UNBridge.CallSync(GameRTCMethodName.enableSpeakerphone, param);
			int code = -1;
			if (res != null)
			{
				code = (int)res;
			}
			string logInfo = string.Format("roomID: {0}, enable: {1}, code: {2}", roomID, enable, code);
            GameRTCLog.ReportApiCall("EnableSpeakerphone", logInfo);
			return code;
		}

		public int EnableAudioReceive(string roomID, string userID, bool enable)
		{
			LogUtils.D("EnableAudioReceive");
			JsonData param = new JsonData();
			param["roomID"] = roomID;
			param["userID"] = userID;
			param["enable"] = enable;
			object res = UNBridge.CallSync(GameRTCMethodName.enableAudioReceive, param);
			int code = -1;
			if (res != null)
			{
				code = (int)res;
			}
			string logInfo = string.Format("roomID: {0}, userID: {1}, enable: {2}, code: {3}", roomID, userID, enable, code);
            GameRTCLog.ReportApiCall("EnableAudioReceive", logInfo);
			return code;
		}

		public int SetRecordingVolume(int volume)
		{
			LogUtils.D("SetRecordingVolume");
			JsonData param = new JsonData();
			param["volume"] = volume;
			object res = UNBridge.CallSync(GameRTCMethodName.setRecordingVolume, param);
			int code = -1;
			if (res != null)
			{
				code = (int)res;
			}
			string logInfo = string.Format("volume: {0}, code: {1}", volume, code);
            GameRTCLog.ReportApiCall("SetRecordingVolume", logInfo);
			return code;
		}

		public int SetPlaybackVolume(int volume)
		{
			LogUtils.D("SetPlaybackVolume");
			JsonData param = new JsonData();
			param["volume"] = volume;
			object res = UNBridge.CallSync(GameRTCMethodName.setPlaybackVolume, param);
			int code = -1;
			if (res != null)
			{
				code = (int)res;
			}
			string logInfo = string.Format("volume: {0}, code: {1}", volume, code);
            GameRTCLog.ReportApiCall("SetPlaybackVolume", logInfo);
			return code;
		}

		public int SetRemoteAudioPlaybackVolume(string roomID, string userID, int volume)
		{
			LogUtils.D("SetRemoteAudioPlaybackVolume");
			JsonData param = new JsonData();
			param["roomID"] = roomID;
			param["userID"] = userID;
			param["volume"] = volume;
			object res = UNBridge.CallSync(GameRTCMethodName.setRemoteAudioPlaybackVolume, param);
			int code = -1;
			if (res != null)
			{
				code = (int)res;
			}
			string logInfo = string.Format("roomID: {0}, userID: {1}, volume: {2}, code: {3}",
                roomID, userID, volume, code);
            GameRTCLog.ReportApiCall("SetRemoteAudioPlaybackVolume", logInfo);
			return code;
		}

		public int SetAudioScenario(AudioScenarioType scenario)
		{
			LogUtils.D("SetAudioScenario");
			JsonData param = new JsonData();
			param["scenario"] = (int)scenario;
			object res = UNBridge.CallSync(GameRTCMethodName.setAudioScenario, param);
			int code = -1;
			if (res != null)
			{
				code = (int)res;
			}
			string logInfo = string.Format("AudioScenarioType: {0}, code: {1}", scenario, code);
            GameRTCLog.ReportApiCall("SetAudioScenario", logInfo);
			return code;
		}

		public int SetAudioProfile(AudioProfileType audioProfile)
		{
			LogUtils.D("SetAudioProfile");
			JsonData param = new JsonData();
			param["audioProfile"] = (int)audioProfile;
			object res = UNBridge.CallSync(GameRTCMethodName.setAudioProfile, param);
			int code = -1;
			if (res != null)
			{
				code = (int)res;
			}
			string logInfo = string.Format("AudioProfile: {0}, code: {1}", audioProfile, code);
			GameRTCLog.ReportApiCall("SetAudioProfile", logInfo);
			return code;
		}

		public int SetVoiceChangerType(VoiceChangerType voiceChanger)
		{
			LogUtils.D("SetVoiceChangerType");
			JsonData param = new JsonData();
			param["voiceChanger"] = (int)voiceChanger;
			object res = UNBridge.CallSync(GameRTCMethodName.setVoiceChangerType, param);
			int code = -1;
			if (res != null)
			{
				code = (int)res;
			}
			string logInfo = string.Format("VoiceChangerType: {0}, code: {1}", voiceChanger, code);
            GameRTCLog.ReportApiCall("SetVoiceChangerType", logInfo);
			return code;
		}

		public int SetVoiceReverbType(VoiceReverbType voiceReverb)
		{
			LogUtils.D("SetVoiceReverbType");
			JsonData param = new JsonData();
			param["voiceReverb"] = (int)voiceReverb;
			object res = UNBridge.CallSync(GameRTCMethodName.setVoiceReverbType, param);
			int code = -1;
			if (res != null)
			{
				code = (int)res;
			}
			string logInfo = string.Format("VoiceReverbType: {0}, code: {1}", voiceReverb, code);
            GameRTCLog.ReportApiCall("SetVoiceReverbType", logInfo);
			return code;
		}

		private void GameRTCListenJoinRoomResult()
		{
			GameRTCCallbackGSDK unCallback = new GameRTCCallbackGSDK();
			unCallback.GameRTCCallback = _instance;
			unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnJoinRoomResultCallback);
			UNBridge.Listen(GameRTCResultName.onJoinRoomResult, unCallback);
		}

		private void GameRTCListenLeaveRoom()
		{
			GameRTCCallbackGSDK unCallback = new GameRTCCallbackGSDK();
			unCallback.GameRTCCallback = _instance;
			unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnLeaveRoomCallback);
			UNBridge.Listen(GameRTCResultName.onLeaveRoom, unCallback);
		}

		private void GameRTCListenUserJoined()
		{
			GameRTCCallbackGSDK unCallback = new GameRTCCallbackGSDK();
			unCallback.GameRTCCallback = _instance;
			unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnUserJoinedCallback);
			UNBridge.Listen(GameRTCResultName.onUserJoined, unCallback);
		}

		private void GameRTCListenUserLeave()
		{
			GameRTCCallbackGSDK unCallback = new GameRTCCallbackGSDK();
			unCallback.GameRTCCallback = _instance;
			unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnUserLeaveCallback);
			UNBridge.Listen(GameRTCResultName.onUserLeave, unCallback);
		}

		private void GameRTCListenMicrophoneEnabled()
		{
			GameRTCCallbackGSDK unCallback = new GameRTCCallbackGSDK();
			unCallback.GameRTCCallback = _instance;
			unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnMicrophoneEnabledCallback);
			UNBridge.Listen(GameRTCResultName.onMicrophoneEnabled, unCallback);
		}

		private void GameRTCListenAudioSendEnabled()
		{
			GameRTCCallbackGSDK unCallback = new GameRTCCallbackGSDK();
			unCallback.GameRTCCallback = _instance;
			unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnAudioSendEnabledCallback);
			UNBridge.Listen(GameRTCResultName.onAudioSendEnabled, unCallback);
		}

		private void GameRTCListenSpeakerphoneEnabled()
		{
			GameRTCCallbackGSDK unCallback = new GameRTCCallbackGSDK();
			unCallback.GameRTCCallback = _instance;
			unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnSpeakerphoneEnabledCallback);
			UNBridge.Listen(GameRTCResultName.onSpeakerphoneEnabled, unCallback);
		}

		private void GameRTCListenAudioVolumeIndication()
		{
			GameRTCCallbackGSDK unCallback = new GameRTCCallbackGSDK();
			unCallback.GameRTCCallback = _instance;
			unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnAudioVolumeIndicationCallback);
			UNBridge.Listen(GameRTCResultName.onAudioVolumeIndication, unCallback);
		}

		private void GameRTCListenConnectionState()
		{
			GameRTCCallbackGSDK unCallback = new GameRTCCallbackGSDK();
			unCallback.GameRTCCallback = _instance;
			unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnConnectionStateChangedCallback);
			UNBridge.Listen(GameRTCResultName.onConnectionStateChanged, unCallback);
		}

		private void GameRTCListenNetworkQuality()
		{
			GameRTCCallbackGSDK unCallback = new GameRTCCallbackGSDK();
			unCallback.GameRTCCallback = _instance;
			unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnNetworkQualityCallback);
			UNBridge.Listen(GameRTCResultName.onNetworkQuality, unCallback);
		}

		private void GameRTCListenRoomWarning()
		{
			GameRTCCallbackGSDK unCallback = new GameRTCCallbackGSDK();
			unCallback.GameRTCCallback = _instance;
			unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnRoomWarningCallback);
			UNBridge.Listen(GameRTCResultName.onRoomWarning, unCallback);
		}

		private void GameRTCListenRoomError()
		{
			GameRTCCallbackGSDK unCallback = new GameRTCCallbackGSDK();
			unCallback.GameRTCCallback = _instance;
			unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnRoomErrorCallback);
			UNBridge.Listen(GameRTCResultName.onRoomError, unCallback);
		}

		private void GameRTCListenEngineWarning()
		{
			GameRTCCallbackGSDK unCallback = new GameRTCCallbackGSDK();
			unCallback.GameRTCCallback = _instance;
			unCallback.OnSuccess = new OnSuccessDelegate(unCallback.OnEngineWarningCallback);
			UNBridge.Listen(GameRTCResultName.onEngineWarning, unCallback);
		}

		#region IGameRTCCallback implementation
		public void OnJoinRoomResult(JoinRoomResult result)
		{
			bool _isRejoined = Convert.ToBoolean(result.isRejoined);
			string logInfo = string.Format("roomID: {0}, userID: {1}, errorCode: {2}, isRejoined: {3}, elapsed: {4}",
                result.roomID, result.userID, result.errorCode, _isRejoined, result.elapsed);
            GameRTCLog.ReportCallback("OnJoinRoomResult", logInfo);
			if (_instance == null) {
                GameRTCLog.LogWarning("GameRTCService is null");
                return;
            }
			if (_instance.OnJoinRoomResultEvent != null)
			{
				Loom.QueueOnMainThread(() =>
				{
					++GameRTCService._roomCount;
					_instance.OnJoinRoomResultEvent(result.roomID, result.userID, result.errorCode, _isRejoined, result.elapsed);
				});
			}
		}

		public void OnLeaveRoom(LeaveRoomResult result)
		{
			string logInfo = string.Format("roomID: {0}", result.roomID);
            GameRTCLog.ReportCallback("OnLeaveRoom", logInfo);
			if (_instance == null)
			{
				GameRTCLog.LogWarning("GameRTCService is null");
                return;
			}
			if (_instance.OnLeaveRoomEvent != null)
			{
				Loom.QueueOnMainThread(() =>
				{
					_instance.OnLeaveRoomEvent(result.roomID);
					--_roomCount;
					if (_instance._initParams.IsLeaveRoomReleaseEngine && GameRTCService._roomCount == 0)
					{
						_instance.Release();
					}
				});
			}
		}

		public void OnUserJoined(UserJoinedResult result)
		{
			string logInfo = string.Format("roomID: {0}, userID: {1}", result.roomID, result.userID);
            GameRTCLog.ReportCallback("OnUserJoined", logInfo);
			if (_instance == null)
			{
				GameRTCLog.LogWarning("GameRTCService is null");
                return;
			}
			if (_instance.OnUserJoinedEvent != null)
			{
				Loom.QueueOnMainThread(() =>
				{
					_instance.OnUserJoinedEvent(result.roomID, result.userID);
				});
			}
		}

		public void OnUserLeave(UserLeaveResult result)
		{
			string logInfo = string.Format("roomID: {0}, userID: {1}, reason: {2}", result.roomID, result.userID, result.reason);
            GameRTCLog.ReportCallback("OnUserLeave", logInfo);
			if (_instance == null)
			{
				GameRTCLog.LogWarning("GameRTCService is null");
                return;
			}
			if (_instance.OnUserLeaveEvent != null)
			{
				Loom.QueueOnMainThread(() =>
				{
					_instance.OnUserLeaveEvent(result.roomID, result.userID, result.reason);
				});
			}
		}

		public void OnMicrophoneEnabled(MicrophoneEnabledResult result)
		{
			bool _enable = Convert.ToBoolean(result.enable);
			string logInfo = string.Format("roomID: {0}, userID: {1}, enable: {2}", result.roomID, result.userID, _enable);
            GameRTCLog.ReportCallback("OnMicrophoneEnabled", logInfo);
			if (_instance == null)
			{
				GameRTCLog.LogWarning("GameRTCService is null");
                return;
			}
			if (_instance.OnMicrophoneEnabledEvent != null)
			{
				Loom.QueueOnMainThread(() =>
				{
					_instance.OnMicrophoneEnabledEvent(result.roomID, result.userID, _enable);
				});
			}
		}

		public void OnAudioSendEnabled(AudioSendEnabledResult result)
		{
			bool _enable = Convert.ToBoolean(result.enable);
			string logInfo = string.Format("roomID: {0}, userID: {1}, enable: {2}", result.roomID, result.userID, _enable);
            GameRTCLog.ReportCallback("OnAudioSendEnabled", logInfo);
			if (_instance == null)
			{
				GameRTCLog.LogWarning("GameRTCService is null");
                return;
			}
			if (_instance.OnAudioSendEnabledEvent != null)
			{
				Loom.QueueOnMainThread(() =>
				{
					_instance.OnAudioSendEnabledEvent(result.roomID, result.userID, _enable);
				});
			}
		}

		public void OnSpeakerphoneEnabled(SpeakerphoneEnabledResult result)
		{
			bool _enable = Convert.ToBoolean(result.enable);
			string logInfo = string.Format("roomID: {0}, userID: {1}, enable: {2}", result.roomID, result.userID, _enable);
            GameRTCLog.ReportCallback("OnSpeakerphoneEnabled", logInfo);
			if (_instance == null)
			{
				GameRTCLog.LogWarning("GameRTCService is null");
                return;
			}
			if (_instance.OnSpeakerphoneEnabledEvent != null)
			{
				Loom.QueueOnMainThread(() =>
				{
					_instance.OnSpeakerphoneEnabledEvent(result.roomID, result.userID, _enable);
				});
			}
		}

		public void OnAudioVolumeIndication(AudioVolumeIndicationResult result)
		{
			if (_instance == null)
			{
				GameRTCLog.LogWarning("GameRTCService is null");
                return;
			}
			if (_instance.OnAudioVolumeIndicationEvent != null)
			{
				Loom.QueueOnMainThread(() =>
				{
					_instance.OnAudioVolumeIndicationEvent(result.roomID, result.speakers, result.totalVolume);
				});
			}
		}

		public void OnConnectionStateChanged(ConnectionStateChangedResult result)
		{
			string logInfo = string.Format("state: {0}", result.state);
            GameRTCLog.ReportCallback("OnConnectionStateChanged", logInfo);
			if (_instance == null)
			{
				GameRTCLog.LogWarning("GameRTCService is null");
                return;
			}
			if (_instance.OnConnectionStateChangedEvent != null)
			{
				Loom.QueueOnMainThread(() =>
				{
					_instance.OnConnectionStateChangedEvent(result.state);
				});
			}
		}

		public void OnNetworkQuality(NetworkQualityResult result)
		{
			if (_instance == null)
			{
				GameRTCLog.LogWarning("GameRTCService is null");
                return;
			}
			if (_instance.OnNetworkQualityEvent != null)
			{
				Loom.QueueOnMainThread(() =>
				{
					_instance.OnNetworkQualityEvent(result.roomID, result.userID, result.txQuality, result.rxQuality);
				});
			}
		}

		public void OnRoomWarning(RoomWarningResult result)
		{
			string logInfo = string.Format("roomID: {0}, warn: {1}", result.roomID, result.warn);
            GameRTCLog.ReportCallback("OnRoomWarning", logInfo);
			if (_instance == null)
			{
				GameRTCLog.LogWarning("GameRTCService is null");
                return;
			}
			if (_instance.OnRoomWarningEvent != null)
			{
				Loom.QueueOnMainThread(() =>
				{
					_instance.OnRoomWarningEvent(result.roomID, result.warn);
				});
			}
		}

		public void OnRoomError(RoomErrorResult result)
		{
			string logInfo = string.Format("roomID: {0}, err: {1}", result.roomID, result.err);
            GameRTCLog.ReportCallback("OnRoomError", logInfo);
			if (_instance == null)
			{
				GameRTCLog.LogWarning("GameRTCService is null");
                return;
			}
			if (_instance.OnRoomErrorEvent != null)
			{
				Loom.QueueOnMainThread(() =>
				{
					_instance.OnRoomErrorEvent(result.roomID, result.err);
				});
			}
		}

		public void OnEngineWarning(EngineWarningResult result)
		{
			string logInfo = string.Format("warn: {0}", result.warn);
            GameRTCLog.ReportCallback("OnEngineWarning", logInfo);
			if (_instance == null)
			{
				GameRTCLog.LogWarning("GameRTCService is null");
                return;
			}
			if (_instance.OnEngineWarningEvent != null)
			{
				Loom.QueueOnMainThread(() =>
				{
					_instance.OnEngineWarningEvent(result.warn);
				});
			}
		}

#if UNITY_STANDALONE_WIN
		public int GetRecordingDeviceCount()
        {
            throw new NotImplementedException();
        }

        public int SetRecordingDevice(string deviceID)
        {
            throw new NotImplementedException();
        }

        public int GetCurrentRecordingDevice(ref string deviceID)
        {
            throw new NotImplementedException();
        }

        public int GetAllRecordingDevices(ref List<AudioDeviceInfo> audioDeviceList)
        {
            throw new NotImplementedException();
        }

        public int GetPlaybackDeviceCount()
        {
            throw new NotImplementedException();
        }

        public int SetPlaybackDevice(string deviceID)
        {
            throw new NotImplementedException();
        }

        public int GetCurrentPlaybackDevice(ref string deviceID)
        {
            throw new NotImplementedException();
        }

        public int GetAllPlaybackDevices(ref List<AudioDeviceInfo> audioDeviceList)
        {
            throw new NotImplementedException();
        }
#endif

#endregion
#endregion
	}
}