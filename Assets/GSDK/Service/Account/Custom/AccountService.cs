using UNBridgeLib.LitJson;


namespace GSDK
{
    public class AccountCustomService : GSDK.CustomAccount.IAccountCustomService
    {
        public event AccountLoginEventHandler LoginEvent;

        private const string AccountLoginWithoutUI = "requestAccountCustomLogin";
        private const string AccountLogout = "requestAccountCustomLogout";
        private const string AccountHasLogined = "requestAccountCustomLoginState";
        private const string AccountGetLoginRecord = "requestAccountCustomUserInfo";
        
        public void LoginWithoutUI(string token)
        {
            GLog.LogDebug("LoginWithoutUI, token:" + token,GSDK.CustomAccount.AccountInnerTools.TAG);
            var callbackHandler = new GSDK.CustomAccount.AccountCallbackHandler {LoginEventHandler = HandleLoginEvent};
            callbackHandler.OnSuccess = callbackHandler.OnLoginEvent;
            var param = new JsonData();
            param["token"] = token;
            UNBridge.Call(AccountLoginWithoutUI, param, callbackHandler);
        }

        private void HandleLoginEvent(Result result)
        {
            var info = GetLoginRecord();
            if (result.IsSuccess)
            {
                GLog.LogDebug(string.Format("HandleLoginEvent Success, result:{0}, info:{1}", result, info),
                    GSDK.CustomAccount.AccountInnerTools.TAG);    
            }
            else
            {
                GLog.LogDebug("HandleLoginEvent failed, result:" + result,GSDK.CustomAccount.AccountInnerTools.TAG);
            }
            if (LoginEvent != null)
            {
                LoginEvent(result, info);
            }
            else
            {
                GLog.LogError("LoginEvent is null",GSDK.CustomAccount.AccountInnerTools.TAG);
            }
        }

        public void Logout()
        {
            GLog.LogDebug("Logout",GSDK.CustomAccount.AccountInnerTools.TAG);
            UNBridge.CallSync(AccountLogout, null);
            GLog.LogDebug("Logout Success",GSDK.CustomAccount.AccountInnerTools.TAG);
        }

        public bool HasLogined
        {
            get
            {
                GLog.LogDebug("HasLogined", GSDK.CustomAccount.AccountInnerTools.TAG);
                var res = UNBridge.CallSync(AccountHasLogined, null);
                var hasLogined = (bool?) res ?? false;
                GLog.LogDebug("HasLogined:" + hasLogined, GSDK.CustomAccount.AccountInnerTools.TAG);
                return hasLogined;
            }
        }

        public AccountInfo GetLoginRecord()
        {
            GLog.LogDebug("GetLoginRecord", GSDK.CustomAccount.AccountInnerTools.TAG);
            object res = UNBridge.CallSync(AccountGetLoginRecord, null);
            AccountInfo accountInfo = new AccountInfo();
            if (res != null)
            {
                var data = JsonMapper.ToObject(JsonMapper.ToJson(res));
                accountInfo.OpenID = data["sdkOpenId"].ToString();
                accountInfo.Token = data["accessToken"].ToString();
            }

            GLog.LogDebug("GetLoginRecord:" + JsonMapper.ToJson(accountInfo), GSDK.CustomAccount.AccountInnerTools.TAG);
            return accountInfo;
        }
    }
}