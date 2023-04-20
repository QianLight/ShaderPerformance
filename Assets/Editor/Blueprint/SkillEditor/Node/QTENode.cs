using EcsData;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using BluePrint;
using System.IO;

namespace EditorNode
{
    public class QTENode : TimeTriggerNode<XQTEData>
    {
        private BaseSkillNode resultNode = null;

        private static Dictionary<int, string> ScriptHash = new Dictionary<int, string>();
        private static bool usePresentID = true;

        public override void Init(BluePrintGraph Root, Vector2 pos, bool AutoDefaultMainPin = true)
        {
            base.Init(Root, pos, AutoDefaultMainPin);
            HeaderImage = "BluePrint/Header9";
        }

        public override void DrawDataInspector()
        {
            base.DrawDataInspector();

            //HosterData.Self = Toggle("Self: ", HosterData.Self);

            if (HosterData.Self)
            {
                HosterData.Duration = TimeFrameField("Duration: ", HosterData.Duration);

                HosterData.CacheTime = TimeFrameField("CacheTime: ", HosterData.CacheTime);

                HosterData.QTEID = EditorGUITool.IntField("QTEID: ", HosterData.QTEID);

                HosterData.EndAtScriptEnd = EditorGUITool.Toggle("EndAtScriptEnd: ", HosterData.EndAtScriptEnd);
                if (HosterData.EndAtScriptEnd) HosterData.Duration = GetRoot.GetConfigData<XConfigData>().Length;


                usePresentID = EditorGUITool.Toggle("UsePresentID", usePresentID);
                List<uint> hashList = XSkillReader.GetQteSkills(HosterData.QTEID);
                Dictionary<uint, List<int>> dic = new Dictionary<uint, List<int>>();
                if (hashList != null && hashList.Count != 0)
                {
                    for (int i = 0; i < hashList.Count; ++i)
                    {
                        CFUtilPoolLib.SeqListRef<int> qte = XSkillReader.GetSkillQTE(hashList[i]);
                        
                        for (int j = 0; j < qte.Count; ++j)
                        {
                            if (qte[j, 0] == HosterData.QTEID && (!usePresentID || XSkillReader.GetSkillPartnerID(hashList[i]) == GetRoot.GetConfigData<XConfigData>().PresentID))
                            {
                                if (!dic.ContainsKey(hashList[i])) dic.Add(hashList[i], new List<int>());
                                List<int> list = dic[hashList[i]];
                                if (list.Contains(qte[j, 1])) continue;
                                EditorGUITool.LabelField(XSkillReader.GetSkillSkillScript(hashList[i]) + "   \tSlotID: " + qte[j, 1]);
                                dic[hashList[i]].Add(qte[j, 1]);
                            }
                        }
                    }
                }
            }
            else
            {
                HosterData.Duration = TimeFrameField("Duration: ", HosterData.Duration);

                HosterData.QTEID = EditorGUITool.IntField("QTEID: ", HosterData.QTEID);

                int index = 0;
                string key;
                GetRoot.FunctionHash2Name.TryGetValue(HosterData.FunctionHash, out key);
                index = GetRoot.GetFunctionIndex(key, "q_");
                string[] funcName = GetRoot.GetFunctionTranslate("q_");
                GetRoot.FunctionName2Hash.TryGetValue(funcName[EditorGUITool.Popup("Function: ", index, EditorGUITool.Translate(funcName))], out HosterData.FunctionHash);

                GetNodeByIndex<ResultNode>(ref resultNode, ref HosterData.Parameter1, true);
            }
        }

        public override void BuildDataFinish()
        {
            base.BuildDataFinish();

            if (HosterData.EndAtScriptEnd) HosterData.Duration = GetRoot.GetConfigData<XConfigData>().Length;

            if (!HosterData.Self)
            {
                GetNodeByIndex<ResultNode>(ref resultNode, ref HosterData.Parameter1);
            }
        }

        public override bool CompileCheck()
        {
            if (!base.CompileCheck()) return false;

            if (HosterData.QTEID >= EditorEcs.Xuthus_VirtualServer.QTE_MAX)
            {
                LogError("QTENode_" + HosterData.Index + ",  QTEID 超过最大上限(" + EditorEcs.Xuthus_VirtualServer.QTE_MAX + ")！！！");
                return false;
            }

            if (!HosterData.Self && HosterData.FunctionHash == (int)CFUtilPoolLib.XCommon.singleton.XHash("q_is_in_result_attack_field") &&
                HosterData.Parameter1 == -1)
            {
                LogError("QTENode_" + HosterData.Index + ",  ResultNodeIndex配置错误！！！");
                return false;
            }

            return true;
        }
    }
}