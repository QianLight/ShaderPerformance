using System;
using GMSDK;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class CdKeyService : ICdKeyService
    {
        #region Variables
        
        private readonly MainSDK _sdk;

        #endregion
        

        #region Methods

        public CdKeyService()
        {
            _sdk = GMSDKMgr.instance.SDK;
        }

        public void RedeemGift(string roleId, int serverId, string cdKey, CdKeyDelegate callback)
        {
            GLog.LogInfo(string.Format("ExchangeGift ,roleId:{0} ,serverId:{1} ,cdKey:{2}",roleId,serverId,cdKey));
            _sdk.SdkGetCDKeyInfo(roleId, serverId.ToString(), cdKey, callbackResult =>
            {
                var result = CdKeyInnerTools.ConvertCdKeyError(callbackResult);
                GLog.LogInfo("Perform CdKeyDelegate, Result====" + result);
                if (callback != null)
                {
                    try
                    {
                        callback(result);
                    }
                    catch (Exception e)
                    {
                        GLog.LogException(e);
                    }
                }
                else
                {
                    GLog.LogWarning("CdKeyDelegate is null");
                }
            });
        }

        #endregion
    }
}