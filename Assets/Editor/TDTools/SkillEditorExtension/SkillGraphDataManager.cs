using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using EcsData;
using EditorNode;

namespace TDTools
{
    public class SkillGraphDataManager
    {
        static readonly string TitleDataFile = "Assets/Editor/TDTools/SkillEditorExtension/DataTitles.asset";
        static SkillGraphTitleAsset TitleAsset;
        public static void InitTitleFile()
        {
            TitleAsset = AssetDatabase.LoadAssetAtPath<SkillGraphTitleAsset>(TitleDataFile);
            TitleAsset.ReBuild();
        }

        public static SkillGraphShowDataTitle GetTitleDataByType(string type)
        {
            return TitleAsset.TitleDic[type];
        }

        public static string[] GetTypeArray()
        {
            return TitleAsset.TitleDic.Keys.ToArray();
        }

        public static SkillGraph GetSkillGraph(string path)
        {
            SkillGraph skillGraph = new SkillGraph();
            skillGraph.NeedInitRes = false;
            skillGraph.OpenData(path);
            return skillGraph;
        }

        public static List<SkillGraphShowData> GetDataByFuncName(List<string> pathList, string name)
        {
            Func<SkillGraph, List<SkillGraphShowData>> func = (graph) =>
            {
                object[] list = new object[] { graph, null };
                MethodInfo getData = typeof(SkillGraphDataManager).GetMethod(name);
                getData.Invoke(null, list);
                return (List<SkillGraphShowData>)list[1];
            };
            return GetSkillGraphData(pathList, func);
        }

        public static List<SkillGraphShowData> GetSkillGraphData(List<string> pathList, Func<SkillGraph, List<SkillGraphShowData>> action)
        {
            List<SkillGraphShowData> result = new List<SkillGraphShowData>();
            result.Add(new SkillGraphShowData("Root", -1, 0));
            foreach(var item in pathList)
            {
                var graph = GetSkillGraph(item);
                for (int i = 0; i < graph.widgetList.Count; ++i) (graph.widgetList[i] as BaseSkillNode).TriggerTime = -1;
                for (int i = 0; i < graph.widgetList.Count; ++i)
                {
                    (graph.widgetList[i] as BaseSkillNode).CalcTriggerTime();
                }
                var data = action?.Invoke(graph);
                if (data != null)
                {
                    result = result.Concat(data).ToList<SkillGraphShowData>();
                }
            }
            return result;
        }

        public static void GetBasicData(SkillGraph graph, out List<SkillGraphShowData> data)
        {
            int hash = (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name);
            data = new List<SkillGraphShowData>();
            data.Clear();
            data.Add(BasicData(graph, hash));
            //data = BasicData(graph, hash);
        }

        private static SkillGraphShowData BasicData(SkillGraph graph, int hash)
        {
            SkillGraphShowData data = new SkillGraphShowData(graph.configData.Name, 0, hash);
            data.IntParam.Add(graph.configData.SkillType);
            data.BoolParam.Add(graph.configData.TriggerCDAtEnd);
            data.BoolParam.Add(graph.configData.DisableGlobalRotate);
            data.StringParam.Add(graph.graphConfigData.tag);
            return data;
        }

        public static void GetStatusData(SkillGraph graph, out List<SkillGraphShowData> datalist)
        {
            //set up for datalist
            datalist = new List<SkillGraphShowData>();
            datalist.Clear();
            
            //get parent
            int hash = (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name);
            SkillGraphShowData parent = new SkillGraphShowData(graph.configData.Name, 0, hash);
            datalist.Add(parent);
            
            //for each action node, get its son 
            foreach(var node in graph.configData.ActionStatusData)
            {
                int nodeHash =  (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name + "-" + node.Index);
                SkillGraphShowData Node = new SkillGraphShowData($"ActionStatus{node.Index}", 1, nodeHash);
                Node.BoolParam.Add(node.CanMove ? true : false);
                Node.BoolParam.Add(node.CanRotate ? true : false);
                if (node.TimeBased)
                {
                    Node.IntParam.Add((int)graph.TimeToFrame(node.At));
                }
                datalist.Add(Node);
            }
        }

        public static void GetQTEData(SkillGraph graph, out SkillGraphShowData data)
        {
            int hash = (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name);
            data = BasicData(graph, hash);
            List<string> qteList = new List<string>();
            foreach (var node in graph.configData.QTEData)
            {
                List<string> qte = new List<string>();
                if (node.TimeBased)
                    qte.Add($"qte id {node.QTEID} At {graph.TimeToFrame(node.At)}");
                else
                    qte.Add($"qte id {node.QTEID} At Node{node.Index}");
                qte.Add($"duration = {graph.TimeToFrame(node.Duration)}"); 
                qte.Add($"cache = {graph.TimeToFrame(node.CacheTime)}");
                //List<uint> hashList = XSkillReader.GetQteSkills(node.QTEID);
                //Dictionary<uint, List<int>> dic = new Dictionary<uint, List<int>>();
                //if (hashList != null && hashList.Count != 0)
                //{
                //    for (int i = 0; i < hashList.Count; ++i)
                //    {
                //        CFUtilPoolLib.SeqListRef<int> qteSeq = XSkillReader.GetSkillQTE(hashList[i]);
                //        for (int j = 0; j < qteSeq.Count; ++j)
                //        {
                //            if (qteSeq[j, 0] == node.QTEID && XSkillReader.GetSkillPartnerID(hashList[i]) == graph.GetConfigData<XConfigData>().PresentID)
                //            {
                //                if (!dic.ContainsKey(hashList[i])) dic.Add(hashList[i], new List<int>());
                //                List<int> list = dic[hashList[i]];
                //                if (list.Contains(qteSeq[j, 1])) continue;
                //                qte.Add(XSkillReader.GetSkillSkillScript(hashList[i]) + "   SlotID: " + qteSeq[j, 1]);
                //                dic[hashList[i]].Add(qteSeq[j, 1]);
                //            }
                //        }
                //    }
                //}
                data.SetMaxLine(qte.Count);
                qteList.Add(string.Join("\n", qte));
            }
            data.ObjectParam.Add(qteList);
        }
        
        public static void GetMessageData(SkillGraph graph, out List<SkillGraphShowData> datalist)
        {
            //set up for datalist
            datalist = new List<SkillGraphShowData>();
            datalist.Clear();
            
            //get parent
            int hash = (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name);
            SkillGraphShowData parent = new SkillGraphShowData(graph.configData.Name, 0, hash);
            datalist.Add(parent);
            
            //for each action node, get its son 
            foreach(var node in graph.configData.MessageData)
            {
                int nodeHash =  (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name + "-" + node.Index);
                SkillGraphShowData Node = new SkillGraphShowData($"MessageData{node.Index}", 1, nodeHash);
                Node.FloatParam.Add((graph.widgetList[node.Index] as BaseSkillNode).TriggerTime);
                Node.IntParam.Add(node.Type);
                if (node.Message != null)
                    Node.StringParam.Add(node.Message);
                else
                {
                    Node.StringParam.Add("");
                }
                datalist.Add(Node);
            }
        }

        public static void GetBuffData(SkillGraph graph, out List<SkillGraphShowData> datalist)
        {
            //set up for datalist
            datalist = new List<SkillGraphShowData>();
            datalist.Clear();

            //get parent
            int hash = (int) CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name);
            SkillGraphShowData parent = new SkillGraphShowData(graph.configData.Name, 0, hash);
            datalist.Add(parent);

            //for each action node, get its son 
            foreach (var node in graph.configData.BuffData)
            {
                int nodeHash = (int) CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name + "-" + node.Index);
                SkillGraphShowData Node = new SkillGraphShowData($"BuffData{node.Index}", 1, nodeHash);
                Node.FloatParam.Add((graph.widgetList[node.Index] as BaseSkillNode).TriggerTime);
                Node.IntParam.Add(node.BuffID);
                Node.IntParam.Add(node.BuffLevel);
                Node.IntParam.Add(node.TargetType);
                datalist.Add(Node);
            }
        }
        
        public static void GetAudioData(SkillGraph graph, out List<SkillGraphShowData> datalist)
        {
            //set up for datalist
            datalist = new List<SkillGraphShowData>();
            datalist.Clear();
            
            //get parent
            int hash = (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name);
            SkillGraphShowData parent = new SkillGraphShowData(graph.configData.Name, 0, hash);
            datalist.Add(parent);
            
            //for each action node, get its son 
            foreach(var node in graph.configData.AudioData)
            {
                int nodeHash =  (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name + "-" + node.Index);
                SkillGraphShowData Node = new SkillGraphShowData($"AudioData{node.Index}", 1, nodeHash);
                Node.FloatParam.Add((graph.widgetList[node.Index] as BaseSkillNode).TriggerTime);
                Node.StringParam.Add(node.AudioName);
                Node.IntParam.Add(node.ChannelID);
                Node.BoolParam.Add(node.StopAtSkillEnd);
                Node.BoolParam.Add(node.Follow);
                datalist.Add(Node);
            }
        }
        
        public static void GetMobData(SkillGraph graph, out List<SkillGraphShowData> datalist)
        {
            //set up for datalist
            datalist = new List<SkillGraphShowData>();
            datalist.Clear();
            
            //get parent
            int hash = (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name);
            SkillGraphShowData parent = new SkillGraphShowData(graph.configData.Name, 0, hash);
            datalist.Add(parent);
            
            //for each action node, get its son 
            foreach(var node in graph.configData.MobUnitData)
            {
                int nodeHash =  (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name + "-" + node.Index);
                SkillGraphShowData Node = new SkillGraphShowData($"MessageData{node.Index}", 1, nodeHash);
                Node.FloatParam.Add((graph.widgetList[node.Index] as BaseSkillNode).TriggerTime);
                Node.IntParam.Add(node.TemplateID);
                Node.BoolParam.Add(node.LifewithinSkill);
                Node.FloatParam.Add(node.OffsetX);
                Node.FloatParam.Add(node.OffsetY);
                Node.FloatParam.Add(node.OffsetZ);
                Node.BoolParam.Add(node.MobAtTarget);
                Node.FloatParam.Add(node.Angle);
                Node.BoolParam.Add(node.Random);
                Node.FloatParam.Add(node.RandomRange);
                datalist.Add(Node);
            }
        }
        
        public static void GetWarningData(SkillGraph graph, out List<SkillGraphShowData> datalist)
        {
            //set up for datalist
            datalist = new List<SkillGraphShowData>();
            datalist.Clear();
            
            //get parent
            int hash = (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name);
            SkillGraphShowData parent = new SkillGraphShowData(graph.configData.Name, 0, hash);
            datalist.Add(parent);
            
            //for each action node, get its son 
            foreach(var node in graph.configData.WarningData)
            {
                int nodeHash =  (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name + "-" + node.Index);
                SkillGraphShowData Node = new SkillGraphShowData($"WarningData{node.Index}", 1, nodeHash);
                Node.BoolParam.Add(node.NeedTarget);
                Node.BoolParam.Add(node.Random);
                Node.BoolParam.Add(node.NeedBullet);             
                datalist.Add(Node);
            }
        }
        
        public static void GetLookAtData(SkillGraph graph, out List<SkillGraphShowData> datalist)
        {
            //set up for datalist
            datalist = new List<SkillGraphShowData>();
            datalist.Clear();
            
            //get parent
            int hash = (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name);
            SkillGraphShowData parent = new SkillGraphShowData(graph.configData.Name, 0, hash);
            datalist.Add(parent);
            
            //for each action node, get its son 
            foreach(var node in graph.configData.LookAtData)
            {
                int nodeHash =  (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name + "-" + node.Index);
                SkillGraphShowData Node = new SkillGraphShowData($"LookAtData{node.Index}", 1, nodeHash);
                Node.FloatParam.Add(node.LifeTime);           
                datalist.Add(Node);
            }
        }
        
        public static void GetTargetSelectData(SkillGraph graph, out List<SkillGraphShowData> datalist)
        {
            //set up for datalist
            datalist = new List<SkillGraphShowData>();
            datalist.Clear();
            
            //get parent
            int hash = (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name);
            SkillGraphShowData parent = new SkillGraphShowData(graph.configData.Name, 0, hash);
            datalist.Add(parent);
            
            //for each action node, get its son 
            foreach(var node in graph.configData.TargetSelectData)
            {
                int nodeHash =  (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name + "-" + node.Index);
                SkillGraphShowData Node = new SkillGraphShowData($"LookAtData{node.Index}", 1, nodeHash);
                Node.FloatParam.Add(node.RangeLower);        
                Node.FloatParam.Add(node.RangeUpper);           
                Node.FloatParam.Add(node.Scope);           
                Node.FloatParam.Add(node.OffsetX);
                Node.FloatParam.Add(node.OffsetZ);           
                Node.IntParam.Add(node.SelectFilter);
                Node.IntParam.Add(node.RandomNum);
                Node.BoolParam.Add(node.Sync);
                Node.BoolParam.Add(node.SelectHoster);
                Node.BoolParam.Add(node.Sync);
                Node.BoolParam.Add(node.LookAt);
                Node.BoolParam.Add(node.DiscardLastTarget);
                datalist.Add(Node);
            }
        }
        
        public static void GetScriptTransData(SkillGraph graph, out List<SkillGraphShowData> datalist)
        {
            //set up for datalist
            datalist = new List<SkillGraphShowData>();
            datalist.Clear();
            
            //get parent
            int hash = (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name);
            SkillGraphShowData parent = new SkillGraphShowData(graph.configData.Name, 0, hash);
            datalist.Add(parent);
            
            //for each action node, get its son 
            foreach(var node in graph.configData.ScriptTransData)
            {
                int nodeHash =  (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name + "-" + node.Index);
                SkillGraphShowData Node = new SkillGraphShowData($"ScriptTransData{node.Index}", 1, nodeHash);
                Node.StringParam.Add(node.Name);
                Node.IntParam.Add(node.Hash);
                Node.IntParam.Add(node.Type);
                Node.BoolParam.Add(node.Force);
                Node.BoolParam.Add(node.InheritTarget);
                datalist.Add(Node);
            }
        }
        
        public static void GetAimTargetData(SkillGraph graph, out List<SkillGraphShowData> datalist)
        {
            //set up for datalist
            datalist = new List<SkillGraphShowData>();
            datalist.Clear();
            
            //get parent
            int hash = (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name);
            SkillGraphShowData parent = new SkillGraphShowData(graph.configData.Name, 0, hash);
            datalist.Add(parent);
            
            //for each action node, get its son 
            foreach(var node in graph.configData.AimTargetData)
            {
                int nodeHash =  (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name + "-" + node.Index);
                SkillGraphShowData Node = new SkillGraphShowData($"AimTargetData{node.Index}", 1, nodeHash);
                Node.FloatParam.Add(node.LifeTime);
                Node.FloatParam.Add(node.MaxAimAngle);
                Node.FloatParam.Add(node.AimSpeed);
                datalist.Add(Node);
            }
        }
        
        public static void GetChargeData(SkillGraph graph, out List<SkillGraphShowData> datalist)
        {
            //set up for datalist
            datalist = new List<SkillGraphShowData>();
            datalist.Clear();
            
            //get parent
            int hash = (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name);
            SkillGraphShowData parent = new SkillGraphShowData(graph.configData.Name, 0, hash);
            datalist.Add(parent);
            
            //for each action node, get its son 
            foreach(var node in graph.configData.ChargeData)
            {
                int nodeHash =  (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name + "-" + node.Index);
                SkillGraphShowData Node = new SkillGraphShowData($"ChargeData{node.Index}", 1, nodeHash);
                Node.IntParam.Add(node.TargetPosType);
                Node.BoolParam.Add(node.IgnoreCollision);
                Node.BoolParam.Add(node.DynamicForward);
                datalist.Add(Node);
            }
        }
        
        public static void GetActionStatusData(SkillGraph graph, out List<SkillGraphShowData> datalist)
        {
            //set up for datalist
            datalist = new List<SkillGraphShowData>();
            datalist.Clear();
            
            //get parent
            int hash = (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name);
            SkillGraphShowData parent = new SkillGraphShowData(graph.configData.Name, 0, hash);
            datalist.Add(parent);
            
            //for each action node, get its son 
            foreach(var node in graph.configData.ActionStatusData)
            {
                int nodeHash =  (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name + "-" + node.Index);
                SkillGraphShowData Node = new SkillGraphShowData($"ActionStatusData{node.Index}", 1, nodeHash);
                Node.FloatParam.Add((graph.widgetList[node.Index] as BaseSkillNode).TriggerTime);
                Node.BoolParam.Add(node.CanMove);
                Node.BoolParam.Add(node.CanRotate);
                Node.FloatParam.Add(node.Scale);
                datalist.Add(Node);
            }
        }
        
        public static void GetCameraLayerMaskData(SkillGraph graph, out List<SkillGraphShowData> datalist)
        {
            //set up for datalist
            datalist = new List<SkillGraphShowData>();
            datalist.Clear();
            
            //get parent
            int hash = (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name);
            SkillGraphShowData parent = new SkillGraphShowData(graph.configData.Name, 0, hash);
            datalist.Add(parent);
            
            //for each action node, get its son 
            foreach(var node in graph.configData.CameraLayerMaskData)
            {
                int nodeHash =  (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name + "-" + node.Index);
                SkillGraphShowData Node = new SkillGraphShowData($"CameraLayerMaskData{node.Index}", 1, nodeHash);
                Node.FloatParam.Add((graph.widgetList[node.Index] as BaseSkillNode).TriggerTime);
                Node.FloatParam.Add(node.LifeTime);
                Node.IntParam.Add(node.Mask);
                Node.BoolParam.Add(node.ChangeSelf2Player2);
                datalist.Add(Node);
            }
        }

        public static void GetCameraShakeData(SkillGraph graph, out List<SkillGraphShowData> datalist)
        {
            //set up for datalist
            datalist = new List<SkillGraphShowData>();
            datalist.Clear();
            
            //get parent
            int hash = (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name);
            SkillGraphShowData parent = new SkillGraphShowData(graph.configData.Name, 0, hash);
            datalist.Add(parent);
            
            //for each action node, get its son 
            foreach(var node in graph.configData.CameraShakeData)
            {
                int nodeHash =  (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name + "-" + node.Index);
                SkillGraphShowData Node = new SkillGraphShowData($"CameraShakeData{node.Index}", 1, nodeHash);
                Node.FloatParam.Add((graph.widgetList[node.Index] as BaseSkillNode).TriggerTime);
                Node.FloatParam.Add(node.LifeTime);
                Node.FloatParam.Add(node.Frequency);
                Node.FloatParam.Add(node.Amplitude);
                Node.FloatParam.Add(node.AttackTime);
                Node.FloatParam.Add(node.DecayTime);
                Node.FloatParam.Add(node.ImpactRadius);
                Node.BoolParam.Add(node.PlayerTrigger);
                Node.BoolParam.Add(node.StopAtEnd);
                datalist.Add(Node);
            }
        }
        
        public static void GetCameraStretchData(SkillGraph graph, out List<SkillGraphShowData> datalist)
        {
            //set up for datalist
            datalist = new List<SkillGraphShowData>();
            datalist.Clear();
            
            //get parent
            int hash = (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name);
            SkillGraphShowData parent = new SkillGraphShowData(graph.configData.Name, 0, hash);
            datalist.Add(parent);
            
            //for each action node, get its son 
            foreach(var node in graph.configData.CameraStretchData)
            {
                int nodeHash =  (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name + "-" + node.Index);
                SkillGraphShowData Node = new SkillGraphShowData($"CameraStretchData{node.Index}", 1, nodeHash);
                Node.FloatParam.Add((graph.widgetList[node.Index] as BaseSkillNode).TriggerTime);
                Node.IntParam.Add(node.Type);
                Node.BoolParam.Add(node.UsingFov);
                Node.FloatParam.Add(node.FOVLastTime);
                if (node.Type != 0)
                {
                    Node.FloatParam.Add(node.DampingCurve.keys[0].value);
                    Node.FloatParam.Add(node.DampingCurve.keys[node.DampingCurve.keys.Length - 1].value);
                }
                datalist.Add(Node);
            }
        }
        
        public static void GetFreezeData(SkillGraph graph, out List<SkillGraphShowData> datalist)
        {
            //set up for datalist
            datalist = new List<SkillGraphShowData>();
            datalist.Clear();
            
            //get parent
            int hash = (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name);
            SkillGraphShowData parent = new SkillGraphShowData(graph.configData.Name, 0, hash);
            datalist.Add(parent);
            
            //for each action node, get its son 
            foreach(var node in graph.configData.FreezeData)
            {
                int nodeHash =  (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name + "-" + node.Index);
                SkillGraphShowData Node = new SkillGraphShowData($"FreezeData{node.Index}", 1, nodeHash);
                Node.FloatParam.Add((graph.widgetList[node.Index] as BaseSkillNode).TriggerTime);
                Node.FloatParam.Add(node.LifeTime);
                Node.BoolParam.Add(node.Fixed);
                Node.BoolParam.Add(node.UseTimeScale);
                datalist.Add(Node);
            }
        }
        
        public static void GetQTEData(SkillGraph graph, out List<SkillGraphShowData> datalist)
        {
            //set up for datalist
            datalist = new List<SkillGraphShowData>();
            datalist.Clear();
            
            //get parent
            int hash = (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name);
            SkillGraphShowData parent = new SkillGraphShowData(graph.configData.Name, 0, hash);
            datalist.Add(parent);
            
            //for each action node, get its son 
            foreach(var node in graph.configData.QTEData)
            {
                int nodeHash =  (int)CFUtilPoolLib.XCommon.singleton.XHash(graph.configData.Name + "-" + node.Index);
                SkillGraphShowData Node = new SkillGraphShowData($"FreezeData{node.Index}", 1, nodeHash);
                Node.FloatParam.Add((graph.widgetList[node.Index] as BaseSkillNode).TriggerTime);
                Node.FloatParam.Add(node.Duration);
                Node.FloatParam.Add(node.CacheTime);
                Node.IntParam.Add(node.QTEID);
                Node.BoolParam.Add(node.EndAtScriptEnd);
                datalist.Add(Node);
            }
        }
    }
}
