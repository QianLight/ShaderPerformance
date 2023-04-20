using UnityEngine;

namespace GMSDK
{
    public class BaseGPMSDK
    {
		private GPMMonitor monitor = null;

        public BaseGPMSDK()
        {
			GPMGlobalInfo.Init();
			GPMGraphicLevel.RequestGraphicLevel();
			if (monitor == null)
			{
				GameObject gameObject = new GameObject("GPMMonitor");
				UnityEngine.Object.DontDestroyOnLoad(gameObject);
				monitor = gameObject.AddComponent<GPMMonitor>();
			}
		}

		/// <summary>
		/// 获取画质分档
		/// </summary>
		public int GraphicLevel()
        {
			return GPMGraphicLevel.RequestGraphicLevel();
        }

		/// <summary>
		/// 场景开始
		/// </summary>
		/// <param name="sceneName">场景名</param>
		public void LogSceneStart(string sceneName)
		{
			if (monitor != null)
			{
				monitor.LogSceneStart(sceneName);
			}
		}

		/// <summary>
		/// 场景加载完成
		/// </summary>
		public void LogSceneLoaded()
		{
			if (monitor != null)
			{
				monitor.LogSceneLoaded();
			}
		}

		/// <summary>
		/// 场景结束
		/// </summary>
		public void LogSceneEnd(bool isUpload = true)
		{
			if (monitor != null)
			{
				monitor.LogSceneEnd(isUpload);
			}
		}

		/// <summary>
		/// 全局信息，设置以后对所有场景生效
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="value">value</param>
        public void LogGlobalInfo(string key, string value)
        {
			GPMMonitor.LogGlobalInfo(key, value);
        }
		public void LogGlobalInfo(string key, int value)
		{
			GPMMonitor.LogGlobalInfo(key, value);
		}

		/// <summary>
		/// 场景信息，只能在场景开始后设置，只对该场景生效
		/// </summary>
		/// <param name="key">key</param>
		/// <param name="value">value</param>
		public void LogSceneInfo(string key, string value)
		{
			if (monitor != null)
			{
				monitor.LogSceneInfo(key, value);
			}
		}
		public void LogSceneInfo(string key, int value)
		{
			if (monitor != null)
			{
				monitor.LogSceneInfo(key, value);
			}
		}
	}
}
