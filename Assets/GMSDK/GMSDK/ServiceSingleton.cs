using System;
using UnityEngine;
using UNBridgeLib;
using UnityEngine.Scripting;


#if UNITY_2018_3_OR_NEWER
[assembly: AlwaysLinkAssembly]
[assembly: Preserve]
#endif
namespace GMSDK {
	public abstract class ServiceSingleton<T> where T : class
	{
		private static T _instance;
		
		public static T instance
		{
			get
			{
				if (_instance != null) return _instance;
				lock (typeof(T))
				{
					LogUtils.D("Create GSDK Service Singleton:" , typeof(T).Name);
					return _instance ?? (_instance = (T)System.Activator.CreateInstance(typeof(T), true));
				}
			}
		}
	}
}