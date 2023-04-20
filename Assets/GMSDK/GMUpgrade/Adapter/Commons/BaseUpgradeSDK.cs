using UNBridgeLib.LitJson;
using GMSDK;
using System;
using UnityEngine;
using UNBridgeLib;
using System.Collections;
using System.Collections.Generic;
using GSDK;

namespace GMSDK
{
    public class BaseUpgradeSDK
    {
        public BaseUpgradeSDK()
        {
#if UNITY_ANDROID
            UNBridge.Call(UpgradeMethodName.Init, null);
#endif
        }
        private const string UpgradeDownloading = "upgradeDownloadResult";
        private const string OwnRuleUpgradeEvent = "ownRuleUpgradeEvent";
        private const string ContinueExecutionEvent = "continueExecutionEvent";

        public void CheckForceUpgrade(Action<CheckUpgradeResult> callBack, bool withUI = true)
        {
			UpgradeCallbackHandler unCallBack = new UpgradeCallbackHandler(){
				checkUpgradeCallback = callBack
			};
			JsonData param = new JsonData ();
			param ["withUI"] = withUI;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnCheckUpgradeCallback);
			UNBridge.Call (UpgradeMethodName.CheckUpgrade, param, unCallBack);
        }
        
        public void CheckForceUpgradeV2(Action<CheckUpgradeResult> callBack, bool withUI = true)
        {
	        UpgradeCallbackHandler unCallBack = new UpgradeCallbackHandler(){
		        checkUpgradeCallback = callBack
	        };
	        JsonData param = new JsonData ();
	        param ["withUI"] = withUI;
	        unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnCheckUpgradeCallback);
	        UNBridge.Call (UpgradeMethodName.CheckUpgradeV2, param, unCallBack);
        }
		
#if UNITY_ANDROID		
		public void StartCustomUpgrade(Action<DownloadUpdateResult> callBack)
		{
			UpgradeCallbackHandler unCallBack = new UpgradeCallbackHandler(){
				downloadUpgradeCallback = callBack
			};
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnUpgradeDownloadCallback);
			UNBridge.Listen(UpgradeDownloading, unCallBack);
			UNBridge.Call(UpgradeMethodName.StartCustomUpgrade);
		}
		
		public void StartCustomUpgradeV2(Action<DownloadUpdateResult> callBack)
		{
			UpgradeCallbackHandler unCallBack = new UpgradeCallbackHandler(){
				downloadUpgradeCallback = callBack
			};
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnUpgradeDownloadCallback);
			UNBridge.Listen(UpgradeDownloading, unCallBack);
			UNBridge.Call(UpgradeMethodName.StartCustomUpgradeV2);
		}

		public void CancelCustomUpgrade()
		{
			UNBridge.Call(UpgradeMethodName.CancelCustomUpgrade);
		}
		
		public void QueryUpgradeInfoForOwnRule(Action<QueryUpgradeInfoResult> callBack)
		{
			UpgradeCallbackHandler unCallBack = new UpgradeCallbackHandler(){
				queryUpgradeInfoCallback = callBack
			};
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnQueryUpgradeInfoCallback);
			UNBridge.Call(UpgradeMethodName.QueryUpgradeInfoForOwnRule, null, unCallBack);
		}

		public void StartUpgradeForOwnRule(OwnRuleUpgradeInfo ownRuleUpgradeInfo,
			Action<StartUpgradeInfoResult> callBack)
		{
			UpgradeCallbackHandler unCallBack = new UpgradeCallbackHandler(){
				startUpgradeInfoCallback = callBack
			};
			JsonData param = new JsonData();
			JsonData upgradeInfoWrapper = JsonMapper.ToObject(JsonMapper.ToJson(ownRuleUpgradeInfo));
			param ["upgradeInfoWrapper"] = upgradeInfoWrapper;
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnStartUpgradeCallback);
			UNBridge.Listen(OwnRuleUpgradeEvent, unCallBack);
			UNBridge.Call (UpgradeMethodName.StartUpgradeForOwnRule, param);
		}

		public void ContinueExecution(OwnRuleLifeCycle ownRuleLifeCycle, Action<ContinueExecutionResult> callBack)
		{
			UpgradeCallbackHandler unCallBack = new UpgradeCallbackHandler(){
				continueExecutionCallback = callBack
			};
			unCallBack.OnSuccess = new OnSuccessDelegate(unCallBack.OnContinueExecutionCallback);
			JsonData param = new JsonData();
			param ["lifeCycle"] = (int)ownRuleLifeCycle;
			UNBridge.Listen(ContinueExecutionEvent, unCallBack);
			UNBridge.Call(UpgradeMethodName.ContinueExecution, param);
		}

		public void CancelExecution()
		{
			UNBridge.Call(UpgradeMethodName.CancelExecution, null);
		}

		public void RestartApp()
		{
			UNBridge.Call(UpgradeMethodName.RestartApp, null);
		}

		public void OverwriteInstallApk(string apkPath)
		{
			JsonData param = new JsonData ();
			param ["apkPath"] = apkPath;
			UNBridge.Call (UpgradeMethodName.OverwriteInstallApk, param);
		}
		
#endif
    }
}