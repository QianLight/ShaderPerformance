using System;
using System.Collections;
using System.Collections.Generic;
using CFEngine;
using UnityEditor;
using UnityEngine;

public class SFXTypeAlphaMonitor : EditorWindow
{
	public static SFXTypeAlphaMonitor instance;
    const string menuFolder = "ArtTools/SFX/技能特效显隐";
	// const string CombatPlayerToggle = menuFolder + "玩家特效";
	// private static bool cp = true;
	// const string PartnerToggle = menuFolder + "队友特效";
	// private static bool p = true;
	// const string EnemyToggle = menuFolder + "敌人特效";
	// private static bool e = true;
	// const string EnemyPlayerToggle = menuFolder + "敌方玩家特效";
	// private static bool ep = true;
	
	[MenuItem(menuFolder)]
	public static void ShowWindow()
	{
		if(!instance)instance = GetWindow<SFXTypeAlphaMonitor>("技能特效显隐");
		instance.Focus();
	}
	private void OnGUI()
	{
		SFXMgr.typeAlpha[1] = EditorGUILayout.Slider("我方玩家 透明度", SFXMgr.typeAlpha[1], 0, 1);
		SFXMgr.typeAlpha[2] = EditorGUILayout.Slider("我方队友 透明度", SFXMgr.typeAlpha[2], 0, 1);
		SFXMgr.typeAlpha[3] = EditorGUILayout.Slider("敌方怪物 透明度", SFXMgr.typeAlpha[3], 0, 1);
		SFXMgr.typeAlpha[4] = EditorGUILayout.Slider("敌方玩家 透明度", SFXMgr.typeAlpha[4], 0, 1);
	}

	// [MenuItem(CombatPlayerToggle, false, 0)]
	// static void SwitchCombatPlayer()
	// {
	// 	cp = !cp;
	// 	Menu.SetChecked(CombatPlayerToggle, cp);
	// 	SFXMgr.typeAlpha[1] = cp ? 1 : 0;
	// }
	// [MenuItem(PartnerToggle, false, 0)]
	// static void SwitchPartner(){
	// 	p = !p;
	// 	Menu.SetChecked(PartnerToggle, p);
	// 	SFXMgr.typeAlpha[2] = p ? 1 : 0;
	// }
	// [MenuItem(EnemyToggle, false, 0)]
	// static void SwitchEnemy(){
	// 	e = !e;
	// 	Menu.SetChecked(EnemyToggle, e);
	// 	SFXMgr.typeAlpha[3] = e ? 1 : 0;
	// }
	//
	// [MenuItem(EnemyPlayerToggle, false, 0)]
	// static void SwitchEnemyPlayer(){
	// 	ep = !ep;
	// 	Menu.SetChecked(EnemyPlayerToggle, ep);
	// 	SFXMgr.typeAlpha[4] = ep ? 1 : 0;
	// }
}
