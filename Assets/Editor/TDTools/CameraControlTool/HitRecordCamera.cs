using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using Cinemachine;
using VirtualSkill;

namespace TDTools
{
    public class HitRecordCamera
    {
        [MenuItem("Tools/TDTools/监修相关工具/监修受击相机")]
        public static void AddCamera()
        {
            var freeLook = AssetDatabase.LoadAssetAtPath("Assets/BundleRes/Prefabs/Cinemachine/Jianxiu/Jianxiu2.prefab", typeof(GameObject)) as GameObject;
            var gameObject = GameObject.Instantiate<GameObject>(freeLook, null);
            var cntg = gameObject?.GetComponentInChildren<CinemachineTargetGroup>();
            if (cntg != null)
            {
                cntg.m_Targets[0].target = SkillHoster.GetHoster.EntityDic[SkillHoster.PlayerIndex].posXZ.transform;
                cntg.m_Targets[1].target = SkillHoster.GetHoster.EntityDic[SkillHoster.GetHoster.Target].posXZ.transform;
                cntg.m_Targets[0].weight = 1f;
                cntg.m_Targets[1].weight = 5f;
            }
        }
    }
}

