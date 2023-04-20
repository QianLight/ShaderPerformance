using System;
using UnityEngine;
using UNBridgeLib.LitJson;
using System.Text.RegularExpressions;
using System.Text;
using System.Reflection;
using System.Collections.Generic;

namespace GMSDK
{
    public class SdkUtil
    {
        public static readonly string UnityTag = "GMSDK Unity";
        public static bool IsFromUnity3 = false;
			
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || UNITY_EDITOR

#elif UNITY_ANDROID
        //public static AndroidJavaClass androidLog = new AndroidJavaClass("com.tencent.msdk.tools.Logger");
#elif UNITY_IOS
        //[DllImport("__Internal")]
        //public static extern void iosLog(string msg);
#endif

		public static T ToObject<T> (string json) where T:new()
		{
			try {
				T obj = JsonMapper.ToObject<T>(json);
				return obj;
			} catch (Exception e) {
				GMExeption ex = new GMExeption ();
				ex.exception = e;
				if (GMSDKMgr.instance.exceptionCallback != null) {
					GMSDKMgr.instance.exceptionCallback.onGMExceptionCallback (ex);
				}
				return new T ();
			}
		}

		public static void InvokeAction<T> (Action<T> act, T res)
		{
			try {
				if (act != null) {
					act.Invoke (res);
				}
			} catch (Exception e) {
				GMExeption ex = new GMExeption ();
				ex.exception = e;
				if (GMSDKMgr.instance.exceptionCallback != null) {
					GMSDKMgr.instance.exceptionCallback.onGMExceptionCallback (ex);
				}
			}
		}
		//RegisterLogCallback LogType.Exception
		public static void ExceptionReportFormat(string condition, string stackTrace, out string name, out string reason, out string stack)
		{
			name = null;
			reason = condition;
			stack = stackTrace;
			if (condition.Contains("Exception")) {
				Match match = new Regex(@"^(?<errorType>\S+):\s*(?<errorMessage>.*)", RegexOptions.Singleline).Match(condition);
				if (match.Success)
				{
					name = match.Groups["errorType"].Value.Trim();
					reason = match.Groups["errorMessage"].Value.Trim();
				}
			}
			if (string.IsNullOrEmpty(name)) {
				name = "Unity0";
			}
			stack = StacktraceFormat (stackTrace);
		}

		//RegisterLogCallback LogType.Error
		public static void ErrorReportFormat(string condition, string stackTrace, out string name, out string reason, out string stack)
		{
			name = null;
			reason = condition;
			stack = stackTrace;
			if (condition.StartsWith ("Unhandled Exception:")) 
			{
				Match match = new Regex(@"^Unhandled\s+Exception:\s*(?<exceptionName>\S+):\s*(?<exceptionDetail>.*)", RegexOptions.Singleline).Match(condition);

				if (match.Success)
				{
					string exceptionName = match.Groups["exceptionName"].Value.Trim();
					string exceptionDetail = match.Groups["exceptionDetail"].Value.Trim();

					// 
					int dotLocation = exceptionName.LastIndexOf(".");
					if (dotLocation > 0 && dotLocation != exceptionName.Length)
					{
						name = exceptionName.Substring(dotLocation + 1);
					}
					else
					{
						name = exceptionName;
					}

					int stackLocation = exceptionDetail.IndexOf(" at ");
					if (stackLocation > 0)
					{
						// 
						reason = exceptionDetail.Substring(0, stackLocation);
						// substring after " at "
						string callStacks = exceptionDetail.Substring(stackLocation + 3).Replace(" at ", "\n").Replace("in <filename unknown>:0", "").Replace("[0x00000]", "");
						//
						stackTrace = string.Format("{0}\n{1}", stackTrace, callStacks.Trim());

					}
					else
					{
						reason = exceptionDetail;
					}

					// for LuaScriptException
					if (name.Equals("LuaScriptException") && exceptionDetail.Contains(".lua") && exceptionDetail.Contains("stack traceback:"))
					{
						stackLocation = exceptionDetail.IndexOf("stack traceback:");
						if (stackLocation > 0)
						{
							reason = exceptionDetail.Substring(0, stackLocation);
							// substring after "stack traceback:"
							string callStacks = exceptionDetail.Substring(stackLocation + 16).Replace(" [", " \n[");
							//
							stackTrace = string.Format("{0}\n{1}", stackTrace, callStacks.Trim());
						}
					}
				}
			}
			if (string.IsNullOrEmpty(name)) {
				name = "Unity4";
			}
			stack = StacktraceFormat (stackTrace);
		}

		public static string StacktraceFormat (string stackTrace)
		{
			string stack = null;
			if (string.IsNullOrEmpty (stackTrace)) {
				stack = StackTraceUtility.ExtractStackTrace ();
			}

			if (string.IsNullOrEmpty (stackTrace)) {
				stack = "Empty";
			} else {
				try {
					string[] frames = stackTrace.Split ('\n');

					if (frames != null && frames.Length > 0) {

						StringBuilder trimFrameBuilder = new StringBuilder ();

						string frame = null;
						int count = frames.Length;
						for (int i = 0; i < count; i++) {
							frame = frames [i];

							if (string.IsNullOrEmpty (frame) || string.IsNullOrEmpty (frame.Trim ())) {
								continue;
							}

							frame = frame.Trim ();

							// System.Collections.Generic
							if (frame.StartsWith ("System.Collections.Generic.") || frame.StartsWith ("ShimEnumerator")) {
								continue;
							}
							if (frame.Contains ("..ctor")) {
								continue;
							}

							int start = frame.ToLower ().IndexOf ("(at");
							int end = frame.ToLower ().IndexOf ("/assets/");

							if (start > 0 && end > 0) {
								trimFrameBuilder.AppendFormat ("{0}(at {1}", frame.Substring (0, start).Replace (":", "."), frame.Substring (end));
							} else {
								trimFrameBuilder.Append (frame.Replace (":", "."));
							}

							trimFrameBuilder.AppendLine ();
						}

						stack = trimFrameBuilder.ToString ();
					}
				} catch {
				}
			}
			return stack;
		}
		//UnhandledExceptionEventHandler (object sender, UnhandledExceptionEventArgs e)
		public static void SystemExceptionFormat(UnhandledExceptionEventArgs args, out string name, out string reason, out string stack)
		{
			name = null;
			reason = null;
			stack = null;
			if (args==null || args.ExceptionObject == null) {
				return;
			}
			try
			{
				if (args.ExceptionObject.GetType() != typeof(System.Exception))
				{
					return;
				}
			}
			catch
			{
			}
			Exception e = (Exception)(args.ExceptionObject);
			if (e == null) {
				return;
			}
			name = e.GetType().Name;
			reason = e.Message;
			StringBuilder stackTraceBuilder = new StringBuilder("");
			System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(e, true);
			int count = stackTrace.FrameCount;
			for (int i = 0; i < count; i++)
			{
				System.Diagnostics.StackFrame frame = stackTrace.GetFrame(i);

				stackTraceBuilder.AppendFormat("{0}.{1}", frame.GetMethod().DeclaringType.Name, frame.GetMethod().Name);

				ParameterInfo[] parameters = frame.GetMethod().GetParameters();
				if (parameters == null || parameters.Length == 0)
				{
					stackTraceBuilder.Append(" () ");
				}
				else
				{
					stackTraceBuilder.Append(" (");

					int pcount = parameters.Length;

					ParameterInfo param = null;
					for (int p = 0; p < pcount; p++)
					{
						param = parameters[p];
						stackTraceBuilder.AppendFormat("{0} {1}", param.ParameterType.Name, param.Name);

						if (p != pcount - 1)
						{
							stackTraceBuilder.Append(", ");
						}
					}
					param = null;

					stackTraceBuilder.Append(") ");
				}

				string fileName = frame.GetFileName();
				if (!string.IsNullOrEmpty(fileName) && !fileName.ToLower().Equals("unknown"))
				{
					fileName = fileName.Replace("\\", "/");

					int loc = fileName.ToLower().IndexOf("/assets/");
					if (loc < 0)
					{
						loc = fileName.ToLower().IndexOf("assets/");
					}

					if (loc > 0)
					{
						fileName = fileName.Substring(loc);
					}

					stackTraceBuilder.AppendFormat("(at {0}:{1})", fileName, frame.GetFileLineNumber());
				}
				stackTraceBuilder.AppendLine();
			}
			stack = stackTraceBuilder.ToString ();
			stack = StacktraceFormat (stack);
		}
		
		/// <summary>
		/// 业务埋点（玄武）
		/// </summary>
		/// <param name="eventName">事件名称</param>
		/// <param name="eventParams">事件参数</param>
		internal static void SdkTrackSDKEvent(string eventName, JsonData eventParams)
		{
			JsonData param = new JsonData();
			param["eventName"] = eventName;
			if (eventParams != null)
			{
				param["eventParams"] = eventParams;
			}
			UNBridge.Call(SDKMethodName.SdkTrackSDKEvent, param);
		}
		
		/// <summary>
		/// 内部接口，上报Unity自定义事件，用于sdk维度的埋点
		/// </summary>
		/// <param name="eventName">事件名称</param>
		/// <param name="category">可枚举类型</param>
		internal static void SdkMonitorEvent(string eventName, JsonData category)
		{
			if (category == null)
			{
				category = new JsonData();
			}
			category["is_from_unity3"] = IsFromUnity3;

			SdkMonitorEvent(eventName, category, null, null);
		}
		
		/// <summary>
		/// 内部接口，上报Unity自定义事件，用于sdk维度的埋点
		/// </summary>
		/// <param name="eventName">事件名称</param>
		/// <param name="category">可枚举类型</param>
		/// <param name="isFromUnity3">是否来源于Unity3</param>
		internal static void SdkMonitorEvent(string eventName, JsonData category, bool isFromUnity3)
		{
			if (category == null)
			{
				category = new JsonData();
			}
			category["is_from_unity3"] = isFromUnity3;

			SdkMonitorEvent(eventName, category, null, null);
		}

		private static void SdkMonitorEvent(string eventName, JsonData category, JsonData metric, JsonData extra)
		{
			category["package_name"] = Application.identifier;
			category["app_version"] = Application.version;
			category["sdk_version"] = MainSDK.Version;
			
			JsonData p = new JsonData();
			p["event"] = eventName;
			p["category"] = category;
			p["metric"] = metric;
			p["extra"] = extra;
	        
			JsonData finalParam = new JsonData();
			finalParam["data"] = p;
			UNBridge.Call(SDKMethodName.SdkMonitorEvent, finalParam);
		}
	}
}