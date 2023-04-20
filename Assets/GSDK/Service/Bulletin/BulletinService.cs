using UNBridgeLib.LitJson;
using GMSDK;
using UNBridgeLib;
using UnityEngine;
using System.Runtime.InteropServices;

namespace GSDK
{
    public class BulletinService : IBulletinService
    {
        public void FetchBulletins(BulletinConfig bulletinConfig, FetchBulletinsDelegate callback)
        {
#if UNITY_STANDALONE_WIN && !GMEnderOn
            FetchBulletinsCallback = callback;
            BulletinFetchBulletins(JsonMapper.ToJson(bulletinConfig), FetchBulletinsFunc);
#else
            Loom.QueueOnMainThread(() =>
            {
                JsonData extraJsonData = null;
                try
                {
                    extraJsonData = bulletinConfig.ExtraInfo == null ? null : JsonMapper.ToObject(bulletinConfig.ExtraInfo);
                }
                catch
                {
                    if (callback != null)
                    {
                        callback(new Result(ErrorCode.BulletinInvalidJsonString, "extraInfo invalid json string"), null);
                    }
                    return;
                }
                GMSDKMgr.instance.SDK.SdkGetBulletins(bulletinConfig.OpenId, bulletinConfig.Language, bulletinConfig.Region, bulletinConfig.Scene, (BulletinRet ret) =>
                    {
                        if (callback != null)
                        {
                            callback(BulletinInnerTools.ConvertBulletinRet(ret), BulletinInnerTools.ConvertGMBulletin(ret.bulletin));
                        }
                    }
                    , bulletinConfig.ServerId, bulletinConfig.ZoneId, bulletinConfig.RoleId, extraJsonData);
            });
#endif
        }

        public FetchBulletinsDelegate FetchBulletinsCallback;

        [MonoPInvokeCallback(typeof(FetchBulletinsDelegate))]
        private static void FetchBulletinsFunc(string resultStr, string callbackStr)
        {
            GLog.LogDebug("callbackstr:" + resultStr);
            Result result = JsonMapper.ToObject<Result>(resultStr);
            BulletinInfo bulletInfo = JsonMapper.ToObject<BulletinInfo>(callbackStr);
            BulletinService service = Bulletin.Service as BulletinService;

            if (service.FetchBulletinsCallback != null)
            {
                service.FetchBulletinsCallback(result, bulletInfo);
            }
        }

        [DllImport(PluginName.GSDK)]
        private static extern void BulletinFetchBulletins(string bulletinConfig, Callback2P callback);

    }
}
