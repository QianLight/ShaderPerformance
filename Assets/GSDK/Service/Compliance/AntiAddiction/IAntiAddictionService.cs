
namespace GSDK
{
	#region delegates

	/// <summary>
    /// 防沉迷状态的回调
    /// </summary>
    /// <param name="info">防沉迷状态（类型+文案）</param>
    public delegate void AntiAddictionStatusEventHandler(AntiAddictionInfo info);

	#endregion

	#region static service class
	public static class AntiAddiction
	{
		public static IAntiAddictionService Service
		{
			get
			{
				return ServiceProvider.Instance.GetService(ServiceType.AntiAddiction) as IAntiAddictionService;
			}
		}
	}
	#endregion

	#region IAntiAddictionService 
	public interface IAntiAddictionService : IService
    {

	    #region 回调Event

		event AntiAddictionStatusEventHandler AntiAddictionStatusEvent;

	    #endregion

	    #region 接口

	    /// <summary>
        /// 控制防沉迷是否弹窗
        /// </summary>
		/// <param>
        /// true：表示启用弹窗展示
        /// false：表示不弹出窗口
        /// </param>
	    bool EnableAlert { set; }

		/// <summary>
        /// 获取本地缓存的防沉迷状态
        /// </summary>
        void FetchLatestAntiAddictionStatus(AntiAddictionStatusEventHandler callback);

		/// <summary>
		/// 从服务器直接获取最新的防沉迷状态
		/// </summary>
		void FetchServiceAntiAddictionStatus(AntiAddictionStatusEventHandler callback);
		
		#endregion

    }
	#endregion

	#region public defines

	/// <summary>
	/// 防沉迷状态（类型+文案）
	/// </summary>
	public class AntiAddictionInfo
	{
		/// <summary>
		/// 防沉迷类型
		/// </summary>
		public AntiAddictionOperation operation;
		/// <summary>
		/// 防沉迷文案
		/// </summary>
		public string message;
		/// <summary>
		/// 防沉迷登录文案
		/// </summary>
		public string loginMessage;
		/// <summary>
		/// 防沉迷规则限制文案
		/// </summary>
		public string ruleMessage;
	}

	/// <summary>
	/// 防沉迷场景
	/// </summary>
	public enum AntiAddictionOperation
    {
	    /// <summary>
	    /// 网络问题或者服务端问题导致获取防沉迷状态失败
	    /// </summary>
	    Fail = -1,
	    /// <summary>
	    /// 无需操作
	    /// </summary>
	    Ignore = 0,
		/// <summary>
		/// 未成年用户即将达到单日累计在线时长限制
		/// </summary>
		MinorRemind = 1,
		/// <summary>
		/// 强制下线（默认策略无此场景）
		/// </summary>
		ForceOffline = 2,
		/// <summary>
		/// 未成年用户达到单日累计在线时长限制
		/// </summary>
		MinorLimit = 3,
		/// <summary>
		/// 未成年用户宵禁
		/// </summary>
		MinorCurfew = 4,
		/// <summary>
		/// 游客（未实名）用户达到在线时长限制
		/// </summary>
		VisitorLimit = 5,
		/// <summary>
		/// 游客（未实名）用户即将达到在线时长限制
		/// </summary>
		VisitorRemind = 6,
		/// <summary>
		/// 提醒玩家即将被下线（默认策略无此场景）
		/// </summary>
		RemindOffline = 7,
		/// <summary>
		/// 游客（未实名）用户宵禁（默认策略无此场景）
		/// </summary>
		VisitorCurfew = 8,
		/// <summary>
		/// 未成年用户登录成功后提醒
		/// </summary>
		MinorLoginTips = 10,
		/// <summary>
		/// 游客（未实名）用户登录成功后提醒
		/// </summary>
		VisitorLoginTips = 11
    }

	#endregion
}

