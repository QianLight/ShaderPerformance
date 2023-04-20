using System.Collections.Generic;

namespace GSDK
{
	/// <summary>
	/// 获取公告的回调
	/// </summary>
	/// <param name="result">回调结果</param>
	/// <para>
	///     当前接口可能返回的错误码:
	///             Success:成功
	///             BulletinNetworkError, 网络错误
	///             BulletinParamsError, 参数错误
	///             BulletinServerError, 服务器错误
	///             BulletinUnknownError, 未知错误
	/// </para>
	/// <param name="BulletinInfo">公告信息</param>
	public delegate void FetchBulletinsDelegate(Result result, BulletinInfo bulletin);
	
	/// <summary>
	/// 用于获取实例进行接口调用
	/// e.g.Bulletin.Service.MethodName();
	/// </summary>
	public static class Bulletin
	{
		public static IBulletinService Service
		{
			get
			{
				return ServiceProvider.Instance.GetService(ServiceType.Bulletin) as IBulletinService;
			}
		}
	}
	
	public interface IBulletinService : IService
    {
        /// <summary>
        /// 获取公告
        /// </summary>
        /// <param name="bulletinConfig">获取公告相关参数</param>
        /// <param name="callback">完成后的回调</param>
        void FetchBulletins(BulletinConfig bulletinConfig, FetchBulletinsDelegate callback);
    }
	
	/// <summary>
	/// 公告请求参数
	/// </summary>
    public class BulletinConfig
    {
		/// <summary>
		/// 游戏当前语言，可选，默认值为国际化中设置的语言，相关值请参考文档
		/// </summary>
		public string Language;
		/// <summary>
		/// 用户所在地区，可选，国内默认值为CN，海外则根据IP定位地区，相关值请参考文档
		/// </summary>
		public string Region;
		/// <summary>
		/// 公告场景，必传，请向发行同学索要位置代码
		/// </summary>
		public int Scene;
		/// <summary>
		/// 游戏的服务器区服标记，可选
		/// </summary>
		public string ServerId;
		/// <summary>
		/// 用户的渠道唯一id，可选
		/// </summary>
		public string OpenId;
		/// <summary>
		/// 区id，可选
		/// </summary>
		public string ZoneId;
		/// <summary>
		/// 角色id，可选
		/// </summary>
		public string RoleId;
		/// <summary>
		/// 额外扩展字段，json字符串，可选
		/// </summary>
		public string ExtraInfo; 

	    public BulletinConfig(int scene)
	    {
#if UNITY_STANDALONE_WIN && !GMEnderOn
			Scene = scene;
			Language = "";
			Region = "";
			ServerId = "";
			ZoneId = "";
			RoleId = "";
			ExtraInfo = "";
#else
			Scene = scene;
		    Language = null;
		    Region = null;
		    ServerId = null; 
	        ZoneId = null; 
	        RoleId = null; 
	        ExtraInfo = null; 
#endif
		}
	}
	
	/// <summary>
	/// 公告回调
	/// </summary>
    public class BulletinInfo
    {
		/// <summary>
		/// 公告数组
		/// </summary>
		public List<BulletinItem> BulletinItems;
		/// <summary>
		/// 总共页数
		/// </summary>
		public int TotalPage;
		/// <summary>
		/// 当前页数
		/// </summary>
		public int CurrentPage;
		/// <summary>
		///  每页的数量
		/// </summary>
		public int PageSize;
		/// <summary>
		/// 筛选条件，cp透传进来的, json字符串
		/// </summary>
		public string Filters; 
    }

	/// <summary>
	/// 公告回调参数
	/// </summary>
    public class BulletinItem
    {
		/// <summary>
		/// 公告ID
		/// </summary>
		public string BID;
		/// <summary>
		/// 公告展示语言
		/// </summary>
		public string Language;
		/// <summary>
		/// 公告类型
		/// </summary>
		public int Scene;
		/// <summary>
		/// 公告的标题
		/// </summary>
		public string Title;
		/// <summary>
		/// 公告的内容
		/// </summary>
		public string Content;
		/// <summary>
		/// 跳转链接
		/// </summary>
		public string TargetURL;
		/// <summary>
		/// 优先级，置顶:0, 置底:-1, 自定义:大于0的自然数
		/// </summary>
		public int Priority;
		/// <summary>
		/// 图片url
		/// </summary>
		public string ImageURL;
		/// <summary>
		/// 富文本编码格式，类型有ubb、html、ugui、text、ugui-text、ngui
		/// </summary>
		public string Encoding;
		/// <summary>
		/// 活动公告按钮文字
		/// </summary>
		public string ButtonText;
		/// <summary>
		/// 公告起始时间
		/// </summary>
		public long StartTime;
		/// <summary>
		/// 公告过期时间
		/// </summary>
		public long ExpireTime;
		/// <summary>
		/// 频率 0:每次，-1:仅展示一次, >0:前n次展示
		/// </summary>
		public int Times;
		/// <summary>
		/// tab文案
		/// </summary>
		public string Tab;
		/// <summary>
		/// 图片信息，json格式
		/// </summary>
		public string ImageInfoJson;
		/// <summary>
		/// 公告图片数组
		/// </summary>
		public List<ImageItem> ImageList;
		/// <summary>
		/// 公告tab标签
		/// </summary>
		public string TabLabel;
		/// <summary>
		/// 红点提醒开关，开：true、关：false
		/// </summary>
		public bool BadgeSwitch;
		/// <summary>
		/// 额外信息
		/// </summary>
		public string Extra;
    }
	
	/// <summary>
	/// 公告图片信息
	/// </summary>
    public class ImageItem
    {
	    //图片本身的链接
	    public string imageLink;
	    //图片跳转链接
	    public string imageJumpLink;
	    //图片参数
	    public string imageInfoJson;
    }

    public partial class ErrorCode
    {
		/// <summary>
		/// 输入Json字符串非法
		/// </summary>
	    public const int BulletinInvalidJsonString = -429001;

		/// <summary>
		/// 网络错误
		/// </summary>
		public const int BulletinNetworkError = -423001;

		/// <summary>
		/// 参数错误
		/// </summary>
		public const int BulletinParamsError = -420001;

	    /// <summary>
	    /// 服务器异常
	    /// </summary>
	    public const int BulletinServerError = -425000;

		/// <summary>
		/// 未知错误
		/// </summary>
		public const int BulletinUnknownError = -429999;
	}
}