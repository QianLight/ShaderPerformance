using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// shenmo
namespace AssetCheck
{
    [CheckRuleDescription("ParticleSystem", "���Ӳ���", "t:Prefab", "���ж��ٸ�ParticleSystem���")]
    public class ParticleLayerCheck : RuleBase
    {
        [PublicParam("������ֵ", eGUIType.Input)]
        public int layerCount = 10;

        [PublicMethod]
        public bool Check(string path, out string output)
        {
            output = string.Empty;
            GameObject gObject = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (gObject == null)
                return true;
            ParticleSystem[] pss = gObject.GetComponentsInChildren<ParticleSystem>();
            output = $"{ pss.Length}--��׼ֵ:{layerCount}--��������ռ��:{(float)pss.Length/layerCount}";
            return pss.Length < layerCount;
        }
    }
}