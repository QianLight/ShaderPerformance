using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace BluePrint
{
    public class BlueprintGraphVariantManager
    {
        protected GUIContent _content_add = new GUIContent(" +");
        protected GUIContent _content_sub = new GUIContent(" -");

        public List<BluePrintVariant> UserVariant = new List<BluePrintVariant>();

        public int HostGraphID { get; set; }

        public void UnInit()
        {
            UserVariant.Clear();
        }

        public static List<BluePrintVariant> GlobalVariants
        {
            get { return BlueprintEditor.GetMainGraph().VarManager.UserVariant; }
        }

        public void DrawDataInspector()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Variable: ", new GUILayoutOption[] { GUILayout.Width(150f) });
            if (GUILayout.Button(_content_add, GUILayout.MaxWidth(20)))
            {
                AddNewVariant();
            }
            GUILayout.EndHorizontal();

            int tempIndex = -1;
            for (int i = 0; i < UserVariant.Count; ++i)
            {
                GUILayout.BeginHorizontal();
                UserVariant[i].VarType = (VariantType)EditorGUILayout.EnumPopup(UserVariant[i].VarType, new GUILayoutOption[] { GUILayout.MaxWidth(100) });
                string varName = UserVariant[i].VariableName;
                varName = EditorGUILayout.TextArea(varName, new GUILayoutOption[] { GUILayout.MaxWidth(100) });
                if (GUILayout.Button(_content_sub, GUILayout.MaxWidth(20)))
                {
                    tempIndex = i;
                }
                GUIContent content = new GUIContent("初始值", "类型为bool时填0为false其他任意值为true");
                EditorGUILayout.LabelField(content, GUILayout.Width(50f));
                UserVariant[i].InitValue = EditorGUILayout.TextField(UserVariant[i].InitValue, GUILayout.MaxWidth(100f));

                if(VariantNameExists(varName, i))
                {
                    BlueprintEditor.Editor.ShowNotification(new GUIContent("重复变量名"), 3);
                }
                else
                {
                    UserVariant[i].VariableName = varName;
                }
                GUILayout.EndHorizontal();
            }
            if(tempIndex>=0)
                RemoveVariant(tempIndex);
        }

        public void AddVariant(VariantType varType, string name,string value)
        {
            BluePrintVariant newVar = new BluePrintVariant();
            newVar.VariableName = name;
            newVar.VarType = varType;
            newVar.InitValue = value;
            UserVariant.Add(newVar);
        }

        public void AddNewVariant()
        {
            int i = UserVariant.Count;
            string defaultVarName = "Var" + i;

            while (VariantNameExists(defaultVarName, -1))
            {
                i++;
                defaultVarName = "Var" + i;
            }

            BluePrintVariant newVar = new BluePrintVariant();
            newVar.VariableName = defaultVarName;

            UserVariant.Add(newVar);
        }

        protected bool VariantNameExists(string name, int searchIndex)
        {
            for(int i = 0; i < UserVariant.Count; ++i)
            {
                if (UserVariant[i].VariableName == name && (searchIndex != i))
                    return true;
            }

            if(HostGraphID != 1)
            {
                for (int i = 0; i < GlobalVariants.Count; ++i)
                {
                    if (GlobalVariants[i].VariableName == name) return true;
                }
            }
            

            return false;
        }

        protected void RemoveVariant(int index)
        {
            string VarName = UserVariant[index].VariableName;
            UserVariant.RemoveAt(index);

            //删除节点
        }

        public bool GetVariantType(string name, ref VariantType t)
        {
            for(int i = 0; i < GlobalVariants.Count; ++i)
            {
                if (GlobalVariants[i].VariableName == name)
                {
                    t = GlobalVariants[i].VarType;
                    return true;
                }
            }

            for (int i = 0; i < UserVariant.Count; ++i)
            {
                if (UserVariant[i].VariableName == name)
                {
                    t = UserVariant[i].VarType;
                    return true;
                }
            }

            return false;
        }

        
    }
}
