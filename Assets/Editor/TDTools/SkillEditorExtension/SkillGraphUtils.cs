using System;


using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using A;
using UnityEngine;
using UnityEditor;
using EcsData;
using EditorNode;
using XLua.Cast;
using Int32 = System.Int32;

namespace TDTools
{
    public class FilterData
    {
        public bool Logic;
        public List<bool> Selected = new List<bool>();
        public List<int> IntMax = new List<int>();
        public List<int> IntMin = new List<int>();
        public List<float> FloatMax = new List<float>();
        public List<float> FloatMin = new List<float>();
        public List<bool> BoolFilter = new List<bool>();

        public FilterData()
        {
            Selected.Clear();
            IntMax.Clear();
            IntMin.Clear();
            FloatMax.Clear();
            FloatMin.Clear();
            BoolFilter.Clear();
        }
    }

   public class PopupFilter : PopupWindowContent
   { 
       private FilterData filterData;
       private SkillGraphShowDataTitle title;
       private SkillGraphShow skillGraphShow;

       public PopupFilter(FilterData filter, SkillGraphShowDataTitle dataTitle, SkillGraphShow graphShow) : base()
       {
           filterData = filter;
           title = dataTitle;
           skillGraphShow = graphShow;
       }
       
       public override Vector2 GetWindowSize()
       {
           return new Vector2(230, 150);
       }

       public override void OnGUI(Rect rect)
       {
           var titleItems = title.TitleItems;
           for (int i = 1; i < titleItems.Length; i++)
           {
               //decide space for click button
               var rect1 = new []{GUILayout.Width(10)};
               
               var item = titleItems[i];
               int index = item.ListIndex;
               
               if (item.ParamType == "string")
                   continue;
               
               EditorGUILayout.BeginHorizontal();
               filterData.Selected[i] = EditorGUILayout.Toggle("", filterData.Selected[i], rect1);
               switch (item.ParamType)
               {
                   case "int":
                       filterData.IntMin[index] = EditorGUILayout.IntField(filterData.IntMin[index],GUILayout.Width(50));
                       EditorGUILayout.LabelField($"<={item.Title}<=",GUILayout.Width(100));
                       filterData.IntMax[index] = EditorGUILayout.IntField(filterData.IntMax[index],GUILayout.Width(50));
                       break;
                   case "float":
                       filterData.FloatMin[index] = EditorGUILayout.FloatField(filterData.FloatMin[index],GUILayout.Width(50));
                       EditorGUILayout.LabelField($"<={item.Title}<=",GUILayout.Width(100));
                       filterData.FloatMax[index] = EditorGUILayout.FloatField(filterData.FloatMax[index],GUILayout.Width(50));                       break;
                   case "bool": 
                       EditorGUILayout.LabelField($"{item.Title}=",GUILayout.Width(100));
                       filterData.BoolFilter[index] = EditorGUILayout.Toggle("", filterData.BoolFilter[index],GUILayout.Width(10));
                       EditorGUILayout.LabelField($"True",GUILayout.Width(40));
                       filterData.BoolFilter[index] = !EditorGUILayout.Toggle("", !filterData.BoolFilter[index],GUILayout.Width(10));
                       EditorGUILayout.LabelField($"False",GUILayout.Width(40));
                       break;
               }
               EditorGUILayout.EndHorizontal();
           }

           if (GUILayout.Button("Reload Data"))
           {
               skillGraphShow.Repaint();
           }
       }
    }
   

   public class PopupSelect : PopupWindowContent
   { 
       private FilterData filterData;
       private SkillGraphShowDataTitle title;
       private SkillGraphInspector skillGraphShow;

       public PopupSelect(FilterData filter, SkillGraphShowDataTitle dataTitle, SkillGraphInspector graphShow) : base()
       {
           filterData = filter;
           title = dataTitle;
           skillGraphShow = graphShow;
       }
       
       public override Vector2 GetWindowSize()
       {
           return new Vector2(230, 150);
       }

       public override void OnGUI(Rect rect)
       {
           var titleItems = title.TitleItems;
           for (int i = 1; i < titleItems.Length; i++)
           {
               //decide space for click button
               var rect1 = new []{GUILayout.Width(10)};
               
               var item = titleItems[i];
               int index = item.ListIndex;
               if (item.ParamType == "string")
                   continue;
               
               EditorGUILayout.BeginHorizontal();
               filterData.Selected[i] = EditorGUILayout.Toggle("", filterData.Selected[i], rect1);
               EditorGUILayout.LabelField($"{item.Title}",GUILayout.Width(100));
               EditorGUILayout.EndHorizontal();
           }

           if (GUILayout.Button("显示已选中的过滤选项"))
           {
               skillGraphShow.Repaint();
           }
       }
    }   
   static class SkillGraphUtils
    {
        public static bool KeepData(int judge, int number, bool logic)
        {
            if (number == 0)
                return true;
            if (logic && judge == number)
                return true;
            if (!logic && judge > 0)
                return true;
            return false;
        }
        
        public static void ReadFromList<T>(List<T> list, int index, ref string value)
        {
            if (index < list.Count)
            {
                value = list[index].ToString();
            }
        }

        public static void FihlData(List<int> param, List<int> paramMax, List<int> paramMin, int index, ref int judge) 
        {
            if (index >= param.Count)
            {
                judge += 1;
            }
            else if (param[index] <= paramMax[index] && param[index] >= paramMin[index])
            {
                judge += 1;
            }
        }
        
        public static void FihlData(List<bool> param, List<bool> paramMax, List<bool> paramMin, int index, ref int judge) 
        {
            if (index >= param.Count)
            {
                judge += 1;
            }
            else if (param[index] == paramMax[index] && param[index] == paramMin[index])
            {
                judge += 1;
            }
        }
        
        public static void FihlData_float(List<float> param, List<float> paramMax, List<float> paramMin, int index, ref int judge) 
        {
            if (index >= param.Count)
            {
                judge += 1;
            }
            else if (param[index] <= paramMax[index] && param[index] >= paramMin[index])
            {
                judge += 1;
            }
        }
    }
}