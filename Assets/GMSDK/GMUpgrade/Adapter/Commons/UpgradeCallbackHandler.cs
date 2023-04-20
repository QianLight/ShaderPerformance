using System;
using UNBridgeLib;
using UNBridgeLib.LitJson;
using UnityEngine;

namespace GMSDK
{
	public class UpgradeCallbackHandler : BridgeCallBack
	{
		public Action<CheckUpgradeResult> checkUpgradeCallback;

		public Action<DownloadUpdateResult> downloadUpgradeCallback;

		public Action<QueryUpgradeInfoResult> queryUpgradeInfoCallback;

		public Action<StartUpgradeInfoResult> startUpgradeInfoCallback;

		public Action<ContinueExecutionResult> continueExecutionCallback;

		public UpgradeCallbackHandler()
		{
			this.OnFailed = new OnFailedDelegate(OnFailCallBack);
			this.OnTimeout = new OnTimeoutDelegate(OnTimeoutCallBack);
		}

		public void OnCheckUpgradeCallback(JsonData data)
		{
			LogUtils.D("onCheckUpgradeCallback");
			CheckUpgradeResult result = SdkUtil.ToObject<CheckUpgradeResult>(data.ToJson());
			SdkUtil.InvokeAction<CheckUpgradeResult> (checkUpgradeCallback, result);
		}

		public void OnUpgradeDownloadCallback(JsonData data)
		{
			LogUtils.D("onUpgradeDownloadCallback");
			DownloadUpdateResult result = SdkUtil.ToObject<DownloadUpdateResult>(data.ToJson());
			SdkUtil.InvokeAction<DownloadUpdateResult>(downloadUpgradeCallback, result);
		}
		
		public void OnQueryUpgradeInfoCallback(JsonData data)
		{
			LogUtils.D("OnQueryUpgradeInfoCallback");
			QueryUpgradeInfoResult result = SdkUtil.ToObject<QueryUpgradeInfoResult>(data.ToJson());
			SdkUtil.InvokeAction<QueryUpgradeInfoResult> (queryUpgradeInfoCallback, result);
		}

		public void OnStartUpgradeCallback(JsonData data)
		{
			LogUtils.D("OnStartUpgradeCallback");
			StartUpgradeInfoResult result = SdkUtil.ToObject<StartUpgradeInfoResult>(data.ToJson());
			SdkUtil.InvokeAction<StartUpgradeInfoResult> (startUpgradeInfoCallback, result);
		}

		public void OnContinueExecutionCallback(JsonData data)
		{
			LogUtils.D("OnContinueExecutionCallback");
			ContinueExecutionResult result = SdkUtil.ToObject<ContinueExecutionResult>(data.ToJson());
			SdkUtil.InvokeAction<ContinueExecutionResult> (continueExecutionCallback, result);
		}

		/// <summary>
		/// 失败统一回调，用于调试接口
		/// </summary>
		public void OnFailCallBack(int code, string failMsg)
		{
			LogUtils.D("接口访问失败 " + code.ToString() + " " + failMsg);
		}
		/// <summary>
		/// 超时统一回调
		/// </summary>
		public void OnTimeoutCallBack()
		{
			JsonData jd = new JsonData();
			jd["code"] = -321;
			jd["message"] = "Upgrade - request time out";
			if (this.OnSuccess != null)
			{
				this.OnSuccess(jd);
			}
		}
	}
}