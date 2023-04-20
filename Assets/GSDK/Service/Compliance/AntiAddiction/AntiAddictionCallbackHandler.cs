using GMSDK;
using UNBridgeLib;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class AntiAddictionCallbackHandler : BridgeCallBack
    {
        public IAntiAddictionStatusListener antiAddictionStatusListener = null;
        public AntiAddictionStatusEventHandler antiAddictionStatusCallback;
        
        /// <summary>
        /// 防沉迷状态回调
        /// </summary>
        /// <param name="jd"></param>
        public void OnAntiAddictionStatusChangedCallBack(JsonData jd)
        {
            LogUtils.D("OnAntiAddictionStatusChangedCallBack:" , jd.ToJson());
            AntiAddictionInfo ret = SdkUtil.ToObject<AntiAddictionInfo>(jd.ToJson());
            if (antiAddictionStatusListener != null)
            {
                SdkUtil.InvokeAction<AntiAddictionInfo>(antiAddictionStatusListener.onChangedCallback, ret);
            }
        }

        /// <summary>
        /// 获取本地缓存的防沉迷状态回调
        /// </summary>
        /// <param name="jd"></param>
        public void OnFetchLatestAntiAddictionStatusCallBack(JsonData jd)
        {
            LogUtils.D("OnFetchLatestAntiAddictionStatusCallBack:" , jd.ToJson());
            AntiAddictionInfo ret = SdkUtil.ToObject<AntiAddictionInfo>(jd.ToJson());
            antiAddictionStatusCallback.Invoke(ret);
        }
        
        /// <summary>
        /// 从服务器直接获取最新的防沉迷状态回调
        /// </summary>
        /// <param name="jd"></param>
        public void OnFetchServiceAntiAddictionStatusCallBack(JsonData jd)
        {
            LogUtils.D("OnFetchServiceAntiAddictionStatusCallBack:" , jd.ToJson());
            AntiAddictionInfo ret = SdkUtil.ToObject<AntiAddictionInfo>(jd.ToJson());
            antiAddictionStatusCallback.Invoke(ret);
        }
    }
}
