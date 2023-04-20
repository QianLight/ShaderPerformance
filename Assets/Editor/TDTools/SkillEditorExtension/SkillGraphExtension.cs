using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EcsData;
using EditorNode;

namespace TDTools
{
    public static class SkillGraphExtension
    {
        private static Assembly assembly = Assembly.Load("CFUtilPoolLib");
        public static int GetNodeCount(this SkillGraph graph)
        {
            int count = 0;
            foreach (string nodeName in graph.NodeNameArray)
            {
                string dataType = $"EcsData.X{nodeName}Data";
                string listName = $"{nodeName}Data";
                count += ListCount(graph, graph.configData, dataType, listName);
            }
            return count;
        }

        private static int ListCount(SkillGraph graph, XConfigData data, string typename1, string listName)
        {
            Type t1 = assembly.GetType(typename1);
            object[] list = new object[] { graph, data.GetType().GetField(listName).GetValue(data), null };
            MethodInfo checkListCount = typeof(SkillGraphExtension).GetMethod("CheckListCount").MakeGenericMethod(new Type[] { t1 });
            checkListCount.Invoke(graph, list);
            return (int)list[2];
        }

        public static void CheckListCount<T>(this SkillGraph graph, List<T> list, out int count) where T : XBaseData
        {
            count = list.Count;
        }

        public static List<int> GetTransferNode(this SkillGraph graph)
        {
            var result = new List<int>();
            foreach (string nodeName in graph.NodeNameArray)
            {
                string dataType = $"EcsData.X{nodeName}Data";
                string listName = $"{nodeName}Data";
                result.AddRange(GetTransferData(graph, graph.configData, dataType, listName));
            }
            return result.Distinct().ToList();
        }

        private static List<int> GetTransferData(SkillGraph graph, XConfigData data, string typename1, string listName)
        {
            Type t1 = assembly.GetType(typename1);
            object[] list = new object[] { graph, data.GetType().GetField(listName).GetValue(data), null };
            MethodInfo getTransfer = typeof(SkillGraphExtension).GetMethod("GetTransferList").MakeGenericMethod(new Type[] { t1 });
            getTransfer.Invoke(graph, list);
            return (List<int>)list[2];
        }

        public static void GetTransferList<T>(this SkillGraph graph, List<T> list, out List<int> transfer) where T : XBaseData
        {
            transfer = new List<int>();
            foreach(var data in list)
            {
                foreach(var trans in data.TransferData)
                {
                    transfer.Add(trans.Index);
                }
            }
            transfer = transfer.Distinct().ToList();
        }
    }

    static class OrderExtensionMethods
    {
        public static IOrderedEnumerable<T> Order<T, TKey>(this IEnumerable<T> source, Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.OrderBy(selector);
            }
            else
            {
                return source.OrderByDescending(selector);
            }
        }

        public static IOrderedEnumerable<T> ThenBy<T, TKey>(this IOrderedEnumerable<T> source, Func<T, TKey> selector, bool ascending)
        {
            if (ascending)
            {
                return source.ThenBy(selector);
            }
            else
            {
                return source.ThenByDescending(selector);
            }
        }
    }
}
