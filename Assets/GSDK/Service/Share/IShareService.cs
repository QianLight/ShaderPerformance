using System.Text.RegularExpressions;
using UnityEngine.Scripting;

namespace GSDK
{
    #region delegate
    
    /// <summary>
    /// 分享回调
    /// </summary>
    /// <param name="result">回调结果</param>
    /// <para>
    ///    普通分享当可能返回的错误码:
    ///             Success:成功
    ///             ShareUserCancel:用户取消
    ///             ShareAppNotSupported:不支持该三方app分享
    ///             ShareAppNotInstalled:三方app未安装
    ///             ShareAppAPINotSupported:不支持的三方API
    ///             ShareAppNotSupportShareType:三方app不支持该分享类型
    ///             ShareAppVersionNotSupported:该三方app版本不支持
    ///             ShareAppInitFailed:三方app初始化初始化失败
    ///             ShareInvalidContent:不可用的分享内容
    ///             ShareNoTitle:缺少Title字段
    ///             ShareNoWebPageUrl:缺少WebPageUrl字段
    ///             ShareNoImage:缺少Image字段
    ///             ShareNoVideo:缺少VideoPath/VideoUrl字段
    ///             ShareNoValidPanel:没有可用的面板，需要在应用云后台配置
    ///             ShareExceedMaxVideoSize:超出视频最大大小
    ///             ShareExceedMaxImageSize:超出图片最大大小
    ///             ShareExceedMaxTitleSize:超出Title最大长度
    ///             ShareExceedMaxDescriptionSize:超出Description最大长度
    ///             ShareExceedMaxWebPageUrlSize:超出WebPageUrl最大长度
    ///             ShareDataNil:分享数据为空
    ///             ShareContextNil:分享Context为空(Android)
    ///             ShareIconNil:链接分享是的缩略图为空
    ///             ShareImageError:分享图片错误
    ///             ShareVideoNil:分享视频为空
    ///             ShareTextNotSupported:不支持分享文本
    ///             ShareLinkNotSupported:不支持分享链接
    ///             ShareImageNotSupported:不支持分享图片
    ///             ShareVideoNotSupported:不支持分享视频
    ///             ShareLocalImagePathError:本地图片(ImagePath)路径出错
    ///             ShareImageUrlNotSupported:不支持网络图片
    ///             ShareImageDownloadFailed:分享图片下载失败
    ///             ShareVideoTitleNil:分享视频标题为空
    ///             ShareTargetUrlNil:目标链接为空
    ///             ShareVideoDownloadFailed:分享视频下载失败
    ///             ShareUnknownError:未知错误
    ///
    ///     静默分享可能返回的错误码:
    ///             Success:成功
    ///             ShareVideoNil:分享视频为空
    ///             ShareSilentlyAuthError:静默分享授权失败
    ///             ShareSilentlyCreateVideoError:静默分享生成视频失败
    ///             ShareSilentlyUploadError:静默分享上传视频失败
    ///             ShareSilentlyNotTtLogin:静默分享,登录或绑定不是头条渠道
    ///             ShareSilentlyNotAweLogin:静默分享,登录或绑定不是抖音渠道
    ///             ShareUnknownError:未知错误
    /// </para>
    /// <param name="source">最终分享渠道(不包括ShareSource.Panel)</param>
    public delegate void ShareDelegate(Result result, ShareSource source);
    
    #endregion

    /// <summary>
    /// 用于获取实例进行接口调用
    /// e.g.Share.Service.MethodName();
    /// </summary>
    public static class Share
    {
        public static IShareService Service
        {
            get { return ServiceProvider.Instance.GetService(ServiceType.Share) as IShareService; }
        }
    }

    public interface IShareService : IService
    {
        #region Properties
        
        /// <summary>
        /// 是否支持静默分享
        /// </summary>
        bool ShareSilentlyEnabled { get; }

        #endregion

        #region Methods

#if UNITY_IOS
        /// <summary>
        /// 配置iOS面板的属性(iOS如果使用了面板分享，则必接)
        /// </summary>
        /// <param name="isAutoRotated">面板是否支持自动旋转,默认支持(需要设置成和游戏一样，否则使用面板分享时会出现UI问题)</param>
        /// <param name="orientation">面板支持的方向,默认值All(需要设置成和游戏一样，否则使用面板分享时会出现UI问题)</param>
        void ConfigIOSPanel(bool isAutoRotated, Orientation orientation);
#endif
        
        /// <summary>
        /// 分享
        /// </summary>
        /// <param name="shareData">分享数据</param>
        /// <para>
        /// ShareData是分享数据基类，传入该参数时需要根据分享渠道和分享方式进行选择，可以参考下方ShareData Constructor部分定义的数据类
        /// e.g. new QQShare.Text(),new AweShare.Image()
        /// </para>
        /// <param name="shareCallback">分享回调</param>
        void Share(ShareData shareData, ShareDelegate shareCallback);
        /// <summary>
        /// 静默分享(不会拉起三方app,用户无感知)
        /// </summary>
        /// <param name="shareSilentlyData">静默分享数据</param>
        /// <para>
        /// ShareSilentlyData是静默分享数据基类，传入该参数时需要根据分享渠道和分享方式进行选择，可以参考下方ShareSilentlyData Constructor部分定义的数据类
        /// e.g. new AweShareSilently.Video()
        /// </para>
        /// <param name="callback">分享回调</param>
        void ShareSilently(ShareSilentlyData shareSilentlyData, ShareDelegate callback);

        #endregion
    }

    #region PublicDefinitions ShareService使用的所有定义

    public abstract class ShareData
    {
        #region Common Params

        /// <summary>
        /// 标题
        /// </summary>
        public string Title; 
        /// <summary>
        /// 描述内容
        /// </summary>
        public string Description; 
        /// <summary>
        /// 网页Url
        /// </summary>
        public string WebPageUrl; 
        /// <summary>
        /// 本地图片路径
        /// </summary>
        public string ImagePath; 
        /// <summary>
        /// 图片Url
        /// </summary>
        public string ImageUrl;

        public string ImageCode;
        /// <summary>
        /// 本地视频路径
        /// </summary>
        public string VideoPath; 
        /// <summary>
        /// 视频Url
        /// </summary>
        public string VideoUrl; 

        #endregion

        #region Awe and Tiktok Params

        /// <summary>
        /// 抖音分享话题
        /// </summary>
        public string AweHashtag; 
        /// <summary>
        /// 抖音锚点分享使用的json字符串
        /// </summary>
        public string AweExtraJson; 

        #endregion

        #region Weibo Super Group Params
        
        /// <summary>
        /// 微博超话名称
        /// </summary>
        public string WeiboSgName;

        /// <summary>
        /// 微博超话板块名称
        /// </summary>
        public string WeiboSecName;

        /// <summary>
        /// 微博超话额外参数
        /// </summary>
        public string WeiboExtParam;

        #endregion
      

        #region 使用sdk提供的构造函数时无需关注

        /// <summary>
        /// 分享类型
        /// </summary>
        public ShareType ShareType { get; set; } 
        /// <summary>
        /// 分享渠道
        /// </summary>
        public ShareSource ShareSource { get; set; } 

        #endregion

        #region Method

        /// <summary>
        /// 用于判断字符串是否是Url
        /// </summary>
        protected bool IsUrl(string testString)
        {
            const string regexString = @"https*://";
            var regex = new Regex(regexString);
            return regex.IsMatch(testString);
        }

        #endregion
        
    }

    #region ShareData Constructor

    public static class SystemShare
    {
        public class Text : ShareData
        {
            /// <summary>
            /// 文本分享
            /// </summary>
            /// <param name="title">分享的文本内容</param>
#if UNITY_2018_3_OR_NEWER
            [Preserve]
#endif
            public Text(string title)
            {
                ShareType = ShareType.Text;
                ShareSource = ShareSource.System;
                Title = title;
            }
        }

        public class Link : ShareData
        {
            /// <summary>
            /// 链接分享
            /// </summary>
            /// <param name="title">标题</param>
            /// <param name="description">描述</param>
            /// <param name="webPageUrl">网页Url</param>
            /// <param name="imagePath">本地图片路径</param>
            /// <param name="imageUrl">图片Url</param>
#if UNITY_2018_3_OR_NEWER
            [Preserve]
#endif
            public Link(string title, string description, string webPageUrl, string imagePath, string imageUrl)
            {
                ShareType = ShareType.Link;
                ShareSource = ShareSource.System;
                Title = title;
                Description = description;
                WebPageUrl = webPageUrl;
                ImagePath = imagePath;
                ImageUrl = imageUrl;
            }
        }

        public class Image : ShareData
        {
            /// <summary>
            /// 图片分享
            /// </summary>
            /// <param name="imagePathOrUrl">本地图片路径或Url</param>
            /// <param name="imageCode">BASE64图片</param>
#if UNITY_2018_3_OR_NEWER
            [Preserve]
#endif
            public Image(string imagePathOrUrl,string imageCode = "")
            {
                ShareType = ShareType.Image;
                ShareSource = ShareSource.System;
                if (!IsUrl(imagePathOrUrl))
                {
                    ImagePath = imagePathOrUrl;
                }
                else
                {
                    ImageUrl = imagePathOrUrl;
                }

                ImageCode = imageCode;
            }
        }

        public class Video : ShareData
        {
            /// <summary>
            /// 视频分享
            /// </summary>
            /// <param name="videoUrl">视频Url</param>
            /// <param name="aweHashtag">可选参数，抖音分享话题</param>
            /// <param name="aweExtraJson">可选参数，抖音锚点分享使用的json字符串</param>
#if UNITY_2018_3_OR_NEWER
            [Preserve]
#endif
            public Video(string videoPath, string videoUrl,
                string aweHashtag = "", string aweExtraJson = "")
            {
                ShareType = ShareType.Video;
                ShareSource = ShareSource.System;
                VideoPath = videoPath;
                VideoUrl = videoUrl;
                AweHashtag = aweHashtag;
                AweExtraJson = aweExtraJson;
            }
        }
    }

    public static class SaveImageShare
    {
        public class Image : ShareData
        {
            /// <summary>
            /// 图片分享
            /// </summary>
            /// <param name="imagePathOrUrl">本地图片路径或Url</param>
            /// <param name="imageCode">BASE64图片</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Image(string imagePathOrUrl,string imageCode = "")
            {
                ShareType = ShareType.Image;
                ShareSource = ShareSource.SaveImage;
                if (!IsUrl(imagePathOrUrl))
                {
                    ImagePath = imagePathOrUrl;
                }
                else
                {
                    ImageUrl = imagePathOrUrl;
                }

                ImageCode = imageCode;
            }
        }
    }

    /// <summary>
    /// 面板分享，可以包含以下所有分享方式，面板需要在应用云平台配置，面板Id需要在GMSDK/Config Settings中配置
    /// </summary>
    public static class PanelShare
    {
        public class Text : ShareData
        {
            /// <summary>
            /// 文本分享
            /// </summary>
            /// <param name="title">分享的文本内容</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Text(string title)
            {
                ShareType = ShareType.Text;
                ShareSource = ShareSource.Panel;
                Title = title;
            }
        }

        public class Link : ShareData
        {
            /// <summary>
            /// 链接分享
            /// </summary>
            /// <param name="title">标题</param>
            /// <param name="description">描述</param>
            /// <param name="webPageUrl">网页Url</param>
            /// <param name="imagePath">本地图片路径</param>
            /// <param name="imageUrl">图片Url</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Link(string title, string description, string webPageUrl, string imagePath, string imageUrl)
            {
                ShareType = ShareType.Link;
                ShareSource = ShareSource.Panel;
                Title = title;
                Description = description;
                WebPageUrl = webPageUrl;
                ImagePath = imagePath;
                ImageUrl = imageUrl;
            }
        }

        public class Image : ShareData
        {
            /// <summary>
            /// 图片分享
            /// </summary>
            /// <param name="imagePathOrUrl">本地图片路径或Url</param>
            /// <param name="imageCode">BASE64图片</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Image(string imagePathOrUrl,string imageCode = "")
            {
                ShareType = ShareType.Image;
                ShareSource = ShareSource.Panel;
                if (!IsUrl(imagePathOrUrl))
                {
                    ImagePath = imagePathOrUrl;
                }
                else
                {
                    ImageUrl = imagePathOrUrl;
                }

                ImageCode = imageCode;
            }
        }

        public class Video : ShareData
        {
            /// <summary>
            /// 视频分享
            /// </summary>
            /// <param name="title">标题</param>
            /// <param name="description">描述</param>
            /// <param name="imagePath">本地图片路径</param>
            /// <param name="videoPath">本地视频路径</param>
            /// <param name="videoUrl">视频Url</param>
            /// <param name="aweHashtag">可选参数，抖音分享话题</param>
            /// <param name="aweExtraJson">可选参数，抖音锚点分享使用的json字符串</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Video(string title, string description, string imagePath, string videoPath, string videoUrl,
                string aweHashtag = "", string aweExtraJson = "")
            {
                ShareType = ShareType.Video;
                ShareSource = ShareSource.Panel;
                Title = title;
                Description = description;
                ImagePath = imagePath;
                VideoPath = videoPath;
                VideoUrl = videoUrl;
                AweHashtag = aweHashtag;
                AweExtraJson = aweExtraJson;
            }
        }
        
        public class ImageAndText : ShareData
        {
            /// <summary>
            /// 微博超话分享
            /// </summary>
            /// <param name="imagePathOrUrl">本地图片路径或Url</param>
            /// <param name="title">标题</param>
            /// <param name="imageCode">BASE64图片</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public ImageAndText(string imagePathOrUrl, string title, string imageCode)
            {
                ShareType = ShareType.ImageAndText;
                ShareSource = ShareSource.Panel;
                Title = title;
                if (!IsUrl(imagePathOrUrl))
                {
                    ImagePath = imagePathOrUrl;
                }
                else
                {
                    ImageUrl = imagePathOrUrl;
                }

                ImageCode = imageCode;
            }
        }
        
        public class SuperGroup : ShareData
        {
            /// <summary>
            /// 微博超话分享
            /// </summary>
            /// <param name="title">标题</param>
            /// <param name="imagePathOrUrl">本地图片路径或Url</param>
            /// <param name="weiboSgName">微博超话名称</param>
            /// <param name="weiboSecName">微博超话板块名称</param>
            /// <param name="weiboExtParam">微博超话额外参数</param>
            /// <param name="imageCode">BASE64图片</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public SuperGroup(string title,string imagePathOrUrl,string weiboSgName,string weiboSecName, string weiboExtParam,string imageCode = "")
            {
                ShareType = ShareType.SuperGroup;
                ShareSource = ShareSource.Panel;
                Title = title;
                WeiboSgName = weiboSgName;
                WeiboSecName = weiboSecName;
                WeiboExtParam = weiboExtParam;
                if (!IsUrl(imagePathOrUrl))
                {
                    ImagePath = imagePathOrUrl;
                }
                else
                {
                    ImageUrl = imagePathOrUrl;
                }

                ImageCode = imageCode;
            }
        }
    }

    public static class QQFriendShare
    {
        public class Text : ShareData
        {
            /// <summary>
            /// 文本分享
            /// </summary>
            /// <param name="title">分享的文本内容</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Text(string title)
            {
                ShareType = ShareType.Text;
                ShareSource = ShareSource.QQFriend;
                Title = title;
            }
        }

        public class Link : ShareData
        {
            /// <summary>
            /// 链接分享
            /// </summary>
            /// <param name="title">标题</param>
            /// <param name="description">描述</param>
            /// <param name="webPageUrl">网页Url</param>
            /// <param name="imagePathOrUrl">本地图片路径或Url</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Link(string title, string description, string webPageUrl, string imagePathOrUrl)
            {
                ShareType = ShareType.Link;
                ShareSource = ShareSource.QQFriend;
                Title = title;
                Description = description;
                WebPageUrl = webPageUrl;
                if (!IsUrl(imagePathOrUrl))
                {
                    ImagePath = imagePathOrUrl;
                }
                else
                {
                    ImageUrl = imagePathOrUrl;
                }

            }
        }

        public class Image : ShareData
        {
            /// <summary>
            /// 图片分享
            /// </summary>
            /// <param name="imagePathOrUrl">本地图片路径或Url</param>
            /// <param name="imageCode">BASE64图片</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Image(string imagePathOrUrl,string imageCode = "")
            {
                ShareType = ShareType.Image;
                ShareSource = ShareSource.QQFriend;
                if (!IsUrl(imagePathOrUrl))
                {
                    ImagePath = imagePathOrUrl;
                }
                else
                {
                    ImageUrl = imagePathOrUrl;
                }

                ImageCode = imageCode;
            }
        }

        public class Video : ShareData
        {
#if UNITY_ANDROID
            /// <summary>
            /// 视频分享
            /// </summary>
            /// <param name="videoPathOrUrl">本地视频路径或Url</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Video(string videoPathOrUrl)
            {
                ShareType = ShareType.Video;
                ShareSource = ShareSource.QQFriend;
                if (!IsUrl(videoPathOrUrl))
                {
                    VideoPath = videoPathOrUrl;
                }
                else
                {
                    VideoUrl = videoPathOrUrl;
                }
            }
#endif
#if UNITY_IOS
            /// <summary>
            /// 视频分享
            /// </summary>
            /// <param name="title">标题</param>
            /// <param name="description">描述</param>
            /// <param name="imagePath">本地图片路径</param>
            /// <param name="videoUrl">视频Url</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Video(string title,string description,string imagePath,string videoUrl)
            {
                ShareType = ShareType.Video;
                ShareSource = ShareSource.QQFriend;
                Title = title;
                Description = description;
                ImagePath = imagePath;
                VideoUrl = videoUrl;
            }
#endif
        }
    }

    public static class QQZoneShare
    {
#if UNITY_ANDROID
        public class Text : ShareData
        {
            /// <summary>
            /// 文本分享
            /// </summary>
            /// <param name="title">分享的文本内容</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Text(string title)
            {
                ShareType = ShareType.Text;
                ShareSource = ShareSource.QQZone;
                Title = title;
            }
        }
#endif

        public class Link : ShareData
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="title">标题</param>
            /// <param name="description">描述</param>
            /// <param name="webPageUrl">网页Url</param>
            /// <param name="imagePathOrUrl">本地图片路径或Url</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Link(string title, string description, string webPageUrl, string imagePathOrUrl)
            {
                ShareType = ShareType.Link;
                ShareSource = ShareSource.QQZone;
                Title = title;
                Description = description;
                WebPageUrl = webPageUrl;
                if (!IsUrl(imagePathOrUrl))
                {
                    ImagePath = imagePathOrUrl;
                }
                else
                {
                    ImageUrl = imagePathOrUrl;
                }

            }
        }

        public class Image : ShareData
        {
            /// <summary>
            /// 图片分享
            /// </summary>
            /// <param name="imagePathOrUrl">本地图片路径或Url</param>
            /// <param name="imageCode">BASE64图片</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Image(string imagePathOrUrl,string imageCode = "")
            {
                ShareType = ShareType.Image;
                ShareSource = ShareSource.QQZone;
                if (!IsUrl(imagePathOrUrl))
                {
                    ImagePath = imagePathOrUrl;
                }
                else
                {
                    ImageUrl = imagePathOrUrl;
                }

                ImageCode = imageCode;
            }
        }

        public class Video : ShareData
        {
#if UNITY_ANDROID
            /// <summary>
            /// 视频分享
            /// </summary>
            /// <param name="videoPathOrUrl">本地视频路径或Url</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Video(string videoPathOrUrl)
            {
                ShareType = ShareType.Video;
                ShareSource = ShareSource.QQZone;
                if (!IsUrl(videoPathOrUrl))
                {
                    VideoPath = videoPathOrUrl;
                }
                else
                {
                    VideoUrl = videoPathOrUrl;
                }
            }
#endif
#if UNITY_IOS
            /// <summary>
            /// 视频分享
            /// </summary>
            /// <param name="title">标题</param>
            /// <param name="description">描述</param>
            /// <param name="imagePath">本地图片路径</param>
            /// <param name="videoUrl">视频Url</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Video(string title,string description,string imagePath,string videoUrl)
            {
                ShareType = ShareType.Video;
                ShareSource = ShareSource.QQZone;
                Title = title;
                Description = description;
                ImagePath = imagePath;
                VideoUrl = videoUrl;
            }
#endif
        }
    }

    public static class WeChatShare
    {
        public class Text : ShareData
        {
            /// <summary>
            /// 文本分享
            /// </summary>
            /// <param name="title">分享的文本内容</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Text(string title)
            {
                ShareType = ShareType.Text;
                ShareSource = ShareSource.WeChat;
                Title = title;
            }
        }

        public class Link : ShareData
        {
            /// <summary>
            /// 链接分享
            /// </summary>
            /// <param name="title">标题</param>
            /// <param name="description">描述</param>
            /// <param name="webPageUrl">网页Url</param>
            /// <param name="imagePathOrUrl">本地图片路径或Url</param>
            /// <param name="imageCode">BASE64图片</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Link(string title, string description, string webPageUrl, string imagePathOrUrl)
            {
                ShareType = ShareType.Link;
                ShareSource = ShareSource.WeChat;
                Title = title;
                Description = description;
                WebPageUrl = webPageUrl;
                if (!IsUrl(imagePathOrUrl))
                {
                    ImagePath = imagePathOrUrl;
                }
                else
                {
                    ImageUrl = imagePathOrUrl;
                }

            }
        }

        public class Image : ShareData
        {
            /// <summary>
            /// 图片分享
            /// </summary>
            /// <param name="imagePathOrUrl">本地图片路径或Url</param>
            /// <param name="imageCode">BASE64图片</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Image(string imagePathOrUrl,string imageCode = "")
            {
                ShareType = ShareType.Image;
                ShareSource = ShareSource.WeChat;
                if (!IsUrl(imagePathOrUrl))
                {
                    ImagePath = imagePathOrUrl;
                }
                else
                {
                    ImageUrl = imagePathOrUrl;
                }

                ImageCode = imageCode;
            }
        }

        public class Video : ShareData
        {
#if UNITY_ANDROID
            /// <summary>
            /// 视频分享
            /// </summary>
            /// <param name="videoPathOrUrl">本地视频路径或Url</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Video(string videoPathOrUrl)
            {
                ShareType = ShareType.Video;
                ShareSource = ShareSource.WeChat;
                if (!IsUrl(videoPathOrUrl))
                {
                    VideoPath = videoPathOrUrl;
                }
                else
                {
                    VideoUrl = videoPathOrUrl;
                }
            }
#endif
#if UNITY_IOS
            /// <summary>
            /// 视频分享
            /// </summary>
            /// <param name="title">标题</param>
            /// <param name="description">描述</param>
            /// <param name="imagePath">本地图片路径</param>
            /// <param name="videoUrl">视频Url</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Video(string title,string description,string imagePath,string videoUrl)
            {
                ShareType = ShareType.Video;
                ShareSource = ShareSource.WeChat;
                Title = title;
                Description = description;
                ImagePath = imagePath;
                VideoUrl = videoUrl;
            }
#endif
        }
    }

    public static class WeChatMomentShare
    {
        public class Text : ShareData
        {
            /// <summary>
            /// 文本分享
            /// </summary>
            /// <param name="title">分享的文本内容</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Text(string title)
            {
                ShareType = ShareType.Text;
                ShareSource = ShareSource.WeChatMoment;
                Title = title;
            }
        }

        public class Link : ShareData
        {
            /// <summary>
            /// 链接分享
            /// </summary>
            /// <param name="title">标题</param>
            /// <param name="webPageUrl">网页Url</param>
            /// <param name="imagePathOrUrl">本地图片路径或Url</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Link(string title, string webPageUrl, string imagePathOrUrl)
            {
                ShareType = ShareType.Link;
                ShareSource = ShareSource.WeChatMoment;
                Title = title;
                WebPageUrl = webPageUrl;
                if (!IsUrl(imagePathOrUrl))
                {
                    ImagePath = imagePathOrUrl;
                }
                else
                {
                    ImageUrl = imagePathOrUrl;
                }

            }
        }

        public class Image : ShareData
        {
            /// <summary>
            /// 图片分享
            /// </summary>
            /// <param name="imagePathOrUrl">本地图片路径或Url</param>
            /// <param name="imageCode">BASE64图片</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Image(string imagePathOrUrl,string imageCode = "")
            {
                ShareType = ShareType.Image;
                ShareSource = ShareSource.WeChatMoment;
                if (!IsUrl(imagePathOrUrl))
                {
                    ImagePath = imagePathOrUrl;
                }
                else
                {
                    ImageUrl = imagePathOrUrl;
                }

                ImageCode = imageCode;
            }
        }

#if UNITY_IOS
        public class Video:ShareData
        {
            /// <summary>
            /// 视频分享
            /// </summary>
            /// <param name="title">标题</param>
            /// <param name="description">描述</param>
            /// <param name="imagePath">本地图片路径</param>
            /// <param name="videoUrl">视频Url</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Video(string title,string description,string imagePath,string videoUrl)
            {
                ShareType = ShareType.Video;
                ShareSource = ShareSource.WeChatMoment;
                Title = title;
                Description = description;
                ImagePath = imagePath;
                VideoUrl = videoUrl;
            }
        }
#endif
    }

    public static class AweShare
    {
        public class Image : ShareData
        {
            /// <summary>
            /// 图片分享
            /// </summary>
            /// <param name="imagePathOrUrl">本地图片路径或Url</param>
            /// <param name="imageCode">BASE64图片</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Image(string imagePathOrUrl,string imageCode = "")
            {
                ShareType = ShareType.Image;
                ShareSource = ShareSource.Awe;
                if (!IsUrl(imagePathOrUrl))
                {
                    ImagePath = imagePathOrUrl;
                }
                else
                {
                    ImageUrl = imagePathOrUrl;
                }

                ImageCode = imageCode;
            }
        }

        public class Video : ShareData
        {
            /// <summary>
            /// 
            /// </summary>
            /// <param name="videoPathOrUrl">本地视频路径或Url</param>
            /// <param name="aweHashtag">可选参数，抖音分享话题</param>
            /// <param name="aweExtraJson">可选参数，抖音锚点分享使用的json字符串</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Video(string videoPathOrUrl, string aweHashtag = "", string aweExtraJson = "")
            {
                ShareType = ShareType.Video;
                ShareSource = ShareSource.Awe;
                if (!IsUrl(videoPathOrUrl))
                {
                    VideoPath = videoPathOrUrl;
                }
                else
                {
                    VideoUrl = videoPathOrUrl;
                }

                AweHashtag = aweHashtag;
                AweExtraJson = aweExtraJson;
            }
        }
    }

    public static class AweImShare
    {
        public class Link : ShareData
        {
            /// <summary>
            /// 链接分享
            /// </summary>
            /// <param name="title">标题</param>
            /// <param name="description">描述</param>
            /// <param name="webPageUrl">网页Url</param>
            /// <param name="imageUrl">图片Url</param>
#if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Link(string title, string description, string webPageUrl, string imageUrl)
            {
                ShareType = ShareType.Link;
                ShareSource = ShareSource.AweIm;
                Title = title;
                Description = description;
                WebPageUrl = webPageUrl;
                ImageUrl = imageUrl;
            }
        }

        public class Image : ShareData
        {
            /// <summary>
            /// 图片分享
            /// </summary>
            /// <param name="imagePathOrUrl">本地图片路径或Url</param>
            /// <param name="imageCode">BASE64图片</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Image(string imagePathOrUrl,string imageCode = "")
            {
                ShareType = ShareType.Image;
                ShareSource = ShareSource.AweIm;
                if (!IsUrl(imagePathOrUrl))
                {
                    ImagePath = imagePathOrUrl;
                }
                else
                {
                    ImageUrl = imagePathOrUrl;
                }

                ImageCode = imageCode;
            }
        }
    }

    public static class WeiboShare
    {
        public class Text : ShareData
        {
            /// <summary>
            /// 文本分享
            /// </summary>
            /// <param name="title">分享的文本内容</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Text(string title)
            {
                ShareType = ShareType.Text;
                ShareSource = ShareSource.Weibo;
                Title = title;
            }
        }

        public class Link : ShareData
        {
#if UNITY_ANDROID
            /// <summary>
            /// 链接分享
            /// </summary>
            /// <param name="title">标题</param>
            /// <param name="description">描述</param>
            /// <param name="webPageUrl">网页Url</param>
            /// <param name="imagePathOrUrl">本地图片路径或Url</param>
            /// <param name="imageCode">BASE64图片</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Link(string title, string description, string webPageUrl, string imagePathOrUrl)
            {
                ShareType = ShareType.Link;
                ShareSource = ShareSource.Weibo;
                Title = title;
                Description = description;
                WebPageUrl = webPageUrl;
                if (!IsUrl(imagePathOrUrl))
                {
                    ImagePath = imagePathOrUrl;
                }
                else
                {
                    ImageUrl = imagePathOrUrl;
                }

            }
#endif
#if UNITY_IOS
            /// <summary>
            /// 链接分享
            /// </summary>
            /// <param name="title">标题</param>
            /// <param name="description">描述</param>
            /// <param name="webPageUrl">网页Url</param>
            /// <param name="imagePath">本地图片路径</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Link(string title,string description,string webPageUrl,string imagePath)
            {
                ShareType = ShareType.Link;
                ShareSource = ShareSource.Weibo;
                Title = title;
                Description = description;
                WebPageUrl = webPageUrl;
                ImagePath = imagePath;
            }
#endif
        }

        public class Image : ShareData
        {
            /// <summary>
            /// 图片分享
            /// </summary>
            /// <param name="imagePathOrUrl">本地图片路径或Url</param>
            /// <param name="imageCode">BASE64图片</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Image(string imagePathOrUrl,string imageCode = "")
            {
                ShareType = ShareType.Image;
                ShareSource = ShareSource.Weibo;
                if (!IsUrl(imagePathOrUrl))
                {
                    ImagePath = imagePathOrUrl;
                }
                else
                {
                    ImageUrl = imagePathOrUrl;
                }

                ImageCode = imageCode;
            }
        }
        
        public class ImageAndText : ShareData
        {
            /// <summary>
            /// 微博超话分享
            /// </summary>
            /// <param name="imagePathOrUrl">本地图片路径或Url</param>
            /// <param name="title">标题</param>
            /// <param name="imageCode">BASE64图片</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public ImageAndText(string imagePathOrUrl, string title,string imageCode = "")
            {
                ShareType = ShareType.ImageAndText;
                ShareSource = ShareSource.Weibo;
                Title = title;
                if (!IsUrl(imagePathOrUrl))
                {
                    ImagePath = imagePathOrUrl;
                }
                else
                {
                    ImageUrl = imagePathOrUrl;
                }

                ImageCode = imageCode;
            }
        }
        
        public class SuperGroup : ShareData
        {
            /// <summary>
            /// 微博超话分享
            /// </summary>
            /// <param name="title">标题</param>
            /// <param name="imagePathOrUrl">本地图片路径或Url</param>
            /// <param name="weiboSgName">微博超话名称</param>
            /// <param name="weiboSecName">微博超话板块名称</param>
            /// <param name="weiboExtParam">微博超话额外参数</param>
            /// <param name="imageCode">BASE64图片</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public SuperGroup(string title,string imagePathOrUrl,string weiboSgName,string weiboSecName, string weiboExtParam,string imageCode = "")
            {
                ShareType = ShareType.SuperGroup;
                ShareSource = ShareSource.WeiboSg;
                Title = title;
                WeiboSgName = weiboSgName;
                WeiboSecName = weiboSecName;
                WeiboExtParam = weiboExtParam;
                if (!IsUrl(imagePathOrUrl))
                {
                    ImagePath = imagePathOrUrl;
                }
                else
                {
                    ImageUrl = imagePathOrUrl;
                }

                ImageCode = imageCode;
            }
        }
    }

    #endregion

    public abstract class ShareSilentlyData : ShareData
    {
        #region Awe Params

        public bool UseAweAnchor; //是否使用抖音锚点

        #endregion
    }

    #region ShareSilently Constructor

    /// <summary>
    /// 抖音静默分享
    /// </summary>
    public static class AweShareSilently
    {
        public class Video : ShareSilentlyData
        {
            /// <summary>
            /// 视频分享
            /// </summary>
            /// <param name="title">标题</param>
            /// <param name="videoPathOrUrl">本地视频路径或Url</param>
            /// <param name="aweHashtag">可选参数，抖音分享话题</param>
            /// <param name="useAweAnchor">可选参数，是否启用抖音锚点分享</param>
            /// <param name="aweExtraJson">可选参数，抖音锚点分享使用的json字符串</param>
            #if UNITY_2018_3_OR_NEWER
            [Preserve]
            #endif
            public Video(string title, string videoPathOrUrl, string aweHashtag = "",
                bool useAweAnchor = false, string aweExtraJson = "")
            {
                ShareType = ShareType.Video;
                ShareSource = ShareSource.Awe;
                Title = title;
                if (!IsUrl(videoPathOrUrl))
                {
                    VideoPath = videoPathOrUrl;
                }
                else
                {
                    VideoUrl = videoPathOrUrl;
                }

                AweHashtag = aweHashtag;
                UseAweAnchor = useAweAnchor;
                AweExtraJson = aweExtraJson;
            }
        }
    }

    #endregion


    public enum ShareType
    {
        /// <summary>
        /// 文本类型
        /// </summary>
        Text = 1,

        /// <summary>
        /// 链接类型
        /// </summary>
        Link = 2,

        /// <summary>
        /// 图片类型
        /// </summary>
        Image = 3,

        /// <summary>
        /// 视频类型
        /// </summary>
        Video = 4,
        
        /// <summary>
        /// 图文分享
        /// </summary>
        ImageAndText = 5,
        
        /// <summary>
        /// 微博超话
        /// </summary>
        SuperGroup = 6
        
        
    }

    public enum ShareSource
    {
        /// <summary>
        /// 使用面板分享，可以包括以下各个分享渠道，面板需要在应用云平台配置，面板Id需要在GMSDK/Config Settings中配置
        /// </summary>
        Panel = 0,
        /// <summary>
        /// QQ好友
        /// </summary>
        QQFriend = 1,

        /// <summary>
        /// QQ空间
        /// </summary>
        QQZone = 2,

        /// <summary>
        /// 微信
        /// </summary>
        WeChat = 3,

        /// <summary>
        /// 微信朋友圈
        /// </summary>
        WeChatMoment = 4,

        /// <summary>
        /// 抖音
        /// </summary>
        Awe = 5,

        /// <summary>
        /// 抖音好友
        /// </summary>
        AweIm = 6,

        /// <summary>
        /// 微博
        /// </summary>
        Weibo = 7,
        /// <summary>
        /// SAVE IMAGE
        /// </summary>
        // IMAGE_SHARE = 17,
        
        SaveImage = 17,
        
        /// <summary>
        /// 系统分享
        /// </summary>
        System = 18,
        
        WeiboSg = 19
    }

#if UNITY_IOS
    public enum Orientation
    {
        /// <summary>
        /// 横竖屏都支持(默认值)
        /// </summary>
        All,
        /// <summary>
        /// 仅支持横屏
        /// </summary>
        Landscape,
        /// <summary>
        /// 仅支持竖屏
        /// </summary>
        Portrait, 
    }
#endif

    public enum ShareSilentlyStatus
    {
        BeforeDouyinAuthRenew = 1001,
        BeforeFormalShare = 1002,
    }

    #endregion

    public static partial class ErrorCode
    {
        /// <summary>
        /// 用户取消
        /// </summary>
        public const int ShareUserCancel = -410001;

        /// <summary>
        /// 三方app未安装
        /// </summary>
        public const int ShareAppNotInstalled = -410002;

        /// <summary>
        /// 不可用的分享内容
        /// </summary>
        public const int ShareInvalidContent = -410003;

        /// <summary>
        /// 三方app不支持该分享类型
        /// </summary>
        public const int ShareAppNotSupportShareType = -410004;

        /// <summary>
        /// 缺少Title字段
        /// </summary>
        public const int ShareNoTitle = -410005;

        /// <summary>
        /// 缺少WebPageUrl字段
        /// </summary>
        public const int ShareNoWebPageUrl = -410006;

        /// <summary>
        /// 缺少Image字段
        /// </summary>
        public const int ShareNoImage = -410007;

        /// <summary>
        /// 缺少VideoPath/VideoUrl字段
        /// </summary>
        public const int ShareNoVideo = -410008;

        /// <summary>
        /// 分享视频为空
        /// </summary>
        public const int ShareVideoNil = -410009;

        /// <summary>
        /// 超出视频最大大小
        /// </summary>
        public const int ShareExceedMaxVideoSize = -410010;

        /// <summary>
        /// 超出图片最大大小
        /// </summary>
        public const int ShareExceedMaxImageSize = -410011;

        /// <summary>
        /// 超出图片最大大小
        /// </summary>
        public const int ShareExceedMaxFileSize = -410012;

        /// <summary>
        /// 抖音未授权
        /// </summary>
        public const int ShareAwemeUnAuthError = -410013;

        /// <summary>
        /// 未引入相关渠道
        /// </summary>
        public const int ShareChannelNotImportedError = -410014;

        /// <summary>
        /// 不支持该三方app分享
        /// </summary>
        public const int ShareAppNotSupported = -411001;

        /// <summary>
        /// 用户未授权
        /// </summary>
        public const int ShareUserNotAuth = -411002;

        /// <summary>
        /// 不支持的三方API
        /// </summary>
        public const int ShareAppAPINotSupported = -411003;

        /// <summary>
        /// 没有可用的面板，需要在应用云后台配置
        /// </summary>
        public const int ShareNoValidPanel = -411004;

        /// <summary>
        /// 超出Title最大长度
        /// </summary>
        public const int ShareExceedMaxTitleSize = -411005;

        /// <summary>
        /// 超出Description最大长度
        /// </summary>
        public const int ShareExceedMaxDescriptionSize = -411006;

        /// <summary>
        /// 超出WebPageUrl最大长度
        /// </summary>
        public const int ShareExceedMaxWebPageUrlSize = -411007;

        /// <summary>
        /// 发送请求失败
        /// </summary>
        public const int ShareSendReuqestFail = -411008;

        /// <summary>
        /// 分享失败
        /// </summary>
        public const int ShareFail = -412001;

        /// <summary>
        /// 分享Context为空(Android)
        /// </summary>
        public const int ShareContextNil = -412002;
        
        /// <summary>
        /// 该三方app版本不支持
        /// </summary>
        public const int ShareAppVersionNotSupported = -412003;

        /// <summary>
        /// 三方app初始化失败
        /// </summary>
        public const int ShareAppInitFailed = -412004;

        /// <summary>
        /// 资源下载失败
        /// </summary>
        public const int ShareSourceFetchFailed = -412005;

        /// <summary>
        /// 图片分享图片异常
        /// </summary>
        public const int ShareImageRespDataEmpty = -412006;
        
        /// <summary>
        /// 本地图片(ImagePath)路径出错
        /// </summary>
        public const int ShareLocalImagePathError = -412007;

        /// <summary>
        /// 不支持网络图片
        /// </summary>
        public const int ShareImageUrlNotSupported = -412008;
        
        /// <summary>
        /// 视频分享目标链接为空
        /// </summary>
        public const int ShareTargetUrlNil = -412009;
        

        
        /// <summary>
        /// 静默分享授权失败
        /// </summary>
        public const int ShareSilentlyAuthError = -412010;

        /// <summary>
        /// 静默分享生成视频失败
        /// </summary>
        public const int ShareSilentlyCreateVideoError = -412011;

        /// <summary>
        /// 静默分享上传视频失败
        /// </summary>
        public const int ShareSilentlyUploadError = -412012;

        /// <summary>
        /// 静默分享,登录或绑定不是头条渠道
        /// </summary>
        public const int ShareSilentlyNotTtLogin = -412013;

        /// <summary>
        /// 静默分享,登录或绑定不是抖音渠道
        /// </summary>
        public const int ShareSilentlyNotAweLogin = -412014;

        
        /// <summary>
        /// 不支持微博超话分享
        /// </summary>
        public const int ShareSuperGroupNotSupport = -412015;

        /// <summary>
        /// 微博超话标题为空
        /// </summary>
        public const int ShareSuperGroupTitleEmpty = -412016;
        /// <summary>
        /// 未知错误
        /// </summary>
        public const int ShareUnknownError = -419999;
    }
}