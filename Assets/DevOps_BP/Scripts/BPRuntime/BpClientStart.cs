//Generated by Blueprint
using System;
using System.Collections.Generic;
using UnityEngine;
namespace Blueprint.Logic
{
	public class BpClientStart : Blueprint.Logic.BP_Base
	{
		public BpClientStart()
		{
			
		}
		public override void Start()
		{
			//执行节点LoadGameObject_9
			var LoadGameObject_9_ret = BpPluginNode.LoadGameObject("AT_EventManager");
			//执行节点DontDestroyOnLoad_8
			UnityEngine.Object.DontDestroyOnLoad(LoadGameObject_9_ret);
			//执行节点LoadGameObject_10
			var LoadGameObject_10_ret = BpPluginNode.LoadGameObject("AT_NodeTest");
			//执行节点DontDestroyOnLoad_11
			UnityEngine.Object.DontDestroyOnLoad(LoadGameObject_10_ret);
			
		}
		[RuntimeInitializeOnLoadMethod]
		public static void BlueprintClientStaticInit()
		{
			BPInit.Init();
			var init = new Blueprint.Logic.BpClientStart();
			init.Start();
			
		}
		
	}
}