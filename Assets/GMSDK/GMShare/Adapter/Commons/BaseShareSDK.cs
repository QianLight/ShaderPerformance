using UNBridgeLib;
using System;
using UNBridgeLib.LitJson;

namespace GMSDK
{
	public class ShareSDK
	{

		public ShareSDK(){
			UNBridge.Call (SDKShareMethodName.SDKInit, null);
		}

		/// <summary>
		/// 设置是否使用默认面板
		/// </summary>
		/// <param name="need">使用默认面板</param>
		public void SdkSetNeedUsePanel(bool need) {
			JsonData param = new JsonData ();
			param ["need"] = need;
			UNBridge.Call (SDKShareMethodName.SDKNeedUsePanel, param);
		}
		#if UNITY_IOS
		/// <summary>
		/// 设置面板是否支持旋转（仅iOS）
		/// </summary>
		/// <param name="autoRotate">ture表示可以旋转，默认true</param>
		public void SdkSetAutoRotate(bool autoRotate) {
			JsonData param = new JsonData ();
			param ["supportAutorotate"] = autoRotate;
			UNBridge.Call (SDKShareMethodName.SDKShareSupportAutoRotate, param);
		}
		/// <summary>
		/// 设置面板支持的方向（仅iOS）
		/// </summary>
		/// <param name="orientation">默认全屏（0:全屏，1：横屏，2：竖屏）</param>
		public void SdkSetSupportOrientation(int orientation) {
			JsonData param = new JsonData ();
			param ["supportOrientation"] = orientation;
			UNBridge.Call (SDKShareMethodName.SDKShareSupportOrientation, param);
		}
		#endif

		/// <summary>
		/// 社交分享
		/// </summary>
		/// <param name="title">标题</param>
		/// <param name="desc">文本</param>
		/// <param name="imagePath">图片路径</param>
		/// <param name="thumbImagePath">缩略图路径</param>
		/// <param name="imageUrl">图片链接</param>
		/// <param name="webPageUrl">网页链接</param>
		/// <param name="videoUrl">视频链接</param>
		/// <param name="videoPath">视频路径</param>
		/// <param name="contentType">分享内容类型（文本，图片，链接，视频）</param>
		/// <param name="shareSource">分享渠道信息╭q，微信，微博...）</param>
		/// <param name="aweUniqueId">抖音分享id</param>
		/// <param name="aweHashtag">抖音分享话题</param>
		/// <param name="aweExtraJson">抖音分享id</param>
		/// <param name="weiboSgName">微博超话名称</param>
		/// <param name="weiboSecName">微博超话板块名称</param>
		/// <param name="weiboExtParam">微博超话额外参数</param>
		/// <param name="imageCode">BASE64图片</param>

		public void SdkShareContent(string title, string desc, string imagePath, string thumbImagePath, string imageUrl, string webPageUrl, 
			string videoUrl, string videoPath, GMShareContentType contentType, GMUGShareSource shareSource, Action<GMShareResult> shareCallback, 
			string aweUniqueId = "", string aweHashtag = "", string aweExtraJson = "",string weiboSgName = "",string weiboSecName = "",string weiboExtParam = "",string imageCode = "") {
			GMShareCallbackHandler unCallBack = new GMShareCallbackHandler () {
				shareCallback = shareCallback
			};
			unCallBack.OnSuccess = new OnSuccessDelegate (unCallBack.OnShareContentCallBack);
			UNBridge.Listen (SDKShareResultName.SDKShareContentResult, unCallBack);
			JsonData p = new JsonData();
			if (!String.IsNullOrEmpty(title))
			{
				p["title"] = GetSafeString(title);
			}
			if (!String.IsNullOrEmpty(desc))
			{
				p["desc"] = GetSafeString(desc);
			}
			if (!String.IsNullOrEmpty(imageUrl))
			{
				p["imageUrl"] = GetSafeString(imageUrl);
			}
			if (!String.IsNullOrEmpty(webPageUrl))
			{
				p["webPageUrl"] = GetSafeString(webPageUrl);
			}
			if (!String.IsNullOrEmpty(videoUrl))
			{
				p["videoUrl"] = GetSafeString(videoUrl);
			}
			if (!String.IsNullOrEmpty(videoPath))
			{
				p["videoPath"] = GetSafeString(videoPath);
			}
			p["imagePath"] = GetSafeString(imagePath);
			p["thumbImagePath"] = GetSafeString(thumbImagePath);
			p["contentType"] = (int)contentType;
			p["shareSource"] = (int)shareSource;
			p["aweUniqueId"] = GetSafeString(aweUniqueId);
			p["aweHashtag"] = GetSafeString(aweHashtag);
			p["aweExtraJson"] = GetSafeString(aweExtraJson);
			p["weiboSgName"] = GetSafeString(weiboSgName);
			p["weiboSecName"] = GetSafeString(weiboSecName);
			p["weiboExtParam"] = GetSafeString(weiboExtParam);
			p["imageCode"] = GetSafeString(imageCode);
			
			#if UNITY_IOS
			if (contentType == GMShareContentType.GMShareContentTypeImageAndText)
			{
				p["contentType"] = (int)GMShareContentType.GMShareContentTypeImage;
			}

			if (contentType == GMShareContentType.GMShareContentTypeSuperGroup)
			{
				if (string.IsNullOrEmpty(imagePath) && string.IsNullOrEmpty(imageUrl) && string.IsNullOrEmpty(imageCode))
				{
					p["contentType"] = (int)GMShareContentType.GMShareContentTypeText;
				}
				else
				{
					p["contentType"] = (int)GMShareContentType.GMShareContentTypeImage;
				}
				p["weiboShareType"] = 1;
			}
			#endif
			
			JsonData param = new JsonData ();
			param ["data"] = p;

			UNBridge.Call (SDKShareMethodName.SDKShareContent, param);
		}

		public bool SdkShareBehindEnable(){
			object res = UNBridge.CallSync(SDKShareMethodName.SDKShareBehindEnable, new JsonData());
			return res != null ? (bool)res : false;
		}

	/// <summary>
		/// 静默分享
		/// </summary>
		/// <param name="title">标题</param>
		/// <param name="videoUrl">视频链接</param>
		/// <param name="videoPath">视频路径</param>
		/// <param name="aweHashtag">抖音分享话题</param>
		/// <param name="aweExtraJson">抖音分享锚点需要的json格式id</param>

		public void SdkShareContentBehind(string videoUrl, string videoPath, Action<GMShareResult> callback, string title = "", string aweHashtag = "", string aweExtraJson = "", bool withAnchor = false) {
			GMShareCallbackHandler unCallBack = new GMShareCallbackHandler () {
				shareCallback = callback
			};
			JsonData p = new JsonData ();
			p ["title"] = GetSafeString(title);
			p ["videoUrl"] = GetSafeString(videoUrl);
			p ["videoPath"] = GetSafeString(videoPath);
			p ["aweHashtag"] = GetSafeString(aweHashtag);
			p ["aweExtraJson"] = GetSafeString(aweExtraJson);
			p ["withAnchor"] = withAnchor;

			JsonData param = new JsonData ();
			param ["data"] = p;
			unCallBack.OnSuccess = new OnSuccessDelegate (unCallBack.OnShareContentCallBack);
			UNBridge.Listen (SDKShareResultName.SDKShareContentBehindResult, unCallBack);
			
			UNBridge.Call (SDKShareMethodName.SDKShareContentBehind, param);
		}

		private string GetSafeString(string unSafeString)
		{
			string safeString = string.IsNullOrEmpty(unSafeString) ? "" : unSafeString;
			return safeString;
		}
	}

}
