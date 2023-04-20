using System;
using System.Collections.Generic;
using System.Linq;
using GMSDK;
using UNBridgeLib.LitJson;

namespace GSDK
{
    public class ShareService : IShareService
    {
        #region Variables

        private readonly ShareSDK _sdk;
        private ShareDelegate _shareCallback;
        private readonly Dictionary<ShareType, GMShareContentType> _shareTypeDictionary =
            new Dictionary<ShareType, GMShareContentType>()
            {
                {ShareType.Text, GMShareContentType.GMShareContentTypeText},
                {ShareType.Link, GMShareContentType.GMShareContentTypeWebPage},
                {ShareType.Image, GMShareContentType.GMShareContentTypeImage},
                {ShareType.Video, GMShareContentType.GMShareContentTypeVideo},
                {ShareType.ImageAndText, GMShareContentType.GMShareContentTypeImageAndText},
                {ShareType.SuperGroup, GMShareContentType.GMShareContentTypeSuperGroup}
            };

        private readonly Dictionary<ShareSource, GMUGShareSource> _shareSourceDictionary =
            new Dictionary<ShareSource, GMUGShareSource>()
            {
                {ShareSource.Panel, GMUGShareSource.GMUGShareSourceNone},
                {ShareSource.SaveImage,GMUGShareSource.GMUGShareSourceImageShare},
                {ShareSource.System, GMUGShareSource.GMUGShareSourceSystem},
                {ShareSource.QQFriend, GMUGShareSource.GMUGShareSourceQQFriend},
                {ShareSource.QQZone, GMUGShareSource.GMUGShareSourceQQZone},
                {ShareSource.WeChat, GMUGShareSource.GMUGShareSourceWeChatFriend},
                {ShareSource.WeChatMoment, GMUGShareSource.GMUGShareSourceWeChatTimeLine},
                {ShareSource.Awe, GMUGShareSource.GMUGShareSourceAwe},
                {ShareSource.AweIm, GMUGShareSource.GMUGShareSourceAweIM},
                {ShareSource.Weibo, GMUGShareSource.GMUGShareSourceWeibo},
                {ShareSource.WeiboSg,GMUGShareSource.GMUGShareSourceWeiboSuperGroup}
                
            };

        private readonly Dictionary<string, ShareSource> _returnSourceDictionary =
            new Dictionary<string, ShareSource>()
            {
#if UNITY_ANDROID
                {"QQ", ShareSource.QQFriend},
                {"QZONE", ShareSource.QQZone},
                {"WX", ShareSource.WeChat},
                {"WX_TIMELINE", ShareSource.WeChatMoment},
                {"DOUYIN", ShareSource.Awe},
                {"DOUYIN_IM", ShareSource.AweIm},
                {"WEIBO", ShareSource.Weibo},
                {"IMAGE_SHARE",ShareSource.SaveImage},
                {"SYSTEM",ShareSource.System},
#elif UNITY_IOS
                {"qq", ShareSource.QQFriend},
                {"qzone", ShareSource.QQZone},
                {"wechat", ShareSource.WeChat},
                {"moments", ShareSource.WeChatMoment},
                {"douyin", ShareSource.Awe},
                {"douyin_im", ShareSource.AweIm},
                {"weibo", ShareSource.Weibo},
                {"weibo_sg", ShareSource.WeiboSg},
                {"image_share",ShareSource.SaveImage},
                {"sys_share",ShareSource.System},
#endif


            };
#if UNITY_IOS
        private readonly Dictionary<Orientation, int> _orientationDictionary = new Dictionary<Orientation, int>()
        {
            {Orientation.All, 0},
            {Orientation.Landscape, 1},
            {Orientation.Portrait, 2},
        };
#endif

        #endregion

        #region Properties
        
        public bool ShareSilentlyEnabled
        {
            get
            {
                bool result = _sdk.SdkShareBehindEnable();
                GLog.LogInfo(string.Format("ShareSilentlyEnabled:{0}", result));
                return result;
            }
        }

        #endregion

        #region Methods
        
        public ShareService()
        {
            _sdk = GMShareMgr.instance.sSDK;
        }

#if UNITY_IOS
        public void ConfigIOSPanel(bool autoRotate, Orientation orientation)
        {
            GLog.LogInfo(string.Format("AutoRotateSupported:{0}", autoRotate));
            _sdk.SdkSetAutoRotate(autoRotate);
            GLog.LogInfo(string.Format("OrientationSupported:{0}", orientation));
            _sdk.SdkSetSupportOrientation(_orientationDictionary[orientation]);
        }
#endif
        
        public void Share(ShareData shareData, ShareDelegate shareCallback)
        {
            _shareCallback = shareCallback;
            _sdk.SdkSetNeedUsePanel(shareData.ShareSource == ShareSource.Panel);
            GLog.LogInfo(string.Format("Share ,ShareData:{0}", JsonMapper.ToJson(shareData)));
            var thumbImagePath = !string.IsNullOrEmpty(shareData.ImageUrl) ? shareData.ImageUrl : shareData.ImagePath;
            _sdk.SdkShareContent(shareData.Title, shareData.Description, shareData.ImagePath, thumbImagePath,
                shareData.ImageUrl, shareData.WebPageUrl, shareData.VideoUrl, shareData.VideoPath,
                _shareTypeDictionary[shareData.ShareType], _shareSourceDictionary[shareData.ShareSource],
                HandleShareCallback, null, shareData.AweHashtag, shareData.AweExtraJson,
                shareData.WeiboSgName,shareData.WeiboSecName,shareData.WeiboExtParam,shareData.ImageCode);
        }

        public void ShareSilently(ShareSilentlyData shareSilentlyData, ShareDelegate callback)
        {
            _shareCallback = callback;
            GLog.LogInfo(string.Format("ShareSilently ,ShareData:{0}", JsonMapper.ToJson(shareSilentlyData)));
            _sdk.SdkShareContentBehind(shareSilentlyData.VideoUrl, shareSilentlyData.VideoPath, HandleShareCallback,
                shareSilentlyData.Title, shareSilentlyData.AweHashtag, shareSilentlyData.AweExtraJson,
                shareSilentlyData.UseAweAnchor);
        }

        private void HandleShareCallback(GMShareResult gmShareResult)
        {
            if (_shareCallback != null)
            {
                try
                {
                    var result = ShareInnerTools.ConvertShareError(gmShareResult);
                    var channel = _returnSourceDictionary.Keys.Contains(gmShareResult.channel??"")
                        ? _returnSourceDictionary[gmShareResult.channel]
                        : ShareSource.Panel;
                    GLog.LogInfo(string.Format("Perform ShareCallback, channel:{0}, Result===={1}", channel, result));
                    _shareCallback(result, channel);
                }
                catch (Exception e)
                {
                    GLog.LogException(e);
                }
            }
            else
            {
                GLog.LogWarning("ShareCallback is null");
            }
        }
        
        #endregion
    }
}