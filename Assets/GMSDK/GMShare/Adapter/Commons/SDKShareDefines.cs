using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GMSDK {

	public class SDKShareMethodName {
		public const string SDKInit = "registerShare";
		public const string SDKNeedUsePanel = "requestNeedUsePanel";
		public const string SDKShareContent = "requestShareContent";
		public const string SDKShareContentBehind = "requestShareContentBehindWithStatus";
		public const string SDKShareSupportAutoRotate = "requestShareAutoRotate";
		public const string SDKShareSupportOrientation = "requestShareOrientation";
		public const string SDKShareBehindEnable = "requestShareBehindEnable";
	}

	public class SDKShareResultName {
		public const string SDKShareContentResult = "requestShareContentResult";
		public const string SDKShareContentBehindResult = "requestShareContentBehindResult";
		public const string SDKShareContentBehindStatusResult = "requestShareContentBehindStatus";
	}

	public class GMShareResult : CallbackResult {
        public string channel;
    }
	

	public class GMShareBehindStatusResult : CallbackResult
	{
		public GMShareBehindStatus status;
	}

	public enum GMShareBehindStatus
	{
		BeforeDouyinAuthRenew = 1001,
		BeforeFormalShare = 1002,
	}
	
	public enum GMShareContentType {
		GMShareContentTypeText = 0,
		GMShareContentTypeImage = 1,
		GMShareContentTypeWebPage = 3,
		GMShareContentTypeVideo = 4,
		GMShareContentTypeImageAndText = 5,
		GMShareContentTypeSuperGroup = 6
	}

	public enum GMUGShareSource {
		GMUGShareSourceNone = 0,
		/*** 国内支持渠道 ***/
		GMUGShareSourceQQZone, /** 支持：文本，图片，链接 */
		GMUGShareSourceQQFriend, /** 支持：文本，图片，链接 */
		GMUGShareSourceWeChatFriend, /** 支持：文本，图片，链接，视频 */
		GMUGShareSourceWeChatTimeLine, /** 支持：文本，图片，链接，视频 */
		GMUGShareSourceWeibo, /** 支持：文本，图片，链接 */
		GMUGShareSourceAwe, /** 支持：图片，视频 */
		/*** 海外支持渠道 ***/
		GMUGShareSourceFacebook, /** 支持：图片，链接，视频 */
		GMUGShareSourceMessenger, /** 支持：图片，链接 */
		GMUGShareSourceInstagram, /** 支持：图片，视频 */
		GMUGShareSourceWhatsApp, /** 支持：文本，图片，视频 */
		GMUGShareSourceLine, /** 支持: 链接, 文本, 图片, 视频 **/
		GMUGShareSourceKakao, /** 支持: 链接, 文本, 图片, 视频**/
		GMUGShareSourceTiktok, /** 支持: 视频**/
        GMUGShareSourceTwitter, /** 支持: 链接, 文本, 图片, 视频**/
		GMUGShareSourceSystem, /**系统分享**/
		/** 抖音好友 **/
		GMUGShareSourceAweIM, /** 支持: 链接, 图片**/
		GMUGShareSourceVK, /** 支持: 链接, 图片, 文本, 视频（Android）**/
		GMUGShareSourceImageShare,/**支持: 保存图片到本地 **/
		GMUGShareSourceWeiboSuperGroup
		
	}

	public enum GMShareErrorType {
		GMShareErrorTypeNotSupportApp = -1, // 不支持该app分享
		GMShareErrorTypeAppNotInstalled = 1001, // 未安装
		GMShareErrorTypeAppNotSupportAPI, // 不支持的API
		GMShareErrorTypeAppNotSupportShareType, // 不支持的分享类型。
		GMShareErrorTypeInvalidContent, // 分享类型不可用

		GMShareErrorTypeNoTitle, // 缺少title字段。
		GMShareErrorTypeNoWebPageURL, // 缺少url字段
		GMShareErrorTypeNoImage, // 缺少Image字段。
		GMShareErrorTypeNoVideo, // 缺少videoURL字段。

		GMShareErrorTypeUserCancel, // 用户取消
		GMShareErrorTypeNoValidItemInPanel, // 无可用面板

		GMShareErrorTypeExceedMaxVideoSize, // 超出视频最大大小。
		GMShareErrorTypeExceedMaxImageSize, // 超出图片最大大小。
		GMShareErrorTypeExceedMaxTitleSize,  // 超出title最大长度
		GMShareErrorTypeExceedMaxDescSize,  // 超出desc最大长度
		GMShareErrorTypeExceedMaxWebPageURLSize,  // 超出url最大长度
		GMShareErrorTypeExceedMaxFileSize,  // 超出文件最大长度

		GMShareErrorTypeOther, // 其他分享错误

		GMShareErrorTypeDataNull, // 分享数据为空
		GMShareErrorTypeContextNull, //分享context为空
		GMShareErrorTypeIconNull, //分享缩略图为空
		GMShareErrorTypeImageError, // 分享图片错误
		GMShareErrorTypeImageNull, // 分享图片为空
		GMShareErrorTypeFilePathNull, // 分享文件路径为空
		GMShareErrorTypeFileNameNull, // 分享文件名为空
		GMShareErrorTypeNotSupportH5, // 分享不支持h5
		GMShareErrorTypeNotSupporttwTW, // 分享不支持图文
		GMShareErrorTypeNotSupportText, // 分享不支持文本
		GMShareErrorTypeNotSupportImage, // 分享不支持图片
		GMShareErrorTypeNotSupportVideo, //分享不支持视频
		GMShareErrorTypeNotSupportFile, //分享不支持文件
		GMShareErrorTypeLocalImagePathError, // 图片本地路径错误
		GMShareErrorTypeNotSupportImageUrl, // 不支持网络图片地址分享
        GMShareErrorTypeNotSupportAppVersion, // 分享的app版本不支持
        GMShareErrorTypeNotAppInit, // 分享的渠道初始化失败
		GMShareErrorTypeImageFetchFailed, // 图片下载失败
		GMShareErrorTypeVideoTitleEmpty, // 视频title为空
		GMShareErrorTypeTargetUrlEmpty, // 目标url为空
		GMShareErrorTypeVideoFetchFailed, // 视频下载失败
		GMShareErrorTypeBehindAuthError, // 抖音认证授权失败
		GMShareErrorTypeBehindCreateVideo, // 抖音创建视频出错
		GMShareErrorTypeBehindUploadVideo, // 抖音上传视频出错
		GMShareErrorTypeNotToutiaoChannel, // 不是头条渠道
		GMShareErrorTypeNotDouyinLogin // 不是抖音登录

	}
}

