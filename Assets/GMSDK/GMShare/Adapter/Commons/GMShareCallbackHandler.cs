using System;
using UNBridgeLib;
using UNBridgeLib.LitJson;
using UnityEngine;

namespace GMSDK
{
	public class GMShareCallbackHandler : BridgeCallBack
	{
		public Action<GMShareResult> shareCallback;
		public GMShareCallbackHandler() {
			this.OnFailed = new OnFailedDelegate (OnFailCallBack);
			this.OnTimeout = new OnTimeoutDelegate (OnTimeoutCallBack);
		}

		public void OnShareContentCallBack(JsonData jd)
		{
			GMShareResult ret = SdkUtil.ToObject<GMShareResult>(jd.ToJson());
			SdkUtil.InvokeAction<GMShareResult> (shareCallback, ret);
		}
		
		/// <summary>
		/// 失败统一回调，用于调试接口
		/// </summary>
		public void OnFailCallBack(int code, string failMsg) {
			LogUtils.D("接口访问失败 " + code.ToString () + " " + failMsg);
		}
		/// <summary>
		/// 超时统一回调
		/// </summary>
		public void OnTimeoutCallBack() {
			JsonData jd = new JsonData();
			jd ["code"] = -321;
			jd ["message"] = "Share - request time out";
			if (this.OnSuccess != null) {
				this.OnSuccess (jd);
			}
		}
	}
}

