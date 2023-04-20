using System.Security;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Reflection;

namespace UIAnalyer
{
    [Serializable]
    public class SystemDefineTable
    {
        [SerializeField]
        public List<SystemDefineData> tables = new List<SystemDefineData>();
    }

    public class FieldValue : XDataBase
    {
        public virtual Type fieldType
        {
            get
            { return typeof(string); }
        }
        public string fieldName;
        public string desc;
        protected string m_StrValue;
        public virtual void SetValue(string value, string _fieldName, string txt)
        {
            fieldName = _fieldName;
            desc = txt;
            SetValue(value);
        }

        public virtual void SetValue(string value){
            m_StrValue = value;
        }

        public virtual string StrValue()
        {
            return m_StrValue;
        }

        public virtual void RowGUI(Rect position , IControl control)
        {
            EditorGUI.TextField(position, m_StrValue);
        }

        
    }

    public class StringFieldValue : FieldValue
    {
        public string value { get { return m_StrValue; } }

        public override void RowGUI(Rect position, IControl control)
        {
            m_StrValue = EditorGUI.TextField(position, m_StrValue);
        }
    }

    public class GameObjectFieldValue : FieldValue
    {
        private string m_fix = Application.dataPath + "/BundleRes/UI";

        public override void RowGUI(Rect position, IControl control)
        {
            float w = position.width;
            position.width = w * 0.7f;
            base.RowGUI(position, control);
            position.x += position.width + 10;
            position.width = w * 0.2f;
            if (GUI.Button(position, "Select"))
            {
                string path = EditorUtility.OpenFilePanel("Select UI Prefab!", Application.dataPath+"/BundleRes/UI/OPsystemprefab","prefab");
                string need = "OPsystemprefab";
                int index = path.IndexOf(need);
                if (index >= 0)
                {
                    m_StrValue = path.Substring(index).Replace(".prefab","");
                }
            }
        }

    }

    public class BooleanFieldValue : FieldValue
    {
        public bool value;
        public override void SetValue(string str)
        {
            base.SetValue(str);
            if (string.IsNullOrEmpty(str))
                value = false;
            else
                value = int.Parse(str) > 0;
        }

        public override string StrValue(){
            return value? "1": "";
        }

        public override void RowGUI(Rect position, IControl control)
        {
            value = EditorGUI.Toggle(position, value);
        }
    }

    public class IntFieldValue : FieldValue
    {

        private int m_value;
        public override void SetValue(string str)
        {
            base.SetValue(str);
            if (string.IsNullOrEmpty(str))
                m_value = 0;
            else
                m_value = int.Parse(str);
        }

        public override string StrValue(){
            return m_value != 0 ? m_value.ToString(): "";
        }

        public override void RowGUI(Rect position, IControl control)
        {
            m_value = EditorGUI.IntField(position, m_value);
        }
        public int value { get { return m_value; } }
    }

    
    [Serializable]
    public class SystemDefineData
    {
        public static string[] tabStrs = null;
        public static string[] filedTypes = null;
        public int id;
        public string displayName;
        private FieldValue[] m_values;

        public string[] TabStrs{get{ return tabStrs;}}

        public FieldValue[] values{get{ return m_values;}}

        public SystemDefineData(int valueSize)
        {
            m_values = new FieldValue[valueSize];
        }


        public SystemDefineData(){
            if(tabStrs == null) {
                Debug.LogError("not set valueSize");
            }
            m_values = new FieldValue[tabStrs.Length]; 
        }

        public void SetValue(int idIndex, FieldValue value)
        {
            if (idIndex < values.Length)
            {
                values[idIndex] = value;
            }
        }

        public void CreateEmptyData()
        {
            for(int i = 0 ; i < values.Length;i++){
                SetPublicField(this, i , "");
            }
        }

        public FieldValue GetPublicField(int index){
            return values[index];
        }

        public  void SetPublicField(SystemDefineData obj, int index, string value)
        {
            // BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy;
            // Type type = obj.GetType();
            // FieldInfo field = GetFieldInfo(type, tabStrs[index], flags);
            FieldValue fieldValue = null;
            switch( SystemDefineData.filedTypes[index])
            {
                case "int":
                    fieldValue = XDataPool<IntFieldValue>.GetData();
                    break;
                case "bool":
                    fieldValue = XDataPool<BooleanFieldValue>.GetData();
                    break;
                case "prefab":
                    fieldValue = XDataPool<GameObjectFieldValue>.GetData();
                    break;
                default:
                    fieldValue = XDataPool<StringFieldValue>.GetData();
                    break;
            }
            fieldValue.SetValue(value, SystemDefineData.tabStrs[index],string.Empty);
            obj.SetValue(index, fieldValue);
        }

        public FieldInfo GetFieldInfo(Type type, string name, BindingFlags flags)
        {
            if (type == null)
                return null;
            FieldInfo field = type.GetField(name, flags);
            if (field == null && type.BaseType != null)
                return GetFieldInfo(type.BaseType, name, flags);
            return field;
        }

        public static StringBuilder build = new StringBuilder();
        public override string ToString()
        {
            build.Clear();
            for(int i =0 ; i < m_values.Length; i++){
                if(i > 0 ) build.Append('\t');
                build.Append(m_values[i].StrValue());
            }
            // Append(build, module, false);
            // Append(build, prefabID, true);
            // Append(build, prefabClass, true);
            // Append(build, Lua, true);
            // Append(build, prefabaddr, true);
            // Append(build, systemID, true);
            // Append(build, Parent, true);
            // Append(build, ServerSysID,true);
            // Append(build, prefabLayer, true);
            // Append(build, hideHall, true);
            // Append(build, hide3D, true);
            // Append(build, hideRecycle, true);
            // Append(build, fullScreen, true);
            // Append(build, adaptive, true);
            // Append(build, Name, true);
            // Append(build, TipType, true);
            // Append(build, TipsStrings, true);
            // Append(build, TipsPictures, true);
            // Append(build, Level, true);
            // Append(build, Titan, true);
            // Append(build, Icon, true);
            // Append(build, ForeshowIcon, true);
            // Append(build, Position, true);
            // Append(build, MainIcon, true);
            // Append(build, Startup, true);
            // Append(build, StartUpTips, true);
            // Append(build, StartupRewards, true);
            // Append(build, Foreshow, true);
            // Append(build, UIBlur, true);
            // Append(build, orderInGetway, true);
            // Append(build, JumpIcon, true);
            // Append(build, ActiveBgm, true);
            // Append(build, ActiveParam, true);
            // Append(build, OpenAudio, true);
            // Append(build, CloseAudio, true);
            // Append(build, Watermark, true);
            return build.ToString();
        }

        // private void Append(StringBuilder build, int value, bool tab = false)
        // {
        //     if (tab) build.Append('\t');
        //     build.Append(value);
        // }

        // private void Append(StringBuilder build, string value, bool tab = false)
        // {
        //     if (tab) build.Append('\t');
        //     build.Append(value);
        // }

        // private void Append(StringBuilder build, float value, bool tab = false)
        // {
        //     if (tab) build.Append('\t');
        //     build.Append(value);
        // }

        // private void Append(StringBuilder build, bool value, bool tab = false)
        // {
        //     if (tab) build.Append('\t');
        //     if (value)
        //         build.Append("1");
        //     else
        //         build.Append("");
        // }

        // public int id = 0;
        // public string module;
        // public string prefabID;
        // public string prefabClass;
        // public int Lua;
        // public string prefabaddr;
        // public int systemID;
        // public int Parent;
        // public int ServerSysID;
        // public int prefabLayer;
        // public bool hideHall;
        // public bool hide3D;
        // public bool hideRecycle;
        // public bool fullScreen;
        // public bool adaptive;
        // public string Name;
        // public int TipType;
        // public string TipsStrings;
        // public string TipsPictures;
        // public int Level;
        // public string Titan;
        // public string Icon;
        // public string ForeshowIcon;
        // public int Position;
        // public string MainIcon;
        // public string Startup;
        // public string StartUpTips;
        // public string StartupRewards;
        // public string Foreshow;
        // public int UIBlur;
        // public int orderInGetway;
        // public string JumpIcon;
        // public string ActiveBgm;
        // public string ActiveParam;
        // public string OpenAudio;
        // public string CloseAudio;
        // public bool Watermark;
    }
}